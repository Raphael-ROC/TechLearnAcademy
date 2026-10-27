package br.com.techlearn.academy;

import org.json.JSONObject;

public class Sessao {
    // O token verdadeiro vem da API e fica somente na memória.
    public static String token = "";
    public static JSONObject usuario = new JSONObject();

    public static boolean perfil(String nome) {
        return usuario.optString("perfil").equals(nome);
    }

    public static void limpar() {
        token = "";
        usuario = new JSONObject();
    }
}
