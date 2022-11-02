![](../../../Docs/media/CabeceraDocumentosMD.png)

| Fecha         | 21/10/2022                                                  |
| ------------- | ------------------------------------------------------------ |
|Titulo|API Hercules.ED.SemanticScholar| 
|Descripción|Manual del servicio API Hercules.ED.SemanticScholar para la importación de recursos desde SemanticScholar|
|Versión|1.0|
|Módulo|Hercules.ED.ExternalSources|
|Tipo|Manual|
|Cambios de la Versión|Versión inicial |

## Sobre Hercules.ED.SemanticScholar

[![SonarCloud](https://sonarcloud.io/images/project_badges/sonarcloud-white.svg)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.SemanticScholar)

[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.SemanticScholar&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.SemanticScholar)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.SemanticScholar&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.SemanticScholar)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.SemanticScholar&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.SemanticScholar)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.SemanticScholar&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.SemanticScholar)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.SemanticScholar&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.SemanticScholar)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.SemanticScholar&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.SemanticScholar)

## Descripción.
Servicio encargado de obtener la información de SemanticScholar. Documentación del API de [SemanticScholar](https://api.semanticscholar.org/api-docs/graph#tag/Paper-Data/operation/get_graph_get_paper_search). 

## Controladores

**APIController**  
[GET] GetROs -> Obtiene los datos de una publicación.  
[GET] GetReferences -> Obtiene los datos de las referencias de una publicación.

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
	"LogPath": ""
}
```

- LogLevel.Default: Nivel de error por defecto.
- LogLevel.Microsoft: Nivel de error para los errores propios de Microsoft.
- LogLevel.Microsoft.Hosting.Lifetime: Nivel de error para los errores de host.
- LogPath: Ruta de guardado del fichero de logs.

## Dependencias
- **ClosedXML**: v0.95.4
- **ExcelDataReader.DataSet**: v3.6.0
- **Newtonsoft.Json**: v13.0.1
- **Serilog.AspNetCore**: v4.1.0
- **Swashbuckle.AspNetCore**: v6.2.1
- **System.Net.Http.Json**: v5.0.0
