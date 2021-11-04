using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using WoSConnect.ROs.WoS.Models;
using WoSConnect.ROs.WoS.Models.Inicial;

namespace WoSConnect.ROs.WoS
{
   interface WoSInterface
    {
        List<Publication> getPublications(string userId, string uri);

    }
}