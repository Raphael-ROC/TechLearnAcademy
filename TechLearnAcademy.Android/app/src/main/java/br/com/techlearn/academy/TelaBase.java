package br.com.techlearn.academy;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.Intent;
import android.content.res.ColorStateList;
import android.graphics.Color;
import android.graphics.Typeface;
import android.graphics.drawable.GradientDrawable;
import android.os.Bundle;
import android.view.View;
import android.view.WindowInsets;
import android.widget.*;
import org.json.JSONObject;
import org.json.JSONArray;
import java.net.URLEncoder;
import java.util.ArrayList;

public abstract class TelaBase extends Activity implements View.OnClickListener {
    protected final int AZUL = Color.rgb(17, 46, 72);
    protected final int VERDE = Color.rgb(8, 127, 131);
    protected LinearLayout corpo;
    protected TextView mensagem;
    private ProgressBar progresso;
    protected boolean ocupado;
    private final ArrayList<Button> botoes = new ArrayList<>();
    private final ArrayList<Button> bloqueados = new ArrayList<>();

    protected boolean publica() { return false; }

    @Override public void onCreate(Bundle estado) {
        super.onCreate(estado);
        if (!publica() && Sessao.token.isEmpty()) { voltarAoLogin(); return; }
        getWindow().setSoftInputMode(android.view.WindowManager.LayoutParams.SOFT_INPUT_ADJUST_RESIZE);
    }

    protected void preparar(String titulo) {
        LinearLayout raiz = new LinearLayout(this);
        raiz.setOrientation(LinearLayout.VERTICAL);
        raiz.setBackgroundColor(Color.rgb(243, 246, 250));
        raiz.setFitsSystemWindows(true);
        if (android.os.Build.VERSION.SDK_INT >= 30) {
            raiz.setOnApplyWindowInsetsListener(new View.OnApplyWindowInsetsListener() {
                @Override public WindowInsets onApplyWindowInsets(View tela, WindowInsets insets) {
                    android.graphics.Insets barras = insets.getInsets(WindowInsets.Type.systemBars());
                    tela.setPadding(barras.left, barras.top, barras.right, barras.bottom);
                    return insets;
                }
            });
        }
        LinearLayout cabecalho = new LinearLayout(this);
        cabecalho.setOrientation(LinearLayout.VERTICAL);
        cabecalho.setPadding(dp(20), dp(18), dp(20), dp(18));
        cabecalho.setBackgroundColor(AZUL);
        TextView marca = texto(cabecalho, "TechLearn Academy", 23, true);
        marca.setTextColor(Color.WHITE);
        TextView subtitulo = texto(cabecalho, titulo, 16, false);
        subtitulo.setTextColor(Color.WHITE);
        raiz.addView(cabecalho);
        progresso = new ProgressBar(this, null, android.R.attr.progressBarStyleHorizontal);
        progresso.setIndeterminate(true);
        progresso.setVisibility(View.GONE);
        raiz.addView(progresso, new LinearLayout.LayoutParams(-1, dp(4)));
        mensagem = texto(raiz, "", 15, false);
        mensagem.setPadding(dp(20), dp(6), dp(20), dp(6));
        mensagem.setAccessibilityLiveRegion(View.ACCESSIBILITY_LIVE_REGION_POLITE);
        ScrollView rolagem = new ScrollView(this);
        rolagem.setFillViewport(true);
        corpo = new LinearLayout(this);
        corpo.setOrientation(LinearLayout.VERTICAL);
        corpo.setPadding(dp(20), dp(6), dp(20), dp(24));
        rolagem.addView(corpo);
        raiz.addView(rolagem, new LinearLayout.LayoutParams(-1, 0, 1));
        setContentView(raiz);
    }

    protected int dp(int valor) { return Math.round(valor * getResources().getDisplayMetrics().density); }

    protected TextView texto(LinearLayout pai, String valor, int tamanho, boolean negrito) {
        TextView campo = new TextView(this);
        campo.setText(valor);
        campo.setTextSize(tamanho);
        campo.setTextColor(Color.rgb(24, 44, 67));
        campo.setPadding(0, dp(5), 0, dp(7));
        if (negrito) campo.setTypeface(Typeface.DEFAULT, Typeface.BOLD);
        pai.addView(campo, new LinearLayout.LayoutParams(-1, -2));
        return campo;
    }

    protected LinearLayout cartao(LinearLayout pai, String titulo) {
        LinearLayout cartao = new LinearLayout(this);
        cartao.setOrientation(LinearLayout.VERTICAL);
        cartao.setPadding(dp(16), dp(12), dp(16), dp(12));
        GradientDrawable fundo = new GradientDrawable();
        fundo.setColor(Color.WHITE);
        fundo.setCornerRadius(dp(8));
        fundo.setStroke(dp(1), Color.rgb(213, 224, 233));
        cartao.setBackground(fundo);
        LinearLayout.LayoutParams tamanho = new LinearLayout.LayoutParams(-1, -2);
        tamanho.setMargins(0, dp(8), 0, dp(8));
        pai.addView(cartao, tamanho);
        if (!titulo.isEmpty()) texto(cartao, titulo, 19, true);
        return cartao;
    }

    protected EditText campo(LinearLayout pai, String rotulo, int tipo) {
        TextView label = texto(pai, rotulo, 15, true);
        EditText campo = new EditText(this);
        campo.setId(View.generateViewId());
        campo.setTextSize(16);
        campo.setMinHeight(dp(48));
        campo.setSingleLine((tipo & android.text.InputType.TYPE_TEXT_FLAG_MULTI_LINE) == 0);
        // Define o tipo por último para preservar a máscara dos campos de senha.
        campo.setInputType(tipo);
        campo.setSaveEnabled(false);
        campo.setTypeface(Typeface.DEFAULT);
        label.setLabelFor(campo.getId());
        pai.addView(campo, new LinearLayout.LayoutParams(-1, -2));
        return campo;
    }

    protected Button botao(LinearLayout pai, String rotulo, String acao) {
        Button botao = new Button(this);
        botao.setText(rotulo);
        botao.setAllCaps(false);
        botao.setMinHeight(dp(48));
        botao.setTextColor(Color.WHITE);
        botao.setBackgroundTintList(ColorStateList.valueOf(VERDE));
        botao.setTag(acao);
        botao.setOnClickListener(this);
        LinearLayout.LayoutParams tamanho = new LinearLayout.LayoutParams(-1, -2);
        tamanho.setMargins(0, dp(7), 0, dp(3));
        pai.addView(botao, tamanho);
        botoes.add(botao);
        return botao;
    }

    protected Spinner opcoes(LinearLayout pai, String rotulo, String[] valores) {
        TextView label = texto(pai, rotulo, 15, true);
        Spinner lista = new Spinner(this);
        lista.setId(View.generateViewId());
        lista.setAdapter(new ArrayAdapter<String>(this, android.R.layout.simple_spinner_dropdown_item, valores));
        label.setLabelFor(lista.getId());
        pai.addView(lista, new LinearLayout.LayoutParams(-1, dp(48)));
        return lista;
    }

    protected CheckBox marcar(LinearLayout pai, String rotulo, boolean marcado) {
        CheckBox campo = new CheckBox(this);
        campo.setText(rotulo);
        campo.setChecked(marcado);
        campo.setMinHeight(dp(48));
        pai.addView(campo);
        return campo;
    }

    protected String valor(EditText campo) { return campo.getText().toString(); }
    protected String acao(View tela) { return String.valueOf(tela.getTag()); }
    protected int id(String acao) { return Integer.parseInt(acao.substring(acao.indexOf('/') + 1)); }
    protected String codificar(String texto) throws Exception { return URLEncoder.encode(texto, "UTF-8"); }

    protected void aviso(String texto) {
        mensagem.setText(texto);
        mensagem.setTextColor(Color.rgb(24, 44, 67));
    }

    protected void erro(Exception falha) {
        String texto = falha.getMessage() == null ? "Não foi possível concluir a operação." : falha.getMessage();
        if (falha instanceof org.json.JSONException) texto = "O servidor devolveu uma resposta inesperada.";
        if (falha instanceof java.io.IOException) texto = "Não foi possível acessar o arquivo ou o servidor. Confira a conexão e tente novamente.";
        mensagem.setText(texto);
        mensagem.setTextColor(Color.rgb(158, 37, 37));
    }

    protected void abrir(Class<?> tela) { startActivity(new Intent(this, tela)); }

    protected void abrir(Class<?> tela, int id) {
        Intent pedido = new Intent(this, tela);
        pedido.putExtra("id", id);
        startActivity(pedido);
    }

    protected void voltarAoLogin() {
        Sessao.limpar();
        Intent pedido = new Intent(this, LoginActivity.class);
        pedido.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TASK);
        startActivity(pedido);
        finish();
    }

    protected void confirmar(String texto, final String acaoConfirmada) {
        new AlertDialog.Builder(this).setMessage(texto)
            .setNegativeButton("Cancelar", null)
            .setPositiveButton("Confirmar", new android.content.DialogInterface.OnClickListener() {
                @Override public void onClick(android.content.DialogInterface dialogo, int botao) {
                    if (!ocupado) executarAcao(acaoConfirmada);
                }
            }).show();
    }

    protected void executarAcao(String acao) { }
    protected void receber(String operacao, String resposta) throws Exception { }
    protected void falhou(Exception falha) { erro(falha); }
    protected void limparCorpo() { corpo.removeAllViews(); botoes.clear(); }

    protected void pedir(final String metodo, final String caminho, final JSONObject dados, final String operacao) {
        if (ocupado) return;
        bloquear(true);
        // A rede roda fora da tela; a resposta volta para a interface.
        new Thread(new Runnable() {
            @Override public void run() {
                try {
                    final String resposta = Api.enviar(metodo, caminho, dados);
                    runOnUiThread(new Runnable() {
                        @Override public void run() {
                            if (isFinishing() || isDestroyed()) return;
                            bloquear(false);
                            aviso("");
                            try { receber(operacao, resposta); }
                            catch (Exception falha) { erro(falha); }
                        }
                    });
                } catch (final Exception falha) {
                    runOnUiThread(new Runnable() {
                        @Override public void run() {
                            if (isFinishing() || isDestroyed()) return;
                            bloquear(false);
                            if (falha instanceof Api.Erro && ((Api.Erro) falha).status == 401 && !publica()) {
                                android.widget.Toast.makeText(TelaBase.this, "Sua sessão expirou. Entre novamente.", Toast.LENGTH_LONG).show();
                                voltarAoLogin();
                            } else falhou(falha);
                        }
                    });
                }
            }
        }).start();
    }

    protected void bloquear(boolean bloquear) {
        ocupado = bloquear;
        progresso.setVisibility(bloquear ? View.VISIBLE : View.GONE);
        if (bloquear) {
            aviso("Carregando...");
            bloqueados.clear();
            for (Button botao : botoes) {
                if (botao.isEnabled()) { bloqueados.add(botao); botao.setEnabled(false); }
            }
        } else {
            for (Button botao : bloqueados) botao.setEnabled(true);
            bloqueados.clear();
        }
    }


    protected void pedirPdf(final String metodo, final String caminho, final JSONObject dados, final android.net.Uri arquivo, final String operacao) {
        if (ocupado) return;
        bloquear(true);
        new Thread(new Runnable() {
            @Override public void run() {
                try {
                    byte[] bytes = Api.ler(getContentResolver().openInputStream(arquivo), 10 * 1024 * 1024);
                    Validacao.exigir(bytes.length >= 5 && new String(bytes, 0, 5, "US-ASCII").equals("%PDF-"), "Selecione um documento PDF válido.");
                    final String resposta = Api.enviarPdf(metodo, caminho, dados, bytes);
                    runOnUiThread(new Runnable() {
                        @Override public void run() {
                            if (isFinishing() || isDestroyed()) return;
                            bloquear(false);
                            try { receber(operacao, resposta); }
                            catch (Exception falha) { erro(falha); }
                        }
                    });
                } catch (final Exception falha) {
                    runOnUiThread(new Runnable() {
                        @Override public void run() {
                            if (isFinishing() || isDestroyed()) return;
                            bloquear(false);
                            if (falha instanceof Api.Erro && ((Api.Erro) falha).status == 401) voltarAoLogin();
                            else falhou(falha);
                        }
                    });
                }
            }
        }).start();
    }

    protected void listaVazia(JSONArray lista) {
        if (lista.length() == 0) texto(corpo, "Nenhum registro encontrado.", 16, false);
    }
}
