![](../../Docs/media/CabeceraDocumentosMD.png)

| Fecha         | 4/3/2022                                                   |
| ------------- | ------------------------------------------------------------ |
|Título|Servicio de procesamiento de curriculums vitae (CV).| 
|Descripción|Servicio encargado de importación y exportación de curriculums vitae (CV).|
|Versión|1.0|
|Módulo|Documentación|
|Tipo|Especificación|
|Cambios de la Versión|Versión inicial|

# Introducción
El servicio de procesamiento de CV, es el encargado de leer los archivos recibidos por FECYT y tratar los datos leidos, para posteriormente insertarlos en base de datos (BBDD).
Tambien es posible extraer los datos almacenados en BBDD, y recibir un CV en formato .pdf acorde a la normativa de FECYT.

El análisis funcional relacionado con el CV se puede consultar en [Edición CV - CVN](https://confluence.um.es/confluence/pages/viewpage.action?pageId=397534628).

## Servicios
- Preimportación
- Importación
- Postimportación
- Exportación
- Exportación limitada

## Preimportación de CVN
El servicio de preimportación devuelve los datos leidos de un Documento XML o PDF pasados como parametro, junto al identificador de CV de la persona, y como dato opcional las secciones que se desea leer, las cuales se deberá pasar el codigo CVN de FECYT.
Los datos leidos se devolverán como un JSON, para posteriormente elegir cual de ellos importar, junto a un documento XML obtenido del fichero inicial pasado como parametro, ademas de un fichero XML el cual contendrá los datos de un objeto Preimport.

## Importación de CVN
Dado un identificador del CV, el fichero de CV en formato PDF o XML asociado a la persona y opcionalmente las secciones que se desean incluir, las cuales se deberá pasar el codigo CVN de FECYT. 
Insertará en BBDD los datos leidos del documento, en caso de que se encuentren duplicidades, se resolverán por medio del [motor de desambiguación](https://github.com/HerculesCRUE/HerculesED/tree/main/src/Hercules.ED.DisambiguationEngine).

## Postimportación de CVN
Dado un identificador del CV, el fichero de CV en formato XML asociado a la persona como array de bytes, el fichero Preimport en formato XML como array de bytes, el listado de identificadores de los recusos a añadir y opcionalmente el listado de identificadores de los recursos concatenados por "|||" con las opciones seleccionadas, las cuales pueden ser:
- Duplicar - "du"
- Fusionar - "fu"
- Sobrescribir - "so"
- Ignorar - "ig"

Insertará en BBDD los datos leidos del documento que formen parte del listado de identificadores y les aplicará la opción seleccionada en cada uno de ellos, en caso de no tener ninguna se duplicará.

## Exportación de CVN
El servicio de exportación devolverá un fichero PDF con los datos almacenados en BBDD, pertenecientes al usuario con identificador de CV, en el tipo de formato de CV, la versión de exportación y en el lenguaje indicado.

Los formatos de CV son:
- CV - Curriculum Vitae - "PN2008"
- CVA-ISCIII - Curriculum Vitae Abreviado (ISCIII) - "CVAISCIII"
- CVA-AEI - Curriculum Vitae Abreviado (AEI) - "CVA2015"

Las versiones de exportación soportadas son:
- 1.4.0 - "1_4_0"
- 1.4.3 - "1_4_3"

Los posibles lenguajes son:
- Español - "es"
- Catalan - "ca"
- Euskera - "eu"
- Gallego - "gl"
- Inglés - "en"
- Frances - "fr"

## Exportación limitada de CVN
El servicio de exportación limitada, es similar al de exportación, pero filtrando mediante un listado de identificadores los recursos que se desean recibir de BBDD.

## Configuración en el appsetting.json
```json{
	{
		"Logging": {
			"LogLevel": {
				"Default": "",
				"Microsoft": "",
				"Microsoft.Hosting.Lifetime": ""
			}
		},
		"AllowedHosts": "*",
		"Usuario_PDF": "",
		"PSS_PDF": "",
		"Version": "",
		"ConnectionStrings": {
			"RabbitMQ": ""
		},
		"UrlEnriquecimiento": "",
		"UrlPublicationAPI": "",
		"DenormalizerQueueRabbit": ""
	}
}
```
- LogLevel.Default: Nivel de error por defecto.
- LogLevel.Microsoft: Nivel de error para los errores propios de Microsoft.
- LogLevel.Microsoft.Hosting.Lifetime: Nivel de error para los errores de host.
- Usuario_PDF: Usuario de autenticación para la conversión de ficheros en el endpoint de FECYT.
- PSS_PDF: Contraseña de autenticación para la conversión de ficheros en el endpoint de FECYT.
- Version: Version del documento PDF.
- ConnectionStrings.RabbitMQ: Cadena de conexión de Rabbit.
- UrlEnriquecimiento: URL dónde está instalado el servicio de enriquecimiento.
- UrlPublicationAPI: URL dónde está instalado el servicio de publicación.
- DenormalizerQueueRabbit: Nombre de la cola de Rabbit.

## Dependencias
- **GnossApiWrapper.NetCore**: v1.0.8
- **RabbitMQ.Client**: v6.3.0
- **Swashbuckle.AspNetCore**: v5.6.3
- **System.ServiceModel.Http**: v4.9.0
