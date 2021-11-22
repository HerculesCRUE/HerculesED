using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using PublicationConnect.ROs.Publications.Models;
using PublicationConnect.Controllers;
using PublicationConnect.ROs.Publications.Controllers;
using PublicationConnect.ROs.Publications.Models;

namespace PublicationConnect.ROs.Publications
{
   interface PublicationInterface
    {
        List<Publication> getPublications(string orcid, string date);

    }
}