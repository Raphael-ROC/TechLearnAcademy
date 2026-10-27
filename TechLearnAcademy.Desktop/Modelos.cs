namespace TechLearnAcademy.Desktop;

// Estas classes representam o JSON devolvido pela API.
public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public string Email { get; set; } = "";
    public string? EmailContato { get; set; }
    public string Perfil { get; set; } = "";
    public bool Ativo { get; set; } = true;
}

public class RespostaApi
{
    public string Mensagem { get; set; } = "";
    public string Email { get; set; } = "";
}

public class PaginaUsuarios
{
    public List<Usuario> Itens { get; set; } = [];
    public int Pagina { get; set; }
    public int TotalPaginas { get; set; }
    public int Total { get; set; }
}

public class ResumoAdmin
{
    public int Ativos { get; set; }
    public int Inativos { get; set; }
    public List<ContagemPerfil> Perfis { get; set; } = [];
}

public class ContagemPerfil
{
    public string Perfil { get; set; } = "";
    public int Quantidade { get; set; }
}

public class Chamado
{
    public int Id { get; set; }
    public int SolicitanteId { get; set; }
    public int? ResponsavelId { get; set; }
    public string Assunto { get; set; } = "";
    public string Descricao { get; set; } = "";
    public string Solicitante { get; set; } = "";
    public string? Responsavel { get; set; }
    public string Status { get; set; } = "";
    public bool PodeAtender { get; set; }
    public bool PodeResponder { get; set; }
    public bool PodeEncerrar { get; set; }
    public List<MensagemChamado> Mensagens { get; set; } = [];
}

public class MensagemChamado
{
    public string Nome { get; set; } = "";
    public string Perfil { get; set; } = "";
    public string Texto { get; set; } = "";
    public DateTime Data { get; set; }
}
