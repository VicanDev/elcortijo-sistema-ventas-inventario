# El Cortijo — ventas e inventario

Sistema de gestión de ventas y control de inventario para la Distribuidora El Cortijo — EFSRT V Cibertec.

## Puesta en marcha

Requisitos:

- .NET 9 SDK (el repositorio fija la familia 9 en `global.json`).
- SQL Server 2016 SP1 o posterior, con una instancia local iniciada.
- `sqlcmd` instalado y el usuario de Windows autorizado para crear la base de datos y conectarse a ella.

La cadena de conexión de desarrollo usa por defecto la instancia predeterminada (`localhost`) y autenticación integrada de Windows. Si instalaste SQL Server Express, normalmente la instancia es `localhost\SQLEXPRESS`. Usa el mismo nombre de servidor al ejecutar los scripts y al iniciar la aplicación.

Ejecuta los scripts en este orden:

```powershell
sqlcmd -S localhost -E -b -i database\01_schema.sql
sqlcmd -S localhost -E -b -i database\02_seeds.sql
sqlcmd -S localhost -E -b -i database\03_stored_procedures.sql
```

Para SQL Server Express, sustituye `localhost` por `localhost\SQLEXPRESS` en los tres comandos. Ejecuta los scripts una sola vez y en ese orden; `01_schema.sql` crea la base `BDElCortijo` y los siguientes scripts cargan los datos de demostración y procedimientos.

Después ejecuta:

```powershell
dotnet restore ElCortijo.sln
dotnet run --project src\ElCortijo.Web\ElCortijo.Web.csproj
```

Si usas SQL Server Express, configura la instancia en la sesión actual de PowerShell antes de ejecutar la app:

```powershell
$env:ConnectionStrings__CadenaSQL = "Server=localhost\SQLEXPRESS;Database=BDElCortijo;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
dotnet run --project src\ElCortijo.Web\ElCortijo.Web.csproj
```

La variable solo afecta esa sesión y permite cambiar la conexión sin editar ni subir credenciales al repositorio. Si SQL Server usa autenticación SQL, configura la cadena con variables de entorno y no guardes la contraseña en `appsettings.json`.

Credenciales de demostración: usuario `admin` y contraseña `Admin123*`. Los usuarios `vendedor` y `almacen` utilizan la misma contraseña únicamente en el entorno académico.

## Pruebas y CI

Las pruebas de integración usan SQLite en memoria, por lo que no modifican la base de desarrollo:

```powershell
dotnet test ElCortijo.sln --configuration Release
```

GitHub Actions restaura, compila y ejecuta toda la suite en cada Pull Request y en cada envío a las ramas principales. Los resultados y la cobertura se publican como artefacto.

## Chatbot

La integración de IBM watsonx Assistant está desactivada hasta configurar los identificadores de la instancia. Consulta [docs/WATSON_ASSISTANT.md](docs/WATSON_ASSISTANT.md) para crear el asistente, conectar la consulta dinámica de precio/stock y activar el widget.

## Landing pública

La ruta principal `/` presenta El Cortijo, muestra métricas y productos destacados directamente desde la base de datos y ofrece acceso al sistema interno. El panel administrativo continúa disponible después de iniciar sesión.
