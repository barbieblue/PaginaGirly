const API_BASE = "/api";
let usuarioSeleccionado = null;
let tabActiva = "pujas";

async function cargarUsuarios() {
    const respuesta = await fetch(`${API_BASE}/usuarios`);
    const usuarios = await respuesta.json();
    const select = document.getElementById("selectUsuario");
    usuarios.forEach(u => {
        const opt = document.createElement("option");
        opt.value = u.id;
        opt.textContent = `${u.nombre} (${u.email})`;
        select.appendChild(opt);
    });
}

function normalizarFecha(fechaIso) {
    return new Date(fechaIso.endsWith('Z') ? fechaIso : fechaIso + 'Z');
}

function formatearTiempoRestante(fechaFin) {
    const fin = normalizarFecha(fechaFin);
    const diffMs = fin - new Date();
    if (diffMs <= 0) return "Finalizada";
    const minutos = Math.floor(diffMs / 60000);
    const horas = Math.floor(minutos / 60);
    if (horas > 0) return `${horas}h ${minutos % 60}m`;
    return `${minutos}m`;
}

function renderizarCards(subastas, modo) {
    // modo: "pujas" muestra badge Liderando/Superado
    //       "publicaciones" muestra métricas de recaudación
    if (!subastas || subastas.length === 0) {
        return `<p class="text-muted">No hay resultados.</p>`;
    }

    return `<div class="row g-3">
        ${subastas.map(s => {
        const imagen = s.urlImagen || generarPlaceholder(s.titulo);
        const tiempoRestante = formatearTiempoRestante(s.fechaFin);
        const estadoBadge = s.estado === "ACTIVA"
            ? `<span class="badge bg-success">${s.estado}</span>`
            : s.estado === "FINALIZADA"
                ? `<span class="badge bg-secondary">${s.estado}</span>`
                : `<span class="badge bg-warning text-dark">${s.estado}</span>`;

        // Badge de liderazgo solo en "Mis Pujas"
        const liderazgoBadge = modo === "pujas"
            ? s.esLider
                ? `<span class="badge bg-success ms-1">Liderando</span>`
                : `<span class="badge bg-danger ms-1">Superado</span>`
            : "";

        return `
                <div class="col-md-4">
                    <div class="card card-subasta h-100">
                        <img src="${imagen}" class="card-img-top" style="height:160px; object-fit:cover;"
                            onerror="this.onerror=null; this.src='${generarPlaceholder(s.titulo).replace(/'/g, "%27")}';">
                        <div class="card-body">
                            <h6 class="card-title">${s.titulo}</h6>
                            <p class="text-muted small mb-1">${s.categoria}</p>
                            ${estadoBadge} ${liderazgoBadge}
                            <hr>
                            <p class="mb-1">Oferta más alta: <strong>$${s.ofertaMasAlta.toLocaleString()}</strong></p>
                            <p class="mb-1 small text-muted">${s.cantidadOfertas} oferta(s)</p>
                            <p class="mb-2 small text-muted">⏱ ${tiempoRestante}</p>
                            <a href="detalle.html?id=${s.id}" class="btn btn-outline-primary btn-sm w-100">Ver subasta</a>
                        </div>
                    </div>
                </div>
            `;
    }).join("")}
    </div>`;
}

function generarPlaceholder(texto) {
    const inicial = (texto || "P").charAt(0).toUpperCase();
    const svg = `<svg xmlns="http://www.w3.org/2000/svg" width="400" height="300">
        <rect width="100%" height="100%" fill="#e9ecef"/>
        <text x="50%" y="50%" font-size="80" fill="#adb5bd" text-anchor="middle" dominant-baseline="middle" font-family="Arial">${inicial}</text>
    </svg>`;
    return `data:image/svg+xml;utf8,${encodeURIComponent(svg).replace(/'/g, "%27")}`;
}

async function cargarActividades(usuarioId) {
    // Mis Pujas: subastas donde el usuario pujó al menos una vez
    const resPujas = await fetch(`${API_BASE}/subastas?compradorId=${usuarioId}`);
    const pujas = await resPujas.json();
    document.getElementById("contenidoPujas").innerHTML = renderizarCards(pujas, "pujas");

    // Mis Publicaciones: subastas creadas por el usuario
    const resPubs = await fetch(`${API_BASE}/subastas?vendedorId=${usuarioId}`);
    const publicaciones = await resPubs.json();
    document.getElementById("contenidoPublicaciones").innerHTML = renderizarCards(publicaciones, "publicaciones");
}

function cambiarTab(tab) {
    tabActiva = tab;

    document.getElementById("tabPujas").classList.toggle("active", tab === "pujas");
    document.getElementById("tabPublicaciones").classList.toggle("active", tab === "publicaciones");
    document.getElementById("contenidoPujas").classList.toggle("d-none", tab !== "pujas");
    document.getElementById("contenidoPublicaciones").classList.toggle("d-none", tab !== "publicaciones");
}

// Eventos de las pestañas
document.getElementById("tabPujas")?.addEventListener("click", () => cambiarTab("pujas"));
document.getElementById("tabPublicaciones")?.addEventListener("click", () => cambiarTab("publicaciones"));

// Evento del selector de usuario
document.getElementById("selectUsuario").addEventListener("change", async (e) => {
    usuarioSeleccionado = Number(e.target.value);
    if (!usuarioSeleccionado) {
        document.getElementById("panelActividades").classList.add("d-none");
        return;
    }
    document.getElementById("panelActividades").classList.remove("d-none");
    await cargarActividades(usuarioSeleccionado);
});

cargarUsuarios();