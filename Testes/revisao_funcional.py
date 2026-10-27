"""Verificação HTTP no banco fictício TechLearnAcademy_Revisao20261007.
Não execute contra o banco de uso real. Resultado sem senhas ou tokens em tmp/revisao.
"""
import json, urllib.request, urllib.error, socketserver, threading, time, re
from pathlib import Path
from email.parser import BytesParser
from email.policy import default
BASE = "http://127.0.0.1:5407"
ROOT = Path(__file__).resolve().parents[1]
PASS = "Teste123!"
checks = []
def check(ok, nome):
    checks.append({"verificacao": nome, "passou": bool(ok)})
    (ROOT/"tmp/revisao/resultado-http.json").write_text(json.dumps(checks,ensure_ascii=False,indent=2),encoding="utf-8")
    if not ok: raise AssertionError(nome)
def req(path, role=None, data=None, method=None, status=200, raw=None, headers=None):
    h = dict(headers or {})
    if role: h["Authorization"] = "Bearer " + users[role]["token"]
    if data is not None:
        raw = json.dumps(data).encode(); h["Content-Type"]="application/json"
    try:
        r=urllib.request.urlopen(urllib.request.Request(BASE+path,data=raw,headers=h,method=method or ("POST" if raw is not None else "GET")),timeout=12)
    except urllib.error.HTTPError as e: r=e
    body=r.read()
    value = json.loads(body) if "json" in r.headers.get("Content-Type","") else body
    check(r.status == status, f"{role or 'Público'} {method or ('POST' if raw is not None else 'GET')} {path}: HTTP {status} (recebido {r.status})")
    return value
users={}
def login(role,email,password=PASS):
    users[role]=req("/api/usuarios/login",data={"email":email,"senha":password})
    return users[role]
def conta(role,nome,perfil,autor="admin"):
    c=req("/api/administracao/usuarios",autor,{"nome":nome,"emailContato":role+"@revisao.test","perfil":perfil,"senha":PASS,"confirmarSenha":PASS},status=201)
    login(role,c["email"]); return c
def post(path,role,data,status=200): return req(path,role,data,status=status)
def delete(path,role="gestor",status=200): return req(path,role,method="DELETE",status=status)
def upload(mid,nome="aula.pdf",body=b"%PDF-1.4\nDocumento ficticio\n%%EOF\n",status=201,id=0):
    boundary="----TechLearnRevisao"
    data=b""
    fields={"moduloId":str(mid),"titulo":"Documento da aula","ativo":"true"}
    for key,val in fields.items():
        data+=f'--{boundary}\r\nContent-Disposition: form-data; name="{key}"\r\n\r\n{val}\r\n'.encode()
    data+=f'--{boundary}\r\nContent-Disposition: form-data; name="arquivo"; filename="{nome}"\r\nContent-Type: application/pdf\r\n\r\n'.encode()+body+f"\r\n--{boundary}--\r\n".encode()
    return req("/api/materiais/"+(str(id)+"/pdf" if id else "pdf"),"gestor",method="PUT" if id else "POST",status=status,raw=data,headers={"Content-Type":"multipart/form-data; boundary="+boundary})
mailbox=[]
class SMTP(socketserver.StreamRequestHandler):
    def handle(self):
        self.wfile.write(b"220 revisao local\r\n")
        while True:
            line=self.rfile.readline()
            if not line: break
            cmd=line.decode(errors="replace").strip().upper()
            if cmd.startswith(("EHLO","HELO")): self.wfile.write(b"250-revisao\r\n250 SIZE 1500000\r\n")
            elif cmd.startswith("DATA"):
                self.wfile.write(b"354 Envie\r\n")
                data=b""
                while True:
                    line=self.rfile.readline()
                    if line==b".\r\n" or not line: break
                    data+=line
                mailbox.append(BytesParser(policy=default).parsebytes(data)); self.wfile.write(b"250 Aceito\r\n")
            elif cmd.startswith("QUIT"): self.wfile.write(b"221 Bye\r\n"); break
            else: self.wfile.write(b"250 OK\r\n")
server=socketserver.ThreadingTCPServer(("127.0.0.1",2527),SMTP)
threading.Thread(target=server.serve_forever,daemon=True).start()
try:
    login("admin","admin@revisao.test")
    conta("gestor","Gestor Teste","Gestor")
    conta("mentor","Mentor Teste","Mentor")
    conta("mentor2","Outro Mentor","Mentor","gestor")
    conta("gestor2","Outro Gestor","Gestor")
    for role in ["aluno","aluno2"]:
        post("/api/usuarios/cadastro",None,{"nome":"Aluno Teste" if role=="aluno" else "Outro Aluno","email":role+"@revisao.test","senha":PASS,"confirmarSenha":PASS},201)
        login(role,role+"@revisao.test")
    check(users["gestor"]["email"]==f'gestor.teste.{users["gestor"]["id"]}@gestor.techlearn.com',"Gestor: e-mail gerado com nomes e ID")
    check(users["mentor"]["email"]==f'mentor.teste.{users["mentor"]["id"]}@mentor.techlearn.com',"Mentor: e-mail gerado com nomes e ID")
    for senha in ["Teste1!", "teste123!", "Testeabc!", "Teste1234", "Teste 123!"]:
        post("/api/usuarios/cadastro",None,{"nome":"Invalido Teste","email":"senha@revisao.test","senha":senha,"confirmarSenha":senha},400)
    post("/api/administracao/usuarios","gestor",{"nome":"Invalido Perfil","emailContato":"invalido@revisao.test","perfil":"Gestor","senha":PASS,"confirmarSenha":PASS},403)
    post("/api/administracao/usuarios","admin",{"nome":"Invalido Perfil","emailContato":"invalido@revisao.test","perfil":"Administrador","senha":PASS,"confirmarSenha":PASS},403)
    check(len(req("/api/administracao/usuarios","admin")["itens"])==7,"Admin vê todos os sete usuários")
    check(all(x["perfil"] in ["Aluno","Mentor"] for x in req("/api/administracao/usuarios","gestor")["itens"]),"Gestor lista apenas alunos e mentores")
    req("/api/administracao/usuarios","aluno",status=403)
    req(f'/api/administracao/usuarios/{users["gestor"]["id"]}',"gestor",{"nome":"Alterado"},method="PUT",status=403)
    req(f'/api/administracao/usuarios/{users["aluno2"]["id"]}',"gestor",{"nome":"Aluno Editado","email":"aluno2@revisao.test","emailContato":"aluno2@revisao.test","perfil":"Mentor","ativo":True},method="PUT")
    login("aluno2","aluno2@revisao.test")
    check(req("/api/usuarios/perfil","aluno2")["perfil"]=="Aluno","Perfil enviado na edição é ignorado: permanece Aluno")
    req(f'/api/administracao/usuarios/{users["mentor2"]["id"]}',"gestor",{"nome":"Mentor Editado","email":"mudar@revisao.test","emailContato":"mentor2@revisao.test","ativo":True},method="PUT")
    login("mentor2",users["mentor2"]["email"])
    check(req("/api/usuarios/perfil","mentor2")["nome"]=="Mentor Editado","Gestor edita dados do mentor")
    req(f'/api/administracao/usuarios/{users["admin"]["id"]}',"admin",{"ativo":False},method="PUT",status=409)
    post("/api/usuarios/trocar-senha","admin",{"senhaAtual":PASS,"senha":PASS,"confirmarSenha":PASS},403)
    r=req("/api/relatorios","gestor"); check(r["cursos"]==[] and isinstance(r["atendimentos"],list) and r["chamadosMentores"]==[],"Relatório vazio tem listas válidas")
    cid=post("/api/cursos","gestor",{"nome":"Curso ADS","descricao":"Curso de teste para a revisão funcional","cargaHoraria":40},201)["id"]
    mods=[post("/api/modulos","gestor",{"cursoId":cid,"titulo":f"Módulo {i}","ordem":i},201)["id"] for i in range(1,5)]
    post("/api/modulos","gestor",{"cursoId":cid,"titulo":"Quinto módulo","ordem":5},409)
    post("/api/matriculas","aluno",{"cursoId":cid},201)
    post("/api/matriculas","aluno",{"cursoId":cid},409)
    pdf=upload(mods[0])["id"]
    check(req(f"/api/materiais/{pdf}/arquivo","aluno").startswith(b"%PDF-"),"Aluno baixa PDF que foi enviado ao banco")
    req(f"/api/materiais/{pdf}/arquivo","aluno2",status=403)
    upload(mods[0],"falso.pdf",b"texto nao PDF",400)
    upload(mods[0],"aula.txt",b"%PDF-",400)
    upload(mods[0],body=b"%PDF-"+b"x"*(10*1024*1024),status=400)
    upload(mods[0],body=b"%PDF-1.4\nVersao nova\n%%EOF\n",id=pdf,status=200)
    req(f"/api/materiais/{pdf}","gestor",{"titulo":"PDF editado","tipo":"PDF","url":"","ativo":True},method="PUT")
    check(b"Versao nova" in req(f"/api/materiais/{pdf}/arquivo","aluno"),"Editar título preserva o arquivo enviado")
    links=[post("/api/materiais","gestor",{"moduloId":mods[0],"titulo":f"Link {i}","tipo":"Link","url":"https://example.test/aula"},201)["id"] for i in range(3)]
    check(len(req(f"/api/modulos/{mods[0]}/materiais","aluno"))==4,"Vários materiais no mesmo módulo")
    post("/api/materiais","gestor",{"moduloId":mods[0],"titulo":"Invalido","tipo":"Link","url":"javascript:alert(1)"},400)
    delete(f"/api/materiais/{links[0]}")
    check(len(req(f"/api/modulos/{mods[0]}/materiais","aluno"))==3,"Material excluído desaparece")
    avs=[]; questions=[]
    for i,mid in enumerate(mods):
        aid=post("/api/avaliacoes","gestor",{"moduloId":mid,"titulo":f"Avaliação {i+1}"},201)["id"]; avs.append(aid)
        qid=post("/api/questoes","gestor",{"avaliacaoId":aid,"enunciado":"Escolha a resposta certa","alternativas":[{"texto":"Certa","correta":True},{"texto":"Errada","correta":False}]},201)["id"]
        questions.append(qid)
    post("/api/questoes","gestor",{"avaliacaoId":avs[0],"enunciado":"Mais de cinco","alternativas":[{"texto":str(i),"correta":i==0} for i in range(6)]},400)
    req(f"/api/avaliacoes/{avs[0]}","gestor",{"titulo":"Avaliação inicial","ativo":True},method="PUT")
    req(f"/api/questoes/{questions[0]}","gestor",{"enunciado":"Escolha a certa, de A até E","ativo":True,"alternativas":[{"texto":str(i),"correta":i==0} for i in range(5)]},method="PUT")
    check(len(req(f"/api/avaliacoes/{avs[0]}","gestor")["questoes"][0]["alternativas"])==5,"Aceita cinco alternativas")
    exam=req(f"/api/avaliacoes/{avs[0]}","aluno")
    check("correta" not in exam["questoes"][0]["alternativas"][0],"Aluno não recebe o gabarito antes da prova")
    def responder(aid,correta=True):
        exam=req(f"/api/avaliacoes/{aid}","gestor")
        return post(f"/api/avaliacoes/{aid}/respostas","aluno",{"respostas":[{"questaoId":q["id"],"alternativaId":next(a["id"] for a in q["alternativas"] if a["correta"]==correta)} for q in exam["questoes"] if q["ativo"]]})
    fail=responder(avs[0],False); check(not fail["aprovado"] and fail["nota"]==0,"Nota abaixo de seis reprova")
    post("/api/progresso","aluno",{"moduloId":mods[0]},409)
    post("/api/certificados","aluno",{"cursoId":cid},409)
    passed=[responder(aid) for aid in avs]
    check(req("/api/matriculas/minhas","aluno")[0]["progresso"]==100,"Aprovação nos quatro módulos conclui automaticamente o curso")
    fail2=responder(avs[0],False)
    check(req("/api/matriculas/minhas","aluno")[0]["progresso"]==75,"Reprovação posterior exige refazer o módulo")
    responder(avs[0])
    extra=post("/api/avaliacoes","gestor",{"moduloId":mods[0],"titulo":"Avaliação complementar"},201)["id"]
    post("/api/certificados","aluno",{"cursoId":cid},409)
    post("/api/questoes","gestor",{"avaliacaoId":extra,"enunciado":"Complementar","alternativas":[{"texto":"Sim","correta":True},{"texto":"Não","correta":False}]},201)
    responder(extra)
    post("/api/certificados","aluno",{"cursoId":cid})
    cert=req("/api/certificados","gestor")[0]["id"]
    post("/api/certificados","aluno",{"cursoId":cid},409)
    req(f"/api/certificados/{cert}","aluno",status=404)
    post(f"/api/certificados/{cert}/decisao","aluno",{"aprovar":True},403)
    post(f"/api/certificados/{cert}/decisao","gestor",{"aprovar":False,"motivo":"Confirmação de teste"})
    check(req("/api/certificados","aluno")[0]["status"]=="Recusado","Aluno recebe motivo da recusa do certificado")
    post("/api/certificados","aluno",{"cursoId":cid})
    responder(extra,False)
    post(f"/api/certificados/{cert}/decisao","gestor",{"aprovar":True},409)
    responder(extra)
    post(f"/api/certificados/{cert}/decisao","gestor",{"aprovar":True})
    emitted=req(f"/api/certificados/{cert}","aluno")
    check(emitted["curso"]=="Curso ADS" and emitted["codigo"].startswith("TL-"),"Gestor aprova e gera certificado com código e dados")
    req(f"/api/certificados/{cert}","aluno2",status=404)
    support=post("/api/chamados","aluno",{"cursoId":cid,"assunto":"Dúvida da aula","texto":"Preciso de ajuda com o módulo"},201)["id"]
    check(any(x["id"]==support for x in req("/api/chamados","mentor")),"Chamado do curso entra na fila do mentor")
    post(f"/api/chamados/{support}/mensagens","mentor",{"texto":"Sem assumir"},403)
    post(f"/api/chamados/{support}/assumir","mentor",{})
    post(f"/api/chamados/{support}/assumir","mentor2",{},409)
    req(f"/api/chamados/{support}","mentor2",status=403)
    post(f"/api/chamados/{support}/mensagens","mentor",{"texto":"Vamos revisar o material juntos"})
    post(f"/api/chamados/{support}/mensagens","aluno",{"texto":"Obrigado, tenho outra pergunta"})
    check(len(req(f"/api/chamados/{support}","gestor")["mensagens"])==2,"Gestor acompanha mentor, aluno e conversa para moderação")
    check(req("/api/mentor/alunos","mentor")[0]["id"]==users["aluno"]["id"],"Mentor vê somente aluno atendido")
    req(f'/api/mentor/alunos/{users["aluno"]["id"]}',"mentor2",status=403)
    req(f'/api/resultados/{passed[0]["id"]}',"mentor")
    req(f'/api/resultados/{passed[0]["id"]}',"mentor2",status=403)
    check(req("/api/aluno/orientacoes","aluno")[0]["mentor"]=="Mentor Teste","Resposta do mentor registrada como orientação")
    ouv_aluno=post("/api/chamados","aluno",{"assunto":"Ajuda da gestão","texto":"Solicitação fictícia"},201)["id"]
    ouv_mentor=post("/api/chamados","mentor",{"assunto":"Ouvidoria do mentor","texto":"Solicitação fictícia"},201)["id"]
    for id in [ouv_aluno,ouv_mentor]:
        post(f"/api/chamados/{id}/assumir","gestor",{})
        post(f"/api/chamados/{id}/mensagens","gestor",{"texto":"Atendimento da gestão"})
    post(f"/api/chamados/{ouv_mentor}/encerrar","gestor",{})
    post(f"/api/chamados/{ouv_mentor}/mensagens","mentor",{"texto":"Após encerrado"},409)
    adm_call=post("/api/chamados","gestor",{"assunto":"Solicitação ao admin","texto":"Precisamos de apoio da administração"},201)["id"]
    check(any(x["id"]==adm_call for x in req("/api/chamados","admin")),"Admin recebe chamado da gestão")
    post(f"/api/chamados/{adm_call}/assumir","admin",{})
    post(f"/api/chamados/{adm_call}/mensagens","admin",{"texto":"Admin respondeu à gestão"})
    post(f"/api/chamados/{adm_call}/mensagens","gestor",{"texto":"Gestor confirmou o recebimento"})
    req(f"/api/chamados/{adm_call}","aluno",status=403)
    req(f"/api/chamados/{adm_call}","gestor2",status=403)
    report=req("/api/relatorios","gestor")
    stats=next(x for x in report["atendimentos"] if x["id"]==users["gestor"]["id"])
    check(stats["assumidos"]==2 and stats["encerrados"]==1,"Relatório conta chamados assumidos e encerrados por gestor")
    check(report["chamadosMentores"][0]["responsavel"]=="Mentor Teste","Relatório relaciona mentor e aluno atendido")
    listalunos=req(f"/api/relatorios/cursos/{cid}/alunos","gestor")
    check(len(listalunos["alunos"])==1,"Relatório apresenta alunos por curso")
    detail=req(f'/api/relatorios/cursos/{cid}/alunos/{users["aluno"]["id"]}',"gestor")
    check(detail["aprovadoCurso"] and len(detail["historico"])>=8 and all(a["nota"]==10 for a in detail["avaliacoes"]),"Relatório detalha módulos, avaliações, notas e todas as tentativas")
    req(f'/api/relatorios/cursos/{cid}/alunos/{users["aluno"]["id"]}',"aluno",status=403)
    req(f'/api/administracao/usuarios/{users["mentor2"]["id"]}',"admin",{"ativo":False},method="PUT")
    req("/api/usuarios/perfil","mentor2",status=401)
    post("/api/usuarios/login",None,{"email":users["mentor2"]["email"],"senha":PASS},401)
    req(f'/api/administracao/usuarios/{users["mentor2"]["id"]}',"admin",{"ativo":True},method="PUT")
    login("mentor2",users["mentor2"]["email"])
    # Envio SMTP inteiramente local: não usa contas ou serviços externos.
    for role in ["aluno2","mentor2","gestor2"]:
        before=len(mailbox)
        post("/api/usuarios/recuperar-senha",None,{"email":users[role]["email"]})
        check(len(mailbox)==before+1 and mailbox[-1]["To"]==role+"@revisao.test",f"{role}: recuperação enviada ao contato pessoal pelo SMTP local")
        token=re.search(r"token=([A-F0-9]+)",mailbox[-1].get_content()).group(1)
        post("/api/usuarios/redefinir-senha",None,{"token":token,"senha":"Nova123!","confirmarSenha":"Nova123!"})
        req("/api/usuarios/perfil",role,status=401)
        login(role,users[role]["email"],"Nova123!")
        post("/api/usuarios/redefinir-senha",None,{"token":token,"senha":PASS,"confirmarSenha":PASS},400)
        post("/api/usuarios/trocar-senha",role,{"senhaAtual":"Nova123!","senha":PASS,"confirmarSenha":PASS})
        login(role,users[role]["email"])
    before=len(mailbox);post("/api/usuarios/recuperar-senha",None,{"email":users["admin"]["email"]})
    check(before==len(mailbox),"Admin não recebe recuperação de senha")
    # Exclusões verificadas em outro curso; Curso ADS continua disponível para a inspeção visual.
    dc=post("/api/cursos","gestor",{"nome":"Curso para excluir","descricao":"Dados fictícios de exclusão","cargaHoraria":8},201)["id"]
    dm=post("/api/modulos","gestor",{"cursoId":dc,"titulo":"Módulo descartável","ordem":1},201)["id"]
    da=post("/api/avaliacoes","gestor",{"moduloId":dm,"titulo":"Prova descartável"},201)["id"]
    delete(f"/api/avaliacoes/{da}")
    check(req(f"/api/modulos/{dm}/avaliacoes","gestor")==[],"Exclui avaliação")
    delete(f"/api/modulos/{dm}")
    req(f"/api/modulos/{dm}/materiais","gestor",status=404)
    dm2=post("/api/modulos","gestor",{"cursoId":dc,"titulo":"Módulo substituto","ordem":1},201)["id"]
    delete(f"/api/cursos/{dc}")
    req(f"/api/cursos/{dc}","gestor",status=404)
    check(all(x["id"]!=dc for x in req("/api/cursos?todos=true","gestor")["itens"]),"Curso excluído desaparece da lista")
    req(f"/api/modulos/{dm2}/materiais","gestor",status=404)
    delete(f"/api/cursos/{cid}","aluno",403)
    # Contrato e cache das páginas públicas.
    for path in ["/index.html","/usuarios.html","/cadastrar-usuario.html","/relatorios.html","/js/api.js","/css/estilo.css"]:
        r=urllib.request.urlopen(BASE+path,timeout=10)
        check("no-store" in r.headers.get("Cache-Control",""),f"{path}: não mantém arquivos antigos em cache")
        if path.endswith(".html"): check(b'meta name="versao-site"' in r.read(),f"{path}: versão do site inserida pelo servidor")
    meta={"curso":cid,"modulos":mods,"avaliacoes":avs,"certificado":cert,"chamadoMentor":support,"chamadoAdmin":adm_call,"aluno":users["aluno"]["id"],"gestorEmail":users["gestor"]["email"],"mentorEmail":users["mentor"]["email"]}
    (ROOT/"tmp/revisao/dados-ui.json").write_text(json.dumps(meta,ensure_ascii=False,indent=2),encoding="utf-8")
    print(f"CONCLUÍDO: {len(checks)} verificações HTTP passaram, com banco e SMTP fictícios.")
except Exception:
    print(f"FALHOU após {len(checks)} verificações. Consulte resultado-http.json.")
    raise
finally: server.shutdown(); server.server_close()

