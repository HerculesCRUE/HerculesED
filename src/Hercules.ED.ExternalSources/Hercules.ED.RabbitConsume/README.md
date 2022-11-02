![](../../../Docs/media/CabeceraDocumentosMD.png)

| Fecha         | 21/10/2022                                                  |
| ------------- | ------------------------------------------------------------ |
|Titulo|API Hercules.ED.RabbitConsume| 
|Descripción|Manual del servicio API Hercules.ED.RabbitConsume|
|Versión|1.0|
|Módulo|Hercules.ED.ExternalSources|
|Tipo|Manual|
|Cambios de la Versión| |

## Sobre Hercules.ED.RabbitConsume

[![SonarCloud](https://sonarcloud.io/images/project_badges/sonarcloud-white.svg)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.RabbitConsume)

[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.RabbitConsume&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.RabbitConsume)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.RabbitConsume&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.RabbitConsume)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.RabbitConsume&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.RabbitConsume)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.RabbitConsume&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.RabbitConsume)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.RabbitConsume&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.RabbitConsume)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.RabbitConsume&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.RabbitConsume)

## Configuración en el appsetting.json
```json{
{
	"Logging": {
		"LogLevel": {
			"Default": "",
			"Microsoft": "",
			"Microsoft.Hosting.Lifetime": ""
		}
	},
	"AllowedHosts": "*",
	"FuentesExternasQueueRabbit": "",
	"UrlPublicacion": "",
	"DirectorioEscritura": "",
	"UrlZenodo": "",
	"UrlFigShare": "",
	"UrlGitHub": "",
	"LogPath": ""
}
```

- LogLevel.Default: Nivel de error por defecto.
- LogLevel.Microsoft: Nivel de error para los errores propios de Microsoft.
- LogLevel.Microsoft.Hosting.Lifetime: Nivel de error para los errores de host.
- FuentesExternasQueueRabbit: Nombre de la cola Rabbit de configuración.
- UrlPublicacion: URL del API de Publicacion.
- DirectorioEscritura: Ruta de escritura de los ficheros.
- UrlZenodo: URL del API de Zenodo.
- UrlFigShare: URL del API de FigShare.
- UrlGitHub: URL del API de GitHub.
- LogPath: Ruta de guardado del fichero de logs.

## Dependencias
- **GnossApiWrapper.NetCore**: v6.0.6
- **Microsoft.Extensions.Hosting**: v6.0.1
- **Newtonsoft.Json**: v13.0.1
- **RabbitMQ.Client**: v6.2.3
- **Serilog**: v2.10.0
- **Serilog.AspNetCore**: v5.0.0