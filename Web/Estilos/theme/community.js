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
        this.copiarNavegacion();
        return;
    },
    config: function () {
        this.body = body;
        this.header = this.body.find('#header');
        this.usuario = this.header.find('.col.col03 .usuario');
        this.menuOriginal = this.body.find('#menuLateralUsuario');
        this.navegacion = this.menuOriginal.find('#navegacion');
        this.navegacionClonado = this.clonarNavegacion();
        return;
    },
    clonarNavegacion: function () {
        var navegacionClonadoAux = this.navegacion.clone();
        navegacionClonadoAux.attr('id', 'navegacionClonado');
        navegacionClonadoAux.attr('class', 'navegacion clonado');
        return navegacionClonadoAux;
    },
    copiarNavegacion: function () {
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

var menusLateralesManagement = {
    init: function () {
        this.config();
        this.montarMenusLaterales();
        this.comportamientoBotonCerrar();
        return;
    },
    config: function () {
        this.body = body;
        this.header = this.body.find('#header');
        this.buscador = this.header.find('.col-buscador');
        this.main = this.body.find('main');
        return;
    },
    montarMenusLaterales: function () {
        this.montarMenuLateral();
        this.montarMenuLateralComunidad();
        this.montarMenuLateralMetabuscador();
        this.montarMenuLateralUsuario();
        // this.onResize();
    },
    montarMenuLateralUsuario: function () {
        if (!$('#menuLateralUsuario').length > 0) return;

        $('#menuLateralUsuario').slideReveal({
            trigger: $("#menuLateralUsuarioTrigger"),
            width: 320,
            overlay: true,
            position: 'right',
            push: false,
        });
    },
    montarMenuLateralMetabuscador: function () {
        var that = this;
        var container = that.main.find('.container');

        if (!$('#menuLateralMetabuscador').length > 0) return;

        $('#menuLateralMetabuscador').slideReveal({
            trigger: $('#txtBusquedaPrincipal'),
            width: 740,
            overlay: true,
            position: 'left',
            push: false,
            show: function (slider) {
                that.body.addClass('metabuscador-abierto');

                // Add position
                var position;
                setTimeout(() => {
                    if (container.offset().left < 177) {
                        position = that.buscador.offset().left;
                    } else {
                        position = container.offset().left;
                    }
                    slider.css('left', position);
                });

                // Click to close
                $(document).on('click', '#menuLateralMetabuscador, .row.upper-row, #txtBusquedaPrincipal', function(e) {
                    e.stopPropagation();
                    var $target = $(e.target);
                    if(!$target.closest('#menuLateralMetabuscador').length && $('#menuLateralMetabuscador').is(":visible")) {
                        $('#menuLateralMetabuscador').slideReveal('hide');
                    }
                });
                $('#txtBusquedaPrincipal').off('click');
                $('#txtBusquedaPrincipal').on('click', function(e){
                    e.stopPropagation();
                    $('#menuLateralMetabuscador').slideReveal('show');
                });
            },
            hide: function () {
                that.body.removeClass('metabuscador-abierto');
            }
        });
    },
    montarMenuLateral: function () {
        if (!$('#menuLateral').length > 0) return;

        $('#menuLateral').slideReveal({
            trigger: $("#menuLateralTrigger"),
            width: 320,
            overlay: true,
            position: 'left',
            push: false,
        });
    },
    montarMenuLateralComunidad: function () {
        if (!$('#menuLateralComunidad').length > 0) return;

        body.append($('#menuLateralComunidad'))

        $('#menuLateralComunidad').slideReveal({
            trigger: $("#menuLateralComunidadTrigger"),
            width: 320,
            overlay: true,
            position: 'left',
            push: false,
        });
    },
    comportamientoBotonCerrar: function () {
        var localBody = this.body;
        var menus = localBody.find('.menuLateral');
        var cerrar = menus.find('.header .cerrar');

        cerrar.on('click', function () {

            var item = $(this);
            var menu = item.closest('.menuLateral');
            menu.slideReveal("hide");

        });

        return;
    }
};

var metabuscador = {
    init: function () {
        this.config();
        this.comportamiento();
        return;
    },
    config: function () {
        this.body = body;
        this.header = this.body.find('#header');
        this.metabuscadorTrigger = this.header.find('.col-buscador');
        this.metabuscador = this.body.find('#menuLateralMetabuscador');
        this.input = this.metabuscadorTrigger.find('#txtBusquedaPrincipal');
        this.resultadosMetabuscador = this.body.find('#resultadosMetabuscador');
        this.verMasEcosistema = this.body.find('#verMasEcosistema');
        return;
    },
    comportamiento: function () {
        var that = this;

        that.metabuscadorTrigger.on('click', function (e) {
            that.input.focus();
        });

        that.input.on('keyup', function () {
            var val = that.input.val();
            if (val.length > 0) {
                that.cargarResultados();
            } else {
                that.ocultarResultados();
            }
        });

        return;
    },
    ocultarResultados: function () {
        this.metabuscador.removeClass('mostrarResultados');
    },
    cargarResultados: function () {
        var that = this;
        this.metabuscador.addClass('mostrarResultados');
        // simular la carga de cada secciÃ³n
        that.cargarRecursos();
        that.cargarDebates();
        that.cargarPreguntas();
        that.cargarPersonas();
        that.linkVerEnElEcosistema();
    },
    showLoader: function (loader_div, time) {
        var loaderBar = loader_div.progressBarTimer({
            autostart: false,
            timeLimit: time,
            warningThreshold: 0,
            baseStyle: '',
            warningStyle: '',
            completeStyle: '',
            smooth: true,
            striped: false,
            animated: false,
            height: 12
        });
        return loaderBar;

    },
    cargar: function (tipo_recurso, tiempo) {
        var loader_container = $('#loader-' + tipo_recurso + '-wrap');
        loader_container.find('.progress-bar').remove();

        var loader_div = $('<div id="#loader-' + tipo_recurso + '" class="progress-bar"></div>');
        loader_container.append(loader_div);
        loader_container.show();

        var loader = this.showLoader(loader_div, tiempo);
        loader.start();

        return new Promise((resolve, reject) => {
            setTimeout(function () {
                loader.stop();
                loader_div.remove();
                loader_container.hide();
                resolve();
            }, tiempo * 1000);
        });
    },
    cargarRecursos: function () {
        var that = this;
        var bloque = that.resultadosMetabuscador.find('.bloque.recursos');
        bloque.hide();

        var procesoCarga = this.cargar('recursos', 5);
        procesoCarga.then(function () {
            bloque.show();
        });
    },
    cargarDebates: function () {
        var that = this;
        var bloque = that.resultadosMetabuscador.find('.bloque.debates');
        bloque.hide();

        var procesoCarga = this.cargar('debates', 4);
        procesoCarga.then(function () {
            bloque.show();
        });
    },
    cargarPreguntas: function () {
        var that = this;
        var bloque = that.resultadosMetabuscador.find('.bloque.preguntas');
        bloque.hide();

        var procesoCarga = this.cargar('preguntas', 3);
        procesoCarga.then(function () {
            bloque.show();
        });
    },
    cargarPersonas: function () {
        var that = this;
        var bloque = that.resultadosMetabuscador.find('.bloque.personas');
        bloque.hide();

        var procesoCarga = this.cargar('personas', 2);
        procesoCarga.then(function () {
            bloque.show();
        });
    },
    linkVerEnElEcosistema: function () {
        var that = this;
        that.verMasEcosistema.hide();
        setTimeout(() => {
            that.verMasEcosistema.show()
        }, 5000);
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

var accionesPlegarDesplegarModal = {
    init: function () {
        this.cambiarSeccion();
        this.montarTabla();
        this.expandAll();
        this.collapseAll();
        this.collapse();
        this.expandAllModal();
        this.collapseAllModal();
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
    },
    expandAllModal: function () {
        var button = $('.expandAll');

        button.off('click').on('click', function(e) {
            var $this = $(e.target);
            var dataTarget = $this.attr('data-target');
            var form = button.parents('.formulario-edicion');

            form.find(dataTarget + '[data-toggle="collapse"]').each(function (i, event) {
                var $this = $(event);
                var objectID = $this.attr('href');
                $(objectID).collapse('hide');
            });
        });
    },
    collapseAllModal: function () {
        var button = $('.collapseAll');

        button.off('click').on('click', function (e) {
            var $this = $(e.target);
            var dataTarget = $this.attr('data-target');
            var form = button.parents('.formulario-edicion');

            form.find(dataTarget + '[data-toggle="collapse"]').each(function (i, event) {
                var $this = $(event);
                var objectID = $this.attr('href');
                $(objectID).collapse('show');
            });
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

var iniciarDatepicker = {
    init: function() {
        this.montarPlugin();
    },
    montarPlugin: function() {
        $('.datepicker').datepicker({
            closeText: 'Cerrar',
            prevText: '<Ant',
            nextText: 'Sig>',
            currentText: 'Hoy',
            monthNames: ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio', 'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'],
            monthNamesShort: ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun', 'Jul', 'Ago', 'Sep', 'Oct', 'Nov', 'Dic'],
            dayNames: ['Domingo', 'Lunes', 'Martes', 'MiÃ©rcoles', 'Jueves', 'Viernes', 'SÃ¡bado'],
            dayNamesShort: ['Dom', 'Lun', 'Mar', 'MiÃ©', 'Juv', 'Vie', 'SÃ¡b'],
            dayNamesMin: ['Do', 'Lu', 'Ma', 'Mi', 'Ju', 'Vi', 'SÃ¡'],
            weekHeader: 'Sm',
            dateFormat: 'dd/mm/yy',
            firstDay: 1,
            isRTL: false,
            showMonthAfterYear: false,
            yearSuffix: ''
        });
    }
};

var collapseResource = {
    init: function() {
        this.collapse();
    },
    collapse: function() {
        var boton = $('.arrow');

        boton.off('click').on('click', function() {
            var resource = $(this).parents('.resource');

            if (resource.hasClass('plegado')) {
                $(this).text('keyboard_arrow_down');
                resource.removeClass('plegado');
            } else {
                $(this).text('keyboard_arrow_up');
                resource.addClass('plegado');
            }
        });
    }
};

var comportamientoVerMasVerMenos = {
    init: function () {
        this.comportamiento();
    },
    comportamiento: function () {
        $('.description-wrap .moreResults .ver-mas').off('click').on('click', function () {
            var showMore = $(this).closest('.showMore');
            showMore.find('.ver-mas').css('display', 'none');
            showMore.find('.ver-menos').css('display', 'flex');
            var description_wrap = $(this).parents('.description-wrap');
            var desc = description_wrap.find('.desc');
            desc.css('display', 'block');
        });

        $('.description-wrap .moreResults .ver-menos').off('click').on('click', function () {
            var showMore = $(this).closest('.showMore');
            showMore.find('.ver-menos').css('display', 'none');
            showMore.find('.ver-mas').css('display', 'flex');
            var description_wrap = $(this).parents('.description-wrap');
            var desc = description_wrap.find('.desc');
            desc.css('display', '-webkit-box');
        });
    }
};

var comportamientoVerMasVerMenosTags = {
    init: function() {
        this.comportamiento();
    },
    comportamiento: function() {
        $('.list-wrap .moreResults .ver-mas').off('click').on('click', function () {
            var list = $(this).closest('.list-wrap');
            list.find('ul > .ocultar').show(200);
            list.find('.ver-mas').css('display', 'none');
            list.find('.ver-menos').css('display', 'flex');
        });

        $('.list-wrap .moreResults .ver-menos').off('click').on('click', function () {
            var list = $(this).closest('.list-wrap');
            list.find('ul > .ocultar').hide(200);
            list.find('.ver-menos').css('display', 'none');
            list.find('.ver-mas').css('display', 'flex');
        });
    }
};

var operativaFormularioAutor = {
    init: function () {
        this.config();
        this.formPrincipal();
        this.formAutor();
        this.formCodigo();
    },
    config: function () {
        this.modal_autor = $('#modal-anadir-autor');
        this.formularioPrincipal = this.modal_autor.find('.formulario-principal');
        this.formularioAutor = this.modal_autor.find('.formulario-autor');
        this.formularioCodigo = this.modal_autor.find('.formulario-codigo');
    },
    formPrincipal: function () {
        var that = this;
        var btn = $('.form-principal');

        // Si se hace click en el enlace
        btn.off('click').on('click', function() {
            that.formularioPrincipal.show();
            that.formularioCodigo.hide();
            that.formularioAutor.hide();
        });

        this.modal();
    },
    formAutor: function () {
        var that = this;
        var btn = $('.form-autor');

        // Si se hace click en el enlace
        btn.off('click').on('click', function() {
            that.formularioPrincipal.hide();
            that.formularioCodigo.hide();
            that.formularioAutor.show();
        });

        this.modal();
    },
    formCodigo: function () {
        var that = this;
        var btn = $('.form-buscar');

        // Si se hace click en el enlace
        btn.off('click').on('click', function() {
            that.formularioPrincipal.hide();
            that.formularioCodigo.show();
            that.formularioAutor.hide();
        });

        this.modal();
    },
    modal: function () {
        var that = this;

        $('#modal-anadir-autor').on('show.bs.modal', function () {
            $('#modal-anadir-datos-experiencia').modal('hide');
        });

        $('#modal-editar-autor').on('show.bs.modal', function () {
            $('#modal-anadir-datos-experiencia').modal('hide');
        });

        $('#modal-anadir-autor').on('hide.bs.modal', function () {
            $('#modal-anadir-datos-experiencia').modal('show');
            $(this).find('.formulario-edicion').hide();
            that.formularioPrincipal.show();
        });

        $('#modal-editar-autor').on('hide.bs.modal', function () {
            $('#modal-anadir-datos-experiencia').modal('show');
        });
    }
};

var operativaFormularioTesauro = {
    init: function () {
        $('#modal-editar-entidad-0').on('show.bs.modal', function () {
            $('#modal-anadir-datos-experiencia').modal('hide');
            plegarSubFacetas.init();
        });

        $('#modal-editar-entidad-0').on('hide.bs.modal', function () {
            $('#modal-anadir-datos-experiencia').modal('show');
        });

        $('.js-desplegar-facetas-modal').on('click', function (){
            const button = $(this);
            const aux = button.closest('.entityaux');
            const action_buttons = button.closest('ul').find('li');
            const facetas_con_subfaceta = aux.find('.faceta.con-subfaceta.ocultarSubFaceta');
            const boton_desplegar_faceta = facetas_con_subfaceta.find('.desplegarSubFaceta span');
            boton_desplegar_faceta.trigger('click');
            action_buttons.removeClass('active');
            button.addClass('active');
        });

        $('.js-plegar-facetas-modal').on('click', function (){
            const button = $(this);
            const aux = button.closest('.entityaux');
            const action_buttons = button.closest('ul').find('li');
            const facetas_con_subfaceta = aux.find('.faceta.con-subfaceta:not(.ocultarSubFaceta)');
            const boton_desplegar_faceta = facetas_con_subfaceta.find('.desplegarSubFaceta span');
            boton_desplegar_faceta.trigger('click');
            action_buttons.removeClass('active');
            button.addClass('active');
        });
    }
};

var comportamientoTopicosCV = {
    init: function () {
        $('#modal-anadir-topicos').on('show.bs.modal', function () {
            $('#modal-anadir-datos-experiencia').modal('hide');
        });

        $('#modal-anadir-topicos').on('hide.bs.modal', function () {
            $('#modal-anadir-datos-experiencia').modal('show');
        });
    }
};

var mostrarFichaCabeceraFixed = {
    init: function () {
        this.config();
        this.comportamiento();
    },
    config: function () {
        this.body = body;
        this.cabecera = this.body.find('.cabecera-ficha');
        this.contenido = this.body.find('.contenido-ficha');
    },
    comportamiento: function () {
        if (this.contenido.length < 1) return;
        const position = this.contenido.position().top;
        $(window).scroll(function (e) {
            var scroll = $(window).scrollTop();
            if(scroll >= position) {
                body.addClass('cabecera-ficha-fixed');
                return;
            } else {
                body.removeClass('cabecera-ficha-fixed');
                return;
            }
        });
    }
};

var clonarNombreFicha = {
    init: function () {
        this.config();
        this.copyName();
        this.copyIcon();
    },
    config: function () {
        this.body = body;
        this.cabeceraFicha = this.body.find('.cabecera-ficha');
        this.paneles = this.body.find('.tab-paneles-ficha');
        this.tabs = this.paneles.find('.tabs');
    },
    copyName: function () {
        if (body.hasClass('fichaRecurso-investigador')) {
            var nombre = this.cabeceraFicha.find('.nombre-usuario-wrap .nombre');
        } else {
            var nombre = this.cabeceraFicha.find('.ficha-title');
        }
        var divNombre = $('<div />').addClass('nombre');
        divNombre.append(nombre.text());
        this.tabs.prepend(divNombre);
    },
    copyIcon: function () {
        var icono = this.cabeceraFicha.find('.ficha-icon-wrap');
        this.tabs.find('.nombre').append(icono.html());
    }
};

var montarTooltip = {
    init: function () {
        this.config();
        this.comportamiento();
    },
    config: function () {
        this.body = body;
        this.listWrap = this.body.find('.list-wrap');
        this.info_resource = this.body.find('.info-resource');
        this.quotes = this.body.find('.quotes');
        this.link = this.listWrap.find('.link');
    },
    comportamiento: function () {
        var that = this;
        this.montarTooltips();

        this.link.on('shown.bs.tooltip', function () {
            $('.tooltip').find('.cerrar').off('click').on('click', function () {
                $('.tooltip').tooltip('hide');
            });
        });
    },
    montarTooltips: function () {
        var that = this;
        this.info_resource.tooltip({
            html: true,
            placement: 'bottom',
            template: '<div class="tooltip background-gris grupos" role="tooltip"><div class="tooltip-arrow"></div><div class="tooltip-inner"></div></div>',
        });
        this.quotes.tooltip({
            html: true,
            placement: 'bottom',
            template: '<div class="tooltip background-blanco citas" role="tooltip"><div class="tooltip-arrow"></div><div class="tooltip-inner"></div></div>',
            title: that.getTooltipQuotes()
        });
        this.link.tooltip({
            html: true,
            placement: 'bottom',
            template: '<div class="tooltip background-blanco link" role="tooltip"><div class="tooltip-arrow"></div><div class="tooltip-inner"></div></div>',
            title: that.getTooltipLink(),
            trigger: 'manual'
        }).on("mouseenter", function () {
            //console.log('enter')
            var _this = this;
            $(this).tooltip('show');
            $(this).siblings('.tooltip').on('mouseleave', function () {
                $(_this).tooltip('hide');
            });
        }).on("mouseleave", function () {
            //console.log('leave')
            var _this = this;
            setTimeout(function () {
                if ($('.tooltip:hover').length < 0) {
                    //console.log('hover')
                    $(_this).tooltip("hide")
                }
                $('.tooltip').on('mouseleave', function () {
                    //console.log('leave')
                    $(_this).tooltip("hide");
                });
            }, 100);
        });
    },
    getTooltipQuotes: function () {
        return `<p class="tooltip-title">Fuente de citas</p>
                <span class="material-icons cerrar">close</span>
                <ul class="no-list-style">
                    <li>
                        <span class="texto">SCOPUS</span>
                        <span class="num-resultado">144</span>
                    </li>
                    <li>
                        <span class="texto">WOS</span>
                        <span class="num-resultado">24</span>
                    </li>
                    <li>
                        <span class="texto">INRECS</span>
                        <span class="num-resultado">18</span>
                    </li>
                </ul>`;
    },
    getTooltipLink: function () {
        return `<p class="tooltip-title">Enlazado con</p>
                <span class="material-icons cerrar">close</span>
                <div class="tooltip-content">
                    <img src="./theme/resources/logotipos/logognossazul.png">
                    <p>National Center for Biotechnology Information Search database</p>
                </div>
                <a href="#" class="tooltip-link">Ir al enlace <span class="material-icons">keyboard_arrow_right</span></a>`
    }
};

var contarLineasDescripcion = {
    init: function () {
        this.config();
        this.comportamiento();
    },
    config: function () {
        this.descriptionWrap = $('.description-wrap');
    },
    comportamiento: function () {
        this.descriptionWrap.each(function () {
            var descDiv = $(this).find('.desc');
            var divHeight = parseInt(descDiv.innerHeight());
            var lineHeight = parseInt(descDiv.css('line-height'));
            var lines = parseInt(divHeight / lineHeight);

            // Si no ha sido contado
            if(!$(this).hasClass('counted')) {
                // Do
                if (lines > 3) {
                    descDiv.css('overflow', 'hidden')
                        .css('display', '-webkit-box')
                        .css('-webkit-line-clamp', '3')
                        .css('-webkit-box-orient', 'vertical')
                        .css('text-overflow', 'ellipsis');
                } else {
                    $(this).closest('.description-wrap').find('.moreResults').hide();
                }
                $(this).addClass('counted');
            }
        });
    }
};

// var comportamientoSeleccionarHijo = {
//     init: function () {
//         this.config();
//         this.comportamiento();
//     },
//     config: function () {
//         this.arbol = body.find('.divTesArbol');
//     },
//     comportamiento: function () {
//         var categoria_padre = this.arbol.find('> .categoria-wrap');

//         $.each(categoria_padre, function (i, val) {
//             var panHijos = $(val).find('.panHijos');
//             var categorias_hijas = panHijos.find('> .categoria-wrap');

//             if (panHijos.length > 0) {
//                 // deshabilita las categorias padres
//                 $(val).find('> .categoria input').attr('disabled', true);
//             }

//             if (categorias_hijas.find('.panHijos').length) {
//                 // deshabilita las categorias hijas con hijos
//                 var categoria = categorias_hijas.find('.panHijos').closest('.categoria-wrap');
//                 categoria.find('> .categoria input').attr('disabled', true);
//             }
//         });
//     }
// };

var comportamientoAbrirArbol = {
    init: function () {
        this.config();
        this.comportamiento();
    },
    config: function () {
        this.arbol = body.find('.divTesArbol');
    },
    comportamiento: function () {
        var categoria_padre = this.arbol.find('> .categoria-wrap');
        var categoria_label = categoria_padre.find('.custom-control-label');

        categoria_label.off('click').on('click', function () {
            var padre = $(this).closest('.categoria-wrap');
            var desplegar = padre.find('> .boton-desplegar');
            if (desplegar.length > 0) {
                desplegar.trigger('click');
            }
        });
    }
};

$(function () {
    accionesBuscadorCabecera.init();
    communityMenuMovil.init();
    iniciarDatepicker.init();
    collapseResource.init();
    montarTooltip.init();
    contarLineasDescripcion.init();

    comportamientoVerMasVerMenos.init();
    comportamientoVerMasVerMenosTags.init();

    accionesPlegarDesplegarModal.init();

    if (body.hasClass('fichaRecurso') || body.hasClass('edicionRecurso')) {
        comportamientoCargaFacetasComunidad();
    }

    if (body.hasClass('fichaRecurso')) {
        mostrarFichaCabeceraFixed.init();
        clonarNombreFicha.init();
    }

    if (body.hasClass('fichaRecurso-investigador')) {
        filtrarMovil.init();
        buscadorSeccion.init();
        cambioVistaListado.init();
    }

    if (body.hasClass('page-cv')) {
        iniciarComportamientoImagenUsuario.init();
        operativaFormularioAutor.init();
        operativaFormularioTesauro.init();
        comportamientoTopicosCV.init();
    }

    if (body.hasClass('edicionCluster')) {
        comportamientoAbrirArbol.init();
    }
});

; (function ($) {

    /**
    Para hacer que la imagen se guarde directamente por ajax hay que configurar las siguientes opciones
    (Por defecto es "false" por lo que el File se guardarÃ¡ con el formulario al que pertenezca):

    options: {
        ajax: {
            url: (string) url a la que se quiere hacer la peticiÃ³n,
            param_name: (string) nombre del parÃ¡metro con el que se va a pasar el objeto File
        }
    }

    Se puede configurar tambiÃ©n cual serÃ¡n los selectores para cada elemento del droparea
    options: {
        inputSelector: ".image-uploader__input",
        dropAreaSelector: ".image-uploader__drop-area",
        preview: ".image-uploader__preview",
        previewImg: ".image-uploader__img",
        errorDisplay: ".image-uploader__error",
    }

    Configurar lÃ­mite de tamaÃ±o en Kb (por defecto sin lÃ­mite)
    options: {
        sizeLimit: 100
    }

    El html por defecto deberÃ­a ser asÃ­:
        <div class="image-uploader js-image-uploader">
            <div class="image-uploader__preview">
                <!-- Si hay una imagen en el servidor pintarla en el src, si no dejarlo vacÃ­o  -->
                <img class="image-uploader__img" src="">
            </div>
            <div class="image-uploader__drop-area">
                <div class="image-uploader__icon">
                    <span class="material-icons">backup</span>
                </div>
                <div class="image-uploader__info">
                    <p><strong>Arrastra y suelta en la zona punteada una foto para tu perfil</strong></p>
                    <p>ImÃ¡genes en formato .PNG o .JPG</p>
                    <p>Peso mÃ¡ximo de las imÃ¡genes 250 kb</p>
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
         * para aÃ±adirla al input file
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
                    displayError('El archivo no es una imÃ¡gen vÃ¡lida. Los formatos vÃ¡lidos son .png y .jpg.');
                    return;
                }

                if (!imageSizeAllowed()) {
                    displayError('El archivo pesa demasiado. El lÃ­mite es ' + plugin.settings.sizeLimit + 'Kb');
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
         * Muestra la imagen que se ha aÃ±adido al input file
         */
        var showImageTemporalPreview = function () {

            const [file] = plugin.input.get(0).files
            if (file) {
                showImagePreview(URL.createObjectURL(file))
            }
        };

        /**
         * Incia lÃ³gica para llamada ajax
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
                console.log('La opciÃ³n "ajax.param_name" no estÃ¡ configurada')
                return false;
            }
            if (plugin.settings.ajax.hasProperty('url')) {
                console.log('La opciÃ³n de "ajax.url" no estÃ¡ configurada')
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
         * true: MostrarÃ¡ el "loading"
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