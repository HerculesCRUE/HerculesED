using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using ScopusConnect.ROs.Scopus.Models;
using ScopusConnect.ROs.Scopus.Models.Inicial;

namespace ScopusConnect.ROs.Scopus
{
   interface ScopusInterface
    {
        List<Publication> getPublications(string userId, string data, string uri);

    }
}