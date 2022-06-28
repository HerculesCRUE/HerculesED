![](../../Docs/media/CabeceraDocumentosMD.png)

| Fecha         | 28/6/2022                                                   |
| ------------- | ------------------------------------------------------------ |
|Título|Servicio matching de etiquetas| 
|Descripción|Servicio encargado de hacer el matching de las etiquetas de las publicaciones.|
|Versión|1.0|
|Módulo|Documentación|
|Tipo|Especificación|
|Cambios de la Versión|Versión inicial|

Introducción
============
El servicio de matching se ocupa de matchear las etiquetas de las publicaciones con términos de Mesh para ofrecer más información. Mesh es una plataforma de ámbito médico, con lo cual todas las etiquetas que se vayan a enriquecer serán de dicho carácter.

Búsqueda por "Exact Match"
============================
Este proceso va a consistir en el que la búsqueda se hace por palabras exactas, es decir, el descriptor se busca tal y como esté guardado en la publicación.

Búsqueda por "All fragments"
============================
En el caso que no se haya detectado la etiqueta en "Exact Match" se procederá a la búsqueda por "All frgments". En este formato se buscará por pares de palabras, obviando las posibles preposiciones en inglés/castellano.

Inserción/Modificación de datos
===========================
Una vez obtenida la información, se procede a insertar o modificar las etiquetas de base de datos con la información obtenida.

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
- UrlRelaciones: URL para obtener ls reaciones de las etiquetas SNOMED. 

Dependencias
============
- EnterpriseLibrary.Data.NetCore: v6.3.2
- GnossApiWrapper.NetCore: v1.0.8
- HtmlAgilityPack: v1.11.42
- Newtonsoft.Json: v13.0.1
