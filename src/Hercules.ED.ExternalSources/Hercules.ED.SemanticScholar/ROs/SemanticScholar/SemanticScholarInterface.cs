using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using SemanticScholarConnect.ROs.SemanticScholar.Models;
using SemanticScholarConnect.ROs.SemanticScholar.Models.Inicial;

namespace SemanticScholarConnect.ROs.SemanticScholar
{
   interface SemanticScholarInterface
    {
        Publication getPublications(string doi, string uri);

    }
}