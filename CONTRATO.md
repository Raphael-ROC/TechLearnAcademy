# Contrato da nova solução independente

As regras de 06/10/2026 estão em [Documentacao/novas-funcoes.md](Documentacao/novas-funcoes.md). O código e as rotas atuais incluem certificados, chamados, PDFs e perfis fixos. A referência anterior abaixo permanece como histórico.

Esta solução segue o TXT do PIM IV. O projeto antigo não será alterado.

## Decisões comuns

- .NET 10. API com Controllers, Servicos, Banco e Modelos, Microsoft.Data.SqlClient e SQL Server.
- Web HTML/CSS/JavaScript sem frameworks/CDN. A API também serve a pasta do Web na mesma origem; Web separado é prévia estática.
- fetch com URLs relativas /api e credentials: 'include'; cookie HttpOnly com token aleatório real, hash em Sessoes, logout revoga. Login JSON também devolve token. Nunca localStorage/sessionStorage.
- Cadastro público sempre Aluno. Senha mínima 15 caracteres, maiúscula, número e símbolo; espaços preservados; regras iguais no servidor e nos clientes.
- Nenhum usuário/senha/curso fixo. Primeiro administrador criado por comando explícito com dados do operador.
- Recuperação real via SMTP, token de uso único e 30 minutos, armazenado como hash. Não expor token em API. SMTP ausente gera 503, sem sucesso fictício.
- JSON camelCase. Erros {mensagem}. Cursos e usuários com paginação {itens,pagina,totalPaginas,total}.
- Mobile MAUI exibe a interface responsiva hospedada. O Desktop do Administrador usa telas nativas C# em Windows Forms. Ambos consultam a API e nenhum cliente recebe conexão SQL.

## Público e perfil

POST /api/usuarios/cadastro {nome,email,senha,confirmarSenha} -> 201 {mensagem}.
POST /api/usuarios/login {email,senha} -> {mensagem,id,nome,email,perfil,token} e cookie; inválido 401.
POST /api/usuarios/recuperar-senha {email} -> {mensagem}.
POST /api/usuarios/redefinir-senha {token,senha,confirmarSenha} -> {mensagem}.
GET /api/status -> {mensagem}.
GET /api/usuarios/perfil -> {id,nome,email,perfil,ativo}.
PUT /api/usuarios/perfil {nome,email} -> {mensagem}.
POST /api/usuarios/trocar-senha {senhaAtual,senha,confirmarSenha} -> {mensagem}.
POST /api/usuarios/logout -> {mensagem}, revoga sessão.
GET /api/status/banco -> {mensagem}, somente Administrador.

## Cursos

GET /api/cursos?busca=&pagina=1 -> {itens:[{id,nome,descricao,cargaHoraria,ativo}],pagina,totalPaginas,total}. Gestor consulta inativos com todos=true.
GET /api/cursos/{id} -> {id,nome,descricao,cargaHoraria,ativo,matriculado,modulos:[{id,titulo,ordem,ativo,concluido}]}.
POST /api/cursos (Gestor) {nome,descricao,cargaHoraria} -> {id,mensagem}.
PUT /api/cursos/{id} (Gestor) {nome,descricao,cargaHoraria,ativo} -> {mensagem}.
POST /api/modulos (Gestor) {cursoId,titulo,ordem} -> {id,mensagem}.
PUT /api/modulos/{id} (Gestor) {titulo,ordem,ativo} -> {mensagem}.
GET /api/modulos/{id}/materiais -> [{id,moduloId,titulo,tipo,url,ativo}], só aluno matriculado ou Gestor.
POST /api/materiais (Gestor) {moduloId,titulo,tipo,url} -> {id,mensagem}, tipo PDF ou Link, URL http/https.
PUT /api/materiais/{id} (Gestor) {titulo,tipo,url,ativo} -> {mensagem}.
POST /api/matriculas (Aluno) {cursoId} -> 201 {mensagem}, usa procedure.
GET /api/matriculas/minhas -> [{id,cursoId,nome,descricao,cargaHoraria,progresso}].
POST /api/progresso (Aluno) {moduloId} -> {mensagem}, matriculado, idempotente.
GET /api/aluno/painel -> {nome,cursosMatriculados,progressoGeral,avaliacoesRecentes:[{id,titulo,nota,aprovado}]}.

## Avaliações

GET /api/modulos/{id}/avaliacoes -> [{id,moduloId,titulo,ativo}], aluno matriculado ou Gestor.
POST /api/avaliacoes (Gestor) {moduloId,titulo} -> {id,mensagem}.
PUT /api/avaliacoes/{id} (Gestor) {titulo,ativo} -> {mensagem}.
GET /api/avaliacoes/{id} -> {id,moduloId,titulo,questoes:[{id,enunciado,alternativas:[{id,texto}]}]}; correta só para Gestor.
POST /api/questoes (Gestor) {avaliacaoId,enunciado,alternativas:[{texto,correta}]} -> {id,mensagem}.
PUT /api/questoes/{id} (Gestor) {enunciado,ativo,alternativas:[{texto,correta}]} -> {mensagem}; proteger histórico se houve tentativa, permitir desativar.
POST /api/avaliacoes/{id}/respostas (Aluno) {respostas:[{questaoId,alternativaId}]} -> {id,acertos,total,percentual,nota,aprovado}. Correção no servidor, rejeitar incompletas/duplicadas/alternativas de outras questões; aprovação 60%, nota 0–10. Aprovação conclui módulo.
GET /api/resultados -> [{id,avaliacaoId,titulo,acertos,total,percentual,nota,aprovado,data}], somente próprios.
GET /api/resultados/{id} -> {id,titulo,acertos,total,percentual,nota,aprovado,progresso,respostas:[{enunciado,texto,correta,respostaCorreta}]}, dono ou Mentor vinculado.

## Mentor

GET /api/mentor/alunos -> [{id,nome,email,progresso,media}], somente vínculos ativos.
GET /api/mentor/alunos/{id} -> {id,nome,email,cursos:[{cursoId,nome,progresso}],resultados:[{id,titulo,nota,aprovado}],orientacoes:[{id,texto,data}]}.
POST /api/mentor/orientacoes {alunoId,texto} -> {mensagem}, vínculo existente.

## Gestor

GET /api/relatorios -> {quantidadeCursos,totalMatriculas,mediaGeral,aprovacao,progressoMedio,cursos:[{cursoId,nome,matriculas,media,aprovacao}],alunosBaixoProgresso:[{id,nome,progresso}]}.
Usar sp_RelatorioCurso e definir métricas no README.

## Administrador

GET /api/administracao/painel -> {ativos,inativos,perfis:[{perfil,quantidade}]}.
GET /api/administracao/usuarios?busca=&pagina=1 -> {itens:[{id,nome,email,perfil,ativo}],pagina,totalPaginas,total}.
PUT /api/administracao/usuarios/{id} {perfil,ativo} -> {mensagem}. Preservar último administrador ativo e auditar por trigger.
GET /api/administracao/vinculos -> [{id,mentorId,mentorNome,alunoId,alunoNome,ativo}].
POST /api/administracao/vinculos {mentorId,alunoId} -> {mensagem}, perfis reais exigidos.
PUT /api/administracao/vinculos/{id} {ativo} -> {mensagem}.

## Páginas obrigatórias

Públicas: index.html, cadastro.html, recuperar-senha.html, redefinir-senha.html.
Comum: perfil.html.
Aluno: painel-aluno.html, cursos.html, curso.html, meus-cursos.html, estudar.html, avaliacao.html, resultado.html.
Mentor: painel-mentor.html, alunos-mentor.html, acompanhamento-aluno.html, orientacao.html.
Gestor: painel-gestor.html, gerenciar-cursos.html, gerenciar-conteudo.html, gerenciar-avaliacoes.html, relatorios.html.
Administrador: painel-administrador.html, usuarios.html, editar-usuario.html. Vínculos mentor/aluno na página usuarios.html.
Todas consumirão respostas reais; listas vazias têm mensagens claras.

## Banco e documentação

Banco TechLearnAcademy; 15 tabelas do TXT mais Sessoes. Não executar script no banco antigo. PK/FK/UNIQUE/CHECK/índices, sp_RealizarMatricula, sp_RelatorioCurso, trigger AuditoriaUsuarios.
Documentar MER, lógico/físico, dicionário, arquitetura, inclusão/social, acessibilidade, negócio com estimativas identificadas, nuvem/DevOps/backup, backlog/sprints e testes com limitações reais.
