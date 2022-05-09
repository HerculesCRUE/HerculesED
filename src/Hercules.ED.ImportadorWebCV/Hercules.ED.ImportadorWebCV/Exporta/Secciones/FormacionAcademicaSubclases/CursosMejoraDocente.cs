using ImportadorWebCV;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ImportadorWebCV.Exporta.Secciones.FormacionAcademicaSubclases
{
    public class CursosMejoraDocente:SeccionBase
    {
        public CursosMejoraDocente(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {

        }

        public void ExportaCursosMejoraDocente(Entity entity, string seccion, [Optional] List<string> secciones, [Optional] bool preimportar)
        { }

        }
}
