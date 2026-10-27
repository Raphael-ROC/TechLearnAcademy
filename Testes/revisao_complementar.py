"""Casos adicionais da revisão, somente na API fictícia da porta 5407."""
import json, urllib.request, urllib.error
from pathlib import Path
from concurrent.futures import ThreadPoolExecutor
B="http://127.0.0.1:5407"; root=Path(__file__).resolve().parents[1]; users={}; checks=[]
def check(ok,nome):
    checks.append({"verificacao":nome,"passou":bool(ok)})
    (root/"tmp/revisao/resultado-complementar.json").write_text(json.dumps(checks,ensure_ascii=False,indent=2),encoding="utf-8")
    if not ok: raise AssertionError(nome)
def req(path,role=None,data=None,method=None,status=200):
    h={}
    if role: h["Authorization"]="Bearer "+users[role]["token"]
    raw=None
    if data is not None: raw=json.dumps(data).encode(); h["Content-Type"]="application/json"
    try:r=urllib.request.urlopen(urllib.request.Request(B+path,data=raw,headers=h,method=method or ("POST" if raw else "GET")),timeout=10)
    except urllib.error.HTTPError as e:r=e
    value=json.loads(r.read())
    check(r.status==status,f"{role or 'Público'} {method or ('POST' if raw else 'GET')} {path}: {r.status} esperado {status}")
    return value
for role,email in {"admin":"admin@revisao.test","gestor":"gestor.teste.2@gestor.techlearn.com","mentor":"mentor.teste.3@mentor.techlearn.com","mentor2":"outro.mentor.4@mentor.techlearn.com","aluno":"aluno@revisao.test"}.items():
    users[role]=req("/api/usuarios/login",data={"email":email,"senha":"Teste123!"})
check(users["admin"]["nome"]=="Admin Teste","Administrador fictício confirmado")
cid=req("/api/cursos","gestor",{"nome":"Curso complementar","descricao":"Teste de mudanças nas avaliações","cargaHoraria":8},status=201)["id"]
mid=req("/api/modulos","gestor",{"cursoId":cid,"titulo":"Módulo complementar","ordem":1},status=201)["id"]
req("/api/matriculas","aluno",{"cursoId":cid},status=201)
def prova(titulo):
    aid=req("/api/avaliacoes","gestor",{"moduloId":mid,"titulo":titulo},status=201)["id"]
    questao(aid)
    return aid
def questao(aid):
    return req("/api/questoes","gestor",{"avaliacaoId":aid,"enunciado":"Escolha Sim","alternativas":[{"texto":"Sim","correta":True},{"texto":"Não","correta":False}]},status=201)["id"]
def responder(aid,correta=True):
    a=req(f"/api/avaliacoes/{aid}","gestor")
    return req(f"/api/avaliacoes/{aid}/respostas","aluno",{"respostas":[{"questaoId":q["id"],"alternativaId":next(x["id"] for x in q["alternativas"] if x["correta"]==correta)} for q in a["questoes"] if q["ativo"]]})
def prog():return next(x["progresso"] for x in req("/api/matriculas/minhas","aluno") if x["cursoId"]==cid)
a1=prova("Avaliação 1");responder(a1);check(prog()==100,"Progresso completo após aprovação")
a2=prova("Avaliação 2");check(prog()==0,"Nova avaliação invalida conclusão anterior")
req(f"/api/avaliacoes/{a2}","gestor",{"titulo":"Avaliação 2","ativo":False},method="PUT")
check(prog()==100,"Desativação recalcula aprovação e conclusão")
req(f"/api/avaliacoes/{a2}","gestor",{"titulo":"Avaliação 2","ativo":True},method="PUT")
check(prog()==0,"Reativação obriga aluno a realizar avaliação pendente")
responder(a2);check(prog()==100,"Todas as avaliações realizadas restauram conclusão")
responder(a2,False);check(prog()==0,"Última tentativa reprovada deixa módulo pendente")
req(f"/api/avaliacoes/{a2}","gestor",method="DELETE")
check(prog()==100,"Excluir avaliação reprovada recalcula conclusão usando provas restantes")
req(f"/api/avaliacoes/{a1}","gestor",method="DELETE")
check(prog()==0,"Módulo sem avaliação não fica concluído")
req("/api/certificados","aluno",{"cursoId":cid},status=409)
a3=prova("Avaliação atual");responder(a3);check(prog()==100,"Avaliação atual aprovada")
qextra=questao(a3);check(prog()==0,"Adicionar questão exige realizar novamente a avaliação")
responder(a3);check(prog()==100,"Nova tentativa corresponde às questões atuais")
req(f"/api/questoes/{qextra}","gestor",{"enunciado":"Modificar histórico","ativo":True,"alternativas":[{"texto":"A","correta":True},{"texto":"B","correta":False}]},method="PUT",status=409)
req(f"/api/questoes/{qextra}","gestor",{"ativo":False},method="PUT")
check(prog()==0,"Mudança nas questões não aproveita nota de outra versão da prova")
responder(a3)
req("/api/certificados","aluno",{"cursoId":cid})
cert=next(c["id"] for c in req("/api/certificados","gestor") if c["cursoId"]==cid)
req(f"/api/certificados/{cert}/decisao","gestor",{"aprovar":True})
req(f"/api/cursos/{cid}","gestor",{"nome":"Curso renomeado","descricao":"Teste alterado","cargaHoraria":16,"ativo":True},method="PUT")
req(f"/api/cursos/{cid}","gestor",method="DELETE")
c=req(f"/api/certificados/{cert}","aluno");check(c["curso"]=="Curso complementar" and c["carga"]==8,"Certificado mantém dados da emissão após edição e exclusão")
check(any(x["titulo"]=="Avaliação atual" for x in req("/api/resultados","aluno")),"Histórico de notas preservado após exclusão do curso")
# Dois mentores disputam um chamado: apenas um pode assumir.
call=req("/api/chamados","aluno",{"cursoId":1,"assunto":"Teste de concorrência","texto":"Chamado fictício"},status=201)["id"]
def assumir(role):
    h={"Authorization":"Bearer "+users[role]["token"],"Content-Type":"application/json"}
    try:r=urllib.request.urlopen(urllib.request.Request(B+f"/api/chamados/{call}/assumir",data=b"{}",headers=h),timeout=10)
    except urllib.error.HTTPError as e:r=e
    r.read();return role,r.status
with ThreadPoolExecutor(2) as ex: disputa=list(ex.map(assumir,["mentor","mentor2"]))
check(sorted(s for _,s in disputa)==[200,409],"Concorrência: somente um mentor assume")
winner=next(r for r,s in disputa if s==200);other=next(r for r,s in disputa if s==409)
req(f"/api/chamados/{call}/mensagens",winner,{"texto":"Mentor confirmou o atendimento"})
req(f'/api/administracao/usuarios/{users[winner]["id"]}',"admin",{"ativo":False},method="PUT")
check(next(c for c in req("/api/chamados",other) if c["id"]==call)["status"]=="Aberto","Desativação do responsável devolve chamado à fila")
report=req("/api/relatorios","gestor")
check(any(c["id"]==call and c["responsavel"]==users[winner]["nome"] for c in report["chamadosMentores"]),"Moderação preserva mentor que já respondeu mesmo após desativação")
check(next(x["assumidos"] for x in report["atendimentos"] if x["id"]==users[winner]["id"])>=1,"Contagem de atendimentos preservada após desativação")
req(f"/api/chamados/{call}/assumir",other,{})
req(f"/api/chamados/{call}/mensagens",other,{"texto":"Novo mentor continuou o atendimento"})
check(len([x for x in req("/api/relatorios","gestor")["chamadosMentores"] if x["id"]==call])==2,"Gestão identifica os dois mentores que atenderam a conversa")
req(f'/api/administracao/usuarios/{users[winner]["id"]}',"admin",{"ativo":True},method="PUT")
print(f"CONCLUÍDO: {len(checks)} verificações complementares passaram.")

