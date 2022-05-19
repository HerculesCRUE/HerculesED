$(document).ready(function() {
    metricas.init();
});

var metricas = {
    init: function() {
        this.createEmptyPage('123');
        this.getPage('123');
        return;
    },
    config: function() {
        return;
    },
    getPage: function(pIdPagina) {
        var that = this;
        //var url = "https://localhost:44352/GetPaginaGrafica";
        var url = "https://serviciosedma.gnoss.com/graphicengine/GetPaginaGrafica";
        var arg = {};
        arg.pIdPagina = "123";
        arg.pLang = lang;

        // Petición para obtener los datos de la página.
        $.get(url, arg, function(data) {
            that.fillPage(data);
        });
    },
    getGrafica: function(pIdPagina, pIdGrafica, pFiltroFacetas) {
        //var url = "https://localhost:44352/GetGrafica";
        var url = "https://serviciosedma.gnoss.com/graphicengine/GetGrafica";
        var arg = {};
        arg.pIdPagina = pIdPagina;
        arg.pIdGrafica = pIdGrafica;
        arg.pFiltroFacetas = pFiltroFacetas;
        arg.pLang = lang;

        // Petición para obtener los datos de las gráficas.
        $.get(url, arg, function(data) {
            var ctx = document.getElementById("grafica_" + pIdPagina + "_" + pIdGrafica);
            var myChart = new Chart(ctx, data);
        });
    },
    getFaceta: function(pIdPagina, pIdFaceta, pFiltroFacetas) {
        var that = this;
        //var url = "https://localhost:44352/GetFaceta";
        var url = "https://serviciosedma.gnoss.com/graphicengine/GetFaceta";
        var arg = {};
        arg.pIdPagina = pIdPagina;
        arg.pIdFaceta = pIdFaceta;
        arg.pFiltroFacetas = pFiltroFacetas;
        arg.pLang = lang;

        // Petición para obtener los datos de las gráficas.
        $.get(url, arg, function(data) {

            var numItemsPintados = 0;

            $('div[idfaceta="' + data.id + '"]').append(`
            	<h1>${data.nombre}</h1>
            	`);
            data.items.forEach(function(item, index, array) {

                // Límite de los ítems de las facetas para mostrar.
                if (numItemsPintados == data.numeroItemsFaceta) {
                    return;
                }

                var filtros = decodeURIComponent(ObtenerHash2());
                var filtrosArray = filtros.split('&');
                var contieneFiltro = false;

                for (var i = 0; i < filtrosArray.length; i++) {
                    if (filtrosArray[i] != '') {
                        if (filtrosArray[i] == item.filtro) {
                            contieneFiltro = true;
                        }
                    }
                }

                if (contieneFiltro) {
                    $('div[idfaceta="' + data.id + '"]').append(`
            			<a href="javascript: void(0);" class="filtroMetrica" filtro="${item.filtro}"><strong>${item.nombre} (${item.numero})</strong></a>
            		`);
                } else {
                    $('div[idfaceta="' + data.id + '"]').append(`
            			<a href="javascript: void(0);" class="filtroMetrica" filtro="${item.filtro}">${item.nombre} (${item.numero})</a>
            		`);
                }

                numItemsPintados++;
            });

            that.engancharComportamientos();
        });
    },
    createEmptyPage: function(pIdPagina) {
        $('#containerMetrics').append(`
			<div id="page_${pIdPagina}" class="pageMetrics">
				<div class="containerGraficas">
				</div>
				<div class="containerFacetas">
				</div>
			</div>
		`);
    },
    fillPage: function(pPageData) {
        var that = this;

        // Crear estructura para el apartado de gráficas.

        var rowNumber = 1;
        $('#page_' + pPageData.id + ' .containerGraficas').append(`
			        	<div class="row" id="row_${rowNumber}"></div>
			        `);
        var espacio = 12;

        pPageData.listaConfigGraficas.forEach(function(item, index, array) {
            if (espacio - item.anchura < 0) {
                rowNumber++;
                $('#page_' + pPageData.id + ' .containerGraficas').append(`
			        	<div class="row" id="row_${rowNumber}"></div>
			        `);
                espacio = 12;
            }
            $('#row_' + rowNumber).append(`
                            <div class='grafica col-xl-${item.anchura}' idgrafica='${item.id}'></div>
                    `);
            espacio = espacio - item.anchura;
        });

        // Crear estructura para el apartado de facetas.
        pPageData.listaIdsFacetas.forEach(function(item, index, array) {
            $('#page_' + pPageData.id + ' .containerFacetas').append(`
			        	<div class='faceta' idfaceta='${item}'></div>
			        `);
        });

        that.pintarPagina(pPageData.id)
    },
    pintarPagina: function(pIdPagina) {
        var that = this;

        // Vacias contenedores.
        $('#page_' + pIdPagina + ' .grafica').empty();
        $('#page_' + pIdPagina + ' .faceta').empty();

        // Recorremos el div de las gráficas.
        $('#page_' + pIdPagina + ' .grafica').each(function() {
            $(this).append(`
			        	<canvas id="grafica_${pIdPagina}_${$(this).attr("idgrafica")}" width="600" height="250"></canvas>
			        `);
            that.getGrafica(pIdPagina, $(this).attr("idgrafica"), ObtenerHash2());
        });

        // Recorremos el div de las facetas.
        $('#page_' + pIdPagina + ' .faceta').each(function() {
            that.getFaceta(pIdPagina, $(this).attr("idfaceta"), ObtenerHash2());
        });
    },
    engancharComportamientos: function() {
        var that = this;

        $('.containerFacetas a.filtroMetrica')
            .unbind()
            .click(function(e) {
                var filtroActual = $(this).attr('filtro');
                var filtros = decodeURIComponent(ObtenerHash2());
                var filtrosArray = filtros.split('&');
                filtros = '';
                var contieneFiltro = false;
                for (var i = 0; i < filtrosArray.length; i++) {
                    if (filtrosArray[i] != '') {
                        if (filtrosArray[i] == filtroActual) {
                            contieneFiltro = true;
                        } else {
                            filtros += filtrosArray[i] + '&';
                        }

                    }
                }
                if (!contieneFiltro) {
                    filtros += filtroActual;
                }

                history.pushState('', 'New URL: ' + filtros, '?' + filtros);
                e.preventDefault();
                that.pintarPagina($(this).closest('.pageMetrics').attr('id').substring(5));
            });

    }
}