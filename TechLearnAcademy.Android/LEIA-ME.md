# TechLearn Academy — Android Java

Aplicativo Android com telas nativas em Java. Não utiliza WebView, JavaScript dentro do aplicativo, AndroidX ou biblioteca de interface. Os componentes são os padrões do Android: TextView, EditText, Button, Spinner, RadioGroup e ScrollView.

As cores azul, verde e o fundo claro seguem o site.

## Instalar

Abra entregas/Android/TechLearnAcademy-Android.apk em um celular com Android 8 ou mais recente e siga a confirmação de instalação do próprio Android. A internet e a API da Azure precisam estar disponíveis. Use a mesma conta do site.

Esta é uma versão assinada para distribuição acadêmica. Não é uma publicação na Play Store. Não existe conta ou senha fixa no aplicativo.

## Perfis e funções

O perfil é recebido da API depois do login. O servidor verifica a sessão e a permissão em cada solicitação.

| Perfil | Telas e funções |
| --- | --- |
| Aluno | Cadastro público, painel, catálogo, matrícula, meus cursos, módulos, materiais, PDFs, avaliações, resultados, reinício após reprovação, orientações, ajuda com Mentor, ouvidoria e certificados. |
| Mentor | Alunos acompanhados, resultados permitidos, orientações, atendimento dos chamados de ajuda e ouvidoria com Gestor. |
| Gestor | Cursos, módulos, materiais por link ou PDF, avaliações, questões de duas a cinco alternativas, cadastro de Mentor, edição de Aluno/Mentor, chamados, aprovação de certificados e relatórios. |
| Administrador | Resumo por perfil, usuários, cadastro de Mentor/Gestor, edição, ativação/desativação, atendimento dos Gestores e verificação do banco pela API. |
| Todos | Login, perfil, saída e mensagens dentro da tela. Aluno, Mentor e Gestor também têm troca e recuperação de senha, seguindo as regras do site. |

Não é possível escolher um perfil no cadastro público: a API cria Aluno. O perfil e os e-mails institucionais existentes permanecem fixos. A API protege o último Administrador ativo.

O leitor de PDF é nativo. Links externos de materiais abrem no aplicativo disponível no celular. O certificado em PDF usa somente os dados da emissão aprovada pelo servidor. A pessoa escolhe onde salvar o documento.

A recuperação envia a solicitação ao serviço real. Quando receber o e-mail, o usuário pode copiar o código depois de token= no link para a tela nativa. Sem serviço de e-mail disponível, aparece a mensagem de erro da API.

## Organização

Cada arquivo terminado em Activity corresponde a uma tela. As telas são separadas por assunto e usam condições simples para escolher a ação do botão.

| Arquivo | Função |
| --- | --- |
| LoginActivity, CadastroActivity e RecuperarActivity | Entrada, cadastro público e recuperação. |
| PainelActivity e PerfilActivity | Menu do perfil e dados da conta. |
| UsuariosActivity e UsuarioActivity | Lista e formulário administrativo de usuários. |
| CursosActivity, CursoActivity e CursoFormActivity | Cursos, matrícula e edição do curso. |
| ModuloActivity e ModuloFormActivity | Materiais, avaliações e edição do módulo. |
| MaterialActivity e PdfActivity | Cadastro de material, envio e leitura de PDF. |
| AvaliacaoActivity, AvaliacaoFormActivity e QuestaoActivity | Avaliação do Aluno e edição pelo Gestor. |
| ResultadosActivity | Notas e respostas corrigidas pelo servidor. |
| ChamadosActivity, ChamadoFormActivity e ConversaActivity | Fila, abertura e conversa do chamado. |
| MentoriaActivity | Acompanhamento e orientações. |
| RelatoriosActivity | Relatórios e desempenho por curso e Aluno. |
| CertificadosActivity | Solicitação, aprovação e PDF do certificado emitido. |
| TelaBase.java | Cria os controles comuns, apresenta mensagens e aguarda a rede. |
| Api.java | Envia e recebe JSON ou arquivos por HTTPS. |
| Sessao.java | Mantém o token verdadeiro e os dados da conta na memória. |
| Validacao.java | Confere os campos antes do envio. |
| AndroidManifest.xml | Declara as telas e a permissão de internet. |
| app/build.gradle | Declara as versões do Android e do Java para compilar. |
| compilar-apk.ps1 | Alternativa de compilação usando diretamente o SDK Android e o Java. |

## O que precisa de explicação na banca

- Activity: é uma tela do aplicativo.
- JSON: é o formato usado para enviar os campos para a API.
- Thread: executa a solicitação sem bloquear a tela. O Android não permite fazer a comunicação na tarefa principal da interface.
- runOnUiThread: devolve a resposta para a tarefa que atualiza os componentes visuais.
- Sessao.token: recebe um token real do servidor. Não cria autenticação falsa nem identifica o perfil pelo e-mail.
- Handler: agenda a atualização da conversa a cada cinco segundos, somente enquanto a tela está aberta.
- PdfRenderer: mostra uma página de PDF como imagem.
- PdfDocument: cria o PDF do certificado com os dados aprovados pela API.

Fala sugerida:

“O aplicativo foi feito em Java com os controles nativos do Android. Cada Activity representa uma tela. Os botões enviam os dados em JSON para a mesma API usada pelo site. A API em C# confere o usuário, as permissões e os dados antes de acessar o banco SQL Server. A rede roda em segundo plano para a interface não travar. A senha não é salva e a sessão fica apenas na memória.”

## Abrir e compilar os fontes

No Android Studio, abra esta pasta e permita a sincronização do Gradle. Use JDK 17 ou 21, SDK Android 36 e Build Tools 36.0.0. O projeto contém o Gradle Wrapper e não depende de bibliotecas de interface.

O Android Studio usa o arquivo local.properties do seu computador. Esse arquivo não acompanha o ZIP dos fontes.

Alternativa pelo PowerShell, depois de instalar Java e SDK Android:

    .\compilar-apk.ps1 -Sdk 'CAMINHO_DO_SDK' -Java 'CAMINHO_DO_JDK'

O desktop C# continua sendo aberto no Visual Studio. O APK Java não exige Visual Studio ou SQL Server no celular.

## Dados e conexão

Android Java → HTTPS/JSON → API C# na Azure → SQL Server/Azure SQL.

Api.java contém somente o endereço público da API. Não contém a senha do SQL Server. O aplicativo não usa SharedPreferences, banco local ou arquivo para guardar a senha ou o token. Os campos de senha não são salvos no estado da tela.

Os únicos arquivos criados pelo aplicativo são o PDF temporário usado pelo leitor e os documentos que o usuário escolher salvar. O PDF temporário é apagado ao fechar o leitor.

Se o processo do Android for encerrado, a sessão em memória desaparece e será necessário entrar novamente. Sair solicita a revogação do token na API e limpa a memória local.

## Testes e limite desta entrega

O APK foi compilado e sua assinatura foi verificada. O cliente Java foi exercitado com a API pública e com uma API real local ligada a um banco descartável, cobrindo os quatro perfis e os principais fluxos. O banco de teste foi removido.

Não havia celular conectado nem emulador Android disponível. A instalação e a interação visual no aparelho ainda precisam ser conferidas. Os testes do cliente HTTP não substituem esse teste físico.


Também passaram cinco testes dos controles nativos Android em ambiente JVM com Robolectric: validação do login, colagem e máscara de senha, confirmação do cadastro, adaptação de largura e menus dos quatro perfis. Essas bibliotecas e os dados de interface de teste não integram o APK. A análise oficial Android Lint terminou sem erros. O teste de instalação em aparelho físico continua pendente.
