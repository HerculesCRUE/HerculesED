$(document).ready(function () {
    metricas.init();
});
// Año máximo y mínimo para las facetas de años
var minYear;
var maxYear;
// Lista de páginas
// ID de la página actual.
var idPaginaActual = "";
var tituloPaginaActual;
var ordenPaginaActual;
// ID de la gráfica seleccionada.
var idGraficaActual = "";
var tituloActual;
var tamanioActual;
var ordenActual;
var TAMANIO_GRAFICA_MIN = 318;

var numPagina = 0;
// Lista de páginas.
var listaPaginas;

var metricas = {
    init: function () {
        // Esto impide que las facetas se apliquen a la primera pagina al recargar desde otra pagina

        if (ObtenerHash2().includes('~~~')) {//este codigo se incluye para no borrar las facetas al quitar una
            var splitHash = ObtenerHash2().split('~~~');
            numPagina = ObtenerHash2().split('~~~')[1];
            history.pushState('', 'New URL: ', '?' + splitHash[0]);
        } else if (performance.navigation.type == performance.navigation.TYPE_RELOAD && ObtenerHash2()) {// si se recarga la pagina con filtos, se eliminan estos para que no se apliquen a la 2a pagina por error
            history.pushState('', 'New URL: ', '?'); //TODO quitar esto haciendo que obtenga la pagina en la que se esta
        }


        if (!$('div').hasClass('indicadoresPersonalizados')) {
            this.getPages();
        } else {
            this.getPagesPersonalized();
        }
        return;
    },
    config: function () {
        return;
    },
    getPages: function () {
        var that = this;
        var url = url_servicio_graphicengine + "GetPaginasGraficas"; //"https://localhost:44352/GetPaginaGrafica";        
        var arg = {};
        arg.pLang = lang;

        // Petición para obtener los datos de la página.
        $.get(url, arg, function (listaData) {
            for (let i = 0; i < listaData.length; i++) {
                $(".listadoMenuPaginas").append(`
                    <li class="nav-item" id="${listaData[i].id}" num="${i}">
                        <a class="nav-link ${i == numPagina ? "active" : ""} uppercase">${listaData[i].nombre}</a>
                    </li>
                `);
            }
            that.createEmptyPage(listaData[numPagina].id);
            that.fillPage(listaData[numPagina]);
            listaPaginas = listaData;
        });
    },
    getPagesPersonalized: function () {
        var that = this;
        var url = url_servicio_graphicengine + "GetPaginasUsuario"; //"https://localhost:44352/GetPaginasUsuario"  
        var arg = {};
        arg.pUserId = $('.inpt_usuarioID').attr('value');
        if (arg.pUserId == "ffffffff-ffff-ffff-ffff-ffffffffffff") { // Sin usuario
            $('div.row-content').append(`<div><h1>Login Required</h1></div>`);
        } else {
            // Petición para obtener los datos de la página.
            $.get(url, arg, function (listaData) {
                if (listaData.length == 0) {
                    $('div.row-content').append(`<div><h1>No tienes paginas</h1></div>`);
                } else {
                    for (let i = 0; i < listaData.length; i++) {
                        $(".listadoMenuPaginas").append(`
                    <li class="nav-item" id="${listaData[i].idRecurso}" num="${i}">
                        <a class="nav-link ${i == 0 ? "active" : ""} uppercase">${listaData[i].titulo}</a>
                    </li>
                `);

                    }
                    $('.topbar-container').css('display', 'flex');


                    that.createEmptyPagePersonalized(listaData[0].idRecurso.split('/')[listaData[0].idRecurso.split('/').length - 1]);
                    that.fillPagePersonalized(listaData[0]);
                    listaPaginas = listaData;
                }
            });
        }
    },
    getGrafica: function (pIdPagina, pIdGrafica, pFiltroFacetas, ctx = null, barSize = 50, pIdRecurso = null, pTitulo = null) {
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
                if (!pIdRecurso) {
                    ctx = document.getElementById("grafica_" + pIdPagina + "_" + pIdGrafica);
                } else {
                    ctx = document.getElementById(pIdRecurso);
                }
            }

            var combo = $(ctx).parents("article").find(".toggleGraficas ul.no-list-style");
            var graficaContenedor = $(ctx).parent();



            // Controla si el objeto es de ChartJS o Cytoscape.
            if ("container" in data) {
                var controls = $(ctx).parent().find(".graph-controls");
                var download = $(ctx).parents("div.wrap").find('a.descargar');
                data.container = ctx;


                data.ready = function () { window.cy = this };
                var cy = cytoscape(data);

                var combo = $(ctx).parents("article").find(".toggleGraficas ul.no-list-style");
                var titulo = data.title;
                if (pTitulo) {
                    titulo = pTitulo;
                }
                if (combo) { //para graficas agrupadas 


                    /*
                    var option = combo.find('a[value="' + "grafica_" + pIdPagina + "_" + pIdGrafica + '"]');
                    var canvasOrder = combo.parents('.wrap').find("canvas#grafica_" + pIdPagina + "_" + pIdGrafica);
                    var parent = canvasOrder.parents('div.grafica');
                    if (option.length === 0) {
                        combo.append(`
                            <a order=${parent.attr("order")} value="${"grafica_" + pIdPagina + "_" + pIdGrafica}" class="item-dropdown">
                                <span class="material-icons">${tipo}</span>
                                <span class="texto">${titulo}</span>
                            </a>
                        `)
                    }
                    */
                    //find the option with the value of the selected value of the combo



                    var selectedOption = combo.find('a[value="' + "grafica_" + pIdPagina + "_" + pIdGrafica + '"]');
                    var canvasOrder = combo.parents('.wrap').find("div#grafica_" + pIdPagina + "_" + pIdGrafica);
                    var parent = canvasOrder.parents('div.grafica');
                    if (selectedOption.length == 0) {
                        combo.append(`
                        <a order=${parent.attr("order")} value="${"grafica_" + pIdPagina + "_" + pIdGrafica}" class="item-dropdown">
                                <span class="material-icons">bubble_chart</span>
                                <span class="texto">${titulo}</span>
                            </a>
                    `)
                    }
                }
                if (!pIdRecurso) {
                    $(`#titulo_grafica_${pIdPagina}_${pIdGrafica}`).empty().append(titulo);
                } else {
                    document.getElementById('titulo_grafica_' + pIdRecurso).textContent = titulo;
                }

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
                $(download).addClass('descargarcyto');
                $(download).removeClass('descargar');
                $(download).off('click.img').on('click.clickimgcy', function (e) {
                    if ($(this).hasClass('descargar')) {
                        return;
                    }
                    var image = cy.jpg(
                        {
                            full: true,
                            quality: 1,
                        }
                    );
                    var a = document.createElement('a');
                    a.href = image;
                    var titulo
                    if (!$('div').hasClass('indicadoresPersonalizados')) {
                        titulo = cy._private.options.title;
                    } else {
                        titulo = $(this).parents('article').find('div.grafica p').text();
                    }
                    a.download = titulo + '.jpg';
                    a.click();
                });

                $(controls.find("#zoomOut"))
                    .unbind()
                    .click(function (e) {
                        cy.zoom({
                            level: cy.zoom() / 1.2,
                            renderedPosition: { x: cy.width() / 2, y: cy.height() / 2 }
                        });
                    });
                $(controls.find("#zoomIn"))
                    .unbind()
                    .click(function (e) {
                        cy.zoom({
                            level: cy.zoom() * 1.2,
                            renderedPosition: { x: cy.width() / 2, y: cy.height() / 2 }
                        });
                    });
            } else {

                var titulo = data.options.plugins.title.text;
                if (pTitulo) {
                    titulo = pTitulo;
                    data.options.plugins.title.text = pTitulo;
                }
                if (combo) {
                    var tipo = "";
                    switch (data.type) {
                        case "bar":
                            if (data.isHorizontal) {
                                tipo = "bar_chart"
                            } else {
                                tipo = "align_horizontal_left"

                            }
                            break;
                        case "pie":
                            tipo = "pie_chart";

                    }

                    var option = combo.find('a[value="' + "grafica_" + pIdPagina + "_" + pIdGrafica + '"]');
                    var canvasOrder = combo.parents('.wrap').find("canvas#grafica_" + pIdPagina + "_" + pIdGrafica);
                    var parent = canvasOrder.parents('div.grafica');
                    if (option.length === 0) {
                        combo.append(`
                            <a order=${parent.attr("order")} value="${"grafica_" + pIdPagina + "_" + pIdGrafica}" class="item-dropdown">
                                <span class="material-icons">${tipo}</span>
                                <span class="texto">${titulo}</span>
                            </a>
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
                    that.drawChart(ctx, data, pIdGrafica, barSize, titulo);
                } else {
                    if (!graficaContenedor[0].classList.contains("chartAreaWrapper")) {
                        data.options.responsive = true;
                        data.options.maintainAspectRatio = false;
                    }

                    if (pIdGrafica != null && pIdGrafica.includes("prc")) { //prefijo que indica porcentaje
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
                if (data.verTodos) {
                    $('div[idfaceta="' + data.id + '"]').append(`
                        <p class="moreResults"><a class="no-close open-popup-link open-popup-link-resultados" href="#" data-toggle="modal" faceta="6" data-target="#modal-resultados">Ver todos</a></p>
                    `);
                }
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
                if (minYear == 10000 && maxYear == 0) {
                    minYear = new Date().getFullYear();
                    maxYear = minYear;
                }
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
        if (!$('div').hasClass('indicadoresPersonalizados')) {
            $('canvas').each(function () {
                Chart.getChart(this)?.destroy();
            });
            $(window).unbind('resize');

            $('#panFacetas').empty()
            $('.resource-list-wrap').empty();
            history.pushState('', 'New URL: ', '?');
        } else {
            $('canvas').each(function () {
                Chart.getChart(this)?.destroy();
            });
            $(window).unbind('resize');

            $('.resource-list-wrap').empty();
        }
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
    createEmptyPagePersonalized: function (pIdPagina) {
        $('.containerPage').attr('id', 'page_' + pIdPagina);
        $('.containerPage').addClass('pageMetrics');
        $('main').find('.modal-backdrop').remove();
        $('main').append(`
        <div class="modal-backdrop fade" style="pointer-events: none;"></div>
        `);
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
                var tipoGrafica = (grafica.isHorizontal ? "horizontal " : "") +
                    (grafica.isNodes ? "nodes " : "") +
                    (grafica.isCircular ? "circular " : "") +
                    (grafica.isAbr ? "abr " : "") +
                    (grafica.isPercentage ? "prc " : "");
                if (!tipoGrafica) {
                    tipoGrafica = "vertical";
                }
                //tmp += `<div style="display:${index != 0 ? "none" : ""};" class="${index == 0 ? "show" : "hide"} grafica" tipoGrafica="${tipoGrafica}" idgrafica='${grafica.id}'></div>`;

                tmp += `<div order="${index}" class="${index == 0 ? "show" : "hide"} grafica" style="opacity:${index != 0 ? "0" : "100"}" tipoGrafica="${tipoGrafica}" idgrafica='${grafica.id}'></div>`;
            });
            graficasGrupo = tmp;
            /*
            ${item.length != 1 ? `
            <select class="chartMenu js-select2" href="javascript: void(0);" style="width:" ></select>`: ""}
            */
            $('#page_' + pPageData.id + '.containerPage').find('.resource-list-wrap').append(`
                <article class="resource span${item[0].anchura}"> 
                    <div class="wrap" >
                        <div class="acciones-mapa ${item.length != 1 ? 'showAcciones' : ''}" >
                            ${item.length != 1 ? `
                            <div class="toggleGraficas">
                                <a href="javascript: void(0);"  id="dropdownMasOpciones" data-toggle="dropdown">
                                    <span class="material-icons">sync_alt</span>
                                </a>
                                <div class="dropdown-menu basic-dropdown dropdown-icons dropdown-menu-left" aria-labelledby="dropdownMasOpciones">
                                        <p class="dropdown-title">Gráficas</p>
                                        <ul class="no-list-style">
                                        </ul>
                                    </div>
                            </div>`: ""}
                            <div class="wrap">
                                <div class="expand">
                                    <a href="javascript: void(0);">
                                        <span class="material-icons">expand_more</span>
                                    </a>
                                </div>

                                <div class="zoom">
                                    <a href="javascript: void(0);"   data-toggle="modal">
                                        <span class="material-icons">zoom_in</span>
                                    </a>
                                </div>

                                <div class="dropdown">
                                    <a href="javascript: void(0);"  id="dropdownMasOpciones" data-toggle="dropdown">
                                        <span class="material-icons">more_vert</span>
                                    </a>
                                    <div class="dropdown-menu basic-dropdown dropdown-icons dropdown-menu-right" aria-labelledby="dropdownMasOpciones">
                                        <p class="dropdown-title">Acciones</p>
                                        <ul class="no-list-style">
                                            <li>
                                                <a class="item-dropdown guardar">
                                                    <span class="material-icons">assessment</span>
                                                    <span class="texto">Guardar en mi panel</span>
                                                </a>
                                            </li>
                                            <li>
                                                <a class="item-dropdown csv">
                                                    <span class="material-icons">insert_drive_file</span>
                                                    <span class="texto">Descargar como .csv</span>
                                                </a>
                                            </li>
                                            <li>
                                                <a class="item-dropdown descargar" id="img">
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
    fillPagePersonalized: function (pPaginaUsuario) {
        idPaginaActual = pPaginaUsuario.idRecurso;
        var that = this;
        var url = url_servicio_graphicengine + "GetGraficasUser"; //"https://localhost:44352/GetGraficasUser"  
        var arg = {};
        arg.pPageId = pPaginaUsuario.idRecurso;
        // Petición para obtener los datos de la página.
        $.get(url, arg, function (listaData) {
            if (listaData.length == 0) {
                if ($('div.row-content').find('div.sin-graficas').length == 0) {
                    $('div.row-content').append(`<div class="sin-graficas"><h1>Aún no hay gráficas en esta página</h1></div>`); //TODO Cambiar
                } metricas.engancharComportamientos();
            }
            var tmp = [];
            var id = "";
            var gruposDeIDs = [];
            var lista = listaData.slice();
            while (lista.length > 0) {
                tmp = [];
                var grafica = lista.shift();
                tmp.push(grafica);
                gruposDeIDs.push(tmp);
            }
            gruposDeIDs.forEach(function (item, index, array) {
                var graficasGrupo;
                var tmp = '';
                item.forEach(function (grafica, index, array) {
                    if (!grafica.filtro) {
                        grafica.filtro = "";
                    }
                    tmp += `<div style="display:${index != 0 ? "none" : ""};" class="${index == 0 ? "show" : "hide"} grafica" filtro="${grafica.filtro}" idgrafica="${grafica.idGrafica}" idpagina="${grafica.idPagina}" idrecurso="${grafica.idRecurso}"></div>`;
                });
                graficasGrupo = tmp;

                $('#page_' + pPaginaUsuario.idRecurso.split('/')[pPaginaUsuario.idRecurso.split('/').length - 1] + '.containerPage').find('.resource-list-wrap').append(`
                    <article class="resource span${item[0].anchura}"> 
                        <div class="wrap" >
                            <div class="acciones-mapa ${item.length != 1 ? "showAcciones" : ""}">
                            ${item.length != 1 ? `
                                <div class="toggleGraficas">
                                    <a href="javascript: void(0);"  id="dropdownMasOpciones" data-toggle="dropdown">
                                        <span class="material-icons">sync_alt</span>
                                    </a>
                                    <div class="dropdown-menu basic-dropdown dropdown-icons dropdown-menu-left" aria-labelledby="dropdownMasOpciones">
                                            <p class="dropdown-title">Gráficas</p>
                                            <ul class="no-list-style">
                                            </ul>
                                        </div>
                                </div>`: ""}
                                <div class="wrap">
                                    <div class="expand">
                                        <a href="javascript: void(0);">
                                            <span class="material-icons">expand_more</span>
                                        </a>
                                    </div>
                                    <div class="zoom">
                                        <a href="javascript: void(0);"   data-toggle="modal">
                                            <span class="material-icons">zoom_in</span>
                                        </a>
                                    </div>
                                    <div class="dropdown">
                                        <a href="javascript: void(0);"  id="dropdownMasOpciones" data-toggle="dropdown">
                                            <span class="material-icons">more_vert</span>
                                        </a>
                                        <div class="dropdown-menu basic-dropdown dropdown-icons dropdown-menu-right" aria-labelledby="dropdownMasOpciones">
                                                <p class="dropdown-title">Acciones</p>
                                                <ul class="no-list-style">
                                                    <li>
                                                        <a class="item-dropdown csv">
                                                            <span class="material-icons">insert_drive_file</span>
                                                            <span class="texto">Descargar como .csv</span>
                                                        </a>
                                                    </li>
                                                    <li>
                                                        <a class="item-dropdown descargar" id="img">
                                                            <span class="material-icons">download</span>
                                                            <span class="texto">Descargar como imagen .jpg</span>
                                                        </a>
                                                    </li>
                                                    <li>
                                                        <a class="item-dropdown editargrafica" data-toggle="modal" data-target="#modal-editargrafica">
                                                            <span class="material-icons">edit</span>
                                                            <span class="texto">Editar y ordenar gráfica</span>
                                                        </a>
                                                    </li>
                                                    <li>
                                                        <a class="item-dropdown eliminargrafica" data-toggle="modal" data-target="#modal-eliminar">
                                                            <span class="material-icons">delete</span>
                                                            <span class="texto">Eliminar gráfica</span>
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

            that.pintarPaginaPersonalized(pPaginaUsuario.idRecurso, listaData)
        });
    },
    pintarPagina: function (pIdPagina) {
        var that = this;

        // Borra la clase modal-open del body cuando se abre el pop-up del tesáuro. 
        // TODO: Mirar porque no lo hace automáticamente.
        $("body").removeClass("modal-open");

        if ($('#selectorPage').children().length == 0) {
            $("#createPageRadio").prop("checked", true);
            $("#selectPageRadio").parent().hide();
            $('#selectPage').hide();
        } else {
            $("#selectPageRadio").prop("checked", true);
            $('#createPage').hide();
        }
        // Vacias contenedores.
        $('#page_' + pIdPagina + ' .grafica').empty();
        $('#page_' + pIdPagina + ' .box').empty();

        // Recorremos el div de las gráficas.
        $('#page_' + pIdPagina + ' .grafica').each(function () {
            if ($(this).attr("tipografica").includes("nodes")) {
                $(this).append(`
                        <p id="titulo_grafica_${pIdPagina}_${$(this).attr("idgrafica")}" style="text-align:center; width: 100%; font-weight: bold; color: #6F6F6F; font-size: 0.90em;"></p>
                        <div class="graph-controls">
                            <ul class="no-list-style align-items-center">
                                <li class="control zoomin-control" id="zoomIn">
                                    <span  class="material-icons" >add</span>
                                </li>
                                <li class="control zoomout-control" style="margin-top:5px" id="zoomOut">
                                    <span class="material-icons" >remove</span>
                                </li>
                            </ul>
                        </div>
                        <div class="graficoNodos" id="grafica_${pIdPagina}_${$(this).attr("idgrafica")}" style="height: 500px;"></div>
                    `);
            } else if (!$(this).attr("tipografica").includes("circular")) {
                $(this).append(`
                <div class="chartWrapper" >
                    <div class="chartScroll custom-css-scroll">
                        <div class="chartAreaWrapper">
                            <canvas width = "600" height = "250" id="grafica_${pIdPagina}_${$(this).attr("idgrafica")}"></canvas>
                        </div>
                    </div>
                </div>
                `);
            } else {
                $(this).css("height", "300px");
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
        filtros = filtros.replaceAll(" & ", "|||");
        var filtrosArray = filtros.split('&');
        for (let i = 0; i < filtrosArray.length; i++) {
            let filtro = filtrosArray[i].replace("|||", " & ");
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
    pintarPaginaPersonalized: function (pIdRecurso, pPageData) {
        var that = this;
        var idPagina = pIdRecurso.split('/')[pIdRecurso.split('/').length - 1];

        // Borra la clase modal-open del body cuando se abre el pop-up del tesáuro. 
        // TODO: Mirar porque no lo hace automáticamente.
        $("body").removeClass("modal-open");

        // Vacias contenedores.
        $('#page_' + idPagina + ' .grafica').empty();
        $('#page_' + idPagina + ' .box').empty();

        // Recorremos el div de las gráficas.
        var index = 0;
        $('#page_' + idPagina + ' .grafica').each(function () {
            if ($(this).attr("idgrafica").includes("nodes")) {
                $(this).append(`
                        <p id="titulo_grafica_${pPageData[index].idRecurso}" style="text-align:center; width: 100%; font-weight: bold; color: #6F6F6F; font-size: 0.90em;"></p>
                        <div class="graph-controls">
                            <ul class="no-list-style align-items-center">
                                <li class="control zoomin-control" id="zoomIn">
                                    <span class="material-icons">add</span>
                                </li>
                                <li class="control zoomout-control" id="zoomOut">
                                    <span class="material-icons" >remove</span>
                                </li>
                            </ul>
                        </div>
                        <div class="graficoNodos" id="${pPageData[index].idRecurso}" style="height: 500px;"></div>
                    `);
            } else if (!$(this).attr("idgrafica").includes("circular")) {
                $(this).append(`
                <div class="chartWrapper" >
                    <div class="chartScroll custom-css-scroll " style="overflow-${pPageData[index].idGrafica.includes("isHorizontal") ? "y" : "x"}: scroll;height:${TAMANIO_GRAFICA_MIN}px;">
                        <div class="chartAreaWrapper">
                            <canvas width = "600" height = "250" id="${pPageData[index].idRecurso}"></canvas>
                        </div>
                    </div>
                </div>
                `);
            } else {
                $(this).css("height", "300px");
                $(this).append(`
                    <canvas id = "${pPageData[index].idRecurso}"></canvas>
                        `);
            }

            that.getGrafica(pPageData[index].idPagina, pPageData[index].idGrafica, pPageData[index].filtro, null, 50, pPageData[index].idRecurso, pPageData[index].titulo);
            index++;
        });
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
    drawChart: function (ctx, data, pIdGrafica = null, barSize = 100, pTitulo = null) {
        if (Chart.getChart(ctx) != null) {// en caso de que se ejecute esto despues de cambiar de pagina desde la anterior
            return;
        }
        var numBars = data.data.labels.length; // Número de barras.
        var canvasSize = (numBars * barSize) * 1.5; // Tamaño del canvas, el 1.5 representa el espacio entre las barras.
        var canvas = ctx;
        var chartAreaWrapper = canvas.parentNode;
        var scrollContainer = chartAreaWrapper.parentNode;
        var chartContainer = scrollContainer.parentNode;
        var graficaContainer = chartContainer.parentNode;
        var horizontal = data.options.indexAxis == "y";
        var titulo = data.options.plugins.title.text;
        barSize /= data.data.datasets.length;
        if (pTitulo) {
            titulo = pTitulo;
        }

        if (horizontal) {
            graficaContainer.classList.add("horizontal");
        } else {
            graficaContainer.classList.add("vertical");
        }

        data.options.maintainAspectRatio = false;
        data.options.responsive = true;

        //esto modifica el tamaño de las barras 
        data.data.datasets.forEach((item) => {
            item['barThickness'] = barSize;

        }) //todo mover a json
        data.options.scale = {
            ticks: {
                precision: 0
            }
        }
        //Abrebiacion de los labels del eje
        // TODO echar un vistazo para ver como queda el tema de la abreviación
        if (pIdGrafica != null && pIdGrafica.includes("abr")) {
            // Se modifica la propiedad que usa Chart.js para obtener los labels de la gráfica.
            if (horizontal) {
                data.options.scales['y'] = {
                    ticks: {
                        callback: ticksAbr,
                        mirror: true,
                        padding: -5,
                        font: {
                            weight: "bold"
                        },
                        z: 1000
                    }

                }
            } else { //vertical
                data.options.scales['x'] = {
                    ticks: {
                        callback: ticksAbr
                    }
                }
            }
        }
        function ticksAbr(value) {
            const labels = data.data.labels; // Obtención de los labels.
            if (value >= 0 && value < labels.length) {
                if (labels[value].length >= 40) {
                    return labels[value].substring(0, 40) + "..."; // Se muestran solo los 40 primeros caractéres para que no se salga de la barra.
                }
                return labels[value];
            }
            return value;
        }
        // Si el canvas no supera el tamaño del contenedor, no se hace scroll.

        var modalBody = $(graficaContainer).parents(".modal-body");
        //si la grafica es horizontal y su altura es menor a (310 si no esta en zoom, tamaño de ventana - 270 si esta en zoom ) o si es vertical y su ancho es menor a (su contenedor si no tiene zoom, 1110 si tiene zoom) no necesita scroll 
        if ((canvasSize < (modalBody.length != 0 ? $(window).height() - 230 : TAMANIO_GRAFICA_MIN) && horizontal) || (canvasSize < (modalBody.length != 0 ? 1110 : $(graficaContainer).width()) && !horizontal)) {
            if (barSize < 100) {
                $(ctx).parents(".modal-content").css("display", "block");
            }
            graficaContainer.classList.add("small");
            if (modalBody.length == 0) {
                chartAreaWrapper.style.height = TAMANIO_GRAFICA_MIN + "px";
            } else {
                chartAreaWrapper.style.height = "100%";
            }
            scrollContainer.style.overflow = "hidden";



            var myChart = new Chart(ctx, data);
        } else { // a partir de aqui se prepara el scroll
            if (barSize < 100) { //para revelar el modal del zoom 
                $(ctx).parents(".modal-content").css("display", "block");
            }
            var hasMainAxis = false; //eje superior en caso horizontal, izquierdo en vertical
            var hasSecondaryAxis = false; // eje inferior o derecho

            if (horizontal) {
                ctx.parentNode.style.height = canvasSize + 'px'; //se establece la altura del eje falso
            } else {// -- vertical
                //myChart.canvas.parentNode.style.width = canvasSize + 'px';
                ctx.parentNode.style.height = 100 + '%'; //se escala la altura //css done
                ctx.parentNode.style.width = canvasSize + 'px'; //se escala la anchura respecto al canvas para que ocupe el scroll
            }

            var myChart = new Chart(ctx, data);
            // Se comprueba si tiene eje principal/secundario.
            Object.entries(data.options.scales).forEach((scale) => { // por cada escala que tenga data
                if ((scale[1].axis == "x" && horizontal) || (scale[1].axis == "y" && !horizontal)) { //se comprueba si tiene eje principal (top en caso de horizontal, left en caso de vertical)
                    if ((scale[1].position == "top" || scale[1].position == "left") && !hasMainAxis) {
                        hasMainAxis = true;
                    } else if ((scale[1].position == "bottom" || scale[1].position == "right") && !hasSecondaryAxis) {//se comprueba si tiene eje secundario (bottom en caso de horizontal, right en caso de vertical)
                        hasSecondaryAxis = true;
                    }

                }
            });
            // Leyenda con titulo y contenedor para datasets. style="text-align: center; position: absolute; top: 0px; background-color: white;
            var legend = $(`<div class="chartLegend" >
                <h4 id="legendTitle">${titulo}</h4>
                </div>`);

            $(chartContainer).append(legend);
            var dataSetLabels = $(`<div class="dataSetLabels"></div>`)
            $(legend).append(dataSetLabels);

            // Por cada dataset que exista se creara un div con su nombre y color y se añade a dataSetLabels.
            var datasets = data.data.datasets;
            datasets.forEach((dataset, index) => {
                var labelContainer = $(`<div id="label-${index}" class="labelContainer" >
                    <div style="background-color: ${dataset.backgroundColor[0]};"></div>
                    <p class="dataSetLabel">${dataset.label}</p>
                    </div>`);
                $(dataSetLabels).append(labelContainer);
            });

            //Se añade el eje principal al contenedor.
            if (hasMainAxis) {
                if (horizontal) {
                    var mainAxis = $(`<canvas id="topAxis" class="myChartAxis"></canvas>`);
                    $(legend).append(mainAxis);
                } else {
                    var mainAxis = $(`<canvas id="leftAxis" class="myChartAxis"></canvas>`);
                    $(chartContainer).append(mainAxis);
                }
            }
            //Se añade el eje secundario al contenedor.
            if (hasSecondaryAxis) {
                if (horizontal) {
                    var secondaryAxis = $(`<canvas id="bottomAxis" class="myChartAxis"></canvas>`);
                } else {
                    var secondaryAxis = $(`<canvas id="rightAxis" class="myChartAxis"></canvas>`);
                }
                $(chartContainer).append(secondaryAxis);
            }

            // Cuando se actualiza el canvas.
            if (!pIdGrafica.includes("circular")) {
                data.options.animation.onProgress = () => this.reDrawChart(myChart, mainAxis, secondaryAxis, canvasSize, legend, horizontal);
                $(window).bind('resize', function () {// evento que se dispara al reescalar el navegador o hacer zoom (esto desalinea los ejes)
                    myChart.update();
                });
            }

            if (data.isDate) { //las graficas de fechas se mueven hasta el año mas reciente que se encuentra al final del scroll
                if (horizontal) {
                    $(scrollContainer).animate({ scrollTop: $(chartAreaWrapper).height() - $(scrollContainer).height() }, 6000);
                    $(scrollContainer).mousedown((e) => {//evento que se dispara al hacer click en el scroll
                        if (scrollContainer.clientWidth <= e.offsetX) {
                            $(scrollContainer).stop();//y detiene la animacion 
                        }
                    });
                } else {
                    $(scrollContainer).animate({ scrollLeft: $(chartAreaWrapper).width() - $(scrollContainer).width() }, 6000);
                    // Se detiene la animacion al hacer click en el scroll o al pulsar la rueda del raton.
                    $(scrollContainer).mousedown((e) => {
                        if (e.button == 1 || scrollContainer.clientHeight <= e.offsetY) {
                            $(scrollContainer).stop();
                        }
                    });


                }


            }

        }

    },
    //funcion que se encarga de actualizar el tamaño de los ejes y el scroll en caso de reescalado. tambien los pinta al generarse la grafica
    reDrawChart: function (myChart, mainAxis, secondaryAxis, canvasSize, legend, horizontal = false) {

        /* TODO - Actualizar el tamaño de las barras dependiendo de los datasets visibles.
        myChart.data.datasets.forEach((dataset, index) => { // esto casi funciona, pero hace cosas raras
            dataset['barThickness'] = 50/(myChart.getVisibleDatasetCount());
        })
        */


        // Se obtiene la escala del navegador (afecta cuando el usuario hace zoom).
        var scale = window.devicePixelRatio;
        //anchura y altura del recorte de la grafica
        var copyWidth;
        var copyHeight;
        // Anchura y altura del pegado a los ejes.
        var axisHeight;
        var axisWidth;
        if (horizontal) {
            myChart.canvas.parentNode.style.height = canvasSize + 'px'; //se establece la altura del eje falso
            copyWidth = myChart.width;
            // Altura del titulo, leyenda y eje superior menos el margen.
            copyHeight = myChart.boxes[0].height + myChart.boxes[1].height + myChart.boxes[2]?.height - 5;
            // Altura del eje
            axisHeight = myChart.boxes[2]?.height;
        } else {// -- vertical
            myChart.canvas.parentNode.style.width = canvasSize + 'px'; //se escala la anchura respecto al canvas para que ocupe el scroll

            copyWidth = myChart.boxes[2]?.width; //anchura del eje
            copyHeight = myChart.chartArea.bottom + 5;
            targetY = 20; //posicion del eje
            // Le asignamos tamaño a la leyenda.
            axisHeight = myChart.height - 10;
        }
        // Preparamos el eje superior.
        $(legend).css("width", horizontal ? copyWidth + "px" : "100%");
        $(legend).css("height", horizontal ? myChart.chartArea.top + "px" : "auto");

        //si la leyenda falsa es mayor a la del canvas se añade la diferencia en margen para compensar
        //esto sucede cuando en el canvas la leyenda ocupa una fila pero en el div 2 o mas;
        if ($(legend).height() > myChart.chartArea.top) {
            //añadimos el margen
            myChart.canvas.style.marginTop = $(legend).height() - myChart.chartArea.top + 5 + "px";
            if (mainAxis) {
                mainAxis[0].style.marginTop = $(legend).height() - myChart.chartArea.top + 5 + "px";
            }
            if (secondaryAxis) {
                secondaryAxis[0].style.marginTop = $(legend).height() - myChart.chartArea.top + 5 + "px";
            }

        }

        // Posición del comienzo del recorte.
        var targetX = 0;
        var targetY = 0;
        var targetWidth = copyWidth * scale;
        var targetHeight = axisHeight * scale;

        // Posicionamiento del pegado.
        var x = 0;
        var y = 0;
        var width = copyWidth;
        var height = horizontal ? axisHeight + 4 : copyHeight;
        var ctx;
        // (Preparamos el eje superior o izquierdo.) 

        if (mainAxis) {
            ctx = mainAxis[0].getContext('2d');
            if (horizontal) {
                ctx.canvas.height = axisHeight;
            } else {
                copyHeight = myChart.chartArea.bottom + 5;
                targetHeight = copyHeight * scale;
                height = copyHeight;
                copyWidth = myChart.chartArea.left;
                targetWidth = copyWidth * scale;
                width = copyWidth;
                ctx.canvas.height = copyHeight;

            }
            ctx.scale(scale, scale); // Escala del zoom.
            ctx.canvas.width = copyWidth;

            ctx.drawImage(myChart.canvas, targetX, targetY, targetWidth, targetHeight, x, y, width, height);
        }

        // Preparamos el eje inferior o derecho.
        if (secondaryAxis) {
            copyWidth = myChart.boxes[2]?.width; //anchura del eje

            ctx = secondaryAxis[0].getContext('2d');
            if (horizontal) {
                ctx.canvas.height = axisHeight;
                targetY = myChart.chartArea.bottom * scale;
                ctx.canvas.style.paddingLeft = myChart.chartArea.left - 5 + "px";
                copyWidth += 15;
                targetX = myChart.chartArea.left * scale - 5;
            } else {


                ctx.canvas.height = copyHeight;
                targetX = (myChart.width - copyWidth) * scale;
                targetWidth = copyWidth * scale;
                width = copyWidth;
                axisHeight -= 7 * scale; //se le quita al eje falso el margen sobrante 



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
        // este codigo se asegura que el item seleccionado en los menus es el que esta mostrandose. 
        var menus = $(".toggleGraficas");
        menus.each((index, menu) => { // recorre todos los menus 

            var listItems = $(menu).find("ul").children(); // obtiene todos los items de un menu
            listItems.detach().sort(function (a, b) { // bordena los items
                return $(a).attr("order") < $(b).attr("order") ? -1 : 1;
            }).appendTo($(menu).find("ul")); // los agrega al menu
            var selectedID = $(menu).parents("article div.wrap").find("div.show.grafica").attr("idgrafica"); //Obtiene la id de la grafica visible
            $(menu).find("a[value='" + "grafica_" + idPaginaActual + "_" + selectedID + "']").addClass("active"); //Añade la clase active al item que esta visible
        });

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
        $('.containerFacetas a.filtroMetrica,.listadoTesauro a.filtroMetrica, .indice-lista .faceta')
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
                filtros = filtros.replaceAll(" & ", "|||");
                var filtrosArray = filtros.split('&');
                filtros = '';
                var contieneFiltro = false;
                for (var i = 0; i < filtrosArray.length; i++) {
                    let filtro = filtrosArray[i].replace("|||", " & ");
                    if (filtro != '') {
                        if (filtro == filtroActual) {
                            contieneFiltro = true;
                        } else {
                            filtros += filtro + '&';
                        }

                    }
                }
                if (!contieneFiltro) {
                    filtros += filtroActual;
                }

                var numPagina = $(".nav-item#" + idPaginaActual).attr("num");
                history.pushState('', 'New URL: ' + filtros, '?' + filtros + '~~~' + numPagina);
                e.preventDefault();

                location.reload();
            });

        $('.borrarFiltros')
            .unbind()
            .click(function (e) {
                history.pushState('', 'New URL: ', '?');
                e.preventDefault();
                location.reload();
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

                try { // Hay problemas con el gráfico de líneas + grafico de barras stackeado, si falla se repinta el chart.
                    chart.update();
                } catch (e) {
                    console.log(e);
                    chart.draw();
                }
            });

        // Botón de descarga.
        $('a.descargar').off('click.img').on('click.img', function (e) {
            if ($(this).hasClass('descargarcyto')) {
                return;
            }
            // Obtención del chart usando el elemento canvas de graficas con scroll.
            var canvas = $(this).parents('div.wrap').find('div.grafica.show canvas') || $(this).parents('div.wrap').find('div.chartAreaWrapper canvas');
            var chart = Chart.getChart(canvas);
            // Obtención del chart usando el elemento canvas de graficas sin scroll y de Chart.js
            if (chart == null) {
                canvas = $(this).parents('div.acciones-mapa').parents("div.wrap").find("div.grafica canvas");
                chart = Chart.getChart(canvas);
            }
            var image = chart.toBase64Image('image/jpeg', 1);
            // Creación del elemento para empezar la descarga.
            var a = document.createElement('a');
            a.href = image;
            a.download = chart.config._config.options.plugins.title.text + '.jpg';
            a.click();
        });
        $('a.csv')
            .unbind()
            .click(function (e) {
                var url = url_servicio_graphicengine + "GetCSVGrafica";
                if (!$('div').hasClass('indicadoresPersonalizados')) {
                    url += "?pIdPagina=" + $(this).closest('div.row.containerPage.pageMetrics').attr('id').substring(5);
                    url += "&pIdGrafica=" + $(this).parents('div.wrap').find('div.grafica.show').attr('idgrafica');
                    url += "&pFiltroFacetas=" + decodeURIComponent(ObtenerHash2());
                    url += "&pLang=" + lang;
                    var urlAux = url_servicio_graphicengine + "GetGrafica"; //"https://localhost:44352/GetGrafica"
                    var argAux = {};
                    argAux.pIdPagina = $(this).closest('div.row.containerPage.pageMetrics').attr('id').substring(5);
                    argAux.pIdGrafica = $(this).parents('div.wrap').find('div.grafica.show').attr('idgrafica');
                    argAux.pFiltroFacetas = decodeURIComponent(ObtenerHash2());
                    argAux.pLang = lang;
                    $.get(urlAux, argAux, function (listaData) {
                        if (!listaData.options) {
                            url += "&pTitulo=" + listaData.title;
                        } else {
                            url += "&pTitulo=" + listaData.options.plugins.title.text;
                        }
                        document.location.href = url;
                    });
                } else {
                    url += "?pIdPagina=" + $(this).parents('div.wrap').find('div.grafica.show').attr('idpagina');
                    url += "&pIdGrafica=" + $(this).parents('div.wrap').find('div.grafica.show').attr('idgrafica');
                    var filtro = $(this).parents('div.wrap').find('div.grafica.show').attr('filtro');
                    if (filtro != "") {
                        url += "&pFiltroFacetas=" + $(this).parents('div.wrap').find('div.grafica.show').attr('filtro');
                    }
                    url += "&pLang=" + lang;
                    idGraficaActual = $(this).closest('article').find("div.show.grafica").attr("idrecurso");
                    var urlAux = url_servicio_graphicengine + "GetGraficasUser"; //"https://localhost:44352/GetGraficasUser"
                    var argAux = {};
                    argAux.pPageId = idPaginaActual;
                    $.get(urlAux, argAux, function (listaData) {
                        listaData.forEach(data => {
                            if (data.idRecurso == idGraficaActual) {
                                tituloActual = data.titulo;
                            }
                        });
                        url += "&pTitulo=" + tituloActual;
                        document.location.href = url;
                    });
                }
            });

        $('a.editargrafica')
            .unbind()
            .click(function (e) {
                // Limpia los campos.
                $("#labelTituloGrafica").val("");
                $("#idSelectorOrden").empty();
                $("#idSelectorTamanyo").val("11").change();
                idGraficaActual = $(this).closest('article').find("div.show.grafica").attr("idrecurso");
                // Leer gráficas de esta página
                var url = url_servicio_graphicengine + "GetGraficasUser"; //"https://localhost:44352/GetGraficasUser"
                var arg = {};
                arg.pPageId = idPaginaActual;
                var orden = 1;
                // Petición para obtener los datos de la página.
                $.get(url, arg, function (listaData) {
                    listaData.forEach(data => {
                        if (data.idRecurso == idGraficaActual) {
                            tituloActual = data.titulo;
                            tamanioActual = data.anchura;
                            ordenActual = data.orden;
                        }
                        $('#idSelectorOrden').append(`
                            <option value="${orden}">${orden}</option>    
                        `)
                        orden++;
                    });
                    // Rellena los campos
                    $("#labelTituloGrafica").val(tituloActual);
                    $("#idSelectorTamanyo").val(tamanioActual).change();
                    $("#idSelectorOrden").val(ordenActual).change();
                });
            });
        $('div.edit-page')
            .unbind()
            .click(function (e) {
                // Limpia los campos.
                $("#labelTituloPagina").val("");
                $("#idSelectorOrdenPg").empty();
                // Leer páginas
                var url = url_servicio_graphicengine + "GetPaginasUsuario"; //"https://localhost:44352/GetPaginasUsuario"
                var arg = {};
                arg.pUserId = $('.inpt_usuarioID').attr('value');
                var orden = 1;
                // Petición para obtener los datos de la página.
                $.get(url, arg, function (listaData) {
                    listaData.forEach(data => {
                        if (data.idRecurso == idPaginaActual) {
                            tituloPaginaActual = data.titulo;
                            ordenPaginaActual = data.orden;
                        }
                        $('#idSelectorOrdenPg').append(`
                            <option value="${orden}">${orden}</option>    
                        `)
                        orden++;
                    });
                    // Rellena los campos
                    $("#labelTituloPagina").val(tituloPaginaActual);
                    $("#idSelectorOrdenPg").val(ordenPaginaActual).change();
                });
            });

        $('a.eliminargrafica')
            .unbind()
            .click(function (e) {
                idGraficaActual = $(this).closest('article').find("div.show.grafica").attr("idrecurso");
            });

        $('a.eliminar')
            .unbind()
            .click(function (e) {
                // Leer paginas de usuario
                var idUsuario = $('.inpt_usuarioID').attr('value');
                var idPagina = idPaginaActual;
                var idGrafica = idGraficaActual;
                var url = url_servicio_graphicengine + "BorrarGrafica"; //"https://localhost:44352/BorrarGrafica"
                var arg = {};
                arg.pUserId = idUsuario;
                arg.pPageID = idPagina;
                arg.pGraphicID = idGrafica;

                // Petición para eliminar la gráfica.
                $.get(url, arg, function (listaData) {
                    location.reload();
                });
            });

        $('a.eliminarpg')
            .unbind()
            .click(function (e) {
                // Leer paginas de usuario
                var idUsuario = $('.inpt_usuarioID').attr('value');
                var idPagina = idPaginaActual;
                var url = url_servicio_graphicengine + "BorrarPagina"; //"https://localhost:44352/BorrarPagina"
                var arg = {};
                arg.pUserId = idUsuario;
                arg.pPageID = idPagina;

                // Petición para eliminar la página.
                $.get(url, arg, function (listaData) {
                    location.reload();
                });
            });

        $('a.guardar')
            .unbind()
            .click(function (e) {
                // Lipia los campos.
                $("#labelTituloGrafica").val("");
                $("#idSelectorPagina").empty();
                $("#labelTituloPagina").val("");
                $("#idSelectorTamanyo").val("11").change();

                // Obtiene el ID de la gráfica seleccionada.
                idGraficaActual = $(this).closest('article').find("div.show.grafica").attr("idgrafica");

                // Leer paginas de usuario

                var idUsuario = $('.inpt_usuarioID').attr('value');
                var url = url_servicio_graphicengine + "GetPaginasUsuario"; //"https://localhost:44352/GetPaginasUsuario"
                var arg = {};
                arg.pUserId = idUsuario;

                // Petición para obtener los datos de la página.
                $.get(url, arg, function (listaData) {
                    listaData.forEach(data => {
                        $('#idSelectorPagina').append(`
                            <option idPagina="${data.idRecurso}">${data.titulo}</option>    
                        `)
                    });

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

                    if ($('#idSelectorPagina').children().length == 0) {
                        $("#createPageRadio").prop("checked", true);
                        $("#selectPageRadio").parent().hide();
                        $('#selectPage').hide();
                        $('#createPage').show();
                    } else {
                        $("#selectPageRadio").prop("checked", true);
                        $("#selectPageRadio").parent().show();
                        $('#selectPage').show();
                        $('#createPage').hide();
                    }

                });
            });
        $('#btnGuardarEditPagina')
            .unbind()
            .click(function (e) {
                //TODO translate
                var formTituloPagina = $('#labelTituloPagina').val();
                if (!formTituloPagina) {
                    mostrarNotificacion("warning", "Introduce el titulo de la pagina por favor");
                    return;
                }

                MostrarUpdateProgress();
                var user = $('.inpt_usuarioID').attr('value');
                var url = url_servicio_graphicengine + "EditarNombrePagina"; //"https://localhost:44352/EditarNombrePagina"
                var arg = {};
                arg.pUserId = user;
                arg.pPageID = idPaginaActual;
                arg.pNewTitle = $('#labelTituloPagina').val();
                arg.pOldTitle = tituloPaginaActual;

                $.get(url, arg, function () {
                    var urlOrd = url_servicio_graphicengine + "EditarOrdenPagina"; //"https://localhost:44352/EditarOrdenPagina"
                    var argOrd = {};
                    argOrd.pUserId = user;
                    argOrd.pPageID = idPaginaActual;
                    argOrd.pNewOrder = $('#idSelectorOrdenPg option:selected').val();

                    argOrd.pOldOrder = ordenPaginaActual;

                    $.get(urlOrd, argOrd, function () {
                        location.reload();
                    });
                });
            });
        $('#btnGuardarEditGrafica')
            .unbind()
            .click(function (e) {
                //TODO translate
                var formTitle = $('#labelTituloGrafica').val();
                if (!formTitle) {
                    mostrarNotificacion("warning", "Introduce el titulo por favor");
                    return;
                }

                MostrarUpdateProgress();
                var user = $('.inpt_usuarioID').attr('value');
                var url = url_servicio_graphicengine + "EditarNombreGrafica"; //"https://localhost:44352/EditarNombreGrafica"
                var arg = {};
                arg.pUserId = $('.inpt_usuarioID').attr('value');
                arg.pPageID = idPaginaActual;
                arg.pGraphicID = idGraficaActual;
                arg.pNewTitle = $('#labelTituloGrafica').val();
                arg.pOldTitle = tituloActual;

                $.get(url, arg, function () {
                    var urlOrd = url_servicio_graphicengine + "EditarOrdenGrafica"; //"https://localhost:44352/EditarOrdenGrafica"
                    var argOrd = {};
                    argOrd.pUserId = user;
                    argOrd.pPageID = idPaginaActual;
                    argOrd.pGraphicID = idGraficaActual;
                    argOrd.pNewOrder = $('#idSelectorOrden option:selected').val();
                    argOrd.pOldOrder = ordenActual;

                    $.get(urlOrd, argOrd, function () {
                        var urlAnch = url_servicio_graphicengine + "EditarAnchuraGrafica"; //"https://localhost:44352/EditarAnchuraGrafica"
                        var argAnch = {};
                        argAnch.pUserId = user;
                        argAnch.pPageID = idPaginaActual;
                        argAnch.pGraphicID = idGraficaActual;
                        argAnch.pNewWidth = $('#idSelectorTamanyo option:selected').val();
                        argAnch.pOldWidth = tamanioActual;

                        $.get(urlAnch, argAnch, function () {
                            location.reload();
                        });
                    });
                });


            });

        $('#btnGuardarGrafica')
            .unbind()
            .click(function (e) {

                //TODO translate
                var formTitle = $('#labelTituloGrafica').val();
                if (!formTitle) {
                    mostrarNotificacion("warning", "Introduce el titulo de la grafica por favor");
                    return;
                }
                var formCreatePageRadio = $('#createPageRadio').is(':checked');
                if (formCreatePageRadio) {
                    var formCreatePage = $('#labelTituloPagina').val();
                    if (!formCreatePage) {
                        mostrarNotificacion("warning", "Introduce el titulo de la pagina por favor");
                        return;
                    }
                }




                var url = url_servicio_graphicengine + "GuardarGrafica"; //"https://localhost:44352/GuardarGrafica"
                var arg = {};
                arg.pTitulo = $('#labelTituloGrafica').val();
                arg.pAnchura = $('#idSelectorTamanyo option:selected').val();
                arg.pIdPaginaGrafica = idPaginaActual;
                arg.pIdGrafica = idGraficaActual;
                arg.pFiltros = ObtenerHash2();
                arg.pUserId = $('.inpt_usuarioID').attr('value');

                if ($("#selectPage").is(":visible")) {
                    arg.pIdRecursoPagina = $('#idSelectorPagina option:selected').attr("idPagina");
                } else {
                    arg.pIdRecursoPagina = "";
                }

                if ($("#createPage").is(":visible")) {
                    arg.pTituloPagina = $('#labelTituloPagina').val();
                } else {
                    arg.pTituloPagina = "";
                }

                // Petición para obtener los datos de la página.
                $.get(url, arg, function (data) {
                    mostrarNotificacion("success", "Grafica guardada correctamente"); //TODO - asegurarse que se guarda, por que si falla sigue saliendo este mensaje
                    cerrarModal();
                });
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
        $(".toggleGraficas ul a")
            .unbind()
            .click(function (e) {
                // Establecemos la grafica seleccionada como activa en el menu
                $(this).parent().find(".active").removeClass("active");
                $(this).addClass("active");
                // enconntramos la grafica que esta siendo mostrada
                var parent = $(this).parents('div.wrap');
                var shown = parent.find('div.grafica.show');
                // y la ocultamos
                shown.css('opacity', '0'); // display none causa problemas con redrawChart por que intenta modifica un elemento sin altura
                shown.css('position', 'absolute');
                shown.removeClass('show');
                shown.addClass('hide');

                // ahora buscamos la grafica que se quiere mostrar
                var selected = parent.find('#' + $(this).attr("value")).parents('div.hide');
                if (selected.length) { // si la grafica existe
                    // la mostramos
                    selected.css('display', 'flex');
                    selected.css('opacity', '1');
                    selected.css('position', 'relative');
                    selected.css('width', '100%');
                    selected.removeClass('hide');
                    selected.addClass('show');


                    if (selected.attr('idgrafica').includes('nodes')) {
                        selected.parents('article').find('a#img').addClass('descargarcyto');
                        selected.parents('article').find('a#img').removeClass('descargar');
                    } else {
                        selected.parents('article').find('a#img').addClass('descargar');
                        selected.parents('article').find('a#img').removeClass('descargarcyto');
                        metricas.engancharComportamientos();
                    }
                }
                /* TODO testear sin esto
                var canvas = parent.find('canvas#' + $(this).attr("value"));
                if (canvas.length) {
                    var chart = Chart.getChart(canvas[0]);
                    // ejecutamos el redraw para reescalar la grafica en caso que sea necesario.
                    chart.config._config.options.animation.onProgress(); 
                }*/
            });

        //boton del pop-up con la grafica escalada
        $("div.zoom")
            .unbind()
            .click(function (e) {
                // Obtiene la gráfica seleccionada (en caso de menu) o la grafica del contenedor en casos normales.
                var canvas = $(this).parents('div.wrap').find('div.grafica.show canvas') || $(this).parents('div.wrap').find('div.chartAreaWrapper canvas');
                var idgrafica = (canvas).parents('div.grafica').attr("tipografica") || (canvas).parents('div.grafica').attr("idgrafica");
                var parent = $('#modal-ampliar-mapa').find('.graph-container');
                parent.removeClass('small horizontal vertical'); // se le quitan los estilos que podria tener
                var pIdGrafica = (canvas).parents('div.grafica').attr("idgrafica");
                var ctx;
                var modalContent = $('#modal-ampliar-mapa').find('.modal-content');
                // Tamaño del contenedor (dejando 50px de margen arriba y abajo).
                modalContent.css({ height: 'calc(100vh - 100px)' });
                modalContent.parent().css({ maxWidth: '1310px' }); // El tamaño maximo del contendor de los articles.

                // Se revela el popup.
                $('#modal-ampliar-mapa').css('display', 'block');
                $('#modal-ampliar-mapa').css('pointer-events', 'none');
                $('.modal-backdrop').addClass('show');
                $('.modal-backdrop').css('pointer-events', 'auto');
                $('#modal-ampliar-mapa').addClass('show');

                //se popula con los contenedores adecuados
                if (!$('div').hasClass('indicadoresPersonalizados')) {
                    parent.append(`<div class="acciones-mapa">
                    <div class="wrap">
                        <div class="dropdown">
                            <a href="javascript: void(0);"  id="dropdownMasOpciones" data-toggle="dropdown">
                                <span class="material-icons">more_vert</span>
                            </a>
                            <div class="dropdown-menu basic-dropdown dropdown-icons dropdown-menu-right" aria-labelledby="dropdownMasOpciones">
                                <p class="dropdown-title">Acciones</p>
                                <ul class="no-list-style">
                                    <li>
                                        <a class="item-dropdown guardarzoom">
                                            <span class="material-icons">assessment</span>
                                            <span class="texto">Guardar en mi panel</span>
                                        </a>
                                    </li>
                                    <li>
                                        <a class="item-dropdown csvzoom">
                                            <span class="material-icons">insert_drive_file</span>
                                            <span class="texto">Descargar como .csv</span>
                                        </a>
                                    </li>
                                    <li>
                                        <a class="item-dropdown descargarzoom">
                                            <span class="material-icons">download</span>
                                            <span class="texto">Descargar como imagen .jpg</span>
                                        </a>
                                    </li>
                                </ul>
                            </div>
                        </div>
                    </div>
                </div> `)
                } else {
                    parent.append(`<div class="acciones-mapa">
                    <div class="wrap">
                        <div class="dropdown">
                            <a href="javascript: void(0);"  id="dropdownMasOpciones" data-toggle="dropdown">
                                <span class="material-icons">more_vert</span>
                            </a>
                            <div class="dropdown-menu basic-dropdown dropdown-icons dropdown-menu-right" aria-labelledby="dropdownMasOpciones">
                                <p class="dropdown-title">Acciones</p>
                                <ul class="no-list-style">
                                    <li>
                                        <a class="item-dropdown csvzoom">
                                            <span class="material-icons">insert_drive_file</span>
                                            <span class="texto">Descargar como .csv</span>
                                        </a>
                                    </li>
                                    <li>
                                        <a class="item-dropdown descargarzoom">
                                            <span class="material-icons">download</span>
                                            <span class="texto">Descargar como imagen .jpg</span>
                                        </a>
                                    </li>
                                    <li>
                                        <a class="item-dropdown editargraficazoom" data-toggle="modal" data-target="#modal-editargrafica">
                                            <span class="material-icons">edit</span>
                                            <span class="texto">Editar y ordenar gráfica</span>
                                        </a>
                                    </li>
                                    <li>
                                        <a class="item-dropdown eliminargraficazoom" data-toggle="modal" data-target="#modal-eliminar">
                                            <span class="material-icons">delete</span>
                                            <span class="texto">Eliminar gráfica</span>
                                        </a>
                                    </li>
                                </ul>
                            </div>
                        </div>
                    </div>
                </div>`)
                }
                // Preparo la imagen a descargar
                var botonImagen = idgrafica.includes("nodes") ? $(this).parent().find('.descargarcyto') : $(this).parent().find('.descargar');
                $('.descargarzoom').unbind().click(function (e) {
                    botonImagen.click();
                });
                var botonCSV = $(this).parent().find('.csv');
                $('.csvzoom').unbind().click(function (e) {
                    botonCSV.click();
                });
                // Preparo los modales
                var botonGuardar = $(this).parent().find('.guardar');
                $('.guardarzoom').unbind().click(function (e) {
                    cerrarModal();
                    botonGuardar.click();
                });
                var botonEditar = $(this).parent().find('.editargrafica');
                $('.editargraficazoom').unbind().click(function (e) {
                    cerrarModal();
                    botonEditar.click();
                });
                var botonEliminar = $(this).parent().find('.eliminargrafica');
                $('.eliminargraficazoom').unbind().click(function (e) {
                    cerrarModal();
                    botonEliminar.click();
                });

                if (idgrafica.includes("nodes")) {
                    ctx = $(`<div class="graficoNodos" id="grafica_${idPaginaActual}_${pIdGrafica}" style=" height:${$(modalContent).height() - 130}px;"></div>`)
                    parent.append(`
                            <p id="grafica_${idPaginaActual}_${pIdGrafica}" style="text-align:center; width: 100%; font-weight: bold; color: #6F6F6F; font-size: 0.90em;"></p>
                            <div class="graph-controls">
                                <ul class="no-list-style align-items-center">
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

                    if (idgrafica.includes("circular")) {
                        var chartWrapper;
                        parent.append(`
                            <div class="chartAreaWrapper">
                            </div>`);
                        chartWrapper = parent.find('.chartAreaWrapper');
                        chartWrapper.css({ height: '100%' });
                        chartWrapper.css({ width: $(modalContent).height() - 200 });
                        chartWrapper.parent().css({ display: 'flex', flexDirection: 'column', alignItems: 'center' });

                    } else {
                        modalContent.css({ display: 'none' });
                        parent.append(`
                            <div class="chartWrapper">
                                <div class="chartScroll custom-css-scroll" style="height:${$(modalContent).height() - 130}px;">
                                    <div  class="chartAreaWrapper" >
                                    </div>
                                </div>
                            </div>
                        
                            `);
                    }
                    parent.find('div.chartAreaWrapper').append(ctx);
                }
                var filtro;
                var idPagina;
                if (!$('div').hasClass('indicadoresPersonalizados')) {
                    filtro = ObtenerHash2();
                    idPagina = idPaginaActual;
                } else {
                    filtro = (canvas).parents('div.grafica').attr("filtro");
                    idPagina = (canvas).parents('div.grafica').attr("idpagina");
                }
                $('#grafica_' + idPagina + '_' + pIdGrafica).attr('filtro', filtro);

                //obtenemos los datos y pintamos la grafica

                if (!$('div').hasClass('indicadoresPersonalizados')) {
                    that.getGrafica(idPagina, pIdGrafica, filtro, ctx[0], 50);
                } else {
                    //Obtengo el título de la gráfica
                    idGraficaActual = $(this).closest('article').find("div.show.grafica").attr("idrecurso");
                    var url = url_servicio_graphicengine + "GetGraficasUser"; //"https://localhost:44352/GetGraficasUser"
                    var arg = {};
                    arg.pPageId = idPaginaActual;
                    $.get(url, arg, function (listaData) {
                        listaData.forEach(data => {
                            if (data.idRecurso == idGraficaActual) {
                                tituloActual = data.titulo;
                            }
                        });

                        that.getGrafica(idPagina, pIdGrafica, filtro, ctx[0], 50, null, tituloActual)
                    });
                }
            });

        $('.modal-backdrop')
            .unbind()
            .click(cerrarModal);
        $('span.cerrar-grafica')
            .unbind()
            .click(cerrarModal);

        function cerrarModal() {
            $('#modal-ampliar-mapa').find('div.graph-container').empty();
            $('#modal-ampliar-mapa').removeClass('show');
            $('.modal-backdrop').removeClass('show');
            $('.modal-backdrop').css('pointer-events', 'none');
            $('#modal-ampliar-mapa').css('display', 'none');
            $('#modal-agregar-datos').removeClass('show');
            $('.modal-backdrop').removeClass('show');
            $('.modal-backdrop').css('pointer-events', 'none');
            $('#modal-agregar-datos').css('display', 'none');
        }

        $(".listadoMenuPaginas li.nav-item")
            .unbind()
            .click(function (e) {
                var numero = $(this).attr("num");
                if (!$('div').hasClass('indicadoresPersonalizados')) {
                    $(this).parents('ul').find('a.active').removeClass('active');
                    $(this).find('a').addClass('active');
                    metricas.clearPage();
                    metricas.createEmptyPage(listaPaginas[numero].id);
                    metricas.fillPage(listaPaginas[numero]);
                } else {
                    $(this).parents('ul').find('a.active').removeClass('active');
                    $(this).find('a').addClass('active');
                    metricas.clearPage();
                    metricas.createEmptyPagePersonalized(listaPaginas[numero].idRecurso.split('/')[listaPaginas[numero].idRecurso.split('/').length - 1]);
                    metricas.fillPagePersonalized(listaPaginas[numero]);
                }
            });

        plegarSubFacetas.init();
        comportamientoFacetasPopUp.init();

        // Agrega el enganche sin sobreescribir la función.
        $('#panFacetas .open-popup-link-tesauro').unbind('.clicktesauro').bind("click.clicktesauro", (function (event) {
            that.engancharComportamientos();
        }));


        /*$('#panFacetas .open-popup-link').unbind();*/
        $('#panFacetas .open-popup-link-resultados').unbind().click(function (event) {
            $('#modal-resultados').show();
            $(".indice-lista.no-letra").html('');
            event.preventDefault();
            $('#modal-resultados .modal-dialog .modal-content .modal-title').text($($(this).closest('.box')).find('.faceta-title').text());
            comportamientoFacetasPopUp.cargarFaceta($(this).closest('.box').attr('idfaceta'));
            that.engancharComportamientos();
        });
    }
}

comportamientoFacetasPopUp.cargarFaceta = function (pIdFaceta) {
    var that = this;
    var url = url_servicio_graphicengine + "GetFaceta"; //"https://localhost:44352/GetFaceta"
    var arg = {};

    arg.pIdPagina = idPaginaActual;
    arg.pIdFaceta = pIdFaceta;
    arg.pFiltroFacetas = ObtenerHash2();
    arg.pLang = lang;
    arg.pGetAll = true;
    that.textoActual = '';
    that.paginaActual = 1;
    // Petición para obtener los datos de las gráficas.
    $.get(url, arg, function (data) {

        $('.buscador-coleccion .buscar .texto').keyup(function () {
            that.textoActual = that.eliminarAcentos($(this).val());
            that.paginaActual = 1;
            that.buscarFacetas();
            $('.indice-lista .faceta')
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
                    } else {
                        location.reload();
                    }

                    history.pushState('', 'New URL: ' + filtros, '?' + filtros);
                    e.preventDefault();

                    metricas.pintarPagina(idPaginaActual);
                });
        });

        that.arrayTotales = new Array(data.items.length);
        var i = 0;
        data.items.forEach(function (item, index, array) {
            that.arrayTotales[i] = new Array(2);
            that.arrayTotales[i][0] = that.eliminarAcentos(item.nombre.toLowerCase());
            that.arrayTotales[i][1] = $(`<a href="javascript: void(0);" class="faceta filtroMetrica" filtro="${item.filtro}">
                                <span class="textoFaceta">${item.nombre}</span>
                                <span class="num-resultados">(5)</span>
                            </a>`);
            i++;
        });

        //Ordena por orden alfabético
        that.arrayTotales = that.arrayTotales.sort(function (a, b) {
            if (a[0] > b[0]) return 1;
            if (a[0] < b[0]) return -1;
            return 0;
        });

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
                        $('.indice-lista .faceta')
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
                                } else {
                                    location.reload();
                                }

                                history.pushState('', 'New URL: ' + filtros, '?' + filtros);
                                e.preventDefault();

                                metricas.pintarPagina(idPaginaActual);
                            });
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
                        $('.indice-lista .faceta')
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
                                } else {
                                    location.reload();
                                }

                                history.pushState('', 'New URL: ' + filtros, '?' + filtros);
                                e.preventDefault();

                                metricas.pintarPagina(idPaginaActual);
                            });
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
        $('.indice-lista .faceta')
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
                } else {
                    location.reload();
                }

                history.pushState('', 'New URL: ' + filtros, '?' + filtros);
                e.preventDefault();

                metricas.pintarPagina(idPaginaActual);
            });
    });
};