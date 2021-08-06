/*
    Theme Name: GNOSS Front - MyGnoss community theme
    Theme URI: http://dewenir.es

    Author: GNOSS Front
    Author URI: http://dewenir.es

    Description: Fichero community del tema de MyGNOSS.
    Version: 1.0
*/

var clonarMenuUsuario = {
    init: function () {
        this.config();
        this.copiar();
        return;
    },
    config: function () {
        this.body = body;
        this.header = this.body.find('#header');
        this.usuario = this.header.find('.col.col03 .usuario');
        this.menuOriginal = this.body.find('#menuLateralUsuario');
        this.navegacion = this.menuOriginal.find('#navegacion');
        this.navegacionClonado = this.clonar()
        return;
    },
    clonar: function () {
        var navegacionClonadoAux = this.navegacion.clone();
        navegacionClonadoAux.attr('id', 'navegacionClonado');
        navegacionClonadoAux.attr('class', 'navegacion clonado');
        return navegacionClonadoAux;
    },
    copiar: function () {
        if (this.navegacionClonado.length > 0) {
            this.usuario.prepend(this.navegacionClonado);
        }
        return;
    }
};

var accionesBuscadorCabecera = {
    init: function () {
        this.config();
        //this.mover();
        this.posicionar();
        this.comportamiento();
        return;
    },
    config: function () {
        this.body = body;
        this.header = this.body.find('#header');
        this.upperRow = this.header.find('.upper-row');
        this.col02 = this.header.find('.col.col02');
        this.col03 = this.header.find('.col.col03');
        this.buscador = this.body.find('.col-buscador');
        this.main = this.body.find('main');
        return;
    },
    mover: function () {
        this.buscador.removeClass('col col-12');
        this.col02.append(this.buscador);
        return;
    },
    posicionar: function () {
        var that = this;
        var container = that.main.find('.container');
        that.buscador.css('left', container.offset().left);

        $(window).resize(function () {
            that.buscador.css('left', container.offset().left);
        });
        return;
    },
    comportamiento: function () {
        var that = this;
        var buscador = this.col03.find('.buscar > a');
        var icono = buscador.find('span');

        buscador.off('click').on('click', function () {
            if (that.upperRow.hasClass('show')) {
                that.upperRow.removeClass('show');
                icono.text('search');
            } else {
                that.upperRow.addClass('show');
                icono.text('close');
            }
        });
        return;
    }
};

var communityMenuMovil = {
    init: function () {
        this.config();
        this.comportamiento();
        return;
    },
    config: function () {
        this.body = body;
        this.menu = this.body.find('#community-menu');
        return;
    },
    comportamiento: function () {
        var item = this.menu.find('li');

        item.off('click').on('click', function () {
            var that = $(this);
            that.parent().toggleClass('visible');
        });
        return;
    }
};

var accionesCurriculum = {
    init: function () {
        this.cambiarSeccion();
        this.montarTabla();
        this.expandAll();
        this.collapseAll();
        this.collapse();
    },
    cambiarSeccion: function () {
        var cabecera = $('.cabecera-cv');
        var h1_container = cabecera.find('.h1-container');
        var enlace = h1_container.find('.dropdown-menu a');

        enlace.click(function (e) {
            var id = $(this).attr('href');
            enlace.removeClass('active');
            $(id).tab('show');
        });
    },
    montarTabla: function () {
        this.tabla = body.find('.tabla-curriculum');

        this.opciones = {
            lengthChange: false,
            info: false
        };

        this.tabla.DataTable(this.opciones);
    },
    expandAll: function () {
        var button = $('#expandAll');

        button.off('click').on('click', function (e) {
            var $this = $(e.target);
            var dataTarget = $this.attr('data-target');

            $(dataTarget + ' ' + 'a[data-toggle="collapse"]').each(function (i, event) {
                var $this = $(event);
                var objectID = $this.attr('href');
                if ($(objectID).hasClass('in') === false) {
                    $(objectID).collapse('show');
                    $(objectID).parent().addClass('active');
                }
            });
        });
    },
    collapseAll: function () {
        var button = $('#collapseAll');

        button.off('click').on('click', function (e) {
            var $this = $(e.target);
            var dataTarget = $this.attr('data-target');

            $(dataTarget + ' ' + 'a[data-toggle="collapse"]').each(function (i, event) {
                var $this = $(event);
                var objectID = $this.attr('href');
                $(objectID).collapse('hide');
                $(objectID).parent().removeClass('active');
            });
        });
    },
    collapse: function () {
        var button = $('.arrow');

        button.off('click').on('click', function () {
            var resource = $(this).parents('.resource');
            if (resource.hasClass('activo')) {
                resource.removeClass('activo');
            } else {
                resource.addClass('activo');
            }
        });
    }
};

var iniciarComportamientoImagenUsuario = {
    init: function () {
        this.config();
        this.montarPlugin();
    },
    config: function () {
        this.droparea = $('#foto-perfil-cv');
    },
    montarPlugin: function () {
        options = {
            sizeLimit: 250,
        };
        this.droparea.imageDropArea2(options);
    },
};

$(function () {
    accionesBuscadorCabecera.init();
    communityMenuMovil.init();

    if (body.hasClass('fichaRecurso-investigador')) {
        filtrarMovil.init();
        buscadorSeccion.init();
        cambioVistaListado.init();
        comportamientoCargaFacetasComunidad();
    }

    if (body.hasClass('page-cv')) {
        accionesCurriculum.init();
        iniciarComportamientoImagenUsuario.init();
    }
});

; (function ($) {

    /**
    Para hacer que la imagen se guarde directamente por ajax hay que configurar las siguientes opciones
    (Por defecto es "false" por lo que el File se guardará con el formulario al que pertenezca):

    options: {
        ajax: {
            url: (string) url a la que se quiere hacer la petición,
            param_name: (string) nombre del parámetro con el que se va a pasar el objeto File
        }
    }

    Se puede configurar también cual serán los selectores para cada elemento del droparea
    options: {
        inputSelector: ".image-uploader__input",
        dropAreaSelector: ".image-uploader__drop-area",
        preview: ".image-uploader__preview",
        previewImg: ".image-uploader__img",
        errorDisplay: ".image-uploader__error",
    }

    Configurar límite de tamaño en Kb (por defecto sin límite)
    options: {
        sizeLimit: 100
    }

    El html por defecto debería ser así:
        <div class="image-uploader js-image-uploader">
            <div class="image-uploader__preview">
                <!-- Si hay una imagen en el servidor pintarla en el src, si no dejarlo vacío  -->
                <img class="image-uploader__img" src="">
            </div>
            <div class="image-uploader__drop-area">
                <div class="image-uploader__icon">
                    <span class="material-icons">backup</span>
                </div>
                <div class="image-uploader__info">
                    <p><strong>Arrastra y suelta en la zona punteada una foto para tu perfil</strong></p>
                    <p>Imágenes en formato .PNG o .JPG</p>
                    <p>Peso máximo de las imágenes 250 kb</p>
                </div>
            </div>
            <div class="image-uploader__error">
                <p class="ko"></p>
            </div>
            <input type="file" class="image-uploader__input">
        </div>
    */

    $.imageDropArea2 = function (element, options) {
        var defaults = {
            sizeLimit: false,
            ajax: false,
            inputSelector: ".image-uploader__input",
            dropAreaSelector: ".image-uploader__drop-area",
            preview: ".image-uploader__preview",
            previewImg: ".image-uploader__img",
            errorDisplay: ".image-uploader__error",
            // funcionPrueba: function() {}
        };
        var plugin = this;

        plugin.settings = {};

        var $element = $(element);
        var element = element;

        plugin.init = function () {
            plugin.settings = $.extend({}, defaults, options);
            plugin["input"] = $(plugin.settings.inputSelector);
            plugin["dropAreaSelector"] = $(plugin.settings.dropAreaSelector);
            plugin["preview"] = $(plugin.settings.preview);
            plugin["previewImg"] = $(plugin.settings.previewImg);
            plugin["errorDisplay"] = $(plugin.settings.errorDisplay);
            addInputChangeEvent();
            addDragAndDropEvents();
            initialImageCheck();
        };

        /**
         * Comprueba si en el inicio del plugin ya hay una imagen
         * para añadirla al input file
         */
        var initialImageCheck = async function () {
            const image_url = plugin.previewImg.attr("src");
            if (image_url && image_url !== "") {
                const dT = new ClipboardEvent('').clipboardData || new DataTransfer();
                const response = await fetch(image_url);
                const data = await response.blob();
                const metadata = {
                    type: 'image/jpeg'
                };
                const file = new File([data], image_url, metadata);
                dT.items.add(file);
                plugin.input.get(0).files = dT.files;
                plugin.input.trigger('change');
            };
        };

        var addInputChangeEvent = function () {
            plugin.input.change(function () {
                if (!isFileImage()) {
                    displayError('El archivo no es una imágen válida. Los formatos válidos son .png y .jpg.');
                    return;
                }

                if (!imageSizeAllowed()) {
                    displayError('El archivo pesa demasiado. El límite es ' + plugin.settings.sizeLimit + 'Kb');
                    return;
                }

                if (plugin.settings.ajax) {
                    uploadImageWithAjax();
                } else {
                    showImageTemporalPreview();
                }
            });
        };

        /**
         * Muestra la imagen que se ha añadido al input file
         */
        var showImageTemporalPreview = function () {

            const [file] = plugin.input.get(0).files
            if (file) {
                showImagePreview(URL.createObjectURL(file))
            }
        };

        /**
         * Incia lógica para llamada ajax
         */
        var uploadImageWithAjax = function () {

            if (checkAjaxSettings()) {
                return;
            };

            var data = new FormData();
            var files = plugin.input.get(0).files;
            if (files.length > 0) {
                data.append("ImagenRegistroUsuario", files[0]);
            }
            upload(data);
        };

        /**
         * Llamada ajax
         */
        var upload = function () {
            $.ajax({
                url: document.location.href,
                type: "POST",
                processData: false,
                contentType: false,
                data: data,
                success: function (response) {
                    onSuccesResponse(response);
                },
                error: function (er) {
                    displayError(er.statusText);
                },
            });
        }

        /**
         * @return {boolean}
         * true: Las opciones de ajax se han configurado
         * false: Las opciones de ajax se no han configurado
         */
        var checkAjaxSettings = function () {
            if (plugin.settings.ajax.hasProperty('param_name')) {
                console.log('La opción "ajax.param_name" no está configurada')
                return false;
            }
            if (plugin.settings.ajax.hasProperty('url')) {
                console.log('La opción de "ajax.url" no está configurada')
                return false;
            }
            return true;
        };

        /**
         * @return {string} error
         * Shows error message
         */
        var displayError = function (error) {
            plugin.errorDisplay.find(".ko").text(error);
            plugin.errorDisplay.find(".ko").show();
            plugin.preview.removeClass("show-preview");
        };

        /**
         * Hides error message
         */
        var hideError = function () {
            plugin.errorDisplay.find(".ko").hide();
        };

        /**
         * @param {string} response
         */
        var onSuccesResponse = function (response) {
            if (response.indexOf("imagenes/") === 0) {
                // Es una url de imagen
                showImagePreview(response);
                showLoadingImagePreview(false);
            } else {
                // No es una url de imagen
                displayError(response);
                showLoadingImagePreview(false);
            }
        };

        /**
         * @param {boolean} showLoading: Indicar si ha iniciado la carga y por lo tanto, es necesario mostrar un "loading".
         * true: Mostrará el "loading"
         * false: Quitar ese "loading" -> Fin carga de imagen
         */
        var showLoadingImagePreview = function (showLoading) {
            // Mostrar loading
            if (showLoading) {
                // Quitar la imagen actual del preview
                plugin.preview.attr("src", "");
            } else {
                // Quitar loading
                $(".spinner-border").remove();
            }
        };

        /**
         * @param {string} : url de la imagen
         */
        var showImagePreview = function (url) {
            let image_url = url;
            if (plugin.settings.ajax) {
                image_url = "http://serviciospruebas.gnoss.net" + "/" + url;
            }
            hideError();
            plugin.previewImg.attr("src", image_url);
            plugin.preview.addClass("show-preview");
        };

        var addDragAndDropEvents = function () {
            plugin.dropAreaSelector
                .off("dragenter dragover")
                .on("dragenter dragover", function (e) {
                    e.preventDefault();
                    e.stopPropagation();
                });

            plugin.dropAreaSelector.off("click").on("click", function (e) {
                e.preventDefault();
                e.stopPropagation();
                plugin.input.trigger("click");
            });

            plugin.dropAreaSelector.off("dragleave").on("dragleave", function (e) {
                e.preventDefault();
                e.stopPropagation();
            });

            plugin.dropAreaSelector.off("drop").on("drop", function (e) {
                e.preventDefault();
                e.stopPropagation();
                let dt = e.originalEvent.dataTransfer;
                let files = dt.files;
                plugin.input.get(0).files = files;
                plugin.input.trigger("change");
            });
        };

        /**
         * @return {boolean} return if file is a valid image
         */
        var isFileImage = function () {
            const acceptedImageTypes = ['image/jpeg', 'image/png'];
            const file = plugin.input.get(0).files[0];
            return file && acceptedImageTypes.includes(file['type'])
        }

        /**
         * @return {boolean} return if file is valid size
         */
        var imageSizeAllowed = function () {
            if (plugin.settings.sizeLimit) {
                const file = plugin.input.get(0).files[0];
                return (file.size / 1024) < plugin.settings.sizeLimit;
            } else {
                return true;
            }
        }

        plugin.init();
    };

    // add the plugin to the jQuery.fn object
    $.fn.imageDropArea2 = function (options) {
        return this.each(function () {
            if (undefined == $(this).data("imageDropArea2")) {
                var plugin = new $.imageDropArea2(this, options);
                $(this).data("imageDropArea2", plugin);
            }
        });
    };
})(jQuery);