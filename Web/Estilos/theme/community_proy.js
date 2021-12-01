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
}

function CompletadaCargaRecursosComunidad()
{
	comportamientoVerMasVerMenos.init();
	comportamientoVerMasVerMenosTags.init();
	enlazarFacetasBusqueda();
	montarTooltip.init();
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
	
	for (i = 0; i < this.facetasConPopUp.length; i++) {
		var faceta = this.facetasConPopUp[i][0];
		var facetaSinCaracteres = faceta.replace(/\@@@/g, '---').replace(/\:/g, '_');
		var enlaceVerTodos = $('<span><a class="no-close open-popup-link" href="#" data-toggle="modal" faceta="' + i + '" data-target="#modal-resultados">Ver todos</a></span>');
		if (configuracion.idioma == 'en') 
		{
			enlaceVerTodos = $('<span><a class="no-close open-popup-link" href="#" data-toggle="modal" faceta="' + i + '" data-target="#modal-resultados">See all</a></span>');
		}
		if ($('#panFacetas #' + facetaSinCaracteres + ' .moreResults').length > 0) {
			if ($('#panFacetas #' + facetaSinCaracteres + ' .moreResults .open-popup-link ').length == 0) {
				$('#panFacetas #' + facetaSinCaracteres + ' .moreResults').html($(enlaceVerTodos).html());
			} 
		}
	}		
	
	$('#panFacetas .open-popup-link').unbind().click(function(event) 
	{		
		$(".indice-lista.no-letra").html('');
		event.preventDefault();
		$('#modal-resultados .modal-dialog .modal-content .modal-title').text($($(this).closest('.box')).find('.faceta-title').text());
		that.IndiceFacetaActual = parseInt($(this).attr('faceta'));
		$('#modal-resultados').removeClass('modal-categorias');
		that.cargarFaceta();
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
		$('.buscador-coleccion .buscar .texto').val(that.facetasConPopUp[that.IndiceFacetaActual][1]);
	}else
	{
		$('.buscador-coleccion .buscar .texto').val(that.facetasConPopUp[that.IndiceFacetaActual][2]);
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
		if(!$('#modal-resultados').hasClass('modal-categorias'))
		{		
			that.textoActual = that.eliminarAcentos($(this).val());
			that.paginaActual = 1;
			that.buscarFacetas();
		}
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
    init: function (idFooterJQuery, idItemJQuery) {
        this.pagActual = 1;
        this.footer = $(idFooterJQuery);
        this.item = idItemJQuery;
        this.cargarScroll();
        return;
    },
    cargarScroll: function () {
        var that = this;
        that.destroyScroll();
        // opciones del waypoint
        var opts = {
            offset: '100%'
        };
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
			var scopusInt=$(this).attr('scopus');
			var wosInt=$(this).attr('wos');
			var inrecsInt=$(this).attr('inrecs');			
			var html=`<p class="tooltip-title">Fuente de citasX</p>
                <span class="material-icons cerrar">close</span>
                <ul class="no-list-style">
                    <li>
                        <span class="texto">SCOPUS</span>
                        <span class="num-resultado">${scopusInt}</span>
                    </li>
                    <li>
                        <span class="texto">WOS</span>
                        <span class="num-resultado">${wosInt}</span>
                    </li>
                    <li>
                        <span class="texto">INRECS</span>
                        <span class="num-resultado">${inrecsInt}</span>
                    </li>
                </ul>`;
				
			$(this).tooltip({
				html: true,
				placement: 'bottom',
				template: '<div class="tooltip background-blanco citas" role="tooltip"><div class="tooltip-arrow"></div><div class="tooltip-inner"></div></div>',
				title: html
			});
	});
}

/*
var comportamientoFacetasPopUp = {
	numPaginas:2,
	numResultadosPagina:10,
    init: function () {
        this.config();
        this.IndiceFacetaActual = 0;
    },
    config: function () {
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
		['bibo:authorList@@@rdf:member@@@roh:hasKnowledgeArea@@@roh:categoryNode', 'Busca por tópicos', "Search by topics", true]
        ];
		
        for (i = 0; i < this.facetasConPopUp.length; i++) {
            var faceta = this.facetasConPopUp[i][0];
            var facetaSinCaracteres = faceta.replace(/\@@@/g, '---').replace(/\:/g, '_');
            var enlaceVerTodos = $('<span><a class="no-close open-popup-link" href="#" data-toggle="modal" faceta="' + i + '" data-target="#modal-resultados">Ver todos</a></span>');
			if (configuracion.idioma == 'en') 
			{
				enlaceVerTodos = $('<span><a class="no-close open-popup-link" href="#" data-toggle="modal" faceta="' + i + '" data-target="#modal-resultados">See all</a></span>');
			}
            if ($('#panFacetas #' + facetaSinCaracteres + ' .moreResults').length > 0) {
                if ($('#panFacetas #' + facetaSinCaracteres + ' .moreResults .open-popup-link ').length == 0) {
                    $('#panFacetas #' + facetaSinCaracteres + ' .moreResults').html($(enlaceVerTodos).html());
                } 
            }
        }		
		
		$('#panFacetas .open-popup-link').unbind().click(function(event) 
		{		
			$(".indice-lista.no-letra").html('');
			event.preventDefault();
			$('#modal-resultados .modal-dialog .modal-content .modal-title').text($($(this).closest('.box')).find('.faceta-title').text());
			that.IndiceFacetaActual = parseInt($(this).attr('faceta'));
			$('#modal-resultados').removeClass('modal-categorias');
			that.cargarFaceta();
		});			
		
    },
    eliminarAcentos: function (texto) {
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
    },
    cargarFaceta: function () {
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
			$('.buscador-coleccion .buscar .texto').val(that.facetasConPopUp[that.IndiceFacetaActual][1]);
		}else
		{
			$('.buscador-coleccion .buscar .texto').val(that.facetasConPopUp[that.IndiceFacetaActual][2]);
		}
        $('.mfp-content h2').text($('#panFacetas div[faceta=' + facetaSinCaracteres + '] h2 ').text());
        this.textoActual = '';
        $(".indice-lista.no-letra").html('');
		
        var servicio = new WS($('input.inpt_UrlServicioFacetas').val(), WSDataType.jsonp);
        var metodo = 'CargarFacetas';
        var params = {};
        params['pProyectoID'] = $('input.inpt_proyID').val();
        params['pEstaEnProyecto'] = $('input.inpt_bool_estaEnProyecto').val() == 'True';
        params['pEsUsuarioInvitado'] = $('input.inpt_bool_esUsuarioInvitado').val() == 'True';
        params['pIdentidadID'] = $('input.inpt_identidadID').val();
        params['pParametros'] = '' + replaceAll(replaceAll(replaceAll(ObtenerHash2().replace(/&/g, '|').replace('#', ''), '%', '%25'), '#', '%23'), '+', "%2B");
		if(typeof filtroPersonalizado !== 'undefined' && filtroPersonalizado!=null && filtroPersonalizado!='')
		{
			 params['pParametros'] +=filtroPersonalizado;
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
			if(!$('#modal-resultados').hasClass('modal-categorias'))
			{		
				that.textoActual = that.eliminarAcentos($(this).val());
				that.paginaActual = 1;
				that.buscarFacetas();
			}
        });


        servicio.call(metodo, params, function (data) {
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
    },
    buscarFacetas: function () {
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
}*/