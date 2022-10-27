![](../../Docs/media/CabeceraDocumentosMD.png)

| Fecha         | 28/6/2022                                                   |
| ------------- | ------------------------------------------------------------ |
|Título|Servicio matching de descriptores específicos| 
|Descripción|Servicio encargado de hacer el matching de los descriptores específicos de las publicaciones|
|Versión|1.0|
|Módulo|Documentación|
|Tipo|Especificación|
|Cambios de la Versión|Versión inicial|


[![SonarCloud](https://sonarcloud.io/images/project_badges/sonarcloud-white.svg)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.Matching)

[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.Matching&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.Matching)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.Matching&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.Matching)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.Matching&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.Matching)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.Matching&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.Matching)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.Matching&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.Matching)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.Matching&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.Matching)



Introducción
============
El servicio de matching se ocupa de matchear los descriptores específicos de las publicaciones con términos de MESH y SNOMED para ofrecer más información. En Hércules ED esta función va a ser offline, es decir, se mandará a ejecutar e irá trabajando por detrás. Obtendrá todas las publicaciones de BBDD en las que no se haya hecho el matching e irá consultando a MESH y SNOMED para la recuperación de información. Una vez obtenidos los datos, se mostrarán en las fichas de Hércules MA.

El análisis funcional del proceso de matching (enlazado) se puede consultar en [Flujo e interfaces del enriquecimiento. Descriptores y Matching](https://confluence.um.es/confluence/display/HERCULES/Flujo+e+interfaces+del+enriquecimiento.+Descriptores+y+Matching#Flujoeinterfacesdelenriquecimiento.DescriptoresyMatching-Matching).

MESH - Búsqueda por "Exact Match"
============================
Este proceso va a consistir en el que la búsqueda se hace por palabras exactas, es decir, el descriptor se busca tal y como esté guardado en la publicación.

MESH - Búsqueda por "All fragments"
============================
En el caso que no se haya detectado la etiqueta en "Exact Match" se procederá a la búsqueda por "All fragments". En este formato se buscará por pares de palabras, obviando las posibles preposiciones en inglés/castellano.

SNOMED - Búsqueda mediante el ID de MESH
============================
Una vez finalizada la búsqueda en MESH, si se ha obtenido algún dato, se habrá guardado el código SNOMED relacionado. Con este código, se puede obtener la información de SNOMED.

Inserción/Modificación de datos
===========================
Finalmente, se procede a insertar o modificar las etiquetas de base de datos con la información obtenida. En el caso que no se haya obtenido nada, no se hará nada.

Configuración en el appsettings.json
====================================
```json
{
  "SparqlEndpoint": "",
  "ApiKey": "",
  "UrlTGT": "https://utslogin.nlm.nih.gov/cas/v1/api-key",
  "UrlTicket": "https://utslogin.nlm.nih.gov/cas/v1/tickets",
  "UrlSNOMED": "https://uts-ws.nlm.nih.gov/rest/crosswalk/current/source/MSH",
  "UrlRelaciones": "https://uts-ws.nlm.nih.gov/rest/content/current/source/SNOMEDCT_US"
}
```
- SparqlEndpoint: Endpoint de Sparql dónde se realizarán las consultas.
- ApiKey: Clave de acceso.
- UrlTGT: URL para obtener el Ticket-Granting Ticket. 
- UrlTicket: URL para obtener el Service Ticket. 
- UrlSNOMED: URL para hacer las peticiones al servicio de SNOMED. 
- UrlRelaciones: URL para obtener las reaciones de las etiquetas SNOMED. 

Dependencias
============
- EnterpriseLibrary.Data.NetCore: v6.3.2
- GnossApiWrapper.NetCore: v1.0.8
- HtmlAgilityPack: v1.11.42
- Newtonsoft.Json: v13.0.1
