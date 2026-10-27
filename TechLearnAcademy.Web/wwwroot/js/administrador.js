const pagina = document.body.dataset.pagina;
let autor;

async function listarUsuarios(paginaNumero = 1) {
    const retorno = await requisicao("/api/administracao/usuarios?busca=" + encodeURIComponent(textoCampo("busca")) + "&pagina=" + paginaNumero);
    const area = campo("lista"); area.replaceChildren();
    if (!retorno.itens.length) vazio(area, "Nenhum usuário encontrado.");
    for (const usuario of retorno.itens) {
        const item = cartao(usuario.nome, usuario.email);
        item.append(elemento("p", usuario.perfil + " · " + (usuario.ativo ? "Ativo" : "Inativo")),
            criarLink("Editar dados", "editar-usuario.html?id=" + usuario.id));
        if (autor.perfil === "Administrador") item.append(botaoAcao(usuario.ativo ? "Desativar conta" : "Ativar conta", async function () {
            if (!await dialogo("Situação da conta", (usuario.ativo ? "Desativar" : "Ativar") + " a conta de " + usuario.nome + "?")) return;
            const resposta = await requisicao("/api/administracao/usuarios/" + usuario.id, "PUT", { ativo: !usuario.ativo });
            aviso(resposta.mensagem, "sucesso"); await listarUsuarios(paginaNumero);
        }));
        area.append(item);
    }
    paginacao(retorno, listarUsuarios);
}

iniciar(["Administrador", "Gestor"], async function (usuario) {
    autor = usuario;
    if (pagina === "painel") {
        if (usuario.perfil !== "Administrador") throw new Error("Este painel pertence ao administrador.");
        const resumo = await requisicao("/api/administracao/painel");
        metricas([["Usuários ativos", resumo.ativos], ["Usuários inativos", resumo.inativos]]);
        for (const perfil of resumo.perfis) {
            const card = cartao(perfil.perfil); card.append(elemento("strong", perfil.quantidade, "numero")); campo("lista").append(card);
        }
    }
    if (pagina === "usuarios") {
        campo("form-busca").onsubmit = function (e) { e.preventDefault(); listarUsuarios().catch(erro => aviso(erro.message, "erro")); };
        await listarUsuarios();
    }
    if (pagina === "cadastrar") {
        if (usuario.perfil === "Gestor") {
            campo("titulo-cadastro").textContent = "Cadastrar mentor";
            campo("conta-perfil").querySelector('option[value="Gestor"]').remove();
            campo("conta-perfil").disabled = true;
        }
        enviarFormulario("form-conta", async function (form) {
            verificarNome(textoCampo("conta-nome")); verificarEmail(textoCampo("conta-contato"));
            verificarSenha(valor("conta-senha"), valor("conta-confirmacao"));
            const retorno = await requisicao("/api/administracao/usuarios", "POST", {
                nome: textoCampo("conta-nome"), emailContato: textoCampo("conta-contato"), perfil: valor("conta-perfil"),
                senha: valor("conta-senha"), confirmarSenha: valor("conta-confirmacao")
            });
            form.reset(); aviso(retorno.mensagem, "sucesso");
            await dialogo("Conta cadastrada", "E-mail de acesso: " + retorno.email + "\nInforme esse e-mail e a senha inicial ao usuário.", false, "Entendi");
        });
    }
    if (pagina === "editar") {
        const u = await requisicao("/api/administracao/usuarios/" + numeroId());
        campo("nome-editado").textContent = u.nome; campo("perfil-editado").textContent = u.perfil;
        campo("nome-editado-input").value = u.nome; campo("email-editado-input").value = u.email;
        campo("email-editado-input").readOnly = u.perfil !== "Aluno";
        campo("contato-editado").value = u.emailContato || u.email;
        campo("ativo").value = String(u.ativo);
        campo("situacao-conta").hidden = usuario.perfil !== "Administrador";
        enviarFormulario("form-usuario", async function () {
            const resposta = await requisicao("/api/administracao/usuarios/" + u.id, "PUT", {
                nome: textoCampo("nome-editado-input"), email: textoCampo("email-editado-input"),
                emailContato: textoCampo("contato-editado"), ativo: usuario.perfil === "Administrador" ? valor("ativo") === "true" : u.ativo
            });
            aviso(resposta.mensagem, "sucesso");
        });
    }
});

