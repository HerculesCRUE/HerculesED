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
        private cvnRootResultBean cvn;
        private string cvID;

        public SincroDatos()
        {
            cvn = new cvnRootResultBean();
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
        /// Construyo el cvnRootResultBean a partir de un archivo PDF o XML, en el caso del PDF lo transformo a XML.
        /// </summary>
        /// <param name="_Configuracion"></param>
        /// <param name="cvID"></param>
        /// <param name="CVFile"></param>
        public SincroDatos(ConfigService _Configuracion, string cvID, IFormFile CVFile)
        {
            string extensionFile = Path.GetExtension(CVFile.FileName);

            //Si no es un XML o un PDF. No hago nada
            if (!extensionFile.Equals(".xml") && !extensionFile.Equals(".pdf"))
            {
                throw new FileLoadException("Extensión de archivo invalida");
            }
            //Si es un PDF lo convierto a XML y lo inserto.
            if (extensionFile.Equals(".pdf"))
            {
                FormFile CVFileAsXML = GenerarRootBean(_Configuracion, CVFile);

                XmlSerializer ser = new XmlSerializer(typeof(cvnRootResultBean));
                using (StreamReader reader = new StreamReader(CVFileAsXML.OpenReadStream()))
                {
                    cvn = (cvnRootResultBean)ser.Deserialize(reader);
                }
                this.cvID = cvID;
            }
            else
            {
                XmlSerializer ser = new XmlSerializer(typeof(cvnRootResultBean));
                using (StreamReader reader = new StreamReader(CVFile.OpenReadStream()))
                {
                    cvn = (cvnRootResultBean)ser.Deserialize(reader);
                }
                this.cvID = cvID;
            }
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
            listadoSecciones.Add(new Subseccion("000.000.000.000", datosIdentificacion.SincroDatosIdentificacion(secciones != null && secciones.Contains("000.000.000.000"), preimportar)));

            return listadoSecciones;
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// apartado de Situación profesional.
        /// Con el codigo identificativo 010.000.000.000
        /// </summary>
        public List<Subseccion> SincroDatosSituacionProfesional([Optional] List<string> secciones, [Optional] bool preimportar)
        {
            SituacionProfesional situacionProfesional = new SituacionProfesional(cvn, cvID);

            List<Subseccion> listadoSecciones = new List<Subseccion>();
            listadoSecciones.Add(new Subseccion("010.010.000.000", situacionProfesional.SincroSituacionProfesionalActual(secciones != null && secciones.Contains("010.010.000.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("010.020.000.000", situacionProfesional.SincroCargosActividades(secciones != null && secciones.Contains("010.020.000.000"), preimportar)));

            return listadoSecciones;
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// apartado de Formación académica recibida.
        /// Con el codigo identificativo 020.000.000.000
        /// </summary>
        public List<Subseccion> SincroFormacionAcademica([Optional] List<string> secciones, [Optional] bool preimportar)
        {
            FormacionAcademica formacionAcademica = new FormacionAcademica(cvn, cvID);

            List<Subseccion> listadoSecciones = new List<Subseccion>();
            listadoSecciones.Add(new Subseccion("020.010.010.000", formacionAcademica.SincroEstudiosCiclos(secciones != null && secciones.Contains("020.010.010.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("020.010.020.000", formacionAcademica.SincroDoctorados(secciones != null && secciones.Contains("020.010.020.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("020.010.030.000", formacionAcademica.SincroOtraFormacionPosgrado(secciones != null && secciones.Contains("020.010.030.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("020.020.000.000", formacionAcademica.SincroFormacionEspecializada(secciones != null && secciones.Contains("020.020.000.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("020.050.000.000", formacionAcademica.SincroCursosMejoraDocente(secciones != null && secciones.Contains("020.050.000.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("020.060.000.000", formacionAcademica.SincroConocimientoIdiomas(secciones != null && secciones.Contains("020.060.000.000"), preimportar)));

            return listadoSecciones;
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// apartado de Actividad docente.
        /// Con el codigo identificativo 030.000.000.000
        /// </summary>
        public List<Subseccion> SincroActividadDocente([Optional] List<string> secciones, [Optional] bool preimportar)
        {
            ActividadDocente actividadDocente = new ActividadDocente(cvn, cvID);

            List<Subseccion> listadoSecciones = new List<Subseccion>();
            listadoSecciones.Add(new Subseccion("030.040.000.000", actividadDocente.SincroDireccionTesis(secciones != null && secciones.Contains("030.040.000.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("030.010.000.000", actividadDocente.SincroFormacionAcademica(secciones != null && secciones.Contains("030.010.000.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("030.050.000.000", actividadDocente.SincroTutoriasAcademicas(secciones != null && secciones.Contains("030.050.000.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("030.060.000.000", actividadDocente.SincroCursosSeminarios(secciones != null && secciones.Contains("030.060.000.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("030.070.000.000", actividadDocente.SincroPublicacionDocentes(secciones != null && secciones.Contains("030.070.000.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("030.080.000.000", actividadDocente.SincroParticipacionProyectosInnovacionDocente(secciones != null && secciones.Contains("030.080.000.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("030.090.000.000", actividadDocente.SincroParticipacionCongresosFormacionDocente(secciones != null && secciones.Contains("030.090.000.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("060.030.080.000", actividadDocente.SincroPremiosInovacionDocente(secciones != null && secciones.Contains("060.030.080.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("030.100.000.000", actividadDocente.SincroOtrasActividades(secciones != null && secciones.Contains("030.100.000.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("030.110.000.000", actividadDocente.SincroAportacionesRelevantes(secciones != null && secciones.Contains("030.110.000.000"), preimportar)));

            return listadoSecciones;
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// apartado de Experiencia científica y tecnológica.
        /// Con el codigo identificativo 050.000.000.000
        /// </summary>
        public List<Subseccion> SincroExperienciaCientificaTecnologica([Optional] List<string> secciones, [Optional] bool preimportar)
        {
            ExperienciaCientificaTecnologica experienciaCientificaTecnologica = new ExperienciaCientificaTecnologica(cvn, cvID);

            List<Subseccion> listadoSecciones = new List<Subseccion>();
            listadoSecciones.Add(new Subseccion("050.020.010.000", experienciaCientificaTecnologica.SincroProyectosIDI(secciones != null && secciones.Contains("050.020.010.000"), preimportar)));//TODO palabras clave
            listadoSecciones.Add(new Subseccion("050.020.020.000", experienciaCientificaTecnologica.SincroContratos(secciones != null && secciones.Contains("050.020.020.000"), preimportar)));//TODO palabras clave
            listadoSecciones.Add(new Subseccion("050.030.010.000", experienciaCientificaTecnologica.SincroPropiedadIndustrialIntelectual(secciones != null && secciones.Contains("050.030.010.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("050.010.000.000", experienciaCientificaTecnologica.SincroGrupoIDI(secciones != null && secciones.Contains("050.010.000.000"), preimportar)));//TODO - tesauro palabras clave->areas tematicas
            listadoSecciones.Add(new Subseccion("050.020.030.000", experienciaCientificaTecnologica.SincroObrasArtisticas(secciones != null && secciones.Contains("050.020.030.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("050.030.020.000", experienciaCientificaTecnologica.SincroResultadosTecnologicos(secciones != null && secciones.Contains("050.030.020.000"), preimportar)));

            return listadoSecciones;
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// apartado de Actividad científica y tecnológica.
        /// Con el codigo identificativo 060.000.000.000
        /// </summary>
        public List<Subseccion> SincroActividadCientificaTecnologica([Optional] List<string> secciones, [Optional] bool preimportar)
        {
            ActividadCientificaTecnologica actividadCientificaTecnologica = new ActividadCientificaTecnologica(cvn, cvID);

            List<Subseccion> listadoSecciones = new List<Subseccion>();
            listadoSecciones.Add(new Subseccion("060.010.000.000", actividadCientificaTecnologica.SincroProduccionCientifica(secciones != null && secciones.Contains("060.010.000.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("060.010.060.010", actividadCientificaTecnologica.SincroIndicadoresGenerales(secciones != null && secciones.Contains("060.010.060.010"), preimportar)));
            listadoSecciones.Add(new Subseccion("060.010.010.000", actividadCientificaTecnologica.SincroPublicacionesDocumentos(secciones != null && secciones.Contains("060.010.010.000"), preimportar)));//TODO
            listadoSecciones.Add(new Subseccion("060.010.020.000", actividadCientificaTecnologica.SincroTrabajosCongresos(secciones != null && secciones.Contains("060.010.020.000"), preimportar)));//TODO
            listadoSecciones.Add(new Subseccion("060.010.030.000", actividadCientificaTecnologica.SincroTrabajosJornadasSeminarios(secciones != null && secciones.Contains("060.010.030.000"), preimportar))); //TODO - error al insertar despues de eliminar todos
            listadoSecciones.Add(new Subseccion("060.010.040.000", actividadCientificaTecnologica.SincroOtrasActividadesDivulgacion(secciones != null && secciones.Contains("060.010.040.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("060.020.010.000", actividadCientificaTecnologica.SincroComitesCTA(secciones != null && secciones.Contains("060.020.010.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("060.020.030.000", actividadCientificaTecnologica.SincroOrganizacionIDI(secciones != null && secciones.Contains("060.020.030.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("060.020.040.000", actividadCientificaTecnologica.SincroGestionIDI(secciones != null && secciones.Contains("060.020.040.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("060.020.050.000", actividadCientificaTecnologica.SincroForosComites(secciones != null && secciones.Contains("060.020.050.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("060.020.060.000", actividadCientificaTecnologica.SincroEvalRevIDI(secciones != null && secciones.Contains("060.020.060.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("060.010.050.000", actividadCientificaTecnologica.SincroEstanciasIDI(secciones != null && secciones.Contains("060.010.050.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("060.030.010.000", actividadCientificaTecnologica.SincroAyudasBecas(secciones != null && secciones.Contains("060.030.010.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("060.020.020.000", actividadCientificaTecnologica.SincroOtrosModosColaboracion(secciones != null && secciones.Contains("060.020.020.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("060.030.020.000", actividadCientificaTecnologica.SincroSociedadesAsociaciones(secciones != null && secciones.Contains("060.030.020.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("060.030.030.000", actividadCientificaTecnologica.SincroConsejos(secciones != null && secciones.Contains("060.030.030.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("060.030.040.000", actividadCientificaTecnologica.SincroRedesCooperacion(secciones != null && secciones.Contains("060.030.040.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("060.030.050.000", actividadCientificaTecnologica.SincroPremiosMenciones(secciones != null && secciones.Contains("060.030.050.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("060.030.060.000", actividadCientificaTecnologica.SincroOtrasDistinciones(secciones != null && secciones.Contains("060.030.060.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("060.030.070.000", actividadCientificaTecnologica.SincroPeriodosActividad(secciones != null && secciones.Contains("060.030.070.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("060.030.090.000", actividadCientificaTecnologica.SincroAcreditacionesObtenidas(secciones != null && secciones.Contains("060.030.090.000"), preimportar)));
            listadoSecciones.Add(new Subseccion("060.030.100.000", actividadCientificaTecnologica.SincroResumenOtrosMeritos(secciones != null && secciones.Contains("060.030.100.000"), preimportar)));

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
            listadoSecciones.Add(new Subseccion("070.010.000.000", textoLibre.SincroTextoLibre(secciones != null && secciones.Contains("070.010.000.000"), preimportar)));

            return listadoSecciones;
        }
    }
}
