namespace TechLearnAcademy.Api.Modelos;

// Campos dos formulários HTTP reunidos aqui. Não fazem parte do diagrama de classes.

public struct DadosAlternativa
{
    public DadosAlternativa() { }
    public string Texto { get; set; } = "";
    public bool Correta { get; set; }
}

public struct DadosAvaliacao
{
    public DadosAvaliacao() { }
    public int ModuloId { get; set; }
    public string Titulo { get; set; } = "";
    public bool Ativo { get; set; } = true;
}

public struct DadosCadastro
{
    public DadosCadastro() { }
    public string Nome { get; set; } = "";
    public string Email { get; set; } = "";
    public string Senha { get; set; } = "";
    public string ConfirmarSenha { get; set; } = "";
}

public struct DadosCurso
{
    public DadosCurso() { }
    public string Nome { get; set; } = "";
    public string Descricao { get; set; } = "";
    public int CargaHoraria { get; set; }
    public bool Ativo { get; set; } = true;
}

public struct DadosEmail
{
    public DadosEmail() { }
    public string Email { get; set; } = "";
}

public struct DadosLogin
{
    public DadosLogin() { }
    public string Email { get; set; } = "";
    public string Senha { get; set; } = "";
}

public struct DadosMaterial
{
    public DadosMaterial() { }
    public int ModuloId { get; set; }
    public string Titulo { get; set; } = "";
    public string Tipo { get; set; } = "";
    public string Url { get; set; } = "";
    public bool Ativo { get; set; } = true;
}

public struct DadosMatricula
{
    public DadosMatricula() { }
    public int CursoId { get; set; }
}

public struct DadosModulo
{
    public DadosModulo() { }
    public int CursoId { get; set; }
    public string Titulo { get; set; } = "";
    public int Ordem { get; set; }
    public bool Ativo { get; set; } = true;
}

public struct DadosOrientacao
{
    public DadosOrientacao() { }
    public int AlunoId { get; set; }
    public string Texto { get; set; } = "";
}

public struct DadosPerfil
{
    public DadosPerfil() { }
    public string EmailContato { get; set; } = "";
    public string Nome { get; set; } = "";
    public string Email { get; set; } = "";
}

public struct DadosProgresso
{
    public DadosProgresso() { }
    public int ModuloId { get; set; }
}

public struct DadosProva
{
    public DadosProva() { }
    public int TentativaCurso { get; set; } = 1;
    public List<DadosResposta> Respostas { get; set; } = [];
}

public struct DadosQuestao
{
    public DadosQuestao() { }
    public int AvaliacaoId { get; set; }
    public string Enunciado { get; set; } = "";
    public bool Ativo { get; set; } = true;
    public List<DadosAlternativa> Alternativas { get; set; } = [];
}

public struct DadosResposta
{
    public DadosResposta() { }
    public int QuestaoId { get; set; }
    public int AlternativaId { get; set; }
}

public struct DadosSenha
{
    public DadosSenha() { }
    public string Token { get; set; } = "";
    public string SenhaAtual { get; set; } = "";
    public string Senha { get; set; } = "";
    public string ConfirmarSenha { get; set; } = "";
}

// Dados de uma conta: o perfil é escolhido no cadastro e permanece fixo.
public struct DadosUsuario
{
    public DadosUsuario() { }
    public string Nome { get; set; } = "";
    public string Email { get; set; } = "";
    public string EmailContato { get; set; } = "";
    public bool Ativo { get; set; } = true;
}

public struct DadosConta
{
    public DadosConta() { }
    public string Nome { get; set; } = "";
    public string EmailContato { get; set; } = "";
    public string Perfil { get; set; } = "Mentor";
    public string Senha { get; set; } = "";
    public string ConfirmarSenha { get; set; } = "";
}

public struct DadosChamado
{
    public DadosChamado() { }
    public int CursoId { get; set; }
    public string Assunto { get; set; } = "";
    public string Texto { get; set; } = "";
}

public struct DadosMensagem
{
    public DadosMensagem() { }
    public string Texto { get; set; } = "";
}

public struct DadosCertificado
{
    public DadosCertificado() { }
    public int CursoId { get; set; }
    public bool Aprovar { get; set; }
    public string Motivo { get; set; } = "";
}
