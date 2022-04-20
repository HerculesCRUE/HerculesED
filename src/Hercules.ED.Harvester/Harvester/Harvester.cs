using Harvester.Models;
using Newtonsoft.Json;
using OAI_PMH.Models.SGI.Organization;
using OAI_PMH.Models.SGI.PersonalData;
using OAI_PMH.Models.SGI.Project;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Harvester
{
    public class Harvester
    {
        public IHaversterServices HaversterServices;

        public Harvester(IHaversterServices haversterServices)
        {
            this.HaversterServices = haversterServices;
        }

        public List<string> Harvest(ReadConfig pConfig, string pSet, string pFecha)
        {
            // Obtención de los IDs.
            HashSet<string> listaIdsSinRepetir = new HashSet<string>();

            List<IdentifierOAIPMH> idList = HaversterServices.ListIdentifiers(pFecha, set: pSet);
            foreach (var id in idList)
            {
                listaIdsSinRepetir.Add(id.Identifier);
            }
            List<string> listaIdsOrdenados = listaIdsSinRepetir.ToList();
            listaIdsOrdenados.Sort();

            // Guardado de los IDs.
            File.WriteAllLines(pConfig.GetLogCargas() + $@"/{pSet}/pending/{pSet}_{pFecha.Replace(":", "-")}.txt", listaIdsOrdenados);

            return listaIdsOrdenados;
        }

        public List<string> HarvestPRC(ReadConfig pConfig, string pSet, string pFecha)
        {
            // Obtención de los IDs.
            HashSet<string> listaIdsSinRepetir = new HashSet<string>();

            List<ListRecordsOAIPMH> idList = HaversterServices.ListRecords(pFecha, set: pSet);
            foreach (var id in idList)
            {
                listaIdsSinRepetir.Add(id.Identifier + "||" + id.Estado);
            }
            List<string> listaIdsOrdenados = listaIdsSinRepetir.ToList();
            listaIdsOrdenados.Sort();

            // Guardado de los IDs.
            File.WriteAllLines(pConfig.GetLogCargas() + $@"/{pSet}/pending/{pSet}_{pFecha.Replace(":", "-")}.txt", listaIdsOrdenados);

            return listaIdsOrdenados;
        }
    }
}
