![](../../Docs/media/CabeceraDocumentosMD.png)

| Fecha         | 4/3/2022                                                   |
| ------------- | ------------------------------------------------------------ |
|Título|Servicio de carga de datos obtenidos de fuentes externas.| 
|Descripción|Servicio encargado del almacenamiento de datos en BBDD pertenecientes a fuentes externas.|
|Versión|1.0|
|Módulo|Documentación|
|Tipo|Especificación|
|Cambios de la Versión|Versión inicial|

## Introducción
Este documento describe el funcionamiento detallado del proceso de carga/modificación de los datos obtenidos por las fuentes externas.
El programa va a estar comprobando constantemente si en un directorio le llega un fichero json con nuevos datos. En el caso que haya un json nuevo, va a comprobar de que tipo de datos es y va a hacer el proceso de carga con todo lo que conlleva. Una vez cargados los datos en la BBDD, generará una copia en .rar en otro directorio a modo de backup y borrará el json leido. Finalmente se volverá a quedar a la escucha de detección de nuevos json.

## Configuración del modelo de desambiguación
 ```csharp
private string mPropEjemplo { get; set; }

...

public string propEjemplo
{
    get
    {
        return mPropEjemplo;
    }
    set
    {
        if (value == null)
        {
            mPropEjemplo = string.Empty;
        }
        else
        {
            mPropEjemplo = value;
        }
    }
}

...

private static DisambiguationDataConfig configPropEjemplo = new DisambiguationDataConfig()
{
    # Utilizado para la similitud de nombres (string). Score -> Cantidad a incrementar si el valor es similar.
    #type = DisambiguationDataConfigType.algoritmoNombres,
    #score = 1f
    
    # Utilizado para la similitud de listas de datos (HashSet). Score -> Cantidad a incrementar si el valor es similar.
    #type = DisambiguationDataConfigType.equalsItemList,
    #score = 0.5f
    
    # Utilizado para la similitud de identificadores (string).
    #type = DisambiguationDataConfigType.equalsIdentifiers
};

public override List<DisambiguationData> GetDisambiguationData()
{
    List<DisambiguationData> data = new List<DisambiguationData>();

    ...

    data.Add(new DisambiguationData()
    {
        property = "propEjemplo",
        config = configPropEjemplo,
        value = propEjemplo
    });

    ...

    return data;
}
    
 ```

## Funcionamiento
El programa se va a quedar a la escucha de nuevos ficheros JSON. Estos ficheros se generan en el WorkerServiceRabbitConsume. El tipo de dato del JSON se identifica en el nombre del archivo. El formato del nombre del JSON está formado por {ID_TIPO} + ___ + {ID_AUTOR} + ___ + {FECHA} + .json
Dichos tipos pueden ser:
- Wos + Scopus + OpenAire (Publicaciones + Investigadores)
- FigShare (RO)
- GitHub (RO)
- SlideShare (RO)
- Zenodo (RO)

Cuando detecta un JSON nuevo y detecta de que tipo es, hace dos consultas a base de datos. Estas consultas van a ayudar a la desambiguación de datos.
- Obtienen las personas relacionadas con el ID_AUTOR.
- Obtiene los documentos/ROs relacionados con el ID_AUTOR.
A su vez, obtiene los mismos datos pero del JSON, teniendo como resultado cuatro listas de datos (PersonasBBDD, DocumentosBBDD/ROsBBDD, PersonasJSON, DocumentosJSON/ROsJSON).

TODO: Continuar documentando...

## Configuración en el appsettings.json
```json
{
  "DirectorioLectura": "",
  "DirectorioEscritura": ""
}
```
- DirectorioLectura: Directorio dónde va a leer los JSON generados con los datos a cargar.
- DirectorioEscritura: Directorio dónde va a hacer la copia de los JSON leídos.

## Dependencias
- **GnossApiWrapper.NetCore**: v1.0.8
- **System.Configuration.ConfigurationManager**: v6.0.0
