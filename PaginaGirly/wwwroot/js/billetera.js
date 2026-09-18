const API_BASE = "/api";
let usuarioSeleccionado = null;

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

async function cargarBilletera(usuarioId) {
    // Saldo
    const resSaldo = await fetch(`${API_BASE}/billetera/balance?usuarioId=${usuarioId}`);
    if (!resSaldo.ok) return;
    const saldo = await resSaldo.json();

    document.getElementById("saldoTotal").textContent = `$${saldo.saldoTotal.toLocaleString()}`;
    document.getElementById("saldoRetenido").textContent = `$${saldo.saldoRetenido.toLocaleString()}`;
    document.getElementById("saldoDisponible").textContent = `$${saldo.saldoDisponible.toLocaleString()}`;

    // Historial
    const resTrans = await fetch(`${API_BASE}/billetera/transactions?usuarioId=${usuarioId}`);
    if (!resTrans.ok) return;
    const transacciones = await resTrans.json();

    const contenedor = document.getElementById("historialMovimientos");

    if (!transacciones || transacciones.length === 0) {
        contenedor.innerHTML = `<p class="text-muted">Sin movimientos.</p>`;
        return;
    }

    const badgeColor = {
        "DEPOSITO": "success",
        "RETENCION": "warning",
        "LIBERACION": "info",
        "PAGO": "danger",
        "COBRO": "primary"
    };

    contenedor.innerHTML = `
        <table class="table table-hover">
            <thead>
                <tr>
                    <th>Tipo</th>
                    <th>Monto</th>
                    <th>Fecha</th>
                    <th>Subasta</th>
                </tr>
            </thead>
            <tbody>
                ${transacciones.map(t => `
                    <tr>
                        <td><span class="badge bg-${badgeColor[t.tipo] || 'secondary'}">${t.tipo}</span></td>
                        <td>$${t.monto.toLocaleString()}</td>
                        <td>${new Date(t.fecha + 'Z').toLocaleString()}</td>
                        <td>${t.subastaId ? `#${t.subastaId}` : '—'}</td>
                    </tr>
                `).join("")}
            </tbody>
        </table>
    `;
}

async function depositar() {
    const monto = Number(document.getElementById("inputDeposito").value);
    const alerta = document.getElementById("alertaDeposito");

    if (!monto || monto <= 0) {
        alerta.className = "alert alert-danger";
        alerta.textContent = "Ingresá un monto válido.";
        alerta.classList.remove("d-none");
        return;
    }

    const boton = document.getElementById("btnDepositar");
    boton.disabled = true;
    boton.textContent = "Depositando...";

    try {
        const respuesta = await fetch(`${API_BASE}/billetera/deposit`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ usuarioId: usuarioSeleccionado, monto })
        });

        const datos = await respuesta.json();

        if (respuesta.ok) {
            alerta.className = "alert alert-success";
            alerta.textContent = `¡Depósito exitoso! Nuevo saldo total: $${datos.saldoTotal.toLocaleString()}`;
            alerta.classList.remove("d-none");
            document.getElementById("inputDeposito").value = "";
            await cargarBilletera(usuarioSeleccionado);
        } else {
            alerta.className = "alert alert-danger";
            alerta.textContent = datos.mensaje || "No se pudo realizar el depósito.";
            alerta.classList.remove("d-none");
        }
    } catch (error) {
        alerta.className = "alert alert-danger";
        alerta.textContent = "Error de conexión con el servidor.";
        alerta.classList.remove("d-none");
    }

    boton.disabled = false;
    boton.textContent = "Depositar";
}

document.getElementById("selectUsuario").addEventListener("change", async (e) => {
    usuarioSeleccionado = Number(e.target.value);
    if (!usuarioSeleccionado) {
        document.getElementById("panelSaldo").classList.add("d-none");
        return;
    }
    document.getElementById("panelSaldo").classList.remove("d-none");
    await cargarBilletera(usuarioSeleccionado);
});

document.getElementById("btnDepositar").addEventListener("click", depositar);

cargarUsuarios();