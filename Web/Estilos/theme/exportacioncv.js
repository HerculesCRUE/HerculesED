var urlExportacionCV = "https://localhost:5002/ExportadoCV/";
//var urlExportacionCV = "http://serviciosedma.gnoss.com/editorcv/ExportadoCV/";

var exportacionCV = {
    idUsuario: null,
    init: function() {
        this.config();
        //TODO this.idUsuario = $('.contenido-cv').attr('userID');
		
        return;
    },
	config: function(){
		$('body').addClass('page-cv');
	},
	//Métodos de pestañas
    cargarCV: function() {
        var that = this;
		MostrarUpdateProgressTime(0);
        //MostrarUpdateProgress();
		//TODO cambiar url
        //$.get(urlExportacionCV + 'GetAllTabs?userID=' + that.idUsuario + "&pLang=" + lang, null, function(data) {
        $.get(urlExportacionCV + 'GetAllTabs?userID=d7711fd2-41d2-464b-8838-e42c52213927&pLang=es', null, function(data) {
            //recorrer items y por cada uno	
			for(var i=0;i<data.length;i++){
				var id = 'x' + RandomGuid();
				var contenedorTab=`<div class="panel-group pmd-accordion" id="datos-accordion" role="tablist" aria-multiselectable="true">
										<div class="panel">
											<div class="panel-heading" role="tab" id="datos-tab">
												<p class="panel-title">
													<a data-toggle="collapse" data-parent="#datos-accordion" href="#datos-panel" aria-expanded="true" aria-controls="datos-tab" data-expandable="false" class="">
														<span class="texto">${data[i].title}</span>
														<span class="material-icons pmd-accordion-arrow">keyboard_arrow_down</span>
													</a>
												</p>
											</div>
											<div about="${id}">
												<div class="row cvTab">
													<div class="col-12 col-contenido">
													</div>
												</div>
											</div>
										</div>
									</div>`
				$('.cabecera-cv').append( $(contenedorTab));		
				edicionCV.printTab(id, data[i]);
			}			
            OcultarUpdateProgress();
			
			$('.resource-list.listView .resource .wrap').css("margin-left", "70px")
			checkAllCVWrapper();
        });
        return;
    }
};

$(window).on('load', function(){
	exportacionCV.cargarCV();
	
	var myForm = `<form action="" id="myForm" method="post">
					<button type="submit" >Exportar</button>
				</form>`;
	$('#containerCV').append(myForm);
	
	$(function() {
		$('#myForm').submit(function(e) {
			e.preventDefault();
			e.stopPropagation();
			
			var listaId = "";
			$('.resource-list .custom-control-input:checkbox:checked').each(function(){
				listaId += (this.checked ? $(this).val()+"@@@" : "")
			});
			
			listaId = listaId.slice(0,-3);			
			
			$.ajax({
				type: 'POST',
				url: urlExportacionCV+'GetCV',
				dataType: 'json',
				data: {
					userID: 'd7711fd2-41d2-464b-8838-e42c52213927', 
					lang: lang,
					listaId: listaId
				}
			});
			return false;
		});
	});
});

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
	
edicionCV.printPersonalData=function(data) {        
	return 'SECCION DE DATOS PERSONALES';
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
				<div id="${id2}" class="panel-collapse collapse" role="tabpanel">				
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