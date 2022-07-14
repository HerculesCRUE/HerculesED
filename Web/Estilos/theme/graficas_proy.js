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

function pintarGraficaIndividual(pContenedor, pIdPagina, idGrafica = "") {
    //pintarContenedoresGraficas(pContenedor, pIdPagina);
    getGrafica(pIdPagina, idGrafica, "", pContenedor);
}
function pintarContenedoresGraficas(pContenedor, pIdPagina = "", tipografica = "", idgrafica = "") {
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
                <div class="graficoNodos" id="grafica_${pIdPagina}_${idgrafica}" style="height: 500px;"></div>
            `);
    } else if (tipografica.includes("circular")) {
        $(pContenedor).css("height", "300px");
        $(pContenedor).append(`
            <canvas id = "grafica_${pIdPagina}_${idgrafica}" width = "600" height = "250" ></canvas>
                `);
    } else {

        $(pContenedor).append(`
        <div class="chartWrapper" >
            <div class="chartScroll custom-css-scroll">
                <div class="chartAreaWrapper">
                    <canvas width = "600" height = "250" id="grafica_${pIdPagina}_${idgrafica}"></canvas>
                </div>
            </div>
        </div>
        `);
    }
}
function getGrafica(pIdPagina, pIdGrafica, pFiltroFacetas, ctx = null, barSize = 50, pIdRecurso = null, pTitulo = null, maxScales = null) {
    var that = this;
    var url = url_servicio_graphicengine + "GetGrafica"; //"https://localhost:44352/GetGrafica"
    var arg = {};
    arg.pIdPagina = pIdPagina;
    arg.pIdGrafica = pIdGrafica;
    arg.pFiltroFacetas = pFiltroFacetas;
    arg.pLang = lang; //"es";

    // Petición para obtener los datos de las gráficas.
    $.get(url, arg, function (data) {

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
        if (ctx.tagName == "DIV") { //igual es mejor comprobar si la id es de
            pintarContenedoresGraficas(ctx, pIdPagina, tipoGrafica, pIdGrafica);
            ctx = document.getElementById("grafica_" + pIdPagina + "_" + pIdGrafica);
        }



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

                                    console.log(data);
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
                                            console.log(outer);

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
        comportamientos();
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
            data.options.animation.onProgress = () => reDrawChart(myChart, mainAxis, secondaryAxis, canvasSize, legend, horizontal);
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

}
function comportamientos() {
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
} 