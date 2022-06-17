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
            Dictionary<string, string> dicOpciones = new Dictionary<string, string>();
            List<Tuple<string, string>> filtrador = new List<Tuple<string, string>>();
            foreach (string str in listaId)
            {
                filtrador.Add(new Tuple<string, string>(str.Split("_").First(), str.Split("_").Last()));
            }
            Preimport preimport = new Preimport();

            XmlSerializer serializer = new XmlSerializer(typeof(Preimport));
            using (TextReader reader = new StringReader(filePreimport))
            {
                preimport = (Preimport)serializer.Deserialize(reader);
            }

            List<SubseccionItem> listadoSubsetionItems = new List<SubseccionItem>();
            foreach (Subseccion subseccion in preimport.secciones)
            {
                foreach (SubseccionItem subseccionItem in subseccion.subsecciones)
                {
                    listadoSubsetionItems.Add(subseccionItem);
                }
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
            if (listadoItems.ElementAt(0).Code.Equals("000.020.000.000"))
            {
                listadoItems.RemoveAt(0);
            }
            if (listadoItems.ElementAt(0).Items.Count == 0)
            {
                listadoItems.RemoveAt(0);
            }

            string opcionSeleccionada = "";
            List<CvnItemBean> listadoDuplicar = new List<CvnItemBean>();
            List<CvnItemBean> listadoFusionar = new List<CvnItemBean>();
            List<CvnItemBean> listadoSobrescribir = new List<CvnItemBean>();
            List<string> listadoDuplicarBBDD = new List<string>();
            List<string> listadoFusionarBBDD = new List<string>();
            List<string> listadoSobrescribirBBDD = new List<string>();
            List<string> listadoTextoLibreBBDD = new List<string>();
            int contadorEliminados = 1;

            for (int i = 0; i < listadoItems.Count; i++)
            {
                if (listadoSubsetionItems.ElementAt(i).propiedades.Count == 0)
                {
                    listadoSubsetionItems.RemoveAt(i);
                    i--;
                    contadorEliminados++;
                    continue;
                }
                if (!filtrador.Any(x => x.Item2.Equals((i + contadorEliminados).ToString())))
                {
                    continue;
                }

                if (listadoItems.ElementAt(i).Code.Equals("060.010.060.000"))
                {
                    if (!listadoSobrescribir.Exists(x => x.Code.Equals("060.010.060.000")))
                    {
                        listadoSobrescribir.Add(listadoItems.Last());
                    }
                    if (listadoSubsetionItems.ElementAt(i).idBBDD.StartsWith("http://gnoss.com/items/GeneralQualityIndicatorCV_"))
                    {
                        listadoSobrescribirBBDD.Add(listadoSubsetionItems.ElementAt(i).idBBDD + "@@@so");                        
                    }
                    continue;
                }
                if (i.Equals(listadoItems.Count() - 1))
                {
                    for (int contadorTexto = 0; contadorTexto < 3; contadorTexto++)
                    {
                        if (filtrador.Any(x => x.Item2.Equals((i + contadorEliminados + contadorTexto).ToString())))
                        {
                            if (!listadoSobrescribir.Exists(x => x.Code.Equals("070.010.000.000")))
                            {
                                listadoSobrescribir.Add(listadoItems.Last());
                            }
                            if (listadoSubsetionItems.Last().idBBDD.StartsWith("http://gnoss.com/items/FreeTextSummaryValuesCV_"))
                            {
                                listadoTextoLibreBBDD.Add(listadoSubsetionItems.Last().idBBDD + "@@@" + contadorTexto);
                            }
                        }
                    }

                    continue;
                }



                opcionSeleccionada = "du";
                if (dicOpciones.ContainsKey((i + contadorEliminados).ToString()))
                {
                    opcionSeleccionada = dicOpciones[(i + contadorEliminados).ToString()];
                }
                if (opcionSeleccionada.Equals("du"))
                {
                    listadoDuplicar.Add(listadoItems.ElementAt(i));
                    listadoDuplicarBBDD.Add(listadoSubsetionItems.ElementAt(i).idBBDD + "@@@du");
                }
                if (opcionSeleccionada.Equals("fu"))
                {
                    listadoFusionar.Add(listadoItems.ElementAt(i));
                    listadoFusionarBBDD.Add(listadoSubsetionItems.ElementAt(i).idBBDD + "@@@fu");
                }
                if (opcionSeleccionada.Equals("so"))
                {
                    listadoSobrescribir.Add(listadoItems.ElementAt(i));
                    listadoSobrescribirBBDD.Add(listadoSubsetionItems.ElementAt(i).idBBDD + "@@@so");
                }


            }

            cvnRootResultBean duplicadosResultBean = new cvnRootResultBean() { cvnRootBean = listadoDuplicar.ToArray() };
            cvnRootResultBean fusionResultBean = new cvnRootResultBean() { cvnRootBean = listadoFusionar.ToArray() };
            cvnRootResultBean sobrescribirResultBean = new cvnRootResultBean() { cvnRootBean = listadoSobrescribir.ToArray() };


            //Duplicar
            base.cvn = duplicadosResultBean;
            base.SincroDatosSituacionProfesional(preimportar: false, listadoIdBBDD: listadoDuplicarBBDD);
            base.SincroFormacionAcademica(preimportar: false, listadoIdBBDD: listadoDuplicarBBDD);
            base.SincroActividadDocente(preimportar: false, listadoIdBBDD: listadoDuplicarBBDD);
            base.SincroExperienciaCientificaTecnologica(preimportar: false, listadoIdBBDD: listadoDuplicarBBDD);
            base.SincroActividadCientificaTecnologica(preimportar: false, listadoIdBBDD: listadoDuplicarBBDD);

            //Fusionar
            base.cvn = fusionResultBean;
            base.SincroDatosSituacionProfesional(preimportar: false, listadoIdBBDD: listadoFusionarBBDD);
            base.SincroFormacionAcademica(preimportar: false, listadoIdBBDD: listadoFusionarBBDD);
            base.SincroActividadDocente(preimportar: false, listadoIdBBDD: listadoFusionarBBDD);
            base.SincroExperienciaCientificaTecnologica(preimportar: false, listadoIdBBDD: listadoFusionarBBDD);
            base.SincroActividadCientificaTecnologica(preimportar: false, listadoIdBBDD: listadoFusionarBBDD);

            //Sobrescribir
            base.cvn = sobrescribirResultBean;
            base.SincroDatosIdentificacion(preimportar: false, listadoIdBBDD: listadoSobrescribirBBDD);
            base.SincroDatosSituacionProfesional(preimportar: false, listadoIdBBDD: listadoSobrescribirBBDD);
            base.SincroFormacionAcademica(preimportar: false, listadoIdBBDD: listadoSobrescribirBBDD);
            base.SincroActividadDocente(preimportar: false, listadoIdBBDD: listadoSobrescribirBBDD);
            base.SincroExperienciaCientificaTecnologica(preimportar: false, listadoIdBBDD: listadoSobrescribirBBDD);
            base.SincroActividadCientificaTecnologica(preimportar: false, listadoIdBBDD: listadoSobrescribirBBDD);
            base.SincroTextoLibre(preimportar: false, listadoIdBBDD: listadoTextoLibreBBDD);
        }
    }
}

