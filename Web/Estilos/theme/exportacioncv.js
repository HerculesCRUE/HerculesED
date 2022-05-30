var urlExportacionCV = url_servicio_editorcv+"ExportadoCV/";

var exportacionCV = {
    idUsuario: null,
    init: function() {
        this.config();
        this.idUsuario = $('#inpt_usuarioID').val();
		
        return;
    },
	config: function(){
		var that = this;
		$('body').addClass('page-cv');
		that.cargarListadoCV();
		
		//Preparar exportación
		$('.btGenerarExportarCV').off('click').on('click', function(e) {
            e.preventDefault();
			if($('#exportCvName').val().length == 0){
				window.alert("Debes Añadir un nombre al fichero");
				return false;
			}
			that.cargarCV();
		});
		
		
		//Generar exportación
		$('.btExportarCV').off('click').on('click', function(e) {
            e.preventDefault();
						
			var listaId = "";
			$('.resource-list .custom-control-input:checkbox:checked').each(function(){
				listaId += (this.checked ? $(this).val()+"@@@" : "")
			});
			
			listaId = listaId.slice(0,-3);			
			
			if(listaId.length == 0){
				window.alert("Debes seleccionar alguna opción");
				return false;
			}
			
			var data = {};
			data.userID= that.idUsuario;
			data.lang= lang;
			data.listaId= listaId;
			data.nombreCV= $('#exportCvName').val();
			
			MostrarUpdateProgress();
			$.post(urlExportacionCV + 'GetCV', data, function(data) {
				that.cargarListadoCV();		
			});
        });		
		
		
	},
	//Carga los CV exporatdos
    cargarListadoCV: function() {
		$('.listadoCV li').remove();

        var that = this;
		that.idUsuario = $('#inpt_usuarioID').val();
		$('.col-contenido.listadoExportacion').show();
		$('.col-contenido.exportacion').hide();
		$.get(urlExportacionCV + 'GetListadoCV?userID=' + that.idUsuario , null, function(data) {
            //recorrer items y por cada uno			
			for(var i=0;i<data.length;i++){
				
				$('.listadoCV').append($('<li>'+data[i].titulo+'</li>'));
				$('.listadoCV').append($('<li>'+data[i].fecha+'</li>'));
				$('.listadoCV').append($('<li>'+data[i].estado+'</li>'));
				$('.listadoCV').append($('<li>'+data[i].fichero+'</li>'));
			}
		});
	},
	//Carga los datos del CV para la exportacion
    cargarCV: function() {
		$('.col-contenido.listadoExportacion').hide();
		$('.col-contenido.exportacion').show();
        var that = this;
		MostrarUpdateProgressTime(0);
		$('.panel-group.pmd-accordion').remove();
        $.get(urlExportacionCV + 'GetAllTabs?userID=' + that.idUsuario + "&pLang=" + lang, null, function(data) {
        
            //recorrer items y por cada uno			
			for(var i=0;i<data.length;i++){
				var id = 'x' + RandomGuid();
				var contenedorTab=`<div class="panel-group pmd-accordion" id="datos-accordion${i}" role="tablist" aria-multiselectable="true">
										<div class="panel">
											<div class="panel-heading" role="tab" id="datos-tab">
												<p class="panel-title">
													<a data-toggle="collapse" data-parent="#datos-accordion${i}" href="#${id}" aria-expanded="true" aria-controls="datos-tab" data-expandable="false" class="">
														<span class="texto">${data[i].title}</span>
														<span class="material-icons pmd-accordion-arrow">keyboard_arrow_up</span>
													</a>
												</p>
											</div>
											<div id="${id}" class="collapse show">
												<div class="row cvTab">
													<div class="col-12 col-contenido">
													</div>
												</div>
											</div>
										</div>
									</div>`
				if(i==0){
					$('.cabecera-cv').append( $(contenedorTab));
					var html = edicionCV.printPersonalData(id, data[i]);					
					$('div[id="' + id + '"] .col-12.col-contenido').append(html);
					$('#'+id+' input[type="checkbox"]').prop('checked',true);
				}else if(i == 6){
					$('.cabecera-cv').append( $(contenedorTab));		
					var html = printFreeText(id, data[i]);
					$('div[id="' + id + '"] .col-12.col-contenido').append(html);
				}else{
					$('.cabecera-cv').append( $(contenedorTab));		
					edicionCV.printTab(id, data[i]);
				}
			}			
            OcultarUpdateProgress();
			
			$('.resource-list.listView .resource .wrap').css("margin-left", "70px")
			checkAllCVWrapper();
        });
        return;
    }
};


function checkAllCVWrapper(){
	$('.checkAllCVWrapper input[type="checkbox"]').off('click').on('click', function(e) {
		$(this).closest('.panel-body').find('article div.custom-checkbox input[type="checkbox"]').prop('checked',$(this).prop('checked'));
	});
	
	$('.checkAllCVWrapper input[type="checkbox"]').closest('.panel-body').find('article div.custom-checkbox input[type="checkbox"]').off('change').on('change', function(e) {
		if(!$(this).prop('checked')){
			$(this).closest('.panel-body').find('.checkAllCVWrapper input[type="checkbox"]').prop('checked', false);
		}
	});
}
	
function printFreeText(id, data){
	var id2 = 'x' + RandomGuid();
	var expanded = "";
	var show = "";
	if (data.sections != null) {
		if (Object.keys(data.sections).length > 0) {
			//Desplegado
			expanded = "true";
			show = "show";
		} else {
			//No desplegado	
			expanded = "false";
		}
		var html = `<div class="panel-group pmd-accordion collapse show" section="${data.sections[0].title}" id="${id}" role="tablist" aria-multiselectable="true">
						<div class="panel">
							<div class="panel-heading" role="tab" id="publicaciones-tab">
								<p class="panel-title">
									<a data-toggle="collapse" data-parent="#${id}" href="#${id2}" aria-expanded="${expanded}" aria-controls="${id2}" data-expandable="false">
										<span class="material-icons pmd-accordion-icon-left">folder_open</span>
										<span class="texto">${data.title}</span>
										<span class="material-icons pmd-accordion-arrow">keyboard_arrow_down</span>
									</a>
								</p>
							</div>
							<div id="${id2}" class="panel-collapse collapse ${show}" role="tabpanel">
								<div id="situacion-panel" class="panel-collapse collapse show" role="tab-panel" aria-labelledby="situacion-tab" style="">
									<div class="panel-body">
										<div class="acciones-listado acciones-listado-cv">
											<div class="wrap">
												<div class="checkAllCVWrapper" id="checkAllCVWrapper">
													<div class="custom-control custom-checkbox">
														<input type="checkbox" class="custom-control-input" id="checkAllResources_${id2}">
														<label class="custom-control-label" for="checkAllResources_${id2}">
														</label>
													</div>
												</div>
											</div>
										</div>
										<div class="resource-list listView">
											<div class="resource-list-wrap">
						`
		var secciones = data.sections[0].item.sections[0].rows;
		for (var i = 0; i < secciones.length; i++){
			var id = 'x' + RandomGuid();
			var html2 = `<article class="resource success" >
							<div class="custom-control custom-checkbox">
								<input type="checkbox" class="custom-control-input" id="check_resource_${id}"  value="${data.sections[0].item.entityID + "|||" + secciones[i].properties[0].property}">
								<label class="custom-control-label" for="check_resource_${id}"></label>
							</div>
							<div class="wrap">
								<div class="middle-wrap">
									<div class="title-wrap">
									</div>
									<div class="title-wrap">
										<h2 class="resource-title">
											<a href="#" data-id="${id}" internal-id="${data.sections[0].item.entityID}">${secciones[i].properties[0].title}</a>
										</h2>
										<span class="material-icons arrow">keyboard_arrow_down</span>
									</div>
									<div class="content-wrap">
										<div class="description-wrap">
										</div>
									</div>
								</div>
							</div>
						</article>`;
			html += html2;
		}			
		html += `						</div>
									</div>
								</div>
							</div>
						</div>
					</div>
				</div>`;
					
		 return html;
	}
}	
	
edicionCV.printTab= function(entityID, data) {
	var that=this;
	if (data.entityID != null) {
		$('div[id="' + entityID + '"] .col-12.col-contenido').append(this.printPersonalData(data));
	} else {
		for (var i = 0; i < data.sections.length; i++) {	
			$('div[id="' + entityID + '"] .col-12.col-contenido').append(this.printTabSection(data.sections[i]));				
			if (data.sections[i].items != null) {
				this.repintarListadoTab(data.sections[i].identifier,true);
			} else if (data.sections[i].item != null) {
				that.printSectionItem(data.sections[i].item.idContenedor, data.sections[i].item, data.sections[i].identifier, $('div[id="' + entityID + '"]').attr('rdftype'), data.sections[i].item.entityID);
				//Si no tiene ningun campo valor se repliega
				var plegar=true;
				$('div[section="' + data.sections[i].identifier+'"] input').each(function() {
					if($(this).val()!='')
					{
						plegar=false;
					}
				});
				//
				$('div[section="' + data.sections[i].identifier+'"] div.visuell-view').each(function() {
					if($(this).html()!='')
					{
						plegar=false;
					}
				});
				if(plegar)
				{
					$('div[section="' + data.sections[i].identifier+'"] .panel-collapse.collapse').removeClass('show');
					$('div[section="' + data.sections[i].identifier+'"] .panel-heading a').attr('aria-expanded','false');
				}
			}
		}
	}
	accionesPlegarDesplegarModal.init();
	this.engancharComportamientosCV();
	this.mostrarTraducciones();		
};
	
edicionCV.printPersonalData=function(id, data) {	
	var id2 = 'x' + RandomGuid();
	var expanded = "";
	var show = "";
	if (data.sections != null) {
		if (Object.keys(data.sections).length > 0) {
			//Desplegado
			expanded = "true";
			show = "show";
		} else {
			//No desplegado	
			expanded = "false";
		}
		var nombre = '';
		for(var i =0; i < data.sections[0].rows[0].properties.length; i++){
			if(data.sections[0].rows[0].properties[i].values[0] != null){
				nombre += data.sections[0].rows[0].properties[i].values[0];
				nombre += " ";				
			}
		}
		
		var html = `<div class="panel-group pmd-accordion collapse show" section="${data.sections[0].title}" id="${id}" role="tablist" aria-multiselectable="true">
						<div class="panel">
							<div class="panel-heading" role="tab" id="publicaciones-tab">
								<p class="panel-title">
									<a data-toggle="collapse" data-parent="#${id}" href="#${id2}" aria-expanded="${expanded}" aria-controls="${id2}" data-expandable="false">
										<span class="material-icons pmd-accordion-icon-left">folder_open</span>
										<span class="texto">${data.title}</span>
										<span class="material-icons pmd-accordion-arrow">keyboard_arrow_down</span>
									</a>
								</p>
							</div>
							<div id="${id2}" class="panel-collapse collapse ${show}" role="tabpanel">				
								<div id="situacion-panel" class="panel-collapse collapse show" role="tab-panel" aria-labelledby="situacion-tab" style="">
									<div class="panel-body">								
										<div class="resource-list listView">
											<div class="resource-list-wrap">									
												<article class="resource success" >
													<div class="custom-control custom-checkbox">
														<input type="checkbox" class="custom-control-input" id="check_resource_${id}"  value="${data.entityID}">
														<label class="custom-control-label" for="check_resource_${id}"></label>
													</div>
													<div class="wrap">
														<div class="middle-wrap">
															<div class="title-wrap">
															</div>
															<div class="title-wrap">
																<h2 class="resource-title">
																	<a href="#" data-id="${id}" internal-id="${data.entityID}">${nombre}</a>
																</h2>
																${this.printHtmlListItemEditable(data)}	
																${this.printHtmlListItemIdiomas(data)}
																<span class="material-icons arrow">keyboard_arrow_down</span>
															</div>
															<div class="content-wrap">
																<div class="description-wrap">
																</div>
															</div>
														</div>
													</div>
												</article>
											</div>
										</div>
									</div>
								</div>
							</div>
						</div>
					</div>	`;
		 return html;
	}
};

edicionCV.printTabSection= function(data) {
	//Pintado sección listado
	//css mas generico
	var id = 'x' + RandomGuid();
	var id2 = 'x' + RandomGuid();

	var expanded = "";
	var show = "";
	if (data.items != null) {
		if (Object.keys(data.items).length > 0) {
			//Desplegado
			expanded = "true";
			show = "show";
		} else {
			//No desplegado	
			expanded = "false";
		}
		//TODO texto ver items
		var htmlSection = `
		<div class="panel-group pmd-accordion" section="${data.identifier}" id="${id}" role="tablist" aria-multiselectable="true">
			<div class="panel">
				<div class="panel-heading" role="tab" id="publicaciones-tab">
					<p class="panel-title">
						<a data-toggle="collapse" data-parent="#${id}" href="#${id2}" aria-expanded="${expanded}" aria-controls="${id2}" data-expandable="false">
							<span class="material-icons pmd-accordion-icon-left">folder_open</span>
							<span class="texto">${data.title}</span>
							<span class="numResultados">(${Object.keys(data.items).length})</span>
							<span class="material-icons pmd-accordion-arrow">keyboard_arrow_up</span>
						</a>
					</p>
				</div>
				<div id="${id2}" class="panel-collapse collapse ${show}" role="tabpanel">				
					<div id="situacion-panel" class="panel-collapse collapse show" role="tab-panel" aria-labelledby="situacion-tab" style="">
						<div class="panel-body">
							<div class="acciones-listado acciones-listado-cv">
								<div class="wrap">
									<div class="checkAllCVWrapper" id="checkAllCVWrapper">
										<div class="custom-control custom-checkbox">
											<input type="checkbox" class="custom-control-input" id="checkAllResources_${id2}">
											<label class="custom-control-label" for="checkAllResources_${id2}">
											</label>
										</div>
									</div>
								</div>
								<div class="wrap">
									<div class="ordenar dropdown">${this.printOrderTabSection(data.orders)}</div>
									<div class="buscador">
										<div class="fieldsetGroup searchGroup">
											<div class="textoBusquedaPrincipalInput">
												<input type="text" class="not-outline txtBusqueda" placeholder="${GetText('CV_ESCRIBE_ALGO')}" autocomplete="off">
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
											<ul class="pagination arrows">
											</ul>
											<ul class="pagination numbers">	
												<li class="actual"><a href="javascript: void(0)" page="1">1</a></li>
											</ul>
										</nav>
									</div>
								</div>
							</div>
						</div>
					</div>					
				</div>
			</div>
		</div>`;
		return htmlSection;
	}
};

edicionCV.printHtmlListItem= function(id, data) {
	let openAccess="";
	if (data.isopenaccess) {
		openAccess = "open-access";
	}
	var htmlListItem = `<article class="resource success ${openAccess}" >
							<div class="custom-control custom-checkbox">
								<input type="checkbox" class="custom-control-input" id="check_resource_${data.identifier}"  value="${id}">
								<label class="custom-control-label" for="check_resource_${data.identifier}"></label>
							</div>
							<div class="wrap">
								<div class="middle-wrap">
									${this.printHtmlListItemOrders(data)}
									<div class="title-wrap">
									</div>
									<div class="title-wrap">
										<h2 class="resource-title">
											<a href="#" data-id="${id}" internal-id="${data.identifier}">${data.title}</a>
										</h2>
										${this.printHtmlListItemEditable(data)}	
										${this.printHtmlListItemIdiomas(data)}
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
};