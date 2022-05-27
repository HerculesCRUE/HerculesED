var importarCVN = {
    init: function (){
        $('#file_cvn').GnossDragAndDrop({
            acceptedFiles: '.PDF',
			maxSize: 15000,
            onFileAdded: function (plugin, files) {
                $('.col-contenido .botonera').css('display', 'block');
            },
            onFileRemoved: function (plugin, files) {
                $('.col-contenido .botonera').css('display', 'none');
            }
        });
    }
};