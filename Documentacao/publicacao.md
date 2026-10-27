# Publicação no Azure

## Ambiente consultado em 27/10/2026

| Componente | Recurso |
| --- | --- |
| Grupo de recursos | `techlearn-pim` |
| App Service | `techlearn-proj-pim` |
| Plataforma | Windows, .NET 10 |
| Banco | `TechLearnAcademy_PIMIV`, no Azure SQL |
| Site | https://techlearn-proj-pim-hyayf7cehyd3edfq.westus3-01.azurewebsites.net/ |
| Downloads | `/aplicativos.html` |

O grupo também possui recursos de Azure Communication Services e de e-mail. Senhas, strings de conexão e chaves pertencem à configuração do ambiente e não acompanham o Git.

## Fontes e publicação

Os arquivos do site foram comparados por leitura HTTP. A API foi conferida com os símbolos e com análise estática do pacote publicado. A integração de e-mail e as regras de chamados que faltavam nas fontes locais foram incorporadas à cópia nova.

Este repositório reúne fontes organizados; não é uma cópia binária do servidor. O tratamento local de erros SQL foi preservado e uma dependência com alerta foi atualizada. Consulte os limites em `verificacao.md`.

A organização do GitHub não republicou o App Service, não executou SQL no Azure e não alterou contas. Não foi criado deploy automático.

## Gerar o pacote

```powershell
dotnet publish TechLearnAcademy.Api/TechLearnAcademy.Api.csproj -c Release -o artifacts/api
```

O `.csproj` da API inclui os arquivos de `TechLearnAcademy.Web/wwwroot`. O projeto Web separado é uma prévia estática e não inclui a API do banco.

Confira as variáveis no App Service antes de uma futura publicação:

| Variável | Finalidade |
| --- | --- |
| `ConnectionStrings__Banco` | Conexão privada com o Azure SQL. |
| `Email__ConexaoAzure` | Conexão do serviço de e-mail do Azure. |
| `Email__Remetente` | Remetente autorizado. |
| `Email__UrlRedefinicao` | URL HTTPS pública de `redefinir-senha.html`. |
| `Email__Servidor`, `Email__Porta`, `Email__Usuario`, `Email__Senha`, `Email__UsarTls` | Alternativa SMTP. |

Com `Email__ConexaoAzure` configurada, o código usa o Azure; caso contrário, usa SMTP. Não envie a configuração da produção ao Git.

## Instaladores e materiais

Os três links de `aplicativos.html` usam pacotes em `/aplicativos/`. Esses pacotes não são gerados por `dotnet publish` da API. Preserve-os na hospedagem ou envie novas versões separadamente. Uma publicação que apague arquivos adicionais pode remover os downloads existentes.

O desktop tem o perfil `TechLearnAcademy.Desktop/Properties/PublishProfiles/Windows-x64.pubxml`. O Android pode ser compilado pelo Gradle; assinatura e instalação em aparelho são etapas separadas. Não versione chaves privadas.

Os materiais PDF e os registros dos alunos ficam no banco. Clonar o Git e executar o workflow não recria esses dados.

## Verificação e implantação

O workflow `Verificar projeto` compila e confere os fontes em cada envio à `main` e nos pull requests. Ele não tem credencial do Azure e não altera o banco.

Para uma futura implantação, confira os testes, prepare os pacotes, valide em ambiente separado e revise as configurações antes de atualizar produção. Depois confira `/api/status`, as telas, os downloads e os fluxos de cada perfil.
