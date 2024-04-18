using LinkOS.Plugin;
using LinkOS.Plugin.Abstractions;
using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.Printer.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Forms;
using MovilBusiness.Utils;
using ESCPOS_NET.Emitters;
using ESCPOS_NET;

namespace MovilBusiness.Printer
{
    public class PrinterManager
    {
        //private readonly string MacAddress;
        public static IConnection Conn { get; set; }
        private iOSEscPosPrinterConn iOSConn { get; set; }

        public bool IsEscPosForiOS { get; private set; }

        public bool IsConnectionAvailable
        {
            get
            {
                if (IsEscPosForiOS)
                {
                    return iOSConn != null;
                }
                else
                {
                    return Conn != null;
                }
            }
        }

        private string DocumentCPCL = "! 0 200 200 @PAPERSIZE@ @COPIAS@ \r\nON-FEED IGNORE\r\n! U1 PW @PageWidth@\r\n";
        private List<byte[]> DocumentESCPOS = new List<byte[]>()
        {
            new byte[] {27, 64}, new byte[] { 0x1B, 0x21, 0x01 }, new byte[] { 0x1b, Convert.ToByte('a'), 0x00 }
        };

        //private string DocumentESCPOS = "";//"ESC @ ESC !, LF";
        private int DocumentX = 3, DocumentY = 20;

        private bool bold;
        public bool Bold { get => bold;
            set
            {
                if (language == PrinterLanguageMB.ESCPOS)
                {

                    DocumentESCPOS.Add(value ? ESC_BOLD : ESC_CANCEL_BOLD);

                   /* if(Conn == null)
                    {
                        return;
                    }

                    if (!IsOpen)
                    {
                        IsOpen = true;
                        Conn.Open();
                    }*/

                    // Conn.Write(value ? ESC_BOLD: ESC_CANCEL_BOLD);

                }
                else
                {
                    bold = value;
                }
            }
        }

        public readonly bool IsEscPos = false;

        private Justification textalign;
        public Justification TextAlign { get => textalign;
            set
            {
                if (language == PrinterLanguageMB.ESCPOS)
                {

                    
                   /* if(Conn == null)
                    {
                        return;
                    }

                    if (!Conn.IsConnected)
                    {
                        Conn.Open();
                    }*/

                    switch (value)
                    {
                        case Justification.CENTER:
                            DocumentESCPOS.Add(ESC_ALIGN_CENTER);
                            break;
                        case Justification.LEFT:
                            DocumentESCPOS.Add(ESC_ALIGN_LEFT);
                            break;
                        case Justification.RIGHT:
                            DocumentESCPOS.Add(ESC_ALIGN_RIGHT);
                            break;
                    }
                }
                else
                {
                    textalign = value;
                }
            }
        }

        public PrinterLanguageMB language = PrinterLanguageMB.CPCL;

        private PrinterFont font = PrinterFont.BODY;
        public PrinterFont Font { get => font;
            set
            {
                if (language == PrinterLanguageMB.ESCPOS)
                {
                   /* if(Conn == null)
                    {
                        return;
                    }

                    if (!Conn.IsConnected)
                    {
                        Conn.Open();
                    }*/

                    switch (value)
                    {
                        case PrinterFont.BODY:
                        case PrinterFont.FOOTER:
                            DocumentESCPOS.Add(cc);
                            break;
                        case PrinterFont.TITLE:
                            DocumentESCPOS.Add(cc);
                            DocumentESCPOS.Add(ESC_BOLD);
                            break;
                        case PrinterFont.MAXTITLE:
                            DocumentESCPOS.Add(E_BOLD);
                            break; 
                        case PrinterFont.SUPERMAXTITLE:
                            DocumentESCPOS.Add(E_BOLDv2);
                            break; 
                        case PrinterFont.MINTITLE:
                            DocumentESCPOS.Add(E_MIN);
                            break;
                    }
                }
                else
                {
                    font = value;
                }
            }
        }
        private int PageWidth;

        private const int pulgadaCPCL = 192;
        private const int LineHeight = 20;

        public Empresa Empresa { get; set; }

        private readonly PrinterMetaData PrinterData;
        //public readonly bool IsEscPos = false;

        private bool PrintEmpLogo = false;

        public PrinterManager(string seccodigo="")
        {
            try
            {
                PrinterData = DS_RepresentantesParametros.GetInstance().GetImpresora();

                if (PrinterData == null || string.IsNullOrWhiteSpace(PrinterData.PrinterMac))
                {
                    throw new Exception("No tienes la impresora configurada");
                }

               // IsEscPos = PrinterData.PrinterLanguage != null && PrinterData.PrinterLanguage == "ESCPOS";

                if(PrinterData.PrinterLanguage == "ESCPOS")
                {
                    language = PrinterLanguageMB.ESCPOS;
                    IsEscPos = true;
                }
                else if (PrinterData.PrinterLanguage == "CPCL")
                {
                    language = PrinterLanguageMB.CPCL;
                }

                IsEscPosForiOS = Device.RuntimePlatform == Device.iOS && language == PrinterLanguageMB.ESCPOS;

                if (!string.IsNullOrWhiteSpace(PrinterData.PrinterMac) && IsEscPosForiOS)
                {
                    iOSConn = DependencyService.Get<iOSEscPosPrinterConn>();
                    iOSConn.Initialize(PrinterData.PrinterMac);
                }
                else
                if ((!string.IsNullOrWhiteSpace(PrinterData.PrinterMac)) && language != PrinterLanguageMB.ESCPOS)
                {
                    Conn = ConnectionBuilder.Current.Build("BT:" + PrinterData.PrinterMac);//Error aqui
                }
                else
                {
                    Conn = DependencyService.Get<IConnection>();
                    Conn.Initialize(PrinterData.PrinterMac);
                }

                PageWidth = 3 * pulgadaCPCL;

                Empresa = new DS_Empresa().GetEmpresa(DS_RepresentantesParametros.GetInstance().GetParEmpresasBySector() ? seccodigo : "");

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
            
        }

        public void PrintEmpresa(int trasec = 0, bool putfecha = false, bool Notbold = false, bool NoSpaces = false, bool NoDireccion = false, int length1 = 45, int length2 = 48, bool TitleNotBold = false)
        {
             if (Conn == null && !IsEscPosForiOS)
            {
                return;
            }

            if (!NoSpaces)
            {
                DrawText("");
                DrawText("");
            }
            
            if (!string.IsNullOrEmpty(DS_RepresentantesParametros.GetInstance().GetParChangeFontEmpresa()))
            {
                Bold = true;
                Font = GetFontDynamic(DS_RepresentantesParametros.GetInstance().GetParChangeFontEmpresa());
            }
            else if (!Notbold)
            {
                Bold = true;
                Font = PrinterFont.TITLE;
            }
            else
            {
                Font = PrinterFont.BODY;
            }

            if (TitleNotBold)
            {
                Bold = false;
                Font = PrinterFont.TITLE;

            }

            var myParametro = DS_RepresentantesParametros.GetInstance();

            if (Empresa != null && !myParametro.GetParEmpresaNoMostrar())
            {
                if (!myParametro.GetParNoLogo())
                {
                    PrintEmpLogo = true;
                }

                if (putfecha)
                    DrawText(Empresa.EmpNombre + "        " + DateTime.Now.ToString("dd/MM/yyyy"));
                else
                    DrawText(Empresa.EmpNombre, length2);

                if (language != PrinterLanguageMB.ESCPOS)
                {
                    if (!Notbold || DS_RepresentantesParametros.GetInstance().GetParChangeFontEmpresa().Equals("TITLE"))
                    {
                        DrawText("");
                    }
                }

                if (DS_RepresentantesParametros.GetInstance().GetParDirrecionDelVendedor())
                {
                    var CentrosDistribucion = new DS_CentrosDistribucion().GetCentrosDistribucion(Arguments.CurrentUser.RepCodigo);

                    if (CentrosDistribucion.CedDescripcion.Contains(","))
                    {
                        var direccion = CentrosDistribucion.CedDescripcion.Split(',');


                        Empresa.EmpDireccion = direccion[0];
                        Empresa.EmpDireccion2 = direccion[1];
                        Empresa.EmpDireccion3 = CentrosDistribucion.CedReferencia;
                    }
                    else
                    {
                        Empresa.EmpDireccion = CentrosDistribucion.CedDescripcion;
                        Empresa.EmpDireccion2 = CentrosDistribucion.CedReferencia;
                        Empresa.EmpDireccion3 = "";
                    }
                }

                if (!string.IsNullOrWhiteSpace(Empresa.EmpDireccion))
                {
                    if (!NoDireccion) { 

                        if (Empresa.EmpDireccion.Length > length2)
                        {
                            Char[] SeparateName = Empresa.EmpDireccion.ToCharArray();
                            for (int i = 0; i < SeparateName.Length; i++)
                            {
                                if (SeparateName[i] == 'á')
                                {
                                    SeparateName[i] = 'a';
                                }

                                if (SeparateName[i] == 'é')
                                {
                                    SeparateName[i] = 'e';
                                }

                                if (SeparateName[i] == 'í')
                                {
                                    SeparateName[i] = 'i';
                                }

                                if (SeparateName[i] == 'ó')
                                {
                                    SeparateName[i] = 'o';
                                }

                                if (SeparateName[i] == 'ú')
                                {
                                    SeparateName[i] = 'u';
                                }

                                if (SeparateName[i] == 'º')
                                {
                                    SeparateName[i] = '#';
                                }
                            }
                            string Emp = new string(SeparateName);
                            Empresa.EmpDireccion = Emp;
                            DrawText(Empresa.EmpDireccion.Substring(0, length1));
                        }
                        else
                        {
                            Char[] SeparateName = Empresa.EmpDireccion.ToCharArray();
                            for (int i = 0; i < SeparateName.Length; i++)
                            {
                                if (SeparateName[i] == 'á')
                                {
                                    SeparateName[i] = 'a';
                                }

                                if (SeparateName[i] == 'é')
                                {
                                    SeparateName[i] = 'e';
                                }

                                if (SeparateName[i] == 'í')
                                {
                                    SeparateName[i] = 'i';
                                }

                                if (SeparateName[i] == 'ó')
                                {
                                    SeparateName[i] = 'o';
                                }

                                if (SeparateName[i] == 'ú')
                                {
                                    SeparateName[i] = 'u';
                                }

                                if (SeparateName[i] == 'º')
                                {
                                    SeparateName[i] = '#';
                                }
                            }
                            string Emp = new string(SeparateName);
                            Empresa.EmpDireccion = Emp;
                            DrawText(Empresa.EmpDireccion,length1);
                        }
                    }
                }


                if (!string.IsNullOrWhiteSpace(Empresa.EmpDireccion2))
                {
                    if (!NoDireccion) 
                    { 
                        if (Empresa.EmpDireccion2.Length > length2)
                        {
                            Char[] SeparateName = Empresa.EmpDireccion2.ToCharArray();
                            for (int i = 0; i < SeparateName.Length; i++)
                            {
                                if (SeparateName[i] == 'á')
                                {
                                    SeparateName[i] = 'a';
                                }

                                if (SeparateName[i] == 'é')
                                {
                                    SeparateName[i] = 'e';
                                }

                                if (SeparateName[i] == 'í')
                                {
                                    SeparateName[i] = 'i';
                                }

                                if (SeparateName[i] == 'ó')
                                {
                                    SeparateName[i] = 'o';
                                }

                                if (SeparateName[i] == 'ú')
                                {
                                    SeparateName[i] = 'u';
                                }

                                if (SeparateName[i] == 'º')
                                {
                                    SeparateName[i] = '#';
                                }
                            }
                            string Emp1 = new string(SeparateName);
                            Empresa.EmpDireccion2 = Emp1;
                            if (!Notbold || DS_RepresentantesParametros.GetInstance().GetParChangeFontEmpresa().Equals("TITLE"))
                            {
                                if (!NoSpaces)
                                {
                                    DrawText("");
                                }
                            }
                            DrawText(Empresa.EmpDireccion2.Substring(0, length2));
                        }
                        else
                        {
                            Char[] SeparateName = Empresa.EmpDireccion2.ToCharArray();
                            for (int i = 0; i < SeparateName.Length; i++)
                            {
                                if (SeparateName[i] == 'á')
                                {
                                    SeparateName[i] = 'a';
                                }

                                if (SeparateName[i] == 'é')
                                {
                                    SeparateName[i] = 'e';
                                }

                                if (SeparateName[i] == 'í')
                                {
                                    SeparateName[i] = 'i';
                                }

                                if (SeparateName[i] == 'ó')
                                {
                                    SeparateName[i] = 'o';
                                }

                                if (SeparateName[i] == 'ú')
                                {
                                    SeparateName[i] = 'u';
                                }

                                if (SeparateName[i] == 'º')
                                {
                                    SeparateName[i] = '#';
                                }
                            }
                            string Emp1 = new string(SeparateName);
                            Empresa.EmpDireccion2 = Emp1;
                            if (!Notbold || DS_RepresentantesParametros.GetInstance().GetParChangeFontEmpresa().Equals("TITLE"))
                            {
                                if (!NoSpaces)
                                {
                                    DrawText("");
                                }
                            }
                            DrawText(Empresa.EmpDireccion2);
                        }
                    }

                }

                if (!string.IsNullOrWhiteSpace(Empresa.EmpDireccion3))
                {
                    if (!NoDireccion) 
                    { 
                        if (Empresa.EmpDireccion3.Length > length2)
                        {
                            Char[] SeparateName = Empresa.EmpDireccion3.ToCharArray();
                            for (int i = 0; i < SeparateName.Length; i++)
                            {
                                if (SeparateName[i] == 'á')
                                {
                                    SeparateName[i] = 'a';
                                }

                                if (SeparateName[i] == 'é')
                                {
                                    SeparateName[i] = 'e';
                                }

                                if (SeparateName[i] == 'í')
                                {
                                    SeparateName[i] = 'i';
                                }

                                if (SeparateName[i] == 'ó')
                                {
                                    SeparateName[i] = 'o';
                                }

                                if (SeparateName[i] == 'ú')
                                {
                                    SeparateName[i] = 'u';
                                }

                                if (SeparateName[i] == 'º')
                                {
                                    SeparateName[i] = '#';
                                }
                            }
                            string Emp1 = new string(SeparateName);
                            Empresa.EmpDireccion3 = Emp1;
                            if (!Notbold || DS_RepresentantesParametros.GetInstance().GetParChangeFontEmpresa().Equals("TITLE"))
                            {
                                if (!NoSpaces)
                                {
                                    DrawText("");
                                }
                            }
                            DrawText(Empresa.EmpDireccion3.Substring(0, length2));
                        }
                        else
                        {
                            Char[] SeparateName = Empresa.EmpDireccion3.ToCharArray();
                            for (int i = 0; i < SeparateName.Length; i++)
                            {
                                if (SeparateName[i] == 'á')
                                {
                                    SeparateName[i] = 'a';
                                }

                                if (SeparateName[i] == 'é')
                                {
                                    SeparateName[i] = 'e';
                                }

                                if (SeparateName[i] == 'í')
                                {
                                    SeparateName[i] = 'i';
                                }

                                if (SeparateName[i] == 'ó')
                                {
                                    SeparateName[i] = 'o';
                                }

                                if (SeparateName[i] == 'ú')
                                {
                                    SeparateName[i] = 'u';
                                }

                                if (SeparateName[i] == 'º')
                                {
                                    SeparateName[i] = '#';
                                }
                            }
                            string Emp1 = new string(SeparateName);
                            Empresa.EmpDireccion3 = Emp1;
                            if (!Notbold || DS_RepresentantesParametros.GetInstance().GetParChangeFontEmpresa().Equals("TITLE"))
                            {
                                if (!NoSpaces)
                                {
                                    DrawText("");
                                }
                            }
                            DrawText(Empresa.EmpDireccion3);
                        }
                    }

                }

                if (language != PrinterLanguageMB.ESCPOS)
                {
                    if (!Notbold || DS_RepresentantesParametros.GetInstance().GetParChangeFontEmpresa().Equals("TITLE"))
                    {
                        if (!NoSpaces)
                        {
                            DrawText("");
                        }
                    }
                }

                DrawText("Telefono: " + Empresa.EmpTelefono);

                if (language != PrinterLanguageMB.ESCPOS)
                {
                    if (!Notbold || DS_RepresentantesParametros.GetInstance().GetParChangeFontEmpresa().Equals("TITLE"))
                    {
                        if (!NoSpaces)
                        {
                            DrawText("");
                        }
                    }
                }
                if (!string.IsNullOrWhiteSpace(Empresa.EmpFax))
                {
                    int formato = DS_RepresentantesParametros.GetInstance().GetFormatoImpresionRecibos();

                    switch (formato)
                    {
                        case 11:
                            DrawText("Cobros: " + Empresa.EmpFax);
                            break;
                        default:
                            DrawText("Fax: " + Empresa.EmpFax);
                            break;
                    }
                }
                if (language != PrinterLanguageMB.ESCPOS && !string.IsNullOrWhiteSpace(Empresa.EmpFax))
                {
                    if (!Notbold || DS_RepresentantesParametros.GetInstance().GetParChangeFontEmpresa().Equals("TITLE"))
                    {
                        if (!NoSpaces)
                        {
                            DrawText("");
                        }
                    }
                }

                DrawText("RNC: " + Empresa.EmpRNC);

                if(trasec != 0)
                {
                    if (!Notbold || DS_RepresentantesParametros.GetInstance().GetParChangeFontEmpresa().Equals("TITLE"))
                    {
                        if (!NoSpaces)
                        {
                            DrawText("");
                        }
                    }
                    DrawText("Sec: " + Arguments.CurrentUser.RepCodigo + " - " + trasec);
                }

            }
            Font = PrinterFont.BODY;
            //FontSize = 0;
            Bold = false;
        }
        // public static byte[] SELECT_BIT_IMAGE_MODE = { 0x1B, 0x2A, 33, -128, 0 };
        //public static byte[] SELECT_BIT_IMAGE_MODE = { 0x1B, 0x2A, 33, 255, 3 };
        //private readonly byte[] SELECT_BIT_IMAGE_MODE = { 0x1B, 0x2A, 33, (byte)255, 0 };
        private static byte[] SELECT_BIT_IMAGE_MODE = { 0x1B, 0x2A, 33, 255, 3 };
        //private readonly byte[] SELECT_BIT_IMAGE_MODE = { 0x0A, 0x0, 0x1d, 0x2f, 0x31, 0xA };
        //private readonly byte[] SELECT_BIT_IMAGE_MODE = new byte[] { 0x1B, 0x2A };

        public void PrintEmpresaV2(int trasec = 0, bool putfecha = false, bool Notbold = false, bool NoSpaces = false, bool NoDireccion = false)
        {
            if (Conn == null && !IsEscPosForiOS)
            {
                return;
            }

            if (!NoSpaces)
            {
                DrawText("");
                DrawText("");
            }

            var myParametro = DS_RepresentantesParametros.GetInstance();
            if (Empresa != null && !myParametro.GetParEmpresaNoMostrar())
            {
                if (!myParametro.GetParNoLogo())
                {
                    PrintEmpLogo = true;
                }
                Font = PrinterFont.SUPERMAXTITLE;
                if (putfecha)
                    DrawText(Empresa.EmpNombre + "        " + Functions.CurrentDate("dd-MM-yyyy hh:mm tt"));
                else
                    DrawText(Empresa.EmpNombre);

                if (language != PrinterLanguageMB.ESCPOS)
                {
                    if (!Notbold || DS_RepresentantesParametros.GetInstance().GetParChangeFontEmpresa().Equals("TITLE"))
                    {
                        DrawText("");
                    }
                }

                Font = PrinterFont.MAXTITLE;
                DrawText("RNC: " + Empresa.EmpRNC);

                if (!string.IsNullOrEmpty(DS_RepresentantesParametros.GetInstance().GetParChangeFontEmpresa()))
                {
                    Bold = true;
                    Font = GetFontDynamic(DS_RepresentantesParametros.GetInstance().GetParChangeFontEmpresa());
                }
                else if (!Notbold)
                {
                    Bold = true;
                    Font = PrinterFont.TITLE;
                }
                else
                {
                    Font = PrinterFont.BODY;
                }

                if (!string.IsNullOrWhiteSpace(Empresa.EmpDireccion))
                {
                    if (!NoDireccion)
                    {

                        if (Empresa.EmpDireccion.Length > 48)
                        {
                            Char[] SeparateName = Empresa.EmpDireccion.ToCharArray();
                            for (int i = 0; i < SeparateName.Length; i++)
                            {
                                if (SeparateName[i] == 'á')
                                {
                                    SeparateName[i] = 'a';
                                }

                                if (SeparateName[i] == 'é')
                                {
                                    SeparateName[i] = 'e';
                                }

                                if (SeparateName[i] == 'í')
                                {
                                    SeparateName[i] = 'i';
                                }

                                if (SeparateName[i] == 'ó')
                                {
                                    SeparateName[i] = 'o';
                                }

                                if (SeparateName[i] == 'ú')
                                {
                                    SeparateName[i] = 'u';
                                }

                                if (SeparateName[i] == 'º')
                                {
                                    SeparateName[i] = '#';
                                }
                            }
                            string Emp = new string(SeparateName);
                            Empresa.EmpDireccion = Emp;
                            DrawText(Empresa.EmpDireccion,48);
                            //DrawText(Empresa.EmpDireccion.Substring(48, 46));
                        }
                        else
                        {
                            Char[] SeparateName = Empresa.EmpDireccion.ToCharArray();
                            for (int i = 0; i < SeparateName.Length; i++)
                            {
                                if (SeparateName[i] == 'á')
                                {
                                    SeparateName[i] = 'a';
                                }

                                if (SeparateName[i] == 'é')
                                {
                                    SeparateName[i] = 'e';
                                }

                                if (SeparateName[i] == 'í')
                                {
                                    SeparateName[i] = 'i';
                                }

                                if (SeparateName[i] == 'ó')
                                {
                                    SeparateName[i] = 'o';
                                }

                                if (SeparateName[i] == 'ú')
                                {
                                    SeparateName[i] = 'u';
                                }

                                if (SeparateName[i] == 'º')
                                {
                                    SeparateName[i] = '#';
                                }
                            }
                            string Emp = new string(SeparateName);
                            Empresa.EmpDireccion = Emp;
                            DrawText(Empresa.EmpDireccion);
                        }
                    }
                }

                if (language != PrinterLanguageMB.ESCPOS)
                {
                    if (!Notbold || DS_RepresentantesParametros.GetInstance().GetParChangeFontEmpresa().Equals("TITLE"))
                    {
                        if (!NoSpaces)
                        {
                            DrawText("");
                        }
                    }
                }

                DrawText("Telefono: " + Empresa.EmpTelefono);

                if (language != PrinterLanguageMB.ESCPOS)
                {
                    if (!Notbold || DS_RepresentantesParametros.GetInstance().GetParChangeFontEmpresa().Equals("TITLE"))
                    {
                        if (!NoSpaces)
                        {
                            DrawText("");
                        }
                    }
                }
                if (!string.IsNullOrWhiteSpace(Empresa.EmpDireccion2))
                {
                    DrawText("E-Mail: " + Empresa.EmpDireccion2);
                }
                if (language != PrinterLanguageMB.ESCPOS && !string.IsNullOrWhiteSpace(Empresa.EmpDireccion2))
                {
                    if (!Notbold || DS_RepresentantesParametros.GetInstance().GetParChangeFontEmpresa().Equals("TITLE"))
                    {
                        if (!NoSpaces)
                        {
                            DrawText("");
                        }
                    }
                }

            }
            Font = PrinterFont.BODY;
            //FontSize = 0;
            Bold = false;
        }




        /*ESTE METODO DEBE SER LLAMADO DESDE UN THREAD SECUNDARIO DE LO CONTRARIO NO FUNCIONA.*/
        public void Print(int W = 370, int H = 180)
        {
            if (IsEscPosForiOS)
            {
                PrintEscPosIniOS();
                return;
            }

            if (Conn == null)
            {
                return;
            }

            Exception error = null;

            try
            {
                DocumentCPCL += "PRINT\r\n";

                DocumentCPCL = DocumentCPCL.Replace("@PAPERSIZE@", (DocumentY + 60).ToString());
                DocumentCPCL = DocumentCPCL.Replace("@COPIAS@", "1");
                DocumentCPCL = DocumentCPCL.Replace("@PageWidth@", PageWidth.ToString());

                Task.Delay(500).Wait();
                if(!Conn.IsConnected)
                {                    
                    Conn.Open();
                }               

                if (language == PrinterLanguageMB.ESCPOS)
                {
                    DrawText("");
                    DrawText("");
                    DrawText("");

                    Task.Delay(500).Wait();

                    if (Empresa != null && Empresa.EmpLogo != null && (Empresa.EmpLogo).Length > 0 && PrintEmpLogo)
                    {
                        //Conn.Write(FEED_LINE);
                        //Task.Delay(100).Wait();
                        //Conn.Write(SELECT_BIT_IMAGE_MODE);
                        //Task.Delay(100).Wait();
                        //Conn.Write((byte[])Image);
                        //Task.Delay(200).Wait();*/
                        PrintImage(Empresa.EmpLogo, Conn, 120, W : W, H : H);

                        Task.Delay(100);

                    }

                    foreach (var data in DocumentESCPOS)
                    {
                        Task.Delay(50).Wait();
                        Conn.Write(data);
                    }

                    Conn.Write(new byte[] { 27, 64 });
                    Conn.Write(new byte[] { 0x1B, 0x21, 0x01 });
                    Conn.Write(new byte[] { 0x1b, Convert.ToByte('a'), 0x00 });                   
                }
                else
                {
                    Task.Delay(500).Wait();

                    if (Empresa != null && Empresa.EmpLogo != null && Empresa.EmpLogo.Length > 0 && PrintEmpLogo)
                    {
                        PrintImage(Empresa.EmpLogo, Conn, 120, W : W, H : H);
                    }

                   // string[] data = DocumentCPCL.Split(new string[] { "@IMAGE@" }, StringSplitOptions.None);
                   
                    Conn.Write(Encoding.ASCII.GetBytes(DocumentCPCL));
                    
                }
            }
            catch (Exception e)
            {
                if(e.Message.Contains("pipe"))
                {
                    Conn = ConnectionBuilder.Current.Build("BT:" + PrinterData.PrinterMac);
                    Print();
                    return;
                }

                error = e;
            }
            finally
            {
                DocumentCPCL = "! 0 200 200 @PAPERSIZE@ @COPIAS@ \r\nON-FEED IGNORE\r\n! U1 PW @PageWidth@\r\n";
                DocumentESCPOS = new List<byte[]>()
                {
                    new byte[] {27, 64}, new byte[] { 0x1B, 0x21, 0x01 }, new byte[] { 0x1b, Convert.ToByte('a'), 0x00 }
                };

                DocumentY = 20;

                if (!DS_RepresentantesParametros.GetInstance().GetParInitPrinterManager() && Conn.IsConnected)
                {
                    Conn.Close();
                }
            }

            if (error != null)
            {
                throw error;
            }
        }

        public async static void ConnToClose()
        {
            await Task.Run(() => 
            {
                if (Conn == null)
                {
                    return;
                }
                else if (Conn.IsConnected)
                {
                    Conn.Close();
                }
            });
        }

        public void PrintEscPosIniOS()
        {
            if (iOSConn == null)
            {
                return;
            }

            Exception error = null;

            try
            {
                DocumentCPCL += "PRINT\r\n";

                DocumentCPCL = DocumentCPCL.Replace("@PAPERSIZE@", (DocumentY + 60).ToString());
                DocumentCPCL = DocumentCPCL.Replace("@COPIAS@", "1");
                DocumentCPCL = DocumentCPCL.Replace("@PageWidth@", PageWidth.ToString());

                Task.Delay(500).Wait();

                iOSConn.Open().Wait();

                DrawText("");
                DrawText("");
                DrawText("");

                Task.Delay(500).Wait();

               /* if (Empresa != null && Empresa.EmpLogo != null && (Empresa.EmpLogo).Length > 0 && PrintEmpLogo)
                {
                    PrintImage(Empresa.EmpLogo, Conn, 120);

                    Task.Delay(100).Wait();

                }*/

                foreach (var data in DocumentESCPOS)
                {
                    Task.Delay(50).Wait();
                    iOSConn.Write(data).Wait();
                }

                iOSConn.Write(new byte[] { 27, 64 }).Wait();
                iOSConn.Write(new byte[] { 0x1B, 0x21, 0x01 }).Wait();
                iOSConn.Write(new byte[] { 0x1b, Convert.ToByte('a'), 0x00 }).Wait();
            }
            catch (Exception e)
            {
                error = e;
            }
            finally
            {
                DocumentCPCL = "! 0 200 200 @PAPERSIZE@ @COPIAS@ \r\nON-FEED IGNORE\r\n! U1 PW @PageWidth@\r\n";
                DocumentESCPOS = new List<byte[]>()
                {
                    new byte[] {27, 64}, new byte[] { 0x1B, 0x21, 0x01 }, new byte[] { 0x1b, Convert.ToByte('a'), 0x00 }
                };

                DocumentY = 20;

                if (iOSConn.IsConnected)
                {
                    iOSConn.Close().Wait();
                }
            }

            if (error != null)
            {
                throw error;
            }
        }

        private void PrintImage(byte[] rawImage, IConnection Conn, int Y = 100, string ModifyLogo = "", int W = 370, int H = 180)
        {
            var imageConverter = DependencyService.Get<IPlatformImageConverter>();

            if (language != PrinterLanguageMB.ESCPOS)
            {
                var image = imageConverter.Create(rawImage, W, H);
                var printer = ZebraPrinterFactory.Current.GetInstance(PrinterLanguage.CPCL, Conn);

                if (printer != null)
                {
                    /*if (imgbyte != null) {*/
                    var zplCadena = "";
                    var zplCadena2 = "";

                    zplCadena += "! 0 300 300 300 1\r\n";
                    zplCadena += "LABEL\r\n";
                    zplCadena += "CONTRAST 0\r\n";
                    zplCadena += "TONE 0\r\n";
                    zplCadena += "SPEED 5\r\n";
                    zplCadena += "SETFF 10\r\n";
                    zplCadena += "PAGE-WIDTH 380\r\n";
                    zplCadena += "BAR-SENSE\r\n";
                    zplCadena += ";// PAGE 0000000003800240\r\n";

                    zplCadena2 = "PRINT\r\n";

                    Conn.Write(Encoding.UTF8.GetBytes(zplCadena));

                    if (ModifyLogo.Equals("")) {
                        printer.PrintImage(image, 100, Y, 370, 180, true);
                    }
                    else
                    {
                        var cadenaML = ModifyLogo.Split('|');
                        printer.PrintImage(image, 100, 170, int.Parse(cadenaML[0].ToString()), int.Parse(cadenaML[1].ToString()), true);
                    }

                   // printer.PrintImage(image, 100, 170, 330, 130, true);
                    
                    Conn.Write(Encoding.UTF8.GetBytes(zplCadena2));
                    Task.Delay(500).Wait();
                }
            }
            else
            {
                var image = Arguments.Imagenes;

                if(image == null)
                {
                    return;
                }
                // Conn.Write(SELECT_BIT_IMAGE_MODE);
                //Task.Delay(50).Wait();

               // if (IsEscPosForiOS)
                //{
                    //iOSConn.Write(image as byte[]).Wait();
                    //Task.Delay(500).Wait();
                    //iOSConn.Write(FEED_LINE).Wait();
               // }
               // else
               // {
                    Conn.Write(image as byte[]);
                    Task.Delay(500).Wait();
                    Conn.Write(FEED_LINE);
               // }
  
            }
        }
        
        public void DrawText(string texto, int caracteresXlinea)
        {
            if (texto == null || (Conn == null && !IsEscPosForiOS))
            {
                return;
            }
            texto = Regex.Replace(texto, @"[\r\n]+", "");
            if (texto.Length <= caracteresXlinea)
            {
                DrawText(texto);
            }
            else
            {
                int temp = 0;
             //   int longitud = texto.Length;

                while (temp + caracteresXlinea < texto.Length)
                {
                   
                    DrawText(texto.Substring(temp, caracteresXlinea + temp < texto.Length ? caracteresXlinea :  caracteresXlinea - (texto.Length - temp ) ));
                    temp += caracteresXlinea;
                }

                if (temp != texto.Length)
                {
                    DrawText(texto.Substring(temp));
                }
            }
        }

        public void DrawText(string value)
        {
            try
            {
                if (value == null || (Conn == null && !IsEscPosForiOS))
                {

                    return;
                }


                if (language == PrinterLanguageMB.ESCPOS)
                {
                    value = GetCharacterMapping(value);
                    DocumentESCPOS.Add(Encoding.ASCII.GetBytes(value + "\r\n"));
                    DocumentY += LineHeight;
                    return;
                }

                string textAlign = "LEFT " + PageWidth + "\r\n";

                switch (TextAlign)
                {
                    case Justification.CENTER: textAlign = "CENTER " + PageWidth + "\r\n"; break;
                    case Justification.RIGHT: textAlign = "RIGHT " + PageWidth + "\r\n"; break;
                }

                string strBold;
                if (Bold)
                {
                    strBold = "SETBOLD 1<CR><LF>\r\n";
                }
                else
                {
                    strBold = "SETBOLD 0<CR><LF>\r\n";
                }

                string font = "7";

                string fontSize = "0";

                switch (Font)
                {
                    case PrinterFont.BODY: font = "7"; break;
                    case PrinterFont.TITLE: font = "7"; fontSize = "1"; break;
                    case PrinterFont.FOOTER: font = "0"; fontSize = "0"; break;
                        /*
                        case Font.STANDARD: font = "0"; break;
                        case Font.SCRIPT: font = "1"; break;
                        case Font.OCR_A: font = "2"; break;
                        case Font.UNISON: font = "4"; break;
                        case Font.MANHATAN: font = "5"; break;
                        case Font.MICR: font = "6"; break;
                        case Font.WARWICK: font = "7"; break;*/
                }

                DocumentCPCL += strBold + textAlign + "T " + font + " " + fontSize + " " + DocumentX.ToString() + " " + DocumentY.ToString() + " " + value + "\r\n";
                DocumentY += LineHeight;

            }
            catch(Exception e)
            {
                if (Conn != null && Conn.IsConnected)
                {
                    Conn.Close();
                }

                throw e;
            }
        }

        private string GetCharacterMapping(string value)
        {
            Dictionary<char, char> characterMapping = new Dictionary<char, char>()
            {
                {'á', 'a'},
                {'é', 'e'},
                {'í', 'i'},
                {'ó', 'o'},
                {'ú', 'u'},
                {'ñ', 'n'},
                {'Ñ', 'N'},
                {'º', '#'}
            };

            char[] characters = value.ToCharArray();

            for (int i = 0; i < characters.Length; i++)
            {
                if (characterMapping.ContainsKey(characters[i]))
                {
                    characters[i] = characterMapping[characters[i]];
                }
            }

            string newValue = new string(characters);

            return newValue;
        }

        public void DrawLine()
        {
            if (language == PrinterLanguageMB.ESCPOS)
            {
                DrawText("----------------------------------------------");
                return;
            }

            DocumentCPCL += "LINE 2 " + (DocumentY + 3).ToString() + " " + (PageWidth - 3).ToString() + " " + (DocumentY + 3).ToString() + " 2\r\n";

            DocumentY += LineHeight;
        }

        public void TestPrinter()
        {
            if (Conn == null && !IsEscPosForiOS)
            {
                return;
            }
            
            TextAlign = Justification.CENTER;
            Bold = true;
            DrawText("Impresora configurada");
            Bold = false;
            TextAlign = Justification.RIGHT;
            DrawLine();
            DrawText("Correctamente");

            Print();
        }

        private readonly byte[] SET_LINE_SPACING = new byte[] { 0x1B, 0x33, 24 };
        private readonly byte[] INITIALIZE_PRINTER = new byte[] { 0x1B, 0x40 };
        private readonly byte[] PRINT_AND_FEED_PAPER = new byte[] { 0x0A };
        private readonly byte[] ESC_ALIGN_LEFT = new byte[] { 0x1b, Convert.ToByte('a'), 0x00 };
        private readonly byte[] ESC_ALIGN_RIGHT = new byte[] { 0x1b, Convert.ToByte('a'), 0x02 };
        private readonly byte[] ESC_ALIGN_CENTER = new byte[] { 0x1b, Convert.ToByte('a'), 0x01 };
        public readonly byte[] FEED_LINE = { 10 };

        private readonly byte[] ESC_CANCEL_BOLD = new byte[] { 0x1B, 0x45, 0 };
        byte[] cc = new byte[] { 0x1D, 0x21, 0 };  // 0- normal size text
        //byte[] cc1 = new byte[]{0x1B,0x21,0x00};  // 0- normal size text
        //byte[] ESC_BOLD = new byte[] { 0x1B, 0x45, 1 };  // 1- only bold text
        /*byte[] bb2 = new byte[] { 0x1B, 0x21, 0x20 }; // 2- bold with medium text
        byte[] bb3 = new byte[] { 0x1B, 0x21, 0x10 }; // 3- bold with large text*/
        byte[] ESC_BOLD = new byte[] { 0x1B, 0x21, 0x08 };  // 1- only bold text
        byte[] bb2 = new byte[] { 0x1D, 0x21, 1};
        byte[] E_BOLD = new byte[] { 0x1B, 0x21, 0x10 };
        byte[] E_BOLDv2 = new byte[] { 0x1B, 0x21, 0x30 };
        byte[] E_MIN = new byte[] { 0x1B, 0x21, 0x01 };


        public void DrawBarcode(string BarCodeType, string texto, string orientacion)
        {
            if (language == PrinterLanguageMB.ESCPOS)
            {
                //string ESC = Convert.ToString((char)27);
                //string GS = Convert.ToString((char)29);
                //string center = ESC + "a" + (char)1; //align center
                //string left = ESC + "a" + (char)0; //align left
                //string bold_on = ESC + "E" + (char)1; //turn on bold mode
                //string bold_off = ESC + "E" + (char)0; //turn off bold mode
                //string cut = ESC + "d" + (char)1 + GS + "V" + (char)66; //add 1 extra line before partial cut
                //string initp = ESC + (char)64;


                //Encoding m_encoding = Encoding.GetEncoding("iso-8859-1"); //set encoding for QRCode
                //int store_len = (texto).Length + 3;
                //byte store_pL = (byte)(store_len % 256);
                //byte store_pH = (byte)(store_len / 256);
                ////buffer += initp; //initialize printer
                //string buffer = "";
                //buffer += m_encoding.GetString(new byte[] { 29, 40, 107, 4, 0, 49, 65, 50, 0 });
                //buffer += m_encoding.GetString(new byte[] { 29, 40, 107, 3, 0, 49, 67, 8 });
                //buffer += m_encoding.GetString(new byte[] { 29, 40, 107, 3, 0, 49, 69, 48 });
                //buffer += m_encoding.GetString(new byte[] { 29, 40, 107, store_pL, store_pH, 49, 80, 48 });
                //buffer += texto;
                //buffer += m_encoding.GetString(new byte[] { 29, 40, 107, 3, 0, 49, 81, 48 });

                //buffer += buffer + initp;
                //DocumentESCPOS.Add(Encoding.ASCII.GetBytes(buffer));
                ICommandEmitter e;
                e = new EPSON();
                //BarcodeTypes(e);

                //var buffer = Encoding.ASCII.GetBytes("  ");

                var epson =  e.PrintBarcode(BarcodeType.CODE128, texto);
                DocumentESCPOS.Add(Encoding.ASCII.GetBytes("           "));
                DocumentESCPOS.Add(epson);
                //Conn.Write(Arguments.Imagenes);
                //Conn.Write(new byte[] { 29, 40, 107, 4, 0, 49, 65, 50, 0 });
                //Conn.Write(new byte[] { 29, 40, 107, 3, 0, 49, 67, 8 });
                //Conn.Write(new byte[] { 29, 40, 107, 3, 0, 49, 69, 48 });
                //Conn.Write(new byte[] { 29, 40, 107, store_pL, store_pH, 49, 80, 48 });
                //Conn.Write(new byte[] { 29, 40, 107, 3, 0, 49, 81, 48 });

                //DependencyService.Get<IPlatformImageConverter>().DecodeForEscPosToPrintBarCode(texto, 100, 100);
                //Conn.Write(Arguments.Imagenes as byte[]);
                //Task.Delay(500).Wait();
                //Conn.Write(FEED_LINE);
                //Conn.Write(Encoding.ASCII.GetBytes("\x0a"));           // Beginning line feed
                //Conn.Write(Encoding.ASCII.GetBytes("\x1c\x7d\x25"));  // Start QR Code® command
                //Conn.Write(Encoding.ASCII.GetBytes("\x06"));           // Length of string to follow (6 bytes in this example)
                //Conn.Write(Encoding.ASCII.GetBytes("\xE5\x90\x8C\xE5\x83\x9A")); // 同僚 (Colleague)
                //Conn.Write(Encoding.ASCII.GetBytes("\x0a"));

                //Encoding m_encoding = Encoding.GetEncoding("iso-8859-1"); //set encoding for QRCode
                //int store_len = (QrData).Length + 3;
                //byte store_pL = (byte)(store_len % 256);
                //byte store_pH = (byte)(store_len / 256);
                //buffer += initp; //initialize printer
                //buffer += m_encoding.GetString(new byte[] { 29, 40, 107, 4, 0, 49, 65, 50, 0 });
                //buffer += m_encoding.GetString(new byte[] { 29, 40, 107, 3, 0, 49, 67, 8 });
                //buffer += m_encoding.GetString(new byte[] { 29, 40, 107, 3, 0, 49, 69, 48 });
                //buffer += m_encoding.GetString(new byte[] { 29, 40, 107, store_pL, store_pH, 49, 80, 48 });
                //buffer += QrData;
                //buffer += m_encoding.GetString(new byte[] { 29, 40, 107, 3, 0, 49, 81, 48 });


                return;
            }

            string CodComprobadoBarcode="No Data";
            //tipos de dodigos de barra
            string[] type = { "UPCA", "EAN13", "39", "128", "39C", "F39C" };
            string[] tabla39 = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "-", ".", " ", "$", "/", "+", "%" };
            //se utilizara la tabla 128 B que contienen letras mayusculas, miusculas,numeros, simbolos etc.
            string[] tabla128 ={" ","!","\"","#","$","%","&","'","(",")","*","+",",","-",".","/","0","1","2","3","4","5","6","7","8","9",":",";","<","=",">","?"
                ,"@","A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T","U","V","W","X","Y","Z","[","\\","]","^","_","`","a"
                ,"b","c","d","e","f","g","h","i","j","k","l","m","n","o","p","q","r","s","t","u","v","w","x","y","z","{","|","}","~","DEL","FNC 3","FNC 2","Shift A","99","FNC 4","101","FNC 1","103","104","105","       "};
            //Orientaciones -> V=vertical   H=horizontal
            string BcOrientacion = "";
            string textoOrentacion = "";
            string radio = "1";
            var Xposicion = DocumentX;
            var YPosicion = DocumentY + 20;
            string width = "2";//1
            string height = "80";//50
            int barx = Xposicion;
            int bary;
            string codigoComprobado = "NO HAY CODIGO";
            if (orientacion == "V")
            {
                BcOrientacion = "VBARCODE";
                textoOrentacion = "VTEXT";
                bary = YPosicion + 100;
            }
            else
            {
                BcOrientacion = "BARCODE";
                textoOrentacion = "TEXT";
                bary = YPosicion;//ypOSICION +50
            }

            int indice = -1;

            for (int i = 0; i < type.Length; i++)
            {
                if (type[i] == BarCodeType)
                {
                    indice = i;
                    break;
                }
            }

            switch (indice)
            {
                case 0:
                    //Calculando el checksum de UPCA
                    try
                    {
                        int pospares = 0;
                        int posimpares = 0;
                        int checksum = 0;
                        int x;

                        char[] codigo = texto.ToCharArray();
                        
                        for (int i = 0; i < codigo.Length; i += 2)
                        {
                            posimpares += int.Parse(codigo[i].ToString());//Integer.parseInt(codigo[i]);
                        }
                        posimpares *= 3;

                        for (int i = 1; i < codigo.Length; i += 2)
                        {
                            pospares += int.Parse(codigo[i].ToString());
                        }

                        x = posimpares + pospares;

                        checksum = (10 - (x % 10));

                        codigoComprobado = texto + (checksum.ToString());
                        CodComprobadoBarcode = codigoComprobado.ToUpper();

                    }
                    catch (Exception e)
                    {
                        Console.Write(e.Message);
                        // TODO: handle exception
                    }
                    break;

                case 1:// calculando el checksum EAN13
                    try
                    {
                        int pospares = 0;
                        int posimpares = 0;
                        int checksum = 0;
                        int x;

                        char[] codigo = texto.ToCharArray();
                        
                        for (int i = 0; i < codigo.Length; i += 2)
                        {
                            posimpares += int.Parse(codigo[i].ToString());
                        }
                        posimpares *= 3;

                        for (int i = 1; i < codigo.Length; i += 2)
                        {

                            pospares += int.Parse(codigo[i].ToString());
                        }

                        x = posimpares + pospares;

                        checksum = (10 - (x % 10));

                        codigoComprobado = texto + checksum.ToString();
                        CodComprobadoBarcode = codigoComprobado;

                    }
                    catch (Exception e)
                    {
                        Console.Write(e.Message);
                        // TODO: handle exception
                    }

                    break;

                case 2: //calculando el checksum Cod 39
                    try
                    {
                        char[] x = texto.ToCharArray();//separa las letras de la palabra o codigo.

                        int i = 0;//indice correspondiente al vector x que almacena cada letra del codigo introducido

                        int[] valores = new int[x.Length];//vector que almacena los numeros correspondientes a cada letra en la tabla

                        int suma = 0;
                        for (i = 0; i < x.Length; i++)
                        {

                            for (int o = 0; o < tabla39.Length; o++)
                            {//o es el indice que corresponde a la tabla39

                                if (tabla39[o].Equals(x[i].ToString()))
                                {
                                    valores[i] = o;
                                    break;
                                }
                            }

                        }
                        for (int j = 0; j < x.Length; j++)
                        {
                            suma += valores[j];
                        }

                        int checksum = suma % 43;
                        codigoComprobado = texto + (tabla128[checksum]);
                        CodComprobadoBarcode = codigoComprobado.ToUpper();

                    }

                    catch (Exception e)
                    {
                        Console.Write(e.Message);
                        // TODO: handle exception
                    }

                    break;

                case 3:// cod 128
                    try
                    {
                        char[] x = texto.ToCharArray();//separa las letras de la palabra o codigo.

                        int i = 0;//indice correspondiente al vector x que almacena cada letra del codigo introducido

                        int[] valores = new int[x.Length];//vector que almacena los numeros correspondientes a cada letra en la tabla


                        for (i = 0; i < x.Length; i++)
                        {

                            for (int o = 0; o < tabla128.Length; o++)
                            {//o es el indice que corresponde a la tabla39

                                if (tabla128[o].Equals(x[i].ToString()))
                                {
                                    valores[i] = o;
                                    break;
                                }
                            }

                        }
                        int suma = 104;
                        //el numero de inicio de la tabla B es 104
                        for (int j = 0; j < x.Length; j++)
                        {
                            suma += (valores[j] * (j + 1));
                        }
                        //int checksum=(suma%104);
                        //CodComprobadoBarcode="B"+texto.concat(tabla128[checksum]);
                        CodComprobadoBarcode = texto;

                    }
                    catch (Exception e)
                    {
                        Console.Write(e.Message);
                        // TODO: handle exception
                    }

                    break;

                case 4:

                    break;

                case 5:

                    break;


                default:
                    break;
            }
            //"@BARCODEORIENTACION@ @BARCODETYPE@ @BARCODEWIDTH@ @BARCODERADIO@ @BARCODEHEIGHT@ @BARCODEX@ @BARCODEY@ @BARCODEDATA@\r\n"+"@TEXTOORIENTACION@ @TEXTOFUENTE@ @TEXTOSIZE@ @TEXTOX@ @TEXTOY@ @TEXTO@\r\n";
            string BarCode = "@BARCODEORIENTACION@ @BARCODETYPE@ @BARCODEWIDTH@ @BARCODERADIO@ @BARCODEHEIGHT@ @BARCODEX@ @BARCODEY@ @BARCODEDATA@\r\n" + "@TEXTOORIENTACION@ @TEXTOFUENTE@ @TEXTOSIZE@ @TEXTOX@ @TEXTOY@ @TEXTO@\r\n";
            BarCode = BarCode.Replace("@BARCODEORIENTACION@", BcOrientacion);// orientacion de la barra (vertical u horizontal)
            BarCode = BarCode.Replace("@BARCODETYPE@", "128");// tipo de codigo de barra (UCPA, EAN13 etc)
            BarCode = BarCode.Replace("@BARCODEWIDTH@", width);// ancho de las barras -> recomendado=1
            BarCode = BarCode.Replace("@BARCODERADIO@", radio);// espacio entre las lineas del codigo de barra. recomendado=1
            BarCode = BarCode.Replace("@BARCODEHEIGHT@", height);// altura de la imagend el codigo de barra
            BarCode = BarCode.Replace("@BARCODEX@",barx.ToString());//posicion X
            BarCode = BarCode.Replace("@BARCODEY@", bary.ToString());// posicion y
            BarCode = BarCode.Replace("@BARCODEDATA@", CodComprobadoBarcode); // DATA DEL CODIGO DE BARRA
            BarCode = BarCode.Replace("@TEXTOORIENTACION@", textoOrentacion);
            BarCode = BarCode.Replace("@TEXTOFUENTE@", "7");//fuente nativa 7
            BarCode = BarCode.Replace("@TEXTOSIZE@", "0");
            BarCode = BarCode.Replace("@TEXTOX@", (Xposicion + 130).ToString());
            BarCode = BarCode.Replace("@TEXTOY@", (YPosicion + 80).ToString());//130
            BarCode = BarCode.Replace("@TEXTO@", texto);

            //Xposicion += 100;
            DocumentY += 100;

            DocumentCPCL += BarCode;
        }

        public PrinterFont GetFontDynamic(string value)
        {
            var font = PrinterFont.BODY;
            switch (value)
            {
                case "BODY":
                    font = PrinterFont.BODY;
                    break;
                case "TITLE":
                    font = PrinterFont.TITLE;
                    break;
                case "FOOTER":
                    font = PrinterFont.FOOTER;
                    break;

            }
            return font;
        }
        public static byte[][] BarcodeTypes(ICommandEmitter e) => new byte[][] {
            e.SetBarcodeHeightInDots(600),
            e.SetBarWidth(BarWidth.Thinnest),
            e.SetBarLabelPosition(BarLabelPrintPosition.Below),

            e.PrintLine("CODABAR_NW_7:"),
            e.PrintBarcode(BarcodeType.CODABAR_NW_7, "A31117013206375B"),

            e.PrintLine("CODE128:"),
            e.PrintBarcode(BarcodeType.CODE128, "ESC_POS_NET"),
            e.PrintLine(),

            e.PrintLine("CODE128 Type C:"),
            e.PrintBarcode(BarcodeType.CODE128, "123456789101", BarcodeCode.CODE_C),
            e.PrintLine(),

            e.PrintLine("CODE39:"),
            e.PrintBarcode(BarcodeType.CODE39, "*ESC-POS-NET*"),
            e.PrintLine(),

            e.PrintLine("CODE93:"),
            e.PrintBarcode(BarcodeType.CODE93, "*ESC_POS_NET*"),
            e.PrintLine(),

            e.PrintLine("GS1_128:"),
            e.PrintBarcode(BarcodeType.GS1_128, "(01)9501234567890*"),
            e.PrintLine(),

            e.PrintLine("GS1_DATABAR_EXPANDED:"),
            e.PrintBarcode(BarcodeType.GS1_DATABAR_EXPANDED, "0001234567890"),
            e.PrintLine(),

            e.PrintLine("GS1_DATABAR_LIMITED:"),
            e.PrintBarcode(BarcodeType.GS1_DATABAR_LIMITED, "0001234567890"),
            e.PrintLine(),

            e.PrintLine("GS1_DATABAR_OMNIDIRECTIONAL:"),
            e.PrintBarcode(BarcodeType.GS1_DATABAR_OMNIDIRECTIONAL, "0001234567890"),
            e.PrintLine(),

            e.PrintLine("GS1_DATABAR_TRUNCATED:"),
            e.PrintBarcode(BarcodeType.GS1_DATABAR_TRUNCATED, "0001234567890"),
            e.PrintLine(),

            e.PrintLine("ITF:"),
            e.PrintBarcode(BarcodeType.ITF, "1234567895"),
            e.PrintLine(),

            e.PrintLine("JAN13_EAN13:"),
            e.PrintBarcode(BarcodeType.JAN13_EAN13, "5901234123457"),
            e.PrintLine(),

            e.PrintLine("JAN8_EAN8:"),
            e.PrintBarcode(BarcodeType.JAN8_EAN8, "96385074"),
            e.PrintLine(),

            e.PrintLine("UPC_A:"),
            e.PrintBarcode(BarcodeType.UPC_A, "042100005264"),
            e.PrintLine(),

            e.PrintLine("UPC_E:"),
            e.PrintBarcode(BarcodeType.UPC_E, "425261"),
            e.PrintLine()
        };
    }
}
