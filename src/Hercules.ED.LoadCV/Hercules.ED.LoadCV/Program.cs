using Gnoss.ApiWrapper;
using Hercules.ED.LoadCV.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Hercules.ED.LoadCV
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Selecciona acción");
                Console.WriteLine("1. Opción 1 (Carga de ORCID de CVs)");
                Console.WriteLine("2. Opción 2 (Carga de CVs)");

                string? opcion = Console.ReadLine();
                if (string.IsNullOrEmpty(opcion) || opcion != "1" && opcion != "2") 
                {
                    Console.WriteLine("El valor introducido no es valido");
                    continue;
                }

                //1. Cargar ORCIDs de CVs
                if (opcion == "1")
                {
                    CargaCV.CargaORCID(new ConfigService());
                    continue;
                }

                //2. Cargar CVs
                if (opcion=="2")
                {
                    CargaCV.CargarCV(new ConfigService());
                    continue;
                }
            }
        }
    }
}