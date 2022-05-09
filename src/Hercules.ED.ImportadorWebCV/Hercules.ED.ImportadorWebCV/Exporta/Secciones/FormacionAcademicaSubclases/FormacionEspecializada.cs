using ImportadorWebCV;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ImportadorWebCV.Exporta.Secciones.FormacionAcademicaSubclases
{
    public class FormacionEspecializada : SeccionBase
    {
        public FormacionEspecializada(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {

        }

        public void ExportaFormacionEspecializada(Entity entity, string seccion, [Optional] List<string> secciones, [Optional] bool preimportar)
        { }

    }
}
