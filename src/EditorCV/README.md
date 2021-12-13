![](../../Docs/media/CabeceraDocumentosMD.png)

| Fecha         | 10/12/2021                                                   |
| ------------- | ------------------------------------------------------------ |
|Título|Configuración del Editor de CV| 
|Descripción|Descripción de la configuración del editor de CV en Hércules|
|Versión|1.0|
|Módulo|Documentación|
|Tipo|Especificación|
|Cambios de la Versión|Versión inicial|

# Hércules ED. Configuración del Editor de CV

[Introducción](#introducción)

[Ejemplo de edición](#ejemplo-de-edición)

Introducción
============

Este documento describe, mediante un ejemplo práctico, cómo se realiza la configuración de los distintos ítems de la norma CVN para su posterior incorporación y edición en el currículum vitae del investigador en Hércules ED.

La configuración de las pestañas que figuran en el CV del investigador se lleva a cabo, fundamentalmente, mediante la edición de archivos JSON situados en la carpeta ./Config/TabTemplates/ que definen diversos aspectos para cada uno de los ítems contenidos en las secciones o pestañas que define la norma CVN.

Ejemplo de edición
==================

Véase el caso en el que se desee realizar la configuración para el ítem "Publicaciones, documentos científicos y técnicos", de la pestaña "Actividad científica" en el editor del CV del investigador:

![](../../Docs/media/EditorCV/EdicionCV1.png)



El archivo a editar para la configuración de los ítems de "Actividad científica" es ScientificActivity.json, y lo encontramos en la carpeta TabTemplates. En él veremos la siguiente estructura:

```
{
	"rdftype": "http://w3id.org/roh/ScientificActivity",
	"property": "http://w3id.org/roh/scientificActivity",
	"sections": [
		{
			"rdftype": "http://w3id.org/roh/RelatedScientificPublication",
			"property": "http://w3id.org/roh/scientificPublications",
			"presentation": {
				"type": "listitems",
				"title": {
					"es": "Publicaciones, documentos científicos y técnicos"
				},
```

Una vez localizada la propiedad del modelo semántico que corresponde con la actividad científica y tecnológica del investigador, vemos un listado de secciones (ítems) que podemos editar. En este caso, y como vemos en la imagen anterior, se procede a definir el RDF, la propiedad del ítem y el título ("Publicaciones, documentos científicos y técnicos") con el que figurará en el listado del editor.

Seguidamente, definimos la presentación para cada una de las publicaciones que el titular del CV puede añadir en esta sección. En primer lugar, se añade la propiedad que vincula el CV del investigador con el objeto que contendrá los datos generales de la publicación (vivo:relatedBy). Después definifimos el grafo sobre el que vamos a trabajar (document) y la propiedad que mostrará el título de cada una de las publicaciones que añadamos, en este caso roh:title.

```
{
"listItemsPresentation": {
	"property": "http://vivoweb.org/ontology/core#relatedBy",
	"listItem": {
		"propertyTitle": {
			"property": "http://vivoweb.org/ontology/core#relatedBy",
			"graph": "document",
			"child": {
				"property": "http://w3id.org/roh/title"
			}
		},
		
```

Procedemos a configurar el orden con el que filtrar las publicaciones mediante la propiedad "orders". En el siguiente ejemplo se contempla la ordenación por fecha más reciente y por fecha más antigua de publicación:


```
"orders": [
	{
		"name": {
			"es": "Ordenar por fecha más reciente"
		},
		"properties": [
			{
				"property": "http://vivoweb.org/ontology/core#relatedBy",
				"graph": "document",
				"child": {
					"property": "http://purl.org/dc/terms/issued"
				},
				"asc": false
			},
			{
				"property": "http://vivoweb.org/ontology/core#relatedBy",
				"graph": "document",
				"child": {
					"property": "http://w3id.org/roh/title"
				},
				"asc": true
			}
		]
	},
	{
		"name": {
			"es": "Ordenar por fecha más antigua"
		},
		"properties": [
			{
				"property": "http://vivoweb.org/ontology/core#relatedBy",
				"graph": "document",
				"child": {
					"property": "http://purl.org/dc/terms/issued"
				},
				"asc": true
			},
			{
				"property": "http://vivoweb.org/ontology/core#relatedBy",
				"graph": "document",
				"child": {
					"property": "http://w3id.org/roh/title"
				},
				"asc": true
			}
		]
	}
	],
```

En la vista se muestra el siguiente resultado:

![](../../Docs/media/EditorCV/EdicionCV2.png)


Tras ello, queremos definir las propiedades que mostrará cada publicación en el listado de minifichas, es decir, en la visualización previa a la edición. Además de la propiedad para el título de la publicación que hemos añadido anteriormente, queremos mostrar la fecha de la publicación, el nombre de la publicación principal (la revista) que sirve como soporte para el documento en cuestión, los autores y el tipo de producción de la publicación:


```
"properties": [
	{
		"name": {
			"es": "Fecha de publicación"
		},
		"showMiniBold": true,
		"type": "date",
		"child": {
			"property": "http://vivoweb.org/ontology/core#relatedBy",
			"graph": "document",
			"child": {
				"property": "http://purl.org/dc/terms/issued"
			}
		}
	},
	{
		"name": {
			"es": "Nombre de la publicación"
		},
		"child": {
			"property": "http://vivoweb.org/ontology/core#relatedBy",
			"graph": "document",
			"child": {
				"property": "http://vivoweb.org/ontology/core#hasPublicationVenue",
				"graph": "maindocument",
				"child": {
					"property": "http://w3id.org/roh/title"
				}
			}
		}
	},
	{
		"name": {
			"es": "Autores/as (p. o. de firma)"
		},
		"showMini": true,
		"childOR": [
			{
				"property": "http://vivoweb.org/ontology/core#relatedBy",
				"graph": "document",
				"order": "http://www.w3.org/1999/02/22-rdf-syntax-ns#comment",
				"child": {
					"property": "http://purl.org/ontology/bibo/authorList",
					"child": {
						"property": "http://xmlns.com/foaf/0.1/nick"
					}
				}
			},
			{
				"property": "http://vivoweb.org/ontology/core#relatedBy",
				"graph": "document",
				"order": "http://www.w3.org/1999/02/22-rdf-syntax-ns#comment",
				"child": {
					"property": "http://purl.org/ontology/bibo/authorList",
					"child": {
						"property": "http://www.w3.org/1999/02/22-rdf-syntax-ns#member",
						"graph": "person",
						"child": {
							"property": "http://xmlns.com/foaf/0.1/name"
						}
					}
				}
			}
		]
	},


```
