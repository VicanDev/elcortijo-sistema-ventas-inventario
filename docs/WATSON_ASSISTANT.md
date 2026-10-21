# IBM watsonx Assistant para El Cortijo

La aplicación incluye el widget web y un endpoint que consulta precio y stock en tiempo real. No se guardan credenciales ni identificadores personales en Git.

## 1. Crear y preparar el asistente

1. Inicia sesión en IBM Cloud y crea una instancia gratuita o de prueba de **watsonx Assistant / watsonx Orchestrate**.
2. Crea un asistente llamado `El Cortijo` y abre su entorno de borrador.
3. Crea una acción `Consultar precio y stock` con frases como `¿Cuánto cuesta el Ricocan?`, `¿Hay Whiskas?` y `Consulta el stock de PROD-001`.
4. Solicita el nombre o código del alimento y guárdalo en una variable `producto`.

## 2. Conectar la consulta dinámica

1. Publica temporalmente la aplicación en una URL HTTPS accesible desde Internet.
2. Reemplaza `https://REEMPLAZAR-DOMINIO-PUBLICO` en `docs/watson-extension-openapi.json` por esa URL.
3. En IBM, crea una extensión personalizada importando ese archivo OpenAPI.
4. En la acción agrega **Call an extension**, selecciona `consultarProductos` y asigna `buscar` a `producto`.
5. Muestra `productos[0].nombre`, `productos[0].precio`, `productos[0].stock` y `productos[0].unidadMedida`. Si `encontrados` es `0`, solicita otro nombre.

La API es `GET /api/chatbot/productos?buscar=Ricocan` y devuelve como máximo cinco productos activos. Nunca expone precio de compra ni datos privados.

## 3. Activar el widget

En IBM abre **Integrations > Web chat > Embed** y copia `integrationID`, `region` y `serviceInstanceID` del fragmento generado.

Para desarrollo, configura `src/ElCortijo.Web/appsettings.Development.json` sin confirmar identificadores reales al repositorio:

```json
{
  "WatsonAssistant": {
    "Enabled": true,
    "IntegrationId": "TU_INTEGRATION_ID",
    "Region": "us-south",
    "ServiceInstanceId": "TU_SERVICE_INSTANCE_ID"
  }
}
```

En despliegue usa `WatsonAssistant__Enabled`, `WatsonAssistant__IntegrationId`, `WatsonAssistant__Region` y `WatsonAssistant__ServiceInstanceId` como variables de entorno.

Reinicia la aplicación. El botón del chat aparecerá en todas las páginas del layout principal.

## 4. Prueba de aceptación

Guarda capturas de estas conversaciones: consulta de precio, consulta de stock por código, producto inexistente y consulta ambigua.

Los identificadores del Web Chat son visibles en el navegador por diseño. Para una publicación real, habilita la seguridad JWT y la restricción de origen desde IBM.
