using System.Globalization;

namespace TechLearnAcademy.Desktop;

public class TelaChamado : Form
{
    private readonly ApiCliente api;
    private readonly int id;
    private readonly Label titulo;
    private readonly Label detalhes;
    private readonly Label mensagem;
    private readonly RichTextBox conversa;
    private readonly TextBox resposta;
    private readonly Button atender;
    private readonly Button enviar;
    private readonly Button encerrar;
    private readonly System.Windows.Forms.Timer atualizacao = new() { Interval = 5000 };
    private Chamado? chamado;
    private bool ocupado;

    public TelaChamado(ApiCliente api, int id)
    {
        this.api = api;
        this.id = id;
        Estilo.Preparar(this, "Atendimento do chamado", 820, 660);
        MinimumSize = new Size(750, 630);
        titulo = Estilo.Texto(this, "Carregando chamado...", 22, 18, 760, 35, true);
        titulo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        detalhes = Estilo.Texto(this, "", 22, 60, 760, 65);
        detalhes.Anchor = titulo.Anchor;
        conversa = new RichTextBox { Location = new Point(22, 130), Size = new Size(770, 265), ReadOnly = true,
            BackColor = Color.White, AccessibleName = "Histórico da conversa", Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right };
        Controls.Add(conversa);
        var rodape = new Panel { Size = new Size(ClientSize.Width, 250), Dock = DockStyle.Bottom };
        Estilo.Texto(rodape, "Sua mensagem", 22, 5, 700, 24, true);
        resposta = new TextBox { Location = new Point(22, 30), Size = new Size(770, 60), Multiline = true, MaxLength = 4000,
            ScrollBars = ScrollBars.Vertical, AccessibleName = "Sua mensagem", Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
        Estilo.HabilitarColagem(resposta);
        rodape.Controls.Add(resposta);
        atender = Estilo.Botao(rodape, "Atender chamado", 22, 106, 180);
        enviar = Estilo.Botao(rodape, "Enviar mensagem", 212, 106, 180);
        encerrar = Estilo.Botao(rodape, "Encerrar", 402, 106, 135, true);
        var atualizar = Estilo.Botao(rodape, "Atualizar", 547, 106, 135, true);
        mensagem = Estilo.Texto(rodape, "", 22, 160, 760, 75);
        mensagem.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        Controls.Add(rodape);
        atender.Click += (_, _) => Executar(async () => { await api.Enviar<RespostaApi>("api/chamados/" + id + "/assumir", HttpMethod.Post); });
        enviar.Click += (_, _) => Executar(EnviarMensagem);
        encerrar.Click += (_, _) => Executar(Encerrar);
        atualizar.Click += (_, _) => Executar(() => Task.CompletedTask);
        Shown += (_, _) => { Executar(() => Task.CompletedTask); atualizacao.Start(); };
        // A atualização mantém intacta a mensagem que está sendo digitada.
        atualizacao.Tick += (_, _) => { if (ContainsFocus && !ocupado) Executar(() => Task.CompletedTask, true); };
        FormClosing += (_, e) => { if (ocupado) e.Cancel = true; };
        FormClosed += (_, _) => atualizacao.Dispose();
        LiberarAcoes();
    }

    private void LiberarAcoes(bool manterDigitacao = false)
    {
        atender.Enabled = !ocupado && chamado?.PodeAtender == true;
        enviar.Enabled = !ocupado && chamado?.PodeResponder == true;
        if (!manterDigitacao) resposta.Enabled = !ocupado && chamado?.PodeResponder == true;
        encerrar.Enabled = !ocupado && chamado?.PodeEncerrar == true;
    }

    private async void Executar(Func<Task> tarefa, bool automatico = false)
    {
        if (ocupado) return;
        ocupado = true;
        LiberarAcoes(automatico);
        bool sessaoVencida = false;
        try
        {
            await tarefa();
            chamado = await api.Enviar<Chamado>("api/chamados/" + id, HttpMethod.Get);
            titulo.Text = "#" + id + " · " + chamado.Assunto;
            detalhes.Text = "Solicitante: " + chamado.Solicitante + "\nResponsável: " + (chamado.Responsavel ?? "Aguardando atendimento") + " · " + chamado.Status;
            string historico = "Descrição do chamado\n" + chamado.Descricao + "\n\n";
            foreach (var item in chamado.Mensagens)
            {
                DateTime data = item.Data.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(item.Data, DateTimeKind.Utc) : item.Data;
                historico += item.Nome + " · " + item.Perfil + " · " + data.ToLocalTime().ToString("g", CultureInfo.GetCultureInfo("pt-BR")) + "\n" + item.Texto + "\n\n";
            }
            if (chamado.Mensagens.Count == 0) historico += "Ainda não há respostas.\n";
            if (conversa.Text != historico)
            {
                conversa.Text = historico;
                conversa.SelectionStart = conversa.TextLength;
                conversa.ScrollToCaret();
            }
            if (!automatico || mensagem.ForeColor != Color.FromArgb(158, 37, 37))
                Estilo.Aviso(mensagem, chamado.Status == "Encerrado" ? "Chamado encerrado. A conversa permanece no histórico." :
                    chamado.PodeAtender ? "Clique em Atender chamado para assumir esta conversa." : "Conversa atualizada pela API.");
        }
        catch (Exception erro)
        {
            Estilo.Aviso(mensagem, erro.Message, true);
            sessaoVencida = erro is ErroApi falha && falha.Status == 401;
        }
        finally { ocupado = false; LiberarAcoes(); }
        if (sessaoVencida) { atualizacao.Stop(); DialogResult = DialogResult.Abort; }
    }

    private async Task EnviarMensagem()
    {
        string texto = resposta.Text.Trim();
        if (texto.Length == 0) throw new InvalidOperationException("Escreva uma mensagem antes de enviar.");
        await api.Enviar<RespostaApi>("api/chamados/" + id + "/mensagens", HttpMethod.Post, new { texto });
        resposta.Clear();
    }

    private async Task Encerrar()
    {
        if (MessageBox.Show("Encerrar este atendimento? A conversa será mantida no histórico.", "Encerrar chamado",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            await api.Enviar<RespostaApi>("api/chamados/" + id + "/encerrar", HttpMethod.Post);
    }
}
