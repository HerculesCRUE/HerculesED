$(document).ready(function () {	
	comportamientoFacetasPopUp.init();	
	if ($('.resource-list-buscador').length) {
        MontarResultadosScroll.init('footer', 'article');
        MontarResultadosScroll.CargarResultadosScroll = function (data) {
            var htmlRespuesta = document.createElement("div");
            htmlRespuesta.innerHTML = data;
            $(htmlRespuesta).find('article').each(function () {
                $('#panResultados article').last().after(this);
            });
         comportamientoVerMasVerMenosTags.init();
		 enlazarFacetasBusqueda();
        }
	}
});

function comportamientoCargaFacetasComunidad() {	
	comportamientoFacetasPopUp.init();	
	plegarSubFacetas.init();
	comportamientoRangosFechas();
	comportamientoRangosNumeros();
}

var setFilter = function(element) {
	// actualizarValoresSlider(self, pos);
	// Get the element data
	let theId = element.id;
	let filterFinalPart1 = $(element).data("filterpart1");
	let filterFinalPart2 = $(element).data("filterpart2");
	let facekey = $(element).data("facekey");
	let inputname1 = $(element).data("inputname1");
	let inputname2 = $(element).data("inputname2");

	let minDate = $("#" + inputname1).val();
	let maxDate = Number.parseInt($("#" + inputname2).val()) + 1;

	// Set the url for the filter
	let filterString = filterFinalPart1 + minDate + '0000-' + maxDate + '0000&' + filterFinalPart2;
	// window.location = filterString;

	// Set name attr
	$(element).parent().find(".searchButton").attr("name", facekey + '=' + minDate + '0000-' + maxDate + '0000');
	// Set href attr
	$(element).parent().find(".searchButton").attr("href", filterString);
	setFilterButtons(element);
}

var setFilterNumbers = function(element) {
	// Get the element data
	let theId = element.id;
	let filterFinalPart1 = $(element).data("filterpart1");
	let filterFinalPart2 = $(element).data("filterpart2");
	let facekey = $(element).data("facekey");
	let inputname1 = $(element).data("inputname1");
	let inputname2 = $(element).data("inputname2");
	let minDate = $("#" + inputname1).val();
	let maxDate = $("#" + inputname2).val();
	// Set the url for the filter
	let filterString = filterFinalPart1 + minDate + '-' + maxDate + '&' + filterFinalPart2;
	// Set name attr
	$(element).parent().find(".searchButton").attr("name", facekey + '=' + minDate + '-' + maxDate);
	// Set href attr
	$(element).parent().find(".searchButton").attr("href", filterString);
}

var setFilterButtons = function(element) {
	let filterFinalPart1 = $(element).data("filterpart1");
	let filterFinalPart2 = $(element).data("filterpart2");
	let facekey = $(element).data("facekey");

	let actYear = new Date().getFullYear();
	let maxYear = actYear + 1;
	let min5year = actYear - 4;
	let lastYear = actYear;

	// Set the url for the filter
	let filterString = filterFinalPart1 + min5year + '0000-' + maxYear + '0000&' + filterFinalPart2;

	// LAST YEAR
	// Set name attr
	$(element).parent().find(".last5Years").attr("name", facekey + '=' + min5year + '0000-' + maxYear + '0000');
	// Set href attr
	$(element).parent().find(".last5Years").attr("href", filterString);

	// Set the url for the filter
	let filterStringAY = filterFinalPart1 + lastYear + '0000-' + maxYear + '0000&' + filterFinalPart2;

	// LAST YEAR
	// Set name attr
	$(element).parent().find(".lastYear").attr("name", facekey + '=' + lastYear + '0000-' + maxYear + '0000');
	// Set href attr
	$(element).parent().find(".lastYear").attr("href", filterStringAY);

	// Set the url for the filter
	let filterStringActY = filterFinalPart1 + '19000000-' + '23000000&' + filterFinalPart2;

	// ALL YEARS
	// Set name attr
	$(element).parent().find(".allYears").attr("name", facekey + '=' + '19000000-' + '23000000');
	// Set href attr
	$(element).parent().find(".allYears").attr("href", filterStringActY);
}

var changeSliderVals = function(sldrEl, el) {
	var value = el.value;
	totalVal = $(sldrEl).slider( "option", "values" );
	if (el.classList.contains('minVal')) {
		totalVal[0] = value;
	} else if(el.classList.contains('maxVal')) {
		totalVal[1] = value;
	}
	$(sldrEl).slider("values", totalVal);
}

function comportamientoRangosFechas()
{
	// Inicialite all facetas general range
	$(".faceta-general-range .ui-slider").each((i, e) => {
		$(e).slider({
			range: true,
			min: $(e).data('minnumber'),
			max: $(e).data('maxnumber'),
			values: [$(e).data('minnumber'), $(e).data('maxnumber')],
			slide: function(event, ui) {
				$("#" + $(e).data('inputname1')).val(ui.values[0]);
				$("#" + $(e).data('inputname2')).val(ui.values[1]);
			}
		});

		$(e).off("slidechange").on( "slidechange", function( event, ui ) {
			setFilter(e);
		});


		$(e).parent().find("input.filtroFacetaFecha").off("input").on( "input", function( event, ui ) {
			setFilter(e);
			changeSliderVals(e, this);
		});

		setFilter(e);
	});

	$('.faceta-general-range a.searchButton').unbind().click(function (e) {
		AgregarFaceta($(this).attr("name"),true);
		// Quitar el panel de filtrado para móvil para visualizar resultados correctamente
		$(body).removeClass("facetas-abiertas");
		e.preventDefault();
	});

	$('.faceta-general-range a.last5Years, .faceta-general-range a.lastYear, .faceta-general-range a.allYears').unbind().click(function (e) {
		AgregarFaceta($(this).attr("name"),true);
		// Quitar el panel de filtrado para móvil para visualizar resultados correctamente
		$(body).removeClass("facetas-abiertas");
		e.preventDefault();
	});
}

function comportamientoRangosNumeros()
{
	// Inicialite all facetas general range
	$(".faceta-general-number-range .ui-slider").each((i, e) => {
		$(e).slider({
			range: true,
			min: $(e).data('minnumber'),
			max: $(e).data('maxnumber'),
			values: [$(e).data('minnumber'), $(e).data('maxnumber')],
			slide: function(event, ui) {
				$("#" + $(e).data('inputname1')).val(ui.values[0]);
				$("#" + $(e).data('inputname2')).val(ui.values[1]);
		}
		});
		$(e).off("slidechange").on( "slidechange", function( event, ui ) {
			setFilterNumbers(e);
		});
		
		$(e).parent().find("input.filtroFacetaFecha").off("input").on( "input", function( event, ui ) {
			setFilterNumbers(e);
			changeSliderVals(e, this);
		});

		setFilterNumbers(e);
	});



	$('.faceta-general-number-range a.searchButton').unbind().click(function (e) {
		AgregarFaceta($(this).attr("name"),true);
		// Quitar el panel de filtrado para móvil para visualizar resultados correctamente
		$(body).removeClass("facetas-abiertas");
		e.preventDefault();
	});
}
	

function CompletadaCargaRecursosComunidad()
{	
	comportamientoVerMasVerMenos.init();
	comportamientoVerMasVerMenosTags.init();
	enlazarFacetasBusqueda();
	montarTooltip.init();
	contarLineasDescripcion.init();
}

comportamientoFacetasPopUp.numPaginas=2,
comportamientoFacetasPopUp.numResultadosPagina=10,
comportamientoFacetasPopUp.init= function () {
	this.config();
	this.IndiceFacetaActual = 0;
};
comportamientoFacetasPopUp.config= function () {
	//- -> :
	//--- -> @@@
	//1º Nombre de la faceta
	//2º Titulo del buscador ES
	//3º Titulo del buscador EN
	//4º True para ordenar por orden alfabético, False para utilizar el orden por defecto
	var that = this;
	this.facetasConPopUp = [
	['bibo:authorList@@@rdf:member@@@foaf:name', 'Busca por nombre o apellido de la persona', "Search by person's name or surname", true],
	['vivo:freeTextKeyword', 'Busca por nombre de la etiqueta', "Search by tag's name", true],
	['vivo:hasPublicationVenue', 'Busca por nombre de la revista', "Search by journal's name", true],
	['roh:hasKnowledgeArea@@@roh:categoryNode', 'Busca por área de conocimiento', "Search by knowledge area", true],
	['bibo:authorList@@@rdf:member@@@roh:hasKnowledgeArea@@@roh:categoryNode', 'Busca por tópicos', "Search by topics", true],
	['vivo:departmentOrSchool@@@dc:title', 'Busca por departamento', "Search by department", true],
	['vivo:relates@@@roh:title','Busca por grupos de investigación','Search by research group',true],
	['roh:lineResearch','Busca por línea de investigación','Search by research line',true]
	
	];
	//TODO textos
	for (i = 0; i < this.facetasConPopUp.length; i++) {
		var faceta = this.facetasConPopUp[i][0];
		var facetaSinCaracteres = faceta.replace(/\@@@/g, '---').replace(/\:/g, '_');
		var enlaceVerTodos = `<a class="no-close open-popup-link" href="#" data-toggle="modal" faceta="${i}" data-target="#modal-resultados">Ver todos</a>`;
		if (configuracion.idioma == 'en') 
		{
			enlaceVerTodos = `<a class="no-close open-popup-link" href="#" data-toggle="modal" faceta="${i}" data-target="#modal-resultados">See all</a>`;
		}
		if ($('#panFacetas #' + facetaSinCaracteres + ' .moreResults').length > 0) {
			if ($('#panFacetas #' + facetaSinCaracteres + ' .moreResults .open-popup-link ').length == 0) {
				$('#panFacetas #' + facetaSinCaracteres + ' .moreResults').html(enlaceVerTodos);
			} 
		}
	}		
	
	$('#panFacetas .open-popup-link').unbind().click(function(event) 
	{		
		$(".indice-lista.no-letra").html('');
		event.preventDefault();
		$('#modal-resultados .modal-dialog .modal-content .modal-title').text($($(this).closest('.box')).find('.faceta-title').text());
		that.IndiceFacetaActual = parseInt($(this).attr('faceta'));
		that.cargarFaceta();
	});	

	this.facetasConPopUpCategorias = [
		['roh:hasKnowledgeArea@@@roh:categoryNode', 'Busca por nombre de la categoría', "Search by category name", true]//Categorias
        ];
	for (i = 0; i < this.facetasConPopUpCategorias.length; i++) {
		var faceta = this.facetasConPopUpCategorias[i][0];
		var facetaSinCaracteres = faceta.replace(/\@@@/g, '---').replace(/\:/g, '_');
		var enlaceVerTodos = `<p class="moreResults"><a class="no-close open-popup-link open-popup-link-tesauro" href="#" data-toggle="modal" faceta="${i}" data-target="#modal-tesauro">Ver todos</a></p>`;
		if (configuracion.idioma == 'en') 
		{
			enlaceVerTodos =`<p class="moreResults"><a class="no-close open-popup-link open-popup-link-tesauro" href="#" data-toggle="modal" faceta="${i}" data-target="#modal-tesauro">See all</a></p>`;
		}
		$('#panFacetas #' + facetaSinCaracteres+' .moreResults').remove();
		$('#panFacetas #' + facetaSinCaracteres).append(enlaceVerTodos);
	}			
	
	$('#panFacetas .open-popup-link-tesauro').unbind().click(function(event) 
	{		
		$("#modal-tesauro input.texto").val('');
		event.preventDefault();		
		$('#modal-tesauro .modal-title').text($($(this).closest('.box')).find('.faceta-title').text());		
		if( $('input.inpt_Idioma').val()=='es')
		{
			$('#modal-tesauro .buscador-coleccion .buscar .texto').attr('placeholder',that.facetasConPopUpCategorias[0][1]);
		}else
		{
			$('#modal-tesauro .buscador-coleccion .buscar .texto').attr('placeholder',that.facetasConPopUpCategorias[0][2]);
		}	
		$('#modal-tesauro ul.listadoTesauro').html($(this).closest('.box').find('ul.listadoFacetas').html());
		$('#modal-tesauro ul.listadoTesauro .material-icons').html('expand_more');
		$('#modal-tesauro ul.listadoTesauro .num-resultados').addClass('textoFaceta');
		$('#modal-tesauro ul.listadoTesauro').addClass('facetedSearch');
		$('#modal-tesauro ul.listadoTesauro a').attr('data-dismiss','modal');
		$('#modal-tesauro ul.listadoTesauro a.applied').addClass('selected');
		plegarSubFacetas.init();
		enlazarFacetasBusqueda();
		operativaFormularioTesauro.init();
				
	});	

	//Autocompletar tesauros		
	$('#modal-tesauro input.texto').off('keyup').on('keyup', function (e) {		
		var txt=$(this).val();
		var lista = $('#modal-tesauro ul.listadoTesauro').find('li');
		lista.each(function(indice) {
			var item = $(this);
			var enlaceItem = item.children('a');
			var itemText = enlaceItem.text();
			item.removeClass('oculto');
			if (itemText.toLowerCase().indexOf(txt.toLowerCase()) < 0) {
				item.addClass('oculto');
			} else {
				item.removeHighlight().highlight(txt);
				item.parents('.oculto').removeClass('oculto');
			}
		});	
	});
	
	
};
comportamientoFacetasPopUp.eliminarAcentos= function (texto) {
	var ts = '';
	for (var i = 0; i < texto.length; i++) {
		var c = texto.charCodeAt(i);
		if (c >= 224 && c <= 230) { ts += 'a'; }
		else if (c >= 232 && c <= 235) { ts += 'e'; }
		else if (c >= 236 && c <= 239) { ts += 'i'; }
		else if (c >= 242 && c <= 246) { ts += 'o'; }
		else if (c >= 249 && c <= 252) { ts += 'u'; }
		else { ts += texto.charAt(i); }
	}
	return ts;
};
comportamientoFacetasPopUp.cargarFaceta= function () {
	var that = this;
	var FacetaActual = that.facetasConPopUp[that.IndiceFacetaActual][0];
	var facetaSinCaracteres = FacetaActual.replace(/\@@@/g, '---').replace(/\:/g, '--');
	this.paginaActual = 1;
	this.textoActual = '';
	this.fin = true;
	this.buscando = false;
	this.arrayTotales = null;

	if( $('input.inpt_Idioma').val()=='es')
	{
		$('.buscador-coleccion .buscar .texto').attr('placeholder',that.facetasConPopUp[that.IndiceFacetaActual][1]);
	}else
	{
		$('.buscador-coleccion .buscar .texto').attr('placeholder',that.facetasConPopUp[that.IndiceFacetaActual][2]);
	}
	$('.mfp-content h2').text($('#panFacetas div[faceta=' + facetaSinCaracteres + '] h2 ').text());
	this.textoActual = '';
	$(".indice-lista.no-letra").html('');
	
	var metodo = 'CargarFacetas';
	var params = {};
	params['pProyectoID'] = $('input.inpt_proyID').val();
	params['pEstaEnProyecto'] = $('input.inpt_bool_estaEnProyecto').val() == 'True';
	params['pEsUsuarioInvitado'] = $('input.inpt_bool_esUsuarioInvitado').val() == 'True';
	params['pIdentidadID'] = $('input.inpt_identidadID').val();
	//params['pParametros'] = '' + replaceAll(replaceAll(replaceAll(ObtenerHash2().replace(/&/g, '|').replace('#', ''), '%', '%25'), '#', '%23'), '+', "%2B");
	
	var filtros = ObtenerHash2();
    filtros = replaceAll(filtros, '%26', '---AMPERSAND---');
    filtros = decodeURIComponent(filtros);
    filtros = replaceAll(filtros, '---AMPERSAND---', '%26');
	filtros = replaceAll(filtros, '&', '|');
	params['pParametros']=filtros;
	
	
	if(typeof filtroPersonalizado !== 'undefined' && filtroPersonalizado!=null && filtroPersonalizado!='')
	{
		 params['pParametros'] +=filtroPersonalizado;
	}
	
	if(typeof buscadorPersonalizado !== 'undefined' && buscadorPersonalizado.filtro!=null && buscadorPersonalizado.filtro!='')
	{
		 params['pParametros'] +='|'+buscadorPersonalizado.filtro;
	}
	
	
	params['pLanguageCode'] = $('input.inpt_Idioma').val();
	params['pPrimeraCarga'] = false;
	params['pAdministradorVeTodasPersonas'] = false;
	params['pTipoBusqueda'] = tipoBusqeda;
	params['pGrafo'] = grafo;
	params['pFiltroContexto'] = filtroContexto;
	params['pParametros_adiccionales'] = parametros_adiccionales + '|NumElementosFaceta=10000|';
	params['pUbicacionBusqueda'] = ubicacionBusqueda;
	params['pNumeroFacetas'] = -1;
	params['pUsarMasterParaLectura'] = bool_usarMasterParaLectura;
	params['pFaceta'] = FacetaActual;

	$('.buscador-coleccion .buscar .texto').keyup(function () {				
		that.textoActual = that.eliminarAcentos($(this).val());
		that.paginaActual = 1;
		that.buscarFacetas();
	});



	$.post(obtenerUrl($('input.inpt_UrlServicioFacetas').val()) + "/" + metodo, params, function (data) {
		var htmlRespuesta = $('<div>').html(data);
		that.arrayTotales = new Array($(htmlRespuesta).find('.faceta').length);
		var i = 0;
		$(htmlRespuesta).find('.faceta').each(function () {
			that.arrayTotales[i] = new Array(2);
			that.arrayTotales[i][0] = that.eliminarAcentos($(this).text().toLowerCase());
			that.arrayTotales[i][1] = $(this);
			i++;
		});

		//Ordena por orden alfabético
		if (that.facetasConPopUp[that.IndiceFacetaActual][3]) {
			that.arrayTotales = that.arrayTotales.sort(function (a, b) {
				if (a[0] > b[0]) return 1;
				if (a[0] < b[0]) return -1;
				return 0;
			});
		}

		that.paginaActual = 1;
		
		$(".modal-body .buscador-coleccion .action-buttons-resultados").remove();
		$(".modal-body .buscador-coleccion").append($('<div></div>').attr('class', 'action-buttons-resultados'));
		$(".modal-body .buscador-coleccion .action-buttons-resultados").append($('<ul></ul>').attr('class', 'no-list-style'));
		
		$(".modal-body .buscador-coleccion .action-buttons-resultados .no-list-style").append($('<li></li>').attr('class', 'js-anterior-facetas-modal'));
		$(".modal-body .buscador-coleccion .action-buttons-resultados .no-list-style .js-anterior-facetas-modal").append($('<span></span>').attr('class', 'material-icons').text('navigate_before'));
		$(".modal-body .buscador-coleccion .action-buttons-resultados .no-list-style .js-anterior-facetas-modal").append($('<span></span>').attr('class', 'texto').text('Anteriores'));
		
		$(".modal-body .buscador-coleccion .action-buttons-resultados .no-list-style").append($('<li></li>').attr('class', 'js-siguiente-facetas-modal'));
		$(".modal-body .buscador-coleccion .action-buttons-resultados .no-list-style .js-siguiente-facetas-modal").append($('<span></span>').attr('class', 'texto').text('Siguientes'));
		$(".modal-body .buscador-coleccion .action-buttons-resultados .no-list-style .js-siguiente-facetas-modal").append($('<span></span>').attr('class', 'material-icons').text('navigate_next'));
					
		
		$('.modal-body .buscador-coleccion .action-buttons-resultados .no-list-style .js-anterior-facetas-modal .texto').click(function () {
			if (!that.buscando && that.paginaActual > 1) {
				that.buscando = true;
				that.paginaActual--;
				var hacerPeticion = true;
				$('.indice-lista ul').animate({
					marginLeft: 30,
					opacity: 0
				}, 200, function () {
					if (hacerPeticion) {
						that.buscarFacetas();
						hacerPeticion = false;
					}
					$('.indice-lista ul').css({ marginLeft: -30 });
					$('.indice-lista ul').animate({
						marginLeft: 20,
						opacity: 1
					}, 200, function () {
						// Left Animation complete.							
					});
				});
			}
		});

		$('.modal-body .buscador-coleccion .action-buttons-resultados .no-list-style .js-siguiente-facetas-modal .texto').click(function () {
			if (!that.buscando && !that.fin) {
				that.buscando = true;
				that.paginaActual++;
				var hacerPeticion = true;
				$('.indice-lista ul').animate({
					marginLeft: -30,
					opacity: 0
				}, 200, function () {
					if (hacerPeticion) {
						that.buscarFacetas();
						hacerPeticion = false;
					}
					$('.indice-lista ul').css({ marginLeft: 30 });
					$('.indice-lista ul').animate({
						marginLeft: 20,
						opacity: 1
					}, 200, function () {
						// Right Animation complete.							
					});
				});
			}
		});
		that.buscarFacetas();
	});
};
comportamientoFacetasPopUp.buscarFacetas= function () {
	buscando = true;
	this.textoActual = this.textoActual.toLowerCase();
	$(".indice-lista.no-letra ul").remove();
	
	$(".indice-lista.no-letra.facetas-wrap").append($('<div></div>').attr('class', 'resultados-wrap'));		
	
	var facetaMin = ((this.paginaActual - 1) * (this.numPaginas *this.numResultadosPagina)) + 1;
	var facetaMax = facetaMin + (this.numPaginas *this.numResultadosPagina) -1;

	var facetaActual = 0;
	var facetaPintadoActual = 0;
	var ul = $('<ul>');

	this.fin = true;

	var arrayTextoActual = this.textoActual.split(" ");

	for (i = 0; i < this.arrayTotales.length; i++) {
		var nombre = this.arrayTotales[i][0];

		var mostrar = true;
		for (j = 0; j < arrayTextoActual.length; j++) {
			mostrar = mostrar && nombre.indexOf(arrayTextoActual[j]) >= 0;
		}

		if (facetaPintadoActual < (this.numPaginas *this.numResultadosPagina) && mostrar) {
			facetaActual++;
			if (facetaActual >= facetaMin && facetaActual <= facetaMax) {
				facetaPintadoActual++;
				if (facetaPintadoActual % this.numResultadosPagina == 1) {
					ul = $('<ul>').attr('class', 'listadoFacetas').css('margin-left', '30px;').css('opacity', '1');
					$(".indice-lista.no-letra.facetas-wrap .resultados-wrap").append(ul);						
				} 
				var li = $('<li>');
				li.append(this.arrayTotales[i][1]);					
				ul.append(li);
			}
		}
		if (this.fin && facetaPintadoActual == (this.numPaginas *this.numResultadosPagina) && mostrar) {
			this.fin = false;
		}
	}
	
	$(".indice-lista.no-letra.facetas-wrap ul li a").each(function () {		
		if(!$(this).find('span.resultado').length)
		{
			var resultado=$(this).find('span.textoFaceta').text();
			var numResult=$($(this).find('span')[1]).html();
			$(this).empty();
			$(this).append('<span class="textoFaceta">'+resultado+'</span>');
			$(this).append('<span class="num-resultados">'+numResult+'</span>');
			$(this).attr('onclick',"$('#modal-resultados').modal('hide')");
		}			
	});

	$('.indice-lista .faceta').click(function (e) {
		AgregarFaceta($(this).attr("name").replace('#','%23'));
		$('button.mfp-close').click();
		e.preventDefault();
	});
	
	
	if(this.paginaActual==1)
	{
		$('.modal-body .buscador-coleccion .action-buttons-resultados .no-list-style .js-anterior-facetas-modal .texto').addClass('disabled');
		$('.modal-body .buscador-coleccion .action-buttons-resultados .no-list-style .js-anterior-facetas-modal .material-icons').addClass('disabled');
	}else
	{
		$('.modal-body .buscador-coleccion .action-buttons-resultados .no-list-style .js-anterior-facetas-modal .texto').removeClass('disabled');
		$('.modal-body .buscador-coleccion .action-buttons-resultados .no-list-style .js-anterior-facetas-modal .material-icons').removeClass('disabled');
	}
	if(this.fin)
	{
		$('.modal-body .buscador-coleccion .action-buttons-resultados .no-list-style .js-siguiente-facetas-modal .texto').addClass('disabled');
		$('.modal-body .buscador-coleccion .action-buttons-resultados .no-list-style .js-siguiente-facetas-modal .material-icons').addClass('disabled');
	}else
	{
		$('.modal-body .buscador-coleccion .action-buttons-resultados .no-list-style .js-siguiente-facetas-modal .texto').removeClass('disabled');
		$('.modal-body .buscador-coleccion .action-buttons-resultados .no-list-style .js-siguiente-facetas-modal .material-icons').addClass('disabled');
	}
	this.buscando = false;
}

var MontarResultadosScroll = {
    footer: null,
    item: null,
    pagActual: null,
    active: true,
    init: function (idFooterJQuery, idItemJQuery, callback = () => {}) {
        this.pagActual = 1;
        this.footer = $(idFooterJQuery);
        this.item = idItemJQuery;
        this.cargarScroll(callback());
        return;
    },
    cargarScroll: function (callback = () => {}) {
        var that = this;
        that.destroyScroll();
        // opciones del waypoint
        var opts = {
            offset: '100%'
        };
		contarLineasDescripcion.init();
        that.footer.waypoint(function (event, direction) {
            that.peticionScrollResultados().done(function (data) {
                that.destroyScroll();
                var htmlRespuesta = document.createElement("div");
                htmlRespuesta.innerHTML = data;
                if ($(htmlRespuesta).find(that.item).length > 0) {
                    that.CargarResultadosScroll(data);
                    that.cargarScroll();
                } else {
                    that.CargarResultadosScroll('');
                }
                if ((typeof CompletadaCargaRecursos != 'undefined')) {
                    CompletadaCargaRecursos();
                }
                if (typeof (urlCargarAccionesRecursos) != 'undefined') {
                    ObtenerAccionesListadoMVC(urlCargarAccionesRecursos);
                }
                console.log("llegado cargarScroll");
                callback();
            });
        }, opts);
        return;
    },
    destroyScroll: function () {
        this.footer.waypoint('destroy');
        return;
    },
    peticionScrollResultados: function () {
        var defr = $.Deferred();
        //Realizamos la peticion 
        if (this.pagActual == null) {
            this.pagActual = 1;
        }
        this.pagActual++;
        //var servicio = new WS($('input.inpt_UrlServicioResultados').val(), WSDataType.jsonp);
        var filtros = ObtenerHash2().replace(/&/g, '|');

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
}

montarTooltip.montarTooltips= function () {
	var that = this;	
	this.quotes.each(function () {
			console.log("toltipFN(montarTooltip)");
			var scopusInt=$(this).data('scopus');
			var wosInt=$(this).data('wos');
			var inrecsInt=$(this).data('inrecs');
			var otrasCitas=$(this).data('otros');

			var htmlScopus = "";
			if(typeof scopusInt !== "undefined" && scopusInt != "" && scopusInt != "0"){
				htmlScopus=`
				<li>					
					<span class="texto">SCOPUS</span>
					<span class="num-resultado">${scopusInt}</span>					
				</li>`;
			}
			
			var htmlWos = "";
			if(typeof wosInt !== "undefined" && wosInt != "" && wosInt != "0"){
				htmlWos=`
				<li>					
					<span class="texto">WOS</span>
					<span class="num-resultado">${wosInt}</span>					
				</li>`;
			}
			
			var htmlInrecs = "";
			if(typeof inrecsInt !== "undefined" && inrecsInt != "" && inrecsInt != "0"){
				htmlInrecs=`
				<li>					
					<span class="texto">INRECS</span>
					<span class="num-resultado">${inrecsInt}</span>					
				</li>`;
			}
			
			var htmlOtros = "";
			if(typeof otrasCitas !== "undefined" && otrasCitas != ""){
				
				var listaSplit = otrasCitas.split("|");
				
				if(listaSplit != null && listaSplit.length > 0)
				{
					listaSplit.forEach( function(valor, indice, array) {
						var nombreCita = valor.split("~")[0];
						var numCita = valor.split("~")[1];
						if(nombreCita != "" && numCita != "")
						{
							htmlOtros +=`
							<li>					
								<span class="texto">${nombreCita}</span>
								<span class="num-resultado">${numCita}</span>					
							</li>`;
						}
					});
				}
			}
			
			var html=`<p class="tooltip-title">Fuente de citas</p>
                <ul class="no-list-style">
				${htmlScopus}				
                ${htmlWos}
                ${htmlInrecs}
				${htmlOtros}
                </ul>`;
				
			if((typeof scopusInt !== "undefined" && scopusInt != "" && scopusInt != "0") || (typeof wosInt !== "undefined" && wosInt != "" && wosInt != "0") || (typeof inrecsInt !== "undefined" && inrecsInt != "" && inrecsInt != "0") || (typeof otrasCitas !== "undefined" && otrasCitas != "" && otrasCitas != "0"))
			{
				$(this).tooltip({
					html: true,
					placement: 'bottom',
					template: '<div class="tooltip background-blanco citas" role="tooltip"><div class="tooltip-arrow"></div><div class="tooltip-inner"></div></div>',
					title: html
				});
			}
	});
}

function MontarResultados(pFiltros, pPrimeraCarga, pNumeroResultados, pPanelID, pTokenAfinidad) {
    contResultados = contResultados + 1;
    if (document.getElementById('ctl00_ctl00_CPH1_CPHContenido_txtRecursosSeleccionados') != null) {
        document.getElementById('ctl00_ctl00_CPH1_CPHContenido_txtRecursosSeleccionados').value = '';
        document.getElementById('ctl00_ctl00_CPH1_CPHContenido_lblErrorMisRecursos').style.display = 'none';
    }
    var servicio = new WS($('input.inpt_UrlServicioResultados').val(), WSDataType.jsonp);

    var paramAdicional = parametros_adiccionales;

    /*
    if ($('li.mapView').attr('class') == "mapView activeView") {
        paramAdicional += 'busquedaTipoMapa=true';
    }*/
    /*
    if ($('.chartView').attr('class') == "chartView activeView") {
        paramAdicional = 'busquedaTipoChart=' + chartActivo + '|' + paramAdicional;
    }*/

    if ($('li.mapView').hasClass('activeView')) {
        paramAdicional += 'busquedaTipoMapa=true';
    }


    if ($('.chartView').hasClass('activeView')) {
        paramAdicional = 'busquedaTipoChart=' + chartActivo + '|' + paramAdicional;
    }

    var metodo = 'CargarResultados';
    var params = {};

    if (bool_usarMasterParaLectura) {
        if (finUsoMaster == null) {
            finUsoMaster = new Date();
            finUsoMaster.setMinutes(finUsoMaster.getMinutes() + 1);
        }
        else {
            var fechaActual = new Date();
            if (fechaActual > finUsoMaster) {
                bool_usarMasterParaLectura = false;
                finUsoMaster = null;
            }
        }
    }

    params['pUsarMasterParaLectura'] = bool_usarMasterParaLectura;
    params['pProyectoID'] = $('input.inpt_proyID').val();
    params['pEsUsuarioInvitado'] = $('input.inpt_bool_esUsuarioInvitado').val() == 'True';

    if (typeof (identOrg) != 'undefined') {
	 
        params['pIdentidadID'] = identOrg;
    }
    else {
	 
        params['pIdentidadID'] = $('input.inpt_identidadID').val();
    }
    params['pParametros'] = '' + pFiltros.replace('#', '');
    params['pLanguageCode'] = $('input.inpt_Idioma').val();
    params['pPrimeraCarga'] = pPrimeraCarga == "True";
    params['pAdministradorVeTodasPersonas'] = adminVePersonas == "True";
    params['pTipoBusqueda'] = tipoBusqeda;
    params['pNumeroParteResultados'] = pNumeroResultados;
    params['pGrafo'] = grafo;
    params['pFiltroContexto'] = filtroContexto;
    params['pParametros_adiccionales'] = paramAdicional;
    params['cont'] = contResultados;
    params['tokenAfinidad'] = pTokenAfinidad;

    $.post(obtenerUrl($('input.inpt_UrlServicioResultados').val()) + "/" + metodo, params, function (response) {
        if (params['cont'] == contResultados) {
            var data = response
            if (response.Value != null) {
                data = response.Value;
            }

            var vistaMapa = (params['pParametros_adiccionales'].indexOf('busquedaTipoMapa=true') != -1);
            var vistaChart = (params['pParametros_adiccionales'].indexOf('busquedaTipoChart=') != -1);

            var descripcion = data;

            var funcionJS = '';
            if (descripcion.indexOf('###ejecutarFuncion###') != -1) {
                var funcionJS = descripcion.substring(descripcion.indexOf('###ejecutarFuncion###') + '###ejecutarFuncion###'.length);
                funcionJS = funcionJS.substring(0, funcionJS.indexOf('###ejecutarFuncion###'));

                descripcion = descripcion.replace('###ejecutarFuncion###' + funcionJS + '###ejecutarFuncion###', '');
            }

            if (tipoBusqeda == 12) {
                var panelListado = $(pPanelID).parent();
                panelListado.html('<div id="' + pPanelID.replace('#', '') + '"></div><div id="' + panResultados.replace('#', '') + '"></div>')

                var panel = $(pPanelID);
                panel.css('display', 'none');
                panel.html(descripcion);
                panelListado.append(panel.find('.resource-list').html())
                panel.find('.resource-list').html('');
            } else if (!vistaMapa && !vistaChart) {
                $(pPanelID).append(descripcion);
            }
            else {
                var arraydatos = descripcion.split('|||');

                if ($('#panAuxMapa').length == 0) {
                    $(pPanelID).parent().html($(pPanelID).parent().html() + '<div id="panAuxMapa" style="display:none;"></div>');
                }

                if (vistaMapa) {
                    $('#panAuxMapa').html('<div id="numResultadosRemover">' + arraydatos[0] + '</div>');
                }

                if (vistaChart) {
                    datosChartActivo = arraydatos;
                    $(pPanelID).html('<div id="divContChart"></div>');
                    eval(jsChartActivo);
                }
                else {
                    utilMapas.MontarMapaResultados(pPanelID, arraydatos);
                }
            }
            FinalizarMontarResultados(paramAdicional, funcionJS, pNumeroResultados, pPanelID);
        }
        if (MontarResultadosScroll.pagActual != null) {
            MontarResultadosScroll.pagActual = 1;
            MontarResultadosScroll.cargarScroll();
        }
    }, "json");
}


function FiltrarPorFacetasGenerico(filtro) {
    filtro = filtro.replace(/&/g, '|');

    if (typeof (filtroDePag) != 'undefined' && filtroDePag != '') {
        if (filtro != '') {
            filtro = filtroDePag + '|' + filtro;
        }
        else {
            filtro = filtroDePag;
        }
    }
    //Si hay orden por relevancia pero no hay filtro search, quito el orden para que salga el orden por defecto
    //    if(QuitarOrdenReleavanciaSinSearch(filtro))
    //    {
    //        return false;
    //    }
    filtrosPeticionActual = filtro;

    var rdf = false;
    if (filtro.indexOf('?rdf') != -1 && ((filtro.indexOf('?rdf') + 4) == filtro.length)) {
        filtro = filtro.substring(0, filtro.length - 4);
        document.location.hash = document.location.hash.substring(0, document.location.hash.length - 4);
        rdf = true;
    }

    enlazarJavascriptFacetas = true;

    var arg = filtro;

    /*
    var vistaMapa = ($('li.mapView').attr('class') == "mapView activeView");
    var vistaChart = ($('.chartView').attr('class') == "chartView activeView");
    */

    var vistaMapa = $('li.mapView').hasClass('activeView');
    var vistaChart = $('.chartView').hasClass('activeView');

    if (!primeraCargaDeFacetas && !vistaMapa) {
        MostrarUpdateProgress();
    }

    var parametrosFacetas = 'ObtenerResultados';

    var gruposPorTipo = $('#facetedSearch.facetedSearch .listadoAgrupado ').length>0;

    if (cargarFacetas && !gruposPorTipo) {
        if (typeof panFacetas != "undefined" && panFacetas != "" && $('#' + panFacetas).length > 0 && !primeraCargaDeFacetas && !gruposPorTipo) {
            $('#' + panFacetas).html('')
        }
        if (numResultadosBusq != "" && $('#' + numResultadosBusq).length > 0 && !primeraCargaDeFacetas) {
            $('#' + numResultadosBusq).html('')
            $('#' + numResultadosBusq).css('display', 'none');
        }
        if (!clickEnFaceta && panFiltrosPulgarcito != "" && $('#' + panFiltrosPulgarcito).length > 0 && !primeraCargaDeFacetas) {
            $('#' + panFiltrosPulgarcito).html('')
        }
    }

    if (!vistaMapa) {
        SubirPagina();
    }

    if (typeof idNavegadorBusqueda != "undefined") {
        $('#' + idNavegadorBusqueda).html('');
        $('#' + idNavegadorBusqueda).css('display', 'none');
    }

    if (!vistaMapa && !primeraCargaDeFacetas) {
        // Vaciar el contenido actual de resultados - Nuevo Front
        // document.getElementById(updResultados).innerHTML = '';
        $(`#${updResultados}`).html('');
        $('#' + updResultados).attr('style', '');
    }

    clickEnFaceta = false;
    var primeraCarga = false;

    if (filtro.length > 1 || document.location.href.indexOf('/tag/') > -1 || (filtroContexto != null && filtroContexto != '')) {
        parametrosFacetas = 'AgregarFacetas|' + arg;
        var parametrosResultados = 'ObtenerResultados|' + arg;
        if (!cargarFacetas) {
            var parametrosResultados = 'ObtenerResultadosSinFacetas|' + arg;
        }
        //cargarFacetas
        var displayNone = '';
        document.getElementById('query').value = parametrosFacetas;
        if (HayFiltrosActivos(filtro) && (tipoBusqeda != 12 || filtro.indexOf("=") != -1)) {
            $('#' + divFiltros).css('display', '');
            $('#' + divFiltros).css('padding-top', '0px !important');
            //$('#' + divFiltros).css('margin-top', '10px');
        }
        var pLimpiarFilt = $('p', $('#' + divFiltros)[0]);

        if (pLimpiarFilt != null && pLimpiarFilt.length > 0) {
            if (!(filtro.length > 1 || document.location.href.indexOf('/tag/') > -1)) {
                pLimpiarFilt[0].style.display = 'none';
            }
            else {
                pLimpiarFilt[0].style.display = '';
            }
        }
    }
    else {
        primeraCarga = true;
        $('#' + divFiltros).css('display', 'none');
        $('#' + divFiltros).css('padding-top', '0px !important');
        //$('#' + divFiltros).css('margin-top', '10px');
    }

    if (rdf) {
        eval(document.getElementById('rdfHack').href);
    }

    var tokenAfinidad = guidGenerator();

    if (vistaMapa || !primeraCargaDeFacetas) {
        MontarResultados(filtro, primeraCarga, 1, '#' + panResultados, tokenAfinidad);
    }

    if (panFacetas != "" && (cargarFacetas || document.getElementById(panFacetas).innerHTML == '')) {
        var inicioFacetas = 1;

        MontarFacetas(filtro, primeraCarga, inicioFacetas, '#' + panFacetas, null, tokenAfinidad);
    }

    primeraCargaDeFacetas = false;
    cargarFacetas = true;

    var txtBusquedaInt = $('.aaCabecera .searchGroup .text')
    var textoSearch = 'search=';
    if ((filtro.indexOf(textoSearch) > -1) && txtBusquedaInt.length > 0) {
        var filtroSearch = filtro.substring(filtro.indexOf(textoSearch) + textoSearch.length);
        if (filtroSearch.indexOf('|') > -1) {
            filtroSearch = filtroSearch.substring(0, filtroSearch.indexOf('|'));
        }

        txtBusquedaInt.val(decodeURIComponent(filtroSearch));
        txtBusquedaInt.blur();
    }
    CambiarOrden(filtro);
    return false;
}


function AgregarFaceta(faceta,eliminarFiltroAnterior=false) {
    faceta = faceta.replace(/%22/g, '"');
    estamosFiltrando = true;
    //var filtros = ObtenerHash2().replace(/%20/g, ' ');
    var filtros = ObtenerHash2();
    filtros = replaceAll(filtros, '%26', '---AMPERSAND---');
    filtros = decodeURIComponent(filtros);
    filtros = replaceAll(filtros, '---AMPERSAND---', '%26');
	


    var esFacetaTesSem = false;

    if (faceta.indexOf('|TesSem') != -1) {
        esFacetaTesSem = true;
        faceta = faceta.replace('|TesSem', '');
    }

    var eliminarFacetasDeGrupo = '';
    if (faceta.indexOf("rdf:type=") != -1 && filtros.indexOf(faceta) != -1) {
        //Si faceta es RDF:type y filtros la contiene, hay que eliminar las las que empiezen por el tipo+;
        eliminarFacetasDeGrupo = faceta.substring(faceta.indexOf("rdf:type=") + 9) + ";";
    }

    var filtrosArray = filtros.split('&');
    filtros = '';

    var tempNamesPace = '';
    if (faceta.indexOf('|replace') != -1) {
        tempNamesPace = faceta.substring(0, faceta.indexOf('='));
        faceta = faceta.replace('|replace', '');
    }

    var facetaDecode = decodeURIComponent(faceta);
    var contieneFiltro = false;

    for (var i = 0; i < filtrosArray.length; i++) {
        if (filtrosArray[i] != '' && filtrosArray[i].indexOf('pagina=') == -1) {
            if (eliminarFacetasDeGrupo == '' || filtrosArray[i].indexOf(eliminarFacetasDeGrupo) == -1) {
                if (tempNamesPace == '' || (tempNamesPace != '' && filtrosArray[i].indexOf(tempNamesPace) == -1)) {
                    filtros += filtrosArray[i] + '&';
                }
            }
        }

        if (filtrosArray[i] != '' && (filtrosArray[i] == faceta || filtrosArray[i] == facetaDecode)) {
            contieneFiltro = true;
        }
    }

    if (filtros != '') {
        filtros = filtros.substring(0, filtros.length - 1);
    }
    if (faceta.indexOf('search=') == 0) {
	 
        $('h1 span#filtroInicio').remove();
    }

    if (typeof (filtroDePag) != 'undefined' && filtroDePag.indexOf(faceta) != -1) {
        var url = document.location.href;
        //var filtros = '';

        if (filtros != '') {
            filtros = '?' + filtros.replace(/ /g, '%20');
            //filtros = '?' + encodeURIComponent(filtros);
        }

        if (url.indexOf('?') != -1) {
            //filtros = url.substring(url.indexOf('?'));
            url = url.substring(0, url.indexOf('?'));
        }

        if (url.substring(url.length - 1) == '/') {
            url = url.substring(0, (url.length - 1));
        }

        //Quito los dos ultimos trozos:
        url = url.substring(0, url.lastIndexOf('/'));
        url = url.substring(0, url.lastIndexOf('/'));

        if (filtroDePag.indexOf('skos:ConceptID') != -1) {
            var busAvazConCat = false;

            if (typeof (textoCategoria) != 'undefined') {
                //busAvazConCat = (url.indexOf('/' + textoCategoria) == (url.length - textoCategoria.length - 1));
                if (url.indexOf(textoComunidad + '/') != -1) {
                    var trozosUrl = url.substring(url.indexOf(textoComunidad + '/')).split('/');
                    busAvazConCat = (trozosUrl[2] == textoCategoria);
                }
            }

            url = url.substring(0, url.lastIndexOf('/'));

            if (busAvazConCat) {
                url += '/' + textoBusqAvaz;
            }
        }


        MostrarUpdateProgress();

        document.location = url + filtros;
        return;
    }
    else if (!contieneFiltro) {
		if (eliminarFiltroAnterior)
		{
			filtros='';
			var filtroEliminar=faceta.substring(0,faceta.indexOf('=')+1);			
			for (var i = 0; i < filtrosArray.length; i++) {				
				if (filtrosArray[i].indexOf(filtroEliminar)==-1)  {
					filtros += filtrosArray[i] + '&';
				}
			}
		}	
				
		//Si no existe el filtro, lo a?adimos
		if (filtros.length > 0) { filtros += '&'; }
		filtros += faceta;

		if (typeof searchAnalitics != 'undefined') {
			searchAnalitics.facetsSearchAdd(faceta);
		}
    }
    else {
        filtros = '';

        for (var i = 0; i < filtrosArray.length; i++) {
			var filtroEliminar="";
			if (eliminarFiltroAnterior)
			{
				filtroEliminar=faceta.substring(0,faceta.indexOf('=')+1);
			}
            if (filtrosArray[i] != '' && filtrosArray[i] != faceta && filtrosArray[i] != facetaDecode && (!eliminarFiltroAnterior || filtrosArray[i].indexOf(filtroEliminar)==-1) ) {
                filtros += filtrosArray[i] + '&';
            }
        }

        if (filtros != '') {
            filtros = filtros.substring(0, filtros.length - 1);
        }

        if (!esFacetaTesSem && typeof searchAnalitics != 'undefined') {
            searchAnalitics.facetsSearchRemove(faceta);
        }
    }

    filtros = filtros.replace(/&&/g, '&');
    if (filtros.indexOf('&') == 0) {
        filtros = filtros.substr(1, filtros.length);
    }
    if (filtros[filtros.length - 1] == '&') {
        filtros = filtros.substr(0, filtros.length - 1);
    }

    filtros = filtros.replace(/\\\'/g, '\'');
    filtros = filtros.replace('|', '%7C');

    history.pushState('', 'New URL: ' + filtros, '?' + filtros);
    FiltrarPorFacetas(ObtenerHash2());
    EscribirUrlForm(filtros);
}