document.addEventListener("DOMContentLoaded", () => {
    document.querySelectorAll(".auto-dismiss-alert").forEach((alert) => {
        window.setTimeout(() => {
            bootstrap.Alert.getOrCreateInstance(alert).close();
        }, 3000);
    });

    const busca = document.getElementById("filtroNome");
    const filtroTipo = document.getElementById("filtroTipo");
    const lista = document.getElementById("listaCongressistas");
    const quantidade = document.getElementById("quantidadeResultados");
    const semResultados = document.getElementById("semResultados");
    const paginacao = document.getElementById("paginacao");

    if (!busca || !filtroTipo || !lista || !quantidade || !semResultados || !paginacao) {
        return;
    }

    const tamanhoPagina = 25;
    const listaPaginacao = paginacao.querySelector(".pagination");
    let paginaAtual = 1;

    const normalizar = (valor) => valor
        .normalize("NFD")
        .replace(/[\u0300-\u036f]/g, "")
        .toLocaleLowerCase("pt-BR");

    const adicionarBotaoPagina = (texto, destino, opcoes = {}) => {
        const item = document.createElement("li");
        item.className = "page-item";
        if (opcoes.ativo) {
            item.classList.add("active");
        }
        if (opcoes.desabilitado) {
            item.classList.add("disabled");
        }

        const botao = document.createElement("button");
        botao.type = "button";
        botao.className = "page-link";
        botao.textContent = texto;
        botao.disabled = Boolean(opcoes.desabilitado);
        if (opcoes.rotulo) {
            botao.setAttribute("aria-label", opcoes.rotulo);
        }
        if (opcoes.ativo) {
            botao.setAttribute("aria-current", "page");
        }
        botao.addEventListener("click", () => {
            paginaAtual = destino;
            atualizarLista();
        });

        item.append(botao);
        listaPaginacao.append(item);
    };

    const atualizarPaginacao = (totalPaginas) => {
        listaPaginacao.replaceChildren();
        paginacao.hidden = totalPaginas <= 1;
        if (totalPaginas <= 1) {
            return;
        }

        adicionarBotaoPagina("Anterior", paginaAtual - 1, {
            desabilitado: paginaAtual === 1,
            rotulo: "Página anterior"
        });

        const adicionarReticencias = () => {
            const item = document.createElement("li");
            item.className = "page-item disabled";

            const texto = document.createElement("span");
            texto.className = "page-link";
            texto.textContent = "…";
            texto.setAttribute("aria-label", "Páginas omitidas");

            item.append(texto);
            listaPaginacao.append(item);
        };

        let primeiraPaginaVisivel = Math.max(2, paginaAtual - 1);
        let ultimaPaginaVisivel = Math.min(totalPaginas - 1, paginaAtual + 1);
        if (paginaAtual <= 4) {
            primeiraPaginaVisivel = 2;
            ultimaPaginaVisivel = Math.min(5, totalPaginas - 1);
        } else if (paginaAtual >= totalPaginas - 3) {
            primeiraPaginaVisivel = Math.max(2, totalPaginas - 4);
            ultimaPaginaVisivel = totalPaginas - 1;
        }

        const paginasVisiveis = new Set([1, totalPaginas]);
        for (let numero = primeiraPaginaVisivel; numero <= ultimaPaginaVisivel; numero += 1) {
            paginasVisiveis.add(numero);
        }

        let paginaAnterior = 0;
        for (const numero of [...paginasVisiveis].sort((a, b) => a - b)) {
            if (numero - paginaAnterior > 1) {
                adicionarReticencias();
            }

            adicionarBotaoPagina(String(numero), numero, {
                ativo: numero === paginaAtual,
                rotulo: `Página ${numero}`
            });
            paginaAnterior = numero;
        }

        adicionarBotaoPagina("Próxima", paginaAtual + 1, {
            desabilitado: paginaAtual === totalPaginas,
            rotulo: "Próxima página"
        });
    };

    const atualizarLista = () => {
        const termo = normalizar(busca.value.trim());
        const tipoSelecionado = filtroTipo.value;
        const linhas = Array.from(lista.querySelectorAll("tr"));
        const correspondentes = linhas.filter((linha) => {
            const nome = normalizar(linha.dataset.nome || "");
            const nomeCracha = normalizar(linha.dataset.nomeCracha || "");
            const tipo = linha.dataset.tipo || "";
            const correspondeAoNome = !termo || nome.includes(termo) || nomeCracha.includes(termo);
            const correspondeAoTipo = !tipoSelecionado || tipo === tipoSelecionado;
            return correspondeAoNome && correspondeAoTipo;
        });

        const totalPaginas = Math.ceil(correspondentes.length / tamanhoPagina);
        paginaAtual = Math.min(paginaAtual, Math.max(totalPaginas, 1));
        const primeiraLinha = (paginaAtual - 1) * tamanhoPagina;
        const linhasDaPagina = new Set(correspondentes.slice(primeiraLinha, primeiraLinha + tamanhoPagina));

        linhas.forEach((linha) => {
            linha.hidden = !linhasDaPagina.has(linha);
        });

        const primeiroResultado = correspondentes.length === 0 ? 0 : primeiraLinha + 1;
        const ultimoResultado = Math.min(primeiraLinha + tamanhoPagina, correspondentes.length);
        quantidade.textContent = `Mostrando ${primeiroResultado}-${ultimoResultado} de ${correspondentes.length} congressista(s)`;
        semResultados.hidden = correspondentes.length !== 0;
        atualizarPaginacao(totalPaginas);
    };

    busca.addEventListener("input", () => {
        paginaAtual = 1;
        atualizarLista();
    });
    filtroTipo.addEventListener("change", () => {
        paginaAtual = 1;
        atualizarLista();
    });
    atualizarLista();
});
