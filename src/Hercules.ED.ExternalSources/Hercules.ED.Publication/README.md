![](../../../Docs/media/CabeceraDocumentosMD.png)

| Fecha         | 21/10/2022                                                  |
| ------------- | ------------------------------------------------------------ |
|Titulo|API Hercules.ED.Publication| 
|Descripción|Manual del servicio API Hercules.ED.Publication para la importación de recursos desde Publication|
|Versión|1.0|
|Módulo|Hercules.ED.ExternalSources|
|Tipo|Manual|
|Cambios de la Versión| |

## Sobre Hercules.ED.Publication

[![SonarCloud](https://sonarcloud.io/images/project_badges/sonarcloud-white.svg)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.Publication)

[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.Publication&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.Publication)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.Publication&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.Publication)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.Publication&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.Publication)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.Publication&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.Publication)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.Publication&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.Publication)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.Publication&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.Publication)

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
	"LogPath": "",
	"UrlWos": "",
	"UrlScopus": "",
	"UrlOpenAire": "",
	"UrlCrossRef": "",
	"UrlOpenCitations": "",
	"UrlSemanticScholar": "",
	"UrlZenodo": "",
	"RutaJsonSalida": "",
	"UrlEnriquecimiento": ""
}
```

- LogLevel.Default: Nivel de error por defecto.
- LogLevel.Microsoft: Nivel de error para los errores propios de Microsoft.
- LogLevel.Microsoft.Hosting.Lifetime: Nivel de error para los errores de host.
- LogPath: Ruta de guardado del fichero de logs.
- UrlWos: URL del servicio API de Wos.
- UrlScopus: URL del servicio API de Scopus.
- UrlOpenAire: URL del servicio API de OpenAire.
- UrlCrossRef: URL del servicio API de CrossRef.
- UrlOpenCitations: URL del servicio API de OpenCitations.
- UrlSemanticScholar: URL del servicio API de SemanticScholar.
- UrlZenodo: URL del servicio API de Zenodo.
- RutaJsonSalida: Ruta de gardado del JSON (Usada para depuración).
- UrlEnriquecimiento: URL del servicio de enriquecimiento.

## Dependencias
- **ClosedXML**: v0.95.4
- **ExcelDataReader.DataSet**: v3.6.0
- **Newtonsoft.Json**: v13.0.1
- **Serilog.AspNetCore**: v4.1.0
- **Swashbuckle.AspNetCore**: v6.2.1
- **System.Net.Http.Json**: v5.0.0