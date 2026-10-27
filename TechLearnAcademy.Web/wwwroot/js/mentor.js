const pagina = document.body.dataset.pagina;

function cardAluno(aluno) {
    const item = cartao(aluno.nome, aluno.email);
    item.append(elemento("p", "Aluno atendido por você em um chamado."));
    item.append(criarLink("Ver orientações e curso", "acompanhamento-aluno.html?id=" + aluno.id));
    return item;
}

function mostrarOrientacoes(lista, area, nome = "") {
    area.replaceChildren();
    if (!lista.length) { vazio(area, "Ainda não há orientações registradas."); return; }
    for (const orientacao of lista) {
        const item = cartao(nome || "Orientação", orientacao.texto);
        item.append(elemento("p", dataBrasileira(orientacao.data), "ajuda"));
        area.append(item);
    }
}

async function detalheAluno() {
    const aluno = await requisicao("/api/mentor/alunos/" + numeroId());
    campo("detalhes").append(elemento("h2", aluno.nome), elemento("p", aluno.email));
    campo("acoes").append(criarLink("Registrar orientação", "orientacao.html?id=" + aluno.id));
    if (!aluno.cursos.length) vazio(campo("cursos"), "O aluno ainda não possui matrículas.");
    for (const curso of aluno.cursos) {
        const item = cartao(curso.nome);
        item.append(progresso(curso.progresso));
        campo("cursos").append(item);
    }
    listarResultados(aluno.resultados, campo("lista"));
    mostrarOrientacoes(aluno.orientacoes, campo("orientacoes"));
}

iniciar(["Mentor"], async function (usuario) {
    if (pagina === "painel" || pagina === "alunos") {
        const alunos = await requisicao("/api/mentor/alunos");
        if (pagina === "alunos") {
            if (!alunos.length) vazio(campo("lista"), "Você ainda não assumiu chamados de alunos. Acesse Ajuda e ouvidoria para escolher um chamado.");
            for (const aluno of alunos) campo("lista").append(cardAluno(aluno));
        } else {
            const chamados = await requisicao("/api/chamados");
            const disponiveis = chamados.filter(c => c.podeAtender === true);
            const meus = chamados.filter(c => c.responsavelId === usuario.id && c.status === "Em atendimento");
            metricas([["Chamados disponíveis", disponiveis.length], ["Em atendimento", meus.length], ["Alunos atendidos", alunos.length]]);
            campo("lista").append(criarLink("Ver chamados e atender aluno", "chamados.html"));
            for (const chamado of meus) {
                const card = cartao(chamado.solicitante, chamado.assunto);
                card.append(criarLink("Abrir conversa", "chamados.html?id=" + chamado.id)); campo("lista").append(card);
            }
            // Cada histórico vem da API; não criamos orientações de exemplo.
            const recentes = [];
            for (const aluno of alunos) {
                const detalhe = await requisicao("/api/mentor/alunos/" + aluno.id);
                for (const orientacao of detalhe.orientacoes) {
                    recentes.push({ texto: orientacao.texto, data: orientacao.data, nome: aluno.nome });
                }
            }
            recentes.sort((a, b) => new Date(b.data) - new Date(a.data));
            if (!recentes.length) vazio(campo("orientacoes"), "Ainda não há orientações registradas.");
            for (const orientacao of recentes.slice(0, 6)) {
                const item = cartao(orientacao.nome, orientacao.texto);
                item.append(elemento("p", dataBrasileira(orientacao.data), "ajuda"));
                campo("orientacoes").append(item);
            }
        }
    }
    if (pagina === "acompanhamento") await detalheAluno();
    if (pagina === "orientacao") {
        async function carregar() {
            const aluno = await requisicao("/api/mentor/alunos/" + numeroId());
            campo("nome-aluno").textContent = aluno.nome;
            mostrarOrientacoes(aluno.orientacoes, campo("orientacoes"));
        }
        await carregar();
        enviarFormulario("form-orientacao", async function (formulario) {
            const texto = textoCampo("texto");
            if (!texto) throw new Error("Escreva uma orientação antes de enviar.");
            const retorno = await requisicao("/api/mentor/orientacoes", "POST", { alunoId: numeroId(), texto: texto });
            formulario.reset();
            aviso(retorno.mensagem, "sucesso");
            await carregar();
        });
    }
});
