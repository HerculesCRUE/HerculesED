using System.Collections.Generic;
using ScopusConnect.ROs.Scopus.Models;
using Newtonsoft.Json.Linq;
using System;


namespace ScopusConnect.ROs.Scopus.Controllers
{
    public class ROScopusControllerJSON
    {
        public string getInfoAuthor(JObject json)
        {
            string sol = "{";
            Console.WriteLine(json["author-retrieval-response"][0]["@status"]);
            if (String.Equals((string)json["author-retrieval-response"][0]["@status"], "found"))
            {
                string b = json["author-retrieval-response"][0]["subject-areas"].ToString();
                sol = sol + "\"subject-areas\":" + b + ",";
                sol = sol + "\"hasPosition\":" + (string)json["author-retrieval-response"][0]["author-profile"]["affiliation-current"].ToString() + ",";
                sol = sol + "\"status\":\"" + (string)json["author-retrieval-response"][0]["author-profile"]["status"].ToString() + "\",";
                sol = sol + "\"date-created\":" + (string)json["author-retrieval-response"][0]["author-profile"]["date-created"].ToString() + ",";
                sol = sol + "\"preferred-name\":" + (string)json["author-retrieval-response"][0]["author-profile"]["preferred-name"].ToString() + ",";
                sol = sol + "\"publication-range\":" + (string)json["author-retrieval-response"][0]["author-profile"]["publication-range"].ToString() + ",";
                sol = sol + "\"coredata\":" + (string) json["author-retrieval-response"][0]["coredata"].ToString();


                //sol = sol + "name-variant: [" + (string)json["author-retrieval-response"][0]["author-profile"]["name-variant"][0].ToString() + "],";
                //sol =sol + "ID:" + (Dictionary<string: string>) json["author-retrieval-response"][0]["coredata"]["dc:identifier"][0].ToString() + ";";
                //sol= sol + "eid:" + (string) json["author-retrieval-response"]["coredata"]["eid"].ToString()+";";
                //sol= sol + "document-count:" + (string) json["author-retrieval-response"]["coredata"]["document-count"].ToString()+";";
                //sol= sol + "cited-by-count:" + (string) json["author-retrieval-response"]["coredata"]["cited-by-count"].ToString()+";";
                //sol= sol + "citation-count:" + (string) json["author-retrieval-response"]["coredata"]["citation-count"].ToString()+";";



                //Console.Write(property);

                // sol[property..Name] = property.Value;        }

                //Console.WriteLine( + " - " + property.Value);

                //

                sol = sol + "}";
            }
            else
            {
                Console.Write("Author not found!");
            }

           return sol;
        }


    }
}