# Testes realmente executados — 4 de outubro de 2026

Atualização de 06/10/2026: consulte [novas-funcoes.md](novas-funcoes.md) para as regras atuais de chamados, certificados, perfis fixos, senhas e materiais. As descrições e evidências abaixo registram a versão anterior.

Esta é a nova solução independente. O projeto anterior e o TXT original foram preservados. Na conferência anterior à entrega, os hashes SHA256 dos 13 arquivos originais comparados permaneceram iguais. Nenhum teste de banco foi realizado na instância ou no banco do projeto anterior.

## Ambiente

- Windows, SDK .NET 10.0.401 e SQL Server LocalDB 17.
- Instância exclusiva: `(localdb)\TechLearnPimIVTeste20261004`.
- API em C# executada em Development, HTTP de loopback `127.0.0.1:5299`, somente para testes locais.
- SMTP: receptor local que recebeu mensagens em memória; não enviou e-mail para pessoas.
- Contas e senhas de teste geradas durante a execução; sem credenciais fixas no produto ou nas evidências.
- Navegador: interface da API real e SQL real, sem substituir respostas por sucesso simulado.

## Compilação

`dotnet build TechLearnAcademy.slnx --no-restore` terminou com **zero erros e zero avisos**, incluindo:

| Projeto | Resultado |
|---|---|
| TechLearnAcademy.Api — C#/ASP.NET Core | Compilou |
| TechLearnAcademy.Web — HTML/CSS/JavaScript | Compilou |
| TechLearnAcademy.Desktop — C#/Windows Forms | Compilou |
| TechLearnAcademy.Mobile — C#/MAUI Android | Compilou |
| TechLearnAcademy.Mobile — C#/MAUI Windows | Compilou |
| TechLearnAcademy.Tests — testes console C# | Compilou |

Após a correção de abertura dos materiais externos no mobile, os dois destinos MAUI foram recompilados sem erros ou avisos. Compilar não comprova a execução em aparelho nem a utilização do programa desktop.

A publicação Release da API foi gerada com `dotnet publish`. Ela foi executada localmente em Development: as 24 páginas HTML responderam 200, incluindo a raiz do site e o cadastro. O status do banco sem sessão respondeu 401, conforme a autorização exigida. Não houve publicação em nuvem ou IIS.

## Testes C# — 22 verificações aprovadas

O executável `TechLearnAcademy.Tests` foi executado. Verificou regra de senha, confirmação, preservação de espaços, hash/sal, conferência de senha, geração/hash de token, limites de nome, normalização/formato de e-mail e URLs de material. As 22 verificações passaram. Ele não utiliza banco nem finge autenticação.

## Integração real — 47 eventos aprovados

As evidências sem dados sensíveis estão em:

- `Testes/evidencias/integracao.json`: 30 eventos aprovados.
- `Testes/evidencias/adicionais.json`: 17 eventos aprovados.

Esses 47 eventos, somados às 22 verificações C#, totalizam **69 verificações registradas**. Não significam 69 requisitos integralmente validados: alguns eventos agrupam chamadas HTTP, e outros requisitos possuem cobertura parcial.

### Cadastro, sessão e permissões

- Cadastro público sempre Aluno, mesmo quando o pedido tenta enviar outro perfil.
- E-mail duplicado recusado; senha curta recusada; senha incorreta devolve 401.
- Pedido sem sessão ou com token aleatório recusado.
- Permissões de Aluno, Mentor, Gestor e Administrador conferidas no servidor.
- Conta inativa não entra e perde acesso; último Administrador ativo não pode ser removido dessa condição.
- Origem não autorizada recusada; logout revoga sessão.
- SQL armazena hash PBKDF2 com sal e hash de token, não senha original.
- Trigger registra alterações de perfil e situação.

### Curso, matrícula, mentoria e relatório

- Consulta real de curso; acesso ao conteúdo exige matrícula.
- Matrícula gravada no SQL; repetição sequencial recusada.
- Dois pedidos de matrícula concorrentes: um foi criado e o outro recusado; apenas uma matrícula ativa permaneceu no banco.
- Mentor vinculado registra orientação lida pelo aluno; Mentor sem vínculo recebe recusa.
- Relatório consulta registros reais.

### Avaliação, resultado e progresso

- Questão exige pelo menos duas alternativas e exatamente uma correta.
- Gabarito oculto do Aluno antes do envio.
- Respostas incompletas, questão repetida e alternativa de outra questão recusadas; não deixam tentativas parciais.
- Resultado pertence ao próprio Aluno; acesso de outro Aluno recusado.
- Acerto total produziu nota 10, aprovação e progresso de módulo.
- Dois acertos em cinco produziram 40%, nota 4 e reprovação, sem concluir o módulo.
- Três acertos em cinco produziram 60%, nota 6 e aprovação no limite, concluindo o módulo.
- Um acerto em 32 produziu 3,13% e nota 0,31.
- Edição de questão já respondida recusada com 409; enunciado, respostas, gabarito e nota preservados.
- Desativação da questão alterou sua situação e conservou o histórico.

### Recuperação de senha

- Código de envio SMTP entregou o link ao receptor local.
- Redefinição alterou a senha real e revogou sessões anteriores.
- Token usado novamente e token vencido foram recusados.
- Sem configuração SMTP, a API devolveu 503; não afirmou envio fictício.

## Navegador e arquivos web

As 24 páginas foram abertas por HTTP com resultado 200. Foram conferidos 157 caminhos locais de links/recursos e a sintaxe dos oito arquivos JavaScript. Não foram encontrados identificadores HTML duplicados na verificação; 42 rótulos associados foram conferidos.

No navegador, com a API e o SQL reais, foram executados:

1. Cadastro pela tela, mensagem de sucesso e limpeza dos campos.
2. Login com senha incorreta, exibindo a mensagem de erro.
3. Login correto e redirecionamento por perfil para Aluno, Mentor, Gestor e Administrador.
4. Jornada do Aluno: catálogo, curso, matrícula, módulo, listagem de material, resposta de avaliação, resultado e progresso.
5. Abertura dos quatro painéis com indicadores reais e saída para o login.
6. Painel do Aluno em 390 × 844 pixels, sem transbordamento horizontal.
7. Foco por Tab no link de pular para o conteúdo do login.

As sete capturas reais ficam em `Documentacao/prints`. Não incluem senha, token ou conexão SQL. O roteiro da banca também sugere outras capturas, ainda a produzir pelo grupo.

## SQL Server, backup e restauração

A primeira execução de criação exigiu correção das opções SET para os índices filtrados. Depois da correção, **o script inteiro** foi executado do início ao fim em um banco vazio separado, `TechLearnAcademyValidacao20261004`, na mesma instância exclusiva. Apenas o nome de destino foi ajustado nessa cópia de teste.

Foram confirmados 16 tabelas, 19 índices adicionais, duas procedures e uma trigger. Isso não prova todos os cenários possíveis de cada restrição.

O backup `COPY_ONLY` com `CHECKSUM` foi executado e aprovado por `RESTORE VERIFYONLY`. Ele foi restaurado em **outro banco novo**, `TechLearnAcademy_Restauracao`, sem sobrescrever a origem. A contagem foi de 10 usuários na origem e 10 na cópia restaurada naquele momento. Arquivos de dados e backup não são incluídos na entrega do código-fonte.

O comando C# `--criar-admin` foi testado no banco vazio de validação: criou o primeiro Administrador com os dados informados pelo operador. A senha não foi exibida no terminal. Uma segunda execução foi recusada com a mensagem “Já existe um Administrador ativo. Use a tela de usuários.” e código de saída 1.

## O que ainda precisa ser testado

- Execução do MAUI em dispositivo/emulador e do Windows Forms com runtime WebView2.
- Compatibilidade simultânea das três interfaces em rede HTTPS confiável.
- Entrega por provedor SMTP externo, TLS/autenticação e caixa postal de destino.
- Todos os formulários administrativos no navegador e demais cenários não citados acima.
- Leitor de tela, navegação completa por teclado, zoom e auditoria de acessibilidade.
- Carga/desempenho com grande volume, outros cenários concorrentes e cobertura completa das restrições SQL.
- Publicação HTTPS em IIS/nuvem, proxy, certificado e rede SQL privada.
- Aceite institucional da opção MAUI/Windows Forms com WebView e revisão do formato acadêmico.

As contas e os dados de teste pertencem exclusivamente ao ambiente isolado. Não existem contas preenchidas automaticamente ao iniciar o produto. O projeto não foi declarado pronto para operação pública.
