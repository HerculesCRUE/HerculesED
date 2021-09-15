using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using GitHubAPI.ROs.Codes;
using GitHubAPI.ROs.Codes.Models;
using GitHubAPI.ROs.Codes.Models.Inicial;
using System.Web;
using System.Text.Json;

namespace GitHubAPI.ROs.Codes.Controllers
{

    public class ROCodeLogic : CodesInterface
    {

        protected string bareer;
        protected string baseUri { get; set; }
        //protected List<Repository> respositories = new List<Repository>();
        protected Dictionary<string, string> headers = new Dictionary<string, string>();


        public ROCodeLogic(string baseUri, string bareer)
        {
            this.baseUri = baseUri;
            this.bareer = bareer;
        }


        /// <summary>
        /// A Http calls function
        /// </summary>
        /// <param name="url">the http call url</param>
        /// <param name="method">Crud method for the call</param>
        /// <param name="headers">The headers for the call</param>
        /// <returns></returns>
        protected async Task<string> httpCall(string url, string method = "GET", Dictionary<string, string> headers = null)
        {
            //Console.Write(url);
            //   Console.Write("--------------");
            HttpResponseMessage response;
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod(method), url))
                {
                    request.Headers.TryAddWithoutValidation("Authorization", "token " + bareer);
                    request.Headers.TryAddWithoutValidation("Connection", "keep-alive");
                    request.Headers.TryAddWithoutValidation("Accept", "application/json");

                    if (headers != null && headers.Count > 0)
                    {
                        // if (headers.ContainsKey("Authorization"))
                        // {
                        //     request.Headers.TryAddWithoutValidation("Authorization", headers["Authorization"]);
                        // }
                        foreach (var item in headers)
                        {
                            request.Headers.TryAddWithoutValidation(item.Key, item.Value);
                        }
                    }
                    try
                    {
                        response = await httpClient.SendAsync(request);
                    }
                    catch (System.Exception)
                    {
                        throw new Exception("Error in the http call");
                    }
                }
            }
            if (response.Content != null)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                return "";
            }
        }


        /// <summary>
        /// Main function from get all repositories from the RO account
        /// </summary>
        /// <param name="userId">The user of the repositories</param>
        /// <param name="uri">The uri for the call</param>
        /// <returns></returns>
        public List<repositorio_roh> getAllRepositories(string userId, string uri = "/users/{0}/repos")
        {
            Uri url = new Uri(baseUri + string.Format(uri, userId));
            string repos = httpCall(url.ToString(), "GET", headers).Result;

            List<repositorio_roh> sol = new List<repositorio_roh>();
            List<Repository_inicial> respositories = new List<Repository_inicial>();
            try
            {
                respositories = JsonConvert.DeserializeObject<List<Repository_inicial>>(repos);
            }
            catch (System.Exception)
            {
                throw new Exception("Error when deserialize the respositories: " + repos);
            }
            // Get all data from each repository
            for (int i = 0; i < respositories.Count; i++)
            {
                repositorio_roh repo = new repositorio_roh();
                string userlogin = respositories[i].owner.login;
                string repositoryId = respositories[i].name;
                //----------------------Licencia----------------------------------------
                repo.hasLicense = getRepositoryLicense(userlogin, repositoryId);
                //-----------------------lenguajes -------------------------------------
                repo.language = getRepositoryLanguages(userlogin, repositoryId);
                //---------------------------Topics-----------------------------------
                repo.freetextKeyword = getRepositoryTopics(userlogin, repositoryId);
                //---------------------------Status-------------------------------------
                repo.repositoryStatus = getStatusRepository(respositories[i].@private);
                //-----------date time ---------------------------------------------------------
                repo.dataIssued = getDateIssued(respositories[i]);
                //------------title----------------------------------------------------------------
                repo.title = respositories[i].name;
                //----------------description----------------------------------------------
                repo.description = "GitHub_id:" + respositories[i].id;
                //------------------- autor principal -----------------------------------------
                repo.correspondingAuthor = getAuthorPrincipal(respositories[i]);
                //---------------------------Contributors----------------------------------
                repo.seqOfAuthors = getRepositoryContributors(userlogin, repositoryId);
                //-----------------------README----------------------
                repo.hasReadme = getRepositoryFile(userlogin, repositoryId, "README.md");
                //-------------------Issues-------------------------------------------------
                repo.infoIssues = getRepositoryIssues(userlogin, repositoryId);
                //----------------------Commits-------------------------------------------------
                repo.commit = getRepositoryCommits(userlogin, repositoryId);
                //------------------------------fileFolkder-----------------------------------
                repo.fileFolder = getRepositoryDir(userlogin, repositoryId, "");
                //---------------------------Tags---------------------------------------------
                repo.tags = getRepositoryTags(userlogin, repositoryId);
                //-----------------------------------Fork------------------------------------
                repo.infoforks=getInfoFork(respositories[i]);

                sol.Add(repo);
            }
            return sol;
            //respositories[i].branches = getRepositoryBranches(userIdBranches, repositoryId);
            //respositories[i].issues = getRepositoryIssues(userIdBranches, repositoryId);
            //respositories[i].listForks = getRepositoryForks(userIdBranches, repositoryId);
        }
      /// <summary>
        /// Get a specified repository RO
        /// </summary>
        /// <param Repository_inicial="repository">Repository returned by GitHub </param>
        /// <returns></returns>
        public DateTimeValue getDateIssued(Repository_inicial respository)
        {
                DateTimeValue dateTime = new DateTimeValue();
                dateTime.datimeTime = respository.created_at.Date.ToString();
                return dateTime;
        }
         /// <summary>
        /// Get a specified repository RO
        /// </summary>
        /// <param Repository_inicial="repository">Repository returned by GitHub </param>
        /// <returns></returns>
        public Person getAuthorPrincipal(Repository_inicial respository)
        {
            Person autor_principal = new Person();

                autor_principal.identifier = "GitHub_id: " + respository.owner.login;
                List<Url> links = new List<Url>();
                Url link = new Url();
                link.link = respository.owner.avatar_url;
                links.Add(link);
                autor_principal.link = links;
                return autor_principal;

        }

        /// <summary>
        /// Get a specified repository RO
        /// </summary>
        /// <param Repository_inicial="repository">Repository returned by GitHub </param>
        /// <returns></returns>
        public InfoForks getInfoFork(Repository_inicial respository)
        {
            InfoForks infoForks = new InfoForks();
            infoForks.nForks= respository.forks_count;
            infoForks.isFork=respository.fork;
            return infoForks;

        }
        /// <summary>
        /// Get a specified repository RO
        /// </summary>
        /// <param name="privatee">Private or public repository</param>
        /// <returns></returns>
        public Status getStatusRepository(Boolean privatee)
        {
            Status status = new Status();
            if (privatee == false)
            {
                status.typeStatus = "Closed";
                return status;
            }
            else
            {
                status.typeStatus = "Open";
                return status;
            }
        }

        /// <summary>
        /// Get a specified repository RO
        /// </summary>
        /// <param name="userId">The user of the repositories</param>
        /// <param name="repositoryId">Token for the user.</param>
        /// <param name="uri">The uri for the call</param>
        /// <param name="method">Crud method for the call</param>
        /// <returns></returns>
        public Repository_inicial getRepository(string userId, string repositoryId, string uri = "/repos/{0}/{1}", string method = "GET")
        {
            var repository = new Repository_inicial();
            Uri url = new Uri(baseUri + string.Format(uri, userId, repositoryId));
            string repos = httpCall(url.ToString(), method, headers).Result;
            try
            {
                repository = JsonConvert.DeserializeObject<Repository_inicial>(repos);
            }
            catch (System.Exception)
            {
                throw new Exception("Error when deserialize the respository " + repos);
            }
            return repository;
        }

        /// <summary>
        /// Get all branches from a repository
        /// </summary>
        /// <param name="userId">The user of the repositories</param>
        /// <param name="repositoryId">Token for the user.</param>
        /// <param name="uri">The uri for the call</param>
        /// <param name="method">Crud method for the call</param>
        /// <returns></returns>
      /*   public List<Branch> getRepositoryBranches(string userId, string repositoryId, string uri = "/repos/{0}/{1}/branches", string method = "GET")
        {
            Uri url;
            List<Branch> branches = new List<Branch>();
            string branchesJson;
            url = new Uri(baseUri + string.Format(uri, userId, repositoryId));
            // Http GET call
            branchesJson = httpCall(url.ToString(), method, headers).Result;
            try
            {
                branches = JsonConvert.DeserializeObject<List<Branch>>(branchesJson);
            }
            catch (System.Exception)
            {
                throw new Exception("Error when deserialize branches " + branchesJson);
            }
            return branches;
        } */


        /// <summary>
        /// Get all code languages of a repository
        /// </summary>
        /// <param name="userId">The user of the repositories</param>
        /// <param name="repositoryId">Token for the user.</param>
        /// <param name="uri">The uri for the call</param>
        /// <param name="method">Crud method for the call</param>
        /// <returns></returns>
        public List<String> getRepositoryLanguages(string userId, string repositoryId, string uri = "/repos/{0}/{1}/languages", string method = "GET")
        {
            List<string> sol = new List<string>();
            Uri url = new Uri(baseUri + string.Format("/repos/{0}/{1}/languages", userId, repositoryId));
            string langs = httpCall(url.ToString(), method, headers).Result;
            sol = iteratorLanguagesJson(langs);
            return sol;
        }

        /// <summary>
        /// Get all commit of a repository
        /// </summary>
        /// <param name="userId">The user of the repositories</param>
        /// <param name="repositoryId">Token for the user.</param>
        /// <param name="uri">The uri for the call</param>
        /// <param name="method">Crud method for the call</param>
        /// <returns></returns>
        public InfoCommits getRepositoryCommits(string userId, string repositoryId, string uri = "/repos/{0}/{1}/commits", string method = "GET")
        {

            InfoCommits infoCommits = new InfoCommits();
            List<Commit> commits = new List<Commit>();

            Uri url = new Uri(baseUri + string.Format(uri, userId, repositoryId));

            // Http GET call
            string commitsJson = httpCall(url.ToString(), method, headers).Result;
            string date = null;
            try
            {
                commits = JsonConvert.DeserializeObject<List<Commit>>(commitsJson);
                infoCommits.nCommit = commits.Count.ToString();
                foreach (Commit com in commits)
                {
                    if (date == null)
                    {
                        date = com.commit.author.date;
                    }
                    else
                    {
                        DateTime dateCommit = Convert.ToDateTime(com.commit.author.date);
                        DateTime dateLasCommit = Convert.ToDateTime(date);
                        int result = DateTime.Compare(dateLasCommit, dateCommit);
                        if (result < 0)
                        {
                            date = com.commit.author.date;
                        }
                    }
                }
                DateTimeValue lastCommitDate = new DateTimeValue();
                lastCommitDate.datimeTime = date;
                infoCommits.lastCommit = lastCommitDate;
            }
            catch (System.Exception)
            {
                throw new Exception("Error when deserialize commits " + commitsJson);
            }
            return infoCommits;
        }

        /// <summary>
        /// Get the basic information about the issues of a repository
        /// </summary>
        /// <param name="userId">The user of the repositories</param>
        /// <param name="repositoryId">Token for the user.</param>
        /// <param name="uri">The uri for the call</param>
        /// <param name="method">Crud method for the call</param>
        /// <returns></returns>
        public InfoIssues getRepositoryIssues(string userId, string repositoryId, string uri = "/repos/{0}/{1}/issues", string method = "GET")
        {
            List<Issue> issues = new List<Issue>();
            InfoIssues infoIssues = new InfoIssues();
            List<IssueFinal> Issuesfinal = new List<IssueFinal>();
            Uri url = new Uri(baseUri + string.Format(uri, userId, repositoryId));

            // Http GET call
            string resultJson = httpCall(url.ToString(), method, headers).Result;
            int nIssuesOpen = 0;

            try
            {
                issues = JsonConvert.DeserializeObject<List<Issue>>(resultJson);
                string date = null;

                foreach (Issue issue in issues)
                {
                    IssueFinal issueFinal = new IssueFinal();
                    if (issue.state == "open")
                    {
                        nIssuesOpen = nIssuesOpen + 1;
                        issueFinal.open = true;
                        issueFinal.dateClosed = null;
                    }
                    else
                    {
                        issueFinal.open = false;
                        DateTimeValue dateClosed = new DateTimeValue();
                        dateClosed.datimeTime = issue.closed_at.ToString();
                        issueFinal.dateClosed = dateClosed;
                    }
                    issueFinal.title = issue.title;
                    //------------------------------ links
                    List<Url> links = new List<Url>();
                    Url url_1 = new Url();
                    url_1.link = issue.html_url;
                    links.Add(url_1);
                    issueFinal.links = links;
                    //------------------------------ date created
                    DateTimeValue dateOpen = new DateTimeValue();
                    dateOpen.datimeTime = issue.created_at.ToString();
                    issueFinal.dateIssued = dateOpen;
                    if (date == null)
                    {
                        date = issue.created_at.ToString();
                    }
                    else
                    {
                        DateTime dateCreate = Convert.ToDateTime(issue.created_at.ToString());
                        DateTime dateLastIssue = Convert.ToDateTime(date);
                        int result = DateTime.Compare(dateLastIssue, dateCreate);
                        if (result < 0)
                        {
                            date = issue.created_at.ToString();
                        }
                    }
                    Issuesfinal.Add(issueFinal);
                }
                infoIssues.nIssuesOpen = nIssuesOpen;
                infoIssues.nIssuesClosed = issues.Count - nIssuesOpen;
                infoIssues.nIssues = issues.Count;
                infoIssues.Issues = Issuesfinal;
                DateTimeValue dateCreadLast = new DateTimeValue();
                dateCreadLast.datimeTime = date;
                infoIssues.lastIssuedOpen = dateCreadLast;
            }
            catch (System.Exception)
            {
                throw new Exception("Error while deserialize Issues " + resultJson);
            }
            return infoIssues;

        }

        /// <summary>
        /// Get all forks of a repository
        /// </summary>
        /// <param name="userId">The user of the repositories</param>
        /// <param name="repositoryId">Token for the user.</param>
        /// <param name="uri">The uri for the call</param>
        /// <param name="method">Crud method for the call</param>
        /// <returns></returns>
        /* public List<Fork> getRepositoryForks(string userId, string repositoryId, string uri = "/repos/{0}/{1}/forks", string method = "GET")
        {
            Uri url;
            List<Fork> forks = new List<Fork>();
            string resultJson;
            url = new Uri(baseUri + string.Format(uri, userId, repositoryId));
            // Http GET call
            resultJson = httpCall(url.ToString(), method, headers).Result;
            try
            {
                forks = JsonConvert.DeserializeObject<List<Fork>>(resultJson);
            }
            catch (System.Exception)
            {
                throw new Exception("Error while deserialize the Forks " + resultJson);
            }  
            return forks;
        } */

        /// <summary>
        /// Get basic information about the licenses of a repository
        /// </summary>
        /// <param name="userId">The user of the repositories</param>
        /// <param name="repositoryId">Token for the user.</param>
        /// <param name="uri">The uri for the call</param>
        /// <param name="method">Crud method for the call</param>
        /// <returns></returns>
        public vivoLicense getRepositoryLicense(string userId, string repositoryId, string uri = "/repos/{0}/{1}/license", string method = "GET")
        {

            FullLicense license = new FullLicense();
            Uri url = new Uri(baseUri + string.Format(uri, userId, repositoryId));
            string resultJson = httpCall(url.ToString(), method, headers).Result;
            try
            {
                license = JsonConvert.DeserializeObject<FullLicense>(resultJson);
                vivoLicense licencia_roh = new vivoLicense();
                //------------------------ rellenar los datos de licencia! 
                // ---- name! 
                List<string> names = new List<string>();
                names.Add(license.name);
                names.Add(license.license.name);
                licencia_roh.title = names;
                //-------------links
                List<Url> links = new List<Url>();
                if (license.git_url != null)
                {
                    Url link_1 = new Url();
                    link_1.link = license.git_url;
                    links.Add(link_1);
                }
                if (license.html_url != null)
                {
                    Url link_2 = new Url();
                    link_2.link = license.html_url;
                    links.Add(link_2);
                }
                if (license.url != null)
                {
                    Url link_3 = new Url();
                    link_3.link = license.url;
                    links.Add(link_3);
                }
                if (license.license.url != null)
                {
                    Url link_4 = new Url();
                    link_4.link = license.license.url;
                    links.Add(link_4);
                }
                licencia_roh.url = links;
                //TODO rellenar mas... (no se como...)
                return licencia_roh;
            }
            catch (System.Exception)
            {
                //Aqui una extepcion asi me parece demasiado.... salta si el repo no tiene licencia... ya que
                // en este caos al estructura deñ json devuelto no es la de FullLicense. 
                //throw new Exception("Error while deserialize the licence " + resultJson);
                vivoLicense licencia_roh = null;

                return licencia_roh;
            }
        }

        /// <summary>
        /// Get all contributors of a repository
        /// </summary>
        /// <param name="userId">The user of the repositories</param>
        /// <param name="repositoryId">Token for the user.</param>
        /// <param name="uri">The uri for the call</param>
        /// <param name="method">Crud method for the call</param>
        /// <returns></returns>
        public List<Person> getRepositoryContributors(string userId, string repositoryId, string uri = "/repos/{0}/{1}/contributors", string method = "GET")
        {
            List<Person> contributors = new List<Person>();
            Uri url = new Uri(baseUri + string.Format(uri, userId, repositoryId));
            // Http GET call
            string resultJson = httpCall(url.ToString(), method, headers).Result;
            try
            {
                List<User> autres_inicial = JsonConvert.DeserializeObject<List<User>>(resultJson);
                foreach (User user in autres_inicial)
                {
                    Person person = new Person();
                    //-----------------------------
                    List<string> names = new List<string>();
                    if (user.login != null) { names.Add(user.login); }
                    person.name = names;
                    //--------------------
                    person.identifier = "GitHub_id: " + user.id;
                    //---------------------------------------------
                    List<Url> links = new List<Url>();
                    Url link = new Url();
                    link.link = user.url;
                    links.Add(link);
                    person.link = links;
                    contributors.Add(person);
                }
                return contributors;
            }
            catch (System.Exception)
            {

                return null;
                //throw new Exception("Error while deserialize the licence " + resultJson);
            }
        }

        /// <summary>
        /// Get all repository relations (with other repositories)
        /// </summary>
        /// <param name="userId">The user of the repositories</param>
        /// <param name="repositoryId">Token for the user.</param>
        /// <param name="uri">The uri for the call</param>
        /// <param name="method">Crud method for the call</param>
        /// <returns></returns>
        /*  public List<Repository> getRepositoryRelations(string userId, string repositoryId, string uri = "/repos/{0}/{1}/relations", string method = "GET")
         {
             Uri url;
             List<Repository> relations = new List<Repository>();
             string resultJson;
             url = new Uri(baseUri + string.Format(uri, userId, repositoryId));
             // Http GET call
             resultJson = httpCall(url.ToString(), method, headers).Result;
             try
             {
                 relations = JsonConvert.DeserializeObject<List<Repository>>(resultJson);
             }
             catch (System.Exception)
             {
                 throw new Exception("Error while deserialize the licence " + resultJson);
             }   
             return relations;
         } */


        /// <summary>
        /// Get a specified file data of a Repository
        /// </summary>
        /// <param name="userId">The user of the repositories</param>
        /// <param name="repositoryId">Token for the user.</param>
        /// <param name="file">The file to read.</param>
        /// <param name="uri">The uri for the call</param>
        /// <param name="method">Crud method for the call</param>
        /// <returns></returns>
        public Readmee getRepositoryFile(string userId, string repositoryId, string file, string uri = "/repos/{0}/{1}/contents/{2}", string method = "GET")
        {
            FileFolder fileReadme = new FileFolder();
            Uri url = new Uri(baseUri + string.Format(uri, userId, repositoryId, file));
            // Http GET callProtocols
            string resultJson = httpCall(url.ToString(), method, headers).Result;
            try
            {
                fileReadme = JsonConvert.DeserializeObject<FileFolder>(resultJson);
                Readmee readmee = new Readmee();
                List<Url> links = new List<Url>();
                Url urls = new Url();
                urls.link = fileReadme._links.self;
                links.Add(urls);
                readmee.url = links;
                readmee.typeOfPublication = "README";
                readmee.title = "README.md";
                return readmee;
            }
            catch (System.Exception)
            {
                Readmee readmee = null;
                return readmee;
                // throw new Exception("Error while deserialize the file");
            }
        }

        /// <summary>
        /// Get the folders and files of a dir
        /// </summary>
        /// <param name="userId">The user of the repositories</param>
        /// <param name="repositoryId">Token for the user.</param>
        /// <param name="route">The route of the dir</param>
        /// <param name="uri">The uri for the call</param>
        /// <param name="method">Crud method for the call</param>
        /// <returns></returns>
        public List<FileFolderFinal> getRepositoryDir(string userId, string repositoryId, string route, string uri = "/repos/{0}/{1}/contents/{2}", string method = "GET")
        {
            List<FileFolderFinal> result = new List<FileFolderFinal>();
            Uri url;
            List<FileFolder> files_Folders = new List<FileFolder>();
            string resultJson;


            url = new Uri(baseUri + string.Format(uri, userId, repositoryId, route));

            // Http GET call
            resultJson = httpCall(url.ToString(), method, headers).Result;
            try
            {
                files_Folders = JsonConvert.DeserializeObject<List<FileFolder>>(resultJson);
                foreach (FileFolder fileFolder in files_Folders)
                {
                    FileFolderFinal fileFolderFinal = new FileFolderFinal();
                    fileFolderFinal.name = fileFolder.name;
                    fileFolderFinal.path = fileFolder.path;
                    result.Add(fileFolderFinal);
                }
            }
            catch (System.Exception)
            {
                // throw new Exception("Error while deserialize the file");
            }
            return result;
        }

        /// <summary>
        /// Get all repository topics
        /// </summary>
        /// <param name="userId">The user of the repositories</param>
        /// <param name="repositoryId">Token for the user.</param>
        /// <param name="uri">The uri for the call</param>
        /// <param name="method">Crud method for the call</param>
        /// <returns></returns>
        public List<string> getRepositoryTopics(string userId, string repositoryId, string uri = "/repos/{0}/{1}/topics", string method = "GET")
        {
            Uri url;
            List<string> topics = new List<string>();
            string resultJson;
            url = new Uri(baseUri + string.Format(uri, userId, repositoryId));
            // Http GET call
            resultJson = httpCall(url.ToString(), method, headers).Result;
            try
            {
                topics = JsonConvert.DeserializeObject<List<string>>(resultJson);
            }
            catch (System.Exception)
            {
                topics=null;
                // throw new Exception("Error while deserialize the topics");
            }

            return topics;
        }


        /// <summary>
        /// Get all repository tags
        /// </summary>
        /// <param name="userId">The user of the repositories</param>
        /// <param name="repositoryId">Token for the user.</param>
        /// <param name="uri">The uri for the call</param>
        /// <param name="method">Crud method for the call</param>
        /// <returns></returns>
        public List<TagsFinal> getRepositoryTags(string userId, string repositoryId, string uri = "/repos/{0}/{1}/tags", string method = "GET")
        {
            List<TagsFinal> result = new List<TagsFinal>();
            List<Tags> tags = new List<Tags>();
            Uri url = new Uri(baseUri + string.Format(uri, userId, repositoryId));

            // Http GET call
            string resultJson = httpCall(url.ToString(), method, headers).Result;

            try
            {
                tags = JsonConvert.DeserializeObject<List<Tags>>(resultJson);
                foreach (Tags tag in tags)
                {
                    TagsFinal tagFinal = new TagsFinal();
                    tagFinal.name = tag.name;
                    List<Url> links = new List<Url>();
                    Url link_1 = new Url();
                    link_1.link = tag.tarball_url;
                    links.Add(link_1);
                    Url link_2 = new Url();
                    link_2.link = tag.zipball_url;
                    links.Add(link_2);

                    tagFinal.links = links;
                    result.Add(tagFinal);
                }
            }
            catch (System.Exception)
            {

                throw new Exception("Error while deserialize the topics " + resultJson);
            }

            return result;
        }


        /// <summary>
        /// Convert a return structure to a valid object (model)
        /// </summary>
        /// <param name="json">The user of the repositories</param>
        /// <returns></returns>
        protected List<string> iteratorLanguagesJson(string json)
        {

            List<string> languages = new List<string>();

            try
            {
                // Deserialize the json into a JsonDocument object
                using JsonDocument doc = JsonDocument.Parse(json);
                JsonElement root = doc.RootElement;

                // Check if the current element is a object
                if (root.ValueKind.ToString() == "Object")
                {
                    // Get all elements into the iteration
                    using (JsonElement.ObjectEnumerator objectEnumerator = root.EnumerateObject())
                    {
                        var enumerators = objectEnumerator;

                        // Get all key => Value into the json, and add its into the languages list
                        foreach (var item in enumerators)
                        {
                            languages.Add(item.Name.ToString());
                        }
                    }
                }
                // The same that the before situation, but with an array before
                else if (root.ValueKind.ToString() == "Array")
                {
                    using (JsonElement.ArrayEnumerator objectEnumerator = root.EnumerateArray())
                    {
                        var ArrEnumerators = objectEnumerator;
                        var count = objectEnumerator.Count();

                        foreach (var arrItem in ArrEnumerators)
                        {
                            if (arrItem.ValueKind.ToString() == "Object")
                            {
                                using (JsonElement.ObjectEnumerator arrItemEnum = root.EnumerateObject())
                                {
                                    var enumerators = arrItemEnum;

                                    foreach (var item in arrItemEnum)
                                    {

                                        var type = item.Value.ValueKind.ToString();

                                        languages.Add(item.Name.ToString());
                                    }
                                }
                            }
                        }
                    }

                }

            }
            catch (System.Exception)
            {
                throw new Exception("Error when deserialize Languages " + json);
            }

            return languages;

        }

        public JsonResult getRepositoryLastUpdate(string repositoryId)
        {
            return new JsonResult("");
        }


    }
}
