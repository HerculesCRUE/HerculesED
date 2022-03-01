Librería de conexión con repositorios externos
 

El funcionamiento de los servicios de extracción está documentado en [Fuentes externas de publicaciones científicas](https://confluence.um.es/confluence/pages/viewpage.action?pageId=397534572)

# Microservicios de fuentes externas

Los microservicios de Scopus, WoS, CrossRef, OpenAire, OpenCitations, Semantic Scholar y Zenodo tienen un funcionamiento similar:  
- Desde la interfaz swagger de cada microservidor, se ejecuta el archivo APIcontroller del microservicio asociado. Dependiendo de la petición que realicemos en ese programa se ejecutara una función u otra de este programa. En la última fila de la Tabla -- podemos observar la petición http que ejecuta cada posible petición de cada microservicio.  
- Esta función (petición) llamara al programa RO**servidor_name**Logic, que realizara la petición al microservicio. En la tabla se puede observar exactamente que petición http se realiza el microservidor en función de que petición (última columna) estemos realizando. Esta función, cuyo nombre se facilita en la tercera columna ejecutara esta petición y obtendrá un string del microservidor. 
- El texto devuelto será convertido a un objeto de C#, para ello el modelo devuelto por cada fuente externa se almacenará en el archivo  ROs/**servicor_name**/Models/ModeloInicial
- Se llamará a las funcionalidades  de RO**servidor_name**CambioModelo que nos permitirá cambiar el modelo devuelto, por la fuente externa, al modelo final deseado. (Más adelante se hablara más en detalle de cómo funciona este cambio de modelo).
    - Este modelo final es igual en todos los microservicios y está guardado en el archivo  ROs/*servidor_name*/Models/ROPublicationModel. Si se realiza una modificación de este fichero se deberá hacer en todos los microservicios también.
- Una vez tenemos el modelo final con toda la información simplemente se devuelve el modelo generado.
 
| Microservicio | petición especifica | nombre de la función que ejecuta la petición en el fichero RO*servidor_name*Logic |input | Url relativa de la petición a realizar en el servidor| 
|:---------------------:|:----------------------------:|:----------------------------:|:----------------------------------------------------------------------------------------------------------|:-----------------:|
| WoS | https://wos-api.clarivate.com/api/wos/?databaseId=WOK&usrQuery=AI=({orcid})&count={numItems}&firstRecord={(numItems * n) + 1}&publishTimeSpan={date}%2B3000-12-31" |getPublications |orcid and date (opcional) | WoS/GetROs?**orcid={}**&**date={}**|
| WoS | https://wos-api.clarivate.com/api/wos/id/WOS:{pIdWos}?databaseId=WOK&count=1&firstRecord=1" |getPublicationWos |Id de WoS de una publicación | WoS/GetRoByWosId?**pIdWos={}**|
| WoS | https://wos-api.clarivate.com/api/wos/?databaseId=WOK&usrQuery=DO=({pDoi})&count=1&firstRecord=1 | getPublicationDoi | doi de una publicación | WoS/GetRoByDoi?**pDoi={}** |
| Scopus | https://api.elsevier.com/content/search/scopus?query=ORCID(\"{0}\")&date={1}%&start={2} |getPublications |Orcid del autor and date (opcional) |/scopus/GetROs?**orcid={}**&**date={}** |
| Scopus | https://api.elsevier.com/content/search/scopus?apikey={apiKey}&query=DOI({pDoi} | getPublicationDoi | doi de la publicacion |/scopus/GetPublicationByDOI?**pDoi={}**  |
| Zenodo | https://zenodo.org/api/records/?q=doi:\"{0}\" | getPublications | doi de la publicacion |/scopus/GetROs?**ID={}**  |
| OpenCitations | https://w3id.org/oc/index/api/v1/references/{0} |getPublications | doi de la publicacion | OpenCitations/GetROs?**doi={}**|
| CrossRef |  https://api.crossref.org/works/{0} |getPublications | Doi de la publicación | CrossRef/GetROs?**DOI={}** |
| SemanticScholar | https://api.semanticscholar.org/graph/v1/paper/{0}?fields=externalIds,title,abstract,url,venue,year,referenceCount,citationCount,authors,authors.name,authors.externalIds  |getPublications | doi de la publicacion | SemanticScholar/GetROs?**doi={}** |

Respecto al cambio de modelo, en todos los microservicios se tienen las mismas funciones, denominadas del mismo modo, salvo que si esta fuente no nos proporciona informacion sobre dicho metadatado entonces esa función ha sido comentada/eliminada. En cada microservidor, estas funciones serán diferentes dependiendo de donde este la información necesaria en el modelo devuelto por la fuente externa. Esta información será recogida y puesta en el formato apropiado que el modelo final necesita, por eso en cada microservidor están funciones son diferentes. Estas funciones estan definidas en ROs/**nombre_servidor**/Controllers/RO**nombre_servidor**CambioModelo.cs. En la siguiente lista se puede observar estas funciones.

- publicacion.typeOfPublication = getType(objInicial);        
- publicacion.IDs = getIDs(objInicial);
- publicacion.title = getTitle(objInicial);
- publicacion.Abstract = getAbstract(objInicial);
- publicacion.language = getLanguage(objInicial);
- publicacion.doi = getDoi(objInicial);                
- publicacion.url = getLinks(objInicial);
- publicacion.dataIssued = getDate(objInicial);
- publicacion.pageStart = getPageStart(objInicial);
- publicacion.pageEnd = getPageEnd(objInicial);
- publicacion.hasKnowledgeAreas = getKnowledgeAreas(objInicial);
- publicacion.freetextKeywords = getFreetextKeyword(objInicial);
- publicacion.correspondingAuthor = getAuthorPrincipal(objInicial);
- publicacion.seqOfAuthors = getAuthors(objInicial);
- publicacion.hasPublicationVenue = getJournal(objInicial);
- publicacion.hasMetric = getPublicationMetric(objInicial);
- publicacion.bibliografia = getBiblografia(objInicial);
- publicacion.citas = getCitas(objInicial);




# Microservicio de publicaciones 

El microservicio más distintivo y diferente es el de publicación, en él se llama de una forma específica, siguiendo el algoritmo diseñado, a los microservicios descritos anteriormente. La mayor diferencia es que en este caso en vez de cambiar el modelo, las publicaciones obtenidas por las diversas fuentes externas van a ir convergiendo de una forma específica según el diseño del algoritmo. De esta tarea en específico se encarga el código de *ROPublicactionLogic*.
 
Este microservicio también debe devolver la informacion en el formato final deseado, por lo que este modelo se almacena en ROs/Publication/Models/ROPublicacionModel.
- En este caso el formato es un poco diferente porque se han introducido dos entidades dentro del modelo que no están en los otros microservicios. Estas entidades son aquellas que modelan los metadatos enriquecidos. 

A continuación, se describen los pasos que se llevarán a cabo durante el proceso de reclamación de publicaciones de un determinado autor. 

## Función de combinar dos publicaciones **compactacion**

Con esta función se combinan todos los metadatos de las publicaciones recibidas. Cada metadato se combina de forma independiente. En el caso de los autores se hace de modo que no permita duplicidad de usuarios en el mismo conjunto de colaboradores de la publicación. También se obtiene la informacion de las métricas de la revista. 



## Función principal.

- Primeramente, el investigador ofrece su ORCID y una fecha a partir de la cual quiere obtener sus ROs.  
- Se llamará a los servicios de WoS, Scopus y OpenAire  para obtener la información de las publicaciones principales de este autor. 
- Se recorre cada una de las publicaciones obtenidas en WoS. Por cada una de ellas: 
    - Se almacena el DOI en una lista para saber qué artículos ya hemos completado del investigador en cuestión. 
    - Se llama al servicio de Semantic Scholar y se fusiona la información obtenida por este microservicio y la publicación que estamos examinando (función de combinar dos publicaciones). El resultado de esta unificación será la publicación que estamos observando. Esta fuente externa nos devuelve la información de los documentos referenciados. Estas publicaciones tendrán únicamente unos pocos metadatos básicos que no serán completados con ninguna red externa adicional. 
    - Se llama a la fuente externa Zenodo y en caso de encontrarse un fichero PDF con la publicación se añadirá como metadato.
    - Se llama al enriquecimiento de áreas temáticas y de palabras clave para completar la publicación. 
    - Se añaden las métricas de las revistas.
    - Se recorren todos los documentos obtenidos por Scopus y para cada uno de ellos:
        - Si el DOI de esta publicación coincide con la publicación que estamos examinando entonces se combina la información (función de combinar dos publicaciones).
        - En caso contrario no se hace nada.
    - Se recorren todos los documentos obtenidos en OpenAire y para cada uno de ellos:
        - Si el DOI de esta publicación coincide con la publicación que estamos examinando entonces se combina la información (función de combinar dos publicaciones).
        - En caso contrario no se hace nada.
    -Llegados a este punto la publicación central está completa, así como todas las bibliográficas y citas que la componen. Se guarda para devolverse. 
- Recorremos la lista de publicaciones de Scopus con el fin de completar aquellas que no se han obtenido de WoS. Por tanto, por cada una de las publicaciones:
    - Si ya ha sido completada y almacenada antes, no hace nada con ella. 
    - En caso contrario: 
        - Se llama al servicio de Semantic Scholar y se fusiona la información obtenida por este microservicio y la publicación que estamos examinando (función de combinar dos publicaciones). El resultado de esta unificación será la publicación que estamos observando. Esta fuente externa nos devuelve la información de los documentos referenciados. Estas publicaciones tendrán únicamente unos pocos metadatos básicos que no serán completados con ninguna red externa adicional. 
        - Se llama a la fuente externa Zenodo y en caso de encontrarse un fichero PDF con la publicación se añadirá como metadato. 
        - Se llama al enriquecimiento de áreas temáticas y de palabras clave para completar la publicación. 
        - Se añaden las métricas de las revistas. 
- Recorrimos la lista de publicaciones de OpenAire con el fin de completar aquellas que no se han obtenido de WoS y Scopus. Por tanto, por cada una de las publicaciones:
    - Si ya ha sido completada y almacenada antes, no hace nada con ella. 
    - En caso contrario: 
        - Se llama al servicio de Semantic Scholar y se fusiona la información obtenida por este microservicio y la publicación que estamos examinando (función de combinar dos publicaciones). El resultado de esta unificación será la publicación que estamos observando. Esta fuente externa nos devuelve la información de los documentos referenciados. Estas publicaciones tendrán únicamente unos pocos metadatos básicos que no serán completados con ninguna red externa adicional. 
        - Se llama a la fuente externa Zenodo y en caso de encontrarse un fichero PDF con la publicación se añadirá como metadato. 
        -Se llama al enriquecimiento de áreas temáticas y de palabras clave para completar la publicación. 
        -Se añaden las métricas de las revistas. 
- Llegados a este punto ya tenemos completas todas las publicaciones de este autor. 
