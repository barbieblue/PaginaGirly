const API_BASE = "/api";

let categoriasCache = [];
let idsRenderizadosActualmente = []; // qué subastas están dibujadas ahora mismo

function generarPlaceholder(texto) {
    const inicial = (texto || "P").charAt(0).toUpperCase();
    const svg = `<svg xmlns="http://www.w3.org/2000/svg" width="300" height="200">
        <rect width="100%" height="100%" fill="#e9ecef"/>
        <text x="50%" y="50%" font-size="60" fill="#adb5bd" text-anchor="middle" dominant-baseline="middle" font-family="Arial">${inicial}</text>
    </svg>`;
    return `data:image/svg+xml;utf8,${encodeURIComponent(svg).replace(/'/g, "%27")}`;
}

async function cargarCategorias() {
    const respuesta = await fetch(`${API_BASE}/categorias`);
    categoriasCache = await respuesta.json();

    const select = document.getElementById("filtroCategoria");
    categoriasCache.forEach(cat => {
        const option = document.createElement("option");
        option.value = cat.nombre;
        option.textContent = cat.nombre;
        select.appendChild(option);
    });
}

async function obtenerSubastasDelBackend() {
    const estado = document.getElementById("filtroEstado").value;
    const categoria = document.getElementById("filtroCategoria").value;

    const params = new URLSearchParams();
    if (estado) params.append("estado", estado);
    if (categoria) params.append("categoria", categoria);

    const respuesta = await fetch(`${API_BASE}/subastas?${params.toString()}`);
    return await respuesta.json();
}

function normalizarFecha(fechaIso) {
    return new Date(fechaIso.endsWith('Z') ? fechaIso : fechaIso + 'Z');
}

function formatearTiempoRestante(fechaFinIso) {
    const ahora = new Date();
    const fin = normalizarFecha(fechaFinIso);
    const diffMs = fin - ahora;

    if (diffMs <= 0) return { texto: "Finalizada", critico: false };

    const minutos = Math.floor(diffMs / 60000);
    const segundos = Math.floor((diffMs % 60000) / 1000);
    const critico = diffMs <= 60000;

    return {
        texto: minutos > 0 ? `${minutos}m ${segundos}s` : `${segundos}s`,
        critico
    };
}

function renderizarCatalogoCompleto(subastas) {
    const contenedor = document.getElementById("contenedorSubastas");
    contenedor.innerHTML = "";

    if (subastas.length === 0) {
        contenedor.innerHTML = `<p class="text-muted">No hay subastas para este filtro.</p>`;
        idsRenderizadosActualmente = [];
        return;
    }

    subastas.forEach(s => {
        const tiempo = formatearTiempoRestante(s.fechaFin);
        const imgSrc = s.urlImagen || generarPlaceholder(s.titulo);
        const imgFallback = generarPlaceholder(s.titulo).replace(/'/g, "%27");

        const col = document.createElement("div");
        col.className = "col-md-4";
        col.dataset.subastaId = s.id;
        col.innerHTML = `
            <div class="card card-subasta h-100">
                <img src="${imgSrc}" class="card-img-top" alt="${s.titulo}"
                     onerror="this.onerror=null; this.src='${imgFallback}';">
                <div class="card-body d-flex flex-column">
                    <span class="badge bg-secondary estado-badge mb-2 align-self-start" data-campo="estado">${s.estado}</span>
                    <h5 class="card-title">${s.titulo}</h5>
                    <p class="card-text text-muted mb-1">${s.categoria}</p>
                    <p class="card-text mb-1">Oferta más alta: <strong data-campo="oferta">$${s.ofertaMasAlta.toLocaleString()}</strong></p>
                    <p class="card-text mb-1"><span data-campo="cantidad">${s.cantidadOfertas}</span> oferta(s)</p>
                    <p class="card-text ${tiempo.critico ? 'tiempo-critico' : ''}" data-campo="tiempo">⏱ ${tiempo.texto}</p>
                    <a href="detalle.html?id=${s.id}" class="btn btn-primary mt-auto">Ver subasta</a>
                </div>
            </div>
        `;
        contenedor.appendChild(col);
    });

    idsRenderizadosActualmente = subastas.map(s => s.id);
}


function actualizarCatalogoExistente(subastas) {
    subastas.forEach(s => {
        const card = document.querySelector(`[data-subasta-id="${s.id}"]`);
        if (!card) return;

        const tiempo = formatearTiempoRestante(s.fechaFin);

        card.querySelector('[data-campo="estado"]').textContent = s.estado;
        card.querySelector('[data-campo="oferta"]').textContent = `$${s.ofertaMasAlta.toLocaleString()}`;
        card.querySelector('[data-campo="cantidad"]').textContent = s.cantidadOfertas;

        const elTiempo = card.querySelector('[data-campo="tiempo"]');
        elTiempo.textContent = `⏱ ${tiempo.texto}`;
        elTiempo.classList.toggle("tiempo-critico", tiempo.critico);
    });
}

async function cargarSubastas() {
    const subastas = await obtenerSubastasDelBackend();
    const idsNuevos = subastas.map(s => s.id);


    const mismoConjunto =
        idsNuevos.length === idsRenderizadosActualmente.length &&
        idsNuevos.every((id, i) => id === idsRenderizadosActualmente[i]);

    if (mismoConjunto) {
        actualizarCatalogoExistente(subastas);
    } else {
        renderizarCatalogoCompleto(subastas);
    }
}

function iniciarPolling() {
    cargarSubastas();
    setInterval(cargarSubastas, 3000);
}

document.getElementById("filtroEstado").addEventListener("change", cargarSubastas);
document.getElementById("filtroCategoria").addEventListener("change", cargarSubastas);

cargarCategorias();
iniciarPolling();