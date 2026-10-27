# TechLearn Academy

[![Verificar projeto](https://github.com/Raphael-ROC/TechLearnAcademy/actions/workflows/verificar.yml/badge.svg)](https://github.com/Raphael-ROC/TechLearnAcademy/actions/workflows/verificar.yml)

Projeto acadêmico do PIM IV: uma plataforma para estudar cursos, realizar avaliações, acompanhar alunos e administrar o atendimento.

O site, o aplicativo Windows e o aplicativo Android usam a mesma API em C#. A API valida as permissões e acessa o SQL Server.

**[Acessar o sistema no Azure](https://techlearn-proj-pim-hyayf7cehyd3edfq.westus3-01.azurewebsites.net/)** · **[Aplicativos](https://techlearn-proj-pim-hyayf7cehyd3edfq.westus3-01.azurewebsites.net/aplicativos.html)**

## O que cada perfil faz

| Perfil | Principais funções |
| --- | --- |
| Aluno | Matrícula, materiais, avaliações, resultados, orientações, chamados e certificados. |
| Mentor | Acompanhamento dos alunos, orientações e atendimento. |
| Gestor | Gestão de cursos, módulos, materiais, avaliações, relatórios e certificados. |
| Administrador | Gestão de usuários, ativação de contas e atendimento dos gestores. |

O cadastro público cria uma conta de Aluno. A API confere as permissões em cada operação; o menu da interface apenas apresenta as opções.

## Organização dos fontes

| Pasta | Conteúdo |
| --- | --- |
| `TechLearnAcademy.Api` | API ASP.NET Core, autenticação, regras e acesso ao banco. |
| `TechLearnAcademy.Web` | Páginas HTML, CSS e JavaScript do site. |
| `TechLearnAcademy.Desktop` | Aplicativo administrativo em C# e Windows Forms. |
| `TechLearnAcademy.Android` | Aplicativo Android nativo em Java. |
| `TechLearnAcademy.Mobile` | Protótipo anterior em .NET MAUI, mantido como referência. O Android distribuído usa Java. |
| `Banco` | Estrutura, atualizações e exemplos de backup do SQL Server. |
| `TechLearnAcademy.Tests` | Verificações das regras em um programa console C#. |
| `Testes` | Ferramentas complementares de interface e integração. |
| `Documentacao` | Arquitetura, execução, publicação e roteiro de apresentação. |

## Executar no computador

É necessário o SDK .NET 10. Para usar as funções que gravam dados, também é necessário um SQL Server de desenvolvimento.

```powershell
dotnet restore TechLearnAcademy.Api/TechLearnAcademy.Api.csproj
dotnet run --project TechLearnAcademy.Api
```

A API também entrega as páginas do site. O endereço do perfil local está em `TechLearnAcademy.Api/Properties/launchSettings.json`. A configuração do banco e a criação do primeiro administrador estão no [guia de execução](Documentacao/executar-localmente.md).

## Verificações

```powershell
dotnet run --project TechLearnAcademy.Tests
node scripts/verificar-site.mjs
```

No Windows, o desktop pode ser compilado com:

```powershell
dotnet build TechLearnAcademy.Desktop/TechLearnAcademy.Desktop.csproj
```

O Android tem [instruções próprias de compilação](TechLearnAcademy.Android/LEIA-ME.md). Compilar o APK e conferir seu código não substitui instalar e testar em um aparelho.

## Guias do projeto

- [Como o sistema funciona](Documentacao/arquitetura.md)
- [Como executar e configurar](Documentacao/executar-localmente.md)
- [Banco e ordem dos scripts](Banco/README.md)
- [Publicação no Azure](Documentacao/publicacao.md)
- [Roteiro para explicar ao professor](Documentacao/guia-professor.md)
- [Como trabalhar com commits](CONTRIBUTING.md)
- [Conferência desta organização](Documentacao/verificacao.md)

Este repositório reúne os fontes em commits por assunto. Os commits desta importação registram a organização feita agora; não representam datas anteriores de desenvolvimento.

Os instaladores são distribuídos pela página de aplicativos. Backups do banco, dados de usuários, senhas, chaves de assinatura e arquivos de publicação não integram o histórico de fontes.
