using HerculesAplicacionConsola.Sincro.Secciones;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace HerculesAplicacionConsola.Sincro
{
    public class SincroDatos
    {
        private cvnRootResultBean cvn;
        private string cvID;

        public SincroDatos()
        {
            cvn = new cvnRootResultBean();
        }

        public SincroDatos(string ruta, string cvID)
        {
            XmlSerializer ser = new XmlSerializer(typeof(cvnRootResultBean));

            using (Stream reader = new FileStream(ruta, FileMode.Open))
            {
                try
                {
                    cvn = (cvnRootResultBean)ser.Deserialize(reader);
                }
                catch (InvalidOperationException e)
                {
                    Console.Error.WriteLine("Error de lectura del XML " + e);
                }
            }
            this.cvID = cvID;
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// apartado de Datos de identificacion y contacto.
        /// Con el codigo identificativo 000.000.000.000
        /// </summary>
        public void SincroDatosIdentificacion()
        {       
            DatosIdentificacion datosIdentificacion = new DatosIdentificacion(cvn, cvID);
            //datosIdentificacion.SincroDatosIdentificacion();
        }


        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// apartado de Situación profesional.stop
        /// Con el codigo identificativo 010.000.000.000
        /// </summary>
        public void SincroDatosSituacionProfesional()
        {
            //SituacionProfesional situacionProfesional = new SituacionProfesional(cvn, cvID);

            //situacionProfesional.SincroSituacionProfesionalActual();
            //situacionProfesional.SincroCargosActividades();
        }
        
        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// apartado de Formación académica recibida.
        /// Con el codigo identificativo 020.000.000.000
        /// </summary>
        public void SincroFormacionAcademica()
        {
            //FormacionAcademica formacionAcademica = new FormacionAcademica(cvn,cvID);

            //formacionAcademica.SincroEstudiosCiclos();
            //formacionAcademica.SincroDoctorados();
            //formacionAcademica.SincroOtraFormacionPosgrado();
            //formacionAcademica.SincroFormacionEspecializada();
            //formacionAcademica.SincroFormacionSanitaria();
            //formacionAcademica.SincroFormacionSanitariaIMasD();
            //formacionAcademica.SincroCursosMejoraDocente();
            //formacionAcademica.SincroConocimientoIdiomas();
        }
        
        
        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// apartado de Actividad docente.
        /// Con el codigo identificativo 030.000.000.000
        /// </summary>
        public void SincroActividadDocente()
        {
            //ActividadDocente actividadDocente = new ActividadDocente(cvn, cvID);

            //actividadDocente.SincroDireccionTesis();
            //actividadDocente.SincroFormacionAcademica();
            //actividadDocente.SincroFormacionSanitariaEspecializada();
            //actividadDocente.SincroFormacionSanitariaIMasD();
            //actividadDocente.SincroTutoriasAcademicas();
            //actividadDocente.SincroCursosSeminarios();
            //actividadDocente.SincroPublicacionDocentes();
            //actividadDocente.SincroParticipacionProyectosInnovacionDocente();
            //actividadDocente.SincroParticipacionCongresosFormacionDocente();
            //actividadDocente.SincroPremiosInovacionDocente();
            //actividadDocente.SincroOtrasActividades();
            //actividadDocente.SincroAportacionesRelevantes();
        }
        
        
        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// apartado de Experiencia científica y tecnológica.
        /// Con el codigo identificativo 050.000.000.000
        /// </summary>
        public void SincroExperienciaCientificaTecnologica()
        {
            //ExperienciaCientificaTecnologica experienciaCientificaTecnologica = new ExperienciaCientificaTecnologica(cvn,cvID);

            //experienciaCientificaTecnologica.SincroProyectosIDI();//TODO - Entidad financiadora, Autores, Entidades Participantes, regimen dedicacion, aportacion solicitante, palabras clave, entidad financiacion, grado contribucion,palabras clave, tipo participacion
            //experienciaCientificaTecnologica.SincroContratos();//TODO - Autores, Entidades participantes, ¿entidad realizacion tipo?, grado contribucion, palabras clave, nombre programa, entidad financiadora
            //experienciaCientificaTecnologica.SincroPropiedadIndustrialIntelectual();//TODO - Explotacion, Autores, Palabras clave, Inscripcion, Descripcion(genera errores)
            //experienciaCientificaTecnologica.SincroGrupoIDI();//TODO - Autores, palabras clave, Radicacion, Fechainicio, duracion, clase colaboracion
            //experienciaCientificaTecnologica.SincroObrasArtisticas();//TODO - Autores, comisario sin propiedad, foro exposicion, titulos publicacion
            //experienciaCientificaTecnologica.SincroResultadosTecnologicos();//TODO - falta todo
        }


        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// apartado de Actividad científica y tecnológica.
        /// Con el codigo identificativo 060.000.000.000
        /// </summary>
        public void SincroActividadCientificaTecnologica()
        {
            ActividadCientificaTecnologica actividadCientificaTecnologica = new ActividadCientificaTecnologica(cvn, cvID);

            // -------------------           TODO - comprobar si las entidades exiten en BBDD en los apartados de las mismas

            actividadCientificaTecnologica.SincroIndicadoresGenerales();
            //actividadCientificaTecnologica.SincroPublicacionesDocumentos();//TODO - Autores, categoria, autor correspondencia, publicacion ciudad, publicacion editorial¿?, posicion, numautores
            //actividadCientificaTecnologica.SincroTrabajosCongresos();//TODO - Autores, publicacion editorial¿?
            //actividadCientificaTecnologica.SincroTrabajosJornadasSeminarios();//TODO - Autores, TiposEntidadOrganizadora?
            //actividadCientificaTecnologica.SincroOtrasActividadesDivulgacion();//TODO - Autor, TipoEntidad?, +++
            //actividadCientificaTecnologica.SincroComitesCTA();
            //actividadCientificaTecnologica.SincroOrganizacionIDI();
            //actividadCientificaTecnologica.SincroGestionIDI();
            actividadCientificaTecnologica.SincroForosComites();
            //actividadCientificaTecnologica.SincroEvalRevIDI();
            //actividadCientificaTecnologica.SincroEstanciasIDI();
            //actividadCientificaTecnologica.SincroAyudasBecas();
            //actividadCientificaTecnologica.SincroOtrosModosColaboracion();//TODO - falta añadir participantes, palabras clave //TODO - eliminar entidades secundarias (entidad participante)
            //actividadCientificaTecnologica.SincroSociedadesAsociaciones();
            actividadCientificaTecnologica.SincroConsejos();
            actividadCientificaTecnologica.SincroRedesCooperacion();
            actividadCientificaTecnologica.SincroPremiosMenciones();
            actividadCientificaTecnologica.SincroOtrasDistinciones();
            //actividadCientificaTecnologica.SincroPeriodosActividad();
            actividadCientificaTecnologica.SincroAcreditacionesObtenidas();
            actividadCientificaTecnologica.SincroResumenOtrosMeritos();
        }
    }
}
