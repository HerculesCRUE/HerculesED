![](../Docs/media/CabeceraDocumentosMD.png)

# Servicios Web y Back de Herramienta de CV. Hércules ED - Enriquecimiento de Datos

Contiene los servicios web y back que proporcionan la funcionalidad propia del proyecto.

El listado de componentes es:

- [Hercules.ED.Desnormalizador](./Hercules.ED.Desnormalizador). Proceso encargado de generar datos desnormalizados para su consulta, búsqueda o representación.
- [Hercules.ED.DisambiguationEngine](./Hercules.ED.DisambiguationEngine). Servicio encargado de la deduplicación de datos (investigadores, publicaciones, ROs, etc.
- [Hercules.ED.DoiRabbitConsume](./Hercules.ED.DoiRabbitConsume). Servicio de Rabbit.
- [Hercules.ED.EditorCV](./Hercules.ED.EditorCV). Configuración del Editor de CV.
- [Hercules.ED.Enrichment](./Hercules.ED.Enrichment). Servicios y pipeline del enriquecimiento de descriptores y el servicio de similitud de ROs.
- [Hercules.ED.ExternalSources](./Hercules.ED.ExternalSources). Servicios de conexión con fuentes externas de información, para publicaciones y otros ROs
- [Hercules.ED.Harvester](./Hercules.ED.Harvester). Servicio encargado de la carga de datos (investigadores, publicaciones, ROs, etc) desde Hércules SGI - Sistema de Gestión de Investigación (SGI).
- [Hercules.ED.ImportExportCV](./Hercules.ED.ImportExportCV). Servicio encargado de la importación y exportación de curriculums vitae (CV) en formato CVN de FECYT.
- [Hercules.ED.Login](./Hercules.ED.Login). Descripción de la configuración del Servicio de Login con SAML
- [Hercules.ED.Matching](./Hercules.ED.Matching). Servicio encargado de hacer el matching (enlazado) de los descriptores específicos de las publicaciones.
- [Hercules.ED.OAI_PMH](./Hercules.ED.OAI_PMH). Servicio OAI-PMH para recolección de datos desde los sistemas de la universidad, Hércules SGI y otros.
- [Hercules.ED.ResearcherObjectLoad](./Hercules.ED.ResearcherObjectLoad). Servicio encargado del almacenamiento de datos pertenecientes a fuentes externas.
- [Hercules.ED.Synchronization](./Hercules.ED.Synchronization). Servicio de sincronizacion para la última actualización de datos.
- [Hercules.ED.Taxonomy](./Hercules.ED.Taxonomy). Taxonomía y mapeos para la clasificación de resultados y actividades de investigación
