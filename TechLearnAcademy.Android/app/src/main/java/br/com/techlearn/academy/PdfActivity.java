package br.com.techlearn.academy;

import android.os.Bundle;
import android.os.ParcelFileDescriptor;
import android.view.View;
import android.widget.*;
import android.graphics.*;
import android.graphics.pdf.PdfRenderer;
import android.content.Intent;
import java.io.*;

public class PdfActivity extends TelaBase {
    private PdfRenderer documento;
    private ParcelFileDescriptor descritor;
    private File arquivo;
    private ImageView imagem;
    private TextView paginaTexto;
    private Button anterior, proxima;
    private Bitmap paginaImagem;
    private int pagina;

    @Override public void onCreate(Bundle estado) {
        super.onCreate(estado); if (isFinishing()) return;
        preparar("Leitor de PDF");
        paginaTexto = texto(corpo, "Abrindo documento...", 18, true);
        anterior = botao(corpo, "Página anterior", "anterior"); anterior.setEnabled(false);
        proxima = botao(corpo, "Próxima página", "proxima"); proxima.setEnabled(false);
        botao(corpo, "Salvar PDF", "salvar");
        botao(corpo, "Voltar", "voltar");
        imagem = new ImageView(this);
        imagem.setAdjustViewBounds(true);
        imagem.setContentDescription("Página do documento PDF");
        corpo.addView(imagem, new LinearLayout.LayoutParams(-1, -2));
        final int materialId = getIntent().getIntExtra("id", 0);
        arquivo = new File(getCacheDir(), "material-" + materialId + "-" + System.nanoTime() + ".pdf");
        bloquear(true);
        new Thread(new Runnable() {
            @Override public void run() {
                try {
                    byte[] bytes = Api.baixar("api/materiais/" + materialId + "/arquivo");
                    try (FileOutputStream saida = new FileOutputStream(arquivo)) { saida.write(bytes); }
                    runOnUiThread(new Runnable() {
                        @Override public void run() {
                            if (isFinishing() || isDestroyed()) { arquivo.delete(); return; }
                            bloquear(false);
                            try {
                                descritor = ParcelFileDescriptor.open(arquivo, ParcelFileDescriptor.MODE_READ_ONLY);
                                documento = new PdfRenderer(descritor);
                                mostrarPagina();
                                aviso("Use Próxima página ou salve o documento para abrir no seu leitor.");
                            } catch (Exception falha) { erro(falha); }
                        }
                    });
                } catch (final Exception falha) {
                    runOnUiThread(new Runnable() {
                        @Override public void run() {
                            if (isFinishing() || isDestroyed()) return;
                            bloquear(false);
                            if (falha instanceof Api.Erro && ((Api.Erro) falha).status == 401) voltarAoLogin();
                            else erro(falha);
                        }
                    });
                }
            }
        }).start();
    }

    private void mostrarPagina() throws Exception {
        // O leitor nativo transforma uma página do PDF em imagem.
        try (PdfRenderer.Page atual = documento.openPage(pagina)) {
            imagem.setImageBitmap(null);
            if (paginaImagem != null) paginaImagem.recycle();
            int largura = Math.min(getResources().getDisplayMetrics().widthPixels - dp(40), 1400);
            int altura = Math.round((float) largura * atual.getHeight() / atual.getWidth());
            if (altura > 3000) { largura = Math.round((float) largura * 3000 / altura); altura = 3000; }
            paginaImagem = Bitmap.createBitmap(Math.max(1, largura), Math.max(1, altura), Bitmap.Config.ARGB_8888);
            paginaImagem.eraseColor(Color.WHITE);
            atual.render(paginaImagem, null, null, PdfRenderer.Page.RENDER_MODE_FOR_DISPLAY);
            imagem.setImageBitmap(paginaImagem);
            paginaTexto.setText("Página " + (pagina + 1) + " de " + documento.getPageCount());
            anterior.setEnabled(pagina > 0); proxima.setEnabled(pagina + 1 < documento.getPageCount());
        }
    }

    @Override public void onClick(View tela) {
        if (ocupado) return;
        String acao = acao(tela);
        if (acao.equals("voltar")) { finish(); return; }
        if (documento == null) { aviso("Aguarde o carregamento do PDF."); return; }
        try {
            if (acao.equals("anterior")) { pagina--; mostrarPagina(); }
            if (acao.equals("proxima")) { pagina++; mostrarPagina(); }
            if (acao.equals("salvar")) {
                Intent pedido = new Intent(Intent.ACTION_CREATE_DOCUMENT);
                pedido.addCategory(Intent.CATEGORY_OPENABLE); pedido.setType("application/pdf");
                pedido.putExtra(Intent.EXTRA_TITLE, "Material-TechLearn.pdf");
                startActivityForResult(pedido, 1);
            }
        } catch (Exception falha) { erro(falha); }
    }

    @Override protected void onActivityResult(int pedido, int resultado, Intent dados) {
        super.onActivityResult(pedido, resultado, dados);
        if (pedido != 1 || resultado != RESULT_OK || dados == null) return;
        try (InputStream entrada = new FileInputStream(arquivo); OutputStream saida = getContentResolver().openOutputStream(dados.getData())) {
            byte[] parte = new byte[8192];
            int quantidade;
            while ((quantidade = entrada.read(parte)) != -1) saida.write(parte, 0, quantidade);
            aviso("PDF salvo no local escolhido.");
        } catch (Exception falha) { erro(falha); }
    }

    @Override public void onDestroy() {
        if (documento != null) documento.close();
        try { if (descritor != null) descritor.close(); } catch (IOException ignorado) { }
        if (paginaImagem != null) { imagem.setImageBitmap(null); paginaImagem.recycle(); }
        if (arquivo != null) arquivo.delete();
        super.onDestroy();
    }
}
