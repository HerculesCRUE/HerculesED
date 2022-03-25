/*
    Theme Name: GNOSS Front - MyGnoss base theme
    Theme URI: http://dewenir.es

    Author: GNOSS Front
    Author URI: http://dewenir.es

    Description: Fichero base de customización del tema de MyGNOSS.
    Version: 1.0
*/

var bodyScrolling = {
    lastScrollTop: 0,
    init: function () {
        this.config();
        this.scroll();
        return;
    },
    config: function () {
        this.body = body;
        return;
    },
    scroll: function () {
        var that = this;
        $(window).scroll(function () {
            that.lanzar();
        });
        return;
    },
    lanzar: function () {
        const currentScrollTop = $(window).scrollTop();

        // mark if scroll is up or down
        if (currentScrollTop > this.lastScrollTop) {
            this.body.addClass("scrolling-down");
            this.body.removeClass("scrolling-up");
            $(window).trigger("scrolling-down");
        } else {
            this.body.addClass("scrolling-up");
            this.body.removeClass("scrolling-down");
            $(window).trigger("scrolling-up");
        }

        // remove scrolling-up if scroll is on top
        if (currentScrollTop <= 30) {
            this.body.removeClass("scrolling-up");
        }

        // mark if scrolling
        if (currentScrollTop <= 1) {
            this.body.removeClass("scrolling");
        } else {
            this.body.addClass("scrolling");
        }
        this.lastScrollTop = $(window).scrollTop();
        return;
    },
};

var operativaFullWidth = {
    init: function () {
        this.config();
        this.lanzar();
        this.escalado();

        return;
    },
    config: function () {
        this.body = body;
        this.fullwidthrow = this.body.find('.fullwidthrow');
        this.main = this.body.find('main[role="main"] > .container');
        return;
    },
    lanzar: function () {
        var windows_width = $(window).width();
        var container_width = this.main.width();
        var anchoScrollbar = (this.body.width() > 767) ? 31.5 : 0;
        var margen = (windows_width - container_width + anchoScrollbar) / 2;

        var margenNegativo = parseFloat('-' + margen);

        this.fullwidthrow.each(function () {
            var item = $(this);
            item.css({
                "transform": "translateX(" + margenNegativo + "px)",
                "width": "100vw"
            });
        });

        return;
    },
    escalado: function () {
        var that = this;

        $(window).resize(function () {
            that.lanzar();
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
        if (!$('#menuLateralMetabuscador').length > 0) return;

        $('#menuLateralMetabuscador').slideReveal({
            trigger: $("#menuLateralMetabuscadorTrigger"),
            width: 820,
            overlay: true,
            position: 'right',
            push: false,
            show: function (slider, trigger) {
                var width = 820
                var windowWidth = $(window).width();
                var width = windowWidth < width ? windowWidth : width;
                slider.css('width', width + 'px');
            },
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
    },
    onResize: function () {
        var that = this;
        var width = $(window).width();
        $(window).on('resize', function () {
            if ($(this).width() !== width) {
                width = $(this).width();
                that.montarMenuLateralMetabuscador()
            }
        });
    }

};

const botonDesplegarMenuClonado = {
    init: function () {
        this.config();
        this.configEvents();
        this.setupInitialState();
        return;
    },
    config: function () {
        this.body = body;
        this.main = this.body.find('main');
        // Id del botón desplegar
        this.idBtnDesplegarMenuClonado = 'btn-desplegar-menu-clonado';
        // Menú clonado
        this.menuClonado = $('#menuLateralUsuarioClonado');
        // Iconos del botón
        this.iconoDesplegarMenuClonado = 'chevron_right';
        this.iconoPlegarMenuClonado = 'chevron_left';
        // Tamaños que se le establecerá al menú cuando se contraiga
        this.tamayoContraido = 52;
        // Posición actual en la que se moverá el botón (+ -)
        this.posicionOriginalBoton = 308;
        this.posicionBotonContraido = 40;
        // Configurar por defecto que el panel esté desplegado
        this.isDesplegado = true;
        // Configurar si el localStorage está disponible para guardar el estado del menú. False por defecto
        this.isLocalStorageAvailable = false;
        // Key del valor del estado del panel clonado
        this.keyIsDesplegado = 'isDesplegado';
        
        this.tamayoDesplegado = $('#menuLateralUsuarioClonado').width();
        // Paneles o información que ocultaremos una vez esté contraido el menú
        this.menusOcultar = ["espacio", "comunidades", "gestionar", "identidades", "desconectar"];

        this.areaMenuClonado = $('<div />').addClass('area-grab-menu-clonado');
        this.button = $('<button />').addClass('navigation-area-grab');
        //this.botonDesplegarMenuClonadoHTML = `<i id='${this.idBtnDesplegarMenuClonado}' class="material-icons material-icons--rounded">${this.iconoDesplegarMenuClonado}</i>`;

        this.botonDesplegarMenuClonadoHTML = `
        <i id='${this.idBtnDesplegarMenuClonado}' class="material-icons material-icons--rounded">
            ${this.iconoDesplegarMenuClonado}
        </i>
    `;
        this.menuClonado.after(this.areaMenuClonado);
        this.areaMenuClonado.append(this.button);
        this.areaMenuClonado.append(this.botonDesplegarMenuClonadoHTML);
                      
        // Registro el botón menú clonado
        this.botonDesplegarMenu = $(`#${this.idBtnDesplegarMenuClonado}`);

        return;
    },

        configEvents: function () {

        const that = this;
        /* Configurar si se desea desplegar o no el menú */
        this.botonDesplegarMenu.click(function (event) {
            that.desplegarMenuClonado(true);
        });

        /* Configurar el onFocus para que aparezca el botón de desplegar el menú */
        this.menuClonado.hover(function (ev) {
            // Mostrar el botón
            that.botonDesplegarMenu.css("display", "flex");

            if (that.isDesplegado == false) {
                $(this).stop().animate({
                    width: ev.type == "mouseenter" ? that.tamayoDesplegado : that.tamayoContraido
                }, 500, function () {
                    that.menusOcultar.forEach(menu => {
                        that.menuClonado.find('.menuUsuario').find(`.${menu}`).fadeIn(100);
                        // that.menuClonado.find('.menuUsuario').find(`.${menu}`).css('display', "block");
                    });
                });

                // that.botonDesplegarMenu.stop().animate({
                //     left: that.posicionOriginalBoton
                // }, 500);
            }
        }, function (ev) {
            // Ocultar el botón
            that.botonDesplegarMenu.css("display", "none");

            if (that.isDesplegado == false) {
                $(this).stop().animate({
                    width: ev.type == "mouseleave" ? that.tamayoContraido : that.tamayoDesplegado
                }, 500);

                // that.botonDesplegarMenu.stop().animate({
                //     left: that.posicionBotonContraido
                // }, 500);

                that.menusOcultar.forEach(menu => {
                    that.menuClonado.find('.menuUsuario').find(`.${menu}`).css('display', "none");
                });
            }
        });

        // Mostar y ocultar boton antes de arrastrar el cursor al panel
        this.areaMenuClonado.hover(function () {
            that.botonDesplegarMenu.css("display", "flex");
        }, function () {
            that.botonDesplegarMenu.css("display", "none");
        });

        return;
    },

    /**
     * Configurar el estado inicial de paneles y botones
     * */
    setupInitialState: function () {
        // Controlar si se puede usar el localStorage
        if (this.storageAvailable('localStorage')) {
            // El storage está disponible   
            this.isLocalStorageAvailable = true;
            if (localStorage.getItem(this.keyIsDesplegado)) {
                this.isDesplegado = JSON.parse(localStorage.getItem(this.keyIsDesplegado));                
            }
        }
                
        // Establecer el menuClonado como valor inicial
        this.desplegarMenuClonado();

        // Por defecto el botón siempre será invisible
        this.botonDesplegarMenu.css("display", "none");       

        return;
    },

    /**
     * Ocultar/ Mostrar el menú clonado para tener más espacio en pantalla principal           
     * 
     */

    /**  
     * @param {any} hasClickedButton: Indicar si se desplegará el menú debido al click hecho en el botón. Por defecto no (valor inicial)
     */
    desplegarMenuClonado: function (hasClickedButton = false) {
        if (hasClickedButton) {
            if (this.isDesplegado) {
                this.menuClonado.css("width", this.tamayoContraido);            
            } else {
                this.menuClonado.css("width", this.tamayoDesplegado);
            }
        } else {
            if (this.isDesplegado) {
                this.menuClonado.css("width", this.tamayoDesplegado);
            } else {
                this.menuClonado.css("width", this.tamayoContraido);
            }
        }

        // Cambiar el estado si se ha hecho click en el botón (deseo del usuario)
        if (hasClickedButton == true) {
            this.isDesplegado = !this.isDesplegado;
        }
               
        // Ocultar/Mostrar info dependiendo del estado del menú lateral
        this.mostrarOcultarOpcionesMenuClonado();
        // Establecer el icono del botón
        this.setBotonMenuClonado();
        // Establecer la opción en localStorage
        if (this.isLocalStorageAvailable) {
            localStorage.setItem('isDesplegado', this.isDesplegado);
        }
    },

    /**
     * Mostrar u ocultar opciones del menú desplegado según el estado actual del mismo
     * @param {any} isDesplegado
     */
     mostrarOcultarOpcionesMenuClonado: function () {

        const that = this;

        if (this.isDesplegado) {
            // Mostrar las opciones ocultadas
            this.menuClonado.find('.menuUsuario').find('.navegacion').find("a:first").fadeIn(200);
            this.menuClonado.find('.menuUsuario').css('margin-top', "0px");
            this.menusOcultar.forEach(menu => {
                that.menuClonado.find('.menuUsuario').find(`.${menu}`).fadeIn(200);
            });
        } else {
            // Ocultar las opciones no deseadas cuando el menú no esté desplegado
            this.menuClonado.find('.menuUsuario').find('.navegacion').find("a:first").css('display', "none");
            this.menuClonado.find('.menuUsuario').css('margin-top', "10px");
            this.menusOcultar.forEach(menu => {
                that.menuClonado.find('.menuUsuario').find(`.${menu}`).css('display', "none");
            });
        }
    },

    /**
     * Establecer el icono y la posición del botón de contraer o desplegar menú
     * */
    setBotonMenuClonado: function () {       
        if (this.isDesplegado) {
            // Establezco el icono
            this.iconoPlegarMenu = this.iconoPlegarMenuClonado;            
            // Establezco la posición            
            // this.botonDesplegarMenu.css({ left: this.posicionOriginalBoton });
        } else {
            // Establezco el icono
            this.iconoPlegarMenu = this.iconoDesplegarMenuClonado;
            // Establezco la posición
            // this.botonDesplegarMenu.css({ left: this.posicionBotonContraido });
        }

        // Asignación de imagen
        this.botonDesplegarMenu.html(this.iconoPlegarMenu);
    },

    /**
     * Comprobar si está disponible el storage del navegador
     * @param {any} type
     */
    storageAvailable: function (type) {
        try {
            var storage = window[type],
                x = '__storage_test__';
            storage.setItem(x, x);
            storage.removeItem(x);
            return true;
        }
        catch (e) {
            return e instanceof DOMException && (
                // everything except Firefox
                e.code === 22 ||
                // Firefox
                e.code === 1014 ||
                // test name field too, because code might not be present
                // everything except Firefox
                e.name === 'QuotaExceededError' ||
                // Firefox
                e.name === 'NS_ERROR_DOM_QUOTA_REACHED') &&
                // acknowledge QuotaExceededError only if there's something already stored
                storage.length !== 0;
        }
    },

}

var clonarMenuUsuario = {
    init: function () {
        this.config();
        this.copiar();
        return;
    },
    config: function () {
        this.body = body;
        this.main = body.find('main[role="main"]');
        this.menuOriginal = this.body.find('#menuLateralUsuario');
        this.menuClonado = this.clonar();
        return;
    },
    clonar: function () {
        var menuClonadoAux = this.menuOriginal.clone();
        menuClonadoAux.attr("id", "menuLateralUsuarioClonado");
        menuClonadoAux.attr("class", "menuLateral usuario clonado");
        return menuClonadoAux;
    },
    copiar: function () {
        if (this.menuClonado.length > 0) {
            this.main.prepend(this.menuClonado);
            // Añadir botón para desplegar/ocultar botón para plegado/desplegado menú lateral
            botonDesplegarMenuClonado.init();
        }
    }
};

var sacarPrimerasLetrasNombre = {
    init: function (numLetras, nombre) {
        var resul = this.sacar(numLetras, nombre);
        return resul;
    },
    sacar: function (numLetras, nombre) {
        var resul = "";
        if (nombre == undefined) return;
        var partes = nombre.split(' ');
        $.each(partes, function (c, v) {
            if (c > numLetras - 1) return false;
            var primera = v.substring(0, 1);
            resul = resul + primera;
        });

        return this.sustituirAcentos(resul);
    },
    sustituirAcentos: function (text) {
        var acentos = "ÃÀÁÄÂÈÉËÊÌÍÏÎÒÓÖÔÙÚÜÛãàáäâèéëêìíïîòóöôùúüûÑñÇç";
        var original = "AAAAAEEEEIIIIOOOOUUUUaaaaaeeeeiiiioooouuuunncc";
        for (var i = 0; i < acentos.length; i++) {
            text = text.replace(acentos.charAt(i), original.charAt(i));
        }
        return text;
    }
};

var obtenerClaseBackgroundColor = {
    init: function (nombre) {
        var resul = this.obtener(nombre);
        return resul;
    },
    obtener: function (nombre) {
        //var number = Math.floor(Math.random() * maximo) + 1;
        if (nombre == undefined) return;
        var letra = sacarPrimerasLetrasNombre.init(1, this.sustituirAcentos(nombre)).toLowerCase();
        return 'color-' + letra;
    },
    sustituirAcentos: function (text) {
        if (text == null) return;
        var acentos = "ÃÀÁÄÂÈÉËÊÌÍÏÎÒÓÖÔÙÚÜÛãàáäâèéëêìíïîòóöôùúüûÑñÇç";
        var original = "AAAAAEEEEIIIIOOOOUUUUaaaaaeeeeiiiioooouuuunncc";
        for (var i = 0; i < acentos.length; i++) {
            text = text.replace(acentos.charAt(i), original.charAt(i));
        }
        return text;
    }
};

var circulosPersona = {
    init: function () {
        this.config();
        this.circulos();
        return;
    },
    config: function () {
        this.body = body;
        this.headerResource = this.body.find('.header-resource');
        return;
    },
    circulos: function () {

        var h1Container = this.headerResource.find('.h1-container');

        var titulo = h1Container.find('h1');

        if (titulo.text() != undefined) {
            var iniciales = sacarPrimerasLetrasNombre.init(2, titulo.text());
            var clase = obtenerClaseBackgroundColor.init(titulo.text());
            var spanCirculo = $('<span />').addClass('circuloPersona ' + clase).text(iniciales);

            h1Container.append(spanCirculo);
        }

        return;
    }
};

var filtrarMovil = {
    init: function () {
        this.config();
        this.comportamiento();
        return;
    },
    config: function () {
        this.body = body;
        this.filtrarMovil = this.body.find('.btn-filtrar-movil');
        this.colFacetas = this.body.find('.col-facetas');
        return;
    },
    comportamiento: function () {
        var that = this;

        this.filtrarMovil.off('click').on('click', function (e) {
            if (that.body.hasClass('facetas-abiertas')) {
                that.cerrarPanelLateralFacetas();
            } else {
                that.abrirPanelLateralFacetas();
                that.body.addClass('facetas-abiertas');
            }
        });

        this.colFacetas.find('.cerrar').off('click').on('click', function (e) {
            that.cerrarPanelLateralFacetas();
        });

        return;
    },
    abrirPanelLateralFacetas: function () {
        this.body.addClass('facetas-abiertas');
    },
    cerrarPanelLateralFacetas: function () {
        this.body.removeClass('facetas-abiertas');

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
        const that = this;
        this.header = this.body.find('#header');
        this.metabuscadorTrigger = this.header.find('a[data-target="menuLateralMetabuscador"]');
        this.metabuscador = this.body.find('#menuLateralMetabuscador');
        this.input = this.metabuscador.find('#txtBusquedaPrincipal');
        this.resultadosMetabuscador = this.metabuscador.find('#resultadosMetabuscador');
        this.resultadosMetabuscadorAppend = this.metabuscador.find('#listaMetabuscadorResultados');
        this.verMasEcosistema = this.body.find('#verMasEcosistema');
        this.buscadorTrigger = $(document).find("#txtBusquedaPrincipal");
        this.buscador = this.body.find('#btnBuscarPrincipal');
        this.metabuscadorBusquedaHome = this.body.find('.row-content');
        this.inputBuscador = this.metabuscadorBusquedaHome.find('#txtBusquedaPrincipal');
        this.resultadosMetabuscadorHome = this.metabuscadorBusquedaHome.find('#resultadosMetabuscador');
        // Fila donde se pintarán cada uno de los resultados encontrados
        this.resultadosMetabuscadorAppendHome = $(".metabuscador-panel-resultados .row"); //this.metabuscadorBusquedaHome.find('#listaMetabuscadorResultados');
        // Loading /Spinner del metabuscador de la home
        this.metabuscadorLoading = $("#metabuscador-loading");
        // Panel de resultados para el metabuscador de la home
        this.metabuscadorPanelResultados = $("#metabuscador-panel-resultados");
        // Objetivo donde se asignará el observer
        let target = $(".metabuscador-panel-resultados .row")[0];
        // Teclas que se ignorarán si se pulsan en el input para que no dispare la búsqueda (Flechas, Espacio, Windows, Ctrol, Alt, Bloq. Mayus, Inicio, Alt, Escape)
        this.ignoreKeysToBuscador = [37, 38, 39, 40, 32, 91, 17, 18, 20, 36, 18, 27];
        // Botón para cerrar panel de resultados de la home
        this.btnCloseMetabuscador = this.body.find(".metabuscador-panel-close");        
        // Nº de resultados que se mostrarán
        this.numResultadoPintar = 3;
        // Clases para columnas (responsive design)
        this.metabuscadorPanelResultadosBloqueCssMinResultados = "col";
        this.metabuscadorPanelResultadosBloqueCssMaxResultados = "col-lg-4 col-sm-6 col-12";
        // Panel sin resultados por elementos no encontrados
        this.panelSinResultados = $(`#sinResultadosMetabuscador`);
        // Palabra clave introducida en el metaBuscador para mostrar en el panel de resultados no encontrados
        this.idPalabraBuscadaMetabuscador = `metabuscadorBusqueda`;
        this.timeWaitingForUserToType = 1000; // Esperar 1 segundos a si el usuario ha dejado de escribir para iniciar búsqueda
        // Items de sugerencias de búsquedas para Metabuscador
        this.sugerenciasMetabuscadorItems = $(`#sugerenciasMetabuscadorItems`);
        // Nº de búsquedas del metabuscador que se guardarán a modo de histórico
        this.numMaxSearchs = 6;
        // Key de búsquedas del localStorage
        this.KEY_LOCAL_SEARCHS = "localSearchs";

        // Creación del observador para resultados del metabuscador
        let observer = new MutationObserver(function (mutations) {
            mutations.forEach(function (mutation) {
                const newNodes = mutation.addedNodes; // DOM NodeList
                const removedNodes = mutation.removedNodes; // DOM NodeList

                // Resultados encontrdos
                if (newNodes.length > 0) {
                    // Ocultar loading del metabuscador home y mostrar el panel de resultados
                    that.metabuscadorLoading.addClass("d-none") 
                    that.metabuscadorPanelResultados.removeClass("d-none");

                    // Buscar el nº de bloques añadidos para establecer el tamaño de columnas
                    const metabuscadorPanelResultadosAdded = $(that.resultadosMetabuscadorAppendHome).find(".metabuscador-panel-resultados-bloque");
                    metabuscadorPanelResultadosAdded.each(function () {
                        const panelResultadoAdded = $(this);
                        if (metabuscadorPanelResultadosAdded.length > 3) {
                            panelResultadoAdded.removeClass(that.metabuscadorPanelResultadosBloqueCssMinResultados);
                            panelResultadoAdded.addClass(that.metabuscadorPanelResultadosBloqueCssMaxResultados);
                        } else {
                            panelResultadoAdded.addClass(that.metabuscadorPanelResultadosBloqueCssMinResultados);
                            panelResultadoAdded.removeClass(that.metabuscadorPanelResultadosBloqueCssMaxResultados);
                        }
                    })                   
                }

                // Los resultados son eliminados (nueva posible búsqueda)
                if (removedNodes.length > 0) {
                    // Ocultar el panel de resultados y loading del metabuscador home 
                    that.metabuscadorLoading.addClass("d-none");
                    that.metabuscadorPanelResultados.addClass("d-none");
                }
            });
        });

        // Configuración del observador:
        const config = {
            childList: true,
            subtree: true,
        };

        // Inicializar el mostrado de búsquedas de metaBuscador
        this.drawSearchsFromLocalStorage();

        // Activación del observador mostrado de resultados en Home con su configuración
        if (target != undefined) { observer.observe(target, config); }
        return;
    },
    comportamiento: function () {
        var that = this;

        that.metabuscadorTrigger.on('click', function (e) {
            that.input.focus();
        });

        // Escribir para buscar en el Metabuscador del panel de la derecha
        that.input.on("keydown", function (e) {
            if ($(this).val().length == 0 || e.keyCode == 27) { that.ocultarResultados }

            if (that.validarKeyPulsada(e) == true) {
                clearTimeout(that.timer);
                that.timer = setTimeout(function () {
                    var val = that.input.val();
                    if (val.length > 2) {
                        that.ocultarResultados();
                        // Ocultar panel sin resultados por posible busqueda anterior sin resultados
                        that.mostrarPanelSinResultados(false);
                        that.cargarResultados(val, false);
                        // Guardar búsqueda en localStorage
                        that.saveSearchInLocalStorage(val);
                    } else {
                        that.ocultarResultados();
                    }
                }, that.timeWaitingForUserToType);
            }            
        });
        /* Desactivado Buscador Home    
        that.buscadorTrigger.on("keydown", function (e) {
            // No disparar la búsqueda si se ha pulsado en teclas definidas o en Escape      
            if ($(this).val().length == 0 || e.keyCode == 27) { that.mostrarOculto(); }

            if (that.validarKeyPulsada(e) == true) {
                clearTimeout(that.timer);

                that.timer = setTimeout(function () {
                    var val = that.inputBuscador.val();
                    if (val.length > 2) {
                        that.mostrarOculto();
                        that.cargarResultados(val, true);
                    } else {
                        that.mostrarOculto();
                    }
                }, that.timeWaitingForUserToType);
            }            
        });
        */

        // Botón para cerrar resultados de la Home
        this.btnCloseMetabuscador.on("click", function () {
            that.mostrarOculto();
        });

        // Deshabilitar posible "ENTER" del formulario de búsqueda e iniciar búsqueda
        this.metabuscadorPanelResultados.parents("form").keydown(function (e) {
            if (e.keyCode == 13) {
                e.preventDefault();
                that.buscadorTrigger.keyup();
                return false;
            }
        });

        // Click en cada item de histórico de metabuscador              
        //this.metabuscador.on('click', this.sugerenciasMetabuscadorItems.children(), function (event) {
        this.sugerenciasMetabuscadorItems.on('click', function (event) {
            // Establecer como búsqueda a realizar
            const search = event.target.text;
            that.input.val(search);
            that.input.trigger("keydown");
        });


        // Click para eliminar histórico de metabúsquedas al cerrar sesión
        $('.desconectar-usuario').on("click", function () {
            that.removeSearchsFromLocalStorage();
        });

        return;
    },

    /**
     * Comprobará la tecla pulsada, y si no se encuentra entre las excluidas, dará lo introducido por válido devolviendo true
     * Si se pulsa una tecla de las excluidas, devolverá false y por lo tanto el metabuscador no debería iniciarse
     * @param {any} event: Evento o tecla pulsada en el teclado
     */
    validarKeyPulsada: function (event) {
        const keyPressed = this.ignoreKeysToBuscador.find(key => key == event.keyCode);
        if (keyPressed) {
            return false
        }
        return true;
    },

    mostrarOculto: function () {
        //$(".col-contenido").show();
        //$(".col-contexto").show();
        // Borrar items de la búsqueda
        this.resultadosMetabuscadorAppendHome.children().remove();
        //this.metabuscadorBusquedaHome.find('.cargadores').children().remove();
        //this.metabuscadorBusquedaHome.removeClass('mostrarResultados');
    },
    ocultarResultados: function () {
        this.resultadosMetabuscadorAppend.children().remove()
        this.metabuscador.find('.cargadores').children().remove();
        this.metabuscador.removeClass('mostrarResultados');
    },
    cargarResultados: function (val, buscadorHome) {
        var that = this;
        if (buscadorHome) {
            /*$(".col-contenido").hide();
            $(".col-contexto").hide();*/
            // Mostrar loading de metabuscador home
            this.metabuscadorLoading.removeClass("d-none");
            this.metabuscadorBusquedaHome.addClass('mostrarResultados');
        }
        else {
            this.metabuscador.addClass('mostrarResultados');
        }
        //Realizar la peticion a Facetas para devolver 
        var metodo = 'CargarFacetas';
        var tokenAfinidad = guidGenerator();
        var params = {};
        params['pProyectoID'] = $('input.inpt_proyID').val();
        //params['pEstaEnProyecto'] = $('input.inpt_bool_estaEnProyecto').val() == 'True';
        params['pEsUsuarioInvitado'] = $('input.inpt_bool_esUsuarioInvitado').val() == 'True';
        params['pIdentidadID'] = $('input.inpt_identidadID').val();
        //Descomentarlo mas adelante
        params['pEstaEnProyecto'] = true;
        params['pParametros'] = 'search=' + val;
        params['pLanguageCode'] = $('input.inpt_Idioma').val();
        params['pPrimeraCarga'] = 'false';
        params['pAdministradorVeTodasPersonas'] = 'false';
        params['pTipoBusqueda'] = 6;
        params['pGrafo'] = $('input.inpt_proyID').val();
        params['pFiltroContexto'] = '';
        params['pUbicacionBusqueda'] = 'Meta';
        params['pNumeroFacetas'] = '1';
        params['pParametros_adiccionales'] = '';
        params['pUsarMasterParaLectura'] = "true";
        params['pFaceta'] = 'rdf:type';
        params['pJson'] = 'true';

        var datos = '';
        $.post(obtenerUrl($('input.inpt_UrlServicioFacetas').val()) + "/" + metodo, params, function (response) {
            var data = response;
            var valorActual = that.input.val();
            if (buscadorHome) {
                valorActual = that.inputBuscador.val();
            }
            if (val == valorActual) {
                if (data["FacetList"][0] != undefined && data["FacetList"][0]["FacetItemList"] != undefined) {
                    //tipos del servicio de facetas.
                    var types = data["FacetList"][0]["FacetItemList"];
                    //Ontologias excluidas
                    var ontoExcluidas = JSON.parse($('input.inpt_Ontologias_Exclu_Model').val());
                    //Pestañas
                    var jsonPestanyas = JSON.parse($('input.inpt_Pestañas_Model').val());
                    //Devolver la lista con las pestañas a las quie hacer las peticiones
                    var busqueda = that.ComprobarExclusionYPestania(types, ontoExcluidas, jsonPestanyas);
                    var valorActual = that.input.val();
                    if (buscadorHome) {
                        valorActual = that.inputBuscador.val();
                    }
                    if (val == valorActual) {
                        //meter los cargando
                        for (var i = 0; i < busqueda.length; i++) {
                            var NombreFaceta = busqueda[i].pestanya.Nombre;
                            if (NombreFaceta == 'Debates') {
                                that.cargarDebatesBIS(i + 1, buscadorHome);
                            }
                            else if (NombreFaceta == 'Preguntas') {
                                that.cargarPersonasBIS(i + 1, buscadorHome);
                            }
                            else if (NombreFaceta == 'PersonasYOrganizaciones') {
                                that.cargarPreguntasBIS(i + 1, buscadorHome);

                            } else if (NombreFaceta == 'Recursos') {
                                that.cargarRecursosBIS(i + 1), buscadorHome;
                            }
                            else {
                                that.cargarRecursosBIS(i + 1, buscadorHome);
                            }
                        }
                        if (busqueda.length > 0 && val == valorActual) {
                            that.CargarResultadosMetabuscador(busqueda, val, buscadorHome);
                        }
                    }
                    else {
                        if (buscadorHome) {
                            if (that.resultadosMetabuscadorAppendHome.children()[0] != undefined) {
                                that.resultadosMetabuscadorAppendHome.children().remove()
                            }

                            if (that.metabuscadorBusquedaHome.find('.cargadores').children()[0] != undefined) {
                                that.metabuscadorBusquedaHome.find('.cargadores').children().remove();
                            }
                        }
                        else {
                            if (that.resultadosMetabuscadorAppend.children()[0] != undefined) {
                                that.resultadosMetabuscadorAppend.children().remove()
                            }
                            if (that.metabuscador.find('.cargadores').children()[0] != undefined) {
                                that.metabuscador('.cargadores').children().remove();
                            }
                        }
                    }
                }
                // Busqueda realizada: Sin resultados encontrados
                else {
                    if (buscadorHome) {
                        that.mostrarOculto();
                    } else {
                        that.ocultarResultados();
                        // Mostrar panel de resultados no encontrados
                        that.mostrarPanelSinResultados(true);
                    }
                }
            } else {
                if (that.resultadosMetabuscadorAppend.children()[0] != undefined) {
                    that.resultadosMetabuscadorAppend.children().remove()
                }
                if ($('.cargadores').children()[0] != undefined) {
                    $('.cargadores').children().remove();
                }
            }
        }, "json");
        that.ocultarBuscando;
        that.cargarRecursos();
        that.cargarDebates();
        that.cargarPreguntas();
        that.cargarPersonas();
        //that.linkVerEnElEcosistema();
    },
    cargarBIS: function (tipo_recurso, tiempo) {
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
    cargarRecursosBIS: function (time, buscadorHome) {
        var that = this;
        var html = "<div class='progress-loader' id='loader-recursos-wrap'><p class='progress-loader-label'>Buscando en recursos</p></div>";
        if (buscadorHome) {
            that.metabuscadorBusquedaHome.find('.cargadores').append(html);
        } else {
            that.metabuscador.find('.cargadores').append(html);
        }
        var bloque = that.resultadosMetabuscador.find('.bloque.recursos');
        if (buscadorHome) {
            bloque = that.resultadosMetabuscadorHome.find('.bloque.recursos');
        }
        bloque.hide();

        var procesoCarga = this.cargarBIS('recursos', time);
        procesoCarga.then(function () {
            bloque.show();
        });
    },
    cargarDebatesBIS: function (time, buscadorHome) {
        var that = this;
        var html = "<div class='progress-loader' id='loader-debates-wrap'><p class='progress-loader-label'>Buscando en debates</p></div>";
        if (buscadorHome) {
            that.metabuscadorBusquedaHome.find('.cargadores').append(html);
        } else {
            that.metabuscador.find('.cargadores').append(html);
        }
        var bloque = that.resultadosMetabuscador.find('.bloque.debates');
        if (buscadorHome) {
            bloque = that.resultadosMetabuscadorHome.find('.bloque.debates');
        }
        bloque.hide();

        var procesoCarga = this.cargarBIS('debates', time);
        procesoCarga.then(function () {
            bloque.show();
        });
    },
    cargarPreguntasBIS: function (time, buscadorHome) {
        var that = this;
        var html = "<div class='progress-loader' id='loader-preguntas-wrap'><p class='progress-loader-label'>Buscando en preguntas</p></div>";
        if (buscadorHome) {
            that.metabuscadorBusquedaHome.find('.cargadores').append(html);
        } else {
            that.metabuscador.find('.cargadores').append(html);
        }
        var bloque = that.resultadosMetabuscador.find('.bloque.personas');
        if (buscadorHome) {
            bloque = that.resultadosMetabuscadorHome.find('.bloque.personas');
        }
        bloque.hide();

        var procesoCarga = this.cargarBIS('preguntas', time);
        procesoCarga.then(function () {
            bloque.show();
        });
    },
    cargarPersonasBIS: function (time, buscadorHome) {
        var that = this;
        var html = "<div class='progress-loader' id='loader-personas-wrap'><p class='progress-loader-label'>Buscando en personas</p></div>";
        if (buscadorHome) {
            that.metabuscadorBusquedaHome.find('.cargadores').append(html);
        } else {
            that.metabuscador.find('.cargadores').append(html);
        }
        var bloque = that.resultadosMetabuscador.find('.bloque.personas');
        if (buscadorHome) {
            bloque = that.resultadosMetabuscadorHome.find('.bloque.personas');
        }
        bloque.hide();

        var procesoCarga = this.cargarBIS('personas', time);
        procesoCarga.then(function () {
            bloque.show();
        });
    },
    CargarResultadosMetabuscador: function (busqueda, val, buscadorHome) {
        var that = this;
        var valorActual = that.input.val();
        if (buscadorHome) {
            valorActual = that.inputBuscador.val();
        }
        var pestanyaBusqueda = busqueda[0];
        var textoParametrosAdicionales = 'PestanyaActualID=' + pestanyaBusqueda.pestanya.PestanyaID + '|';
        if (pestanyaBusqueda.pestanya.CampoFiltro != null) {
            textoParametrosAdicionales += pestanyaBusqueda.pestanya.CampoFiltro;
        }
        var tipoBusqueda = pestanyaBusqueda.pestanya.TipoBusqueda;

        var paramsResul = {};
        var metodo = 'CargarResultados';
        paramsResul['pProyectoID'] = $('input.inpt_proyID').val();
        paramsResul['pEstaEnProyecto'] = $('input.inpt_bool_estaEnProyecto').val() == 'True';
        paramsResul['pEsUsuarioInvitado'] = $('input.inpt_bool_esUsuarioInvitado').val() == 'True';
        paramsResul['pIdentidadID'] = $('input.inpt_identidadID').val();
        paramsResul['pParametros'] = 'search=' + val;
        paramsResul['pLanguageCode'] = $('input.inpt_Idioma').val();
        paramsResul['pPrimeraCarga'] = 'false';
        paramsResul['pAdministradorVeTodasPersonas'] = 'false';
        paramsResul['pTipoBusqueda'] = tipoBusqueda;
        paramsResul['pGrafo'] = $('input.inpt_proyID').val();
        paramsResul['pFiltroContexto'] = '';
        paramsResul['pUbicacionBusqueda'] = 'Meta';
        paramsResul['pNumeroParteResultados'] = '1';
        paramsResul['pParametros_adiccionales'] = textoParametrosAdicionales;
        paramsResul['pUsarMasterParaLectura'] = "true";
        paramsResul['pJson'] = 'true';
        //peticion al servicio de Resultados
        if (val == valorActual) {
            $.post(obtenerUrl($('input.inpt_UrlServicioResultados').val()) + "/" + metodo, paramsResul, function (respuestaResultaodos) {
                var datos = [];
                var devolver = [];
                var nombreBusqueda = busqueda[0].pestanya.Nombre;
                var urlSearch;
                //Crear ojeto para mostrar los resultados.
                var valorActual = that.input.val();
                if (buscadorHome) {
                    valorActual = that.inputBuscador.val();
                }
                if (respuestaResultaodos.ListaResultados != undefined && respuestaResultaodos.ListaResultados.$values != undefined && respuestaResultaodos.ListaResultados.$values.length > 0) {
                    var recursos = respuestaResultaodos.ListaResultados.$values;
                    for (var i = 0; i < recursos.length; i++) {
                        if (nombreBusqueda == 'PersonasYOrganizaciones') {
                            var nombre = recursos[i].NamePerson;
                            var organizacion = recursos[i].NameOrganization;
                            var url = recursos[i].Url;
                            if (nombre != undefined && nombre != '') {
                                if (organizacion != undefined) {
                                    nombre += '·' + organizacion;
                                }
                            }
                            else {
                                nombre = organizacion;
                            }
                            datos.push(
                                {
                                    nombre: nombre,
                                    url: url
                                });
                            if (urlSearch == undefined) {
                                urlSearch = $('input.inpt_baseUrlBusqueda').val() + '/personas-y-organizaciones';
                            }
                        } else {
                            var nombreRecurso = recursos[i].Title;
                            var urlRecurso = recursos[i].CompletCardLink;
                            datos.push(
                                {
                                    nombre: nombreRecurso,
                                    url: urlRecurso
                                });
                            if (urlSearch == undefined) {
                                urlSearch = recursos[i].UrlSearch;
                            }
                        }
                        
                    }
                    //Creo la lista para los resultados
                    devolver.push(
                        {
                            nombreBusqueda: nombreBusqueda,
                            urlSearch: urlSearch,
                            recursos: datos
                        })
                    // Hay resultados que pintar: Ocultar Panel sin resultados
                    that.mostrarPanelSinResultados(false);
                    that.PintarResultados(devolver[0], that.numResultadoPintar, val, buscadorHome, busqueda[0]);
                }else{
                    // No se han encontrado resultados de una pestaña en concreto. Dar tiempo a posibles nuevas búsquedas pero mostrar el panel
                    setTimeout(() => {
                        if (that.resultadosMetabuscadorAppend.children().length == 0){
                            that.mostrarPanelSinResultados(true);
                        }else{
                            that.mostrarPanelSinResultados(false);
                        }                                                                     
                    },800)                                
                }
                //Tratar los resultados.
                busqueda.shift();
                if (busqueda.length > 0 && val == valorActual) {
                    that.CargarResultadosMetabuscador(busqueda, val, buscadorHome);
                }
                else {
                    that.ocultarBuscando();
                }
            }, "json");
        }
    },

    /**
     * Mostrar u ocultar el panel que muestre error debido a que no se ha encontrado ningún resultado
     * @param {Bool} mostrarPanel
     */
     mostrarPanelSinResultados: function(mostrarPanel){

        // Vaciar posibles palabras anteriores
        $(`#${this.idPalabraBuscadaMetabuscador}`).text('');
        // Establecer la palabra buscada para ser mostrada en el panel
        const cadenaBuscada = this.input.val();
        // Panel de búsqueda no encontrada
        const panelSinResultadosContent = `
            <div class="container">
                <div class="row">
                    <div class="col-md-12">
                        <div class="errorPlantilla">
                            <h3 style="font-size:18px">Sin resultados</h3>
                            <div class="detallesError"><p class="text-muted">Lo sentimos pero no se ha encontrado ningún resultado con <span id="metabuscadorBusqueda" class="font-weight-bold"></span></p></div>
                        </div>
                    </div>
                </div>
            </div>
        `;
        
        if (mostrarPanel == true) {
            this.panelSinResultados.removeClass("d-none");
            this.panelSinResultados.html(panelSinResultadosContent);
            // Añadir la palabra buscada al panel de no encontrado
            $(`#${this.idPalabraBuscadaMetabuscador}`).text(cadenaBuscada);
        } else {
            this.panelSinResultados.addClass("d-none");            
        }    
    },

    PintarResultados: function (resultados, numeroResultados, val, home, busqueda) {
        var that = this;
        var tipo;
        var html;
        var esBusqueda = false;
        if (resultados.nombreBusqueda == 'Debates') {
            tipo = 'debate';
            resultados.nombreBusqueda = metaBuscador.Debates;
            that.cargarDebates();
        }
        else if (resultados.nombreBusqueda == 'Preguntas') {
            tipo = 'pregunta';
            resultados.nombreBusqueda = metaBuscador.Preguntas;
            that.cargarPersonas();
        }
        else if (resultados.nombreBusqueda == 'PersonasYOrganizaciones') {
            that.cargarPreguntas();
            tipo = 'persona';
            resultados.nombreBusqueda = metaBuscador.personasYorganizaciones;
        } else if (resultados.nombreBusqueda == 'Recursos') {
            tipo = 'recurso';
            resultados.nombreBusqueda = metaBuscador.Recursos;
            that.cargarRecursos();
        }
        else if (resultados.nombreBusqueda == 'BusquedaAvanzada') {
            tipo = 'recurso';
            resultados.nombreBusqueda = metaBuscador.BusquedaAvanzada;
            esBusqueda = true;
            that.cargarRecursos();
        }
        else {
            that.cargarRecursos();
            tipo = 'recurso';
        }
        
        /* Pintar el encabezado del resultado donde corresponda (MetaBuscador o Metabuscador panel derecho) */
        if (!home) {
            html = "<li class='bloque " + tipo + "s'><p class='title'>" + resultados.nombreBusqueda + "</p><ul>"
        } else {
            /* Encabezado del bloque de resultados */
            html = `<div role="presentation" class="col metabuscador-panel-resultados-bloque">
                        <div class="metabuscador-panel-resultados-bloque-contenido">
                            <h2>${resultados.nombreBusqueda}</h2>
                                <ul class="metabuscador-list-items">
            `;            
        }

        /* Pintar el bloque de resultados donde corresponda (MetaBuscador o Metabuscador panel derecho) */
        for (var i = 0; i < resultados.recursos.length; i++) {
            if (i == numeroResultados) {
                break;
            }
            var recurso = resultados.recursos[i];
            if (!home) {
                // Búsqueda realizada desde Panel lateral
                html += "<li class='con-icono-before icono-" + tipo + "'><a href='" + recurso.url + "'>" + recurso.nombre + "</a></li>";
            } else {
                // Búsqueda realizada desde Home
                html += `<li class="metabuscador-item ${tipo}"><a href="${recurso.url}">${recurso.nombre}</a></li>`;
            }
            
        }

        if (resultados.recursos.length > numeroResultados) {
            if (esBusqueda) {
                val += '&' + busqueda.pestanya.CampoFiltro.replace('|','&');
            }
            html += "<li class='con-icono-after ver-mas-icono ver-mas'><a href='" + resultados.urlSearch + "?search=" + val +"'>Ver más</a></li>";
        }

        if (!home) {
            html += "</ul></li>";
        } else {
            html += "</ul></div></div>";
        }

        if (home) {
            // Limpiar posibles resultados anteriores
            //that.mostrarOculto();
            that.resultadosMetabuscadorAppendHome.append(html);
        } else {
            that.resultadosMetabuscadorAppend.append(html);
        }

    },
    ComprobarExclusionYPestania: function (types, ontoExcluidas, jsonPestanyas) {
        var devolver = [];
        for (var i = 0; i < types.length; i++) {
            var typeNombre = types[i].Tittle;
            var rdfType = types[i].Name;
            //Recorro las ontologias para ver cuial esta excluida
            if (!ontoExcluidas.includes(typeNombre)) {
                //Recorro las pestañas para obtener a cuales tengo que hacer las peticiones
                for (var x = 0; x < jsonPestanyas.length; x++) {
                    var pestIteracion = jsonPestanyas[x];
                    //Hago agrupacion para las pestañas, para que se sumen si tienen la misma pestaña configurada para los rdf:type
                    if (pestIteracion.CampoFiltro != null && pestIteracion.CampoFiltro.includes(rdfType)) {
                        devolver = this.ProcesarPestanya(devolver, pestIteracion, types[i].Number);
                        break;
                    }
                    else if (pestIteracion.Nombre.includes(typeNombre)) {
                        devolver = this.ProcesarPestanya(devolver, pestIteracion, types[i].Number);
                        break;
                    }
                    else if (pestIteracion.Nombre == 'BusquedaAvanzada') {
                        devolver = this.ProcesarPestanyaBusquedaAvanzada(devolver, pestIteracion, types[i].Number, rdfType);
                        break;
                    }
                }
            }
        }
        //Procesar la lista para devolver la primera la que mas resultados tenga.
        devolver = devolver.sort(function (a, b) {
            return b.resultados - a.resultados;
        });

        return devolver;

    },
    ProcesarPestanya: function (devolver, pestIteracion, valor) {
        if (devolver.length > 0 && this.ContienePestanya(devolver, pestIteracion)) {
            devolver = this.ModificarValor(devolver, pestIteracion, valor)
        }
        else {
            var pestanyaDevolver = {
                pestanya: pestIteracion,
                resultados: valor
            }
            devolver.push(pestanyaDevolver);
        }
        return devolver;

    },
    ProcesarPestanyaBusquedaAvanzada: function (devolver, pestIteracion, valor, valorRdf) {
        if (pestIteracion.CampoFiltro != null) {
            pestIteracion.CampoFiltro += valorRdf + '|';
        } else {
            pestIteracion.CampoFiltro = valorRdf + '|';
        }
        if (devolver.length > 0 && this.ContienePestanya(devolver, pestIteracion)) {
            devolver = this.ModificarValor(devolver, pestIteracion, valor)
        }
        else {
            var pestanyaDevolver = {
                pestanya: pestIteracion,
                resultados: valor
            }
            devolver.push(pestanyaDevolver);
        }
        return devolver;

    },
    ContienePestanya: function (lista, pestanya) {
        for (var i = 0; i < lista.length; i++) {
            if (lista[i].pestanya.Nombre == pestanya.Nombre) {
                return true;
            }
        }
        return false;
    },
    ModificarValor: function (lista, pestanya, valorsumar) {
        for (var i = 0; i < lista.length; i++) {
            if (lista[i].pestanya.Nombre == pestanya.Nombre) {
                lista[i].resultados += valorsumar;
            }
        }
        return lista;
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
    ocultarBuscando: function () {
        $('.progress-loader').hide();
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
    },


    /**
     * Guardar en localStorage una búsqueda realizada. Eliminará el más antiguo si se ha llegado al número de items máximo guardado.
     * @param {string} search
     */
    saveSearchInLocalStorage: function (search) {
        // Comprobar si la búsqueda reciente ya existe en el histórico de búsquedas
        let searchRepeated = false;

        try {
            // Obtener las búsquedas actuales que hay en el localStorage. Si no hay devolverá array vacío
            const localSearchs = JSON.parse(
                localStorage.getItem(this.KEY_LOCAL_SEARCHS) || "[]"
            );

            // Comprobar que no existe la búsqueda actual en localStorage
            localSearchs.forEach((item) => {
                if (item.search.indexOf(search) != -1) {
                    searchRepeated = true;
                }
            });

            if (searchRepeated) {
                return;
            }

            // Si hay un máximo de X resultados en localStorage, eliminar el último
            if (localSearchs.length >= this.numMaxSearchs) {
                localSearchs.shift();
            }

            // Crear el nuevo item "search" y guardarlo en localStorage
            const searchObject = { "search": search };
            localSearchs.push(searchObject);
            localStorage.setItem(this.KEY_LOCAL_SEARCHS, JSON.stringify(localSearchs));
        } catch (error) {
            console.log("Error saving metaSearch");
        }

        this.drawSearchsFromLocalStorage();
    },

    /**
    * Creará o dibujará las metabusquedas del localStorage en el HTML para que sean mostradas correctamente
    */
    drawSearchsFromLocalStorage: function () {
        // Listas de las búsquedas en localStorage
        let searchListItems = "";


        // Comprobar que el localStorache está disponible
        if (!botonDesplegarMenuClonado.storageAvailable("localStorage")){
            return
        }

        // Obtener las búsquedas actuales que hay en el localStorage. Si no hay devolverá array vacío
        const localSearchs = JSON.parse(localStorage.getItem(this.KEY_LOCAL_SEARCHS) || "[]");
        // Si no hay resultados ... no hacer nada
        if (localSearchs.length == 0) {
            return;
        }

        // Eliminar posibles items
        this.sugerenciasMetabuscadorItems.children().remove();

        // Construir cada itemList con las búsquedas almacenadas en localStorage
        localSearchs.reverse().forEach((item) => {
            searchListItems += `<li class="reciente con-icono-before icono-busqueda">
                                    <a href="javascript: void(0);">${item.search}</a>
                                </li>`;
        });

        // Añadir los items para el metabuscador
        this.sugerenciasMetabuscadorItems.append(searchListItems);
    },

    /**
     * Eliminar las búsquedas históricas al cerrar sesión
     * */
    removeSearchsFromLocalStorage: function () {

        if (botonDesplegarMenuClonado.storageAvailable('localStorage')) {
            return;
        }

        try {
            localStorage.removeItem(this.KEY_LOCAL_SEARCHS);
        } catch(e) {
            console.log("Error removing localSearchs from localStorage");
        }        
    }
};

var buscadorSeccion = {
    init: function () {
        this.config();
        this.comportamiento();
        return;
    },
    config: function () {
        this.body = body;
        return;
    },
    comportamiento: function () {

        var headerListado = this.body.find('#headerListado');
        var input = headerListado.find('#finderSection');

        input.focusin(function () {
            headerListado.addClass('sugerencias');
        }).focusout(function () {
            headerListado.removeClass('sugerencias buscando');
        });

        input.on('keydown', function (e) {
            setTimeout(function () {
                var val = input.val();
                if (val.length > 0) {
                    headerListado.removeClass('sugerencias').addClass('buscando');
                } else {
                    headerListado.removeClass('buscando').addClass('sugerencias');
                }
            }, 100);
        });

        return;
    }
};

var modificarCabeceraOnScrolling = {
    init: function () {
        this.config();
        this.observarBody();
        this.observerScroll();
    },
    config: function () {
        this.body = body;
        this.buscador = body.find(".col-buscador");
        this.colContenido = body.find(".col-contenido");
        this.accionesListado = body.find(".header-listado .acciones-listado");
        this.filtros = body.find("#panFiltros").length > 0 ? body.find("#panFiltros") : body.find("#divFiltros") ;
        this.resultados = body.find(
            ".header-listado .h1-container .numResultados"
        );
        this.fakeResultados = body.find(
            ".col-buscador .btn-filtrar-movil .resultados"
        );
    },
    observarBody: function () {
        const that = this;
        const target = this.body.get(0);

        var observer = new MutationObserver(function (mutations) {
            mutations.forEach(function (mutation) {
                if (mutation.attributeName !== "class") return;

                if (that.isScrolling()) {
                    that.ajustarElementos();
                } else {
                    that.volverElementosAlEstadoInicial();
                }
            });
        });

        var config = {
            attributes: true,
        };

        observer.observe(target, config);
    },
    isScrolling: function () {
        return this.body.hasClass("scrolling");
    },
    ajustarElementos: function () {
        this.ajustarBuscador();
        this.ajustarNombreComunidad();
    },
    ajustarBuscador: function () {
        const width = this.colContenido.innerWidth();

        if ($(window).width() > 1199) {
            this.buscador.css("width", width - 25 + "px");
        } else {
            this.buscador.css("width", "auto");
        }
    },
    volverBuscadoralEstadoInicial: function () {
        this.buscador.css("width", "100%");
    },
    ajustarNombreComunidad: function () {
        $('.community-menu-wrapper .page-name-wrapper').css("width", this.buscador.offset().left - 100 + "px");
    },
    volverNombreComunidadEstadoInicial: function () {
        $('.community-menu-wrapper .page-name-wrapper').css("width", "auto");
    },
    volverElementosAlEstadoInicial: function () {
        this.volverBuscadoralEstadoInicial();
        this.volverNombreComunidadEstadoInicial();
    },
    observerScroll: function () {
        $(window).on("scrolling-down", this.onScrollDown.bind(this));
        $(window).on("scrolling-up", this.onScrollUp.bind(this));
    },
    onScrollDown: function () {
        this.copiarResultados();
        this.aplicarSombra();
    },
    onScrollUp: function () {
        this.aplicarSombra()
    },
    copiarResultados: function () {
        if (this.resultados.length === 0 || this.fakeResultados.length === 0)
            return;

        this.fakeResultados.text(this.resultados.text());
    },
    aplicarSombra: function () {
        // depende de si hay filtros o no hay que aplicar la sombra a
        // los filtros o a las acciones para que no se superponga una sombra con otra
        if(this.filtros.length === 0 || this.filtros.is(':hidden')){
            this.filtros.removeClass('add-shadow')
            this.accionesListado.addClass('add-shadow');
        }else{
            this.filtros.addClass('add-shadow');
            this.accionesListado.removeClass('add-shadow');
        }
    },
};

var listadoMensajesAcciones = {
    init: function () {
        this.config();
        this.comportamientoCheck();
        this.comportamientosDropdown();
        this.comportamientoRecargar();
        return;
    },
    config: function () {
        this.body = body;
        this.checkAllMesages = this.body.find('#checkAllMesages');
        this.checkActions = this.body.find('#checkActions');
        this.reloadButton = this.body.find('#reloadMensajes');
        return;
    },
    comportamientoRecargar: function () {
        var that = this;

        this.reloadButton.off('click').on('click', function (e) {
            // SIMULACION DE PETICIÓN DE PRUEBAS
            var resourceList = that.body.find('.col-contenido .resource-list');
            resourceList.hide();
            MostrarUpdateProgress()
            setTimeout(function () {
                resourceList.show();
                OcultarUpdateProgress()
            }, 1000);

        });

        return;
    },
    comportamientoCheck: function () {
        var resourceList = this.body.find('.col-contenido .resource-list');
        var resources = resourceList.find('.resource');
        var resourcesCheck = resources.find('.check-wrapper input[type="checkbox"]');

        this.checkAllMesages.off('change').on('change', function (e) {
            if ($(this).is(':checked')) {
                resourcesCheck.prop('checked', true)
            } else {
                resourcesCheck.prop('checked', false)
            }
        });

        return;
    },
    comportamientosDropdown: function () {
        var that = this;
        this.checkActions.on('click', '.item-dropdown', function () {
            var action = $(this);
            var resourceList = that.body.find('.col-contenido .resource-list');
            var resources = resourceList.find('.resource');

            if (action.hasClass('checkall')) {
                that.seleccionarTodos();
            }
            if (action.hasClass('decheckall')) {
                that.deSeleccionarTodos();
            }
            if (action.hasClass('checkAllRead')) {
                that.seleccionarTodosLeidos(resources);
            }
            if (action.hasClass('checkAllNonRead')) {
                that.seleccionarTodosNoLeidos(resources);
            }
        });

        return;
    },
    seleccionarTodos: function () {
        this.checkAllMesages.prop('checked', true);
        this.checkAllMesages.trigger('change');
    },
    deSeleccionarTodos: function () {
        this.checkAllMesages.prop('checked', false);
        this.checkAllMesages.trigger('change');
    },
    seleccionarTodosLeidos: function (resources) {
        this.deSeleccionarTodos()
        var checks = resources.filter('.no-leido').find('.check-wrapper input[type="checkbox"]');;
        checks.prop('checked', true).trigger('change');
    },
    seleccionarTodosNoLeidos: function (resources) {
        this.deSeleccionarTodos()
        var checks = resources.not('.no-leido').find('.check-wrapper input[type="checkbox"]');
        checks.prop('checked', true).trigger('change');
    }
};

var listadoRecursosAcciones = {
    init: function () {
        this.config();
        this.comportamientoCheck();
        this.comportamientosDropdown();
        this.comportamientoRecargar();
        return;
    },
    config: function () {
        this.body = body;
        this.checkAllResources = this.body.find('#checkAllResources');
        this.checkActions = this.body.find('#checkActions');
        this.reloadButton = this.body.find('#reloadResources');
    },
    comportamientoRecargar: function () {
        var that = this;

        this.reloadButton.off('click').on('click', function (e) {
            // SIMULACION DE PETICIÓN DE PRUEBAS
            var resourceList = that.body.find('.col-contenido .resource-list');
            resourceList.hide();
            MostrarUpdateProgress()
            setTimeout(function () {
                resourceList.show();
                OcultarUpdateProgress()
            }, 1000);

        });
    },
    comportamientoCheck: function () {
        var resourceList = this.body.find('.col-contenido .resource-list');
        var resources = resourceList.find('.resource');
        var resourcesCheck = resources.find('.check-wrapper input[type="checkbox"]');

        this.checkAllResources.off('change').on('change', function (e) {
            if ($(this).is(':checked')) {
                resourcesCheck.prop('checked', true)
            } else {
                resourcesCheck.prop('checked', false)
            }
        });
    },
    comportamientosDropdown: function () {
        var that = this;
        this.checkActions.on('click', '.item-dropdown', function () {
            var action = $(this);
            var resourceList = that.body.find('.col-contenido .resource-list');

            if (action.hasClass('checkall')) {
                that.seleccionarTodos();
            }
            if (action.hasClass('decheckall')) {
                that.deSeleccionarTodos();
            }
        });
    },
    seleccionarTodos: function () {
        this.checkAllResources.prop('checked', true);
        this.checkAllResources.trigger('change');
    },
    deSeleccionarTodos: function () {
        this.checkAllResources.prop('checked', false);
        this.checkAllResources.trigger('change');
    },
};

var accionDropdownSelect = {
    init: function () {
        if ($('.dropdown-select').length > 0) this.comportamiento();
    },
    comportamiento: function () {
        var select = $('.dropdown-select');
        var menu = select.find('.dropdown-menu');
        var item = menu.find('.item-dropdown');

        item.off('click').on('click', function () {
            var parent = $(this).parents('.dropdown-select');
            var toggle = parent.find('.dropdown-toggle');
            var items = parent.find('.item-dropdown');
            toggle.html($(this).html());
            toggle.addClass('active');
            items.removeClass('activeView');
            $(this).addClass('activeView');
        });
    }
};

var modalCategorizarRecursos = {
    init: function () {
        this.config();
        this.comportamientoModal();
        this.comportamientoChecks();
        this.comportamientoBotones();
    },
    config: function () {
        this.modal = body.find('#modal-container');
        this.categoriesSection = this.modal.find('#seccion-categorias');
        this.buttonCheckAll = this.modal.find('#checkAllCategories');
        this.checkActions = this.modal.find('#checkActions');
        this.nuevaCategoriaButton = this.modal.find('#nueva-categoria');
        this.renombrarCategoriaButton = this.modal.find('#renombrar-categoria');
        this.moverCategoriaButton = this.modal.find('#mover-categoria');
        this.ordenarCategoriaButton = this.modal.find('#ordenar-categoria');
        this.eliminarCategoriaButton = this.modal.find('#eliminar-categoria');
        this.botonesVolver = this.modal.find('.volver');
    },
    comportamientoModal: function () {
        // volver al selector de categorías cuando se cierre el modal
        const that = this;

        /* Div creado dinámicamente
        that.modal.on('hidden.bs.modal', function (e) {
            that.modal.attr('data-mostrar', 'categorias');            
        });*/

        this.modal.attr('data-mostrar', 'categorias');        
    },
    comportamientoChecks: function () {
        const that = this;

        // Comportamiento del checkbox general
        this.buttonCheckAll.on('change', function () {
            if ($(this).is(':checked')) {
                that.checkAll();
            } else {
                that.unCheckAll();
            }
            that.actualizarEstadoBotones();
        });

        this.checkActions.on('click', '.checkall', function () {
            that.buttonCheckAll.prop('checked', true);
            that.buttonCheckAll.trigger('change');
            that.actualizarEstadoBotones();
        });

        // Accion deseleccionar todo del dropdown del checkbox general
        this.checkActions.on('click', '.decheckall', function () {
            that.buttonCheckAll.prop('checked', false);
            that.buttonCheckAll.trigger('change');
            that.actualizarEstadoBotones();
        });

        this.observarCambiosEnChecks();

    },
    observarCambiosEnChecks: function () {
        // observa cambios en el estado de todos los checkbox
        const that = this;
        const checks = this.getAllCategoriesChecks();
        checks.off('change').on('change', function () {
            that.actualizarEstadoBotones();
        });
    },
    unCheckAll: function () {
        // desmarca todos los checkobx de la pestaña activa
        const checks = this.getActiveTabAllCategoriesChecks();
        checks.prop('checked', false);
    },
    checkAll: function () {
        // marca todos los checkobx de la pestaña activa
        const checks = this.getActiveTabAllCategoriesChecks();
        checks.prop('checked', true);
    },
    getAllCategoriesChecks: function () {
        /**
         * @return  {jQuery} todas los checkbox de las categorias de ambas pestñas
         */
        const activeTab = this.categoriesSection.find('.tab-pane');
        return activeTab.find('input[type="checkbox"]');
    },
    getActiveTabAllCategoriesChecks: function () {
        /**
         * @return {jQuery} todas las categorías de la pestaña activa
         */
        const activeTab = this.categoriesSection.find('.tab-pane.active');
        return activeTab.find('input[type="checkbox"]');
    },
    getCheckedCategories: function () {
        /**
         * @return {jQuery} todas las categorías seleccionadas
         */
        return this.getActiveTabAllCategoriesChecks().filter(function () {
            return $(this).is(':checked');
        });
    },
    comportamientoBotones: function () {
        const that = this;

        // muestra los diferentes formularios
        this.nuevaCategoriaButton.on('click', function () {
            that.modal.attr('data-mostrar', 'nueva');
        });
        this.renombrarCategoriaButton.on('click', function () {
            that.modal.attr('data-mostrar', 'renombrar');
        });
        this.moverCategoriaButton.on('click', function () {
            that.modal.attr('data-mostrar', 'mover');
        });
        this.ordenarCategoriaButton.on('click', function () {
            that.modal.attr('data-mostrar', 'ordenar');
        });
        this.eliminarCategoriaButton.on('click', function () {
            // Borrará directamente la categoría sin mostrar sección
            /*that.modal.attr('data-mostrar', 'eliminar');*/
        });

        // botón de cancelar/volver al selector de categorías
        this.botonesVolver.on('click', function () {
            that.modal.attr('data-mostrar', 'categorias');
        });
    },
    actualizarEstadoBotones: function () {
        // Actualiza el estado enabled/disabled de los botones según el número
        // de categorías seleccionadas
        const checked = this.getCheckedCategories();

        if (checked.length === 0) {
            this.moverCategoriaButton.addClass('disabled');
            this.ordenarCategoriaButton.addClass('disabled');
            this.eliminarCategoriaButton.addClass('disabled');
            this.renombrarCategoriaButton.addClass('disabled');
        } else {
            this.moverCategoriaButton.removeClass('disabled');
            this.ordenarCategoriaButton.removeClass('disabled');
            this.eliminarCategoriaButton.removeClass('disabled');

            this.renombrarCategoriaButton.addClass('disabled');
            if (checked.length === 1) {
                this.renombrarCategoriaButton.removeClass('disabled');
            }
        }
    }
};

// Añadir la clase .dropdown-autofocus (al padre del .dropdown-menu) para que cuando se despligue el dropdown
// se haga autofocus en el primer input que haya en el dropdown
var accionDropdownAutofocus = {
    init: function () {
        /*$('.dropdown-autofocus').on('shown.bs.dropdown', function () {            
            $dropdown = $(this);
            $dropdown.find('input').first().focus()            
        });
        */     
       // Asignación de comportamiento a las facetas que se cargan de forma asíncrona   
        $(document).on('.dropdown-autofocus').on('shown.bs.dropdown', function (e) {
            setTimeout(() => {
                $dropdown = $(e.target);
                // Realizar sólo el foco si hay un único input (evitar que se muestre calendarios)
                $dropdown.find('input').length == 1 && $dropdown.find('input').first().focus();

            }, 100);
        });
    },
};

var calcularFacetaDropdown = {
    init: function () {
        this.config();
        this.lanzar();
    },
    config: function () {
        this.facetas = body.find('#panFacetas');
        this.dropdown = this.facetas.find('.faceta-dropdown');
    },
    lanzar: function () {
        var that = this;
        $('.faceta-dropdown').on('show.bs.dropdown', function () {
            var menu = $(this).parent().find('.dropdown-menu');
            menu.css('visibility', 'hidden');
        });
        $('.faceta-dropdown').on('shown.bs.dropdown', function () {
            var menu = $(this).parent().find('.dropdown-menu');
            const width = menu.parents('.faceta-name').find('.faceta-title').innerWidth();

            setTimeout(function() {
                var matrix = menu.css('transform');
                var values = matrix.match(/-?[\d\.]+/g);
                var x = parseInt(values[4]);
                var y = parseInt(values[5]);
                menu.css({'transform' : 'translate3d(' + -(width + 3) + 'px,' + y + 'px, 0px)'});
                menu.css('visibility', 'visible');
            });
        });
    }
};



var cambioVistaListado = {
    init: function () {
        this.config();
        this.comportamiento();
        return;
    },
    config: function () {
        this.body = body;
        return;
    },
    comportamiento: function () {

        var accionesListado = this.body.find('.acciones-listado');
        var visualizacion = accionesListado.find('.visualizacion');
        var resourceList = this.body.find('.resource-list');
        var dropdownMenu = visualizacion.find('.dropdown-menu');
        var dropdownToggle = visualizacion.find('.dropdown-toggle');
        var dropdownToggleIcon = dropdownToggle.find('.material-icons');
        var modosVisualizacion = dropdownMenu.find('a');

        modosVisualizacion.on('click', function (e) {
            e.preventDefault();
            var item = $(this);
            var clase = item.data('class-resource-list');

            modosVisualizacion.removeClass('activeView');
            item.addClass('activeView');

            if (clase != "") {
                var icon = item.find('.material-icons').text();
                dropdownToggleIcon.text(icon);
                resourceList.removeClass('compacView listView mosaicView mapView graphView grafoView');
                resourceList.addClass(clase);
            }
        });

        return;
    }
};

var masHeaderMensaje = {
    init: function () {
        this.config();
        this.comportamiento();
        return;
    },
    config: function () {
        this.body = body;
        this.headerMensaje = this.body.find('.header-mensaje');
        return;
    },
    comportamiento: function () {

        this.headerMensaje.off('click', '.ver-mas').on('click', '.ver-mas', function (e) {
            e.preventDefault();
            var verMas = $(this);
            var padre = verMas.parent();
            var ul = padre.find('ul');

            ul.children().each(function (i) {
                var li = $(this);
                if (padre.hasClass('abierto') && i > 1) {
                    li.addClass('oculto');
                    padre.removeClass('abierto');
                    verMas.text('más');
                } else if (!padre.hasClass('abierto') && i > 1) {
                    li.removeClass('oculto');
                    padre.addClass('abierto');
                    verMas.text('menos');
                }
            });

        });

        return;
    }
};

var plegarFacetasCabecera = {
    init: function () {
        this.config();
        this.comportamiento();
        return;
    },
    config: function () {
        this.body = body;
        return;
    },
    comportamiento: function () {
        var that = this;
        this.facetas = this.body.find('#panFacetas');
        var facetasTitle = this.facetas.find('.faceta-title');

        facetasTitle.off('click').on('click', function (e) {
            var title = $(this);
            var target = $(e.target);
            var box = title.parents('.box');

            if (target.hasClass('search-icon')) {
                e.preventDefault();
                e.stopPropagation();                
            } else {
                that.mostrarOcultarFaceta(box);
            }

            alturizarBodyTamanoFacetas.init();
        });
    },
    mostrarOcultarFaceta: function (box) {
        box.toggleClass('plegado');
    },
    mostrarFaceta: function (box) {
        box.removeClass('plegado');
    },


};

var facetasVerMasVerTodos = {
    init: function () {
        this.config();
        this.comportamientoVerMas();
        this.comportamientoVerTodos();
        return;
    },
    config: function () {
        this.body = body;
        return;
    },
    comportamientoVerTodos: function () { },
    comportamientoVerMas: function () {

        $('.moreResults .ver-mas').off('click').on('click', function () {
            var facetaContainer = $(this).closest('.facetedSearch');
            facetaContainer.find('ul.listadoFacetas > .ocultar').show(200);
            facetaContainer.find('.ver-mas').css('display', 'none');
            facetaContainer.find('.ver-menos').css('display', 'flex');
            alturizarBodyTamanoFacetas.init();
        });

        $('.moreResults .ver-menos').off('click').on('click', function () {
            var facetaContainer = $(this).closest('.facetedSearch');
            facetaContainer.find('ul.listadoFacetas > .ocultar').hide(200);
            facetaContainer.find('.ver-menos').css('display', 'none');
            facetaContainer.find('.ver-mas').css('display', 'flex');
            alturizarBodyTamanoFacetas.init();
            return false;
        });
    }
};

var comportamientosModalFacetas = {
    init: function () {
        this.comportamientoBotonesDesplegables();
        //this.comportamientoBotonesAnteriorSiguiente();
        this.cambiarTituloModal();
    },
    comportamientoBotonesDesplegables: function () {
        $(document).on('click', '.js-desplegar-facetas-modal', function (event) {        
            const button = $(this);
            const faceta_wrap = button.closest('.facetas-wrap');
            const action_buttons = button.closest('ul').find('li');
            const facetas_con_subfaceta = faceta_wrap.find('.faceta.con-subfaceta.ocultarSubFaceta');
            const boton_desplegar_faceta = facetas_con_subfaceta.find('.desplegarSubFaceta span');
            boton_desplegar_faceta.trigger('click');
            action_buttons.show();
            button.hide();
        });
        $(document).on('click', '.js-plegar-facetas-modal', function (event) {        
            const button = $(this);
            const faceta_wrap = button.closest('.facetas-wrap');
            const action_buttons = button.closest('ul').find('li');
            const facetas_con_subfaceta = faceta_wrap.find('.faceta.con-subfaceta:not(.ocultarSubFaceta)');
            const boton_desplegar_faceta = facetas_con_subfaceta.find('.desplegarSubFaceta span');
            boton_desplegar_faceta.trigger('click');
            action_buttons.show();
            button.hide();
        });

        $('.js-plegar-facetas-modal').hide();
    },
        
        /*
         * Lógica de los botones creados en 'comportamientoFacetasPopup'
           Lo incluyo en unificado.js para comportamiento de facetas
         * 
        $(document).on('click', '.js-anterior-facetas-modal', function (event) {        
            $('.resultados-wrap .listadoFacetas').animate({
                marginLeft: 30,
                opacity: 0
            }, 200, function () {
                $('.resultados-wrap .listadoFacetas').css({ marginLeft: -30 });
                $('.resultados-wrap .listadoFacetas').animate({
                    marginLeft: 30,
                    opacity: 1
                }, 200);
            });
        });
        $(document).on('click', '.js-siguiente-facetas-modal', function (event) {        
            $('.resultados-wrap .listadoFacetas').animate({
                marginLeft: -30,
                opacity: 0
            }, 200, function () {
                $('.resultados-wrap .listadoFacetas').css({ marginLeft: 30 });
                $('.resultados-wrap .listadoFacetas').animate({
                    marginLeft: 30,
                    opacity: 1
                }, 200);
            });
        });
        */
    cambiarTituloModal: function () {
        $('#modal-resultados').on('show.bs.modal', function (e) {
            const modal = $(this);
            const modal_title = modal.find('.modal-title');
            const faceta_title = modal.find('.faceta-title');
            modal_title.text(faceta_title.text());
            faceta_title.hide();
        });
    },
}

/**
 * Comportamiento de carga de facetas cuando se pulse en "Ver más". Extraido del viejo front "comportamientoCargaFacetas"
 * */
const comportamientoCargaFacetasNewFront = {
    init: function () {
        // Longitud facetas por CSS
		// limiteLongitudFacetas.init();
        facetedSearch.init();
        $('.verMasFacetas').each(function () {
            var enlace = $(this);
            var params = enlace.attr('rel').split('|');
            var faceta = params[0];
            var controlID = params[1];
            enlace.unbind("click").click(function (evento) {
                evento.preventDefault();
                VerFaceta(faceta, controlID);
            });
        });
        return;
    }
};

var plegarSubFacetas = {
    init: function () {
        this.config();
        this.comportamiento();
        return;
    },
    config: function () {
        this.body = body;
        return;
    },
    comportamiento: function () {
        $('.desplegarSubFaceta .material-icons').unbind().click(function () {
            var padre = $(this).closest('a');
            var icono = $(this);
            var icono_texto = icono.text().trim();
            if (icono_texto == 'expand_more') {
                padre.removeClass('ocultarSubFaceta');
                icono.text('expand_less');
            } else {
                padre.addClass('ocultarSubFaceta');
                icono.text('expand_more');
            }
            alturizarBodyTamanoFacetas.init();
            return false;
        });
    }
};

var alturizarBodyTamanoFacetas = {
    init: function () {
        this.config();
        this.calcularAltoFacetas();
        return;
    },
    config: function () {
        this.body = body;
        this.panFacetas = this.body.find('#panFacetas');
        this.container = this.body.find('main[role="main"] > .container');
        return;
    },
    calcularAltoFacetas: function () {
        var altoFacetas = this.panFacetas.height();

        this.container.css({
            'min-height': altoFacetas
        });

        return;
    }
};

var mensajeAnyadirRegistro = {
    init: function () {
        this.config();
        this.comportamiento();
        return;
    },
    config: function () {
        this.body = body;
        this.contenido = this.body.find('.contenido');
        return;
    },
    comportamiento: function () {

        var that = this;
        var btn = this.contenido.find('.anyadir-registro');
        var divMensaje = $('<div></div>').addClass('mensaje-anyadir-registro');
        var tituloMensaje = $('<h3></h3>').text('Miembro añadido correctamente');
        var mensaje = $('<p></p>').text('Mensaje visible durante 10 segundos....');
        divMensaje.append(tituloMensaje);
        divMensaje.append(mensaje);

        btn.off('click').on('click', function () {
            that.contenido.append(divMensaje);
            setTimeout(function () {
                divMensaje.remove();
            }, 10000);
        });

        return;
    }
};

// calcular en la carga de la página cuanto padding-top tiene que tener el contenido
// según la altura de la cabecera fixed
var calcularPaddingTopMain = {
    init: function () {
        this.config();
        this.comportamiento();
        return;
    },
    config: function () {
        this.body = body;
        this.header = this.body.find('#header');
        this.main = this.body.find('main[role="main"]');
        return;
    },
    comportamiento: function () {
        var headerHeight = this.header.innerHeight();
        this.main.css('padding-top', headerHeight + 'px');
    }
};

var desplegarDestinatarios = {
    init: function () {
        this.config();
        this.comportamiento();
        return;
    },
    config: function () {
        this.body = body;
        this.destinatarios = this.body.find('.destinatarios-mensaje');
        this.lista = this.destinatarios.find('.lista-destinatarios');
        this.listaExtra = this.lista.filter('.extra');
        this.desplegarDestinatarios = this.body.find('.desplegar-destinatarios');
        return;
    },
    initialCheck: function () {
        if (this.listaExtra.length > 0) {
            this.comportamiento()
        }
    },
    comportamiento: function () {
        var that = this;
        this.desplegarDestinatarios.click(function () {
            that.destinatarios.toggleClass('show');
        });
    }
};

var addCustomScrollBar = {
    init: function () {
        this.config();
        this.comportamiento();
        return;
    },
    config: function () {
        this.body = body;
        this.header = this.body.find('.mensaje-principal');
        return;
    },
    comportamiento: function () {
    }
};

var accionesRecurso = {
    init: function () {
        accionHistorial.init();
    }
}

var accionHistorial = {
    init: function () {
        this.config();
    },
    config: function () {
        this.opciones = {
            paging: false,
            ordering: false,
            select: false,
            searching: false,
            info: false,
            responsive: true,
            autoWidth: false,
            columnDefs: [
                { responsivePriority: 1, targets: 0 },
                { responsivePriority: 1, targets: 1 },
                { responsivePriority: 1, targets: 2 },
                { responsivePriority: 2, targets: 4 },
                { responsivePriority: 1, targets: 5 },
                { responsivePriority: 3, targets: 3 }
            ]
        }
    },
    montarTabla: function () {
        this.modal = $('#modal-container');
        this.tabla = body.find('.tabla-versiones');
        this.hay_tabla = this.tabla.length > 0 ? true : false;

        if (!this.hay_tabla) return;
        this.tabla_montada = this.tabla.DataTable(this.opciones);
        this.comportamientosChecks();
        this.observarModal();

    },
    comportamientosChecks: function () {
        var checks = this.tabla.find("input[type='checkbox']");
        checks.on('change', function () {
            var check = $(this);
            var tr = check.closest('tr');
            if (check.is(':checked')) {
                tr.addClass('activo');
            } else {
                tr.removeClass('activo');
            }
        });
    },
    observarModal: function () {
        var that = this;

        var observer = new MutationObserver(function (mutations) {
            mutations.forEach(function (mutation) {
                if (mutation.attributeName === "class") {
                    that.tabla_montada.responsive.rebuild();
                    that.tabla_montada.responsive.recalc();
                }
            });
        });

        observer.observe(this.modal[0], {
            attributes: true
        });
    }
}

var accionDesplegarCategorias = {
    init: function () {
        this.config();
        this.mostrarOcultarCategoriasHijas();
    },
    config: function () {
        this.pan_categorias = $('.divTesArbol.divCategorias');
        this.desplegables = this.pan_categorias.find('.boton-desplegar')
    },
    mostrarOcultarCategoriasHijas: function () {
        if (this.desplegables.length > 0) {
            this.desplegables.off('click').on('click', function () {
                $(this).toggleClass('mostrar-hijos');
            });
        }
    },
}

var iniciarSelects2 = {
    init: function () {
        this.config();
        this.montarSelects();
    },
    config: function () {
        this.select2 = body.find('.js-select2');
        this.defaultOptions = {
            minimumResultsForSearch: 10
        };
    },
    montarSelects: function () {
        this.select2.select2(this.defaultOptions);
    }
}

var iniciarDropAreaImagenPerfil = {
    init: function () {
        this.config();
        this.montar();
    },
    config: function () {
        this.droparea = body.find('.js-image-uploader');
    },
    montar: function () {
        this.droparea.imageDropArea({});
    }
}

var customizarAvisoCookies = {
    init: function () {
        this.config();
        this.comportamientoAceptar();
        if (this.avisoCookies.length < 1) return;
        return;
    },
    config: function () {
        this.body = body;
        this.avisoCookies = this.body.find('#aviso-cookies');
        return;
    },
    comportamientoAceptar: function () {
		var that = this;
        var aceptar = $('#aceptarCookies');

		aceptar.on('click', function (e) {
			e.preventDefault();
			that.guardarAceptarCookies();
		});
        return;
    },

     /**
     * Guardado de cookie enviando petición al servidor (como se realiza en el anterior Front)
     * */
    guardarAceptarCookies: function () {
        var that = this;
        MostrarUpdateProgress();
        GnossPeticionAjax(
            document.location.origin + '/aceptar-cookies',
            null,
            true
        ).fail(function (data) {
            mostrarErrorGuardado(data);
        }).done(function () {
            // Ocultar panel de cookies
            that.avisoCookies.animate({ opacity: '0' }, 400, 'swing', function () {
                that.avisoCookies.remove();
            });            
        }).always(function () {
            OcultarUpdateProgress();             
        });       
        return;
    },
    /**   
     * Mostrar un error en la vista modal de la cookie en caso de que se haya producido un error en el guardado de la cookie
     * */
    mostrarErrorGuardado: function () {
        if (data) {
            $('#modal-accept-coookie-wrapper').after('<div class="ko" style="display:block;">' + data + '</div>');            
        } else {            
            $('#modal-accept-coookie-wrapper').after('<div class="ko" style="display:block;">Ha habido errores en el guardado</div>');
        }
    },
};

var customizarDonutChart = {
    init: function () {
        var donut = $('.donut-size');
        $.each(donut, function () {
            var id = '#' + $(this).attr('id');
            var percent = $(this).attr('data-percent');
            updateDonutChart(id, percent, true);
        });
    }
};


var modalCerrarSesion = {
    init: function () {
        //this.config();
        //this.comportamiento();
        // De momento no se realiza con modal
        this.startLoader();
    },
    config: function () {
        this.modal = body.find('#modal-cerrando-sesion')
    },
    comportamiento: function () {
        if (this.modal.length == 0) return;
        var that = this;

        this.modal.on('show.bs.modal', function (event) {
            that.callToCerrarSesión()
            that.startLoader()
        })
    },
    callToCerrarSesión: function () {
        /* TODO: llamada Ajax para cerrar la sesión y luego redirigir
        *  a la url que se quiera
        */
    },
    startLoader: function () {
        //loader_div = this.modal.find('#loader-cerrando-sesion');
        loader_div = $(document).find('#loader-cerrando-sesion');
        var loaderBar = loader_div.progressBarTimer({
            autostart: true,
            timeLimit: 4,
            warningThreshold: 0,
            baseStyle: '',
            warningStyle: '',
            completeStyle: '',
            smooth: true,
            striped: false,
            animated: false,
            height: 12,
        });
        return loaderBar;
    }
};

var customizarMenuMensajes = {
    init: function () {
        this.config();
        if ($(window).width() < 991) {
            this.comportamiento();
        }
    },
    config: function () {
        this.header = body.find('#header');
        this.col03 = this.header.find('.col.col03');
        this.menuAcciones = this.clonarAcciones();
    },
    clonarAcciones: function () {
        var acciones = $('.acciones-mensaje').clone();
        return acciones;
    },
    comportamiento: function () {
        this.col03.find('ul').hide();
        if (this.menuAcciones.length > 0) {
            this.col03.prepend(this.menuAcciones);
        }
    }
};

/**
 * [mostrarNotificacion]
 * @param  {string} tipo Puede ser 'info', 'success', 'warning', 'error'
 * @param  {string} contenido 'Mensaje que se quiere mostrar'
 */
var mostrarNotificacion = function (tipo, contenido) {
    toastr[tipo](contenido, '', {
        toastClass: 'toast themed',
        positionClass: "toast-bottom-center",
        target: 'body',
        closeHtml: '<span class="material-icons">close</span>',
        showMethod: 'slideDown',
        timeOut: 5000,
        escapeHtml: false,
        closeButton: true,
    });
};

function comportamientoCargaFacetasComunidad() {
    alturizarBodyTamanoFacetas.init();
    plegarFacetasCabecera.init();
    plegarSubFacetas.init();
    facetasVerMasVerTodos.init();
    // Funcionamiento de botones de navegación de Facetas en Modal (Siguente, Desplegar...)
    comportamientosModalFacetas.init();
};

var timeoutUpdateProgres;
function MostrarUpdateProgress() {
    // Quitar la opción de que a los 15 segundos desaparezca el "Loading"
    // MostrarUpdateProgressTime(15000)
    MostrarUpdateProgressTime();
}

function MostrarUpdateProgressTime(time) {
    if ($('#mascaraBlanca').length > 0) {
        $('body').addClass('mascaraBlancaActiva');

        if (time > 0) {
            timeoutUpdateProgress = setTimeout("OcultarUpdateProgress()", time);
        }
    }
}

function OcultarUpdateProgress() {
    if ($('#mascaraBlanca').length > 0) {
        clearTimeout(timeoutUpdateProgress);
        $('body').removeClass('mascaraBlancaActiva');
    }
}

; (function ($) {

    $.imageDropArea = function (element, options) {
        var defaults = {
          inputSelector: ".image-uploader__input",
          dropAreaSelector: ".image-uploader__drop-area",
          preview: ".image-uploader__preview",
          previewImg: ".image-uploader__img",
          errorDisplay: ".image-uploader__error",
          urlUploadImage: document.location.href,
          urlUploadImageType: 'ImagenRegistroUsuario', // Valor por defecto para la foto en el registro de usuario


          // funcionPrueba: function() {}
        };
        var plugin = this;

        // Objeto HTML del Loading que se mostraría mientras se esté realizando la carga de la imagen
        var loadingSpinnerHtml = "";
        loadingSpinnerHtml +=
          '<div class="spinner-border texto-primario" role="status" style="position: absolute; top: 45%; left:40%">';
        loadingSpinnerHtml += '<span class="sr-only">Cargando...</span>';
        loadingSpinnerHtml += "</div>";

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
          onInputChange();
          addDragAndDropEvents();
            // Añadir endPoint /save-image si la URL contiene editar-perfil
            if (plugin.settings.urlUploadImage.search('editar-perfil') != -1 || plugin.settings.urlUploadImage.search('editar-perfil-org') != -1) {
                plugin.settings.urlUploadImage += '/save-image';
                defaults.urlUploadImageType = "FicheroImagen";                
            } 
        };

        var onInputChange = function () {
          plugin.input.change(function () {
            var data = new FormData();
            var files = plugin.input.get(0).files;
            if (files.length > 0) {
              // Mostrar spinner de carga de imagen
              showLoadingImagePreview(true);
              data.append(defaults.urlUploadImageType, files[0]);
              $.ajax({
                url: plugin.settings.urlUploadImage,
                type: "POST",
                processData: false,
                contentType: false,
                data: data,
                success: function (response) {
                  hideError();
                  onSuccesResponse(response);
                },
                error: function (er) {
                    displayError(er.statusText);
                    showLoadingImagePreview(false);
                },
              });
            }            
          });
        };

        var displayError = function (error) {
          plugin.errorDisplay.find(".ko").text(error);
          plugin.errorDisplay.find(".ko").show();
          plugin.preview.removeClass("show-preview");
        };
        var hideError = function () {
          plugin.errorDisplay.find(".ko").hide();
        };

        var onSuccesResponse = function (response) {
          if (response.indexOf("imagenes/") === 0) {
            showImagePreview(response);
            showLoadingImagePreview(false);
            // Mostrar botón para poder eliminar imagen
            $('#btn_delete_profile_image').removeClass("d-none");
          } else {
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
            // Mostrar un spinner dentro del preview.
            plugin.preview.append(loadingSpinnerHtml);
          } else {
            // Quitar loading
            $(".spinner-border").remove();
          }
        };

        var showImagePreview = function (response) {
            var urlContent = $('input.inpt_baseURLContent').val();
            plugin.previewImg.attr(
            "src",
            urlContent + "/" + response
            );
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
        plugin.init();
      };

      // add the plugin to the jQuery.fn object
      $.fn.imageDropArea = function (options) {
        return this.each(function () {
          if (undefined == $(this).data("imageDropArea")) {
            var plugin = new $.imageDropArea(this, options);
            $(this).data("imageDropArea", plugin);
          }
        });
      };
})(jQuery);


$.fn.reverse = [].reverse;

var body;

$(function () {
    body = $('body');

    bodyScrolling.init();
    calcularPaddingTopMain.init();
    // No clonar el menú del usuario si el body contiene la clase no-clonarMenuUsuario
    if (!body.hasClass('no-clonarMenuUsuario')) {
        clonarMenuUsuario.init();
    }    
    menusLateralesManagement.init();
    metabuscador.init();
    iniciarSelects2.init();
    accionDesplegarCategorias.init();
    //circulosPersona.init();
    customizarAvisoCookies.init();
    accionDropdownSelect.init();
    accionDropdownAutofocus.init();
    // Operativa para búsquedas Facetas por Fecha
    operativaFechasFacetas.init();
    
    // Operativa de Facetas PopUp / y Popup Plegados    
    comportamientoFacetasPopUp.init();    
    comportamientoFacetasPopUpPlegado.init();
    customizarDonutChart.init();

    // Operativa de UsuariosOrganizacion (Administrar organización)
    if (body.hasClass('usuariosOrganizacion')) {
        operativaUsuariosOrganizacion.init();
    }
    
    if (body.hasClass('fichaRecurso') || body.hasClass('edicionRecurso')) {
        accionesRecurso.init();
        listadoRecursosAcciones.init();
    }

    if (body.hasClass('fichaMensaje')) {
        desplegarDestinatarios.init();
        customizarMenuMensajes.init();
    }

    if (body.hasClass('listado')) {
        filtrarMovil.init();
        buscadorSeccion.init();
        cambioVistaListado.init();
        listadoRecursosAcciones.init();        
        modificarCabeceraOnScrolling.init();
        calcularFacetaDropdown.init();
        comportamientoCargaFacetasComunidad();

        if (body.hasClass('mensajes')) {
            listadoMensajesAcciones.init();
        }
    }

    if (body.hasClass('registro') || body.hasClass('edicionPerfil')) {
        iniciarDropAreaImagenPerfil.init();
    }

    // Cargado de datos/resultados al hacer Scroll (Búsqueda)
    if (body.hasClass('showResultsScrolling')){
        scrollingListadoRecursos.init();
    }

    // Espacio personal del usuario
    if (body.hasClass('espacio-personal')) {        
        //modalCategorizarRecursos.init();
        operativaEspacioPersonalGnoss.init();
    }
});