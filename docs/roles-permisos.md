# Roles y permisos (RBAC)

Documento de referencia para mantener mapeado qué puede hacer cada rol en el sistema. Generarlo manualmente evita tener que leer el código cada vez que se necesita saber "¿qué ve este rol?".

> Si agregas, quitas o reasignas un permiso, **actualiza este archivo en el mismo cambio**.

## Modelo de datos

```
User  (N) ---- (N) Role  (N) ---- (N) Permission
         UserRole              RolePermission
```

- Un usuario puede tener **varios roles** (`UserRole`); sus permisos son la **unión** de los permisos de todos sus roles.
- Los permisos **no se asignan directamente a un usuario**, solo a través de roles.
- No existe tabla de "menú" en el backend: la relación permiso → ítem de menú vive en el código del frontend.

Tablas: `Roles`, `Permissions`, `RolePermissions`, `UserRoles` (`Models/Security/`).
Gestión vía API (requiere permiso `settings.access`): `/api/roles`, `/api/permissions`, `/api/users/{id}/roles`.

## Catálogo de permisos

Definidos en `Security/PermissionCodes.cs`, sembrados en `Data/Seeding/Seeders/PermissionSeeder.cs`.

| Código | Descripción |
|---|---|
| `dashboard.access` | Acceso al dashboard general |
| `reception.access` | **(legado)** Permiso genérico de Recepción. Ya no lo valida ningún endpoint; se mantiene sembrado para no romper asignaciones existentes. Ver [Granularidad de Recepción](#granularidad-de-recepción). |
| `reception.emision.access` | Emisión de guías en Recepción |
| `reception.retiro.access` | Retiro de prendas en Recepción |
| `reception.consultas.access` | Consultas en Recepción |
| `reception.caja.access` | Caja en Recepción |
| `reception.gastos.access` | Gastos en Recepción |
| `reception.maestros.access` | Maestros (catálogos) en Recepción |
| `customers.access` | Módulo de Clientes |
| `invoices.access` | Módulo de Facturación |
| `purchases.access` | Módulo de Compras |
| `expenses.access` | Módulo de Gastos (general, no confundir con Gastos de Recepción) |
| `employees.access` | Módulo de Recursos Humanos |
| `payroll.access` | Módulo de Planillas |
| `suppliers.access` | Módulo de Proveedores |
| `reports.access` | Módulo de Reportes |
| `settings.access` | Configuración del sistema (incluye gestión de roles/permisos) |
| `corporate-operations.access` | Módulo de Operaciones Corporativas |
| `pickup_mobile.access` | App móvil de recojos (PWA Pickup Mobile) |

## Granularidad de Recepción

El módulo de Recepción se dividió en 6 permisos independientes (antes era uno solo, `reception.access`, que daba acceso a todo). Mapeo permiso ↔ endpoints/submenú:

| Permiso | Submenú | Endpoints que protege |
|---|---|---|
| `reception.emision.access` | Emisión | `POST /api/workGuideMain` (crear guía), `DELETE /api/workGuideMain/{id}`, `PUT .../cancelWorkGuide/{id}`, `PUT .../cancelItem/{id}`, `POST .../createAlert`, `POST .../{id}/reprint`, `POST /api/collectionGuide` |
| `reception.retiro.access` | Retiro | `PUT .../pickUpItem/{id}`, `PUT .../returnUnwashedClothes/{id}`, `PUT .../pickupDate/{id}` |
| `reception.consultas.access` | Consultas | `GET /api/workGuideMain/{id}`, `GET /api/workGuideMain`, `GET .../document/{serie}/{numero}`, `GET .../getAlertsByWorkGuide/{id}`, `/api/ticket/*`, `/api/customerGarmentHistory/*` |
| `reception.caja.access` | Caja | `/api/cashBox/*`, `PUT .../updateWorkGuideInfoPay/{id}` |
| `reception.gastos.access` | Gastos | `/api/expenseBox/*` |
| `reception.maestros.access` | Maestros | `/api/clothingItem/*`, `/api/clothingWorker/*`, `/api/locationclothes/*`, `/api/prodService/*`, `/api/catService/*`, `/api/workguide-details/*` (Sneakers e Inspecciones), `/api/sneaker-brands/*`, `/api/workguides/*` (resumen de inspecciones) |

Todos los endpoints de `WorkGuideMain` validan el permiso **por acción individual** (no a nivel de grupo), porque esa feature mezcla Emisión, Retiro, Consultas y Caja en el mismo controlador.

## Matriz Rol → Permisos

Fuente: `Data/Seeding/Seeders/RolePermissionSeeder.cs`.

| Rol | Permisos asignados |
|---|---|
| **Administrador** | Todos (`PermissionCodes.All`) |
| **Recepción** | Dashboard, Reception (legado), Emisión, Retiro, Consultas, Caja, Gastos, Maestros, Clientes, Facturación, Operaciones Corporativas |
| **Recepción Básico** | Dashboard, **solo** Emisión y Retiro |
| **Compras** | Dashboard, Compras, Proveedores |
| **RRHH** | Dashboard, Recursos Humanos, Planillas |
| **Gerencia** | Dashboard, Clientes, Facturación, Compras, Gastos, Recursos Humanos, Planillas, Reportes, Operaciones Corporativas |
| **Operaciones Corporativas** | Operaciones Corporativas |
| **Recolector** | Pickup Mobile (app móvil de recojos) |

### Ejemplo de uso: restringir un usuario de Recepción a solo Emisión/Retiro

1. Verificar que el rol **"Recepción Básico"** exista (se crea automáticamente al correr el seeding).
2. Quitarle al usuario el rol "Recepción" (si lo tenía) y asignarle "Recepción Básico":
   ```
   PUT /api/users/{userId}/roles
   { "roleIds": [idDeRecepcionBasico] }
   ```
   `UpdateUserRoles` **reemplaza** la lista completa de roles del usuario — no es incremental.

## Horario de acceso por usuario (independiente de roles/permisos)

Además del RBAC, existe una capa **ortogonal** que restringe el acceso de un usuario específico a un rango de horas/días, sin importar su rol o permisos. Por defecto **ningún usuario tiene restricción** — esto es opt-in por usuario.

### Modelo

Tabla `UserAccessSchedules` (`Models/Security/UserAccessSchedule.cs`): `UserId`, `DayOfWeek`, `StartTime`, `EndTime`, `IsActive`.

- **Sin filas para un usuario** → acceso sin restricción (comportamiento por defecto).
- **Con una o más filas** → solo puede acceder dentro de la unión de esos rangos. Un día de la semana sin ninguna fila queda completamente bloqueado ese día. Soporta múltiples rangos por día (turnos partidos).
- Todas las horas se evalúan en **horario de Perú (UTC-5 fijo, sin DST)** vía `Security/PeruClock.cs`, independiente de la zona horaria del servidor.
- No soporta rangos que cruzan medianoche (ej. 22:00–06:00) — cada rango debe estar dentro del mismo día.

### Dónde se aplica

1. **Login (`POST /login`) y refresh (`POST /refresh-token`)**: se consulta el horario del usuario; si está fuera de rango, se rechaza con `403` y no se emite token. Si tiene horario configurado, las ventanas se incrustan en el JWT como claims `accessWindow` (uno por rango, formato `"{diaSemana 0-6}:{HH:mm}-{HH:mm}"`).
2. **`Middleware/AccessScheduleMiddleware.cs`** (registrado en `Program.cs` entre `UseAuthentication` y `UseAuthorization`): en **cada request autenticado**, si el token trae claims `accessWindow`, se valida la hora actual contra esas ventanas sin consultar la base de datos. Fuera de rango → `403` inmediato, aunque el token JWT siga vigente (los tokens duran 60 min, así que sin este middleware un usuario podría seguir operando hasta 59 min después de su hora límite).

Esta capa se evalúa **aparte** de las políticas de permisos (`RequirePermission`) — un usuario puede tener todos los permisos que necesite y aun así quedar bloqueado por horario, o viceversa.

### Administración

Mismo patrón que roles/usuarios, protegido por `settings.access`:
- `GET /api/users/{id}/access-schedule`
- `PUT /api/users/{id}/access-schedule` (reemplaza la lista completa — lista vacía = quitar restricción)

## Pendiente / notas para el frontend

- El frontend debe mapear cada ítem del menú de Recepción a su permiso específico (`reception.emision.access`, `reception.retiro.access`, etc.) en vez de validar solo el permiso legado `reception.access`. Ese mapeo vive en el código Angular, no en este repo.
- El JWT y el endpoint `GET /me` exponen el array `permissions` con los códigos vigentes del usuario — es la fuente de verdad que debe consultar el front para mostrar/ocultar menús.
- No existe tabla `MenuItem` en el backend; si en el futuro se requiere que el menú sea 100% data-driven (sin releases de frontend para cambios de menú), evaluar crear esa tabla y un endpoint `/api/menu`.
