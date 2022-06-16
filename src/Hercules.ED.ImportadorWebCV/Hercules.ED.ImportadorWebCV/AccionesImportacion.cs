using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.Model;
using Hercules.ED.ImportadorWebCV.Controllers;
using Hercules.ED.ImportadorWebCV.Models;
using ImportadorWebCV;
using ImportadorWebCV.Sincro;
using Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Hercules.ED.ImportadorWebCV
{
    public class AccionesImportacion : SincroDatos
    {
        private static readonly ResourceApi mResourceApi = new ResourceApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/ConfigOAuth/OAuthV3.config");

        public AccionesImportacion(ConfigService Configuracion, string cvID, string fileData) : base(Configuracion, cvID, fileData)
        {

        }

        public void ImportacionTriples(string pCVID, string filePreimport, List<string> listaId, List<string> listaOpciones)
        {
            List<Subseccion> listadoSubsecciones = new List<Subseccion>();
            Dictionary<string, string> dicOpciones = new Dictionary<string, string>();
            Dictionary<string, string> filtrador = new Dictionary<string, string>();
            foreach (string str in listaId)
            {
                filtrador.Add(str.Split("_").First(), str.Split("_").Last());
            }
            Preimport preimport = new Preimport();

            XmlSerializer serializer = new XmlSerializer(typeof(Preimport));
            using (TextReader reader = new StringReader(filePreimport))
            {
                preimport = (Preimport)serializer.Deserialize(reader);
            }

            string idOpcion;
            string valueOpcion;
            if (listaOpciones != null && listaOpciones.Count > 0)
            {
                foreach (string opcion in listaOpciones)
                {
                    if (opcion == null)
                    {
                        continue;
                    }
                    idOpcion = opcion.Split("|||").First().Split("_").Last();
                    valueOpcion = opcion.Split("|||").Last();
                    dicOpciones.Add(idOpcion, valueOpcion);
                }
            }

            List<CvnItemBean> listadoItems = base.cvn.cvnRootBean.ToList();
            if (listadoItems.Count == 0 || !listadoItems.ElementAt(0).Code.Equals("000.020.000.000"))
            {
                return;
            }

            string opcionSeleccionada = "";
            List<CvnItemBean> listadoDuplicar = new List<CvnItemBean>();
            List<CvnItemBean> listadoFusionar = new List<CvnItemBean>();
            List<CvnItemBean> listadoSobrescribir = new List<CvnItemBean>();
            List<string> listadoDuplicarBBDD = new List<string>();
            List<string> listadoFusionarBBDD = new List<string>();
            List<string> listadoSobrescribirBBDD = new List<string>();
            for (int i = 1; i < listadoItems.Count; i++)
            {
                if (!filtrador.ContainsValue(i.ToString()))
                {
                    continue;
                }
                opcionSeleccionada = "so";
                if (dicOpciones.ContainsKey(i.ToString()))
                {
                    opcionSeleccionada = dicOpciones[i.ToString()];
                }
                if (opcionSeleccionada.Equals("du"))
                {
                    listadoDuplicar.Add(listadoItems.ElementAt(i));
                    listadoDuplicarBBDD.Add(filtrador.Where(x=>x.Value.Equals(i.ToString())).Select(x=>x.Key).FirstOrDefault() + "_du");
                }
                if (opcionSeleccionada.Equals("fu"))
                {
                    listadoFusionar.Add(listadoItems.ElementAt(i));
                    listadoFusionarBBDD.Add(filtrador.Where(x => x.Value.Equals(i.ToString())).Select(x => x.Key).FirstOrDefault() + "_fu");
                }
                if (opcionSeleccionada.Equals("so"))
                {
                    listadoSobrescribir.Add(listadoItems.ElementAt(i));
                    listadoSobrescribirBBDD.Add(filtrador.Where(x => x.Value.Equals(i.ToString())).Select(x => x.Key).FirstOrDefault() + "_so");
                }
            }

            cvnRootResultBean duplicadosResultBean = new cvnRootResultBean() { cvnRootBean = listadoDuplicar.ToArray() };
            cvnRootResultBean fusionResultBean = new cvnRootResultBean() { cvnRootBean = listadoFusionar.ToArray() };
            cvnRootResultBean sobrescribirResultBean = new cvnRootResultBean() { cvnRootBean = listadoSobrescribir.ToArray() };


            //Duplicar
            base.cvn = duplicadosResultBean;
            base.SincroDatosIdentificacion(preimportar: false);
            base.SincroDatosSituacionProfesional(preimportar: false, listadoIdBBDD: listadoDuplicarBBDD);
            base.SincroFormacionAcademica(preimportar: false, listadoIdBBDD: listadoDuplicarBBDD);
            base.SincroActividadDocente(preimportar: false, listadoIdBBDD: listadoDuplicarBBDD);
            base.SincroExperienciaCientificaTecnologica(preimportar: false, listadoIdBBDD: listadoDuplicarBBDD);
            base.SincroActividadCientificaTecnologica(preimportar: false, listadoIdBBDD: listadoDuplicarBBDD);
            base.SincroTextoLibre(preimportar: false);

            //Fusionar - TODO
            base.cvn = fusionResultBean;

            //Sobrescribir - TODO
            base.cvn = sobrescribirResultBean;
        }
    }
}

