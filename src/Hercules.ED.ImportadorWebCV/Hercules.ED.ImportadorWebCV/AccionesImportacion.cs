using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.Model;
using Hercules.ED.ImportadorWebCV.Models;
using ImportadorWebCV.Sincro;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hercules.ED.ImportadorWebCV
{
    public class AccionesImportacion
    {
        private static readonly ResourceApi mResourceApi = new ResourceApi($@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/ConfigOAuth/OAuthV3.config");


        public void ImportacionTriples(SincroDatos sincroDatos, string pCVID, List<string> listaId, List<string> listaOpciones)
        {
            List<Subseccion> listadoSubsecciones = new List<Subseccion>();
            foreach (Subseccion subseccion in sincroDatos.preimport.secciones)
            {
                Subseccion subsec = new Subseccion(subseccion.id);
                foreach (SubseccionItem subseccionItem in subseccion.subsecciones)
                {
                    if (listaId.Contains(subseccionItem.guid))
                    {
                        subsec.subsecciones.Add(subseccionItem);
                    }
                }
                listadoSubsecciones.Add(subsec);
            }
            sincroDatos.preimport = new Preimport(listadoSubsecciones);
            List<TriplesToInclude> triplesToInclude = new List<TriplesToInclude>();
            foreach (Subseccion sub in sincroDatos.preimport.secciones)
            {
                foreach (SubseccionItem subseccionItem in sub.subsecciones)
                {
                    if (!listaId.Contains(subseccionItem.guid))
                    {
                        continue;
                    }

                    //Compruebo el tipo de opcion para tratar
                    //añado en triplestoinclude las propiedades 
                    //inserto/modifico el recurso                        

                    foreach (Entity.Property property in subseccionItem.propiedades)
                    {
                        foreach (string value in property.values)
                        {
                            triplesToInclude.Add(new TriplesToInclude(property.prop, value));
                        }
                    }

                    if (false)
                    {
                        Dictionary<Guid, List<TriplesToInclude>> triplesInclude = new Dictionary<Guid, List<TriplesToInclude>>() { { mResourceApi.GetShortGuid(pCVID), triplesToInclude } };
                        bool b = mResourceApi.InsertPropertiesLoadedResources(triplesInclude)[mResourceApi.GetShortGuid(subseccionItem.idBBDD)];
                    }
                }
            }

            //Utils.UtilityCV.UpdateEntityAux(Guid.NewGuid(), new List<string>(), new List<string>(), new Entity(), new Entity(), mResourceApi);

            List<TriplesToModify> triplesToModify = new List<TriplesToModify>();
            //foreach()
        }
    }
}
