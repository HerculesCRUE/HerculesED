$(document).ready(function () {
    metricas.init();
});
// Año máximo y mínimo para las facetas de años
var minYear;
var maxYear;
// Lista de páginas
// ID de la página actual.
var idPaginaActual = "";

// Lista de páginas.
var listaPaginas;

var metricas = {
    init: function () {
        this.getPages();
        return;
    },
    config: function () {
        return;
    },
    getPages: function () {
        var that = this;
        var url = url_servicio_graphicengine + "GetPaginasGraficas"; //"https://localhost:44352/GetPaginaGrafica"
        var arg = {};
        arg.pLang = lang;

        // Petición para obtener los datos de la página.
        $.get(url, arg, function (listaData) {
            for (let i = 0; i < listaData.length; i++) {
                $(".listadoMenuPaginas").append(`
                    <li id="${listaData[i].id}" num="${i}">${listaData[i].nombre}</li>
                `);
            }
            that.createEmptyPage(listaData[0].id);
            that.fillPage(listaData[0]);
            listaPaginas = listaData;
        });
    },
    getGrafica: function (pIdPagina, pIdGrafica, pFiltroFacetas, ctx = null) {
        var that = this;
        var url = url_servicio_graphicengine + "GetGrafica"; //"https://localhost:44352/GetGrafica"
        var arg = {};
        arg.pIdPagina = pIdPagina;
        arg.pIdGrafica = pIdGrafica;
        arg.pFiltroFacetas = pFiltroFacetas;
        arg.pLang = lang;

        // Petición para obtener los datos de las gráficas.
        $.get(url, arg, function (data) {
            if (!ctx) {
                ctx = document.getElementById("grafica_" + pIdPagina + "_" + pIdGrafica);
            }
            // Controla si el objeto es de ChartJS o Cytoscape.
            var combo = $(ctx).parents("article").find("select");
            var graficaContenedor = $(ctx).parent();
            if ("container" in data) {
                data.container = ctx;

                data.ready = function () { window.cy = this };
                var cy = window.cy = cytoscape(data);
                //var combo = $(ctx).parents("article").find("select");
                if (combo) {
                    combo.append(`
                        <option value="${"grafica_" + pIdPagina + "_" + pIdGrafica}">${data.title}</options>
                    `)
                }

                $(`#titulo_grafica_${pIdPagina}_${pIdGrafica}`).empty().append(data.title);

                var arrayNodes = [];
                var nodos = cy.nodes();
                for (i = 0; i < cy.nodes().length; i++) {
                    arrayNodes.push(nodos[i]._private.data.name);
                };

                var arrayEdges = [];
                var edges = cy.edges();
                for (i = 0; i < cy.edges().length; i++) {
                    var data = edges[i]._private.data.id.split('~');
                    arrayEdges.push(data[data.length - 1]);
                    edges[i]._private.data.name = "";
                };

                cy.on('click', 'node', function (e) {
                    e = e.target;
                    var indice = cy.nodes().indexOf(e);
                    if (e._private.data.name === "") {
                        e._private.data.name = arrayNodes[indice];
                    } else {
                        e._private.data.name = "";
                    }
                })

                cy.on('click', 'edge', function (e) {
                    e = e.target;
                    var indice = cy.edges().indexOf(e);
                    if (e._private.data.name === "") {
                        e._private.data.name = arrayEdges[indice];
                    } else {
                        e._private.data.name = "";
                    }
                });

                $('li#zoomIn')
                    .unbind()
                    .click(function (e) {
                        cy.zoom(cy.zoom() + 0.2);
                    });

                $('li#zoomOut')
                    .unbind()
                    .click(function (e) {
                        cy.zoom(cy.zoom() - 0.2);
                    });

            } else {
                if (combo) {
                    var option = $(combo).find("option[value='grafica_" + pIdPagina + "_" + pIdGrafica + "']");
                    if (option.length === 0) {
                        combo.append(`
                        <option value="${"grafica_" + pIdPagina + "_" + pIdGrafica}">${data.options.plugins.title.text}</options>
                    `)
                    }
                }
                // Plugin para color de fondo, le pongo el color blanco.
                var plugin = {
                    id: 'custom_canvas_background_color',
                    beforeDraw: (chart) => {
                        chart.ctx.save();
                        chart.ctx.globalCompositeOperation = 'destination-over';
                        chart.ctx.fillStyle = '#FFFFFF';
                        chart.ctx.fillRect(0, 0, chart.width, chart.height);
                        chart.ctx.restore();
                    }
                };
                data.plugins = [plugin];

                if (pIdGrafica.indexOf("circular") == -1) { //si no es circular
                    that.drawChart(ctx, data, pIdGrafica);
                } else {
                    var myChart = new Chart(ctx, data);
                }
            }

            that.engancharComportamientos();
        });
    },
    getFaceta: function (pIdPagina, pIdFaceta, pFiltroFacetas) {
        var that = this;
        var url = url_servicio_graphicengine + "GetFaceta"; //"https://localhost:44352/GetFaceta"
        var arg = {};

        arg.pIdPagina = pIdPagina;
        arg.pIdFaceta = pIdFaceta;
        arg.pFiltroFacetas = pFiltroFacetas;
        arg.pLang = lang;

        // Año máximo y mínimo para las facetas de años.
        minYear = 10000;
        maxYear = 0;

        // Petición para obtener los datos de las gráficas.
        $.get(url, arg, function (data) {

            var numItemsPintados = 0;
            if (data.isDate) {
                $('div[idfaceta="' + data.id + '"]').append(`
                <span class="faceta-title">${data.nombre}</span>
                <span class="facet-arrow"></span>
                <div class="faceta-date-range">
                <div id="gmd_ci_daterange" class="ui-slider ui-corner-all ui-slider-horizontal ui-widget ui-widget-content">
                    <div class="ui-slider-range ui-corner-all ui-widget-header"></div>
                        <span tabindex="0" class="ui-slider-handle ui-corner-all ui-state-default" style="left: 0%;"></span>
                        <span tabindex="1" class="ui-slider-handle ui-corner-all ui-state-default" style="left: 100%;"></span>
                    </div>
                <div class="d-flex" id="inputs_rango"></div>
                    <a name="gmd_ci:date" class="searchButton">Aplicar</a>
                    <ul class="no-list-style">
                        <li>
                            <a href="javascript: void(0);" id="fiveyears">Últimos cinco años</a>
                        </li>
                        <li>
                            <a href="javascript: void(0);" id="lastyear">Último año</a>
                        </li>
                        <li>
                            <a href="javascript: void(0);" id="allyears">Todos</a>
                        </li>
                    </ul>
                </div>
                `);
            } else if (data.tesauro) {
                $('div[idfaceta="' + data.id + '"]').append(`
                    <span class="faceta-title">${data.nombre}</span>
                    <ul class="listadoFacetas">
                        ${metricas.pintarTesauro(data.items)}
                    </ul>
                    <p class="moreResults"><a class="no-close open-popup-link open-popup-link-tesauro" href="#" data-toggle="modal" faceta="0" data-target="#modal-tesauro">Ver todos</a></p>
                `);
            } else {
                $('div[idfaceta="' + data.id + '"]').append(`
                    <span class="faceta-title">${data.nombre}</span>
                    <span class="facet-arrow"></span><ul class="listadoFacetas"></ul>
                `);
            }

            data.items.forEach(function (item, index, array) {
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

                if (data.isDate) {
                    minYear = Math.min(minYear, item.nombre);
                    maxYear = Math.max(maxYear, item.nombre);
                } else if (!data.tesauro) {
                    $('div[idfaceta="' + data.id + '"] .listadoFacetas').append(`
                        <li>
                            <a href="javascript: void(0);" class="faceta filtroMetrica" filtro="${item.filtro}">
                                <span class="textoFaceta">${item.nombre}</span>
                                <span class="num-resultados">(${item.numero})</span>
                            </a>
                        </li>
                    `);
                    if (filtros.includes(item.filtro)) {
                        // Negrita
                        $('li').find(`[filtro='${item.filtro}']`).addClass("applied");
                    }
                } else {
                    // Negrita del tesauro.
                    for (var i = 0; i < filtrosArray.length; i++) {
                        $(`a[filtro="${filtrosArray[i]}"]`).addClass("applied");
                    }
                }
                numItemsPintados++;
            });
            if (data.isDate) {
                $('div[idfaceta="' + data.id + '"] #inputs_rango').append(`
                    <input title="Año" type="number" min="${minYear}" max="${maxYear}" autocomplete="off" class="filtroFacetaFecha hasDatepicker minVal" placeholder="${minYear}" value="${minYear}" name="gmd_ci_datef1" id="gmd_ci_datef1">
                    <input title="Año" type="number" min="${minYear}" max="${maxYear}" autocomplete="off" class="filtroFacetaFecha hasDatepicker maxVal" placeholder="${maxYear}" value="${maxYear}" name="gmd_ci_datef2" id="gmd_ci_datef2">
                `)
            }
            that.corregirFiltros();
            that.engancharComportamientos();
        });
    },
    clearPage: function () {
        $('#panFacetas').empty()
        $('.resource-list-wrap').empty();
        $('.borrarFiltros').click();
    },
    createEmptyPage: function (pIdPagina) {
        $('.containerPage').attr('id', 'page_' + pIdPagina);
        $('.containerPage').addClass('pageMetrics');
        $('#panFacetas').attr('idfaceta', 'page_' + pIdPagina);
        $('#panFacetas').addClass('containerFacetas');

        $('main').find('.modal-backdrop').remove();
        $('main').append(`
        <div class="modal-backdrop fade" style="pointer-events: none;"></div>
        `);

        /*$('#containerMetrics').append(`
            <div id="page_${pIdPagina}" class="pageMetrics">
                <div class="containerGraficas">
                </div>
                <div class="containerFacetas">
                </div>
            </div>
        `);*/
    },
    fillPage: function (pPageData) {
        idPaginaActual = pPageData.id;
        var that = this;

        // Crear estructura para el apartado de gráficas.

        var rowNumber = 0;
        var espacio = 12;

        /*if (espacio - item.anchura < 0 || index == 0) {
            rowNumber++;
            $('#page_' + pPageData.id + ' .containerGraficas').append(`
                    <div class="row" id="row_${rowNumber}"></div>
                `);
            espacio = 12;
        }
        $('#row_' + rowNumber).append(`
                        <div class='grafica col-xl-${item.anchura}' idgrafica='${item.id}'></div>
                `);
        espacio = espacio - item.anchura;*/

        var tmp = [];
        var id = "";
        var gruposDeIDs = [];
        var lista = pPageData.listaConfigGraficas.slice();
        while (lista.length > 0) {
            tmp = [];
            var grafica = lista.shift();
            id = grafica.idGrupo;

            if (id == null) { // Si el ID es nulo, la mete en un grupo nuevo.
                tmp.push(grafica);
            } else {
                tmp.push(grafica);
                for (let i = 0; i < lista.length; i++) {
                    if (lista[i].idGrupo == id) {
                        tmp.push(lista[i]);
                        lista.splice(i, 1);
                        i--;
                    }
                }
            }
            gruposDeIDs.push(tmp);
        }


        gruposDeIDs.forEach(function (item, index, array) {
            var graficasGrupo;
            var tmp = '';
            item.forEach(function (grafica, index, array) {

                tmp += `<div style="display:${index === 0 ? "flex" : "none"}; margin-top:20px; flex-direction:column;height:100%;width:100%" class="${index == 0 ? "show" : "hide"} grafica" idgrafica='${grafica.id}'></div>`;
            });
            graficasGrupo = tmp;

            $('#page_' + pPageData.id + '.containerPage').find('.resource-list-wrap').append(`
                <article class="resource span${item[0].anchura}"> 
                    <div class="wrap" >
                        <div class="acciones-mapa" ${item.length != 1 ? `style="display:flex;justify-content: space-between;width: 100%;padding-left: 15px;padding-right: 15px;right:0px` : ""}">
                            ${item.length != 1 ? `
                            <select class="chartMenu js-select2" href="javascript: void(0);" style="height:24px"></select>`: ""}
                            <div class="wrap" style="z-index:1">
                                <div class="zoom">
                                    <a href="javascript: void(0);" style="height:24px"  data-toggle="modal">
                                        <span class="material-icons">zoom_in</span>
                                    </a>
                                </div>
                                <div class="dropdown show">
                                    <a href="javascript: void(0);" style="height:24px" id="dropdownMasOpciones" data-toggle="dropdown">
                                        <span class="material-icons">more_vert</span>
                                    </a>
                                    <div class="dropdown-menu basic-dropdown dropdown-icons dropdown-menu-right" aria-labelledby="dropdownMasOpciones">
                                            <p class="dropdown-title">Acciones</p>
                                            <ul class="no-list-style">
                                                <li>
                                                    <a class="item-dropdown guardar">
                                                        <span class="material-icons">assessment</span>
                                                        <span class="texto">Guardar en espacio personal</span>
                                                    </a>
                                                </li>
                                                <li>
                                                    <a class="item-dropdown csv">
                                                        <span class="material-icons">insert_drive_file</span>
                                                        <span class="texto">Descargar como .csv</span>
                                                    </a>
                                                </li>
                                                <li>
                                                    <a class="item-dropdown descargar">
                                                        <span class="material-icons">download</span>
                                                        <span class="texto">Descargar como imagen .jpg</span>
                                                    </a>
                                                </li>
                                            </ul>
                                    </div>
                                </div>
                            </div>
                        </div>                      
                        ${graficasGrupo}
                        </div>
                </article>

            `);
        });


        // Crear estructura para el apartado de facetas.
        pPageData.listaIdsFacetas.forEach(function (item, index, array) {
            $('#page_' + pPageData.id + ' .containerFacetas').append(`
                    <div class='facetedSearch'>
                        <div class='box' idfaceta='${item}'></div>
                        </div>
                    `);
        });

        that.pintarPagina(pPageData.id)
    },
    pintarPagina: function (pIdPagina) {
        var that = this;

        // Borra la clase modal-open del body cuando se abre el pop-up del tesáuro. 
        // TODO: Mirar porque no lo hace automáticamente.
        $("body").removeClass("modal-open");
        // Vacias contenedores.
        $('#page_' + pIdPagina + ' .grafica').empty();
        $('#page_' + pIdPagina + ' .box').empty();

        // Recorremos el div de las gráficas.
        $('#page_' + pIdPagina + ' .grafica').each(function () {
            if ($(this).attr("idgrafica").includes("nodes")) {
                $(this).append(`
                        <p id="titulo_grafica_${pIdPagina}_${$(this).attr("idgrafica")}" style="text-align:center; width: 100%; font-weight: bold; color: #6F6F6F; font-size: 0.90em;"></p>
                        <div class="graph-controls" style="position: absolute; top: 24px; left: 20px; z-index: 200;">
                            <ul class="no-list-style align-items-center" style="display: flex; flex-direction: column;align-items:center">
                                <li class="control zoomin-control" id="zoomIn">
                                    <span class="material-icons">add</span>
                                </li>
                                <li class="control zoomout-control" style="margin-top:5px" id="zoomOut">
                                    <span class="material-icons" >remove</span>
                                </li>
                            </ul>
                        </div>
                        <div id="grafica_${pIdPagina}_${$(this).attr("idgrafica")}" style="width: 100%; height: 500px; -webkit-tap-highlight-color: rgba(0, 0, 0, 0);"></div>
                    `);
            } else if (!$(this).attr("idgrafica").includes("circular")) {
                $(this).append(`
                <div class="chartWrapper" style="position:relative; margin-top:15px ;width:100%">
                    <div class="chartScroll" style="overflow-${$(this).attr("idgrafica").includes("isHorizontal") ? "y" : "x"}: scroll;height:546px;">
                        <div style="height: 00px;" class="chartAreaWrapper">
                            <canvas width = "600" height = "250" id="grafica_${pIdPagina}_${$(this).attr("idgrafica")}"></canvas>
                        </div>
                    </div>
                </div>
                `);
            } else {
                $(this).css("height", "auto");
                $(this).append(`
                    <canvas id = "grafica_${pIdPagina}_${$(this).attr("idgrafica")}" width = "600" height = "250" ></canvas>
                        `);
            }
            that.getGrafica(pIdPagina, $(this).attr("idgrafica"), ObtenerHash2());
        });

        // Recorremos el div de las facetas.
        $('#page_' + pIdPagina + ' .box').each(function () {
            that.getFaceta(pIdPagina, $(this).attr("idfaceta"), ObtenerHash2());
        });

        // Etiquetas
        $("#panListadoFiltros").children().remove();
        var filtros = decodeURIComponent(ObtenerHash2());
        var filtrosArray = filtros.split('&');
        for (let i = 0; i < filtrosArray.length; i++) {
            let filtro = filtrosArray[i];
            let nombre;
            if (filtro === "" || !filtro) {
                continue;
            }
            if (filtro.split('=')[1].includes('@')) {
                nombre = filtro.split('=')[1].split('@')[0].replaceAll("'", "");
                $(".borrarFiltros-wrap").remove();
                $("#panListadoFiltros").append(`
                <li class="Categoria" filtro="${filtro}">
                    <span>${nombre}</span>
                    <a rel="nofollow" class="remove faceta" name="search=Categoria" href="javascript:;">eliminar</a>
                </li>
                <li class="borrarFiltros-wrap">
                    <a class="borrarFiltros" href="javascript:;">Borrar</a>
                </li>
                `);
            } else if (filtro.split('=')[1].includes('http://')) {
                // Agregado la clase oculto para procesarlo después de que se carguen las facetas.
                $(".borrarFiltros-wrap").remove();
                $("#panListadoFiltros").append(`
                <li class="Categoria oculto" filtro="${filtro}">
                    <span>${filtro}</span>
                    <a rel="nofollow" class="remove faceta" name="search=Categoria" href="javascript:;">eliminar</a>
                </li>
                <li class="borrarFiltros-wrap">
                    <a class="borrarFiltros" href="javascript:;">Borrar</a>
                </li>
                `);
            } else {
                nombre = filtro.split('=')[1].replaceAll("'", "");
                if (nombre === "lastyear") {
                    nombre = GetText("ULTIMO_ANIO");
                } else if (nombre === "fiveyears") {
                    nombre = GetText("ULTIMOS_CINCO_ANIOS");
                }
                $(".borrarFiltros-wrap").remove();
                $("#panListadoFiltros").append(`
                <li class="Categoria" filtro="${filtro}">
                    <span>${nombre}</span>
                    <a rel="nofollow" class="remove faceta" name="search=Categoria" href="javascript:;">eliminar</a>
                </li>
                <li class="borrarFiltros-wrap">
                    <a class="borrarFiltros" href="javascript:;">Borrar</a>
                </li>
                `);
            }
        }
        function GetText(id, param1, param2, param3, param4) {
            if ($('#' + id).length) {
                var txt = $('#' + id).val();
                if (param1 != null) {
                    txt = txt.replace("PARAM1", param1);
                }
                if (param2 != null) {
                    txt = txt.replace("PARAM2", param1);
                }
                if (param3 != null) {
                    txt = txt.replace("PARAM3", param1);
                }
                if (param4 != null) {
                    txt = txt.replace("PARAM4", param1);
                }
                return txt;
            } else {
                return id;
            }
        }
    },
    corregirFiltros: function () {
        // Permite pintar el filtro del tesauro con el nombre del nivel correspondiente.
        $("#panListadoFiltros").each(function () {
            $("#panListadoFiltros").find('li').each(function () {
                if ($(this).hasClass("oculto")) {
                    var valor = $(this).find('span').text();
                    var nombre = $(`a[filtro="${valor}"]`).attr("title");
                    if (nombre != null) {
                        $(this).find('span').text(nombre);
                        $(this).removeClass('oculto');
                    }
                }
            });
        });
    },
    drawChart: function (ctx, data, pIdGrafica = null, barSize = 100) {
        if (Chart.getChart(ctx) != null) {
            return;
        }
        var myChart = new Chart(ctx, data);


        var numBars = data.data.labels.length; // Número de barras.
        var canvasSize = (numBars * barSize); // Tamaño del canvas.
        var canvas = myChart.canvas;
        var chartAreaWrapper = canvas.parentNode;
        var scrollContainer = chartAreaWrapper.parentNode;
        var chartContainer = scrollContainer.parentNode;
        // En caso de que los datos de la gráfica se representen con porcentajes
        if (pIdGrafica != null && pIdGrafica.includes("prc")) {
            data.options.plugins.tooltip = {
                callbacks: {
                    afterLabel: function (context) {
                        let label = "Porcentaje: ";
                        let sum = context.dataset.data.reduce((a, b) => a + b, 0);
                        let porcentaje = context.dataset.data[context.dataIndex] * 100 / sum;
                        label += porcentaje.toFixed(2) + '%';
                        return label;
                    }
                }
            }
        }

        // Solo si es una gráfica horizontal.
        if (data.options.indexAxis == "y") {
            // En caso de que los labels de la gráfica deban de estar abreviados...
            data.options.maintainAspectRatio = false;
            data.options.responsive = true;
            if (pIdGrafica != null && pIdGrafica.includes("abr")) {
                // Se modifica la propiedad que usa Chart.js para obtener los labels de la gráfica.
                data.options.scales.y.ticks.callback = function (value) {
                    const labels = data.data.labels; // Obtención de los labels.
                    if (value >= 0 && value < labels.length) {
                        if (labels[value].length >= 7) {
                            return labels[value].substring(0, 7) + "..."; // Se muestran solo los 7 primeros caractéres.
                        }
                        return labels[value];
                    }
                    return value;
                }
            }

            // Obtenemos los elementos de la gráfica.

            //scrollContainer.style.height = 550 + "px";
            // Si el canvas no supera el tamaño del contenedor, no se hace scroll.
            if (canvasSize < 550) { //TODO cambiar 550 por el tamaño del contenedor.
                chartAreaWrapper.style.height = myChart.height + "px";
                scrollContainer.style.height = "auto";
                scrollContainer.parentNode.style.height = "auto";
                scrollContainer.style.overflowY = 'hidden';
                scrollContainer.parentNode.parentNode.style.justifyContent = 'center';
            } else {
                // De lo contrario se prepara todo para el scroll.
                // Importante que no mantega el ratio para poder reescalarlo.


                var hasTopAxis = false;
                var hasBottomAxis = false;


                // Se comprueba si tiene eje inferior y superior.
                Object.entries(data.options.scales).forEach((scale) => {
                    if (scale[1].axis == "x") {
                        if (scale[1].position == "top" && !hasTopAxis) {
                            hasTopAxis = true;
                        } else if (scale[1].position == "bottom" && !hasBottomAxis) {
                            hasBottomAxis = true;
                        }
                    }
                });

                // leyenda con titulo y contenedor para datasets.
                var legend = $(`<div id="chartLegend" style="text-align: center; position: absolute; top: 0px; background-color: white;">
                <h4 id="legendTitle" style="width="100%" margin: 10px; font-family: Calibri, sans-serif; font-size: 90%; font-weight: bold;">${data.options.plugins.title.text}</h4>
                </div>`);

                $(chartContainer).append(legend);

                var dataSetLabels = $(`<div id="dataSetLabels" style="display: flex; flex-flow: row wrap; justify-content: center;"></div>`)
                $(legend).append(dataSetLabels);


                // Por cada dataset que exista se creara un div con su nombre y color y se añade a dataSetLabels.
                var datasets = data.data.datasets;
                datasets.forEach((dataset, index) => {
                    var labelContainer = $(`<div id="label-${index}" class="labelContainer" style="margin: 5px; height: 15px; display: flex; align-items: center;">
                    <div style="height: 15px; width: 45px; background-color: ${dataset.backgroundColor[0]}; border: 1px solid lightgrey; box-sizing: border-box;"></div>
                    <p class="dataSetLabel" style="font-family: Calibri; margin: 5px;">${dataset.label}</p>
                    </div>`);
                    //labelContainer.appendChild(colorDiv);
                    $(dataSetLabels).append(labelContainer);


                });
                // Eje superior. 
                if (hasTopAxis) {
                    var topAxis = $(`<canvas id="topAxis" class="myChartAxis" style="background: white; position: absolute; bottom: 0px; left: 0px;"></canvas>`);
                    $(legend).append(topAxis);
                }

                // Si existe un eje inferior, se agrega con estilos.
                if (hasBottomAxis) {
                    var bottomAxis = $(`<canvas id="bottomAxis" class="myChartAxis" style="background: white; position: absolute; bottom: 0px; left: 0px;"></canvas>`);
                    $(chartContainer).append(bottomAxis);
                }
                // Cuando se acutaliza el canvas.
                data.options.animation.onProgress = () => this.reDrawChart(myChart, topAxis, bottomAxis, canvasSize, legend);
                // Cuando se reescala el navegador se redibuja la leyenda.
                window.addEventListener('resize', (e) => {
                    this.reDrawChart(myChart, topAxis, bottomAxis, canvasSize, legend);
                    myChart.update();
                });

            }
        } else {


            data.options.maintainAspectRatio = false;
            data.options.responsive = true;
            /*if (pIdGrafica != null && pIdGrafica.includes("abr")) {
                // Se modifica la propiedad que usa Chart.js para obtener los labels de la gráfica.
                data.options.scales.y.ticks.callback = function (value) {
                    const labels = data.data.labels; // Obtención de los labels.
                    if (value >= 0 && value < labels.length) {
                        if (labels[value].length >= 7) {
                            return labels[value].substring(0, 7) + "..."; // Se muestran solo los 7 primeros caractéres.
                        }
                        return labels[value];
                    }
                    return value;
                }
            }*/

            if (pIdGrafica != null && pIdGrafica.includes("abr")) {
                // Se modifica la propiedad que usa Chart.js para obtener los labels de la gráfica.
                data.options.scales.x.ticks.callback = function (value) {
                    const labels = data.data.labels; // Obtención de los labels.
                    if (value >= 0 && value < labels.length) {
                        if (labels[value].length >= 7) {
                            return labels[value].substring(0, 7) + "..."; // Se muestran solo los 7 primeros caractéres.
                        }
                        return labels[value];
                    }
                    return value;
                }
            }

            // Si el canvas no supera el tamaño del contenedor, no se hace scroll.
            if (canvasSize < $(scrollContainer).width()) { //TODO cambiar 550 por el tamaño del contenedor.
                //chartAreaWrapper.style.width = myChart.width-10 + "px";
                scrollContainer.style.overflowX = 'hidden';
                chartAreaWrapper.style.height = "546px";

            }
            else {

                var hasRightAxis = false;
                var hasLeftAxis = false;

                // De lo contrario se prepara todo para el scroll.
                // Importante que no mantega el ratio para poder reescalarlo.


                Object.entries(data.options.scales).forEach((scale) => {
                    if (scale[1].axis == "y") {

                        if (scale[1].position == "left" && !hasTopAxis) {
                            hasLeftAxis = true;
                        } else if (scale[1].position == "right" && !hasBottomAxis) {
                            hasRightAxis = true;
                        }
                    }
                });

                // leyenda con titulo y contenedor para datasets.
                var legend = $(`<div id="chartLegend" style="text-align: center; position: absolute; top: 0px; background-color: white;">
                 <h4 id="legendTitle" style=" margin: 10px; font-family: Calibri, sans-serif; font-size: 90%; font-weight: bold;">${data.options.plugins.title.text}</h4>
                 </div>`);
                $(chartContainer).append(legend);

                var dataSetLabels = $(`<div id="dataSetLabels" style="display: flex; flex-flow: row wrap; justify-content: center;"></div>`)
                $(legend).append(dataSetLabels);


                // Por cada dataset que exista se creara un div con su nombre y color y se añade a dataSetLabels.
                var datasets = data.data.datasets;
                datasets.forEach((dataset, index) => {
                    var labelContainer = $(`<div id="label-${index}" class="labelContainer" style="margin: 5px; height: 15px; display: flex; align-items: center;">
                     <div style="height: 15px; width: 45px; background-color: ${dataset.backgroundColor[0]}; border: 1px solid lightgrey; box-sizing: border-box;"></div>
                     <p class="dataSetLabel" style="font-family: Calibri; margin: 5px;">${dataset.label}</p>
                     </div>`);
                    //labelContainer.appendChild(colorDiv);
                    $(dataSetLabels).append(labelContainer);


                });


                // Eje superior. 
                if (hasLeftAxis) {
                    var topAxis = $(`<canvas id="leftAxis" class="myChartAxis" style="background: white; position: absolute; top:0px; left: 0px;"></canvas>`);
                    $(chartContainer).append(topAxis);
                }

                // Si existe un eje inferior, se agrega con estilos.
                if (hasRightAxis) {
                    var bottomAxis = $(`<canvas id="leftAxis" class="myChartAxis" style="background: white; position: absolute; top: -5px; right: 0px;"></canvas>`);
                    $(chartContainer).append(bottomAxis);
                }
                // Cuando se acutaliza el canvas.
                if (!pIdGrafica.includes("circular")) {
                    data.options.animation.onProgress = () => this.reDrawChart(myChart, topAxis, bottomAxis, canvasSize, legend, true);
                    // Cuando se reescala el navegador se redibuja la leyenda.
                    window.addEventListener('resize', (e) => {
                        this.reDrawChart(myChart, topAxis, bottomAxis, canvasSize, legend, true);
                        myChart.update();
                    });
                }
                /**/
            }
        }

    },
    reDrawChart: function (myChart, mainAxis, secondaryAxis, canvasSize, legend, vertical = false) {
        // Se obtiene la escala del navegador (afecta cuando el usuario hace zoom).
        /*data.options.maintainAspectRatio = false;
        data.options.responsive = true;*/

        var scale = window.devicePixelRatio;


        var copyWidth;
        var copyHeight;
        var axisHeight;
        var axisWidth;

        if (vertical) {

            //myChart.canvas.parentNode.style.width = canvasSize + 'px';
            myChart.canvas.parentNode.style.height = 100 + '%'; //se escala la altura
            myChart.canvas.parentNode.style.width = canvasSize + 'px'; //se escala la anchura respecto al canvas para que ocupe el scroll

            copyWidth = myChart.boxes[2]?.width;
            // Altura del titulo, leyenda y eje superior menos el margen.
            copyHeight = myChart.height - 20;
            targetY = 20;
            // Le asignamos tamaño a la leyenda.
            axisHeight = myChart.height - 10;
        } else {
            myChart.canvas.parentNode.style.height = canvasSize + 'px';
            copyWidth = myChart.width;
            // Altura del titulo, leyenda y eje superior menos el margen.
            copyHeight = myChart.boxes[0].height + myChart.boxes[1].height + myChart.boxes[2]?.height - 5;
            // Le asignamos tamaño a la leyenda.
            axisHeight = myChart.boxes[2]?.height;

        }

        // Preparamos el eje superior.
        $(legend).css("width", vertical ? "100%" : copyWidth + "px");
        $(legend).css("height", vertical ? "auto" : myChart.chartArea.top + "px");

        if ($(legend).height() > myChart.chartArea.top) {
            myChart.canvas.style.marginTop = $(legend).height() - myChart.chartArea.top + "px";
            myChart.canvas.parentNode.parentNode.style.height = "auto";
            if (mainAxis) {
                mainAxis[0].style.marginTop = $(legend).height() - myChart.chartArea.top + "px";
            }
            if (secondaryAxis) {
                secondaryAxis[0].style.marginTop = $(legend).height() - myChart.chartArea.top + "px";
            }

        }
        var targetX = 0;
        var targetY = 0;
        var targetWidth = copyWidth * scale;
        var targetHeight = axisHeight * scale;
        var x = 0;
        var y = 0;
        var width = copyWidth;
        var height = vertical ? copyHeight : axisHeight + 4;
        var ctx;


        if (mainAxis) {
            ctx = mainAxis[0].getContext('2d');
            if (vertical) {
                ctx.canvas.height = copyHeight;
                targetHeight -= 10 * scale;
            } else {
                ctx.canvas.height = axisHeight;
            }
            targetY = (copyHeight - axisHeight + 10) * scale;
            ctx.scale(scale, scale); // Escala del zoom.
            ctx.canvas.width = copyWidth;
            ctx.drawImage(myChart.canvas, targetX, targetY, targetWidth, targetHeight, x, y, width, height);
        }

        // Preparamos el eje inferior.
        if (secondaryAxis) {
            ctx = secondaryAxis[0].getContext('2d');
            if (vertical) {
                ctx.canvas.height = copyHeight;
                targetX = (myChart.width - copyWidth) * scale;
                targetHeight -= 10 * scale;
            } else {
                ctx.canvas.height = axisHeight;
                targetY = myChart.chartArea.bottom * scale;

            }

            ctx.scale(scale, scale); // Escala del zoom.
            ctx.canvas.width = copyWidth;
            ctx.canvas.height = axisHeight;
            ctx.drawImage(myChart.canvas, targetX, targetY, targetWidth, targetHeight, x, y, width, height);
        }

    },
    pintarTesauro: function (pData) {
        var etiqueta = "";
        var hijos = "";

        if (pData.length > 0) {

            pData.forEach(function (item, index, array) {
                hijos += metricas.pintarTesauro(item);
            });

            return hijos;

        } else {

            // Si tiene hijos, los pinta llamando a la propia función recursivamente.
            if (pData.childsTesauro.length > 0) {

                etiqueta += `<ul>`;
                pData.childsTesauro.forEach(function (item, index, array) {
                    hijos += metricas.pintarTesauro(item);
                });
                etiqueta += `${hijos}</ul>`;

                return `<li>
                            <a rel="nofollow" href="javascript: void(0);" class="faceta filtroMetrica con-subfaceta ocultarSubFaceta ocultarSubFaceta" filtro="${pData.filtro}" title="${pData.nombre}">
                                <span class="desplegarSubFaceta"><span class="material-icons">add</span></span>
                                <span class="textoFaceta">${pData.nombre}</span>
                                <span class="num-resultados">(${pData.numero})</span>                          
                            </a>
                            ${etiqueta}
                        </li>`;

            } else {

                return `<li>
                            <a rel="nofollow" href="javascript: void(0);" class="faceta filtroMetrica ocultarSubFaceta ocultarSubFaceta" filtro="${pData.filtro}" title="${pData.nombre}">
                                <span class="textoFaceta">${pData.nombre}</span>
                                <span class="num-resultados">(${pData.numero})</span>                          
                            </a>
                        </li>`;
            }
        }

    },
    engancharComportamientos: function (cyto = null) {
        var that = this;
        var menus = $("select.chartMenu");
        menus.each((index, menu) => { //por cada menu en la pagina
            var selectedID = $(menu).parents("article div.wrap").find("div.grafica:visible").attr("idgrafica"); //Obtiene la id de la grafica visible
            $(menu).val(idPaginaActual + "_" + selectedID); // y la selecciona en el menu
        });
        iniciarSelects2.init(); // Se inicializa la libreria selects2.

        $(".faceta-date-range .ui-slider").slider({
            range: true,
            min: minYear,
            max: maxYear,
            values: [minYear, maxYear],
            slide: function (event, ui) {
                $("#gmd_ci_datef1").val(ui.values[0]);
                $("#gmd_ci_datef2").val(ui.values[1]);
            }
        });

        $(".faceta-date-range").find("input.filtroFacetaFecha").on("input", function (event, ui) {
            var valores = $(".faceta-date-range .ui-slider").slider("option", "values");

            if ($(this).attr("id") === "gmd_ci_datef1") {
                valores[0] = this.value;
            } else {
                valores[1] = this.value;
            }
            $(".faceta-date-range .ui-slider").slider("values", valores);
        });

        $('.containerFacetas a.filtroMetrica,.listadoTesauro a.filtroMetrica')
            .unbind()
            .click(function (e) {
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

                that.pintarPagina(idPaginaActual);
            });

        $('a.remove.faceta')
            .unbind()
            .click(function (e) {
                var filtroActual = $(this).parent().attr('filtro');
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

                that.pintarPagina(idPaginaActual);
            });

        $('.borrarFiltros')
            .unbind()
            .click(function (e) {
                history.pushState('', 'New URL: ', '?');
                e.preventDefault();
                that.pintarPagina(idPaginaActual);
            });


        $('#fiveyears')
            .unbind()
            .click(function (e) {
                var filtro = $(this).parent().parent().parent().parent().attr('idfaceta');
                var filtroActual = `${filtro}=fiveyears`;
                var filtros = decodeURIComponent(ObtenerHash2());
                var filtrosArray = filtros.split('&');
                filtros = '';
                for (var i = 0; i < filtrosArray.length; i++) {
                    if (filtrosArray[i] != '') {
                        filtros += filtrosArray[i] + '&';
                    }
                }
                // Borrar filtros año anteriores
                var reg = new RegExp(filtro + "=[0-9]*-[0-9]*");
                if (filtros.includes(filtro)) {
                    filtros = filtros.replace(reg, "");
                    filtros = filtros.replace(filtro + "=lastyear", "");
                    filtros = filtros.replace(filtro + "=fiveyears", "");
                }
                filtros += filtroActual;

                history.pushState('', 'New URL: ' + filtros, '?' + filtros);
                e.preventDefault();

                that.pintarPagina(idPaginaActual);
            });

        $('#lastyear')
            .unbind()
            .click(function (e) {
                var filtro = $(this).parent().parent().parent().parent().attr('idfaceta');
                var filtroActual = `${filtro}=lastyear`;
                var filtros = decodeURIComponent(ObtenerHash2());
                var filtrosArray = filtros.split('&');
                filtros = '';
                for (var i = 0; i < filtrosArray.length; i++) {
                    if (filtrosArray[i] != '') {
                        filtros += filtrosArray[i] + '&';
                    }
                }
                // Borrar filtros año anteriores
                var reg = new RegExp(filtro + "=[0-9]*-[0-9]*");
                if (filtros.includes(filtro)) {
                    filtros = filtros.replace(reg, "");
                    filtros = filtros.replace(filtro + "=lastyear", "");
                    filtros = filtros.replace(filtro + "=fiveyears", "");
                }
                filtros += filtroActual;

                history.pushState('', 'New URL: ' + filtros, '?' + filtros);
                e.preventDefault();

                that.pintarPagina(idPaginaActual);
            });

        $('#allyears')
            .unbind()
            .click(function (e) {
                var filtro = $(this).parent().parent().parent().parent().attr('idfaceta');
                var filtros = decodeURIComponent(ObtenerHash2());
                var filtrosArray = filtros.split('&');
                filtros = '';
                for (var i = 0; i < filtrosArray.length; i++) {
                    if (filtrosArray[i] != '') {
                        filtros += filtrosArray[i] + '&';
                    }
                }
                // Borrar filtros año anteriores
                var reg = new RegExp(filtro + "=[0-9]*-[0-9]*");
                if (filtros.includes(filtro)) {
                    filtros = filtros.replace(reg, "");
                    filtros = filtros.replace(filtro + "=lastyear", "");
                    filtros = filtros.replace(filtro + "=fiveyears", "");
                }

                history.pushState('', 'New URL: ' + filtros, '?' + filtros);
                e.preventDefault();

                that.pintarPagina(idPaginaActual);
            });

        $('.faceta-date-range a.searchButton')
            .unbind()
            .click(function (e) {
                var min, max;
                // Cojo el valor del input y si no tiene le pongo el placeholder
                $("#gmd_ci_datef1").val() === '' ? min = $("#gmd_ci_datef1").attr("placeholder") : min = $("#gmd_ci_datef1").val();
                $("#gmd_ci_datef2").val() === '' ? max = $("#gmd_ci_datef2").attr("placeholder") : max = $("#gmd_ci_datef2").val();
                var filtro = $(this).parent().parent().attr('idfaceta');
                var filtroActual = `${filtro}=${min}-${max}`;
                var filtros = decodeURIComponent(ObtenerHash2());
                var filtrosArray = filtros.split('&');
                filtros = '';
                for (var i = 0; i < filtrosArray.length; i++) {
                    if (filtrosArray[i] != '') {
                        filtros += filtrosArray[i] + '&';
                    }
                }
                // Borrar filtros año anteriores
                var reg = new RegExp(filtro + "=[0-9]*-[0-9]*");
                if (filtros.includes(filtro)) {
                    filtros = filtros.replace(reg, "");
                    filtros = filtros.replace(filtro + "=lastyear", "");
                    filtros = filtros.replace(filtro + "=fiveyears", "");
                }
                filtros += filtroActual;

                history.pushState('', 'New URL: ' + filtros, '?' + filtros);
                e.preventDefault();

                that.pintarPagina(idPaginaActual);
            });


        // Labels de la leyenda.
        $('div.labelContainer')
            .unbind()
            .click(function (e) {
                // Se obtiene el chart desde el canvas.
                var canvas = $(this).parents('div.chartWrapper').find('div.chartAreaWrapper canvas');
                var chart = Chart.getChart(canvas);

                // El ID del dataset está en el ID del contenedor del label.
                var id = $(this).attr('id').split('-')[1];
                var label = $(this).find('p.dataSetLabel');

                // Si el label no está tachado se tacha y oculta el dataset.
                if (label.css('text-decoration').indexOf("line-through") == -1) {
                    label.css("text-decoration", "line-through");
                    chart.setDatasetVisibility(id, false);
                } else {
                    label.css("text-decoration", "none");
                    chart.setDatasetVisibility(id, true);
                }

                try { // Hay problemas con el gráfico de líneas, si falla se repinta el chart.
                    chart.update();
                    chart.redraw();
                } catch (e) {
                    chart.draw();
                }
            });

        // Botón de descarga.
        $('a.descargar')
            .unbind()
            .click(function (e) {
                // Obtención del chart usando el elemento canvas de graficas con scroll.
                var canvas = $(this).parents('div.wrap').find('div.grafica.show canvas') || $(this).parents('div.wrap').find('div.chartAreaWrapper canvas');

                /*
                if (!canvas) {
                    canvas = $(this).parents('div.wrap').find('div.chartAreaWrapper canvas');
                }
                */

                var chart = Chart.getChart(canvas);

                // Obtención del chart usando el elemento canvas de graficas sin scroll y de Chart.js
                if (chart == null) {
                    canvas = $(this).parents('div.acciones-mapa').parents("div.wrap").find("div.grafica canvas");
                    chart = Chart.getChart(canvas);
                }

                // Obtención del chart usando el elemento canvas de graficas sin scroll y de Cytoscape.js
                var image;
                if (chart == null) {
                    image = cy.jpg();
                }
                else {
                    image = chart.toBase64Image('image/jpeg', 1);
                }

                // Creación del elemento para empezar la descarga.
                var a = document.createElement('a');
                a.href = image;
                a.download = Date.now() + '.jpg';
                a.click();
            });
        $('a.csv')
            .unbind()
            .click(function (e) {
                var url = url_servicio_graphicengine + "GetCSVGrafica";
                url += "?pIdPagina=" + $(this).closest('div.row.containerPage.pageMetrics').attr('id').substring(5);
                url += "&pIdGrafica=" + $(this).parents('div.wrap').find('div.grafica.show').attr('idgrafica');
                url += "&pFiltroFacetas=" + decodeURIComponent(ObtenerHash2());
                url += "&pLang=" + lang;
                document.location.href = url;
            });
        $('a.guardar')
            .unbind()
            .click(function (e) {
                var canvas = $(this).parents('div.wrap').find('div.grafica.show canvas') || $(this).parents('div.wrap').find('div.chartAreaWrapper canvas');

                var parent = $('#modal-agregar-datos').find('.graph-container');
                var pIdGrafica = (canvas).parents('div.grafica').attr("idgrafica");
                var ctx;

                parent.css("height", "calc(100vh-100px)");

                $('#modal-agregar-datos').css('display', 'block');
                $('#modal-agregar-datos').css('pointer-events', 'none');

                $('.modal-backdrop').addClass('show');
                $('.modal-backdrop').css('pointer-events', 'auto');

                $('#modal-agregar-datos').addClass('show');
                //titulo del pop-up
                $('#modal-agregar-datos').find('p.modal-title').text("Guardar gráfica en espacio personal");

                // Leer paginas de usuario
                var idUsuario = $('.inpt_usuarioID').attr('value');
                var url = url_servicio_graphicengine + "GetPaginasUsuario"; //"https://localhost:44352/GetPaginasUsuario"
                var arg = {};
                arg.pUserId = idUsuario;

                // Petición para obtener los datos de la página.
                $.get(url, arg, function (listaData) {
                    listaData.forEach(data => {
                        // TODO
                        $('#selectorPage').append(`
                            <option></option>    
                        `)
                    });
                });
                if ($('#selectorPage').children().length == 0) {
                    $("#createPageRadio").prop("checked", true);
                    $("#selectPageRadio").parent().hide();
                    $('#selectPage').hide();
                } else {
                    $("#selectPageRadio").prop("checked", true);
                    $('#createPage').hide();
                }
            });

        $('#createPageRadio')
            .unbind()
            .change(function () {
                if (this.checked) {
                    $('#selectPage').hide();
                    $('#createPage').show();
                }
            });

        $('#selectPageRadio')
            .unbind()
            .change(function () {
                if (this.checked) {
                    $('#selectPage').show();
                    $('#createPage').hide();
                }
            });

        //boton para cambiar entre graficas (en desuso)
        /*
        $('div.toggleChart')
            .unbind()
            .click(function (e) {
                var parent = $(this).parents('div.wrap');
                var shown = parent.find('div.show');
                var hidden = parent.find('div.hide');
                shown.css('display', 'none');
                hidden.css('display', 'block');
                shown.removeClass('show');
                hidden.removeClass('hide');
                shown.addClass('hide');
                hidden.addClass('show');
            });
        */
        //menu para cambiar entre graficas
        $("select.chartMenu")
            .unbind()
            .change(function (e) {
                var parent = $(this).parents('div.wrap');
                var shown = parent.find('div.show');
                shown.css('display', 'none');
                shown.removeClass('show');
                shown.addClass('hide');
                var selected = parent.find('canvas#' + $(this).val()).parents('div.hide');
                if (selected.length) {
                    selected.css('display', 'flex');
                    selected.css('width', '100%');
                    selected.removeClass('hide');
                    selected.addClass('show');

                }
            });

        $("div.zoom")
            .unbind()
            .click(function (e) {
                var canvas = $(this).parents('div.wrap').find('div.grafica.show canvas') || $(this).parents('div.wrap').find('div.chartAreaWrapper canvas');

                var parent = $('#modal-ampliar-mapa').find('.graph-container');
                var pIdGrafica = (canvas).parents('div.grafica').attr("idgrafica");
                var ctx;
                var modalContent = $('#modal-ampliar-mapa').find('.modal-content');

                modalContent.css({ height: 'calc(100vh - 100px)' });
                modalContent.parent().css({ maxWidth: '1310px' });


                $('#modal-ampliar-mapa').css('display', 'block');
                $('#modal-ampliar-mapa').css('pointer-events', 'none');

                $('.modal-backdrop').addClass('show');
                $('.modal-backdrop').css('pointer-events', 'auto');

                $('#modal-ampliar-mapa').addClass('show');
                //titulo del pop-up
                $('#modal-ampliar-mapa').find('p.modal-title').text("grafica_" + (canvas.parents('div.grafica').attr("idgrafica")));

                if ($(canvas).parents('div.grafica').attr("idgrafica").includes("nodes")) {
                    ctx = $(`<div id="grafica_${idPaginaActual}_${pIdGrafica}" style="width: 100%; height: 500px; -webkit-tap-highlight-color: rgba(0, 0, 0, 0);"></div>`)
                    parent.append(`
                            <p id="grafica_${idPaginaActual}_${pIdGrafica}" style="text-align:center; width: 100%; font-weight: bold; color: #6F6F6F; font-size: 0.90em;"></p>
                            <div class="graph-controls" style="position: absolute; top: 24px; left: 20px; z-index: 200;">
                                <ul class="no-list-style align-items-center" style="display: flex; flex-direction: column;align-items:center">
                                    <li class="control zoomin-control" id="zoomIn">
                                        <span class="material-icons">add</span>
                                    </li>
                                    <li class="control zoomout-control" style="margin-top:5px" id="zoomOut">
                                        <span class="material-icons" >remove</span>
                                    </li>
                                </ul>
                            </div>
                        `);
                    parent.append(ctx);


                } else {
                    ctx = $(`<canvas id="grafica_${idPaginaActual}_${pIdGrafica}" width = "600" height = "250"></canvas>`);

                    if (!(canvas.parents('div.grafica').attr("idgrafica").includes("circular"))) {
                        parent.append(`
                            <div class="chartWrapper" style="position:relative; margin-top:15px">
                                <div class="chartScroll" style="overflow-${($(canvas).parents('div.grafica').attr("idgrafica").includes("isHorizontal")) ? "y" : "x"}: scroll;height:${$(modalContent).height() - 130}px;">
                                    <div style="height: 00px;" class="chartAreaWrapper">
                                    </div>
                                </div>
                            </div>
                        `);
                        parent.find('div.chartAreaWrapper').append(ctx);
                    } else {
                        parent.append(ctx);
                    }
                }
                that.getGrafica(idPaginaActual, pIdGrafica, ObtenerHash2(), ctx);

            });

        $('.modal-backdrop')
            .unbind()
            .click(cerrarModal);
        $('span.cerrar')
            .unbind()
            .click(cerrarModal);

        function cerrarModal() {

            var controls = $('#modal-ampliar-mapa').find('div.graph-container').find('li').length;
            $('#modal-ampliar-mapa').find('div.graph-container').empty();
            $('#modal-ampliar-mapa').removeClass('show');
            $('.modal-backdrop').removeClass('show');
            $('.modal-backdrop').css('pointer-events', 'none');
            $('#modal-ampliar-mapa').css('display', 'none');

            $('#modal-agregar-datos').removeClass('show');
            $('.modal-backdrop').removeClass('show');
            $('.modal-backdrop').css('pointer-events', 'none');
            $('#modal-agregar-datos').css('display', 'none');

            //Hay que repintar las graficas de nodos para que se enganche correctamente el zoom

            if (controls == 2) { //solo las graficas de nodos tienen controles
                var nodes = $('div.__________cytoscape_container').empty();
                var idGrafica = nodes.attr("id").split("_").at(-1);
                that.getGrafica(idPaginaActual, idGrafica, ObtenerHash2(), nodes);
            }

        }

        $(".listadoMenuPaginas li")
            .unbind()
            .click(function (e) {
                var numero = $(this).attr("num");
                metricas.clearPage();
                metricas.createEmptyPage(listaPaginas[numero].id);
                metricas.fillPage(listaPaginas[numero]);
            });

        plegarSubFacetas.init();
        comportamientoFacetasPopUp.init();

        // Agrega el enganche sin sobreescribir la función.
        $('#panFacetas .open-popup-link-tesauro').unbind('.clicktesauro').bind("click.clicktesauro", (function (event) {
            that.engancharComportamientos();
        }));
    }
}