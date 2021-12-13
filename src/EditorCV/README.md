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

``` {
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
				}, ```
