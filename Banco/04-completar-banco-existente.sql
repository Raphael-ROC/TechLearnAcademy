-- Completa o esquema da TechLearn Academy sem apagar a tabela dbo.Usuarios nem seus dados.
-- Execute somente depois de conferir que dbo.Usuarios pertence a esta solução.
-- No SSMS: habilite Modo SQLCMD antes de executar.
:ON ERROR EXIT
USE TechLearnAcademy;
GO

IF OBJECT_ID(N'dbo.Usuarios', N'U') IS NULL
    THROW 51000, 'A tabela dbo.Usuarios não existe. Este script não cria nem substitui usuários.', 1;

IF COL_LENGTH(N'dbo.Usuarios', N'Id') IS NULL
   OR COL_LENGTH(N'dbo.Usuarios', N'Nome') IS NULL
   OR COL_LENGTH(N'dbo.Usuarios', N'Email') IS NULL
   OR COL_LENGTH(N'dbo.Usuarios', N'SenhaHash') IS NULL
   OR COL_LENGTH(N'dbo.Usuarios', N'Perfil') IS NULL
   OR COL_LENGTH(N'dbo.Usuarios', N'Ativo') IS NULL
   OR COL_LENGTH(N'dbo.Usuarios', N'DataCadastro') IS NULL
    THROW 51000, 'A estrutura atual de dbo.Usuarios não corresponde à aplicação. Nenhuma alteração foi feita.', 1;

IF EXISTS (SELECT 1 FROM sys.tables WHERE name IN
    (N'RecuperacoesSenha',N'Sessoes',N'Cursos',N'Modulos',N'Materiais',N'Matriculas',N'Avaliacoes',N'Questoes',N'Alternativas',N'TentativasAvaliacao',N'RespostasAluno',N'ProgressoModulos',N'AcompanhamentosMentor',N'OrientacoesMentor',N'AuditoriaUsuarios'))
   OR OBJECT_ID(N'dbo.sp_RealizarMatricula', N'P') IS NOT NULL
   OR OBJECT_ID(N'dbo.sp_RelatorioCurso', N'P') IS NOT NULL
   OR OBJECT_ID(N'dbo.tr_AuditoriaUsuarios', N'TR') IS NOT NULL
    THROW 51000, 'Já existem partes do esquema da aplicação. O script foi interrompido para evitar conflitos.', 1;

IF EXISTS (SELECT Email FROM dbo.Usuarios GROUP BY Email HAVING COUNT(*) > 1)
    THROW 51000, 'Há e-mails duplicados em dbo.Usuarios; corrija-os antes de criar o índice único.', 1;

BEGIN TRANSACTION;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.Usuarios') AND is_unique=1 AND name=N'UQ_Usuarios_Email')
    CREATE UNIQUE INDEX UQ_Usuarios_Email ON dbo.Usuarios(Email);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.Usuarios') AND name=N'IX_Usuarios_PerfilAtivo')
    CREATE INDEX IX_Usuarios_PerfilAtivo ON dbo.Usuarios(Perfil, Ativo);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.Usuarios') AND name=N'IX_Usuarios_Nome')
    CREATE INDEX IX_Usuarios_Nome ON dbo.Usuarios(Nome);

-- Índices filtrados exigem estas opções em todas as conexões.
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;

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


