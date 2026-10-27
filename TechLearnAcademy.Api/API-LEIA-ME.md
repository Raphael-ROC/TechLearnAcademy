# API da nova solução

As regras de 06/10/2026 estão em [Documentacao/novas-funcoes.md](../Documentacao/novas-funcoes.md). O código e as rotas atuais incluem certificados, chamados, PDFs e perfis fixos. A referência anterior abaixo permanece como histórico.

Esta API acessa somente o SQL Server indicado pelo operador. Ela nunca executa scripts nem cria contas ao iniciar. O projeto antigo não participa desta solução.

## Configurar e executar

1. Em uma instância SQL Server isolada, execute `Banco/01-criar-banco.sql` no modo SQLCMD do SSMS. O script recusa um banco `TechLearnAcademy` já existente. Não execute no banco do projeto antigo.
2. No PowerShell que iniciará a API, configure a conexão com uma conta SQL de permissões mínimas: `$env:ConnectionStrings__Banco = 'Server=SEU_SERVIDOR;Database=TechLearnAcademy;Integrated Security=True;Encrypt=True;TrustServerCertificate=False'`. Em desenvolvimento isolado, somente se necessário para o certificado local, use `TrustServerCertificate=True`.
3. `dotnet run --project TechLearnAcademy.Api -- --criar-admin` solicita nome, e-mail e senha para criar o primeiro administrador. Não existe conta predefinida. O comando exige que ainda não exista administrador ativo; não inicia o servidor.
4. `dotnet run --project TechLearnAcademy.Api` inicia a API em HTTPS `https://localhost:7291`. O próprio servidor entrega o HTML/CSS/JavaScript do projeto Web. Para HTTPS local, configure o certificado com `dotnet dev-certs https --trust`.
5. Abra o endereço da API no navegador. Cadastre alunos; o Administrador altera o perfil dos usuários que devem ser Mentor ou Gestor e cria vínculos. O Gestor registra cursos e conteúdo; o Aluno se matricula; o Mentor acompanha somente seus vínculos.

A pasta `wwwroot` da API contém apenas um arquivo de conservação da pasta. O SDK precisa dessa pasta durante o início em desenvolvimento. O `Program.cs` procura o `index.html`: em desenvolvimento entrega os arquivos do Web irmão; na publicação entrega o `wwwroot` copiado pelo projeto. Há uma única fonte do site.

`appsettings.json` contém campos vazios, nunca credenciais. Prefira variáveis de ambiente ou segredos do usuário. Conexão/Senhas SMTP não devem entrar no Git. Em produção, use HTTPS e certificado válido. Não exponha SQL Server aos clientes.

## E-mail real

Configure `Email__Servidor`, `Email__Porta`, `Email__Usuario`, `Email__Senha`, `Email__Remetente`, `Email__UsarTls=true` e `Email__UrlRedefinicao=https://SEU_DOMINIO/redefinir-senha.html`. O link enviado vale 30 minutos. A API armazena apenas o hash do código, recusa seu segundo uso e encerra sessões depois da redefinição. Sem SMTP configurado ou se o envio falhar, a API informa HTTP 503; não afirma que enviou um e-mail. O endereço deve ser da nova solução.

## Como explicar as camadas

- Controller recebe os campos e aplica a autorização por perfil.
- Serviço verifica regras: matrícula, vínculo, senha, avaliação ou último administrador ativo.
- Banco executa SQL com parâmetros. Os nomes do SQL estão escritos pelo programa; os valores vêm de parâmetros.
- Modelo declara os campos de cada pedido.

`async` e `await` esperam banco e e-mail sem bloquear o atendimento. Uma transação confirma várias gravações juntas ou desfaz todas se algo falhar. Os bloqueios breves preservam matrícula, avaliação e último administrador diante de pedidos simultâneos. Estas técnicas são necessárias para não produzir registros parciais.

## Sessão e senha

O login confere a senha com PBKDF2-SHA256, 600.000 repetições, sal aleatório de 16 bytes e comparação de tempo constante. A senha original nunca é gravada. A regra é pelo menos 15 caracteres Unicode, uma maiúscula, um número e um símbolo que não seja espaço. Espaços são preservados; não há máximo específico da senha.

O token de sessão tem 32 bytes aleatórios. Seu hash SHA256 fica em `Sessoes`, por oito horas. O navegador recebe um cookie HttpOnly; JavaScript não consegue ler esse cookie. O JSON também inclui o token para clientes API. Não há localStorage, sessionStorage ou token inventado. Cada pedido protegido consulta a sessão no SQL e o perfil atual. Desativar usuário ou alterar seu perfil encerra suas sessões. Sair revoga o token. Trocar senha encerra todas as sessões do usuário.

Em produção o cookie usa Secure com HTTPS. HTTP é permitido somente para desenvolvimento local; nunca use HTTP com credenciais reais fora de uma máquina isolada. CORS só permite as origens explicitamente configuradas em `OrigensPermitidas`. A configuração padrão hospeda tudo na mesma origem, com cookie SameSite Strict e bloqueio de gravações provenientes de origem não autorizada.

## Avaliações e indicadores

Cada questão exige pelo menos duas alternativas e exatamente uma correta. A API envia o gabarito somente ao Gestor. O Aluno recebe id/texto. A correção rejeita respostas incompletas, repetidas e alternativas de outras questões. O servidor calcula acertos, percentual = 100 × acertos / total, nota = 10 × acertos / total, arredondados a duas casas. Percentual de pelo menos 60 aprova e conclui o módulo. Depois de qualquer tentativa, o conteúdo e as alternativas da questão ficam preservados: pode somente desativar e criar outra questão.

O progresso é a quantidade de módulos ativos concluídos dividida pela quantidade de módulos ativos do curso. Um curso sem módulos tem progresso zero. O progresso geral do aluno é a média dos percentuais de suas matrículas ativas. A conclusão manual é idempotente.

O relatório chama `sp_RelatorioCurso`. Média é a média de todas as tentativas; aprovação é a porcentagem de tentativas aprovadas (inclui repetições); matrículas são registros ativos; progresso médio considera matrículas ativas e módulos ativos, incluindo zero em cursos sem módulos. Baixo progresso significa menos de 30%, inclusive alunos ainda sem matrícula. Mentor vê somente alunos vinculados e suas próprias orientações. O Aluno pode consultar suas orientações em `GET /api/aluno/orientacoes`.

## Verificação e limitações

Compilar e abrir o site não demonstram gravação real. SQL Server e SMTP precisam ser configurados pelo operador numa instância nova. Sem esses serviços, as funções mostram erros claros; listas não são substituídas por dados fictícios. A lista de rotas e respostas está em `CONTRATO.md`. O projeto de testes verifica regras locais e disponibilidade sem fingir autenticação. Execute o roteiro de integração com contas criadas por você e registre os resultados reais antes da banca.

O uso de cookie, hash e autorização protege as rotas de dados; arquivos HTML por si só são públicos. Os conteúdos são URLs externas: a API controla quem obtém o cadastro do material, mas não pode proteger um PDF ou vídeo público hospedado por terceiros. Para materiais privados em produção, é necessário um provedor com controle de acesso próprio.
