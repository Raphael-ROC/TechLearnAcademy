# Como alterar o projeto

1. Atualize sua cópia com `git pull` antes de começar.
2. Crie uma branch com um nome que descreva a tarefa, por exemplo `corrigir-cadastro`.
3. Altere somente o que pertence à tarefa.
4. Execute as verificações indicadas no README e confira a tela ou o fluxo afetado.
5. Veja o diff com `git diff` e selecione os arquivos com `git add caminho/do/arquivo`.
6. Faça um commit explicando a mudança e envie a branch.

Exemplos de mensagens:

- `Corrige a confirmação de senha no cadastro`
- `Adiciona filtro por perfil na lista de usuários`
- `Documenta a configuração do banco local`

Um commit deve reunir uma mudança que faça sentido ser revisada em conjunto. Evite juntar ajustes de banco, telas e documentação sem relação na mesma alteração.

Não altere datas ou autores para simular um histórico de desenvolvimento. Não adicione credenciais, dumps, backups ou dados pessoais. As credenciais são configuradas no ambiente local ou no Azure.

A publicação em produção é uma etapa separada do envio ao GitHub. O workflow de verificação apenas compila e confere o projeto; ele não publica no Azure nem executa scripts no banco.
