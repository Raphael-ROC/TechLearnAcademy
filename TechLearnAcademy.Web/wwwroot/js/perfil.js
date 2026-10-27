iniciar([], async function (usuario) {
    campo("nome").value = usuario.nome;
    campo("email").value = usuario.email;
    campo("email").readOnly = usuario.perfil !== "Aluno";
    campo("email-contato").value = usuario.emailContato || usuario.email;
    if (usuario.perfil !== "Administrador") campo("modelo-senha").before(campo("modelo-senha").content.cloneNode(true));
    campo("modelo-senha").remove();
    if (usuario.perfil === "Administrador") campo("email-contato").previousElementSibling.textContent = "E-mail pessoal de contato";
    campo("perfil").textContent = usuario.perfil;
    enviarFormulario("form-perfil", async function () {
        const nome = textoCampo("nome");
        const email = textoCampo("email");
        verificarNome(nome);
        verificarEmail(email);
        const resposta = await requisicao("/api/usuarios/perfil", "PUT", { nome: nome, email: email, emailContato: textoCampo("email-contato") });
        campo("nome-usuario").textContent = nome + " · " + usuario.perfil;
        aviso(resposta.mensagem, "sucesso");
    });
    if (usuario.perfil !== "Administrador") enviarFormulario("form-senha", async function (formulario) {
        verificarSenha(valor("senha"), valor("confirmar-senha"));
        const resposta = await requisicao("/api/usuarios/trocar-senha", "POST", {
            senhaAtual: valor("senha-atual"), senha: valor("senha"), confirmarSenha: valor("confirmar-senha")
        });
        formulario.reset();
        aviso(resposta.mensagem, "sucesso");
    });
});

