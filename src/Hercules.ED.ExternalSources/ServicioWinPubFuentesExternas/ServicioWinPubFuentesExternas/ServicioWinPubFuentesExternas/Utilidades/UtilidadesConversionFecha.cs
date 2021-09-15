using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioWinPubFuentesExternas.Utilidades
{
    public static class UtilidadesConversionFecha
    {
        /// <summary>
        /// Convierte una fecha formato(dd/mm/aaaa) o (dd/mm/aaaa hh:mm) a formato Gnoss (yyyyMMddhhmmss)
        /// </summary>
        /// <param name="pFecha"></param>
        /// <returns></returns>
        public static string ConvertirFechaAFormatoGnoss(string pFecha)
        {
            string fechaFormateada = "00000000000000";
            //try
            //{
            if (pFecha.Length == 0)
            {
                return fechaFormateada;
            }

            if (pFecha.Length <= 10)
            {
                //formato dd/mm/aaaa
                fechaFormateada = pFecha.Substring(6, 4) + pFecha.Substring(3, 2) + pFecha.Substring(0, 2).ToString() + "000000";
            }
            else if (pFecha.Substring(2, 1) == "/" || (pFecha.Substring(1, 1) == "/"))
            {
                //formato dd/mm/aaaa hh:mm
                string aaaa = "";
                string MM = "";
                string dd = "";
                string hh = "00";
                string mm = "00";
                string ss = "00";
                bool dia1digito = false;

                if (pFecha.Substring(1, 1) == "/")
                {
                    dia1digito = true;
                    dd = "0" + pFecha.Substring(0, 1);
                }

                if (dia1digito)
                {
                    MM = pFecha.Substring(2, 2);
                    aaaa = pFecha.Substring(5, 4);

                    //si la hora contiene sólo un dígito
                    if (pFecha.Substring(11, 1) == ":")
                    {
                        hh = "0" + pFecha.Substring(10, 1);
                        mm = pFecha.Substring(12, 2);
                        ss = pFecha.Substring(15, 2);
                    }
                    else
                    {
                        hh = pFecha.Substring(10, 2);
                        mm = pFecha.Substring(13, 2);
                        ss = pFecha.Substring(16, 2);
                    }
                }
                else
                {
                    dd = pFecha.Substring(0, 2);
                    MM = pFecha.Substring(3, 2);
                    aaaa = pFecha.Substring(6, 4);

                    //si la hora contiene sólo un dígito
                    if (pFecha.Substring(12, 1) == ":")
                    {
                        hh = "0" + pFecha.Substring(11, 1);
                        mm = pFecha.Substring(13, 2);
                        ss = pFecha.Substring(16, 2);
                    }
                    else
                    {
                        hh = pFecha.Substring(11, 2);
                        mm = pFecha.Substring(14, 2);
                        if (pFecha.Length > 16)
                        {
                            ss = pFecha.Substring(17, 2);
                        }
                    }
                }

                fechaFormateada = aaaa + MM + dd + hh + mm + ss;
            }
            //}
            //catch (Exception exception)
            //{
            //    pResourceAPI.Log.Error("Error al convertir la fecha a formato Gnoss. " + pFecha + " - " + exception.GetBaseException(), "Utilidades", nombreMetodo);
            //}
            return fechaFormateada;
        }

        /// <summary>
        /// Convierte una fecha de formato Gnoss (yyyyMMddhhmmss) a DateTime
        /// </summary>
        /// <param name="pFecha"></param>
        /// <returns></returns>
        public static DateTime ConvertirFechaGnossADatetime(string pFecha)
        {
            DateTime fechaFormateada = new DateTime();
            //try
            //{
            fechaFormateada = new DateTime(Convert.ToInt32(pFecha.Substring(0, 4)), Convert.ToInt32(pFecha.Substring(4, 2)),
               Convert.ToInt32(pFecha.Substring(6, 2)), Convert.ToInt32(pFecha.Substring(8, 2)),
               Convert.ToInt32(pFecha.Substring(10, 2)), Convert.ToInt32(pFecha.Substring(12, 2)));
            //}
            //catch (Exception exception)
            //{
            //    AdveoService.GuardarLog("Error al convertir la fecha a DateTime. " + pFecha + ": " + exception.ToString());
            //}

            return fechaFormateada;
        }


        public static DateTime ConvertirFechaGnossADatetimeUtc(string pFecha)
        {
            DateTime fechaFormateada = new DateTime();
            fechaFormateada = new DateTime(Convert.ToInt32(pFecha.Substring(0, 4)), Convert.ToInt32(pFecha.Substring(4, 2)),
               Convert.ToInt32(pFecha.Substring(6, 2)), Convert.ToInt32(pFecha.Substring(8, 2)),
               Convert.ToInt32(pFecha.Substring(10, 2)), Convert.ToInt32(pFecha.Substring(12, 2)));
            fechaFormateada = DateTime.SpecifyKind(fechaFormateada, DateTimeKind.Utc);

            return fechaFormateada;
        }
        //formato Gnoss (yyyyMMddhhmmss) 
        public static string ConvertirFechaDateTimeAFormatoGnoss(DateTime pFecha)
        {
            string fechaFormateada = pFecha.Year.ToString() + pFecha.Month.ToString().PadLeft(2, '0') + pFecha.Day.ToString().PadLeft(2, '0');
            fechaFormateada += pFecha.Hour.ToString().PadLeft(2, '0') + pFecha.Minute.ToString().PadLeft(2, '0') + pFecha.Second.ToString().PadLeft(2, '0');
            pFecha = DateTime.SpecifyKind(pFecha, DateTimeKind.Utc);
            return fechaFormateada;
        }


        public static string GetFechaActualGNOSS()
        {
            return ConvertirFechaDateTimeAFormatoGnoss(DateTime.UtcNow);
        }
        //Obtiene la fecha actual en formato 2018-03-06T14:19:29+00:00
        public static string GetFechaActualISO8601()
        {
            return DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH\\:mm\\:ss%K");
        }

        //Convierte la fecha introducida en 2018-03-06T14:19:29+00:00
        public static string ConvertirDateTimeAISO8601(DateTime pFecha)
        {
            pFecha = DateTime.SpecifyKind(pFecha, DateTimeKind.Utc);
            DateTimeOffset time = pFecha;
            return time.ToString("yyyy-MM-ddTHH\\:mm\\:ss%K");
        }

        public static DateTime GetDateTimeFromISO8601(string pFecha)
        {
            DateTime oDate = Convert.ToDateTime(pFecha);
            return oDate;
        }

        /// Convierte una fecha formato(dd/mm/aaaa) o (dd/mm/aaaa HH:mm) a DateTime
        public static DateTime ConvertirFechaADateTime(string pFecha)
        {
            DateTime fechaFormateada = new DateTime();
            //try
            //{
            if (pFecha.Length <= 10)
            {
                //formato dd/mm/aaaa
                fechaFormateada = new DateTime(Convert.ToInt32(pFecha.Substring(6, 4)), Convert.ToInt32(pFecha.Substring(3, 2)), Convert.ToInt32(pFecha.Substring(0, 2)));
            }
            else
            {
                fechaFormateada = new DateTime(Convert.ToInt32(pFecha.Substring(0, 4)), Convert.ToInt32(pFecha.Substring(4, 2)),
                   Convert.ToInt32(pFecha.Substring(6, 2)), Convert.ToInt32(pFecha.Substring(8, 2)),
                   Convert.ToInt32(pFecha.Substring(10, 2)), Convert.ToInt32(pFecha.Substring(12, 2)));
            }
            //}
            //catch (Exception exception)
            //{
            //    AdveoService.GuardarLog("Error al convertir la fecha a dateTime. " + pFecha + ": " + exception.ToString());
            //}

            return fechaFormateada;
        }

        public static string RestarMesFechaAnyoMes(string pFecha)
        {
            if (!string.IsNullOrEmpty(pFecha))
            {
                int anyo = int.Parse(pFecha.Substring(0, 4));
                int mes = int.Parse(pFecha.Substring(4, 2));

                mes--;

                if (mes == 0)
                {
                    anyo--;
                    mes = 12;
                }

                string mesTexto = mes.ToString();

                if (mesTexto.Length == 1)
                {
                    mesTexto = "0" + mesTexto;
                }

                pFecha = anyo.ToString() + mesTexto;
            }

            return pFecha;
        }

        public static string RestarSemanaFechaAnyoSemana(string pFecha)
        {
            if (!string.IsNullOrEmpty(pFecha))
            {
                int anyo = int.Parse(pFecha.Substring(0, 4));
                int semana = int.Parse(pFecha.Substring(4, 2));

                semana--;

                if (semana == 0)
                {
                    anyo--;
                    semana = 52;
                }

                string semanaTexto = semana.ToString();

                if (semanaTexto.Length == 1)
                {
                    semanaTexto = "0" + semanaTexto;
                }

                pFecha = anyo.ToString() + semanaTexto;
            }

            return pFecha;
        }

        public static string ConvertirFechaGnossAMonthDate(string pFecha)
        {
            return pFecha.Substring(0, 6);
        }

        public static string RestarMesesFechaAnyoMes(string pFecha, int pNumMeses)
        {
            if (!string.IsNullOrEmpty(pFecha))
            {

                int anyo = int.Parse(pFecha.Substring(0, 4));
                int mes = int.Parse(pFecha.Substring(4, 2));
                int numAnios = pNumMeses / 12;
                int numMesesDiv = pNumMeses % 12;
                anyo = anyo - numAnios;
                for (int i = 0; i < numMesesDiv; i++)
                {
                    mes--;
                    if (mes == 0)
                    {
                        anyo--;
                        mes = 12;
                    }
                }

                string mesTexto = mes.ToString();

                if (mesTexto.Length == 1)
                {
                    mesTexto = "0" + mesTexto;
                }

                pFecha = anyo.ToString() + mesTexto;
            }

            return pFecha;
        }

        public static string ObtenerFechaSemanaFromDateTime(DateTime pFecha)
        {
            return pFecha.Year + System.Globalization.CultureInfo.CurrentUICulture.Calendar.GetWeekOfYear(pFecha, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday).ToString().PadLeft(2, '0');
        }

        public static string GetFechaGnossFromMonthDate(string pMonthDate)
        {
            string fechaFormateada = pMonthDate.Substring(0, 4) + pMonthDate.Substring(4, 2) + "01";
            fechaFormateada += "000000";
            return fechaFormateada;
        }
    }

}
