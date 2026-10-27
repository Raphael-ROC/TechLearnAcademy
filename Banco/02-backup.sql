-- Preencha o caminho de um NOVO arquivo na máquina do SQL Server.
-- Não execute automaticamente nem use o banco antigo do grupo.
USE master;

DECLARE @ArquivoBackup nvarchar(4000) = N'PREENCHA_CAMINHO\TechLearnAcademy-data-hora.bak';

IF @ArquivoBackup LIKE N'PREENCHA%'
    THROW 51000, 'Configure o caminho e um nome de arquivo exclusivo antes de executar.', 1;

IF DB_ID(N'TechLearnAcademy') IS NULL
    THROW 51000, 'Banco não encontrado nesta instância. Confira a instância isolada.', 1;

-- COPY_ONLY preserva a rotina de backups; sem INIT, nada é sobrescrito.
BACKUP DATABASE TechLearnAcademy TO DISK = @ArquivoBackup
    WITH COPY_ONLY, CHECKSUM, STATS = 10;

RESTORE VERIFYONLY FROM DISK = @ArquivoBackup WITH CHECKSUM;
