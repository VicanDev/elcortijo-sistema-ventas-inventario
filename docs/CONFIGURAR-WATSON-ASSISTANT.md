# Configurar IBM watsonx Assistant para El Cortijo

La aplicaci&oacute;n ya tiene el widget de IBM preparado en `/Asistente`. Mientras falten los identificadores, esa p&aacute;gina conserva el asistente local. Cuando se configure IBM, mostrar&aacute; el chat de watsonx Assistant a pantalla completa.

## 1. Crear el asistente en IBM Cloud

1. Inicia sesi&oacute;n o crea tu cuenta en [IBM Cloud](https://cloud.ibm.com/).
2. Crea una instancia de **watsonx Assistant** con el plan Lite, si est&aacute; disponible para tu cuenta y regi&oacute;n.
3. Dentro del asistente, crea un Action llamado **Consultar precio y stock**. Configura preguntas de ejemplo: "&iquest;Tienen Canbo?", "&iquest;Cu&aacute;nto cuesta PROD-005?" y "&iquest;Hay stock de Ricocan?".
4. Para que los datos sean actuales, despliega El Cortijo en una URL p&uacute;blica HTTPS y crea una extensi&oacute;n OpenAPI desde `docs/watson-stock-openapi.yml`; reemplaza `https://TU-DOMINIO-PUBLICO` por el dominio desplegado. Agrega la extensi&oacute;n como paso de llamada al servicio del Action y pasa a `buscar` el nombre, marca o c&oacute;digo que escribi&oacute; el usuario.
5. En la respuesta del Action, usa `productos` para informar nombre, `precio`, `unidadMedida` y `stock`; si `productos` est&aacute; vac&iacute;o, indica que no hubo coincidencias y pide probar un nombre m&aacute;s corto o c&oacute;digo. Publica el Action en el entorno usado por Web chat.
6. Abre la integraci&oacute;n **Web chat** del asistente y copia `integrationID`, `region` y `serviceInstanceID` de la pesta&ntilde;a de incrustaci&oacute;n.

> La extensi&oacute;n debe alcanzar el sitio desde IBM. `localhost` no es accesible desde IBM Cloud; el archivo OpenAPI es una plantilla y no se puede probar hasta tener un dominio HTTPS p&uacute;blico.

## 2. Conectar la web

No guardes credenciales ni claves privadas en Git. En la consola donde ejecutas la aplicaci&oacute;n configura estos valores (los IDs del web chat no son una API key):

```powershell
$env:WatsonAssistant__Enabled = "true"
$env:WatsonAssistant__IntegrationId = "PEGA_EL_INTEGRATION_ID"
$env:WatsonAssistant__Region = "PEGA_LA_REGION"
$env:WatsonAssistant__ServiceInstanceId = "PEGA_EL_SERVICE_INSTANCE_ID"
dotnet run --project src\ElCortijo.Web\ElCortijo.Web.csproj
```

Abre `/Asistente`. Si alguno de los tres IDs falta o `Enabled` no es `true`, la p&aacute;gina muestra el asistente local como respaldo.

En despliegue, define las mismas variables en la configuraci&oacute;n segura del hosting. No subas los valores personales de tu cuenta ni tokens al repositorio.

## 3. Prueba manual

- Pregunta por una marca (por ejemplo, `Canbo`) y verifica que el Action use la extensi&oacute;n y responda con productos disponibles.
- Pregunta por un c&oacute;digo existente (por ejemplo, `PROD-005`) y comprueba precio/unidad/stock.
- Pregunta por un producto inexistente y comprueba que el bot ofrezca una alternativa de b&uacute;squeda.
- Modifica stock/precio en el sistema y repite la consulta para confirmar que la respuesta no sea un dato fijo.

## Referencias oficiales

- [Incrustar Web chat](https://cloud.ibm.com/docs/watson-assistant?topic=watson-assistant-deploy-web-chat)
- [Personalizar y desarrollar Web chat](https://cloud.ibm.com/docs/watson-assistant?topic=watson-assistant-web-chat-develop)
