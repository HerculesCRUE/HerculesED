$(document).ready(function () {
    metricas.init();
});
// Año máximo y mínimo para las facetas de años
var minYear;
var maxYear;
// Página actual.
var idPaginaActual = "";
var tituloPaginaActual;
var ordenPaginaActual;
// Gráfica seleccionada.
var idGraficaActual = "";
var tituloActual;
var tamanioActual;
var ordenActual;
var escalaActual;

// Lista de páginas.
var listaPaginas;
var numPagina = 0;
const { jsPDF } = window.jspdf;
String.prototype.width = function (font) {
    var f = font || '12px arial',
        o = $('<div></div>')
            .text(this)
            .css({ 'position': 'absolute', 'float': 'left', 'white-space': 'nowrap', 'visibility': 'hidden', 'font': f })
            .appendTo($('body')),
        w = o.width();

    o.remove();

    return w;
}
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
            //this.engancharComportamientoAdmin();
        } else {
            this.getPagesPersonalized();
        }
        //Estilos 
        $('.block').addClass('no-cms-style');
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
        arg.userId = $('.inpt_usuarioID').attr('value');
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
    engancharComportamientoAdmin: function () {
        var that = this;
        var url = url_servicio_graphicengine + "IsAdmin"; //"https://localhost:44352/IsAdmin";
        var arg = {};
        arg.pLang = lang;
        arg.pUserId = $('.inpt_usuarioID').attr('value');

        var isAdmin = false;
        $.get(url, arg, function (data) {
            isAdmin = data;
        }).done(
            function () {
                if (isAdmin) {
                    $(".acciones-mapa .dropdown ul").append(`
                        <li>
                            <a class="item-dropdown admin-config">
                                <span class="material-icons">settings</span>
                                <span class="text">Configuración</span>
                            </a>
                        </li>
                    `);
                    if ($(".pageOptions .admin-page").length == 0) {
                        $('.pageOptions').append(`
                        <div class="admin-page">
                            <span class="material-icons btn-download-page">manage_accounts</span>
                            <p>${that.GetText("ADMINISTRAR_GRAFICAS")}</p>
                        </div>
                        `);
                    }
                    $('div.admin-page')
                        .unbind()
                        .click(function (e) {
                            that.clearPage();
                            that.createAdminPage();
                        });
                }
            }
        );
    },
    createAdminPage: function () {
        var that = this;
        var url = url_servicio_graphicengine + "ObtenerConfigs"; //"https://localhost:44352/ObtenerConfigs";
        var arg = {};
        arg.pLang = lang;
        arg.pUserId = $('.inpt_usuarioID').attr('value');
        if ($("div.pageMetrics table.tablaAdmin").length == 0) {
            $("main div.row-content div.pageMetrics").append(`
            <table class="tablaAdmin">
                <tbody>
                    <tr>
                        <th>Fichero</th>
                     
                        <th>Subir</th>
                    </tr>
                </tbody>
            </table>
        `);
            $.get(url, arg, function (data) {
                data.forEach(function (jsonName, index) {
                    jsonName = jsonName.substring(1);
                    var link = url_servicio_graphicengine + "DescargarConfig";
                    link += "?pLang=" + lang;
                    link += "&pConfig=" + jsonName;
                    link += "&pUserId=" + $('.inpt_usuarioID').attr('value');
                    $("table.tablaAdmin tbody").append(`
                <tr id="${index}">
                    <td><a href="${link}" id="jsonName">${jsonName}</a></td> 
                    <td class="subir">
                        <input type="file">
                        <a class="btn btn-primary subir">Subir</a>   
                    </td>

                </tr>
            `);
                }
                );
                metricas.engancharComportamientos();
                $(`div.download-page`).unbind();
                // change color
                $('.admin-page').css("color", "var(--c-primario)");
                $('a.nav-link.active').removeClass('active');
            });
        }
    },
    getPagesPersonalized: function () {
        var that = this;
        var url = url_servicio_graphicengine + "GetPaginasUsuario"; //"https://localhost:44352/GetPaginasUsuario"  
        var arg = {};
        arg.pUserId = $('.inpt_usuarioID').attr('value');
        if (arg.pUserId == "ffffffff-ffff-ffff-ffff-ffffffffffff") { // Sin usuario
            $('div.row-content').append(`
                <div class="container">
                    <div class="row-content">
                        <div class="row">
                            <div class="col">
                                <div class="form panel-centrado">
                                    <h1>${metricas.GetText("NO_HAS_INICIADO_SESION")}</h1>
                                    <p>${metricas.GetText("DESCRIPCION_NO_HAS_INICIADO_SESION")}</p>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            `);
        } else {
            // Petición para obtener los datos de la página.
            $.get(url, arg, function (listaData) {
                if (listaData.length == 0) {
                    $('div.row-content').append(`
                        <div class="container">
                            <div class="row-content">
                                <div class="row">
                                    <div class="col">
                                        <div class="form panel-centrado">
                                            <h1>${metricas.GetText("NO_TIENES_INDICADORES")}</h1>
                                            <p>${metricas.GetText("COMIENZA_A_CREAR_INDICADORES")}</p>
                                            <p><a href="${metricas.GetText("URL_INDICADORES")}">${metricas.GetText("LA_PAGINA_DE_INDICADORES")}</a>.</p>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    `);
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

            if ($("div[idFaceta='" + pIdFaceta + "']").find("*").length > 0) {
                return;
            }


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
                                <span class="textoFaceta">${item.nombre == 'true' ? 'Sí' : (item.nombre == 'false' ? 'No' : item.nombre)}</span>
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
            $('table.tablaAdmin').remove();
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
        metricas.engancharComportamientoAdmin();
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

                tmp += `<div order="${index}" class="${index == 0 ? "show" : "hide"} grafica" style="opacity:${index != 0 ? "0; position:absolute;top:-9999px;left:-9999px;z-index:-1" : "1"};" tipoGrafica="${tipoGrafica}" idgrafica='${grafica.id}'></div>`;
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


        if ($("div.facetedSearch").length == 0) {
            pPageData.listaIdsFacetas.forEach(function (item, index, array) {

                $('#page_' + pPageData.id + ' .containerFacetas').append(`
                    <div class='facetedSearch'>
                        <div class='box' idfaceta="${item.includes('(((') ? item.split('(((')[0] : item}" reciproca="${item.includes('(((') ? item.split('(((')[1] : ''}"></div>
                        </div>
                    `);

            });
        }
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
            //remove _filter 

            listaData.forEach(function (item, index, array) {
                if (item.filtro) {
                    item.filtro = item.filtro.replace('_filter', '');
                }
            });

            if (listaData.length == 0) {
                if ($('div.row-content').find('div.sin-graficas').length == 0) {
                    $('div.row-content').append(`
                    <div class="sin-graficas">
                        <div class="container">
                            <div class="row-content">
                                <div class="row">
                                    <div class="col">
                                        <div class="form panel-centrado">
                                            <h1>${metricas.GetText("NO_HAY_GRAFICAS")}</h1>
                                            <p>${metricas.GetText("PUEDES")} <a href="#" onclick="$('.delete-page').click()">${metricas.GetText("BORRAR_LA_PAGINA")}</a> ${metricas.GetText("O")} <a href="${metricas.GetText("URL_INDICADORES")}">${metricas.GetText("ANIADIR_NUEVAS_GRAFICAS")}</a>.</p>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    `);
                } metricas.engancharComportamientos();
            } else {
                $('div.row-content').find('div.sin-graficas').remove();
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
            //that.pintarContenedoresGraficas(this, pIdPagina);
            getGrafica(pIdPagina, $(this).attr("idgrafica"), ObtenerHash2(), this);
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
                    <span>${nombre == 'true' ? 'Sí' : (nombre == 'false' ? 'No' : (nombre.includes('(((') ? nombre.split('(((')[0] : nombre))}</span>
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
                    nombre = that.GetText("ULTIMO_ANIO");
                } else if (nombre === "fiveyears") {
                    nombre = that.GetText("ULTIMOS_CINCO_ANIOS");
                }
                $(".borrarFiltros-wrap").remove();
                $("#panListadoFiltros").append(`
                <li class="Categoria" filtro="${filtro}">
                    <span>${nombre == 'true' ? 'Sí' : (nombre == 'false' ? 'No' : (nombre.includes('(((') ? nombre.split('(((')[0] : nombre))}</span>
                    <a rel="nofollow" class="remove faceta" name="search=Categoria" href="javascript:;">eliminar</a>
                </li>
                <li class="borrarFiltros-wrap">
                    <a class="borrarFiltros" href="javascript:;">Borrar</a>
                </li>
                `);
            }
        }
    },
    GetText: function (id, param1, param2, param3, param4) {
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
            /*if ($(this).attr("idgrafica").includes("nodes")) {
                $(this).append(`
                        <p id="titulo_grafica_${pPageData[index].idRecurso}" style="text-align:center; margin-top: 0.60em; width: 100%; font-weight: 500; color: #666666; font-size: 0.87em;"></p>
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
                    <div class="chartScroll custom-css-scroll " style="overflow-${pPageData[index].idGrafica.includes("isHorizontal") ? "y" : "x"}: scroll;height:${318}px;">
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
            }*/

            getGrafica(pPageData[index].idPagina, pPageData[index].idGrafica, pPageData[index].filtro, this, 50, pPageData[index].idRecurso, pPageData[index].titulo, pPageData[index].escalas, pPageData[index]);

            index++;
        });
        metricas.engancharComportamientos();
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
    /*drawChart: function (ctx, data, pIdGrafica = null, barSize = 100, pTitulo = null) {
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

        var barCount = 0;
        var stacks = []
        //Obtenemos el numero de barras que tendra la grafica por cada dataset
        data.data.datasets.forEach(function (dataset) {
            if (dataset.type == "bar") {
                if (dataset.stack != null) {
                    if (stacks.indexOf(dataset.stack) == -1) {
                        stacks.push(dataset.stack);
                        barCount++;
                    }
                } else {
                    barCount++;
                }
            }
        })
        if (barCount == 0) {
            barCount = 1;
        }

        barSize /= barCount;

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
        if ((canvasSize < (modalBody.length != 0 ? $(window).height() - 230 : 318) && horizontal) || (canvasSize < (modalBody.length != 0 ? 1110 : $(graficaContainer).width()) && !horizontal)) {
            if (barSize < 100) {
                $(ctx).parents(".modal-content").css("display", "block");
            }
            graficaContainer.classList.add("small");
            if (modalBody.length == 0) {
                chartAreaWrapper.style.height = 318 + "px";
            } else {
                chartAreaWrapper.style.height = "100%";
            }
            scrollContainer.style.overflow = "hidden";



            var myChart = new Chart(ctx, data);
        } else { // a partir de aqui se prepara el scroll

            //Se revela el modal de zoom
            $(ctx).parents(".modal-content").css("display", "block");

            var hasMainAxis = false; //eje superior en caso horizontal, izquierdo en vertical
            var hasSecondaryAxis = false; // eje inferior o derecho

            if (horizontal) {
                ctx.parentNode.style.height = canvasSize + 'px'; //se establece la altura del eje falso
            } else {// -- vertical
                var parent = $(ctx).parents(".wrap")[0] || $(ctx).parents(".modal-content")[0];
                if ($(parent).find(".acciones-mapa .expand").length == 0) {
                    $(parent).find(".acciones-mapa .wrap").prepend(`
                    <div class="expand">
                        <a href="javascript: void(0);">
                            <span class="material-icons">close_fullscreen</span>
                        </a>
                    </div>
                `)
                }
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


            copyHeight = myChart.chartArea.bottom + 5;
            //targetY = 20; //posicion del eje
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
                var padding = -1 * (myChart.boxes[2].width - myChart.boxes[2].left - myChart.boxes[2].right);
                ctx.canvas.height = axisHeight;
                targetY = myChart.chartArea.bottom * scale;
                ctx.canvas.style.paddingLeft = myChart.chartArea.left - 5 + "px";
                copyWidth += padding - 1;
                targetX = (myChart.chartArea.left - 5) * scale;

            } else {
                ctx.canvas.height = copyHeight;
                //copyWidth += 12;
                copyWidth = myChart.width - myChart.chartArea.right;
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

    },*/
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
                                <span class="textoFaceta">${pData.nombre == 'true' ? 'Sí' : (pData.nombre == 'false' ? 'No' : pData.nombre)}</span>
                                <span class="num-resultados">(${pData.numero})</span>                          
                            </a>
                            ${etiqueta}
                        </li>`;

            } else {

                return `<li>
                            <a rel="nofollow" href="javascript: void(0);" class="faceta filtroMetrica ocultarSubFaceta ocultarSubFaceta" filtro="${pData.filtro}" title="${pData.nombre}">
                                <span class="textoFaceta">${pData.nombre == 'true' ? 'Sí' : (pData.nombre == 'false' ? 'No' : pData.nombre)}</span>
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
                if ($(this).parents('.box').attr('reciproca')) {
                    filtroActual = filtroActual + '(((' + $(this).parents('.box').attr('reciproca');
                }
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

                var numPagina = $(".nav-item#" + idPaginaActual).attr("num");
                history.pushState('', 'New URL: ' + filtros, '?' + filtros + '~~~' + numPagina);
                e.preventDefault();

                location.reload();
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
        /*$('div.labelContainer')
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
                    chart.draw();
                }
            });*/

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
        $('table.tablaAdmin td.subir a.btn.subir')
            .unbind()
            .click(function (e) {
                var url = url_servicio_graphicengine + "SubirConfig";
                var formData = new FormData();
                formData.append('pConfigName', $(this).closest('tr').find('a#jsonName').text());
                formData.append('pUserID', $('.inpt_usuarioID').attr('value'));
                formData.append('pLang', lang);
                formData.append('pConfigFile', $(this).parent().find('input[type=file]')[0].files[0]);

                $.ajax({
                    url: url,
                    type: 'POST',
                    data: formData,
                    cache: false,
                    processData: false,
                    enctype: 'multipart/form-data',
                    contentType: false,
                    success: function (response) {
                        if (response) {
                            mostrarNotificacion('success', 'Configuración subida correctamente');
                            location.reload();
                        } else {
                            mostrarNotificacion('error', 'Error al subir la configuración');
                        }
                    },
                    error: function (response) {
                        mostrarNotificacion('error', 'Error al subir la configuración');
                    }

                });
            });
        $('table.tablaAdmin a.btn.editar')
            .unbind()
            .click(function (e) {
                $('#modal-editar-configuracion').css('display', 'block');
                $('#modal-editar-configuracion').css('pointer-events', 'none');
                $('.modal-backdrop').addClass('show');
                $('.modal-backdrop').css('pointer-events', 'auto');
                $('#modal-editar-configuracion').addClass('show');
                var url = url_servicio_graphicengine + "ObtenerPaginaConfig";
                var arg = {};

                arg.pUserId = $('.inpt_usuarioID').attr('value');
                //arg.pageID = $(this).closest('tr').id;
                arg.pConfig = $(this).closest('tr').find('a#jsonName').text();
                arg.pLang = lang;
                $.get(url, arg, function (data) {
                    $('#labelTituloPaginaEditar').val(data.nombre);
                    var numPaginas = $(".tablaAdmin").find('tr').length - 1;
                    $('#idSelectorOrdenPagina').empty();
                    for (var i = 0; i < numPaginas; i++) {
                        $('#idSelectorOrdenPagina').append(`
                            <option value="${i + 1}">${i + 1}</option>
                        `)
                    }

                    $('#idSelectorOrdenPagina').val(data.orden).change();

                });
                url = url_servicio_graphicengine + "ObtenerGraficasConfig";

                $.get(url, arg, function (data) {
                    data.forEach(function (grafica, index, array) {
                        $("#modal-editar-configuracion").find('.modal-body').append(`
                        <div class="custom-form-row">
                            <div class="simple-collapse">
                                <a class="collapse-toggle collapsed" data-toggle="collapse" href="#collapse-${index}">grafica ${index}</a>
                                <div id="collapse-${index}" class="collapse">
                                    <div class="form-group full-group disabled ">
                                        <label class="control-label d-block"></label>
                                        <input id="labelTituloGrafica" onfocus="" type="text" class="form-control not-outline">
                                    </div>
                                    <div class="form-group full-group disabled">
                                        <label class="control-label d-block">Anchura</label>
                                        <select id="idSelectorTamanyoEditar" class="js-select2 select2-hidden-accessible" dependency="" data-select-search="true" tabindex="-1" aria-hidden="true">
                                            <option value="11">100%</option>
                                            <option value="34">75%</option>
                                            <option value="23">66%</option>
                                            <option value="12">50%</option>
                                            <option value="13">33%</option>
                                            <option value="14">25%</option>
                                        </select>
                                    </div>
                                    <div class="form-group full-group disabled">
                                        <label class="control-label d-block">Orden</label>
                                        <select id="idSelectorOrden" class="js-select2 select2-hidden-accessible" dependency="" data-select-search="true" tabindex="-1" aria-hidden="true">
                                        </select>
                                    </div>
                                </div>
                            </div>
                        </div>`);

                        $("collapse" + index).find('#labelTituloGrafica').val(grafica.options.plugins.title);
                        var numGraficas = data.length;
                        for (var i = 0; i < numGraficas; i++) {
                            $("collapse-${index}").find('#idSelectorOrden').append(`
                            <option value="${i + 1}">${i + 1}</option>
                        `)
                        }
                        $("collapse-${index}").find('#idSelectorOrden').val(index).change();

                    });


                });

                var orden = "yo que se añadir luego ";
                $('#idSelectorOrdenPagina').empty().append(`
                    <option value="${orden}">${orden}</option>    
                `)
                $('#idSelectorOrden').empty().append(`
                    <option value="${orden}">${orden}</option>    
                `)
            });
        $('a.csv')
            .unbind()
            .click(function (e) {
                var url = url_servicio_graphicengine + "GetCSVGrafica";
                if (!$('div').hasClass('indicadoresPersonalizados')) {
                    url += "?pIdPagina=" + $(this).closest('div.row.containerPage.pageMetrics').attr('id').substring(5);
                    url += "&pIdGrafica=" + $(this).parents('div.wrap').find('div.grafica.show').attr('idgrafica');
                    url += "&pFiltroFacetas=" + encodeURIComponent(ObtenerHash2());
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
                $("#labelEscalaGrafica").val("");
                $("#labelEscalaSecundariaGrafica").val("");

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
                            escalaActual = data.escalas;
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
                    if (!escalaActual) {
                        $("#escalaPrimaria").hide();
                        $("#escalaSecundaria").hide();
                    } else if (escalaActual.includes(',')) {
                        $("#escalaPrimaria").show();
                        $("#escalaSecundaria").show();
                        $("#labelEscalaGrafica").val(escalaActual.split(',')[0]);
                        $("#labelEscalaSecundariaGrafica").val(escalaActual.split(',')[1]);
                    } else {
                        $("#escalaPrimaria").show();
                        $("#escalaSecundaria").hide();
                        $("#labelEscalaGrafica").val(escalaActual);
                    }
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
        $(`div.download-page`)
            .unbind()
            .click(function (e) {

                MostrarUpdateProgress();
                $(".acciones-mapa").hide();
                $(".Categoria a").hide();
                $(".borrarFiltros-wrap").hide();
                var htmlsource = $("div.col-contenido ")[0] || $("div.resource-list-wrap")[0];
                html2canvas(htmlsource, { scale: 2, scrolly: -window.scrolly }).then(function (canvas) {
                    var img = canvas.toDataURL();
                    var orientation = canvas.height > canvas.width ? 'portrait' : 'landscape';
                    var doc = new jsPDF(orientation, "mm", [(canvas.height + 200) / 4, (canvas.width + 200) / 4]);
                    doc.addImage(img, 'jpeg', 25, 25, canvas.width / 4, canvas.height / 4);
                    doc.save('Indicadores.pdf');
                    OcultarUpdateProgress();
                    $(".acciones-mapa").show();
                    $(".Categoria a").show();
                    $(".borrarFiltros-wrap").show();
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
                MostrarUpdateProgress();
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

                MostrarUpdateProgress();
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
        $('a.admin-config').unbind().click(function (e) {
            $('#modal-admin-config').css('display', 'block');
            $('#modal-admin-config').css('pointer-events', 'none');
            $('.modal-backdrop').addClass('show');
            $('.modal-backdrop').css('pointer-events', 'auto');
            $('#modal-admin-config').addClass('show');

            var url = url_servicio_graphicengine + "ObtenerGraficaConfig"; //"https://localhost:44352/GetConfiguracion"
            idGraficaActual = $(this).closest('article').find("div.show.grafica").attr("idgrafica");
            var args = {}
            args.pLang = lang;
            args.pUserId = $('.inpt_usuarioID').attr('value');
            args.pGraphicId = idGraficaActual;
            args.pPageID = idPaginaActual;
            var numGraficas = $(this).closest('article').parent().find('article').length;
            $("#modal-admin-config #idSelectorOrden").empty();
            for (var i = 0; i < numGraficas; i++) {
                $("#modal-admin-config #idSelectorOrden").append(`
                    <option value="${i + 1}">${i + 1}</option>
                `)
            }
            var parent = $(this).parents('article');
            var index = parent.index();
            $("#idSelectorOrden").val(index + 1).change();
            $.get(url, args, function (listaData) {
                $("#modal-admin-config #labelTituloGraficaConfig").val(listaData.nombre[lang]);
                $("#modal-admin-config #idSelectorTamanyoConfig").val(listaData.anchura).change();

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
                            var urlScal = url_servicio_graphicengine + "EditarEscalasGrafica"; //"https://localhost:44352/EditarEscalasGrafica"
                            var argScal = {};
                            argScal.pUserId = user;
                            argScal.pPageID = idPaginaActual;
                            argScal.pGraphicID = idGraficaActual;
                            var escalas = $('#labelEscalaGrafica').val() == '' ? "" : $('#labelEscalaSecundariaGrafica').val() == '' ? parseInt($('#labelEscalaGrafica').val()) : parseInt($('#labelEscalaGrafica').val()) + "," + parseInt($('#labelEscalaSecundariaGrafica').val());
                            argScal.pNewScales = escalas;
                            argScal.pOldScales = escalaActual;

                            $.get(urlScal, argScal, function () {
                                location.reload();
                            });
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

                var grafica = $("#grafica_" + idPaginaActual + "_" + idGraficaActual);
                var chart = Chart.getChart(grafica);
                var scales = chart.scales;
                var isHorizontal = grafica.parents('div.grafica').hasClass('horizontal');
                var max = [];
                /*scales.each(function (scale) {
                    if (scale.axis == 'y' && !isHorizontal) {
                        max.push(scale.max);
                    } else if (scale.axis == 'x' && isHorizontal) {
                        max.push(scale.max);
                    }
                });*/

                for (scale in scales) {
                    if (scales[scale].axis == 'y' && !isHorizontal) {
                        max.push(scales[scale].max);
                    } else if (scales[scale].axis == 'x' && isHorizontal) {
                        max.push(scales[scale].max);
                    }
                }
                var escalas = max[0];
                if (max.length > 1) {
                    escalas = max[0] + "," + max[1];
                }


                var url = url_servicio_graphicengine + "GuardarGrafica"; //"https://localhost:44352/GuardarGrafica"
                var arg = {};
                arg.pTitulo = $('#labelTituloGrafica').val();
                arg.pAnchura = $('#idSelectorTamanyo option:selected').val();
                arg.pIdPaginaGrafica = idPaginaActual;
                arg.pIdGrafica = idGraficaActual;
                arg.pFiltros = ObtenerHash2();
                arg.pUserId = $('.inpt_usuarioID').attr('value');
                arg.pEscalas = escalas;

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
                MostrarUpdateProgress();
                $.get(url, arg, function (data) {
                    if (data) {
                        mostrarNotificacion("success", "Grafica guardada correctamente");
                    } else {
                        mostrarNotificacion("error", "Error al guardar la grafica");
                    }
                }).done(function () {
                    OcultarUpdateProgress();
                    cerrarModal();
                });
            });
        $("#btnGuardarGraficaConfig")
            .unbind()
            .click(function (e) {
                var url = url_servicio_graphicengine + "EditarConfig"; //"https://localhost:44352/GuardarGraficaConfig"
                var arg = {};
                arg.pUserId = $('.inpt_usuarioID').attr('value');
                arg.pPageId = idPaginaActual;
                arg.pGraphicId = idGraficaActual;
                arg.pLang = lang
                arg.pGraphicName = $('#labelTituloGraficaConfig').val();
                arg.pGraphicWidth = $('#idSelectorTamanyoConfig option:selected').val();
                arg.pGraphicOrder = $('#idSelectorOrden').val();
                $.get(url, arg, function (data) {
                    if (data) {
                        mostrarNotificacion("success", "Grafica guardada correctamente");
                    } else {
                        mostrarNotificacion("error", "Error al guardar la grafica");
                    }
                    location.reload();

                }).fail(function (data) {
                    mostrarNotificacion("error", "Error de servidor al guardar la grafica");
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
                shown.css('opacity', '0'); // display none csa problemas con redrawChart por que intenta modifica un elemento sin altura
                shown.css('position', 'absolute');
                // Muevo el div fuera de la página para que no cree espacios en blanco
                shown.css('left', '-9999px');
                shown.css('top', '-9999px');
                shown.css('z-index', '-1');

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
                    // Lo muevo de vuelta
                    selected.css('left', '0px');
                    selected.css('top', '0px');
                    selected.css('z-index', '1');

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
                shown = parent.find('div.grafica.show');
                if (shown.find(".chartScroll").hasClass("collapsed")) {
                    parent.find(".expand span").text("open_in_full");
                    parent.find(".expand").addClass("collapsed");
                } else {
                    parent.find(".expand span").text("close_fullscreen");
                    parent.find(".expand").removeClass("collapsed");

                }

            });


        $("div.expand")
            .unbind()
            .click(function (e) {
                var parent = $(this).parents('article > div.wrap')[0] || $(this).parents('.modal-body > .graph-container')[0];
                parent = $(parent);
                var canvas = parent.find('.show.grafica .chartAreaWrapper canvas')[0] || parent.find('.chartAreaWrapper canvas')[0];
                canvas = $(canvas);
                var myChart = Chart.getChart(canvas[0]);
                var data = myChart.data;
                var config = myChart.config;

                var idGrafica = canvas.attr('id').split('_')[2];
                var idPagina = canvas.attr('id').split('_')[1];
                if ($('div').hasClass('indicadoresPersonalizados')) {
                    parent = $(this).parents('article > div.wrap');
                    if (parent.length != 0) {
                        idGrafica = parent.find("div.grafica").attr("idGrafica");
                        idPagina = parent.find("div.grafica").attr("idPagina");
                        idGraficaActual = idGrafica;
                        idPaginaActual = idPagina;

                    } else {
                        idPagina = idPaginaActual;
                        idGrafica = idGraficaActual;
                    }
                }




                //plugin para que el color de fondo sea blanco.
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
                var isExpanded = !$(this).hasClass('collapsed');
                $(this).toggleClass('collapsed');
                if (isExpanded) {
                    $(this).find("span").text("open_in_full");

                    data.datasets.forEach(function (dataset) {
                        delete dataset['barThickness'];
                    });

                    //Destruyo el chart para que se redibuje con el nuevo tamaño
                    myChart.destroy();
                    canvas.parents(".chartScroll").addClass('collapsed');
                    canvas.parent().css('width', 'auto');
                    var chartWrapper = canvas.parents(".chartWrapper");
                    //Elimino la leyenda y los ejes
                    chartWrapper.find(".myChartAxis, .chartLegend").remove();
                    //Elimino el callback que llama a reDrawChart
                    delete config.options.animation['onProgress'];

                    //Remake the chart with the data obtained from the previous chart
                    var newChart = new Chart(canvas[0].getContext('2d'), {
                        type: 'bar',
                        data: data,
                        options: config.options,
                        plugins: [plugin]
                    });
                    getGrafica(idPagina, idGrafica, ObtenerHash2(), null, 50)
                    idGraficaActual = idGrafica;
                    idPaginaActual = idPagina;
                } else {
                    // cambio el icono
                    $(this).find("span").text("close_fullscreen");
                    canvas.parents(".chartScroll").removeClass('collapsed');
                    myChart.destroy();


                    var filtro;
                    var idPagina;
                    if (!$('div').hasClass('indicadoresPersonalizados')) {
                        filtro = ObtenerHash2();
                        idPagina = idPaginaActual;
                    } else {
                        filtro = (canvas).parents('div.grafica.show').attr("filtro");
                        idPagina = (canvas).parents('div.grafica.show').attr("idpagina") || idPaginaActual;
                        idGrafica = idGraficaActual;
                    }
                    $('#grafica_' + idPagina + '_' + idGrafica).attr('filtro', filtro);

                    //obtenemos los datos y pintamos la grafica

                    if (!$('div').hasClass('indicadoresPersonalizados')) {
                        getGrafica(idPagina, idGrafica, filtro, canvas[0], 50);
                    } else {
                        //idGraficaActual = $(this).closest('article').find("div.show.grafica").attr("idrecurso");
                        var url = url_servicio_graphicengine + "GetGraficasUser"; //"https://localhost:44352/GetGraficasUser"
                        var arg = {};
                        arg.pPageId = idPaginaActual;
                        $.get(url, arg, function (listaData) {
                            listaData.forEach(data => {
                                if (data.idRecurso == idGraficaActual) {
                                    tituloActual = data.titulo;
                                }
                            });

                            getGrafica(idPagina, idGrafica, filtro, canvas[0], 50, null, tituloActual)
                        });
                    }
                }
            });
        //boton del pop-up con la grafica escalada
        $("div.zoom")
            .unbind()
            .click(function (e) {
                if ($("#modal-ampliar-mapa").length == 0) {
                    $(".modal")[0].parent().append(`
                        <div id="modal-ampliar-mapa" class="modal modal-top fade modal-ampliar-mapa" style="pointer-events:none" tabindex="-1" role="dialog">
                            <div class="modal-dialog" style="margin:50px" role="document">
                                <div class="modal-content">
                                    <div class="modal-header">
                                        <p class="modal-title"></p>
                                        <span class="material-icons cerrar cerrar-grafica" aria-label="Close">close</span>
                                    </div>
                                        <div class="modal-body">
                                        <div class="graph-container grafica" style="width:100%;"></div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    `)
                }else{
                    $("#modal-ampliar-mapa").find(".graph-container").addClass("grafica");
                }

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
                } else {/*
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
                </div>`)*/

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
                /*
                if (idgrafica.includes("nodes")) {
                    ctx = $(`<div class="graficoNodos" id="grafica_${idPaginaActual}_${pIdGrafica}" style=" height:${$(modalContent).height() - 130}px;"></div>`)
                    parent.append(`
                            <p id="grafica_${idPaginaActual}_${pIdGrafica}" style="text-align:center;margin-top: 0.60em; width: 100%; font-weight: 500; color: #666666; font-size: 0.87em;"></p>
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
                        //modalContent.css({ display: 'none' });
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
                */
                ctx = $(`<div class="zoom" style="height:${$(modalContent).height() - 130}px;"></div>`);
                parent.append(ctx);
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
                    getGrafica(idPagina, pIdGrafica, filtro, ctx[0], 50);
                } else {
                    //Obtengo el título de la gráfica
                    idGraficaActual = $(this).closest('article').find("div.show.grafica").attr("idrecurso");
                    var url = url_servicio_graphicengine + "GetGraficasUser"; //"https://localhost:44352/GetGraficasUser"
                    var arg = {};
                    idPaginaActual = $('a.nav-link.active').parent().attr("id");
                    arg.pPageId = idPaginaActual;
                    $.get(url, arg, function (listaData) {
                        listaData.forEach(data => {
                            if (data.idRecurso == idGraficaActual) {
                                tituloActual = data.titulo;
                            }
                        });
                        getGrafica(idPagina, pIdGrafica, filtro, ctx[0], 50, null, tituloActual)

                    });
                }
                //metricas.engancharComportamientos();
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
            $('#modal-agregar-datos').css('display', 'none');
            $('#modal-admin-config').removeClass('show');
            $('#modal-admin-config').css('display', 'none');
        }

        $(".listadoMenuPaginas li.nav-item")
            .unbind()
            .click(function (e) {
                $('.admin-page').removeAttr('style');
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
                                <span class="textoFaceta">${item.nombre == 'true' ? 'Sí' : (item.nombre == 'false' ? 'No' : item.nombre)}</span>
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