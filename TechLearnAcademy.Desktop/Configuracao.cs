namespace TechLearnAcademy.Desktop;

public static class Configuracao
{
    public const string EnderecoAzure = "https://techlearn-proj-pim-hyayf7cehyd3edfq.westus3-01.azurewebsites.net/";

    public static Uri LerEndereco()
    {
        // O arquivo contém somente o endereço público da API.
        string arquivo = Path.Combine(AppContext.BaseDirectory, "servidor.txt");
        string endereco = File.Exists(arquivo) ? File.ReadAllText(arquivo).Trim() : EnderecoAzure;
        return ValidarEndereco(endereco);
    }

    public static Uri ValidarEndereco(string endereco)
    {
        if (!Uri.TryCreate(endereco, UriKind.Absolute, out Uri? url) || url.Scheme != "https" ||
            url.UserInfo != "" || url.Query != "" || url.Fragment != "" || url.AbsolutePath != "/")
            throw new InvalidOperationException("Informe em servidor.txt somente a raiz HTTPS da API, sem usuário, senha ou caminho.");
        return url;
    }
}
