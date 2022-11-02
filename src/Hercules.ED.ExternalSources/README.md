
![](../../Docs/media/CabeceraDocumentosMD.png)

| Fecha         | 02/11/2022                                                   |
| ------------- | ------------------------------------------------------------ |
|Título|Librería de conexión con repositorios externos| 
|Descripción|Servicios de conexión con fuentes externas de información, para publicaciones y otros ROs|
|Versión|1.1|
|Módulo|Documentación|
|Tipo|Especificación|
|Cambios de la Versión|Modificada documentación|

# Hércules ED. Librería de conexión con repositorios externos

Este directorio va a contener todos aquellos microservicios encargados de hacer peticiones a fuentes externas para obtener los datos.

Para más información, se puede ver el [confluence](https://confluence.um.es/confluence/display/HERCULES/Servicios+centrales.+Core+services) asociado al repositorio.

El listado de componentes es:

- [Hercules.ED.Publication](./Hercules.ED.Publication). Servicio encargado de hacer las peticiones al resto de microservicios y contruir el JSON en formato string.
- [Hercules.ED.RabbitConsume](./Hercules.ED.RabbitConsume). Consumidor de la cola de rabbit.
- [Hercules.ED.CrossRefConnect](./Hercules.ED.CrossRefConnect). Obtiene los datos de CrossRef.
- [Hercules.ED.FigShare](./Hercules.ED.FigShare). Obtiene los datos de FigShare.
- [Hercules.ED.GitHubConnect](./Hercules.ED.GitHubConnect). Obtiene los datos de GitHub.
- [Hercules.ED.OpenAireConnect](./Hercules.ED.OpenAireConnect). Obtiene los datos de OpenAire.
- [Hercules.ED.OpenCitations](./Hercules.ED.OpenCitations). Obtiene los datos de OpenCitations.
- [Hercules.ED.ScopusConnect](./Hercules.ED.ScopusConnect). Obtiene los datos de Scopus.
- [Hercules.ED.SemanticScholar](./Hercules.ED.SemanticScholar). Obtiene los datos de SemanticScholar.
- [Hercules.ED.WoSConnect](./Hercules.ED.WoSConnect). Obtiene los datos de WoS.
- [Hercules.ED.Zenodo](./Hercules.ED.Zenodo). Obtiene los datos de Zenodo.
