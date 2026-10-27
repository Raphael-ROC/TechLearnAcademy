# Executar localmente

## Pré-requisitos

- SDK .NET 10.
- SQL Server de desenvolvimento e SQL Server Management Studio para os fluxos com dados. No Windows, pode ser usado SQL Server Express LocalDB.
- Node.js para a verificação dos arquivos do site.
- Windows para o aplicativo desktop.
- Android Studio, JDK e SDK Android para o aplicativo Java.

## 1. Obter o projeto

```powershell
git clone https://github.com/Raphael-ROC/TechLearnAcademy.git
cd TechLearnAcademy
```

## 2. Criar um banco de desenvolvimento

No SSMS, conecte-se à sua instância local. Consulte a [ordem dos scripts](../Banco/README.md) antes de executar qualquer arquivo.

Para uma instalação nova com o nome usado pelo site, execute `Banco/Instalar-banco-do-site.sql`. Depois aplique `Banco/05-atualizar-funcionalidades.sql` e `Banco/07-corrigir-recuperacao-e-avaliacoes.sql` no banco criado. Os scripts devem ser executados com interrupção em caso de erro.

O arquivo 06 é opcional e depende de PDFs externos que não fazem parte dos fontes. A aplicação pode iniciar sem esse catálogo; criar conteúdo real é uma operação administrativa separada.

Esses passos são para um banco novo de desenvolvimento. Não apontam para o Azure nem restauram dados de usuários.

## 3. Configurar a conexão

`appsettings.Development.json` contém apenas um exemplo de LocalDB com autenticação do Windows. Se usar outra instância, defina a conexão somente no seu terminal:

```powershell
$env:ConnectionStrings__Banco = 'Server=SEU_SERVIDOR;Database=TechLearnAcademy_PIMIV;Integrated Security=true;Encrypt=true;TrustServerCertificate=true'
```

`TrustServerCertificate=true` é usado no exemplo local. Para o Azure SQL, use uma conexão com validação do certificado e configure a credencial no App Service.

Para um SQL Server com usuário e senha, mantenha a string fora do Git. O arquivo `appsettings.Local.json` está ignorado, mas não é carregado automaticamente pelo programa; use variáveis de ambiente ou um provedor de segredos configurado explicitamente.

## 4. Executar a API e o site

```powershell
dotnet restore TechLearnAcademy.Api/TechLearnAcademy.Api.csproj
dotnet run --project TechLearnAcademy.Api
```

Confira os endereços impressos no terminal. O perfil local usa `https://localhost:7291` e/ou o endereço HTTP indicado no `launchSettings.json`. Abra `/api/status` para conferir a disponibilidade da API; esse endpoint não testa a conexão com o banco.

Caso o certificado de desenvolvimento não esteja configurado, configure-o com a ferramenta oficial do .NET antes de usar HTTPS.

## 5. Criar o primeiro administrador

Somente em um banco novo, pare a API e execute:

```powershell
dotnet run --project TechLearnAcademy.Api -- --criar-admin
```

O programa pede nome, e-mail e senha no terminal. Não há senha inicial fixa no repositório. Depois inicie normalmente. O cadastro público cria somente Aluno.

## Desktop e Android

O desktop lê a URL pública em `TechLearnAcademy.Desktop/servidor.txt`. Para usar uma API local, ajuste esse arquivo na sua cópia e confira o certificado antes de testar.

```powershell
dotnet run --project TechLearnAcademy.Desktop
```

Para o Android, abra `TechLearnAcademy.Android` no Android Studio e siga o [guia da pasta](../TechLearnAcademy.Android/LEIA-ME.md). O endereço HTTPS está em `Api.java`. O `localhost` do celular aponta para o próprio celular.

## Testar sem alterar o banco

```powershell
dotnet run --project TechLearnAcademy.Tests
node scripts/verificar-site.mjs
```

O primeiro comando verifica regras em memória; o segundo confere a sintaxe JavaScript e referências locais do HTML. Os scripts de integração em `Testes` exigem leitura e configuração prévias porque criam dados em uma instância exclusiva de teste.
