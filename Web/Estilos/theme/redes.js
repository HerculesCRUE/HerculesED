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
                var html = `<div class="form-group mb-4"><div class="d-flex matchingTooltip"><label id="${valor.id}" class="control-label d-block">${label}</label><span class="material-icons-outlined" style="margin-left: 10px; width: 24px"">information</span></div><div class="form-check form-check-inline" style="flex-flow: nowrap column"><div style="width:100%"><input type="radio" name="radioMatching" id="check-si" value="true" class="form-check-input form-control not-outline"> <label for="html">Sí</label></div> <div style="width:100%"><input type="radio" name="radioMatching" id="check-no" value="false" class="form-check-input form-control not-outline"> <label for="html">No</label></div> </div></div>`;
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

        $("div.block").addClass("no-cms-style");
        $('.matchingTooltip').find('span').tooltip({
            html: true,
            title: traducir.GetText('AYUDA_MATCHING'),
            placement: 'right',
            template: '<div class="tooltip background-gris-oscuro" role="tooltip"><div class="tooltip-arrow"></div><div class="tooltip-inner"></div></div>'
        });
        OcultarUpdateProgress();
    });
});

function GuardarDatos() {
    MostrarUpdateProgress();
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
    }).done(function (data) {
        OcultarUpdateProgress();
        mostrarNotificacion("success", "Datos guardados correctamente");
    }).fail(function (data) {
        OcultarUpdateProgress();
        mostrarNotificacion("error", "Error al guardar los datos");
    });
};
var traducir = {
    GetText: function (id, param1, param2, param3, param4) {
        if ($('#' + id).length) {
            var txt = $('#' + id).val();
            if (param1 != null) {
                txt = txt.replace("PARAM1", param1);
            }
            if (param2 != null) {
                txt = txt.replace("PARAM2", param1);
            }
            if (param3 != null) {
                txt = txt.replace("PARAM3", param1);
            }
            if (param4 != null) {
                txt = txt.replace("PARAM4", param1);
            }
            return txt;
        } else {
            return id;
        }
    }
}