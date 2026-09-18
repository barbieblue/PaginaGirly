using SubastaYa.Dominio;

namespace SubastaYa.Infraestructura
{
    public static class SeedData
    {
        public static void Inicializar(SubastaYaDbContext context)
        {
            // Guarda de seguridad: si ya hay usuarios cargados, no vuelvas a sembrar
            // (si no, cada vez que arranques el proyecto se duplicarían los datos).
            if (context.Usuarios.Any())
            {
                return;
            }

            // ---------- 4 CATEGORÍAS ----------
            var catTecnologia = new Categoria { Nombre = "Tecnología", Url_Icono = "tecnologia.png" };
            var catColeccionables = new Categoria { Nombre = "Coleccionables", Url_Icono = "coleccionables.png" };
            var catIndumentaria = new Categoria { Nombre = "Indumentaria", Url_Icono = "indumentaria.png" };
            var catVehiculos = new Categoria { Nombre = "Vehículos", Url_Icono = "vehiculos.png" };

            context.Categorias.AddRange(catTecnologia, catColeccionables, catIndumentaria, catVehiculos);

            // ---------- 4 USUARIOS ----------
            var vendedor = new Usuario { Email = "vendedor@test.com", Nombre = "Vendedor Test", Password_Hash = "hash-simulado", Fecha_Registro = DateTime.UtcNow };
            var comprador1 = new Usuario { Email = "comprador1@test.com", Nombre = "Comprador Uno", Password_Hash = "hash-simulado", Fecha_Registro = DateTime.UtcNow };
            var comprador2 = new Usuario { Email = "comprador2@test.com", Nombre = "Comprador Dos", Password_Hash = "hash-simulado", Fecha_Registro = DateTime.UtcNow };
            var sinFondos = new Usuario { Email = "sinfondos@test.com", Nombre = "Sin Fondos", Password_Hash = "hash-simulado", Fecha_Registro = DateTime.UtcNow };

            context.Usuarios.AddRange(vendedor, comprador1, comprador2, sinFondos);

            context.SaveChanges();

            // ---------- 4 BILLETERAS (según los montos exactos que pide la consigna) ----------
            var billeteraVendedor = new Billetera { Usuario_Id = vendedor.Id, Saldo_Total = 0, Saldo_Retenido = 0 };
            var billeteraComprador1 = new Billetera { Usuario_Id = comprador1.Id, Saldo_Total = 150000, Saldo_Retenido = 45000 };
            var billeteraComprador2 = new Billetera { Usuario_Id = comprador2.Id, Saldo_Total = 200000, Saldo_Retenido = 23000 };
            var billeteraSinFondos = new Billetera { Usuario_Id = sinFondos.Id, Saldo_Total = 500, Saldo_Retenido = 0 };

            context.Billeteras.AddRange(billeteraVendedor, billeteraComprador1, billeteraComprador2, billeteraSinFondos);
            context.SaveChanges();

            var ahora = DateTime.UtcNow;

            // ---------- 5 SUBASTAS (casos de prueba pedidos por la consigna) ----------

            // Activa estándar: cierra en 20-30 min, líder actual $45.000
            var subastaActivaEstandar = new Subasta
            {
                Vendedor_Id = vendedor.Id,
                Categoria_Id = catTecnologia.Id,
                Titulo = "Notebook Gamer",
                Descripcion = "Notebook usada en buen estado, ideal para gaming.",
                Url_Imagen = "https://via.placeholder.com/300",
                Precio_Base = 30000,
                Incremento_Minimo = 5000,
                Fecha_Inicio = ahora.AddHours(-1),
                Fecha_Fin = ahora.AddMinutes(25),
                Estado = "ACTIVA"
            };

            // Activa crítica: cierra en menos de 2 min (para probar anti-sniping)
            var subastaActivaCritica = new Subasta
            {
                Vendedor_Id = vendedor.Id,
                Categoria_Id = catColeccionables.Id,
                Titulo = "Figura de colección edición limitada",
                Descripcion = "Figura sellada, nunca abierta.",
                Url_Imagen = "https://via.placeholder.com/300",
                Precio_Base = 10000,
                Incremento_Minimo = 1000,
                Fecha_Inicio = ahora.AddHours(-2),
                Fecha_Fin = ahora.AddSeconds(90),
                Estado = "ACTIVA"
            };

            // Próxima: inicio programado a +24hs (pujas bloqueadas todavía)
            var subastaProxima = new Subasta
            {
                Vendedor_Id = vendedor.Id,
                Categoria_Id = catIndumentaria.Id,
                Titulo = "Campera de cuero vintage",
                Descripcion = "Campera de colección, talle M.",
                Url_Imagen = "https://via.placeholder.com/300",
                Precio_Base = 15000,
                Incremento_Minimo = 2000,
                Fecha_Inicio = ahora.AddHours(24),
                Fecha_Fin = ahora.AddHours(48),
                Estado = "PROGRAMADA"
            };

            // Vencida con ganador: fecha fin pasada + tuvo una puja ganadora
            var subastaVencidaConGanador = new Subasta
            {
                Vendedor_Id = vendedor.Id,
                Categoria_Id = catVehiculos.Id,
                Titulo = "Bicicleta rodado 29",
                Descripcion = "Poco uso, service reciente.",
                Url_Imagen = "https://via.placeholder.com/300",
                Precio_Base = 20000,
                Incremento_Minimo = 3000,
                Fecha_Inicio = ahora.AddDays(-3),
                Fecha_Fin = ahora.AddMinutes(-10),
                Estado = "ACTIVA" // el Worker la va a pasar a FINALIZADA
            };

            // Vencida desierta: fecha fin pasada, nunca tuvo pujas
            var subastaVencidaDesierta = new Subasta
            {
                Vendedor_Id = vendedor.Id,
                Categoria_Id = catTecnologia.Id,
                Titulo = "Teclado mecánico usado",
                Descripcion = "Funciona bien, le faltan dos teclas de repuesto.",
                Url_Imagen = "https://via.placeholder.com/300",
                Precio_Base = 8000,
                Incremento_Minimo = 1000,
                Fecha_Inicio = ahora.AddDays(-2),
                Fecha_Fin = ahora.AddMinutes(-5),
                Estado = "ACTIVA" // el Worker la va a pasar a DESIERTA
            };

            context.Subastas.AddRange(
                subastaActivaEstandar,
                subastaActivaCritica,
                subastaProxima,
                subastaVencidaConGanador,
                subastaVencidaDesierta);

            context.SaveChanges();

            // ---------- HISTORIAL: 2 pujas previas en la subasta activa estándar ----------
            var puja1 = new Puja
            {
                Subasta_Id = subastaActivaEstandar.Id,
                Comprador_Id = comprador2.Id,
                Monto = 35000,
                Fecha_Puja = ahora.AddMinutes(-40)
            };
            var puja2 = new Puja
            {
                Subasta_Id = subastaActivaEstandar.Id,
                Comprador_Id = comprador1.Id,
                Monto = 45000,
                Fecha_Puja = ahora.AddMinutes(-20)
            };

            // Puja ganadora en la subasta ya vencida
            var pujaGanadora = new Puja
            {
                Subasta_Id = subastaVencidaConGanador.Id,
                Comprador_Id = comprador2.Id,
                Monto = 23000,
                Fecha_Puja = ahora.AddDays(-2)
            };

            context.Pujas.AddRange(puja1, puja2, pujaGanadora);
            context.SaveChanges();

            // ---------- LIBRO CONTABLE (Ledger): respalda depósitos y el saldo retenido de $45.000 ----------
            context.Transacciones.AddRange(
                new Transaccion_Ledger { Billetera_Id = billeteraComprador1.Id, Tipo = "DEPOSITO", Monto = 150000, Fecha = ahora.AddDays(-5) },
                new Transaccion_Ledger { Billetera_Id = billeteraComprador1.Id, Tipo = "RETENCION", Monto = 45000, Fecha = puja2.Fecha_Puja, Subasta_Id = subastaActivaEstandar.Id },
                new Transaccion_Ledger { Billetera_Id = billeteraComprador2.Id, Tipo = "DEPOSITO", Monto = 200000, Fecha = ahora.AddDays(-5) },   
                new Transaccion_Ledger { Billetera_Id = billeteraComprador2.Id, Tipo = "RETENCION", Monto = 23000, Fecha = pujaGanadora.Fecha_Puja, Subasta_Id = subastaVencidaConGanador.Id },
                new Transaccion_Ledger { Billetera_Id = billeteraSinFondos.Id, Tipo = "DEPOSITO", Monto = 500, Fecha = ahora.AddDays(-5) }
            );

            context.SaveChanges();
        }
    }
}