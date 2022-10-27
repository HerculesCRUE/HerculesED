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

            IdentificacionCurriculum identificacionCurriculo = new IdentificacionCurriculum(cvn, cvID);
            identificacionCurriculo.ExportaIdentificacionCurriculum(MultilangProp, version);

            DatosIdentificacion datosIdentificacion = new DatosIdentificacion(cvn, cvID);
            datosIdentificacion.ExportaDatosIdentificacion(entity, seccion, listaId);
        }

        public void ExportaSituacionProfesional(Entity entity, [Optional] List<string> listaId)
        {
            SituacionProfesionalActual situacionProfesional = new SituacionProfesionalActual(cvn, cvID);
            situacionProfesional.ExportaSituacionProfesional(MultilangProp, listaId);

            CargosActividades cargosActividades = new CargosActividades(cvn, cvID);
            cargosActividades.ExportaCargosActividades(MultilangProp, listaId);

        }

        public void ExportaFormacionAcademica(Entity entity, [Optional] List<string> listaId)
        {
            EstudiosCiclos estudiosCiclos = new EstudiosCiclos(cvn, cvID);
            estudiosCiclos.ExportaEstudiosCiclos(MultilangProp, listaId);

            Doctorados doctorados = new Doctorados(cvn, cvID);
            doctorados.ExportaDoctorados(MultilangProp, listaId);

            OtraFormacionPosgrado otraFormacionPosgrado = new OtraFormacionPosgrado(cvn, cvID);
            otraFormacionPosgrado.ExportaOtraFormacionPosgrado(entity, MultilangProp, listaId);

            FormacionEspecializada formacionEspecializada = new FormacionEspecializada(cvn, cvID);
            formacionEspecializada.ExportaFormacionEspecializada(entity, MultilangProp, listaId);

            CursosMejoraDocente cursosMejora = new CursosMejoraDocente(cvn, cvID);
            cursosMejora.ExportaCursosMejoraDocente(MultilangProp, listaId);

            ConocimientoIdiomas conocimientoIdiomas = new ConocimientoIdiomas(cvn, cvID);
            conocimientoIdiomas.ExportaConocimientoIdiomas(MultilangProp, listaId);
        }

        public void ExportaActividadDocente(Entity entity, [Optional] List<string> listaId)
        {
            DireccionTesis direccionTesis = new DireccionTesis(cvn, cvID);
            direccionTesis.ExportaDireccionTesis(MultilangProp, listaId);

            FormacionAcademicaSubclase formacionAcademica = new FormacionAcademicaSubclase(cvn, cvID);
            formacionAcademica.ExportaFormacionAcademica(MultilangProp, listaId);

            TutoriasAcademicas tutoriasAcademicas = new TutoriasAcademicas(cvn, cvID);
            tutoriasAcademicas.ExportaTutoriasAcademicas(MultilangProp, listaId);

            CursosSeminarios cursosSeminarios = new CursosSeminarios(cvn, cvID);
            cursosSeminarios.ExportaCursosSeminarios(MultilangProp, listaId);

            PublicacionesDocentes publicacionesDocentes = new PublicacionesDocentes(cvn, cvID);
            publicacionesDocentes.ExportaPublicacionesDocentes(MultilangProp, listaId);

            ParticipacionProyectosInnovacionDocente participacionProyectos = new ParticipacionProyectosInnovacionDocente(cvn, cvID);
            participacionProyectos.ExportaParticipacionProyectos(MultilangProp, listaId);

            ParticipacionCongresosFormacionDocente participacionCongresos = new ParticipacionCongresosFormacionDocente(cvn, cvID);
            participacionCongresos.ExportaParticipacionCongresos(MultilangProp, listaId);

            PremiosInnovacionDocente premiosInnovacionDocente = new PremiosInnovacionDocente(cvn, cvID);
            premiosInnovacionDocente.ExportaPremiosInnovacionDocente(MultilangProp, listaId);

            OtrasActividades otrasActividades = new OtrasActividades(cvn, cvID);
            otrasActividades.ExportaOtrasActividades(MultilangProp, listaId);

            AportacionesRelevantes aportacionesRelevantes = new AportacionesRelevantes(cvn, cvID);
            aportacionesRelevantes.ExportaAportacionesRelevantes(MultilangProp, listaId);
        }
        public void ExportaExperienciaCientificaTecnologica(Entity entity, [Optional] List<string> listaId)
        {
            string seccion = "http://w3id.org/roh/scientificExperience";

            ProyectosIDI proyectosIDI = new ProyectosIDI(cvn, cvID);
            proyectosIDI.ExportaProyectosIDI(seccion, MultilangProp, listaId);

            Contratos contratos = new Contratos(cvn, cvID);
            contratos.ExportaContratos(MultilangProp, listaId);

            PropiedadIndustrialIntelectual propII = new PropiedadIndustrialIntelectual(cvn, cvID);
            propII.ExportaPropiedadII(MultilangProp, listaId);

            GrupoIDI grupoIDI = new GrupoIDI(cvn, cvID);
            grupoIDI.ExportaGrupoIDI(MultilangProp, listaId);

            ObrasArtisticas obrasArtisticas = new ObrasArtisticas(cvn, cvID);
            obrasArtisticas.ExportaObrasArtisticas(MultilangProp, listaId);

            ResultadosTecnologicos resultadosTecnologicos = new ResultadosTecnologicos(cvn, cvID);
            resultadosTecnologicos.ExportaResultadosTecnologicos(MultilangProp, listaId);

        }
        public void ExportaActividadCientificaTecnologica(Entity entity, string versionExportacion, [Optional] List<string> listaId)
        {
            string seccion = "http://w3id.org/roh/scientificActivity";

            ProduccionCientifica produccionCientifica = new ProduccionCientifica(cvn, cvID);
            produccionCientifica.ExportaProduccionCientifica(MultilangProp, versionExportacion, listaId);

            IndicadoresGenerales indicadoresGenerales = new IndicadoresGenerales(cvn, cvID);
            indicadoresGenerales.ExportaIndicadoresGenerales(entity, MultilangProp, listaId);

            PublicacionesDocumentos publicacionesDocumentos = new PublicacionesDocumentos(cvn, cvID);
            publicacionesDocumentos.ExportaPublicacionesDocumentos(seccion, MultilangProp, listaId);

            TrabajosCongresos trabajosCongresos = new TrabajosCongresos(cvn, cvID);
            trabajosCongresos.ExportaTrabajosCongresos(seccion, MultilangProp, versionExportacion, listaId);

            TrabajosJornadasSeminarios trabajosJornadasSeminarios = new TrabajosJornadasSeminarios(cvn, cvID);
            trabajosJornadasSeminarios.ExportaTrabajosJornadasSeminarios(MultilangProp, listaId);

            OtrasActividadesDivulgacion otrasActividadesDivulgacion = new OtrasActividadesDivulgacion(cvn, cvID);
            otrasActividadesDivulgacion.ExportaOtrasActividadesDivulgacion(MultilangProp, listaId);

            ComitesCta comitesCTA = new ComitesCta(cvn, cvID);
            comitesCTA.ExportaComitesCTA(MultilangProp, listaId);

            OrganizacionesIDI organizacionesIDI = new OrganizacionesIDI(cvn, cvID);
            organizacionesIDI.ExportaOrganizacionesIDI(MultilangProp, listaId);

            GestionIdi gestionIDI = new GestionIdi(cvn, cvID);
            gestionIDI.ExportaGestionIDI(MultilangProp, listaId);

            ForosComites forosComites = new ForosComites(cvn, cvID);
            forosComites.ExportaForosComites(MultilangProp, listaId);

            EvalRevIDI evalRevIDI = new EvalRevIDI(cvn, cvID);
            evalRevIDI.ExportaEvalRevIDI(MultilangProp, listaId);

            EstanciasIdi estanciasIDI = new EstanciasIdi(cvn, cvID);
            estanciasIDI.ExportaEstanciasIDI(MultilangProp, listaId);

            AyudaBecas ayudaBecas = new AyudaBecas(cvn, cvID);
            ayudaBecas.ExportaAyudaBecas(MultilangProp, listaId);

            OtrosModosColaboracion otrosModosColaboracion = new OtrosModosColaboracion(cvn, cvID);
            otrosModosColaboracion.ExportaOtrosModosColaboracion(MultilangProp, listaId);

            SociedadesAsociaciones sociedadesAsociaciones = new SociedadesAsociaciones(cvn, cvID);
            sociedadesAsociaciones.ExportaSociedadesAsociaciones(MultilangProp, listaId);

            Consejos consejos = new Consejos(cvn, cvID);
            consejos.ExportaConsejos(MultilangProp, listaId);

            RedesCooperacion redesCooperacion = new RedesCooperacion(cvn, cvID);
            redesCooperacion.ExportaRedesCooperacion(MultilangProp, listaId);

            PremiosMenciones premiosMenciones = new PremiosMenciones(cvn, cvID);
            premiosMenciones.ExportaPremiosMenciones(MultilangProp, listaId);

            OtrasDistinciones otrasDistinciones = new OtrasDistinciones(cvn, cvID);
            otrasDistinciones.ExportaOtrasDistinciones(MultilangProp, listaId);

            PeriodosActividad periodosActividad = new PeriodosActividad(cvn, cvID);
            periodosActividad.ExportaPeriodosActividad(MultilangProp, listaId);

            AcreditacionesReconocimientos acreditacionesReconocimientos = new AcreditacionesReconocimientos(cvn, cvID);
            acreditacionesReconocimientos.ExportaAcreditacionesReconocimientos(MultilangProp, listaId);

            OtrosMeritos otrosMeritos = new OtrosMeritos(cvn, cvID);
            otrosMeritos.ExportaOtrosMeritos(MultilangProp, listaId);
        }

        public void ExportaTextoLibre(Entity entity, [Optional] List<string> listaId)
        {
            TextoLibre textoLibre = new TextoLibre(cvn, cvID);
            textoLibre.ExportaTextoLibre(entity, listaId);
        }
    }
}
