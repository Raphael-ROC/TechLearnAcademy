namespace TechLearnAcademy.Desktop;

public class TelaUsuario : Form
{
    private readonly ApiCliente api;
    private readonly Usuario? usuario;
    private readonly bool proprioPerfil;
    private readonly TextBox nome;
    private readonly TextBox contato;
    private readonly TextBox email;
    private readonly TextBox senha;
    private readonly TextBox confirmacao;
    private readonly ComboBox perfil = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly CheckBox ativo = new() { Text = "Conta ativa", AutoSize = true };
    private readonly Button salvar;
    private readonly Label mensagem;
    private bool salvando;
    public bool Alterou { get; private set; }

    public TelaUsuario(ApiCliente api, Usuario? usuario = null, bool proprioPerfil = false)
    {
        this.api = api;
        this.usuario = usuario;
        this.proprioPerfil = proprioPerfil;
        string titulo = usuario == null ? "Cadastrar usuário" : proprioPerfil ? "Meu perfil" : "Editar usuário";
        Estilo.Preparar(this, titulo, 565, 640);
        MinimumSize = new Size(581, 679);
        Estilo.Texto(this, titulo, 28, 20, 490, 30, true);
        Estilo.Texto(this, usuario == null ? "O e-mail institucional será gerado pela API.\nInforme o primeiro e o segundo nome." :
            "O perfil e o e-mail institucional permanecem fixos.", 28, 55, 490, 45);
        nome = Estilo.Campo(this, "Nome completo", 108);
        contato = Estilo.Campo(this, proprioPerfil ? "E-mail pessoal de contato" : "E-mail pessoal para recuperação", 174);
        email = Estilo.Campo(this, "E-mail de acesso", 240, mostrar: usuario != null);
        Estilo.Texto(this, "Perfil", 28, 240, 490, 24, true).Visible = usuario == null;
        perfil.SetBounds(28, 266, 490, 28);
        perfil.AccessibleName = "Perfil da conta";
        perfil.Items.AddRange(["Mentor", "Gestor"]);
        perfil.SelectedIndex = 0;
        Controls.Add(perfil);
        senha = Estilo.Campo(this, "Senha inicial", 306, true, usuario == null);
        confirmacao = Estilo.Campo(this, "Confirmar senha", 372, true, usuario == null);
        ativo.Location = new Point(28, 391);
        ativo.Checked = usuario?.Ativo ?? true;
        Controls.Add(ativo);
        Estilo.Texto(this, usuario == null ? "Mínimo de 8 caracteres, uma letra maiúscula, um número e um caractere especial. Sem espaços." :
            "As alterações serão conferidas pela API e gravadas no mesmo banco do site.", 28, 443, 490, 46);
        mensagem = Estilo.Texto(this, "", 28, 500, 490, 58);
        salvar = Estilo.Botao(this, usuario == null ? "Cadastrar conta" : "Salvar alterações", 28, 574, 240);
        var cancelar = Estilo.Botao(this, "Cancelar", 285, 574, 233, true);
        // As margens acompanham o tamanho da janela.
        foreach (Control controle in Controls)
            if (controle is TextBox || controle is Label || controle is ComboBox)
                controle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        salvar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        cancelar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        mensagem.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        cancelar.Click += (_, _) => Close();
        CancelButton = cancelar;
        AcceptButton = salvar;
        // O mesmo formulário atende cadastro, edição e os próprios dados.
        perfil.Visible = usuario == null;
        ativo.Visible = usuario != null && !proprioPerfil;
        if (usuario != null)
        {
            nome.Text = usuario.Nome;
            contato.Text = usuario.EmailContato ?? usuario.Email;
            email.Text = usuario.Email;
            email.ReadOnly = proprioPerfil || usuario.Perfil != "Aluno";
            Estilo.Texto(this, "Perfil: " + usuario.Perfil, 28, 324, 490, 28, true);
        }
        salvar.Click += Salvar;
        FormClosing += (_, e) => { if (salvando) e.Cancel = true; };
        Shown += (_, _) => nome.Focus();
    }

    private async void Salvar(object? sender, EventArgs e)
    {
        if (salvando) return;
        try
        {
            string nomeInformado = nome.Text.Trim();
            string contatoInformado = contato.Text.Trim();
            Validacao.Nome(nomeInformado);
            Validacao.Email(contatoInformado);
            if (usuario == null)
            {
                if (nomeInformado.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length < 2)
                    throw new InvalidOperationException("Informe o primeiro e o segundo nome.");
                Validacao.Senha(senha.Text, confirmacao.Text);
            }
            else Validacao.Email(email.Text.Trim());
            if (usuario != null && !proprioPerfil && ativo.Checked != usuario.Ativo &&
                MessageBox.Show((ativo.Checked ? "Ativar" : "Desativar") + " a conta de " + usuario.Nome + "?", "Situação da conta",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
            salvando = true;
            salvar.Enabled = false;
            salvar.Text = "Salvando...";
            Estilo.Aviso(mensagem, "Salvando...");
            RespostaApi resposta;
            if (usuario == null)
            {
                resposta = await api.Enviar<RespostaApi>("api/administracao/usuarios", HttpMethod.Post,
                    new { nome = nomeInformado, emailContato = contatoInformado, perfil = perfil.Text, senha = senha.Text, confirmarSenha = confirmacao.Text });
                senha.Clear(); confirmacao.Clear();
                MessageBox.Show(resposta.Mensagem + "\n\nInforme ao usuário o e-mail de acesso e a senha inicial que você definiu.",
                    "Conta cadastrada", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                string caminho = proprioPerfil ? "api/usuarios/perfil" : "api/administracao/usuarios/" + usuario.Id;
                resposta = await api.Enviar<RespostaApi>(caminho, HttpMethod.Put,
                    new { nome = nomeInformado, email = email.Text.Trim(), emailContato = contatoInformado, ativo = ativo.Checked });
            }
            Alterou = true;
            salvando = false;
            DialogResult = DialogResult.OK;
        }
        catch (Exception erro)
        {
            Estilo.Aviso(mensagem, erro.Message, true);
            if (erro is ErroApi falha && falha.Status == 401) { salvando = false; DialogResult = DialogResult.Abort; }
        }
        finally
        {
            salvando = false;
            salvar.Enabled = true;
            salvar.Text = usuario == null ? "Cadastrar conta" : "Salvar alterações";
        }
    }
}
