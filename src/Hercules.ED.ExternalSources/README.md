
![](../../Docs/media/CabeceraDocumentosMD.png)

| Fecha         | 29/6/2022                                                   |
| ------------- | ------------------------------------------------------------ |
|Título|Librería de conexión con repositorios externos| 
|Descripción|Servicios de conexión con fuentes externas de información, para publicaciones y otros ROs|
|Versión|1.1|
|Módulo|Documentación|
|Tipo|Especificación|
|Cambios de la Versión|Modificada documentación|

# Hércules ED. Librería de conexión con repositorios externos

Este servicio se va a encargar de preguntar por los datos de fuentes externas y generar un json unificado con dichos datos. El fichero json se guarda en el servidor para que el [servicio de carga de datos de fuentes externas](https://github.com/HerculesCRUE/HerculesED/tree/main/src/Hercules.ED.ResearcherObjectLoad) se encargue de cargar los datos.

El funcionamiento de los servicios de extracción está documentado en [Fuentes externas de publicaciones científicas](https://confluence.um.es/confluence/pages/viewpage.action?pageId=397534572)

# Microservicios de fuentes externas

Todos los servicios principales encargados de obtener datos de fuentes primarias tienen como controlador principal GetROs, el cual necesita dos parametros que son el ORCID del investigador a consultar y la fecha (en formato YYYY-MM-DD) en la que se quiere obtener los datos.

[Web of Science (WoS)](https://github.com/HerculesCRUE/HerculesED/tree/main/src/Hercules.ED.ExternalSources/Hercules.ED.WoSConnect): Servicio encargado de preguntar al API de Clarivate por los datos de un investigador.

[Scopus](https://github.com/HerculesCRUE/HerculesED/tree/main/src/Hercules.ED.ExternalSources/Hercules.ED.ScopusConnect): Servicio encargado de preguntar al API de Scopus por los datos de un investigador. 

[OpenAire](https://github.com/HerculesCRUE/HerculesED/tree/main/src/Hercules.ED.ExternalSources/Hercules.ED.OpenAireConnect): Servicio encargado de preguntar al API de OpenAire por los datos de un investigador. 

[SemanticScholar](https://github.com/HerculesCRUE/HerculesED/tree/main/src/Hercules.ED.ExternalSources/Hercules.ED.SemanticScholar): Servicio encargado de preguntar al API de SemanticScholar. Únicamente se recuperará las referencias de la publicación.

[Zenodo](https://github.com/HerculesCRUE/HerculesED/tree/main/src/Hercules.ED.ExternalSources/Hercules.ED.Zenodo): Servicio encargado de preguntar al API de Zenodo. Únicamente se recuperará la url del pdf de la publicación.

Los microservicios de Scopus, WoS, OpenAire, SemanticScholar y Zenodo tienen un funcionamiento similar:  
- Desde la interfaz swagger de cada microservidor, se ejecuta el archivo APIcontroller del microservicio asociado. Dependiendo de la petición que realicemos en ese programa se ejecutara una función u otra de este programa. 
- Esta función (petición) llamara al programa RO**servidor_name**Logic, que realizara la petición al microservicio. 
- El texto devuelto será convertido a un objeto de C#, para ello el modelo devuelto por cada fuente externa se almacenará en el archivo  ROs/**servicor_name**/Models/ModeloInicial
- Se llamará a las funcionalidades  de RO**servidor_name**CambioModelo que nos permitirá cambiar el modelo devuelto, por la fuente externa, al modelo final deseado. (Más adelante se hablara más en detalle de cómo funciona este cambio de modelo).
    - Este modelo final es igual en todos los microservicios y está guardado en el archivo  ROs/*servidor_name*/Models/ROPublicationModel. Si se realiza una modificación de este fichero se deberá hacer en todos los microservicios también.
- Una vez tenemos el modelo final con toda la información simplemente se devuelve el modelo generado.
 
Respecto al cambio de modelo, en todos los microservicios se tienen las mismas funciones, denominadas del mismo modo, salvo que si esta fuente no nos proporciona informacion sobre dicho metadatado entonces esa función ha sido comentada/eliminada. En cada microservidor, estas funciones serán diferentes dependiendo de donde este la información necesaria en el modelo devuelto por la fuente externa. Esta información será recogida y puesta en el formato apropiado que el modelo final necesita, por eso en cada microservidor están funciones son diferentes. Estas funciones estan definidas en ROs/**nombre_servidor**/Controllers/RO**nombre_servidor**CambioModelo.cs. 

# Microservicio de publicaciones 

El microservicio más distintivo y diferente es el de publicación, en él se llama de una forma específica, siguiendo el algoritmo diseñado, a los microservicios descritos anteriormente. La mayor diferencia es que en este caso en vez de cambiar el modelo, las publicaciones obtenidas por las diversas fuentes externas van a ir convergiendo de una forma específica según el diseño del algoritmo. De esta tarea en específico se encarga el código de *ROPublicactionLogic*.
 
Este microservicio también debe devolver la informacion en el formato final deseado, por lo que este modelo se almacena en ROs/Publication/Models/ROPublicacionModel.
- En este caso el formato es un poco diferente porque se han introducido dos entidades dentro del modelo que no están en los otros microservicios. Estas entidades son aquellas que modelan los metadatos enriquecidos. 

A continuación, se describen los pasos que se llevarán a cabo durante el proceso de reclamación de publicaciones de un determinado autor. 

## Función de combinar dos publicaciones **compactación**

Con esta función se combinan todos los metadatos de las publicaciones recibidas. Cada metadato se combina de forma independiente. En el caso de los autores se hace de modo que no permita duplicidad de usuarios en el mismo conjunto de colaboradores de la publicación.



## Función principal.

- Primeramente, el investigador ofrece su ORCID y una fecha a partir de la cual quiere obtener sus ROs.  
- Se llamará a los servicios de WoS, Scopus y OpenAire para obtener la información de las publicaciones principales de este autor. 
- Se recorre cada una de las publicaciones obtenidas en WoS. Por cada una de ellas: 
    - Se almacena el DOI en una lista para saber qué artículos ya hemos completado del investigador en cuestión. 
    - Se llama al servicio de SemanticScholar y se fusiona la información obtenida por este microservicio y la publicación que estamos examinando (función de combinar dos publicaciones). El resultado de esta unificación será la publicación que estamos observando. Esta fuente externa nos devuelve la información de los documentos referenciados. Estas publicaciones tendrán únicamente unos pocos metadatos básicos que no serán completados con ninguna red externa adicional. 
    - Se llama a la fuente externa Zenodo y en caso de encontrarse un fichero PDF con la publicación se añadirá como metadato.
    - Se llama al enriquecimiento de áreas temáticas y de palabras clave para completar la publicación. 
    - Se recorren todos los documentos obtenidos por Scopus y para cada uno de ellos:
        - Si el DOI de esta publicación coincide con la publicación que estamos examinando entonces se combina la información (función de combinar dos publicaciones). Se asigna como prioridad WoS > Scopus.
        - En caso contrario no se hace nada.
    - Se recorren todos los documentos obtenidos en OpenAire y para cada uno de ellos:
        - Si el DOI de esta publicación coincide con la publicación que estamos examinando entonces se combina la información (función de combinar dos publicaciones). Se asigna como prioridad WoS > Scopus > OpenAire.
        - En caso contrario no se hace nada.
        - Llegados a este punto la publicación central está completa, así como todas las bibliográficas que la componen. Se guarda para devolverse. 
- Recorremos la lista de publicaciones de Scopus con el fin de completar aquellas que no se han obtenido de WoS. Por tanto, por cada una de las publicaciones:
    - Si ya ha sido completada y almacenada antes, no hace nada con ella. 
    - En caso contrario: 
        - Se llama al servicio de SemanticScholar y se fusiona la información obtenida por este microservicio y la publicación que estamos examinando (función de combinar dos publicaciones). El resultado de esta unificación será la publicación que estamos observando. Esta fuente externa nos devuelve la información de los documentos referenciados. Estas publicaciones tendrán únicamente unos pocos metadatos básicos que no serán completados con ninguna red externa adicional. 
        - Se llama a la fuente externa Zenodo y en caso de encontrarse un fichero PDF con la publicación se añadirá como metadato. 
        - Se llama al enriquecimiento de áreas temáticas y de palabras clave para completar la publicación. 
- Recorrimos la lista de publicaciones de OpenAire con el fin de completar aquellas que no se han obtenido de WoS y Scopus. Por tanto, por cada una de las publicaciones:
    - Si ya ha sido completada y almacenada antes, no hace nada con ella. 
    - En caso contrario: 
        - Se llama al servicio de SemanticScholar y se fusiona la información obtenida por este microservicio y la publicación que estamos examinando (función de combinar dos publicaciones). El resultado de esta unificación será la publicación que estamos observando. Esta fuente externa nos devuelve la información de los documentos referenciados. Estas publicaciones tendrán únicamente unos pocos metadatos básicos que no serán completados con ninguna red externa adicional.        
        - Se llama a la fuente externa Zenodo y en caso de encontrarse un fichero PDF con la publicación se añadirá como metadato.        
        - Se llama al enriquecimiento de áreas temáticas y de palabras clave para completar la publicación. 
- Llegados a este punto ya tenemos completas todas las publicaciones de este autor. 

## SLEP
A su vez, hay servicios adicionales capaces de enriquecer la información del usuario. Para activar dichos servicios hay que ir al menú lateral al apartado de configuración de fuentes externas:

![image](https://user-images.githubusercontent.com/88077103/191783029-c4de9988-177a-45af-b43b-68055e0d89da.png)

Una vez dentro, el usuario puede configurar de que fuentes adicionales puede enriquecer sus datos:

![image](https://user-images.githubusercontent.com/88077103/191783246-84123805-50ca-4a62-8065-9d5b0d14fcd3.png)

- [FigShare](https://github.com/HerculesCRUE/HerculesED/tree/main/src/Hercules.ED.ExternalSources/Hercules.ED.FigShare): Obtiene ROs de presentaciones y documentos.
- [GitHub](https://github.com/HerculesCRUE/HerculesED/tree/main/src/Hercules.ED.ExternalSources/Hercules.ED.GitHubConnect): Obtiene ROs de código.
- [Matching](https://github.com/HerculesCRUE/HerculesED/tree/main/src/Hercules.ED.Matching): Permite enriquecer las etiquetas de ámbito médico de las publicaciones del usuario.

Al pulsar sobre el botón de "Guardar" (situado en la parte inferior izquierda), el sistema por detrás obtendrá los ROs configurados y aplicará el sistema de matching a las publicaciones del usuario.

Configuración del appsettings.json
============
```json
{
  "AllowedHosts": "*",
  "LogPath": "",
  "UrlWos": "",
  "UrlScopus": "",
  "UrlOpenAire": "",
  "UrlSemanticScholar": "",
  "UrlZenodo": "",
  "RutaJsonSalida": "",
  "UrlEnriquecimiento": ""
}
```
- LogPath: Ruta dónde se van a almacenar lo logs.
- UrlWos: URL dónde está el instalado el microservicio de WoS.
- UrlScopus: URL dónde está el instalado el microservicio de Scopus.
- UrlOpenAire: URL dónde está el instalado el microservicio de OpenAire.
- UrlSemanticScholar: URL dónde está el instalado el microservicio de SemanticScholar.
- UrlZenodo: URL dónde está el instalado el microservicio de Zenodo.
- RutaJsonSalida: Ruta dónde se va a guardar el json generado.
- UrlEnriquecimiento: URL dónde está instalado el servicio de enriquecimiento.

Dependencias
============
- ClosedXML: v0.95.4
- ExcelDataReader.DataSet: v3.6.0
- Newtonsoft.Json: v13.0.1
- Serilog.AspNetCore: v4.1.0
- Swashbuckle.AspNetCore: v6.2.1
- System.Net.Http.Json: v5.0.0
