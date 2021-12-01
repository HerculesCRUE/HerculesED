$(document).ready(function () {
	// urlComunidadConIdioma="http://localhost:44319/";
	urlComunidadConIdioma="http://serviciosedma.gnoss.com/servicioalertas/"
	comportamientoAlertas.init();
	$('#modal-anadir-item-cv').on('show.bs.modal', function(e) {    
		var idGnossNofification = $(e.relatedTarget).data().value;
		$('#modal-anadir-item-cv .form-actions .btn-primary').prop('rel',idGnossNofification);
		
	});
});

var comportamientoAlertas={
	init: function () {				
		this.config();
		this.configClikAnyadirPublicacion();
	},
	config: function () {
		this.btnAnyadirPublication=$('#modal-anadir-item-cv .form-actions .btn-primary');
		this.primeraCarga();
		this.configClikTabs();
	},
	primeraCarga: function () {
		var pestanya= $('.tab-paneles-alertas li a[aria-selected="true"] ');
		var tipoNot=pestanya.prop('rel');
		this.getNotificaciones(tipoNot);
		
	},
	configClikTabs: function () {
		var that=this;
		$('#navegacion-alertas a').click(function (event) {
			var tipoNot=$(this).prop('rel');
			that.getNotificaciones(tipoNot);
		});
		
	},
	configClikAnyadirPublicacion: function () {
		var that=this;
		this.btnAnyadirPublication.click(function (event) {
			var pIdGnossAlerta=$(this).prop('rel');
			MostrarUpdateProgressTime(0);
			// var metodo = urlComunidadConIdioma + '/ExternalService';
			var metodo = urlComunidadConIdioma + '/GestorAlertas/AnyadirPublicacion';
			var params = {};
			params['pNombreServicio'] = 'GestorAlertas';
			params['pNombreAccion'] = 'AnyadirPublicacion';
			params['pIdGnossAlerta'] = pIdGnossAlerta;
			params['userID'] = $('input.inpt_usuarioID').val();	
			$.post(metodo, params, function (respuesta) {
				OcultarUpdateProgress();
				if(respuesta.status==1){
					//Ver que hacemos con la respuesta		
					$('#modal-anadir-item-cv .formulario-alertas').html('<p>La publicación se ha incorporado correctamente a su CV. Puede revisarla <a href="'+$('#inpt_baseUrlBusqueda').val()+'/recurso/x/'+respuesta.message+'">aqui</a></p>');
					$('#modal-anadir-item-cv .form-actions').remove();
					$('#modal-anadir-item-cv .cerrar').click(function (event) {
						event.preventDefault();
						MostrarUpdateProgressTime(0);
						window.location.replace(window.location.href);
						});
					//window.location.replace(window.location.href);
				}
				else{
					//that.modal_error.find('p').html(varErrorCodigoAccesoIncorrecto[lang]);
					//that.modal_error.removeClass('oculto');
				}
				}, 'json').fail(function() {
				OcultarUpdateProgress();
			});
		});
		
	},
	getNotificaciones: function (pType) {
		var that = this;
		MostrarUpdateProgressTime(0);
		// var metodo = urlComunidadConIdioma + '/ExternalService';
		var metodo = urlComunidadConIdioma + '/GestorAlertas/ObtenerAlertas';
		var params = {};
		params['pNombreServicio'] = 'GestorAlertas';
		params['pNombreAccion'] = 'ObtenerAlertas';
		params['pType'] = pType;
		params['userID'] = $('input.inpt_usuarioID').val();	
		$.post(metodo, params, function (respuesta) {
			OcultarUpdateProgress();
			if(respuesta.status==1){
				var notificaciones=$(JSON.parse(respuesta.message));
				var strNotificaciones='';
				for (var i = 0; i < notificaciones.length; ++i) {
					strNotificaciones+=that.pintarPublicacion($(JSON.parse(notificaciones[i].Dc_description))[0],notificaciones[i].Dc_source, notificaciones[i].GNOSSID, notificaciones[i].Cargada);
				}
				$('#paneles-alertas .resource-list-wrap').html('');
				$('#paneles-alertas .resource-list-wrap').append(strNotificaciones);
				collapseResource.init();
				
			}
			else{
				//that.modal_error.find('p').html(varErrorCodigoAccesoIncorrecto[lang]);
				//that.modal_error.removeClass('oculto');
			}
			}, 'json').fail(function() {
			OcultarUpdateProgress();
		});
	},
	pintarPublicacion: function (publication, source, idGnoss, cargada){
		var htmlPublicacion=`<article class="resource plegado">
		<div class="wrap">
		<div class="middle-wrap">
		<div class="title-wrap">
		<h2 class="resource-title">
		<a href="`+publication.url[0].link+`" target="_blank">`+publication.title+`</a>`;
		if(cargada){
			htmlPublicacion+=`<span class="tag-title update">Actualizado</span>`;
			
			}else{
			htmlPublicacion+=`<span class="tag-title new">Nuevo</span>`;
			
		}
		
		htmlPublicacion+=`</h2>
		<ul class="no-list-style d-flex align-items-center">
		<li>`;
		if(cargada){
			htmlPublicacion+=`<a class="btn btn-fusionar" href="javascript: void(0);" data-toggle="modal" data-target="#modal-fusionar">
			<span class="texto">Fusionar</span>
			</a>`;
			
			}else{
			htmlPublicacion+=`<a class="btn btn-anadir" href="javascript: void(0);" data-value="`+idGnoss+`" data-toggle="modal" data-target="#modal-anadir-item-cv">
			<span class="texto">Añadir</span>
			</a>`;
		}
		htmlPublicacion+=`
		</li>
		</ul>
		<span class="material-icons arrow no-line">keyboard_arrow_down</span>
		</div>
		<div class="content-wrap">
		<div class="description-wrap">
		<div class="group fuente">
		<p class="title">Fuente</p>
		<p>`+source+`</p>
		</div>
		<div class="group fecha">
		<p class="title">Fecha de publicación</p>
		<p>`+publication.dataIssued.datimeTime+`</p>
		</div>
		<div class="group nombre">
		<p class="title">Nombre de la publicación</p>
		<p>`+publication.title+`</p>
		</div>
		<div class="group authors">
		<p class="title">Autores</p>
		<p>`+this.pintarAutoresPublicacion(publication.seqOfAuthors)+`</p>
		</div>
		<div class="group identificador">
		<p class="title">Identificador de publicación</p>
		<p>https://doi.org/`+publication.doi+`</p>
		</div>
		<div class="group tipo">
		<p class="title">Tipo de producción</p>
		<p>`+publication.typeOfPublication+`</p>
		</div>
		</div>
		</div>
		</div>
		</div>
        </article>`;
		return htmlPublicacion;
		
	}, 
	pintarAutoresPublicacion: function(seqAutors){
		var strAutores='';
		for (var i = 0; i < seqAutors.length; ++i) {
			strAutores+=seqAutors[i].surname +", " +seqAutors[i].name[1];
			if(seqAutors.length!=i+1){
				strAutores+="; ";
			}
		}
		return strAutores;
		
	}
	
}
