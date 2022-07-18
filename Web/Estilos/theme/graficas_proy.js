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
var callbacks = {}


function pintarGraficaIndividual(pContenedor, pIdPagina, idGrafica = "") {
    if (!$(pContenedor).hasClass("grafica")) {
        $(pContenedor).addClass("grafica");
    }
    //pintarContenedoresGraficas(pContenedor, pIdPagina);
    getGrafica(pIdPagina, idGrafica, "", pContenedor);
    callbacks[idGrafica + "_" + pIdPagina] = {
        ctx: pContenedor,
        zoom: addZoomButton.bind(null, pContenedor),
        //downloadcv: addDownloadCvButton.bind(null, pContenedor),
        //downloadjpg: addDownloadJpgButton.bind(null, pContenedor),
    };

}
function pintarContenedoresGraficas(pContenedor, pIdPagina = "", tipografica = "", idgrafica = "") {
    var isZoom = $(pContenedor).parents("#modal-ampliar-mapa").length > 0;
    var ctx;

    if (tipografica.includes("nodes")) {
        $(pContenedor).append(`
                <p id="titulo_grafica_${pIdPagina}_${$(pContenedor).data("idgrafica")}" style="text-align:center; margin-top: 0.60em; width: 100%; font-weight: 500; color: #666666; font-size: 0.87em;"></p>
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
                
            `);
        ctx = $(`<div class="graficoNodos" id="grafica_${pIdPagina}_${idgrafica}" style="height: 100%;"></div>`);
        $(pContenedor).append(ctx);
        $(pContenedor).parent().parent().css("overflow", "hidden");
    } else {
        ctx = $(`<canvas id = "grafica_${pIdPagina}_${idgrafica}" width = "600" height = "250" ></canvas>`)
        if (tipografica.includes("circular")) {
            if (!isZoom) { $(pContenedor).css("height", "90%"); }
            $(pContenedor).append(ctx);
        } else {
            $(pContenedor).append(`
        <div class="chartWrapper" >
            <div class="chartScroll custom-css-scroll">
                <div class="chartAreaWrapper">
                </div>
            </div>
        </div>
        `);
            $(pContenedor).find(".chartAreaWrapper").append(ctx);
        }
    }
    return ctx[0];
}
function pintarContenedoresPersonalizados(pContenedor, pPageData, tipoGrafica) {
    //var isZoom = pContenedor.parents("#modal-ampliar-mapa").length > 0;

    if (tipoGrafica == "nodes") {
        $(pContenedor).append(`
                <p id="titulo_grafica_${pPageData.idRecurso}" style="text-align:center; margin-top: 0.60em; width: 100%; font-weight: 500; color: #666666; font-size: 0.87em;"></p>
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
                <div class="graficoNodos" id="${pPageData.idRecurso}" style="height: 500px;"></div>
            `);
    } else if (tipoGrafica == "circular") {
        $(pContenedor).css("height", "300px");
        $(pContenedor).append(`
            <canvas id = "${pPageData.idRecurso}"></canvas>
                `);
    } else {
        $(pContenedor).append(`
        <div class="chartWrapper" >
            <div class="chartScroll custom-css-scroll " style="overflow-${tipoGrafica == "horizontal" ? "y" : "x"}: scroll;height:${318}px;">
                <div class="chartAreaWrapper">
                    <canvas width = "600" height = "250" id="${pPageData.idRecurso}"></canvas>
                </div>
            </div>
        </div>
        `);
    }
}

function getGrafica(pIdPagina, pIdGrafica, pFiltroFacetas, ctx = null, barSize = 50, pIdRecurso = null, pTitulo = null, maxScales = null, pPageData = null) {
    var that = this;
    var url = url_servicio_graphicengine + "GetGrafica"; //"https://localhost:44352/GetGrafica"
    var arg = {};
    arg.pIdPagina = pIdPagina;
    arg.pIdGrafica = pIdGrafica;
    arg.pFiltroFacetas = pFiltroFacetas;
    arg.pLang = lang; //"es";

    // Petición para obtener los datos de las gráficas.
    $.get(url, arg, function (data) {
        var container = ctx;

        var tipoGrafica = "";
        if (data.isNodes) {
            tipoGrafica = "nodes";
        } else if (data.isVertical) {
            tipoGrafica = "vertical";
        } else if (data.isHorizontal) {
            tipoGrafica = "horizontal";
        } else {
            tipoGrafica = "circular";
        }
        //if ctx is a canvasElement
        if (ctx) {
            if (ctx.tagName == "DIV") {//$(ctx).parents(".grafica").length == 0) { //igual es mejor comprobar si la id es de
                if (pPageData) {
                    ctx = pintarContenedoresPersonalizados(ctx, pPageData, tipoGrafica)
                } else {
                    ctx = pintarContenedoresGraficas(ctx, pIdPagina, tipoGrafica, pIdGrafica);
                    //addZoomButton(tmp);
                }

                // ctx = $("#grafica_" + pIdPagina + "_" + pIdGrafica).last()[0];
            }
        }
        if (!ctx) {
            if (!pIdRecurso) {
                ctx = document.getElementById("grafica_" + pIdPagina + "_" + pIdGrafica);
            } else {
                ctx = document.getElementById(pIdRecurso);
            }
        }
        $(ctx).data("tipoGrafica", tipoGrafica);
        $(ctx).data("idGrafica", pIdGrafica);
        $(ctx).data("idPagina", pIdPagina);

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
            var horizontal = data.options.indexAxis == "y";
            if (maxScales) {

                if (horizontal) {
                    //if contains ","
                    if (maxScales.indexOf(",") > -1) {
                        //parse maxScales to int
                        data.options.scales.x1['max'] = parseInt(maxScales.split(",")[0]);
                        data.options.scales.x2['max'] = parseInt(maxScales.split(",")[1]);
                    } else {
                        data.options.scales.x1['max'] = parseInt(maxScales);
                    }
                } else {
                    if (maxScales.indexOf(",") > -1) {
                        data.options.scales.y1['max'] = parseInt(maxScales.split(",")[0]);
                        data.options.scales.y2['max'] = parseInt(maxScales.split(",")[1]);
                    } else {
                        data.options.scales.y1['max'] = parseInt(maxScales);
                    }
                }
            }

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
            data.options.plugins["legend"] = {
                onHover: (e) => {
                    e.chart.canvas.style.cursor = 'pointer';
                },
                onLeave: (e) => {
                    e.chart.canvas.style.cursor = 'default';
                }
            };

            var fontSize = 12;
            //data.options.plugins.title.text = "AAAA";
            //	font 'Helvetica Neue', 'Helvetica', 'Arial', sans-serif
            function getTextWidth(text, font) {
                // if given, use cached canvas for better performance
                // else, create new canvas
                var canvas = getTextWidth.canvas || (getTextWidth.canvas = document.createElement("canvas"));
                var context = canvas.getContext("2d");
                context.font = font;
                var metrics = context.measureText(text);
                return metrics.width;
            };
            //beforeDraw: function (chart) {
            if ($(ctx).parents("div.grafica").length > 0) {
                while (fontSize > 1 && titulo.width("bold " + fontSize + "px Helvetica") > $(ctx).parents("div.grafica").width() - 200) {
                    fontSize--;
                }
            }

            data.options.plugins.title.font = {
                size: fontSize,
                style: 'bold',
                //family: 'Comic Sans MS, cursive, sans-serif',
            }


            if (pIdGrafica.indexOf("circular") == -1) { //si no es circular

                drawChart(ctx, data, pIdGrafica, barSize, titulo);
            } else { //Graficas circulares
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

                if (data.data.datasets.length > 1) {
                    var dataBack = {};
                    data.options.plugins['legend'] = {
                        onHover: (e) => {
                            e.chart.canvas.style.cursor = 'pointer';
                        },
                        onLeave: (e) => {
                            e.chart.canvas.style.cursor = 'default';
                        },
                        color: '#FFFFFF',
                        labels: {
                            generateLabels(chart) {
                                const data = chart.data;
                                if (data.labels.length && data.datasets.length) {
                                    var outer = data.datasets[1].label.split("~~~")[1].split("---");
                                    var outerLabels = []
                                    var outerColors = []
                                    outer.forEach(function (item, index) {
                                        outerLabels.push(item.split("|")[0]);
                                        outerColors.push(item.split("|")[1]);
                                    });
                                    data.labels = data.labels.filter(function (el) {
                                        return !outerLabels.includes(el);
                                    });
                                    data.labels = data.labels.concat(outerLabels);
                                    return data.labels.map(function (label, i) {
                                        var meta = chart.getDatasetMeta(1);
                                        var style = meta.controller.getStyle(i);
                                        var isOuter = outerLabels.indexOf(label) != -1;
                                        if (!isOuter && !dataBack[i]) {
                                            var grupo = data.datasets[0].grupos[i];
                                            dataBack[i] = {
                                                "inner": data.datasets[1].data[i],
                                                "outer": {}
                                                /* 0: data.datasets[0].data[i * 2],
                                                 1: data.datasets[0].grupos[i * 2 + 1] == grupo ? data.datasets[0].data[i * 2 + 1] : 0,*/
                                            };
                                            var itemsGrupo = 0;
                                            var grupoStart = -1;
                                            var indexTmp = 0;
                                            var grupoTmp = 0;
                                            for (var j = 0; j < data.datasets[0].grupos.length; j++) {
                                                if (data.datasets[0].grupos[j] != grupoTmp) {
                                                    grupoTmp = data.datasets[0].grupos[j];
                                                    indexTmp++;
                                                }
                                                if (indexTmp == i) {
                                                    if (grupoStart == -1) {
                                                        grupoStart = j;
                                                    }
                                                    itemsGrupo++;
                                                }
                                            }
                                            for (var j = 0; j < itemsGrupo; j++) {
                                                //dataBack[i]["outer"][j] = data.datasets[0].data[j]; CANNOT SET PROPRETIES OF UNDEFINED
                                                dataBack[i].outer[j] = data.datasets[0].data[grupoStart + j];
                                            }
                                        }
                                        var hidden;
                                        var color = style.backgroundColor;
                                        if (isOuter) {
                                            hidden = false;
                                            color = outerColors[outerLabels.indexOf(label)];
                                        } else {
                                            hidden = isNaN(data.datasets[0].data[i]) || meta.data[i].hidden;
                                        }
                                        return {
                                            text: label,
                                            fillStyle: color,
                                            strokeStyle: color == "#FFFFFF" ? "#666666" : style.borderColor,
                                            lineWidth: style.borderWidth,
                                            hidden: hidden,
                                            index: i,
                                            grupos: data.datasets[0].grupos,
                                            data: dataBack[i]
                                        };
                                    });//.reverse();
                                }
                                return [];
                            }
                        },
                        onClick: function (e, legendItem) {
                            const toggleMeta = (meta, index, numGrupo) => {
                                if (meta.data[index]) {
                                    if (meta.data[index].hidden) {
                                        meta.data[index].hidden = false;
                                        //this.chart.data.datasets[meta.index].data[index] = meta.index == 0 ? legendItem.data.outer[index >= 2 ? index % 2 : index] : legendItem.data.inner;
                                        this.chart.data.datasets[meta.index].data[index] = meta.index == 0 ? legendItem.data.outer[index >= numGrupo ? index % numGrupo : index] : legendItem.data.inner;
                                    }
                                    else {
                                        meta.data[index].hidden = true;
                                        this.chart.data.datasets[meta.index].data[index] = 0;
                                    }

                                }
                            }
                            const innerMeta = this.chart.getDatasetMeta(1);
                            toggleMeta(innerMeta, legendItem.index);

                            //TOOD implementar grupo
                            const outerMeta = this.chart.getDatasetMeta(0);
                            var numItemsGrupo = 0;
                            var grupoStart = -1;
                            var indexTmp = 0;
                            var grupoTmp = 0;
                            for (var j = 0; j < legendItem.grupos.length; j++) {
                                if (grupoTmp != legendItem.grupos[j]) {
                                    grupoTmp = legendItem.grupos[j];
                                    indexTmp++;
                                }

                                if (indexTmp == legendItem.index) {
                                    if (grupoStart == -1) {
                                        grupoStart = j;
                                    }
                                    numItemsGrupo++;
                                }
                            }
                            for (var j = 0; j < numItemsGrupo; j++) {
                                toggleMeta(outerMeta, grupoStart + j, numItemsGrupo);
                            }

                            this.chart.update();
                        }
                    };
                    data.options.plugins.tooltip = {
                        callbacks: {
                            label: function (context) {
                                let label = context.dataset.label.split('|')[context.dataIndex] + ": ";
                                let valor = context.dataset.data[context.dataIndex];
                                return label + valor;
                            }
                        }
                    }
                }
                var myChart = new Chart(ctx, data);
            }
        }
        //that.engancharComportamientos();
        comportamientos(container);
    });
}

function drawChart(ctx, data, pIdGrafica = null, barSize = 100, pTitulo = null) {
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
    console.log(data);

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
            var acciones = pintarAccionesMapa($(ctx).parents(".grafica")[0]);
            if ($(acciones).find(".wrap .expand").length == 0) {
                $(acciones).find(".wrap").prepend(`
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
            data.options.animation.onProgress = () => reDrawChart(myChart, mainAxis, secondaryAxis, canvasSize, legend, horizontal);
            $(window).bind('resize', function () {// evento que se dispara al reescalar el navegador o hacer zoom (esto desalinea los ejes)
                reDrawChart(myChart, mainAxis, secondaryAxis, canvasSize, legend, horizontal);

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
        comportamientos(ctx);
    }
}
//funcion que se encarga de actualizar el tamaño de los ejes y el scroll en caso de reescalado. tambien los pinta al generarse la grafica
function reDrawChart(myChart, mainAxis, secondaryAxis, canvasSize, legend, horizontal = false) {

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
        myChart.canvas.style.marginTop = $(legend).height() - myChart.chartArea.top + 9 + "px";
        if (mainAxis) {
            mainAxis[0].style.marginTop = $(legend).height() - myChart.chartArea.top + 9 + "px";
        }
        if (secondaryAxis) {
            secondaryAxis[0].style.marginTop = $(legend).height() - myChart.chartArea.top + 9 + "px";
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
        try {

            ctx.drawImage(myChart.canvas, targetX, targetY, targetWidth, targetHeight, x, y, width, height);
        } catch (e) {
            // console.log(e);
        }
    }

    // Preparamos el eje inferior o derecho.
    if (secondaryAxis) {
        copyWidth = myChart.boxes[2]?.width; //anchura del eje

        ctx = secondaryAxis[0].getContext('2d');
        if (horizontal) {
            var padding = -1 * (myChart.boxes[2].width - myChart.boxes[2].left - myChart.boxes[2].right) + 5;
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
        try {
            ctx.drawImage(myChart.canvas, targetX, targetY, targetWidth, targetHeight, x, y, width, height);
        } catch (e) {
            //console.log(e);
        }

    }

}
function comportamientos(ctx) {
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
                chart.draw();
            }
        });

    $("div.expand")
        .unbind()
        .click(function (e) {
            var parent = $(this).parents('.acciones-mapa').parent()[0] || $(this).parents('.modal-body > .graph-container')[0];
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
    console.log(callbacks);

    for (const callback of Object.values(callbacks)) {
        callback.zoom();
        callback.zoom = function () { };
    }
}

function addZoomButton(pContenedor) {
    var accionesMapa = $(pContenedor).parent().find('.acciones-mapa');
    $(pContenedor).parent().css('position', 'relative');
    if (accionesMapa.length == 0) {
        $(pContenedor).parent().append(`
        <div class="acciones-mapa">
            <div class="wrap">
                <div class="zoom">
                    <a href="avascript:void(0);" data-toggle="modal">
                        <span class="material-icons">zoom_in</span>
                    </a>
                </div>
            </div>
        </div>
        `)
    } else {
        accionesMapa.find('.wrap').append(`
        <div class="zoom">
            <a href="javascript:void(0);" data-toggle="modal">
                <span class="material-icons">zoom_in</span>
            </a>
        </div>
        `)
    }
    $('div.zoom')
        .unbind()
        .click(function (e) {
            if ($("#modal-ampliar-mapa").length == 0) {
                $(".modal").first().parent().append(`
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
            } else {
                $("#modal-ampliar-mapa").find('.graph-container').addClass('grafica');
            }
            if ($(".modal-backdrop").length == 0) {
                $(".modal").first().parent().append(`
                    <div class="modal-backdrop fade" style="pointer-events:none" tabindex="-1" role="dialog"></div>
                `)
            }
            $('.modal-backdrop')
                .unbind()
                .click(cerrarModal);
            $('span.cerrar-grafica')
                .unbind()
                .click(cerrarModal);

            // Obtiene la gráfica seleccionada (en caso de menu) o la grafica del contenedor en casos normales.
            var canvas = $(this).parents(".acciones-mapa").parent().find('div.grafica.show canvas') || $(this).parents("acciones-mapa").parent().find('div.chartAreaWrapper canvas');
            var idGrafica = $(canvas).data('idGrafica');
            var idPagina = $(canvas).data('idPagina');
            var tipografica = $(canvas).data('tipoGrafica');
            var parent = $('#modal-ampliar-mapa').find('.graph-container');
            parent.removeClass('small horizontal vertical'); // se le quitan los estilos que podria tener
            //var pIdGrafica = (canvas).parents('div.grafica').attr("idgrafica");
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
            var botonImagen = tipografica == "nodes" ? $(this).parent().find('.descargarcyto') : $(this).parent().find('.descargar');
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
            ctx = $(`<div class="zoom" style="height:${$(modalContent).height() - 130}px;"></div>`);
            parent.append(ctx);
            var filtro;

            if (!$('div').hasClass('indicadoresPersonalizados')) {
                filtro = ObtenerHash2();

            } else {
                filtro = (canvas).parents('div.grafica').attr("filtro");
                idPagina = (canvas).parents('div.grafica').attr("idpagina");
            }
            $('#grafica_' + idPagina + '_' + idGrafica).attr('filtro', filtro);

            //obtenemos los datos y pintamos la grafica
            if (!$('div').hasClass('indicadoresPersonalizados')) {
                getGrafica(idPagina, idGrafica, filtro, ctx[0], 50);
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
                    getGrafica(idPagina, idGrafica, filtro, ctx[0], 50, null, tituloActual)

                });
            }
            //metricas.engancharComportamientos();
        });
}
function addDownloadCvButton(pContenedor) {
    var accionesMapa = $(pContenedor).parent().find("div.acciones-mapa");
    if (accionesMapa.length == 0) {
        accionesMapa = pintarAccionesMapa(pContenedor);
    }
}
function addDownloadJpgButton(pContenedor) {
    var accionesMapa = $(pContenedor).parent().find("div.acciones-mapa");
    if (accionesMapa.length == 0) {
        accionesMapa = pintarAccionesMapa(pContenedor);
    }

}
function pintarAccionesMapa(pContenedor) {

    if ($(pContenedor).parent().find("div.acciones-mapa").length == 0) {


        var accionesMapa = (`<div class="acciones-mapa"><div class="wrap"></div></div>`);
        $(pContenedor).parent().append(accionesMapa);
        $(pContenedor).parent().css("position", "relative");
        return $(pContenedor).parent().find("div.acciones-mapa");
    }
    else {
        return $(pContenedor).parent().find("div.acciones-mapa");
    }
}
/*function addExpandButton(pContenedor) {

    var accionesMapa = pContenedor.parent().find('.acciones-mapa');
    if (accionesMapa.length > 0) {
        accionesMapa.append(`<div class="expand-button">
                                <i class="material-icons">expand_more</i>
                            </div>`);
    } else {
        pContenedor.append(`<div class="expand-button">
                                <i class="material-icons">expand_more</i>
                            </div>`);
    }
}*/

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