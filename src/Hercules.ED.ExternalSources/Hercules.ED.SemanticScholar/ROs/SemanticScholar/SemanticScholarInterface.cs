using SemanticScholarConnect.ROs.SemanticScholar.Models;

namespace SemanticScholarConnect.ROs.SemanticScholar
{
   interface SemanticScholarInterface
    {
        Publication getPublications(string doi, string uri);

    }
}