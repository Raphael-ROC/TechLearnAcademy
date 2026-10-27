package br.com.techlearn.academy;

import android.os.Bundle;
import android.view.View;
import android.widget.EditText;
import android.text.InputType;
import org.json.JSONObject;

public class CadastroActivity extends TelaBase {
    private EditText nome, email, senha, confirmacao;
    @Override protected boolean publica() { return true; }

    @Override public void onCreate(Bundle estado) {
        super.onCreate(estado); if (isFinishing()) return;
        preparar("Criar conta de aluno");
        nome = campo(corpo, "Nome completo", InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_FLAG_CAP_WORDS);
        email = campo(corpo, "E-mail", InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_VARIATION_EMAIL_ADDRESS);
        senha = campo(corpo, "Senha", InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_VARIATION_PASSWORD);
        confirmacao = campo(corpo, "Confirmar senha", InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_VARIATION_PASSWORD);
        texto(corpo, "Mínimo de 8 caracteres, uma letra maiúscula, um número e um caractere especial. Sem espaços.", 15, false);
        botao(corpo, "Criar conta", "salvar");
        botao(corpo, "Voltar para o login", "voltar");
    }

    @Override public void onClick(View tela) {
        if (ocupado) return;
        if (acao(tela).equals("voltar")) { finish(); return; }
        try {
            Validacao.nome(valor(nome).trim());
            Validacao.email(valor(email).trim());
            Validacao.senha(valor(senha), valor(confirmacao));
            JSONObject dados = new JSONObject();
            dados.put("nome", valor(nome).trim());
            dados.put("email", valor(email).trim());
            dados.put("senha", valor(senha));
            dados.put("confirmarSenha", valor(confirmacao));
            pedir("POST", "api/usuarios/cadastro", dados, "cadastro");
        } catch (Exception falha) { erro(falha); }
    }

    @Override protected void receber(String operacao, String resposta) throws Exception {
        aviso(new JSONObject(resposta).getString("mensagem"));
        nome.setText(""); email.setText(""); senha.setText(""); confirmacao.setText("");
    }
}
