using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Resx;
using MovilBusiness.Utils;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TouchTracking;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Modals
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class FirmaModal : ContentPage
	{
        private Dictionary<long, SKPath> inProgressPaths = new Dictionary<long, SKPath>();
        private List<SKPath> completedPaths = new List<SKPath>();
        private SKBitmap CurrentBitmap;

        private DS_TransaccionesImagenes myTraImg;

        private string tableName;
        private int traSecuencia, titId;
        private List<int> traSecuencias;

        public bool obligatory { get; set; }

        public Action signSaved { get; set; }
        public Action signCancel { get; set; }

        private SKPaint paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Black,
            StrokeWidth = 4,
            StrokeCap = SKStrokeCap.Round,
            StrokeJoin = SKStrokeJoin.Round
        };

        public FirmaModal(string tableName, List<int>traSecuencias, int titId)
        {
            this.tableName = tableName;
            this.traSecuencias = traSecuencias;
            this.titId = titId;

            myTraImg = new DS_TransaccionesImagenes();

            BindingContext = this;
            InitializeComponent();
        }

        public FirmaModal(string tableName, int traSecuencia, int titId)
        {
            this.tableName = tableName;
            this.traSecuencia = traSecuencia;
            this.titId = titId;

            myTraImg = new DS_TransaccionesImagenes();

            BindingContext = this;
            InitializeComponent();
        }
        
        void OnTouchEffectAction(object sender, TouchActionEventArgs args)
        {
            switch (args.Type)
            {
                case TouchActionType.Pressed:
                    if (!inProgressPaths.ContainsKey(args.Id))
                    {
                        SKPath path = new SKPath();
                        path.MoveTo(ConvertToPixel(args.Location));
                        inProgressPaths.Add(args.Id, path);
                        UpdateBitmap();
                    }
                    break;

                case TouchActionType.Moved:
                    if (inProgressPaths.ContainsKey(args.Id))
                    {
                        SKPath path = inProgressPaths[args.Id];
                        path.LineTo(ConvertToPixel(args.Location));
                        UpdateBitmap();
                    }
                    break;

                case TouchActionType.Released:
                    if (inProgressPaths.ContainsKey(args.Id))
                    {
                        completedPaths.Add(inProgressPaths[args.Id]);
                        inProgressPaths.Remove(args.Id);
                        UpdateBitmap();
                    }
                    break;

                case TouchActionType.Cancelled:
                    if (inProgressPaths.ContainsKey(args.Id))
                    {
                        inProgressPaths.Remove(args.Id);
                        UpdateBitmap();
                    }
                    break;
            }
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            var canvas = args.Surface.Canvas;
            canvas.Clear();

            SKImageInfo info = args.Info;
            //SKSurface surface = args.Surface;
            

            // Create bitmap the size of the display surface
            if (CurrentBitmap == null)
            {
                CurrentBitmap = new SKBitmap(info.Width, info.Height);
            }
            // Or create new bitmap for a new size of display surface
            else if (CurrentBitmap.Width < info.Width || CurrentBitmap.Height < info.Height)
            {
                SKBitmap newBitmap = new SKBitmap(Math.Max(CurrentBitmap.Width, info.Width),
                                                  Math.Max(CurrentBitmap.Height, info.Height));

                using (SKCanvas newCanvas = new SKCanvas(newBitmap))
                {
                    newCanvas.Clear();
                    newCanvas.DrawBitmap(CurrentBitmap, 0, 0);
                }

                CurrentBitmap = newBitmap;
            }
            
            // Render the bitmap
            canvas.Clear();
            canvas.DrawBitmap(CurrentBitmap, 0, 0);

            if (ClearRequested)
            {
                IsEmpty = true;
                ClearRequested = false;
            }
            else
            {
                IsEmpty = false;
            }

        }

        SKPoint ConvertToPixel(Point pt)
        {
            return new SKPoint((float)(canvasView.CanvasSize.Width * pt.X / canvasView.Width),
                               (float)(canvasView.CanvasSize.Height * pt.Y / canvasView.Height));
        }

        /* private void ShowPreview()
         {
             if(CurrentBitmap == null)
             {
                 return;
             }

             using (SKImage image = SKImage.FromBitmap(CurrentBitmap))
             {
                 SKData data = image.Encode();
                 var source = ImageSource.FromStream(() => { return new MemoryStream(data.ToArray()); });
             }
         }*/

        private void Dismiss(object sender, EventArgs e)
        {
            if (obligatory)
            {
                DisplayAlert(AppResource.Warning, AppResource.SignatureIsRequired, AppResource.Aceptar);
                return;
            }

            signCancel?.Invoke();
            Navigation.PopModalAsync(false);
        }

        private async void AttempSave(object sender, EventArgs args)
        {
            if(IsEmpty)
            {
                await DisplayAlert(AppResource.Warning, AppResource.SignNotTracedYet, AppResource.Aceptar);
                return;
            }

            progressIndicator.IsVisible = true;

            try
            {
                await Task.Run(() => 
                {
                    using (SKImage image = SKImage.FromBitmap(CurrentBitmap))
                    {
                        SKData data = image.Encode();

                        if(traSecuencias != null)
                        {
                            foreach(var secuence in traSecuencias)
                            {
                                saveImg(secuence, data);
                            }
                        }
                        else
                        {
                            saveImg(traSecuencia, data);
                        }                        
                    }
                });

                await DisplayAlert(AppResource.Success, AppResource.SignatureSavedSuccessfully, AppResource.Aceptar);

                signSaved?.Invoke();

                await Navigation.PopModalAsync(true);

            }catch(Exception e)
            {
                await DisplayAlert(AppResource.Error, e.Message, AppResource.Aceptar);
            }

            progressIndicator.IsVisible = false;
        }

        protected override bool OnBackButtonPressed()
        {
            return obligatory;
        }

        private void saveImg(int traSecuencia, SKData data)
        {
            myTraImg.DeleteTemp(true, tableName, traSecuencia.ToString(), -1, false);
            myTraImg.SaveImagenInTemp(data.ToArray(), tableName, traSecuencia.ToString(), true, titId);
            myTraImg.MarkToSendToServer(tableName, traSecuencia.ToString(), true);
        }

        private bool ClearRequested = true;
        private bool IsEmpty = true;
        private void ClearDraw(object sender = null, EventArgs args = null)
        {
            completedPaths.Clear();
            inProgressPaths.Clear();
            UpdateBitmap();
            ClearRequested = true;
        }

        private void UpdateBitmap()
        {
            if(CurrentBitmap == null)
            {
                return;
            }

            using (SKCanvas saveBitmapCanvas = new SKCanvas(CurrentBitmap))
            {
                saveBitmapCanvas.Clear();

                foreach (SKPath path in completedPaths)
                {
                    saveBitmapCanvas.DrawPath(path, paint);
                }

                foreach (SKPath path in inProgressPaths.Values)
                {
                    saveBitmapCanvas.DrawPath(path, paint);
                }
            }

            canvasView.InvalidateSurface();
        }
    }
}