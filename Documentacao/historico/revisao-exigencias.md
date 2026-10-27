# Revisão das exigências — 07/10/2026

## Correções desta revisão

A lista de usuários e o formulário de cadastro estavam na mesma página. Agora são duas páginas, com opções separadas no menu.

A troca de senha do administrador estava escondida. Agora o formulário não é inserido na página desse perfil; a API também bloqueia a operação.

O envio de PDF já tinha código, mas começava escondido pela escolha de link. Agora **Enviar documento PDF** é a opção inicial, com o botão de arquivo visível.

Os chamados já tinham conversa e atualização manual. Agora a fila e as mensagens chegam automaticamente a cada cinco segundos. O administrador recebe e responde às solicitações dos gestores.

Os relatórios foram conferidos com cursos, alunos, reprovações e aprovações. A página verifica as listas recebidas para evitar o erro técnico de leitura de `length` de um valor indefinido. Arquivos do site recebem a versão atual junto com a página.

A exclusão de curso, módulo, material e avaliação já existia no código. As rotas foram exercitadas e a confirmação de exclusão de curso foi conferida na tela.

Também foram corrigidos:

- Recálculo da conclusão ao ativar, desativar ou excluir avaliações.
- Histórico de atendimento: mensagens preservam qual mentor ou gestor já atendeu, mesmo quando sua conta é desativada.
- Texto antigo do painel do mentor que ainda falava em alunos abaixo de 50%.
- Horários das conversas e relatórios, convertidos do UTC do banco para Brasília.
- Indicação de carregamento enquanto a tela recebe os dados.

## Conferência de todos os pedidos

| Pedido | Estado atual e onde encontrar | Verificação |
| --- | --- | --- |
| Programação orientada a objetos simples | Nove classes principais em `Modelos`; serviços existentes coordenam as operações. | Contagem das classes e 50 testes de regras. |
| Manter caminhos e funcionamento | As páginas anteriores foram mantidas; cadastro ganhou sua própria página. | Referências de HTML conferidas, sem arquivos faltando. |
| Usar SQL Server | LocalDB e banco `TechLearnAcademy_PIMIV`; tabelas de apoio para mensagens e certificados. | Instalação completa em banco fictício e conferência do banco atual. |
| No máximo quatro módulos | Gestor → Conteúdo. A API e o banco impedem o quinto módulo. | Rejeição do quinto módulo e reaproveitamento da ordem após exclusão. |
| Identificar conclusão do aluno | Aprovação em todas as avaliações atuais conclui o módulo automaticamente. | Progresso de 0%, 75% e 100% conferido. |
| Identificar aprovação | Nota mínima 6, equivalente a 60% de acertos. | Limite de 59%/60%, reprovação e nova tentativa. |
| Reprovação exige reiniciar o curso (regra atualizada em 11/10/2026) | Curso ou resultado → Reiniciar curso. Cada avaliação aceita um envio por tentativa do curso. | Reinício zera todos os módulos, preserva o histórico e invalida provas abertas na tentativa anterior. |
| Certificado solicitado ao gestor | Aluno → Certificados → Solicitar certificado. | Solicitação bloqueada antes da aprovação e duplicidade impedida. |
| Gestor aprova e gera certificado | Gestor → Certificados → Aprovar e gerar. | Aprovação, recusa com motivo e nova conferência das notas. |
| Aba de certificados | Aluno e gestor têm Certificados no menu. | Página do certificado e botão de impressão conferidos. |
| Ajuda dentro do curso | Aluno → Curso → Ajuda com mentor. | Chamado aberto pela tela com o curso selecionado. |
| Mentor escolhe chamado | Mentor → Ajuda e ouvidoria → Atender chamado. | Atendimento pela tela e disputa simultânea entre dois mentores. |
| Chat aluno/mentor | Abrir conversa no chamado do curso. | Mensagens nas duas direções e orientação registrada. |
| Ouvidoria para aluno e mentor | Ajuda e ouvidoria → Ouvidoria / ajuda com o gestor. | Encaminhamento e resposta da gestão. |
| Gestor contata administrador | Gestor → Chamados → Novo chamado. | Admin recebe, assume e responde; outros perfis não leem a conversa. |
| Admin tem aba de chamados | Admin → Chamados. | Conversa visível e resposta enviada pelo navegador. |
| Todos recebem respostas automaticamente | Fila e conversa verificadas a cada cinco segundos. | Resposta da gestão apareceu no admin sem atualização manual. |
| Gestor edita aluno e mentor | Gestor → Usuários → Editar dados. | Edição permitida para esses perfis; editar gestor é bloqueado. |
| Admin cadastra gestor e mentor | Admin → Cadastrar usuário. | Cadastro e geração dos dois tipos de e-mail institucional. |
| Gestor cadastra mentor | Gestor → Cadastrar usuário. | Opção Mentor na tela; tentativa de cadastrar gestor é recusada pela API. |
| E-mail institucional por nome e ID | Dois primeiros nomes e ID do SQL; domínios gestor/mentor.techlearn.com. | Nomes, acentos, ID e domínio conferidos. |
| Senha mínima de oito caracteres | Uma maiúscula, número, símbolo e nenhum espaço. | Senha com oito aceita; cinco tipos inválidos recusados. |
| Recuperação por e-mail, exceto admin | Login e perfil de aluno, mentor e gestor. | Envio, redefinição, revogação de sessões e token de uso único testados com SMTP local. **Falta configurar um serviço real.** |
| Retirar troca de senha do admin | Perfil do administrador contém apenas os dados da conta. | Formulário ausente do DOM e API retorna 403. |
| Retirar mudança de perfil | Perfil permanece fixo desde o cadastro. | Tentativa de promover aluno pela edição mantém Aluno. |
| Admin ativa/desativa contas | Admin → Usuários. | Conta desativada perde acesso; reativação permite login. |
| Retirar vínculo manual mentor/aluno | Atendimento começa quando o mentor assume um chamado. | Ausência de tela/rota de vínculo e acesso limitado aos alunos atendidos. |
| Moderação: mentor que atendeu aluno | Gestor → Relatórios → Mentores e alunos atendidos → Ver conversa. | Relação e histórico preservados, inclusive após troca de responsável. |
| Quantidade de atendimentos por gestor | Gestor → Relatórios → Quantidade de chamados por responsável. | Assumidos e encerrados conferidos, com histórico de respostas. |
| Relatório detalhado por curso/aluno | Gestor → Relatórios → Ver alunos e notas → Ver desempenho. | Módulos, última nota, acertos, percentual, tentativas e histórico conferidos na tela. |
| Vários materiais por módulo | Gestor → Conteúdo → Novo material / Adicionar outro material. | Quatro materiais no mesmo módulo. |
| Enviar documento PDF | Gestor → Conteúdo → Enviar documento PDF. | Upload pela página e pela API; download autorizado; limite de 10 MB e arquivo inválido recusados. |
| Campo chamado Link | Gestor → Conteúdo → Adicionar link. | Rótulo conferido e cadastro de link pela tela. |
| Excluir material | Botão Excluir no cartão de material. | Material deixa de aparecer. |
| Excluir módulo | Botão Excluir no cartão de módulo. | Conteúdos saem das telas e a ordem pode ser usada em novo módulo. |
| Excluir avaliação | Botão Excluir no cartão de avaliação. | Prova sai das telas e progresso é recalculado. |
| Excluir curso | Gestor → Cursos → Excluir. | Rota de exclusão e janela de confirmação conferidas; histórico de notas/certificados preservado. |
| Diálogos legíveis | Janelas com título, mensagem, contraste e foco no botão Cancelar. | Confirmação de exclusão observada no navegador. |
| Remover frase da página inicial | “Uma jornada, quatro perfis, muitas possibilidades.” removida. | Página inicial conferida. |
| Resolver atualização da página | Sem cache antigo; versão nos arquivos e atualização automática. | Página mudou de versão sozinha; rascunho permaneceu até salvar. |

## Evidências e limites

**357 verificações automatizadas passaram:** 225 HTTP + 82 complementares + 50 das regras.
A API e o projeto Web compilaram sem erros. Os nove arquivos JavaScript passaram na conferência de sintaxe.

Os testes usaram bancos `TechLearnAcademy_Revisao...`, separados do banco de uso real.
Nenhum cadastro, nota ou curso real foi alterado por esses testes.
O banco real permaneceu com seus três usuários, um curso e dois módulos, além das 19 tabelas já atualizadas.

Arquivos de apoio:

- `Testes/revisao_funcional.py` e `tmp/revisao/resultado-http.json`.
- `Testes/revisao_complementar.py` e `tmp/revisao/resultado-complementar.json`.
- `TechLearnAcademy.Tests/Program.cs`, com os 50 testes de regras.
- [Relatório detalhado](Evidencias/relatorio-aluno.png).
- [Lista de usuários do administrador](Evidencias/admin-usuarios.png).
- [Envio de PDF](Evidencias/material-pdf.png).
- [Conversa do mentor](Evidencias/chat-mentor.png).
- [Confirmação de exclusão](Evidencias/excluir-curso.png).
- [Certificado aprovado](Evidencias/certificado.png).

A única dependência externa pendente é o serviço real de envio de e-mail. O teste usou SMTP fictício no próprio computador, sem enviar mensagens externas.
A criação de um e-mail institucional gera o identificador de login; uma caixa postal depende de um serviço de e-mail.
O certificado é um documento HTML emitido pelo sistema; o botão usa a impressão do navegador para salvar em PDF.

