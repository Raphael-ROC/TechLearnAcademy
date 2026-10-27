# Testes C# simples

Este projeto console verifica as funções de validação, hash, token e URL da API. Não usa banco, SMTP, usuário fixo ou autenticação simulada.

No Visual Studio, escolha TechLearnAcademy.Tests como projeto de inicialização e pressione Ctrl+F5. Cada verificação imprime PASSOU. Se uma condição não for atendida, o programa termina com erro e mostra FALHOU.

No terminal:

    dotnet run --project TechLearnAcademy.Tests/TechLearnAcademy.Tests.csproj

Não usa framework externo de testes, portanto não aparece como suite no Test Explorer. É um executável didático. A integração HTTP e SQL real fica nas ferramentas da pasta Testes/, com resultados em Documentacao/testes-executados.md.

