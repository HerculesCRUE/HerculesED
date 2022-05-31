var importarCVN = {
	idUsuario: null,
    init: function (){		
		this.config();		
        return;
        
    },
	config: function (){
		var that=this;
		this.idUsuario = $('#inpt_usuarioID').val();
		$('#file_cvn').GnossDragAndDrop({
            acceptedFiles: ["pdf"],
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
    },
	//Carga los datos del CV para la exportacion
    cargarCV: function() {
		$('.col-contenido.paso1').hide();
		$('.col-contenido.paso2').show();
		var formData = new FormData();
		formData.append('pCVID', 'idcv');
		formData.append('File', $('#file_cvn')[0].files[0]);
		 
		$.ajax({
			url: 'http://serviciosedma.gnoss.com/importadorcv/ImportadorCV/Preimportar',
			headers: { 'Content-Type': 'multipart/form-data;' },
			data: formData,
			processData: false,
			type: 'POST',
			success: function ( data ) {
				alert( data );
			}
		});
        
    }
};