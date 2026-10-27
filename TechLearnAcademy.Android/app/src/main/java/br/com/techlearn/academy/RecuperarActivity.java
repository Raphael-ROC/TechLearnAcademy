package br.com.techlearn.academy;

import android.os.Bundle;
import android.view.View;
import android.widget.EditText;
import android.text.InputType;
import org.json.JSONObject;

public class RecuperarActivity extends TelaBase {
    private EditText email, token, senha, confirmacao;
    @Override protected boolean publica() { return true; }

    @Override public void onCreate(Bundle estado) {
        super.onCreate(estado); if (isFinishing()) return;
        preparar("Recuperar senha");
        texto(corpo, "Aluno, Mentor e Gestor recebem a recuperação no e-mail de contato. O Administrador segue a regra do site e não usa esta função.", 16, false);
        email = campo(corpo, "E-mail", InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_VARIATION_EMAIL_ADDRESS);
        botao(corpo, "Enviar recuperação", "enviar");
        texto(corpo, "Já recebeu o link? Copie o código que aparece depois de token= para definir a nova senha.", 15, false);
        token = campo(corpo, "Código recebido por e-mail", InputType.TYPE_CLASS_TEXT);
        senha = campo(corpo, "Nova senha", InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_VARIATION_PASSWORD);
        confirmacao = campo(corpo, "Confirmar nova senha", InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_VARIATION_PASSWORD);
        texto(corpo, "Mínimo de 8 caracteres, maiúscula, número e caractere especial. Sem espaços.", 15, false);
        botao(corpo, "Definir nova senha", "redefinir");
        botao(corpo, "Voltar", "voltar");
    }

    @Override public void onClick(View tela) {
        if (ocupado) return;
        if (acao(tela).equals("voltar")) { finish(); return; }
        try {
            JSONObject dados = new JSONObject();
            if (acao(tela).equals("enviar")) {
                Validacao.email(valor(email).trim());
                dados.put("email", valor(email).trim());
                pedir("POST", "api/usuarios/recuperar-senha", dados, "enviar");
            } else {
                Validacao.exigir(!valor(token).trim().isEmpty(), "Informe o código recebido por e-mail.");
                Validacao.senha(valor(senha), valor(confirmacao));
                dados.put("token", valor(token).trim());
                dados.put("senha", valor(senha));
                dados.put("confirmarSenha", valor(confirmacao));
                pedir("POST", "api/usuarios/redefinir-senha", dados, "redefinir");
            }
        } catch (Exception falha) { erro(falha); }
    }

    @Override protected void receber(String operacao, String resposta) throws Exception {
        aviso(new JSONObject(resposta).getString("mensagem"));
        if (operacao.equals("redefinir")) { senha.setText(""); confirmacao.setText(""); token.setText(""); }
    }
}
