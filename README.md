# elcortijo-sistema-ventas-inventario
Sistema de Gestión de Ventas y Control de Inventario para la Distribuidora El Cortijo - EFSRT V Cibertec

## Asistente, pruebas y CI

- La configuración del asistente IBM watsonx y sus pasos de activación están en `docs/CONFIGURAR-WATSON-ASSISTANT.md`. Sin IDs de IBM configurados, la página conserva el asistente local.
- Las pruebas de integración de ventas usan SQLite en memoria y no requieren una instancia SQL Server: `dotnet test ElCortijo.sln`.
- GitHub Actions restaura, compila y ejecuta las pruebas en cada push y pull request.
