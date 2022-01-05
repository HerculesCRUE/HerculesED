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
		MontarResultadosScroll.init('footer', 'article', callback());
		MontarResultadosScroll.CargarResultadosScroll = function (data) {
			var htmlRespuesta = document.createElement("div");
			htmlRespuesta.innerHTML = data;
			$(htmlRespuesta).find('article').each(function () {
				$('#panResultados article').last().after(this);
			});
			console.log("loaded $(htmlRespuesta).find('article')")
			comportamientoVerMasVerMenosTags.init();
			console.log("loaded comportamientoVerMasVerMenosTags")
			enlazarFacetasBusqueda();
			console.log("enlazarFacetasBusqueda")
			if (callback && typeof(callback) === "function") {
				callback();
			}
		}
		return;
	},
	config: function (callback = () => {}) {
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
										<!-- Número de resultados -->
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
		console.log("config llamado")
		callback();
	}
}

function PintarGraficaPublicaciones(data,idContenedor) {	
	$('#'+idContenedor+'_aux').remove();
	$('#'+idContenedor).append($('<canvas id="'+idContenedor+'_aux" class="js-chart"></canvas>'));
	var ctx = document.getElementById(idContenedor+'_aux');
	data.options={
		scales: {
		  y1: {
			type: 'linear',
			display: true,
			position: 'left',
			title: {
              text: "Publicaciones",
              display: true,
              color: "red",
            },
		  },
		  y2: {
			type: 'linear',
			display: true,
			position: 'right',
			title: {
              text: "Citas",
              display: true,
              color: "red",
            },
		  },
		},		
		scale:{
			ticks:{
				precision:0
			}
		},
		maintainAspectRatio: false
	}
	var parent = ctx.parentElement;
	var height = parent.offsetHeight;
	ctx.setAttribute('height', 400);
	var myChart = new Chart(ctx, data);
}

function PintarGraficaProyectos(data,idContenedorAnios,idContenedorMiembros,idContenedorAmbito) {	
	$('#'+idContenedorAnios).empty();
	$('#'+idContenedorAnios).parent().css("height", 450);
	$('#'+idContenedorMiembros).empty();
	$('#'+idContenedorMiembros).parent().css("height", 166);
	$('#'+idContenedorAmbito).empty();
	$('#'+idContenedorAmbito).parent().css("height", 234);	
	
	//Gráfico de barras años
	var ctxBarrasAnios = document.getElementById(idContenedorAnios);
	data.graficaBarrasAnios.options={
		scale:{
			ticks:{
				precision:0
			}
		},
		maintainAspectRatio: false
	}
	var myChartBarrasAnios = new Chart(ctxBarrasAnios, data.graficaBarrasAnios);
	
	
	//Gráfico de barras miembros
	var ctxBarrasMiembros = document.getElementById(idContenedorMiembros);
	data.graficaBarrasMiembros.options={
		scale:{
			ticks:{
				precision:0
			}
		},
		plugins: { legend: { display: false } },
		maintainAspectRatio: false
	}


	var items = [];
	data.graficaBarrasMiembros.data.datasets.forEach(e => {
	  items = [...e.data];
	});
	var maxData = Math.max(...items);

	data.graficaBarrasMiembros.options.scale.suggestedMax = maxData + 1;



	var myChartBarrasMiembros = new Chart(ctxBarrasMiembros, data.graficaBarrasMiembros);
	
	
	//Gráfico de sectores ambito
	var ctxBarrasAmbito = document.getElementById(idContenedorAmbito);
	data.graficaSectoresAmbito.options={
		scale:{
			ticks:{
				precision:0
			}
		},
		plugins: { legend: { display: false } },
		maintainAspectRatio: false
	}


	items = [];
	data.graficaSectoresAmbito.data.datasets.forEach(e => {
	  items = [...e.data];
	});
	maxData = Math.max(...items);

	data.graficaSectoresAmbito.options.scale.suggestedMax = maxData + 1;

	var myBarrasAmbito = new Chart(ctxBarrasAmbito, data.graficaSectoresAmbito);
}

function PintarGraficaArania(data,idContenedor){
	$('#'+idContenedor).empty();
	var cy = window.cy = cytoscape({
		// Contenedor
		container: document.getElementById(idContenedor),
		// Layout
		layout: {
			name: 'cose',
			idealEdgeLength: 100,
			nodeOverlap: 20,
			refresh: 20,
			fit:true,
			padding: 30,
			randomize: false,
			componentSpacing: 100,
			nodeRepulsion: 400000,
			edgeElasticity: 100,
			nestingFactor: 5,
			gravity: 80,
			numIter: 1000,
			initialTemp: 200,
			coolingFactor: 0.95,
			minTemp: 1.0

		},
		// Estilos
		style: [{
			"selector": "node",
			"style": {
				"width": "mapData(score, 0, 25, 30, 70)",
				"height": "mapData(score, 0, 25, 30, 70)",
				"content": "data(name)",
				"font-size": "12px",
				"font-family": 'Roboto',
				"font-color": "#999999",
				"background-color": "#c2c2c2",
				"text-outline-width": "0px",
				"overlay-padding": "6px",
				"z-index": "10"
			}
		}, {
			"selector": "edge",
			"style": {
				"curve-style": "haystack",
				"content": "data(name)",
				"font-size": "24px",
				"font-family": 'Roboto',
				"font-color": "#999999",
				"background-color": "#c2c2c2",
				"haystack-radius": "0.5",
				"opacity": "0.5",
				"line-color": "#E1E1E1",
				"width": "mapData(weight, 0, 10, 0, 10)",
				"overlay-padding": "1px",
				"z-index": "11"
			}
		}],
		// Datos
		elements: data
	});

	var arrayNodes = [];
	var nodos = cy.nodes();

	for (i = 0; i < cy.nodes().length; i++) { //starts loop
		arrayNodes.push(nodos[i]._private.data.name);                
		switch (nodos[i]._private.data.type) {
			case 'icon_ip':
				cy.nodes()[i].style({
					'background-color': 'white',
					'background-image': 'https://cdn.iconscout.com/icon/free/png-256/user-1912184-1617653.png',
					'background-fit': 'cover',
					'border-width': '2px',
					'border-color': 'rgb(0,0,0)',
					'shape': 'circle'
				})                        
				break;
			case 'icon_member':
				cy.nodes()[i].style({
					'background-color': 'white',
					'background-image': 'https://cdn.iconscout.com/icon/free/png-256/user-1648810-1401302.png',
					'background-fit': 'cover',
					'border-width': '2px',
					'border-color': 'rgb(4,184,209)',
					'shape': 'circle'
				})
				break;
			default:
				nodos[i].style({
					'border-width': '2px',
					'border-color': 'black',
					'shape': 'circle'
				});
				break;
		}
	};

	var arrayEdges = [];
	var edges = cy.edges();

	for (i = 0; i < cy.edges().length; i++) { //starts loop
		var data=edges[i]._private.data.id.split('~');	
		arrayEdges.push(data[data.length-1]);
		edges[i]._private.data.name = "";
		switch (edges[i]._private.data.type) {
			case 'relation_document':
				edges[i].style({
					"line-color": "#FF0000"
				})
				break;
			case 'relation_project':
				edges[i].style({
					"line-color": "#0000FF"
				})
				break;
			default:
				edges[i].style({
					"line-color": "#E1E1E1"
				})
				break;
		}
	};

	cy.on('click', 'node', function (e) {
		e = e.target;
		var indice = cy.nodes().indexOf(e);
		if (e._private.data.name === "") {
			e._private.data.name = arrayNodes[indice];
		}
		else {
			e._private.data.name = "";
		}
	})

	cy.on('click', 'edge', function (e) {
		e = e.target;
		var indice = cy.edges().indexOf(e);
		if (e._private.data.name === "") {
			e._private.data.name = arrayEdges[indice];
		}
		else {
			e._private.data.name = "";
		}
	})
}

function PintarGraficaAreasTematicas(data,idContenedor) {	
	$('#'+idContenedor).empty();
	// Porcentajes en parte inferior.
	data.options.scales.x.ticks.callback = function (value) { return value + "%" }
	var altura = data.data.labels.length * 50;
	if(altura==0)
	{
		altura=50;
	}
	$('#'+idContenedor).removeAttr("style");
	$('#'+idContenedor).css("height", altura + 50);
	$('#'+idContenedor).append($(`<canvas id="${idContenedor}_aux" class="js-chart" width="600" height="' + altura + '"></canvas>`));
	var ctx = document.getElementById(idContenedor+'_aux');
	var parent = ctx.parentElement;
	var width = parent.offsetWidth;
	ctx.setAttribute('width', width);
	var height = parent.offsetHeight;
	ctx.setAttribute('height', height);
	var myChart = new Chart(ctx, data);
}


//Sobreescribimos FiltrarPorFacetas para que coja el filtro por defecto (y el orden)
function FiltrarPorFacetas(filtro, callback = () => {}) {
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
	comportamientoRangosFechas();
	comportamientoRangosNumeros();
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