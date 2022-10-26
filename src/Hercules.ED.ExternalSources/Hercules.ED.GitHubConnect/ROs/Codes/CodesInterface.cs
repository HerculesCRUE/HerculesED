using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using GitHubAPI.ROs.Codes.Models;


namespace GitHubAPI.ROs.Codes
{
    interface CodesInterface
    {
        List<repositorio_roh> getAllRepositories(string userId, string uri);
    }
}