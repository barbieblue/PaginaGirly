# SubastaYa 🏷️

Plataforma web de subastas en tiempo real con billetera virtual, sistema de garantías (Escrow), regla Anti-Sniping y concurrencia optimista.

**Integrantes:**
- Melany Ailén Piriz
- Bárbara Mariel Pantano de Marco

---

## Tecnologías utilizadas

| Capa | Tecnología |
|---|---|
| Backend | C# / ASP.NET Core 8 / Web API |
| ORM | Entity Framework Core (Code-First) |
| Base de datos | SQL Server LocalDB |
| Frontend | HTML / CSS / JavaScript (Vanilla) |
| Documentación API | Swagger / OpenAPI |
| Proceso en segundo plano | BackgroundService (.NET) |

---

## Paquetes NuGet

Los siguientes paquetes se restauran automáticamente al ejecutar `dotnet restore`:

| Paquete | Versión | Uso |
|---|---|---|
| Microsoft.EntityFrameworkCore.SqlServer | 8.0.0 | Proveedor de SQL Server para EF Core |
| Microsoft.EntityFrameworkCore.Design | 8.0.0 | Herramientas en tiempo de diseño para migraciones |
| Microsoft.EntityFrameworkCore.Tools | 8.0.0 | Comandos CLI para migraciones (`dotnet ef`) |
| Microsoft.Extensions.Hosting.Abstractions | 8.0.0 | Soporte para BackgroundService (Worker) |
| Swashbuckle.AspNetCore | 6.6.2 | Generación automática de Swagger / OpenAPI |

---

## Requisitos previos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 (incluye SQL Server LocalDB automáticamente)

> **Nota:** No se requiere instalar SQL Server por separado. LocalDB viene incluido con Visual Studio.

---

## Pasos para levantar el proyecto

### 1. Clonar el repositorio

```bash
git clone https://github.com/barbieblue/PaginaGirly.git
cd PaginaGirly
```

### 2. Restaurar dependencias

```bash
dotnet restore
```

### 3. Aplicar las migraciones (crea la base de datos)

```bash
dotnet ef database update --project SubastaYa.Infraestructura --startup-project PaginaGirly
```

Este comando crea la base de datos `SubastaYaDb` en LocalDB y aplica la migración `Init` con el esquema completo.

### 4. Levantar el proyecto

```bash
cd PaginaGirly
dotnet run
```

El servidor arranca en `http://localhost:5137`.

> **Datos de prueba:** Al iniciar por primera vez, el sistema carga automáticamente los datos semilla (usuarios, billeteras, categorías y subastas de prueba) definidos en `SeedData.cs`.

### 5. Acceder a la aplicación

| URL | Descripción |
|---|---|
| `http://localhost:5137` | Frontend (catálogo de subastas) |
| `http://localhost:5137/swagger` | Documentación interactiva de la API |

---

## Usuarios de prueba (Seed Data)

| Email | Rol | Saldo Total | Retenido | Disponible |
|---|---|---|---|---|
| vendedor@test.com | Vendedor | $0 | $0 | $0 |
| comprador1@test.com | Postor líder | $150.000 | $45.000 | $105.000 |
| comprador2@test.com | Postor habilitado | $200.000 | $0 | $200.000 |
| sinfondos@test.com | Sin fondos | $500 | $0 | $500 |

Los IDs de usuario se asignan en el orden de la tabla (vendedor = 1, comprador1 = 2, comprador2 = 3, sinfondos = 4).

---

## Arquitectura

El proyecto sigue el **Patrón de Capas** con **CQRS** y un **Mediator propio** (sin librerías externas):

```
PaginaGirly/               → Presentación (Controllers, wwwroot)
SubastaYa.Servicios/       → Aplicación (Commands, Queries, Handlers)
SubastaYa.Dominio/         → Dominio (Entidades, Interfaces, Excepciones de negocio)
SubastaYa.Infraestructura/ → Infraestructura (DbContext, Repositorios, Workers, Migraciones)
```

### Decisiones de diseño destacadas

- **Unit of Work:** todas las operaciones críticas se confirman en un único `SaveChangesAsync`, garantizando atomicidad (ACID).
- **Optimistic Locking:** las entidades `Subasta` y `Billetera` tienen una columna `Version` de tipo `rowversion` (SQL Server). EF Core emite `UPDATE ... WHERE Version = @original`; si otra transacción modificó la fila primero, lanza `DbUpdateConcurrencyException` que se convierte en `409 Conflict`.
- **BackgroundWorker:** `ProcesosCierreSubastas` corre cada 30 segundos, detecta subastas vencidas y ejecuta la liquidación final o las marca como DESIERTA, todo en una transacción atómica por subasta.
- **Short-Polling:** el frontend consulta `GET /api/subastas/{id}` cada 3 segundos para mantener el temporizador y el historial de pujas actualizados en tiempo real.

---

## Prueba de Concurrencia (Stress Test)

El script `test-concurrencia.ps1` lanza dos peticiones de puja sobre la misma subasta **en paralelo** usando `HttpClient` con `Task.WaitAll`, simulando dos usuarios pujando en el mismo instante.

### Cómo ejecutar

1. Levantar el proyecto (`dotnet run`)
2. Crear una subasta nueva sin pujas desde Swagger (`POST /api/Subastas`) y anotar el `id` devuelto
3. Actualizar `$subastaId` en el script con ese id
4. Ejecutar desde PowerShell:

> **Nota:** Windows bloquea la ejecución de scripts `.ps1` por defecto. Antes de correr el script, ejecutar este comando en la misma ventana de PowerShell:
> ```powershell
> Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
> ```
> Esto habilita los scripts solo para esa sesión, sin cambiar la configuración global del sistema.

```powershell
.\test-concurrencia.ps1
```

### Resultado esperado

```
=== Peticion 1 (usuario 2) ===
Status: Conflict
{"error":"Conflicto de concurrencia: el estado de la subasta cambió. Intentá pujar de nuevo."}

=== Peticion 2 (usuario 3) ===
Status: OK
{"mensaje":"Puja registrada exitosamente"}
```

### Mecanismo

EF Core emite `UPDATE Subastas SET ... WHERE Id = @id AND Version = @versionOriginal`. Si dos transacciones leen la misma `Version` y ambas intentan guardar, la segunda encuentra que la `Version` ya cambió, EF lanza `DbUpdateConcurrencyException` y el handler lo convierte en `409 Conflict`.

---

## Endpoints principales

| Método | URL | Descripción |
|---|---|---|
| GET | `/api/Subastas` | Listado con filtros por estado y categoría |
| GET | `/api/Subastas/{id}` | Detalle de una subasta |
| POST | `/api/Subastas` | Crear nueva subasta |
| POST | `/api/Subastas/{id}/bids` | Registrar una puja |
| GET | `/api/billetera/balance?usuarioId={id}` | Saldo desglosado |
| POST | `/api/billetera/deposit` | Acreditar fondos |
| GET | `/api/billetera/transactions?usuarioId={id}` | Historial de movimientos |
| GET | `/api/auditoria` | Log de eventos del sistema |

La documentación completa con esquemas de request/response está disponible en Swagger (`/swagger`).
