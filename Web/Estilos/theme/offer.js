const uriSaveOffer = "Ofertas/SaveOffer"
const uriLoadOffer = "Ofertas/LoadOffer"
const uriLoadUsersGroup = "Ofertas/LoadUsersGroup"
// STEP 3
const uriLoadLineResearchs = "Ofertas/LoadLineResearchs"
const uriLoadFramingSectors = "Ofertas/LoadFramingSectors"
const uriLoadMatureStates = "Ofertas/LoadMatureStates"

var urlSOff ="";
var urlSTAGSOffer = "";
var urlLoadUsersGroup ="";

var urlLoadLineResearchs ="";
var urlLoadFramingSectors = "";
var urlLoadMatureStates ="";

$(document).ready(function () {
	servicioExternoBaseUrl=$('#inpt_baseURLContent').val()+'/servicioexterno/';
	urlSOff = new URL(servicioExternoBaseUrl +  uriSaveOffer);
	urlSTAGSOffer = new URL(servicioExternoBaseUrl +  uriSearchTags);
	urlLoadOffer = new URL(servicioExternoBaseUrl +  uriLoadOffer);
	urlLoadUsersGroup = new URL(servicioExternoBaseUrl +  uriLoadUsersGroup);

	urlLoadLineResearchs = new URL(servicioExternoBaseUrl +  uriLoadLineResearchs);
	urlLoadFramingSectors = new URL(servicioExternoBaseUrl +  uriLoadFramingSectors);
	urlLoadMatureStates = new URL(servicioExternoBaseUrl +  uriLoadMatureStates);
});




class StepsOffer {
	/**
	 * Constructor de la clase StepsOffer
	 */
	constructor() {
		var _self = this
		this.body = $('body')
		this.dataTaxonomies = null

		// Secciones principales
		this.crearOferta = this.body.find('#wrapper-crear-oferta')
		this.stepProgressWrap = this.crearOferta.find(".step-progress-wrap")
		this.stepsCircle = this.stepProgressWrap.find(".step-progress__circle")
		this.stepsBar = this.stepProgressWrap.find(".step-progress__bar")
		this.stepsText = this.stepProgressWrap.find(".step-progress__text")
		this.crearOfertaStep1 = this.crearOferta.find("#wrapper-crear-oferta-step1")
		this.crearOfertaStep2 = this.crearOferta.find("#wrapper-crear-oferta-step2")
		this.crearOfertaStep3 = this.crearOferta.find("#wrapper-crear-oferta-step3")
		this.crearOfertaStep4 = this.crearOferta.find("#wrapper-crear-oferta-step4")
		this.crearOfertaStep5 = this.crearOferta.find("#wrapper-crear-oferta-step5")
		this.ofertaAccordionPerfil = this.crearOferta.find("#accordion_oferta")
		this.errorDiv = this.crearOferta.find("#error-modal-oferta")
		this.errorDivStep2 = this.crearOferta.find("#error-modal-oferta-step2")
		this.errorDivStep2Equals = this.crearOferta.find("#error-modal-oferta-step2-equals")
		this.errorDivServer = this.crearOferta.find("#error-modal-server-oferta")
		this.researchersStep2 = this.crearOfertaStep2.find("#researchers-stp2-result-oferta")

		this.stepContentWrap = this.crearOferta.find(".steps-content-wrap")
		this.stepsContent = this.stepContentWrap.find(".section-steps")

		// Buttons
		this.botoneraSteps = this.crearOferta.find('#botonera-steps')
		this.btnBefore = this.botoneraSteps.find('.beforeStep')
		this.nextStep = this.botoneraSteps.find('.nextStep')
		this.endStep = this.botoneraSteps.find('.endStep')

		// Step 1
		this.listInvestigadoresSTP1 = this.crearOfertaStep1.find(".resource-list-investigadores > div")
		this.researchers = undefined
		this.numSeleccionadosInvestigadores = 0


		// STEP 3
		this.ddlMadurez = this.crearOfertaStep3.find("#ddlMadurez2")
		this.ddlEncuadre = this.crearOfertaStep3.find("#ddlEncuadre2")
		

		// Añadir perfil
		this.modalPerfil = this.body.find("#modal-anadir-perfil-oferta")
		this.inputPerfil = this.modalPerfil.find("#input-anadir-perfil")

		// Editar perfil
		this.modalPerfilEditar = this.body.find("#modal-editar-perfil-oferta")
		this.inputPerfilEditar = this.modalPerfilEditar.find("#input-editar-perfil")

		// Areas temáticas Modal
		this.modalAreasTematicas = this.body.find('#modal-seleccionar-area-tematica')
		this.divTesArbol = this.modalAreasTematicas.find('.divTesArbol')
		this.divTesLista = this.modalAreasTematicas.find('.divTesLista')
		this.divTesListaCaths = undefined
		this.btnSaveAT = this.modalAreasTematicas.find('.btnsave')
		this.cambiosAreasTematicas = 0

		// Steps info
		this.step = 1
		this.numSteps = this.stepsCircle.length

		// Información para el guardado 
		this.userId = document.getElementById('inpt_usuarioID').value
		this.ofertaId = undefined
		this.data = {
			researchers: {}
		}
		this.editDataSave = undefined
		this.communityShortName = $(this.crearOferta).data('cshortname')
		this.communityUrl = $(this.crearOferta).data('comurl')
		this.communityResourceUrl = this.communityUrl + '/' + $(this.crearOferta).data('urlrecurso')
		this.communityKey = $(this.crearOferta).data('comkey')
		this.currentLang = document.getElementById('inpt_Idioma').value

		// Tags
		this.topicsM = undefined

		// Textos obtenido de los 'data-'
		this.eliminarText = this.crearOferta.data("eliminartext")
		this.editarOfertaText = this.crearOferta.data("editaroferta")
		this.AnadirOtroPerfilText = this.crearOferta.data("addotherprofile")
		this.AnadirNuevoPerfilText = this.crearOferta.data("addnewprofile")
		this.areasTematicasText = this.crearOferta.data("areastematicastext")
		this.descriptoresEspecificosText = this.crearOferta.data("descriptoresespecificostext")
	}

	/**
	 * Método que inicia el funcionamiento funcionalidades necesarias para el creador 
	 * de ofertas
	 */
	init() {

		var _self = this

			
		// Carga los usuarios del grupo al que perteneces 
		_self.LoadUsersGroup().then((res) => {

			// Check if we need load the oferta (after the taxonomies are loaded)
			var currentUrl = new URL(window.location)
			_self.ofertaId = currentUrl.searchParams.get("id")
			if (_self.ofertaId) {
				// Load the oferta
				_self.loadOffer()
			}
		})

		// Fill taxonomies data
		// _self.LoadUsersGroup().then((res) => {
		// 	_self.fillDataTaxonomies(data)
		// 	_self.dataTaxonomies = data['researcharea']

			
		// })


		this.topicsM = new ModalSearchTagsOffer()
	}

	/**
	 * Método que carga el ofertas indicado e inicializa los datos con los parámetros 
	 * indicados
	 */
	loadOffer() {
		var _self = this
		this.callLoadOffer().then((res) => {
			this.data = res
			this.editDataSave = res
			var nameInput = document.getElementById('nombreofertainput')
			var descInput = document.getElementById('txtDescripcion')
			var selectTerms = this.crearOferta.find('#oferta-modal-sec1-tax-wrapper')

			$('h1').text(this.editarOfertaText)

			// Fill section 1
			nameInput.value = this.data.name
			descInput.value = this.data.description
			this.saveAreasTematicasEvent(selectTerms, this.data.terms)

			// Fill section 2
			this.data.profiles.forEach(profile => {
				profile.shortEntityID = profile.entityID.split('_')[2]

				// Crea el perfil
				// this.addPerfilSearch(profile).then(nameId => {

				// 	// Carga las áreas temáticas
				// 	let selectTerms = $('#modal-seleccionar-area-tematica-' + nameId)

				// 	let timer = setTimeout(function () {
				// 		if (selectTerms.length !== 0)
				// 		{
				// 			_self.saveAreasTematicasEvent(selectTerms, profile.terms)
				// 			clearTimeout(timer)
				// 		}
				// 	}, 100);

				// 	// carga los tags
				// 	let selectTags = $('#modal-seleccionar-tags-' + nameId)
				// 	let timerTerms = setTimeout(function () {
				// 		if (selectTerms.length !== 0)
				// 		{
				// 			_self.saveTAGS(selectTags, profile.tags)
				// 			clearTimeout(timerTerms)
				// 		}
				// 	}, 100);
				// })

				// Editamos los IDs de los usuarios
				profile.users.forEach(user => {
					user.shortUserID = user.userID.split('_')[1]
				})
			})
		});
	}

	/**
	 * Método que realiza una llamada ajax para cargar la oferta tecnológica
	 */
	callLoadOffer() {
		MostrarUpdateProgress();
		urlLoadOffer.searchParams.set('pIdOfertaId', this.ofertaId);
		return new Promise((resolve, reject) => {
			
			$.get(urlLoadOffer.toString(), function (res) {
				resolve(res);
				OcultarUpdateProgress();
			});
		})
	}

	/**
	 * Método que carga el personal investigador del grupo al que pertenece el usuario
	 * que está creando la oferta tecnológica actualmente
	 */
	LoadUsersGroup() {
		var _self = this
		MostrarUpdateProgress();
		return new Promise((resolve, reject) => {
			_self.callLoadUsersGroup().then((res) => {

				if (res && Object.keys(res).length > 0) {

					this.researchers = res;

					let imgUser = this.crearOferta.data('imguser')
					let resHtml = ""
					let i = 0
					for (const [idperson, datospersona] of Object.entries(res)) {
						resHtml += `
							<article class="resource" id="stp1-res-${idperson}" data-id="${idperson}">
					            <div class="custom-control custom-checkbox-resource add">
									<span class="material-icons">add</span>
								</div>
					            <div class="wrap">
					                <div class="usuario-wrap">
					                    <div class="user-miniatura">
					                        <div class="imagen-usuario-wrap">
					                            <a href="#">
					                                <div class="imagen">
					                                    <span style="background-image: url(${imgUser})"></span>
					                                </div>
					                            </a>
					                        </div>
					                        <div class="nombre-usuario-wrap">
					                            <a href="#" target="_blank">
					                                <p class="nombre">${datospersona.name}</p>
					                                <p class="nombre-completo">${datospersona.organization ? datospersona.organization + ',': ''} ${datospersona.hasPosition ? datospersona.hasPosition : ''} ${datospersona.departamento ? datospersona.departamento: ''}</p>
					                            </a>
					                        </div>
					                    </div>
					                </div>
					                <div class="publicaciones-wrap d-none"></div>
					            </div>
					        </article>
						`
					}
					_self.listInvestigadoresSTP1.html(resHtml)
					_self.listenChecks(Object.keys(res).map(e => "stp1-res-" + e))
					checkboxResources.init()
				} else {
					_self.listInvestigadoresSTP1.parent().parent().remove()
					_self.listInvestigadoresSTP1 = _self.crearOfertaStep1.find(".resource-list-investigadores > div")
				}

				OcultarUpdateProgress()
				resolve(true)
			})
		})
	}

	/**
	 * Comprueba los cambios para el los investigadores del primer nivel
	 * @param ids, array con los ids para 
	 * @param callback, Función para ejecutar después.
	 */
	listenChecks(ids, callback = () => {}) {

		let _self = this

		ids.forEach(idUser => {
			// Crea un evento para los investigadores del primer nivel, para detectar cambios en el dom
			$('#' + idUser).on("DOMSubtreeModified", function(e) {

				// Obtiene el ID del investigador
				let dataId = $(this).data("id")

				// Detecta si se ha seleccionado (texto igual a "done") o no
				let selector = $(this).find(".custom-checkbox-resource")
				if ($(selector).text().trim() == "done")
				{
					
					// Añade el investigador a la lista de investigadores seleccionados
					if(_self.data.researchers == null)					
					{
						_self.data.researchers = {}
					}
					_self.data.researchers[dataId] = _self.researchers[dataId]

				}else
				{
					if(_self.data.researchers == null)					
					{
						// Crea el objeto de investigadores si se encuentra vacío
						_self.data.researchers = {}
					} else {
						// Borrar investigador del objeto
						delete _self.data.researchers[dataId]
					}
				}

				_self.numSeleccionadosInvestigadores = Object.keys(_self.data.researchers).length
				callback()
			})	

		})
	}

	/**
	 * Método que es llamado por "LoadUsersGroup" para la petición ajax de carga del 
	 * personal investigador del grupo al que pertenece el usuario que está creando 
	 * la oferta tecnológica actualmente
	 */
	callLoadUsersGroup() {
		MostrarUpdateProgress()
		urlLoadUsersGroup.searchParams.set('pIdUserId', this.userId)
		return new Promise((resolve, reject) => {
			
			$.get(urlLoadUsersGroup.toString(), function (res) {
				resolve(res)
				OcultarUpdateProgress()
			})
		})
	}

	/**
	 * Método que inicia el proceso de ir al siguiente paso en el stepBar
	 */
	goNext() {
		this.goStep(this.step + 1)
	}

	/**
	 * Método que inicia el proceso de ir al paso anterior en el stepBar
	 */
	goBack() {
		if (this.step > 1) {
			this.goStep(this.step - 1)
		}
	}
	
	/**
	 * Método que inicia las comprobaciones para pasar a la siguiente sección
	 * @param pos: Posición a la que se quiere pasar
	 */
	async goStep(pos) {
		var _self = this

		// Vuelve atrás
		if (pos > 0 && this.step > pos) {

			this.errorDiv.hide()
			this.errorDivStep2.hide()
			this.errorDivStep2Equals.hide()
			this.errorDivServer.hide()
			this.setStep(pos)

		// Siguiente paso
		} else if(pos > 0) {

			this.errorDiv.hide()
			this.errorDivStep2.hide()
			this.errorDivStep2Equals.hide()
			this.errorDivServer.hide()

			let continueStep = true
			switch (this.step) {
				case 1:
				continueStep = this.checkContinue1()
				if (continueStep) {
					_self.startStep2()
				}
				break;
				case 2:
				continueStep = this.checkContinue2()
				if (continueStep) {
					_self.startStep3()
				}
				break;
				case 3:
				continueStep = this.checkContinue3()
				if (continueStep) {
					_self.startStep4()
				}
				break;
				case 4:
				try {
					continueStep = await this.saveInit()
					if (continueStep) {
						var urlCom = this.communityResourceUrl+"/"+ this.data.name.replace(/[^a-z0-9_]+/gi, '-').replace(/^-|-$/g, '').toLowerCase() +"/"+ this.ofertaId.split('_')[1]
						window.location = urlCom
					}
				} catch(err) {
					// this.errorDiv.show()
					this.errorDivServer.show()
					window.location.hash = '#' + this.errorDivServer.attr('id')
					continueStep = false
				}
				break;
			}

			if (continueStep && this.step > (pos - 2)) {
				this.errorDiv.hide()
				this.errorDivStep2.hide()
				this.errorDivStep2Equals.hide()
				this.errorDivServer.hide()
				this.setStep(pos)
			} else {
				if (this.step == 2) {
					this.errorDivStep2.show()
				} else {
					this.errorDiv.show()
				}
				window.location.hash = '#' + this.errorDiv.attr('id')
			}
		}
	}


	/**
	 * Método que comprueba que los campos obligatorios de la sección 1 han sido rellenados
	 * También guarda el estado de la sección 1
	 * @return bool: Devuelve true or false dependiendo de si ha pasado la validación
	 */
	checkContinue1() {
		var _self = this

		// Get first screen data
		let name = document.getElementById('nombreofertainput').value
		// let description = document.getElementById('txtDescripcion').value
		let tags = []
		let inputsTagsItms = this.crearOfertaStep1.find('#oferta-modal-seleccionar-tags-stp1').find('input')
		inputsTagsItms.each((i, e) => {tags.push(e.value)})

		// let researchers = {}
		// // Obtener los investigadores seleccionados
		// if (_self.listInvestigadoresSTP1 && _self.listInvestigadoresSTP1.length > 0) {
		// 	let reserchersDom = _self.listInvestigadoresSTP1.find(".resource.seleccionado")
		// 	reserchersDom.each((i, e) => {if (e) {researchers[$(e).data("id")] = _self.researchers[$(e).data("id")]}})
		// }

		this.numSeleccionadosInvestigadores = Object.keys(_self.data.researchers).length


		this.data = {
			...this.data,
			entityID: _self.ofertaId,
			name,
			tags,
			// researchers
		}

		return (name.length > 0 && tags.length > 0)
	}

	/**
	 * Método que comprueba que al menos hay un investigador para la sección 2
	 * También guarda el estado de la sección 2
	 * @return bool: Devuelve true or false dependiendo de si ha pasado la validación
	 */
	checkContinue2() {
		return this.data.researchers && Object.keys(this.data.researchers).length > 0
	}

	/**
	 * Método que comprueba que al menos hay un perfil con areas temáticas para la sección 2
	 * También guarda el estado de la sección 2
	 * @return bool: Devuelve true or false dependiendo de si ha pasado la validación
	 */
	checkContinue3() {
		var _self = this


		// Get the second screen
		let lineasInvestigacion = this.crearOfertaStep3.find('.edit-etiquetas')
		let inputsTermsProf = lineasInvestigacion.find('input')
		let profTerms = []
		inputsTermsProf.each((i, el) => {profTerms["id_" + el.value] = _self.divTesListaCaths[el.value]})


		// Set the post data
		this.data = {
			...this.data,
			lineResearchs: profTerms,
			matureState: _self.ddlMadurez.val(),
			framingSector: _self.ddlEncuadre.val(),
		}
		
		// Comprueba si las líneas de investigación se han rellenado
		let existenTerms = Object.keys(profTerms).length > 0

		// Comprueba si las estado de madurez se ha seleccionado
		let ematRellenado = _self.ddlMadurez.val() != ""

		// Comprueba si el sector de encuadre se ha seleccionado
		let sencuaRellenado = _self.ddlEncuadre.val() != ""

		return existenTerms && ematRellenado && sencuaRellenado
	}

	/**
	 * Método que genera la petición get para obtener las taxonomías
	 */
	getDataTaxonomies() {
		
		// https://localhost:44321/Oferta/GetThesaurus?listadoOferta=%5B%22researcharea%22%5D
		let listThesaurus = ["researcharea"]
		urlLT.searchParams.set('listThesaurus', JSON.stringify(listThesaurus))

		return new Promise((resolve, reject) => {
			$.get(urlLT.toString(), function (data) {
				resolve(data)
			})
		})
	}

	/**
	 * Inicia la generación del html para las diferentes taxonomías
	 * @param data, Objeto con los items
	 */
	fillDataTaxonomies(data) {
		// Set tree
		let resultHtml = this.fillTaxonomiesTreeWithoutParent(data)
		this.divTesArbol.find('.categoria-wrap').remove()
		this.divTesArbol.append(resultHtml)

		// Set list
		/* resultHtml = this.fillTaxonomiesList(data['researcharea'])
		this.divTesLista.append(resultHtml)
		this.divTesListaCaths = this.divTesLista.find(".categoria-wrap") */

		// Open & close event trigger
		let desplegables = this.modalAreasTematicas.find('.boton-desplegar')
	
		if (desplegables.length > 0) {
			desplegables.off('click').on('click', function () {
				$(this).toggleClass('mostrar-hijos')
			})
		}

		// Add events when the items are clicked
		this.itemsClicked()
	}

	/**
	 * Add events when the Taxonomies items are clicked
	 */
	itemsClicked() {

		var _self = this

		// Click into the tree
		this.divTesArbol.off('click').on("click", "input.at-input", function() {
			let dataVal = this.checked
			let dataId = $(this).attr('id')

			// Añadimos un cambio para las areas tematicas
			_self.cambiosAreasTematicas ++
			_self.btnSaveAT.removeClass('disabled')
			
		})
	}

	/**
	 * Crea el html con las taxonomías
	 * @param data, array con los items
	 * @param idParent, id del nodo padre, para generar los hijos
	 * @return string con el texto generado
	 */
	fillTaxonomiesTreeWithoutParent(data) {

		var _self = this

		let resultHtml = ""
		data.forEach((e, id) => {
			
			resultHtml += `<div class="categoria-wrap">
					<div class="categoria investigacion ${id}">
						<div class="custom-control custom-checkbox themed little primary">
							<input type="checkbox" class="custom-control-input at-input" id="${id}" data-id="${id}" data-name="${e}">
							<label class="custom-control-label" for="${id}">${e}</label>
						</div>
					</div>`

			resultHtml += '</div>'
		})

		return resultHtml
	}

	/**
	 * Crea el html con las taxonomías en arbol
	 * @param data, array con los items
	 * @return string con el texto generado
	 */
	fillTaxonomiesList(data) {

		var _self = this

		let resultHtml = ""
		data.forEach((e, id) => {
			resultHtml += '<div class="categoria-wrap" data-text="' + e + '">\
					<div class="categoria list__' + id + '">\
						<div class="custom-control custom-checkbox themed little primary">\
							<input type="checkbox" class="custom-control-input at-input" id="list__' + id + '" data-id="' + id + '" data-name="' + e + '">\
							<label class="custom-control-label" for="list__' + id + '">' + e + '</label>\
						</div>\
					</div>\
				</div>'
		})

		return resultHtml
	}


	/**
	 * Método que guarda los 2 pasos iniciales
	 */
	saveInit() {

		var _self = this
		this.data.pIdGnossUser=this.userId
		
		return new Promise((resolve) => {
			
			$.post(urlSOff, this.data)
				.done(
					function (rdata) {
						_self.ofertaId = rdata
						_self.data.entityID=rdata
						_self.startStep2()
						resolve(true)
					}
				)
				.fail(
					function (xhr, status, error) {
						resolve(false)
					}
				)
		})		

	}

	/** 
	 * Método que realiza la llamada POST
	 * @param url, objeto URL que contiene la url de la petición POST
	 * @param theParams, parámetros para la petición POST
	 */
	postCall(url, theParams) {
		let _self = this
		return new Promise((resolve) => {

			$.ajax({
				url: url.toString(),
				type: "POST",
				dataType: "json",
				crossDomain: true,
				contentType: "application/json; charset=utf-8",
				data: JSON.stringify(theParams),
				traditional: true,
				headers: {
					"accept": "application/json",
					"Access-Control-Allow-Origin":"*"
				},
				success: function(rdata) {
					resolve(rdata)
				},
				failure: function(err) {
					resolve(err)
				}
			})

			// $.post(url.toString(), theParams, function(rdata) {
			// 	resolve(rdata)
			// }).fail(function(err) {
			// 	resolve(err)
			// })
		})
	}

	/** 
	 * Método que realiza la llamada GET
	 * @param url, objeto URL que contiene la url de la petición POST
	 */
	getCall(url) {
		return new Promise((resolve) => {
			$.get(url.toString(), (rdata) => {
				resolve(rdata)
			})
		})
	}

	/**
	 * Carga todas las áreas temáticas seleccionadas para ese perfil / sección 
	 * @param item, sección donde se encuentra la información para cargar las areas temáticas
	 */
	setAreasTematicas(item) {
		let _self = this

		let relItem = $('#' + $(item).data("rel"))

		if (relItem.length > 0) {

			let dataJson = relItem.data('jsondata')

			// Reestablecer las categorías
			this.divTesArbol.find('input.at-input').each((i, e) => {
				e.checked = false
				e.classList.remove('selected')
			})

			// Establece las categorías de la sección
			if (typeof dataJson == "object") {

				dataJson.forEach(e => {
					let item = document.getElementById(e)
					
					if (item) {
						item.checked = true
					} else {
						console.log ("item no existe: #", e)
					}

				})
			}


			// Reestablecemos el botón de guardar las Áreas Temáticas
			this.cambiosAreasTematicas = 0
			this.btnSaveAT.addClass('disabled')

			// Muestra el modal de las áreas temáticas
			this.modalAreasTematicas.modal('show')

			this.saveAreasTematicasEvent(relItem)
		}
	}

	/**
	 * Método que inicia el modal de los tópicos 
	 */
	loadModalTopics(el) {

		let parent = $(el).parent()

		// Establecer los tags de la sección
		this.setTAGS(el)
		// Inicia el funcionamiento de los tags
		this.topicsM.init()

		this.topicsM.closeBtnClick().then((data) => {
			// parent.data('jsondata', data)
			let relItem = $('#' + $(el).data("rel"))
			this.saveTAGS(relItem, data)
		})
	}

	/**
	 * Método que genera el evento para añadir los tags selecciondas en el popup
	 * @param relItem, elemento relacionado para indicar dónde deben de guardarse las areas temátcias seleccionadas
	 */
	saveTAGS(relItem, data) {

		let htmlResWrapper = $('<div class="tag-list mb-4 d-inline"></div>')

		let htmlRes = ''
		let arrayItems = [] // Array para guardar los items que se van a usar
		data.forEach(e => {
			htmlRes += `<div class="tag" title="` + e + `" data-id="` + e + `">
				<div class="tag-wrap">
					<span class="tag-text">` + e + `</span>
					<span class="tag-remove material-icons">close</span>
				</div>
				<input type="hidden" value="` + e + `">
			</div>`
		})

		htmlResWrapper.append(htmlRes)

		relItem.html(htmlResWrapper)

		// Se añade un json para saber qué categorías se han seleccionado
		relItem.data('jsondata', data)

		this.deleteTAGS(relItem)
	}



	/**
	 * Método que elimina las etiquetas del arbol y del listado
	 * @param relItem, contenedor donde se encuentran las áreas temáticas seleccionadas
	 */
	deleteTAGS(relItem) {

		// Selecciona la áreas temáticas seleccionadas dentro de selector
		let tagsItems = relItem.find('.tag-wrap')
		tagsItems.on('click', '.tag-remove', function() {
			// Selecciona el item padre para eliminar
			let itemToDel = $(this).parent().parent()
			let inputShortId = itemToDel.data('id')

			// Remove the current item from array
			let jsonItems = relItem.data('jsondata')
			let myIndex = jsonItems.indexOf(inputShortId)
			if (myIndex !== -1) {
				jsonItems.splice(myIndex, 1)
			}


			// Delete item
			itemToDel.remove()

		})
	}


	/**
	 * Carga todas las áreas temáticas seleccionadas para ese perfil / sección 
	 * @param item, sección donde se encuentra la información para cargar las areas temáticas
	 */
	setTAGS(item) {
		let _self = this

		let relItem = $('#' + $(item).data("rel"))

		if (relItem.length > 0) {

			let dataJson = relItem.data('jsondata')

			// Reestablecer las categorías
			_self.topicsM.removeTags()

			// Establece las categorías de la sección
			if (typeof dataJson == "object") {

				dataJson.forEach(e => {
					_self.topicsM.addTag(e)
				})
			}
		}
	}


	/**
	 * Método que genera el evento para añadir las áreas temáticas selecciondas en el popup
	 * @param relItem, elemento relacionado para indicar dónde deben de guardarse las areas temáticas seleccionadas
	 * @param data, array con las areas temáticas seleccionadas
	 */
	saveAreasTematicasEvent(relItem, data = null) {


		if (data != null) {
			// Entra aquí por primera vez si el oferta ha sido guardado

			let htmlResWrapper = $('<div class="tag-list mb-4 d-inline"></div>')

			let htmlRes = ''
			let dataWithNames = []

			if (this.dataTaxonomies != null) {
				data.forEach(id => {
					dataWithNames.push({id, name: this.dataTaxonomies.find(e => e.id == id).name})
				})
			}

			let arrayRes = []
			dataWithNames.forEach(e => {
				htmlRes += '<div class="tag" title="' + e.name + '" data-id="' + e.id.split('/').pop() + '">\
					<div class="tag-wrap">\
						<span class="tag-text">' + e.name + '</span>\
						<span class="tag-remove material-icons">close</span>\
					</div>\
					<input type="hidden" value="' + e.id + '">\
				</div>'
				arrayRes.push(e.id.split('/').pop())
			})

			htmlResWrapper.append(htmlRes)
			relItem.html(htmlResWrapper)

			// Se añade un json para saber qué categorías se han seleccionado
			relItem.data('jsondata', arrayRes)

			this.deleteAreasTematicasEvent(relItem)

		} else {

			// Evento para cuando se seleccione guardar las áreas temáticas desde el popup
			this.btnSaveAT.off('click').on('click', (e) => {
				e.preventDefault()

				// Oculta el modal de las áreas temáticas
				this.modalAreasTematicas.modal('hide')

				// Reestablecemos el botón de guardar las Áreas Temáticas
				this.cambiosAreasTematicas = 0
				this.btnSaveAT.addClass('disabled')

				// Selecciona y establece el contenedor de las areas temáticas
				// let relItem = $('#' + $(item).data("rel"))

				let htmlResWrapper = $('<div class="tag-list mb-4 d-inline"></div>')

				let htmlRes = ''
				let arrayItems = [] // Array para guardar los items que se van a usar
				this.divTesArbol.find('input.at-input').each((i, e) => {
					if (e.checked) {
						htmlRes += '<div class="tag" title="' + $(e).data('name') + '" data-id="' + $(e).data('id') + '">\
							<div class="tag-wrap">\
								<span class="tag-text">' + $(e).data('name') + '</span>\
								<span class="tag-remove material-icons">close</span>\
							</div>\
							<input type="hidden" value="' + $(e).data('id') + '">\
						</div>'
						arrayItems.push(e.id)
					}
				})

				htmlResWrapper.append(htmlRes)

				relItem.html(htmlResWrapper)

				// Se añade un json para saber qué categorías se han seleccionado
				relItem.data('jsondata', arrayItems)

				this.deleteAreasTematicasEvent(relItem)

			})
		}
	}


	/**
	 * Método que elimina las áreas temáticas del arbol y del listado
	 * @param relItem, contenedor donde se encuentran las áreas temáticas seleccionadas
	 */
	deleteAreasTematicasEvent(relItem) {

		// Selecciona la áreas temáticas seleccionadas dentro de selector
		let tagsItems = relItem.find('.tag-wrap')
		tagsItems.on('click', '.tag-remove', function() {
			// Selecciona el item padre para eliminar
			let itemToDel = $(this).parent().parent()
			let inputShortId = itemToDel.data('id')

			if (inputShortId.length > 0) {
				inputShortId = inputShortId.split('/').pop()
			}

			// Set the inputs into the areas temáticas in false
			try {
				document.getElementById(inputShortId).checked = false
				// document.getElementById('list__' + inputShortId).checked = false
			}catch (error) { }

			// Remove the current item from array
			let jsonItems = relItem.data('jsondata')
			let myIndex = jsonItems.indexOf(inputShortId)
			if (myIndex !== -1) {
				jsonItems.splice(myIndex, 1)
			}


			// Delete item
			itemToDel.remove()

		})
	}


	/**
	 * Establece el "estado" del "step-progress"
	 * @param tstep, posición a establecer
	 */
	setStep(tstep) {
		this.step = tstep

		// Steps Checks
		this.stepsCircle.each((index, step) => {
			$(step).removeClass("done active")
		})

		// Bar step
		this.stepsBar.each((index, step) => {
			$(step).removeClass("active")
		})

		// Title step
		this.stepsText.each((index, step) => {
			$(step).removeClass("current")
		})

		// Content
		this.stepsContent.each((index, step) => {
			$(step).removeClass("show")
		})

		// Add class before
		for (var i = 0; i < (this.step - 1); i++) {
			$(this.stepsCircle[i]).addClass("done active")
			$(this.stepsBar[i]).addClass("active")

		}
		// Add class current
		$(this.stepsCircle[this.step - 1]).addClass("active")
		$(this.stepsContent[this.step - 1]).addClass("show")
		$(this.stepsText[this.step - 1]).addClass("current")

		// Set the buttons 
		this.btnBefore.hide()
		this.nextStep.hide()
		this.endStep.hide()
		if (tstep > 1) {
			this.btnBefore.show()
		}
		if (tstep < this.numSteps) {
			this.nextStep.show()
		} else {
			this.endStep.show()
		}
	}

	/**
	 * Inicia el paso 2
	 */
	startStep2() { 
		comportamientoPopupOferta.init(this.data)
		this.PrintSelectedUsersStp2 ()
		$('#sugeridos-oferta-tab').click()
	}

	/**
	 * Inicia el paso 3
	 */
	startStep3() { 
		let _self = this

		MostrarUpdateProgress();
		Promise.all([this.loadLineResearchs(), this.loadMatureStates(), this.loadFramingSectors()]).then(values => {

			let lineResearchs = values[0]
			let maturesStates = values[1]
			let framingsectors = values[2]

			// Carga las taxonomías
			_self.fillDataTaxonomies(lineResearchs)
			_self.divTesListaCaths = lineResearchs


			// Pintar Estado de madurez
			let htmlMaturesStates = ""
			for (const[i, el] of Object.entries(maturesStates)) {
				htmlMaturesStates += `<option value="${i}">${el}</option>`
			}
			this.ddlMadurez.append(htmlMaturesStates)
			this.ddlMadurez.select2();


			// Pintar categoría de encuadre
			let htmlFramingsectors = ""
			for (const[i, el] of Object.entries(framingsectors)) {
				htmlFramingsectors += `<option value="${i}">${el}</option>`
			}
			this.ddlEncuadre.append(htmlFramingsectors)
			this.ddlEncuadre.select2();

			OcultarUpdateProgress();
		})

		// this.PrintSelectedUsersStp2 ()
		// $('#sugeridos-oferta-tab').click()
	}

	/**
	 * Inicia el paso 4
	 */
	startStep4() { 
		let _self = this
		this.crearOfertaStep4.find('.edmaTextEditor').each((i, el) => {
			$(el).off('click').on('click', (event) => {
				if (!el.classList.contains("inicilized")) {
					new TextField(el);
				}
			})
		})

	}

	/** 
	 * Devuelve una nueva promesa con el listado de investigadores
	 */ 
	loadLineResearchs() {
		let _self = this
		return new Promise((resolve, reject) => {

			$.post(urlLoadLineResearchs, {pIdUsersId: Object.keys(_self.data.researchers)})
				.done(
					function (rdata) {
						resolve(rdata)
					}
				)
				.fail(
					function (xhr, status, error) {
						resolve(false)
					}
				)
		})
	}

	/** 
	 * Devuelve los estados de madurez de las ofertas
	 */
	loadMatureStates() {
		let _self = this
		return new Promise((resolve, reject) => {
			// Añado el idioma para obtener los resultados
			urlLoadFramingSectors.searchParams.set('lang', _self.currentLang)

			_self.getCall(urlLoadFramingSectors).then(rdata => {
				resolve(rdata)
			})
		})
	}

	/** 
	 * Devuelve los sectores de encaje de las ofertas
	 */
	loadFramingSectors() {
		let _self = this
		return new Promise((resolve, reject) => {
			// Añado el idioma para obtener los resultados
			urlLoadMatureStates.searchParams.set('lang', _self.currentLang)
			
			_self.getCall(urlLoadMatureStates).then(rdata => {
				resolve(rdata)
			})
		})
	}

	/**
	 * Pintar los perfiles "finales"
	 */
	PrintSelectedUsersStp2 () {

		let imgUser = this.crearOferta.data('imguser')

		let profiles = Object.keys(this.data.researchers).map((e, i) => {
			let htmlUser = ""

			// Obtenemos el usuario actual
			let user = this.data.researchers[e]

			// Creamos el literal para la información del usuario si esta no existe.
			if (user.info == undefined) {
				user.info = user.organization + ', ' + user.hasPosition + ' ' + user.departamento
			}

			if(user != null)
			{

				htmlUser = `
				<article class="resource">
                    <div class="wrap">
                        <div class="usuario-wrap">
                            <div class="user-miniatura">
                                <div class="imagen-usuario-wrap">
                                    <a href="#">
                                        <div class="imagen">
                                            <span style="background-image: url(${imgUser})"></span>
                                        </div>
                                    </a>
                                </div>
                                <div class="nombre-usuario-wrap">
                                    <a href="#">
                                        <p class="nombre">${user.name}</p>
                                        <p class="nombre-completo">`+ user.info +`</p>
                                    </a>
                                </div>
                            </div>
                        </div>
                        <div class="publicaciones-wrap">
                            ${user.numPublicacionesTotal}
                        </div>
                        <div class="acciones-wrap">
                            <ul class="no-list-style">
                                <li>
                                    <a href="javascript:stepsOffer.removeSelectedUserSelected('`+ e +`')" class="texto-gris-claro">
                                        Eliminar
                                        <span class="material-icons-outlined">delete</span>
                                    </a>
                                </li>
                            </ul>
                        </div>
                    </div>
                </article>`
			}

			return htmlUser
		})
		// Añadimos el html de los investigadores
		let htmlUsersCont = `
			<div class="resource-list listView resource-list-investigadores">
			    <div class="resource-list-wrap">
			    	${profiles.join('')}
			    </div>
			</div>`

		this.crearOfertaStep2.find('.resource-list-investigadores').remove()
		this.researchersStep2.append(htmlUsersCont)

		// pintamos el número de investigadores
		this.printNumResearchers()
	}

	/**
	 * Método que pinta el número de investigadores seleccionados
	 */
	printNumResearchers() {

		// Establecemos el número de investigadores
		this.crearOfertaStep2.find('.numResultados').text(this.numSeleccionadosInvestigadores)
		this.crearOfertaStep2.find('#stp2-num-selected').text('(' + this.numSeleccionadosInvestigadores + ')')
	}

	/**
	 * Borra los investigadores del perfil
	 */
	removeSelectedUserFromProfile(idProfile, idUser) {

		let currentProfile = stepsOffer.data.profiles.filter(function (perfilInt) {
			return perfilInt.shortEntityID==idProfile
		})[0]

		currentProfile.users=currentProfile.users.filter(function (userInt) {
			return userInt.shortUserID!=idUser
		})
		this.PrintSelectedUsersStp2()
	}
}


// class areasTematicasModal {
// 	constructor() {

// 		var _self = this
// 		this.body = $('body')
// 		this.modalAreasTematicas = this.body.find('#modal-seleccionar-area-tematica')

// 		this.modalAreasTematicas.on('shown.bs.modal', function () {
// 			_self.init()
// 		});
// 	}
// }



// función para actualizar la gráfica de colaboradores
function ActualizarGraficaOfertaolaboradoresOferta(typesOcultar = [], showRelation = true) {
	AjustarGraficaArania(dataCB, idContenedorCB, typesOcultar, showRelation)
}



function CompletadaCargaRecursosInvestigadoresOfertas()
{	
	let currentsIds = []
	if(typeof stepsOffer != 'undefined' && stepsOffer != null && stepsOffer.data != null)
	{		
		$('#ofertaListUsers article.resource h2.resource-title').attr('tagert','_blank')
		stepsOffer.data.pPersons = $('#ofertaListUsers article.resource').toArray().map(e => {return $(e).attr('id')})
		
		$('#ofertaListUsers article.resource').each((i, e) => {

			currentsIds.push(e.id)

			if ($(e).find(".custom-checkbox-resource .material-icons").length == 0) {

				if (Object.values(stepsOffer.data.researchers).filter(pr => pr.shortId == e.id).length > 0) {
					$(e).prepend(`<div class="custom-control custom-checkbox-resource done">
		                <span class="material-icons">done</span>
		            </div>`)
					$(e).addClass('seleccionado')
				}
				else {
					$(e).prepend(`<div class="custom-control custom-checkbox-resource add">
				        <span class="material-icons">add</span>
				    </div>`)
				}
			}
		})

		checkboxResources.init()

		currentsIds.forEach(idUser => {

			$('#' + idUser).on("DOMSubtreeModified", function(e) {

				let selector = $(this).find(".custom-checkbox-resource")

				if ($(selector).text().trim() == "done")
				{
					let elementUser = $(this)
					let user = {}
					let arrInfo = []
					user.shortId = idUser
					user.name = elementUser.find('h2.resource-title').text().trim()

					// Obtener la descripción
					elementUser.find('.middle-wrap > .content-wrap > .list-wrap li').each((i, elem) => {
						arrInfo.push($(elem).text().trim())
					})
					user.info = arrInfo.join(', ')

					let numPubDOM = $(this).find('.info-resource .texto')
					numPubDOM.each((i, e) => {
						let textPubDom = $(numPubDOM).text()
						if (textPubDom.includes('publicaciones')) {
							let numPub = textPubDom.split('publicaciones')[0].trim()
							user.numPublicacionesTotal = numPub
						}
					})

					// user.ipNumber = elementUser.data('ipNumber')
					if(stepsOffer.data.researchers == null)					
					{
						stepsOffer.data.researchers = {};
					}
					stepsOffer.data.researchers[idUser] = user

				}else
				{
					// Borrar investigador del objeto
					delete stepsOffer.data.researchers[idUser]
				}

				stepsOffer.numSeleccionadosInvestigadores = Object.keys(stepsOffer.data.researchers).length
				stepsOffer.PrintSelectedUsersStp2();
			});	

		})

		

		

		// $('article.resource .user-perfil').remove()
		// let htmlPerfiles=''
		// if(score.numPublicaciones>0)
		// {
		// 	let idProfileEdit = idProfile
		// 	if(idProfileEdit.length!=36)
		// 	{
		// 		idProfileEdit=idProfileEdit.split('_')[2]
		// 	}
		// 	let nombrePerfil = stepsOffer.data.researchers.filter(function (item) {return item.shortEntityID ==idProfileEdit || item.entityID ==idProfileEdit})[0].name
			
		// 	let publicationsPercent = score.numPublicaciones/score.numPublicacionesTotal*100



		// 	htmlPerfiles+=`	<div class="perfil-wrap">
		// 			        <div class="custom-wrap">
		// 			            <div class="custom-control custom-checkbox">
		// 			                <input type="checkbox" class="custom-control-input" id="${idperson}-${idProfileEdit}">
		// 			                <label class="custom-control-label" for="${idperson}-${idProfileEdit}">
		// 			                    ${nombrePerfil}
		// 			                </label>
		// 			            </div>
		// 			            <div class="check-actions-wrap">
		// 			                <a href="javascript: void(0)" class="dropdown-toggle check-actions-toggle" data-toggle="dropdown" aria-expanded="true">
		// 			                    <span class="material-icons">
		// 			                        arrow_drop_down
		// 			                    </span>
		// 			                </a>
		// 			                <div class="dropdown-menu basic-dropdown check-actions" id="checkActions" x-placement="bottom-start">
		// 			                    <div class="barras-progreso-wrapper">
		// 			                        <div class="progreso-wrapper">
		// 			                            <div class="progress">
		// 			                                <div class="progress-bar background-success" role="progressbar" style="width: ${score.ajuste * 100}%" aria-valuenow="25" aria-valuemin="0" aria-valuemax="100"></div>
		// 			                            </div>
		// 			                            <span class="progress-text"><span class="font-weight-bold">${Math.round(score.ajuste * 10000)/100}%</span></span>
		// 			                        </div>
		// 			                        <div class="progreso-wrapper">
		// 			                            <div class="progress">
		// 			                                <div class="progress-bar" role="progressbar" style="width: ${publicationsPercent}%" aria-valuenow="25" aria-valuemin="0" aria-valuemax="100"></div>
		// 			                            </div>
		// 			                            <span class="progress-text"><span class="font-weight-bold">${score.numPublicaciones} /</span> ${score.numPublicacionesTotal}</span>
		// 			                        </div>
		// 			                    </div>
		// 			                    <div class="wrap">
		// 			                        <div class="header-wrap">
		// 			                            <p>Areas temáticas</p>
		// 			                            <p>Publicaciones</p>
		// 			                        </div>
		// 			                        <div class="areas-tematicas-wrap">
		// 			                            <ul class="no-list-style">
		// 			                                ${termsHtml}
		// 			                            </ul>
		// 			                        </div>
		// 			                    </div>
		// 			                    <div class="wrap">
		// 			                        <div class="header-wrap">
		// 			                            <p>Descriptores</p>
		// 			                        </div>
		// 			                        <div class="descriptores-wrap">
		// 			                            <ul class="no-list-style">
		// 			                                ${tagsHtml}
		// 			                            </ul>
		// 			                        </div>
		// 			                    </div>
		// 			                </div>
		// 			            </div>
		// 			        </div>
		// 			        <div class="barras-progreso-wrap">
		// 			            <div class="progreso-wrapper">
		// 			                <div class="progress">
		// 			                    <div class="progress-bar background-success" role="progressbar" style="width: ${score.ajuste * 100}%" aria-valuenow="25" aria-valuemin="0" aria-valuemax="100"></div>
		// 			                </div>
		// 			                <span class="progress-text"><span class="font-weight-bold">${Math.round(score.ajuste * 10000)/100}%</span></span>
		// 			            </div>
		// 			            <div class="progreso-wrapper">
		// 			                <div class="progress">
		// 			                    <div class="progress-bar" role="progressbar" style="width: ${publicationsPercent}%" aria-valuenow="25" aria-valuemin="0" aria-valuemax="100"></div>
		// 			                </div>
		// 			                <span class="progress-text"><span class="font-weight-bold">${score.numPublicaciones} /</span> ${score.numPublicacionesTotal}</span>
		// 			            </div>
		// 			        </div>
		// 			    </div>`


		// }
		// let htmlPerfilesPersona=`	<div class="user-perfil pl-0">
		// 								${htmlPerfiles}
		// 							</div>`				
		// $('#'+idperson+' .content-wrap.flex-column').append(htmlPerfilesPersona)
		// try {
		// 	$('#'+idperson).data('numPublicacionesTotal', Object.values(datospersona)[0].numPublicacionesTotal)
		// 	$('#'+idperson).data('ipNumber', Object.values(datospersona)[0].ipNumber)
		// } catch (e) { }

		// let repintar = false
		// //Marcamos como checkeados los correspondientes
		// stepsOffer.data.researchers.forEach(function(perfil, index) {
		// 	let idProfile= perfil.shortEntityID
		// 	if(perfil.users!=null)
		// 	{
		// 		perfil.users.forEach(function(user, index) {
		// 			var elementUser = $('#'+user.shortUserID)
		// 			$('#'+user.shortUserID+'-'+idProfile).prop('checked', true)
		// 		})					
		// 	}
		// })
		// if (repintar) {
		// 	stepsOffer.PrintPerfilesstp3()
		// }
		
		// //Enganchamos el chek de los chekbox	
		// $('.perfil-wrap .custom-control-input').change(function() {
		// 	let id=$(this).attr('id');
		// 	let idUser=id.substring(0,36);
		// 	let idProfile=id.substring(37);					
		// 	let perfil=stepsOffer.data.researchers.filter(function (perfilInt) {
		// 		return perfilInt.shortEntityID==idProfile || perfilInt.entityID==idProfile ;
		// 	})[0];
		// 	if(this.checked) {
		// 		let elementUser = $(this).closest('.resource.investigador')
		// 		let user = {}
		// 		let arrInfo = []
		// 		user.shortUserID = idUser
		// 		user.name = elementUser.find('h2.resource-title').text().trim()

		// 		// Obtener la descripción
		// 		elementUser.find('.middle-wrap > .content-wrap > .list-wrap li').each((i, elem) => {
		// 			arrInfo.push($(elem).text().trim())
		// 		})
		// 		user.info = arrInfo.join(', ')

		// 		user.numPublicacionesTotal = elementUser.data('numPublicacionesTotal')
		// 		user.ipNumber = elementUser.data('ipNumber')
		// 		if(perfil.users==null)					
		// 		{
		// 			perfil.users=[];
		// 		}
		// 		perfil.users.push(user);
		// 	}else
		// 	{
		// 		perfil.users=perfil.users.filter(function (userInt) {
		// 			return userInt.shortUserID!=idUser;
		// 		});
		// 	}
		// 	stepsOffer.PrintPerfilesstp3();
		// 	newGrafProjClust.CargarGraficaColaboradores(stepsOffer.data, 'colaboratorsgraphCluster', true);
		// });	
	}
}




// Comportamiento página proyecto
var comportamientoPopupOferta = {
	tabActive: null,

	config: function () {
		var that = this;

		this.printitem = $('#ofertaListUsers')
		this.text_volumen = this.printitem.data('volumen')
		this.text_ajuste = this.printitem.data('ajuste')
		this.text_mixto = this.printitem.data('mixto')

		return;
	},

	init: function (ofertaObj) {
		let that = this
		this.config();
		// let paramsCl = this.workCO(ofertaObj)
		// let paramsResearchers = this.workCOProfiles(ofertaObj)
		// let researchers = this.setProfiles(ofertaObj)

		buscadorPersonalizado.profile=null;
		buscadorPersonalizado.search='searchOfertaMixto';
		
		// Iniciar el listado de usuarios
		// buscadorPersonalizado.init($('#INVESTIGADORES').val(), "#ofertaListUsers", "searchOfertaMixto=" + paramsCl, null, "profiles=" + JSON.stringify(profiles) + "|viewmode=oferta|rdf:type=person", $('inpt_baseUrlBusqueda').val(), $('#inpt_proyID').val());
		buscadorPersonalizado.init($('#INVESTIGADORES').val(), "#ofertaListUsers", null, null, "viewmode=oferta|rdf:type=person", $('inpt_baseUrlBusqueda').val(), $('#inpt_proyID').val());
		
		// Agregamos los ordenes
		// $('.searcherResults .h1-container').after(
		// `<div class="acciones-listado acciones-listado-buscador">
		// 	<div class="wrap">
				
		// 		<div class="ordenar dropdown">
		// 			<a class="dropdown-toggle" data-toggle="dropdown">
		// 				<span class="material-icons">swap_vert</span>
		// 				<span class="texto">${that.text_mixto}</span>
		// 			</a>
		// 			<div class="dropdown-menu basic-dropdown dropdown-menu-right">
		// 				<a href="javascript: void(0)" filter="searchOfertaMixto" class="item-dropdown">${that.text_mixto}</a>
		// 				<a href="javascript: void(0)" filter="searchOfertaVolumen" class="item-dropdown">${that.text_volumen}</a>
		// 				<a href="javascript: void(0)" filter="searchOfertaAjuste" class="item-dropdown">${that.text_ajuste}</a>
		// 			</div>
		// 		</div>
		// 	</div>
		// </div>`);
		
		// $('.acciones-listado-buscador a.item-dropdown').unbind().click(function (e) {
		// 	$('.acciones-listado-buscador .dropdown-toggle .texto').text($(this).text())
		// 	e.preventDefault();
		// 	buscadorPersonalizado.search=$(this).attr('filter');
		// 	if(buscadorPersonalizado.profile==null)
		// 	{
		// 		buscadorPersonalizado.filtro=$(this).attr('filter')+'='+paramsCl;
		// 	}else
		// 	{
		// 		buscadorPersonalizado.filtro=$(this).attr('filter')+'='+paramsResearchers[buscadorPersonalizado.profile];
		// 	}
		// 	FiltrarPorFacetas(ObtenerHash2());
		// });

		//Enganchamos comportamiento grafica seleccionados
		$('#seleccionados-oferta-tab').unbind().click(function (e) {			
			e.preventDefault();
			// newGrafProjClust.CargarGraficaSeleccionados(stepsOffer.data, 'selectedgraphOferta', true);
		});

		return;
	},

	/*
	* Convierte el objeto del oferta a los parámetros de consulta 
	*/
	workCO: function (ofertaObj) {

		let results = null
		if (ofertaObj && ofertaObj.profiles) {
			results = ofertaObj.profiles.map(e => {
				let terms = (e.terms.length) ? e.terms.map(itm => '<' + itm + '>').join(',') : "<>"
				let tags = (e.tags.length) ? e.tags.map(itm => "'" + itm + "'").join(',') : "''"
				return terms + '@@@' + tags
			}).join('@@@@')
		}

		return results
	},
	
	/*
	* Convierte el objeto del oferta a los parámetros de consulta 
	*/
	workCOProfiles: function (ofertaObj) {
		var dicPerfiles = [];
		stepsOffer.data.profiles.forEach(function(perfil, index) {
			let terms = (perfil.terms.length) ? perfil.terms.map(itm => '<' + itm + '>').join(',') : "<>"
			let tags = (perfil.tags.length) ? perfil.tags.map(itm => "'" + itm + "'").join(',') : "''"
			dicPerfiles[perfil.shortEntityID]=terms + '@@@' + tags
		});
		return dicPerfiles;
	},

	/*
	* Convierte los profiles en json 
	*/
	setProfiles: function (ofertaObj) {

		let results = null
		if (ofertaObj && ofertaObj.profiles) {
			ofertaObj.profiles.forEach((e, i) => {

			})
			results = ofertaObj.profiles.map((e, i) => (
				{
					[e.name.replace(/[^a-z0-9_]+/gi, '-').replace(/^-|-$/g, '').toLowerCase() + "-" + i]: e.name
				}
			))
		}

		return results
	}
};

/**
* Clase que contiene la funcionalidad del modal de los TAGS para el Oferta
*/
class ModalSearchTagsOffer {
	constructor() {
		this.body = $('body')
		this.modal = this.body.find('#modal-anadir-topicos-oferta')
		this.inputSearch = this.modal.find('#tagsSearchModalOferta')
		this.results = this.modal.find('.ac_results')
		this.resultsUl = this.results.find('ul')
		this.addedTags = []
		this.timeWaitingForUserToType = 750; // Esperar 0.75 segundos a si el usuario ha dejado de escribir para iniciar búsqueda
		this.tagsWrappper = this.modal.find('.tags ul')
		// this.ignoreKeysToBuscador = [37, 38, 39, 40, 46, 8, 32, 91, 17, 18, 20, 36, 18, 27];
		this.ignoreKeysToBuscador = [37, 38, 39, 40, 8, 32, 91, 17, 18, 20, 36, 18, 27];

		// Inicializa la funcionalidad del buscador de TAGS
		this.inputSearchEnter()

		/* if (window.location.hostname == 'depuracion.net' || window.location.hostname.includes("localhost")) {
			var urlSTAGSOffer = new URL(servicioExternoBaseUrl + 'servicioexterno/' + uriSearchTags)
		} */
	}

	/**
	 * Inicia la funcionalidad del modal
	 */
	init() {
		this.modal.modal('show') 
	}

	/**
	 * Funcionalidad para cuando se introduce un valor en el buscador
	 */
	inputSearchEnter() {
		let _self = this
		this.inputSearch.off('keyup').on('keyup', function(e) {
			let inputVal = this.value

			if (inputVal.length <= 1) 
			{
				_self.hiddenResults()
			}
			else {
				// Valida si el valor introducirdo en el input es 'válido'
				if (_self.validarKeyPulsada(e) == true) {
					// Limpia el 'time' para reinicializar la espera para iniciar la búsqueda de TAGS
					clearTimeout(_self.timer)
					// Espera 0.5s para realizar la búsqueda de TAGS
					_self.timer = setTimeout(function () {
						_self.hiddenResults()

						_self.searchCall(inputVal).then((data) => {
							// Muestra el selector 
							_self.results.show()
							if (data.length > 0) {
								// Pinta los resultados
								let resultHTML = data.map(e => {
									return '<li class="ac_even">' + e + '</li>'
								})
								_self.resultsUl.html(resultHTML.join(''))
								_self.addTagClick()

							} else {
								_self.mostrarPanelSinResultados()
							}
							
						})

					}, _self.timeWaitingForUserToType);
				}               
			}
		})
	}

	/**
	 * Oculta y borra los resultados del API
	 */
	hiddenResults() {
		this.resultsUl.html('')
		this.results.hide()
	}

	/**
	 * Muestra un texto "sin resultados" cuando no hay resultados del API
	 */
	mostrarPanelSinResultados() {
		this.resultsUl.html('<li>Sin resultados</li>')
	}

	/**
	 * Método que genera un inicia el evento de añadir el tag (al hacer 'click') cuando se 
	 * ha seleccionado un elemento de la lista de opciones disponibles 
	 */
	addTagClick() {
		let _self = this
		// Evento para el click
		this.resultsUl.off('click').on('click', 'li', function(e) {
			let texto = $(this).text()

			if (texto != "Sin resultados") {
				_self.addTag(texto) // Añade el texto
				_self.hiddenResults() // Vacía los resultados de la búsqueda
				_self.inputSearch.val('') // Vacía el campo de búsqueda
			}

		})
		// Delete list item  
	}

	/**
	 * Añade un tag al modal
	 * @param texto, texto del tag a añadir
	 */
	addTag(texto) {
		var _self = this
		let item = $(`<li>
						<a href="javascript: void(0);">
							<span class="texto">` + texto + `</span>
						</a>
						<span class="material-icons cerrar" data-texto="` + texto + `">close</span>
					</li>`)
		this.tagsWrappper.append(item)
		this.addedTags.push(texto)
		
		item.off('click').on('click', '.cerrar', function(e) {
			let texto = $(this).data('texto')
			let li = $(this).parent().remove()
			_self.addedTags = _self.addedTags.filter(txt => txt != texto)
		})
	}

	/**
	 * Remove all tags html
	 */
	removeTags() {
		this.tagsWrappper.html('')
	}

	/**
	 * Método que genera un evento para el botón "guardar" y devuelve el listado de los TAGS añadidas
	 * @return promise (array) con la lista de resultados 
	 */
	closeBtnClick() {
		let _self = this
		return new Promise((resolve, reject) => {

			this.modal.find('.btnclosemodal').off('click').on('click', function(e) {
			
				let result = new Array()
				_self.tagsWrappper.find('li .texto').each((i, item) => {
					result.push(item.innerText)
				})
				resolve(result)
			})
		})
	}


	/**
	 * Comprobará la tecla pulsada, y si no se encuentra entre las excluidas, dará lo introducido por válido devolviendo true
	 * Si se pulsa una tecla de las excluidas, devolverá false y por lo tanto el metabuscador no debería iniciarse
	 * @param {any} event: Evento o tecla pulsada en el teclado
	 * @return bool, devuelve si la tecla pulsada es válida o no
	 */
	validarKeyPulsada (event) {
		const keyPressed = this.ignoreKeysToBuscador.find(key => key == event.keyCode);
		if (keyPressed) {
			return false
		}
		return true;
	}

	/**
	 * Realiza la petición ajax (GET) para buscar los tags sugeridos en el input
	 * @param {string} inputVal: Texto para la búsqueda
	 * @return promise (array) con la lista de resultados 
	 */
	searchCall (inputVal) {
		var _self = this
		// Set the url parameters
		urlSTAGSOffer.searchParams.set('tagInput', inputVal)

		return new Promise((resolve) => {
			$.get(urlSTAGSOffer.toString(), function (data) {
				resolve(data.filter(itm => !_self.addedTags.includes(itm)))
			});
		})
	}
}
