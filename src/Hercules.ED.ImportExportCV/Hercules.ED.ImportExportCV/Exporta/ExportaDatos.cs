using Gnoss.ApiWrapper;
using ImportadorWebCV.Exporta.Secciones;
using ImportadorWebCV.Exporta.Secciones.ActividadCientificaSubclases;
using ImportadorWebCV.Exporta.Secciones.ActividadDocenteSubclases;
using ImportadorWebCV.Exporta.Secciones.DatosIdentificacion;
using ImportadorWebCV.Exporta.Secciones.ExperienciaCientificaSubclases;
using ImportadorWebCV.Exporta.Secciones.FormacionAcademicaSubclases;
using ImportadorWebCV.Exporta.Secciones.SituacionProfesionalSubclases;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ImportadorWebCV.Exporta
{
    public class ExportaDatos : SeccionBase
    {
        private readonly string cvID;
        private readonly cvnRootResultBean cvn;
        private readonly ResourceApi resourceApi;
        protected Dictionary<string, List<Dictionary<string, Data>>> MultilangProp;

        public ExportaDatos(cvnRootResultBean cvn, string cvID, string lang) : base(cvn, cvID)
        {
            this.cvID = cvID;
            this.cvn = cvn;
            resourceApi = mResourceApi;
            Utils.UtilitySecciones.IniciarLenguajes(mResourceApi);
            MultilangProp = GetMultilangProperties(cvID, lang);
        }

        public ResourceApi GetResourceApi()
        {
            return resourceApi;
        }

        public void ExportaDatosIdentificacion(Entity entity, string version, [Optional] List<string> listaId)
        {
            string seccion = "http://w3id.org/roh/personalData";

            IdentificacionCurriculum identificacionCurriculo = new (cvn, cvID);
            identificacionCurriculo.ExportaIdentificacionCurriculum(MultilangProp, version);

            DatosIdentificacion datosIdentificacion = new (cvn, cvID);
            datosIdentificacion.ExportaDatosIdentificacion(entity, seccion, listaId);
        }

        public void ExportaSituacionProfesional(Entity entity, [Optional] List<string> listaId)
        {
            SituacionProfesionalActual situacionProfesional = new (cvn, cvID);
            situacionProfesional.ExportaSituacionProfesional(MultilangProp, listaId);

            CargosActividades cargosActividades = new (cvn, cvID);
            cargosActividades.ExportaCargosActividades(MultilangProp, listaId);

        }

        public void ExportaFormacionAcademica(Entity entity, [Optional] List<string> listaId)
        {
            EstudiosCiclos estudiosCiclos = new (cvn, cvID);
            estudiosCiclos.ExportaEstudiosCiclos(MultilangProp, listaId);

            Doctorados doctorados = new (cvn, cvID);
            doctorados.ExportaDoctorados(MultilangProp, listaId);

            OtraFormacionPosgrado otraFormacionPosgrado = new (cvn, cvID);
            otraFormacionPosgrado.ExportaOtraFormacionPosgrado(entity, MultilangProp, listaId);

            FormacionEspecializada formacionEspecializada = new (cvn, cvID);
            formacionEspecializada.ExportaFormacionEspecializada(entity, MultilangProp, listaId);

            CursosMejoraDocente cursosMejora = new (cvn, cvID);
            cursosMejora.ExportaCursosMejoraDocente(MultilangProp, listaId);

            ConocimientoIdiomas conocimientoIdiomas = new (cvn, cvID);
            conocimientoIdiomas.ExportaConocimientoIdiomas(MultilangProp, listaId);
        }

        public void ExportaActividadDocente(Entity entity, [Optional] List<string> listaId)
        {
            DireccionTesis direccionTesis = new (cvn, cvID);
            direccionTesis.ExportaDireccionTesis(MultilangProp, listaId);

            FormacionAcademicaSubclase formacionAcademica = new (cvn, cvID);
            formacionAcademica.ExportaFormacionAcademica(MultilangProp, listaId);

            TutoriasAcademicas tutoriasAcademicas = new (cvn, cvID);
            tutoriasAcademicas.ExportaTutoriasAcademicas(MultilangProp, listaId);

            CursosSeminarios cursosSeminarios = new (cvn, cvID);
            cursosSeminarios.ExportaCursosSeminarios(MultilangProp, listaId);

            PublicacionesDocentes publicacionesDocentes = new (cvn, cvID);
            publicacionesDocentes.ExportaPublicacionesDocentes(MultilangProp, listaId);

            ParticipacionProyectosInnovacionDocente participacionProyectos = new (cvn, cvID);
            participacionProyectos.ExportaParticipacionProyectos(MultilangProp, listaId);

            ParticipacionCongresosFormacionDocente participacionCongresos = new (cvn, cvID);
            participacionCongresos.ExportaParticipacionCongresos(MultilangProp, listaId);

            PremiosInnovacionDocente premiosInnovacionDocente = new (cvn, cvID);
            premiosInnovacionDocente.ExportaPremiosInnovacionDocente(MultilangProp, listaId);

            OtrasActividades otrasActividades = new (cvn, cvID);
            otrasActividades.ExportaOtrasActividades(MultilangProp, listaId);

            AportacionesRelevantes aportacionesRelevantes = new (cvn, cvID);
            aportacionesRelevantes.ExportaAportacionesRelevantes(MultilangProp, listaId);
        }
        public void ExportaExperienciaCientificaTecnologica(Entity entity, [Optional] List<string> listaId)
        {
            string seccion = "http://w3id.org/roh/scientificExperience";

            ProyectosIDI proyectosIDI = new (cvn, cvID);
            proyectosIDI.ExportaProyectosIDI(seccion, MultilangProp, listaId);

            Contratos contratos = new (cvn, cvID);
            contratos.ExportaContratos(MultilangProp, listaId);

            PropiedadIndustrialIntelectual propII = new (cvn, cvID);
            propII.ExportaPropiedadII(MultilangProp, listaId);

            GrupoIDI grupoIDI = new (cvn, cvID);
            grupoIDI.ExportaGrupoIDI(MultilangProp, listaId);

            ObrasArtisticas obrasArtisticas = new (cvn, cvID);
            obrasArtisticas.ExportaObrasArtisticas(MultilangProp, listaId);

            ResultadosTecnologicos resultadosTecnologicos = new (cvn, cvID);
            resultadosTecnologicos.ExportaResultadosTecnologicos(MultilangProp, listaId);

        }
        public void ExportaActividadCientificaTecnologica(Entity entity, string versionExportacion, [Optional] List<string> listaId)
        {
            string seccion = "http://w3id.org/roh/scientificActivity";

            ProduccionCientifica produccionCientifica = new (cvn, cvID);
            produccionCientifica.ExportaProduccionCientifica(MultilangProp, versionExportacion, listaId);

            IndicadoresGenerales indicadoresGenerales = new (cvn, cvID);
            indicadoresGenerales.ExportaIndicadoresGenerales(entity, MultilangProp, listaId);

            PublicacionesDocumentos publicacionesDocumentos = new (cvn, cvID);
            publicacionesDocumentos.ExportaPublicacionesDocumentos(seccion, MultilangProp, listaId);

            TrabajosCongresos trabajosCongresos = new (cvn, cvID);
            trabajosCongresos.ExportaTrabajosCongresos(seccion, MultilangProp, versionExportacion, listaId);

            TrabajosJornadasSeminarios trabajosJornadasSeminarios = new (cvn, cvID);
            trabajosJornadasSeminarios.ExportaTrabajosJornadasSeminarios(MultilangProp, listaId);

            OtrasActividadesDivulgacion otrasActividadesDivulgacion = new (cvn, cvID);
            otrasActividadesDivulgacion.ExportaOtrasActividadesDivulgacion(MultilangProp, listaId);

            ComitesCta comitesCTA = new (cvn, cvID);
            comitesCTA.ExportaComitesCTA(MultilangProp, listaId);

            OrganizacionesIDI organizacionesIDI = new (cvn, cvID);
            organizacionesIDI.ExportaOrganizacionesIDI(MultilangProp, listaId);

            GestionIdi gestionIDI = new (cvn, cvID);
            gestionIDI.ExportaGestionIDI(MultilangProp, listaId);

            ForosComites forosComites = new (cvn, cvID);
            forosComites.ExportaForosComites(MultilangProp, listaId);

            EvalRevIDI evalRevIDI = new (cvn, cvID);
            evalRevIDI.ExportaEvalRevIDI(MultilangProp, listaId);

            EstanciasIdi estanciasIDI = new (cvn, cvID);
            estanciasIDI.ExportaEstanciasIDI(MultilangProp, listaId);

            AyudaBecas ayudaBecas = new (cvn, cvID);
            ayudaBecas.ExportaAyudaBecas(MultilangProp, listaId);

            OtrosModosColaboracion otrosModosColaboracion = new (cvn, cvID);
            otrosModosColaboracion.ExportaOtrosModosColaboracion(MultilangProp, listaId);

            SociedadesAsociaciones sociedadesAsociaciones = new (cvn, cvID);
            sociedadesAsociaciones.ExportaSociedadesAsociaciones(MultilangProp, listaId);

            Consejos consejos = new (cvn, cvID);
            consejos.ExportaConsejos(MultilangProp, listaId);

            RedesCooperacion redesCooperacion = new (cvn, cvID);
            redesCooperacion.ExportaRedesCooperacion(MultilangProp, listaId);

            PremiosMenciones premiosMenciones = new (cvn, cvID);
            premiosMenciones.ExportaPremiosMenciones(MultilangProp, listaId);

            OtrasDistinciones otrasDistinciones = new (cvn, cvID);
            otrasDistinciones.ExportaOtrasDistinciones(MultilangProp, listaId);

            PeriodosActividad periodosActividad = new (cvn, cvID);
            periodosActividad.ExportaPeriodosActividad(MultilangProp, listaId);

            AcreditacionesReconocimientos acreditacionesReconocimientos = new (cvn, cvID);
            acreditacionesReconocimientos.ExportaAcreditacionesReconocimientos(MultilangProp, listaId);

            OtrosMeritos otrosMeritos = new (cvn, cvID);
            otrosMeritos.ExportaOtrosMeritos(MultilangProp, listaId);
        }

        public void ExportaTextoLibre(Entity entity, [Optional] List<string> listaId)
        {
            TextoLibre textoLibre = new (cvn, cvID);
            textoLibre.ExportaTextoLibre(entity, listaId);
        }
    }
}
