using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Gnoss.ApiWrapper.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesnormalizadorHercules.Models
{
    public class ActualizadorBase
    {
        private string mRutaOauth = $@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/OAuthV3.config";
        protected ResourceApi mResourceApi;

        public ActualizadorBase()
        {
            mResourceApi = new ResourceApi(mRutaOauth);
        }

        public void ActualizadorTriple(string pSujeto, string pPredicado, string pValorAntiguo, string pValorNuevo)
        {
            Guid guid = mResourceApi.GetShortGuid(pSujeto);

            if (!string.IsNullOrEmpty(pValorAntiguo) && !string.IsNullOrEmpty(pValorNuevo))
            {
                //Si el valor nuevo y el viejo no son nulos -->modificamos
                TriplesToModify t = new TriplesToModify();
                t.NewValue = pValorNuevo;
                t.OldValue = pValorAntiguo;
                t.Predicate = pPredicado;
                var resultado = mResourceApi.ModifyPropertiesLoadedResources(new Dictionary<Guid, List<Gnoss.ApiWrapper.Model.TriplesToModify>>() { { guid, new List<Gnoss.ApiWrapper.Model.TriplesToModify>() { t } } });
            }
            else if (string.IsNullOrEmpty(pValorAntiguo) && !string.IsNullOrEmpty(pValorNuevo))
            {
                //Si el valor nuevo no es nulo y viejo si es nulo -->insertamos
                TriplesToInclude t = new();
                t.Predicate = pPredicado;
                t.NewValue = pValorNuevo;
                var resultado = mResourceApi.InsertPropertiesLoadedResources(new Dictionary<Guid, List<Gnoss.ApiWrapper.Model.TriplesToInclude>>() { { guid, new List<Gnoss.ApiWrapper.Model.TriplesToInclude>() { t } } });
            }
            else if (!string.IsNullOrEmpty(pValorAntiguo) && string.IsNullOrEmpty(pValorNuevo))
            {
                //Si el valor nuevo es nulo y viejo si no es nulo -->eliminamos
                RemoveTriples t = new();
                t.Predicate = pPredicado;
                t.Value = pValorAntiguo;
                var resultado = mResourceApi.DeletePropertiesLoadedResources(new Dictionary<Guid, List<Gnoss.ApiWrapper.Model.RemoveTriples>>() { { guid, new List<Gnoss.ApiWrapper.Model.RemoveTriples>() { t } } });
            }
        }

        public void EliminarDuplicados(string pGraph,string pRdfType, string pProperty)
        {
            while (true)
            {            
                int limit = 500;
                String select = @"select ?id count(?data) ";
                String where = @$"where
                                {{
                                    ?id a <{pRdfType}>.
                                    ?id <{pProperty}> ?data. 
                                }}group by (?id) HAVING (COUNT(?data) > 1) limit {limit}";
                SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, pGraph);

                foreach(Dictionary<string,SparqlObject.Data> fila in resultado.results.bindings)
                {
                    string id = fila["id"].value;
                    String select2 = @"select ?data ";
                    String where2 = @$"where
                                {{
                                    <{id}> <{pProperty}> ?data. 
                                }}";
                    SparqlObject resultado2 = mResourceApi.VirtuosoQuery(select2, where2, pGraph);
                    foreach(Dictionary<string, SparqlObject.Data> fila2 in resultado2.results.bindings.GetRange(1, resultado2.results.bindings.Count-1))
                    {
                        string value = fila2["data"].value;
                        ActualizadorTriple(id, pProperty, value, "");
                    }
                }
                if (resultado.results.bindings.Count() != limit)
                {
                    break;
                }
            }
        }
    }
}
