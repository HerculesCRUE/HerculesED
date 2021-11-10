using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using OpenCitationsConnect.ROs.OpenCitations.Models;
using OpenCitationsConnect.ROs.OpenCitations.Models.Inicial;

namespace OpenCitationsConnect.ROs.OpenCitations
{
   interface OpenCitationsInterface
    {
        Publication getPublications(string doi);//, string uri);

    }
}