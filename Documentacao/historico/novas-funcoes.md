# Novas funções — guia simples

## Abrir o sistema

O banco `TechLearnAcademy_PIMIV` recebeu as correções de chamados, recuperação e avaliações em 11/10/2026.
Os cadastros existentes foram mantidos. A cópia anterior desta instalação está em .local/TechLearnAcademy_PIMIV-antes-integracao-Luis-20261011.bak.

Para iniciar, selecione **TechLearnAcademy.Api** como projeto de inicialização no Visual Studio e pressione **F5**. Depois, [abra o site](https://localhost:7291/index.html). Execute apenas uma instância da API para evitar o erro de porta ocupada.
O site recebe os arquivos atuais sem cache antigo. Uma página já aberta confere a versão a cada cinco segundos e se atualiza automaticamente. Se houver um formulário em digitação, aguarda o salvamento e mantém o rascunho.
Mudanças no código C# exigem reiniciar a execução da API; os arquivos HTML, CSS e JavaScript passam a aparecer automaticamente.

### Conexão com o banco local

O site usa o banco `TechLearnAcademy_PIMIV` em `(localdb)\MSSQLLocalDB`. A conexão aguarda até 30 segundos enquanto o SQL inicia. O fechamento automático do banco foi desativado para evitar reabri-lo entre os acessos do site, conforme a [recomendação da Microsoft](https://learn.microsoft.com/pt-br/sql/relational-databases/policy-based-management/set-the-auto-close-database-option-to-off?view=sql-server-ver17).

Em 07/10/2026, a consulta do login foi conferida na própria execução do Visual Studio. Um endereço fictício recebeu “E-mail ou senha inválidos”, confirmando que a consulta ao banco funcionou. Nenhuma conta foi criada ou alterada nessa conferência.

Em outro computador que já tenha o banco antigo completo, abra `Banco/05-atualizar-funcionalidades.sql` no SQL Server Management Studio e pressione F5.
Esse arquivo atualiza o banco existente. Confira o nome do banco na primeira linha.
Para uma instalação nova, use `Banco/Instalar-banco-do-site.sql`.

## O que cada usuário faz

| Perfil | Funções |
| --- | --- |
| Aluno | Estuda, faz avaliações, pede ajuda ao mentor dentro do curso, abre ouvidoria com o gestor e solicita certificado. |
| Mentor | Escolhe um chamado disponível de aluno, responde na conversa e registra orientações. Pode abrir ouvidoria com o gestor. |
| Gestor | Cadastra mentor, edita dados de aluno/mentor, gerencia cursos e materiais, atende ouvidoria, aprova certificados e consulta notas e atendimentos. Abre chamado para o administrador. |
| Administrador | Cadastra gestor/mentor, edita dados, ativa ou desativa contas e atende os chamados dos gestores. O perfil de uma conta permanece fixo. |

## Curso, módulos e aprovação

1. O gestor cadastra um curso.
2. Em **Conteúdo**, adiciona até **quatro módulos**.
3. Cada módulo pode ter vários materiais, por link ou por documento PDF de até 10 MB.
4. Em **Avaliações**, o gestor cadastra questões com **duas a cinco alternativas**, identificadas automaticamente como A, B, C, D e E. Marca uma correta.
5. O aluno precisa de **60% de acertos**, equivalente à nota **6**, em cada avaliação.
6. Quando passa em todas as avaliações do módulo, o módulo é concluído automaticamente.
7. Cada avaliação pode ser enviada **uma única vez por tentativa do curso**, mesmo que o aluno tenha sido aprovado. Depois de responder, aparece **Ver resultado**.
8. Se reprovar em qualquer avaliação, novas avaliações do curso ficam bloqueadas. O aluno usa **Reiniciar curso**, confirma que o progresso de **todos os módulos** será zerado e começa de novo. As notas anteriores ficam no histórico.
9. Um módulo sem avaliação não libera a conclusão nem o certificado.

Somente as notas da tentativa atual do curso contam para aprovação e certificado. O relatório do gestor mostra essa tentativa e mantém todas as notas anteriores no histórico.
Uma prova aberta antes do reinício não pode ser enviada depois dele; o aluno precisa abri-la novamente.
Quando as questões mudam, a conclusão anterior do módulo é retirada.
O certificado também confere se a tentativa corresponde às questões atuais.
Módulos e avaliações inativos não contam para a conclusão; o curso precisa ter ao menos um módulo ativo.

O botão **Excluir** está disponível para curso, módulo, material e avaliação.
A exclusão remove o conteúdo das telas e mantém o histórico. Excluir um curso também remove seus módulos e conteúdos das telas.
Excluir um módulo libera espaço para cadastrar outro, respeitando o limite de quatro.

## Certificados

1. O aluno entra em **Certificados**.
2. Depois de ser aprovado em todos os módulos do curso, usa **Solicitar certificado**.
3. O gestor recebe a solicitação em sua aba **Certificados**.
4. Ao aprovar, o sistema confere novamente a aprovação e registra aluno, curso, carga horária, gestor, data e código de emissão.
5. O aluno abre **Ver certificado / salvar PDF**.
6. Usa **Imprimir ou salvar PDF** e escolhe **Salvar como PDF** na janela do navegador.

Se o gestor recusar, informa o motivo. O aluno vê o motivo e pode solicitar novamente quando atender aos requisitos.
Certificados já emitidos mantêm o nome do aluno, curso e carga horária registrados na emissão, mesmo depois de uma edição ou exclusão do curso.

## Chamados e conversa de ajuda

- Dentro do curso, **Ajuda com mentor** envia a dúvida para a fila dos mentores.
- O mentor seleciona **Atender chamado**. Apenas um responsável pode assumir o mesmo chamado.
- Aluno e mentor trocam mensagens em **Abrir conversa**. As respostas e a fila se atualizam automaticamente a cada cinco segundos, sem apagar a mensagem em digitação.
- As respostas do mentor também ficam como orientações do aluno.
- Aluno e mentor podem abrir **Ouvidoria / ajuda com o gestor**.
- O gestor responde às ouvidorias e pode abrir um chamado para o administrador.
- Ao enviar, o solicitante vê **Acompanhar meu chamado**, o status e o aviso de espera. Ele não recebe os botões de atendimento.
- A troca de mensagens é liberada quando alguém assume o chamado. Somente o responsável pelo atendimento pode encerrar a conversa. O histórico permanece disponível.
- Se uma conta responsável for desativada, seus chamados em atendimento voltam para a fila.

O gestor consegue consultar qual mentor atendeu cada aluno e ler a conversa para moderação.
Em **Relatórios**, vê a quantidade de chamados assumidos e encerrados por mentor e por gestor.
Não existe mais cadastro manual de vínculo entre aluno e mentor: o atendimento começa pelo chamado.

## Contas e e-mails

**Usuários** mostra a lista de contas e a pesquisa. **Cadastrar usuário** é uma página separada: o administrador cria mentor ou gestor e o gestor cria somente mentor.
O cadastro pede nome completo, e-mail pessoal e senha inicial.

Exemplos:

- Ana Silva com ID 12: `ana.silva.12@mentor.techlearn.com`.
- João Santos com ID 13: `joao.santos.13@gestor.techlearn.com`.

O ID é gerado pelo SQL Server na ordem dos cadastros. Pode haver números pulados.
O e-mail institucional identifica a conta para o login. Gerar esse endereço não cria uma caixa postal.
O e-mail pessoal é usado para receber a recuperação de senha e **não pode ser repetido em outra conta**, inclusive desativada. A regra vale no cadastro, na edição pelo gestor/administrador e no perfil do próprio usuário. O banco também impede contatos repetidos.
Contas anteriores mantêm seu e-mail de acesso; o formato institucional é aplicado aos novos cadastros.

O gestor edita apenas dados de aluno e mentor. O administrador controla a ativação das contas.
O perfil de aluno não pode ser convertido em mentor, nem o de gestor em aluno.

## Senhas e recuperação

Novas senhas precisam de **no mínimo oito caracteres**, uma letra maiúscula, um número e um caractere especial.
**Espaços não são permitidos.**
As senhas existentes continuam válidas; não foram substituídas.

Aluno, mentor e gestor usam **Recuperar senha** na tela de login ou no perfil.
O administrador não tem troca nem recuperação de senha pelo site.

Vocês informaram que ainda não têm serviço de e-mail. Por isso, o envio de recuperação precisa ser configurado depois.
Os campos ficam na seção `Email` da configuração da API: servidor SMTP, porta, remetente, usuário, senha do serviço e endereço da página de redefinição.
Sem configuração, a página mostra uma mensagem e não anuncia um envio inexistente.
O código de recuperação dura 30 minutos e pode ser usado uma vez.

## Como explicar a programação na banca

O campo `TentativaCurso` começa em 1 na matrícula. Cada resultado guarda esse mesmo número. Ao reiniciar após reprovação, a classe `Matricula` aumenta o número e zera o progresso. Assim, as notas antigas permanecem disponíveis, mas não contam para a nova aprovação. A API bloqueia envios repetidos e confere o número da tentativa, inclusive quando há duas abas abertas.

Continuam existindo **nove classes principais** na pasta `Modelos`:

`Usuario`, `Curso`, `Modulo`, `Material`, `Matricula`, `Avaliacao`, `Questao`, `Resultado` e `OrientacaoMentor`.

Os quatro perfis são representados por `Usuario.Perfil`.
As classes validam seus dados. Por exemplo, `Questao` limita as alternativas e `Resultado` calcula a nota.
Os serviços existentes coordenam o cadastro, as avaliações e a gestão. Os controllers recebem os pedidos do site.
`ConexaoBanco` executa consultas SQL com parâmetros.

Chamados, mensagens e solicitações de certificado são guardados em tabelas de apoio e tratados pelo `GestaoServico`.
Isso mantém as nove classes de negócio pedidas para o diagrama.
A página do certificado usa HTML e a impressão do navegador, sem um gerador de PDF complicado no código.

Fala sugerida:
“O aluno pede ajuda por um chamado; um mentor assume e responde. Para receber o certificado, o aluno precisa passar em todas as avaliações. O gestor confere a solicitação e aprova a emissão.”

## Estado desta atualização

- A API e o site foram compilados; a sintaxe JavaScript foi conferida.
- A atualização SQL foi aplicada ao banco local com cópia de segurança.
- Foram aprovadas **225 verificações HTTP**, **82 verificações complementares** e **50 testes das regras** em um banco separado com dados fictícios.
- As telas dos quatro perfis foram conferidas no navegador: usuários, chat, relatórios detalhados, PDF, certificado, confirmação de exclusão e alternativas A–E.
- A atualização automática foi conferida com a página aberta, incluindo preservação de um formulário em digitação.
- A recuperação foi testada com SMTP fictício local, sem enviar e-mails externos.
- Confira a comparação completa dos pedidos em [revisao-exigencias.md](revisao-exigencias.md).
- O envio de e-mail depende da contratação ou configuração de um serviço SMTP.
