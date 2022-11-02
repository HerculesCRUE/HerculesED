![](../../../Docs/media/CabeceraDocumentosMD.png)

| Fecha         | 21/10/2022                                                  |
| ------------- | ------------------------------------------------------------ |
|Titulo|API Hercules.ED.FigShare| 
|Descripción|Manual del servicio API Hercules.ED.FigShare para la importación de recursos desde FigShare|
|Versión|1.0|
|Módulo|Hercules.ED.ExternalSources|
|Tipo|Manual|
|Cambios de la Versión| Versión inicial |

## Sobre Hercules.ED.FigShare

[![SonarCloud](https://sonarcloud.io/images/project_badges/sonarcloud-white.svg)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.FigShare)

[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.FigShare&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.FigShare)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.FigShare&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.FigShare)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.FigShare&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.FigShare)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.FigShare&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.FigShare)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.FigShare&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.FigShare)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.FigShare&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.FigShare)

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
    "Token": "",
	"LogPath": ""
}
```

- LogLevel.Default: Nivel de error por defecto.
- LogLevel.Microsoft: Nivel de error para los errores propios de Microsoft.
- LogLevel.Microsoft.Hosting.Lifetime: Nivel de error para los errores de host.
- Token: Token de FigShare.
- LogPath: Ruta de guardado del fichero de logs.

## Dependencias
- **Serilog.AspNetCore**: v4.1.0
- **Swashbuckle.AspNetCore**: v6.2.3