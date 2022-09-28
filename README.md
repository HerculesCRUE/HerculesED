![](./Docs/media/CabeceraDocumentosMD.png)

# Herramienta de CV. Hércules ED - Enriquecimiento de Datos

Hércules ED - Enriquecimiento de Datos proporciona su funcionalidad entorno a un conjunto de aplicaciones entre las que destaca la herramienta de edición y gestión de CV, en la que se cargan datos obtenidos desde fuentes externas confiables de información de producción científica y datos provenientes de otros sistemas de la Universidad, particularmente [Hércules SGI](https://github.com/HerculesCRUE/SGI).

El análisis funcional se puede consultar en [Análisis funcional. Herramienta de CV. Hércules ED - Enriquecimiento de Datos](https://confluence.um.es/confluence/pages/viewpage.action?pageId=397534539).

Los módulos de Hércules ED son:

- **Servicios centrales (HERCULES Core Services)**. Este módulo provee de servicios de Single Sign-On (SSO), para facilitar el acceso a múltiples aplicaciones con credenciales comunes; y Single Login Entry Point (SLEP), que permite el acceso a fuentes externas de información de resultados de investigación.
- **Módulo de Gestión de FAIR Research Objects (FAIR RO)**. HERCULES permite al investigador declarar y catalogar sus RO independientemente de su formato (publicaciones, código, datasets, etc.) y de la aplicación en la que se encuentren.
- **Procesamiento y análisis de Research Objects (HERCULES RO Enrichment)**. Proporciona métodos y algoritmos de Machine Learning que, aplicados sobre los Research Objects recuperados desde fuentes o introducidos por el usuario, proponen descriptores temáticos y específicos que ayudan en la clasificación de los ROs y potencian las posteriores utilidades de búsqueda, tanto para el usuario como en el Portal Nacional de Investigación. Además, otros procesos potenciados por Machine Learning ofrecen al investigador ROs similares (similitud semántica), lo que permite recomendaciones de interés de otros resultados de investigación.
- **Perfil del investigador (HERCULES Researcher Dashboard)**. Es el núcleo de Hércules ED y proporciona al usuario una herramienta de gestión del CV, compatible con la norma CVN de FECYT, conectada a fuentes externas de información de resultados de investigación y también a sistemas de la universidad.

## Relación con otros repositorios

Existen 2 repositorios relacionados con Hércules ED:

- [Portal Nacional Avanzado de Investigación. Hércules MA - Métodos de Análisis](https://github.com/HerculesCRUE/HerculesMA). Constituye el Portal Nacional Avanzado de Investigación, con una serie de aplicaciones que permitirán la explotación y el análisis de los datos existentes en HERCULES, incluidos los gestionados en Hércules ED.
- [Commons-ED-MA](https://github.com/HerculesCRUE/Commons-ED-MA). Contiene componentes o diseños compartidos en los proyectos Hércules ED y [Hércules MA](https://github.com/HerculesCRUE/HerculesMA).

## Estructura del repositorio

Las carpetas del repositorio son:

- [Docs](./Docs). Contiene documentos del proyecto y recursos estáticos, como imágenes, que se usan en los documentos del repositorio.
- [Web](./Web). Contiene la configuración de vistas y estilos que configuran la presentación y funcionamiento propios del proyecto.
- [src](/.src). Contiene los servicios web y back que proporcionan la funcionalidad propia del proyecto.

## Despliegue

La información de despliegue se puede consultar en [Kubernetes Helm Deploy](./Docs/kubernetes-helm-deploy.md).

La descripción de la arquitectura de Hércules ED se puede consultar en [Arquitectura de Hércules ED](https://confluence.um.es/confluence/pages/viewpage.action?pageId=420085932).

## Información de versión y compatibilidad

![](https://content.gnoss.ws/imagenes/proyectos/personalizacion/7e72bf14-28b9-4beb-82f8-e32a3b49d9d3/cms/logognossazulprincipal.png)

Los componentes de Hércules ED funcionan y son compatibles con la versión 5 de [GNOSS Semantic AI Platform Open Core](https://github.com/equipognoss/Gnoss.SemanticAIPlatform.OpenCORE) y con la [versión Enterprise de GNOSS Semantic AI Platform](https://www.gnoss.com/contacto).
