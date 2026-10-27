"""Confere regras reais na API e no banco exclusivo que já estão ligados."""
import argparse
from concurrent.futures import ThreadPoolExecutor
import importlib.util
import json
from pathlib import Path
import secrets
import threading
import urllib.error
import urllib.request

# Reaproveita o cliente HTTP e a conexão SQL restrita à instância de teste.
pasta = Path(__file__).resolve().parent
especificacao = importlib.util.spec_from_file_location("integracao", pasta / "testar-integracao.py")
integracao = importlib.util.module_from_spec(especificacao)
especificacao.loader.exec_module(integracao)


def criar_conta(publico, nome, sufixo, senha):
    endereco = nome.lower() + sufixo + "@example.test"
    publico.chamar("POST", "/api/usuarios/cadastro", {
        "nome": "Teste adicional " + nome, "email": endereco,
        "senha": senha, "confirmarSenha": senha}, 201)
    cliente = integracao.Cliente()
    login = cliente.chamar("POST", "/api/usuarios/login", {"email": endereco, "senha": senha})
    return cliente, login, endereco


def criar_prova(gestor, modulo, quantidade, titulo):
    prova = gestor.chamar("POST", "/api/avaliacoes", {
        "moduloId": modulo, "titulo": titulo}, 201)["id"]
    for numero in range(quantidade):
        gestor.chamar("POST", "/api/questoes", {
            "avaliacaoId": prova, "enunciado": "Questão de teste " + str(numero + 1),
            "alternativas": [{"texto": "Alternativa correta", "correta": True},
                             {"texto": "Alternativa incorreta", "correta": False}]}, 201)
    dados = gestor.chamar("GET", "/api/avaliacoes/" + str(prova))
    return prova, dados["questoes"]


def respostas(questoes, acertos):
    lista = []
    for numero, questao in enumerate(questoes):
        alternativa = next(a for a in questao["alternativas"] if a["correta"] == (numero < acertos))
        lista.append({"questaoId": questao["id"], "alternativaId": alternativa["id"]})
    return {"respostas": lista}


def matricular_juntos(token, curso):
    barreira = threading.Barrier(2)

    def enviar():
        pedido = urllib.request.Request(integracao.base + "/api/matriculas",
            data=json.dumps({"cursoId": curso}).encode(), method="POST",
            headers={"Content-Type": "application/json", "Authorization": "Bearer " + token})
        # Os dois pedidos saem juntos. O banco decide quem pode gravar.
        barreira.wait(timeout=10)
        try:
            resposta = urllib.request.urlopen(pedido, timeout=30)
        except urllib.error.HTTPError as erro:
            resposta = erro
        with resposta:
            status = resposta.code
            resposta.read()
        return status

    with ThreadPoolExecutor(max_workers=2) as tarefas:
        futuros = [tarefas.submit(enviar), tarefas.submit(enviar)]
        return sorted(futuro.result(timeout=40) for futuro in futuros)


def executar():
    conferir = integracao.conferir
    publico = integracao.Cliente()
    publico.chamar("GET", "/api/status")
    publico.chamar("GET", "/api/usuarios/perfil", esperado=401)
    conferir(True, "Ausência de token impede acesso ao perfil")
    publico.chamar("GET", "/api/usuarios/perfil", esperado=401,
                   cabecalhos={"Authorization": "Bearer " + secrets.token_hex(32)})
    conferir(True, "Token aleatório não cadastrado é recusado")

    sufixo = secrets.token_hex(6)
    senha = "A!" + secrets.token_hex(16) + "9 "
    gestor, login_gestor, email_gestor = criar_conta(publico, "Gestor", sufixo, senha)
    aluno, login_aluno, email_aluno = criar_conta(publico, "Aluno", sufixo, senha)
    # Somente esta conta de teste é promovida, no banco isolado.
    integracao.sql("UPDATE Usuarios SET Perfil=N'Gestor' WHERE Id=" + str(int(login_gestor["id"])))
    gestor.chamar("POST", "/api/usuarios/login", {"email": email_gestor, "senha": senha})
    publico.chamar("POST", "/api/usuarios/recuperar-senha", {"email": email_aluno}, 503)
    conferir(True, "SMTP sem configuração informa 503 sem sucesso fictício")

    curso = gestor.chamar("POST", "/api/cursos", {
        "nome": "Regras adicionais " + sufixo, "descricao": "Dados somente para teste isolado",
        "cargaHoraria": 12}, 201)["id"]
    statuses = matricular_juntos(login_aluno["token"], curso)
    conferir(statuses == [201, 409], "Matrícula concorrente: uma criada e outra recusada")
    quantidade = int(integracao.sql("SET NOCOUNT ON; SELECT COUNT(*) FROM Matriculas WHERE AlunoId="
        + str(int(login_aluno["id"])) + " AND CursoId=" + str(int(curso)) + " AND Ativo=1"))
    conferir(quantidade == 1, "Banco conserva somente uma matrícula ativa após concorrência")

    modulo = gestor.chamar("POST", "/api/modulos", {
        "cursoId": curso, "titulo": "Limite de aprovação", "ordem": 1}, 201)["id"]
    prova, questoes = criar_prova(gestor, modulo, 5, "Prova com cinco questões")
    caminho = "/api/avaliacoes/" + str(prova) + "/respostas"
    reprovado = aluno.chamar("POST", caminho, respostas(questoes, 2))
    conferir(reprovado["acertos"] == 2 and reprovado["total"] == 5
        and reprovado["percentual"] == 40 and reprovado["nota"] == 4
        and reprovado["aprovado"] is False, "Dois acertos em cinco reprovam com 40% e nota 4")
    minhas = aluno.chamar("GET", "/api/matriculas/minhas")
    conferir(next(c["progresso"] for c in minhas if c["cursoId"] == curso) == 0,
             "Reprovação não conclui o módulo")
    aprovado = aluno.chamar("POST", caminho, respostas(questoes, 3))
    conferir(aprovado["acertos"] == 3 and aprovado["total"] == 5
        and aprovado["percentual"] == 60 and aprovado["nota"] == 6
        and aprovado["aprovado"] is True, "Três acertos em cinco aprovam exatamente no limite de 60%")
    minhas = aluno.chamar("GET", "/api/matriculas/minhas")
    conferir(next(c["progresso"] for c in minhas if c["cursoId"] == curso) == 100,
             "Aprovação no limite conclui o módulo")

    modulo_redondo = gestor.chamar("POST", "/api/modulos", {
        "cursoId": curso, "titulo": "Arredondamento", "ordem": 2}, 201)["id"]
    prova_redonda, questoes_redondas = criar_prova(gestor, modulo_redondo, 32, "Prova com 32 questões")
    caminho_redondo = "/api/avaliacoes/" + str(prova_redonda) + "/respostas"
    redondo = aluno.chamar("POST", caminho_redondo, respostas(questoes_redondas, 1))
    conferir(redondo["acertos"] == 1 and redondo["total"] == 32
        and redondo["percentual"] == 3.13 and redondo["nota"] == 0.31
        and redondo["aprovado"] is False, "Arredondamento real: 1 de 32 resulta em 3,13% e nota 0,31")

    repetidas = respostas(questoes_redondas, 1)
    repetidas["respostas"][-1] = dict(repetidas["respostas"][0])
    aluno.chamar("POST", caminho_redondo, repetidas, 400)
    conferir(True, "Questão repetida é recusada mesmo com quantidade total correta")
    trocadas = respostas(questoes_redondas, 1)
    trocadas["respostas"][0]["alternativaId"] = questoes_redondas[1]["alternativas"][0]["id"]
    aluno.chamar("POST", caminho_redondo, trocadas, 400)
    conferir(True, "Alternativa de outra questão é recusada")
    resultados = aluno.chamar("GET", "/api/resultados")
    conferir(sum(r["avaliacaoId"] == prova_redonda for r in resultados) == 1,
             "Envios inválidos não criam tentativas parciais")

    historico = aluno.chamar("GET", "/api/resultados/" + str(aprovado["id"]))
    alterar = {"enunciado": "Enunciado diferente", "ativo": True,
        "alternativas": [{"texto": "Novo gabarito", "correta": True},
                         {"texto": "Outra opção", "correta": False}]}
    gestor.chamar("PUT", "/api/questoes/" + str(questoes[0]["id"]), alterar, 409)
    conferir(True, "Editar questão depois de respostas reais é recusado com 409")
    conferir(aluno.chamar("GET", "/api/resultados/" + str(aprovado["id"])) == historico,
             "Histórico conserva enunciados, respostas, gabarito e nota após edição recusada")
    alterar["ativo"] = False
    gestor.chamar("PUT", "/api/questoes/" + str(questoes[0]["id"]), alterar)
    conferir(aluno.chamar("GET", "/api/resultados/" + str(aprovado["id"])) == historico,
             "Desativar questão conserva todo o histórico já registrado")
    dados = gestor.chamar("GET", "/api/avaliacoes/" + str(prova))
    questao_inativa = next(q for q in dados["questoes"] if q["id"] == questoes[0]["id"])
    conferir(questao_inativa["ativo"] is False
        and questao_inativa["enunciado"] == questoes[0]["enunciado"]
        and questao_inativa["alternativas"] == questoes[0]["alternativas"],
        "Desativação altera somente a situação da questão")


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--executar", action="store_true",
                        help="Autoriza criar dados somente no banco exclusivo de teste")
    if not parser.parse_args().executar:
        print("Use --executar somente depois de ler Testes/LEIA-ME.md.")
        return
    if integracao.instancia != "TechLearnPimIVTeste20261004" or integracao.base != "http://127.0.0.1:5299":
        raise RuntimeError("Endereço diferente da instância de teste permitida.")
    try:
        executar()
        evidencia = pasta / "evidencias" / "adicionais.json"
        evidencia.parent.mkdir(parents=True, exist_ok=True)
        evidencia.write_text(json.dumps(integracao.eventos, ensure_ascii=False, indent=2), encoding="utf-8")
        print("Testes adicionais concluídos. Nenhuma senha ou token foi salvo.")
    except Exception as erro:
        # O erro não grava os dados usados na autenticação.
        print("Teste interrompido: " + type(erro).__name__ + ": " + str(erro))
        raise SystemExit(1)


if __name__ == "__main__":
    main()
