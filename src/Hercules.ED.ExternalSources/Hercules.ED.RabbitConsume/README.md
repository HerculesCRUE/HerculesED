![](../../../Docs/media/CabeceraDocumentosMD.png)

| Fecha         | 02/11/2022                                                  |
| ------------- | ------------------------------------------------------------ |
|Titulo|API Hercules.ED.RabbitConsume| 
|Descripción|Manual del servicio API Hercules.ED.RabbitConsume|
|Versión|1.0|
|Módulo|Hercules.ED.ExternalSources|
|Tipo|Manual|
|Cambios de la Versión| Versión inicial |

## Sobre Hercules.ED.RabbitConsume

[![SonarCloud](https://sonarcloud.io/images/project_badges/sonarcloud-white.svg)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.RabbitConsume)

[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.RabbitConsume&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.RabbitConsume)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.RabbitConsume&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.RabbitConsume)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.RabbitConsume&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.RabbitConsume)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.RabbitConsume&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.RabbitConsume)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.RabbitConsume&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.RabbitConsume)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.RabbitConsume&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.RabbitConsume)

## Funcionamiento
El consumidor se va a encargar de leer de la cola de Rabbit y hacer unas acciones u otras dependiendo del dato leído. El mensaje recibido será una lista en formato string, la cual va a tener los siguientes valores:
- listaString[0] determina el tipo del mensaje para saber a que controlador ha de hacer la petición. Los posibles valores son los siguientes:
	- doi: Hace la petición a fuentes externas únicamente por DOI. Es requerido que en listaString[3] esté el nombre del autor y en listaString[1] el código DOI y listaString[2] el ID del recurso.
	- investigador: Hace la petición a fuentes externas únicamente por ORCID. Es requerido que en listaString[1] esté el código ORCID y en listaString[2] la fecha y listaString[3] el ID del recurso.
	- publicación: Hace la petición a fuentes externas únicamente por DOI. Es requerido que en listaString[1] esté el DOI de la publicación.
	- zenodo: Hace la petición a Zenodo. Es requerido que en listaString[1] tenga el ORCID.
	- figshare: Hace la petición a FigShare. Es requerido que en listaString[1] tenga el token de FigShare.
	- github: Hace la petición a GitHub. Es requerido que en listaString[1] tenga el token de GitHub y listaString[2] el usuario de GitHub.

Al identificar dónde ha de hacer la petición, hace la llamada al servicio correspondiente y obtiene los datos en JSON en formato string. Posteriormente, guarda los datos en un fichero JSON en la ruta indicada en el appsettings.

En el caso que todo haya funcionado correctamente, quitará la petición de la cola y continuará con la siguiente. Si falla al leer la petición, escribirá en el log y la volverá a encolar.

Finalmente, modificará la fecha de actualización de la persona que ha solicitado dicha actualización de datos.

## Configuración en el appsetting.json
```json{
{
	"ConnectionStrings": {
		"RabbitMQ": ""
	},
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

- ConnectionStrings.RabbitMQ: Cadena de conexión de la cola.
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
