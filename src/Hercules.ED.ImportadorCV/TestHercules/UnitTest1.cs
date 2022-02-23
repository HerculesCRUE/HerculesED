using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml.Serialization;
using System.IO;
using System.Linq;
using System;
using Utils;
using HerculesAplicacionConsola;
using System.Collections.Generic;
using HerculesAplicacionConsola.Sincro.Secciones;

namespace TestUnitarios
{

    [TestClass]
    public class TestGenericos
    {

        [TestMethod]
        public void TestMethodPathExist()
        {
            Assert.IsTrue(Utility.ExistePath(@"C:\GNOSS\Proyectos\Hercules\"));
        }

        [TestMethod]
        public void TestMethodFileExist()
        {
            Assert.IsNotNull(Utility.ExisteArchivo(@"C:\GNOSS\Proyectos\Hercules\prueba.xml"));
        }

        [TestMethod]
        public void TestMethodFileStreamNotNull()
        {
            try
            {
                FileStream fs = new FileStream(@"C:\GNOSS\Proyectos\Hercules\prueba.xml", FileMode.Open);
                Assert.IsNotNull(fs);

                fs.Close();
            }
            catch (Exception e)
            {
                Assert.IsTrue(false);
            }
        }

        [TestMethod]
        public void TestMethodFileStreamNull()
        {
            try
            {
                FileStream fs = new FileStream(@"C:\GNOSS\Proyectos\Hercules\ArchivoInexistente.xml", FileMode.Open);
                Assert.IsNull(fs);

                fs.Close();
            }
            catch (Exception e)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void TestListadoNoNull()
        {
            XmlSerializer ser = new XmlSerializer(typeof(cvnRootResultBean));
            cvnRootResultBean cvn;
            using (Stream reader = new FileStream(@"C:\GNOSS\Proyectos\Hercules\prueba.xml", FileMode.Open))
            {
                cvn = (cvnRootResultBean)ser.Deserialize(reader);
            }
            string regex = @"000.010";

            Assert.IsNotNull(Utility.ListadoBloque(cvn, regex));
        }

        [TestMethod]
        public void TestListadoConElementos()
        {
            XmlSerializer ser = new XmlSerializer(typeof(cvnRootResultBean));
            cvnRootResultBean cvn;
            using (Stream reader = new FileStream(@"C:\GNOSS\Proyectos\Hercules\prueba.xml", FileMode.Open))
            {
                cvn = (cvnRootResultBean)ser.Deserialize(reader);
            }
            string regex = @"000.010";

            Assert.IsTrue(Utility.ListadoBloque(cvn, regex).Count() > 1);
        }

        [TestMethod]
        public void TestExisteApartado()
        {
            XmlSerializer ser = new XmlSerializer(typeof(cvnRootResultBean));
            cvnRootResultBean cvn;
            using (Stream reader = new FileStream(@"C:\GNOSS\Proyectos\Hercules\prueba.xml", FileMode.Open))
            {
                cvn = (cvnRootResultBean)ser.Deserialize(reader);
            }
            string regex = @"000";

            Assert.IsTrue(Utility.ExisteIdentificadorApartado(cvn, regex));
        }

        [TestMethod]
        public void TestExisteSubapartado()
        {
            XmlSerializer ser = new XmlSerializer(typeof(cvnRootResultBean));
            cvnRootResultBean cvn;
            using (Stream reader = new FileStream(@"C:\GNOSS\Proyectos\Hercules\prueba.xml", FileMode.Open))
            {
                cvn = (cvnRootResultBean)ser.Deserialize(reader);
            }
            string regex = @"000.020";

            Assert.IsTrue(Utility.ExisteIdentificadorSubapartado(cvn, regex));
        }

        [TestMethod]
        public void TestExisteBloque()
        {
            XmlSerializer ser = new XmlSerializer(typeof(cvnRootResultBean));
            cvnRootResultBean cvn;
            using (Stream reader = new FileStream(@"C:\GNOSS\Proyectos\Hercules\prueba.xml", FileMode.Open))
            {
                cvn = (cvnRootResultBean)ser.Deserialize(reader);
            }
            string regex = @"000.020.000";

            Assert.IsTrue(Utility.ExisteIdentificadorBloque(cvn, regex));
        }

    }

    [TestClass]
    public class TestCodigo
    {
        [TestMethod]
        public void TestCodigoValido()
        {
            Assert.IsTrue(Utility.CodigoCorrecto("000.010.020"));
        }

        [TestMethod]
        public void TestCodigoInvalido()
        {
            Assert.IsFalse(Utility.CodigoCorrecto("000.010.020a"));
        }

        [TestMethod]
        public void TestCodigoInvalido2()
        {
            Assert.IsFalse(Utility.CodigoCorrecto("000..010.020"));
        }

        [TestMethod]
        public void TestCodigoNull()
        {
            Assert.IsFalse(Utility.CodigoCorrecto(null));
        }

        [TestMethod]
        public void TestCodigoCorrectoValido()
        {
            Assert.IsTrue(Utility.CodigoCampoCorrecto("000.010.020.000"));
        }

        [TestMethod]
        public void TestCodigoCorrectoInvalido()
        {
            Assert.IsFalse(Utility.CodigoCampoCorrecto("000.010.02a.000"));
        }

        [TestMethod]
        public void TestCodigoCorrectoInvalido2()
        {
            Assert.IsFalse(Utility.CodigoCampoCorrecto("000..000010.020"));
        }

        [TestMethod]
        public void TestCodigoCorrectoNull()
        {
            Assert.IsFalse(Utility.CodigoCampoCorrecto(null));
        }

        [TestMethod]
        public void TestCodigoSeccion_000_010_xxx_xxx()
        {
            XmlSerializer ser = new XmlSerializer(typeof(cvnRootResultBean));
            cvnRootResultBean cvn;
            using (Stream reader = new FileStream(@"C:\GNOSS\Proyectos\Hercules\prueba.xml", FileMode.Open))
            {
                cvn = (cvnRootResultBean)ser.Deserialize(reader);
            }
            string Codigo = @"000.010";

            Assert.IsTrue(Utility.ListadoBloque(cvn, Codigo).Count > 0);
        }

        [TestMethod]
        public void TestCodigoTitulaciónUniversitaria_DirectorTesis_020_010_020_170()
        {
            XmlSerializer ser = new XmlSerializer(typeof(cvnRootResultBean));
            cvnRootResultBean cvn;
            using (Stream reader = new FileStream(@"C:\GNOSS\Proyectos\Hercules\prueba.xml", FileMode.Open))
            {
                cvn = (cvnRootResultBean)ser.Deserialize(reader);
            }
            string codigo = @"020.010.020";
            List<CVNObject> listado = Utility.ListadoBloque(cvn, codigo);
            List<CvnItemBeanCvnAuthorBean> listadoAuthor = new List<CvnItemBeanCvnAuthorBean>();
            foreach (object c in listado)
            {
                if (c.GetType().Name.Equals("CvnItemBeanCvnAuthorBean"))
                {
                    listadoAuthor.Add((CvnItemBeanCvnAuthorBean)c);
                }
            }

            Assert.IsTrue(listadoAuthor.Where(x => x.Code.Equals("020.010.020.170")).Any());
        }

        [TestMethod]
        public void TestCodigoTitulaciónUniversitaria_Fecha_020_010_010_130()
        {
            XmlSerializer ser = new XmlSerializer(typeof(cvnRootResultBean));
            cvnRootResultBean cvn;
            using (Stream reader = new FileStream(@"C:\GNOSS\Proyectos\Hercules\prueba.xml", FileMode.Open))
            {
                cvn = (cvnRootResultBean)ser.Deserialize(reader);
            }
            string codigo = @"020\.010\.010\.\d{3}";
            List<CVNObject> listado = Utility.ListadoBloque(cvn, codigo);
            List<CvnItemBeanCvnAuthorBean> listadoAuthor = new List<CvnItemBeanCvnAuthorBean>();
            foreach (object c in listado)
            {
                if (c.GetType().Name.Equals("CvnItemBeanCvnAuthorBean"))
                {
                    listadoAuthor.Add((CvnItemBeanCvnAuthorBean)c);
                }
            }

            Assert.IsFalse(listadoAuthor.Where(x => x.Code.Equals("020.010.010.130")).Any());
        }

    }


    [TestClass]
    public class TestTipos
    {
        [TestMethod]
        public void TestTipo_Existe_CvnItemBeanCvnAuthorBean()
        {
            XmlSerializer ser = new XmlSerializer(typeof(cvnRootResultBean));
            cvnRootResultBean cvn;
            using (Stream reader = new FileStream(@"C:\GNOSS\Proyectos\Hercules\prueba.xml", FileMode.Open))
            {
                cvn = (cvnRootResultBean)ser.Deserialize(reader);
            }

            List<CvnItemBeanCvnAuthorBean> listado = new List<CvnItemBeanCvnAuthorBean>();
            List<object> listadoAuthorBean = new List<object>();
            foreach (CvnItemBean c in cvn.cvnRootBean)
            {
                for (int i = 0; i < c.Items.Count; i++)
                {
                    if (c.Items[0].GetType().Name == "CvnItemBeanCvnAuthorBean")
                    {
                        listadoAuthorBean.Add(c);
                    }
                }
            }

            foreach (object c in listadoAuthorBean)
            {
                if (c.GetType().Name.Equals("CvnItemBeanCvnAuthorBean"))
                {
                    listado.Add((CvnItemBeanCvnAuthorBean)c);
                }
            }
            Assert.IsFalse(listado.Count() > 0);
        }

        [TestMethod]
        public void TestTipo_Existe_CvnItemBeanCvnBoolean()
        {
            XmlSerializer ser = new XmlSerializer(typeof(cvnRootResultBean));
            cvnRootResultBean cvn;
            using (Stream reader = new FileStream(@"C:\GNOSS\Proyectos\Hercules\prueba.xml", FileMode.Open))
            {
                cvn = (cvnRootResultBean)ser.Deserialize(reader);
            }

            List<CvnItemBeanCvnBoolean> listado = new List<CvnItemBeanCvnBoolean>();
            List<object> listadoAuthorBean = new List<object>();
            foreach (CvnItemBean c in cvn.cvnRootBean)
            {
                for (int i = 0; i < c.Items.Count; i++)
                {
                    if (c.Items[0].GetType().Name == "CvnItemBeanCvnBoolean")
                    {
                        listadoAuthorBean.Add(c);
                    }
                }
            }

            foreach (object c in listadoAuthorBean)
            {
                if (c.GetType().Name.Equals("CvnItemBeanCvnBoolean"))
                {
                    listado.Add((CvnItemBeanCvnBoolean)c);
                }
            }
            Assert.IsFalse(listado.Count() > 0);
        }

        [TestMethod]
        public void TestTipo_Existe_CvnItemBeanCvnCodeGroup()
        {
            XmlSerializer ser = new XmlSerializer(typeof(cvnRootResultBean));
            cvnRootResultBean cvn;
            using (Stream reader = new FileStream(@"C:\GNOSS\Proyectos\Hercules\prueba.xml", FileMode.Open))
            {
                cvn = (cvnRootResultBean)ser.Deserialize(reader);
            }

            List<CvnItemBeanCvnCodeGroup> listado = new List<CvnItemBeanCvnCodeGroup>();
            List<object> listadoCodeGroup = new List<object>();
            foreach (CvnItemBean c in cvn.cvnRootBean)
            {
                for (int i = 0; i < c.Items.Count; i++)
                {
                    if (c.Items[0].GetType().Name == "CvnItemBeanCvnCodeGroup")
                    {
                        listadoCodeGroup.Add(c);
                    }
                }
            }

            foreach (object c in listadoCodeGroup)
            {
                if (c.GetType().Name.Equals("CvnItemBeanCvnCodeGroup"))
                {
                    listado.Add((CvnItemBeanCvnCodeGroup)c);
                }
            }
            Assert.IsFalse(listado.Count() > 0);
        }

        [TestMethod]
        public void TestTipo_Existe_CvnItemBeanCvnDateDayMonthYear()
        {
            XmlSerializer ser = new XmlSerializer(typeof(cvnRootResultBean));
            cvnRootResultBean cvn;
            using (Stream reader = new FileStream(@"C:\GNOSS\Proyectos\Hercules\prueba.xml", FileMode.Open))
            {
                cvn = (cvnRootResultBean)ser.Deserialize(reader);
            }

            List<CvnItemBeanCvnDateDayMonthYear> listado = new List<CvnItemBeanCvnDateDayMonthYear>();
            List<object> listadoDateDayMonthYear = new List<object>();
            foreach (CvnItemBean c in cvn.cvnRootBean)
            {
                for (int i = 0; i < c.Items.Count; i++)
                {
                    if (c.Items[0].GetType().Name == "CvnItemBeanCvnDateDayMonthYear")
                    {
                        listadoDateDayMonthYear.Add(c);
                    }
                }
            }

            foreach (object c in listadoDateDayMonthYear)
            {
                if (c.GetType().Name.Equals("CvnItemBeanCvnDateDayMonthYear"))
                {
                    listado.Add((CvnItemBeanCvnDateDayMonthYear)c);
                }
            }
            Assert.IsFalse(listado.Count() > 0);
        }

        [TestMethod]
        public void TestTipo_Existe_CvnItemBeanCvnDateMonthYear()
        {
            XmlSerializer ser = new XmlSerializer(typeof(cvnRootResultBean));
            cvnRootResultBean cvn;
            using (Stream reader = new FileStream(@"C:\GNOSS\Proyectos\Hercules\prueba.xml", FileMode.Open))
            {
                cvn = (cvnRootResultBean)ser.Deserialize(reader);
            }

            List<CvnItemBeanCvnDateMonthYear> listado = new List<CvnItemBeanCvnDateMonthYear>();
            List<object> listadoDateMonthYear = new List<object>();
            foreach (CvnItemBean c in cvn.cvnRootBean)
            {
                for (int i = 0; i < c.Items.Count; i++)
                {
                    if (c.Items[0].GetType().Name == "CvnItemBeanCvnDateMonthYear")
                    {
                        listadoDateMonthYear.Add(c);
                    }
                }
            }

            foreach (object c in listadoDateMonthYear)
            {
                if (c.GetType().Name.Equals("CvnItemBeanCvnDateMonthYear"))
                {
                    listado.Add((CvnItemBeanCvnDateMonthYear)c);
                }
            }
            Assert.IsFalse(listado.Count() > 0);
        }

        [TestMethod]
        public void TestTipo_Existe_CvnItemBeanCvnDateYear()
        {
            XmlSerializer ser = new XmlSerializer(typeof(cvnRootResultBean));
            cvnRootResultBean cvn;
            using (Stream reader = new FileStream(@"C:\GNOSS\Proyectos\Hercules\prueba.xml", FileMode.Open))
            {
                cvn = (cvnRootResultBean)ser.Deserialize(reader);
            }

            List<CvnItemBeanCvnDateYear> listado = new List<CvnItemBeanCvnDateYear>();
            List<object> listadoDateYear = new List<object>();
            foreach (CvnItemBean c in cvn.cvnRootBean)
            {
                for (int i = 0; i < c.Items.Count; i++)
                {
                    if (c.Items[0].GetType().Name == "CvnItemBeanCvnDateYear")
                    {
                        listadoDateYear.Add(c);
                    }
                }
            }

            foreach (object c in listadoDateYear)
            {
                if (c.GetType().Name.Equals("CvnItemBeanCvnDateYear"))
                {
                    listado.Add((CvnItemBeanCvnDateYear)c);
                }
            }
            Assert.IsFalse(listado.Count() > 0);
        }

        [TestMethod]
        public void TestTipo_Existe_CvnItemBeanCvnDouble()
        {
            XmlSerializer ser = new XmlSerializer(typeof(cvnRootResultBean));
            cvnRootResultBean cvn;
            using (Stream reader = new FileStream(@"C:\GNOSS\Proyectos\Hercules\prueba.xml", FileMode.Open))
            {
                cvn = (cvnRootResultBean)ser.Deserialize(reader);
            }

            List<CvnItemBeanCvnDouble> listado = new List<CvnItemBeanCvnDouble>();
            List<object> listadoDouble = new List<object>();
            foreach (CvnItemBean c in cvn.cvnRootBean)
            {
                for (int i = 0; i < c.Items.Count; i++)
                {
                    if (c.Items[0].GetType().Name == "CvnItemBeanCvnDouble")
                    {
                        listadoDouble.Add(c);
                    }
                }
            }

            foreach (object c in listadoDouble)
            {
                if (c.GetType().Name.Equals("CvnItemBeanCvnDouble"))
                {
                    listado.Add((CvnItemBeanCvnDouble)c);
                }
            }
            Assert.IsFalse(listado.Count() > 0);
        }

        [TestMethod]
        public void TestTipo_Existe_CvnItemBeanCvnDuration()
        {
            XmlSerializer ser = new XmlSerializer(typeof(cvnRootResultBean));
            cvnRootResultBean cvn;
            using (Stream reader = new FileStream(@"C:\GNOSS\Proyectos\Hercules\prueba.xml", FileMode.Open))
            {
                cvn = (cvnRootResultBean)ser.Deserialize(reader);
            }

            List<CvnItemBeanCvnDuration> listado = new List<CvnItemBeanCvnDuration>();
            List<object> listadoDuration = new List<object>();
            foreach (CvnItemBean c in cvn.cvnRootBean)
            {
                for (int i = 0; i < c.Items.Count; i++)
                {
                    if (c.Items[0].GetType().Name == "CvnItemBeanCvnDuration")
                    {
                        listadoDuration.Add(c);
                    }
                }
            }

            foreach (object c in listadoDuration)
            {
                if (c.GetType().Name.Equals("CvnItemBeanCvnDuration"))
                {
                    listado.Add((CvnItemBeanCvnDuration)c);
                }
            }
            Assert.IsFalse(listado.Count() > 0);
        }

        [TestMethod]
        public void TestTipo_Existe_CvnItemBeanCvnEntityBean()
        {
            XmlSerializer ser = new XmlSerializer(typeof(cvnRootResultBean));
            cvnRootResultBean cvn;
            using (Stream reader = new FileStream(@"C:\GNOSS\Proyectos\Hercules\prueba.xml", FileMode.Open))
            {
                cvn = (cvnRootResultBean)ser.Deserialize(reader);
            }

            List<CvnItemBeanCvnEntityBean> listado = new List<CvnItemBeanCvnEntityBean>();
            List<object> listadoEntityBean = new List<object>();
            foreach (CvnItemBean c in cvn.cvnRootBean)
            {
                for (int i = 0; i < c.Items.Count; i++)
                {
                    if (c.Items[0].GetType().Name == "CvnItemBeanCvnEntityBean")
                    {
                        listadoEntityBean.Add(c);
                    }
                }
            }

            foreach (object c in listadoEntityBean)
            {
                if (c.GetType().Name.Equals("CvnItemBeanCvnEntityBean"))
                {
                    listado.Add((CvnItemBeanCvnEntityBean)c);
                }
            }
            Assert.IsFalse(listado.Count() > 0);
        }

        [TestMethod]
        public void TestTipo_Existe_CvnItemBeanCvnExternalPKBean()
        {
            XmlSerializer ser = new XmlSerializer(typeof(cvnRootResultBean));
            cvnRootResultBean cvn;
            using (Stream reader = new FileStream(@"C:\GNOSS\Proyectos\Hercules\prueba.xml", FileMode.Open))
            {
                cvn = (cvnRootResultBean)ser.Deserialize(reader);
            }

            List<CvnItemBeanCvnExternalPKBean> listado = new List<CvnItemBeanCvnExternalPKBean>();
            List<object> listadoExternalPKBean = new List<object>();
            foreach (CvnItemBean c in cvn.cvnRootBean)
            {
                for (int i = 0; i < c.Items.Count; i++)
                {
                    if (c.Items[0].GetType().Name == "CvnItemBeanCvnExternalPKBean")
                    {
                        listadoExternalPKBean.Add(c);
                    }
                }
            }

            foreach (object c in listadoExternalPKBean)
            {
                if (c.GetType().Name.Equals("CvnItemBeanCvnExternalPKBean"))
                {
                    listado.Add((CvnItemBeanCvnExternalPKBean)c);
                }
            }
            Assert.IsFalse(listado.Count() > 0);
        }

        [TestMethod]
        public void TestTipo_Existe_CvnItemBeanCvnFamilyNameBean()
        {
            XmlSerializer ser = new XmlSerializer(typeof(cvnRootResultBean));
            cvnRootResultBean cvn;
            using (Stream reader = new FileStream(@"C:\GNOSS\Proyectos\Hercules\prueba.xml", FileMode.Open))
            {
                cvn = (cvnRootResultBean)ser.Deserialize(reader);
            }

            List<CvnItemBeanCvnFamilyNameBean> listado = new List<CvnItemBeanCvnFamilyNameBean>();
            List<object> listadoFamilyNameBean = new List<object>();
            foreach (CvnItemBean c in cvn.cvnRootBean)
            {
                for (int i = 0; i < c.Items.Count; i++)
                {
                    if (c.Items[0].GetType().Name == "CvnItemBeanCvnFamilyNameBean")
                    {
                        listadoFamilyNameBean.Add(c);
                    }
                }
            }

            foreach (object c in listadoFamilyNameBean)
            {
                if (c.GetType().Name.Equals("CvnItemBeanCvnFamilyNameBean"))
                {
                    listado.Add((CvnItemBeanCvnFamilyNameBean)c);
                }
            }
            Assert.IsFalse(listado.Count() > 0);
        }

        [TestMethod]
        public void TestTipo_Existe_CvnItemBeanCvnPageBean()
        {
            XmlSerializer ser = new XmlSerializer(typeof(cvnRootResultBean));
            cvnRootResultBean cvn;
            using (Stream reader = new FileStream(@"C:\GNOSS\Proyectos\Hercules\prueba.xml", FileMode.Open))
            {
                cvn = (cvnRootResultBean)ser.Deserialize(reader);
            }

            List<CvnItemBeanCvnPageBean> listado = new List<CvnItemBeanCvnPageBean>();
            List<object> listadoPageBean = new List<object>();
            foreach (CvnItemBean c in cvn.cvnRootBean)
            {
                for (int i = 0; i < c.Items.Count; i++)
                {
                    if (c.Items[0].GetType().Name == "CvnItemBeanCvnPageBean")
                    {
                        listadoPageBean.Add(c);
                    }
                }
            }

            foreach (object c in listadoPageBean)
            {
                if (c.GetType().Name.Equals("CvnItemBeanCvnPageBean"))
                {
                    listado.Add((CvnItemBeanCvnPageBean)c);
                }
            }
            Assert.IsFalse(listado.Count() > 0);
        }

        [TestMethod]
        public void TestTipo_Existe_CvnItemBeanCvnPhoneBean()
        {
            XmlSerializer ser = new XmlSerializer(typeof(cvnRootResultBean));
            cvnRootResultBean cvn;
            using (Stream reader = new FileStream(@"C:\GNOSS\Proyectos\Hercules\prueba.xml", FileMode.Open))
            {
                cvn = (cvnRootResultBean)ser.Deserialize(reader);
            }

            List<CvnItemBeanCvnPhoneBean> listado = new List<CvnItemBeanCvnPhoneBean>();
            List<object> listadoPhoneBean = new List<object>();
            foreach (CvnItemBean c in cvn.cvnRootBean)
            {
                for (int i = 0; i < c.Items.Count; i++)
                {
                    if (c.Items[0].GetType().Name == "CvnItemBeanCvnPhoneBean")
                    {
                        listadoPhoneBean.Add(c);
                    }
                }
            }

            foreach (object c in listadoPhoneBean)
            {
                if (c.GetType().Name.Equals("CvnItemBeanCvnPhoneBean"))
                {
                    listado.Add((CvnItemBeanCvnPhoneBean)c);
                }
            }
            Assert.IsFalse(listado.Count() > 0);
        }

        [TestMethod]
        public void TestTipo_Existe_CvnItemBeanCvnRichText()
        {
            XmlSerializer ser = new XmlSerializer(typeof(cvnRootResultBean));
            cvnRootResultBean cvn;
            using (Stream reader = new FileStream(@"C:\GNOSS\Proyectos\Hercules\prueba.xml", FileMode.Open))
            {
                cvn = (cvnRootResultBean)ser.Deserialize(reader);
            }

            List<CvnItemBeanCvnRichText> listado = new List<CvnItemBeanCvnRichText>();
            List<object> listadoRichText = new List<object>();
            foreach (CvnItemBean c in cvn.cvnRootBean)
            {
                for (int i = 0; i < c.Items.Count; i++)
                {
                    if (c.Items[0].GetType().Name == "CvnItemBeanCvnRichText")
                    {
                        listadoRichText.Add(c);
                    }
                }
            }

            foreach (object c in listadoRichText)
            {
                if (c.GetType().Name.Equals("CvnItemBeanCvnRichText"))
                {
                    listado.Add((CvnItemBeanCvnRichText)c);
                }
            }
            Assert.IsFalse(listado.Count() > 0);
        }

        [TestMethod]
        public void TestTipo_Existe_CvnItemBeanCvnString()
        {
            XmlSerializer ser = new XmlSerializer(typeof(cvnRootResultBean));
            cvnRootResultBean cvn;
            using (Stream reader = new FileStream(@"C:\GNOSS\Proyectos\Hercules\prueba.xml", FileMode.Open))
            {
                cvn = (cvnRootResultBean)ser.Deserialize(reader);
            }

            List<CvnItemBeanCvnString> listado = new List<CvnItemBeanCvnString>();
            List<object> listadoString = new List<object>();
            foreach (CvnItemBean c in cvn.cvnRootBean)
            {
                for (int i = 0; i < c.Items.Count; i++)
                {
                    if (c.Items[0].GetType().Name == "CvnItemBeanCvnString")
                    {
                        listadoString.Add(c);
                    }
                }
            }

            foreach (object c in listadoString)
            {
                if (c.GetType().Name.Equals("CvnItemBeanCvnString"))
                {
                    listado.Add((CvnItemBeanCvnString)c);
                }
            }
            Assert.IsFalse(listado.Count() > 0);
        }

        [TestMethod]
        public void TestTipo_Existe_CvnItemBeanCvnTitleBean()
        {
            XmlSerializer ser = new XmlSerializer(typeof(cvnRootResultBean));
            cvnRootResultBean cvn;
            using (Stream reader = new FileStream(@"C:\GNOSS\Proyectos\Hercules\prueba.xml", FileMode.Open))
            {
                cvn = (cvnRootResultBean)ser.Deserialize(reader);
            }

            List<CvnItemBeanCvnTitleBean> listado = new List<CvnItemBeanCvnTitleBean>();
            List<object> listadoTitleBean = new List<object>();
            foreach (CvnItemBean c in cvn.cvnRootBean)
            {
                for (int i = 0; i < c.Items.Count; i++)
                {
                    if (c.Items[0].GetType().Name == "CvnItemBeanCvnTitleBean")
                    {
                        listadoTitleBean.Add(c);
                    }
                }
            }

            foreach (object c in listadoTitleBean)
            {
                if (c.GetType().Name.Equals("CvnItemBeanCvnTitleBean"))
                {
                    listado.Add((CvnItemBeanCvnTitleBean)c);
                }
            }
            Assert.IsFalse(listado.Count() > 0);
        }

        [TestMethod]
        public void TestTipo_Existe_CvnItemBeanCvnVolumeBean()
        {
            XmlSerializer ser = new XmlSerializer(typeof(cvnRootResultBean));
            cvnRootResultBean cvn;
            using (Stream reader = new FileStream(@"C:\GNOSS\Proyectos\Hercules\prueba.xml", FileMode.Open))
            {
                cvn = (cvnRootResultBean)ser.Deserialize(reader);
            }

            List<CvnItemBeanCvnVolumeBean> listado = new List<CvnItemBeanCvnVolumeBean>();
            List<object> listadoVolumeBean = new List<object>();
            foreach (CvnItemBean c in cvn.cvnRootBean)
            {
                for (int i = 0; i < c.Items.Count; i++)
                {
                    if (c.Items[0].GetType().Name == "CvnItemBeanCvnVolumeBean")
                    {
                        listadoVolumeBean.Add(c);
                    }
                }
            }

            foreach (object c in listadoVolumeBean)
            {
                if (c.GetType().Name.Equals("CvnItemBeanCvnVolumeBean"))
                {
                    listado.Add((CvnItemBeanCvnVolumeBean)c);
                }
            }
            Assert.IsFalse(listado.Count() > 0);
        }
    }

    [TestClass]
    public class TestUtility
    {
        private static cvnRootResultBean cvn = leerXMLApartado_000();
        private static cvnRootResultBean leerXMLApartado_000()
        {
            XmlSerializer ser = new XmlSerializer(typeof(cvnRootResultBean));
            cvnRootResultBean cvn;
            using (Stream reader = new FileStream(@"C:\GNOSS\Proyectos\Hercules\prueba.xml", FileMode.Open))
            {
                cvn = (cvnRootResultBean)ser.Deserialize(reader);
            }
            return cvn;
        }

        private static List<CvnItemBean> ListadoDatosIdentificacion = leerXMLApartado_000().cvnRootBean.ToList();

        [TestMethod]
        public void TestGetListadobloque()
        {
            Assert.IsTrue(Utility.GetListadoBloque(leerXMLApartado_000(), "000").Count > 0);
        }

        [TestMethod]
        public void TestGetListaElementosPorIDCampo()
        {
            Assert.IsFalse(ListadoDatosIdentificacion.GetListaElementosPorIDCampo<CvnItemBeanCvnExternalPKBean>("000.010.000.260").Any());
        }

        [TestMethod]
        public void TestDatetimeStringGNOSS()
        {
            CvnItemBeanCvnDateDayMonthYear dateTimeFechaNacimiento = ListadoDatosIdentificacion.GetElementoPorIDCampo<CvnItemBeanCvnDateDayMonthYear>("000.010.000.050");
            Assert.AreEqual("19650411000000", dateTimeFechaNacimiento.DatetimeStringGNOSS());
        }

        [TestMethod]
        public void TestStringPorIDCampo()
        {
            Assert.IsNotNull(ListadoDatosIdentificacion.GetStringPorIDCampo("000.010.000.020"));
        }

        [TestMethod]
        public void TestNullStringPorIDCampo()
        {
            Assert.IsNull(ListadoDatosIdentificacion.GetStringPorIDCampo("000.010.000.110"));
        }

        [TestMethod]
        public void TestGeneroPorIDCampo()
        {
            Assert.AreEqual("http://gnoss.com/items/gender_000", ListadoDatosIdentificacion.GetGeneroPorIDCampo("000.010.000.030"));
        }

        [TestMethod]
        public void TestNullProvinciaPorIDCampo()
        {
            Assert.AreEqual(null, ListadoDatosIdentificacion.GetProvinciaPorIDCampo("000.010.000.200"));
        }

        [TestMethod]
        public void TestNullRegionPorIDCampo()
        {
            Assert.AreEqual(null, ListadoDatosIdentificacion.GetRegionPorIDCampo("000.010.000.190"));
        }

        [TestMethod]
        public void TestPaisPorIDCampo()
        {
            Assert.AreEqual("http://gnoss.com/items/feature_PCLD_724", ListadoDatosIdentificacion.GetPaisPorIDCampo("000.010.000.180"));
        }

    }

    [TestClass]
    public class TestIdentificacionYContacto
    {
        private static string cvID = "http://gnoss.com/items/CV_fdd3bc7f-305a-417e-8eda-5f5671c897e2_a83b9f7a-a46d-473a-9941-507969f5c147";
        private static cvnRootResultBean cvn = leerXMLApartado_000();
        private static cvnRootResultBean leerXMLApartado_000()
        {
            XmlSerializer ser = new XmlSerializer(typeof(cvnRootResultBean));
            cvnRootResultBean cvn;
            using (Stream reader = new FileStream(@"C:\GNOSS\Proyectos\Hercules\prueba.xml", FileMode.Open))
            {
                cvn = (cvnRootResultBean)ser.Deserialize(reader);
            }
            return cvn;
        }

        [TestMethod]
        public void TestPrimerApellido()
        {
            Assert.AreEqual("SKARMETA", cvn.GetElementoPorIDCampo<CvnItemBeanCvnFamilyNameBean>("000.010.000.010").FirstFamilyName);
        }

        [TestMethod]
        public void TestSegundoApellido()
        {

            Assert.AreEqual("GOMEZ", cvn.GetElementoPorIDCampo<CvnItemBeanCvnFamilyNameBean>("000.010.000.010").SecondFamilyName);
        }

        [TestMethod]
        public void TestNombre()
        {

            Assert.AreEqual("ANTONIO FERNANDO", cvn.GetStringPorIDCampo("000.010.000.020"));
        }

        [TestMethod]
        public void TestSexo()
        {

            Assert.AreEqual("000", cvn.GetStringPorIDCampo("000.010.000.030"));
        }

        [TestMethod]
        public void TestNacionalidad()
        {

            Assert.AreEqual("724", cvn.GetStringPorIDCampo("000.010.000.040"));
        }

        [TestMethod]
        public void TestFechaNacimiento()
        {
            Assert.AreEqual(Convert.ToDateTime("1965-04-11T00:00:00.000+01:00"), cvn.GetDateTimePorIDCampo("000.010.000.050"));
        }

        [TestMethod]
        public void TestPaisNacimiento()
        {

            Assert.AreEqual("152", cvn.GetStringPorIDCampo("000.010.000.060"));
        }

        [Ignore]
        [TestMethod]
        public void TestCCAARegionNacimiento()
        {
            Assert.AreEqual("", cvn.GetStringPorIDCampo("000.010.000.070"));
        }

        [Ignore]
        [TestMethod]
        public void TestCiudadNacimiento()
        {
            Assert.AreEqual("", cvn.GetStringPorIDCampo("000.010.000.090"));
        }

        [TestMethod]
        public void TestDNI()
        {

            Assert.AreEqual("28710458H", cvn.GetStringPorIDCampo("000.010.000.100"));
        }

        [Ignore]
        [TestMethod]
        public void TestNIE()
        {

            Assert.AreEqual("", cvn.GetStringPorIDCampo("000.010.000.110"));
        }

        [Ignore]
        [TestMethod]
        public void TestPasaporte()
        {

            Assert.AreEqual("", cvn.GetStringPorIDCampo("000.010.000.120"));
        }

        [Ignore]
        [TestMethod]
        public void TestFotografiaDigital()
        {

            Assert.AreEqual("", cvn.GetStringPorIDCampo("000.010.000.130"));
        }

        [TestMethod]
        public void TestDireccionContacto()
        {

            Assert.AreEqual("PROFESOR ANTONIO DE HOYOS 3", cvn.GetStringPorIDCampo("000.010.000.140"));
        }

        [TestMethod]
        public void TestRestoDireccion()
        {

            Assert.AreEqual("1A", cvn.GetStringPorIDCampo("000.010.000.150"));
        }

        [TestMethod]
        public void TestCodigoPostal()
        {

            Assert.AreEqual("30009", cvn.GetStringPorIDCampo("000.010.000.160"));
        }

        [TestMethod]
        public void TestCiudadContacto()
        {

            Assert.AreEqual("Murcia", cvn.GetStringPorIDCampo("000.010.000.170"));
        }

        [TestMethod]
        public void TestPaisContacto()
        {

            Assert.AreEqual("724", cvn.GetStringPorIDCampo("000.010.000.180"));
        }

        [Ignore]
        [TestMethod]
        public void TestCCAARegionContacto()
        {
            Assert.AreEqual("ES23", cvn.GetStringPorIDCampo("000.010.000.190"));
        }

        [Ignore]
        [TestMethod]
        public void TestProvinciaContacto()
        {
            Assert.AreEqual("100", cvn.GetStringPorIDCampo("000.010.000.200"));
        }

        [TestMethod]
        public void TestFijo()
        {
            Assert.AreEqual("868884607", Utility.GetListaElementosPorIDCampo<CvnItemBeanCvnPhoneBean>(cvn, "000.010.000.210").FirstOrDefault().Number);
        }

        [TestMethod]
        public void TestFax()
        {
            Assert.AreEqual("868884151", Utility.GetListaElementosPorIDCampo<CvnItemBeanCvnPhoneBean>(cvn, "000.010.000.220").FirstOrDefault().Number);
        }

        [TestMethod]
        public void TestCorreo()
        {
            Assert.AreEqual("skarmeta@um.es", cvn.GetStringPorIDCampo("000.010.000.230"));
        }

        [Ignore]
        [TestMethod]
        public void TestMovil()
        {
            Assert.AreEqual("", Utility.GetListaElementosPorIDCampo<CvnItemBeanCvnPhoneBean>(cvn, "000.010.000.240").FirstOrDefault().Number);
        }

        [Ignore]
        [TestMethod]
        public void TestWeb()
        {

            Assert.AreEqual("", cvn.GetStringPorIDCampo("000.010.000.250"));
        }

        [TestMethod]
        public void TestNullORCID()
        {
            Assert.AreEqual(null, Utility.GetListaElementosPorIDCampo<CvnItemBeanCvnExternalPKBean>(cvn, "000.010.000.260").GetORCID());
        }

        [TestMethod]
        public void TestNullScopus()
        {
            Assert.AreEqual(null, Utility.GetListaElementosPorIDCampo<CvnItemBeanCvnExternalPKBean>(cvn, "000.010.000.260").GetScopus());
        }

        [TestMethod]
        public void TestNullResearcherID()
        {
            Assert.AreEqual(null, Utility.GetListaElementosPorIDCampo<CvnItemBeanCvnExternalPKBean>(cvn, "000.010.000.260").GetResearcherID());
        }

        [TestMethod]
        public void TestOtrosIdentificadores()
        {
            Assert.IsFalse(Utility.GetListaElementosPorIDCampo<CvnItemBeanCvnExternalPKBean>(cvn, "000.010.000.260").Where(x => x.Type.Equals("OTHERS")).Any());
        }

        [Ignore]
        [TestMethod]
        public void TestID_CV()
        {

            Assert.AreEqual("", cvn.GetStringPorIDCampo("000.020.000.010"));
        }

        [TestMethod]
        public void TestFechaDocumento()
        {

            Assert.AreEqual(Convert.ToDateTime("2021-06-28T00:00:00.000+02:00"), cvn.GetDateTimePorIDCampo("000.020.000.020"));
        }

        [TestMethod]
        public void TestIdiomaCV()
        {

            Assert.AreEqual("spa", cvn.GetStringPorIDCampo("000.020.000.070"));
        }

        [Ignore]
        [TestMethod]
        public void TestVersionCodificacion()
        {
            Assert.AreEqual("1.3.0", cvn.GetStringPorIDCampo("000.020.000.080"));
        }

        
    }
}
