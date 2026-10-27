-- Execute somente em uma instância NOVA, no modo SQLCMD do SSMS.
-- O script interrompe se já existir um banco com este nome.
:ON ERROR EXIT

USE master;
GO

IF DB_ID(N'TechLearnAcademy') IS NOT NULL
    THROW 51000, 'Já existe TechLearnAcademy nesta instância. Use uma instância isolada.', 1;

CREATE DATABASE TechLearnAcademy COLLATE Latin1_General_100_CI_AS_SC;
GO

USE TechLearnAcademy;
GO

-- Índices filtrados exigem estas opções em todas as conexões.
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;

-- Se um objeto falhar, a conexão desfeita não deixa um esquema parcial.
BEGIN TRANSACTION;

-- Guarda somente o hash da senha, nunca a senha digitada.
CREATE TABLE dbo.Usuarios (
    Id int IDENTITY PRIMARY KEY,
    Nome nvarchar(100) NOT NULL,
    Email nvarchar(254) NOT NULL,
    SenhaHash nvarchar(500) NOT NULL,
    Perfil nvarchar(20) NOT NULL DEFAULT N'Aluno',
    Ativo bit NOT NULL DEFAULT 1,
    DataCadastro datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_Usuarios_Email UNIQUE (Email),
    CONSTRAINT CK_Usuarios_Nome CHECK (LEN(LTRIM(RTRIM(Nome))) BETWEEN 2 AND 100),
    CONSTRAINT CK_Usuarios_Email CHECK (LEN(Email) BETWEEN 3 AND 254 AND Email = LTRIM(RTRIM(Email))),
    CONSTRAINT CK_Usuarios_Hash CHECK (LEN(SenhaHash) >= 20),
    CONSTRAINT CK_Usuarios_Perfil CHECK (Perfil IN (N'Aluno', N'Mentor', N'Gestor', N'Administrador'))
);

CREATE TABLE dbo.RecuperacoesSenha (
    Id int IDENTITY PRIMARY KEY,
    UsuarioId int NOT NULL REFERENCES dbo.Usuarios(Id),
    TokenHash char(64) NOT NULL UNIQUE,
    Validade datetime2 NOT NULL,
    Utilizado bit NOT NULL DEFAULT 0,
    DataCriacao datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT CK_RecuperacoesSenha_Validade CHECK (Validade > DataCriacao),
    CONSTRAINT CK_RecuperacoesSenha_Hash CHECK (TokenHash NOT LIKE '%[^0-9A-Fa-f]%' COLLATE Latin1_General_100_BIN2)
);

CREATE TABLE dbo.Sessoes (
    Id int IDENTITY PRIMARY KEY,
    UsuarioId int NOT NULL REFERENCES dbo.Usuarios(Id),
    TokenHash char(64) NOT NULL UNIQUE,
    Validade datetime2 NOT NULL,
    Revogada bit NOT NULL DEFAULT 0,
    DataCriacao datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT CK_Sessoes_Validade CHECK (Validade > DataCriacao),
    CONSTRAINT CK_Sessoes_Hash CHECK (TokenHash NOT LIKE '%[^0-9A-Fa-f]%' COLLATE Latin1_General_100_BIN2)
);

CREATE TABLE dbo.Cursos (
    Id int IDENTITY PRIMARY KEY,
    Nome nvarchar(150) NOT NULL,
    Descricao nvarchar(2000) NOT NULL,
    CargaHoraria int NOT NULL,
    Ativo bit NOT NULL DEFAULT 1,
    CONSTRAINT CK_Cursos_Nome CHECK (LEN(LTRIM(RTRIM(Nome))) BETWEEN 2 AND 150),
    CONSTRAINT CK_Cursos_Descricao CHECK (LEN(LTRIM(RTRIM(Descricao))) > 0),
    CONSTRAINT CK_Cursos_CargaHoraria CHECK (CargaHoraria > 0)
);

CREATE TABLE dbo.Modulos (
    Id int IDENTITY PRIMARY KEY,
    CursoId int NOT NULL REFERENCES dbo.Cursos(Id),
    Titulo nvarchar(150) NOT NULL,
    Ordem int NOT NULL,
    Ativo bit NOT NULL DEFAULT 1,
    CONSTRAINT UQ_Modulos_Ordem UNIQUE (CursoId, Ordem),
    CONSTRAINT CK_Modulos_Titulo CHECK (LEN(LTRIM(RTRIM(Titulo))) > 0),
    CONSTRAINT CK_Modulos_Ordem CHECK (Ordem > 0)
);

CREATE TABLE dbo.Materiais (
    Id int IDENTITY PRIMARY KEY,
    ModuloId int NOT NULL REFERENCES dbo.Modulos(Id),
    Titulo nvarchar(150) NOT NULL,
    Tipo nvarchar(10) NOT NULL,
    Url nvarchar(2000) NOT NULL,
    Ativo bit NOT NULL DEFAULT 1,
    CONSTRAINT CK_Materiais_Titulo CHECK (LEN(LTRIM(RTRIM(Titulo))) > 0),
    CONSTRAINT CK_Materiais_Tipo CHECK (Tipo IN (N'PDF', N'Link')),
    CONSTRAINT CK_Materiais_Url CHECK (Url LIKE N'https://%' OR Url LIKE N'http://%')
);

CREATE TABLE dbo.Matriculas (
    Id int IDENTITY PRIMARY KEY,
    AlunoId int NOT NULL REFERENCES dbo.Usuarios(Id),
    CursoId int NOT NULL REFERENCES dbo.Cursos(Id),
    Ativo bit NOT NULL DEFAULT 1,
    DataMatricula datetime2 NOT NULL DEFAULT SYSUTCDATETIME()
);

CREATE TABLE dbo.Avaliacoes (
    Id int IDENTITY PRIMARY KEY,
    ModuloId int NOT NULL REFERENCES dbo.Modulos(Id),
    Titulo nvarchar(150) NOT NULL,
    Ativo bit NOT NULL DEFAULT 1,
    CONSTRAINT CK_Avaliacoes_Titulo CHECK (LEN(LTRIM(RTRIM(Titulo))) > 0)
);

CREATE TABLE dbo.Questoes (
    Id int IDENTITY PRIMARY KEY,
    AvaliacaoId int NOT NULL REFERENCES dbo.Avaliacoes(Id),
    Enunciado nvarchar(2000) NOT NULL,
    Ativo bit NOT NULL DEFAULT 1,
    CONSTRAINT UQ_Questoes_Avaliacao UNIQUE (Id, AvaliacaoId),
    CONSTRAINT CK_Questoes_Enunciado CHECK (LEN(LTRIM(RTRIM(Enunciado))) > 0)
);

CREATE TABLE dbo.Alternativas (
    Id int IDENTITY PRIMARY KEY,
    QuestaoId int NOT NULL REFERENCES dbo.Questoes(Id),
    Texto nvarchar(1000) NOT NULL,
    Correta bit NOT NULL DEFAULT 0,
    CONSTRAINT UQ_Alternativas_Questao UNIQUE (Id, QuestaoId),
    CONSTRAINT CK_Alternativas_Texto CHECK (LEN(LTRIM(RTRIM(Texto))) > 0)
);

CREATE TABLE dbo.TentativasAvaliacao (
    Id int IDENTITY PRIMARY KEY,
    AlunoId int NOT NULL REFERENCES dbo.Usuarios(Id),
    AvaliacaoId int NOT NULL REFERENCES dbo.Avaliacoes(Id),
    Acertos int NOT NULL,
    TotalQuestoes int NOT NULL,
    Percentual decimal(5,2) NOT NULL,
    Nota decimal(4,2) NOT NULL,
    Aprovado bit NOT NULL,
    DataTentativa datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_Tentativas_Avaliacao UNIQUE (Id, AvaliacaoId),
    CONSTRAINT CK_Tentativas_Acertos CHECK (TotalQuestoes > 0 AND Acertos BETWEEN 0 AND TotalQuestoes),
    CONSTRAINT CK_Tentativas_Percentual CHECK (Percentual BETWEEN 0 AND 100 AND Percentual = ROUND(100.0 * Acertos / TotalQuestoes, 2)),
    CONSTRAINT CK_Tentativas_Nota CHECK (Nota BETWEEN 0 AND 10 AND Nota = ROUND(10.0 * Acertos / TotalQuestoes, 2)),
    CONSTRAINT CK_Tentativas_Aprovacao CHECK ((Aprovado = 1 AND Percentual >= 60) OR (Aprovado = 0 AND Percentual < 60))
);

-- As chaves compostas ligam tentativa, questão e alternativa corretamente.
CREATE TABLE dbo.RespostasAluno (
    Id int IDENTITY PRIMARY KEY,
    TentativaId int NOT NULL,
    AvaliacaoId int NOT NULL,
    QuestaoId int NOT NULL,
    AlternativaId int NOT NULL,
    Correta bit NOT NULL,
    CONSTRAINT UQ_Respostas_Questao UNIQUE (TentativaId, QuestaoId),
    CONSTRAINT FK_Respostas_Tentativa FOREIGN KEY (TentativaId, AvaliacaoId) REFERENCES dbo.TentativasAvaliacao(Id, AvaliacaoId),
    CONSTRAINT FK_Respostas_Questao FOREIGN KEY (QuestaoId, AvaliacaoId) REFERENCES dbo.Questoes(Id, AvaliacaoId),
    CONSTRAINT FK_Respostas_Alternativa FOREIGN KEY (AlternativaId, QuestaoId) REFERENCES dbo.Alternativas(Id, QuestaoId)
);

CREATE TABLE dbo.ProgressoModulos (
    Id int IDENTITY PRIMARY KEY,
    AlunoId int NOT NULL REFERENCES dbo.Usuarios(Id),
    ModuloId int NOT NULL REFERENCES dbo.Modulos(Id),
    DataConclusao datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_Progresso_Modulo UNIQUE (AlunoId, ModuloId)
);

CREATE TABLE dbo.AcompanhamentosMentor (
    Id int IDENTITY PRIMARY KEY,
    MentorId int NOT NULL REFERENCES dbo.Usuarios(Id),
    AlunoId int NOT NULL REFERENCES dbo.Usuarios(Id),
    Ativo bit NOT NULL DEFAULT 1,
    CONSTRAINT UQ_Acompanhamentos UNIQUE (MentorId, AlunoId),
    CONSTRAINT CK_Acompanhamentos_Pessoas CHECK (MentorId <> AlunoId)
);

CREATE TABLE dbo.OrientacoesMentor (
    Id int IDENTITY PRIMARY KEY,
    MentorId int NOT NULL,
    AlunoId int NOT NULL,
    Texto nvarchar(4000) NOT NULL,
    DataOrientacao datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Orientacoes_Vinculo FOREIGN KEY (MentorId, AlunoId) REFERENCES dbo.AcompanhamentosMentor(MentorId, AlunoId),
    CONSTRAINT CK_Orientacoes_Texto CHECK (LEN(LTRIM(RTRIM(Texto))) > 0)
);

CREATE TABLE dbo.AuditoriaUsuarios (
    Id int IDENTITY PRIMARY KEY,
    UsuarioId int NOT NULL REFERENCES dbo.Usuarios(Id),
    PerfilAnterior nvarchar(20) NOT NULL,
    PerfilNovo nvarchar(20) NOT NULL,
    AtivoAnterior bit NOT NULL,
    AtivoNovo bit NOT NULL,
    DataAlteracao datetime2 NOT NULL DEFAULT SYSUTCDATETIME()
);

-- Índices aceleram buscas e impedem duplicações.
CREATE INDEX IX_Usuarios_PerfilAtivo ON dbo.Usuarios(Perfil, Ativo);
CREATE INDEX IX_Usuarios_Nome ON dbo.Usuarios(Nome);
CREATE INDEX IX_Recuperacoes_Usuario ON dbo.RecuperacoesSenha(UsuarioId, Utilizado, Validade);
CREATE INDEX IX_Sessoes_Usuario ON dbo.Sessoes(UsuarioId, Revogada, Validade);
CREATE INDEX IX_Cursos_AtivoNome ON dbo.Cursos(Ativo, Nome);
CREATE INDEX IX_Materiais_Modulo ON dbo.Materiais(ModuloId, Ativo);
CREATE UNIQUE INDEX UQ_Matriculas_Ativas ON dbo.Matriculas(AlunoId, CursoId) WHERE Ativo = 1;
CREATE INDEX IX_Matriculas_Curso ON dbo.Matriculas(CursoId, Ativo);
CREATE INDEX IX_Avaliacoes_Modulo ON dbo.Avaliacoes(ModuloId, Ativo);
CREATE INDEX IX_Questoes_Avaliacao ON dbo.Questoes(AvaliacaoId, Ativo);
CREATE INDEX IX_Alternativas_Questao ON dbo.Alternativas(QuestaoId);
CREATE UNIQUE INDEX UQ_Alternativas_Correta ON dbo.Alternativas(QuestaoId) WHERE Correta = 1;
CREATE INDEX IX_Tentativas_Aluno ON dbo.TentativasAvaliacao(AlunoId, DataTentativa);
CREATE INDEX IX_Tentativas_Avaliacao ON dbo.TentativasAvaliacao(AvaliacaoId);
CREATE INDEX IX_Respostas_Alternativa ON dbo.RespostasAluno(AlternativaId, QuestaoId);
CREATE INDEX IX_Progresso_Modulo ON dbo.ProgressoModulos(ModuloId);
CREATE INDEX IX_Acompanhamentos_Aluno ON dbo.AcompanhamentosMentor(AlunoId, Ativo);
CREATE INDEX IX_Orientacoes_Aluno ON dbo.OrientacoesMentor(AlunoId, DataOrientacao);
CREATE INDEX IX_Auditoria_Usuario ON dbo.AuditoriaUsuarios(UsuarioId, DataAlteracao);
GO

CREATE PROCEDURE dbo.sp_RealizarMatricula
    @AlunoId int,
    @CursoId int
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- O bloqueio serializa matrículas concorrentes do mesmo aluno.
        IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WITH (UPDLOCK, HOLDLOCK)
                       WHERE Id = @AlunoId AND Perfil = N'Aluno' AND Ativo = 1)
            THROW 51001, 'Aluno inexistente ou inativo.', 1;

        IF NOT EXISTS (SELECT 1 FROM dbo.Cursos WITH (UPDLOCK, HOLDLOCK)
                       WHERE Id = @CursoId AND Ativo = 1)
            THROW 51002, 'Curso inexistente ou inativo.', 1;

        IF EXISTS (SELECT 1 FROM dbo.Matriculas WHERE AlunoId = @AlunoId AND CursoId = @CursoId AND Ativo = 1)
            THROW 51003, 'O aluno já está matriculado neste curso.', 1;

        INSERT dbo.Matriculas (AlunoId, CursoId) VALUES (@AlunoId, @CursoId);
        DECLARE @Id int = CONVERT(int, SCOPE_IDENTITY());

        COMMIT TRANSACTION;
        SELECT @Id AS Id;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

CREATE PROCEDURE dbo.sp_RelatorioCurso
    @CursoId int = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Cada subconsulta calcula uma métrica sem multiplicar registros.
    SELECT c.Id AS CursoId, c.Nome,
           (SELECT COUNT(*) FROM dbo.Matriculas mt WHERE mt.CursoId = c.Id AND mt.Ativo = 1) AS Matriculas,
           COALESCE((SELECT AVG(t.Nota) FROM dbo.TentativasAvaliacao t
               JOIN dbo.Avaliacoes a ON a.Id = t.AvaliacaoId JOIN dbo.Modulos m ON m.Id = a.ModuloId
               WHERE m.CursoId = c.Id), 0) AS Media,
           COALESCE((SELECT AVG(CAST(t.Aprovado AS decimal(5,2))) * 100 FROM dbo.TentativasAvaliacao t
               JOIN dbo.Avaliacoes a ON a.Id = t.AvaliacaoId JOIN dbo.Modulos m ON m.Id = a.ModuloId
               WHERE m.CursoId = c.Id), 0) AS Aprovacao,
           COALESCE((SELECT AVG(progresso.Percentual) FROM dbo.Matriculas mt
               CROSS APPLY (SELECT CAST(CASE WHEN COUNT(*) = 0 THEN 0
                   ELSE 100.0 * SUM(CASE WHEN p.Id IS NOT NULL THEN 1 ELSE 0 END) / COUNT(*) END AS decimal(5,2)) AS Percentual
                   FROM dbo.Modulos m LEFT JOIN dbo.ProgressoModulos p ON p.ModuloId = m.Id AND p.AlunoId = mt.AlunoId
                   WHERE m.CursoId = c.Id AND m.Ativo = 1) progresso
               WHERE mt.CursoId = c.Id AND mt.Ativo = 1), 0) AS ProgressoMedio
    FROM dbo.Cursos c
    WHERE @CursoId IS NULL OR c.Id = @CursoId
    ORDER BY c.Nome;
END;
GO

-- Registra somente mudanças de perfil ou de situação.
CREATE TRIGGER dbo.tr_AuditoriaUsuarios
ON dbo.Usuarios
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.AuditoriaUsuarios (UsuarioId, PerfilAnterior, PerfilNovo, AtivoAnterior, AtivoNovo)
    SELECT i.Id, d.Perfil, i.Perfil, d.Ativo, i.Ativo
    FROM inserted i JOIN deleted d ON d.Id = i.Id
    WHERE i.Perfil <> d.Perfil OR i.Ativo <> d.Ativo;
END;
GO

-- Confere a estrutura criada, sem adicionar dados fictícios.
COMMIT TRANSACTION;
SELECT name AS Tabela FROM sys.tables ORDER BY name;

-- NOVAS FUNCOES: certificados, chamados e documentos
-- Atualização das novas funções. Execute no banco atual; pode ser repetida.
USE TechLearnAcademy;
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
