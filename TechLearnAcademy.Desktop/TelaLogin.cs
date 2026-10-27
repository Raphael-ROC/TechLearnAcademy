namespace TechLearnAcademy.Desktop;

public class TelaLogin : Form
{
    private readonly ApiCliente api;
    private readonly TextBox email;
    private readonly TextBox senha;
    private readonly Button entrar;
    private readonly Label mensagem;
    private readonly Panel conteudo;
    private readonly Panel formulario;
    private bool enviando;
    public Usuario? Usuario { get; private set; }

    public TelaLogin(ApiCliente api)
    {
        this.api = api;
        Estilo.Preparar(this, "Acesso administrativo", 590, 535);
        MinimumSize = new Size(460, 520);
        var cabecalho = new Panel { BackColor = Estilo.Azul, Dock = DockStyle.Top, Height = 104 };
        var marca = Estilo.Texto(cabecalho, "TechLearn Academy", 28, 22, 520, 30, true);
        marca.ForeColor = Color.White;
        var descricao = Estilo.Texto(cabecalho, "Plataforma de cursos · Gestão administrativa", 28, 59, 520);
        descricao.ForeColor = Color.White;
        marca.Anchor = descricao.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        conteudo = new Panel { Dock = DockStyle.Fill, AutoScroll = true, AutoScrollMinSize = new Size(380, 410) };
        formulario = new Panel { Size = new Size(546, 390) };
        conteudo.Controls.Add(formulario);
        Controls.Add(conteudo);
        Controls.Add(cabecalho);
        Estilo.Texto(formulario, "Entrar como Administrador", 28, 10, 490, 32, true);
        Estilo.Texto(formulario, "Utilize a mesma conta administrativa do site.", 28, 49, 490, 44);
        email = Estilo.Campo(formulario, "E-mail", 101);
        senha = Estilo.Campo(formulario, "Senha", 173, true);
        entrar = Estilo.Botao(formulario, "Entrar", 28, 252, 490);
        mensagem = Estilo.Texto(formulario, "", 28, 303, 490, 76);
        foreach (Control controle in formulario.Controls)
            controle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        AcceptButton = entrar;
        entrar.Click += Entrar;
        conteudo.Resize += AjustarFormulario;
        FormClosing += (_, e) => { if (enviando) e.Cancel = true; };
        Shown += (_, _) => { AjustarFormulario(this, EventArgs.Empty); email.Focus(); };
    }

    private void AjustarFormulario(object? sender, EventArgs e)
    {
        // Centraliza o formulário e aumenta os campos até uma largura confortável.
        formulario.Width = Math.Min(760, Math.Max(360, conteudo.ClientSize.Width - 32));
        formulario.Left = Math.Max(0, (conteudo.ClientSize.Width - formulario.Width) / 2);
        formulario.Top = Math.Max(12, (conteudo.ClientSize.Height - formulario.Height) / 2);
    }

    private async void Entrar(object? sender, EventArgs e)
    {
        if (enviando) return;
        try
        {
            Validacao.Email(email.Text.Trim());
            if (senha.Text.Length == 0) throw new InvalidOperationException("Informe a senha.");
            enviando = true;
            entrar.Enabled = false;
            entrar.Text = "Entrando...";
            Estilo.Aviso(mensagem, "Entrando... A primeira conexão com a Azure pode demorar um pouco.");
            // A API confere a conta e o perfil.
            Usuario = await api.EntrarAdministrador(email.Text.Trim(), senha.Text);
            enviando = false;
            DialogResult = DialogResult.OK;
        }
        catch (Exception erro) { api.LimparSessao(); Estilo.Aviso(mensagem, erro.Message, true); }
        finally
        {
            senha.Clear();
            enviando = false;
            entrar.Enabled = true;
            entrar.Text = "Entrar";
        }
    }
}