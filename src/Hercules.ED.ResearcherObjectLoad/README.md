![](../../Docs/media/CabeceraDocumentosMD.png)

| Fecha         | 28/6/2022                                                   |
| ------------- | ------------------------------------------------------------ |
|Título|Servicio de carga de datos obtenidos de fuentes externas.| 
|Descripción|Servicio encargado del almacenamiento de datos pertenecientes a fuentes externas.|
|Versión|1.0|
|Módulo|Documentación|
|Tipo|Especificación|
|Cambios de la Versión|Versión inicial|



[![SonarCloud](https://sonarcloud.io/images/project_badges/sonarcloud-white.svg)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.ResearcherObjectLoad)

[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.ResearcherObjectLoad&metric=bugs)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.ResearcherObjectLoad)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.ResearcherObjectLoad&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.ResearcherObjectLoad)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.ResearcherObjectLoad&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.ResearcherObjectLoad)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.ResearcherObjectLoad&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.ResearcherObjectLoad)



## Introducción
Este documento describe el funcionamiento detallado del proceso de carga/modificación de los datos obtenidos por las fuentes externas.
El programa va a estar comprobando constantemente si en un directorio (files_publications) le llega un fichero json con nuevos datos. En el caso que haya un json nuevo, va a comprobar de que tipo de datos es y va a hacer el proceso de carga con todo lo que conlleva. Una vez cargados los datos en la BBDD, generará una copia en .rar en otro directorio (files_publications_backups) a modo de backup y borrará el json leido. Finalmente se volverá a quedar a la escucha de detección de nuevos json.

## Funcionamiento
El programa se va a quedar a la escucha de nuevos ficheros JSON. Estos ficheros se generan en el WorkerServiceRabbitConsume. El tipo de dato del JSON se identifica en el nombre del archivo. El formato del nombre del JSON está formado por {ID_TIPO} + ___ + {ID_AUTOR} + ___ + {FECHA} + .json
Dichos tipos pueden ser:
- Wos + Scopus + OpenAire (Publicaciones + Investigadores)
- FigShare (RO)
- GitHub (RO)
- SlideShare (RO)
- Zenodo (RO)

En este punto, se utiliza el [motor de desambiguación](https://github.com/HerculesCRUE/HerculesED/tree/main/src/Hercules.CommonsEDMA.DisambiguationEngine) para obtener las equivalencias entre todos los datos.
Finalmente, se cargan los datos nuevos y se modifican los ya existentes con los datos obtenidos.
Tras cargar los datos, se inserta en la cola de rabbit del [desnormalizador](https://github.com/HerculesCRUE/HerculesED/tree/main/src/Hercules.ED.Desnormalizador) aquellos recursos que tengan que ser modificados con datos previamente cargados.

## Configuración en el appsettings.json
```json
{
  "ConnectionStrings": {
    "RabbitMQ": ""
  },
  "DenormalizerQueueRabbit": "",
  "DirectorioLectura": "",
  "DirectorioEscritura": ""
}
```
- RabbitMQ: Cadena de conexión de Rabbit.
- DenormalizerQueueRabbit: Nombre de la cola de Rabbit.
- DirectorioLectura: Directorio dónde va a leer los JSON generados con los datos a cargar.
- DirectorioEscritura: Directorio dónde va a hacer la copia de los JSON leídos.

## Dependencias
- **EnterpriseLibrary.Data.NetCore**: v6.3.2
- **GnossApiWrapper.NetCore**: v1.0.8
- **Newtonsoft.Json**: v13.0.1
- **RabbitMQ.Client**: v6.3.0
- **System.Configuration.ConfigurationManager**: v6.0.0
