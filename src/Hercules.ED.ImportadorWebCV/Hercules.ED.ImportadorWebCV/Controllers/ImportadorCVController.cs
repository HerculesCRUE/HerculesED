using Hercules.ED.ImportadorWebCV.Models;
using ImportadorWebCV.Sincro;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Hercules.ED.ImportadorWebCV.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImportadorCVController : ControllerBase
    {
        private readonly ILogger<ImportadorCVController> _logger;
        readonly ConfigService _Configuracion;

        public ImportadorCVController(ILogger<ImportadorCVController> logger, ConfigService pConfig)
        {
            _logger = logger;
            _Configuracion = pConfig;
        }

        /// <summary>
        /// Importa los datos del <paramref name="File"/> al curriculum vitae, que corresponde a <paramref name="pCVID"/>
        /// </summary>
        /// <param name="pCVID">ID curriculum</param>
        /// <param name="File">Archivo en formato PDF o XML</param>
        /// <returns></returns>
        [HttpPost("Importar")]
        public ActionResult Importar([Required] string pCVID, [Required] IFormFile File)
        {
            SincroDatos sincro = new SincroDatos(_Configuracion, pCVID, File);

            sincro.SincroDatosIdentificacion();
            sincro.SincroDatosSituacionProfesional();
            sincro.SincroFormacionAcademica();
            sincro.SincroActividadDocente();
            sincro.SincroExperienciaCientificaTecnologica();
            sincro.SincroActividadCientificaTecnologica();
            sincro.SincroTextoLibre();

            return Ok();
        }

        /// <summary>
        /// Devuelve los datos del <paramref name="File"/> , que corresponden a <paramref name="pCVID"/>,
        /// para elegir cuales importar.
        /// </summary>
        /// <param name="pCVID">ID curriculum</param>
        /// <param name="File">Archivo en formato PDF o XML</param>
        /// <returns>Json con los datos a preimportar</returns>
        [HttpPost("Preimportar")]
        public ActionResult Preimportar([Required] string pCVID, [Required] IFormFile File)
        {
            SincroDatos sincro = new SincroDatos(_Configuracion, pCVID, File);
            Preimport preimportar = new Preimport();

            preimportar.secciones.AddRange(sincro.SincroDatosIdentificacion(true));
            preimportar.secciones.AddRange(sincro.SincroDatosSituacionProfesional(true));
            preimportar.secciones.AddRange(sincro.SincroFormacionAcademica(true));
            preimportar.secciones.AddRange(sincro.SincroActividadDocente(true));
            preimportar.secciones.AddRange(sincro.SincroExperienciaCientificaTecnologica(true));
            preimportar.secciones.AddRange(sincro.SincroActividadCientificaTecnologica(true));
            preimportar.secciones.AddRange(sincro.SincroTextoLibre(true));

            return Ok(preimportar);
        } 
    }
}
