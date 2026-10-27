package br.com.techlearn.academy;

import android.util.Patterns;

public class Validacao {
    public static void exigir(boolean condicao, String mensagem) throws Exception {
        if (!condicao) throw new Exception(mensagem);
    }

    public static void nome(String nome) throws Exception {
        exigir(nome.length() >= 2 && nome.length() <= 100, "O nome deve ter de 2 a 100 caracteres.");
    }

    public static void email(String email) throws Exception {
        exigir(email.length() <= 254 && Patterns.EMAIL_ADDRESS.matcher(email).matches(), "Informe um e-mail válido de até 254 caracteres.");
    }

    public static void senha(String senha, String confirmacao) throws Exception {
        exigir(senha.codePointCount(0, senha.length()) >= 8, "A senha deve ter pelo menos 8 caracteres.");
        boolean maiuscula = false;
        boolean numero = false;
        boolean especial = false;
        // Percorre os caracteres, incluindo letras acentuadas.
        for (int posicao = 0; posicao < senha.length();) {
            int caractere = senha.codePointAt(posicao);
            exigir(!Character.isWhitespace(caractere) && !Character.isSpaceChar(caractere), "A senha não pode conter espaços.");
            if (Character.isUpperCase(caractere)) maiuscula = true;
            if (Character.isDigit(caractere) || Character.getType(caractere) == Character.LETTER_NUMBER || Character.getType(caractere) == Character.OTHER_NUMBER) numero = true;
            if (!Character.isLetterOrDigit(caractere) && Character.getType(caractere) != Character.LETTER_NUMBER && Character.getType(caractere) != Character.OTHER_NUMBER) especial = true;
            posicao += Character.charCount(caractere);
        }
        exigir(maiuscula, "Inclua uma letra maiúscula.");
        exigir(numero, "Inclua um número.");
        exigir(especial, "Inclua um caractere especial.");
        exigir(senha.equals(confirmacao), "As senhas devem ser exatamente iguais.");
    }
}
