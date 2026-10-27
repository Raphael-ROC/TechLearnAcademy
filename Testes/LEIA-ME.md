# Testes reproduzíveis

testar-integracao.py executa testes reais de HTTP, SQL Server e SMTP local. Não altera o projeto anterior. Não é parte do site: é uma ferramenta opcional de verificação, em Python padrão sem bibliotecas adicionais.

## Banco exclusivo

A ferramenta só conecta a (localdb)\\TechLearnPimIVTeste20261004. Nunca troque este endereço pelo banco usado por vocês. Os testes criam registros temporários no banco TechLearnAcademy desta instância exclusiva. Contas e senhas são geradas em cada execução e não ficam no código do produto.

1. Instale SQL Server LocalDB (é SQL Server, não SQLite) e sqlcmd.
2. Crie uma instância separada: sqllocaldb create TechLearnPimIVTeste20261004.
3. Inicie: sqllocaldb start TechLearnPimIVTeste20261004.
4. Execute Banco/01-criar-banco.sql nessa instância, pelo SSMS em modo SQLCMD. O script recusa banco já existente.
5. Compile TechLearnAcademy.Api.
6. Execute: python Testes/testar-integracao.py --executar.
7. Pare a instância quando terminar: sqllocaldb stop TechLearnPimIVTeste20261004.

HTTP é usado apenas na porta de loopback do teste em Development. Em utilização, use HTTPS. O SMTP do teste recebe a mensagem em memória dentro deste processo, sem enviar para pessoas. Isso verifica o código de envio/redefinição, mas não comprova entrega de e-mail por um provedor externo. Não grave e-mails, senhas ou tokens em prints.

## Evidências

Testes/evidencias/integracao.json registra nomes dos testes aprovados sem credenciais. api-teste.log contém apenas log técnico. A entrega inclui Documentacao/testes-executados.md com os resultados efetivamente verificados. Estes registros não substituem os testes de dispositivo, leitor de tela, publicação e entrega SMTP real.

