
![](../../../Docs/media/CabeceraDocumentosMD.png)

| Fecha         | 22/06/2022                                                   |
| ------------- | ------------------------------------------------------------ |
|Título|Configuración del Editor de CV| 
|Descripción|Similitud. Requisitos y documentación del API|
|Versión|1.0|
|Módulo|Documentación|
|Tipo|Especificación|
|Cambios de la Versión|Versión inicial|

# Requisitos
Se han observado los requisitos de hardware al procesar una serie de rankings de RO similares. A continuación se detallan el tamaño del conjunto de datos utilizado, las especificaciones de la máquina en la que se ha ejecutado y los requisitos observados.

**Conjunto de datos**
- Tipo de RO: artículos científicos de medicina.
- Tamaño del conjunto de test: 30 RO.
- Tamaño total del conjunto de datos: 112.487 RO.

**Especificaciones de la máquina**
- Modelo CPU: Intel(R) Xeon(R) Gold 5220R CPU @ 2.20GHz (48 núcleos, 96 threads)
- Modelo GPU: GeForce RTX 3090, 24 GB
- Memoria RAM: 128 GB

**Uso de hardware durante el procesamiento de rankings**
- Procesamiento en CPU:
  - Memoria RAM del sistema: 8 GB
- Procesamiento en GPU:
  - Memoria RAM del sistema: 10 GB
  - Memoria del GPU: 2 GB


# Especificaciones de la API de similitud

| Endpoint | Method | Description |
|--|--|--|
|[/ro](#ro-get)|GET|Get RO attributes.|
|[/ro](#ro-put)|PUT|Create or update a RO.|
|[/ro](#ro-delete)|DELETE|Delete an existing RO.|
|[/ro-collection](#ro-collection-get)|GET|Retrieve all RO IDs.|
|[/ro-collection](#ro-collection-post)|POST|Insert a batch of ROs.|
|[/similar](#similar-get)|GET|Get the most similar ROs given a RO.|
|[/rebuild-rankings](#rebuild-rankings-post)|POST|Rebuild all the rankings of similar ROs.|


## /ro [GET]

Devuelve los atributos del RO con el ID indicado.

**Parámetros de entrada (query)**
- ro_id: El identificador del RO.

**Parámetros de salida (JSON)**
- ro_id: El identificador del RO.
- ro_type: Tipo de RO.
- text: El texto del RO resultante de la concatenación entre el título y el abstract.
- authors: Lista de nombres completos de los autores del RO.
- thematic_descriptors: Lista de los descriptores temáticos y sus probabilidades obtenidos a través de la API de los descriptores (lista de listas de dos elementos).
- specific_descriptors: Lista de los descriptores específicos y sus probabilidades obtenidos a través de la API de los descriptores (lista de listas de dos elementos).

**Ejemplo**

Comando curl:
```
$ curl -X GET "http://herculesapi.elhuyar.eus/similarity/ro?ro_id=2-s2.0-85032573110"
```

Respuesta:
```
{
    "ro_id": "2-s2.0-85032573110",
    "ro_type": "research_paper",
    "text": "Analysis of the microstructure and mechanical properties of titanium-based composites reinforced by secondary phases and B In the last decade, titanium metal matrix composites (TMCs) have received considerable attention thanks to their interesting properties as a consequence of the clear interface between the matrix and the reinforcing phases formed. In this work, TMCs with 30 vol % of B",
    "authors": ["Montealegre-Melendez, Isabel", "Arévalo, Cristina", "Ariza, Enrique", "Pérez-Soriano, Eva M.", "Rubio-Escudero, Cristina", "Kitzmantel, Michael", "Neubauer, Erich"],
    "thematic_descriptors": [["Physical Sciences", 0.998]],
    "specific_descriptors": [["tmcs", 0.777], ["titanium-based composites", 0.702], ["secondary phases", 0.664], ["clear interface", 0.564], ["reinforcing phases", 0.534], ["their interesting properties", 0.476], ["titanium metal matrix composites", 0.394], ["consequence", 0.376], ["considerable attention", 0.347], ["thanks", 0.291], ["interesting properties", 0.276], ["analysis", 0.243], ["microstructure and mechanical properties", 0.188], ["decade", 0.187], ["last decade", 0.083], ["work", 0.025]]
}
```

## /ro [PUT]

Este método sirve para crear un nuevo RO en la base de datos, o actualizarlo en el caso de que exista un RO con el mismo ID. En el caso de que se necesite incorporar un gran lote de ROs, como por ejemplo en el caso de una carga masiva inicial, se utilizará el script [`indexar_ros.py`](#carga-de-lotes-de-ro) creado con ese propósito.

Si se trata de una actualización, si se cambia el texto se tiene que reindexar el RO por completo. Si se mantiene el texto solo se actualizan los atributos del RO correspondiente en la base de datos.

**Parámetros de entrada (JSON)**
- ro_id: El identificador del RO.
- ro_type: Tipo de RO. Debe ser uno de los siguientes: `research_paper`, `code_project`.
- text: El texto del RO resultante de la concatenación entre el título y el abstract.
- authors: Lista de nombres completos de los autores del RO.
- thematic_descriptors: Lista de los descriptores temáticos y sus probabilidades obtenidos a través de la API de los descriptores (lista de listas de dos elementos).
- specific_descriptors: Lista de los descriptores específicos y sus probabilidades obtenidos a través de la API de los descriptores (lista de listas de dos elementos).

**Parámetros de salida (JSON)**
- error_msg: Mensaje de error textual en caso de que el servicio no haya podido procesar la petición.

**Información adicional**
- Tanto los parámetros de entrada como los de salida estarán en formato JSON.
- Tiempo de respuesta estimado.
  - 2 segundos aprox. cada petición, tanto en GPU como en CPU.
  - En el caso de procesamiento por lotes:
    - GPU: 1.5 s / 1000 RO + tiempo de operaciones BBDD
    - CPU: 23.5 s / 1000 RO + tiempo de operaciones BBDD
- Habrá un comando para cargas iniciales masivas de RO. El script recibirá un archivo JSON como entrada, siendo este una lista de objetos con el mismo formato de los parámetros de entrada de /ro[PUT].
```
$ python3 indexar_ros.py lote_ros.json
```

**Ejemplo**

Comando curl:
```
$ curl -H "Content-Type:application/json" -X PUT -d '@query.json' "http://herculesapi.elhuyar.eus/similarity/ro"
```

Archivo query.json
```
{
    "ro_id": "2-s2.0-85032573110",
    "ro_type": "research_paper",
    "text": "Analysis of the microstructure and mechanical properties of titanium-based composites reinforced by secondary phases and B In the last decade, titanium metal matrix composites (TMCs) have received considerable attention thanks to their interesting properties as a consequence of the clear interface between the matrix and the reinforcing phases formed. In this work, TMCs with 30 vol % of B",
    "authors": ["Montealegre-Melendez, Isabel", "Arévalo, Cristina", "Ariza, Enrique", "Pérez-Soriano, Eva M.", "Rubio-Escudero, Cristina", "Kitzmantel, Michael", "Neubauer, Erich"],
    "thematic_descriptors": [["Physical Sciences", 0.998]],
    "specific_descriptors": [["tmcs", 0.777], ["titanium-based composites", 0.702], ["secondary phases", 0.664], ["clear interface", 0.564], ["reinforcing phases", 0.534], ["their interesting properties", 0.476], ["titanium metal matrix composites", 0.394], ["consequence", 0.376], ["considerable attention", 0.347], ["thanks", 0.291], ["interesting properties", 0.276], ["analysis", 0.243], ["microstructure and mechanical properties", 0.188], ["decade", 0.187], ["last decade", 0.083], ["work", 0.025]]
}
```

Respuesta: status=201 si se ha creado un nuevo RO, o status=200 si se ha actualizado un RO existente

## /ro [DELETE]

Método para eliminar un RO del sistema. Devuelve error 404 si el ID del RO no se encuentra en la base de datos.

Se comprueban los ranking de similares de todos los RO de la base de datos, por si el RO eliminado tiene que ser sustituído por otro.

**Parámetros de entrada (query)**
- ro_id: ID del RO a eliminar.

**Ejemplo**
```
$ curl -X DELETE -H 'Content-Type: application/json' "http://herculesapi.elhuyar.eus/similarity/delete_ro?ro_id=2-s2.0-85032573110"
```

## /ro-collection [GET]

Devuelve los ID de todos los RO del tipo indicado.

**Parámetros de entrada (query)**
- ro_type_target: Tipo de los ROs a devolver (research_paper, code_project).

**Ejemplo**
```
$ curl -X GET "http://herculesapi.elhuyar.eus/similarity/ro-collection?ro_type_target=research_paper"
```
Respuesta:
```
["2-s2.0-85007093234","2-s2.0-68549104193","2006.14499","1208.4826","hep-ph/0007200","2-s2.0-84874783243","2-s2.0-84883309509","2-s2.0-84982191365", ...]
```

## /ro-collection [POST]

Inserta un lote de ROs de forma eficiente. Normalmente no hay necesidad de utilizar este método de forma explícita, se recomienda utilizar el script [`indexar_ros.py`](#carga-de-lotes-de-ro).

Se insertan todos los RO sin actualizar los ranking de similares. Después de insertar todos los lotes es necesario llamar al método [/rebuild-rankings](#rebuild-rankings-post) para actualizar los ranking de similares. Es el método utilizado por el script `indexar_ros.py`. Ninguno de los IDs del lote debe existir en la base de datos antes de ejecutar el método.

**Parámetros de entrada (json)**
- batch: Lista de ROs en el mismo formato que `/ro[PUT]`.

## /similar [GET]

Este método devuelve los diez RO más similares al RO de entrada. Se puede limitar el tipo de ROs a devolver (solo artículos científicos, protocolos…).

**Parámetros de entrada**
- ro_id: ID del RO del que se quieren obtener RO similares. El RO correspondiente al ro_id debe estar en la colección de este servicio. 
- ro_type_target: Tipo de RO requerido para los RO similares.

**Parámetros de salida**
- similar_ros: Lista de los diez RO más similares de tipo ro_type_target al RO de entrada con ID ro_id. Se devuelven los IDs de los RO y los descriptores más relevantes de la relación. Los datos tienen el siguiente formato: list<text_id, relevant_descriptors>.
- error_msg: Mensaje de error textual en caso de que el servicio no haya podido procesar la petición.

**Información adicional**
- Tiempo de respuesta estimado: 1 s aprox.

**Ejemplo**

Comando curl:
```
$ curl -X GET "http://herculesapi.elhuyar.eus/similarity/similar?ro_id=2-s2.0-33846355632&ro_type_target=research_paper"
```

Respuesta:
```
{
  "similar_ros": [
    ["2-s2.0-84984599941", [
        ["titanium metal matrix composites", 0.390],
        ["titanium-based composites", 0.257],
        ["conventional hot-pressing titanium", 0.144],
        ["titanium", 0.071],
        ["microstructure and mechanical properties", 0.048]
    ]],
    ["2-s2.0-84952673849", [
        ["particle reinforced titanium matrix composites", 0.405],
        ["titanium-based composites", 0.255],
        ["titanium alloys", 0.142],
        ["tic-ti", 0.010],
        ["microstructure and mechanical properties", 0.009]
    ]],
    ["2-s2.0-56249090374", [
        ["titanium metal-matrix composites", 0.380],
        ["net-shape titanium-matrix composites", 0.338],
        ["titanium-based composites", 0.330],
        ["titanium alloys", 0.176],
        ["composite materials", 0.056]
    ]],
    ["2-s2.0-84984605052", [
        ["titanium matrix composites", 0.362],
        ["titanium-based composites", 0.289],
        ["microstructure and mechanical properties", 0.051],
        ["fiber-reinforced materials", 0.045],
        ["microstructure", 0.016]
    ]],
    ["2-s2.0-66849122582", [
        ["titanium-based composites", 0.257],
        ["titanium metal matrix composites", 0.230],
        ["mg-ti composites", 0.178],
        ["titanium additions", 0.075],
        ["titanium volume fraction", 0.072]
    ]],
	...
]}
```
## /rebuild-rankings [POST]

Reconstruye los ranking de similares de todos los RO de la base de datos. Normalmente no hay necesidad de utilizar esta función de forma explícita, se recomienda utilizar el script [`indexar_ros.py`](#carga-de-lotes-de-ro) para insertar lotes de RO.

Este método solo es necesario si previamente se ejecuta `/ro-collection[POST]`. El script `indexar_ros.py` se encarga de llamar a este método despues de insertar los lotes. En el caso de insertar ROs uno por uno mediante el método `/ro[PUT]` los ranking se actualizan de forma implícita.

**Ejemplo**
```
$ curl -X POST "http://herculesapi.elhuyar.eus/similarity/rebuild-rankings"
```

## Carga de lotes de RO

Se recomienda utilizar el script `utils/indexar_ros.py` para cargar lotes de RO de forma eficiente. Este script inserta todos los RO sin actualizar el índice de rankings después de cada inserción, a diferencia de `/ro[PUT]`. Después de cargar todos los RO, el script ejecuta `/rebuild-rankings` para reconstruír los ranking de similares de cada RO de forma eficiente.

**Ejemplo**
```
$ python3 utils/indexar_ros.py papers.json
```

papers.json:
```
[
    {
        "ro_id": "2-s2.0-85007093234",
        "ro_type": "research_paper",
        "text": "Corporate Entrepreneurship: Partial Divestitures as a Real Option Scholars in strategy and entrepreneurship have discussed the benefits and difficulties of keeping ventures inside the firm versus separating them through divestitures and the balance between control and autonomy. Using an in-depth analysis of cases of partial divestitures, this study examines the organizational arrangement that arises from divestitures with a retained parent-unit relationship. The emerging framework connects the parent-unit relationship and its modifications along the divestiture's objective \u2013 specifically, the exploration carried out by the unit. Partial divestitures are designed as real options, for firms to manage corporate venturing, taking advantage of the flexibility that such arrangement may grant.",
        "authors": [],
        "thematic_descriptors": [],
        "specific_descriptors": [
            ["partial divestitures", 0.923],
            ["Real Option Scholars", 0.742],
            ["arrangement", 0.623],
            ["autonomy", 0.599],
            ...
        ]
    },
        {
        "ro_id": "2-s2.0-68549104193",
        "ro_type": "research_paper",
        "text": "5-hydroxytryptamine induced relaxation in the pig urinary bladder neck Background and purpose: 5-Hydroxytryptamine (5-HT) Is one of the inhibitory mediators in the urinary bladder outlet region. Here we Investigated mechanisms involved in 5-HT-induced relaxations of the pig bladder neck. Experimental approach: Urothellum-denuded strips of pig bladder were mounted in organ baths for isometric force recordings of responses to 5-HT and electrical field stimulation (EFS). Key results: After phenylephrine-induced contraction, 5-HT and 5-HT receptor agonists concentration-dependently relaxed the preparations, with the potency order: 5-carboxamidotryptamine (5-CT) > 5-HT = RS67333 > (\u00b1)-8hydroxy-2-dipropyiamlnotetralinhydrobromlde > m-chlorophenylbiguanide > \u03b1-methyl-5-HT > ergotamine. 5-HT and 5-CT relaxations were reduced by the 5-HT",
        "authors": [],
        "thematic_descriptors": [],
        "specific_descriptors": [
            ["induced relaxation", 0.771],
            ["rs67333", 0.77],
            ["ergotamine", 0.758],
        ]
    }
]
```
