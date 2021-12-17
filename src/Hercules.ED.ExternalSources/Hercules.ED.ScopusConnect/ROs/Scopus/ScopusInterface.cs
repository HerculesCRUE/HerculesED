using System.Collections.Generic;
using ScopusConnect.ROs.Scopus.Models;
namespace ScopusConnect.ROs.Scopus
{
   interface ScopusInterface
    {
        List<Publication> getPublications(string orcid, string data, string uri);

    }
}