# Verificação do desktop — 18/10/2026

Foram aprovadas 54 verificações automatizadas, registradas em evidencias/testes.json. Os testes utilizam o próprio ApiCliente e os formulários C# do produto.

## Testes locais com dados descartáveis

Foi criado um banco SQL Server LocalDB com nome aleatório reservado aos testes. A API C# existente foi executada em HTTPS na porta 7345, com a conexão apontando exclusivamente para esse banco. O banco e o processo foram removidos ao terminar. A validação do certificado HTTPS permaneceu habilitada.

Foram verificados login real, senha incorreta, recusa de Aluno/Mentor/Gestor no cliente administrativo, autorização no servidor, sessão por cookie, saída e revogação de sessão, resumo do banco, busca, paginação, cadastro de Mentor/Gestor, geração do e-mail institucional, edição, preservação de perfil e e-mail institucional, ativação e desativação, contato duplicado e proteção do último Administrador.

Também foram verificados abertura de chamado pelo Gestor, fila administrativa, atendimento, resposta, encerramento, preservação do histórico e bloqueio de respostas depois do encerramento.

Os formulários nativos foram abertos com a API de teste: login, painel, lista de usuários, próxima página, fila de chamados, envio de mensagem pelo botão da conversa, perfil e botão Sair. Foram conferidos os tamanhos dos controles e as capturas de tela. Os nomes exibidos nessas capturas pertencem somente ao banco descartável.

## Verificações na Azure

- A API pública devolveu HTTP 200 em /api/status.
- O painel administrativo devolveu HTTP 401 sem sessão.
- Uma única conta aleatória inexistente recebeu HTTP 401 no login, confirmando a consulta ao banco e a recusa pelo servidor.
- Os arquivos públicos de administração, perfil, chamados e navegação foram conferidos para acompanhar as funções do site.

Houve demora além de 45 segundos na primeira tentativa de login da Azure. A consulta seguinte respondeu em aproximadamente 0,8 segundo. O limite de espera do cliente foi ajustado para 90 segundos para comportar a inicialização do serviço. Isso não altera nem inventa respostas da API.

Em 18/10/2026, Raphael confirmou no aplicativo entregue que o login com a conta real de Administrador foi aceito e que o painel carregou os dados do site publicado na Azure. Essa confirmação manual complementa as 54 verificações automatizadas e não inclui testes de gravação na Azure. Os testes de gravação foram feitos somente no banco descartável local. Não foram criadas contas de teste nem alterados registros no banco publicado.

## Repetir os testes de desenvolvimento

O código de teste está separado em Testes/Desktop. Ele precisa do SQL Server LocalDB, do certificado HTTPS de desenvolvimento confiável e da API compilada em Release. O código de teste não integra o executável entregue e suas contas aleatórias não são contas de acesso ao produto.

As verificações não substituem testes em outras versões do Windows, configurações de escala e leitores de tela. O executável entregue é para Windows de 64 bits.

## Correção de colagem — versão 1.0.1

Foram aprovadas mais 85 verificações específicas de colagem, registradas em evidencias/colagem.json. Os testes criaram os próprios formulários do produto e acionaram os eventos de Ctrl+V e Shift+Insert e o item Colar do menu, usando somente textos de teste na área de transferência do Windows. O conteúdo anterior da área de transferência foi restaurado ao terminar.

Foram conferidos e-mail e senha de login, nome e contato, senha inicial e confirmação do cadastro administrativo, e-mail editável do Aluno, edição do próprio perfil, busca de usuários e mensagem de chamado. Foram preservados os caracteres da senha, inclusive espaços e textos longos, a máscara de senha, o bloqueio do e-mail institucional, os campos desabilitados e o limite de 4000 caracteres do chamado.

Essas verificações não enviaram solicitações à API nem acessaram o banco. Não repetem nem substituem a confirmação anterior do login na Azure. A alteração afeta somente a edição dos campos do desktop.

Para repetir no Windows, execute na raiz da solução:

```powershell
dotnet run --project Testes/DesktopColagem/VerificarColagem.csproj -- Testes/Desktop/evidencias/colagem.json
```