using Hercules.ED.ImportExportCV.Models;
using ImportadorWebCV.Sincro;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using Gnoss.ApiWrapper.Model;
using Models;
using ImportadorWebCV.Exporta.Secciones;
using Gnoss.ApiWrapper;
using ImportadorWebCV;
using Utils;

namespace Hercules.ED.ImportExportCV.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImportadorCVController : ControllerBase
    {
        private readonly ILogger<ImportadorCVController> _logger;
        readonly ConfigService _Configuracion;

        private static Dictionary<string, PetitionStatus> petitionStatus = new Dictionary<string, PetitionStatus>();
        public const int elementosTratados = 0;

        public ImportadorCVController(ILogger<ImportadorCVController> logger, ConfigService pConfig)
        {
            System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            _logger = logger;
            _Configuracion = pConfig;
        }

        /// <summary>
        /// Devuelve el ORCID del titular del archivo
        /// </summary>
        /// <param name="File">Archivo en formato pdf</param>
        /// <returns>ORCID del titular del archivo, BadRequest en caso contrario</returns>
        [HttpPost("ObtenerORCID")]
        public ActionResult ObtenerORCID([Required] IFormFile File)
        {
            try
            {
                string crisArchivo = File.FileName.Split(".").First().Substring(0, File.FileName.Split(".").First().Length - 1);

                SincroDatos sincro = new SincroDatos(_Configuracion, File);
                if (sincro.getCVN() == null || sincro.getCVN().numElementos == 0 || sincro.getCVN().errorCode != 0)
                {
                    if (sincro.getCVN() != null)
                    {
                        return BadRequest(sincro.getCVN().errorMessage);
                    }
                    else
                    {
                        return BadRequest();
                    }
                }

                string ORCID = sincro.ObtenerORCID(sincro, crisArchivo);

                return Ok(ORCID);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Importa los datos del <paramref name="File"/> al curriculum vitae, que corresponde a <paramref name="pCVID"/>
        /// </summary>
        /// <param name="pCVID">ID curriculum</param>
        /// <param name="File">Archivo en formato PDF o XML</param>
        /// <param name="Secciones">Listado de secciones a importar</param>
        /// <returns>200Ok si todo ha ido correctamente, 400BadRequest en caso contrario</returns>
        [HttpPost("Importar")]
        public ActionResult Importar([FromForm][Required] string pCVID, [Required] IFormFile File, [FromForm][Optional] List<string> Secciones)
        {
            try
            {

                Utility.updateFechaImportacion(pCVID);
                SincroDatos sincro = new SincroDatos(_Configuracion, pCVID, File);

                List<string> listaDOI = new List<string>();

                sincro.SincroDatosIdentificacion(Secciones);
                sincro.SincroDatosSituacionProfesional(Secciones);
                sincro.SincroFormacionAcademica(Secciones);
                sincro.SincroActividadDocente(Secciones);
                sincro.SincroExperienciaCientificaTecnologica(Secciones);
                sincro.SincroActividadCientificaTecnologica(Secciones, listaDOI: listaDOI);
                sincro.SincroTextoLibre(Secciones);

                sincro.SincroPublicacionesFuenteExternas(pCVID, listaDOI);
                Utility.updateFechaImportacion(pCVID, true);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Devuelve el estado actual de la peticion con identificador <paramref name="petitionID"/>
        /// </summary>
        /// <param name="petitionID">Identificador de la petición</param>
        /// <returns>Estado de la petición</returns>
        [HttpGet("PetitionCVStatus")]
        public IActionResult PetitionCVStatus([Required] string petitionID)
        {
            try
            {
                if (petitionStatus.ContainsKey(petitionID))
                {
                    return Ok(petitionStatus[petitionID]);
                }
                else
                {
                    petitionStatus.TryAdd(petitionID, new PetitionStatus());
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Devuelve los datos del <paramref name="File"/> , que corresponden a <paramref name="pCVID"/>,
        /// para elegir cuales importar.
        /// </summary>
        /// <param name="pCVID">ID curriculum</param>
        /// <param name="File">Archivo en formato PDF o XML</param>
        /// <param name="petitionID">Identificador para la petición de estado</param>
        /// <param name="Secciones">Listado de secciones a importar</param>
        /// <returns>Json con los datos a preimportar</returns>
        [HttpPost("Preimportar")]
        public ActionResult Preimportar([FromForm][Required] string pCVID, [Required] IFormFile File,
            [FromForm][Required] string petitionID, [FromForm][Optional] List<string> Secciones)
        {
            try
            {
                //Estado de la peticion
                PetitionStatus estadoPreimport = new PetitionStatus(0, 0, "ESTADO_PREIMPORTAR_LECTURA");
                petitionStatus[petitionID] = estadoPreimport;

                SincroDatos sincro = new SincroDatos(_Configuracion, pCVID, File);
                Preimport preimportar = new Preimport();

                List<string> listaDOI = new List<string>();

                sincro.ComprobarSecciones();
                petitionStatus[petitionID].totalWorks = sincro.GetNumItems();
                petitionStatus[petitionID].actualWork = 0;
                petitionStatus[petitionID].actualWorkTitle = "ESTADO_PREIMPORTAR_PROCESARDATOS";

                preimportar.secciones.AddRange(sincro.SincroDatosIdentificacion(Secciones, true, petitionStatus: petitionStatus[petitionID]));
                preimportar.secciones.AddRange(sincro.SincroDatosSituacionProfesional(Secciones, true, petitionStatus: petitionStatus[petitionID]));
                preimportar.secciones.AddRange(sincro.SincroFormacionAcademica(Secciones, true, petitionStatus: petitionStatus[petitionID]));
                preimportar.secciones.AddRange(sincro.SincroActividadDocente(Secciones, true, petitionStatus: petitionStatus[petitionID]));
                preimportar.secciones.AddRange(sincro.SincroExperienciaCientificaTecnologica(Secciones, true, petitionStatus: petitionStatus[petitionID]));
                preimportar.secciones.AddRange(sincro.SincroActividadCientificaTecnologica(Secciones, true, petitionStatus: petitionStatus[petitionID], listaDOI: listaDOI));
                preimportar.secciones.AddRange(sincro.SincroTextoLibre(Secciones, true));
                petitionStatus[petitionID].actualWork = petitionStatus[petitionID].totalWorks;

                sincro.GuardarXMLFiltrado();

                string xmlPreimporta = "";
                XmlSerializer serializer = new XmlSerializer(typeof(Preimport));
                using (var sww = new StringWriter())
                {
                    using (XmlWriter writer = XmlWriter.Create(sww))
                    {
                        serializer.Serialize(writer, preimportar);
                        xmlPreimporta = sww.ToString();
                    }
                }
                //Guardo los datos de Preimport
                preimportar.cvn_preimportar = xmlPreimporta;

                //Guardo los datos del fichero XML leido para usarlo despues
                preimportar.cvn_xml = sincro.GuardarXMLFiltrado();

                return Ok(preimportar);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Servicio de PostImportación del CV
        /// </summary>
        /// <param name="pCVID">Identificador del curriculumvitae</param>
        /// <param name="file">Archivo XML</param>
        /// <param name="filePreimport">Preimport del archivo XML tras tratarlo en Preimportar</param>
        /// <param name="petitionID">Identificador para la petición de estado</param>
        /// <param name="listaId">Listado de identificadores de los recursos a añadir</param>
        /// <param name="listaOpciones">Listado de identificadores de los recursos a añadir y las opciones seleccionadas de cada uno, separado por "|||"</param>
        /// <returns>200Ok si todo ha ido correctamente, 400BadRequest en caso contrario</returns>
        [HttpPost("Postimportar")]
        public ActionResult PostImportar([FromForm][Required] string pCVID, [FromForm] byte[] file, [FromForm] string filePreimport,
            [FromForm][Required] string petitionID, [FromForm] List<string> listaId, [FromForm][Optional] List<string> listaOpciones)
        {
            try
            {
                //Estado de la peticion
                Utility.updateFechaImportacion(pCVID);
                PetitionStatus estadoPostimport = new PetitionStatus(0, 0, "ESTADO_POSTIMPORTAR_LECTURA");
                petitionStatus[petitionID] = estadoPostimport;

                string stringFile;
                using (var ms = new MemoryStream(file))
                {
                    using (var reader = new StreamReader(ms))
                    {
                        stringFile = reader.ReadToEnd();
                    }
                }

                AccionesImportacion accionesImportacion = new AccionesImportacion(_Configuracion, pCVID, stringFile);
                accionesImportacion.ImportacionTriples(pCVID, filePreimport, listaId, listaOpciones, petitionStatus[petitionID]);
                Utility.updateFechaImportacion(pCVID,true);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("fechaCheck")]
        public ActionResult fechaCheck([FromForm][Required] string pCVID)
        {
            //Utility.updateFechaImportacion(pCVID);
            bool isToday = Utility.checkFecha(pCVID);
            //Utility.quitarFechaImportacion(pCVID);

            return Ok(isToday);
        }
        [HttpPost("fecha")]
        public ActionResult fecha(string pCVID)
        {
            Utility.updateFechaImportacion(pCVID);
            return Ok();
        }
    }
}
