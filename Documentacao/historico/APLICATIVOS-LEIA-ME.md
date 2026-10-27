# Aplicativos C# — TechLearn Academy

## Desktop do Administrador — atualizado em 18/10/2026

O desktop agora usa formulários nativos em C# com Windows Forms e .NET 10. É exclusivo do Administrador e acessa a mesma API e o mesmo banco usados pelo site na Azure.

Abra output/Desktop-Administrador/TechLearnAcademy.Desktop.exe e utilize sua conta administrativa do site. A publicação inclui o .NET e não exige Visual Studio, WebView2 ou SQL Server no computador do usuário.

O programa oferece visão geral, usuários, busca e paginação, cadastro de Mentor e Gestor, edição, ativação e desativação, chamados, perfil e saída. Os controles são os padrões do Windows Forms e as cores seguem o site.

Para estudar ou executar pelo Visual Studio, abra TechLearnAcademy.Desktop/TechLearnAcademy.Desktop.csproj. A explicação de cada arquivo e a configuração do endereço estão em TechLearnAcademy.Desktop/LEIA-ME.md. servidor.txt contém somente a URL HTTPS pública da API.

O aplicativo recebe JSON por HTTPS. A API confere sessão e permissões e acessa o SQL Server. O desktop não contém conexão SQL nem senha do banco. O cookie de sessão permanece na memória e nenhuma senha é salva em arquivo.

## Mobile existente

O aplicativo mobile continua em .NET MAUI e abre o site em uma WebView. Essa parte não foi modificada nesta entrega.

1. No Visual Studio com suporte a .NET 10, instale a carga de trabalho .NET MAUI.
2. Abra TechLearnAcademy.slnx e escolha TechLearnAcademy.Mobile.
3. Selecione Android ou Windows e execute. Android exige SDK e dispositivo ou emulador.
4. Informe o endereço HTTPS publicado e pressione Abrir plataforma.

No celular, localhost aponta para o próprio celular. Use o domínio publicado. O certificado HTTPS deve ser confiável. No mobile, use Sair antes de fechar quando compartilhar o aparelho.

O desktop e o mobile precisam de conectividade com a API. Nenhum deles cria um banco SQL local ou oferece modo offline.
