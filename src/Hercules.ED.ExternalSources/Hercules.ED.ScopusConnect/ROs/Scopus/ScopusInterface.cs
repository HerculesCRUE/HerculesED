using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using ScopusConnect.ROs.Scopus.Models;

namespace ScopusConnect.ROs.Scopus
{
   interface ScopusInterface
    {
        List<Publication> getPublications(string userId, string year,string count, string uri);

    }
}