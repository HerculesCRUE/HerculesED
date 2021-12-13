//JS común a todas las fichas con buscadores
//Buscador personalizado
var buscadorPersonalizado = {
	nombreelemento: null,
	contenedor: null,
	filtro: null,
	orden:null,
	init: function (nombreelemento, contenedor, filtro, orden, parametrosadicionales, urlcomunidad, idcomunidad, callback = () => {}) {
		this.nombreelemento = nombreelemento;
		this.contenedor = contenedor;
		this.filtro = filtro;
		this.orden = orden;
		this.config();

		history.pushState('', 'New URL: ', ResetearURL());
		urlComunidad = urlcomunidad;
		urlCargarAccionesRecursos = urlcomunidad+'/load-resource-actions';
		panFacetas = 'panFacetas';
		panResultados = 'panResultados';
		numResultadosBusq = 'panNumResultados';
		panFiltrosPulgarcito = 'panListadoFiltros';
		updResultados = 'panResultados';
		divFiltros = 'panFiltros';
		ubicacionBusqueda = 'Meta';
		grafo = idcomunidad.toLowerCase();
		adminVePersonas = 'False';
		tipoBusqeda = 0;
		idNavegadorBusqueda = 'panNavegador';
		ordenPorDefecto = 'gnoss:hasfechapublicacion';
		ordenEnSearch = 'gnoss:relevancia';
		filtroContexto = "";
		tiempoEsperaResultados = 0;
		suplementoFiltros = '';
		primeraCargaDeFacetas = false;
		parametros_adiccionales = parametrosadicionales;
		mostrarFacetas = true;
		mostrarCajaBusqueda = true;
		FiltrarPorFacetas("");
		MontarResultadosScroll.init('footer', 'article');
		MontarResultadosScroll.CargarResultadosScroll = function (data) {
			var htmlRespuesta = document.createElement("div");
			htmlRespuesta.innerHTML = data;
			$(htmlRespuesta).find('article').each(function () {
				$('#panResultados article').last().after(this);
			});
			comportamientoVerMasVerMenosTags.init();
			enlazarFacetasBusqueda();
			callback();
		}
		return;
	},
	config: function () {
		var that = this;
		$('.searcherTitle').remove();
		$('.searcherFacets').remove();
		$('.searcherResults').remove();
		var hmltBuscador = `
							<div class="col col-12 col-xl-3 col-facetas col-lateral izquierda searcherFacets">
								<div class="wrapCol">
									<div class="header-facetas">
										<p>Filtros</p>
										<a href="javascript: void(0);" class="cerrar">
											<span class="material-icons">close</span>
										</a>
									</div>
									<div id="panFacetas" class="facetas-wrap pmd-scrollbar mCustomScrollbar " data-mcs-theme="minimal-dark"></div>
								</div>
							</div>
							<div class="col col-12 col-xl-9 col-contenido derecha searcherResults">
								<div class="wrapCol">
									<div class="header-contenido">
										<!-- NÃºmero de resultados -->
										<div class="h1-container">
											<h1>${that.nombreelemento} <span id="panNumResultados" class="numResultados"></span></h1>
										</div>
										<!-- Etiquetas de filtros -->
										<div class="etiquetas" id="panFiltros">
											<ul class="facetedSearch tags" id="panListadoFiltros">
												<li class="borrarFiltros-wrap">
													<a class="borrarFiltros" href="#" onclick="event.preventDefault();LimpiarFiltros();">Borrar</a>
												</li>
											</ul>
										</div>
									</div>

									<!-- Resultados -->
									<div class="resource-list listView resource-list-buscador">
										<div id="panResultados" class="resource-list-wrap">
										</div>
									</div>
								</div>
							</div>`;
		$(this.contenedor).append(hmltBuscador);
	}
}

//Sobreescribimos FiltrarPorFacetas apra que coja el filtro por defecto (y el orden)
function FiltrarPorFacetas(filtro) {
	filtro += "|" + buscadorPersonalizado.filtro;
	if (buscadorPersonalizado.orden != null) {
		filtro += "|ordenarPor=" + buscadorPersonalizado.orden;
	}
	if (typeof (accionFiltrado) != 'undefined') {
		accionFiltrado(ObtenerHash2());
	}
	return FiltrarPorFacetasGenerico(filtro);
}

//Limpia los filtros y engancha el comportamineto del popup
function comportamientoCargaFacetasComunidad() {
	if (buscadorPersonalizado.filtro != null) {
		$('#panListadoFiltros a[name="' + buscadorPersonalizado.filtro + '"]').closest('li').remove();
		if ($('#panListadoFiltros li').length == 1) {
			$('#panListadoFiltros li').remove();
		}
	}
	comportamientoFacetasPopUp.init();
	plegarSubFacetas.init();
}

// Cuando se filtra no hay que subir arriba
function SubirPagina() {

}

//Resetea la URL eliminando los parámetros
function ResetearURL() {
	var urlActual = window.location.href;
	if (urlActual.includes("?")) {
		urlActual = urlActual.split("?")[0];
	}
	return urlActual;
}

//Scroll
MontarResultadosScroll.peticionScrollResultados = function () {
	var defr = $.Deferred();
	//Realizamos la peticion
	if (this.pagActual == null) {
		this.pagActual = 1;
	}
	this.pagActual++;
	var filtros = ObtenerHash2().replace(/&/g, '|');
	filtros += "|" + buscadorPersonalizado.filtro;
	if (buscadorPersonalizado.orden != null) {
		filtros += "|ordenarPor=" + buscadorPersonalizado.orden;
	}


	if (typeof (filtroDePag) != 'undefined' && filtroDePag != '') {
		if (filtros != '') {
			filtros = filtroDePag + '|' + filtros;
		}
		else {
			filtros = filtroDePag;
		}
	}
	filtros += "|pagina=" + this.pagActual;
	var params = {};

	params['pUsarMasterParaLectura'] = bool_usarMasterParaLectura;
	params['pProyectoID'] = $('input.inpt_proyID').val();
	params['pEsUsuarioInvitado'] = $('input.inpt_bool_esUsuarioInvitado').val() == 'True';
	params['pIdentidadID'] = $('input.inpt_identidadID').val();
	params['pParametros'] = '' + filtros.replace('#', '');
	params['pLanguageCode'] = $('input.inpt_Idioma').val();
	params['pPrimeraCarga'] = false;
	params['pAdministradorVeTodasPersonas'] = false;
	params['pTipoBusqueda'] = tipoBusqeda;
	params['pNumeroParteResultados'] = 1;
	params['pGrafo'] = grafo;
	params['pFiltroContexto'] = filtroContexto;
	params['pParametros_adiccionales'] = parametros_adiccionales;
	params['cont'] = contResultados;
	$.post(obtenerUrl($('input.inpt_UrlServicioResultados').val()) + "/CargarResultados", params, function (response) {
		if (params['cont'] == contResultados) {
			var data = response
			if (response.Value != null) {
				data = response.Value;
			}
			defr.resolve(data);
		}
	}, "json");
	return defr;
}

$(function () {
	// Comportamiento Ver Más / Ver Menos
	comportamientoVerMasVerMenosTags.init();

	// Comportamiento cabecera de las fichas
	mostrarFichaCabeceraFixed.init();
});