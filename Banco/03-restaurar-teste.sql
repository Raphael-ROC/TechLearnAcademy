-- Restaura uma cópia em um banco NOVO, nunca substitui o original.
USE master;

DECLARE @BancoNovo sysname = N'TechLearnAcademy_Restauracao';
DECLARE @ArquivoBackup nvarchar(4000) = N'PREENCHA_CAMINHO\TechLearnAcademy-data-hora.bak';
DECLARE @ArquivoDados nvarchar(4000) = N'PREENCHA_CAMINHO\TechLearnAcademy_Restauracao.mdf';
DECLARE @ArquivoLog nvarchar(4000) = N'PREENCHA_CAMINHO\TechLearnAcademy_Restauracao_log.ldf';
-- Confira estes nomes com RESTORE FILELISTONLY antes de restaurar.
DECLARE @NomeDados sysname = N'TechLearnAcademy';
DECLARE @NomeLog sysname = N'TechLearnAcademy_log';

IF @ArquivoBackup LIKE N'PREENCHA%' OR @ArquivoDados LIKE N'PREENCHA%' OR @ArquivoLog LIKE N'PREENCHA%'
    THROW 51000, 'Configure os caminhos do backup e dos novos arquivos.', 1;

IF @BancoNovo = N'TechLearnAcademy' OR DB_ID(@BancoNovo) IS NOT NULL
    THROW 51000, 'O destino deve ser um novo banco de teste, diferente do banco original.', 1;

RESTORE VERIFYONLY FROM DISK = @ArquivoBackup WITH CHECKSUM;

-- Sem REPLACE: o SQL Server não substitui um banco existente.
RESTORE DATABASE @BancoNovo FROM DISK = @ArquivoBackup
    WITH MOVE @NomeDados TO @ArquivoDados,
         MOVE @NomeLog TO @ArquivoLog,
         CHECKSUM, RECOVERY, STATS = 10;
