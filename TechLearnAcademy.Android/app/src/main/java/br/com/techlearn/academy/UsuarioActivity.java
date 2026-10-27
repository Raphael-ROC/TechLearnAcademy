package br.com.techlearn.academy;

import android.os.Bundle;
import android.view.View;
import android.widget.*;
import android.text.InputType;
import org.json.JSONObject;

public class UsuarioActivity extends TelaBase {
    private int usuarioId;
    private JSONObject usuario;
    private EditText nome, contato, email, senha, confirmacao;
    private Spinner perfil;
    private CheckBox ativo;

    @Override public void onCreate(Bundle estado) {
        super.onCreate(estado); if (isFinishing()) return;
        usuarioId = getIntent().getIntExtra("id", 0);
        preparar(usuarioId == 0 ? "Cadastrar usuário" : "Editar usuário");
        if (usuarioId > 0) pedir("GET", "api/administracao/usuarios/" + usuarioId, null, "usuario");
        else montar();
    }

    private void montar() {
        limparCorpo();
        nome = campo(corpo, "Nome completo", InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_FLAG_CAP_WORDS);
        contato = campo(corpo, "E-mail pessoal para recuperação", InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_VARIATION_EMAIL_ADDRESS);
        if (usuarioId == 0) {
            texto(corpo, "Informe o primeiro e o segundo nome. A API gera o e-mail institucional.", 15, false);
            perfil = opcoes(corpo, "Perfil", Sessao.perfil("Administrador") ? new String[]{"Mentor", "Gestor"} : new String[]{"Mentor"});
            senha = campo(corpo, "Senha inicial", InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_VARIATION_PASSWORD);
            confirmacao = campo(corpo, "Confirmar senha", InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_VARIATION_PASSWORD);
            texto(corpo, "Mínimo de 8 caracteres, maiúscula, número e caractere especial. Sem espaços.", 15, false);
        } else {
            nome.setText(usuario.optString("nome"));
            contato.setText(usuario.isNull("emailContato") ? usuario.optString("email") : usuario.optString("emailContato"));
            texto(corpo, "Perfil: " + usuario.optString("perfil") + " (fixo)", 16, true);
            email = campo(corpo, "E-mail de acesso", InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_VARIATION_EMAIL_ADDRESS);
            email.setText(usuario.optString("email"));
            email.setEnabled(usuario.optString("perfil").equals("Aluno"));
            ativo = marcar(corpo, "Conta ativa", usuario.optBoolean("ativo"));
            ativo.setEnabled(Sessao.perfil("Administrador"));
        }
        botao(corpo, usuarioId == 0 ? "Cadastrar conta" : "Salvar alterações", "salvar");
        botao(corpo, "Cancelar", "voltar");
    }

    @Override public void onClick(View tela) {
        if (ocupado) return;
        if (acao(tela).equals("voltar")) { finish(); return; }
        if (usuarioId > 0 && ativo.isChecked() != usuario.optBoolean("ativo")) {
            confirmar((ativo.isChecked() ? "Ativar" : "Desativar") + " esta conta?", "salvar");
            return;
        }
        executarAcao("salvar");
    }

    @Override protected void executarAcao(String acao) {
        try {
            Validacao.nome(valor(nome).trim());
            Validacao.email(valor(contato).trim());
            JSONObject dados = new JSONObject();
            dados.put("nome", valor(nome).trim());
            dados.put("emailContato", valor(contato).trim());
            if (usuarioId == 0) {
                Validacao.exigir(valor(nome).trim().split("\\s+").length >= 2, "Informe o primeiro e o segundo nome.");
                Validacao.senha(valor(senha), valor(confirmacao));
                dados.put("perfil", perfil.getSelectedItem().toString());
                dados.put("senha", valor(senha));
                dados.put("confirmarSenha", valor(confirmacao));
                pedir("POST", "api/administracao/usuarios", dados, "salvar");
            } else {
                Validacao.email(valor(email).trim());
                dados.put("email", valor(email).trim());
                dados.put("ativo", ativo.isChecked());
                pedir("PUT", "api/administracao/usuarios/" + usuarioId, dados, "salvar");
            }
        } catch (Exception falha) { erro(falha); }
    }

    @Override protected void receber(String operacao, String resposta) throws Exception {
        if (operacao.equals("usuario")) { usuario = new JSONObject(resposta); montar(); return; }
        JSONObject resultado = new JSONObject(resposta);
        if (usuarioId == 0) {
            senha.setText(""); confirmacao.setText("");
            new android.app.AlertDialog.Builder(this).setTitle("Conta cadastrada").setMessage(resultado.getString("mensagem"))
                .setPositiveButton("OK", null).show();
            nome.setText(""); contato.setText("");
        } else {
            android.widget.Toast.makeText(this, resultado.getString("mensagem"), Toast.LENGTH_LONG).show();
            if (usuarioId == Sessao.usuario.optInt("id")) voltarAoLogin();
            else finish();
        }
    }
}
