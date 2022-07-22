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
        console.log(data);
        data.forEach(function (valor, indice, data) {
            var placeholder = $('#PLACEHOLDER_' + valor.id.toUpperCase()).val();
            var label = $('#' + valor.id.toUpperCase()).val();
            var html = `<div class="form-group mb-4"><label id="${valor.id}" class="control-label d-block">${label}</label> <input placeholder="${placeholder}" type="text" name="fname" id="" value="${valor.valor}" class="form-control not-outline"></div>`;
            if (valor.id == "tokenGitHub") {
                var help = "Haz click aquí para generar un token de acceso a tu cuenta de GitHub";
                var tokenPage = "https://github.com/settings/tokens"

            } else if (valor.id == "tokenFigShare") {
                var help = "Haz click aquí para generar un token de acceso a tu cuenta de FigShare";
                var tokenPage = "https://figshare.com/account/applications"
            } else if (valor.id == "useMatching") {
                var html = `<div class="form-group mb-4"><label id="${valor.id}" class="control-label d-block">${label}</label><div class="form-check form-check-inline"><input type="radio" name="radioMatching" id="check-si" value="true" class="form-check-input form-control not-outline"> <label for="html">Sí</label> <input type="radio" name="radioMatching" id="check-no" value="false" class="form-check-input form-control not-outline"> <label for="html">No</label> </div></div>`;
            }

            $('form.formulario-edicion fieldset').append(html);
            if (help != undefined) {
                $('label#' + valor.id).after(
                    `<a href="${tokenPage}" target="_blank" class="text-primary">${help}</a>`
                );
            }

            if(valor.valor == "true")
            {
                $("input[id=check-si]").prop("checked", true);
                $("input[id=check-no]").prop("checked", false);
            }
            else if (valor.valor == "false")
            {
                $("input[id=check-si]").prop("checked", false);
                $("input[id=check-no]").prop("checked", true);
            } 
            else 
            {
                $("input[id=check-si]").prop("checked", false);
                $("input[id=check-no]").prop("checked", true);
            }
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
        if (div.children()[div.children().length - 1].value != "") {
            valorData = div.children()[div.children().length - 1].value;
        }
        if(div.children()[0].innerHTML == "Matching")
        {
            valorData = $('input[name=radioMatching]:checked').attr("value");
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
        mostrarNotificacion("success", "Datos guardados correctamente");
    }).fail(function (data) {
        mostrarNotificacion("error", "Error al guardar los datos");
    });
}