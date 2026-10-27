# Instalação neste computador

Consulte [LEIA-ME-LOCAL.md](LEIA-ME-LOCAL.md) para os caminhos e a configuração instalados neste computador.

# TechLearn Academy — PIM IV

## Correções de chamados e avaliações — 11/10/2026

Chamados enviados ficam para acompanhamento até alguém assumir. Somente o responsável encerra o atendimento. E-mails pessoais de recuperação não podem pertencer a duas contas.

Cada avaliação pode ser respondida uma vez por tentativa do curso. Se reprovar, o aluno precisa usar **Reiniciar curso**, que zera o progresso de todos os módulos e mantém as notas antigas no histórico. O banco atual já recebeu a atualização `07-corrigir-recuperacao-e-avaliacoes.sql`.

Para carregar as alterações em C#, pare a execução no Visual Studio (**Shift+F5**) e inicie novamente (**F5**). Veja [o guia simples](Documentacao/novas-funcoes.md).

## Cursos cadastrados em 11/10/2026

Os seis PDFs enviados foram organizados em **7 cursos, 25 módulos e 50 matérias em PDF**, com **25 avaliações de revisão**. Todos foram cadastrados no banco SQL utilizado pelo projeto, mantendo os dados anteriores.

Entre como **Aluno → Catálogo** para escolher um curso. Como **Gestor**, use **Cursos**, **Conteúdo** e **Avaliações** para consultar ou editar o cadastro. Consulte [a organização completa dos conteúdos](Documentacao/cursos-conteudos.md).

## Revisão de 07/10/2026

Lista de usuários e cadastro agora têm páginas separadas. Chat, PDF, relatórios por aluno, certificados e exclusões foram conferidos. Páginas abertas e conversas recebem atualizações automaticamente.

Confira [a revisão completa dos pedidos](Documentacao/revisao-exigencias.md) e [o guia simples](Documentacao/novas-funcoes.md). Inicie **TechLearnAcademy.Api** pelo Visual Studio (**F5**) para abrir o site em https://localhost:7291. Execute apenas uma instância da API. O envio real de recuperação depende de configurar o serviço de e-mail.

Os procedimentos abaixo incluem a versão anterior.

**Nova solução independente.** O projeto anterior e o TXT de referência não foram alterados.

Abra **TechLearnAcademy.slnx** no Visual Studio com suporte a .NET 10. A solução contém cinco projetos: Web, API C#, desktop Windows Forms, mobile .NET MAUI e testes C#. O banco é SQL Server; os scripts são executados no SQL Server Management Studio (SSMS).

## Leia primeiro

- Documentacao/requisitos.md: requisitos do TXT e onde cada um foi atendido.
- Documentacao/testes-executados.md: resultados efetivamente verificados e testes ainda pendentes.
- Documentacao/roteiro-banca.md: falas e capturas sugeridas para a apresentação.
- Documentacao/relatorio-academico.md: relatório editável, com estimativas identificadas como hipóteses.
- Documentacao/banco.md: MER, modelo lógico/físico e dicionário completo.
- TechLearnAcademy.Api/API-LEIA-ME.md: API, segurança, configuração e endpoints.
- TechLearnAcademy.Web/README-Web.md: páginas, HTML, CSS, JavaScript e acessibilidade.
- APLICATIVOS-LEIA-ME.md: executar mobile e desktop.

O PDF é uma versão de leitura do relatório. Preencha instituição, curso, orientador e dados acadêmicos antes de entregar à faculdade. Confira o manual institucional: esta versão não certifica conformidade integral com todas as normas ABNT.

## 1. Preparar o Visual Studio

Instale as cargas de trabalho:
- ASP.NET e desenvolvimento Web;
- Desenvolvimento para desktop com .NET;
- Desenvolvimento de interface do usuário de aplicativo multiplataforma .NET MAUI, incluindo Android SDK.

Instale SQL Server ou SQL Server LocalDB e SSMS. LocalDB usa o mecanismo SQL Server. Instale sqlcmd apenas se quiser executar os testes automatizados.

O HTML/CSS/JavaScript não usa frameworks, bibliotecas ou CDN. Microsoft.Data.SqlClient é o conector oficial necessário à API. O mobile usa .NET MAUI. O desktop administrativo usa apenas Windows Forms e os componentes do próprio .NET.

## 2. Criar um banco novo sem atingir o anterior

O script Banco/01-criar-banco.sql cria o banco TechLearnAcademy e **recusa execução se esse nome já existir** na instância escolhida. Use uma instância nova ou isolada. Não apague o banco anterior para fazer o script funcionar.

Opção simples para desenvolvimento com LocalDB, no terminal do Visual Studio:

    sqllocaldb create TechLearnPimIV
    sqllocaldb start TechLearnPimIV

No SSMS, conecte-se a **(localdb)\TechLearnPimIV**, com Autenticação do Windows. Abra Banco/01-criar-banco.sql. Ative Consulta > Modo SQLCMD e execute. A estrutura contém 16 tabelas, índices, dois procedimentos e trigger de auditoria. Não existem contas ou cursos preenchidos automaticamente.

Para usar uma instância SQL Server normal, escolha uma instância sem banco TechLearnAcademy anterior e informe o servidor real na conexão. O código e os scripts continuam sendo SQL Server.

## 3. Configurar a conexão

Clique com o botão direito em TechLearnAcademy.Api > Gerenciar segredos do usuário. Se o Visual Studio pedir inicialização, execute na pasta da API:

    dotnet user-secrets init

Use sua conexão real. Exemplo apenas para a instância isolada LocalDB:

    {
      "ConnectionStrings": {
        "Banco": "Server=(localdb)\\TechLearnPimIV;Database=TechLearnAcademy;Integrated Security=true;Encrypt=true;TrustServerCertificate=true"
      }
    }

No arquivo JSON, a barra é escrita duas vezes. O valor lido pelo C# contém uma única barra antes de TechLearnPimIV.

TrustServerCertificate=true é uma exceção do SQL local de desenvolvimento. Em produção, use certificado SQL confiável e TrustServerCertificate=false. HTTPS do site deve ter certificado confiável.

Também é possível configurar a variável de ambiente ConnectionStrings__Banco. Não coloque senha do SQL nem senha do SMTP no código ou em um arquivo enviado à banca.

## 4. Configurar e-mail verdadeiro

Nos segredos da API, configure Email:Servidor, Porta, Remetente, Usuario, Senha e UsarTls. Configure Email:UrlRedefinicao para o endereço publicado de redefinir-senha.html. Veja TechLearnAcademy.Api/API-LEIA-ME.md.

Sem servidor SMTP configurado, a recuperação informa erro de configuração. Ela não inventa e-mail enviado nem mostra token de recuperação publicamente. Não fornecemos credenciais de provedor.

## 5. Criar o primeiro administrador

Na pasta TechLearnAcademy.Api, execute no terminal:

    dotnet run --launch-profile TechLearnAcademy.Api -- --criar-admin

Esse perfil ativa Development e permite ler a conexão definida nos segredos do usuário.

Informe nome, e-mail e senha próprios. A senha não aparece no terminal. O comando recusa a criação quando já existe Administrador ativo. Ele grava hash, não a senha. O cadastro público sempre cria Aluno.

Entre com esse Administrador, cadastre as demais pessoas pela tela pública e altere seus perfis na tela de usuários. Não existe conta fixa. Os perfis possíveis são Aluno, Mentor, Gestor e Administrador. Administrador também configura vínculos entre mentores e alunos.

A senha deve ter pelo menos 15 caracteres, uma maiúscula, um número e um caractere especial. Espaços são preservados. A regra é igual no frontend e na API da nova solução.

## 6. Executar o site

1. Escolha TechLearnAcademy.Api como projeto de inicialização.
2. Selecione o perfil TechLearnAcademy.Api e execute com Ctrl+F5.
3. Abra https://localhost:7291/.
4. Aceite somente o certificado de desenvolvimento emitido pelo .NET no seu computador. Se necessário, execute dotnet dev-certs https --trust.
5. Faça login com uma conta criada por vocês.

A API entrega as páginas do projeto Web e os endpoints /api no mesmo endereço. Isso facilita a comunicação e o cookie de sessão. Não é necessário iniciar dois servidores para a demonstração.

Pare a API antiga antes de iniciar esta solução se ela também estiver usando a porta 7291.

TechLearnAcademy.Web pode ser iniciado sozinho para visualizar o HTML. Nesse modo, configure URL_API e OrigensPermitidas se precisar da integração, conforme README-Web.md. Prefira o modo pela API para a banca.

## 7. Fluxo de demonstração

1. Administrador: configure perfis e vínculos.
2. Gestor: crie curso, módulo, URL de PDF/link, avaliação e questões com uma alternativa correta.
3. Aluno: matricule-se, estude, conclua módulo e responda avaliação.
4. Mentor: acompanhe apenas seus alunos e registre orientação.
5. Gestor: consulte os indicadores e relatórios.
6. Administrador: altere situação de uma conta e confira o impedimento de login.

Não há dados de exemplo escondidos no produto. Sem registros criados, as telas mostram mensagens de lista vazia. Materiais são URLs de arquivos ou páginas externas. Não há upload, chat, pagamento, certificado ou streaming próprio.

## 8. Aplicativos e publicação

O mobile usa uma WebView. O desktop do Administrador tem formulários nativos C# em Windows Forms, ligados à mesma API e banco na Azure. Veja APLICATIVOS-LEIA-ME.md e TechLearnAcademy.Desktop/LEIA-ME.md.

No celular, localhost aponta para o celular. Use endereço HTTPS do servidor com rede e certificado confiáveis. Os aplicativos nunca conectam diretamente ao SQL Server.

Para publicar a API com o site:

    dotnet publish TechLearnAcademy.Api/TechLearnAcademy.Api.csproj -c Release -o publicacao

Configure os segredos no servidor e habilite HTTPS. Não execute o script SQL automaticamente durante publicação. Veja Documentacao/relatorio-academico.md para o plano de nuvem e DevOps. Publicar exige conta, infraestrutura e revisão próprios; não foi criada uma hospedagem em seu nome.

## Organização para explicar

- HTML: estrutura dos formulários, listas, títulos e rótulos.
- CSS: cores, espaçamento, foco e adaptação a telas menores.
- JavaScript: validações, chamadas fetch e apresentação da resposta real.
- Controllers: recebem pedidos HTTP e exigem perfil.
- Servicos: regras de cadastro, matrícula, correção e autorização.
- Banco/ConexaoBanco.cs: conexão e consultas com parâmetros.
- Banco/*.sql: estrutura, índices, procedimentos, trigger e manutenção.
- Mobile: janela C# com a interface web compartilhada. Desktop: formulários nativos C# exclusivos do Administrador.
- TechLearnAcademy.Tests: verificações simples de validações, hash e token; escolha esse projeto e execute com Ctrl+F5.
- Documentacao: requisitos, banco, planejamento, negócio, testes e roteiro.

As poucas técnicas necessárias além de condições simples são explicadas: async/await espera operações sem bloquear; transação evita gravação incompleta; PBKDF2 protege a senha; token aleatório representa sessão real; autorização no servidor impede acesso indevido mesmo quando alguém abre uma página diretamente.
# Revisão de 07/10/2026

Lista de usuários e cadastro separados, chat automático, PDF visível, relatórios por aluno e atualização automática das páginas.
Confira [a revisão completa dos pedidos](Documentacao/revisao-exigencias.md) e [o guia simples](Documentacao/novas-funcoes.md).

