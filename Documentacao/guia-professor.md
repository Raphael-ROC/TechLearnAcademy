# Roteiro para apresentar o TechLearn Academy

## Apresentação inicial

“O TechLearn Academy é uma plataforma acadêmica de cursos. O aluno se matricula, acessa materiais e faz avaliações. O mentor acompanha o aprendizado, o gestor organiza os cursos e o administrador cuida das contas e do atendimento dos gestores.”

## Explicar a arquitetura

“O projeto tem três interfaces: site, desktop Windows e Android. Todas conversam por HTTPS com a mesma API em C#. Somente a API acessa o SQL Server. Assim, as permissões e regras ficam centralizadas.”

Mostre `Documentacao/arquitetura.md` e siga uma solicitação: tela de login → cliente HTTP → controller → serviço → conexão com o banco.

Arquivos que ajudam a explicar:

| Assunto | Onde mostrar |
| --- | --- |
| Inicialização e configuração | `TechLearnAcademy.Api/Program.cs` |
| Rotas de usuários | `TechLearnAcademy.Api/Controllers/UsuariosController.cs` |
| Regras de senha e token | `TechLearnAcademy.Api/Servicos/Regras.cs` |
| Sessão e permissões | `TechLearnAcademy.Api/Servicos/SessaoHandler.cs` e atributos dos controllers |
| Consultas com parâmetros | `TechLearnAcademy.Api/Banco/ConexaoBanco.cs` |
| Comunicação do site | `TechLearnAcademy.Web/wwwroot/js/api.js` |
| Comunicação do desktop | `TechLearnAcademy.Desktop/ApiCliente.cs` |
| Comunicação do Android | `TechLearnAcademy.Android/app/src/main/java/br/com/techlearn/academy/Api.java` |

## Git, GitHub e Azure

- **Git:** registra versões dos arquivos. Cada commit representa uma mudança que pode ser consultada e comparada.
- **GitHub:** hospeda o repositório, mostra o histórico e executa as verificações configuradas em Actions.
- **Azure App Service:** executa a API e entrega o site publicado.
- **Azure SQL:** guarda os dados utilizados pela aplicação.

“Organizei a importação dos fontes por assunto: configuração, banco, API, telas, aplicativos, testes e documentação. Os commits foram feitos na data da organização. Esse histórico não pretende representar datas passadas do desenvolvimento.”

Antes da apresentação, escolha um fluxo e leia os arquivos envolvidos até conseguir explicar a solicitação, a validação e a resposta.

## Demonstração de cinco a dez minutos

1. Abra o README e explique o objetivo e os quatro perfis.
2. Abra o sistema publicado e mostre a navegação. Para entrar, use uma conta autorizada, sem mostrar a senha.
3. Mostre um fluxo que já tenha conferido antes da apresentação, como consultar cursos ou resultados. Se houver gravação, use um ambiente e uma conta preparados para demonstração.
4. Abra um controller e mostre onde o perfil é exigido.
5. Abra uma regra simples, por exemplo a validação de senha, e explique condição e mensagem de erro.
6. Mostre a estrutura SQL e como as entidades se relacionam.
7. Abra o histórico de commits e compare uma alteração.
8. Mostre as verificações em Actions e explique seus limites.

Não crie usuários ou altere o banco em produção apenas para provar que uma tela existe. Combine os dados e o ambiente da demonstração previamente.

## Perguntas comuns

**Por que não acessar o banco diretamente pelo aplicativo?**

Porque isso exigiria distribuir credenciais do banco e repetiria regras em cada interface. A API concentra a validação e o acesso aos dados.

**O que é JSON?**

É o formato de texto usado nas solicitações e respostas entre as interfaces e a API.

**O que muda entre autenticação e autorização?**

Autenticação identifica a conta. Autorização confere o que essa conta pode fazer.

**A senha fica salva em texto?**

O servidor armazena hash com sal. Os clientes enviam a senha por HTTPS para a autenticação; não incluem credenciais do banco.

**O commit publica o sistema no Azure?**

Não. Este repositório tem verificação automática, mas a publicação no App Service é uma etapa separada. Não foi conectado um deploy automático por esta organização.

**O Android está comprovadamente funcionando em celular?**

Há fontes, compilação e verificações automatizadas. Instalação e uso em aparelho são uma validação separada; veja `Documentacao/verificacao.md` para o que foi conferido nesta entrega.

## Preparar a apresentação

Leia o código que vai mostrar, execute os comandos do README, ensaie o fluxo e confira se a internet está disponível. Se algum ponto ainda depender de configuração, explique a dependência com clareza.
