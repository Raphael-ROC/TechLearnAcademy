using Microsoft.Data.SqlClient;
using System.Data;
using TechLearnAcademy.Api.Servicos;

namespace TechLearnAcademy.Api.Banco;

public class ConexaoBanco(IConfiguration configuracao)
{
    public async Task<SqlConnection> Abrir()
    {
        string conexao = configuracao.GetConnectionString("Banco") ?? "";
        if (string.IsNullOrWhiteSpace(conexao))
            throw new ErroAplicacao(503, "Configure a conexão com o SQL Server antes de usar esta função.");
        var banco = new SqlConnection(conexao);
        try { await banco.OpenAsync(); return banco; }
        catch { await banco.DisposeAsync(); throw; }
    }

    // Valores recebidos são parâmetros: nunca são concatenados ao SQL.
    public static SqlCommand Comando(SqlConnection banco, string sql, object? dados = null, SqlTransaction? transacao = null)
    {
        var comando = new SqlCommand(sql, banco, transacao);
        if (dados != null)
        {
            foreach (var campo in dados.GetType().GetProperties())
                comando.Parameters.AddWithValue("@" + campo.Name, campo.GetValue(dados) ?? DBNull.Value);
        }
        return comando;
    }

    public static async Task<List<Dictionary<string, object?>>> Consultar(SqlConnection banco, string sql, object? dados = null, SqlTransaction? transacao = null)
    {
        using var comando = Comando(banco, sql, dados, transacao);
        using var leitor = await comando.ExecuteReaderAsync();
        var lista = new List<Dictionary<string, object?>>();
        while (await leitor.ReadAsync())
        {
            var linha = new Dictionary<string, object?>();
            for (int i = 0; i < leitor.FieldCount; i++)
                linha[leitor.GetName(i)] = leitor.IsDBNull(i) ? null : leitor.GetValue(i);
            lista.Add(linha);
        }
        return lista;
    }

    public static async Task<int> Gravar(SqlConnection banco, string sql, object? dados = null, SqlTransaction? transacao = null)
    {
        using var comando = Comando(banco, sql, dados, transacao);
        return await comando.ExecuteNonQueryAsync();
    }

    public static async Task<int> Numero(SqlConnection banco, string sql, object? dados = null, SqlTransaction? transacao = null)
    {
        using var comando = Comando(banco, sql, dados, transacao);
        return Convert.ToInt32(await comando.ExecuteScalarAsync());
    }
}
