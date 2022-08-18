![](../../Docs/media/CabeceraDocumentosMD.png)

| Fecha         | 29/06/2022                                                   |
| ------------- | ------------------------------------------------------------ |
|Título|Servicio de desnormalización de datos| 
|Descripción|Proceso encargado de generar datos desnormalizados para su consulta, búsqueda yrepresentación|
|Versión|1.0|
|Módulo|Documentación|
|Tipo|Especificación|
|Cambios de la Versión|Versión inicial|

# Servicio de desnormalización de datos

El Desnormalizador es un proceso encargado de generar datos desnormalizados para su consulta, búsqueda y representación; tanto en Hércules ED como en [Hércules MA](https://github.com/HerculesCRUE/HerculesMA).
El Desnormalizador tambien es encargado de eliminar los datos innecesarios del sistema, como las notificaciones antiguas.

## Dependencias
- **dotNetRDF**: v2.7.2
- **GnossApiWrapper.NetCore**: v1.0.6
- **Quartz**: v3.4.0
- **RabbitMQ.Client**: v6.2.4
- **System.Net.Http.Formatting.Extension**: v5.2.3