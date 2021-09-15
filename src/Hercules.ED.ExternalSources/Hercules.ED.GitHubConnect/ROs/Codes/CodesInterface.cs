using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using GitHubAPI.ROs.Codes.Models;


namespace GitHubAPI.ROs.Codes
{
    interface CodesInterface
    {
        List<repositorio_roh> getAllRepositories(string userId, string uri);

        //Repository getRepository(string userId, string repositoryId, string uri, string method);

        //List<Branch> getRepositoryBranches(string userId, string repositoryId, string uri, string method);

        //List<Language> getRepositoryLanguages(string userId, string repositoryId, string uri, string method);

        //List<Commit> getRepositoryCommits(string userId, string repositoryId, string uri, string method);

        //List<Issue> getRepositoryIssues(string userId, string repositoryId, string uri, string method);

        //List<Fork> getRepositoryForks(string userId, string repositoryId, string uri, string method );

        //License getRepositoryLicense(string userId, string repositoryId, string uri, string method);

        //List<User> getRepositoryContributors(string userId, string repositoryId, string uri, string method);

        //List<Repository> getRepositoryRelations(string userId, string repositoryId, string uri, string method);

        //FileFolder getRepositoryFile(string userId, string repositoryId, string file, string uri, string method);

        //List<FileFolder> getRepositoryDir(string userId, string repositoryId, string route, string uri, string method);

        //List<string> getRepositoryTopics(string userId, string repositoryId, string uri, string method);

        //List<Tags> getRepositoryTags(string userId, string repositoryId, string uri, string method);

    }
}