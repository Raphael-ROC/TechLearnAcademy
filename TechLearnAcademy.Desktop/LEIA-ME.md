# TechLearn Academy — Desktop do Administrador

Aplicativo nativo em C#, Windows Forms e .NET 10. As telas usam Label, TextBox, Button, ComboBox, CheckBox, TabControl e DataGridView. As cores azul e verde vêm do site. Não utiliza WebView, navegador interno, biblioteca de interface ou acesso direto ao SQL Server.

## Abrir o executável

1. Extraia o ZIP em uma pasta do Windows 10 ou 11 de 64 bits.
2. Abra TechLearnAcademy.Desktop.exe.
3. Entre com a mesma conta Administrador que funciona no site.

Versão 1.0.2: o login não exibe mais o endereço do servidor. As janelas podem ser maximizadas e os campos acompanham o tamanho disponível. os campos editáveis aceitam Ctrl+V, Shift+Insert e botão direito → Colar, inclusive as senhas. O texto copiado é inserido sem alterar seus caracteres. Os campos que já eram somente de leitura continuam bloqueados.

O executável publicado inclui o .NET necessário. Não é preciso instalar Visual Studio, WebView2 ou SQL Server para utilizá-lo. É necessário ter internet e manter API e banco na Azure disponíveis. A primeira conexão pode demorar enquanto o serviço inicia.

## Funções disponíveis

| Área | Funções do Administrador |
| --- | --- |
| Visão geral | Consultar usuários ativos, inativos e quantidade por perfil; atualizar os dados; conferir o banco pela API. |
| Usuários | Buscar por nome ou e-mail, navegar entre páginas, editar nome e contato, ativar e desativar contas. |
| Cadastrar usuário | Criar Mentor ou Gestor. A API gera o e-mail institucional e confere a senha inicial. |
| Chamados | Ver todos, abertos, próprios ou disponíveis; assumir atendimento, ler a conversa, responder, atualizar e encerrar. |
| Meu perfil | Consultar a conta e editar nome e e-mail pessoal de contato. |
| Sair | Revogar a sessão na API, limpar o cookie da memória e voltar ao login. Fechar a janela também tenta revogar a sessão. |

Os chamados e a conversa recebem atualização a cada cinco segundos quando a respectiva tela está em uso. O texto em digitação é preservado. O Administrador atende os chamados encaminhados pelos Gestores.

O perfil de uma conta permanece fixo depois do cadastro, como no site atual. O e-mail de acesso de Mentor, Gestor e Administrador também é fixo. O e-mail de acesso do Aluno pode ser editado. A API impede a desativação do último Administrador ativo. Uma edição administrativa revoga as sessões da conta alterada; se for a própria conta, o programa retorna ao login.

O site não oferece troca ou recuperação de senha ao Administrador. O desktop segue essa mesma regra. Alunos continuam se cadastrando pelo site, e somente Mentor e Gestor aparecem no cadastro administrativo.

## Ligação com os dados

Desktop C# → solicitação HTTPS → API C# na Azure → Azure SQL Database.

O programa envia e recebe JSON. A API verifica o login, a sessão e as permissões antes de consultar ou gravar os dados. Assim, uma alteração feita no desktop aparece no site ao atualizar a página.

Endereço configurado:

https://techlearn-proj-pim-hyayf7cehyd3edfq.westus3-01.azurewebsites.net/

servidor.txt contém somente esse endereço público. Se a hospedagem mudar, altere esse arquivo e reinicie o aplicativo. Use a raiz HTTPS da aplicação. O link do portal Azure não é o endereço da API. Se servidor.txt estiver ausente, o programa usa o endereço Azure compilado em Configuracao.cs.

Nenhuma senha, conta fixa, chave do banco ou token falso está no código do produto. O cookie de sessão real permanece na memória. As senhas não são salvas em arquivo nem em configurações. A validação normal do certificado HTTPS permanece habilitada.

## Abrir o código no Visual Studio

1. Instale a carga de trabalho Desenvolvimento para desktop com .NET e o SDK .NET 10.
2. Abra TechLearnAcademy.Desktop.csproj, ou escolha esse projeto na solução TechLearnAcademy.Desktop.slnx.
3. Defina TechLearnAcademy.Desktop como projeto de inicialização e pressione F5.
4. Para gerar o executável, use Publicar e o perfil Windows-x64. A saída será entregas/Desktop na raiz da solução.

Também é possível publicar pelo terminal:

```powershell
dotnet publish TechLearnAcademy.Desktop/TechLearnAcademy.Desktop.csproj -p:PublishProfile=Windows-x64
```

## Organização do código

| Arquivo | Função |
| --- | --- |
| Program.cs | Abre o login e depois a janela administrativa. |
| Configuracao.cs e servidor.txt | Informam o endereço público da API. |
| ApiCliente.cs | Envia JSON, recebe respostas e mantém o cookie de sessão em memória. |
| Modelos.cs | Define as classes correspondentes às respostas JSON. |
| Validacao.cs | Confere nome, e-mail e a regra de senha já usada no site. |
| Estilo.cs | Reúne as cores, os controles padrão, os atalhos de colagem e o menu Colar. |
| TelaLogin.cs | Recebe e-mail e senha e permite somente Administrador. |
| TelaPrincipal.cs | Organiza as abas, listas, buscas, resumo e saída. |
| TelaUsuario.cs | Reúne cadastro, edição de usuário e edição dos próprios dados. |
| TelaChamado.cs | Mostra a conversa e envia as ações do atendimento. |
| Properties/PublishProfiles/Windows-x64.pubxml | Configura a publicação do executável com o .NET incluído. |

As posições dos controles ficam nos construtores das telas. Os métodos abaixo cuidam dos cliques e das solicitações. Não foram criadas camadas ou padrões adicionais.

## Explicação curta para a banca

“O desktop foi feito em C# com Windows Forms. Utilizamos os controles padrão e as cores da plataforma. O Administrador entra com a mesma conta do site. Os botões enviam solicitações à API, e a API acessa o banco da Azure. O programa não guarda a senha e não tem a senha do banco.”

“async e await permitem aguardar a resposta do servidor sem travar a janela. try e catch apresentam uma mensagem quando algo falha. finally libera os controles no fim da operação.”

“O método Enviar recebe o caminho, o método HTTP e os dados. A letra T indica a classe da resposta, como Usuario ou ResumoAdmin. Essa função comum evita repetir a comunicação em cada botão.”

## Referências técnicas

- [Publicação de executável único](https://learn.microsoft.com/en-us/dotnet/core/deploying/single-file/)
- [Comunicação com HttpClient](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/httpclient-guidelines)
