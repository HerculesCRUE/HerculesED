![](../Docs/media/CabeceraDocumentosMD.png)

| Fecha         | 21/10/2022                                                   |
| ------------- | ------------------------------------------------------------ |
|Título|Servicio de sincronización| 
|Descripción|Servicio encargado de sincronizar los datos con fuentes externas mediante los ORCID|
|Versión|1.1|
|Módulo|Documentación|
|Tipo|Especificación|
|Cambios de la Versión|Modificada documentación|


[![SonarCloud](https://sonarcloud.io/images/project_badges/sonarcloud-white.svg)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.Synchronization)

[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.Synchronization&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.Synchronization)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.Synchronization&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.Synchronization)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.Synchronization&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.Synchronization)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.Synchronization&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.Synchronization)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.Synchronization&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.Synchronization)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.Synchronization&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.Synchronization)




# Hércules ED. Servicio de sincronización
Este servicio se encarga de obtener los códigos ORCID de los investigadores de la plataforma para recuperar información de sus fuentes externas. 
El proceso estará determinado por una expresión cron que marcará su ejecución. Al obtener los ORCID, los introducirá a una cola RabbitMQ para que la tarea en encargada de leer dicha cola los procese.

Configuración del appsettings.json
============
```json
{
  "ConnectionStrings": {
    "RabbitMQ": ""
  },
  "FuentesExternasQueueRabbit": "",
  "LogPath": "",
  "cronExternalSource": ""
}
```
- RabbitMQ: Cadena de conexión de Rabbit.
- FuentesExternasQueueRabbit: Nombre de la cola para la inserción.
- LogPath: Ruta dónde se van a almacenar lo logs.
- cronExternalSource: Expresión cron para la ejecución del programa.

Dependencias
============
- GnossApiWrapper.NetCore: v6.0.2
- Quartz: v3.4.0
- RabbitMQ.Client: v6.4.0
