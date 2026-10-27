using System.Reflection;
using System.Text.Json;
using TechLearnAcademy.Desktop;

internal static class Program
{
    private static readonly List<string> aprovados = [];

    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        IDataObject? anterior = Clipboard.GetDataObject();
        try
        {
            using var api = new ApiCliente(new Uri("https://localhost:7345/"));
            using var login = new TelaLogin(api);
            Testar("Login: e-mail", Campo(login, "email"));
            Testar("Login: senha", Campo(login, "senha"));
            Conferir("Senha continua oculta", Campo(login, "senha").UseSystemPasswordChar);

            using var cadastro = new TelaUsuario(api);
            foreach (string nome in new[] { "nome", "contato", "senha", "confirmacao" })
                Testar("Cadastro: " + nome, Campo(cadastro, nome));

            var aluno = new Usuario { Id = 1, Nome = "Aluno de teste", Email = "aluno@example.com", Perfil = "Aluno", Ativo = true };
            using var edicao = new TelaUsuario(api, aluno);
            Testar("Editar aluno: e-mail", Campo(edicao, "email"));

            var administrador = new Usuario { Id = 2, Nome = "Administrador de teste", Email = "admin@example.com", Perfil = "Administrador", Ativo = true };
            using var perfil = new TelaUsuario(api, administrador, true);
            Testar("Meu perfil: nome", Campo(perfil, "nome"));
            Testar("Meu perfil: contato", Campo(perfil, "contato"));
            TextBox fixo = Campo(perfil, "email");
            string emailFixo = fixo.Text;
            Clipboard.SetText("nao-alterar@example.com");
            Tecla(fixo, Keys.Control | Keys.V);
            fixo.ContextMenuStrip!.Items[3].PerformClick();
            Conferir("E-mail institucional permanece bloqueado", fixo.ReadOnly && fixo.Text == emailFixo);

            using var principal = new TelaPrincipal(api, administrador);
            Testar("Busca de usuários", Campo(principal, "busca"));
            using var chamado = new TelaChamado(api, 1);
            TextBox resposta = Campo(chamado, "resposta");
            resposta.Enabled = true;
            Testar("Resposta de chamado", resposta, "Primeira linha\r\nSegunda linha: ação!123");
            resposta.Clear();
            Clipboard.SetText(new string('A', 4000));
            Tecla(resposta, Keys.Control | Keys.V);
            Conferir("Chamado aceita 4000 caracteres e mantém o limite", resposta.TextLength == 4000 && resposta.MaxLength == 4000);
            resposta.Enabled = false;
            Clipboard.SetText("Texto que não deve entrar");
            Tecla(resposta, Keys.Control | Keys.V);
            Conferir("Campo desabilitado permanece intacto", resposta.TextLength == 4000);

            TextBox senha = Campo(login, "senha");
            Testar("Senha: espaços e caracteres preservados", senha, "  Árvore!123 com espaços  ");
            Testar("Senha: texto longo", senha, new string('A', 50000) + "!123");

            TestarLayout(api, Path.Combine(AppContext.BaseDirectory, "../../../../evidencias"));

            if (args.Length > 0)
            {
                string destino = Path.GetFullPath(args[0]);
                Directory.CreateDirectory(Path.GetDirectoryName(destino)!);
                File.WriteAllText(destino, JsonSerializer.Serialize(new { data = DateTimeOffset.Now, total = aprovados.Count, aprovados,
                    observacao = "Testes dos próprios controles do aplicativo. Sem login, solicitações à API ou acesso ao banco." },
                    new JsonSerializerOptions { WriteIndented = true }));
            }
            Console.WriteLine($"APROVADOS: {aprovados.Count}");
        }
        finally
        {
            if (anterior != null) Clipboard.SetDataObject(anterior, true);
            else Clipboard.Clear();
        }
    }


    private static void TestarLayout(ApiCliente api, string pasta)
    {
        Directory.CreateDirectory(pasta);
        using var login = new TelaLogin(api);
        login.ShowInTaskbar = false;
        login.Show();
        Application.DoEvents();
        Conferir("Login permite maximização", login.MaximizeBox && login.FormBorderStyle == FormBorderStyle.Sizable);
        Conferir("Login não exibe o endereço do site", !Controles(login).Any(c => c.Text.Contains("azurewebsites.net")));
        foreach (Size tamanho in new[] { new Size(590, 535), new Size(1280, 720), new Size(1920, 1080) })
        {
            login.ClientSize = tamanho;
            Application.DoEvents();
            var formulario = (Panel)login.GetType().GetField("formulario", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(login)!;
            var email = Campo(login, "email");
            var senha = Campo(login, "senha");
            Conferir("Campos acompanham a largura em " + tamanho, email.Width == formulario.Width - 56 && senha.Width == email.Width);
            Conferir("Formulário centralizado em " + tamanho, formulario.Left >= 0 && formulario.Right <= formulario.Parent!.ClientSize.Width);
            Conferir("Campos não se sobrepõem em " + tamanho, email.Bottom < senha.Top);
            using var imagem = new Bitmap(login.Width, login.Height);
            login.DrawToBitmap(imagem, new Rectangle(Point.Empty, imagem.Size));
            imagem.Save(Path.Combine(pasta, "login-" + tamanho.Width + ".png"));
        }
        login.WindowState = FormWindowState.Maximized;
        Application.DoEvents();
        Conferir("Login maximizado no Windows", login.WindowState == FormWindowState.Maximized);
        login.Hide();
        using var usuario = new TelaUsuario(api);
        usuario.ShowInTaskbar = false;
        usuario.Show();
        Application.DoEvents();
        int larguraInicial = Campo(usuario, "nome").Width;
        usuario.ClientSize = new Size(1000, 760);
        Application.DoEvents();
        Conferir("Cadastro permite maximização", usuario.MaximizeBox && usuario.FormBorderStyle == FormBorderStyle.Sizable);
        Conferir("Campos do cadastro aumentam com a janela", Campo(usuario, "nome").Width > larguraInicial);
        foreach (string nome in new[] { "nome", "contato", "senha", "confirmacao" })
            Conferir("Campo dentro do formulário: " + nome, Campo(usuario, nome).Right <= usuario.ClientSize.Width && Campo(usuario, nome).Left >= 0);
        usuario.Hide();
        var admin = new Usuario { Id = 1, Nome = "Prévia de layout", Perfil = "Administrador" };
        using var principal = new TelaPrincipal(api, admin);
        using var chamado = new TelaChamado(api, 1);
        Conferir("Painel permite maximização", principal.MaximizeBox && principal.FormBorderStyle == FormBorderStyle.Sizable);
        Conferir("Chamado permite maximização", chamado.MaximizeBox && chamado.FormBorderStyle == FormBorderStyle.Sizable);
        int respostaInicial = Campo(chamado, "resposta").Width;
        chamado.ClientSize = new Size(1280, 900);
        chamado.PerformLayout();
        Conferir("Resposta do chamado aumenta com a janela", Campo(chamado, "resposta").Width > respostaInicial);
    }

    private static IEnumerable<Control> Controles(Control pai)
    {
        foreach (Control controle in pai.Controls)
        {
            yield return controle;
            foreach (Control filho in Controles(controle)) yield return filho;
        }
    }

    private static TextBox Campo(Form tela, string nome) =>
        (TextBox)tela.GetType().GetField(nome, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(tela)!;

    private static void Tecla(TextBox campo, Keys teclas)
    {
        _ = campo.Handle;
        var evento = new KeyEventArgs(teclas);
        typeof(Control).GetMethod("OnKeyDown", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(campo, [evento]);
        Conferir("Atalho tratado: " + campo.AccessibleName, evento.SuppressKeyPress);
    }

    private static void Testar(string nome, TextBox campo, string texto = "Texto de teste: Árvore!123")
    {
        campo.Clear();
        Clipboard.SetText(texto);
        Tecla(campo, Keys.Control | Keys.V);
        Conferir(nome + ": Ctrl+V", campo.Text == texto);
        campo.SelectAll();
        Clipboard.SetText("Substituição!456");
        Tecla(campo, Keys.Shift | Keys.Insert);
        Conferir(nome + ": Shift+Insert substitui a seleção", campo.Text == "Substituição!456");
        campo.SelectAll();
        Clipboard.SetText(texto);
        campo.ContextMenuStrip!.Items[3].PerformClick();
        Conferir(nome + ": menu Colar", campo.Text == texto);
        Conferir(nome + ": atalhos nativos preservados", campo.ShortcutsEnabled);
    }

    private static void Conferir(string nome, bool condicao)
    {
        if (!condicao) throw new InvalidOperationException("Falhou: " + nome);
        aprovados.Add(nome);
        Console.WriteLine("OK: " + nome);
    }
}
