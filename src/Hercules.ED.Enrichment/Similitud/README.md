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

## add_ro (POST)

Este método sirve para crear un nuevo RO en la base de datos. Se debe ejecutar cada vez que se quiera añadir un nuevo RO al sistema. En el caso de que se necesite incorporar un gran lote de ROs, como por ejemplo en el caso de una carga masiva inicial, se utilizará un script creado con ese propósito.
Esta función se ejecuta de forma síncrona. 

**Parámetros de entrada (JSON)**
- ro_id: El identificador del RO.
- ro_type: Tipo de RO. Debe ser uno de los siguientes: `research_paper`, `code_project`.
- text: El texto del RO resultante de la concatenación entre el título y el abstract.
- authors: Lista de nombres completos de los autores del RO.
- thematic_descriptors: Lista de los descriptores temáticos y sus probabilidades obtenidos a través de la API de los descriptores.
- specific_descriptors: Lista de los descriptores específicos y sus probabilidades obtenidos a través de la API de los descriptores.

**Parámetros de salida (JSON)**
- error_msg: Mensaje de error textual en caso de que el servicio no haya podido procesar la petición.

**Información adicional**
- Tanto los parámetros de entrada como los de salida estarán en formato JSON.
- Habrá un comando para cargas iniciales masivas de RO. El script recibirá un archivo JSON como entrada, siendo este una lista de objetos con el mismo formato de los parámetros de entrada de add_ro.
```
$ indexar_ros lote_ros.json
```
- Tiempo de respuesta estimado.
  - 2 segundos aprox. cada petición, tanto en GPU como en CPU.
  - En el caso de procesamiento por lotes:
    - GPU: 1.5 s / 1000 RO + tiempo de operaciones BBDD
    - CPU: 23.5 s / 1000 RO + tiempo de operaciones BBDD

**Ejemplo**

Comando curl:
```
$ curl -H "Content-Type:application/json" -X POST -d '@query.json' herculesapi.elhuyar.eus/similarity/add_ro
```

Archivo query.json
```
{
    "ro_id": "2-s2.0-85032573110",
    "ro_type": "research_paper",
    "text": "Analysis of the microstructure and mechanical properties of titanium-based composites reinforced by secondary phases and B In the last decade, titanium metal matrix composites (TMCs) have received considerable attention thanks to their interesting properties as a consequence of the clear interface between the matrix and the reinforcing phases formed. In this work, TMCs with 30 vol % of B",
    "authors": ["Montealegre-Melendez, Isabel", "Arévalo, Cristina", "Ariza, Enrique", "Pérez-Soriano, Eva M.", "Rubio-Escudero, Cristina", "Kitzmantel, Michael", "Neubauer, Erich"],
    "thematic_descriptors": [("Physical Sciences", 0.998)],
    "specific_descriptors": [("tmcs", 0.777), ("titanium-based composites", 0.702), ("secondary phases", 0.664), ("clear interface", 0.564), ("reinforcing phases", 0.534), ("their interesting properties", 0.476), ("titanium metal matrix composites", 0.394), ("consequence", 0.376), ("considerable attention", 0.347), ("thanks", 0.291), ("interesting properties", 0.276), ("analysis", 0.243), ("microstructure and mechanical properties", 0.188), ("decade", 0.187), ("last decade", 0.083), ("work", 0.025)]
}
```

Respuesta:
```
{}
```

## query_similar (GET)

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
$ curl -H "Content-Type:application/json" -X POST -d '@query.json' herculesapi.elhuyar.eus/similarity/query_similar
```

Archivo query.json:
```
{
    "ro_id": "2-s2.0-85032573110",
    "ro_type_target": "research_paper"
}
```

Respuesta:
```
{
  "similar_ros": [
    ("2-s2.0-84984599941", [
        ("titanium metal matrix composites", 0.390),
        ("titanium-based composites", 0.257),
        ("conventional hot-pressing titanium", 0.144),
        ("titanium", 0.071),
        ("microstructure and mechanical properties", 0.048)
    ]),
    ("2-s2.0-84952673849", [
        ("particle reinforced titanium matrix composites", 0.405),
        ("titanium-based composites", 0.255),
        ("titanium alloys", 0.142),
        ("tic-ti", 0.010),
        ("microstructure and mechanical properties", 0.009)
    ]),
    ("2-s2.0-56249090374", [
        ("titanium metal-matrix composites", 0.380),
        ("net-shape titanium-matrix composites", 0.338),
        ("titanium-based composites", 0.330),
        ("titanium alloys", 0.176),
        ("composite materials", 0.056)
    ]),
    ("2-s2.0-84984605052", [
        ("titanium matrix composites", 0.362),
        ("titanium-based composites", 0.289),
        ("microstructure and mechanical properties", 0.051),
        ("fiber-reinforced materials", 0.045),
        ("microstructure", 0.016)
    ]),
    ("2-s2.0-66849122582", [
        ("titanium-based composites", 0.257),
        ("titanium metal matrix composites", 0.230),
        ("mg-ti composites", 0.178),
        ("titanium additions", 0.075),
        ("titanium volume fraction", 0.072)
    ])
]}
```
