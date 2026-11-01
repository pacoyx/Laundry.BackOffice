# 24dex Backend Comercial

Backend comercial de **Lavandería Kyo**, construido en **.NET 9 (ASP.NET Core)**. Expone la API que da soporte a las operaciones del negocio: ventas y facturación por sucursal, recojos y entregas a domicilio (Pickup), lavado de calzado (Sneakers), gestión de clientes, portal de autoservicio para clientes, recursos humanos/nómina, compras, cuentas por cobrar/pagar, impresión de tickets y guías, y reportes/dashboards.

## Tecnologías principales

- **.NET 9** / ASP.NET Core Minimal APIs
- **Entity Framework Core 9** + SQL Server
- **MediatR** (CQRS: commands/queries) y **AutoMapper**
- **FluentValidation** para validación de requests
- **JWT Bearer** para autenticación y RBAC (roles y permisos) para autorización, con soporte de horario de acceso por usuario (restricción opcional de días/horas, independiente del rol)
- **SignalR** (`/printhub`) para impresión en tiempo real sobre nodos de impresión (worker nodes)
- **QuestPDF** para generación de PDFs (guías, comprobantes)
- **Serilog** (consola, archivo, Application Insights, Seq) para logging estructurado
- **OpenTelemetry** + Azure Monitor para trazas y métricas
- **Health Checks UI** (`/health`) para monitoreo de disponibilidad
- **Swagger / OpenAPI** para documentación de la API (solo en Development)
- **Azure Blob Storage** / almacenamiento local para adjuntos y evidencias

## Estructura del proyecto

El código sigue una organización por **features** (vertical slices), cada una con sus propios commands, queries, DTOs y endpoints:

```
Features/        Módulos de negocio (uno por carpeta), p. ej.:
  Customer, CustomerPortal, Company, CorporateOperations,
  CollectionGuide, CollectionSheets, WorkGuideMain, PickupMobile,
  BranchSales, BrachSalesUser, CashBox, ExpenseBox,
  Invoices, AccountsReceivable, Purchase, PaymentMethod,
  Sneakers, ClothingItem, ClothingWorker, Inspections, LocationClothes,
  HumanResources, Payroll, Attendance, WorkShift,
  Printer, PrinterDiscovery, PrinterHub, PrintJobs, WorkerNode,
  Dashboard, Reports, Security, User, Login, Tickets, Documents, Health...

Modules/          Infraestructura transversal (Health Check, Watch Dog)
ModuleExtensions/ Registro de servicios, rutas y políticas (DI, routing, caching)
Data/             DbContext y configuración de persistencia (EF Core)
Migrations/       Migraciones de base de datos
Models/           Entidades del dominio
DTOs/             Objetos de transferencia compartidos
Security/         Autenticación, RBAC (roles/permisos) y políticas
Middleware/       Middlewares HTTP personalizados
Transversal/      Utilidades y componentes compartidos
Scripts/          Scripts SQL de datos de ejemplo (seed)
```

## Puesta en marcha

### Requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- SQL Server (local o remoto)
- (Opcional) Docker

### Configuración

1. Copia `appsettings.json` / `appsettings.Development.json` y completa los valores sensibles (cadena de conexión, clave JWT, credenciales de email, almacenamiento, etc.), o usa `dotnet user-secrets`.
2. Aplica las migraciones de base de datos:

   ```bash
   dotnet ef database update
   ```

3. Ejecuta la aplicación:

   ```bash
   dotnet run
   ```

4. En entorno de Desarrollo, la documentación interactiva está disponible en Swagger UI, y el estado de salud en `/health`.

### Con Docker

```bash
docker build -t 24dex-backend-comercial .
docker run -p 8080:80 24dex-backend-comercial
```

## Facturación electrónica (NubeFact) — switch on/off

La integración con [NubeFact](https://www.nubefact.com/integracion) (proveedor de facturación electrónica / SUNAT) está lista en el código pero **apagada por defecto**, controlada por un switch en configuración. Mientras está apagada, el flujo de creación de facturas (`POST /api/invoices`) no cambia en absoluto: ni se llama a NubeFact, ni se encola ningún job en Hangfire, ni se escribe ninguna fila en `ElectronicInvoiceSubmissions`.

### Dónde está el switch

```json
"ElectronicInvoicing": {
  "Provider": "",
  "NubeFact": {
    "BaseUrl": "",
    "Token": ""
  }
}
```

- `Provider` vacío (o cualquier valor distinto de `"NubeFact"`) = **apagado**. Es el valor por defecto en `appsettings.json` y también en `appsettings.Development.json`.
- `Provider = "NubeFact"` = **encendido**. A partir de ahí, cada factura nueva se envía en background (Hangfire) a NubeFact.

### Cómo encenderlo para probar (cuenta demo)

`appsettings.Development.json` ya trae precargada la cuenta demo pública que da NubeFact en su documentación (no son credenciales reales de producción). Para activarlo en local:

1. Cambia `ElectronicInvoicing:Provider` a `"NubeFact"` en `appsettings.Development.json`.
2. Reinicia la API y crea una factura normalmente (Factura o Boleta; otros tipos de comprobante no están soportados).
3. Verifica el envío:
   - Dashboard de Hangfire: `/hangfire` (debe verse el job `SubmitInvoiceAsync` ejecutado).
   - `GET /api/invoices/{id}/electronic-submission` (requiere el permiso `electronic-invoicing.access`) — devuelve si NubeFact aceptó el comprobante, la URL del documento y la respuesta de SUNAT.
   - Tabla `ElectronicInvoiceSubmissions` en la base de datos (guarda cada intento, exitoso o no, incluyendo la respuesta cruda en `RawResponseJson`).

### Cómo apagarlo

Vuelve `ElectronicInvoicing:Provider` a `""` (o quita la clave). No hace falta tocar nada más: el hook en `CreateInvoiceEndpoint` deja de encolar el job y el sistema queda exactamente como antes de la integración.

### Para activarlo en producción con una cuenta real

Completar `ElectronicInvoicing:NubeFact:BaseUrl` y `Token` con los datos de la cuenta contratada y poner `Provider = "NubeFact"` en la configuración del entorno correspondiente (no en `appsettings.json` del repo). Un fallo en el envío a NubeFact nunca reintenta automáticamente (para no duplicar el envío a SUNAT) — queda registrado en `ElectronicInvoiceSubmissions` con el error para revisión manual.

## Documentación adicional

- [Roles y permisos (RBAC)](./docs/roles-permisos.md) — catálogo de permisos, matriz rol → permisos, cómo restringir el acceso de un usuario a submenús específicos, y el horario de acceso por usuario (independiente de roles).

## Licencia

Este proyecto es software propietario de **Lavandería Kyo**. Consulta el archivo [LICENSE](./LICENSE) para más detalles.
