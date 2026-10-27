# Scripts do banco

Os arquivos desta pasta são código SQL. Não são cópias dos registros do banco em produção. Backups `.bak` e arquivos `.mdf`/`.ldf` ficam fora do Git.

## Instalação nova usada pelo site

1. `Instalar-banco-do-site.sql`: cria `TechLearnAcademy_PIMIV` e interrompe se o banco já existir.
2. `05-atualizar-funcionalidades.sql`: complementa o esquema das funcionalidades.
3. `07-corrigir-recuperacao-e-avaliacoes.sql`: atualiza recuperação, avaliações e regras relacionadas.
4. `06-cadastrar-cursos-pdfs.sql`: importação opcional do catálogo e dos PDFs. Exige modo SQLCMD, ajuste de `PastaMateriais` e arquivos externos fornecidos pelo responsável.

Confira o contexto `USE` e os comentários de cada arquivo no SSMS antes da execução. Use modo SQLCMD com interrupção em erro quando indicado. Não execute estes passos sobre o banco do Azure para reproduzir uma instalação local.

## Scripts preservados da versão inicial

| Arquivo | Finalidade |
| --- | --- |
| `01-criar-banco.sql` | Instalação original em um banco chamado `TechLearnAcademy`, diferente do banco `TechLearnAcademy_PIMIV` usado pelo site atual. |
| `02-backup.sql` | Exemplo de backup; exige caminho e banco corretos. |
| `03-restaurar-teste.sql` | Exemplo de restauração em destino separado para teste. |
| `04-completar-banco-existente.sql` | Atualização de uma instalação anterior; não faz parte da sequência acima para banco novo. |

Escolha uma sequência de instalação; não execute todos os arquivos numerados indiscriminadamente. As ferramentas antigas de integração usam uma instância exclusiva e o banco da versão inicial.

Os PDFs dos materiais, os dados pessoais dos usuários e as credenciais do Azure não acompanham o repositório. A importação do catálogo não é executada pelos workflows.
