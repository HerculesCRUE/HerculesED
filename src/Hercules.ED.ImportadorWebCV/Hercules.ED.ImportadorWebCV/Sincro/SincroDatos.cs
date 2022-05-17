using Hercules.ED.ImportadorWebCV.Controllers;
using Hercules.ED.ImportadorWebCV.Models;
using Import;
using ImportadorWebCV.Sincro.Secciones;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using Utils;

namespace ImportadorWebCV.Sincro
{
    public class SincroDatos
    {
        readonly ConfigService mConfiguracion;
        private cvnRootResultBean cvn;
        private string cvID;
        private string personID;

        /// <summary>
        /// Construyo el cvnRootResultBean a partir de un archivo PDF o XML, en el caso del PDF lo transformo a XML.
        /// </summary>
        /// <param name="Configuracion"></param>
        /// <param name="cvID"></param>
        /// <param name="CVFile"></param>
        public SincroDatos(ConfigService Configuracion, string cvID, IFormFile CVFile)
        {
            mConfiguracion = Configuracion;
            string extensionFile = Path.GetExtension(CVFile.FileName);

            //Si no es un XML o un PDF. No hago nada
            if (!extensionFile.Equals(".xml") && !extensionFile.Equals(".pdf"))
            {
                throw new FileLoadException("Extensión de archivo invalida");
            }
            //Si es un PDF lo convierto a XML y lo inserto.
            if (extensionFile.Equals(".pdf"))
            {
                FormFile CVFileAsXML = GenerarRootBean(mConfiguracion, CVFile);

                XmlSerializer ser = new XmlSerializer(typeof(cvnRootResultBean));
                using (StreamReader reader = new StreamReader(CVFileAsXML.OpenReadStream()))
                {
                    cvn = (cvnRootResultBean)ser.Deserialize(reader);
                }
                this.cvID = cvID;
                this.personID = Utility.PersonaCV(cvID);
            }
            else
            {
                XmlSerializer ser = new XmlSerializer(typeof(cvnRootResultBean));
                using (StreamReader reader = new StreamReader(CVFile.OpenReadStream()))
                {
                    cvn = (cvnRootResultBean)ser.Deserialize(reader);
                }
                this.cvID = cvID;
                this.personID = Utility.PersonaCV(cvID);
            }
        }

        /// <summary>
        /// Genero un archivo XML a partir del PDF.
        /// </summary>
        /// <param name="_Configuracion"></param>
        /// <param name="pInput"></param>
        /// <returns></returns>
        private FormFile GenerarRootBean(ConfigService _Configuracion, IFormFile pInput)
        {
            long length = pInput.Length;
            using var fileStream = pInput.OpenReadStream();
            byte[] bytes = new byte[length];
            fileStream.Read(bytes, 0, (int)pInput.Length);

            Cvn2RootBeanClient cvnRootBeanClient = new Cvn2RootBeanClient();
            var x = cvnRootBeanClient.cvnPdf2CvnRootBeanAsync(_Configuracion.GetUsuarioPDF(), _Configuracion.GetContraseñaPDF(), bytes);
            Import.cvnRootResultBean cvnRootResultBean = x.Result.@return;

            XmlSerializer xmlSerializer = new XmlSerializer(cvnRootResultBean.GetType());
            MemoryStream memoryStream = new MemoryStream();

            xmlSerializer.Serialize(memoryStream, cvnRootResultBean);

            FormFile file = new FormFile(memoryStream, 0, memoryStream.Length, null, Path.GetFileName(pInput.FileName))
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/xml"
            };
            return file;
        }
        

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// apartado de Datos de identificacion y contacto.
        /// Con el codigo identificativo 000.000.000.000
        /// </summary>
        public List<Subseccion> SincroDatosIdentificacion([Optional] List<string> secciones, [Optional] bool preimportar)
        {
            DatosIdentificacion datosIdentificacion = new DatosIdentificacion(cvn, cvID);

            List<Subseccion> listadoSecciones = new List<Subseccion>();
            listadoSecciones.Add(new Subseccion("000.000.000.000", datosIdentificacion.SincroDatosIdentificacion(UtilitySecciones.CheckSecciones(secciones,"000.000.000.000"), preimportar)));

            return listadoSecciones;
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// apartado de Situación profesional.
        /// Con el codigo identificativo 010.000.000.000
        /// </summary>
        public List<Subseccion> SincroDatosSituacionProfesional([Optional] List<string> secciones, [Optional] bool preimportar)
        {
            SituacionProfesional situacionProfesional = new SituacionProfesional(cvn, cvID, personID);

            List<Subseccion> listadoSecciones = new List<Subseccion>();
            listadoSecciones.Add(new Subseccion("010.010.000.000", situacionProfesional.SincroSituacionProfesionalActual(UtilitySecciones.CheckSecciones(secciones,"010.010.000.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("010.020.000.000", situacionProfesional.SincroCargosActividades(UtilitySecciones.CheckSecciones(secciones,"010.020.000.000"), preimportar)));

            return listadoSecciones;
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// apartado de Formación académica recibida.
        /// Con el codigo identificativo 020.000.000.000
        /// </summary>
        public List<Subseccion> SincroFormacionAcademica([Optional] List<string> secciones, [Optional] bool preimportar)
        {
            FormacionAcademica formacionAcademica = new FormacionAcademica(cvn, cvID, personID);

            List<Subseccion> listadoSecciones = new List<Subseccion>();
            listadoSecciones.Add(new Subseccion("020.010.010.000", formacionAcademica.SincroEstudiosCiclos(UtilitySecciones.CheckSecciones(secciones,"020.010.010.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("020.010.020.000", formacionAcademica.SincroDoctorados(UtilitySecciones.CheckSecciones(secciones,"020.010.020.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("020.010.030.000", formacionAcademica.SincroOtraFormacionPosgrado(UtilitySecciones.CheckSecciones(secciones,"020.010.030.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("020.020.000.000", formacionAcademica.SincroFormacionEspecializada(UtilitySecciones.CheckSecciones(secciones,"020.020.000.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("020.050.000.000", formacionAcademica.SincroCursosMejoraDocente(UtilitySecciones.CheckSecciones(secciones,"020.050.000.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("020.060.000.000", formacionAcademica.SincroConocimientoIdiomas(UtilitySecciones.CheckSecciones(secciones,"020.060.000.000"), preimportar)));

            return listadoSecciones;
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// apartado de Actividad docente.
        /// Con el codigo identificativo 030.000.000.000
        /// </summary>
        public List<Subseccion> SincroActividadDocente([Optional] List<string> secciones, [Optional] bool preimportar)
        {
            ActividadDocente actividadDocente = new ActividadDocente(cvn, cvID, personID);

            List<Subseccion> listadoSecciones = new List<Subseccion>();
            listadoSecciones.Add(new Subseccion("030.040.000.000", actividadDocente.SincroDireccionTesis(UtilitySecciones.CheckSecciones(secciones,"030.040.000.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("030.010.000.000", actividadDocente.SincroFormacionAcademica(UtilitySecciones.CheckSecciones(secciones,"030.010.000.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("030.050.000.000", actividadDocente.SincroTutoriasAcademicas(UtilitySecciones.CheckSecciones(secciones,"030.050.000.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("030.060.000.000", actividadDocente.SincroCursosSeminarios(UtilitySecciones.CheckSecciones(secciones,"030.060.000.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("030.070.000.000", actividadDocente.SincroPublicacionDocentes(UtilitySecciones.CheckSecciones(secciones,"030.070.000.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("030.080.000.000", actividadDocente.SincroParticipacionProyectosInnovacionDocente(UtilitySecciones.CheckSecciones(secciones,"030.080.000.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("030.090.000.000", actividadDocente.SincroParticipacionCongresosFormacionDocente(UtilitySecciones.CheckSecciones(secciones,"030.090.000.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("060.030.080.000", actividadDocente.SincroPremiosInovacionDocente(UtilitySecciones.CheckSecciones(secciones, "060.030.080.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("030.100.000.000", actividadDocente.SincroOtrasActividades(UtilitySecciones.CheckSecciones(secciones, "030.100.000.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("030.110.000.000", actividadDocente.SincroAportacionesRelevantes(UtilitySecciones.CheckSecciones(secciones, "030.110.000.000"), preimportar)));

            return listadoSecciones;
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// apartado de Experiencia científica y tecnológica.
        /// Con el codigo identificativo 050.000.000.000
        /// </summary>
        public List<Subseccion> SincroExperienciaCientificaTecnologica([Optional] List<string> secciones, [Optional] bool preimportar)
        {
            ExperienciaCientificaTecnologica experienciaCientificaTecnologica = new ExperienciaCientificaTecnologica(cvn, cvID, personID);

            List<Subseccion> listadoSecciones = new List<Subseccion>();
            listadoSecciones.Add(new Subseccion("050.020.010.000", experienciaCientificaTecnologica.SincroProyectosIDI(UtilitySecciones.CheckSecciones(secciones,"050.020.010.000"), preimportar)));//TODO palabras clave
            listadoSecciones.Add(new Subseccion("050.020.020.000", experienciaCientificaTecnologica.SincroContratos(UtilitySecciones.CheckSecciones(secciones, "050.020.020.000"), preimportar)));//TODO palabras clave
            listadoSecciones.Add(new Subseccion("050.030.010.000", experienciaCientificaTecnologica.SincroPropiedadIndustrialIntelectual(UtilitySecciones.CheckSecciones(secciones, "050.030.010.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("050.010.000.000", experienciaCientificaTecnologica.SincroGrupoIDI(UtilitySecciones.CheckSecciones(secciones, "050.010.000.000"), preimportar)));//TODO - tesauro palabras clave->areas tematicas
            listadoSecciones.Add(new Subseccion("050.020.030.000", experienciaCientificaTecnologica.SincroObrasArtisticas(UtilitySecciones.CheckSecciones(secciones, "050.020.030.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("050.030.020.000", experienciaCientificaTecnologica.SincroResultadosTecnologicos(UtilitySecciones.CheckSecciones(secciones, "050.030.020.000"), preimportar)));

            return listadoSecciones;
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// apartado de Actividad científica y tecnológica.
        /// Con el codigo identificativo 060.000.000.000
        /// </summary>
        public List<Subseccion> SincroActividadCientificaTecnologica([Optional] List<string> secciones, [Optional] bool preimportar)
        {
            ActividadCientificaTecnologica actividadCientificaTecnologica = new ActividadCientificaTecnologica(cvn, cvID, personID);

            List<Subseccion> listadoSecciones = new List<Subseccion>();
            listadoSecciones.Add(new Subseccion("060.010.000.000", actividadCientificaTecnologica.SincroProduccionCientifica(UtilitySecciones.CheckSecciones(secciones,"060.010.000.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("060.010.060.010", actividadCientificaTecnologica.SincroIndicadoresGenerales(UtilitySecciones.CheckSecciones(secciones,"060.010.060.010"), preimportar)));
            listadoSecciones.Add(new Subseccion("060.010.010.000", actividadCientificaTecnologica.SincroPublicacionesDocumentos(UtilitySecciones.CheckSecciones(secciones,"060.010.010.000"), preimportar)));//TODO
            listadoSecciones.Add(new Subseccion("060.010.020.000", actividadCientificaTecnologica.SincroTrabajosCongresos(UtilitySecciones.CheckSecciones(secciones,"060.010.020.000"), preimportar)));//TODO
            listadoSecciones.Add(new Subseccion("060.010.030.000", actividadCientificaTecnologica.SincroTrabajosJornadasSeminarios(UtilitySecciones.CheckSecciones(secciones,"060.010.030.000"), preimportar))); //TODO - error al insertar despues de eliminar todos
            listadoSecciones.Add(new Subseccion("060.010.040.000", actividadCientificaTecnologica.SincroOtrasActividadesDivulgacion(UtilitySecciones.CheckSecciones(secciones,"060.010.040.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("060.020.010.000", actividadCientificaTecnologica.SincroComitesCTA(UtilitySecciones.CheckSecciones(secciones,"060.020.010.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("060.020.030.000", actividadCientificaTecnologica.SincroOrganizacionIDI(UtilitySecciones.CheckSecciones(secciones,"060.020.030.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("060.020.040.000", actividadCientificaTecnologica.SincroGestionIDI(UtilitySecciones.CheckSecciones(secciones,"060.020.040.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("060.020.050.000", actividadCientificaTecnologica.SincroForosComites(UtilitySecciones.CheckSecciones(secciones,"060.020.050.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("060.020.060.000", actividadCientificaTecnologica.SincroEvalRevIDI(UtilitySecciones.CheckSecciones(secciones,"060.020.060.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("060.010.050.000", actividadCientificaTecnologica.SincroEstanciasIDI(UtilitySecciones.CheckSecciones(secciones,"060.010.050.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("060.030.010.000", actividadCientificaTecnologica.SincroAyudasBecas(UtilitySecciones.CheckSecciones(secciones,"060.030.010.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("060.020.020.000", actividadCientificaTecnologica.SincroOtrosModosColaboracion(UtilitySecciones.CheckSecciones(secciones,"060.020.020.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("060.030.020.000", actividadCientificaTecnologica.SincroSociedadesAsociaciones(UtilitySecciones.CheckSecciones(secciones,"060.030.020.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("060.030.030.000", actividadCientificaTecnologica.SincroConsejos(UtilitySecciones.CheckSecciones(secciones,"060.030.030.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("060.030.040.000", actividadCientificaTecnologica.SincroRedesCooperacion(UtilitySecciones.CheckSecciones(secciones,"060.030.040.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("060.030.050.000", actividadCientificaTecnologica.SincroPremiosMenciones(UtilitySecciones.CheckSecciones(secciones,"060.030.050.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("060.030.060.000", actividadCientificaTecnologica.SincroOtrasDistinciones(UtilitySecciones.CheckSecciones(secciones,"060.030.060.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("060.030.070.000", actividadCientificaTecnologica.SincroPeriodosActividad(UtilitySecciones.CheckSecciones(secciones,"060.030.070.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("060.030.090.000", actividadCientificaTecnologica.SincroAcreditacionesObtenidas(UtilitySecciones.CheckSecciones(secciones,"060.030.090.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("060.030.100.000", actividadCientificaTecnologica.SincroResumenOtrosMeritos(UtilitySecciones.CheckSecciones(secciones,"060.030.100.000"), preimportar)));

            return listadoSecciones;
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// apartado de Resumen Texto Libre.
        /// Con el codigo identificativo 070.010.000.000
        /// </summary>
        /// <param name="preimportar"></param>
        /// <returns></returns>
        public List<Subseccion> SincroTextoLibre([Optional] List<string> secciones, [Optional] bool preimportar)
        {
            TextoLibre textoLibre = new TextoLibre(cvn, cvID);

            List<Subseccion> listadoSecciones = new List<Subseccion>();
            listadoSecciones.Add(new Subseccion("070.010.000.000", textoLibre.SincroTextoLibre(UtilitySecciones.CheckSecciones(secciones,"070.010.000.000"), preimportar)));

            return listadoSecciones;
        }
    }
}
