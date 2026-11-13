# Entorno de desarrollo local (LocalDB)

Pasos para levantar el backend en una PC nueva contra una base local, sin tocar UAT/producción.
Probado en Windows con SQL Server LocalDB (`MSSQLLocalDB`) y .NET SDK 9+.

## 1. Configuración local (user-secrets, no se commitea)

`appsettings.Development.json` apunta a `Server=.` con usuario `sa`, que no existe en una PC nueva.
En vez de editarlo, sobreescribir con **user-secrets** (solo vive en tu PC):

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\MSSQLLocalDB;Database=Db24Dexter_Dev;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;"

# Archivos (fotos, adjuntos) en disco local (carpeta Uploads/, ignorada por git).
# appsettings.Development.json dice AzureBlob pero sin ConnectionString, y asi la API no arranca.
dotnet user-secrets set "FileStorage:Provider" "Local"
```

Para probar contra Azure Blob real (ojo: escribe en el contenedor configurado):

```bash
dotnet user-secrets set "FileStorage:Provider" "AzureBlob"
dotnet user-secrets set "FileStorage:ConnectionString" "<cadena de conexion de la storage account>"
```

## 2. Crear la base y aplicar migraciones

```bash
sqllocaldb start MSSQLLocalDB
dotnet tool install --global dotnet-ef --version 9.0.8   # una sola vez
dotnet ef database update --context RecepcionDbContext
```

(`--context` es obligatorio: hay más de un DbContext en el ensamblado.)

## 3. Datos de prueba

```bash
sqlcmd -S "(localdb)\MSSQLLocalDB" -d Db24Dexter_Dev -C -I -f 65001 -i Scripts\seed-dev-data.sql
```

El `-I` es necesario (QUOTED_IDENTIFIER). Crea los usuarios `admin / Admin123!` y `cajero1 / Vendedor123!`.

Roles, permisos, categorías de activo fijo, etc. **no** están en el script: los siembran los
`IApplicationSeeder` al arrancar la API (paso 4), incluyendo asignar el rol Administrador a `admin`.

## 4. Levantar la API

```bash
dotnet run --launch-profile http        # http://localhost:5239
```

El entorno debe ser `Development` (el perfil `http` ya lo define). Si se corre con
`--no-launch-profile`, setear `ASPNETCORE_ENVIRONMENT=Development`; si no, no se leen los
user-secrets y Serilog falla al arrancar por la URL vacía de Seq en `appsettings.json`.

El frontend (`dex-front-comercial`, `ng serve`) ya apunta a `http://localhost:5239/` en desarrollo.
