package br.com.techlearn.academy;

import android.os.Bundle;
import android.view.View;
import android.widget.EditText;
import android.text.InputType;
import org.json.JSONObject;

public class PerfilActivity extends TelaBase {
    private EditText nome, email, contato, atual, senha, confirmacao;
    private JSONObject usuario;

    @Override public void onCreate(Bundle estado) {
        super.onCreate(estado); if (isFinishing()) return;
        preparar("Meu perfil");
        pedir("GET", "api/usuarios/perfil", null, "perfil");
    }

    @Override protected void receber(String operacao, String resposta) throws Exception {
        if (operacao.equals("senha")) {
            ToastAviso("Senha alterada. Entre novamente.");
            voltarAoLogin();
            return;
        }
        if (operacao.equals("salvar")) {
            ToastAviso(new JSONObject(resposta).getString("mensagem"));
            pedir("GET", "api/usuarios/perfil", null, "perfil");
            return;
        }
        usuario = new JSONObject(resposta);
        Sessao.usuario = usuario;
        limparCorpo();
        texto(corpo, "Perfil: " + usuario.getString("perfil"), 18, true);
        nome = campo(corpo, "Nome completo", InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_FLAG_CAP_WORDS);
        nome.setText(usuario.getString("nome"));
        email = campo(corpo, "E-mail de acesso", InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_VARIATION_EMAIL_ADDRESS);
        email.setText(usuario.getString("email"));
        email.setEnabled(Sessao.perfil("Aluno"));
        contato = campo(corpo, "E-mail pessoal de contato", InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_VARIATION_EMAIL_ADDRESS);
        contato.setText(usuario.isNull("emailContato") ? usuario.getString("email") : usuario.optString("emailContato"));
        botao(corpo, "Salvar meus dados", "salvar");
        if (!Sessao.perfil("Administrador")) {
            texto(corpo, "Trocar senha", 21, true);
            atual = campo(corpo, "Senha atual", InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_VARIATION_PASSWORD);
            senha = campo(corpo, "Nova senha", InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_VARIATION_PASSWORD);
            confirmacao = campo(corpo, "Confirmar nova senha", InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_VARIATION_PASSWORD);
            texto(corpo, "Mínimo de 8 caracteres, maiúscula, número e caractere especial. Sem espaços.", 15, false);
            botao(corpo, "Trocar senha", "senha");
        }
        botao(corpo, "Voltar", "voltar");
    }

    private void ToastAviso(String texto) { android.widget.Toast.makeText(this, texto, android.widget.Toast.LENGTH_LONG).show(); }

    @Override public void onClick(View tela) {
        if (ocupado) return;
        if (acao(tela).equals("voltar")) { finish(); return; }
        try {
            JSONObject dados = new JSONObject();
            if (acao(tela).equals("salvar")) {
                Validacao.nome(valor(nome).trim());
                Validacao.email(valor(email).trim());
                Validacao.email(valor(contato).trim());
                dados.put("nome", valor(nome).trim());
                dados.put("email", valor(email).trim());
                dados.put("emailContato", valor(contato).trim());
                pedir("PUT", "api/usuarios/perfil", dados, "salvar");
            } else {
                Validacao.exigir(!valor(atual).isEmpty(), "Informe a senha atual.");
                Validacao.senha(valor(senha), valor(confirmacao));
                dados.put("senhaAtual", valor(atual));
                dados.put("senha", valor(senha));
                dados.put("confirmarSenha", valor(confirmacao));
                pedir("POST", "api/usuarios/trocar-senha", dados, "senha");
            }
        } catch (Exception falha) { erro(falha); }
    }
}
