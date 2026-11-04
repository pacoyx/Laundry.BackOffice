# Utiliza la imagen base de .NET SDK para compilar la aplicación
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copia los archivos de proyecto y restaura las dependencias
COPY *.csproj ./
RUN dotnet restore

# Copia el resto de los archivos y compila la aplicación
COPY . ./
RUN dotnet publish -c Release -o out

# Utiliza la imagen base de .NET Runtime para ejecutar la aplicación
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build --chown=$APP_UID:$APP_UID /app/out .

# FileStorage:Provider=Local (ver appsettings.json) escribe en ./Uploads (relativo a /app).
# WORKDIR crea /app como root antes del COPY --chown de arriba, asi que el usuario sin
# privilegios no puede crear esa carpeta en tiempo de ejecucion si no existe de antemano.
RUN mkdir -p /app/Uploads && chown -R $APP_UID:$APP_UID /app/Uploads

# Exponer el puerto en el que la aplicación escuchará
EXPOSE 80

# Configurar variables de entorno
ENV ASPNETCORE_URLS=http://+:80

# Ejecutar como usuario sin privilegios (recomendado por Microsoft para imagenes .NET)
USER $APP_UID

# Comando para ejecutar la aplicación
ENTRYPOINT ["dotnet", "24dex-backend-comercial.dll"]