using ExportadorWebCV.Utils;
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

namespace ImportadorWebCV.Exporta
{
    public class ExportaDatos : SeccionBase
    {
        private string cvID;
        private cvnRootResultBean cvn;

        public ExportaDatos(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {
            this.cvID = cvID;
            this.cvn = cvn;
            Utils.UtilitySecciones.GetLenguajes(mResourceApi);
        }

        public void ExportaDatosIdentificacion(Entity entity, [Optional] List<string> secciones, [Optional] bool preexportar)
        {
            string seccion = "http://w3id.org/roh/personalData";

            DatosIdentificacion datosIdentificacion = new DatosIdentificacion(cvn,cvID);
            datosIdentificacion.ExportaDatosIdentificacion(entity, seccion, secciones);
        }

        public void ExportaSituacionProfesional(Entity entity, [Optional] List<string> secciones, [Optional] bool preexportar)
        {
            string seccion = "http://w3id.org/roh/professionalSituation";

            SituacionProfesionalActual situacionProfesional = new SituacionProfesionalActual(cvn,cvID);
            situacionProfesional.ExportaSituacionProfesional(entity, seccion, secciones);

            CargosActividades cargosActividades = new CargosActividades(cvn, cvID);
            cargosActividades.ExportaCargosActividades(entity, seccion, secciones);

        }

        public void ExportaFormacionAcademica(Entity entity, [Optional] List<string> secciones, [Optional] bool preexportar)
        {
            string seccion = "http://w3id.org/roh/qualifications";

            EstudiosCiclos estudiosCiclos = new EstudiosCiclos(cvn, cvID);
            estudiosCiclos.ExportaEstudiosCiclos(entity, seccion, secciones);

            Doctorados doctorados = new Doctorados(cvn, cvID);
            doctorados.ExportaDoctorados(entity, seccion, secciones);

            OtraFormacionPosgrado otraFormacionPosgrado = new OtraFormacionPosgrado(cvn, cvID);
            otraFormacionPosgrado.ExportaOtraFormacionPosgrado(entity, seccion, secciones);

            FormacionEspecializada formacionEspecializada = new FormacionEspecializada(cvn, cvID);
            formacionEspecializada.ExportaFormacionEspecializada(entity, seccion, secciones);

            CursosMejoraDocente cursosMejora = new CursosMejoraDocente(cvn, cvID);
            cursosMejora.ExportaCursosMejoraDocente(entity, seccion, secciones);

            ConocimientoIdiomas conocimientoIdiomas = new ConocimientoIdiomas(cvn, cvID);
            conocimientoIdiomas.ExportaConocimientoIdiomas(entity, seccion, secciones);
        }
        public void ExportaActividadDocente(Entity entity, [Optional] List<string> secciones, [Optional] bool preexportar)
        {
            string seccion = "http://w3id.org/roh/teachingExperience";

            DireccionTesis direccionTesis = new DireccionTesis(cvn, cvID);
            direccionTesis.ExportaDireccionTesis(entity, seccion, secciones);

            FormacionAcademicaSubclase formacionAcademica = new FormacionAcademicaSubclase(cvn, cvID);
            formacionAcademica.ExportaFormacionAcademica(entity, seccion, secciones);

            TutoriasAcademicas tutoriasAcademicas = new TutoriasAcademicas(cvn, cvID);
            tutoriasAcademicas.ExportaTutoriasAcademicas(entity, seccion, secciones);

            CursosSeminarios cursosSeminarios = new CursosSeminarios(cvn, cvID);
            cursosSeminarios.ExportaCursosSeminarios(entity, seccion, secciones);

            PublicacionesDocentes publicacionesDocentes = new PublicacionesDocentes(cvn, cvID);
            publicacionesDocentes.ExportaPublicacionesDocentes(entity, seccion, secciones);

            ParticipacionProyectosInnovacionDocente participacionProyectos = new ParticipacionProyectosInnovacionDocente(cvn, cvID);
            participacionProyectos.ExportaParticipacionProyectos(entity, seccion, secciones);

            ParticipacionCongresosFormacionDocente participacionCongresos = new ParticipacionCongresosFormacionDocente(cvn, cvID);
            participacionCongresos.ExportaParticipacionCongresos(entity, seccion, secciones);

            PremiosInnovacionDocente premiosInnovacionDocente = new PremiosInnovacionDocente(cvn, cvID);
            premiosInnovacionDocente.ExportaPremiosInnovacionDocente(entity, seccion, secciones);

            OtrasActividades otrasActividades = new OtrasActividades(cvn, cvID);
            otrasActividades.ExportaOtrasActividades(entity, seccion, secciones);

            AportacionesRelevantes aportacionesRelevantes = new AportacionesRelevantes(cvn, cvID);
            aportacionesRelevantes.ExportaAportacionesRelevantes(entity, seccion, secciones);
        }
        public void ExportaExperienciaCientificaTecnologica(Entity entity, [Optional] List<string> secciones, [Optional] bool preexportar)
        {
            string seccion = "http://w3id.org/roh/scientificExperience";

            ProyectosIDI proyectosIDI = new ProyectosIDI(cvn, cvID);
            proyectosIDI.ExportaProyectosIDI(entity, seccion, secciones);

            Contratos contratos = new Contratos(cvn, cvID);
            contratos.ExportaContratos(entity, seccion, secciones);

            PropiedadIndustrialIntelectual propII = new PropiedadIndustrialIntelectual(cvn, cvID);
            propII.ExportaPropiedadII(entity, seccion, secciones);

            GrupoIDI grupoIDI = new GrupoIDI(cvn, cvID);
            grupoIDI.ExportaGrupoIDI(entity, seccion, secciones);

            ObrasArtisticas obrasArtisticas = new ObrasArtisticas(cvn, cvID);
            obrasArtisticas.ExportaObrasArtisticas(entity, seccion, secciones);

            ResultadosTecnologicos resultadosTecnologicos = new ResultadosTecnologicos(cvn, cvID);
            resultadosTecnologicos.ExportaResultadosTecnologicos(entity, seccion, secciones);

        }
        public void ExportaActividadCientificaTecnologica(Entity entity, [Optional] List<string> secciones, [Optional] bool preexportar)
        {
            string seccion = "http://w3id.org/roh/scientificActivity";

            ProduccionCientifica produccionCientifica = new ProduccionCientifica(cvn, cvID);
            produccionCientifica.ExportaProduccionCientifica(entity, seccion, secciones);

            IndicadoresGenerales indicadoresGenerales = new IndicadoresGenerales(cvn, cvID);
            indicadoresGenerales.ExportaIndicadoresGenerales(entity, seccion, secciones);

            //TODO Citas, Indice de impacto
            PublicacionesDocumentos publicacionesDocumentos = new PublicacionesDocumentos(cvn, cvID);
            publicacionesDocumentos.ExportaPublicacionesDocumentos(entity, seccion, secciones);

            //TODO Citas, Indice de impacto
            TrabajosCongresos trabajosCongresos = new TrabajosCongresos(cvn, cvID);
            trabajosCongresos.ExportaTrabajosCongresos(entity, seccion, secciones);

            TrabajosJornadasSeminarios trabajosJornadasSeminarios = new TrabajosJornadasSeminarios(cvn, cvID);
            trabajosJornadasSeminarios.ExportaTrabajosJornadasSeminarios(entity, seccion, secciones);

            OtrasActividadesDivulgacion otrasActividadesDivulgacion = new OtrasActividadesDivulgacion(cvn, cvID);
            otrasActividadesDivulgacion.ExportaOtrasActividadesDivulgacion(entity, seccion, secciones);

            ComitesCTA comitesCTA = new ComitesCTA(cvn, cvID);
            comitesCTA.ExportaComitesCTA(entity, seccion, secciones);

            OrganizacionesIDI organizacionesIDI = new OrganizacionesIDI(cvn, cvID);
            organizacionesIDI.ExportaOrganizacionesIDI(entity, seccion, secciones);

            GestionIDI gestionIDI = new GestionIDI(cvn, cvID);
            gestionIDI.ExportaGestionIDI(entity, seccion, secciones);

            ForosComites forosComites = new ForosComites(cvn, cvID);
            forosComites.ExportaForosComites(entity, seccion, secciones);

            EvalRevIDI evalRevIDI = new EvalRevIDI(cvn, cvID);
            evalRevIDI.ExportaEvalRevIDI(entity, seccion, secciones);

            EstanciasIDI estanciasIDI = new EstanciasIDI(cvn, cvID);
            estanciasIDI.ExportaEstanciasIDI(entity, seccion, secciones);

            AyudaBecas ayudaBecas = new AyudaBecas(cvn, cvID);
            ayudaBecas.ExportaAyudaBecas(entity, seccion, secciones);

            OtrosModosColaboracion otrosModosColaboracion = new OtrosModosColaboracion(cvn, cvID);
            otrosModosColaboracion.ExportaOtrosModosColaboracion(entity, seccion, secciones);

            SociedadesAsociaciones sociedadesAsociaciones = new SociedadesAsociaciones(cvn, cvID);
            sociedadesAsociaciones.ExportaSociedadesAsociaciones(entity, seccion, secciones);

            Consejos consejos = new Consejos(cvn, cvID);
            consejos.ExportaConsejos(entity, seccion, secciones);

            RedesCooperacion redesCooperacion = new RedesCooperacion(cvn, cvID);
            redesCooperacion.ExportaRedesCooperacion(entity, seccion, secciones);

            PremiosMenciones premiosMenciones = new PremiosMenciones(cvn, cvID);
            premiosMenciones.ExportaPremiosMenciones(entity, seccion, secciones);

            OtrasDistinciones otrasDistinciones = new OtrasDistinciones(cvn, cvID);
            otrasDistinciones.ExportaOtrasDistinciones(entity, seccion, secciones);

            PeriodosActividad periodosActividad = new PeriodosActividad(cvn, cvID);
            periodosActividad.ExportaPeriodosActividad(entity, seccion, secciones);

            AcreditacionesReconocimientos acreditacionesReconocimientos = new AcreditacionesReconocimientos(cvn, cvID);
            acreditacionesReconocimientos.ExportaAcreditacionesReconocimientos(entity, seccion, secciones);

            OtrosMeritos otrosMeritos = new OtrosMeritos(cvn, cvID);
            otrosMeritos.ExportaOtrosMeritos(entity, seccion, secciones);
        }

        public void ExportaTextoLibre(Entity entity, [Optional] List<string> secciones, [Optional] bool preexportar)
        {
            TextoLibre textoLibre = new TextoLibre(cvn, cvID);
            textoLibre.ExportaTextoLibre(entity, secciones);
        }
    }
}
