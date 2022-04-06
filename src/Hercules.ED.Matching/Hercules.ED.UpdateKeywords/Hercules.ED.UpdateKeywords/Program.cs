using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.ED.UpdateKeywords.Models;
using System;
using System.Collections.Generic;

namespace Hercules.ED.UpdateKeywords
{
    public class Program
    {
        private static string RUTA_OAUTH = $@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/ConfigOAuth/OAuthV3.config";
        private static ResourceApi mResourceApi = new ResourceApi(RUTA_OAUTH);
        private static CommunityApi mCommunityApi = new CommunityApi(RUTA_OAUTH);


        static void Main(string[] args)
        {
            UtilKeywords utilKeywords = new UtilKeywords(mResourceApi, mCommunityApi);

            // Lista de los IDs de los recursos a hacer Matching.
            List<string> listaIds = utilKeywords.GetDocument();

            // Obtención del ID de MESH.
            foreach (string id in listaIds)
            {
                Dictionary<string, string> dicEtiquetas = utilKeywords.GetKeywords(id);

                foreach (KeyValuePair<string, string> etiquetaTag in dicEtiquetas)
                {
                    string idEtiquetaAux = etiquetaTag.Key;
                    Dictionary<string, string> dicResultados = utilKeywords.SelectDataMesh(etiquetaTag.Value);

                    // Obtencón de información de SNOMED.
                    List<Data> listaSnomed = new List<Data>();
                    Dictionary<string, List<Dictionary<string, string>>> dicIds = new Dictionary<string, List<Dictionary<string, string>>>();
                    foreach (KeyValuePair<string, string> item in dicResultados)
                    {
                        utilKeywords.InsertDataSnomed(item.Key, listaSnomed);

                        // Relación IDs.
                        dicIds = new Dictionary<string, List<Dictionary<string, string>>>();
                        dicIds.Add(item.Key, new List<Dictionary<string, string>>());
                        foreach (Data itemSnomed in listaSnomed)
                        {
                            Dictionary<string, string> dicAux = new Dictionary<string, string>();
                            dicAux.Add(itemSnomed.snomedTerm.ui, "");
                            dicIds[item.Key].Add(dicAux);
                        }
                    }

                    // Cambio de modelo con los datos necesarios.                    
                    List<DataConcept> listaSnomedConcepts = utilKeywords.GetDataConcepts(listaSnomed);

                    // Carga de etiquetas.
                    if (listaSnomedConcepts != null)
                    {
                        foreach (DataConcept tag in listaSnomedConcepts)
                        {
                            utilKeywords.CargarDataConceptCompleto(tag, dicIds);
                        }
                    }

                    // Obtención de información de MESH.
                    List<DataConcept> listaMesh = new List<DataConcept>();
                    foreach (KeyValuePair<string, string> itemAux in dicResultados)
                    {
                        utilKeywords.InsertDataMesh(itemAux.Key, itemAux.Value, listaMesh);
                    }

                    // Carga de etiquetas.
                    foreach (DataConcept tag in listaMesh)
                    {
                        string idRecursoMesh = utilKeywords.CargarDataConceptCompleto(tag, dicIds);
                        utilKeywords.ModificarKeyword(idEtiquetaAux, idRecursoMesh);
                    }
                }

                // Borrar triple de obtención de etiquetas.
                utilKeywords.BorrarGetKeywordProperty(id);
            }
        }
    }
}
