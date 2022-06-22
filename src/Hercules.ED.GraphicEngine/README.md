![](../../Docs/media/CabeceraDocumentosMD.png)

| Fecha         | 22/06/2022                                                   |
| ------------- | ------------------------------------------------------------ |
|Título|Configuración de Indicadores| 
|Descripción|Descripción de la configuración de las gráficas de indicadores en Hércules|
|Versión|1.0|
|Módulo|Documentación|
|Tipo|Especificación|
|Cambios de la Versión|Versión inicial|

# Hércules ED. Configuración de las gráficas de indicadores.
[Introducción](#introducción)

Introducción
============

Este documento describe, mediante un ejemplo práctico, cómo se realiza la configuración de las distintas gráficas de indicadores en Hércules ED. Esta configuración está preparada para que la administre un usuario administrador.
La configuración de las gráficas se lleva a cabo mediante la edición de archivos JSON situados en la carpeta ./Config/configGraficas/ que definen diversos aspectos para cada uno de las gráficas.
También se explicará como un usuario puede guardar las gráficas que quiera en su espacio personal de indicadores.

Ejemplo de configuración de las gráficas de indicadores
========================================================

Se va a proceder a la explicación de la creación de una de las gráficas con su correspondiente página y con un ejemplo de faceta.
Para agregar una página nueva, hay que crear un archivo .json con un nombre distintivo y situarlo en el directorio de configGraficas. En él vamos a crear la siguiente estructura:

```
{
  "nombre": {
    "es": "Publicaciones"
  },
  "filtro": "rdf:type='document'&roh:isValidated='true'",
  "identificador": "pagina1",
  "graficas": [...],
  "facetas": [...]
}
```

- nombre: String. Define el nombre de la página de las gráficas. Se asigna en formato diccionario, es decir, como clave el idioma, y como valor el dato. Es obligatorio en un idioma. 
- filtro: String. Corresponde al filtro base que se asigna a toda la página. Es obligatorio. 
- identificador: String. ID de la página. Ha de ser único y obligatorio. 
- graficas: Listado con las gráficas a mostrar en la página. 
- facetas: Listado de las facetas a mostrar en la página. 

Dentro de la lista de facetas, se van a configurar cada una de las facetas que se van a mostrar en la página en cuestión. La estructura sería la siguiente:

```
"facetas": [
    {
      "nombre": {
        "es": "Tipo de publicación"
      },
      "filtro": "dc:type@@@dc:title"
    }
]
```
- nombre: String. Define el nombre de la faceta. Se asigna en formato diccionario, es decir, como clave el idioma, y como valor el dato. Es obligatorio en un idioma. 
- filtro: String. Corresponde al filtro de la faceta. Es obligatorio. 
- ordenAlfaNum: Valor true o false. Ordena los resultados en orden alfanumérico. De ser false, los ordena por número de resultados. Por defecto es false y es opcional. 
- rangoAnio: Valor true o false. Monta la faceta en formato de rango de años. Únicamente marcar true si el filtro de la faceta devuelve como resultado los años. Por defecto es false y no es obligatorio. 
- numeroItemsFaceta: Valor numérico entero. Indica el número de resultados que se quiere mostrar en las facetas. Si no se le indica, muestra todas las disponibles. No es obligatorio. 
- tesauro: Valor true o false. Indica si la faceta es de tipo tesauro o no. Permite montar la faceta de tipo tesauro con su correspondiente ver todos. Por defecto es false y únicamente indicar true si el filtro devuelve como resultado un tesauro. No es obligatorio. 
- verTodos: Valor true o false. Muestra un desplegable con “Ver todos” que permite visualizar en un pop-up todos los elementos de la faceta. Por defecto es false y no es obligatorio.

Dentro de la lista de gráficas, se van a configurar cada una de las gráficas que se van a mostrar en la página en cuestión. La estructura sería la siguiente:

Gráficas de Barras
```
{
      "identificador": "grafica1",
      "nombre": {
        "es": "Número de publicaciones"
      },
      "tipo": "Barras",
      "anchura": 11,
      "idGrupo": "grupo1",
      "config": {
        "orientacionVertical": true,
        "ejeX": "roh:year",
        "orderDesc": false,
        "rellenarEjeX": true,
        "yAxisPrint": [
          {
            "yAxisID": "y1",
            "posicion": "left",
            "nombreEje": {
              "es": "Nº de publicaciones"
            }
          }
        ],
        "dimensiones": [
          {
            "nombre": {
              "es": "Total de publicaciones por año"
            },
            "filtro": "",
            "tipoDimension": "bar",
            "color": "#BF4858",
            "yAxisID": "y1",
            "orden": 1
          }
        ]
      }
    },
```
- identificador: Es el identificador de la gráfica. Único. Obligatorio. String.
- nombre: Es el nombre o título de la gráfica. Multiidioma. Obligatorio. String.
- tipo: Representa el tipo de gráfica (Barras, Circular, Nodos). Obligatorio. String.
- anchura: Representa el ancho de la gráfica a modo de fracción, es decir, 11 es 1/1, 23 es 2/3, etc... (11, 12, 13, 14, 16, 23, 34, 38, 58). Obligatorio. Número.
- idGrupo: Junta las gráficas con este mismo identificador en un desplegable. Opcional. String.
- config: Configuración específica de la gráfica. Varía en función del tipo de gráfica. Obligatorio.
  - orientacionVertical: Representa si las barras son verticales u horizontales. Opcional. Boolean.
  - datosNodos: Representa si la gráfica obtiene los datos como una gráfica de nodos. Opcional. Boolean.
  - ejeX: Es el filtro de los datos del eje X. Obligatorio. String.
  - rango: El eje X de la gráfica se agrupa tomando los valores 1-3, 4-10, 11-30, 30+. Opcional. Boolean.
  - abreviar: Abrevia los labels del eje X. Opcional. Boolean.
  - orderDesc: Representa si los datos están ordenados de forma descendente o ascendente. Opcional. Boolean.
  - rellenarEjeX: Representa si en el eje X se rellenan los datos sin valor. Opcional. Boolean.
  - yAxisPrint: Configuración de los ejes Y en una gráfica de barras vertical. Obligatorio si es vertical.
      - yAxisID: Es el id (nombre) del eje. Obligatorio. String.
      - posicion: Es la posición del eje (left, right). Obligatorio. String.
      - nombreEje: Define el nombre del eje. Se asigna en formato diccionario, es decir, como clave el idioma, y como valor el dato. Es obligatorio en un idioma.
  - xAxisPrint: Configuración de los ejes X en una gráfica de barras horizontal. Obligatorio si es horizontal.
      - xAxisID: Es el id (nombre) del eje. Obligatorio. String.
      - posicion: Es la posición del eje (top, bottom). Obligatorio. String.
      - nombreEje: Define el nombre del eje. Se asigna en formato diccionario, es decir, como clave el idioma, y como valor el dato. Es obligatorio en un idioma.
  - dimensiones: Distintas dimensiones de la gráfica. Obligatorio.
      - nombre: Es el nombre de la dimensión. Multiidioma. Obligatorio. String.
      - filtro: Es el filtro de la dimensión. En caso de estar vacío ("") tomará los valores del ejeX. Obligatorio. String.
      - calculo: Es el cálculo que aplica la dimensión para sus datos (SUM, AVG, MIN, MAX). Opcional. String.
      - tipoDimension: Es el tipo de la dimensión (bar, line). Obligatorio. String
      - color: Es el color en hexadecimal de la dimensión. Opcional. String.
      - yAxisID: Eje Y al que está asociado. Obligatorio si es vertical. String.
      - xAxisID: Eje X al que está asociado. Obligatorio si es horizontal. String.
      - orden: Orden de dibujado de la dimensión. Opcional. Número.
      - colorLinea: Color en hexadecimal de la línea. Únicamente utilizar si es una dimensión de tipo line. Opcional.
TODO:

Indicadores
============


Ejemplo de guardado de gráficas y funcionamiento de Indicadores Personales
==========================================================================
Los usuarios van a tener una opción en su menú llamada "Mis indicadores" en la que van a poder guardar las gráficas que quieran su espacio personal. 
Para guardar una gráfica en el espacio personal hay que seguir los siguientes pasos:

- Situados en la página de Indicadores, pulsamos al botón de tres puntos de la gráfica a guardar y seleccionamos la opción de "Guardar en mi panel".

![image](https://user-images.githubusercontent.com/88077103/174978119-d2528543-c78e-4328-b5aa-55fb9eadcf32.png)

- Se nos abrirá un pop-up con una pequeña configuración de guardado:

![image](https://user-images.githubusercontent.com/88077103/174978448-a232da46-421a-428b-a381-bd40db6cd54d.png)

  - Título de la gráfica: Título el cual se va a guardar la gráfica.
  - Anchura: Porcentaje de anchura de la gráfica.
  - Seleccionar página / Crear nueva página: Selección de dónde se va a guardar la gráfica. En el caso de que no se tenga páginas creadas, únicamente aparecerá la segunda opción de creación.
 
Una vez pulsado el botón guardar, se nos habrá generado la gráfica en el menú de "Mis indicadores".
Si nos dirigimos allí la veremos tal y como la hayamos guardado, es decir, con filtros incluidos. 
También se habrá creado la página si lo hemos seleccionado.

![image](https://user-images.githubusercontent.com/88077103/174979828-d6692717-6564-459a-8952-2dc6e5076a6a.png)

En el menú de la página se pueden ver dos opciones, editar página y borrar página.
Si pulsamos sobre editar página se nos abrirá un popup el cual podemos cambiar el nombre de la página y el orden de la misma.

![image](https://user-images.githubusercontent.com/88077103/174980559-90c08bf1-a4db-4df9-b6c7-827bfeef33f8.png)

Por otro lado, si pulsamos borrar página se nos mostraá un mensaje de borrado.

![image](https://user-images.githubusercontent.com/88077103/174981116-596330be-18cd-4c50-a083-6fa1d0d2a66e.png)

Pasando al apartado de las gráficas, si pulsamos a los tres puntos se nos abrirá un menú con opciones adicionales.

![image](https://user-images.githubusercontent.com/88077103/174981401-a10944b5-e907-45d4-b8db-f5928311e6c4.png)

Si pulsamos a editar gráfica se nos abrirá el siguiente popup:

![image](https://user-images.githubusercontent.com/88077103/174982103-338c92f8-96c2-44ef-bf64-af430415ed8b.png)

- Título de la gráfica: Permite modificar el nombre de la gráfica.
- Anchura de la gráfica: Permite cambiar el ancho de la gráfica.
- Orden de la gráfica: Mueve la gráfica a la posición seleccionada.

Si se selecciona la opción de eliminar gráfica, se nos mostrará un mensaje similar al de borrar página.

![image](https://user-images.githubusercontent.com/88077103/174982932-60f6f333-7e30-49d8-b2fa-b2094225ef39.png)





