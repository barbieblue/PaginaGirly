const API_BASE = "/api";

const params = new URLSearchParams(window.location.search);
const subastaId = params.get("id");

// Genera una imagen placeholder local (SVG embebido), sin depender de
// ningún servicio externo que pueda estar caído.
function generarPlaceholder(texto) {
    const inicial = (texto || "P").charAt(0).toUpperCase();
    const svg = `<svg xmlns="http://www.w3.org/2000/svg" width="400" height="300">
        <rect width="100%" height="100%" fill="#e9ecef"/>
        <text x="50%" y="50%" font-size="80" fill="#adb5bd" text-anchor="middle" dominant-baseline="middle" font-family="Arial">${inicial}</text>
    </svg>`;
    return `data:image/svg+xml;utf8,${encodeURIComponent(svg).replace(/'/g, "%27")}`;
}

function obtenerUsuarioActual() {
    return localStorage.getItem("usuarioActualId") || "";
}

function fijarUsuarioActual(id) {
    localStorage.setItem("usuarioActualId", id);
}

let usuariosCache = [];
let ultimoEstadoConocido = null;
let yaRenderizadoUnaVez = false;

async function cargarUsuarios() {
    const respuesta = await fetch(`${API_BASE}/usuarios`);
    usuariosCache = await respuesta.json();
}

function normalizarFecha(fechaIso) {
    return new Date(fechaIso.endsWith('Z') ? fechaIso : fechaIso + 'Z');
}

function mostrarToast(mensaje, tipo = "info") {
    const toastEl = document.getElementById("toastFeedback");
    const titulo = document.getElementById("toastTitulo");
    const cuerpo = document.getElementById("toastMensaje");

    titulo.textContent = tipo === "error" ? "Error" : tipo === "exito" ? "¡Listo!" : "Aviso";
    cuerpo.textContent = mensaje;

    const toast = new bootstrap.Toast(toastEl);
    toast.show();
}

function formatearTiempo(diffMs) {
    const minutos = Math.floor(diffMs / 60000);
    const segundos = Math.floor((diffMs % 60000) / 1000);
    return `${minutos}m ${segundos.toString().padStart(2, "0")}s`;
}

function renderizarHistorial(pujas) {
    if (!pujas || pujas.length === 0) {
        return `<li class="list-group-item text-muted">Sin ofertas aún.</li>`;
    }
    return pujas.map(p => `
        <li class="list-group-item d-flex justify-content-between align-items-center">
            <span><strong>${p.usuario}</strong> — $${p.monto.toLocaleString()}</span>
            <small class="text-muted">${new Date(p.fechaPuja + 'Z').toLocaleTimeString()}</small>
        </li>`).join("");
}

async function cargarDetalle() {
    const respuesta = await fetch(`${API_BASE}/subastas/${subastaId}`);

    if (!respuesta.ok) {
        document.getElementById("contenedorDetalle").innerHTML =
            `<div class="alert alert-danger">No se encontró la subasta.</div>`;
        return;
    }

    const subasta = await respuesta.json();

    if (!yaRenderizadoUnaVez) {
        renderizarEstructuraCompleta(subasta);
        yaRenderizadoUnaVez = true;
    } else {
        actualizarValoresDinamicos(subasta);
    }
}

function renderizarEstructuraCompleta(subasta) {
    const usuarioActual = obtenerUsuarioActual();
    ultimoEstadoConocido = subasta;

    const contenedor = document.getElementById("contenedorDetalle");
    contenedor.innerHTML = `
        <div class="row">
            <div class="col-md-6">
                <img src="${subasta.urlImagen || generarPlaceholder(subasta.titulo)}" class="img-fluid rounded mb-3" alt="${subasta.titulo}"
                onerror="this.onerror=null; this.src='${generarPlaceholder(subasta.titulo).replace(/'/g, "%27")}';">
                <h3>${subasta.titulo}</h3>
                <p class="text-muted">${subasta.categoria} · Vendido por ${subasta.vendedor}</p>
                <p>${subasta.descripcion}</p>
                <span class="badge bg-secondary" id="badgeEstado">${subasta.estado}</span>
            </div>
            <div class="col-md-6">
                <div class="card">
                    <div class="card-body text-center">
                        <div class="temporizador" id="temporizador"></div>
                        <hr>
                        <p>Oferta más alta: <strong id="ofertaMasAlta">$${subasta.ofertaMasAlta.toLocaleString()}</strong></p>
                        <p><span id="cantidadOfertas">${subasta.cantidadOfertas}</span> oferta(s) registradas</p>

                        <div class="mb-3 text-start">
                            <label class="form-label">Actuar como:</label>
                            <select id="selectUsuario" class="form-select">
                                <option value="">Elegí un usuario...</option>
                                ${usuariosCache.map(u => `<option value="${u.id}" ${String(u.id) === usuarioActual ? "selected" : ""}>${u.nombre} (${u.email})</option>`).join("")}
                            </select>
                        </div>

                        <div class="mb-3 text-start">
                            <label class="form-label" id="labelSugerido">Tu oferta (sugerido: $${subasta.proximaOfertaSugerida.toLocaleString()})</label>
                            <input type="number" id="inputMonto" class="form-control" value="${subasta.proximaOfertaSugerida}" min="1" step="1">
                        </div>

                        <button id="btnPujar" class="btn btn-primary w-100">Ofertar</button>

                        <hr>
                        <h6 class="text-start mt-2">Historial de ofertas</h6>
                        <ul class="list-group list-group-flush" id="historialPujas">
                            ${renderizarHistorial(subasta.pujas)}
                        </ul>
                    </div>
                </div>
            </div>
        </div>
    `;

    document.getElementById("selectUsuario").addEventListener("change", (e) => fijarUsuarioActual(e.target.value));
    document.getElementById("btnPujar").addEventListener("click", registrarPuja);

    actualizarTemporizadorYEstado(subasta);
}

function actualizarValoresDinamicos(subasta) {
    if (ultimoEstadoConocido && ultimoEstadoConocido.fechaFin !== subasta.fechaFin && subasta.estado === "ACTIVA") {
        mostrarToast("¡La subasta se extendió 2 minutos por una oferta de último momento!", "info");
    }
    ultimoEstadoConocido = subasta;

    document.getElementById("badgeEstado").textContent = subasta.estado;
    document.getElementById("ofertaMasAlta").textContent = `$${subasta.ofertaMasAlta.toLocaleString()}`;
    document.getElementById("cantidadOfertas").textContent = subasta.cantidadOfertas;
    document.getElementById("labelSugerido").textContent = `Tu oferta (sugerido: $${subasta.proximaOfertaSugerida.toLocaleString()})`;

    // Actualizar historial de pujas en cada poll
    const historial = document.getElementById("historialPujas");
    if (historial) {
        historial.innerHTML = renderizarHistorial(subasta.pujas);
    }

    actualizarTemporizadorYEstado(subasta);
}

function actualizarTemporizadorYEstado(subasta) {
    const fin = normalizarFecha(subasta.fechaFin);
    const ahora = new Date();
    const diffMs = fin - ahora;
    const vencida = diffMs <= 0;
    const zonaCritica = !vencida && diffMs <= 60000;

    const temporizador = document.getElementById("temporizador");
    temporizador.textContent = vencida ? "Subasta finalizada" : formatearTiempo(diffMs);
    temporizador.classList.toggle("zona-critica", zonaCritica);

    const boton = document.getElementById("btnPujar");
    boton.disabled = vencida;
    boton.textContent = vencida ? "Subasta cerrada" : "Ofertar";
}

async function registrarPuja() {
    const usuarioId = document.getElementById("selectUsuario").value;
    const monto = document.getElementById("inputMonto").value;

    if (!usuarioId) {
        mostrarToast("Elegí primero qué usuario va a ofertar.", "error");
        return;
    }

    const boton = document.getElementById("btnPujar");
    boton.disabled = true;
    boton.textContent = "Enviando...";

    try {
        const respuesta = await fetch(`${API_BASE}/subastas/${subastaId}/bids`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ usuarioId: Number(usuarioId), monto: Number(monto) })
        });

        const datos = await respuesta.json();

        if (respuesta.ok) {
            mostrarToast("¡Tu oferta fue registrada!", "exito");
        } else {
            mostrarToast(datos.error || "No se pudo registrar la oferta.", "error");
        }
    } catch (error) {
        mostrarToast("Error de conexión con el servidor.", "error");
    }

    await cargarDetalle();
}

async function iniciar() {
    await cargarUsuarios();
    await cargarDetalle();
    setInterval(cargarDetalle, 3000);
}

iniciar();