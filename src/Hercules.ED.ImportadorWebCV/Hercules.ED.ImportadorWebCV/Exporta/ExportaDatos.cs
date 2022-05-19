using Gnoss.ApiWrapper;
using ImportadorWebCV.Exporta.Secciones;
using ImportadorWebCV.Exporta.Secciones.ActividadCientificaSubclases;
using ImportadorWebCV.Exporta.Secciones.ActividadDocenteSubclases;
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
        private string cvID;
        private cvnRootResultBean cvn;
        private ResourceApi resourceApi;
        protected Dictionary<string, List<Dictionary<string, Data>>> MultilangProp;

        public ExportaDatos(cvnRootResultBean cvn, string cvID, string lang) : base(cvn, cvID)
        {
            this.cvID = cvID;
            this.cvn = cvn;
            resourceApi = mResourceApi;
            Utils.UtilitySecciones.GetLenguajes(mResourceApi);
            MultilangProp = GetMultilangProperties(cvID, lang);
        }

        public ResourceApi GetResourceApi()
        {
            return resourceApi;
        }

        public void ExportaDatosIdentificacion(Entity entity, [Optional] List<string> secciones, [Optional] bool preexportar)
        {
            string seccion = "http://w3id.org/roh/personalData";

            DatosIdentificacion datosIdentificacion = new DatosIdentificacion(cvn, cvID);
            datosIdentificacion.ExportaDatosIdentificacion(entity, seccion, secciones);
        }

        public void ExportaSituacionProfesional(Entity entity, [Optional] List<string> secciones, [Optional] bool preexportar)
        {
            string seccion = "http://w3id.org/roh/professionalSituation";

            SituacionProfesionalActual situacionProfesional = new SituacionProfesionalActual(cvn, cvID);
            situacionProfesional.ExportaSituacionProfesional(entity, seccion, MultilangProp, secciones);

            CargosActividades cargosActividades = new CargosActividades(cvn, cvID);
            cargosActividades.ExportaCargosActividades(entity, seccion, MultilangProp, secciones);

        }

        public void ExportaFormacionAcademica(Entity entity, [Optional] List<string> secciones, [Optional] bool preexportar)
        {
            string seccion = "http://w3id.org/roh/qualifications";

            EstudiosCiclos estudiosCiclos = new EstudiosCiclos(cvn, cvID);
            estudiosCiclos.ExportaEstudiosCiclos(entity, seccion, MultilangProp, secciones);

            Doctorados doctorados = new Doctorados(cvn, cvID);
            doctorados.ExportaDoctorados(entity, seccion, MultilangProp, secciones);

            OtraFormacionPosgrado otraFormacionPosgrado = new OtraFormacionPosgrado(cvn, cvID);
            otraFormacionPosgrado.ExportaOtraFormacionPosgrado(entity, seccion, MultilangProp, secciones);

            FormacionEspecializada formacionEspecializada = new FormacionEspecializada(cvn, cvID);
            formacionEspecializada.ExportaFormacionEspecializada(entity, seccion, MultilangProp, secciones);

            CursosMejoraDocente cursosMejora = new CursosMejoraDocente(cvn, cvID);
            cursosMejora.ExportaCursosMejoraDocente(entity, seccion, MultilangProp, secciones);

            ConocimientoIdiomas conocimientoIdiomas = new ConocimientoIdiomas(cvn, cvID);
            conocimientoIdiomas.ExportaConocimientoIdiomas(entity, seccion, MultilangProp, secciones);
        }
        public void ExportaActividadDocente(Entity entity, [Optional] List<string> secciones, [Optional] bool preexportar)
        {
            string seccion = "http://w3id.org/roh/teachingExperience";

            DireccionTesis direccionTesis = new DireccionTesis(cvn, cvID);
            direccionTesis.ExportaDireccionTesis(entity, seccion, MultilangProp, secciones);

            FormacionAcademicaSubclase formacionAcademica = new FormacionAcademicaSubclase(cvn, cvID);
            formacionAcademica.ExportaFormacionAcademica(entity, seccion, MultilangProp, secciones);

            TutoriasAcademicas tutoriasAcademicas = new TutoriasAcademicas(cvn, cvID);
            tutoriasAcademicas.ExportaTutoriasAcademicas(entity, seccion, MultilangProp, secciones);

            CursosSeminarios cursosSeminarios = new CursosSeminarios(cvn, cvID);
            cursosSeminarios.ExportaCursosSeminarios(entity, seccion, MultilangProp, secciones);

            PublicacionesDocentes publicacionesDocentes = new PublicacionesDocentes(cvn, cvID);
            publicacionesDocentes.ExportaPublicacionesDocentes(entity, seccion, MultilangProp, secciones);

            ParticipacionProyectosInnovacionDocente participacionProyectos = new ParticipacionProyectosInnovacionDocente(cvn, cvID);
            participacionProyectos.ExportaParticipacionProyectos(entity, seccion, MultilangProp, secciones);

            ParticipacionCongresosFormacionDocente participacionCongresos = new ParticipacionCongresosFormacionDocente(cvn, cvID);
            participacionCongresos.ExportaParticipacionCongresos(entity, seccion, MultilangProp, secciones);

            PremiosInnovacionDocente premiosInnovacionDocente = new PremiosInnovacionDocente(cvn, cvID);
            premiosInnovacionDocente.ExportaPremiosInnovacionDocente(entity, seccion, MultilangProp, secciones);

            OtrasActividades otrasActividades = new OtrasActividades(cvn, cvID);
            otrasActividades.ExportaOtrasActividades(entity, seccion, MultilangProp, secciones);

            AportacionesRelevantes aportacionesRelevantes = new AportacionesRelevantes(cvn, cvID);
            aportacionesRelevantes.ExportaAportacionesRelevantes(entity, seccion, MultilangProp, secciones);
        }
        public void ExportaExperienciaCientificaTecnologica(Entity entity, [Optional] List<string> secciones, [Optional] bool preexportar)
        {
            string seccion = "http://w3id.org/roh/scientificExperience";

            ProyectosIDI proyectosIDI = new ProyectosIDI(cvn, cvID);
            proyectosIDI.ExportaProyectosIDI(entity, seccion, MultilangProp, secciones);

            Contratos contratos = new Contratos(cvn, cvID);
            contratos.ExportaContratos(entity, seccion, MultilangProp, secciones);

            PropiedadIndustrialIntelectual propII = new PropiedadIndustrialIntelectual(cvn, cvID);
            propII.ExportaPropiedadII(entity, seccion, MultilangProp, secciones);

            GrupoIDI grupoIDI = new GrupoIDI(cvn, cvID);
            grupoIDI.ExportaGrupoIDI(entity, seccion, MultilangProp, secciones);

            ObrasArtisticas obrasArtisticas = new ObrasArtisticas(cvn, cvID);
            obrasArtisticas.ExportaObrasArtisticas(entity, seccion, MultilangProp, secciones);

            ResultadosTecnologicos resultadosTecnologicos = new ResultadosTecnologicos(cvn, cvID);
            resultadosTecnologicos.ExportaResultadosTecnologicos(entity, seccion, MultilangProp, secciones);

        }
        public void ExportaActividadCientificaTecnologica(Entity entity, [Optional] List<string> secciones, [Optional] bool preexportar)
        {
            string seccion = "http://w3id.org/roh/scientificActivity";

            ProduccionCientifica produccionCientifica = new ProduccionCientifica(cvn, cvID);
            produccionCientifica.ExportaProduccionCientifica(entity, seccion, MultilangProp, secciones);

            IndicadoresGenerales indicadoresGenerales = new IndicadoresGenerales(cvn, cvID);
            indicadoresGenerales.ExportaIndicadoresGenerales(entity, seccion, MultilangProp, secciones);

            //TODO Citas, Indice de impacto
            PublicacionesDocumentos publicacionesDocumentos = new PublicacionesDocumentos(cvn, cvID);
            publicacionesDocumentos.ExportaPublicacionesDocumentos(entity, seccion, MultilangProp, secciones);

            //TODO Citas, Indice de impacto
            TrabajosCongresos trabajosCongresos = new TrabajosCongresos(cvn, cvID);
            trabajosCongresos.ExportaTrabajosCongresos(entity, seccion, MultilangProp, secciones);

            TrabajosJornadasSeminarios trabajosJornadasSeminarios = new TrabajosJornadasSeminarios(cvn, cvID);
            trabajosJornadasSeminarios.ExportaTrabajosJornadasSeminarios(entity, seccion, MultilangProp, secciones);

            OtrasActividadesDivulgacion otrasActividadesDivulgacion = new OtrasActividadesDivulgacion(cvn, cvID);
            otrasActividadesDivulgacion.ExportaOtrasActividadesDivulgacion(entity, seccion, MultilangProp, secciones);

            ComitesCTA comitesCTA = new ComitesCTA(cvn, cvID);
            comitesCTA.ExportaComitesCTA(entity, seccion, MultilangProp, secciones);

            OrganizacionesIDI organizacionesIDI = new OrganizacionesIDI(cvn, cvID);
            organizacionesIDI.ExportaOrganizacionesIDI(entity, seccion, MultilangProp, secciones);

            GestionIDI gestionIDI = new GestionIDI(cvn, cvID);
            gestionIDI.ExportaGestionIDI(entity, seccion, MultilangProp, secciones);

            ForosComites forosComites = new ForosComites(cvn, cvID);
            forosComites.ExportaForosComites(entity, seccion, MultilangProp, secciones);

            EvalRevIDI evalRevIDI = new EvalRevIDI(cvn, cvID);
            evalRevIDI.ExportaEvalRevIDI(entity, seccion, MultilangProp, secciones);

            EstanciasIDI estanciasIDI = new EstanciasIDI(cvn, cvID);
            estanciasIDI.ExportaEstanciasIDI(entity, seccion, MultilangProp, secciones);

            AyudaBecas ayudaBecas = new AyudaBecas(cvn, cvID);
            ayudaBecas.ExportaAyudaBecas(entity, seccion, MultilangProp, secciones);

            OtrosModosColaboracion otrosModosColaboracion = new OtrosModosColaboracion(cvn, cvID);
            otrosModosColaboracion.ExportaOtrosModosColaboracion(entity, seccion, MultilangProp, secciones);

            SociedadesAsociaciones sociedadesAsociaciones = new SociedadesAsociaciones(cvn, cvID);
            sociedadesAsociaciones.ExportaSociedadesAsociaciones(entity, seccion, MultilangProp, secciones);

            Consejos consejos = new Consejos(cvn, cvID);
            consejos.ExportaConsejos(entity, seccion, MultilangProp, secciones);

            RedesCooperacion redesCooperacion = new RedesCooperacion(cvn, cvID);
            redesCooperacion.ExportaRedesCooperacion(entity, seccion, MultilangProp, secciones);

            PremiosMenciones premiosMenciones = new PremiosMenciones(cvn, cvID);
            premiosMenciones.ExportaPremiosMenciones(entity, seccion, MultilangProp, secciones);

            OtrasDistinciones otrasDistinciones = new OtrasDistinciones(cvn, cvID);
            otrasDistinciones.ExportaOtrasDistinciones(entity, seccion, MultilangProp, secciones);

            PeriodosActividad periodosActividad = new PeriodosActividad(cvn, cvID);
            periodosActividad.ExportaPeriodosActividad(entity, seccion, MultilangProp, secciones);

            AcreditacionesReconocimientos acreditacionesReconocimientos = new AcreditacionesReconocimientos(cvn, cvID);
            acreditacionesReconocimientos.ExportaAcreditacionesReconocimientos(entity, seccion, MultilangProp, secciones);

            OtrosMeritos otrosMeritos = new OtrosMeritos(cvn, cvID);
            otrosMeritos.ExportaOtrosMeritos(entity, seccion, MultilangProp, secciones);
        }

        public void ExportaTextoLibre(Entity entity, [Optional] List<string> secciones, [Optional] bool preexportar)
        {
            TextoLibre textoLibre = new TextoLibre(cvn, cvID);
            textoLibre.ExportaTextoLibre(entity, secciones);
        }
    }
}
