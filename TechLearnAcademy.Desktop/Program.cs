namespace TechLearnAcademy.Desktop;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        try
        {
            using var api = new ApiCliente(Configuracao.LerEndereco());
            // Sair volta ao login. Fechar a janela encerra o aplicativo.
            while (true)
            {
                using var login = new TelaLogin(api);
                if (login.ShowDialog() != DialogResult.OK) break;
                using var principal = new TelaPrincipal(api, login.Usuario!);
                principal.ShowDialog();
                if (!principal.VoltarAoLogin) break;
            }
        }
        catch (Exception erro)
        {
            MessageBox.Show(erro.Message, "TechLearn Academy", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
