using HerculesAplicacionConsola.Sincro;
using System;

namespace HerculesAplicacionConsola
{
    class Program
    {
        static void Main(string[] args)
        {
            //string rutaprueba = @"C:\GNOSS\Proyectos\Hercules\prueba.xml";
            string rutaprueba = @"C:\Users\mboillos\Downloads\prueba.xml";

            string cvID = "http://gnoss.com/items/CV_c8ba1c41-1084-46cf-9e30-64ea79c99871_786d74a8-7737-4637-819b-199c432294b6";

            SincroDatos sincro = new SincroDatos(rutaprueba, cvID);

            //sincro.SincroDatosIdentificacion();
            //sincro.SincroDatosSituacionProfesional();
            //sincro.SincroFormacionAcademica();
            //sincro.SincroActividadDocente();
            //sincro.SincroExperienciaCientificaTecnologica();
            sincro.SincroActividadCientificaTecnologica();
            //sincro.SincroTextoLibre();

            Console.WriteLine("");
            Console.WriteLine(" --- END --- ");
        }
    }
}
