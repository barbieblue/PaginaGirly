const API_BASE = "/api";

async function cargarSelects() {
    // Cargar usuarios
    const resUsuarios = await fetch(`${API_BASE}/usuarios`);
    const usuarios = await resUsuarios.json();
    const selectVendedor = document.getElementById("selectVendedor");
    usuarios.forEach(u => {
        const opt = document.createElement("option");
        opt.value = u.id;
        opt.textContent = `${u.nombre} (${u.email})`;
        selectVendedor.appendChild(opt);
    });

    // Cargar categorías
    const resCategorias = await fetch(`${API_BASE}/categorias`);
    const categorias = await resCategorias.json();
    const selectCategoria = document.getElementById("selectCategoria");
    categorias.forEach(c => {
        const opt = document.createElement("option");
        opt.value = c.id;
        opt.textContent = c.nombre;
        selectCategoria.appendChild(opt);
    });
}

function mostrarError(mensaje) {
    const alerta = document.getElementById("alertaError");
    alerta.textContent = mensaje;
    alerta.classList.remove("d-none");
    document.getElementById("alertaExito").classList.add("d-none");
}

function mostrarExito(mensaje) {
    const alerta = document.getElementById("alertaExito");
    alerta.textContent = mensaje;
    alerta.classList.remove("d-none");
    document.getElementById("alertaError").classList.add("d-none");
}

function ocultarAlertas() {
    document.getElementById("alertaError").classList.add("d-none");
    document.getElementById("alertaExito").classList.add("d-none");
}

// Convierte el valor de un datetime-local a ISO 8601 UTC
function toIsoUtc(fechaLocal) {
    return new Date(fechaLocal).toISOString();
}

function validar(campos) {
    if (!campos.vendedorId) return "Seleccioná un vendedor.";
    if (!campos.titulo.trim()) return "El título es obligatorio.";
    if (!campos.descripcion.trim()) return "La descripción es obligatoria.";
    if (!campos.categoriaId) return "Seleccioná una categoría.";
    if (!campos.precioBase || campos.precioBase <= 0) return "El precio base debe ser mayor a cero.";
    if (!campos.incrementoMinimo || campos.incrementoMinimo <= 0) return "El incremento mínimo debe ser mayor a cero.";
    if (!campos.fechaInicio) return "La fecha de inicio es obligatoria.";
    if (!campos.fechaFin) return "La fecha de cierre es obligatoria.";
    if (new Date(campos.fechaFin) <= new Date(campos.fechaInicio)) return "La fecha de cierre debe ser posterior a la de inicio.";
    return null;
}

async function publicar() {
    ocultarAlertas();

    const campos = {
        vendedorId: Number(document.getElementById("selectVendedor").value),
        titulo: document.getElementById("inputTitulo").value,
        descripcion: document.getElementById("inputDescripcion").value,
        urlImagen: document.getElementById("inputImagen").value.trim() || null,
        categoriaId: Number(document.getElementById("selectCategoria").value),
        precioBase: Number(document.getElementById("inputPrecioBase").value),
        incrementoMinimo: Number(document.getElementById("inputIncremento").value),
        fechaInicio: document.getElementById("inputFechaInicio").value,
        fechaFin: document.getElementById("inputFechaFin").value,
    };

    // Validación en el frontend antes de mandar al backend
    const error = validar(campos);
    if (error) {
        mostrarError(error);
        return;
    }

    const boton = document.getElementById("btnPublicar");
    boton.disabled = true;
    boton.textContent = "Publicando...";

    try {
        const respuesta = await fetch(`${API_BASE}/subastas`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                vendedorId: campos.vendedorId,
                categoriaId: campos.categoriaId,
                titulo: campos.titulo,
                descripcion: campos.descripcion,
                urlImagen: campos.urlImagen,
                precioBase: campos.precioBase,
                incrementoMinimo: campos.incrementoMinimo,
                fechaInicio: toIsoUtc(campos.fechaInicio),
                fechaFin: toIsoUtc(campos.fechaFin)
            })
        });

        const datos = await respuesta.json();

        if (respuesta.ok) {
            mostrarExito("¡Subasta publicada! Redirigiendo al catálogo...");
            setTimeout(() => { window.location.href = "index.html"; }, 2000);
        } else {
            mostrarError(datos.error || "No se pudo publicar la subasta.");
            boton.disabled = false;
            boton.textContent = "Publicar subasta";
        }
    } catch (error) {
        mostrarError("Error de conexión con el servidor.");
        boton.disabled = false;
        boton.textContent = "Publicar subasta";
    }
}

document.getElementById("btnPublicar").addEventListener("click", publicar);

cargarSelects();