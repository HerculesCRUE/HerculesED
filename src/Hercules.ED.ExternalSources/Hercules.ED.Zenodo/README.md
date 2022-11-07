![](../../../Docs/media/CabeceraDocumentosMD.png)

| Fecha         | 21/10/2022                                                  |
| ------------- | ------------------------------------------------------------ |
|Titulo|API Hercules.ED.Zenodo| 
|Descripción|Manual del servicio API Hercules.ED.Zenodo para la importación de recursos desde Zenodo|
|Versión|1.0|
|Módulo|Hercules.ED.ExternalSources|
|Tipo|Manual|
|Cambios de la Versión| Versión inicial|

## Sobre Hercules.ED.Zenodo

[![SonarCloud](https://sonarcloud.io/images/project_badges/sonarcloud-white.svg)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.Zenodo)

[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.Zenodo&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.Zenodo)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.Zenodo&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.Zenodo)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.Zenodo&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.Zenodo)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.Zenodo&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.Zenodo)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.Zenodo&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.Zenodo)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.Zenodo&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.Zenodo)

## Descripción.
Servicio encargado de obtener la información de Zenodo. Documentación del API de [Zenodo](https://developers.zenodo.org/). 

## Controladores

**APIController**  

[GET] GetROsByOrcid -> Obtiene los datos de una publicación.  
pOrcid: Código ORCID de la publicación.  
Resultado: Lista de objetos [ResearchObject](https://github.com/HerculesCRUE/HerculesED/blob/main/src/Hercules.ED.ExternalSources/Hercules.ED.Zenodo/Models/Data/ResearchObject.cs).

[GET] GetOntologyData -> Obtiene los datos necesarios para la carga en BBDD.  
pOrcid: Código ORCID de la publicación.  
Resultado: Lista de objetos [OntologyRO](https://github.com/HerculesCRUE/HerculesED/blob/main/src/Hercules.ED.ExternalSources/Hercules.ED.Zenodo/Models/Data/OntologyRO.cs).

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
	"UrlBaseEnriquecimiento": ""
}
```

- LogLevel.Default: Nivel de error por defecto.
- LogLevel.Microsoft: Nivel de error para los errores propios de Microsoft.
- LogLevel.Microsoft.Hosting.Lifetime: Nivel de error para los errores de host.
- LogPath: Ruta de guardado del fichero de logs.
- UrlBaseEnriquecimiento: URL base del API de Enriquecimiento.

## Dependencias
- **Newtonsoft.Json**: v13.0.1
- **Serilog.AspNetCore**: v4.1.0
- **Swashbuckle.AspNetCore**: v6.2.1
- **System.Net.Http.Json**: v5.0.0
