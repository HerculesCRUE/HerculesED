![](../../Docs/media/CabeceraDocumentosMD.png)

| Fecha         | 4/3/2022                                                   |
| ------------- | ------------------------------------------------------------ |
|Título|Servicio desambiguación y deduplicación de datos.| 
|Descripción|Servicio encargado de la deduplicación de datos (investigadores, publicaciones, ROs, etc).|
|Versión|1.0|
|Módulo|Documentación|
|Tipo|Especificación|
|Cambios de la Versión|Versión inicial|

# Motor de desambiguación y deduplicación de datos
El servicio de desambiguación y deduplicación de datos se ocupa de reconocer y diferenciar de un conjunto de datos aquellos que sean iguales. Dicho proceso beneficia el desarrollo y mantenimiento de la información en aquellos servicios que hagan uso en Hércules ED.
Este proceso tiene varias ventajas como:
- Reducción del tiempo y el espacio del almacenamiento.
- Información más gestionable.
- Generación de un sistema centralizado de información.

## Funcionamiento
TODO:

En Hércules ED, dicho motor de desambiguación es utilizado en varios servicios que trabajan con datos. Estos servicios son los siguientes:
- Procesos de carga iniciales.
- Importación de CVN.
- Datos de fuentes externas.

## Proceso de Carga Inicial
TODO:

## Importación de CVN
TODO:

## Datos de Fuentes Externas
En el proceso de obtención de datos de fuentes externas se hace uso para obtener las equivalencias de investigadores, publicaciones y research objects.
Para ello, se utiliza el método Disambiguate, el cual requiere de dos parametros que son:
- Listado con los items obtenidos de fuentes externas para ser desambiguados.
- Listado de los items ya cargados en BBDD que tengan algún tipo de relación con los items obtenidos de fuentes externas.

En este proceso, se detecta los items iguales o que tengan un alto umbral de similaridad y se relacionan entre ellos mediante IDs.
Como resultado del proceso, el motor devolverá un diccionario cuya clave es el item a desambiguar y como valor una lista de sus equivalencias.
Posteriormente, el servicio encargado de fuentes externas procederá a la carga de datos.
Para más información sobre el servicio de fuentes externas mirar en el siguiente repositorio [Hercules.ED.ResearcherObjectLoad](https://github.com/HerculesCRUE/HerculesED/tree/main/src/Hercules.ED.ResearcherObjectLoad).

## Dependencias
- **GnossApiWrapper.NetCore**: v1.0.8
