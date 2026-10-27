namespace TechLearnAcademy.Desktop;

public static class Estilo
{
    // Cores da folha estilo.css do site. Os controles são os do Windows Forms.
    public static readonly Color Azul = Color.FromArgb(17, 46, 72);
    public static readonly Color Verde = Color.FromArgb(8, 127, 131);
    public static readonly Color Fundo = Color.FromArgb(243, 246, 250);
    public static readonly Color TextoCor = Color.FromArgb(24, 44, 67);

    public static void Preparar(Form tela, string titulo, int largura, int altura)
    {
        tela.Text = titulo + " | TechLearn Academy";
        tela.ClientSize = new Size(largura, altura);
        tela.StartPosition = FormStartPosition.CenterScreen;
        tela.Font = new Font("Arial", 10);
        tela.BackColor = Fundo;
        tela.ForeColor = TextoCor;
        tela.AutoScaleMode = AutoScaleMode.Dpi;
    }

    public static Label Texto(Control pai, string texto, int x, int y, int largura, int altura = 28, bool negrito = false)
    {
        var label = new Label { Text = texto, AutoSize = false, Location = new Point(x, y), Size = new Size(largura, altura) };
        if (negrito) label.Font = new Font(pai.Font, FontStyle.Bold);
        pai.Controls.Add(label);
        return label;
    }

    public static TextBox Campo(Control pai, string rotulo, int y, bool senha = false, bool mostrar = true)
    {
        Texto(pai, rotulo, 28, y, 490, 24, true).Visible = mostrar;
        var campo = new TextBox
        {
            Location = new Point(28, y + 26), Width = 490, UseSystemPasswordChar = senha,
            AccessibleName = rotulo, MaxLength = 0, TabIndex = pai.Controls.Count, Visible = mostrar
        };
        HabilitarColagem(campo);
        pai.Controls.Add(campo);
        return campo;
    }

    public static void HabilitarColagem(TextBox campo)
    {
        campo.ShortcutsEnabled = true;
        // Ctrl+V e Shift+Insert usam a mesma ação do menu Colar.
        campo.KeyDown += (_, e) =>
        {
            if ((e.Control && e.KeyCode == Keys.V) || (e.Shift && e.KeyCode == Keys.Insert))
            {
                e.SuppressKeyPress = true;
                Colar(campo);
            }
        };
        var menu = new ContextMenuStrip();
        var desfazer = menu.Items.Add("Desfazer", null, (_, _) => campo.Undo());
        var recortar = menu.Items.Add("Recortar", null, (_, _) => campo.Cut());
        var copiar = menu.Items.Add("Copiar", null, (_, _) => campo.Copy());
        var colar = menu.Items.Add("Colar", null, (_, _) => Colar(campo));
        menu.Items.Add("Selecionar tudo", null, (_, _) => campo.SelectAll());
        menu.Opening += (_, _) =>
        {
            desfazer.Enabled = !campo.ReadOnly && campo.CanUndo;
            recortar.Enabled = !campo.ReadOnly && !campo.UseSystemPasswordChar && campo.SelectionLength > 0;
            copiar.Enabled = !campo.UseSystemPasswordChar && campo.SelectionLength > 0;
            colar.Enabled = !campo.ReadOnly;
        };
        campo.ContextMenuStrip = menu;
        campo.Disposed += (_, _) => menu.Dispose();
    }

    private static void Colar(TextBox campo)
    {
        if (campo.ReadOnly || !campo.Enabled) return;
        try
        {
            string texto = Clipboard.GetText();
            if (texto.Length == 0) return;
            if (campo.MaxLength > 0 && campo.TextLength - campo.SelectionLength + texto.Length > campo.MaxLength)
            {
                MessageBox.Show("O texto colado ultrapassa o limite deste campo.", "Colar texto");
                return;
            }
            // Substitui a seleção sem modificar os caracteres copiados.
            campo.SelectedText = texto;
        }
        catch (System.Runtime.InteropServices.ExternalException)
        {
            MessageBox.Show("Não foi possível colar. Copie o texto e tente novamente.", "Colar texto");
        }
    }

    public static Button Botao(Control pai, string texto, int x, int y, int largura = 160, bool secundario = false)
    {
        var botao = new Button
        {
            Text = texto, Location = new Point(x, y), Size = new Size(largura, 38),
            BackColor = secundario ? Color.White : Verde, ForeColor = secundario ? TextoCor : Color.White,
            FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, TabIndex = pai.Controls.Count,
            Font = new Font(pai.Font, FontStyle.Bold)
        };
        botao.FlatAppearance.BorderColor = secundario ? Color.FromArgb(186, 202, 214) : Verde;
        pai.Controls.Add(botao);
        return botao;
    }

    public static DataGridView Tabela(Control pai, params string[] colunas)
    {
        var tabela = new DataGridView
        {
            Dock = DockStyle.Fill, ReadOnly = true, MultiSelect = false, AllowUserToAddRows = false,
            AllowUserToDeleteRows = false, AllowUserToResizeRows = false, RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White, BorderStyle = BorderStyle.FixedSingle, EnableHeadersVisualStyles = false,
            AccessibleName = "Lista de registros", AutoGenerateColumns = false
        };
        tabela.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(237, 243, 247);
        tabela.ColumnHeadersDefaultCellStyle.ForeColor = TextoCor;
        tabela.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(237, 243, 247);
        tabela.ColumnHeadersDefaultCellStyle.SelectionForeColor = TextoCor;
        tabela.ColumnHeadersDefaultCellStyle.Font = new Font(pai.Font, FontStyle.Bold);
        tabela.ColumnHeadersHeight = 38;
        tabela.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        tabela.DefaultCellStyle.SelectionBackColor = Color.FromArgb(208, 237, 232);
        tabela.DefaultCellStyle.SelectionForeColor = TextoCor;
        tabela.DefaultCellStyle.Padding = new Padding(5);
        tabela.RowTemplate.Height = 36;
        foreach (string coluna in colunas) tabela.Columns.Add(coluna, coluna);
        pai.Controls.Add(tabela);
        return tabela;
    }

    public static void Aviso(Label label, string texto, bool erro = false)
    {
        label.Text = texto;
        label.ForeColor = erro ? Color.FromArgb(158, 37, 37) : TextoCor;
        label.AccessibleName = texto;
    }
}
