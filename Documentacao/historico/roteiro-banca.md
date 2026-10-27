# Roteiro de banca — TechLearn Academy

Atualização de 06/10/2026: consulte [novas-funcoes.md](novas-funcoes.md) para as regras atuais de chamados, certificados, perfis fixos, senhas e materiais. As descrições e evidências abaixo registram a versão anterior.

Duração sugerida: 13 minutos e 20 segundos, incluindo as trocas de tela. Este arquivo é editável. As falas entre aspas podem ser lidas literalmente; as instruções de tela não fazem parte da fala. Os tempos são uma sugestão para ensaio.

As referências apontam para o código da nova solução independente. Nenhum arquivo do projeto anterior faz parte deste roteiro. As linhas foram conferidas nesta versão; se o código for alterado, procure também pelo nome da função.

## Preparação antes da apresentação

1. Abra a nova solução no Visual Studio.
2. Configure uma instância de SQL Server exclusiva para a demonstração. Não use o banco anterior.
3. Execute o script da nova solução somente nessa instância, seguindo os avisos de Banco/01-criar-banco.sql.
4. Configure a conexão e o serviço SMTP no backend. Não mostre senhas de configuração.
5. Crie o primeiro Administrador pelo comando --criar-admin, implementado em TechLearnAcademy.Api/Program.cs. As demais contas são cadastradas como Aluno e têm seus perfis alterados pelo Administrador.
6. Prepare contas e conteúdo de demonstração pelo sistema real, em ambiente separado. Esses registros não ficam fixos no JavaScript.
7. Execute a API, que também entrega o site. Use HTTPS e um certificado confiável para apresentar mobile e desktop.
8. Produza as capturas descritas abaixo. O roteiro não declara que essas imagens já existem.
9. Ensaie a navegação com teclado e o aumento do texto pelo navegador.
10. Se um componente não puder ser executado, apresente seu código e diga expressamente que o teste daquela parte está pendente.

## 1. Apresentação do problema — 00:00 a 00:50

**Mostrar:** a tela de login e a marca.

**Fala literal:**

“Boa apresentação. Eu sou Raphael e fiquei responsável pelo frontend da TechLearn Academy. Luis ficou responsável pelo backend em C# e pelo acesso ao SQL Server. Nossa proposta é organizar uma plataforma de cursos de programação.

O problema que estamos tratando é a separação de informações em planilhas, arquivos, links e mensagens. Isso dificulta encontrar materiais, acompanhar os alunos e corrigir avaliações. A plataforma reúne esses elementos em uma única jornada de aprendizagem.

O público inclui estudantes, iniciantes, autodidatas e pessoas em transição de carreira. O aluno aprende, o mentor acompanha, o gestor organiza o conteúdo e o administrador cuida dos usuários e dos acessos.”

**Captura a produzir:** 01-login.png, com os campos vazios. Não mostrar senha ou token.

**Código para apontar:** [index.html: apresentação e marca](../../TechLearnAcademy.Web/wwwroot/index.html).

## 2. Organização da solução — 00:50 a 01:40

**Mostrar:** a árvore de projetos no Visual Studio.

**Fala literal:**

“A solução possui Web, API, Mobile, Desktop, testes e scripts do banco. O site usa HTML, CSS e JavaScript puro. HTML organiza o conteúdo, CSS define a aparência e JavaScript executa as ações.

Todos os clientes conversam com a mesma API. Somente a API conhece a conexão com o SQL Server. O site, o celular e o desktop não fazem consultas SQL diretamente.

No backend, o Controller recebe a solicitação. O Serviço verifica as regras. A classe de Banco executa os comandos SQL com parâmetros. Essa separação ajuda a localizar cada responsabilidade.”

**Captura a produzir:** 02-solucao.png, mostrando os projetos e as pastas, sem configurações secretas.

**Código para apontar:** [UsuariosController: rota de login](../../TechLearnAcademy.Api/Controllers/UsuariosController.cs); [ConexaoBanco: comandos com parâmetros](../../TechLearnAcademy.Api/Banco/ConexaoBanco.cs); [Program.cs da API: entrega do site](../../TechLearnAcademy.Api/Program.cs).

## 3. HTML e acessibilidade — 01:40 a 02:30

**Mostrar:** o formulário de login e seu trecho HTML.

**Fala literal:**

“O formulário tem um identificador, form-login. Esse nome permite que o JavaScript encontre o formulário. Os campos também têm identificadores: email e senha.

Cada campo tem um label visível. O for do label aponta para o id do campo, associando o texto à entrada. O e-mail usa type email. A senha usa type password e autocomplete current-password.

A área de mensagens tem role status e aria-live polite. Assim, leitores de tela podem anunciar o resultado de uma ação. Também existe um link para ir diretamente ao conteúdo, facilitando a navegação por teclado.

As outras páginas mantêm essa organização: título, campos identificados, ação clara e mensagem na própria página.”

**Captura a produzir:** 03-html-formulario.png, mostrando o formulário e a área de mensagem no editor.

**Código para apontar:** [index.html: formulário](../../TechLearnAcademy.Web/wwwroot/index.html); [index.html: mensagem acessível](../../TechLearnAcademy.Web/wwwroot/index.html).

## 4. CSS e adaptação às telas — 02:30 a 03:20

**Mostrar:** o login na largura de um computador e em uma tela estreita.

**Fala literal:**

“O CSS reúne a identidade visual de todas as páginas. Reutilizamos as cores, os botões, os cartões, as mensagens e os espaços. Isso mantém uma aparência semelhante para todos os perfis.

A classe publico organiza a tela inicial em duas colunas. A regra media muda essa organização quando a tela fica menor. Os cartões também se adaptam à largura disponível.

O focus-visible destaca o elemento que está recebendo o foco do teclado. O texto usa unidades relativas, permitindo aumentá-lo pelo navegador. Os botões têm nomes escritos e os erros aparecem com mensagens claras, além da diferença de cor.”

**Capturas a produzir:** 04-login-computador.png e 05-login-celular.png, com o mesmo conteúdo.

**Código para apontar:** [estilo.css: foco do teclado](../../TechLearnAcademy.Web/wwwroot/css/estilo.css); [estilo.css: colunas](../../TechLearnAcademy.Web/wwwroot/css/estilo.css); [estilo.css: tela menor](../../TechLearnAcademy.Web/wwwroot/css/estilo.css).

## 5. JavaScript e comunicação — 03:20 a 04:30

**Mostrar:** as funções requisicao e enviarFormulario.

**Fala literal:**

“O JavaScript foi dividido em arquivos por responsabilidade. O arquivo api.js contém as funções comuns. Os arquivos de aluno, mentor, gestor e administrador contêm as ações de cada área.

Quando o usuário envia um formulário, preventDefault impede o recarregamento da página. As validações conferem os dados antes da solicitação. O botão fica desabilitado enquanto a operação está acontecendo.

A função fetch envia uma solicitação HTTP para a API. JSON.stringify transforma o objeto JavaScript em texto JSON. O cabeçalho Content-Type informa que o conteúdo enviado é JSON.

Async indica que a função pode esperar uma operação. Await espera a resposta sem bloquear toda a interface. Try executa a operação, catch mostra a falha e finally devolve o botão ao estado normal.

A resposta é lida como JSON. Se houver erro HTTP, a mensagem da API aparece na tela. Se a conexão falhar, aparece uma mensagem de comunicação. O site não inventa resultados.”

**Captura a produzir:** 06-js-comunicacao.png, mostrando somente o código, sem valores reais de credenciais.

**Código para apontar:** [api.js: fetch e JSON](../../TechLearnAcademy.Web/wwwroot/js/api.js); [api.js: envio do formulário](../../TechLearnAcademy.Web/wwwroot/js/api.js); [config.js: mesma origem](../../TechLearnAcademy.Web/wwwroot/js/config.js).

## 6. Login, perfil e sessão real — 04:30 a 05:40

**Mostrar:** o código do login, da criação do cookie e da validação de sessão.

**Fala literal:**

“O site envia e-mail e senha para a API. Quem decide se o login está correto é o backend. O perfil recebido determina qual painel será aberto. Não identificamos perfil pelo e-mail e não existe uma lista de usuários no JavaScript.

Após conferir a senha, o servidor gera um token aleatório. Esse token representa uma sessão temporária. O banco guarda o hash do token, sua validade e a indicação de revogação.

O servidor também envia um cookie HttpOnly. Isso significa que o JavaScript não lê o cookie. O navegador o acompanha nas solicitações porque usamos credentials include. Nosso código não salva senha nem token em localStorage ou sessionStorage.

A API consulta a sessão real e o perfil atual em cada acesso. Authorize bloqueia funções que o perfil não pode executar. Esconder um link no menu serve para organizar a navegação; a permissão é conferida no servidor.

Sair revoga a sessão na API. Não é apenas voltar para a tela inicial.”

**Captura a produzir:** 07-sessao-codigo.png, mostrando o código que cria o cookie, sem capturar o valor de uma sessão.

**Código para apontar:** [publico.js: login e direcionamento](../../TechLearnAcademy.Web/wwwroot/js/publico.js); [api.js: quatro destinos](../../TechLearnAcademy.Web/wwwroot/js/api.js); [UsuariosServico: cookie HttpOnly](../../TechLearnAcademy.Api/Servicos/UsuariosServico.cs); [SessaoHandler: sessão real](../../TechLearnAcademy.Api/Servicos/SessaoHandler.cs).

## 7. Jornada do aluno — 05:40 a 06:50

**Mostrar:** painel do aluno, catálogo, curso e módulo.

**Fala literal:**

“O cadastro público sempre cria um Aluno. No painel, ele consulta seus cursos matriculados, seu progresso, suas avaliações e as orientações recebidas do mentor.

O catálogo apresenta os cursos ativos recebidos da API. A pesquisa e a paginação ajudam a localizar os cursos. Ver curso mostra a descrição, a carga horária e os módulos.

Matricular-se envia somente o identificador do curso. A API identifica o aluno pela sessão e realiza a matrícula. A regra impede duas matrículas ativas iguais.

O aluno matriculado acessa PDFs, links de videoaulas e avaliações. Marcar como concluído registra o módulo na API. O progresso usa a quantidade de módulos ativos concluídos em relação aos módulos ativos do curso.

Se ainda não houver registros, a página informa que a lista está vazia. Não usamos cursos fictícios para preencher o painel.”

**Capturas a produzir:** 08-aluno.png e 09-modulo-materiais.png, usando registros reais do ambiente de demonstração.

**Código para apontar:** [aluno.js: matrícula](../../TechLearnAcademy.Web/wwwroot/js/aluno.js); [aluno.js: conclusão](../../TechLearnAcademy.Web/wwwroot/js/aluno.js); [CursosServico: progresso](../../TechLearnAcademy.Api/Servicos/CursosServico.cs); [Cadastro público como Aluno](../../TechLearnAcademy.Api/Servicos/UsuariosServico.cs).

## 8. Avaliações e correção — 06:50 a 07:40

**Mostrar:** uma avaliação e o resultado de uma tentativa concluída.

**Fala literal:**

“A avaliação usa questões objetivas. Cada questão tem alternativas e somente uma é correta. Na página do aluno, as opções são radio buttons: escolher outra alternativa substitui a seleção da mesma questão.

O site informa quantas questões foram respondidas. No envio, manda os identificadores das questões e das alternativas selecionadas. O gabarito não é enviado ao aluno antes da correção.

A API confere se todas as questões foram respondidas e se cada alternativa pertence à questão certa. Depois calcula acertos, percentual, nota de zero a dez e aprovação a partir de sessenta por cento.

O resultado mostra acertos, erros e progresso atualizado. A gravação usa uma transação para guardar a tentativa, suas respostas e a conclusão do módulo como uma única operação.”

**Capturas a produzir:** 10-avaliacao.png e 11-resultado.png, após uma tentativa real no ambiente separado.

**Código para apontar:** [aluno.js: alternativas e progresso da prova](../../TechLearnAcademy.Web/wwwroot/js/aluno.js); [aluno.js: respostas](../../TechLearnAcademy.Web/wwwroot/js/aluno.js); [AvaliacoesServico: correção](../../TechLearnAcademy.Api/Servicos/AvaliacoesServico.cs); [Cálculo da nota e aprovação](../../TechLearnAcademy.Api/Servicos/AvaliacoesServico.cs).

## 9. Acompanhamento do mentor — 07:40 a 08:20

**Mostrar:** alunos vinculados, acompanhamento e uma orientação registrada.

**Fala literal:**

“O Mentor consulta apenas os alunos que o Administrador vinculou a ele. A API verifica o vínculo antes de mostrar o acompanhamento ou registrar uma orientação.

O acompanhamento reúne cursos, matrículas, progresso, notas e histórico. A orientação é um texto registrado no sistema; não existe chat em tempo real.

O aluno consulta as orientações no próprio painel. Os dados de outro aluno não ficam disponíveis apenas porque alguém mudou um identificador no endereço. O servidor confere a permissão.”

**Captura a produzir:** 12-mentor.png, evitando expor dados pessoais de outras pessoas.

**Código para apontar:** [mentor.js: acompanhamento](../../TechLearnAcademy.Web/wwwroot/js/mentor.js); [mentor.js: orientação](../../TechLearnAcademy.Web/wwwroot/js/mentor.js); [Regras: vínculo obrigatório](../../TechLearnAcademy.Api/Servicos/Regras.cs).

## 10. Gestão acadêmica — 08:20 a 09:20

**Mostrar:** cursos, conteúdo, questões e relatório.

**Fala literal:**

“O Gestor cuida da parte acadêmica. Ele cadastra e edita cursos, organiza os módulos, adiciona PDFs e links e prepara as avaliações.

Na questão, o Gestor escreve o enunciado, informa pelo menos duas alternativas e marca uma correta. As mesmas regras são conferidas pela API. Quando já existe uma tentativa, o histórico impede mudar o gabarito antigo. A questão pode ser desativada.

Os registros acadêmicos são ativados ou desativados para preservar relacionamentos. Os vídeos são links externos; não existe uma plataforma própria de streaming.

Os relatórios mostram matrículas, média, aprovação e progresso. Cada indicador vem dos registros reais. No relatório do Gestor, baixo progresso significa abaixo de trinta por cento. No resumo do Mentor, o alerta visual usa cinquenta por cento, identificado na tela.”

**Captura a produzir:** 13-gestor-conteudo.png e, se houver espaço, 14-relatorio.png.

**Código para apontar:** [gestor.js: cursos](../../TechLearnAcademy.Web/wwwroot/js/gestor.js); [gestor.js: módulos e materiais](../../TechLearnAcademy.Web/wwwroot/js/gestor.js); [gestor.js: avaliações](../../TechLearnAcademy.Web/wwwroot/js/gestor.js); [Preservação do histórico](../../TechLearnAcademy.Api/Servicos/AvaliacoesServico.cs).

## 11. Usuários e Administração — 09:20 a 10:10

**Mostrar:** painel administrativo, pesquisa e edição de usuário.

**Fala literal:**

“O Administrador cuida das contas e dos acessos. Essa função é diferente da gestão de cursos.

Ele consulta os usuários, pesquisa, ativa ou desativa contas e altera o perfil entre Aluno, Mentor, Gestor e Administrador. Também cria os vínculos entre mentores e alunos.

O sistema preserva pelo menos um Administrador ativo. As alterações de perfil e situação são registradas pela auditoria do banco. Uma conta desativada perde o acesso, mesmo que tenha entrado antes.

A lista tem paginação. Mostramos somente os dados necessários para administrar a conta, sem apresentar a senha ou seu hash.”

**Captura a produzir:** 15-administrador.png, sem dados pessoais de pessoas externas ao teste.

**Código para apontar:** [administrador.js: usuários e paginação](../../TechLearnAcademy.Web/wwwroot/js/administrador.js); [administrador.js: vínculos](../../TechLearnAcademy.Web/wwwroot/js/administrador.js); [Trigger de auditoria](../../Banco/01-criar-banco.sql).

## 12. Banco, hash e integridade — 10:10 a 11:10

**Mostrar:** as tabelas, a procedure de matrícula e a função de hash.

**Fala literal:**

“O SQL Server guarda usuários, cursos, módulos, materiais, matrículas, avaliações, questões, alternativas, tentativas, respostas e progresso. Também guarda os vínculos, as orientações, a auditoria e as sessões.

A chave primária identifica um registro. A chave estrangeira garante que um relacionamento aponte para um registro existente. Unique impede duplicações e Check limita valores inválidos. Os índices ajudam nas pesquisas.

A procedure de matrícula verifica os dados antes de gravar. A transação funciona como um conjunto: ou as alterações necessárias são confirmadas, ou a operação é desfeita. A outra procedure reúne os indicadores dos cursos.

A senha não é guardada em texto puro. PBKDF2 usa repetições e um valor aleatório chamado sal para produzir o hash. No login, a API calcula novamente para conferir a senha. Não existe um comando para recuperar a senha original do hash.”

**Captura a produzir:** 16-banco-codigo.png, mostrando o script e as restrições, sem exibir valores sensíveis.

**Código para apontar:** [Tabela Usuarios](../../Banco/01-criar-banco.sql); [Procedure de matrícula](../../Banco/01-criar-banco.sql); [Procedure de relatório](../../Banco/01-criar-banco.sql); [Hash PBKDF2](../../TechLearnAcademy.Api/Servicos/Regras.cs).

## 13. Recuperação, mobile e desktop — 11:10 a 11:55

**Mostrar:** recuperar senha, projeto MAUI e projeto desktop.

**Fala literal:**

“A recuperação envia um link por e-mail. O servidor guarda o hash do código, que vence em trinta minutos e funciona uma única vez. A resposta não revela se o endereço pertence a uma conta. Se o serviço SMTP não estiver configurado ou falhar, não mostramos um envio fictício.

O aplicativo mobile foi organizado com .NET MAUI. O desktop usa Windows Forms e WebView2. Ambos exibem a interface responsiva hospedada, usam a mesma API e compartilham as regras.

O endereço do servidor pode ser informado. Em um celular, localhost aponta para o próprio aparelho. Por isso, o servidor precisa ser acessível na rede ou publicado, com HTTPS válido.”

**Capturas a produzir:** 17-mobile.png e 18-desktop.png somente depois de abrir os aplicativos reais. Uma tela do navegador redimensionada não comprova a execução de um aplicativo nativo.

**Código para apontar:** [Recuperação por SMTP](../../TechLearnAcademy.Api/Servicos/UsuariosServico.cs); [Código de uso único](../../TechLearnAcademy.Api/Servicos/UsuariosServico.cs); [WebView MAUI](../../TechLearnAcademy.Mobile/MainPage.xaml); [Endereço HTTPS](../../TechLearnAcademy.Mobile/MainPage.xaml.cs); [WebView2 desktop](../../TechLearnAcademy.Desktop/TelaPrincipal.cs).

## 14. Inclusão, privacidade e planejamento — 11:55 a 12:40

**Mostrar:** requisitos e documentação acadêmica.

**Fala literal:**

“A responsabilidade social aparece em ações concretas: linguagem respeitosa, acesso sem distinção por origem, rótulos, contraste, teclado, texto ampliável e organização dos materiais.

A privacidade também faz parte do projeto. O aluno consulta seus resultados. O Mentor consulta os alunos vinculados. As senhas e os tokens não aparecem nos relatórios. As configurações sensíveis ficam no servidor.

O planejamento inclui backlog, sprints, implantação, versionamento e backup. A publicação em nuvem depende de configurações e validações próprias; um plano escrito não comprova uma implantação realizada.

O modelo de negócio pode usar assinatura, mas os pagamentos não são processados no sistema. As estimativas econômicas precisam ser identificadas como estimativas, não como receitas já obtidas.”

**Captura a produzir:** 19-documentacao.png, mostrando as seções de requisitos e planejamento.

**Referências:** Documentacao/requisitos.md, Documentacao/relatorio-academico.md e Documentacao/banco.md.

## 15. Testes e encerramento — 12:40 a 13:20

**Mostrar:** a lista de testes e suas evidências realmente produzidas.

**Fala literal:**

“Os testes separam uma verificação de código de uma verificação completa de funcionamento. Compilar mostra que o projeto pode ser construído. Abrir uma página mostra que o arquivo foi servido. Para comprovar o cadastro, a matrícula ou a correção, precisamos também da API e do banco funcionando.

O teste de integração usa uma instância LocalDB exclusiva, criada para essa finalidade. Ele gera dados temporários nesse ambiente, nunca no banco do projeto anterior. O SMTP local de teste permite conferir a entrega sem mandar e-mails a pessoas.

As evidências registram os testes efetivamente executados. O que ainda não foi executado deve ser apresentado como pendente. Nosso objetivo é mostrar uma solução organizada, compreender suas regras e explicar claramente seus limites. Obrigado.”

**Captura a produzir:** 20-testes.png, somente com resultados reais. Não criar uma imagem de testes aprovados sem executá-los.

**Código para apontar:** [Instância isolada de testes](../../Testes/testar-integracao.py); [Cliente HTTP com cookie](../../Testes/testar-integracao.py); [Cenários de integração](../../Testes/testar-integracao.py); [Execução explícita dos testes](../../Testes/testar-integracao.py).

## Respostas literais para perguntas frequentes

**“Por que usar uma API?”**

“A API concentra as regras e o acesso ao banco. Assim, Web, Mobile e Desktop usam os mesmos dados e não conhecem a conexão SQL.”

**“O JavaScript consegue criar uma conta sozinho?”**

“Não. Ele envia os campos para a API. A mensagem de sucesso aparece somente depois de uma resposta de sucesso do servidor.”

**“O perfil é definido pelo e-mail?”**

“Não. O perfil vem do servidor. O cadastro público cria Aluno e somente o Administrador autorizado altera esse perfil.”

**“O painel está protegido só porque o link foi escondido?”**

“Não. O HTML organiza a interface. A API verifica a sessão, o perfil e a propriedade ou o vínculo dos registros em cada acesso.”

**“O que é await?”**

“É a espera por uma operação que leva tempo, como a resposta da API. Ele nos permite ler a resposta antes de continuar aquela função.”

**“O que é JSON?”**

“É um formato de texto com nomes e valores. O site envia os campos nesse formato e a API transforma esse conteúdo nos dados que precisa.”

**“Qual é a diferença entre hash e token?”**

“O hash da senha permite conferir uma senha sem guardá-la em texto puro. O token é uma chave aleatória e temporária que representa a sessão. São funções diferentes.”

**“Por que a senha usa uma verificação Unicode?”**

“Para reconhecer letras e números de outros idiomas e preservar os caracteres digitados. A mesma regra é usada no site e na API.”

**“O que foi testado no celular e no desktop?”**

“Eu mostro somente o que estiver registrado nas evidências. Compilar o projeto não comprova todos os fluxos num aparelho. Um componente sem teste real permanece identificado como pendente.”

**“Uma captura vale como teste?”**

“A captura ajuda a documentar um teste que foi realizado. Ela não substitui executar o fluxo, conferir a resposta da API e verificar o resultado esperado.”

## Como preparar as capturas

Use Windows + Shift + S para selecionar a região. Salve os arquivos em Documentacao/capturas e mantenha os nomes sugeridos. Essa pasta pode ser criada manualmente na preparação da banca.

Capturas de código devem mostrar o nome do arquivo e os números das linhas. Capturas da interface devem mostrar título, ação e mensagem relevante. Não incluam senha, código de recuperação, token de sessão, conexão com senha ou dados pessoais de pessoas externas ao teste.

As 20 capturas são um plano de apresentação. Elas não estão embutidas neste arquivo e não foram declaradas como evidências já produzidas. Se houver menos tempo, use 01, 03, 06, 08, 11, 12, 13, 15, 16 e 20; as demais podem ficar como material de apoio.
