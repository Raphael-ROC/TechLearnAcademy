package br.com.techlearn.academy;

import org.json.JSONObject;
import java.io.ByteArrayOutputStream;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.HttpURLConnection;
import java.net.URL;
import java.net.SocketTimeoutException;

public class Api {
    public static final String ENDERECO = "https://techlearn-proj-pim-hyayf7cehyd3edfq.westus3-01.azurewebsites.net/";

    public static class Erro extends Exception {
        public final int status;
        public Erro(int status, String mensagem) {
            super(mensagem);
            this.status = status;
        }
    }

    private static HttpURLConnection abrir(String metodo, String caminho) throws Exception {
        if (!caminho.startsWith("api/"))
            throw new Erro(0, "Caminho de solicitação inválido.");
        HttpURLConnection conexao = (HttpURLConnection) new URL(ENDERECO + caminho).openConnection();
        conexao.setRequestMethod(metodo);
        conexao.setConnectTimeout(90000);
        conexao.setReadTimeout(90000);
        conexao.setInstanceFollowRedirects(false);
        conexao.setRequestProperty("Accept", "application/json");
        // O servidor verifica a sessão e as permissões em cada solicitação.
        if (!Sessao.token.isEmpty())
            conexao.setRequestProperty("Authorization", "Bearer " + Sessao.token);
        return conexao;
    }

    public static String enviar(String metodo, String caminho, JSONObject dados) throws Exception {
        HttpURLConnection conexao = null;
        try {
            conexao = abrir(metodo, caminho);
            if (dados != null) {
                conexao.setDoOutput(true);
                conexao.setRequestProperty("Content-Type", "application/json; charset=utf-8");
                try (OutputStream saida = conexao.getOutputStream()) {
                    saida.write(dados.toString().getBytes("UTF-8"));
                }
            }
            return new String(resposta(conexao), "UTF-8");
        } catch (Erro erro) { throw erro; }
        catch (SocketTimeoutException erro) {
            throw new Erro(0, "O servidor demorou para responder. Tente novamente.");
        } catch (java.io.IOException erro) {
            throw new Erro(0, "Não foi possível conectar ao servidor. Confira sua internet e a disponibilidade da API.");
        } finally { if (conexao != null) conexao.disconnect(); }
    }

    public static byte[] baixar(String caminho) throws Exception {
        HttpURLConnection conexao = abrir("GET", caminho);
        try { return resposta(conexao); }
        finally { conexao.disconnect(); }
    }

    public static String enviarPdf(String metodo, String caminho, JSONObject dados, byte[] arquivo) throws Exception {
        HttpURLConnection conexao = abrir(metodo, caminho);
        String limite = "TechLearn" + java.util.UUID.randomUUID();
        try {
            conexao.setDoOutput(true);
            conexao.setRequestProperty("Content-Type", "multipart/form-data; boundary=" + limite);
            try (OutputStream saida = conexao.getOutputStream()) {
                java.util.Iterator<String> campos = dados.keys();
                while (campos.hasNext()) {
                    String nome = campos.next();
                    String parte = "--" + limite + "\r\nContent-Disposition: form-data; name=\"" + nome + "\"\r\n\r\n" + dados.get(nome) + "\r\n";
                    saida.write(parte.getBytes("UTF-8"));
                }
                saida.write(("--" + limite + "\r\nContent-Disposition: form-data; name=\"arquivo\"; filename=\"material.pdf\"\r\nContent-Type: application/pdf\r\n\r\n").getBytes("UTF-8"));
                saida.write(arquivo);
                saida.write(("\r\n--" + limite + "--\r\n").getBytes("UTF-8"));
            }
            return new String(resposta(conexao), "UTF-8");
        } finally { conexao.disconnect(); }
    }

    public static byte[] ler(InputStream entrada, int limite) throws Exception {
        try (InputStream fluxo = entrada; ByteArrayOutputStream saida = new ByteArrayOutputStream()) {
            byte[] parte = new byte[8192];
            int quantidade;
            while ((quantidade = fluxo.read(parte)) != -1) {
                if (saida.size() + quantidade > limite)
                    throw new Erro(0, "O arquivo ultrapassa o tamanho permitido.");
                saida.write(parte, 0, quantidade);
            }
            return saida.toByteArray();
        }
    }

    private static byte[] resposta(HttpURLConnection conexao) throws Exception {
        int status = conexao.getResponseCode();
        InputStream entrada = status >= 200 && status < 300 ? conexao.getInputStream() : conexao.getErrorStream();
        byte[] bytes = entrada == null ? new byte[0] : ler(entrada, 12 * 1024 * 1024);
        if (status < 200 || status >= 300) {
            String mensagem = "Não foi possível concluir a operação (HTTP " + status + ").";
            try { mensagem = new JSONObject(new String(bytes, "UTF-8")).optString("mensagem", mensagem); }
            catch (Exception ignorado) { /* A hospedagem pode devolver uma página de erro. */ }
            throw new Erro(status, mensagem);
        }
        return bytes;
    }
}
