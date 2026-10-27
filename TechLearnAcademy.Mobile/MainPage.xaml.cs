namespace TechLearnAcademy.Mobile;

public partial class MainPage : ContentPage
{
    private string servidor = "";

    public MainPage() { InitializeComponent(); }

    private void AbrirPlataforma(object? sender, EventArgs e)
    {
        string endereco = (Endereco.Text ?? "").Trim();
        if (!Uri.TryCreate(endereco, UriKind.Absolute, out Uri? url) ||
            url.Scheme != "https" || url.UserInfo != "")
        {
            Mensagem.Text = "Informe um endereço HTTPS válido.";
            return;
        }

        // As telas usam a mesma API do site.
        servidor = url.GetLeftPart(UriPartial.Authority);
        Mensagem.Text = "Abrindo...";
        Navegador.Source = endereco;
    }

    private async void PaginaAbrindo(object? sender, WebNavigatingEventArgs e)
    {
        if (!Uri.TryCreate(e.Url, UriKind.Absolute, out Uri? url))
            return;
        if ((url.Scheme != "https" && url.Scheme != "http") || url.UserInfo != "")
        {
            e.Cancel = true;
            Mensagem.Text = "Este endereço de material não é permitido.";
            return;
        }

        // Materiais externos abrem no leitor do celular.
        if (url.GetLeftPart(UriPartial.Authority) != servidor ||
            url.AbsolutePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            e.Cancel = true;
            try { await Launcher.Default.OpenAsync(url); }
            catch { Mensagem.Text = "Não foi possível abrir este material."; }
        }
    }

    private void PaginaCarregada(object? sender, WebNavigatedEventArgs e)
    {
        Mensagem.Text = e.Result == WebNavigationResult.Success
            ? "Plataforma carregada."
            : "Não foi possível abrir. Confira servidor, rede e certificado HTTPS.";
    }
}

