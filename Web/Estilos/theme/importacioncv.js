var urlImportacionCV = url_servicio_editorcv+"ImportadoCV";
var selectorConflictoNoBloqueado = '';
var selectorConflictoBloqueado = '';
var selectorCamposTexto = '';
var dropdownSimilitudes = '';
var contador = 1;

//window.addEventListener('beforeunload', (event) => {
//  // Cancel the event as stated by the standard.
//  event.preventDefault();
//  // Chrome requires returnValue to be set.
//  event.returnValue = '';
//});

var importarCVN = {
	idUsuario:  null,
    init: function (){		
		this.config(),
		this.idUsuario = $('#inpt_usuarioID').val();
		this.fileData = '';
		this.filePreimport = '';

		dropdownSimilitudes = `<div class="ordenar dropdown selectsimilarity dropdown-select">
									<a class="dropdown-toggle" data-toggle="dropdown" aria-expanded="false">
										<span class="material-icons">swap_vert</span>
										<span class="texto">${GetText('CV_MOSTRAR_TODOS')}</span>
									</a>
									<div class="dropdown-menu basic-dropdown dropdown-menu-right" style="will-change: transform;">
										<a href="javascript: void(0)" class="item-dropdown"><span class="texto">${GetText('CV_MOSTRAR_TODOS')}</span></a>
										<a href="javascript: void(0)" class="item-dropdown"><span class="texto">${GetText('CV_MOSTRAR_CONFLICTOS')}</span></a>
										<a href="javascript: void(0)" class="item-dropdown"><span class="texto">${GetText('CV_MOSTRAR_NUEVOS')}</span></a>
									</div>
								</div>`;

		selectorConflictoNoBloqueado = `<select name="itemConflict" >
											<option value="ig" selected="">${GetText('CV_IGNORAR')}</option>
											<option value="fu">${GetText('CV_FUSIONAR')}</option>
											<option value="so">${GetText('CV_SOBREESCRIBIR')}</option>
											<option value="du">${GetText('CV_DUPLICAR')}</option>
										</select>`;
											
		selectorConflictoBloqueado = `<select name="itemConflict" >
										<option value="ig" selected="">${GetText('CV_IGNORAR')}</option>
										<option value="fu">${GetText('CV_FUSIONAR')}</option>
										<option value="du">${GetText('CV_DUPLICAR')}</option>
									</select>`;
										
		selectorCamposTexto = `<select hidden name="itemConflict">
									<option value="so" selected="">${GetText('CV_SOBREESCRIBIR')}</option>
								</select>`;

        return;        
    },
	config: function (){
		var that=this;
		$('#file_cvn').GnossDragAndDrop({
			//TODO
            acceptedFiles: ["pdf","xml"],
			maxSize: 5000,
            onFileAdded: function (plugin, files) {
                $('.col-contenido .botonera').css('display', 'block');
            },
            onFileRemoved: function (plugin, files) {
                $('.col-contenido .botonera').css('display', 'none');
            }
        });
		$('.btProcesarCV').off('click').on('click', function(e) {
            e.preventDefault();
			that.cargarCV();
		});
		
		$('.btImportarCV').off('click').on('click', function(e) {
			e.preventDefault();
			var listaId = "";
			var listaOpcionSeleccionados = "";
			$('.resource-list .custom-control-input:checkbox:checked').each(function(){
				listaId += (this.checked ? $(this).val()+"@@@" : "")
			});
			$('.resource-list .custom-control-input:checkbox:checked').closest('.resource.success').find(':selected').each(function(){
				listaOpcionSeleccionados += (this.selected ? $(this).closest(".resource.success").find(":checked").val() + "|||" + $(this).val()+"@@@" : "")
			});
			
			listaId = listaId.slice(0,-3);			
			listaOpcionSeleccionados = listaOpcionSeleccionados.slice(0,-3);			
			
			that.importarCV(listaId, listaOpcionSeleccionados);
		});
    },
	//Carga los datos del CV para la exportacion
    cargarCV: function() {
		if($('#file_cvn')[0].files[0]==null){
			OcultarUpdateProgress();
			alert("No hay un fichero adjuntado");
			return;
		}
		
		$('.col-contenido.paso1').hide();
		$('.col-contenido.paso2').show();
		MostrarUpdateProgressTime(0);
		
		if($('#textoMascaraBlanca').length == 0){
			$('#mascaraBlanca').find('.wrap.popup').append('<br><div id="titleMascaraBlanca"></div>');
			$('#mascaraBlanca').find('.wrap.popup').append('<div id="workMascaraBlanca"></div>');
		}
	
		var that=this;
		var formData = new FormData();
		var petition = RandomGuid();
		formData.append('userID', that.idUsuario);
		formData.append('petitionID', petition);
		formData.append('File', $('#file_cvn')[0].files[0]);
		
		//Actualizo el estado cada 500 milisegundos
		var intervalStatus = setInterval(function() {
			$.ajax({
				url: urlImportacionCV + '/PreimportarCVStatus?petitionID='+petition,
				type: 'GET',
				success: function ( response ) {
					if(response != null && response != ''){
						if(response.subTotalWorks == null || response.subTotalWorks == 0 || response.subActualWork==response.subTotalWorks){
							$('#titleMascaraBlanca').text(`${GetText(response.actualWorkTitle)}`);
						}
						else{
							$('#titleMascaraBlanca').text(`${GetText(response.actualWorkTitle)}` + " (" +  response.subActualWork + '/' + response.subTotalWorks + ")");
						}
						//Si no hay pasos maximos no muestro la lista
						if(response.totalWorks != 0){
							$('#workMascaraBlanca').text(response.actualWork + '/' + response.totalWorks);
						}
					}
				}
			});	
		}, 500);
		

		$.ajax({
			url: urlImportacionCV + '/PreimportarCV',
			type: 'POST',
			data: formData,	
			cache: false,
			processData: false,
            enctype: 'multipart/form-data',
            contentType: false,
			success: function ( response ) {
				clearInterval(intervalStatus);
				$('#titleMascaraBlanca').remove();
				$('#workMascaraBlanca').remove();
				for(var i=0;i<7;i++){
					var id = 'x' + RandomGuid();
					var contenedorTab=`<div class="panel-group pmd-accordion" id="datos-accordion${i}" role="tablist" aria-multiselectable="true">
											<div class="panel">
												<div class="panel-heading" role="tab" id="datos-tab">
													<p class="panel-title">
														<a data-toggle="collapse" data-parent="#datos-accordion${i}" href="#${id}" aria-expanded="true" aria-controls="datos-tab" data-expandable="false" class="">
															<span class="texto">${response[i].title}</span>
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
										</div>`;
					if(i==0){
						$('.contenido-cv').append( $(contenedorTab));
						var html = edicionCV.printPersonalData(id, response[i]);
						$('div[id="' + id + '"] .col-12.col-contenido').append(html);
						$('#'+id+' input[type="checkbox"]').prop('checked',true);
					}else if(i == 6){
						$('.contenido-cv').append( $(contenedorTab));		
						var html = printFreeText(id, response[i]);
						$('div[id="' + id + '"] .col-12.col-contenido').append(html);
					}else{
						$('.contenido-cv').append( $(contenedorTab));
						edicionCV.printTab(id, response[i]);
					}
				};
				that.fileData = response[99].title;
				that.filePreimport = response[100].title;
				
				$('.resource-list.listView .resource .wrap').css("margin-left", "70px");
				checkAllCVWrapper();
				checkAllConflict();
				OcultarUpdateProgress();				
			},
			error: function(jqXHR, exception){
				clearInterval(intervalStatus);				
				$('#titleMascaraBlanca').remove();
				$('#workMascaraBlanca').remove();
				var msg = '';
				if (jqXHR.status === 0) {
					msg = 'Not connect.\n Verify Network.';
				} else if (jqXHR.status == 404) {
					msg = 'Requested page not found. [404]';
				} else if (jqXHR.status == 500) {
					msg = 'Internal Server Error [500].';
				} else if (exception === 'parsererror') {
					msg = 'Requested JSON parse failed.';
				} else if (exception === 'timeout') {
					msg = 'Time out error.';
				} else if (exception === 'abort') {
					msg = 'Ajax request aborted.';
				} else {
					msg = 'Uncaught Error.\n' + jqXHR.responseText;
				}
				alert(msg);
			}
		});		
				
		return;
    },
	importarCV: function(listaId, listaOpcionSeleccionados) {
		MostrarUpdateProgressTime(0);
		var that = this;
		var formData = new FormData();
		formData.append('userID', that.idUsuario);
		formData.append('fileData', that.fileData);
		formData.append('filePreimport', that.filePreimport);
		formData.append('listaId', listaId);
		formData.append('listaOpcionSeleccionados', listaOpcionSeleccionados);
		
		$.ajax({
			url: urlImportacionCV + '/PostimportarCV',
			type: 'POST',
			data: formData,
			cache: false,
			processData: false,
            enctype: 'multipart/form-data',
            contentType: false,
			success: function ( response ) {
				OcultarUpdateProgress();
				//TODO
				window.location.href = "http://edma.gnoss.com/comunidad/hercules";//response.form;
			},
			error: function(jqXHR, exception){
				var msg = '';
				if (jqXHR.status === 0) {
					msg = 'Not connect.\n Verify Network.';
				} else if (jqXHR.status == 404) {
					msg = 'Requested page not found. [404]';
				} else if (jqXHR.status == 500) {
					msg = 'Internal Server Error [500].';
				} else if (exception === 'parsererror') {
					msg = 'Requested JSON parse failed.';
				} else if (exception === 'timeout') {
					msg = 'Time out error.';
				} else if (exception === 'abort') {
					msg = 'Ajax request aborted.';
				} else {
					msg = 'Uncaught Error.\n' + jqXHR.responseText;
				}
				alert(msg);
			}
		});
		
	}
};

function checkAllConflict(){
	$('.ordenar.dropdown.dropdown-select a.item-dropdown').off('click').on('click', function(e) {
		var texto = $(this).find('.texto').text();
		var drop = $(this).closest('.ordenar.dropdown.dropdown-select').find('a.dropdown-toggle span.texto');		
		var seccion = $(this).closest('.panel-group.pmd-accordion').attr("section");
		var seleccionar = $(this).closest('.acciones-listado').find('.checkAllCVWrapper input[type="checkbox"]');
		var currentText = texto.split(' ')[1].toUpperCase();
		// Cambio el texto del checkbox de seleccionar en función de los datos mostrados
		$(seleccionar).prop('checked', false);
		$(seleccionar).closest('.custom-control').find('.custom-control-label').text(`${GetText('CV_SELECCIONAR_' + currentText)}`);
		
		if(texto==GetText('CV_MOSTRAR_TODOS'))
		{
			seleccionar.attr('conflict', '');
			drop.text(texto);
			edicionCV.buscarListado(seccion, false, false);
		}
		else if(texto==GetText('CV_MOSTRAR_CONFLICTOS'))
		{
			seleccionar.attr('conflict', 'true');
			drop.text(texto);
			edicionCV.buscarListado(seccion, true, false);
		}
		else if(texto==GetText('CV_MOSTRAR_NUEVOS'))
		{
			seleccionar.attr('conflict', 'false');
			drop.text(texto);
			edicionCV.buscarListado(seccion, false, true);
		}
	});
	
}

function checkAllCVWrapper(){
	$('.checkAllCVWrapper input[type="checkbox"]').off('click').on('click', function(e) {
		var currentText = 'TODOS';
		var conflictType = $(this).attr('conflict') ? '.conflict-' + $(this).attr('conflict') : '';

		currentText = $(this).closest('.acciones-listado').find('.ordenar.dropdown.dropdown-select a.dropdown-toggle span.texto').text().split(' ')[1].toUpperCase();
		if(!$(this)[0].checked)
		{
			$(this).closest('.custom-control').find('.custom-control-label').text(`${GetText('CV_SELECCIONAR_' + currentText)}`);
		}
		else
		{
			$(this).closest('.custom-control').find('.custom-control-label').text(`${GetText('CV_DESELECCIONAR_' + currentText)}`);
		}
		$(this).closest('.panel-body').find('article' + conflictType + ' div.custom-checkbox input[type="checkbox"]').prop('checked',$(this).prop('checked'));
	});
	
	$('.checkAllCVWrapper input[type="checkbox"]').closest('.panel-body').find('article div.custom-checkbox input[type="checkbox"]').off('change').on('change', function(e) {
		if(!$(this).prop('checked')){
			$(this).closest('.panel-body').find('.checkAllCVWrapper input[type="checkbox"]').prop('checked', false);
		}
	});
};

function printCientificProduction(id, data){
	//Pintado sección listado
	//css mas generico
	var id = 'x' + RandomGuid();
	var id2 = 'x' + RandomGuid();

	var expanded = "";
	var show = "";
	var datos = false;
	if (data.items != null) {
		if (Object.keys(data.items).length > 0) {
			//Desplegado
			expanded = "true";
			show = "show";
		} else {
			//No desplegado	
			expanded = "false";
		}
		for(const seccion in data.items){
			if(data.items[seccion].properties[0].values.length != 0){
				datos = true;
			}
		}
		contador=0;
		for(const seccion in data.items){
			
			if(datos){
				//TODO texto ver items
				var htmlSection = `
				<div class="panel-group pmd-accordion" section="${data.items[seccion].properties[0]}" id="${id}" role="tablist" aria-multiselectable="true">
					<div class="panel">
						<div class="panel-heading" role="tab" id="publicaciones-tab">
							<p class="panel-title">
								<a data-toggle="collapse" data-parent="#${id}" href="#${id2}" aria-expanded="${expanded}" aria-controls="${id2}" data-expandable="false">
									<span class="material-icons pmd-accordion-icon-left">folder_open</span>
									<span class="texto">${data.items[seccion].title}</span>
								</a>
							</p>
						</div>`;
						if(data.items[seccion].properties[0].values.length != 0){
						htmlSection += `
						<div id="${id2}" class="panel-collapse collapse ${show}" role="tabpanel">
							<div id="situacion-panel" class="panel-collapse collapse show" role="tab-panel" aria-labelledby="situacion-tab" style="">
								<div class="panel-body">
									<div class="resource-list listView">
										<div class="resource-list-wrap">
											<article class="resource success" >
												<div class="custom-control custom-checkbox">
													<input type="checkbox" class="custom-control-input" id="check_resource_${data.items[seccion].identifier}"  value="${data.items[seccion].identifier}_${contador}" checked>
													<label class="custom-control-label" for="check_resource_${data.items[seccion].identifier}"></label>
												</div>
												<div class="wrap">
													<div class="middle-wrap">
														<div class="title-wrap">
															<h2 class="resource-title">Indicadores generales de calidad de la producción científica</h2>`
															+selectorCamposTexto+														
														`</div>
													</div>
												</div>
											</article>
										</div>
									</div>
								</div>
							</div>
						</div>`;
						
						contador++;
						}
				htmlSection += `
					</div>
				</div>`;
			}
		}
		return htmlSection;
	}
	return '';
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
										<span class="material-icons pmd-accordion-arrow">keyboard_arrow_up</span>
									</a>
								</p>
							</div>
							<div id="${id2}" class="panel-collapse collapse ${show}" role="tabpanel">
								<div id="situacion-panel" class="panel-collapse collapse show" role="tab-panel" aria-labelledby="situacion-tab" style="">
									<div class="panel-body">										
										<div class="resource-list listView">
									<div class="resource-list-wrap">`;
		var secciones = data.sections[0].items;
		contador=0;
		for (const seccion in secciones){
			//Si no hay datos no pinto esa sección
			if(secciones[seccion].properties[0].values.length > 0 && secciones[seccion].properties[0].values[0].length > 0){
				var id = 'x' + RandomGuid();
				var valorSeccion = '';
				if(secciones[seccion].properties[0].values[0]!= null ){
					valorSeccion = secciones[seccion].properties[0].values[0];
				}
				var html2 = `<article class="resource success">
								<div class="custom-control custom-checkbox">
									<input type="checkbox" class="custom-control-input" id="check_resource_${secciones[seccion].identifier}_${contador}"  value="${secciones[seccion].identifier}_${contador}" checked>
									<label class="custom-control-label" for="check_resource_${secciones[seccion].identifier}_${contador}"></label>
								</div>
								<div class="wrap">
									<div class="middle-wrap">
										<div class="title-wrap">
											<h2 class="resource-title">${secciones[seccion].title}</h2>`
											+selectorCamposTexto+`
											<!--span class="material-icons arrow">keyboard_arrow_down</span-->
										</div>	
										<div class="content-wrap">
											<div class="description-wrap">
												<div class="group">
													<p>${valorSeccion}</p>
												</div>
											</div>
										</div>
									</div>
								</div>
							</article>`;
				html += html2;
			}
			contador++;
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
	for (var i = 0; i < data.sections.length; i++) {	
		if(data.sections[i].title=="Indicadores generales de calidad de la producción científica")
		{
			$('div[id="' + entityID + '"] .col-12.col-contenido').append(printCientificProduction(entityID, data.sections[i]));
		}
		else
		{
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
		contador=0;	
		for (const seccion in data.sections[0].items)
		{
			for(var i =0; i<data.sections[0].items[seccion].properties.length; i++){
				if(data.sections[0].items[seccion].properties[i].values[0] != null){
					nombre += data.sections[0].items[seccion].properties[i].values[0];
					nombre += " ";
				}
			}
			
			var html = `<div class="panel-group pmd-accordion collapse show" section="${data.sections[0].items[seccion].title}" id="${id}" role="tablist" aria-multiselectable="true">
							<div class="panel">
								<div class="panel-heading" role="tab" id="publicaciones-tab">
									<p class="panel-title">
										<a data-toggle="collapse" data-parent="#${id}" href="#${id2}" aria-expanded="${expanded}" aria-controls="${id2}" data-expandable="false">
											<span class="material-icons pmd-accordion-icon-left">folder_open</span>
											<span class="texto">${data.title}</span>
											<span class="material-icons pmd-accordion-arrow">keyboard_arrow_up</span>
										</a>
									</p>
								</div>`;
							if(data.sections[0].items[seccion].properties[0].values.length!=0)
							{
							html+=`
								<div id="${id2}" class="panel-collapse collapse ${show}" role="tabpanel">
									<div id="situacion-panel" class="panel-collapse collapse show" role="tab-panel" aria-labelledby="situacion-tab" style="">
										<div class="panel-body">
											<div class="resource-list listView">
												<div class="resource-list-wrap">
													<article class="resource success" >
														<div class="custom-control custom-checkbox">
															<input type="checkbox" class="custom-control-input" id="check_resource_${data.sections[0].items[seccion].identifier}"  value="${data.sections[0].items[seccion].identifier}_${contador}">
															<label class="custom-control-label" for="check_resource_${data.sections[0].items[seccion].identifier}"></label>
														</div>
														<div class="wrap">
															<div class="middle-wrap">
																<div class="title-wrap">
																	<h2 class="resource-title">Datos de identificación</h2>`
																	+selectorCamposTexto+
																`</div>
																<div class="content-wrap">
																	<div class="description-wrap">
																		<p>${nombre}</p>
																	</div>
																</div>
															</div>
														</div>
													</article>
												</div>
											</div>
										</div>
									</div>
							</div>`;
							contador++;
							}
						html += `
							</div>
						</div>`;
			 return html;
		 }
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
		contador=0;
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
											<label class="custom-control-label" for="checkAllResources_${id2}">Seleccionar todo</label>
										</div>
									</div>
								</div>
								<div class="wrap">
									${dropdownSimilitudes}
									<div class="ordenar dropdown orders">${this.printOrderTabSection(data.orders)}</div>
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
	let isCheck ="";
	let isConflict = false;
	if (data.isopenaccess) {
		openAccess = "open-access";
	}
	if(data.idBBDD == null || data.idBBDD == ''){
		isCheck = "checked";
	}
	if(data.idBBDD != ""){
		isConflict = true;
	}
	else
	{
		isConflict = false;
	}
	var htmlListItem = ``;
	if(data.title!= null){
		htmlListItem = `<article class="resource success ${openAccess} conflict-${isConflict}" >
							<div class="custom-control custom-checkbox">
								<input type="checkbox" class="custom-control-input" id="check_resource_${id}" value="${id}_${contador}" ${isCheck}>
								<label class="custom-control-label" for="check_resource_${id}"></label>
							</div>
							<div class="wrap">
								<div class="middle-wrap">
									${this.printHtmlListItemOrders(data)}
									<div class="title-wrap">
										<h2 class="resource-title">${data.title}</h2>`;
		if(data.idBBDD != ""){
			if(data.iseditable){
				htmlListItem += selectorConflictoNoBloqueado;
			}else{
				htmlListItem += selectorConflictoBloqueado;
			}	
		}
		else
		{
			htmlListItem += `<span class="material-icons-outlined new">fiber_new</span>`;
		}			
		htmlListItem += `<span class="material-icons arrow">keyboard_arrow_down</span>
									</div>
									<div class="content-wrap">
										<div class="description-wrap">
											${this.printHtmlListItemEditable(data)}	
											${this.printHtmlListItemPropiedades(data)}
										</div>
									</div>
								</div>
							</div>
						</article>`;
	}
	contador++;
	return htmlListItem;
};
