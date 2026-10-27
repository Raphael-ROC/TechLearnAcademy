-- Atualização das novas funções. Execute no banco atual; pode ser repetida.
USE TechLearnAcademy_PIMIV;
GO
-- Evita fechar e reabrir o banco a cada acesso do site.
ALTER DATABASE TechLearnAcademy_PIMIV SET AUTO_CLOSE OFF;
GO
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
        ALTER TABLE dbo.Usuarios ADD EmailContato nvarchar(254) NULL;
    EXEC(N'UPDATE dbo.Usuarios SET EmailContato=Email WHERE EmailContato IS NULL;');
    IF COL_LENGTH('dbo.Cursos','Excluido') IS NULL
        ALTER TABLE dbo.Cursos ADD Excluido bit NOT NULL DEFAULT 0;
    IF COL_LENGTH('dbo.Modulos','Excluido') IS NULL
        ALTER TABLE dbo.Modulos ADD Excluido bit NOT NULL DEFAULT 0;
    IF COL_LENGTH('dbo.Materiais','Excluido') IS NULL
        ALTER TABLE dbo.Materiais ADD Excluido bit NOT NULL DEFAULT 0;
    IF COL_LENGTH('dbo.Avaliacoes','Excluido') IS NULL
        ALTER TABLE dbo.Avaliacoes ADD Excluido bit NOT NULL DEFAULT 0;
    IF COL_LENGTH('dbo.Materiais','ArquivoNome') IS NULL
        ALTER TABLE dbo.Materiais ADD ArquivoNome nvarchar(255) NULL, ArquivoPdf varbinary(max) NULL;
    IF EXISTS(SELECT 1 FROM sys.key_constraints WHERE name='UQ_Modulos_Ordem')
        ALTER TABLE dbo.Modulos DROP CONSTRAINT UQ_Modulos_Ordem;
    IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='UQ_Modulos_OrdemAtual' AND object_id=OBJECT_ID('dbo.Modulos'))
        EXEC(N'CREATE UNIQUE INDEX UQ_Modulos_OrdemAtual ON dbo.Modulos(CursoId,Ordem) WHERE Excluido=0;');
    IF EXISTS(SELECT 1 FROM sys.check_constraints WHERE name='CK_Materiais_Url')
        ALTER TABLE dbo.Materiais DROP CONSTRAINT CK_Materiais_Url;
    EXEC(N'ALTER TABLE dbo.Materiais ADD CONSTRAINT CK_Materiais_Url CHECK
        ((Tipo=N''PDF'' AND ArquivoPdf IS NOT NULL AND DATALENGTH(ArquivoPdf)<=10485760)
         OR (ArquivoPdf IS NULL AND (Url LIKE N''https://%'' OR Url LIKE N''http://%'')));');
    IF OBJECT_ID('dbo.Chamados','U') IS NULL
    BEGIN
        CREATE TABLE dbo.Chamados(
            Id int IDENTITY PRIMARY KEY,
            SolicitanteId int NOT NULL REFERENCES dbo.Usuarios(Id),
            ResponsavelId int NULL REFERENCES dbo.Usuarios(Id),
            CursoId int NULL REFERENCES dbo.Cursos(Id),
            Tipo nvarchar(30) NOT NULL CHECK(Tipo IN ('AjudaCurso','Ouvidoria','ContatoAdministracao')),
            DestinatarioPerfil nvarchar(20) NOT NULL CHECK(DestinatarioPerfil IN ('Mentor','Gestor','Administrador')),
            Assunto nvarchar(150) NOT NULL,
            Descricao nvarchar(4000) NOT NULL,
            Status nvarchar(20) NOT NULL DEFAULT 'Aberto' CHECK(Status IN ('Aberto','Em atendimento','Encerrado')),
            DataAbertura datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
            DataEncerramento datetime2 NULL
        );
        CREATE INDEX IX_Chamados_Fila ON dbo.Chamados(DestinatarioPerfil,Status,ResponsavelId);
        CREATE INDEX IX_Chamados_Solicitante ON dbo.Chamados(SolicitanteId,CursoId);
    END;
    IF OBJECT_ID('dbo.MensagensChamado','U') IS NULL
        CREATE TABLE dbo.MensagensChamado(
            Id int IDENTITY PRIMARY KEY,
            ChamadoId int NOT NULL REFERENCES dbo.Chamados(Id),
            UsuarioId int NOT NULL REFERENCES dbo.Usuarios(Id),
            Texto nvarchar(4000) NOT NULL,
            DataEnvio datetime2 NOT NULL DEFAULT SYSUTCDATETIME()
        );
    IF OBJECT_ID('dbo.SolicitacoesCertificado','U') IS NULL
    BEGIN
        CREATE TABLE dbo.SolicitacoesCertificado(
            Id int IDENTITY PRIMARY KEY,
            AlunoId int NOT NULL REFERENCES dbo.Usuarios(Id),
            CursoId int NOT NULL REFERENCES dbo.Cursos(Id),
            Status nvarchar(20) NOT NULL DEFAULT 'Pendente' CHECK(Status IN ('Pendente','Aprovado','Recusado')),
            GestorId int NULL REFERENCES dbo.Usuarios(Id),
            Motivo nvarchar(1000) NULL,
            Codigo varchar(64) NULL,
            NomeAluno nvarchar(100) NULL,
            NomeCurso nvarchar(150) NULL,
            CargaHoraria int NULL,
            DataSolicitacao datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
            DataAprovacao datetime2 NULL,
            CONSTRAINT UQ_Certificado_AlunoCurso UNIQUE(AlunoId,CursoId)
        );
        CREATE UNIQUE INDEX UQ_Certificado_Codigo ON dbo.SolicitacoesCertificado(Codigo) WHERE Codigo IS NOT NULL;
    END;
    IF EXISTS(SELECT 1 FROM sys.foreign_keys WHERE name='FK_Orientacoes_Vinculo')
        ALTER TABLE dbo.OrientacoesMentor DROP CONSTRAINT FK_Orientacoes_Vinculo;
    -- A tabela de vínculos antigos permanece como histórico, sem uso pela aplicação.
    EXEC(N'CREATE OR ALTER TRIGGER dbo.tr_LimiteModulos ON dbo.Modulos AFTER INSERT AS
    BEGIN
        SET NOCOUNT ON;
        IF EXISTS(SELECT 1 FROM dbo.Modulos m JOIN inserted i ON i.CursoId=m.CursoId
                  WHERE m.Excluido=0 GROUP BY m.CursoId HAVING COUNT(DISTINCT m.Id)>4)
            THROW 51004, ''O curso pode ter no máximo quatro módulos.'', 1;
    END;');
    -- Conclusões antigas sem aprovação não liberam certificados.
    EXEC(N'DELETE p FROM dbo.ProgressoModulos p WHERE
        NOT EXISTS(SELECT 1 FROM dbo.Avaliacoes a WHERE a.ModuloId=p.ModuloId AND a.Ativo=1 AND a.Excluido=0)
        OR EXISTS(SELECT 1 FROM dbo.Avaliacoes a OUTER APPLY
            (SELECT TOP(1) t.Aprovado FROM dbo.TentativasAvaliacao t WHERE t.AvaliacaoId=a.Id AND t.AlunoId=p.AlunoId ORDER BY t.Id DESC) ultima
            WHERE a.ModuloId=p.ModuloId AND a.Ativo=1 AND a.Excluido=0 AND COALESCE(ultima.Aprovado,0)=0);');
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
SELECT 'Atualização concluída.' AS Mensagem;

-- Correções de recuperação e avaliações de 11/10/2026.
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
