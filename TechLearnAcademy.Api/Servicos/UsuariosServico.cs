using Azure;
using Azure.Communication.Email;
using System.Net;
using System.Net.Mail;
using Microsoft.Data.SqlClient;
using TechLearnAcademy.Api.Banco;
using TechLearnAcademy.Api.Modelos;

namespace TechLearnAcademy.Api.Servicos;

public class UsuariosServico(ConexaoBanco conexao, IConfiguration configuracao)
{
    public Task Cadastro(DadosCadastro dados) => Cadastrar(dados, "Aluno", false);
    public Task PrimeiroAdministrador(DadosCadastro dados) => Cadastrar(dados, "Administrador", true);

    // Comando de manutenção do servidor; não possui rota HTTP.
    public async Task RedefinirSenhaAdministrador(string emailInformado, string senha, string confirmacao)
    {
        string email = Regras.Email(emailInformado);
        Regras.Senha(senha, confirmacao);
        using var banco = await conexao.Abrir();
        using var transacao = banco.BeginTransaction();
        var usuarios = await ConexaoBanco.Consultar(banco, "SELECT Id AS id FROM Usuarios WITH(UPDLOCK,HOLDLOCK) WHERE Email=@email AND Perfil='Administrador' AND Ativo=1", new { email }, transacao);
        Regras.Exigir(usuarios.Count == 1, "Administrador ativo não encontrado.", 404);
        int id = (int)usuarios[0]["id"]!;
        await ConexaoBanco.Gravar(banco, "UPDATE Usuarios SET SenhaHash=@hash WHERE Id=@id; UPDATE Sessoes SET Revogada=1 WHERE UsuarioId=@id; UPDATE RecuperacoesSenha SET Utilizado=1 WHERE UsuarioId=@id", new { id, hash = Regras.HashSenha(senha) }, transacao);
        await transacao.CommitAsync();
    }

    private async Task Cadastrar(DadosCadastro dados, string perfil, bool primeiroAdministrador)
    {
        Usuario usuario = new Usuario(perfil: perfil);
        usuario.AtualizarDados(dados.Nome, dados.Email);
        usuario.DefinirSenha(dados.Senha, dados.ConfirmarSenha);
        using var banco = await conexao.Abrir();
        using var transacao = banco.BeginTransaction(System.Data.IsolationLevel.Serializable);
        if (primeiroAdministrador)
        {
            int existentes = await ConexaoBanco.Numero(banco, "SELECT COUNT(*) FROM Usuarios WITH(UPDLOCK,HOLDLOCK) WHERE Perfil='Administrador' AND Ativo=1", null, transacao);
            Regras.Exigir(existentes == 0, "Já existe um Administrador ativo. Use a tela de usuários.", 409);
        }
        try
        {
            await Regras.EmailDisponivel(banco, usuario.Email, usuario.Email, 0, transacao);
            await ConexaoBanco.Gravar(banco, "INSERT INTO Usuarios(Nome,Email,EmailContato,SenhaHash,Perfil) VALUES(@nome,@email,@email,@hash,@perfil)", new { nome = usuario.Nome, email = usuario.Email, hash = usuario.SenhaHash, perfil = usuario.Perfil }, transacao);
            await transacao.CommitAsync();
        }
        catch (SqlException erro) when (erro.Number == 2601 || erro.Number == 2627)
        { throw new ErroAplicacao(409, "Este e-mail já está cadastrado."); }
    }

    public async Task<object> Login(DadosLogin dados, HttpResponse resposta, bool https)
    {
        string email = Regras.Email(dados.Email);
        Regras.Exigir(!string.IsNullOrEmpty(dados.Senha), "Informe a senha.");
        using var banco = await conexao.Abrir();
        using var transacao = banco.BeginTransaction();
        var lista = await ConexaoBanco.Consultar(banco, "SELECT Id AS id,Nome AS nome,Email AS email,Perfil AS perfil,SenhaHash AS senhaHash FROM Usuarios WITH(UPDLOCK,HOLDLOCK) WHERE Email=@email AND Ativo=1", new { email }, transacao);
        // Apenas a senha armazenada em hash decide se o login é válido.
        Usuario? usuario = lista.Count == 1 ? Usuario.DoBanco(lista[0]) : null;
        Regras.Exigir(usuario != null && usuario.ConferirSenha(dados.Senha), "E-mail ou senha inválidos.", 401);
        string token = Regras.NovoToken();
        DateTime validade = DateTime.UtcNow.AddHours(8);
        await ConexaoBanco.Gravar(banco, "INSERT INTO Sessoes(UsuarioId,TokenHash,Validade) VALUES(@usuarioId,@hash,@validade)", new { usuarioId = usuario!.Id, hash = Regras.HashToken(token), validade }, transacao);
        await transacao.CommitAsync();
        resposta.Cookies.Append("TechLearnSessao", token, new CookieOptions
        {
            HttpOnly = true, Secure = https, SameSite = SameSiteMode.Strict,
            Path = "/", MaxAge = TimeSpan.FromHours(8), IsEssential = true
        });
        return new { mensagem = "Login realizado com sucesso.", id = usuario!.Id, nome = usuario.Nome, email = usuario.Email, perfil = usuario.Perfil, token };
    }

    public async Task<object> Perfil(int id)
    {
        using var banco = await conexao.Abrir();
        var lista = await ConexaoBanco.Consultar(banco, "SELECT Id AS id,Nome AS nome,Email AS email,EmailContato AS emailContato,Perfil AS perfil,Ativo AS ativo FROM Usuarios WHERE Id=@id", new { id });
        Regras.Exigir(lista.Count == 1, "Usuário não encontrado.", 404);
        Usuario usuario = Usuario.DoBanco(lista[0]);
        return new { id = usuario.Id, nome = usuario.Nome, email = usuario.Email, perfil = usuario.Perfil, ativo = usuario.Ativo, emailContato = lista[0]["emailContato"] };
    }

    public async Task AlterarPerfil(int id, DadosPerfil dados)
    {
        string nome = Regras.Texto(dados.Nome, 2, 100, "Nome");
        using var banco = await conexao.Abrir();
        using var transacao = banco.BeginTransaction(System.Data.IsolationLevel.Serializable);
        var atual = (await ConexaoBanco.Consultar(banco, "SELECT Email AS email,Perfil AS perfil FROM Usuarios WITH(UPDLOCK,HOLDLOCK) WHERE Id=@id", new { id }, transacao))[0];
        string perfil = (string)atual["perfil"]!;
        string email = perfil == "Aluno" ? Regras.Email(dados.Email) : (string)atual["email"]!;
        string contato = Regras.Email(string.IsNullOrWhiteSpace(dados.EmailContato) ? email : dados.EmailContato);
        await Regras.EmailDisponivel(banco, email, contato, id, transacao);
        await ConexaoBanco.Gravar(banco, "UPDATE Usuarios SET Nome=@nome,Email=@email,EmailContato=@contato WHERE Id=@id", new { nome, email, contato, id }, transacao);
        await transacao.CommitAsync();
    }

    // Usa o ID do banco: Ana Silva, ID 12 -> ana.silva.12@mentor.techlearn.com.
    public static string EmailInstitucional(string nome, int id, string perfil)
    {
        Regras.Exigir(perfil == "Mentor" || perfil == "Gestor", "O cadastro institucional permite Mentor ou Gestor.");
        string[] partes = nome.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        Regras.Exigir(partes.Length >= 2, "Informe o primeiro e o segundo nome.");
        string Limpar(string parte) => System.Text.RegularExpressions.Regex.Replace(
            parte.Normalize(System.Text.NormalizationForm.FormD), @"[^a-zA-Z0-9]", "").ToLowerInvariant();
        string primeiro = Limpar(partes[0]), segundo = Limpar(partes[1]);
        Regras.Exigir(primeiro.Length > 0 && segundo.Length > 0, "Informe nomes que possam formar o e-mail.");
        return primeiro + "." + segundo + "." + id + "@" + perfil.ToLowerInvariant() + ".techlearn.com";
    }

    public async Task<object> CriarConta(Usuario autor, DadosConta dados)
    {
        Regras.Exigir((autor.Perfil == "Administrador" && (dados.Perfil == "Mentor" || dados.Perfil == "Gestor")) ||
            (autor.Perfil == "Gestor" && dados.Perfil == "Mentor"), "Você não pode cadastrar este perfil.", 403);
        string nome = Regras.Texto(dados.Nome, 2, 100, "Nome");
        string contato = Regras.Email(dados.EmailContato);
        EmailInstitucional(nome, 1, dados.Perfil);
        Regras.Senha(dados.Senha, dados.ConfirmarSenha);
        string hash = Regras.HashSenha(dados.Senha);
        using var banco = await conexao.Abrir();
        using var transacao = banco.BeginTransaction(System.Data.IsolationLevel.Serializable);
        string temporario = Guid.NewGuid().ToString("N") + "@cadastro.techlearn.com";
        await Regras.EmailDisponivel(banco, temporario, contato, 0, transacao);
        int id = await ConexaoBanco.Numero(banco, "INSERT INTO Usuarios(Nome,Email,EmailContato,SenhaHash,Perfil) VALUES(@nome,@temporario,@contato,@hash,@perfil); SELECT CAST(SCOPE_IDENTITY() AS int)", new { nome, temporario, contato, hash, perfil = dados.Perfil }, transacao);
        string email = EmailInstitucional(nome, id, dados.Perfil);
        await Regras.EmailDisponivel(banco, email, contato, id, transacao);
        await ConexaoBanco.Gravar(banco, "UPDATE Usuarios SET Email=@email WHERE Id=@id", new { id, email }, transacao);
        await transacao.CommitAsync();
        return new { id, email, mensagem = "Conta criada. E-mail de acesso: " + email };
    }

    public async Task TrocarSenha(int id, DadosSenha dados)
    {
        Regras.Senha(dados.Senha, dados.ConfirmarSenha);
        using var banco = await conexao.Abrir();
        using var transacao = banco.BeginTransaction();
        var lista = await ConexaoBanco.Consultar(banco, "SELECT SenhaHash AS hash FROM Usuarios WITH(UPDLOCK) WHERE Id=@id AND Perfil<>'Administrador'", new { id }, transacao);
        Regras.Exigir(lista.Count == 1 && Regras.ConferirSenha(dados.SenhaAtual ?? "", (string)lista[0]["hash"]!), "A senha atual está incorreta.", 400);
        await ConexaoBanco.Gravar(banco, "UPDATE Usuarios SET SenhaHash=@hash WHERE Id=@id; UPDATE Sessoes SET Revogada=1 WHERE UsuarioId=@id", new { id, hash = Regras.HashSenha(dados.Senha) }, transacao);
        await transacao.CommitAsync();
    }

    public async Task Recuperar(DadosEmail dados)
    {
        string email = Regras.Email(dados.Email);
        string servidor = configuracao["Email:Servidor"] ?? "";
        string conexaoAzure = configuracao["Email:ConexaoAzure"] ?? "";
        string remetente = configuracao["Email:Remetente"] ?? "";
        Regras.Exigir((conexaoAzure != "" || servidor != "") && remetente != "", "Configure o serviço de e-mail para recuperar a senha.", 503);
        string url = Regras.Url(configuracao["Email:UrlRedefinicao"]);
        using var banco = await conexao.Abrir();
        var usuarios = await ConexaoBanco.Consultar(banco, "SELECT Id AS id,COALESCE(EmailContato,Email) AS destino FROM Usuarios WHERE (Email=@email OR EmailContato=@email) AND Ativo=1 AND Perfil<>'Administrador'", new { email });
        // A resposta não revela se o endereço pertence a uma conta.
        if (usuarios.Count != 1) return;
        int id = (int)usuarios[0]["id"]!;
        string token = Regras.NovoToken();
        string hash = Regras.HashToken(token);
        await ConexaoBanco.Gravar(banco, "UPDATE RecuperacoesSenha SET Utilizado=1 WHERE UsuarioId=@id AND Utilizado=0; INSERT INTO RecuperacoesSenha(UsuarioId,TokenHash,Validade) VALUES(@id,@hash,@validade)", new { id, hash, validade = DateTime.UtcNow.AddMinutes(30) });
        string assunto = "TechLearn Academy — recuperação de senha";
        string texto = "Para criar sua nova senha, abra este link: " + url + "?token=" + Uri.EscapeDataString(token) + "\nO link vence em 30 minutos e funciona uma única vez. Se você não solicitou, ignore este e-mail.";
        string destino = (string)usuarios[0]["destino"]!;
        try
        {
            if (conexaoAzure != "")
            {
                var cliente = new EmailClient(conexaoAzure);
                var mensagem = new EmailMessage(remetente, destino, new EmailContent(assunto) { PlainText = texto });
                await cliente.SendAsync(WaitUntil.Completed, mensagem);
            }
            else
            {
                using var mensagem = new MailMessage(remetente, destino, assunto, texto);
                using var smtp = new SmtpClient(servidor, configuracao.GetValue("Email:Porta", 587));
                smtp.EnableSsl = configuracao.GetValue("Email:UsarTls", true);
                string usuarioSmtp = configuracao["Email:Usuario"] ?? "";
                if (usuarioSmtp != "") smtp.Credentials = new NetworkCredential(usuarioSmtp, configuracao["Email:Senha"]);
                await smtp.SendMailAsync(mensagem);
            }
        }
        catch (Exception)
        {
            await ConexaoBanco.Gravar(banco, "UPDATE RecuperacoesSenha SET Utilizado=1 WHERE TokenHash=@hash", new { hash });
            throw new ErroAplicacao(503, "Não foi possível enviar o e-mail de recuperação. Verifique o serviço de e-mail.");
        }
    }

    public async Task Redefinir(DadosSenha dados)
    {
        Regras.Senha(dados.Senha, dados.ConfirmarSenha);
        Regras.Exigir(!string.IsNullOrWhiteSpace(dados.Token), "Informe o código de recuperação.");
        using var banco = await conexao.Abrir();
        using var transacao = banco.BeginTransaction();
        var lista = await ConexaoBanco.Consultar(banco, "SELECT r.Id AS id,r.UsuarioId AS usuarioId FROM RecuperacoesSenha r WITH(UPDLOCK,HOLDLOCK) JOIN Usuarios u ON u.Id=r.UsuarioId WHERE r.TokenHash=@hash AND r.Utilizado=0 AND r.Validade>SYSUTCDATETIME() AND u.Ativo=1 AND u.Perfil<>'Administrador'", new { hash = Regras.HashToken(dados.Token) }, transacao);
        Regras.Exigir(lista.Count == 1, "Código inválido, vencido ou já utilizado.");
        int id = (int)lista[0]["id"]!;
        int usuarioId = (int)lista[0]["usuarioId"]!;
        await ConexaoBanco.Gravar(banco, "UPDATE Usuarios SET SenhaHash=@hash WHERE Id=@usuarioId; UPDATE RecuperacoesSenha SET Utilizado=1 WHERE UsuarioId=@usuarioId; UPDATE Sessoes SET Revogada=1 WHERE UsuarioId=@usuarioId", new { id, usuarioId, hash = Regras.HashSenha(dados.Senha) }, transacao);
        await transacao.CommitAsync();
    }

    public async Task Logout(string token)
    {
        using var banco = await conexao.Abrir();
        await ConexaoBanco.Gravar(banco, "UPDATE Sessoes SET Revogada=1 WHERE TokenHash=@hash", new { hash = Regras.HashToken(token) });
    }
}
