![](../../../Docs/media/CabeceraDocumentosMD.png)

| Fecha         | 21/10/2022                                                  |
| ------------- | ------------------------------------------------------------ |
|Titulo|API Hercules.ED.ScopusConnect| 
|Descripción|Manual del servicio API Hercules.ED.ScopusConnect para la importación de recursos desde Scopus|
|Versión|1.0|
|Módulo|Hercules.ED.ExternalSources|
|Tipo|Manual|
|Cambios de la Versión|Versión inicial|

## Sobre Hercules.ED.ScopusConnect

[![SonarCloud](https://sonarcloud.io/images/project_badges/sonarcloud-white.svg)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.ScopusConnect)

[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.ScopusConnect&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.ScopusConnect)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.ScopusConnect&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.ScopusConnect)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.ScopusConnect&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.ScopusConnect)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.ScopusConnect&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.ScopusConnect)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.ScopusConnect&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.ScopusConnect)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.ScopusConnect&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.ScopusConnect)

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