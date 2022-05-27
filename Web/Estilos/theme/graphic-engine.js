$(document).ready(function () {
    metricas.init();
});

var metricas = {
    init: function () {
        this.createEmptyPage('123');
        this.getPage('123');
        return;
    },
    config: function () {
        return;
    },
    getPage: function (pIdPagina) {
        var that = this;
        var url = "https://localhost:44352/GetPaginaGrafica";
        //var url = url_servicio_graphicengine+"GetPaginaGrafica";
        var arg = {};
        arg.pIdPagina = "123";
        arg.pLang = lang;

        // Petición para obtener los datos de la página.
        $.get(url, arg, function (data) {
            that.fillPage(data);
        });
    },
    getGrafica: function (pIdPagina, pIdGrafica, pFiltroFacetas) {
        var that = this;
        var url = "https://localhost:44352/GetGrafica";
        //var url = url_servicio_graphicengine+"GetGrafica";
        var arg = {};
        arg.pIdPagina = pIdPagina;
        arg.pIdGrafica = pIdGrafica;
        arg.pFiltroFacetas = pFiltroFacetas;
        arg.pLang = lang;

        // Petición para obtener los datos de las gráficas.
        $.get(url, arg, function (data) {
            var ctx = document.getElementById("grafica_" + pIdPagina + "_" + pIdGrafica);

            // TODO: Controlar ChartJS o Cytoscape
            if ("container" in data) {
                data.container = ctx;
                data.ready = function () { window.cy = this };
                var cy = window.cy = cytoscape(data);

                $(`#titulo_grafica_${pIdPagina}_${pIdGrafica}`).append(data.title);

                var arrayNodes = [];
                var nodos = cy.nodes();
                for (i = 0; i < cy.nodes().length; i++) { //starts loop
                    arrayNodes.push(nodos[i]._private.data.name);
                };

                var arrayEdges = [];
                var edges = cy.edges();
                for (i = 0; i < cy.edges().length; i++) { //starts loop
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

            } else {
                var myChart = new Chart(ctx, data);

                var numBars = data.data.labels.length; //numero de barras
                var barSize = 100; //tamaño arbitrario de las barras 
                var canvasSize = (numBars * barSize); // y establezco el tamaño del canvas 

                //solo si es una grafica horizontal
                if (data.options.indexAxis == "y") {
                    //obtenemos los elementos de la grafica  
                    var canvas = myChart.canvas;
                    var chartAreaWrapper = canvas.parentNode;
                    var scrollContainer = chartAreaWrapper.parentNode;
                    var chartContainer = scrollContainer.parentNode;

                    // En caso de que los labels de la grafica deban de estar abreviados
                    if (pIdGrafica.includes("abr")) { 
                        //Se modifica la propiedad que usa chart.js para obtener los labels de la grafica
                        data.options.scales.y.ticks.callback = function (value) { 
                            const labels = data.data.labels; //obtenemos los labels
                            if (value >= 0 && value < labels.length) {
                                if (labels[value].length >= 3) {
                                    return labels[value].substring(0, 3) + "."; //y mostramos solo los 3 primeros caracteres
                                }
                                return labels[value];
                            }
                            return value;
                        }
                        myChart.update();
                    }

                    //si el canvas no supera el tamaño del contenedor, no se hace scroll
                    if (canvasSize < 456) {
                        chartAreaWrapper.style.height = canvasSize + 'px';
                        scrollContainer.style.overflowY = 'hidden';


                    // de lo contrario se prepara todo para el scroll
                    } else {
                        //importante que no mantega el ratio para poder reescalarlo
                        data.options.maintainAspectRatio = false;
                        data.options.responsive = true;

                        var hasTopAxis = false;
                        var hasBottomAxis = false;



                        // compruebo que si tiene eje inferior y superior
                        Object.entries(data.options.scales).forEach((scale) => {

                            if (scale[1].axis == "x") {
                                if (scale[1].position == "top" && !hasTopAxis) {
                                    hasTopAxis = true;
                                } else if (scale[1].position == "bottom" && !hasBottomAxis) {
                                    hasBottomAxis = true;
                                }
                            }
                        });


                        //si existe un eje superior creo los elementos necesarios para la leyenda y el eje
                        if (hasTopAxis) { 
                            //contenedor de la leyenda
                            var legend = document.createElement('div');
                            legend.id = 'chartLegend';
                            legend.style.textAlign = 'center';
                            legend.style.position = 'absolute';
                            legend.style.top = '0px';
                            legend.style.backgroundColor = 'white';
                            chartContainer.appendChild(legend);
                            //titulo del canvas
                            var legendTitle = document.createElement('h4');
                            legendTitle.innerHTML = data.options.plugins.title.text; //obtenido de data
                            legendTitle.id = 'legendTitle';
                            legendTitle.style.margin = '10px';
                            legendTitle.style.fontFamily = 'Calibri,sans-serif';
                            legendTitle.style.fontSize = '90%';
                            legendTitle.style.fontWeight = 'bold';
                            legend.appendChild(legendTitle);
                            //contenedor de los elementos de la leyenda
                            var dataSetLabels = document.createElement('div');
                            dataSetLabels.id = 'dataSetLabels';
                            dataSetLabels.style.display = 'flex';
                            dataSetLabels.style.flexFlow = 'row wrap';
                            dataSetLabels.style.justifyContent = 'center';
                            legend.appendChild(dataSetLabels);
                            //eje superior 
                            var topAxis = document.createElement('canvas');
                            topAxis.id = 'topAxis';
                            topAxis.className = 'myChartAxis';
                            topAxis.style.background = 'white';
                            topAxis.style.position = 'absolute';
                            topAxis.style.bottom = '0px';
                            topAxis.style.left = '0px';
                            legend.appendChild(topAxis);
                            //ahora por cada dataset que exista se creara un div con su nombre y color y se añade a dataSetLabels
                            var datasets = data.data.datasets;
                            datasets.forEach((dataset, index) => {
                                var labelContainer = document.createElement('div');
                                var colorDiv = document.createElement('div');
                                var barLabel = document.createElement('p');

                                colorDiv.style.height = "15px";
                                colorDiv.style.width = "45px";
                                colorDiv.style.backgroundColor = dataset.backgroundColor[0];
                                colorDiv.style.border = "1px solid lightgrey";
                                colorDiv.style.boxSizing = "border-box";

                                barLabel.style.fontFamily = "Calibri";
                                barLabel.style.margin = "5px";


                                labelContainer.style.margin = "5px";
                                labelContainer.style.height = "15px";
                                labelContainer.style.display = "flex";
                                labelContainer.id = "label-" + index;
                                labelContainer.className = "labelContainer";
                                labelContainer.style.alignItems = "center";

                                barLabel.className = 'dataSetLabel';
                                barLabel.innerHTML = dataset.label;

                                labelContainer.appendChild(colorDiv);
                                labelContainer.appendChild(barLabel);
                                dataSetLabels.appendChild(labelContainer);

                            });

                        }

                        //si existe un eje inferior, lo añado y le aplico estilos
                        if (hasBottomAxis) {
                            var bottomAxis = document.createElement('canvas');
                            bottomAxis.id = 'bottomAxis';
                            bottomAxis.className = 'myChartAxis';
                            bottomAxis.style.background = 'white';
                            bottomAxis.style.position = 'absolute';
                            bottomAxis.style.bottom = '0px';
                            bottomAxis.style.left = '0px';
                            chartContainer.appendChild(bottomAxis);
                        }

                        //cuando se acutaliza el canvas
                        data.options.animation.onProgress = reDrawChart;
                        //y cuando se reescala el navegador se redibuja la leyenda
                        window.addEventListener('resize', (e) => {
                            reDrawChart();
                            myChart.update();

                        });

                        //funcion que dibuja la leyenda y los ejes, tambien reescala todo para que coincida con el chart
                        function reDrawChart() {
                            console.log("sdads");


                            //se obtiene la escala del navegador (afecta cuando el usuario hace zoom)
                            var scale = window.devicePixelRatio;
                            chartAreaWrapper.style.height = canvasSize + 'px';

                            var copyWidth = myChart.width;
                            /*la altura del titulo, leyenda y eje superior menos el margen*/
                            var copyHeight = myChart.boxes[0].height + myChart.boxes[1].height + myChart.boxes[2].height - 5;

                            /*Le asignamos tamaño a la leyenda */

                            var axisHeight = myChart.boxes[2].height;
                            /*Preparamos el eje superior */
                            if (topAxis) {
                                legend.style.height = copyHeight + "px";
                                legend.style.width = copyWidth + "px";
                                var topAxisCtx = topAxis.getContext('2d');
                                topAxisCtx.scale(scale, scale); // para el zoom 
                                topAxisCtx.canvas.width = copyWidth;
                                topAxisCtx.canvas.height = axisHeight;
                                topAxisCtx.drawImage(myChart.canvas, 0, (copyHeight - axisHeight) * scale, copyWidth * scale, axisHeight * scale, 0, 0, copyWidth, axisHeight);
                            }
                            /*Preparamos el eje inferior*/
                            if (bottomAxis) {
                                var bottomAxisCtx = bottomAxis.getContext('2d');

                                bottomAxisCtx.scale(scale, scale); // para el zoom
                                bottomAxisCtx.canvas.width = copyWidth;
                                bottomAxisCtx.canvas.height = axisHeight;
                                bottomAxisCtx.drawImage(myChart.canvas, 0, myChart.chartArea.bottom * scale, copyWidth * scale, axisHeight * scale, 0, 0, copyWidth, axisHeight);
                            }



                        }


                    }
                }

            }
            that.engancharComportamientos();
        });
    },
    getFaceta: function (pIdPagina, pIdFaceta, pFiltroFacetas) {
        var that = this;
        var url = "https://localhost:44352/GetFaceta";
        //var url = url_servicio_graphicengine+"GetFaceta";
        var arg = {};
        arg.pIdPagina = pIdPagina;
        arg.pIdFaceta = pIdFaceta;
        arg.pFiltroFacetas = pFiltroFacetas;
        arg.pLang = lang;

        // Petición para obtener los datos de las gráficas.
        $.get(url, arg, function (data) {

            var numItemsPintados = 0;

            $('div[idfaceta="' + data.id + '"]').append(`
                <h1>${data.nombre}</h1>
                `);
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
    createEmptyPage: function (pIdPagina) {
        $('.containerPage').attr('id', 'page_' + pIdPagina);
        $('.containerPage').addClass('pageMetrics');

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
        var that = this;

        // Crear estructura para el apartado de gráficas.

        var rowNumber = 0;
        var espacio = 12;

        pPageData.listaConfigGraficas.forEach(function (item, index, array) {
            console.log($(item.nombre));

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
            $('#page_' + pPageData.id + '.containerPage').find('.resource-list-wrap').append(`
                <article class="resource span${item.anchura}"> 
                    <div class="wrap">
                        <div class="title-wrap">
                            <h2></h2>
                        </div>
                        <div class="acciones-mapa">
                            <div class="wrap">
                                <div class="zoom">
                                    <a href="javascript: void(0);" style="height:24px"  data-target="#modal-ampliar-mapa" data-toggle="modal">
                                        <span class="material-icons">zoom_in</span>
                                    </a>
                                </div>
                                <div class="descargar">
                                    <a href="javascript: void(0);" style="height:24px" >
                                        <span class="material-icons">download</span>
                                    </a>
                                </div>
                            </div>
                        </div>

                      
                        <div class="grafica " idgrafica='${item.id}'>
                        </div>
                </article>

            `);
        });

        // Crear estructura para el apartado de facetas.
        pPageData.listaIdsFacetas.forEach(function (item, index, array) {
            $('#page_' + pPageData.id + ' .containerFacetas').append(`
                        <div class='faceta' idfaceta='${item}'></div>
                    `);
        });

        that.pintarPagina(pPageData.id)
    },
    pintarPagina: function (pIdPagina) {
        var that = this;

        // Vacias contenedores.
        $('#page_' + pIdPagina + ' .grafica').empty();
        $('#page_' + pIdPagina + ' .faceta').empty();

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
            } else if ($(this).attr("idgrafica").includes("isHorizontal")) {
                $(this).append(`
                <div class="chartWrapper" style="position:relative; margin-top:15px">
                    <div style="overflow-y: scroll;height:546px;">
                        <div style="height: 00px;" class="chartAreaWrapper">
                            <canvas width = "600" height = "250" id="grafica_${pIdPagina}_${$(this).attr("idgrafica")}"></canvas>
                        </div>
                    </div>
                </div>
                `);

                /*
                <div class="chartWrapper" style="position:relative;">
                    <div style="overflow-y: scroll;height:546px;">
                        <div style="height: 1900px;" class="chartAreaWrapper">
                            <canvas width = "600" height = "250" id="grafica_${pIdPagina}_${$(this).attr("idgrafica")}"></canvas>
                        </div>
                    </div>
                    <div id="chartLegend" style="text-align:center;position:absolute;top:0px;background-color:white">
                        <h4 id="legendTitle" style="margin:10px;font-family:Calibri,sans-serif;font-size:90%;font-weight:bold;"></h4>
                        <div id="dataSetLabels" style="display:flex;flex-flow:row wrap;justify-content:center"></div>

                        <canvas id="topAxis" class="myChartAxis" style="background:white;position:absolute;bottom:0px;left:0px"></canvas>
                    </div>
                    <canvas id="bottomAxis" class="myChartAxis" style="background:white;position:absolute;bottom:0px"></canvas>
                </div>
            `);*/
            } else {
                $(this).append(`
                    <canvas id = "grafica_${pIdPagina}_${$(this).attr("idgrafica")}" width = "600" height = "250" ></canvas>
                        `);
            }
            that.getGrafica(pIdPagina, $(this).attr("idgrafica"), ObtenerHash2());
        });

        // Recorremos el div de las facetas.
        $('#page_' + pIdPagina + ' .faceta').each(function () {
            that.getFaceta(pIdPagina, $(this).attr("idfaceta"), ObtenerHash2());
        });
    },
    engancharComportamientos: function () {
        var that = this;

        $('.containerFacetas a.filtroMetrica')
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
                that.pintarPagina($(this).closest('.pageMetrics').attr('id').substring(5));
            });
        $('#zoomIn')
            .unbind()
            .click(function (e) {
                cy.zoom(cy.zoom() + 0.2);
            });

        $('#zoomOut')
            .unbind()
            .click(function (e) {
                cy.zoom(cy.zoom() - 0.2);
            });


        //Labels de la leyenda
        $('div.labelContainer')
            .unbind()
            .click(function (e) {
                //obtenemos el chart desde el canvas
                var canvas = $(this).parents('div.chartWrapper').find('div.chartAreaWrapper canvas');
                var chart = Chart.getChart(canvas);
                //la id del dataset esta en la id del contenedor del label
                var id = $(this).attr('id').split('-')[1];
                var label = $(this).find('p.dataSetLabel');

                //si el label no esta tachado se tacha y oculta el dataset
                if (label.css('text-decoration').indexOf("line-through") == -1) {
                    label.css("text-decoration", "line-through");
                    chart.setDatasetVisibility(id, false);
                } else {
                    label.css("text-decoration", "none");
                    chart.setDatasetVisibility(id, true);
                }
                try {//hay problemas con el grafico de lineas, si falla se redibuja el chart
                    chart.update();
                    chart.redraw();
                } catch (e) {
                    chart.draw();
                    console.log(e);
                }
            });

        //Boton de desacarga
        $('div.descargar')
            .unbind()
            .click(function (e) {
                //obtenemos el chart usando el elemento canvas
                var canvas = $(this).parents('div.wrap').find('div.chartAreaWrapper canvas');
                var chart = Chart.getChart(canvas);
                //lo pasamos a imagen
                var image = chart.toBase64Image();
                //creamos un elemento para comenzar la descarga
                var a = document.createElement('a');
                a.href = image;
                a.download = (chart.id) + '.png';
                a.click();

            });


    }
}