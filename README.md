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

## Documentación adicional

- [Roles y permisos (RBAC)](./docs/roles-permisos.md) — catálogo de permisos, matriz rol → permisos, cómo restringir el acceso de un usuario a submenús específicos, y el horario de acceso por usuario (independiente de roles).

## Licencia

Este proyecto es software propietario de **Lavandería Kyo**. Consulta el archivo [LICENSE](./LICENSE) para más detalles.
