![](../../../Docs/media/CabeceraDocumentosMD.png)

| Fecha         | 21/10/2022                                                  |
| ------------- | ------------------------------------------------------------ |
|Titulo|API Hercules.ED.OpenAireConnect| 
|Descripción|Manual del servicio API Hercules.ED.OpenAireConnect para la importación de recursos desde OpenAire|
|Versión|1.0|
|Módulo|Hercules.ED.ExternalSources|
|Tipo|Manual|
|Cambios de la Versión| Versión inicial |

## Sobre Hercules.ED.OpenAireConnect

[![SonarCloud](https://sonarcloud.io/images/project_badges/sonarcloud-white.svg)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.OpenAireConnect)

[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.OpenAireConnect&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.OpenAireConnect)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.OpenAireConnect&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.OpenAireConnect)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.OpenAireConnect&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.OpenAireConnect)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.OpenAireConnect&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.OpenAireConnect)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.OpenAireConnect&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.OpenAireConnect)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.OpenAireConnect&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.OpenAireConnect)

## Descripción.
Servicio encargado de obtener la información de OpenAire. Documentación del API de [OpenAire](https://graph.openaire.eu/develop/). 

## Controladores

### APIController

**[GET] GetROs**
- Resumen: Obtiene los datos de las publicaciones de un autor.  
- Parámetros: 
	- orcid: El identificador ORCID del autor
	- date: La fecha en formato "YYYY-MM-DD" desde la que se quiere obtener
- Devuelve: Un objeto que sigue este [modelo](https://github.com/HerculesCRUE/HerculesED/blob/main/src/Hercules.ED.ExternalSources/Hercules.ED.OpenAireConnect/ROs/OpenAire/Models/ROPublicationModel.cs)

**[GET] GetRoByDoi**
- Resumen: Obtiene los datos de una publicación.
- Parámetros: 
	- pDoi: El identificador DOI de la publicación a obtener sus datos
- Devuelve: Un objeto que sigue este [modelo](https://github.com/HerculesCRUE/HerculesED/blob/main/src/Hercules.ED.ExternalSources/Hercules.ED.OpenAireConnect/ROs/OpenAire/Models/ROPublicationModel.cs)

## Configuración en el appsetting.json
```json{
{
	"AllowedHosts": "*",
}
```

- LogLevel.Default: Nivel de error por defecto.
- LogLevel.Microsoft: Nivel de error para los errores propios de Microsoft.
- LogLevel.Microsoft.Hosting.Lifetime: Nivel de error para los errores de host.
- LogPath: Ruta de guardado del fichero de logs.

## Dependencias
- **ClosedXML**: v0.95.4
- **ExcelDataReader**: v3.6.0
- **ExcelDataReader.DataSet**: v3.6.0
- **Newtonsoft.Json**: v13.0.1
- **Swashbuckle.AspNetCore**: v6.2.1
- **System.Net.Http.Json**: v5.0.0
