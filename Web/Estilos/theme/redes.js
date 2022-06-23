//<div id="contenedor">
//</div>
//<button onclick="GuardarDatos()">Guardar</button>
var depuracion = false;

$(document).ready(function () {    
    const localUrlBase = "https://localhost:44347/";
    var url = "http://serviciosedma.gnoss.com/servicioexterno/RedesUsuario/GetDatosRedesUsuario"
    if (depuracion) {
        url = localUrlBase + "RedesUsuario/GetDatosRedesUsuario"
    }
    var arg = {};
    arg.pIdGnossUser = $('#inpt_usuarioID').val();
    var datos = null;
    MostrarUpdateProgress();
    $.get(url, arg, function (data) {
        data.forEach(function (valor, indice, data) {
            var html = `<div class="form-group mb-4"><label id="${valor.id}"class="control-label d-block">${valor.nombre}</label> <input placeholder="Introduce el identificador de FigShare" type="text" name="fname" id="" value="${valor.valor}" class="form-control not-outline"></div>`;

            $('form.formulario-edicion fieldset').append(html);
        });
        OcultarUpdateProgress();
    });
});

function GuardarDatos() {
    var url = "http://serviciosedma.gnoss.com/servicioexterno/RedesUsuario/SetDatosRedesUsuario"
    if (depuracion) {
        url = localUrlBase + "RedesUsuario/SetDatosRedesUsuario"
    }
    var lista = [];
    $("form.formulario-edicion fieldset").children('div').each(function () {
        var div = $(this);
        var valorData = "";
        if (div.children()[1].value != "") {
            valorData = div.children()[1].value;
        }
        var obj = { nombre: div.children()[0].innerHTML, id: div.children()[0].id, valor: valorData };
        lista.push(obj);
    });
    var arg = {};
    arg.pIdGnossUser = $('#inpt_usuarioID').val();
    arg.dataUser = lista;
    $.post(url, arg, function (data) {
        OcultarUpdateProgress();
    }).done(function (data) {
        mostrarNotificacion("success","Datos guardados correctamente");
    }).fail(function (data) {
        mostrarNotificacion("error","Error al guardar los datos");
    });
}