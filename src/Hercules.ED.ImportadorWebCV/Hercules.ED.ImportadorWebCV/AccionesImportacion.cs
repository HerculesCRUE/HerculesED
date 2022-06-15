using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.Model;
using Hercules.ED.ImportadorWebCV.Controllers;
using Hercules.ED.ImportadorWebCV.Models;
using ImportadorWebCV;
using ImportadorWebCV.Sincro;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hercules.ED.ImportadorWebCV
{
    public class AccionesImportacion : SincroDatos
    {
        private static readonly ResourceApi mResourceApi = new ResourceApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/ConfigOAuth/OAuthV3.config");

        public AccionesImportacion(ConfigService Configuracion, string cvID, string fileData) : base(Configuracion, cvID, fileData)
        {

        }

        public void ImportacionTriples(string pCVID, List<string> listaId, List<string> listaOpciones)
        {
            List<Subseccion> listadoSubsecciones = new List<Subseccion>();
            Dictionary<string, string> dicOpciones = new Dictionary<string, string>();

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
                    idOpcion = opcion.Split("|||").First();
                    valueOpcion = opcion.Split("|||").Last();
                    dicOpciones.Add(idOpcion, valueOpcion);
                }
            }

            List<CvnItemBean> listadoItems = base.cvn.cvnRootBean.ToList();
            //foreach (Subseccion subseccion in sincroDatos.preimport.secciones)
            //{
            //    Subseccion subsec = new Subseccion(subseccion.id);
            //    foreach (SubseccionItem subseccionItem in subseccion.subsecciones)
            //    {
            //        if (listaId.Contains(subseccionItem.guid))
            //        {
            //            subsec.subsecciones.Add(subseccionItem);
            //        }
            //    }
            //    listadoSubsecciones.Add(subsec);
            //}
            //sincroDatos.preimport = new Preimport(listadoSubsecciones);
            //List<TriplesToInclude> triplesToInclude = new List<TriplesToInclude>();
            //foreach (Subseccion sub in sincroDatos.preimport.secciones)
            //{
            //    foreach (SubseccionItem subseccionItem in sub.subsecciones)
            //    {
            //        string idBBDD = subseccionItem.idBBDD;
            //        bool isBlocked = subseccionItem.isBlocked;

            //        if (!listaId.Contains(subseccionItem.guid))
            //        {
            //            continue;
            //        }

            //        string opcion = "du";
            //        if (dicOpciones.Keys.Contains(subseccionItem.guid))
            //        {
            //            opcion = dicOpciones[subseccionItem.guid];
            //        }

            //        //Compruebo el tipo de opcion para tratar
            //        switch (opcion)
            //        {
            //            //Fusionar
            //            case "fu":
            //                ///<param name="pIdMainEntity">Identificador de la entidad a la que pertenece la entidad auxiliar</param>
            //                /// <param name="pPropertyIDs">Propiedades que apuntan a la auxiliar</param>
            //                /// <param name="pEntityIDs">Entidades que apuntan a la auxiliar</param>
            //                /// <param name="pLoadedEntity">Entidad cargada en BBDD</param>
            //                /// <param name="pUpdatedEntity">Nueva entidad</param>
            //                //Utils.UtilityCV.UpdateEntityAux(pCVID, listapropiedadesauxiliar, listaentidadesauxiliar, entidadBBDD, nueva);

            //                break;

            //            //Sobrescribir
            //            case "so":
            //                if (isBlocked)
            //                {
            //                    break;
            //                }

            //                break;

            //            //Duplicar
            //            case "du":

            //                break;

            //            default:
            //                break;
            //        }


            //        //añado en triplestoinclude/triplestomodify/triplestodelete las propiedades 
            //        //inserto/modifico el recurso                        

            //        foreach (Entity.Property property in subseccionItem.propiedades)
            //        {
            //            foreach (string value in property.values)
            //            {
            //                triplesToInclude.Add(new TriplesToInclude(property.prop, value));
            //            }
            //        }


            //        {
            //            Dictionary<Guid, List<TriplesToInclude>> triplesInclude = new Dictionary<Guid, List<TriplesToInclude>>() { { mResourceApi.GetShortGuid(pCVID), triplesToInclude } };
            //            bool b = mResourceApi.InsertPropertiesLoadedResources(triplesInclude)[mResourceApi.GetShortGuid(subseccionItem.idBBDD)];



            //            Utils.UtilityCV.UpdateEntityAux(Guid.NewGuid(), new List<string>(), new List<string>(), new Entity(), new Entity(), mResourceApi);
            //        }
            //    }
            //}


        }
    }
}
