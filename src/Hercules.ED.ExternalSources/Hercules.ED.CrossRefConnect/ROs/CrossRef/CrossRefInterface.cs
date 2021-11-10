using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using CrossRefConnect.ROs.CrossRef.Models;
using CrossRefConnect.ROs.CrossRef.Models.Inicial;

namespace CrossRefConnect.ROs.CrossRef
{
   interface CrossRefInterface
    {
        Publication getPublications(string DOI, Boolean True,  string uri);

    }
}