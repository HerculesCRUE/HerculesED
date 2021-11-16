$(function () {
    edicionCV.init();
});


//var urlEdicionCV = "https://localhost:44360/EdicionCV/";
//var urlGuardadoCV = "https://localhost:44360/GuardadoCV/";
var urlEdicionCV = "http://serviciosedma.gnoss.com/editorcv/EdicionCV/";
var urlGuardadoCV = "http://serviciosedma.gnoss.com/editorcv/GuardadoCV/";


var edicionCV = {
    idCV: null,
	idPerson: null,
    init: function () {
        this.config();
        this.idCV = $('.contenido-cv').attr('about');
		this.idPerson = $('.contenido-cv').attr('personid');
        return;
    },
    config: function () {
		
		
		$('*').on('shown.bs.modal', function(e) {
			$('.modal-backdrop').last().addClass($(this).attr('id'));
		});
		
		
		
		//TODO cambiar
		$('body').append(`
		<style>
		.entityauxcontainer>.entityaux{display:none!important;}
		.entitycontainer>.entity{display:none!important;}
		.form-group.multiple input,.form-group.entitycontainer:not(.multiple) input{display:inline;height:38px;}
		.form-group.multiple .acciones-listado-edicion{width:30%;float:right;margin-bottom:0px;}
		.form-group.multiple .acciones-listado-edicion{width:auto;float:none;}
		.form-group.multiple:not(.entitycontainer)>div {display:flex;}
		.form-group.entitycontainer:not(.multiple)>div {display:flex;}
		.form-group.multiple.entityauxcontainer>div {
			display: block;
		}
		.form-group-date.material-icons {
				position: absolute;
				top: 42px;
				left: 10px;
				color: var(--c-secundario);
			}
		.custom-form-row .form-group {
			position: relative;
		}
		input.form-group-date {
			padding-left: 45px;
		}
		.page-cv .resource-list .resource .middle-wrap .content-wrap .description-wrap .group.mini, 
		.page-cv .resource-list .resource .middle-wrap .content-wrap .description-wrap .group.mini {
			display: block;
		}
		.page-cv .resource-list .resource .middle-wrap .content-wrap .description-wrap .group.resaltado p {
			font-weight: 500;
		}
		.page-cv .resource-list .resource.activo .middle-wrap .content-wrap .description-wrap .group.resaltado p {
			font-weight: 300;
		}
		
		.page-cv #modal-eliminar {
			z-index: 1501 !important;
		}
		.page-cv .modal-backdrop.show.modal-eliminar {
			z-index: 1500 !important;
		}
		
		.page-cv #modal-editar-entidad {
			z-index: 1062 !important;
		}
		.page-cv .modal-backdrop.show.modal-editar-entidad {
			z-index: 1061 !important;
		}
		
		.page-cv #modal-editar-entidad-0 {
			z-index: 1064 !important;
		}
		.page-cv .modal-backdrop.show.modal-editar-entidad-0 {
			z-index: 1063 !important;
		}
		
		.page-cv #modal-editar-entidad-0 .modal-dialog {
			width: 100%;
			max-width: 1180px;
		}
		
		.page-cv #modal-editar-entidad-1 {
			z-index: 1066 !important;
		}
		.page-cv .modal-backdrop.show.modal-editar-entidad-1 {
			z-index: 1065 !important;
		}
		.page-cv #modal-editar-entidad-1 .modal-dialog {
			width: 100%;
			max-width: 1080px;
		}
		
		.page-cv #modal-editar-entidad-2 {
			z-index: 1068 !important;
		}
		.page-cv .modal-backdrop.show.modal-editar-entidad-2 {
			z-index: 1067 !important;
		}
		.page-cv #modal-editar-entidad-2 .modal-dialog {
			width: 100%;
			max-width: 980px;
		}
		
		.page-cv #modal-editar-entidad-3 {
			z-index: 1070 !important;
		}
		.page-cv .modal-backdrop.show.modal-editar-entidad-3 {
			z-index: 1069 !important;
		}
		.page-cv #modal-editar-entidad-3 .modal-dialog {
			width: 100%;
			max-width: 880px;
		}
		
		.page-cv #modal-editar-entidad-4 {
			z-index: 1072 !important;
		}
		.page-cv .modal-backdrop.show.modal-editar-entidad-4 {
			z-index: 1071 !important;
		}
		.page-cv #modal-editar-entidad-4 .modal-dialog {
			width: 100%;
			max-width: 780px;
		}
		
		.page-cv #modal-anadir-autor {
			z-index: 1500 !important;
		}
		
		.page-cv .modal-backdrop.show.modal-anadir-autor {
			z-index: 1499 !important;
		}
		
		</style>`);
		
        //Carga de secciones principales
        var that = this;
        $('.cabecera-cv .h1-container .dropdown-menu a').click(function (e) {
            $($(this).attr('href')).click();
        });
        $('#navegacion-cv li.nav-item a').click(function (e) {
            var entityID = $($(this).attr('href')).find('.cvTab').attr('about');
            var rdfType = $($(this).attr('href')).find('.cvTab').attr('rdftype');
            that.loadTab(entityID, rdfType);
        });
        return;
    },
    //Métodos de pestañas
    loadTab: function (entityID, rdfType) {
        var that = this;
        $('div[about="' + entityID + '"] .col-12.col-contenido').empty();
        MostrarUpdateProgress();
		
        $.get(urlEdicionCV + 'GetTab?pId=' + entityID + "&pRdfType=" + rdfType + "&pLang=" + lang, null, function (data) {
            that.printTab(entityID, data);
            OcultarUpdateProgress();
        });
        return;
    },
    printTab: function (entityID, data) {
        for (var i = 0; i < data.sections.length; i++) {
            $('div[about="' + entityID + '"] .col-12.col-contenido').append(this.printTabSection(data.sections[i]));
            this.repintarListadoTab(data.sections[i].identifier);
        }
        accionesCurriculum.init();
    }
    //Fin de métodos de pestañas
    ,
    //Métodos de secciones
    printTabSection: function (data) {
        //Pintado sección listado
        //css mas generico
        var id = RandomGuid();
        var id2 = RandomGuid();
        var htmlSection = `
		<div class="panel-group pmd-accordion" section="${data.identifier}" id="${id}" role="tablist" aria-multiselectable="true">
            <div class="panel">
                <div class="panel-heading" role="tab" id="publicaciones-tab">
                    <p class="panel-title">
                        <a data-toggle="collapse" data-parent="#${id}" href="#${id2}" aria-expanded="true" aria-controls="${id2}" data-expandable="false">
                            <span class="material-icons pmd-accordion-icon-left">folder_open</span>
                            <span class="texto">${data.title}</span>
                            <span class="numResultados">(${Object.keys(data.items).length})</span>
                            <span class="material-icons pmd-accordion-arrow">keyboard_arrow_down</span>
                        </a>
                    </p>
                </div>
                <div id="${id2}" class="panel-collapse collapse show" role="tabpanel">
                    <div class="panel-body">
                        <div class="acciones-listado acciones-listado-cv">
							<div class="wrap">								
							</div>
							<div class="wrap">
								<ul class="no-list-style d-flex align-items-center">
									<li>
										<a class="btn btn-outline-grey aniadirEntidad">
											<span class="texto">Añadir</span>
											<span class="material-icons">post_add</span>
										</a>
									</li>
								</ul>
								<div class="ordenar dropdown">${this.printOrderTabSection(data.orders)}</div>
								<div class="buscador">
									<div class="fieldsetGroup searchGroup">
										<div class="textoBusquedaPrincipalInput">
											<input type="text" class="not-outline txtBusqueda" placeholder="Escribe algo..." autocomplete="off">
											<span class="botonSearch">
												<span class="material-icons">search</span>
											</span>
										</div>
									</div>
								</div>    
							</div>
						</div>                        
						<div class="resource-list listView">
							<div class="resource-list-wrap">
								${this.printHtmlListItems(data.items)}
								<div class="panNavegador">
									<div class="items dropdown">
										<a class="dropdown-toggle" data-toggle="dropdown" aria-expanded="true">
											<span class="texto" items="5">Ver 5 items</span>
										</a>
										<div class="dropdown-menu basic-dropdown dropdown-menu-right" x-placement="bottom-end">
											<a href="javascript: void(0)" class="item-dropdown" items="5">Ver 5 items</a>
											<a href="javascript: void(0)" class="item-dropdown" items="10">Ver 10 items</a>
											<a href="javascript: void(0)" class="item-dropdown" items="20">Ver 20 items</a>
											<a href="javascript: void(0)" class="item-dropdown" items="50">Ver 50 items</a>
											<a href="javascript: void(0)" class="item-dropdown" items="100">Ver 100 items</a>
										</div>
									</div>
									<nav>
										<ul class="pagination numbers">	
											<li class="actual"><a href="javascript: void(0)" page="1">1</a></li>
										</ul>
										<ul class="pagination arrows">											
										</ul>
									</nav>
								</div>    
							</div>
						</div>
					</div>
                </div>
            </div>
        </div>`;
        return htmlSection;

    },
    printOrderTabSection: function (orders) {
        if (orders != null && orders.length > 0) {
            let propDefault = "";
            let ascDefault = "";
            for (var item in orders[0].properties) {
                if (propDefault != '') {
                    propDefault += "||";
                }
                if (ascDefault != '') {
                    ascDefault += "||";
                }
                propDefault += orders[0].properties[item].property;
                ascDefault += orders[0].properties[item].asc;
            }
            var oculto = "";
            if (orders.length == 1) {
                oculto = "oculto";
            }
            return `	<a class="dropdown-toggle ${oculto}" data-toggle="dropdown">
							<span class="material-icons">swap_vert</span>
							<span class="texto" property="${propDefault}" asc="${ascDefault}">${orders[0].name}</span>
						</a>
						<div class="dropdown-menu basic-dropdown dropdown-menu-right">
						${this.printOrderTabSectionItems(orders)}
						</div>`;
        }
        return "";
    },
    printOrderTabSectionItems: function (orders) {
        var html = '';
        for (var ord in orders) {
            html += this.printOrderTabSectionItem(orders[ord]);
        }
        return html;
    },
    printOrderTabSectionItem: function (order) {
        let prop = "";
        let asc = "";
        for (var item in order.properties) {
            if (prop != '') {
                prop += "||";
            }
            if (asc != '') {
                asc += "||";
            }
            prop += order.properties[item].property;
            asc += order.properties[item].asc;
        }
        return `<a href="javascript: void(0)" class="item-dropdown"  property="${prop}" asc="${asc}">${order.name}</a>`;
    },
    printHtmlListItems: function (items) {
        var html = "";
        for (var item in items) {
            html += this.printHtmlListItem(item, items[item]);
        }
        return html;
    },
    printHtmlListItem: function (id, data) {

        var htmlListItem = `<article class="resource success">									
									<div class="wrap">
										<div class="middle-wrap">
											${this.printHtmlListItemOrders(data)}
											<div class="title-wrap">
											</div>
											<div class="title-wrap">
												<h2 class="resource-title">
													<a href="#" data-id="${id}">${data.title}</a>
													${this.printHtmlListItemPrivacidad(data)}
												</h2>
												${this.printHtmlListItemAcciones(data, id)}                        
												<span class="material-icons arrow">keyboard_arrow_down</span>
											</div>
											<div class="content-wrap">
												<div class="description-wrap">
												${this.printHtmlListItemPropiedades(data)}
												</div>
											</div>
										</div>
									</div>
								</article>`;
        return htmlListItem;
    },
    printHtmlListItemOrders: function (data) {
        var html = '<div class="orderItems" style="display:none">';
        for (var property in data.orderProperties) {
            var values = "";
            if (data.orderProperties[property].values != null) {
                for (var value in data.orderProperties[property].values) {
                    values += data.orderProperties[property].values[value].toLowerCase().trim();
                }
            }
            html += `<div property="${data.orderProperties[property].property}">${values}</div>`;
        }
        html += `<div property="default">${data.title.toLowerCase().trim()}</div>`;
        html += '</div>';
        return html;
    },


    printHtmlListItemPrivacidad: function (data) {
		if (data.ispublic == "true") {
			return `	<div class="candado-wrapper">
							<div class="con-icono-before candado-activo"></div>
						</div>`;
		} else if (data.ispublic == "false") {
			return `	<div class="candado-wrapper">
							<div class="con-icono-before candado"></div>
						</div>`;
		}
    },
    printHtmlListItemAcciones: function (data, id) {
        var htmlPublicar = "";
		if (data.ispublic.toLowerCase() != "true") {
			htmlPublicar = `	<li>
								<a class="item-dropdown">
									<span class="material-icons">lock_open</span>
									<span class="texto publicaritem" data-id="${id}" property="${data.propertyIspublic}">Publicar</span>
								</a>
							</li>`;
		}
		if (data.ispublic.toLowerCase() != "false") {
			htmlPublicar = `	<li>
								<a class="item-dropdown">
									<span class="material-icons">lock</span>
									<span class="texto despublicaritem" data-id="${id}">Despublicar</span>
								</a>
							</li>`;
		}
		return `<div class="acciones-recurso-listado acciones-recurso">
					<div class="dropdown">
						<a href="javascript: void(0)" class="dropdown-toggle no-flecha" role="button" id="dropdownMasOpciones" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
							<span class="material-icons">more_vert</span>
						</a>
						<div class="dropdown-menu basic-dropdown dropdown-icons dropdown-menu-right" aria-labelledby="dropdownMasOpciones">
							<p class="dropdown-title">Acciones</p>
							<ul class="no-list-style">
								${htmlPublicar}
								<li>
									<a class="item-dropdown">
										<span class="material-icons">delete</span>
										<span class="texto eliminar" data-id="${id}">Eliminar</span>
									</a>
								</li>
							</ul>
						</div>
					</div>
				</div>`;
    },
    printHtmlListItemPropiedades: function (data) {
        var html = ''
        for (var property in data.properties) {
            if (data.properties[property].values.length > 0) {
                html += this.printHtmlListItemPropiedad(data.properties[property]);
            }
        }
        return html;
    },
    printHtmlListItemPropiedad: function (prop) {
        var htmlProp = "";
        if (prop.values.length > 1) {
            htmlProp = `		<div class="list-wrap">
								<ul>`;
            for (var value in prop.values) {
                htmlProp += `<li>${prop.values[value]}</li>`;
            }
            htmlProp += `			</ul>
							</div>`
        } else {
            htmlProp = `<p>${TransFormData(prop.values, prop.type)}</p>`;
        }
		var css="";
		if(prop.showMiniBold)
		{
			css="mini resaltado";
		}else if(prop.showMini)
		{
			css="mini";
		}		
        return `<div class="group ${css}">
					<p class="title">${prop.name}</p>
					${htmlProp}
				</div>`;
    },
    repintarListadoTab: function (id) {

        var numResultadosPagina = parseInt($('.panel-group[section="' + id + '"] .panNavegador .dropdown-toggle span').attr('items'));
        var texto = $('.panel-group[section="' + id + '"] .txtBusqueda').val();
        var paginaActual = parseInt($('.panel-group[section="' + id + '"] .panNavegador .pagination.numbers li.actual a').attr('page'));
        var ordenProperty = $('.panel-group[section="' + id + '"] .ordenar.dropdown .texto').attr('property');
        var ordenAsc = $('.panel-group[section="' + id + '"] .ordenar.dropdown .texto').attr('asc');
        //paginaActual
        //orden

        var NUM_PAG_INICIO = 3;
        var NUM_PAG_PROX_CENTRO = 2;
        var NUM_PAG_FIN = 3;

        var articulos = $('div[section="' + id + '"] article');
        articulos = articulos.sort(function (a, b) {
            if (ordenProperty == null || ordenProperty == '') {
                if ($(a).find('div.orderItems div[property="default"]').text() > $(b).find('div.orderItems div[property="default"]').text()) return 1;
                if ($(a).find('div.orderItems div[property="default"]').text() < $(b).find('div.orderItems div[property="default"]').text()) return -1;
                return 0;
            } else {
                let ordenPropertySplit = ordenProperty.split('||');
                let ordenAscSplit = ordenAsc.split('||');
                for (var i = 0; i < ordenPropertySplit.length; i++) {
                    let property = ordenPropertySplit[i];
                    let asc = ordenAscSplit[i];
                    let valueA = $(a).find('div.orderItems div[property="' + property + '"]').text();
                    let valueB = $(b).find('div.orderItems div[property="' + property + '"]').text();
                    if (asc == 'true') {
                        if (valueA > valueB) {
                            return 1;
                        }
                        if (valueA < valueB) {
                            return -1;
                        }
                    } else {
                        if (valueA < valueB) {
                            return 1;
                        }
                        if (valueA > valueB) {
                            return -1;
                        }
                    }
                }
                return 0;

            }
        });
        $('div[section="' + id + '"] article').remove();
        $('div[section="' + id + '"] .resource-list .resource-list-wrap').prepend(articulos);

        var numTotal = 1;
        var numPaginas = 1;
        var texto = EliminarAcentos(texto).toLowerCase();
        $('div[section="' + id + '"] article').each(function () {
			var existeEnTitulo=EliminarAcentos($(this).find('h2').text()).toLowerCase().indexOf(texto) > -1;
			var existeEnPropiedad=EliminarAcentos($(this).find('.content-wrap .group p:not(.title),.content-wrap .group li').text()).toLowerCase().indexOf(texto) > -1;
            if (existeEnTitulo||existeEnPropiedad) {
                numPaginas = Math.floor((numTotal - 1 + numResultadosPagina) / numResultadosPagina);
                if (numPaginas == paginaActual) {
                    $(this).show();
                } else {
                    $(this).hide();
                }
                numTotal++;
            } else {
                $(this).hide();
            }
        });
        $('div[section="' + id + '"] .pagination.numbers').empty();
        $('div[section="' + id + '"] .pagination.arrows').empty();

        ///INICIO/
        for (i = 1; i <= NUM_PAG_INICIO; i++) {
            if (i > numPaginas) //Hemos excedio el número máximo de páginas, así que dejamos de pintar.
            {
                break;
            }
            if (i == paginaActual) {
                $('div[section="' + id + '"] .pagination.numbers').append(`<li class="actual"><a page="${i}" >${i}</a></li>`);
            }
            else {
                $('div[section="' + id + '"] .pagination.numbers').append(`<li ><a href="javascript: void(0)" page="${i}" >${i}</a></li>`);
            }
        }

        if (numPaginas > NUM_PAG_INICIO) //Continuamos si ha más páginas que las que ya hemos pintado
        {
            var inicioRango = paginaActual - NUM_PAG_PROX_CENTRO;
            var finRango = paginaActual + NUM_PAG_PROX_CENTRO;

            if (paginaActual < (NUM_PAG_INICIO + NUM_PAG_PROX_CENTRO + 1)) {
                inicioRango = NUM_PAG_INICIO + 1;
                if (paginaActual <= NUM_PAG_INICIO) //En el rango de las primeras
                {
                    finRango = paginaActual + NUM_PAG_INICIO + NUM_PAG_PROX_CENTRO - 1;
                }
                else {
                    finRango = NUM_PAG_INICIO + (2 * NUM_PAG_PROX_CENTRO) + 1; //Ultimo número de la serie.
                }
            }
            else if (paginaActual > (numPaginas - NUM_PAG_FIN - NUM_PAG_PROX_CENTRO)) {
                finRango = numPaginas - NUM_PAG_FIN;
                if (paginaActual > numPaginas - NUM_PAG_FIN) //En el rango de las últimas
                {
                    inicioRango = paginaActual - NUM_PAG_FIN - NUM_PAG_PROX_CENTRO + 1;//finRango - (pNumPaginas - paginaActual + 1);
                }
                else {
                    inicioRango = numPaginas - (NUM_PAG_FIN + (2 * NUM_PAG_PROX_CENTRO)); //Ultimo número de la serie empezando atrás.
                }

                //Avanzamos el inicio de la zona final para que no agrege páginas ya pintadas
                while (inicioRango <= NUM_PAG_INICIO) {
                    inicioRango++;
                }
            }

            if (inicioRango > (NUM_PAG_INICIO + 1)) {
                $('div[section="' + id + '"] .pagination.numbers').append(`<span>...</span>`);
            }


            for (i = inicioRango; i <= finRango; i++) {
                if (i > numPaginas) //Hemos excedio el número máximo de páginas, así que dejamos de pintar.
                {
                    break;
                }

                if (i == paginaActual) {
                    $('div[section="' + id + '"] .pagination.numbers').append(`<li class="actual"><a page="${i}" >${i}</a></li>`);
                }
                else {
                    $('div[section="' + id + '"] .pagination.numbers').append(`<li><a href="javascript: void(0)" page="${i}" >${i}</a></li>`);
                }
            }

            if (finRango < numPaginas) {
                //Continuamos si ha más páginas que las que ya hemos pintado
                inicioRango = numPaginas - NUM_PAG_FIN + 1;

                if ((inicioRango - 1) > finRango) {
                    $('div[section="' + id + '"] .pagination.numbers').append(`<span>...</span>`);
                }

                //Avanzamos el inicio de la zona final para que no agrege páginas ya pintadas
                while (inicioRango <= finRango) {
                    inicioRango++;
                }

                finRango = numPaginas;

                for (i = inicioRango; i <= finRango; i++) {
                    if (i > numPaginas) //Hemos excedio el número máximo de páginas, así que dejamos de pintar.
                    {
                        break;
                    }

                    if (i == paginaActual) {
                        $('div[section="' + id + '"] .pagination.numbers').append(`<li class="actual"><a page="${i}" >${i}</a></li>`);
                    }
                    else {
                        $('div[section="' + id + '"] .pagination.numbers').append(`<li><a href="javascript: void(0)" page="${i}" >${i}</a></li>`);
                    }
                }
            }
        }

        if (paginaActual == 1) {
            $('div[section="' + id + '"] .pagination.arrows').append(`<li class="actual"><a class="primeraPagina">Página anterior</a></li>`);
        }
        else {
            $('div[section="' + id + '"] .pagination.arrows').append(`<li><a href="javascript: void(0)" page="${(paginaActual - 1)}" class="primeraPagina">Página anterior</a></li>`);
        }

        if (paginaActual == numPaginas) {
            $('div[section="' + id + '"] .pagination.arrows').append(`<li class="actual"><a class="ultimaPagina">Página siguiente</a></li>`);
        }
        else {
            $('div[section="' + id + '"] .pagination.arrows').append(`<li><a href="javascript: void(0)" page="${(paginaActual + 1)}" class="ultimaPagina">Página siguiente</a></li>`);
        }
        $('div[section="' + id + '"] .numResultados').text('(' + $('div[section="' + id + '"] article').length + ')');
        this.engancharComportamientosCV();
        accionesCurriculum.init();
    },
    paginarListado: function (sectionID, pagina) {
        $('.panel-group[section="' + sectionID + '"] .panNavegador .pagination.numbers .actual').removeClass('actual');
        $('.panel-group[section="' + sectionID + '"] .panNavegador .pagination.numbers li a[page="' + pagina + '"]').parent().addClass('actual');
        this.repintarListadoTab(sectionID);
    }
    ,
    cambiarTipoPaginacionListado: function (sectionID, itemsPagina, texto) {
        $('.panel-group[section="' + sectionID + '"] .panNavegador .dropdown-toggle span').attr('items', itemsPagina);
        $('.panel-group[section="' + sectionID + '"] .panNavegador .dropdown-toggle span').text(texto);
        $('.panel-group[section="' + sectionID + '"] .panNavegador .pagination.numbers .actual').removeClass('actual');
        $('.panel-group[section="' + sectionID + '"] .panNavegador .pagination.numbers li a[page="1"]').parent().addClass('actual');
        this.repintarListadoTab(sectionID);
    }
    ,
    buscarListado: function (sectionID) {
        $('.panel-group[section="' + sectionID + '"] .panNavegador .pagination.numbers .actual').removeClass('actual');
        $('.panel-group[section="' + sectionID + '"] .panNavegador .pagination.numbers li a[page="1"]').parent().addClass('actual');
        this.repintarListadoTab(sectionID);
    }
    ,
    ordenarListado: function (sectionID,text,property,asc) {
        $('.panel-group[section="' + sectionID + '"] .panNavegador .pagination.numbers .actual').removeClass('actual');
        $('.panel-group[section="' + sectionID + '"] .panNavegador .pagination.numbers li a[page="1"]').parent().addClass('actual');
        $('.panel-group[section="' + sectionID + '"] .ordenar.dropdown .dropdown-toggle .texto').text(text);
        $('.panel-group[section="' + sectionID + '"] .ordenar.dropdown .dropdown-toggle .texto').attr('property',property);
        $('.panel-group[section="' + sectionID + '"] .ordenar.dropdown .dropdown-toggle .texto').attr('asc',asc);
        this.repintarListadoTab(sectionID);
    },
    cambiarPrivacidadItem: function (sectionID, rdfTypeTab, entityID, isPublic, element) {
        var that = this;
        var item = {};
        item.pIdSection = sectionID;
        item.pRdfTypeTab = rdfTypeTab;
        item.pEntity = entityID;
        item.pIsPublic = isPublic;
        var article = $(element).closest('article');
        MostrarUpdateProgress();
        $.post(urlGuardadoCV + 'ChangePrivacityItem', item, function (data) {
            $.get(urlEdicionCV + 'GetItemMini?pIdSection=' + sectionID + "&pRdfTypeTab=" + rdfTypeTab + "&pEntityID=" + item.pEntity + "&pLang=" + lang, null, function (data) {
                article.replaceWith(that.printHtmlListItem(item.pEntity, data));
                that.repintarListadoTab(sectionID);
                OcultarUpdateProgress();
            });
        });
    },
    cargarEdicionItem: function (sectionID, rdfTypeTab, entityID) {
        var that = this;
        MostrarUpdateProgress();
        $('#modal-editar-entidad form').empty();
		$('#modal-editar-entidad .form-actions .ko').remove();
        $.get(urlEdicionCV + 'GetEdit?pIdSection=' + sectionID + "&pRdfTypeTab=" + rdfTypeTab + "&pEntityID=" + entityID + "&pLang=" + lang, null, function (data) {
            that.printEditItemCV(data, sectionID, rdfTypeTab, entityID);
            OcultarUpdateProgress();

        });
    }

    //Fin de métodos de secciones
    ,
    //Métodos de edición de un item del cv
    printEditItemCV: function (data, sectionID, rdfTypeTab, entityID) {
        $('#modal-editar-entidad form').empty();
        $('#modal-editar-entidad form').attr('entityid', data.entityID);
        $('#modal-editar-entidad form').attr('rdftype', data.rdftype);
        $('#modal-editar-entidad form').attr('sectionID', sectionID);
        $('#modal-editar-entidad form').attr('rdfTypeTab', rdfTypeTab);
        //TODO ¿se usa, cambiar de nombre?
        $('#modal-editar-entidad form').attr('entityload', entityID);
        for (var i = 0; i < data.sections.length; i++) {
            var collapsed = ""
            var ariaexpanded = "true";
            var show = "show";
            if (i > 0) {
                collapsed = "collapsed";
                ariaexpanded = "false";
                show = "";
            }
            var section =
                `<div class="simple-collapse">
								<a class="collapse-toggle ${collapsed}" data-toggle="collapse" href="#collapse-${i}" role="button" aria-expanded="${ariaexpanded}" aria-controls="collapse-${i}">${data.sections[i].title}</a>
								<div class="collapse ${show}" id="collapse-${i}" style="">
									<div class="simple-collapse-content">${this.printRowsEdit(data.sections[i].rows)}</div>
								</div>
							</div>`;
            $('#modal-editar-entidad form').append(section);
        }
        iniciarSelects2.init();
        iniciarDatepicker.init();		
		this.repintarListadoEntity();		
		this.engancharComportamientosCV();
    },
	printEditEntity: function (modalContenedor,data, rdfType) {
        $(modalContenedor+' form').empty();
        $(modalContenedor+' form').attr('entityid', data.entityID);
        $(modalContenedor+' form').attr('rdftype', data.rdftype);
        for (var i = 0; i < data.sections.length; i++) {
            var collapsed = ""
            var ariaexpanded = "true";
            var show = "show";
            if (i > 0) {
                collapsed = "collapsed";
                ariaexpanded = "false";
                show = "";
            }
            var section =
                `<div class="simple-collapse">
								<a class="collapse-toggle ${collapsed}" data-toggle="collapse" href="#collapse-${i}" role="button" aria-expanded="${ariaexpanded}" aria-controls="collapse-${i}">${data.sections[i].title}</a>
								<div class="collapse ${show}" id="collapse-${i}" style="">
									<div class="simple-collapse-content">${this.printRowsEdit(data.sections[i].rows)}</div>
								</div>
							</div>`;
            $(modalContenedor+' form').append(section);
        }
        iniciarSelects2.init();
        iniciarDatepicker.init();		
		this.repintarListadoEntity();		
		this.engancharComportamientosCV();
    },
    printRowsEdit: function (rows) {
        var rowsHtml = "";
        for (var i = 0; i < rows.length; i++) {
            rowsHtml += this.printRowEdit(rows[i]);
        }
        return rowsHtml;
    },
    printRowEdit: function (row) {
        var rowHtml = `<div class="custom-form-row">`
        for (var k = 0; k < row.properties.length; k++) {
            rowHtml += this.printPropertyEdit(row.properties[k]);
        }
        rowHtml += `</div>`;

        return rowHtml;
    },
    printPropertyEdit: function (property) {
        //Estilos para el contenedor
        var css = "";
        switch (property.width) {
            case 1:
                css = '';
                break;
            case 2:
                css = 'expand-2';
                break;
            case 3:
                css = 'full-group';
                break;
            default:
                css = '';
                break;
        }
        
		var value = "";
		if(!property.multiple)
		{
			for (var i = 0; i < property.values.length; i++) {
				value = property.values[i];
			}
		}
		
		var required="";
		if(property.required)
		{
			required=" *";
		}

		var rdftype='';	
		if (!property.multiple) {
			var htmlInput='';
			
			switch (property.type) {
				case 'text':
					htmlInput=this.printPropertyEditTextInput(property.property,property.placeholder,value,property.required,false);
					break;
				case 'number':
					htmlInput=this.printPropertyEditNumberInput(property.property,property.placeholder,value,property.required);
					break;
				case 'selectCombo':
					htmlInput=this.printSelectCombo(property.property, value, property.comboValues,property.required);
					break;
				case 'textarea':
					htmlInput=this.printPropertyEditTextArea(property.property,property.placeholder,value,property.required);
					break;
				case 'date':
					htmlInput=this.printPropertyEditDate(property.property,property.placeholder,value,property.required);
					break;
				case 'auxEntity':
				case 'auxEntityAuthorList':
					if(property.type=='auxEntityAuthorList')
					{
						css+=" entityauxauthorlist";	
					}
					css+=" entityauxcontainer";					
					if(property.required)
					{
						css+=" obligatorio";
					}
					if(value!='')
					{
						htmlInput=`<div class='item aux entityaux' propertyrdf='${property.property}' rdftype='${property.entityAuxData.rdftype}' about='${value}'>
							${this.printRowsEdit(property.entityAuxData.entities[value])}
							</div>`;
					}else{
						htmlInput=`<div class='item aux entityaux' propertyrdf='${property.property}' rdftype='${property.entityAuxData.rdftype}' about=''>
							${this.printRowsEdit(property.entityAuxData.rows)}
							</div>`;
					}					
					break;
				case 'entity':		
					css+=" entitycontainer";
					if(property.required)
					{
						css+=" obligatorio";
					}
					rdftype=` rdftype='${property.entityData.rdftype}'`;
					htmlInput+=`<div class='item added entity' propertyrdf='${property.property}' rdftype='${property.entityData.rdftype}' about='${value}'>`;
					
					htmlInput+=this.printPropertyEditTextInput(property.property,property.placeholder,value,property.required);
					
					//Pintamos el título
					if(property.values.length>0 && property.values[0]!=null && property.entityData.titles[property.values[0]]!=null)
					{
						htmlInput+=`
						<span class="title" loaded="true" route="${property.entityData.titles[property.values[0]].route}">${property.entityData.titles[property.values[0]].value}</span> `;
					}else
					{
						htmlInput+=`
						<span class="title" loaded="true" route="${property.entityData.titleConfig.route}"></span> `;				
					}
					
					
					//Pintamos las propiedades
					if(property.entityData.propertiesConfig!=null)
					{
						for (var prop in property.entityData.propertiesConfig) {
							if(property.values.length>0 && property.values[0]!=null && property.entityData.properties[property.values[0]]!=null)						
							{	
								htmlInput+=`
							<span class="property" loaded="true" name="${property.entityData.properties[property.values[0]][prop].name}" route="${property.entityData.properties[property.values[0]][prop].route}">${property.entityData.properties[property.values[0]][prop].value}</span> `;
							}else
							{
								htmlInput+=`<span class="property" loaded="true" name="${property.entityData.propertiesConfig[prop].name}" route="${property.entityData.propertiesConfig[prop].route}"></span> `;
							}
						}
					}
							
					htmlInput+=`</div>`;	
										
					break;
			}
			return `<div class="form-group ${css}" ${rdftype}>
					<label class="control-label d-block">${property.title}${required}</label>
					${htmlInput}
				</div>`;
		}else{
			css+=' multiple';
					
			
			var htmlMultiple=`<div class='item aux'>`;
			if(property.type=='auxEntity' || property.type=='auxEntityAuthorList')
			{				
				htmlMultiple=`<div class='item aux entityaux' propertyrdf='${property.property}' rdftype='${property.entityAuxData.rdftype}' about=''>`;
			}else if(property.type=='entity')
			{
				htmlMultiple=`<div class='item aux entity' propertyrdf='${property.property}' rdftype='${property.entityData.rdftype}' about=''>`;
			}
			switch (property.type) {
				case 'text':
					htmlMultiple+=this.printPropertyEditTextInput(property.property,property.placeholder,'',property.required,false);
					break;
				case 'number':
					htmlMultiple=this.printPropertyEditNumberInput(property.property,property.placeholder,value,property.required);
					break;
				case 'selectCombo':
					htmlMultiple+=this.printSelectCombo(property.property, '', property.comboValues,property.required);
					break;
				case 'textarea':
					htmlMultiple+=this.printPropertyEditTextArea(property.property,property.placeholder,'',property.required);
					break;
				case 'date':
					htmlMultiple+=this.printPropertyEditDate(property.property,property.placeholder,'',property.required);
					break;
				case 'auxEntity':
				case 'auxEntityAuthorList':
					htmlMultiple+=this.printRowsEdit(property.entityAuxData.rows);
					break;
				case 'entity':
					htmlMultiple+=this.printPropertyEditTextInput(property.property,property.placeholder,value,property.required);
					break;
			}	
			if(property.type=='auxEntity'||property.type=='auxEntityAuthorList')
			{
				if(property.entityAuxData.propertyOrder!=null && property.entityAuxData.propertyOrder!='')
				{
					htmlMultiple+=`
					<input propertyrdf="${property.entityAuxData.propertyOrder}" value="" type="hidden">`;
				}
				htmlMultiple+=`
					<span class="title" loaded="false" route="${property.entityAuxData.titleConfig.route}"></span> `;				
				
				if(property.entityAuxData.propertiesConfig!=null)
				{
					for (var prop in property.entityAuxData.propertiesConfig) {		
						htmlMultiple+=`
						<span class="property" loaded="false" name="${property.entityAuxData.propertiesConfig[prop].name}" route="${property.entityAuxData.propertiesConfig[prop].route}"></span> `;
					}
				}
			}
			if(property.type=='entity')
			{
				htmlMultiple+=`
					<span class="title" loaded="false" route="${property.entityData.titleConfig.route}"></span> `;				
				
				if(property.entityData.propertiesConfig!=null)
				{
					for (var prop in property.entityData.propertiesConfig) {		
						htmlMultiple+=`
						<span class="property" loaded="false" name="${property.entityData.propertiesConfig[prop].name}" route="${property.entityData.propertiesConfig[prop].route}"></span> `;
					}
				}
			}
			if(property.type!='auxEntity' && property.type!='entity' && property.type!='auxEntityAuthorList')			
			{
				htmlMultiple+= this.printAddButton();
			}
			
			htmlMultiple+='</div>';	
			var order="";
			if(property.type=='auxEntity'||property.type== 'auxEntityAuthorList')
			{
				css+=" entityauxcontainer";		
				if(property.type=='auxEntityAuthorList')
				{
					css+=" entityauxauthorlist";	
				}
				if(property.entityAuxData.propertyOrder!=null && property.entityAuxData.propertyOrder!='')
				{
					order= 'order="'+property.entityAuxData.propertyOrder+'"';
				}
			}
			if(property.type=='entity')
			{
				css+=" entitycontainer";
				rdftype=` rdftype='${property.entityData.rdftype}'`;
			}
			if(property.required)
			{
				css+=" obligatorio";
			}
			for (var valor in property.values) {				
				if(property.type=='auxEntity' || property.type=='auxEntityAuthorList')
				{
					htmlMultiple+=`<div class='item added entityaux' propertyrdf='${property.property}' rdftype='${property.entityAuxData.rdftype}' about='${property.values[valor]}'>`;
				}else if(property.type=='entity')
				{
					htmlMultiple+=`<div class='item added entity' propertyrdf='${property.property}' rdftype='${property.entityData.rdftype}' about='${property.values[valor]}'>`;
				}else
				{
					htmlMultiple+=`	<div class='item added'>`;
				}
				switch (property.type) {
					case 'text':
						htmlMultiple+=this.printPropertyEditTextInput(property.property,property.placeholder,property.values[valor],property.required,true);
						break;
					case 'number':
						htmlMultiple+=this.printPropertyEditNumberInput(property.property,property.placeholder,property.values[valor],property.required,true);
						break;
					case 'selectCombo':
						htmlMultiple+=this.printSelectCombo(property.property, property.values[valor], property.comboValues,property.required,true);
						break;
					case 'textarea':
						htmlMultiple+=this.printPropertyEditTextArea(property.property,property.placeholder,property.values[valor],property.required,true);
						break;
					case 'date':
						htmlMultiple+=this.printPropertyEditDate(property.property,property.placeholder,property.values[valor],property.required,true);
						break;
					case 'auxEntity':	
					case 'auxEntityAuthorList':					
						htmlMultiple+=this.printRowsEdit(property.entityAuxData.entities[property.values[valor]]);
						if(property.entityAuxData.propertyOrder!=null && property.entityAuxData.propertyOrder!='')
						{
							//Pintamos el orden
							if(property.values[valor]!=null && property.entityAuxData.childsOrder[property.values[valor]]!=null)
							{
								htmlMultiple+=`
								<input propertyrdf="${property.entityAuxData.propertyOrder}" value="${property.entityAuxData.childsOrder[property.values[valor]]}" type="hidden">`;
							}else
							{
								htmlMultiple+=`
								<input propertyrdf="${property.entityAuxData.propertyOrder}" value="" type="hidden">`;
							}							
						}
						
						//Pintamos el título
						if(property.values[valor]!=null && property.entityAuxData.titles[property.values[valor]]!=null)
						{
							htmlMultiple+=`
							<span class="title" loaded="true" route="${property.entityAuxData.titles[property.values[valor]].route}">${property.entityAuxData.titles[property.values[valor]].value}</span> `;
						}
						
						//Pintamos las propiedades
						if(property.values[valor]!=null && property.entityAuxData.properties[property.values[valor]]!=null)
						{
							for (var prop in property.entityAuxData.properties[property.values[valor]]) {		
								htmlMultiple+=`
								<span class="property" loaded="true" name="${property.entityAuxData.properties[property.values[valor]][prop].name}" route="${property.entityAuxData.properties[property.values[valor]][prop].route}">${property.entityAuxData.properties[property.values[valor]][prop].value}</span> `;
							}
						}
						
						
						break;
					case 'entity':	
						htmlMultiple+=this.printPropertyEditTextInput(property.property,property.placeholder,value,property.required);
						
						//Pintamos el título
						if(property.values[valor]!=null && property.entityData.titles[property.values[valor]]!=null)
						{
							htmlMultiple+=`
							<span class="title" loaded="true" route="${property.entityData.titles[property.values[valor]].route}">${property.entityData.titles[property.values[valor]].value}</span> `;
						}
						
						//Pintamos las propiedades
						if(property.values[valor]!=null && property.entityData.properties[property.values[valor]]!=null)
						{
							for (var prop in property.entityData.properties[property.values[valor]]) {		
								htmlMultiple+=`
								<span class="property" loaded="true" name="${property.entityData.properties[property.values[valor]][prop].name}" route="${property.entityData.properties[property.values[valor]][prop].route}">${property.entityData.properties[property.values[valor]][prop].value}</span> `;
							}
						}
						
						
						break;
				}
				if(property.type!='auxEntity' && property.type!='entity' && property.type!='auxEntityAuthorList')			
				{
					htmlMultiple+=this.printDeleteButton();
				}
				htmlMultiple+='</div>';	
			}			
			return `<div class="form-group ${css}" ${order} ${rdftype}>
					<label class="control-label d-block">${property.title}${required}</label>
					${htmlMultiple}
				</div>`;
        }		
    },
	printPropertyEditTextInput: function (property,placeholder,value,required,pDisabled) {
		var css="";
		if(required)
		{
			css="obligatorio";
		}
		var prop_property='propertyrdf';
		var disabled='';
		if(pDisabled)
		{
			disabled='disabled';
		}
		return `<input ${disabled} propertyrdf="${property}" placeholder="${placeholder}" value="${value}" type="text" class="form-control not-outline ${css}">`;
	},
	printPropertyEditNumberInput: function (property,placeholder,value,required,pDisabled) {
		var css="";
		if(required)
		{
			css="obligatorio";
		}
		var prop_property='propertyrdf';
		var disabled='';
		if(pDisabled)
		{
			disabled='disabled';
		}
		return `<input ${disabled} propertyrdf="${property}" placeholder="${placeholder}" value="${value}" type="number" class="form-control not-outline ${css}">`;
	},
	printPropertyEditDate: function (property,placeholder,value,required,pDisabled) {
		var valueDate="";
		if (value != '') {
			valueDate = value.substring(6, 8) + "/" + value.substring(4, 6) + "/" + value.substring(0, 4);
		}
		var css="";
		if(required)
		{
			css="obligatorio";
		}
		var disabled='';
		if(pDisabled)
		{
			disabled='disabled';
		}
		return `<input propertyrdf="${property}" value="${value}" type="hidden">
		<input ${disabled} propertyrdf="${property}" placeholder="${placeholder}" value="${valueDate}" type="text" class="form-control aux not-outline form-group-date datepicker ${css}">
				<span class="material-icons form-group-date">today</span>`;
	},
	printPropertyEditTextArea: function (property,placeholder,value,required,pDisabled) {
		var css="";
		if(required)
		{
			css="obligatorio";
		}
		var disabled='';
		if(pDisabled)
		{
			disabled='disabled';
		}
		return `<textarea ${disabled} propertyrdf="${property}" placeholder="${placeholder}" type="text" class="form-control not-outline ${css}">${value}</textarea>`;
	},
    printSelectCombo: function (property, pId, pItems,required,pDisabled) {
		var disabled='';
		if(pDisabled)
		{
			disabled='disabled';
		}
		var css="";
		if(required)
		{
			css="obligatorio";
		}
        var selector = `<select ${disabled} propertyrdf="${property}" class="js-select2 ${css}" data-select-search="true">`;
        for (var propiedad in pItems) {
            if (propiedad == pId) {
                selector += '<option selected value="' + propiedad + '">' + pItems[propiedad] + '</option>';
            } else {
                selector += '<option value="' + propiedad + '">' + pItems[propiedad] + '</option>';
            }
        }
        selector += "</select>";
        return selector;
    },
    printAddButton: function () {
		return `	<div class="acciones-listado-edicion">
						<div class="wrap">
							<ul class="no-list-style d-flex align-items-center">
								<li>
									<a class="btn btn-outline-grey add">
										<span class="texto">Añadir</span>
										<span class="material-icons">add</span>
									</a>
								</li>
							</ul>
						</div>
					</div>`;
	},
    printDeleteButton: function () {
		return `	<div class="acciones-listado-edicion">
						<div class="wrap">
							<ul class="no-list-style d-flex align-items-center">
								<li>
									<a class="btn btn-outline-grey delete">
										<span class="texto">Eliminar</span>
										<span class="material-icons">delete</span>
									</a>
								</li>
							</ul>
						</div>
					</div>`;
	},
    repintarListadoEntity: function () {
		var that=this;		
		
        $( ".entityauxcontainer,.entitycontainer" ).each(function() {
			var aux=true;
			if($(this).attr('class').indexOf('entitycontainer')>-1)
			{
				aux=false;
			}
			var multiple=false;
			if($(this).hasClass('multiple'))
			{
				multiple=true;
			}
			if($(this).attr('idtemp')==null)
			{
				$(this).attr('idtemp',RandomGuid());
			}
			
			var idTemp=	$(this).attr('idtemp');
			$(this).children('.simple-collapse-content').remove();
			var items= $(this).children('.item.added.entityaux');
			if(!aux)
			{
				items= $(this).children('.item.added.entity');
				$(this).children('.item.aux:not(.entity)').remove();
			}
			
			if($(this).attr('order')!=null && $(this).attr('order')!='')
			{
				//Ordenamos
				var ordenProperty=$(this).attr('order');
				
				var maxOrder=1;
				$( items ).each(function() {
					var orderIn=$(this).children('input[propertyrdf="'+ordenProperty+'"]').val();
					if(orderIn!='' && parseInt(orderIn)>=maxOrder)
					{
						maxOrder=parseInt(orderIn);
					}
				});
				
				items = items.sort(function (a, b) {
					var ordenA=$(a).children('input[propertyrdf="'+ordenProperty+'"]').val();
					if(ordenA=='')
					{
						$(a).children('input[propertyrdf="'+ordenProperty+'"]').val(maxOrder);
						ordenA=maxOrder;
						maxOrder+=1;
					}
					ordenA=parseInt(ordenA);
					var ordenB=$(b).children('input[propertyrdf="'+ordenProperty+'"]').val();
					if(ordenB=='')
					{
						$(b).children('input[propertyrdf="'+ordenProperty+'"]').val(maxOrder);
						ordenB=maxOrder;
						maxOrder+=1;
					}
					ordenB=parseInt(ordenB);
					if (ordenA > ordenB)
					{
						return 1;
					}else if (ordenA < ordenB)
					{
						return -1;
					}else
					{
						return 0;
					}
				});
				//Asignamos orden a los que no tengan
				var ordenActual=0;
				$(items).each(function() {
					if(parseInt($(this).children('input[propertyrdf="'+ordenProperty+'"]').val())<=ordenActual)
					{
						$(this).children('input[propertyrdf="'+ordenProperty+'"]').val(ordenActual+1);					
					}
					ordenActual=parseInt($(this).children('input[propertyrdf="'+ordenProperty+'"]').val());
				})
			}
			
			var htmAccionesItems='';
			if(multiple)
			{
				if(items.length>0)
				{
					if($(this).attr('selecteditem')==null)
					{
						$(this).attr('selecteditem',$($(items)[0]).attr('about'));
					}
					
					htmAccionesItems+=that.pintarListadoEntityOrden($(this).attr('order'))+`<li>
											<a class="btn btn-outline-grey edit">
												<span class="texto">Editar</span>
												<span class="material-icons">edit</span>
											</a>
										</li>
										<li>
											<a class="btn btn-outline-grey delete">
												<span class="texto">Eliminar</span>
												<span class="material-icons">delete</span>
											</a>
										</li>`;
				}			
			
				var htmlAcciones=`
									<div class="simple-collapse-content">
										<div class="acciones-listado acciones-listado-edicion">
											<div class="wrap">
												<ul class="no-list-style d-flex align-items-center">
													${htmAccionesItems}
												</ul>
											</div>
											<div class="wrap">
												<ul class="no-list-style d-flex align-items-center">
													<li>
														<a class="btn btn-outline-grey add">
															<span class="texto">Añadir</span>
															<span class="material-icons">person_add</span>
														</a>
													</li>
												</ul>
												<div id="buscador" class="buscador">
													<input type="text" id="txtBusquedaPrincipal" class="not-outline text txtBusqueda autocompletar personalizado ac_input" placeholder="Escribe algo..." autocomplete="off">
													<span class="botonSearch">
														<span class="material-icons">search</span>
													</span>
												</div>
											</div>
										</div>
										<div class="resource-list listView">
											<div class="resource-list-wrap">
												${that.repintarListadoEntityItems(items,idTemp,$(this).attr('order'))}
											</div>
										</div>
									</div>
								</div>`;
				$(this).append(htmlAcciones);
			}else
			{
				htmlAcciones="";
				if($($(items)[0]).find('input').val()!='')
				{
					htmlAcciones+=`	<div class="item aux">
										${that.repintarEntityItem($(items[0]))}
											<div class="acciones-listado-edicion">
												<div class="wrap">
													<ul class="no-list-style d-flex align-items-center">
														<li>
															<a class="btn btn-outline-grey edit">
																<span class="texto">Editar</span>
																<span class="material-icons">edit</span>
															</a>
														</li>
														<li>
															<a class="btn btn-outline-grey delete">
																<span class="texto">Eliminar</span>
																<span class="material-icons">delete</span>
															</a>
														</li>
													</ul>
												</div>
											</div>
										</div>`;
				}else
				{					
					htmlAcciones+=`	<div class="item aux">
										<input disabled="" value="" type="text" class="form-control not-outline ">	<div class="acciones-listado-edicion">
											<div class="wrap">
												<ul class="no-list-style d-flex align-items-center">
													<li>
														<a class="btn btn-outline-grey add">
															<span class="texto">Añadir</span>
															<span class="material-icons">add</span>
														</a>
													</li>
												</ul>
											</div>
										</div>
									</div>`;
				}					
				$(this).append(htmlAcciones);
			}
		});
		this.engancharComportamientosCV();
    },
    repintarListadoEntityItems: function (items,idTemp,propOrden) {
		var that=this;
		var html='';
		var num=0;
		$(items).each(function() {
			num++;
			var checked="";
			if($(this).closest('.entityauxcontainer').attr('selecteditem')!=null 
			&& $(this).closest('.entityauxcontainer').attr('selecteditem')!=''
			&& $(this).closest('.entityauxcontainer').children('div[about="'+$(this).closest('.entityauxcontainer').attr('selecteditem')+'"]').length==1)
			{
				if($(this).attr('about')==$(this).closest('.entityauxcontainer').attr('selecteditem'))
				{
					checked="checked";
				}
			}else if(num==1)
			{
				checked="checked";
				$(this).closest('.entityauxcontainer').attr('selecteditem',$(this).attr('about'));
			}			
			if(propOrden!=null && propOrden!='')
			{
				num=parseInt($(this).children('input[propertyrdf="'+propOrden+'"]').val())
			}
			html+=that.repintarListadoEntityItem(this,num,checked,idTemp);
		});
		return html;
	},
    repintarListadoEntityItem: function (item,num,checked,idTemp) {		
		var that=this;
		/*Cargar*/
		var itemsLoad={};
		itemsLoad.items=[];
		itemsLoad.pLang=lang;
		if($(item).children('span.title[loaded="false"]').length>0)
		{
			//TODO unificar co la siguiente
			var entityID=$(item).attr('about');
			var spanTitle=$(item).children('span.title[loaded="false"]');
			var routeCompleteSplitTitle=spanTitle.attr('route').split('||');
			var entityAux=$(item);
			for (var j = 0; j < routeCompleteSplitTitle.length; j++) {								
				var prop=$(entityAux).children('.custom-form-row').children('.form-group').children('[propertyrdf="'+routeCompleteSplitTitle[j]+'"]');
				if(prop.is('div') && prop.hasClass('enityaux'))
				{
					entityAux=prop;
				}else
				{	
					if($(item).hasClass('entity'))
					{
						var itemLoad={};
						if(entityID.indexOf('http')==0)
						{
							itemLoad.id=entityID;
							itemLoad.about=entityID;
							itemLoad.route=spanTitle.attr('route');
							itemLoad.routeComplete=spanTitle.attr('route');
							itemsLoad.items.push(itemLoad);
							spanTitle.attr('loaded','pending');
							break;
						}
					}else if(prop.is('div') && prop.hasClass('entity'))
					{						
						var itemLoad={};
						if(prop.children('[propertyrdf="'+routeCompleteSplitTitle[j]+'"]').val()!='')
						{
							itemLoad.id=prop.children('[propertyrdf="'+routeCompleteSplitTitle[j]+'"]').val();
							itemLoad.about=spanTitle.closest('.entityaux').attr('about');
							itemLoad.route="";
							itemLoad.routeComplete=spanTitle.attr('route');
							for (var j2 = j+1; j2 < routeCompleteSplitTitle.length; j2++) {
								itemLoad.route+=routeCompleteSplitTitle[j2]+"||";
							}
							itemLoad.route=itemLoad.route.substring(0,itemLoad.route.length-2);
							itemsLoad.items.push(itemLoad);
							spanTitle.attr('loaded','pending');
							break;
						}
					}else if(j+1==routeCompleteSplitTitle.length && prop.val()!='')
					{
						spanTitle.text(prop.val());							
					}
					spanTitle.attr('loaded','true');
					break;					
				}
			}
		}
		if($(item).children('span.property[loaded="false"]').length>0)
		{			
			$(item).children('span.property[loaded="false"]').each(function() {
				var entityID=$(item).attr('about');
				var entityAux=$(item);
				var spanProperty=$(this);
				var routeCompleteSplitProperty=$(this).attr('route').split('||');				
				for (var k = 0; k< routeCompleteSplitProperty.length; k++) {
                    var prop=$(entityAux).children('.custom-form-row').children('.form-group').children('[propertyrdf="'+routeCompleteSplitProperty[k]+'"]');
					if(prop.is('div') && prop.hasClass('enityaux'))
					{
						entityAux=prop;
					}else
					{
						if($(item).hasClass('entity'))
						{
							var itemLoad={};
							if(entityID.indexOf('http')==0)
							{
								itemLoad.id=entityID;
								itemLoad.about=entityID;
								itemLoad.route=spanProperty.attr('route')
								itemLoad.routeComplete=spanProperty.attr('route');
								itemsLoad.items.push(itemLoad);
								spanProperty.attr('loaded','pending');
								break;
							}
						}else if(prop.is('div') && prop.hasClass('entity'))
						{						
							var itemLoad={};
							if(prop.children('[propertyrdf="'+routeCompleteSplitProperty[j]+'"]').val()!='')
							{
								itemLoad.id=prop.children('[propertyrdf="'+routeCompleteSplitProperty[j]+'"]').val();
								itemLoad.about=spanProperty.closest('.entityaux').attr('about');
								itemLoad.route="";
								itemLoad.routeComplete=spanProperty.attr('route');
								for (var j2 = j+1; j2 < routeCompleteSplitProperty.length; j2++) {
									itemLoad.route+=routeCompleteSplitProperty[j2]+"||";
								}
								itemLoad.route=itemLoad.route.substring(0,itemLoad.route.length-2);
								itemsLoad.items.push(itemLoad);
								spanProperty.attr('loaded','pending');
								break;
							}
						}else if(j+1==routeCompleteSplitProperty.length && prop.val()!='')
						{
							spanProperty.text(prop.val());							
						}
						spanProperty.attr('loaded','true');
						break;	
					}
				}
				
			});			
		}
		if(itemsLoad.items.length>0)
		{
			MostrarUpdateProgress();
			$.post(urlEdicionCV + 'LoadProps', itemsLoad, function (data) {
				for (var i = 0; i< data.items.length; i++) {
					var contenedor=$('div[about="'+ data.items[i].about+'"]').children('span[route="'+ data.items[i].routeComplete+'"]');
					contenedor.attr('loaded','true');
					contenedor.text(data.items[i].values.join(', '));
				};
				that.repintarListadoEntity();
				OcultarUpdateProgress();
			});
		}
		
		
		var title="";
		var props="";
		if($(item).children('span.title[loaded="true"]').length>0)
		{
			title=$(item).children('span.title[loaded="true"]').text();
		}
		if($(item).children('span.property[loaded="true"]').length>0)
		{
			$(item).children('span.property[loaded="true"]').each(function() {
				if($(this).text().trim()!='')
				{
					props+='<p>';
					if($(this).attr('name')!=null && $(this).attr('name')!='')
					{
						props+=$(this).attr('name')+": " ;
					}
					props+=$(this).text().trim() ;
					props+='</p>';
				}
			});			
		}
		
		return `<article class="resource" about="${$(item).attr('about')}" order="${num}">
					<div class="custom-control themed little custom-radio">
						<input type="radio" id="edicion-listado-${idTemp}${num}" name="edicion-listado-${idTemp}" class="custom-control-input" ${checked}>
						<label class="custom-control-label" for="edicion-listado-${idTemp}${num}"></label>
					</div>
					<div class="wrap">
						<div class="middle-wrap">
							<div class="title-wrap">
								<h2 class="resource-title">${title}</h2>
							</div>
							<div class="content-wrap">
								<div class="description-wrap">
									<div class="desc">
									${props}
									</div>
								</div>
							</div>
						</div>
					</div>
				</article>`;
	},
    repintarEntityItem: function (item) {				
		var that=this;
		/*Cargar*/
		var itemsLoad={};
		itemsLoad.items=[];
		itemsLoad.pLang=lang;
		if($(item).children('span.title[loaded="false"]').length>0)
		{
			var spanTitle=$(item).children('span.title[loaded="false"]');
			
			var itemLoad={};
			itemLoad.id=$(item).closest('.entity').attr('about');
			itemLoad.about=$(item).closest('.entity').attr('about');
			itemLoad.route=spanTitle.attr('route');
			itemsLoad.items.push(itemLoad);
			spanTitle.attr('loaded','pending');
		}
		if($(item).children('span.property[loaded="false"]').length>0)
		{			
			$(item).children('span.property[loaded="false"]').each(function() {
				var spanProperty=$(this);
				
				var itemLoad={};								
				itemLoad.id=$(this).closest('.entity').attr('about');
				itemLoad.about=$(this).closest('.entity').attr('about');
				itemLoad.route=$(this).attr('route');
				itemsLoad.items.push(itemLoad);
				spanProperty.attr('loaded','pending');
			});			
		}
		if(itemsLoad.items.length>0)
		{
			MostrarUpdateProgress();
			$.post(urlEdicionCV + 'LoadProps', itemsLoad, function (data) {
				for (var i = 0; i< data.items.length; i++) {
					var contenedor=$('div[about="'+ data.items[i].about+'"]').children('span[loaded="pending"][route="'+ data.items[i].route+'"]');
					contenedor.attr('loaded','true');
					contenedor.text(data.items[i].values.join(', '));					
				};
				that.repintarListadoEntity();
				OcultarUpdateProgress();
			});
		}
		
		
		var title="";
		var props="";
		if($(item).children('span.title[loaded="true"]').length>0)
		{
			title=$(item).children('span.title[loaded="true"]').text();
		}
		if($(item).children('span.property[loaded="true"]').length>0)
		{
			$(item).children('span.property[loaded="true"]').each(function() {
				if($(this).text().trim()!='')
				{
					props+=' ';
					if($(this).attr('name')!=null && $(this).attr('name')!='')
					{
						props+=$(this).attr('name')+": " ;
					}
					props+=$(this).text().trim() ;
				}
			});			
		}
		return `<input disabled="" value="${title}${props}" type="text" class="form-control not-outline ">`;
	},
    pintarListadoEntityOrden: function (propOrder) {
		if(propOrder!=null && propOrder!='')
		{
			return 	`<li>
						<a class="btn btn-outline-grey subir">
							<span class="texto">Subir</span>
							<span class="material-icons">arrow_drop_up</span>
						</a>
					</li>
					<li>
						<a class="btn btn-outline-grey bajar">
							<span class="texto">Bajar</span>
							<span class="material-icons">arrow_drop_down</span>
						</a>
					</li>`;
		}
		return "";
	},
    cargarEdicionEntidad: function (button,rdfType, entityID) {
        var that = this;
        MostrarUpdateProgress();
		var modalPopUp=$(button).closest('.modal-top').attr('id');
		if(modalPopUp=='modal-editar-entidad')
		{
			modalPopUp='modal-editar-entidad-0'
		}else
		{
			modalPopUp='modal-editar-entidad-'+(parseInt(modalPopUp.substring(21))+1);
		}
		modalPopUp='#'+modalPopUp;
		$(modalPopUp).modal('show');	
        $(modalPopUp+' form').empty();
		$(modalPopUp+' .form-actions .ko').remove();
        $.get(urlEdicionCV + 'GetEditEntity?pRdfType=' + rdfType + "&pEntityID=" + entityID + "&pLang=" + lang, null, function (data) {
            that.printEditEntity(modalPopUp,data,  rdfType);
            OcultarUpdateProgress();
        });
    }
    //Fin de métodos de edición
    ,
    engancharComportamientosCV: function () {
        var that = this;
		//LISTADOS		
        //Paginación
        $('.panel-group .panNavegador ul.pagination li a').off('click').on('click', function (e) {
            if ($(this).attr('page') != null) {
                var sectionID = $(this).closest('.panel-group').attr('section');
                var pagina = $(this).attr('page');
                that.paginarListado(sectionID, pagina);
            }
        });
        //Cambiar tipo de paginación
        $('.panel-group .panNavegador .item-dropdown').off('click').on('click', function (e) {
            if ($(this).attr('items') != null) {
                var sectionID = $(this).closest('.panel-group').attr('section');
                var itemsPagina = parseInt($(this).attr('items'));
                var texto = $(this).text();
                that.cambiarTipoPaginacionListado(sectionID, itemsPagina, texto);
            }
        });
        //Buscador
        $('.panel-group input.txtBusqueda').off('keyup').on('keyup', function (e) {
            var sectionID = $(this).closest('.panel-group').attr('section');
            that.buscarListado(sectionID);
        });
        //Ordenar
        $('.panel-group .ordenar.dropdown .dropdown-menu a').off('click').on('click', function (e) {
            var sectionID = $(this).closest('.panel-group').attr('section');		
            that.ordenarListado(sectionID,$(this).text(),$(this).attr('property'),$(this).attr('asc'));
        });
        //Publicar/despublicar
        $('.panel-group .resource-list .publicaritem,.panel-group .resource-list .despublicaritem').off('click').on('click', function (e) {
            var sectionID = $(this).closest('.panel-group').attr('section');
            var rdfTypeTab = $(this).closest('.cvTab').attr('rdftype');
            var entityID = $(this).attr('data-id');
            var isPublic = true;
            if ($(this).hasClass('despublicaritem')) {
                isPublic = false;
            }
            var element = $(this);
            that.cambiarPrivacidadItem(sectionID, rdfTypeTab, entityID, isPublic, element);
        });
        //Eliminar item
        $('.panel-group .resource-list .eliminar').off('click').on('click', function (e) {
            //Usa el popup
            $("#modal-eliminar").modal("show");
			var sectionID = $(this).closest('.panel-group').attr('section');
            var entityID = $(this).attr('data-id');
            $('#modal-eliminar .btn-outline-primary').attr('href', 'javascript:edicionCV.eliminarItem("' + sectionID + '","' + entityID + '");$("#modal-eliminar").modal("hide");');
        });

		
		//Fechas hidden
        $('input.datepicker').off('change').on('change', function (e) {
            if($(this).val().length==10)
			{
				$(this).parent().children('input[type="hidden"]').val($(this).val().substring(6, 10) + $(this).val().substring(3, 5) + $(this).val().substring(0, 2) + "000000");
			}else{
				$(this).parent().children('input[type="hidden"]').val('');
			}
        });
		//Seleccion combos
        $('.entityauxcontainer>.simple-collapse-content input').off('change').on('change', function (e) {
            $(this).closest('.entityauxcontainer').attr('selecteditem',$(this).closest('article').attr('about'));
        });
		
        //Cargar edición/creación de item
        $('.panel-group .resource-list h2.resource-title a,.panel-group .acciones-listado a.aniadirEntidad').off('click').on('click', function (e) {
            $('#modal-editar-entidad').modal('show');
			var sectionID = $(this).closest('.panel-group').attr('section');
            var entityID = "";
            if ($(this).attr('data-id') != null) {
                entityID = $(this).attr('data-id');
            }
            var rdfTypeTab = $(this).closest('.cvTab').attr('rdftype');
            that.cargarEdicionItem(sectionID, rdfTypeTab, entityID);
        });
        
		
		//Añadir propiedad multiple que no es otra entidad 		
		$('.multiple:not(.entityauxcontainer ):not(.entitycontainer ) .acciones-listado-edicion .add').off('click').on('click', function (e) {	
			var contenedor=$(this).closest('.multiple');
			var item=$(this).closest('.item');
			if(item.find('input, select, textarea').val()=='')
			{
				return;
			}
			var itemClone=item.clone();
			itemClone.find('input, select, textarea').each(function (index) {				
				if ($(this).attr('propertyrdf_aux') != null) {
					$(this).attr('propertyrdf',$(this).attr('propertyrdf_aux'));
					$(this).removeAttr('propertyrdf_aux');					
				}
			});
			itemClone.removeClass('aux');
			itemClone.addClass('added');
			itemClone.find('input, select, textarea').attr('disabled','');
			itemClone.find('.acciones-listado-edicion').replaceWith(that.printDeleteButton());		
			contenedor.append(itemClone);			
			itemClone.find('input, select, textarea').val(item.find('input, select, textarea').val());
			item.find('input, select, textarea').val('');
			item.find('input, select, textarea').change();
			that.engancharComportamientosCV();
        });
		
		//Eliminar propiedad multiple que no es una entidad
		$('.multiple:not(.entityauxcontainer ):not(.entitycontainer ) .acciones-listado-edicion .delete').off('click').on('click', function (e) {
			var item=$(this).closest('.item').remove();
        });		
		
		//Mostrar popup entidad nueva/editar auxiliar/editar especiales
		$('.multiple.entityauxcontainer .acciones-listado-edicion .add,.multiple.entityauxcontainer .acciones-listado-edicion .edit').off('click').on('click', function (e) {			
			var edit=$(this).hasClass('edit');
			if($(this).closest('.entityauxauthorlist').length>0)
			{
				if(!edit)
				{
					//Creación
					$('#modal-anadir-autor').modal('show');
					$('#modal-anadir-autor .resultados .form-group.full-group').remove();
					$('#inputsignatures').val('');
				}else
				{
					//Edición
				}
			}else
			{	
				var modalPopUp=$(this).closest('.modal-top').attr('id')
				if(modalPopUp=='modal-editar-entidad')
				{
					modalPopUp='modal-editar-entidad-0'
				}else
				{
					modalPopUp='modal-editar-entidad-'+(parseInt(modalPopUp.substring(21))+1);
				}
				modalPopUp='#'+modalPopUp;
				$(modalPopUp).modal('show');
				
				//IDS
				var contenedor=$(this).closest('.entityauxcontainer');
				var idTemp=$(contenedor).attr('idtemp');			
				var id=RandomGuid();
				if(edit)
				{
					id=$('input[name="edicion-listado-'+idTemp+'"]:checked').closest('article').attr('about');
				}
				//Clonamos la entidad auxiliar vacía
				var entityAux=$(contenedor).children('.item.aux.entityaux[about=""]').clone();
				if(edit)
				{
					entityAux=$(contenedor).children('.item.added.entityaux[about="'+id+'"]').clone();
				}
				if(!edit)
				{
					//Cambiamos cosas del clon
					entityAux.attr('about',id);
					entityAux.removeClass('aux');
					entityAux.addClass('added');
				}
				//Rellenamos el popup
				$(modalPopUp+' .formulario-edicion').empty();
				$(modalPopUp+' .form-actions .ko').remove();
				$(modalPopUp+' .formulario-edicion').append(entityAux);			
				$(modalPopUp).attr('idtemp',idTemp);
				$(modalPopUp).attr('about',id);
				$(modalPopUp).attr('new','true');
				if(edit)
				{
					$(modalPopUp).attr('new','false');
					//Reseteamos los campos para mostrar en el listado
					$(modalPopUp+' .formulario-edicion').children('.entityaux').children('span.title').attr('loaded','false');
					$(modalPopUp+' .formulario-edicion').children('.entityaux').children('span.title').html('');
					$(modalPopUp+' .formulario-edicion').children('.entityaux').children('span.property').attr('loaded','false');
					$(modalPopUp+' .formulario-edicion').children('.entityaux').children('span.property').html('');
				}
				//Cambiamos los id temporales del clon
				$(entityAux.find('div.entityauxcontainer,div.entitycontainer')).each(function() {
					if($(this).attr('idtemp')!=null && $(this).attr('idtemp')!='')
					{
						$(this).attr('idtemp',RandomGuid());
					}
				});	
			}			
			that.repintarListadoEntity();
        });			
		
			
		//Eliminar entidad auxiliar/normal de listado
        $('.multiple.entityauxcontainer .acciones-listado-edicion .delete,.multiple.entitycontainer .acciones-listado-edicion .delete').off('click').on('click', function (e) {
			$("#modal-eliminar").modal("show");
            var entityAux=$(this).closest('.multiple').hasClass('entityauxcontainer');
			//Usa el popup			
			var modalID=$(this).closest('.modal').attr('id');
			var contenedor=$(this).closest('.entitycontainer');
			if(entityAux)
			{
				contenedor=$(this).closest('.entityauxcontainer');
			}
			var idTemp=$(contenedor).attr('idtemp');
			var id=$('input[name="edicion-listado-'+idTemp+'"]:checked').closest('article').attr('about');
			$('#modal-eliminar .btn-outline-primary').attr('href', 'javascript:$("#'+modalID+' div[idtemp=\''+idTemp+'\'] div[about=\''+id+'\']").remove();$("#'+modalID+' div[idtemp=\''+idTemp+'\']").attr(\'remove\',$("div[idtemp=\''+idTemp+'\']").attr(\'remove\')+\'||'+id+'\');edicionCV.repintarListadoEntity();$("#modal-eliminar").modal("hide");');            
        });
		
		//Eliminar entidad normal monovaluada
        $('.entitycontainer>div.item.aux .delete').off('click').on('click', function (e) {       
			$("#modal-eliminar").modal("show");		
			//Usa el popup
			var modalID=$(this).closest('.modal').attr('id');
			var contenedor=$(this).closest('.entitycontainer');
			var idTemp=$(contenedor).attr('idtemp');
			$('#modal-eliminar .btn-outline-primary').attr('href', 'javascript:$("#'+modalID+' div[idtemp=\''+idTemp+'\']>div.item.added.entity").attr("about","");$("#'+modalID+' div[idtemp=\''+idTemp+'\']>div.item.added.entity>input").val("");$("#'+modalID+' div[idtemp=\''+idTemp+'\']>div.item.added.entity>span").attr("loaded","true");$("#'+modalID+' div[idtemp=\''+idTemp+'\']>div.item.added.entity>span").html("");edicionCV.repintarListadoEntity();$("#modal-eliminar").modal("hide");');            
        });
		
		//Mover orden entidad auxiliar
        $('.multiple.entityauxcontainer .acciones-listado-edicion .subir,.multiple.entityauxcontainer .acciones-listado-edicion .bajar').off('click').on('click', function (e) {
			var contenedor=$(this).closest('.entityauxcontainer');
			var idTemp=$(contenedor).attr('idtemp');
			
			var subir=$(this).hasClass('subir');
			var idSeleccionado=$('input[name="edicion-listado-'+idTemp+'"]:checked').closest('article').attr('about');			
			var ordenSeleccionado=$('input[name="edicion-listado-'+idTemp+'"]:checked').closest('article').attr('order');			
			var propertyOrder=$(contenedor).attr('order');
			var valorSeleccionado=parseInt($(contenedor).children('div[about="'+idSeleccionado+'"]').children('input[propertyrdf="'+propertyOrder+'"]').val());		
			var idAnterior=null;
			var ordenAnterior=null;
			var actualProcesado=false;
			var idSiguiente=null;
			var ordenSiguiente=null;
			var items= $(contenedor).children('.simple-collapse-content').find('article');
			$(items).each(function() {
				var actual=$(this).attr('about');
				if(actual==idSeleccionado)
				{
					actualProcesado=true;
					return;
				}
				if(!actualProcesado)
				{
					idAnterior=actual;
					ordenAnterior=$(this).attr('order');
				}
				if(actualProcesado&&idSiguiente==null)
				{
					idSiguiente=actual;
					ordenSiguiente=$(this).attr('order');
				}
			});
			if(subir && idAnterior!=null)
			{
				$(contenedor).children('.item.added.entityaux[about="'+idAnterior+'"]').children('input[propertyrdf="'+propertyOrder+'"]').val(ordenSeleccionado);
				$(contenedor).children('.item.added.entityaux[about="'+idSeleccionado+'"]').children('input[propertyrdf="'+propertyOrder+'"]').val(ordenAnterior);
			}else if(!subir && idSiguiente!=null)
			{
				$(contenedor).children('.item.added.entityaux[about="'+idSiguiente+'"]').children('input[propertyrdf="'+propertyOrder+'"]').val(ordenSeleccionado);
				$(contenedor).children('.item.added.entityaux[about="'+idSeleccionado+'"]').children('input[propertyrdf="'+propertyOrder+'"]').val(ordenSiguiente);
			}
			$(contenedor).attr('selecteditem',idSeleccionado);
			that.repintarListadoEntity();
        });
		
		//Mostrar popup entidad nueva/editar listado
		$('.multiple.entitycontainer .acciones-listado-edicion .add,.multiple.entitycontainer .acciones-listado-edicion .edit').off('click').on('click', function (e) {		
			var modalPopUp=$(this).closest('.modal-top').attr('id')
			if(modalPopUp=='modal-editar-entidad')
			{
				modalPopUp='modal-editar-entidad-0'
			}else
			{
				modalPopUp='modal-editar-entidad-'+(parseInt(modalPopUp.substring(21))+1);
			}
			modalPopUp='#'+modalPopUp;
			$(modalPopUp).modal('show');
			
			
			var edit=$(this).hasClass('edit');
			//IDS
			var contenedor=$(this).closest('.entitycontainer');
			var idTemp=$(contenedor).attr('idtemp');	
			$(modalPopUp).attr('idtemp',idTemp);						
			var id=RandomGuid();
			if(edit)
			{
				id=$('input[name="edicion-listado-'+idTemp+'"]:checked').closest('article').attr('about');
			}
			that.cargarEdicionEntidad($(this),$(contenedor).attr("rdftype"), id);
			that.repintarListadoEntity();
        });
		
		//Mostrar popup entidad nueva/editar NO listado
		$('.entitycontainer:not(.multiple) .acciones-listado-edicion .add,.entitycontainer:not(.multiple) .acciones-listado-edicion .edit').off('click').on('click', function (e) {		
			var modalPopUp=$(this).closest('.modal-top').attr('id')
			if(modalPopUp=='modal-editar-entidad')
			{
				modalPopUp='modal-editar-entidad-0'
			}else
			{
				modalPopUp='modal-editar-entidad-'+(parseInt(modalPopUp.substring(21))+1);
			}
			modalPopUp='#'+modalPopUp;
			$(modalPopUp).modal('show');			
			
			var edit=$(this).hasClass('edit');
			//IDS
			var contenedor=$(this).closest('.entitycontainer');
			var idTemp=$(contenedor).attr('idtemp');
			$(modalPopUp).attr('idtemp',idTemp);			
			var id=RandomGuid();
			if(edit)
			{
				id=$(contenedor).find('.item.added.entity').attr('about');
			}
			that.cargarEdicionEntidad($(this),$(contenedor).attr("rdftype"), id);
			that.repintarListadoEntity();
        });
		
		//Buscar personas por firma
		$('#modal-anadir-autor .validar').off('click').on('click', function (e) {
			that.validarFirmas();		
		});
		
		//Enganchamos comportamiento check firmas		
		$('input.chksignature').change(function (e) {
			var that=this;
			$(this).closest('.form-group.full-group').find('input.chksignature').each(function () {
				if(that!=this)
				{
					$(this).prop('checked', false);
				}
			});
		});
		
		operativaFormularioAutor.init();
		
		//Enganchamos comportamiento buscar	firmas		
		$('.coincidencia-wrap a.form-buscar').off('click').on('click', function (e) {
			$('#modal-anadir-autor .formulario-principal').hide();
			$('#modal-anadir-autor .formulario-codigo').show();
			$('#modal-anadir-autor .formulario-autor').hide();				
			$('#modal-anadir-autor .formulario-codigo .firma').text($(this).closest('.simple-collapse').find('.control-label.d-block').text());
		});
		
		//Validar ORCID firma
		$('#modal-anadir-autor .formulario-codigo a.btn-outline-grey').off('click').on('click', function (e) {
			that.validarORCID($('#modal-anadir-autor #signatureorcid').val());		
		});
		
		//Añadimos firmas
		$('#modal-anadir-autor .addsignatures').off('click').on('click', function (e) {
			$('#modal-anadir-autor').modal('hide');
			
			var htmlAuthor=`
			<div class="item added entityaux" propertyrdf="http://purl.org/ontology/bibo/authorList" rdftype="http://purl.obolibrary.org/obo/BFO_0000023" about="${RandomGuid()}">
				<div class="custom-form-row">
					<div class="form-group expand-2">
						<label class="control-label d-block">Firma *</label>
						<input propertyrdf="http://xmlns.com/foaf/0.1/nick" placeholder="" value="Jose Luis Hernandez Ramos" type="text" class="form-control not-outline obligatorio">
					</div>
					<div class="form-group expand-2 entitycontainer obligatorio" rdftype="http://xmlns.com/foaf/0.1/Person" idtemp="${RandomGuid()}">
						<label class="control-label d-block">Persona *</label>
						<div class="item added entity" propertyrdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#member" rdftype="http://xmlns.com/foaf/0.1/Person" about="http://gnoss.com/items/Person_81c44f33-a00f-45b6-9380-ff51350ae5aa_19d0c65c-8a11-4b94-8acf-1a45dd47d18f"><input propertyrdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#member" placeholder="" value="" type="text" class="form-control not-outline obligatorio">
							<span class="title" loaded="true" route="person||http://xmlns.com/foaf/0.1/name">Jose Luis Hernandez Ramos</span> 
							<span class="property" loaded="true" name="ORCID" route="person||http://w3id.org/roh/ORCID"></span> 
						</div>
						<div class="item aux">
							<input disabled="" value="Jose Luis Hernandez Ramos" type="text" class="form-control not-outline ">
							<div class="acciones-listado-edicion">
								<div class="wrap">
									<ul class="no-list-style d-flex align-items-center">
										<li>
											<a class="btn btn-outline-grey edit">
												<span class="texto">Editar</span>
												<span class="material-icons">edit</span>
											</a>
										</li>
										<li>
											<a class="btn btn-outline-grey delete">
												<span class="texto">Eliminar</span>
												<span class="material-icons">delete</span>
											</a>
										</li>
									</ul>
								</div>
							</div>
						</div>
					</div>
				</div>
				<input propertyrdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#comment" value="10" type="hidden">
				<span class="title" loaded="false" route="http://xmlns.com/foaf/0.1/nick"></span> 
				<span class="property" loaded="false" name="Nombre" route="http://www.w3.org/1999/02/22-rdf-syntax-ns#member||person||http://xmlns.com/foaf/0.1/name"></span> 
				<span class="property" loaded="false" name="ORCID" route="http://www.w3.org/1999/02/22-rdf-syntax-ns#member||person||http://w3id.org/roh/ORCID"></span> 
			</div>`
			$('.entityauxauthorlist').append(htmlAuthor);
			
			/*	*/
		});
		
		
		
		
		//GUARDADOS
		$(`#modal-editar-entidad .btn-primary,
		#modal-editar-entidad-0 .btn-primary,
		#modal-editar-entidad-1 .btn-primary,
		#modal-editar-entidad-2 .btn-primary,
		#modal-editar-entidad-3 .btn-primary,
		#modal-editar-entidad-4 .btn-primary`).off('click').on('click', function (e) {
			//Obtenemos modal y formulario
			var modal=$(this).closest('.modal');
			var formulario=$(modal).find('form.formulario-edicion');
			modal.find('.form-actions .ko').remove();
			
			//Validamos
			if(that.validarFormulario(formulario,modal))
			{
				//Procedemos si supera la validación
				that.guardarModal(formulario);
			}			
		});
		
        return;
    }, 
	validarFormulario: function (formulario,modal) {
        var that = this;
		//Validamos los campos obligatorios
		var camposObligatorios = []; 
		//Validamos inputs que no pertenezcan a una entidad auxiliar (ni sean multiples)
		$(formulario).find('input.obligatorio, select.obligatorio, textarea.obligatorio').each(function (index) {
			if($(this).closest('.entityauxcontainer,.entitycontainer,.multiple').length==0)
			{				
				if ($(this).val() == null || $(this).val() == '') {
					camposObligatorios.push($(this).closest('.form-group').find('.control-label').text());
				}
			}
		});
		//Validamos inputs que no pertenezcan a una entidad auxiliar (y sean multiples)
		$(formulario).find('.multiple.obligatorio:not(.entityauxcontainer ):not(.entitycontainer )').each(function (index) {
			if($(this).parent().closest('.entityauxcontainer,.entitycontainer,.multiple').length==0)
			{				
				if($(this).children('.added').length==0 || $($(this).children('.added')[0]).attr('about')=='')
				{
					camposObligatorios.push($(this).children('label').text());
				}
			}
		});
		//Validamos propiedades de entidades auxiliares y entidades que no pertenezcan a una entidad auxiliar
		$(formulario).find('.entityauxcontainer.obligatorio,.entitycontainer.obligatorio').each(function (index) {
			if($(this).parent().closest('.entityauxcontainer,.entitycontainer').length==0)
			{				
				if($(this).children('.added').length==0 || $($(this).children('.added')[0]).attr('about')=='')
				{
					camposObligatorios.push($(this).children('label').text());
				}
			}
		});			
		$(modal).find('.form-actions .ko').remove();
		if (camposObligatorios.length > 0) {
			var error = "";
			for (var indice in camposObligatorios) {
				error += '<p>El campo ' + camposObligatorios[indice] + ' es obligatorio</p>';
			};
			$(formulario).closest('.modal').find(' .form-actions').append('<div class="ko" style="display:block"><p>' + error + '</p></div>');
			return false;
		}
		return true;        
    }, 
	eliminarItem: function (sectionID, entityID) {
        var that = this;
        var item = {};
        item.pEntity = entityID;
        var article = $('a[data-id="' + entityID + '"]').closest('article');
        MostrarUpdateProgress();
        $.post(urlGuardadoCV + 'RemoveItem', item, function (data) {
            article.remove();
            that.repintarListadoTab(sectionID);
            OcultarUpdateProgress();
        });
    }, 
	guardarModal: function (pFormulario) {
        var that = this;
		
		//Los modales son de 3 tipos
		//Modal principal (item del CV)
		//Entidad auxiliar
		//Entidad principal
		
		var modal=$(pFormulario).closest('.modal');		
		//Auxiliar
		if($(pFormulario).attr('entityid')==null && $(pFormulario).attr('entityload')==null)
		{			
			if($(modal).attr('new')=='true')
			{
				$('.entityauxcontainer[idtemp="'+$(modal).attr('idtemp')+'"]').append($(modal).find('.formulario-edicion div[about="'+$(modal).attr('about')+'"]'));
			}else
			{
				$('.entityauxcontainer[idtemp="'+$(modal).attr('idtemp')+'"] div[about="'+$(modal).attr('about')+'"]').replaceWith($(modal).find('.formulario-edicion div[about="'+$(modal).attr('about')+'"]'));
			}
			$(modal).modal('hide');
			that.repintarListadoEntity();
		}else
		{
			//Modal principal (item del CV)	
			//o
			//Entidad principal			
			var entidad = {};
			var entidadPrincipal=false;
			if ($(pFormulario).attr('entityid') != null && $(pFormulario).attr('entityload') == null) {				
				entidadPrincipal=true;
			}
			entidad.id = $(pFormulario).attr('entityid');
			entidad.rdfType = $(pFormulario).attr('rdftype');
			entidad.sectionID = $(pFormulario).attr('sectionid');
			entidad.rdfTypeTab = $(pFormulario).attr('rdftypetab');
			entidad.cvID = this.idCV;
			entidad.properties = [];
			$(pFormulario).find('input, select, textarea').each(function (index) {			
				if ($(this).attr('propertyrdf') != null ) {				
					if($(this).closest('.item').hasClass('aux')|| $(this).hasClass('aux'))
					{
						//Si es multiple y no es una entidad auxiliar y no tiene otros valores añadidos continua pero con valor vacío
						if($(this).closest('.multiple:not(.entityauxcontainer )').length>0 && $(this).closest('.multiple:not(.entityauxcontainer )').find('.added').length==0)
						{						
							$(this).val('');
						}else
						{
							return;
						}
					}
					var property = $(this).attr('propertyrdf');
					//Cargar propiedades padre
					if ($(this).closest('.entityaux').length == 1) {
						//TODO mas de un nivel de auxiliar
						var propertyParent = $(this).closest('.entityaux').attr('propertyrdf');
						var rdfTypeEntity = $(this).closest('.entityaux').attr('rdftype');
						property = propertyParent + "@@@" + rdfTypeEntity + "|" + property;
					}
					
					var prop = null;
					for (var indice in entidad.properties) {
						if (entidad.properties[indice].prop == property) {
							prop = entidad.properties[indice];
						}
					};
					if (prop == null) {
						prop = {};
						prop.prop = property;
						prop.values = [];
					}					
					var valor=$(this).val();
					if ($(this).closest('.entityaux').length == 1) {
						//TODO mas de un nivel de auxiliar
						var entityParent = $(this).closest('.entityaux').attr('about');
						valor = entityParent + "@@@" + valor;
					}
					prop.values.push(valor);					
					entidad.properties.push(prop);
				}
			});
			
			entidad.auxEntityRemove=[];
			$(pFormulario).find('.entityauxcontainer').each(function (index) {	
				if($(this).attr('remove')!=null)
				{
					var remove=$(this).attr('remove').split('||');			
					for (var i = 0; i < remove.length; i++) {
						entidad.auxEntityRemove.push(remove[i]);
					}
				}
			});			
			
			MostrarUpdateProgress();
			if(entidadPrincipal)
			{
				//Entidad principal		
				$.post(urlGuardadoCV + 'updateEntity', entidad, function (data) {
					if (data.ok) {
						$(modal).modal('hide');
						if(entidad.id.indexOf("http")==-1)
						{
							//nueva
							if($('.entitycontainer[idtemp="'+$(modal).attr('idtemp')+'"]').hasClass('multiple'))
							{
								//Clonamos la auxiliar
								var itemAux=$('.entitycontainer[idtemp="'+$(modal).attr('idtemp')+'"] div.item.aux.entity');
								var itemClone=itemAux.clone();
								itemClone.removeClass('aux');
								itemClone.addClass('added');
								itemClone.attr('about',data.id);
								itemClone.find('input').val(data.id);
								itemAux.after(itemClone);
							}else
							{
								$('.entitycontainer[idtemp="'+$(modal).attr('idtemp')+'"] .item.added.entity').attr('about',data.id);
								$('.entitycontainer[idtemp="'+$(modal).attr('idtemp')+'"] .item.added.entity input').val(data.id);
							}
						}
						$('.item.added.entity[about="'+data.id+'"]').children('span.title').attr('loaded','false');
						$('.item.added.entity[about="'+data.id+'"]').children('span.property').attr('loaded','false');
						$('.item.added.entity[about="'+data.id+'"]').children('span.title').html('');
						$('.item.added.entity[about="'+data.id+'"]').children('span.property').html('');
						that.repintarListadoEntity();
						OcultarUpdateProgress();
					} else {
						alert("Error: " + data.error);
						OcultarUpdateProgress();
					}
				});
			}else
			{
				//Modal principal (item del CV)	
				$.post(urlGuardadoCV + 'updateEntity', entidad, function (data) {
					if (data.ok) {
						$(modal).modal('hide');
						var entityLoad = $(pFormulario).attr('entityload');
						if (entityLoad != null && entityLoad != '') {
							//Si viene entityLoad actualiza el item
							$.get(urlEdicionCV + 'GetItemMini?pIdSection=' + entidad.sectionID + "&pRdfTypeTab=" + entidad.rdfTypeTab + "&pEntityID=" + entityLoad + "&pLang=" + lang, null, function (data) {
								$('a[data-id="' + entityLoad + '"]').closest('article').replaceWith(that.printHtmlListItem(entityLoad, data));
								that.repintarListadoTab(entidad.sectionID);
								OcultarUpdateProgress();
							});
						} else {
							//Si no viene entityLoad carga el item
							$.get(urlEdicionCV + 'GetItemMini?pIdSection=' + entidad.sectionID + "&pRdfTypeTab=" + entidad.rdfTypeTab + "&pEntityID=" + data.id + "&pLang=" + lang, null, function (data2) {
								$('div[section="' + entidad.sectionID + '"] .resource-list-wrap').append(that.printHtmlListItem(data.id, data2));
								that.repintarListadoTab(entidad.sectionID);
								OcultarUpdateProgress();
							});
						}

					} else {
						alert("Error: " + data.error);
						OcultarUpdateProgress();
					}

				});
			}
		}        
    },
	validarFirmas: function (){
		var that=this;
		var item={};
		item.pSignatures=$('#inputsignatures').val();
		item.pCVID=this.idCV;
		item.pPersonID=this.idPerson;
		MostrarUpdateProgress();
		$('#modal-anadir-autor .formulario-edicion .resultados').hide();
		$('#modal-anadir-autor .formulario-edicion .resultados .form-group.full-group').remove();
		$.post(urlEdicionCV + 'ValidateSignatures', item, function (data) {
			OcultarUpdateProgress();
			var i=0;
			var htmlSinCandidatos=`<div class="user-miniatura">
                                <div class="imagen-usuario-wrap">                                    
									<div class="imagen">
									</div>									
                                </div>
                                <div class="nombre-usuario-wrap">
									<p class="nombre">Ninguna sugerencia</p>
                                </div>
                                <div class="coincidencia-wrap">
                                    <a href="javascript: void(0);" class="form-buscar">Buscar</a>
                                </div>
                            </div>`;
			for (var firma in data) {
				var numCandidatos=data[firma].length;
				i++;	
				
				var htmlCandidatos=htmlSinCandidatos;
				if(numCandidatos>0)
				{
					//TODO textos
					var score=(data[firma][0].score*100).toFixed(0);
					htmlCandidatos=that.htmlCandidatoFirma(data[firma][0],i,score);
					var otrosCandidatos='';
					for (var j = 1; j < numCandidatos; j++) {
						var scoreIn=(data[firma][j].score*100).toFixed(0);
						if(scoreIn>=80)
						{
							htmlCandidatos+=that.htmlCandidatoFirma(data[firma][j],i,scoreIn);
						}else
						{
							otrosCandidatos+=that.htmlCandidatoFirma(data[firma][j],i,scoreIn);
						}
					}
					htmlCandidatos+=`	<a href="#groupCollapse${i}" data-toggle="collapse" aria-expanded="true" class="collapse-toggle collapsed">Más resultados</a>
										<div id="groupCollapse${i}" class="collapse-wrap collapse">
											${otrosCandidatos}
											<div class="form-actions">
												<a href="javascript: void(0);" class="form-buscar">Buscar</a>
											</div>
										</div>`;
					
					
				}				
				var autor=`	<div class="form-group full-group">
								<div class="simple-collapse">
									<label class="control-label d-block">${firma}</label>
										${htmlCandidatos}									
								</div>
							</div>`;
				$('#modal-anadir-autor .formulario-edicion .resultados').append(autor);
			};			
			$('#modal-anadir-autor .formulario-edicion .resultados').show();
			
			
			that.engancharComportamientosCV();		
			
        });
	},
	htmlCandidatoFirma: function(candidato,indice,score){
		//TODO imagen y textos
		var color="red";
		if(score>=90)
		{
			color="green";
		}else if(score>=80){
			color="orange";
		}else{
			color="red";
		}
		var id=RandomGuid();
		var htmlOrcid='';
		if(candidato.orcid!=null)
		{
			htmlOrcid=`<a target="_blank" href="https://orcid.org/${candidato.orcid}">${candidato.orcid}</a>`;
		}
		var html=`<div class="user-miniatura">
					<div class="custom-control custom-checkbox">
						<input type="checkbox" id="user-${id}" personid="${candidato.personid}" name="user-${indice}" class="custom-control-input chksignature">
						<label class="custom-control-label" for="user-${id}"></label>
					</div>
					<div class="imagen-usuario-wrap">						
						<div class="imagen">
						</div>
					</div>
					<div class="nombre-usuario-wrap">						
						<p class="nombre">${candidato.name}</p>
						<p class="nombre-completo">TODO · ${htmlOrcid}</p>						
					</div>
					<div class="coincidencia-wrap">
						<p class="label">Coincidencia</p>
						<p class="numResultado" style="color: ${color};">${score}%</p>
					</div>
				</div>`;
		return html;
	},
	validarORCID: function (orcid){
		var that=this;
		var item={};
		item.pOrcid=orcid;
		MostrarUpdateProgress();
		$.post(urlGuardadoCV + 'ValidateORCID', item, function (data) {
			OcultarUpdateProgress();			
        });
	}

};

//Métodos auxiliares
function EliminarAcentos(texto) {
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
}

function TransFormData(data, type) {
    switch (type) {
        case 'date':
            var aux = [];
            data.forEach(function (valor, indice, array) {
                aux.push(valor.substring(6, 8) + "/" + valor.substring(4, 6) + "/" + valor.substring(0, 4));
            })
            return aux;
            //Declaraciones ejecutadas cuando el resultado de expresión coincide con el valor1
            break;
        default:
            return data;
            break;
    }
}

function RandomGuid() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}
//Fin de métodos auxiliares

