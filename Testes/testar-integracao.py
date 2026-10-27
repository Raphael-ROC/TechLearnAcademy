"""Teste real da API, limitado a uma instancia LocalDB exclusiva de testes."""
import argparse
import email
import http.cookiejar
import json
import os
from pathlib import Path
import re
import secrets
import socketserver
import subprocess
import threading
import time
import urllib.error
import urllib.request

raiz = Path(__file__).resolve().parents[1]
instancia = "TechLearnPimIVTeste20261004"
base = "http://127.0.0.1:5299"
eventos = []
emails = []

def sql(comando):
    # Nunca conecta ao banco usado pelo projeto anterior.
    p = subprocess.run(["sqlcmd", "-S", "(localdb)\\" + instancia, "-E", "-C",
                        "-b", "-h", "-1", "-W", "-d", "TechLearnAcademy", "-Q", comando],
                       capture_output=True, text=True, encoding="utf-8", errors="replace", timeout=30)
    if p.returncode != 0:
        raise RuntimeError("Falha SQL no teste: " + p.stderr + p.stdout)
    return p.stdout.strip()

def conferir(condicao, nome):
    if not condicao:
        raise AssertionError(nome)
    eventos.append({"teste": nome, "resultado": "PASSOU"})
    print("PASSOU: " + nome, flush=True)

class Cliente:
    def __init__(self):
        self.jar = http.cookiejar.CookieJar()
        self.http = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(self.jar))

    def chamar(self, metodo, caminho, dados=None, esperado=200, cabecalhos=None):
        corpo = None if dados is None else json.dumps(dados).encode()
        headers = {"Content-Type": "application/json"}
        headers.update(cabecalhos or {})
        req = urllib.request.Request(base + caminho, data=corpo, headers=headers, method=metodo)
        try:
            resposta = self.http.open(req, timeout=30)
        except urllib.error.HTTPError as erro:
            resposta = erro
        conteudo = resposta.read().decode()
        valor = json.loads(conteudo) if conteudo else None
        if resposta.code != esperado:
            raise AssertionError(metodo + " " + caminho + ": esperado " + str(esperado)
                                 + ", recebido " + str(resposta.code) + ": " + conteudo)
        return valor

class SmtpTeste(socketserver.StreamRequestHandler):
    def handle(self):
        def responder(texto):
            self.wfile.write((texto + "\r\n").encode())
            self.wfile.flush()
        responder("220 localhost SMTP de teste")
        while True:
            linha = self.rfile.readline()
            if not linha:
                break
            comando = linha.decode(errors="replace").strip().upper()
            if comando.startswith("EHLO") or comando.startswith("HELO"):
                responder("250 localhost")
            elif comando.startswith("MAIL") or comando.startswith("RCPT") or comando == "RSET":
                responder("250 OK")
            elif comando == "DATA":
                responder("354 Envie a mensagem")
                partes = []
                while True:
                    trecho = self.rfile.readline()
                    if trecho in (b".\r\n", b""):
                        break
                    partes.append(trecho)
                emails.append(b"".join(partes))
                responder("250 Mensagem recebida pelo teste local")
            elif comando == "QUIT":
                responder("221 Encerrado")
                break
            else:
                responder("250 OK")

def token_recebido():
    limite = time.time() + 5
    while not emails and time.time() < limite:
        time.sleep(0.1)
    if not emails:
        raise AssertionError("API nao entregou mensagem ao servidor SMTP de teste")
    mensagem = email.message_from_bytes(emails.pop(0))
    texto = mensagem.get_payload(decode=True).decode(mensagem.get_content_charset() or "utf-8")
    token = re.search(r"token=([A-Fa-f0-9]{64})", texto)
    if not token:
        raise AssertionError("E-mail de teste nao trouxe o link de recuperacao")
    return token.group(1)

def executar():
    publico = Cliente()
    conferir(publico.chamar("GET", "/api/status") is not None, "API em execucao")
    publico.chamar("GET", "/api/usuarios/perfil", esperado=401)
    conferir(True, "Sem sessao nao acessa perfil")
    sufixo = secrets.token_hex(6)
    senha = "A!" + secrets.token_hex(16) + "9 "
    contas = {}
    for papel in ["Administrador", "Gestor", "Mentor", "Aluno", "Outro"]:
        endereco = papel.lower() + sufixo + "@example.test"
        publico.chamar("POST", "/api/usuarios/cadastro",
                       {"nome": "Teste " + papel, "email": endereco, "senha": senha,
                        "confirmarSenha": senha, "perfil": "Administrador"}, 201)
        cliente = Cliente()
        login = cliente.chamar("POST", "/api/usuarios/login", {"email": endereco, "senha": senha})
        conferir(login["perfil"] == "Aluno", "Cadastro publico sempre Aluno: " + papel)
        contas[papel] = {"id": login["id"], "email": endereco, "http": cliente, "token": login["token"]}
    publico.chamar("POST", "/api/usuarios/cadastro",
                   {"nome": "Duplicado", "email": contas["Aluno"]["email"], "senha": senha,
                    "confirmarSenha": senha}, 409)
    conferir(True, "E-mail duplicado recusado")
    publico.chamar("POST", "/api/usuarios/login", {"email": contas["Aluno"]["email"], "senha": "incorreta"}, 401)
    conferir(True, "Senha incorreta recusada")
    publico.chamar("POST", "/api/usuarios/cadastro",
                   {"nome": "Nome", "email": "nome@example.test", "senha": "A1!curta",
                    "confirmarSenha": "A1!curta"}, 400)
    conferir(True, "Senha curta recusada")
    # Preparacao exclusiva do teste: promove a primeira conta no banco isolado.
    sql("UPDATE Usuarios SET Perfil=N'Administrador' WHERE Id=" + str(contas["Administrador"]["id"]))
    admin = contas["Administrador"]["http"]
    for papel in ["Gestor", "Mentor"]:
        admin.chamar("PUT", "/api/administracao/usuarios/" + str(contas[papel]["id"]),
                     {"perfil": papel, "ativo": True})
        contas[papel]["http"].chamar("POST", "/api/usuarios/login",
                                     {"email": contas[papel]["email"], "senha": senha})
    gestor = contas["Gestor"]["http"]
    mentor = contas["Mentor"]["http"]
    aluno = contas["Aluno"]["http"]
    outro = contas["Outro"]["http"]
    aluno.chamar("GET", "/api/administracao/painel", esperado=403)
    admin.chamar("POST", "/api/cursos", {"nome": "Nao pode", "descricao": "Teste", "cargaHoraria": 1}, 403)
    conferir(True, "Permissoes por perfil conferidas no servidor")
    curso = gestor.chamar("POST", "/api/cursos",
                          {"nome": "Curso de teste " + sufixo, "descricao": "Registro temporario de teste", "cargaHoraria": 10}, 201)["id"]
    modulo = gestor.chamar("POST", "/api/modulos", {"cursoId": curso, "titulo": "Modulo de teste", "ordem": 1}, 201)["id"]
    material = gestor.chamar("POST", "/api/materiais",
                             {"moduloId": modulo, "titulo": "PDF de teste", "tipo": "PDF",
                              "url": "https://example.test/material.pdf"}, 201)["id"]
    curso_json = aluno.chamar("GET", "/api/cursos/" + str(curso))
    conferir(curso_json["matriculado"] is False, "Curso consultado sem matricula")
    aluno.chamar("GET", "/api/modulos/" + str(modulo) + "/materiais", esperado=403)
    aluno.chamar("POST", "/api/matriculas", {"cursoId": curso}, 201)
    aluno.chamar("POST", "/api/matriculas", {"cursoId": curso}, 409)
    conferir(True, "Matricula real e duplicacao recusada")
    conferir(len(aluno.chamar("GET", "/api/modulos/" + str(modulo) + "/materiais")) == 1,
             "Aluno matriculado acessa material")
    prova = gestor.chamar("POST", "/api/avaliacoes", {"moduloId": modulo, "titulo": "Avaliacao de teste"}, 201)["id"]
    gestor.chamar("POST", "/api/questoes", {"avaliacaoId": prova, "enunciado": "Invalida",
                   "alternativas": [{"texto": "Uma", "correta": True}]}, 400)
    gestor.chamar("POST", "/api/questoes", {"avaliacaoId": prova, "enunciado": "Invalida",
                   "alternativas": [{"texto": "Uma", "correta": True}, {"texto": "Duas", "correta": True}]}, 400)
    conferir(True, "Questao exige ao menos duas alternativas e uma correta")
    questao = gestor.chamar("POST", "/api/questoes", {"avaliacaoId": prova, "enunciado": "Dois mais dois?",
                   "alternativas": [{"texto": "Quatro", "correta": True}, {"texto": "Cinco", "correta": False}]}, 201)["id"]
    edicao = gestor.chamar("GET", "/api/avaliacoes/" + str(prova))
    questoes = edicao["questoes"]
    certa = next(a["id"] for a in questoes[0]["alternativas"] if a["correta"])
    vista = aluno.chamar("GET", "/api/avaliacoes/" + str(prova))
    conferir("correta" not in vista["questoes"][0]["alternativas"][0], "Gabarito oculto antes da resposta")
    outro.chamar("GET", "/api/avaliacoes/" + str(prova), esperado=403)
    aluno.chamar("POST", "/api/avaliacoes/" + str(prova) + "/respostas", {"respostas": []}, 400)
    aluno.chamar("POST", "/api/avaliacoes/" + str(prova) + "/respostas",
                {"respostas": [{"questaoId": questao, "alternativaId": 2147483647}]}, 400)
    resultado = aluno.chamar("POST", "/api/avaliacoes/" + str(prova) + "/respostas",
                             {"respostas": [{"questaoId": questao, "alternativaId": certa}]})
    conferir(resultado["nota"] == 10 and resultado["percentual"] == 100 and resultado["aprovado"],
             "Correcao real com nota 10 e aprovacao")
    detalhe = aluno.chamar("GET", "/api/resultados/" + str(resultado["id"]))
    outro.chamar("GET", "/api/resultados/" + str(resultado["id"]), esperado=403)
    conferir(detalhe["respostas"][0]["correta"], "Resultado proprio e acesso alheio recusado")
    minhas = aluno.chamar("GET", "/api/matriculas/minhas")
    conferir(any(c["cursoId"] == curso and c["progresso"] == 100 for c in minhas),
             "Aprovacao registra progresso no modulo")
    mentor.chamar("GET", "/api/mentor/alunos/" + str(contas["Aluno"]["id"]), esperado=403)
    vinculo = admin.chamar("POST", "/api/administracao/vinculos",
                           {"mentorId": contas["Mentor"]["id"], "alunoId": contas["Aluno"]["id"]})
    mentor.chamar("GET", "/api/mentor/alunos/" + str(contas["Aluno"]["id"]))
    mentor.chamar("POST", "/api/mentor/orientacoes", {"alunoId": contas["Aluno"]["id"], "texto": "Orientacao registrada no teste."})
    conferir(len(aluno.chamar("GET", "/api/aluno/orientacoes")) > 0, "Mentor vinculado registra orientacao lida pelo aluno")
    mentor.chamar("POST", "/api/mentor/orientacoes", {"alunoId": contas["Outro"]["id"], "texto": "Sem vinculo"}, 403)
    conferir(True, "Mentor nao acessa aluno sem vinculo")
    relatorio = gestor.chamar("GET", "/api/relatorios")
    conferir(relatorio["totalMatriculas"] >= 1, "Relatorio consulta dados reais")
    admin.chamar("PUT", "/api/administracao/usuarios/" + str(contas["Administrador"]["id"]),
                 {"perfil": "Aluno", "ativo": True}, 409)
    conferir(True, "Ultimo Administrador ativo preservado")
    admin.chamar("PUT", "/api/administracao/usuarios/" + str(contas["Outro"]["id"]), {"perfil": "Aluno", "ativo": False})
    publico.chamar("POST", "/api/usuarios/login", {"email": contas["Outro"]["email"], "senha": senha}, 401)
    outro.chamar("GET", "/api/usuarios/perfil", esperado=401)
    conferir(True, "Usuario inativo nao entra e perde acesso")
    publico.chamar("POST", "/api/usuarios/recuperar-senha", {"email": contas["Aluno"]["email"]})
    token = token_recebido()
    nova = "B!" + secrets.token_hex(16) + "8 "
    publico.chamar("POST", "/api/usuarios/redefinir-senha", {"token": token, "senha": nova, "confirmarSenha": nova})
    publico.chamar("POST", "/api/usuarios/redefinir-senha", {"token": token, "senha": nova, "confirmarSenha": nova}, 400)
    publico.chamar("POST", "/api/usuarios/login", {"email": contas["Aluno"]["email"], "senha": senha}, 401)
    aluno.chamar("GET", "/api/usuarios/perfil", esperado=401)
    conferir(True, "SMTP local entrega link; redefinicao real; uso unico; sessoes revogadas")
    novo_cliente = Cliente()
    novo_cliente.chamar("POST", "/api/usuarios/login", {"email": contas["Aluno"]["email"], "senha": nova})
    novo_cliente.chamar("POST", "/api/usuarios/logout", {})
    novo_cliente.chamar("GET", "/api/usuarios/perfil", esperado=401)
    conferir(True, "Logout real revoga sessao")
    publico.chamar("POST", "/api/usuarios/recuperar-senha", {"email": contas["Aluno"]["email"]})
    vencido = token_recebido()
    sql("UPDATE RecuperacoesSenha SET DataCriacao=DATEADD(hour,-2,SYSUTCDATETIME()),Validade=DATEADD(hour,-1,SYSUTCDATETIME()) WHERE UsuarioId=" + str(contas["Aluno"]["id"]))
    publico.chamar("POST", "/api/usuarios/redefinir-senha", {"token": vencido, "senha": nova, "confirmarSenha": nova}, 400)
    conferir(True, "Token de recuperacao vencido recusado")
    publico.chamar("POST", "/api/usuarios/login", {"email": contas["Administrador"]["email"], "senha": senha},
                  esperado=403, cabecalhos={"Origin": "https://origem-nao-permitida.example"})
    conferir(True, "Origem nao autorizada recusada")
    conferir(int(sql("SET NOCOUNT ON; SELECT COUNT(*) FROM AuditoriaUsuarios")) >= 3, "Trigger registra mudancas de perfil e status")
    hash_salvo = sql("SET NOCOUNT ON; SELECT SenhaHash FROM Usuarios WHERE Id=" + str(contas["Aluno"]["id"]))
    conferir(hash_salvo.startswith("600000.") and nova not in hash_salvo, "SQL guarda hash PBKDF2 com sal e nao senha")

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--executar", action="store_true", help="Autoriza criar dados so na instancia exclusiva de teste")
    args = parser.parse_args()
    if not args.executar:
        print("Leia Testes/LEIA-ME.md antes. Este script exige --executar.")
        return
    pasta = raiz / "Testes" / "evidencias"
    pasta.mkdir(parents=True, exist_ok=True)
    ambiente = os.environ.copy()
    ambiente.update({"ASPNETCORE_ENVIRONMENT": "Development", "ASPNETCORE_URLS": base,
        "ConnectionStrings__Banco": "Server=(localdb)\\" + instancia + ";Database=TechLearnAcademy;Integrated Security=true;Encrypt=true;TrustServerCertificate=true",
        "Email__Servidor": "127.0.0.1", "Email__Porta": "10025", "Email__UsarTls": "false",
        "Email__Remetente": "testes@example.test", "Email__UrlRedefinicao": base + "/redefinir-senha.html"})
    servidor = socketserver.ThreadingTCPServer(("127.0.0.1", 10025), SmtpTeste)
    servidor.daemon_threads = True
    threading.Thread(target=servidor.serve_forever, daemon=True).start()
    log = open(pasta / "api-teste.log", "w", encoding="utf-8")
    processo = subprocess.Popen(["dotnet", str(raiz / "TechLearnAcademy.Api/bin/Debug/net10.0/TechLearnAcademy.Api.dll")],
                               cwd=raiz / "TechLearnAcademy.Api", env=ambiente, stdout=log, stderr=log)
    try:
        pronto = False
        for tentativa in range(60):
            try:
                urllib.request.urlopen(base + "/api/status", timeout=1).close()
                pronto = True
                break
            except (OSError, urllib.error.URLError):
                if processo.poll() is not None:
                    break
                time.sleep(0.5)
        if not pronto:
            raise RuntimeError("API nao iniciou. Confira api-teste.log.")
        executar()
        (pasta / "integracao.json").write_text(json.dumps(eventos, ensure_ascii=False, indent=2), encoding="utf-8")
        print("Testes concluidos. Nenhuma senha ou token foi gravado nas evidencias.")
    finally:
        processo.terminate()
        try:
            processo.wait(timeout=10)
        except subprocess.TimeoutExpired:
            processo.kill()
        log.close()
        servidor.shutdown()
        servidor.server_close()

if __name__ == "__main__":
    main()
