-- Execute no banco utilizado pelo site. Não apaga notas nem matrículas.
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
SET XACT_ABORT ON;
BEGIN TRY
    BEGIN TRANSACTION;
    IF COL_LENGTH('dbo.Usuarios','EmailContato') IS NULL
        THROW 51000, 'Execute primeiro Banco/05-atualizar-funcionalidades.sql.', 1;
    IF EXISTS(SELECT EmailContato FROM dbo.Usuarios WHERE EmailContato IS NOT NULL GROUP BY EmailContato HAVING COUNT(*)>1)
        THROW 51000, 'Há e-mails pessoais repetidos. Edite os usuários para informar um contato diferente em cada conta antes desta atualização.', 1;
    IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.Usuarios') AND name='UQ_Usuarios_EmailContato')
        EXEC(N'CREATE UNIQUE INDEX UQ_Usuarios_EmailContato ON dbo.Usuarios(EmailContato) WHERE EmailContato IS NOT NULL;');

    -- A tentativa começa em 1. Reiniciar o curso aumenta este número.
    IF COL_LENGTH('dbo.Matriculas','TentativaCurso') IS NULL
        ALTER TABLE dbo.Matriculas ADD TentativaCurso int NOT NULL CONSTRAINT DF_Matriculas_TentativaCurso DEFAULT 1 CONSTRAINT CK_Matriculas_TentativaCurso CHECK(TentativaCurso>0);
    IF COL_LENGTH('dbo.TentativasAvaliacao','TentativaCurso') IS NULL
        ALTER TABLE dbo.TentativasAvaliacao ADD TentativaCurso int NOT NULL CONSTRAINT DF_Tentativas_TentativaCurso DEFAULT 1 CONSTRAINT CK_Tentativas_Curso CHECK(TentativaCurso>0);
    IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('dbo.TentativasAvaliacao') AND name='IX_Tentativas_CursoAtual')
        EXEC(N'CREATE INDEX IX_Tentativas_CursoAtual ON dbo.TentativasAvaliacao(AlunoId,AvaliacaoId,TentativaCurso);');
    -- Notas anteriores ficam na tentativa 1. O servidor bloqueia novos envios
    -- de avaliações já respondidas, preservando o histórico que já existia.
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
