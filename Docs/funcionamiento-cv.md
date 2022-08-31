![](../../Docs/media/CabeceraDocumentosMD.png)

| Fecha         | 31/08/2022                                                   |
| ------------- | ------------------------------------------------------------ |
|Título|Funcionamiento del CV| 
|Descripción|Características y funcionamiento de la edición de CV en Hércules|
|Versión|1.0|
|Módulo|Documentación|
|Tipo|Especificación|
|Cambios de la Versión|Versión inicial|

## Apartados

[Introducción](#introducción)

[Archivos de la edición del CV](#archivos-de-la-edición-del-cv)

[Funcionamiento de la edición del CV](#funcionamiento-de-la-edición-del-cv)

## Introducción
Este documento comprende las distintas características sobre el funcionamiento de la edición de CV en Hércules.
Se detallarán las distintas funcionalidades de la edición CV, la lógica que sigue, los controladores que utiliza y las vistas que se muestran.
### Información ontológica
Los CV hacen referencia al objeto de conocimiento del currículum vitae (curriculumvitae.owl).
### Acceder a la edición del CV
Para acceder al CV de tu usuario, tienes que acceder al menú lateral izquierdo y ahí encontrarás las opción de Editar curriculum vitae dentro de "Curriculum Vitae".
![](../../Docs/media/EditorCV/menu_lateral.png)

## Archivos de la edición del CV
Aquí se detallará la vista, el js y los controladores que se utilizan para la edición del CV.
### Archivo de la vista
La vista utilizada se encuentra en "Views/Recursos/curriculumvitae.cshtml".
### Archivo de JavaScript
La lógica se encuentra tanto dentro de la vista como en "Estilos/theme/edicioncv.js".
En este fichero se hacen distintas llamadas a los controladores, que se detallarán en el siguiente apartado.

# Funcionamiento de la edición del CV
En este apartado se detallarán los distintos controladores y llamadas que se utilizan para la edición del CV.

Al entrar al editor CV se hacen dos peticiones:
- Datos del CV:
    - **API:** EditorCV
	- **Controlador:** EdicionCVController
	- **Función:** [GET]GetTab
    - **Descripción:** Obtiene los datos de una pestaña dentro del editor, en este caso la pestaña de datos personales, que es la primera.
- Gestionar duplicados:
    - **API:** EditorCV
	- **Controlador:** EdicionCVController
	- **Función:** [GET]GetItemsDuplicados
    - **Descripción:** Obtiene los datos de los posibles items duplicados del CV del usuario.

## Gestión de duplicados
Se puede acceder a la gestión de duplicados de dos formas, accediendo a la edición del CV directamente o dentro de la edición dándole al botón de "Gestionar duplicados".
![](../../Docs/media/EditorCV/GestionDuplicadosBoton.png)
Esto hará la petición anteriormente mencionada (GetItemsDuplicados) y se mostrará un modal si encuentra items duplicados, en caso contrario no mostraría ningún mensaje ni modal.
![](../../Docs/media/EditorCV/GestionDuplicadosPaso1.png)
Dentro de este modal se mostrará el item principal y los items duplicados, con diferentes opciones:
- **Fusionar:** Los datos vacíos del ítem principal serán complementados por los datos del ítem secundario. El ítem principal no perderá datos ni serán rescritos en este proceso, únicamente se añadirán datos que estaban vacíos previamente.
- **Eliminar:** Se eliminará el ítem secundario del CV del usuario.
- **Marcar como no duplicado:** El ítem se marcará como diferente sobre el ítem principal, para que no sea mostrado en el futuro.
![](../../Docs/media/EditorCV/GestionDuplicadosPaso2.png)
El usuario puede seleccionar dos opciones:
- **Aplicar y siguiente:** Será necesario que los ítems secundarios tengan una acción asociada. Se aplicarán las acciones elegidas en los ítems secundarios y se mostrará los siguientes ítems detectados como similares.
- **Ignorar y siguiente:** Se pasará al siguiente grupo de ítems detectados como similares sin realizar ninguna acción sobre los ítems mostrados.

Cuando se terminen de revisar los duplicados, se mostrará otro modal sugeriendo una nueva vuelta y petición al servicio pero esta vez con el parámetro pMinSimilarity = 0.7

## Datos del CV
Al acceder al editor del CV se mostrará la primera pestaña, que es la de datos de identificación, tras hacer la petición anteriormente mencionada (GetTab).
Se mostrarán las distintas pestañas del CV, las cuales al hacer click harán la misma petición de antes con distinto pId y pRdfType, y cargarán la pestaña correspondiente.

### Pestaña de datos de identificación
En esta pestaña habrá un botón de editar que mostrará un modal en el que se pueden editar los datos.
A la hora de querer guardar la edición, se hará una petición al servicio de GuardadoCV:
- **API:** EdicionCV
- **Controlador:** GuardadoCVController
- **Función:** [POST]UpdateEntity
- **Descripción:** Crea o actualiza una entidad.

### Otras pestañas
Las pestañas aparecerán con sus secciones cargadas pero no desplegadas, solo la primera.
Cuando se le haga click a una sección se cargarán los items de esta haciendo una llamada a GetTab.
Para estos items hay distintas acciones:
- **Añadir:** Se mostrará un modal con los datos del item para poder añadirlo.
    - **API:** EdicionCV
    - **Controlador:** GuardadoCVController
    - **Función:** [POST]UpdateEntity
    - **Descripción:** Crea o actualiza una entidad.
- **Eliminar:** Se eliminará el item.
    - **API:** EdicionCV
    - **Controlador:** GuardadoCVController
    - **Función:** [POST]RemoveItem
    - **Descripción:** Elimina un item del listado.
- **Editar:** Se mostrará un modal con los datos del item para poder editarlo.
    - **API:** EdicionCV
    - **Controlador:** EdicionCVController
    - **Función:** [GET]GetEdit
    - **Descripción:** Obtiene los datos de un item para poder editarlo.
- **Enviar a producción científica:** Se muestra un modal con los datos del item para poder enviarlo a producción científica.
    - **API:** EdicionCV
    - **Controlador:** EnvioValidacionController
    - **Función:** [GET]ObtenerDatosEnvioPRC
    - **Descripción:** Obtiene los datos para enviar un item a producción científica.
    Al enviar el item, se mostrará un modal con los datos de la validación.
    - **API:** EdicionCV
    - **Controlador:** EnvioValidacionController
    - **Función:** [POST]EnvioPRC
    - **Descripción:** Envía los datos a producción científica.
    Al ser enviado correctamente se actualizará el item
    - **API:** EdicionCV
    - **Controlador:** EdicionCVController
    - **Función:** [GET]GetItemMini
    - **Descripción:** Obtiene una minificha de una entidad de un listado de una pestaña

### Otros controladores
En el archivo js se hacen peticiones a otros controladores:

**EdicionCVController:**
- **API:** EdicionCV
- **Controlador:** EdicionCVController
- **Función:** [GET]GetCVUrl
- **Descripción:** Obtiene la URL de un CV a partir de un usuario

- **API:** EdicionCV
- **Controlador:** EdicionCVController
- **Función:** [POST]GetAutocomplete
- **Descripción:** Obtiene un listado de sugerencias con datos existentes para esa propiedad en algún item de CV

- **API:** EdicionCV
- **Controlador:** EdicionCVController
- **Función:** [POST]GetPropertyEntityData
- **Descripción:** Obtiene datos de una entidad

- **API:** EdicionCV
- **Controlador:** EdicionCVController
- **Función:** [GET]GetItemsDuplicados
- **Descripción:** Obtiene los datos de los posibles items duplicados del CV del usuario.

- **API:** EdicionCV
- **Controlador:** EdicionCVController
- **Función:** [GET]GetTab
- **Descripción:** Obtiene los datos de una pestaña dentro del editor

- **API:** EdicionCV
- **Controlador:** EdicionCVController
- **Función:** [GET]GetAllPublicData
- **Descripción:** Obtiene todos los datos marcados como públicos de la persona

- **API:** EdicionCV
- **Controlador:** EdicionCVController
- **Función:** [GET]GetItemMini
- **Descripción:** Obtiene una minificha de una entidad de un listado de una pestaña

- **API:** EdicionCV
- **Controlador:** EdicionCVController
- **Función:** [GET]GetEdit
- **Descripción:** Obtiene una ficha de edición de una entidad de un listado de una pestaña

- **API:** EdicionCV
- **Controlador:** EdicionCVController
- **Función:** [POST]LoadProps
- **Descripción:** Obtiene una serie de propiedades de una serie de entidades

- **API:** EdicionCV
- **Controlador:** EdicionCVController
- **Función:** [POST]ValidateSignatures
- **Descripción:** Valida las firmas

- **API:** EdicionCV
- **Controlador:** EdicionCVController
- **Función:** [POST]EnrichmentTopics
- **Descripción:** Obtiene los descriptores específicos y temáticos haciendo una petición a un servicio

- **API:** EdicionCV
- **Controlador:** EdicionCVController
- **Función:** [GET]GetTesaurus
- **Descripción:** Devuelve el tesauro pedido en el idioma marcado

**GuardadoCVController:**
- **API:** EdicionCV
- **Controlador:** GuardadoCVController
- **Función:** [POST]ChangePrivacityItem
- **Descripción:** Cambia la privacidad de un item

- **API:** EdicionCV
- **Controlador:** GuardadoCVController
- **Función:** [POST]RemoveItem
- **Descripción:** Elimina un item de un listado

- **API:** EdicionCV
- **Controlador:** GuardadoCVController
- **Función:** [POST]UpdateEntity
- **Descripción:** Crea o actualiza una entidad

- **API:** EdicionCV
- **Controlador:** GuardadoCVController
- **Función:** [POST]ValidateORCID
- **Descripción:** Valida un ORCID

- **API:** EdicionCV
- **Controlador:** GuardadoCVController
- **Función:** [POST]CreatePerson
- **Descripción:** Crea una persona

- **API:** EdicionCV
- **Controlador:** GuardadoCVController
- **Función:** [POST]ProcesarItemsDuplicados
- **Descripción:** Procesa los items duplicados

**EnvioValidacionController:**
- **API:** EdicionCV
- **Controlador:** EnvioValidacionController
- **Función:** [GET]ObtenerDatosEnvioPRC
- **Descripción:** Servicio para obtener todos los proyectos de la persona, junto a su titulo, fecha de inicio, fecha de fin y organización.

- **API:** EdicionCV
- **Controlador:** EnvioValidacionController
- **Función:** [POST]EnvioPRC
- **Descripción:** Servicio de envío a Producción Científica

- **API:** EdicionCV
- **Controlador:** EnvioValidacionController
- **Función:** [POST]EnvioProyecto
- **Descripción:** Servicio de envío de un proyecto a validación.