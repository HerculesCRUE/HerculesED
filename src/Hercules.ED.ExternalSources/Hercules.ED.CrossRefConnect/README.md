![](../../../Docs/media/CabeceraDocumentosMD.png)

| Fecha         | 02/11/2022                                                  |
| ------------- | ------------------------------------------------------------ |
|Titulo|API Hercules.ED.CrossRefConnect| 
|Descripción|Manual del servicio API Hercules.ED.CrossRefConnect para la importación de recursos|
|Versión|1.0|
|Módulo|Hercules.ED.ExternalSources|
|Tipo|Manual|
|Cambios de la Versión| Inicial |

## Sobre Hercules.ED.CrossRefConnect

[![SonarCloud](https://sonarcloud.io/images/project_badges/sonarcloud-white.svg)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.CrossRefConnect)

[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.CrossRefConnect&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.CrossRefConnect)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.CrossRefConnect&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.CrossRefConnect)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.CrossRefConnect&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.CrossRefConnect)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.CrossRefConnect&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.CrossRefConnect)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.CrossRefConnect&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.CrossRefConnect)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.CrossRefConnect&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.CrossRefConnect)

## Descripción.
Servicio encargado de obtener la información de CrossRef. Documentación del API de [CrossRef](https://www.crossref.org/documentation/retrieve-metadata/rest-api/). 

## Controladores

**ROCrossRefController**
[GET] GetROs
- Resumen: Obiene los datos de una publicación
- Parámetros: 
	- **DOI**: El identificador DOI de la publicación a obtener sus datos
- Devuelve: Un objeto que sigue este [modelo](https://github.com/HerculesCRUE/HerculesED/blob/main/src/Hercules.ED.ExternalSources/Hercules.ED.CrossRefConnect/ROs/CrossRef/Models/ROPublicationModel.cs)

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
- **ExcelDAtaReader.DataSet**: v3.6..0
- **Newtonsoft.Json**: v13.0.1
- **Serilog.AspNetCore**: v4.1.0
- **Swashbuckle.AspNetCore**: v6.2.1
- **System.Net.Http.Json**: 5.0.0
