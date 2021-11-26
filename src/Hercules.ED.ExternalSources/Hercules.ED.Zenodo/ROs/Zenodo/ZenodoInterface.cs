using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using ZenodoConnect.ROs.Zenodo.Models;
using ZenodoConnect.ROs.Zenodo.Models.Inicial;

namespace ZenodoConnect.ROs.Zenodo
{
   interface ZenodoInterface
    {
        string getPublications(string userId, string uri);

    }
}