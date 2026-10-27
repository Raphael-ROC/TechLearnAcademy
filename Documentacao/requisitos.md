# Requisitos e rastreabilidade — TechLearn Academy PIM IV

Atualização de 06/10/2026: consulte [novas-funcoes.md](historico/novas-funcoes.md) para as regras atuais de chamados, certificados, perfis fixos, senhas e materiais. As descrições e evidências abaixo registram a versão anterior.

Fonte integral: TXT fornecido pelo grupo, “Resumo definitivo do PIM IV — TechLearn Academy”. A interpretação operacional está em `CONTRATO.md`. Esta matriz refere-se **somente à solução nova**, não modifica nem certifica o projeto antigo.

## Como ler a matriz

- **Artefato:** arquivo, tela, rota ou documentação responsável pelo requisito.
- **Aceite:** resultado que deve ser verificado; não significa teste já aprovado.
- **Pendência externa:** cenários não executados ou que dependem de SMTP externo, publicação ou dispositivo; SQL isolado, SMTP local, restauração em banco novo e alguns percursos no navegador já foram exercitados.
- Código ou documento existente não é evidência automática de funcionamento integrado.
- Resultados reais: 30 eventos em `Testes/evidencias/integracao.json` e 17 em `Testes/evidencias/adicionais.json`, totalizando 47 eventos de integração; mais 22 verificações isoladas em C# no projeto `TechLearnAcademy.Tests`. São 69 verificações registradas, não 69 requisitos integralmente validados. Consulte `Documentacao/testes-executados.md` e a cobertura da seção 12.

Prefixos: `Web/` significa `TechLearnAcademy.Web/wwwroot/`; `API/` significa `TechLearnAcademy.Api/`. JSON usa camelCase; as colunas SQL usam PascalCase. Os serviços finais são UsuariosServico, CursosServico, AvaliacoesServico e GestaoServico; Regras e SessaoHandler concentram validação e autenticação.

## 1. Organização, problema, objetivos e escopo do TXT

| ID | Exigência do TXT | Artefato | Aceite / confirmação necessária |
|---|---|---|---|
| DOC01 | Caracterizar organização, problema e público | relatorio-academico.md, seções 1–2 | Organização identificada como hipotética; sem CNPJ/pesquisa inventados. |
| DOC02 | Proposta de valor e solução integrada | Relatório, seções 2–3 e 5 | Mostrar jornada organizada e única API/banco. |
| DOC03 | Modelo de negócio por assinatura | Relatório, seção 3 | Valores são hipóteses; cobrança não implementada. |
| DOC04 | Viabilidade técnica/operacional/econômica | Relatório, seção 3 | Fórmulas, custos, cenários e limitações transparentes. |
| DOC05 | Responsabilidade social e diversidade | Relatório, seção 4 | Ações concretas e avaliação proposta; sem afirmar piloto realizado. |
| DOC06 | Arquitetura de software | CONTRATO.md; Relatório, seção 5 | Clientes → API → SQL/SMTP; responsabilidades das pastas. |
| DOC07 | Banco completo e documentação | Banco/01-criar-banco.sql; banco.md | 16 tabelas, MER, modelos, dicionário, chaves, restrições, índices, SP e trigger. |
| DOC08 | Nuvem/DevOps/backup | Relatório, seções 9–10; scripts 02 e 03 | Publicação proposta, segredos, CI e restauração isolada; operação externa pendente. |
| DOC09 | Gerenciamento ágil | Relatório, seção 8 | Backlog, prioridades, sprints, aceite e definição de pronto; não inventar reuniões. |
| DOC10 | Testes e evidências | Esta matriz; Documentacao/testes-executados.md; TechLearnAcademy.Tests; Testes/evidencias | Distinguir testes isolados de integração; anexar evidência somente quando existir. |
| DOC11 | Documento acadêmico/ABNT | relatorio-academico.md | Instituição/curso/orientador a preencher; formatação final segundo manual institucional. |
| DOC12 | Ferramentas Visual Studio, SQL Server, SSMS e GitHub | README da solução; relatório e banco.md | Instruções reais e GitHub como repositório de scripts, nunca “o banco funcionando”. |
| DOC13 | Solução organizada com seis áreas | Api, Web, Mobile, Desktop, Tests e Banco | Projetos/pastas independentes e originais preservados. |

## 2. Funcionalidades e módulos da API

| ID | Requisito | Tela/arquivo e API | Critério de aceite / teste externo |
|---|---|---|---|
| RF01 | Cadastro público de aluno | Web/cadastro.html; UsuariosServico; POST /api/usuarios/cadastro | Comprovado: cadastro real sempre Aluno e duplicação 409. Hash consultado no SQL; cadastro inicial também executado em navegador. Cobertura na seção 12. |
| RF02 | Login real | Web/index.html; UsuariosServico; POST /api/usuarios/login | Comprovados credenciais corretas, senha incorreta e conta inativa na API. Login incorreto/correto e destinos dos quatro perfis também executados em navegador. |
| RF03 | Controle por quatro perfis | Web/js/publico.js; SessaoHandler; atributos de autorização | Papéis exercitados na API; direcionamento e abertura dos quatro painéis com dados da API comprovados em navegador. Matriz integral de permissões e dispositivos ainda precisam de execução. |
| RF04 | Sessão temporária/logout | UsuariosServico/SessaoHandler; POST /api/usuarios/logout; Sessoes | Comprovado: logout, revogação de sessão após inativação/redefinição e acesso sem sessão. Expiração de sessão ainda não exercitada. |
| RF05 | Recuperação por e-mail | Web/recuperar-senha.html; POST /api/usuarios/recuperar-senha | Comprovado envio de mensagem SMTP a receptor local. Provedor externo, TLS/autenticação e caixa postal reais ainda pendentes. |
| RF06 | Redefinição de senha | Web/redefinir-senha.html; POST /api/usuarios/redefinir-senha | Comprovado: redefinição pelo link recebido, token reutilizado/vencido recusado e sessões revogadas. Concorrência ainda não testada. |
| RF07 | Perfil e alteração de dados | Web/perfil.html; GET/PUT /api/usuarios/perfil | GET de perfil protegido exercitado; edição de Nome/Email e conflito na edição ainda não executados. |
| RF08 | Troca de senha autenticada | Web/perfil.html; POST /api/usuarios/trocar-senha | Troca autenticada pela senha atual ainda não executada; redefinição via token foi comprovada separadamente. |
| RF09 | Status da API e do banco | StatusController; GET /api/status; GET /api/status/banco | GET /api/status comprovado; status/banco sem sessão recusado com 401 no pacote Release local. Schema/consultas SQL exercitados; resposta administrativa autenticada ainda não conferida. |
| RF10 | Catálogo de cursos ativos e paginação | Web/cursos.html; CursosServico; GET /api/cursos | Consulta/detalhe de curso real comprovados. Busca/paginação e catálogo vazio ainda precisam de cenários específicos. |
| RF11 | Descrição, módulos e matrícula do curso | Web/curso.html; GET /api/cursos/{id} | Comprovados curso real, módulos, estado não matriculado, matrícula e listagem de material no navegador. Demais estados ainda não executados. |
| RF12 | Gestão de cursos | Web/gerenciar-cursos.html; POST/PUT /api/cursos | Criação de curso por Gestor e recusa para Administrador comprovadas. Edição/ativação/desativação ainda não exercitadas. |
| RF13 | Gestão de módulos | Web/gerenciar-conteudo.html; POST/PUT /api/modulos | Criação de módulo real comprovada. Alteração, ordem duplicada e curso inválido ainda não exercitados. |
| RF14 | Materiais PDF e links/videoaula | Web/estudar.html e gerenciar-conteudo.html; GET /api/modulos/{id}/materiais; POST/PUT /api/materiais | Criação, consulta protegida e listagem visual de material PDF comprovadas. URL javascript recusada em teste isolado C#. Videoaula e acesso externo ao arquivo ainda não exercitados. |
| RF15 | Matrícula sem duplicação | Web/curso.html; POST /api/matriculas; sp_RealizarMatricula | Matrícula real no navegador, duplicação sequencial e duas chamadas simultâneas comprovadas. Na disputa houve uma resposta 201 e outra 409; o SQL confirmou uma matrícula ativa. |
| RF16 | Meus cursos e progresso | Web/meus-cursos.html; GET /api/matriculas/minhas | Consulta das matrículas próprias e progresso de 100% após aprovação comprovadas. Outros percentuais ainda não exercitados. |
| RF17 | Marcar módulo concluído | Web/estudar.html; POST /api/progresso | Conclusão por aprovação da prova comprovada. POST manual/repetição de conclusão ainda não exercitados. |
| RF18 | Painel do aluno | Web/painel-aluno.html; GET /api/aluno/painel | Painel aberto após login real em navegador. Estados com vários cursos/avaliações e todos os indicadores ainda precisam de cenários próprios. |
| RF19 | Gestão de avaliações | Web/gerenciar-avaliacoes.html; GET /api/modulos/{id}/avaliacoes; POST/PUT /api/avaliacoes | Criação e consulta de avaliação real comprovadas. Edição/ativação/desativação ainda não executadas. |
| RF20 | Questões, alternativas e gabarito | AvaliacoesServico; POST/PUT /api/questoes | Uma alternativa ou duas corretas recusadas; questão válida gravada. Edição após tentativas recusada com 409; desativação preservou histórico. Demais cenários de edição ainda não exercitados. |
| RF21 | Apresentar prova objetiva | Web/avaliacao.html; GET /api/avaliacoes/{id} | Aluno recebeu questões sem gabarito; apresentação e resposta de uma avaliação foram executadas no navegador. Demais tamanhos e estados ainda precisam de teste. |
| RF22 | Correção automática | Web/avaliacao.html; POST /api/avaliacoes/{id}/respostas | Envios vazios, questão repetida e alternativas inexistentes/de outra questão recusados, sem tentativa parcial. Correção real de provas com uma, cinco e 32 questões comprovada; uma prova também enviada pelo navegador. |
| RF23 | Resultado, nota, percentual e aprovação | Web/resultado.html; GET /api/resultados/{id} | Comprovados 100%/nota 10, 2/5 reprovado com 40%/nota 4, 3/5 aprovado com 60%/nota 6 e 1/32 arredondado para 3,13%/nota 0,31. Resultado de 100% conferido no navegador. Demais limites ainda precisam de cenários próprios. |
| RF24 | Histórico de resultados próprios | GET /api/resultados; TentativasAvaliacao/RespostasAluno | Resultado próprio e recusa de resultado alheio comprovados. Histórico completo e consulta de resultado pelo Mentor ainda não executados. |
| RF25 | Lista/painel do mentor | Web/painel-mentor.html e alunos-mentor.html; GET /api/mentor/alunos | Consulta de aluno vinculado/recusa sem vínculo exercitadas. Painel aberto em navegador com dados da API; todos os indicadores e controles restantes ainda precisam de cenários próprios. |
| RF26 | Acompanhamento de aluno | Web/acompanhamento-aluno.html; GET /api/mentor/alunos/{id} | Consulta real de acompanhamento após vínculo comprovada. Outros históricos/estados e apresentação visual não foram integralmente conferidos. |
| RF27 | Orientação/feedback do mentor | Web/orientacao.html; POST /api/mentor/orientacoes | Comprovado: orientação de Mentor vinculado lida pelo aluno; sem vínculo foi recusada. Interface e consulta visual do histórico ainda não conferidas. |
| RF28 | Painel/relatórios do gestor | Web/painel-gestor.html e relatorios.html; GET /api/relatorios; sp_RelatorioCurso | Relatório consultou dados reais por procedure; painel aberto no navegador com dados da API. Fórmulas em conjuntos maiores/vazios e todos os indicadores ainda não foram individualmente conferidos. |
| RF29 | Resumo administrativo | Web/painel-administrador.html; GET /api/administracao/painel | Recusa de acesso de Aluno comprovada; painel Administrador aberto no navegador com dados da API. Todos os filtros, quantidades e estados ainda não foram individualmente conferidos. |
| RF30 | Pesquisar e consultar usuários | Web/usuarios.html; GET /api/administracao/usuarios | Busca/paginação administrativa e exibição visual ainda não exercitadas. |
| RF31 | Alterar perfil/situação | Web/editar-usuario.html; PUT /api/administracao/usuarios/{id} | Promoções, inativação, perda de acesso, preservação do último Admin e auditoria comprovadas. Cenários restantes na seção 12. |
| RF32 | Vínculo mentor/aluno | Web/usuarios.html; GET/POST/PUT /api/administracao/vinculos | Criação de vínculo real entre Mentor/Aluno comprovada. Alteração/ativação/desativação/lista e perfis inválidos ainda não exercitados. |
| RF33 | Site responsivo | Web/css/estilo.css; todas as telas | 24 HTMLs, oito JS, 157 links e 42 labels conferidos; largura 390 px sem overflow e foco inicial por Tab comprovados em navegador. Demais larguras, percurso completo por teclado, zoom e leitor ainda previstos. |
| RF34 | Aplicativo mobile C# | TechLearnAcademy.Mobile, MAUI WebView | MAUI Android e MAUI Windows compilados sem erros/avisos. Execução em emulador/aparelho e aceite institucional ainda pendentes. |
| RF35 | Programa desktop C# | TechLearnAcademy.Desktop, WinForms/WebView2 | Desktop Windows compilado sem erros/avisos. Execução visual com runtime WebView2 e dados reais ainda pendente. |
| RF36 | Dados/regras compartilhados | CONTRATO.md; interface hospedada; API única | A API/interface compartilham o banco isolado. Compatibilidade de uma alteração entre três clientes em execução ainda não testada. |

Para RF25, RF26 e RF28, os dados podem ser compostos de várias respostas reais disponíveis no contrato. Não deve ser acrescentado um conjunto fixo de alunos/indicadores para preencher o painel.

## 3. Todas as páginas solicitadas no TXT

| Página | Perfil / conteúdo obrigatório | API ou ação ligada |
|---|---|---|
| index.html | Pública: marca, e-mail, senha, entrar, cadastro, recuperação e mensagens | POST /api/usuarios/login. |
| cadastro.html | Pública: nome, e-mail, senha, confirmação, regra e mensagens | POST /api/usuarios/cadastro. |
| recuperar-senha.html | Pública: e-mail e solicitar recuperação | POST /api/usuarios/recuperar-senha. |
| redefinir-senha.html | Pública: token, nova senha, confirmação e salvar | POST /api/usuarios/redefinir-senha. |
| perfil.html | Todos: nome, e-mail, perfil, dados, senha e sair | Perfil GET/PUT, trocar-senha, logout. |
| painel-aluno.html | Aluno: saudação, cursos, progresso, avaliações e continuar | GET /api/aluno/painel e navegação para estudo. |
| cursos.html | Aluno: catálogo, nome, descrição, horas e ver curso | GET /api/cursos; navegação curso.html?id. |
| curso.html | Aluno: descrição, módulos, materiais, matrícula e matricular-se | Curso GET, materiais por módulo, matrícula POST. |
| meus-cursos.html | Aluno: cursos matriculados e progresso | GET /api/matriculas/minhas. |
| estudar.html | Aluno: módulos, materiais e marcar concluído | Curso/materiais/avaliações GET e progresso POST. |
| avaliacao.html | Aluno: questões, alternativas, progresso e enviar | Avaliação GET e respostas POST. |
| resultado.html | Aluno/Mentor autorizado: acertos, erros, nota, aprovação e progresso | GET /api/resultados/{id}. |
| painel-mentor.html | Mentor: quantidade, baixo progresso e orientações recentes | Alunos/consulta de acompanhamento. |
| alunos-mentor.html | Mentor: lista dos vinculados | GET /api/mentor/alunos. |
| acompanhamento-aluno.html | Mentor: cursos, notas, progresso e histórico | GET /api/mentor/alunos/{id}. |
| orientacao.html | Mentor: registrar e consultar feedbacks | Consulta de acompanhamento; orientações POST. |
| painel-gestor.html | Gestor: cursos, alunos matriculados, média, aprovação e progresso | GET /api/relatorios. |
| gerenciar-cursos.html | Gestor: cadastro/consulta/edição/ativação | Cursos GET/POST/PUT. |
| gerenciar-conteudo.html | Gestor: módulos, PDFs e links | Cursos GET; módulos/materiais POST/PUT. |
| gerenciar-avaliacoes.html | Gestor: avaliações, questões, alternativas e gabarito | Avaliações/questões GET/POST/PUT. |
| relatorios.html | Gestor: matrículas, média, aprovação e baixo progresso | GET /api/relatorios. |
| painel-administrador.html | Administrador: ativos, inativos e perfis | GET /api/administracao/painel. |
| usuarios.html | Administrador: busca/dados/situação e vínculos | Usuários GET; vínculos GET/POST/PUT; editar-usuario. |
| editar-usuario.html | Administrador: perfil e situação da conta | PUT /api/administracao/usuarios/{id}. |

Cada página deve abrir sem depender de dados fictícios. Toda lista vazia apresenta mensagem clara. Abrir um HTML diretamente não concede acesso à API: dados e ações requerem sessão/permissão.

## 4. As 22 regras de negócio do TXT

| Regra TXT | Implementação responsável | Aceite / cenário | Cobertura real nos 47 eventos de integração |
|---|---|---|---|
| RN01 Cada e-mail pertence a uma conta | Usuarios.Email UNIQUE; UsuariosServico | Segundo cadastro com e-mail equivalente falha 409. | Comprovado e-mail duplicado recusado. |
| RN02 Cadastro público sempre Aluno | UsuariosController/UsuariosServico | Enviar campo perfil adicional não cria Gestor/Administrador. | Comprovado em cinco cadastros, inclusive envio de perfil adicional. |
| RN03 Só Administrador muda perfis | GestaoController/GestaoServico | Aluno/Mentor/Gestor recebem 403 na rota administrativa. | Parcial: acesso administrativo de Aluno recusado; demais rotas ainda previstas. |
| RN04 Senha nunca em texto puro | Regras.HashSenha; Usuarios.SenhaHash | Conferir valor armazenado/hash sem divulgar senha nos logs. | Comprovado hash PBKDF2 com salt consultado no SQL, sem senha direta. |
| RN05 Recuperação válida/única | RecuperacoesSenha; UsuariosServico.Redefinir | Reutilização/expiração recusadas; consumo concorrente somente uma vez. | Comprovados vencimento/uso único sequencial; disputa simultânea ainda prevista. |
| RN06 Usuário inativo não entra | UsuariosServico.Login; SessaoHandler | Login e sessão anterior de conta inativa recusados. | Comprovados login recusado e sessão antiga sem acesso. |
| RN07 Só Gestor altera conteúdo | CursosController/AvaliacoesController | Chamada direta por outro perfil falha, mesmo alterando HTML. | Parcial: Administrador não cria curso; demais ações/perfis ainda previstos. |
| RN08 Só Administrador muda perfil/situação | GestaoController; trigger de auditoria | Bloqueio/ativação/perfil restritos e auditados. | Mudança/inativação, último Admin e trigger comprovados; matriz integral ainda prevista. |
| RN09 Mentor só vê vinculados | GestaoServico; AcompanhamentosMentor | Id de aluno não vinculado não devolve histórico. | Comprovados vínculo autorizado e acesso/orientação sem vínculo recusados. |
| RN10 Aluno só vê resultados próprios | AvaliacoesServico; TentativasAvaliacao.AlunoId | Trocar Id da tentativa no endereço não expõe resultado alheio. | Comprovados resultado próprio e resultado alheio recusado. |
| RN11 Módulo pertence a curso | FK Modulos.CursoId | Curso inexistente não pode receber módulo. | Criação vinculada comprovada; FK com curso inexistente ainda não testada. |
| RN12 Material pertence a módulo | FK Materiais.ModuloId | Módulo inexistente não pode receber material. | Criação vinculada comprovada; FK com módulo inexistente ainda não testada. |
| RN13 Sem duas matrículas ativas | sp_RealizarMatricula e UQ_Matriculas_Ativas | Envio concorrente não duplica. | Duplicação sequencial e concorrente recusadas; disputa retornou 201/409 e SQL manteve uma matrícula ativa. |
| RN14 Conteúdo/prova exige matrícula | CursosServico/AvaliacoesServico | Aluno não matriculado recebe recusa. Gestor acessa gestão acadêmica. | Comprovadas recusas sem matrícula e material autorizado após matrícula. |
| RN15 Avaliação objetiva | AvaliacoesServico; Questoes/Alternativas | Questões com alternativas; sem correção manual de texto livre. | Prova objetiva real de uma questão foi respondida/corrigida. |
| RN16 ≥2 alternativas e só 1 correta | Validação de transação da API; índice correto filtrado | 0/1 alternativas ou 0/2 corretas recusadas. | Uma alternativa e duas corretas recusadas; questão válida gravada. |
| RN17 Nota 0–10 | API de correção e CK_Tentativas_Nota | Fórmula e valores limites conferidos. | Comprovadas notas 10, 6, 4 e 0,31; outros valores limites ainda previstos. |
| RN18 Percentual acertos/total | API de correção e CK_Tentativas_Percentual | 3/5=60%; valores não enviados como “nota do cliente”. | Comprovados 100%, 3/5=60%, 2/5=40% e 1/32 arredondado para 3,13%. |
| RN19 Aprovação mínima 60% | API e CK_Tentativas_Aprovacao | 60% aprova; abaixo reprova. | Comprovados aprovação a 100% e 60%; reprovação a 40% e 3,13%. |
| RN20 Progresso por módulos concluídos | ProgressoModulos; consultas de CursosServico/relatório | 2/4 módulos ativos=50%; repetir conclusão não aumenta. | Aprovação a 60%/100% concluiu módulo; reprovação não concluiu. 2/4 e repetição manual ainda previstos. |
| RN21 Vídeos por links | Materiais Tipo=Link; URL http/https | Sem upload/transmissão de vídeo próprio. | Material PDF gravado; videoaula externa ainda não exercitada. |
| RN22 Relacionados desativados | Campos Ativo; rotas PUT; FKs sem cascata | Histórico e relacionamentos preservados, sem botão de exclusão destrutiva. | Conta inativada preservou relacionamento; desativar questão preservou enunciado, respostas, gabarito e nota. Demais tipos de conteúdo ainda previstos. |

As regras acima distinguem comportamento previsto e cobertura real parcial. Os 47 eventos exercitaram SQL e API reais na instância isolada, inclusive concorrência de matrícula. As 22 verificações C# são isoladas e não substituem integração. Casos ausentes, demais disputas e ambientes externos continuam previstos. Consulte Documentacao/testes-executados.md e a seção 12.

## 5. Requisitos não funcionais do TXT

| ID | Categoria / exigência | Artefato | Verificação necessária |
|---|---|---|---|
| RNF01 | Segurança: hash, HTTPS, token, perfil, parâmetros | Regras, SessaoHandler, Controllers, ConexaoBanco | Hash/sessão reais; 401/403; SQL parametrizado; HTTPS publicado confiável. |
| RNF02 | Acessibilidade: contraste/labels/teclado/mensagens | Web/css e HTML; relatório seção 4 | Teclado/leitor/zoom/contraste; sem declaração WCAG integral sem auditoria. |
| RNF03 | Usabilidade: telas/botões/navegação clara | Todas as páginas e WebViews | Percurso de cada perfil sem instrução técnica obrigatória ao usuário final. |
| RNF04 | Responsividade: computador/tablet/celular | CSS e viewport; Mobile | Conferir 320 px, tablet, desktop e zoom, sem perda de controles. |
| RNF05 | Integridade | Banco/01-criar-banco.sql e serviços | Duplicações/referências inválidas recusadas; SQL isolado e fluxos principais comprovados; validações restantes ainda previstas. |
| RNF06 | Desempenho: índices/paginação | Índices SQL; cursos/usuários paginados | Conferir paginação; medir tempo com base real antes de afirmar velocidade. |
| RNF07 | Privacidade | Autorizações/propriedade; relatório seção 6 | Dados mínimos; acesso restrito; retenção/canal LGPD planejados. |
| RNF08 | Manutenção: responsabilidades separadas | Controllers, Servicos, Banco, Modelos | Nome claro e fluxo explicável; sem lógica SQL nos clientes. |
| RNF09 | Compatibilidade: API única | Web, Mobile, Desktop; CONTRATO | Mesma alteração visível nos três clientes; integração pendente. |
| RNF10 | Confiabilidade: erros/logs/backup | Tratamento de erros; scripts de backup/restauração | Sem sucesso fictício; logs sem segredos; backup/restauração reais comprovados em banco novo; CHECKDB e percurso da aplicação restaurada pendentes. |
| RNF11 | Explicabilidade | Comentários curtos; relatório seção 14 | Integrantes explicam dados, conditions, fetch e responsabilidade de cada camada. |

## 6. Banco: tabelas e entregas obrigatórias

| Item | Arquivo/estrutura | Confirmação necessária |
|---|---|---|
| Usuarios | Banco/01-criar-banco.sql | Nome/e-mail/hash/quatro perfis/situação/data; e-mail único. |
| RecuperacoesSenha | Mesmo script | Hash do token, validade, uso e usuário. |
| Cursos | Mesmo script | Nome, descrição, horas, situação. |
| Modulos | Mesmo script | Curso, título, ordem, situação. |
| Materiais | Mesmo script | Módulo, título, tipo, URL, situação. |
| Matriculas | Mesmo script | Aluno/curso, situação/data, unicidade ativa. |
| Avaliacoes | Mesmo script | Módulo, título, situação. |
| Questoes | Mesmo script | Avaliação, enunciado, situação. |
| Alternativas | Mesmo script | Questão, texto, gabarito. |
| TentativasAvaliacao | Mesmo script | Aluno/prova/acertos/total/percentual/nota/aprovação/data. |
| RespostasAluno | Mesmo script | Tentativa/avaliação/questão/alternativa/correção; FKs compostas. |
| ProgressoModulos | Mesmo script | Aluno/módulo/data; conclusão única. |
| AcompanhamentosMentor | Mesmo script | Mentor/aluno/situação; vínculo único. |
| OrientacoesMentor | Mesmo script | Mentor/aluno/texto/data e vínculo existente. |
| AuditoriaUsuarios | Mesmo script | Valores anteriores/novos de perfil/situação/data. |
| Sessoes — complemento técnico | Mesmo script | Usuário/token hash/validade/revogação/data. |
| Sem tabela separada Alunos | Usuarios.Perfil | Quatro perfis na mesma tabela. |
| MER/modelos/dicionário | Documentacao/banco.md | Diagramas e todas as colunas correspondem ao script. |
| PK/FK/UNIQUE/CHECK/índices | Script e banco.md | Script completo corrigido executado no novo banco vazio TechLearnAcademyValidacao20261004; objetos conferidos; demais restrições ainda exigem cenários próprios. |
| sp_RealizarMatricula | Script e CursosServico | Matrículas sequencial e concorrente comprovadas sem duplicação; carga e demais disputas pendentes. |
| sp_RelatorioCurso | Script e GestaoServico | Métricas documentadas; consulta geral real comprovada; conjuntos maiores/vazios e filtro CursoId ainda não exercitados. |
| tr_AuditoriaUsuarios | Script | Auditoria de perfil/situação comprovada no SQL; atualização somente de nome/e-mail ainda não exercitada. |
| Script SQL completo | Banco/01-criar-banco.sql | Criar em instância nova, SQLCMD, sem atingir banco antigo. |

## 7. Escopo excluído e proteções da entrega

| Limite | Como conferir |
|---|---|
| Sem chat em tempo real | Orientações são registros históricos, sem canal de chat. |
| Sem pagamentos internos | Assinatura somente hipótese de negócio; nenhum endpoint de pagamento. |
| Sem certificados | Nenhuma geração/validação de certificado. |
| Sem aulas ao vivo | Materiais por PDF/Link, sem sala de transmissão. |
| Sem inteligência artificial | Nenhum serviço ou funcionalidade de IA no produto. |
| Sem streaming próprio | Videos externos por URL; sem infraestrutura de transmissão. |
| Sem SQL nos clientes | Só a API tem conexão; aplicativos abrem interface/HTTP. |
| Sem usuário/senha fixos | Primeiro administrador recebe dados do operador; sem seed de contas. Comando --criar-admin criou uma conta no banco de validação e recusou segunda execução sem criar outra. |
| Sem banco antigo alterado | Nova raiz; scripts recusam banco existente; nada executado no banco antigo. |
| Sem senha/token em armazenamento web | Sem localStorage/sessionStorage; sessão por cookie HttpOnly e hash no servidor. |

## 8. Correções dos problemas atuais listados no TXT

| Item do TXT | Resposta da solução nova | Teste/limitação |
|---|---|---|
| Login devolvia só mensagem | Identificação/nome/e-mail/perfil/token e cookie no contrato/API | Login real com SQL isolado comprovado. |
| Direcionamento não reconhecia perfis | Quatro destinos por perfil vindo da API | Papéis exercitados na API e quatro destinos/painéis conferidos no navegador. Experiência integral dos controles e dispositivos ainda precisa de teste. |
| Painéis não confirmados | Quatro páginas obrigatórias | Quatro painéis abertos no navegador com dados da API; todos os indicadores/estados ainda exigem cenários próprios. |
| Recuperação inexistente | Telas, rotas, SMTP, tokens hash/uso único | Fluxo real SQL e SMTP local comprovado; provedor externo e cenários restantes ainda previstos. |
| Senha 8 no frontend e 15 na API | Regra unificada mínimo 15 e complexidade | Testar clientes e servidor; projeto antigo não alterado. |
| Banco só Usuarios | Script com 16 tabelas completo | Arquivo corrigido integralmente executado em novo banco vazio, com 16 tabelas, 19 índices adicionais, duas procedures e uma trigger conferidos. |
| LocalDB não compartilhado | Endereço/conexão da API configuráveis; plano SQL compartilhado | Publicação real/configuração de rede pendentes. |
| Mobile/desktop inexistentes | Projetos C# MAUI/WebView e WinForms/WebView2 | Desktop, MAUI Android e MAUI Windows compilados; execução nativa por plataforma e aceite acadêmico ainda necessários. |
| Documentos com dois perfis | Contrato/relatório/matriz com quatro | Documentos antigos preservados; novo conteúdo consistente. |
| Mentor/Gestor excluídos | Mentoria e gestão com rotas/páginas próprias | Testar restrições e vínculos SQL. |
| Administração acadêmica no Administrador | Gestor altera conteúdo; Administrador altera usuários | Testar 403 nos endpoints fora do perfil. |
| Web separado da solução API | Nova solução organizada; API serve arquivos Web | Uso integrado na mesma origem; prévia Web não substitui API. |

## 9. Plano de testes de aceite

Os IDs abaixo são **cenários**, não nomes de testes automatizados supostamente existentes. Registrar o resultado observado somente após executar.

| ID | Preparação e ação | Resultado esperado | Dependência |
|---|---|---|---|
| T01 | Abrir login/cadastro/recuperação/painéis e 24 telas | Sem quebra de HTML/JS; layout legível e estados vazios claros | Navegador/API hospedeira. |
| T02 | Cadastrar aluno e repetir e-mail | 201 real e 409; usuário Aluno; hash armazenado | SQL isolado. |
| T03 | Login correto/incorreto/inativo e quatro perfis | 200 ou 401; perfil real; painel correto | SQL/contas criadas pelo operador. |
| T04 | Chamar ações protegidas sem sessão/perfil errado | 401/403, sem dados indevidos | SQL/API. |
| T05 | Logout e reutilização de sessão | Sessão revogada recusada | SQL/API. |
| T06 | Recuperar/trocar/reutilizar token e testar vencimento | E-mail real; uma troca; repetição/expiração recusadas | SMTP real/SQL. |
| T07 | Desligar SMTP/API/SQL | Erro claro; nenhum sucesso fictício | Ambiente isolado, sem produção. |
| T08 | Gestor cria curso/módulo/PDF/Link; outro perfil tenta alterar | Dados reais; outro perfil recusado; desativação preserva histórico | SQL/API. |
| T09 | Duas matrículas concorrentes do mesmo aluno/curso | Uma matrícula ativa; segunda recusada | SQL/API. |
| T10 | Aluno não matriculado tenta conteúdo/prova | Recusa sem gabarito ou material protegido | SQL/API. |
| T11 | Concluir 2 de 4 módulos; repetir um deles | 50% e nenhuma duplicação | SQL/API. |
| T12 | Questão com 1 alternativa/2 corretas; tentativa inválida | 400; não há gravação parcial | SQL/API. |
| T13 | Prova completa com 3/5 acertos | Nota 6, percentual 60, aprovado; histórico e progresso coerentes | SQL/API. |
| T14 | Alternativa de outra questão/prova ou questão duplicada | Recusa; FKs não permitem combinação inválida | SQL/API. |
| T15 | Aluno/mentor tenta Id de resultado alheio | Recusa; mentor acessa somente vínculo válido | SQL/API. |
| T16 | Mentor registra orientação em vínculo ativo/inexistente | Somente vínculo válido é aceito; histórico consultável | SQL/API. |
| T17 | Relatório com curso sem dados e com várias tentativas | Zeros claros ou médias/taxas conforme banco.md, sem multiplicação | SQL/API. |
| T18 | Administrador muda perfil/situação e tenta remover último ativo | Auditoria inserida; último administrador preservado | SQL/API. |
| T19 | Curso cadastrado no desktop, consultado no mobile/web | Mesmos dados sem bancos locais separados | Rede/SQL/aplicativos. |
| T20 | Teclado, leitor de tela, contraste e zoom | Foco/rótulos/mensagens claros; sem perda de controles | Navegador/leitor/aparelho. |
| T21 | Backup, restauração em banco novo e fluxo de consulta | Cópia recuperável; original intacto | SQL/armazenamento/operador. |
| T22 | API/interface publicadas com HTTPS e segredos fora do Git | Clientes acessam domínio confiável; SQL restrito à API | Infraestrutura/homologação. |

## 10. Etapas acadêmicas obrigatórias

| Etapa do PIM IV no TXT | Entrega associada | Limitação que deve permanecer explícita |
|---|---|---|
| Caracterização da organização | Relatório 1–3 | Organização e números hipotéticos, sem pesquisa inventada. |
| Planejamento da solução | Contrato, relatório 2/5/8, esta matriz | Plano deve acompanhar o que o grupo realmente valida. |
| Responsabilidade social/diversidade | Relatório 4 | Ações propostas; piloto/impacto ainda não comprovados. |
| Desenvolvimento web | Projeto Web completo e API integrada | 47 eventos de integração aprovados; percurso real do Aluno e direcionamento quatro perfis no navegador. Demais controles e SMTP externo ainda não conferidos. |
| Desenvolvimento mobile | Projeto Mobile C# MAUI/WebView | Requer workload/alvo, dispositivo e confirmação pedagógica. |
| Arquitetura de software | Relatório 5, pastas da API | WebView reutiliza interface; não afirmar interfaces nativas separadas. |
| Banco de dados | Scripts e banco.md | Não executar no banco antigo; SQL isolado e fluxos principais comprovados; validações restantes ainda previstas. |
| Nuvem e DevOps | Relatório 9; plano de CI/publicação; Dockerfile; scripts SQL | Plano/arquivos não provam CI, publicação ou Docker executados. Backup e restauração isolados foram executados; operação hospedada e recuperação completa ainda pendentes. |
| Gerenciamento ágil | Backlog/sprints/DoD do relatório | Sem inventar reuniões, horas, burndown ou entregas anteriores. |

## 11. Registro de evidências a preencher

| Campo | Valor |
|---|---|
| Cenário | [T01–T22 ou outro ID] |
| Data e executor | [PREENCHER SOMENTE APÓS EXECUÇÃO] |
| Versão/commit | [PREENCHER] |
| Ambiente/dependências | [PREENCHER] |
| Passos realizados | [PREENCHER] |
| Resultado esperado | [REFERENCIAR MATRIZ] |
| Resultado observado | [PREENCHER SEM INVENTAR] |
| Situação | [APROVADO / REPROVADO / PENDENTE] |
| Print/log sem segredos | [CAMINHO DE EVIDÊNCIA REAL] |

Este arquivo entrega rastreabilidade, plano de aceite e cobertura parcial comprovada. O registro detalhado dos comandos e resultados está em `Documentacao/testes-executados.md`.

## 12. Cobertura real registrada em 4 out. 2026

Evidências: `Testes/evidencias/integracao.json`, com **30 eventos PASSOU**, e `Testes/evidencias/adicionais.json`, com **17 eventos PASSOU**. São **47 eventos de integração**. A API foi executada em HTTP Development em loopback, com SQL Server LocalDB numa instância exclusiva e um receptor SMTP local. Foram criadas contas/dados apenas nesse ambiente de teste, com credenciais aleatórias; não há conta fixa na aplicação. O banco antigo não foi utilizado.

O projeto **TechLearnAcademy.Tests** também executou **22 verificações isoladas em C#**, sem SQL, de senha/hash/espaços, tokens, nome, e-mail e URL de material. A soma é **69 verificações registradas**, não 69 requisitos integralmente validados. Compilações, navegador, comando inicial do administrador e publicação local são evidências adicionais, fora dessa soma. Alguns eventos de integração agrupam várias chamadas HTTP. Não é correto descrever esses resultados como respostas falsas/simuladas: houve gravação/consulta SQL real e envio pelo protocolo SMTP a um receptor local. A entrega em provedor/caixa postal externos não foi testada.

### 12.1 Correspondência entre plano T01–T22 e observado

| Cenário | Cobertura realmente executada | Parte ainda não comprovada |
|---|---|---|
| T01 | 24 páginas HTTP 200; oito JS; 157 links; 42 labels; cadastro/login, percurso do Aluno e quatro painéis no navegador; pacote Release local executado com 24 HTMLs HTTP 200 | Funcionamento de todos os controles de edição/desativação, demais telas e estados vazios. |
| T02 | Cadastros reais sempre Aluno, perfil extra ignorado, e-mail repetido recusado e hash consultado; testes C# de limites do nome e regras da senha/e-mail | Todos os limites de tamanho em integração e disputas simultâneas de cadastro. |
| T03 | Login válido/incorreto/inativo na API; papéis reautenticados; quatro destinos/painéis com dados da API e saída conferidos no navegador | Experiência integral dos controles/estados dos painéis e aplicativos em execução. |
| T04 | Perfil sem sessão/token aleatório recusado; Aluno não entra no painel Admin; Administrador não cria curso; Mentor/resultados sem vínculo recusados; status/banco sem sessão retornou 401 no Release local | Matriz integral de todas as rotas × quatro perfis. |
| T05 | Logout revogou sessão; inativação e redefinição também encerraram sessões antigas | Expiração da sessão de oito horas e corrida de revogação. |
| T06 | SMTP local recebeu link; redefinição real, token reutilizado/vencido recusado | Provedor externo, TLS/autenticação SMTP, caixa postal e consumo concorrente. |
| T07 | SMTP sem configuração respondeu 503, sem sucesso fictício | Desligar API/SQL/servidor SMTP e conferir todas as mensagens/recuperações. |
| T08 | Gestor criou curso, módulo, material PDF e avaliação; Administrador recusado ao criar curso; edição histórica da questão recusada, desativação preservou histórico | Demais edições/ativações/desativações; links e relacionamentos inválidos em integração. |
| T09 | Matrícula real no navegador; segunda solicitação sequencial recusada; duas chamadas simultâneas resultaram em 201/409 e uma matrícula ativa no SQL | Deadlock/repetição, outros cenários concorrentes e carga. |
| T10 | Material e avaliação recusados sem matrícula; material permitido após matrícula | Todos os estados de inativação e outras rotas de conteúdo. |
| T11 | Aprovação a 60%/100% concluiu módulo; reprovação não concluiu; progresso real conferido no navegador | 2/4 módulos, conclusão manual repetida e vários cursos. |
| T12 | Questão com uma alternativa/duas corretas recusada; respostas vazias, questão repetida e alternativa inválida/de outra questão recusadas sem tentativa parcial | Nenhuma correta, demais conflitos/transações e falhas durante gravação. |
| T13 | 1/1=100%/nota 10; 3/5=60%/nota 6 aprovado; 2/5=40%/nota 4 reprovado; 1/32=3,13%/nota 0,31; avaliação/resultado real no navegador | Demais limites, tamanhos e percursos visuais de reprovação. |
| T14 | Alternativa inexistente ou pertencente a outra questão e questão duplicada recusadas, sem tentativa parcial | FK composta diretamente inválida e demais combinações entre provas. |
| T15 | Resultado próprio consultado e aluno alheio recusado; Mentor sem vínculo recusado em acompanhamento | Resultado específico consultado por Mentor vinculado e demais combinações. |
| T16 | Vínculo real; Mentor registrou orientação lida pelo Aluno; orientação sem vínculo recusada | Vínculo inativo, alterações de perfis e histórico visual completo. |
| T17 | Relatório retornou dados SQL reais pela procedure | Vários cursos/tentativas, conjunto vazio, filtro CursoId e recomposição manual de métricas. |
| T18 | Promoção, inativação, último Administrador preservado e trigger consultada; comando inicial --criar-admin criou uma conta e recusou segunda execução no banco de validação | Todas as alterações permitidas/recusadas e atualização apenas de nome/e-mail. |
| T19 | API, Web, Tests, desktop Windows, MAUI Android e MAUI Windows compilados com zero erros/avisos | Executar os clientes nativos e confirmar alteração compartilhada entre eles. |
| T20 | Labels/links conferidos; navegador a 390 px sem transbordamento horizontal, foco inicial por Tab e prints | Leitor de tela, percurso completo por teclado, contraste medido, zoom e demais larguras/telas. |
| T21 | Backup COPY_ONLY/CHECKSUM passou no VERIFYONLY e foi restaurado no novo TechLearnAcademy_Restauracao; 10 usuários na origem e 10 na cópia | CHECKDB, comparação integral das tabelas, fluxo da aplicação na cópia e tempo de recuperação. |
| T22 | Origem HTTP não autorizada recusada no teste local | Publicação HTTPS/nuvem, domínio/certificado e rede SQL restrita em ambiente hospedado. |

### 12.2 Estrutura SQL confirmada

Na instância exclusiva foram confirmadas **16 tabelas, 19 índices adicionais, duas procedures e uma trigger**. A primeira tentativa interrompeu num índice filtrado porque a sessão `sqlcmd` estava com QUOTED_IDENTIFIER OFF. Após configurar explicitamente as opções SET, o **arquivo corrigido inteiro foi executado do início ao fim, sem complemento, no novo banco vazio TechLearnAcademyValidacao20261004**. A cópia de teste mudou somente o nome do banco. Matrícula, relatório e auditoria foram exercitados pela integração real.

Backup COPY_ONLY/CHECKSUM, VERIFYONLY e restauração no novo TechLearnAcademy_Restauracao também foram executados, com comparação de 10 usuários na origem e 10 na cópia. Nenhum banco existente foi substituído. Criação/restauração bem-sucedidas não comprovam isoladamente todo CHECK, FK, índice, fluxo da cópia ou cenário concorrente. As regras RN e o plano acima separam cobertura positiva, negativa e casos pendentes.

### 12.3 Limites das evidências

Cadastro, login incorreto/correto, matrícula, listagem de material, avaliação, resultado, progresso e logout do Aluno foram conferidos no navegador. Também foram conferidos direcionamento, abertura com dados da API e saída dos quatro painéis. Os perfis da conta visual foram alterados somente no SQL isolado de teste. Houve prints, largura 390 px sem overflow e conferência inicial do foco por Tab. Isso não comprova todos os controles/estados das páginas ou uma auditoria WCAG integral.

Não se afirma execução nativa desktop/MAUI em aparelho/emulador, SMTP externo, IMAP, HTTPS publicado/nuvem/IIS, CI externo, carga, toda a matriz de permissões, consumo concorrente da recuperação ou recuperação completa do serviço na cópia restaurada. A matrícula concorrente e a restauração descritas foram executadas e não são pendências. Consulte `Documentacao/testes-executados.md` para os arquivos de evidência. Novas execuções devem ampliar esta matriz conforme os cenários restantes forem realizados.
