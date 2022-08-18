//JS común a todas las fichas con buscadores
//Buscador personalizado

// Constantes para el listado de las ofertas
const uriAnn = "Anotaciones/GetOwnAnnotationsInRO"
const uriNA = "Anotaciones/CreateNewAnnotation"
const uriDA = "Anotaciones/DeleteAnnotation"

// Variables
var urlAnn = "";
var urlNA = "";
var urlDA = "";

/**
 * Crea las urls para las llamadas ajax
 */
$(document).ready(function () {
	urlAnn = new URL(url_servicio_externo +  uriAnn);
	urlNA = new URL(url_servicio_externo +  uriNA);
	urlDA = new URL(url_servicio_externo +  uriDA);
})

class CargarAnotaciones {

	/**
	 * Constructor de la clase CargarAnotaciones
	 * @param idRO, string con el id del RO
	 * @param idUser, string con el id del usuario
	 * @param rdfType, string con el rdfType de la ontología
	 * @param ontology,
	 */
	constructor (idRO, idUser, rdfType, ontology) {
		var _self = this
		this.body = $('body')
		this.data = undefined
		this.idRecurso = idRO
		this.idUsuario = idUser
		this.rdfType = rdfType
		this.ontology = ontology
		this.tabItem = undefined
		this.contenedor = undefined
	}


	/**
	 * Método que realiza la petición para obtener las notas de los investigadores actuales 
	 */
	loadInRo () {

		let _self = this

		return new Promise((resolve, reject) => {

			// Variables para la petición
			let args = {
				idRO: this.idRecurso,
				idUser: this.idUsuario,
				rdfType: this.rdfType,
				ontology: this.ontology,
			}
			// Realizamos la llamada
			this.postCall(urlAnn, args).then(data => {
				_self.data = data
				resolve(data)
			})
		})
	}

	/**
	 * Método que pinta las anotaciones en el html 
	 * @param idTab, string con el id (del DOM) del Tab para pintar el número de items
	 * @param idContenedor, string con el id (del DOM) del contenedor para pintar el número de items y los resultados
	 * @param data, array de objetos con los las notas a pintar
	 * @param callback, función que se le pasa por parámetro para realizar alguna acción una vez que se realice el pintado
	 */
	printItems(idTab = null, idContenedor = null, data = null, callback = () => {}) {
		// Establecemos los contenedores
		if (idTab != null) {
			this.tabItem = document.getElementById(idTab)
		}
		if (idContenedor != null) {
			this.contenedor = document.getElementById(idContenedor)
		}

		// Establecer el número de items en el HTML
		let dataSel = this.tabItem.getElementsByClassName('data')[0]
		let numResultados = this.contenedor.getElementsByClassName('h1-container')[0].getElementsByClassName('numResultados')[0]

		dataSel.innerText = this.data.length
		numResultados.innerText = this.data.length

		// Comprobamos si le hemos pasado los datos como parámetro, si no cogemos los cargados en el objeto
		let currentData = data != null ? data : this.data

		// Pintamos el html
		let html = currentData.map(e => {
			return `<article class="resource resource-annotation" id="${e.id}">
                        <div class="wrap">
                            <div class="row">
                                <div class="col">
                                    <div class="middle-wrap">
                                        <div class="title-wrap">
                                            <h2 class="resource-title">
                                                <span> ${e.fecha} </span>
                                            </h2>
                                        </div>
                                        <div class="content-wrap">
                                            <div class="description-wrap counted">
                                                <div class="desc">
                                                    <p>${e.texto}</p>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                <div class="col-auto">
                                    <div class="acciones-recurso-listado">
                                        <div class="dropdown">
                                            <a href="#" class="dropdown-toggle btn btn-outline-grey no-flecha px-1" role="button" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                <span class="material-icons px-0">more_vert</span>
                                            </a>
                                            <div class="dropdown-menu basic-dropdown dropdown-icons dropdown-menu-right">
                                                <p class="dropdown-title">Acciones</p>
                                                <ul class="no-list-style">

                                                    <li>
                                                        <a href="javascript:updateAnotacion('${e.id}', '${e.texto}')" class="item-dropdown">
                                                            <span class="material-icons">edit</span>
                                                            <span class="texto">Editar</span>
                                                        </a>
                                                    </li>

                                                    <li>
                                                        <a href="javascript:borrarAnotacion('${e.id}')" data-toggle="modal" class="item-dropdown">
                                                            <span class="material-icons">delete</span>
                                                            <span class="texto">Borrar</span>
                                                        </a>
                                                    </li>
                                                    
                                                </ul>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </article>`
		})
		let resourceItems = this.contenedor.getElementsByClassName('resource-list-wrap')[0]
		resourceItems.innerHTML = html.join('');

		// Llamamos al callback para realizar alguna acción una vez que se han pintado las notas
		callback()
	}

	/**
	 * Método que realiza las peticiones post 
	 * @param url, string con la url de la llamada
	 * @param args, objeto con los parámetros post para la llamada
	 */
	postCall (url, args) {

		return new Promise((resolve, reject) => {
			$.post(url.toString(), args, function (data) {
				resolve(data)
			})
		})
	}

	/**
	 * Método que crea o añade una nueva anotación 
	 * @param texto, string con el texto de la anotación
	 * @param idAnnotation, texto opcional con el id de la anotación
	 */
	newUpdateAnnotation (texto, idAnnotation = "") {
		return new Promise((resolve, reject) => {

			// Variables para la petición
			let args = {
				idRO: this.idRecurso,
				idUser: this.idUsuario,
				rdfType: this.rdfType,
				ontology: this.ontology,
				texto
			}
			// Comprueba si la anotación está cargada
			if (idAnnotation != "") {
				args.idAnnotation = idAnnotation
			}

			// Realizamos la llamada
			this.postCall(urlNA, args).then(data => {
				resolve(data)
			})
		})
	}
	/**
	 * Método que borra una anotación
	 * @param idAnnotation, string con el id de la anotación a borrar
	 */
	deleteAnnotation (idAnnotation) {
		return new Promise((resolve, reject) => {

			// Variables para la petición
			let args = {
				idAnnotation: idAnnotation,
			}
			// Realizamos la llamada
			this.postCall(urlDA, args).then(data => {
				resolve(data)
			})
		})
	}
}