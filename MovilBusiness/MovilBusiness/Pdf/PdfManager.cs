using MovilBusiness.Abstraction;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xamarin.Forms;
using ZXing;
using ZXing.Common;
using ZXing.Rendering;

namespace MovilBusiness.Pdf
{
    public class PdfManager : IDisposable
    {
        private const int A4Width = 620;//595;
        private const int A4Height = 850;//842;
        private int Y = 30;
        public static string _seccodigo;
        string Title;
        static bool _aligncenter, _noespacioentrelogo, _noname , _fontsizesmall;
        bool IsFirstTimeToTitle;
        Empresa Empresa;
        public Justification TextAlign { get; set; } = Justification.LEFT;
        public PrinterFont Font { get; set; } = PrinterFont.BODY;

        public bool Bold { get; set; }

        public Xamarin.Forms.Color TextColor 
        {
            set
            {
                try
                {
                    var hex = Functions.ToHexString(value);
                    Paint.Color = SKColor.Parse(hex);
                }
                catch (Exception e)
                {
                    Console.Write(e.Message);
                }
            }
        }
        
          //////////////////////////////////////////////////////////////////////
         ///////////////////////// SK COMPONENTS///////////////////////////////
        //////////////////////////////////////////////////////////////////////
        private SKPaint Paint;
        private SKCanvas Canvas;
        private SKDocument Document;
        private SKFileWStream Stream;

        public string FilePath { get; private set; }

        private PdfManager(string name)
        {
            var appInfo = DependencyService.Get<IAppInfo>();
            var directoryPath = appInfo.DocumentsPath();
            Empresa = new DS_Empresa().GetEmpresa(_seccodigo);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            if (!name.ToLower().EndsWith(".pdf"))
            {
                name += ".pdf";
            }

            FilePath = Path.Combine(directoryPath, name);

            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
            }

            Stream = new SKFileWStream(FilePath);
            Document = SKDocument.CreatePdf(Stream);
            
            Canvas = Document.BeginPage(A4Width, A4Height, SKRect.Create(A4Width, A4Height));
            
            Paint = new SKPaint
            {
                TextSize = 13,
                IsAntialias = true,
                IsLinearText = true,
                Color = SKColor.Parse("#000000"),
                IsStroke = false,
                StrokeWidth = 1.1f,
                TextAlign = SKTextAlign.Left,
                Typeface = SKTypeface.FromFamilyName("Helvetica"),
                TextEncoding = SKTextEncoding.Utf8,
                SubpixelText = true
            };
        }

        public static PdfManager NewDocument(string name, string seccodigo = "", bool aligncenter = false, bool noespacioentrelogo = false, bool noname = false, bool fontsizesmall = false)
        {
            _aligncenter = aligncenter;
            _noespacioentrelogo = noespacioentrelogo;
            _noname = noname;
            _fontsizesmall = fontsizesmall;
            _seccodigo = seccodigo;
            return new PdfManager(name);
        }

        private void CalculateNewPage()
        {
            if (Y >= A4Height - 10)
            {
                Document.EndPage();
                Canvas = Document.BeginPage(A4Width, A4Height, SKRect.Create(A4Width, A4Height));
                PrintEmpresa();            
            }
        }

        public void NewLine() { DrawText(""); }
        public void DrawLine() { Paint.Style = SKPaintStyle.StrokeAndFill; Canvas.DrawLine(25, Y-9, 595, Y-8, Paint); Y += 6; Paint.Style = SKPaintStyle.Fill; CalculateNewPage(); }
        public void DrawText(string text, bool withBorders = false, bool noline = false)
        {

            if(IsFirstTimeToTitle && !string.IsNullOrEmpty(text))
            {
                Title = text;
                IsFirstTimeToTitle = false;
            }

            CalculateNewPage();

            Paint.FakeBoldText = Bold;
            Paint.IsVerticalText = false;

            switch (Font)
            {
                case PrinterFont.BODY:
                    Paint.TextSize = 9;
                    break;
                case PrinterFont.TITLE:
                    Paint.TextSize = 15;
                    break;
                case PrinterFont.MINTITLE:
                    Paint.TextSize = 12;
                    break;
                case PrinterFont.FOOTER:
                    Paint.TextSize = 6;
                    break;
            }

            var x = 25.0f;
            Paint.TextAlign = SKTextAlign.Left;

            switch (TextAlign)
            {
                case Justification.CENTER:
                    Paint.TextAlign = SKTextAlign.Center;
                    x = A4Width / 2f;
                    break;
                case Justification.RIGHT:
                    Paint.TextAlign = SKTextAlign.Right;
                    x = A4Width - 50;
                    break;
                case Justification.CENTERRIGHT:
                    x = A4Width - 275;
                    break;
                case Justification.CENTERRIGHT2:
                    x = A4Width - 150;
                    break;
            }
            
            Canvas.DrawText(text, x, Y, Paint);
            
            if (withBorders)
            {
                SKRect textBounds = new SKRect();
                Paint.MeasureText(text, ref textBounds);
                textBounds.Offset(25, Y);

                SKPaint framePaint = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 1,
                    Color = SKColors.Black
                };

                Canvas.DrawRect(22, textBounds.MidY-7, 620 - 50, textBounds.Height + 8, framePaint);
            }

            if (!noline)
            {
                Y += Font == PrinterFont.FOOTER ? withBorders ? 14 : 12 : withBorders ? 20 : 18;
            }
        }

        public void DrawBarcode(string value)
        {
            var writer = new BarcodeWriter<byte[]>()
            {
                Format = BarcodeFormat.CODE_128,
                //Renderer = new StringRenderer()
            };

            var bytes = writer.Write(value);

            DrawImage(bytes);

        }

        public class StringRenderer : IBarcodeRenderer<string>
        {

            public string Block { get; set; } = "  ";
            public string Empty { get; set; } = "\u2588\u2588";
            public string NewLine { get; set; } = "\n";

            public string Render(BitMatrix matrix, BarcodeFormat format, string content) => Render(matrix, format, content, null);

            public string Render(BitMatrix matrix, BarcodeFormat format, string content, EncodingOptions options)
            {
                var SB = new StringBuilder();
                for (var Y = 0; Y < matrix.Height; Y++)
                {
                    if (Y > 0) SB.Append(NewLine);
                    for (var X = 0; X < matrix.Width; X++) SB.Append(matrix[X, Y] ? Block : Empty);
                }
                return SB.ToString();
            }
        }

        public void DrawTableRow(List<string> cells, bool withBorders = false, double Width = 0, int numtocalular = 0)
        {
            try
            {
                string value = "";

                double cellWidth = 0;

                List<string> largerValues = new List<string>();

                if (Width == 0)
                {
                    cellWidth = (double)A4Width / (double)cells.Count;
                }
                else
                {
                    cellWidth = (double)Width / (double)cells.Count;
                }

                double uni = Paint.MeasureText("p");

                var pad = (int)(cellWidth / uni);

                bool recalculate = false;
                int num = 0;
                foreach (var cell in cells)
                {
                    num++;
                    if (num == numtocalular)
                    {
                        cellWidth = 150;
                        pad = (int)(cellWidth / uni);
                    }//else if(numtocalular > 0)
                    //{
                    //    cellWidth = 60;
                    //}
                    else
                    {
                        cellWidth = 85;
                            
                    }
                    
                    if (cell.Length > pad)
                    {
                        recalculate = true;

                        var raw = cell.Substring(0, pad);

                        while(Paint.MeasureText(raw) < cellWidth)
                        {
                            raw += " ";
                        }

                        value += raw;

                        largerValues.Add(cell.Substring(pad, cell.Length - pad));
                    }
                    else
                    {
                        var raw = cell;

                        if (string.IsNullOrEmpty(raw))
                        {
                            raw = " ";
                        }

                        while (Paint.MeasureText(raw) < cellWidth)
                        {
                            raw += " ";
                        }

                        value += raw;
                        largerValues.Add("");
                    }
                }

                DrawText(value, withBorders);

                if (recalculate)
                {
                    DrawTableRow(largerValues, withBorders);
                }

            }catch(Exception e)
            {
                Console.Write(e.Message);
            }
        }

        public void DrawTableRow2(List<string> cells, bool withBorders = false, double Width = 0, int numtocalular = 0, int descrlength = 0)
        {
            try
            {
                string value = "";

                double cellWidth = 0;

                List<string> largerValues = new List<string>();

                if (Width == 0)
                {
                    cellWidth = (double)A4Width / (double)cells.Count;
                }
                else
                {
                    cellWidth = (double)Width / (double)cells.Count;
                }

                double uni = Paint.MeasureText("p");

                var pad = 38;

                bool recalculate = false;
                int num = 0;
                foreach (var cell in cells)
                {
                    num++;
                    if (num == numtocalular)
                    {
                        cellWidth = 150;

                        if (descrlength > 0)
                            cellWidth = descrlength;

                        pad = (int)(cellWidth / uni);
                    }//else if(numtocalular > 0)
                    //{
                    //    cellWidth = 60;
                    //}
                    else
                    {
                        cellWidth = 75;

                        if (numtocalular != 1)
                        {

                            if (num == 1)
                                cellWidth = 50;

                            if (num == 2)
                                cellWidth = 180;
                        } 
                    }

                    if (cell.Length > pad)
                    {
                        recalculate = true;

                        var raw = cell.Substring(0, pad);

                        while (Paint.MeasureText(raw) < cellWidth)
                        {
                            raw += " ";
                        }

                        value += raw;

                        largerValues.Add(cell.Substring(pad, cell.Length - pad));
                    }
                    else
                    {
                        var raw = cell;

                        if (num > 2)
                        {
                            var align = 10 - cell.Length;
                            raw = cell.PadLeft(align);
                        }

                        if (string.IsNullOrEmpty(raw))
                        {
                            raw = " ";
                        }

                        while (Paint.MeasureText(raw) < cellWidth)
                        {
                            raw += " ";
                        }

                        value += raw;
                        largerValues.Add("");
                    }
                }

                DrawText(value, withBorders);

                if (recalculate)
                {
                    DrawTableRow(largerValues, withBorders);
                }

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
        }

        public void DrawImage(byte[] image, int menosY = 0)
        {
            Y = 30;
            
            //Canvas.DrawBitmap()
            int desiredWidth = 80;

            // create the codec
            SKCodec codec = SKCodec.Create(new SKMemoryStream(image));
            SKImageInfo info = codec.Info;

            // get the scale that is nearest to what we want (eg: jpg returned 512)
            SKSizeI supportedScale = codec.GetScaledDimensions((float)desiredWidth / info.Width);

            // decode the bitmap at the nearest size
            SKImageInfo nearest = new SKImageInfo(supportedScale.Width, supportedScale.Height);
            SKBitmap bmp = SKBitmap.Decode(codec, nearest);

            // now scale that to the size that we want
            float realScale = (float)info.Height / info.Width;
            SKImageInfo desired = new SKImageInfo(desiredWidth, (int)(realScale * desiredWidth));
            bmp = bmp.Resize(desired, SKFilterQuality.High);

            var x = 0.0f;

          /*  if(TextAlign == Justification.CENTER)
            {
                x = (A4Width / 2f) - (bmp.Width / 2);
            }else if(TextAlign == Justification.RIGHT)
            {*/
                x = A4Width - bmp.Width;
            //}
            Y += - menosY;
            Canvas.DrawBitmap(bmp, new SKPoint(x, Y));

            //Y += menosY + 85;
            Y = 30;            
        }

        public void DrawImageForFirma(byte[] image, int menosY = 0)
        {
            //Canvas.DrawBitmap()
            int desiredWidth = 200;

            // create the codec
            SKCodec codec = SKCodec.Create(new SKMemoryStream(image));
            SKImageInfo info = codec.Info;

            // get the scale that is nearest to what we want (eg: jpg returned 512)
            SKSizeI supportedScale = codec.GetScaledDimensions((float)desiredWidth / info.Width);

            // decode the bitmap at the nearest size
            SKImageInfo nearest = new SKImageInfo(supportedScale.Width, supportedScale.Height);
            SKBitmap bmp = SKBitmap.Decode(codec, nearest);

            // now scale that to the size that we want
            float realScale = (float)info.Height / info.Width;
            SKImageInfo desired = new SKImageInfo(desiredWidth, (int)(realScale * desiredWidth));
            bmp = bmp.Resize(desired, SKFilterQuality.High);

            var x = 0.0f;

            if (TextAlign == Justification.CENTER)
            {
                x = (A4Width / 2f) - (bmp.Width / 2);
            }
            else if (TextAlign == Justification.RIGHT)
            {
                x = A4Width - bmp.Width;
            }
            Y = Y - menosY;
            Canvas.DrawBitmap(bmp, new SKPoint(x, Y));

            Y += menosY + 85;
        }

        public void PrintEmpresa(bool nologo = false)
        {
            TextAlign = Justification.CENTER;

            var myParametro = DS_RepresentantesParametros.GetInstance();
            if (Empresa != null && !myParametro.GetParEmpresaNoMostrar())
            {
                if (Empresa != null && Empresa.EmpLogo != null && Empresa.EmpLogo.Length > 0 && !nologo)
                {
                    DrawImage(Empresa.EmpLogo);
                }else
                {
                    Y = 30;
                }
                if (_aligncenter)
                {
                    TextAlign = Justification.CENTER;
                }
                else
                {
                    TextAlign = Justification.LEFT;
                }

                /*if (!noespacioentrelogo)
                {
                    DrawText("");
                    DrawText("");
                }*/
                if (_fontsizesmall)
                {
                    Font = PrinterFont.FOOTER;
                }
                Bold = true;
                if (!_noname)
                {
                    DrawText(Empresa.EmpNombre.ToUpper());
                }
                Bold = false;
                if (Empresa.EmpDireccion != null)
                {
                    DrawText(Empresa.EmpDireccion);
                }
                DrawText("Teléfono: " + Empresa.EmpTelefono);
                DrawText("RNC: " + Empresa.EmpRNC);
                if (!string.IsNullOrEmpty(Empresa.EmpDireccion2))
                {
                    DrawText("E-Mail: " + Empresa.EmpDireccion2);
                }

                if(!string.IsNullOrEmpty(Title))
                {
                    TextAlign = Justification.CENTER;
                    Bold = true;
                    DrawText("");
                    DrawText("");
                    //DrawText(Title);
                    //DrawText("");
                    //DrawText("");
                    Bold = false;
                    TextAlign = Justification.LEFT;
                }
                IsFirstTimeToTitle = true;
            }
            else
            {
                TextAlign = Justification.LEFT;
                Y = 30;
            }
        }


        public void PrintEmpresasWhitoutDir(string sector = "", bool aligncenter = false, bool noespacioentrelogo = false, bool noname = false, bool fontsizesmall = false)
        {
            var Empresa = new DS_Empresa().GetEmpresa(sector);

            TextAlign = Justification.CENTER;

            var myParametro = DS_RepresentantesParametros.GetInstance();
            if (Empresa != null && !myParametro.GetParEmpresaNoMostrar())
            {
                if (Empresa != null && Empresa.EmpLogo != null && Empresa.EmpLogo.Length > 0)
                {
                    DrawImage(Empresa.EmpLogo);
                }
                if (aligncenter)
                {
                    TextAlign = Justification.CENTER;
                }
                else
                {
                    TextAlign = Justification.LEFT;
                }

                if (!noespacioentrelogo)
                {
                    DrawText("");
                    DrawText("");
                }
                if (fontsizesmall)
                {
                    Font = PrinterFont.FOOTER;
                }
                Bold = true;
                if (!noname)
                {
                    DrawText(Empresa.EmpNombre.ToUpper());
                }
                Bold = false;
                DrawText("Teléfono: " + Empresa.EmpTelefono);

            }
        }

        public void Dispose()
        {
            if(Paint != null)
            {
                Paint.Dispose();
                Paint = null;
            }

            if(Canvas != null)
            {
                Canvas.Dispose();
                Canvas = null;
            }

            if(Document != null)
            {
                Document.EndPage();
                Document.Close();
                Document.Dispose();
                Document = null;
            }

            if(Stream != null)
            {
                Stream.Dispose();
                Stream = null;
            }
        }
    }
}
