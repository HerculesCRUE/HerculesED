![](../../Docs/media/CabeceraDocumentosMD.png)

| Fecha         | 10/12/2021                                                   |
| ------------- | ------------------------------------------------------------ |
|Título|Servicio de carga inicial de Curriculum Vitae (CV)| 
|Descripción|Descripción del sercicio de carga inicial del CV en Hércules|
|Versión|1.0|
|Módulo|Documentación|
|Tipo|Especificación|
|Cambios de la Versión|Versión inicial|


[![SonarCloud](https://sonarcloud.io/images/project_badges/sonarcloud-white.svg)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.LoadCV)

[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.LoadCV&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.LoadCV)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.LoadCV&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.LoadCV)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.LoadCV&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.LoadCV)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.LoadCV&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.LoadCV)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.LoadCV&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.LoadCV)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.LoadCV&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.LoadCV)



# Introducción
El servicio de carga inicial de CV, es el encargado de leer del sistema los archivos en formato PDF y cargar los datos guardados en el mismo, para posteriormente insertarlos en base de datos (BBDD) con el servicio de [Importación de CV](https://github.com/HerculesCRUE/HerculesED/tree/main/src/Hercules.ED.ImportExportCV).

# Servicios
- [Carga ORCID](#carga-orcid)
- [Carga CV](#carga-cv)

## Carga ORCID
Servicio encargado de la carga del ORCID de los usuarios. El servicio hará una petición al servicio de [Importación de CV](https://github.com/HerculesCRUE/HerculesED/tree/main/src/Hercules.ED.ImportExportCV), enviando el archivo del CV en formato PDF, para la lectura del ORCID del archivo y añadirá o actualizará el ORCID del usuario.

## Carga CV
Servicio encargado de la carga completa del CV de los usuarios. El servicio hará una petición al servicio de [Importación de CV](https://github.com/HerculesCRUE/HerculesED/tree/main/src/Hercules.ED.ImportExportCV), enviando el archivo del CV en formato PDF junto con el identificador del CV del usuario.

## Configuración en el appsetting.json
```json{
{
  "RutaCarpeta": "",
  "url_importador_exportador": ""
}
```

- RutaCarpeta: Ruta de lectura de la carpeta de archivos.
- url_importador_exportador: URL del servicio de Importación de CV.

## Dependencias
- **GnossApiWrapper.NetCore**: v6.0.7
- **Microsoft.Extensions.Hosting**: v6.0.1
