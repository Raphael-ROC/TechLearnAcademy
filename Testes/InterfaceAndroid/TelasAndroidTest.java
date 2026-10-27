package br.com.techlearn.academy;

import android.content.ClipData;
import android.content.ClipboardManager;
import android.os.Bundle;
import android.view.View;
import android.view.ViewGroup;
import android.widget.*;
import org.json.JSONObject;
import org.junit.*;
import org.junit.runner.RunWith;
import org.robolectric.Robolectric;
import org.robolectric.RobolectricTestRunner;
import org.robolectric.annotation.Config;
import org.robolectric.android.controller.ActivityController;
import static org.junit.Assert.*;
import java.lang.reflect.Field;

// Somente testes: não integra o APK e não cria contas no servidor.
@RunWith(RobolectricTestRunner.class)
@Config(sdk = 35, qualifiers = "w360dp-h800dp-mdpi")
public class TelasAndroidTest {
    private <T> T campo(Object tela, String nome, Class<T> tipo) throws Exception {
        Field campo = tela.getClass().getDeclaredField(nome);
        campo.setAccessible(true);
        return tipo.cast(campo.get(tela));
    }
    private Button botao(View raiz, String texto) {
        if (raiz instanceof Button && ((Button) raiz).getText().toString().equals(texto)) return (Button)raiz;
        if (raiz instanceof ViewGroup) {
            ViewGroup grupo = (ViewGroup)raiz;
            for (int i=0;i<grupo.getChildCount();i++) { Button encontrado=botao(grupo.getChildAt(i),texto); if(encontrado!=null)return encontrado; }
        }
        return null;
    }

    @After public void limparSessao() { Sessao.limpar(); }

    @Test public void loginNativoAbreERecusaCamposVazios() throws Exception {
        try (ActivityController<LoginActivity> controle = Robolectric.buildActivity(LoginActivity.class).setup().visible()) {
            LoginActivity tela = controle.get();
            Button entrar = botao(tela.getWindow().getDecorView(),"Entrar");
            assertNotNull(entrar);
            entrar.performClick();
            assertTrue(tela.mensagem.getText().toString().contains("e-mail válido"));
            assertTrue(Sessao.token.isEmpty());
            EditText email=campo(tela,"email",EditText.class);
            email.setText("aluno@example.com");
            entrar.performClick();
            assertEquals("Informe a senha.",tela.mensagem.getText().toString());
        }
    }

    @Test public void campoSenhaNativoAceitaColagemESegueOculto() throws Exception {
        try (ActivityController<LoginActivity> controle = Robolectric.buildActivity(LoginActivity.class).setup().visible()) {
            LoginActivity tela=controle.get();
            EditText senha=campo(tela,"senha",EditText.class);
            ClipboardManager area=(ClipboardManager)tela.getSystemService(android.content.Context.CLIPBOARD_SERVICE);
            area.setPrimaryClip(ClipData.newPlainText("Teste","Abcdef1!"));
            senha.requestFocus();
            assertTrue(senha.onTextContextMenuItem(android.R.id.paste));
            assertEquals("Abcdef1!",senha.getText().toString());
            assertFalse(senha.isSaveEnabled());
            assertTrue(senha.getTransformationMethod() instanceof android.text.method.PasswordTransformationMethod);
        }
    }

    @Test public void cadastroNativoRecusaConfirmacaoDiferente() throws Exception {
        try (ActivityController<CadastroActivity> controle=Robolectric.buildActivity(CadastroActivity.class).setup().visible()) {
            CadastroActivity tela=controle.get();
            campo(tela,"nome",EditText.class).setText("Aluno de teste");
            campo(tela,"email",EditText.class).setText("aluno@example.com");
            campo(tela,"senha",EditText.class).setText("Abcdef1!");
            campo(tela,"confirmacao",EditText.class).setText("Abcdef2!");
            botao(tela.getWindow().getDecorView(),"Criar conta").performClick();
            assertEquals("As senhas devem ser exatamente iguais.",tela.mensagem.getText().toString());
            assertFalse(tela.ocupado);
        }
    }

    @Test public void loginSeAdaptaADiferentesLarguras() throws Exception {
        try (ActivityController<LoginActivity> controle=Robolectric.buildActivity(LoginActivity.class).setup().visible()) {
            LoginActivity tela=controle.get();
            View raiz=((ViewGroup)tela.findViewById(android.R.id.content)).getChildAt(0);
            EditText email=campo(tela,"email",EditText.class);
            for(int largura:new int[]{360,600,1000}) {
                raiz.measure(View.MeasureSpec.makeMeasureSpec(largura,View.MeasureSpec.EXACTLY),View.MeasureSpec.makeMeasureSpec(800,View.MeasureSpec.EXACTLY));
                raiz.layout(0,0,largura,800);
                assertTrue(email.getWidth()>200);
                assertTrue(email.getRight()<=((View)email.getParent()).getWidth());
                assertNotNull(botao(raiz,"Entrar"));
            }
        }
    }

    public static class PainelTeste extends PainelActivity {
        String caminho;
        @Override protected void pedir(String metodo,String caminho,JSONObject dados,String operacao) { this.caminho=caminho; }
        void mostrar(String resposta) throws Exception { super.receber("painel",resposta); }
    }

    @Test public void painelMostraFuncoesDosQuatroPerfis() throws Exception {
        String[] perfis={"Aluno","Mentor","Gestor","Administrador"};
        String[] botoes={"Meus cursos","Alunos e orientações","Gerenciar cursos e conteúdo","Gerenciar usuários"};
        String[] rotas={"api/aluno/painel","api/mentor/alunos","api/relatorios","api/administracao/painel"};
        for(int i=0;i<perfis.length;i++) {
            Sessao.token="somente-interface-de-teste";
            Sessao.usuario=new JSONObject().put("id",1).put("nome","Interface de teste").put("perfil",perfis[i]);
            try(ActivityController<PainelTeste> controle=Robolectric.buildActivity(PainelTeste.class).setup().visible()) {
                PainelTeste tela=controle.get();
                assertEquals(rotas[i],tela.caminho);
                tela.mostrar(perfis[i].equals("Mentor") ? "[]" : "{\"cursosMatriculados\":0,\"progressoGeral\":0,\"quantidadeCursos\":0,\"totalMatriculas\":0,\"ativos\":0,\"inativos\":0,\"perfis\":[]}");
                assertNotNull(botao(tela.getWindow().getDecorView(),botoes[i]));
                assertNotNull(botao(tela.getWindow().getDecorView(),"Sair"));
                if(!perfis[i].equals("Administrador"))assertNull(botao(tela.getWindow().getDecorView(),"Verificar banco pela API"));
            }
        }
    }
}
