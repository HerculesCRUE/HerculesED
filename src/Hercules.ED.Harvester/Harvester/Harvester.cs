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
        //Syncs
        private static readonly string Syncs = @"C:\Data\Syncs.json";

        //Last sync date
        private static readonly string LastSyncDate = GetLastSyncDate();
        public IHaversterServices HaversterServices;

        public Harvester(IHaversterServices haversterServices)
        {
            this.HaversterServices = haversterServices;
        }

        /// <summary>
        /// Obtiene los IDs de las Organizaciones.
        /// </summary>
        /// <returns></returns>
        public List<string> HarvestOrganizationsIds(ReadConfig pConfig)
        {
            // Comprobar si existe el fichero de IDs.
            List<string> listaIds = new List<string>();

            if (!File.Exists(pConfig.GetLogIdentifier() + "/Organizaciones.txt"))
            {             
                HashSet<string> listaIdsSinRepetir = new HashSet<string>();
                List<IdentifierOAIPMH> organizationIdList = HaversterServices.ListIdentifiers("2022-01-01T00:00:00Z", set: "Organizacion");
                //List<IdentifierOAIPMH> organizationIdList = HaversterServices.ListIdentifiers(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss") + "Z", set: "Organizacion");
                foreach (var organizationId in organizationIdList)
                {
                    listaIdsSinRepetir.Add(organizationId.Identifier);
                }
                List<string> listaIdsOrdenados = listaIdsSinRepetir.ToList();
                listaIdsOrdenados.Sort();
                listaIds = listaIdsOrdenados;
                File.WriteAllLines(pConfig.GetLogIdentifier() + "/Organizaciones.txt", listaIdsOrdenados);
            }
            else
            {
                string[] lineas = File.ReadAllLines(pConfig.GetLogIdentifier() + "/Organizaciones.txt");
                listaIds = lineas.ToList();
            }

            return listaIds;
        }

        /// <summary>
        /// Obtiene los IDs de las Personas.
        /// </summary>
        /// <returns></returns>
        public List<string> HarvestPersonsIds(ReadConfig pConfig)
        {
            // Comprobar si existe el fichero de IDs.
            List<string> listaIds = new List<string>();

            if (!File.Exists(pConfig.GetLogIdentifier() + "/Personas.txt"))
            {
                HashSet<string> listaIdsSinRepetir = new HashSet<string>();
                List<IdentifierOAIPMH> personIdList = HaversterServices.ListIdentifiers("2022-01-01T00:00:00Z", set: "Persona");
                //List<IdentifierOAIPMH> personIdList = HaversterServices.ListIdentifiers(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss") + "Z", set: "Persona");
                foreach (var personId in personIdList)
                {
                    listaIdsSinRepetir.Add(personId.Identifier);
                }
                List<string> listaIdsOrdenados = listaIdsSinRepetir.ToList();
                listaIdsOrdenados.Sort();
                listaIds = listaIdsOrdenados;
                File.WriteAllLines(pConfig.GetLogIdentifier() + "/Personas.txt", listaIdsOrdenados);
            }
            else
            {
                string[] lineas = File.ReadAllLines(pConfig.GetLogIdentifier() + "/Personas.txt");
                listaIds = lineas.ToList();
            }

            return listaIds;
        }

        /// <summary>
        /// Obtiene los IDs de los Proyectos.
        /// </summary>
        /// <returns></returns>
        public List<string> HarvestProjectsIds(ReadConfig pConfig)
        {
            // Comprobar si existe el fichero de IDs.
            List<string> listaIds = new List<string>();

            if (!File.Exists(pConfig.GetLogIdentifier() + "/Proyectos.txt"))
            {
                HashSet<string> listaIdsSinRepetir = new HashSet<string>();
                List<IdentifierOAIPMH> projectIdList = HaversterServices.ListIdentifiers("2022-01-01T00:00:00Z", set: "Proyecto");
                //List<IdentifierOAIPMH> projectIdList = HaversterServices.ListIdentifiers(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss") + "Z", set: "Proyecto");
                foreach (var personId in projectIdList)
                {
                    listaIdsSinRepetir.Add(personId.Identifier);
                }
                List<string> listaIdsOrdenados = listaIdsSinRepetir.ToList();
                listaIdsOrdenados.Sort();
                listaIds = listaIdsOrdenados;
                File.WriteAllLines(pConfig.GetLogIdentifier() + "/Proyectos.txt", listaIdsOrdenados);
            }
            else
            {
                string[] lineas = File.ReadAllLines(pConfig.GetLogIdentifier() + "/Proyectos.txt");
                listaIds = lineas.ToList();
            }

            return listaIds;
        }

        public static string GetLastSyncDate()
        {
            string syncs = File.ReadAllText(Syncs);
            List<Sync> syncList = JsonConvert.DeserializeObject<List<Sync>>(syncs);
            string lastSyncDate = syncList[^1].Date;
            return lastSyncDate;
        }
    }
}
