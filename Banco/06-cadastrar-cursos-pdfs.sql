:setvar PastaMateriais "C:\MateriaisTechLearn\cursos"
-- Cadastro dos cursos preparados com os PDFs enviados em 11/10/2026.
-- Execute em modo SQLCMD. Ajuste PastaMateriais se mudar a pasta do projeto.
-- Adiciona conteúdos; preserva os cursos, usuários e históricos anteriores.
-- Repetir ignora cursos não excluídos que já tenham o mesmo nome.
USE TechLearnAcademy_PIMIV;
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
DECLARE @CursoId int, @ModuloId int, @AvaliacaoId int, @QuestaoId int, @NovosCursos int=0;
BEGIN TRY
  BEGIN TRANSACTION;

  -- Lógica de Programação e Estruturas de Dados
  IF NOT EXISTS(SELECT 1 FROM dbo.Cursos WITH(UPDLOCK,HOLDLOCK) WHERE Nome=N'Lógica de Programação e Estruturas de Dados' AND Excluido=0)
  BEGIN
    INSERT INTO dbo.Cursos(Nome,Descricao,CargaHoraria,Ativo) VALUES(N'Lógica de Programação e Estruturas de Dados',N'Aprenda tipos de dados, lógica, vetores, matrizes, busca, ordenação, filas e árvores binárias. Estudo guiado com exemplos e exercícios comentados dos tomos enviados.',8,1);
    SET @CursoId=CAST(SCOPE_IDENTITY() AS int);
    SET @NovosCursos=@NovosCursos+1;

    -- Módulo 1: Lógica e fundamentos da programação
    INSERT INTO dbo.Modulos(CursoId,Titulo,Ordem,Ativo) VALUES(@CursoId,N'Lógica e fundamentos da programação',1,1);
    SET @ModuloId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'Tipos de dados e análise de algoritmos',N'PDF',N'',N'01-01-tipos-de-dados-e-analise-de-algoritmos.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\logica-de-programacao-e-estruturas-de-dados\01-01-tipos-de-dados-e-analise-de-algoritmos.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'Proposições e tabela verdade',N'PDF',N'',N'01-02-proposicoes-e-tabela-verdade.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\logica-de-programacao-e-estruturas-de-dados\01-02-proposicoes-e-tabela-verdade.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'Operadores lógicos e operações em C',N'PDF',N'',N'01-03-operadores-logicos-e-operacoes-em-c.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\logica-de-programacao-e-estruturas-de-dados\01-03-operadores-logicos-e-operacoes-em-c.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Avaliacoes(ModuloId,Titulo,Ativo) VALUES(@ModuloId,N'Revisão - Lógica e fundamentos da programação',1);
    SET @AvaliacaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Qual tipo de dado representa os valores verdadeiro e falso?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Inteiro.',0),
      (@QuestaoId,N'Literal.',0),
      (@QuestaoId,N'Real.',0),
      (@QuestaoId,N'Lógico.',1);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Em qual situação a proposição "se P, então Q" é falsa?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'P é falsa e Q é verdadeira.',0),
      (@QuestaoId,N'P e Q são falsas.',0),
      (@QuestaoId,N'P é verdadeira e Q é falsa.',1),
      (@QuestaoId,N'P e Q são verdadeiras.',0);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Na linguagem C, quando a expressão A && B é verdadeira?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Sempre que A e B são diferentes.',0),
      (@QuestaoId,N'Quando A e B são verdadeiras.',1),
      (@QuestaoId,N'Quando apenas A é verdadeira.',0),
      (@QuestaoId,N'Quando A e B são falsas.',0);

    -- Módulo 2: Vetores, matrizes e execução de algoritmos
    INSERT INTO dbo.Modulos(CursoId,Titulo,Ordem,Ativo) VALUES(@CursoId,N'Vetores, matrizes e execução de algoritmos',2,1);
    SET @ModuloId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'Execução de algoritmos com matrizes',N'PDF',N'',N'02-01-execucao-de-algoritmos-com-matrizes.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\logica-de-programacao-e-estruturas-de-dados\02-01-execucao-de-algoritmos-com-matrizes.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'Vetores, matrizes e validação de entradas',N'PDF',N'',N'02-02-vetores-matrizes-e-validacao-de-entradas.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\logica-de-programacao-e-estruturas-de-dados\02-02-vetores-matrizes-e-validacao-de-entradas.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Avaliacoes(ModuloId,Titulo,Ativo) VALUES(@ModuloId,N'Revisão - Vetores, matrizes e execução de algoritmos',1);
    SET @AvaliacaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Um vetor tem cinco posições e começa no índice zero. Qual é seu último índice?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'6.',0),
      (@QuestaoId,N'1.',0),
      (@QuestaoId,N'4.',1),
      (@QuestaoId,N'5.',0);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Quantos elementos cabem em uma matriz com três linhas e quatro colunas?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'4.',0),
      (@QuestaoId,N'12.',1),
      (@QuestaoId,N'7.',0),
      (@QuestaoId,N'3.',0);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Qual é a finalidade de dois laços aninhados ao percorrer uma matriz?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Percorrer as linhas e as colunas.',1),
      (@QuestaoId,N'Transformar a matriz em uma variável lógica.',0),
      (@QuestaoId,N'Eliminar todas as condições do programa.',0),
      (@QuestaoId,N'Garantir que todos os valores sejam iguais.',0);

    -- Módulo 3: Busca e ordenação de dados
    INSERT INTO dbo.Modulos(CursoId,Titulo,Ordem,Ativo) VALUES(@CursoId,N'Busca e ordenação de dados',3,1);
    SET @ModuloId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'Busca binária em vetores',N'PDF',N'',N'03-01-busca-binaria-em-vetores.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\logica-de-programacao-e-estruturas-de-dados\03-01-busca-binaria-em-vetores.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'Ordenação de vetores passo a passo',N'PDF',N'',N'03-02-ordenacao-de-vetores-passo-a-passo.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\logica-de-programacao-e-estruturas-de-dados\03-02-ordenacao-de-vetores-passo-a-passo.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'Ordenação por inserção',N'PDF',N'',N'03-03-ordenacao-por-insercao.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\logica-de-programacao-e-estruturas-de-dados\03-03-ordenacao-por-insercao.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Avaliacoes(ModuloId,Titulo,Ativo) VALUES(@ModuloId,N'Revisão - Busca e ordenação de dados',1);
    SET @AvaliacaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Qual condição permite aplicar corretamente a busca binária apresentada no material?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'O vetor não possuir índices.',0),
      (@QuestaoId,N'O vetor estar ordenado.',1),
      (@QuestaoId,N'O vetor conter somente números pares.',0),
      (@QuestaoId,N'O vetor ter exatamente dez posições.',0);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Como funciona a ideia principal da ordenação por inserção?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Inserir cada elemento na posição adequada da parte já ordenada.',1),
      (@QuestaoId,N'Excluir todos os valores repetidos.',0),
      (@QuestaoId,N'Trocar todos os elementos pelo maior valor.',0),
      (@QuestaoId,N'Pesquisar somente o último elemento.',0);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Ao ordenar um vetor em ordem crescente comparando vizinhos, o que fazer quando V[j] > V[j+1]?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Excluir V[j+1].',0),
      (@QuestaoId,N'Interromper obrigatoriamente toda a ordenação.',0),
      (@QuestaoId,N'Deixar os valores como estão em todas as passagens.',0),
      (@QuestaoId,N'Trocar os dois valores de posição.',1);

    -- Módulo 4: Filas e árvores binárias
    INSERT INTO dbo.Modulos(CursoId,Titulo,Ordem,Ativo) VALUES(@CursoId,N'Filas e árvores binárias',4,1);
    SET @ModuloId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'Filas e operações de inserção e remoção',N'PDF',N'',N'04-01-filas-e-operacoes-de-insercao-e-remocao.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\logica-de-programacao-e-estruturas-de-dados\04-01-filas-e-operacoes-de-insercao-e-remocao.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'Árvores binárias e seus percursos',N'PDF',N'',N'04-02-arvores-binarias-e-seus-percursos.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\logica-de-programacao-e-estruturas-de-dados\04-02-arvores-binarias-e-seus-percursos.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Avaliacoes(ModuloId,Titulo,Ativo) VALUES(@ModuloId,N'Revisão - Filas e árvores binárias',1);
    SET @AvaliacaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'O que caracteriza a ordem FIFO de uma fila?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'O primeiro elemento inserido é o primeiro removido.',1),
      (@QuestaoId,N'O último elemento inserido é sempre o primeiro removido.',0),
      (@QuestaoId,N'A remoção acontece pela ordem alfabética.',0),
      (@QuestaoId,N'Todos os elementos são removidos juntos.',0);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'O que uma operação de remoção deve verificar antes de retirar um elemento da fila?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Se os valores estão ordenados.',0),
      (@QuestaoId,N'Se todos os elementos são números pares.',0),
      (@QuestaoId,N'Se a fila tem uma árvore associada.',0),
      (@QuestaoId,N'Se a fila está vazia.',1);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Qual é a ordem do percurso em ordem de uma árvore binária?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Raiz, subárvore direita e subárvore esquerda.',0),
      (@QuestaoId,N'Somente os nós sem filhos.',0),
      (@QuestaoId,N'Subárvore esquerda, raiz e subárvore direita.',1),
      (@QuestaoId,N'Raiz, subárvore esquerda e subárvore direita.',0);
  END

  -- Programação Orientada a Objetos e UML
  IF NOT EXISTS(SELECT 1 FROM dbo.Cursos WITH(UPDLOCK,HOLDLOCK) WHERE Nome=N'Programação Orientada a Objetos e UML' AND Excluido=0)
  BEGIN
    INSERT INTO dbo.Cursos(Nome,Descricao,CargaHoraria,Ativo) VALUES(N'Programação Orientada a Objetos e UML',N'Entenda classes, objetos, herança, polimorfismo e relacionamentos. Aprenda a interpretar diagramas UML e conheça componentes reutilizáveis, MVC e padrões de projeto.',12,1);
    SET @CursoId=CAST(SCOPE_IDENTITY() AS int);
    SET @NovosCursos=@NovosCursos+1;

    -- Módulo 1: Classes, objetos e princípios da POO
    INSERT INTO dbo.Modulos(CursoId,Titulo,Ordem,Ativo) VALUES(@CursoId,N'Classes, objetos e princípios da POO',1,1);
    SET @ModuloId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'Conceitos de orientação a objetos',N'PDF',N'',N'01-01-conceitos-de-orientacao-a-objetos.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\programacao-orientada-a-objetos-e-uml\01-01-conceitos-de-orientacao-a-objetos.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'Classes, herança e polimorfismo',N'PDF',N'',N'01-02-classes-heranca-e-polimorfismo.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\programacao-orientada-a-objetos-e-uml\01-02-classes-heranca-e-polimorfismo.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Avaliacoes(ModuloId,Titulo,Ativo) VALUES(@ModuloId,N'Revisão - Classes, objetos e princípios da POO',1);
    SET @AvaliacaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Qual afirmação descreve uma classe?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Uma cópia obrigatória de uma tabela do banco.',0),
      (@QuestaoId,N'Um comando utilizado somente para imprimir textos.',0),
      (@QuestaoId,N'Um modelo que define características e comportamentos de objetos.',1),
      (@QuestaoId,N'Um único valor armazenado na memória.',0);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'O que a herança permite em programação orientada a objetos?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Impedir a criação de especializações.',0),
      (@QuestaoId,N'Uma subclasse aproveitar características de uma superclasse.',1),
      (@QuestaoId,N'Eliminar todos os métodos do programa.',0),
      (@QuestaoId,N'Transformar cada objeto em uma linguagem de programação.',0);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Qual é a finalidade do encapsulamento?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Organizar e proteger o estado do objeto por meio de uma interface de acesso.',1),
      (@QuestaoId,N'Tornar todos os atributos públicos obrigatoriamente.',0),
      (@QuestaoId,N'Substituir qualquer método por uma variável global.',0),
      (@QuestaoId,N'Impedir qualquer comunicação entre objetos.',0);

    -- Módulo 2: Relacionamentos e diagramas de classes
    INSERT INTO dbo.Modulos(CursoId,Titulo,Ordem,Ativo) VALUES(@CursoId,N'Relacionamentos e diagramas de classes',2,1);
    SET @ModuloId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'Herança, sobrescrita e relacionamentos entre classes',N'PDF',N'',N'02-01-heranca-sobrescrita-e-relacionamentos-entre-classes.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\programacao-orientada-a-objetos-e-uml\02-01-heranca-sobrescrita-e-relacionamentos-entre-classes.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'Diagrama de classes, atributos e associações',N'PDF',N'',N'02-02-diagrama-de-classes-atributos-e-associacoes.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\programacao-orientada-a-objetos-e-uml\02-02-diagrama-de-classes-atributos-e-associacoes.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'UML aplicada a classes e casos de uso',N'PDF',N'',N'02-03-uml-aplicada-a-classes-e-casos-de-uso.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\programacao-orientada-a-objetos-e-uml\02-03-uml-aplicada-a-classes-e-casos-de-uso.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'Modelagem de classes a partir de requisitos',N'PDF',N'',N'02-04-modelagem-de-classes-a-partir-de-requisitos.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\programacao-orientada-a-objetos-e-uml\02-04-modelagem-de-classes-a-partir-de-requisitos.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Avaliacoes(ModuloId,Titulo,Ativo) VALUES(@ModuloId,N'Revisão - Relacionamentos e diagramas de classes',1);
    SET @AvaliacaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Uma turma tem vários alunos. Qual multiplicidade representa a quantidade de alunos em uma associação que permite zero ou mais alunos?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Somente 1.',0),
      (@QuestaoId,N'0..*.',1),
      (@QuestaoId,N'1..1.',0),
      (@QuestaoId,N'0..0.',0);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Em um diagrama de classes, o que uma associação representa?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Uma relação entre classes do domínio.',1),
      (@QuestaoId,N'O tempo de execução de um método.',0),
      (@QuestaoId,N'O número de linhas do código-fonte.',0),
      (@QuestaoId,N'A ordem de inicialização do computador.',0);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'O que ocorre quando uma subclasse sobrescreve um método?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Ela transforma obrigatoriamente o método em atributo.',0),
      (@QuestaoId,N'Ela apaga a classe ancestral do programa.',0),
      (@QuestaoId,N'Ela impede que qualquer objeto use o método.',0),
      (@QuestaoId,N'Ela fornece uma implementação própria para o método herdado, respeitando sua assinatura.',1);

    -- Módulo 3: Casos de uso e diagramas comportamentais
    INSERT INTO dbo.Modulos(CursoId,Titulo,Ordem,Ativo) VALUES(@CursoId,N'Casos de uso e diagramas comportamentais',3,1);
    SET @ModuloId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'Casos de uso, atores e funcionalidades',N'PDF',N'',N'03-01-casos-de-uso-atores-e-funcionalidades.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\programacao-orientada-a-objetos-e-uml\03-01-casos-de-uso-atores-e-funcionalidades.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'Diagrama de sequência da UML',N'PDF',N'',N'03-02-diagrama-de-sequencia-da-uml.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\programacao-orientada-a-objetos-e-uml\03-02-diagrama-de-sequencia-da-uml.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'Diagramas de comunicação, estados e atividades',N'PDF',N'',N'03-03-diagramas-de-comunicacao-estados-e-atividades.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\programacao-orientada-a-objetos-e-uml\03-03-diagramas-de-comunicacao-estados-e-atividades.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Avaliacoes(ModuloId,Titulo,Ativo) VALUES(@ModuloId,N'Revisão - Casos de uso e diagramas comportamentais',1);
    SET @AvaliacaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'O que um diagrama de sequência destaca?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'A ordem temporal das mensagens entre participantes.',1),
      (@QuestaoId,N'A distribuição física dos cabos de rede.',0),
      (@QuestaoId,N'Os valores de todas as tabelas do banco.',0),
      (@QuestaoId,N'Somente os nomes das classes sem interação.',0);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'O que um ator representa em um diagrama de casos de uso?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Uma coluna de uma tabela relacional.',0),
      (@QuestaoId,N'Um método privado que nunca é chamado.',0),
      (@QuestaoId,N'Uma cor utilizada na interface.',0),
      (@QuestaoId,N'Um papel externo que interage com o sistema.',1);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Qual aspecto é representado por um diagrama de atividades?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'A senha de cada usuário.',0),
      (@QuestaoId,N'A quantidade de processadores instalados.',0),
      (@QuestaoId,N'O fluxo de ações e decisões de um processo.',1),
      (@QuestaoId,N'Somente os endereços de memória.',0);

    -- Módulo 4: Reutilização, MVC e padrões de projeto
    INSERT INTO dbo.Modulos(CursoId,Titulo,Ordem,Ativo) VALUES(@CursoId,N'Reutilização, MVC e padrões de projeto',4,1);
    SET @ModuloId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'Componentes reutilizáveis e JavaBeans',N'PDF',N'',N'04-01-componentes-reutilizaveis-e-javabeans.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\programacao-orientada-a-objetos-e-uml\04-01-componentes-reutilizaveis-e-javabeans.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'Padrões de projeto e Abstract Factory',N'PDF',N'',N'04-02-padroes-de-projeto-e-abstract-factory.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\programacao-orientada-a-objetos-e-uml\04-02-padroes-de-projeto-e-abstract-factory.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'Orientação a objetos, MVC e padrões de projeto',N'PDF',N'',N'04-03-orientacao-a-objetos-mvc-e-padroes-de-projeto.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\programacao-orientada-a-objetos-e-uml\04-03-orientacao-a-objetos-mvc-e-padroes-de-projeto.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Avaliacoes(ModuloId,Titulo,Ativo) VALUES(@ModuloId,N'Revisão - Reutilização, MVC e padrões de projeto',1);
    SET @AvaliacaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Qual é a intenção principal do padrão Abstract Factory?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Ordenar qualquer vetor em uma única comparação.',0),
      (@QuestaoId,N'Eliminar a necessidade de classes.',0),
      (@QuestaoId,N'Substituir o banco por arquivos de imagem.',0),
      (@QuestaoId,N'Criar famílias de objetos relacionados sem expor suas classes concretas ao cliente.',1);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'No padrão MVC, qual é a responsabilidade da View?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Substituir todos os objetos do domínio.',0),
      (@QuestaoId,N'Controlar exclusivamente os cabos da rede.',0),
      (@QuestaoId,N'Apresentar as informações ao usuário.',1),
      (@QuestaoId,N'Definir a frequência do processador.',0);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Qual benefício é associado aos componentes JavaBeans no material?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Uso exclusivo em programas sem objetos.',0),
      (@QuestaoId,N'Reutilização de componentes por diferentes desenvolvedores.',1),
      (@QuestaoId,N'Obrigatoriedade de reescrever cada componente do início.',0),
      (@QuestaoId,N'Eliminação de qualquer ambiente de execução.',0);
  END

  -- Engenharia de Software e Requisitos
  IF NOT EXISTS(SELECT 1 FROM dbo.Cursos WITH(UPDLOCK,HOLDLOCK) WHERE Nome=N'Engenharia de Software e Requisitos' AND Excluido=0)
  BEGIN
    INSERT INTO dbo.Cursos(Nome,Descricao,CargaHoraria,Ativo) VALUES(N'Engenharia de Software e Requisitos',N'Estude elicitação e especificação de requisitos, protótipos, modelos de desenvolvimento, RUP, programação extrema e controle de versões. Os módulos seguem do entendimento do problema à organização do desenvolvimento.',8,1);
    SET @CursoId=CAST(SCOPE_IDENTITY() AS int);
    SET @NovosCursos=@NovosCursos+1;

    -- Módulo 1: Identificação e especificação de requisitos
    INSERT INTO dbo.Modulos(CursoId,Titulo,Ordem,Ativo) VALUES(@CursoId,N'Identificação e especificação de requisitos',1,1);
    SET @ModuloId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'Técnicas de elicitação de requisitos',N'PDF',N'',N'01-01-tecnicas-de-elicitacao-de-requisitos.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\engenharia-de-software-e-requisitos\01-01-tecnicas-de-elicitacao-de-requisitos.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'Requisitos funcionais e não funcionais',N'PDF',N'',N'01-02-requisitos-funcionais-e-nao-funcionais.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\engenharia-de-software-e-requisitos\01-02-requisitos-funcionais-e-nao-funcionais.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'Especificação e análise de requisitos',N'PDF',N'',N'01-03-especificacao-e-analise-de-requisitos.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\engenharia-de-software-e-requisitos\01-03-especificacao-e-analise-de-requisitos.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Avaliacoes(ModuloId,Titulo,Ativo) VALUES(@ModuloId,N'Revisão - Identificação e especificação de requisitos',1);
    SET @AvaliacaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Qual exemplo representa um requisito funcional de uma plataforma de cursos?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Ser compatível com diferentes resoluções de tela.',0),
      (@QuestaoId,N'Permitir que o aluno se matricule em um curso.',1),
      (@QuestaoId,N'Responder a uma solicitação em até dois segundos.',0),
      (@QuestaoId,N'Ter disponibilidade de 99% no período definido.',0);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Qual é o objetivo da elicitação de requisitos?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Descobrir necessidades e expectativas das pessoas envolvidas.',1),
      (@QuestaoId,N'Escolher apenas a cor dos botões.',0),
      (@QuestaoId,N'Excluir a participação dos usuários.',0),
      (@QuestaoId,N'Publicar o sistema antes de entender o problema.',0);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Como deve ser uma especificação de requisitos útil à equipe e aos interessados?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Formada apenas por nomes de arquivos.',0),
      (@QuestaoId,N'Propositalmente ambígua para permitir qualquer interpretação.',0),
      (@QuestaoId,N'Restrita a informações que não foram discutidas com ninguém.',0),
      (@QuestaoId,N'Clara e compreensível para as pessoas que precisam utilizá-la.',1);

    -- Módulo 2: Ciclos de vida, protótipos e desenvolvimento incremental
    INSERT INTO dbo.Modulos(CursoId,Titulo,Ordem,Ativo) VALUES(@CursoId,N'Ciclos de vida, protótipos e desenvolvimento incremental',2,1);
    SET @ModuloId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'Restrições de projeto e prototipação',N'PDF',N'',N'02-01-restricoes-de-projeto-e-prototipacao.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\engenharia-de-software-e-requisitos\02-01-restricoes-de-projeto-e-prototipacao.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'Modelos de desenvolvimento de software',N'PDF',N'',N'02-02-modelos-de-desenvolvimento-de-software.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\engenharia-de-software-e-requisitos\02-02-modelos-de-desenvolvimento-de-software.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Avaliacoes(ModuloId,Titulo,Ativo) VALUES(@ModuloId,N'Revisão - Ciclos de vida, protótipos e desenvolvimento incremental',1);
    SET @AvaliacaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Qual característica está associada ao modelo em cascata tradicional?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Organização do desenvolvimento em etapas sequenciais.',1),
      (@QuestaoId,N'Ausência de qualquer etapa de teste.',0),
      (@QuestaoId,N'Entrega obrigatória de um produto novo a cada hora.',0),
      (@QuestaoId,N'Proibição de documentação de requisitos.',0);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Qual é uma característica do desenvolvimento incremental?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Construir somente documentos e nunca software.',0),
      (@QuestaoId,N'Recomeçar o sistema inteiro após cada entrega.',0),
      (@QuestaoId,N'Eliminar a avaliação das versões produzidas.',0),
      (@QuestaoId,N'Entregar funcionalidades em partes que ampliam o sistema progressivamente.',1);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Para que um protótipo pode ser utilizado durante o desenvolvimento?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Substituir obrigatoriamente todos os testes.',0),
      (@QuestaoId,N'Definir o resultado de qualquer projeto sem participação do cliente.',0),
      (@QuestaoId,N'Esclarecer e validar necessidades antes da implementação completa.',1),
      (@QuestaoId,N'Garantir que nenhum requisito mude.',0);

    -- Módulo 3: Fases e iterações do RUP
    INSERT INTO dbo.Modulos(CursoId,Titulo,Ordem,Ativo) VALUES(@CursoId,N'Fases e iterações do RUP',3,1);
    SET @ModuloId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'Iterações e atividades do RUP',N'PDF',N'',N'03-01-iteracoes-e-atividades-do-rup.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\engenharia-de-software-e-requisitos\03-01-iteracoes-e-atividades-do-rup.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'Fases e requisitos no RUP',N'PDF',N'',N'03-02-fases-e-requisitos-no-rup.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\engenharia-de-software-e-requisitos\03-02-fases-e-requisitos-no-rup.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Avaliacoes(ModuloId,Titulo,Ativo) VALUES(@ModuloId,N'Revisão - Fases e iterações do RUP',1);
    SET @AvaliacaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Qual sequência apresenta as fases do RUP na ordem correta?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Construção, iniciação, transição e elaboração.',0),
      (@QuestaoId,N'Teste, impressão, compra e instalação.',0),
      (@QuestaoId,N'Transição, construção, iniciação e elaboração.',0),
      (@QuestaoId,N'Iniciação, elaboração, construção e transição.',1);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'O que uma iteração busca produzir em um processo iterativo?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'A suspensão de todas as atividades de desenvolvimento.',0),
      (@QuestaoId,N'Um sistema sem requisitos definidos.',0),
      (@QuestaoId,N'Uma versão avaliada que faz o desenvolvimento avançar.',1),
      (@QuestaoId,N'Somente a exclusão do trabalho anterior.',0);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Como os requisitos são tratados ao longo do RUP?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Não participam do planejamento das iterações.',0),
      (@QuestaoId,N'Podem ser detalhados e revistos durante o ciclo de desenvolvimento.',1),
      (@QuestaoId,N'São proibidos após a primeira reunião.',0),
      (@QuestaoId,N'São considerados somente depois da entrega final.',0);

    -- Módulo 4: Programação extrema, configuração e versões
    INSERT INTO dbo.Modulos(CursoId,Titulo,Ordem,Ativo) VALUES(@CursoId,N'Programação extrema, configuração e versões',4,1);
    SET @ModuloId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'Programação extrema e programação em pares',N'PDF',N'',N'04-01-programacao-extrema-e-programacao-em-pares.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\engenharia-de-software-e-requisitos\04-01-programacao-extrema-e-programacao-em-pares.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'Gerenciamento de configuração e de versões',N'PDF',N'',N'04-02-gerenciamento-de-configuracao-e-de-versoes.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\engenharia-de-software-e-requisitos\04-02-gerenciamento-de-configuracao-e-de-versoes.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Avaliacoes(ModuloId,Titulo,Ativo) VALUES(@ModuloId,N'Revisão - Programação extrema, configuração e versões',1);
    SET @AvaliacaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Como funciona a programação em pares descrita no material de XP?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Uma pessoa escreve o código e a outra não acompanha o trabalho.',0),
      (@QuestaoId,N'O código é produzido sem comunicação entre os participantes.',0),
      (@QuestaoId,N'Duas pessoas colaboram na escrita e na revisão do código.',1),
      (@QuestaoId,N'Cada pessoa programa sem compartilhar suas decisões.',0);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Qual é uma finalidade do controle de versões?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Guardar somente a última imagem da tela.',0),
      (@QuestaoId,N'Registrar alterações e permitir consultar versões anteriores dos artefatos.',1),
      (@QuestaoId,N'Eliminar a necessidade de entender o código.',0),
      (@QuestaoId,N'Impedir todo trabalho em equipe.',0);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'O gerenciamento de configuração pode abranger quais itens de um projeto?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Código-fonte, documentos e outros artefatos controlados.',1),
      (@QuestaoId,N'Somente o papel de parede do computador.',0),
      (@QuestaoId,N'Exclusivamente os nomes dos funcionários.',0),
      (@QuestaoId,N'Apenas arquivos que nunca serão modificados.',0);
  END

  -- Qualidade, Interfaces e Manutenção de Software
  IF NOT EXISTS(SELECT 1 FROM dbo.Cursos WITH(UPDLOCK,HOLDLOCK) WHERE Nome=N'Qualidade, Interfaces e Manutenção de Software' AND Excluido=0)
  BEGIN
    INSERT INTO dbo.Cursos(Nome,Descricao,CargaHoraria,Ativo) VALUES(N'Qualidade, Interfaces e Manutenção de Software',N'Aprenda atributos de qualidade, usabilidade, navegação, acessibilidade, testes e manutenção. Use os exemplos comentados para relacionar qualidade técnica às necessidades do usuário.',8,1);
    SET @CursoId=CAST(SCOPE_IDENTITY() AS int);
    SET @NovosCursos=@NovosCursos+1;

    -- Módulo 1: Qualidade e usabilidade
    INSERT INTO dbo.Modulos(CursoId,Titulo,Ordem,Ativo) VALUES(@CursoId,N'Qualidade e usabilidade',1,1);
    SET @ModuloId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'Confiabilidade, portabilidade e usabilidade',N'PDF',N'',N'01-01-confiabilidade-portabilidade-e-usabilidade.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\qualidade-interfaces-e-manutencao-de-software\01-01-confiabilidade-portabilidade-e-usabilidade.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'Usabilidade e interfaces centradas no usuário',N'PDF',N'',N'01-02-usabilidade-e-interfaces-centradas-no-usuario.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\qualidade-interfaces-e-manutencao-de-software\01-02-usabilidade-e-interfaces-centradas-no-usuario.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Avaliacoes(ModuloId,Titulo,Ativo) VALUES(@ModuloId,N'Revisão - Qualidade e usabilidade',1);
    SET @AvaliacaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Qual característica está relacionada à facilidade de aprender e utilizar um sistema?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Usabilidade.',1),
      (@QuestaoId,N'Frequência do processador.',0),
      (@QuestaoId,N'Capacidade de impressão.',0),
      (@QuestaoId,N'Tamanho físico do gabinete.',0);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Qual ideia está relacionada à confiabilidade do software?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Usar obrigatoriamente muitas cores.',0),
      (@QuestaoId,N'Ter o maior número possível de botões.',0),
      (@QuestaoId,N'Exigir que todo usuário conheça o código-fonte.',0),
      (@QuestaoId,N'Manter seu funcionamento esperado nas condições definidas.',1);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Qual atributo se relaciona à adaptação do software para diferentes ambientes de execução?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Quantidade de anúncios.',0),
      (@QuestaoId,N'Velocidade de digitação do usuário.',0),
      (@QuestaoId,N'Portabilidade.',1),
      (@QuestaoId,N'Ordem alfabética dos métodos.',0);

    -- Módulo 2: Navegação, interfaces e acessibilidade
    INSERT INTO dbo.Modulos(CursoId,Titulo,Ordem,Ativo) VALUES(@CursoId,N'Navegação, interfaces e acessibilidade',2,1);
    SET @ModuloId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'Mapa de navegação e projeto de interfaces',N'PDF',N'',N'02-01-mapa-de-navegacao-e-projeto-de-interfaces.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\qualidade-interfaces-e-manutencao-de-software\02-01-mapa-de-navegacao-e-projeto-de-interfaces.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'Acessibilidade e texto alternativo na web',N'PDF',N'',N'02-02-acessibilidade-e-texto-alternativo-na-web.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\qualidade-interfaces-e-manutencao-de-software\02-02-acessibilidade-e-texto-alternativo-na-web.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Avaliacoes(ModuloId,Titulo,Ativo) VALUES(@ModuloId,N'Revisão - Navegação, interfaces e acessibilidade',1);
    SET @AvaliacaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'O que um mapa de navegação ajuda a representar?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'A posição dos cabos dentro do computador.',0),
      (@QuestaoId,N'Somente a quantidade de arquivos PDF.',0),
      (@QuestaoId,N'O número de usuários sem seus fluxos de interação.',0),
      (@QuestaoId,N'Os caminhos e relações entre telas e funcionalidades.',1);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Qual é a utilidade de um texto alternativo adequado no atributo alt de uma imagem informativa?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Garantir que todos os usuários tenham o mesmo monitor.',0),
      (@QuestaoId,N'Ocultar obrigatoriamente a figura para todos os visitantes.',0),
      (@QuestaoId,N'Permitir que seu significado seja compreendido também por leitores de tela.',1),
      (@QuestaoId,N'Transformar a imagem em uma senha.',0);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Qual é o objetivo da acessibilidade em uma interface?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Eliminar descrições e recursos de apoio.',0),
      (@QuestaoId,N'Permitir a interação de pessoas com diferentes capacidades e necessidades.',1),
      (@QuestaoId,N'Restringir o sistema a um único tipo de usuário.',0),
      (@QuestaoId,N'Exigir visão perfeita de todos os visitantes.',0);

    -- Módulo 3: Verificação, validação e testes
    INSERT INTO dbo.Modulos(CursoId,Titulo,Ordem,Ativo) VALUES(@CursoId,N'Verificação, validação e testes',3,1);
    SET @ModuloId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'Testes de caixa branca e caixa preta',N'PDF',N'',N'03-01-testes-de-caixa-branca-e-caixa-preta.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\qualidade-interfaces-e-manutencao-de-software\03-01-testes-de-caixa-branca-e-caixa-preta.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'Verificação, validação e testes de software',N'PDF',N'',N'03-02-verificacao-validacao-e-testes-de-software.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\qualidade-interfaces-e-manutencao-de-software\03-02-verificacao-validacao-e-testes-de-software.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Avaliacoes(ModuloId,Titulo,Ativo) VALUES(@ModuloId,N'Revisão - Verificação, validação e testes',1);
    SET @AvaliacaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Qual é o foco de um teste de caixa branca?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'A marca do teclado utilizado.',0),
      (@QuestaoId,N'A opinião do usuário sem observar o software.',0),
      (@QuestaoId,N'A estrutura interna e os caminhos do código.',1),
      (@QuestaoId,N'Somente a cor da tela inicial.',0);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Qual é a ideia de um teste de caixa preta?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Ignorar a especificação das funcionalidades.',0),
      (@QuestaoId,N'Avaliar entradas e saídas esperadas sem depender da estrutura interna do código.',1),
      (@QuestaoId,N'Testar somente arquivos de imagem.',0),
      (@QuestaoId,N'Ler todas as linhas do código antes de qualquer avaliação.',0);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Qual pergunta está mais relacionada à validação de um software?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'O produto atende às necessidades de quem vai utilizá-lo?',1),
      (@QuestaoId,N'Quantos cabos foram conectados ao monitor?',0),
      (@QuestaoId,N'Qual é a cor da placa-mãe?',0),
      (@QuestaoId,N'Todos os arquivos possuem o mesmo tamanho?',0);

    -- Módulo 4: Manutenção e evolução de sistemas
    INSERT INTO dbo.Modulos(CursoId,Titulo,Ordem,Ativo) VALUES(@CursoId,N'Manutenção e evolução de sistemas',4,1);
    SET @ModuloId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'Tipos de manutenção de software',N'PDF',N'',N'04-01-tipos-de-manutencao-de-software.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\qualidade-interfaces-e-manutencao-de-software\04-01-tipos-de-manutencao-de-software.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'Planejamento e atividades de manutenção',N'PDF',N'',N'04-02-planejamento-e-atividades-de-manutencao.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\qualidade-interfaces-e-manutencao-de-software\04-02-planejamento-e-atividades-de-manutencao.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'Sistemas legados e evolução de software',N'PDF',N'',N'04-03-sistemas-legados-e-evolucao-de-software.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\qualidade-interfaces-e-manutencao-de-software\04-03-sistemas-legados-e-evolucao-de-software.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Avaliacoes(ModuloId,Titulo,Ativo) VALUES(@ModuloId,N'Revisão - Manutenção e evolução de sistemas',1);
    SET @AvaliacaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Qual tipo de manutenção busca corrigir uma falha já identificada no sistema?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Preventiva.',0),
      (@QuestaoId,N'Corretiva.',1),
      (@QuestaoId,N'Adaptativa.',0),
      (@QuestaoId,N'Perfectiva.',0);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Uma mudança no ambiente exige ajustar o sistema para que ele continue funcionando. Qual manutenção se aplica?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Adaptativa.',1),
      (@QuestaoId,N'Corretiva em todos os casos.',0),
      (@QuestaoId,N'Troca de todos os usuários.',0),
      (@QuestaoId,N'Exclusão do histórico do projeto.',0);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Qual é o objetivo da manutenção preventiva?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Esperar que todo problema provoque uma interrupção.',0),
      (@QuestaoId,N'Impedir a documentação das alterações.',0),
      (@QuestaoId,N'Eliminar os testes após qualquer mudança.',0),
      (@QuestaoId,N'Reduzir a possibilidade de falhas futuras e melhorar a manutenção do sistema.',1);
  END

  -- Fundamentos de Banco de Dados
  IF NOT EXISTS(SELECT 1 FROM dbo.Cursos WITH(UPDLOCK,HOLDLOCK) WHERE Nome=N'Fundamentos de Banco de Dados' AND Excluido=0)
  BEGIN
    INSERT INTO dbo.Cursos(Nome,Descricao,CargaHoraria,Ativo) VALUES(N'Fundamentos de Banco de Dados',N'Introdução à modelagem de dados, entidades, relacionamentos, chaves, consultas SQL e transações. Os estudos de caso ajudam a compreender a organização e a integridade das informações.',4,1);
    SET @CursoId=CAST(SCOPE_IDENTITY() AS int);
    SET @NovosCursos=@NovosCursos+1;

    -- Módulo 1: Modelagem de dados e consultas SQL
    INSERT INTO dbo.Modulos(CursoId,Titulo,Ordem,Ativo) VALUES(@CursoId,N'Modelagem de dados e consultas SQL',1,1);
    SET @ModuloId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'Modelagem de dados, relacionamentos e SQL',N'PDF',N'',N'01-01-modelagem-de-dados-relacionamentos-e-sql.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\fundamentos-de-banco-de-dados\01-01-modelagem-de-dados-relacionamentos-e-sql.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Avaliacoes(ModuloId,Titulo,Ativo) VALUES(@ModuloId,N'Revisão - Modelagem de dados e consultas SQL',1);
    SET @AvaliacaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'O que uma entidade representa em um modelo de dados?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Obrigatoriamente uma tela do navegador.',0),
      (@QuestaoId,N'Somente um comando de impressão.',0),
      (@QuestaoId,N'Uma cor escolhida para o sistema.',0),
      (@QuestaoId,N'Um conceito ou objeto relevante do domínio cujos dados precisam ser representados.',1);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Qual é a finalidade de uma chave primária em uma tabela?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Guardar apenas textos de apresentação.',0),
      (@QuestaoId,N'Substituir todas as outras colunas por imagens.',0),
      (@QuestaoId,N'Identificar de forma única cada registro.',1),
      (@QuestaoId,N'Permitir que todos os registros tenham a mesma identificação.',0);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Na abordagem estudada, qual modelo faz a ligação entre o modelo conceitual e a implementação física?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'O desenho do logotipo.',0),
      (@QuestaoId,N'O modelo lógico.',1),
      (@QuestaoId,N'O mapa de navegação.',0),
      (@QuestaoId,N'O plano de contingência.',0);

    -- Módulo 2: Transações, atomicidade e integridade
    INSERT INTO dbo.Modulos(CursoId,Titulo,Ordem,Ativo) VALUES(@CursoId,N'Transações, atomicidade e integridade',2,1);
    SET @ModuloId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'Transações e atomicidade no banco de dados',N'PDF',N'',N'02-01-transacoes-e-atomicidade-no-banco-de-dados.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\fundamentos-de-banco-de-dados\02-01-transacoes-e-atomicidade-no-banco-de-dados.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Avaliacoes(ModuloId,Titulo,Ativo) VALUES(@ModuloId,N'Revisão - Transações, atomicidade e integridade',1);
    SET @AvaliacaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Qual é a ideia principal da atomicidade de uma transação?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Cada usuário pode ignorar as regras de integridade.',0),
      (@QuestaoId,N'Uma transação serve somente para consultar imagens.',0),
      (@QuestaoId,N'As operações da transação são realizadas por completo ou desfeitas.',1),
      (@QuestaoId,N'Toda operação incompleta deve ser mantida definitivamente.',0);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'O que caracteriza uma transação de banco de dados?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Uma lista de arquivos sem relação entre si.',0),
      (@QuestaoId,N'Uma unidade lógica de processamento que pode reunir várias operações.',1),
      (@QuestaoId,N'Uma mudança de cor na interface.',0),
      (@QuestaoId,N'Uma única letra digitada em qualquer campo.',0);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Uma transferência exige debitar uma conta e creditar outra. Se o crédito falhar, qual comportamento preserva a atomicidade?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Desfazer o débito para não manter a operação incompleta.',1),
      (@QuestaoId,N'Manter apenas o débito como operação concluída.',0),
      (@QuestaoId,N'Apagar as duas contas do banco.',0),
      (@QuestaoId,N'Ignorar a falha e informar sucesso.',0);
  END

  -- Fundamentos de Infraestrutura de TI
  IF NOT EXISTS(SELECT 1 FROM dbo.Cursos WITH(UPDLOCK,HOLDLOCK) WHERE Nome=N'Fundamentos de Infraestrutura de TI' AND Excluido=0)
  BEGIN
    INSERT INTO dbo.Cursos(Nome,Descricao,CargaHoraria,Ativo) VALUES(N'Fundamentos de Infraestrutura de TI',N'Conheça barramentos, sistemas operacionais, multiprogramação, máquinas virtuais e redes locais. Os exemplos explicam como os recursos de hardware e software trabalham em conjunto.',6,1);
    SET @CursoId=CAST(SCOPE_IDENTITY() AS int);
    SET @NovosCursos=@NovosCursos+1;

    -- Módulo 1: Arquitetura de computadores e barramentos
    INSERT INTO dbo.Modulos(CursoId,Titulo,Ordem,Ativo) VALUES(@CursoId,N'Arquitetura de computadores e barramentos',1,1);
    SET @ModuloId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'Arquitetura de computadores e barramentos',N'PDF',N'',N'01-01-arquitetura-de-computadores-e-barramentos.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\fundamentos-de-infraestrutura-de-ti\01-01-arquitetura-de-computadores-e-barramentos.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Avaliacoes(ModuloId,Titulo,Ativo) VALUES(@ModuloId,N'Revisão - Arquitetura de computadores e barramentos',1);
    SET @AvaliacaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Qual é a finalidade do barramento de endereços?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Substituir o processador.',0),
      (@QuestaoId,N'Armazenar permanentemente todos os programas.',0),
      (@QuestaoId,N'Indicar a posição de memória ou o dispositivo que será acessado.',1),
      (@QuestaoId,N'Transportar exclusivamente imagens entre telas.',0);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Qual é a função principal do barramento de dados?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Eliminar a memória do computador.',0),
      (@QuestaoId,N'Transferir dados entre componentes.',1),
      (@QuestaoId,N'Escolher o nome do usuário.',0),
      (@QuestaoId,N'Controlar somente o brilho do monitor.',0);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Quantas combinações diferentes podem ser representadas por três bits?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'8.',1),
      (@QuestaoId,N'3.',0),
      (@QuestaoId,N'6.',0),
      (@QuestaoId,N'9.',0);

    -- Módulo 2: Sistemas operacionais e multiprogramação
    INSERT INTO dbo.Modulos(CursoId,Titulo,Ordem,Ativo) VALUES(@CursoId,N'Sistemas operacionais e multiprogramação',2,1);
    SET @ModuloId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'Sistemas operacionais e multiprogramação',N'PDF',N'',N'02-01-sistemas-operacionais-e-multiprogramacao.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\fundamentos-de-infraestrutura-de-ti\02-01-sistemas-operacionais-e-multiprogramacao.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Avaliacoes(ModuloId,Titulo,Ativo) VALUES(@ModuloId,N'Revisão - Sistemas operacionais e multiprogramação',1);
    SET @AvaliacaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Como a multiprogramação permite o avanço de vários processos em uma CPU?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Impedindo que os processos aguardem dados.',0),
      (@QuestaoId,N'Alternando a execução entre processos.',1),
      (@QuestaoId,N'Executando todos os comandos exatamente no mesmo instante em uma CPU de um único núcleo.',0),
      (@QuestaoId,N'Eliminando o sistema operacional.',0);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Em qual situação um processo pode ficar bloqueado?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Enquanto aguarda uma entrada ou operação de entrada e saída.',1),
      (@QuestaoId,N'Somente quando seu arquivo tem um nome longo.',0),
      (@QuestaoId,N'Sempre que é o primeiro programa instalado.',0),
      (@QuestaoId,N'Quando não possui imagens em sua interface.',0);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Qual benefício existe em executar outro processo enquanto um aguarda dados?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Desligar obrigatoriamente a memória.',0),
      (@QuestaoId,N'Impedir o retorno do processo que está aguardando.',0),
      (@QuestaoId,N'Aumentar a espera sem realizar trabalho.',0),
      (@QuestaoId,N'Aproveitar a CPU que ficaria ociosa durante a espera.',1);

    -- Módulo 3: Máquinas virtuais e virtualização
    INSERT INTO dbo.Modulos(CursoId,Titulo,Ordem,Ativo) VALUES(@CursoId,N'Máquinas virtuais e virtualização',3,1);
    SET @ModuloId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'Máquinas virtuais e virtualização',N'PDF',N'',N'03-01-maquinas-virtuais-e-virtualizacao.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\fundamentos-de-infraestrutura-de-ti\03-01-maquinas-virtuais-e-virtualizacao.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Avaliacoes(ModuloId,Titulo,Ativo) VALUES(@ModuloId,N'Revisão - Máquinas virtuais e virtualização',1);
    SET @AvaliacaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'O que uma máquina virtual oferece?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Um ambiente computacional em software capaz de executar um sistema convidado.',1),
      (@QuestaoId,N'Um computador físico obrigatório para cada arquivo.',0),
      (@QuestaoId,N'Um programa usado apenas para editar imagens.',0),
      (@QuestaoId,N'Um dispositivo que dispensa qualquer hardware real.',0);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'De onde vêm os recursos utilizados por máquinas virtuais em um computador?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'De uma memória que não possui limites físicos.',0),
      (@QuestaoId,N'Somente do teclado conectado.',0),
      (@QuestaoId,N'Exclusivamente do nome do sistema operacional.',0),
      (@QuestaoId,N'Dos recursos disponibilizados pelo computador hospedeiro.',1);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Qual cuidado continua necessário ao utilizar máquinas virtuais?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Compartilhar todas as senhas publicamente.',0),
      (@QuestaoId,N'Ignorar a capacidade de memória e processamento.',0),
      (@QuestaoId,N'Proteger e atualizar os sistemas, pois a virtualização não elimina ameaças.',1),
      (@QuestaoId,N'Dispensar qualquer medida de segurança.',0);

    -- Módulo 4: Redes locais, hubs e switches
    INSERT INTO dbo.Modulos(CursoId,Titulo,Ordem,Ativo) VALUES(@CursoId,N'Redes locais, hubs e switches',4,1);
    SET @ModuloId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'Redes locais, hubs e switches',N'PDF',N'',N'04-01-redes-locais-hubs-e-switches.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\fundamentos-de-infraestrutura-de-ti\04-01-redes-locais-hubs-e-switches.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Avaliacoes(ModuloId,Titulo,Ativo) VALUES(@ModuloId,N'Revisão - Redes locais, hubs e switches',1);
    SET @AvaliacaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Como um hub normalmente trata os dados que recebe em uma porta?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Encaminha o sinal somente ao destino com base em uma tabela de endereços MAC.',0),
      (@QuestaoId,N'Armazena o sinal como um arquivo PDF.',0),
      (@QuestaoId,N'Transforma todo dado recebido em um programa.',0),
      (@QuestaoId,N'Repete o sinal para as outras portas.',1);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Qual informação um switch usa para decidir o encaminhamento de quadros em uma rede Ethernet?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Cores dos cabos.',0),
      (@QuestaoId,N'Nomes dos documentos impressos.',0),
      (@QuestaoId,N'Endereços MAC.',1),
      (@QuestaoId,N'Senhas de e-mail.',0);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Qual característica diferencia o uso de um switch em relação a um hub compartilhado?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'A rede deixa de utilizar quadros Ethernet.',0),
      (@QuestaoId,N'As portas podem formar domínios de colisão separados.',1),
      (@QuestaoId,N'Todos os computadores obrigatoriamente passam a ter o mesmo endereço MAC.',0),
      (@QuestaoId,N'Nenhum dispositivo consegue transmitir dados.',0);
  END

  -- Gestão de Processos, Riscos e Cidadania Digital
  IF NOT EXISTS(SELECT 1 FROM dbo.Cursos WITH(UPDLOCK,HOLDLOCK) WHERE Nome=N'Gestão de Processos, Riscos e Cidadania Digital' AND Excluido=0)
  BEGIN
    INSERT INTO dbo.Cursos(Nome,Descricao,CargaHoraria,Ativo) VALUES(N'Gestão de Processos, Riscos e Cidadania Digital',N'Entenda processos de negócio, melhoria contínua, planejamento, riscos, contingência e princípios do uso da internet. O conteúdo relaciona a tecnologia às atividades e responsabilidades das organizações.',6,1);
    SET @CursoId=CAST(SCOPE_IDENTITY() AS int);
    SET @NovosCursos=@NovosCursos+1;

    -- Módulo 1: Processos de negócio e melhoria contínua
    INSERT INTO dbo.Modulos(CursoId,Titulo,Ordem,Ativo) VALUES(@CursoId,N'Processos de negócio e melhoria contínua',1,1);
    SET @ModuloId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'Modelagem de processos com EPC e ARIS',N'PDF',N'',N'01-01-modelagem-de-processos-com-epc-e-aris.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\gestao-de-processos-riscos-e-cidadania-digital\01-01-modelagem-de-processos-com-epc-e-aris.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'Processos de negócio e melhoria contínua',N'PDF',N'',N'01-02-processos-de-negocio-e-melhoria-continua.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\gestao-de-processos-riscos-e-cidadania-digital\01-02-processos-de-negocio-e-melhoria-continua.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Avaliacoes(ModuloId,Titulo,Ativo) VALUES(@ModuloId,N'Revisão - Processos de negócio e melhoria contínua',1);
    SET @AvaliacaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Qual descrição corresponde a um processo de negócio?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Uma sequência de cores escolhida para uma tela.',0),
      (@QuestaoId,N'Um conjunto de atividades orientadas a um objetivo organizacional.',1),
      (@QuestaoId,N'Uma lista de arquivos sem propósito definido.',0),
      (@QuestaoId,N'Somente uma peça física do computador.',0);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Na modelagem EPC estudada, o que as funções representam?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Atividades executadas no processo.',1),
      (@QuestaoId,N'Somente o início e o fim de uma atividade.',0),
      (@QuestaoId,N'A marca do sistema operacional.',0),
      (@QuestaoId,N'A capacidade de armazenamento do servidor.',0);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Qual é o objetivo da melhoria contínua dos processos?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Evitar qualquer análise de resultados.',0),
      (@QuestaoId,N'Manter todos os problemas sem registro.',0),
      (@QuestaoId,N'Proibir a participação das pessoas envolvidas.',0),
      (@QuestaoId,N'Revisar e aperfeiçoar a forma de realizar o trabalho.',1);

    -- Módulo 2: Planejamento, riscos e continuidade
    INSERT INTO dbo.Modulos(CursoId,Titulo,Ordem,Ativo) VALUES(@CursoId,N'Planejamento, riscos e continuidade',2,1);
    SET @ModuloId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'Planejamento, riscos e continuidade dos negócios',N'PDF',N'',N'02-01-planejamento-riscos-e-continuidade-dos-negocios.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\gestao-de-processos-riscos-e-cidadania-digital\02-01-planejamento-riscos-e-continuidade-dos-negocios.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Avaliacoes(ModuloId,Titulo,Ativo) VALUES(@ModuloId,N'Revisão - Planejamento, riscos e continuidade',1);
    SET @AvaliacaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Para que serve um plano de contingência?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Definir alternativas para manter ou recuperar atividades diante de interrupções.',1),
      (@QuestaoId,N'Registrar somente o logotipo da empresa.',0),
      (@QuestaoId,N'Garantir que nenhum risco possa existir.',0),
      (@QuestaoId,N'Substituir todos os objetivos da organização por tarefas de impressão.',0);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Quais atividades fazem parte do gerenciamento de riscos?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Ignorar ocorrências e eliminar registros.',0),
      (@QuestaoId,N'Considerar somente fatos que já foram solucionados.',0),
      (@QuestaoId,N'Escolher exclusivamente as cores das telas.',0),
      (@QuestaoId,N'Identificar, analisar, planejar respostas e acompanhar riscos.',1);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'O que a missão de uma organização ajuda a expressar?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'O número exato de arquivos que pode armazenar.',0),
      (@QuestaoId,N'A senha necessária para acessar a rede.',0),
      (@QuestaoId,N'Sua razão de existir e seu propósito.',1),
      (@QuestaoId,N'Somente o preço de seus computadores.',0);

    -- Módulo 3: Marco Civil da Internet e cidadania digital
    INSERT INTO dbo.Modulos(CursoId,Titulo,Ordem,Ativo) VALUES(@CursoId,N'Marco Civil da Internet e cidadania digital',3,1);
    SET @ModuloId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Materiais(ModuloId,Titulo,Tipo,Url,ArquivoNome,ArquivoPdf,Ativo)
      SELECT @ModuloId,N'Marco Civil da Internet e direitos dos usuários',N'PDF',N'',N'03-01-marco-civil-da-internet-e-direitos-dos-usuarios.pdf',BulkColumn,1
      FROM OPENROWSET(BULK '$(PastaMateriais)\gestao-de-processos-riscos-e-cidadania-digital\03-01-marco-civil-da-internet-e-direitos-dos-usuarios.pdf',SINGLE_BLOB) AS pdf;
    INSERT INTO dbo.Avaliacoes(ModuloId,Titulo,Ativo) VALUES(@ModuloId,N'Revisão - Marco Civil da Internet e cidadania digital',1);
    SET @AvaliacaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Segundo o material estudado, qual é o tema central do Marco Civil da Internet?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Regras de ordenação de vetores em linguagem C.',0),
      (@QuestaoId,N'O formato obrigatório dos diagramas de classe.',0),
      (@QuestaoId,N'A montagem física de placas de rede.',0),
      (@QuestaoId,N'Princípios, garantias, direitos e deveres para o uso da internet no Brasil.',1);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Qual direito é discutido no material sobre o uso da internet?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'Proibição de informações claras nos contratos.',0),
      (@QuestaoId,N'Obrigatoriedade de compartilhar senhas com outros usuários.',0),
      (@QuestaoId,N'Proteção da intimidade e da vida privada.',1),
      (@QuestaoId,N'Divulgação irrestrita de toda comunicação privada.',0);
    INSERT INTO dbo.Questoes(AvaliacaoId,Enunciado,Ativo) VALUES(@AvaliacaoId,N'Qual aspecto da conexão à internet é apresentado como direito do usuário no material?',1);
    SET @QuestaoId=CAST(SCOPE_IDENTITY() AS int);
    INSERT INTO dbo.Alternativas(QuestaoId,Texto,Correta) VALUES
      (@QuestaoId,N'O acesso obrigatório por um único modelo de computador.',0),
      (@QuestaoId,N'A manutenção da qualidade contratada da conexão.',1),
      (@QuestaoId,N'A alteração do contrato sem informações ao usuário.',0),
      (@QuestaoId,N'A eliminação de todos os registros do serviço em qualquer circunstância.',0);
  END
  COMMIT TRANSACTION;
  SELECT @NovosCursos AS CursosAdicionados;
END TRY
BEGIN CATCH
  IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
  THROW;
END CATCH;
GO
