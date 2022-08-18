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
En el proceso de carga inicial de datos, se cargarán los datos en el siguiente orden:
- Datos de fuentes externas.
- Importación inicial de CV.

## Importación de CVN
En el proceso de carga de datos por medio de los curriculum vitae (CV) de los usuarios, se hace uso para obtener las equivalencias para todos los ítems propios del CV, además de las equivalencias entre las personas almacenadas en base de datos (BBDD) y las cargadas desde el CV. 
Cada ítem tiene diferentes atributos de diferenciación, que se marcarán en el servicio de importación de CV y un score o valor que indicará la similaridad entre diferentes ítems. 
Para considerar similares dos ítems se deberá alcanzar un valor minimo de score, que se conseguirá con la suma de scores de los diferentes atributos.
 
En el caso del apartado de "Situación Profesional Actual" se tendrán en cuenta los siguientes:
- Nombre (+)0.8
- Categoría (+/-)0.5
Siendo el score minimo a alcanzar 0.8, para considerar dos ítems similares. Este proceso puede encontrarse más desarrollado en el apartado de [Deduplicación](https://confluence.um.es/confluence/display/HERCULES/Proceso+de+carga+inicial+de+datos+para+la+UMU#ProcesodecargainicialdedatosparalaUMU-Deduplicaci%C3%B3n)

Tras ello, por medio del metodo SimilarityBBDD de la clase Disambiguation, se compararán los ítems leidos del CV con los almacenados en BBDD y según los criterios descritos anteriormente se diferenciarán las similaridades, devolviendo un listado de equivalencias.

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
