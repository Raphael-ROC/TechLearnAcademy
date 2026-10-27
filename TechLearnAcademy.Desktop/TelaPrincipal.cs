namespace TechLearnAcademy.Desktop;

public class TelaPrincipal : Form
{
    private readonly ApiCliente api;
    private Usuario administrador;
    private readonly TabControl abas = new() { Dock = DockStyle.Fill };
    private readonly Label mensagem = new() { Dock = DockStyle.Fill, Padding = new Padding(20, 12, 20, 0) };
    private readonly Label nomeAdmin;
    private readonly Label totais;
    private readonly Label dadosPerfil;
    private readonly Label paginaTexto;
    private readonly DataGridView perfis;
    private readonly DataGridView usuarios;
    private readonly DataGridView chamados;
    private readonly TextBox busca;
    private readonly ComboBox filtroChamados;
    private readonly Button anterior;
    private readonly Button proxima;
    private readonly Button sair;
    private readonly System.Windows.Forms.Timer atualizacao = new() { Interval = 5000 };
    private int pagina = 1;
    private string buscaAtual = "";
    private bool ocupado;
    private bool fechamentoPronto;
    public bool VoltarAoLogin { get; private set; }

    public TelaPrincipal(ApiCliente api, Usuario administrador)
    {
        this.api = api;
        this.administrador = administrador;
        Estilo.Preparar(this, "Administração", 1100, 730);
        MinimumSize = new Size(960, 650);
        var estrutura = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1 };
        estrutura.RowStyles.Add(new RowStyle(SizeType.Absolute, 88));
        estrutura.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        estrutura.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
        var cabecalho = new Panel { Size = new Size(ClientSize.Width, 88), Dock = DockStyle.Fill, BackColor = Estilo.Azul, Margin = Padding.Empty };
        Estilo.Texto(cabecalho, "TechLearn Academy", 22, 18, 300, 28, true).ForeColor = Color.White;
        nomeAdmin = Estilo.Texto(cabecalho, administrador.Nome + " · Administrador", 22, 50, 850, 25);
        nomeAdmin.ForeColor = Color.White;
        nomeAdmin.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        nomeAdmin.AutoEllipsis = true;
        sair = Estilo.Botao(cabecalho, "Sair", 940, 23, 120, true);
        sair.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        estrutura.Controls.Add(cabecalho, 0, 0);
        estrutura.Controls.Add(abas, 0, 1);
        estrutura.Controls.Add(mensagem, 0, 2);
        Controls.Add(estrutura);

        var painel = NovaAba("Visão geral");
        var topoPainel = Barra(painel, 115, DockStyle.Top);
        Estilo.Texto(topoPainel, "Painel do administrador", 10, 10, 700, 30, true);
        totais = Estilo.Texto(topoPainel, "Carregando os dados reais...", 10, 55, 880, 45);
        var acoesPainel = Barra(painel, 65, DockStyle.Bottom);
        Estilo.Botao(acoesPainel, "Atualizar painel", 10, 12).Click += (_, _) => Executar(CarregarPainel);
        Estilo.Botao(acoesPainel, "Verificar banco", 185, 12).Click += (_, _) => Executar(VerificarBanco);
        perfis = Estilo.Tabela(painel, "Perfil", "Quantidade de usuários");
        perfis.BringToFront();

        var abaUsuarios = NovaAba("Usuários");
        var topoUsuarios = Barra(abaUsuarios, 100, DockStyle.Top);
        Estilo.Texto(topoUsuarios, "Buscar por nome ou e-mail", 10, 8, 500, 24, true);
        busca = new TextBox { Location = new Point(10, 38), Width = 430, AccessibleName = "Buscar por nome ou e-mail" };
        Estilo.HabilitarColagem(busca);
        topoUsuarios.Controls.Add(busca);
        var buscar = Estilo.Botao(topoUsuarios, "Buscar", 455, 32, 110);
        buscar.Click += (_, _) => BuscarUsuarios();
        busca.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; BuscarUsuarios(); } };
        Estilo.Botao(topoUsuarios, "Cadastrar usuário", 580, 32, 190).Click += (_, _) => Executar(CadastrarUsuario);
        var acoesUsuarios = Barra(abaUsuarios, 65, DockStyle.Bottom);
        Estilo.Botao(acoesUsuarios, "Editar dados", 10, 12, 145).Click += (_, _) => Executar(EditarUsuario);
        Estilo.Botao(acoesUsuarios, "Ativar / desativar", 165, 12, 180).Click += (_, _) => Executar(AlterarSituacao);
        anterior = Estilo.Botao(acoesUsuarios, "Anterior", 365, 12, 105, true);
        proxima = Estilo.Botao(acoesUsuarios, "Próxima", 480, 12, 105, true);
        paginaTexto = Estilo.Texto(acoesUsuarios, "", 600, 22, 300, 28);
        anterior.Click += (_, _) => { pagina--; Executar(CarregarUsuarios); };
        proxima.Click += (_, _) => { pagina++; Executar(CarregarUsuarios); };
        usuarios = Estilo.Tabela(abaUsuarios, "ID", "Nome", "E-mail", "Perfil", "Situação");
        usuarios.Columns[0].FillWeight = 25;
        usuarios.Columns[1].FillWeight = 110;
        usuarios.Columns[2].FillWeight = 160;
        usuarios.Columns[3].FillWeight = 65;
        usuarios.Columns[4].FillWeight = 55;
        usuarios.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) Executar(EditarUsuario); };
        usuarios.BringToFront();

        var abaChamados = NovaAba("Chamados");
        var topoChamados = Barra(abaChamados, 105, DockStyle.Top);
        Estilo.Texto(topoChamados, "Receba e responda às solicitações dos gestores.", 10, 8, 850, 28, true);
        filtroChamados = new ComboBox { Location = new Point(10, 50), Width = 230, DropDownStyle = ComboBoxStyle.DropDownList, AccessibleName = "Filtro de chamados" };
        filtroChamados.Items.AddRange(["Todos", "Em aberto", "Meus atendimentos", "Disponíveis para atender"]);
        filtroChamados.SelectedIndex = 0;
        topoChamados.Controls.Add(filtroChamados);
        Estilo.Botao(topoChamados, "Atualizar lista", 260, 44, 170).Click += (_, _) => Executar(CarregarChamados);
        var acoesChamados = Barra(abaChamados, 65, DockStyle.Bottom);
        Estilo.Botao(acoesChamados, "Abrir conversa", 10, 12, 180).Click += (_, _) => Executar(AbrirChamado);
        chamados = Estilo.Tabela(abaChamados, "ID", "Assunto", "Solicitante", "Responsável", "Situação");
        chamados.Columns[0].FillWeight = 25;
        chamados.Columns[1].FillWeight = 150;
        chamados.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) Executar(AbrirChamado); };
        chamados.BringToFront();
        filtroChamados.SelectedIndexChanged += (_, _) => Executar(CarregarChamados);

        var abaPerfil = NovaAba("Meu perfil");
        Estilo.Texto(abaPerfil, "Dados da conta", 25, 25, 700, 35, true);
        dadosPerfil = Estilo.Texto(abaPerfil, "", 25, 80, 850, 165);
        Estilo.Botao(abaPerfil, "Editar meus dados", 25, 265, 230).Click += (_, _) => Executar(EditarPerfil);
        Estilo.Texto(abaPerfil, "O Administrador edita seus dados de contato. A troca de senha não está disponível para este perfil no site.", 25, 330, 800, 65);
        Shown += (_, _) => { Executar(CarregarPainel); atualizacao.Start(); };
        abas.SelectedIndexChanged += (_, _) => Executar(CarregarAba);
        atualizacao.Tick += (_, _) => { if (abas.SelectedIndex == 2 && ContainsFocus && !ocupado) Executar(CarregarChamados); };
        sair.Click += (_, _) => { if (!ocupado) { VoltarAoLogin = true; Close(); } };
        FormClosing += Fechar;
        FormClosed += (_, _) => atualizacao.Dispose();
    }

    private TabPage NovaAba(string titulo)
    {
        var aba = new TabPage(titulo) { BackColor = Estilo.Fundo, Padding = new Padding(15) };
        abas.TabPages.Add(aba);
        return aba;
    }

    private static Panel Barra(Control pai, int altura, DockStyle lado)
    {
        var painel = new Panel { Dock = lado, Height = altura };
        pai.Controls.Add(painel);
        return painel;
    }

    // Uma solicitação por vez evita cliques repetidos e mantém a janela responsiva.
    private async void Executar(Func<Task> tarefa)
    {
        if (ocupado) return;
        ocupado = true;
        abas.Enabled = sair.Enabled = false;
        Estilo.Aviso(mensagem, "Carregando...");
        bool voltar = false;
        try { await tarefa(); if (mensagem.Text == "Carregando...") Estilo.Aviso(mensagem, "Dados atualizados pela API da plataforma."); }
        catch (Exception erro)
        {
            Estilo.Aviso(mensagem, erro.Message, true);
            voltar = erro is ErroApi falha && falha.Status == 401;
        }
        finally { ocupado = false; abas.Enabled = sair.Enabled = true; }
        if (voltar) { VoltarAoLogin = true; Close(); }
    }

    private Task CarregarAba()
    {
        if (abas.SelectedIndex == 0) return CarregarPainel();
        if (abas.SelectedIndex == 1) return CarregarUsuarios();
        if (abas.SelectedIndex == 2) return CarregarChamados();
        return CarregarPerfil();
    }

    private async Task CarregarPainel()
    {
        var resumo = await api.Enviar<ResumoAdmin>("api/administracao/painel", HttpMethod.Get);
        totais.Text = "Usuários ativos: " + resumo.Ativos + "     |     Usuários inativos: " + resumo.Inativos;
        perfis.Rows.Clear();
        foreach (var perfil in resumo.Perfis) perfis.Rows.Add(perfil.Perfil, perfil.Quantidade);
    }

    private async Task VerificarBanco()
    {
        var resposta = await api.Enviar<RespostaApi>("api/status/banco", HttpMethod.Get);
        Estilo.Aviso(mensagem, resposta.Mensagem);
    }

    private void BuscarUsuarios()
    {
        pagina = 1;
        buscaAtual = busca.Text.Trim();
        Executar(CarregarUsuarios);
    }

    private async Task CarregarUsuarios()
    {
        var lista = await api.Enviar<PaginaUsuarios>("api/administracao/usuarios?busca=" + Uri.EscapeDataString(buscaAtual) + "&pagina=" + pagina, HttpMethod.Get);
        pagina = lista.Pagina;
        usuarios.Rows.Clear();
        foreach (var usuario in lista.Itens)
        {
            int linha = usuarios.Rows.Add(usuario.Id, usuario.Nome, usuario.Email, usuario.Perfil, usuario.Ativo ? "Ativo" : "Inativo");
            usuarios.Rows[linha].Tag = usuario;
        }
        anterior.Enabled = pagina > 1;
        proxima.Enabled = pagina < lista.TotalPaginas;
        paginaTexto.Text = "Página " + pagina + " de " + lista.TotalPaginas + " · " + lista.Total + " usuário(s)";
        if (lista.Itens.Count == 0) Estilo.Aviso(mensagem, "Nenhum usuário encontrado.");
    }

    private Usuario SelecionarUsuario()
    {
        return usuarios.CurrentRow?.Tag as Usuario ?? throw new InvalidOperationException("Selecione um usuário na lista.");
    }

    private void ConferirDialogo(DialogResult resultado)
    {
        if (resultado == DialogResult.Abort) throw new ErroApi(401, "Sua sessão expirou. Entre novamente.");
    }

    private async Task CadastrarUsuario()
    {
        using var tela = new TelaUsuario(api);
        ConferirDialogo(tela.ShowDialog(this));
        if (tela.Alterou) { pagina = 1; await CarregarUsuarios(); Estilo.Aviso(mensagem, "Conta cadastrada pela API."); }
    }

    private async Task EditarUsuario()
    {
        var selecionado = SelecionarUsuario();
        var usuario = await api.Enviar<Usuario>("api/administracao/usuarios/" + selecionado.Id, HttpMethod.Get);
        using var tela = new TelaUsuario(api, usuario);
        ConferirDialogo(tela.ShowDialog(this));
        if (!tela.Alterou) return;
        // A edição administrativa revoga as sessões da conta alterada.
        if (usuario.Id == administrador.Id) throw new ErroApi(401, "Sua conta foi alterada. Entre novamente.");
        await CarregarUsuarios();
        Estilo.Aviso(mensagem, "Dados atualizados. O perfil permanece o mesmo.");
    }

    private async Task AlterarSituacao()
    {
        var usuario = SelecionarUsuario();
        string acao = usuario.Ativo ? "Desativar" : "Ativar";
        if (MessageBox.Show(acao + " a conta de " + usuario.Nome + "?", "Situação da conta", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
        var resposta = await api.Enviar<RespostaApi>("api/administracao/usuarios/" + usuario.Id, HttpMethod.Put, new { ativo = !usuario.Ativo });
        if (usuario.Id == administrador.Id) throw new ErroApi(401, "Sua conta foi alterada. Entre novamente.");
        await CarregarUsuarios();
        Estilo.Aviso(mensagem, resposta.Mensagem);
    }

    private async Task CarregarChamados()
    {
        int selecionado = (chamados.CurrentRow?.Tag as Chamado)?.Id ?? 0;
        var lista = await api.Enviar<List<Chamado>>("api/chamados", HttpMethod.Get);
        chamados.Rows.Clear();
        foreach (var chamado in lista)
        {
            if (filtroChamados.SelectedIndex == 1 && chamado.Status == "Encerrado") continue;
            if (filtroChamados.SelectedIndex == 2 && chamado.ResponsavelId != administrador.Id && chamado.SolicitanteId != administrador.Id) continue;
            if (filtroChamados.SelectedIndex == 3 && !chamado.PodeAtender) continue;
            int linha = chamados.Rows.Add(chamado.Id, chamado.Assunto, chamado.Solicitante, chamado.Responsavel ?? "Aguardando", chamado.Status);
            chamados.Rows[linha].Tag = chamado;
            if (chamado.Id == selecionado) chamados.CurrentCell = chamados.Rows[linha].Cells[0];
        }
        if (chamados.Rows.Count == 0) Estilo.Aviso(mensagem, "Nenhum chamado neste filtro.");
    }

    private async Task AbrirChamado()
    {
        var chamado = chamados.CurrentRow?.Tag as Chamado ?? throw new InvalidOperationException("Selecione um chamado na lista.");
        using var tela = new TelaChamado(api, chamado.Id);
        ConferirDialogo(tela.ShowDialog(this));
        await CarregarChamados();
    }

    private async Task CarregarPerfil()
    {
        administrador = await api.Enviar<Usuario>("api/usuarios/perfil", HttpMethod.Get);
        nomeAdmin.Text = administrador.Nome + " · " + administrador.Perfil;
        dadosPerfil.Text = "Nome: " + administrador.Nome + "\n\nE-mail de acesso: " + administrador.Email +
            "\n\nE-mail pessoal: " + (administrador.EmailContato ?? administrador.Email) + "\n\nPerfil: " + administrador.Perfil;
    }

    private async Task EditarPerfil()
    {
        await CarregarPerfil();
        using var tela = new TelaUsuario(api, administrador, true);
        ConferirDialogo(tela.ShowDialog(this));
        if (tela.Alterou) { await CarregarPerfil(); Estilo.Aviso(mensagem, "Seus dados foram atualizados."); }
    }

    private async void Fechar(object? sender, FormClosingEventArgs e)
    {
        if (fechamentoPronto) return;
        e.Cancel = true;
        if (ocupado) return;
        atualizacao.Stop();
        ocupado = true;
        abas.Enabled = sair.Enabled = false;
        Estilo.Aviso(mensagem, "Encerrando a sessão...");
        try { await api.Sair(); }
        catch (ErroApi) { /* Sem conexão, o cookie local é descartado e a sessão do servidor expira. */ }
        fechamentoPronto = true;
        Close();
    }
}
