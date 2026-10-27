# Site TechLearn Academy

As regras de 06/10/2026 estão em [Documentacao/novas-funcoes.md](../Documentacao/novas-funcoes.md). O código e as rotas atuais incluem certificados, chamados, PDFs e perfis fixos. A referência anterior abaixo permanece como histórico.

HTML monta as páginas, CSS define cores e espaços e JavaScript comunica com a API.
Não há frameworks, CDN, usuários fixos, cursos fictícios nem armazenamento de senhas.

## Execução principal
Execute TechLearnAcademy.Api: ela hospeda este wwwroot na mesma origem.
Abra a URL indicada no terminal. Assim os cookies HttpOnly funcionam sem configuração extra.
A API e SQL Server precisam estar configurados para as funções terem dados reais.

## Prévia separada
Execute dotnet run --project TechLearnAcademy.Web.
O site abre, mas as funções precisam da API. Em wwwroot/js/config.js altere URL_API
para a URL HTTPS real da API, e configure a origem da prévia em OrigensPermitidas no backend.
Use HTTPS também na prévia quando os cookies forem Secure. Prefira a execução principal.

## Organização
api.js: funções pequenas de fetch, mensagens, formulários, perfil e elementos seguros.
publico.js: login, cadastro e recuperação; perfil.js: dados e troca de senha.
aluno.js, mentor.js, gestor.js e administrador.js: ações de cada perfil.
Cada página tem um atributo data-pagina para escolher apenas sua função.
Essa divisão evita repetir códigos, mantendo o HTML visível e fácil de explicar.

## Senha e sessão
As senhas têm pelo menos 15 caracteres, uma maiúscula, um número e um símbolo.
Os espaços são preservados. Nunca usamos localStorage ou sessionStorage.
A API cria uma sessão real e envia um cookie HttpOnly; JavaScript não lê nem salva o token.
Sair pede a revogação dessa sessão no servidor antes de voltar ao login.
A API confere o perfil em cada rota. Esconder links no site não concede permissão.

## Explicação das duas expressões Unicode
\\p{Lu} encontra letras maiúsculas, \\p{N} encontra números.
[^\\p{L}\\p{N}\\s] encontra um caractere que não é letra, número ou espaço.
Isso permite nomes e senhas em diferentes idiomas sem tratar letras acentuadas como símbolos.

## Estados reais
As listas mostram apenas os registros recebidos da API. Uma lista vazia informa isso.
Uma falha de conexão informa que o servidor precisa estar ligado.
Nenhuma resposta de sucesso é inventada pelo site.
