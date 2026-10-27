using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

string raiz = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
string origem = @"C:\Users\rapha\source\repos\TechLearnAcademy";
string banco = "TechLearnMobile_Teste_" + Guid.NewGuid().ToString("N")[..12];
string pasta = Path.Combine(raiz, ".local", banco);
string conexao = @"Server=(localdb)\MSSQLLocalDB;Database=master;Integrated Security=true;Encrypt=true;TrustServerCertificate=true";
string senha = "A1!" + Guid.NewGuid().ToString("N");
string senhaCertificado = Guid.NewGuid().ToString("N");
Process? servidor = null;
Directory.CreateDirectory(pasta);

try
{
    using var sql = new SqlConnection(conexao);
    await sql.OpenAsync();
    string sqlScript = File.ReadAllText(Path.Combine(origem, "Banco/Instalar-banco-do-site.sql")).Replace("TechLearnAcademy_PIMIV", banco);
    string criacao = "CREATE DATABASE " + banco + " COLLATE Latin1_General_100_CI_AS_SC;";
    string mdf = Path.Combine(pasta, "dados.mdf").Replace("'", "''");
    string ldf = Path.Combine(pasta, "dados_log.ldf").Replace("'", "''");
    sqlScript = sqlScript.Replace(criacao, $"CREATE DATABASE [{banco}] ON PRIMARY (NAME=N'dados',FILENAME=N'{mdf}') LOG ON (NAME=N'dados_log',FILENAME=N'{ldf}') COLLATE Latin1_General_100_CI_AS_SC;");
    sqlScript += "\nGO\n" + File.ReadAllText(Path.Combine(origem, "Banco/07-corrigir-recuperacao-e-avaliacoes.sql"));
    foreach (string lote in Regex.Split(sqlScript, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase))
        if (!string.IsNullOrWhiteSpace(lote)) { using var comando = new SqlCommand(lote, sql) { CommandTimeout = 90 }; await comando.ExecuteNonQueryAsync(); }
    byte[] sal = RandomNumberGenerator.GetBytes(16);
    string hash = "600000." + Convert.ToBase64String(sal) + "." + Convert.ToBase64String(Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(senha), sal, 600000, HashAlgorithmName.SHA256, 32));
    foreach (string perfil in new[] { "Administrador", "Aluno", "Mentor", "Gestor" })
    {
        using var comando = new SqlCommand("INSERT INTO Usuarios(Nome,Email,EmailContato,SenhaHash,Perfil) VALUES(@nome,@email,@email,@hash,@perfil)", sql);
        comando.Parameters.AddWithValue("@nome", perfil == "Aluno" ? "Aluno Teste" : perfil + " Teste");
        comando.Parameters.AddWithValue("@email", perfil.ToLowerInvariant() + "@mobile.test");
        comando.Parameters.AddWithValue("@hash", hash);
        comando.Parameters.AddWithValue("@perfil", perfil);
        await comando.ExecuteNonQueryAsync();
    }

    using var chave = RSA.Create(2048);
    var pedido = new CertificateRequest("CN=localhost", chave, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    var nomes = new SubjectAlternativeNameBuilder();
    nomes.AddDnsName("localhost");
    pedido.CertificateExtensions.Add(nomes.Build());
    pedido.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
    pedido.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, false));
    using var certificado = pedido.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-5), DateTimeOffset.UtcNow.AddDays(1));
    string pfx = Path.Combine(pasta, "servidor.pfx");
    string cer = Path.Combine(pasta, "servidor.cer");
    string trust = Path.Combine(pasta, "truststore");
    File.WriteAllBytes(pfx, certificado.Export(X509ContentType.Pfx, senhaCertificado));
    File.WriteAllBytes(cer, certificado.Export(X509ContentType.Cert));
    var importar = new ProcessStartInfo(@"C:\Program Files\Android\openjdk\jdk-21.0.8\bin\keytool.exe") { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };
    foreach (string argumento in new[] { "-importcert", "-noprompt", "-alias", "api-local-teste", "-file", cer, "-keystore", trust, "-storepass", senhaCertificado }) importar.ArgumentList.Add(argumento);
    using (var processo = Process.Start(importar)!) { await Task.WhenAll(processo.StandardOutput.ReadToEndAsync(), processo.StandardError.ReadToEndAsync()); await processo.WaitForExitAsync(); if (processo.ExitCode != 0) throw new Exception("Não foi possível preparar o certificado de teste."); }

    string binario = Path.Combine(raiz, ".local", "servidor-api");
    var inicio = new ProcessStartInfo("dotnet") { WorkingDirectory = binario, UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };
    inicio.ArgumentList.Add(Path.Combine(binario, "TechLearnAcademy.Api.dll"));
    inicio.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
    inicio.Environment["ASPNETCORE_URLS"] = "https://localhost:7346";
    inicio.Environment["ASPNETCORE_Kestrel__Certificates__Default__Path"] = pfx;
    inicio.Environment["ASPNETCORE_Kestrel__Certificates__Default__Password"] = senhaCertificado;
    inicio.Environment["ConnectionStrings__Banco"] = conexao.Replace("Database=master", "Database=" + banco);
    servidor = Process.Start(inicio) ?? throw new Exception("Não foi possível iniciar a API local.");
    var logServidor = servidor.StandardOutput.ReadToEndAsync();
    var erroServidor = servidor.StandardError.ReadToEndAsync();
    bool pronto = false;
    for (int i = 0; i < 40; i++)
    {
        try { using var rede = new System.Net.Sockets.TcpClient(); await rede.ConnectAsync("localhost", 7346); pronto = true; break; }
        catch { if (servidor.HasExited) break; await Task.Delay(500); }
    }
    if (!pronto) throw new Exception("A API de teste não iniciou.");
    var java = new ProcessStartInfo(@"C:\Program Files\Android\openjdk\jdk-21.0.8\bin\java.exe") { WorkingDirectory = raiz, UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };
    foreach (string argumento in new[] { "-Djavax.net.ssl.trustStore=" + trust, "-Djavax.net.ssl.trustStorePassword=" + senhaCertificado, "-Dfile.encoding=UTF-8", "-cp", Path.Combine(raiz, "Testes/Integracao/classes") + ";" + Path.Combine(raiz, ".local/testes-java/json.jar"), "VerificarApiJava" }) java.ArgumentList.Add(argumento);
    java.Environment["SENHA_TESTE"] = senha;
    using var cliente = Process.Start(java)!;
    var saida = cliente.StandardOutput.ReadToEndAsync();
    var erro = cliente.StandardError.ReadToEndAsync();
    await cliente.WaitForExitAsync();
    string resultado = await saida;
    Console.Write(resultado);
    File.WriteAllText(Path.Combine(raiz, "Testes/Integracao/resultado.txt"), resultado);
    if (cliente.ExitCode != 0) throw new Exception(await erro);
    Console.WriteLine("Integração concluída somente no banco descartável local.");
}
finally
{
    if (servidor != null && !servidor.HasExited) { servidor.Kill(true); await servidor.WaitForExitAsync(); }
    SqlConnection.ClearAllPools();
    if (!Regex.IsMatch(banco, "^TechLearnMobile_Teste_[a-f0-9]{12}$")) throw new Exception("Nome de banco de teste inválido.");
    using var sql = new SqlConnection(conexao);
    await sql.OpenAsync();
    using var limpar = new SqlCommand($"IF DB_ID('{banco}') IS NOT NULL BEGIN ALTER DATABASE [{banco}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{banco}]; END", sql);
    await limpar.ExecuteNonQueryAsync();
    if (pasta.StartsWith(Path.Combine(raiz, ".local", "TechLearnMobile_Teste_"), StringComparison.OrdinalIgnoreCase)) Directory.Delete(pasta, true);
    Console.WriteLine("Banco descartável, processo e certificado de teste removidos.");
}
