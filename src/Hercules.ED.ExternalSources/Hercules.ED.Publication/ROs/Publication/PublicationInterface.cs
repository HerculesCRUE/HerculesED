using System.Collections.Generic;
using PublicationConnect.ROs.Publications.Models;


namespace PublicationConnect.ROs.Publications
{
   interface PublicationInterface
    {
        List<Publication> getPublications(string orcid, string date);

    }
}