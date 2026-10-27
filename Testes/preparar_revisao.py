"""Prepara SOMENTE o banco isolado da revisão. Nunca modifica o banco do site."""
from pathlib import Path
import hashlib, base64, os
raiz = Path(__file__).resolve().parents[1]
pasta = raiz / "tmp" / "revisao"
pasta.mkdir(parents=True, exist_ok=True)
nome = "TechLearnAcademy_Revisao20261007C"
sql = (raiz / "Banco" / "Instalar-banco-do-site.sql").read_text(encoding="utf-8-sig").replace("TechLearnAcademy_PIMIV", nome)
sal = os.urandom(16)
hash_senha = "600000." + base64.b64encode(sal).decode() + "." + base64.b64encode(hashlib.pbkdf2_hmac("sha256", b"Teste123!", sal, 600000)).decode()
sql += "\nGO\nINSERT INTO Usuarios(Nome,Email,EmailContato,SenhaHash,Perfil) VALUES(N'Admin Teste','admin@revisao.test','admin@revisao.test','" + hash_senha + "','Administrador');\nGO\n"
(pasta / "instalar.sql").write_text(sql, encoding="utf-8")
print("Script do banco isolado preparado: " + nome)
