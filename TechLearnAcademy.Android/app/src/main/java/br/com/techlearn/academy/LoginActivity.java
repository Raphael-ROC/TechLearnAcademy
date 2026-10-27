package br.com.techlearn.academy;

import android.os.Bundle;
import android.view.View;
import android.widget.*;
import android.text.InputType;
import org.json.JSONObject;

public class LoginActivity extends TelaBase {
    private EditText email, senha;
    private Button entrar;

    @Override protected boolean publica() { return true; }

    @Override public void onCreate(Bundle estado) {
        super.onCreate(estado); if (isFinishing()) return;
        preparar("Acesso à plataforma");
        texto(corpo, "Plataforma de cursos", 19, true);
        texto(corpo, "Entre com a mesma conta utilizada no site.", 16, false);
        email = campo(corpo, "E-mail", InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_VARIATION_EMAIL_ADDRESS);
        senha = campo(corpo, "Senha", InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_VARIATION_PASSWORD);
        entrar = botao(corpo, "Entrar", "entrar");
        botao(corpo, "Criar conta de aluno", "cadastro");
        botao(corpo, "Esqueci minha senha", "recuperar");
    }

    @Override public void onClick(View tela) {
        if (ocupado) return;
        try {
            String acao = acao(tela);
            if (acao.equals("cadastro")) { abrir(CadastroActivity.class); return; }
            if (acao.equals("recuperar")) { abrir(RecuperarActivity.class); return; }
            Validacao.email(valor(email).trim());
            Validacao.exigir(!valor(senha).isEmpty(), "Informe a senha.");
            JSONObject dados = new JSONObject();
            dados.put("email", valor(email).trim());
            dados.put("senha", valor(senha));
            pedir("POST", "api/usuarios/login", dados, "login");
            entrar.setText("Entrando...");
            aviso("Entrando...");
        } catch (Exception falha) { erro(falha); }
    }

    @Override protected void receber(String operacao, String resposta) throws Exception {
        JSONObject usuario = new JSONObject(resposta);
        if (operacao.equals("login")) {
            senha.setText("");
            String perfil = usuario.optString("perfil");
            Validacao.exigir(perfil.equals("Aluno") || perfil.equals("Mentor") || perfil.equals("Gestor") || perfil.equals("Administrador"), "Perfil de usuário não reconhecido.");
            Validacao.exigir(!usuario.optString("token").isEmpty(), "A API não devolveu uma sessão válida.");
            Sessao.token = usuario.getString("token");
            // Confere os dados da conta na rota protegida.
            pedir("GET", "api/usuarios/perfil", null, "perfil");
        } else {
            Sessao.usuario = usuario;
            aviso("Login realizado com sucesso.");
            abrir(PainelActivity.class);
            finish();
        }
    }

    @Override protected void falhou(Exception falha) {
        senha.setText("");
        entrar.setText("Entrar");
        Sessao.limpar();
        super.falhou(falha);
    }
}
