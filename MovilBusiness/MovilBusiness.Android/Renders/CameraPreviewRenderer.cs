using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Hardware.Camera2;
using Android.Hardware.Camera2.Params;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Graphics.Drawable;
using Android.Util;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Java.Nio;
using MovilBusiness.DataAccess;
using MovilBusiness.Droid.Renders;
using MovilBusiness.Droid.utils;
using MovilBusiness.Droid.view.components.dialogs;
using MovilBusiness.Utils;
using MovilBusiness.Views;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;


[assembly: ExportRenderer(typeof(CameraPage), typeof(CameraPreviewRenderer))]
namespace MovilBusiness.Droid.Renders
{
    public class CameraPreviewRenderer : PageRenderer, TextureView.ISurfaceTextureListener, Android.Media.ImageReader.IOnImageAvailableListener, Android.Views.View.IOnTouchListener, IDialogInterfaceOnDismissListener, Android.Views.View.IOnClickListener
    {
        private Android.Views.View view;
        private Activity activity;

        private DS_TransaccionesImagenes myTranImg;

        private static readonly SparseIntArray ORIENTATIONS = new SparseIntArray();
        private Android.Util.Size Previewsize;
        private Android.Util.Size[] JpegSizes = null;

        private CameraDevice CameraDevice;
        private CaptureRequest.Builder PreviewBuilder;
        private CameraCaptureSession PreviewSession;
        DS_RepresentantesParametros myparametro;

        private CameraCallback StateCallBack;
        private TextureView Surface;
        private DialogImageViewer dialogViewer;
        private Android.Widget.ImageButton preview, btnSelectFlashMode;
        private FrameLayout layoutFlash;
        private TextView lblCapturing;

        private Android.Views.Animations.Animation AnimShowFlash, AnimHideFlash;

        private FlashMode CurrentFlashMode = FlashMode.Off;

        private bool SelectingFlashMode = false;

        private string TableName, RepTablaKey;

        private CameraZoomHandler zoom;

        static CameraPreviewRenderer()
        {
            ORIENTATIONS.Append((int)SurfaceOrientation.Rotation0, 90);
            ORIENTATIONS.Append((int)SurfaceOrientation.Rotation90, 0);
            ORIENTATIONS.Append((int)SurfaceOrientation.Rotation180, 270);
            ORIENTATIONS.Append((int)SurfaceOrientation.Rotation270, 180);
        }

        public CameraPreviewRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
        {
            try
            {
                base.OnElementChanged(e);

                if (e.OldElement != null || Element == null || e.NewElement == null)
                {
                    return;
                }

                RepTablaKey = ((CameraPage)e.NewElement).repTablaKey;
                TableName = ((CameraPage)e.NewElement).tableName;

                Init();
                AddView(view);


            }
            catch (Exception ex)
            {
                Functions.DisplayAlert("Error", ex.Message);
            }
        }


        private int CurrentZoom = 1;
        public void Init()
        {
            myparametro = DS_RepresentantesParametros.GetInstance();
            useLegacyCamera = myparametro.GetParUsarCamaraLegacy();
            activity = this.Context as Activity;
            view = activity.LayoutInflater.Inflate(Resource.Layout.camera_preview, this, false);

            Surface = view.FindViewById<TextureView>(Resource.Id.Surface);

            preview = view.FindViewById<Android.Widget.ImageButton>(Resource.Id.preview);
            btnSelectFlashMode = view.FindViewById<Android.Widget.ImageButton>(Resource.Id.btnSelectedFlashMode);
            layoutFlash = view.FindViewById<FrameLayout>(Resource.Id.layoutFlash);
            lblCapturing = view.FindViewById<TextView>(Resource.Id.lblCapturing);

            view.FindViewById(Resource.Id.btnZoomIn).Click += delegate { SetZoom(CurrentZoom + 1, true); };
            view.FindViewById(Resource.Id.btnZoomOut).Click += delegate { if (CurrentZoom > 0) { SetZoom(CurrentZoom - 1, false); } };

            myTranImg = new DS_TransaccionesImagenes();

            MainActivity.Instance.Window.AddFlags(WindowManagerFlags.KeepScreenOn);

            AnimShowFlash = AnimationUtils.LoadAnimation(Context, Resource.Animation.expand_bottom);
            AnimHideFlash = AnimationUtils.LoadAnimation(Context, Resource.Animation.shrink_top);

            PackageManager pm = MainActivity.Instance.PackageManager;

            if (!pm.HasSystemFeature(PackageManager.FeatureCamera))
            {
                ShowAlert("Este dispositivo no posee una camara, no puedes continuar", true);
                return;
            }

            if (TableName == null || string.IsNullOrWhiteSpace(RepTablaKey))
            {
                ShowAlert("No se cargaron los datos de la transaccion, no puedes continuar", true);
                return;
            }

            if (!pm.HasSystemFeature(PackageManager.FeatureCameraFlash))
            {
                btnSelectFlashMode.Visibility = ViewStates.Gone;
                CurrentFlashMode = FlashMode.Off;
            }


            Surface.SetOnClickListener(this);
            preview.SetOnClickListener(this);
            view.FindViewById(Resource.Id.btnTakePicture).SetOnClickListener(this);
            btnSelectFlashMode.SetOnClickListener(this);
            view.FindViewById(Resource.Id.flashModeAuto).SetOnClickListener(this);
            view.FindViewById(Resource.Id.flashModeOff).SetOnClickListener(this);
            view.FindViewById(Resource.Id.flashModeOn).SetOnClickListener(this);
            view.FindViewById(Resource.Id.btnGuardar).SetOnClickListener(this);

            dialogViewer = new DialogImageViewer(activity);

            dialogViewer.SetOnDismissListener(this);

            dialogViewer.OnPictureDelete += (pos) =>
            {
                myTranImg.DeleteTemp(false, TableName, RepTablaKey, pos);

                if (dialogViewer.IsEmpty)
                {
                    preview.SetImageBitmap(null);
                }
            };

            Surface.SurfaceTextureListener = this;

            StateCallBack = new CameraCallback((s) => { CameraDevice = s; StartCamera(); });

            Surface.SetOnTouchListener(this);

            MessagingCenter.Subscribe<string>(this, "StopPreview", (msg) =>
            {
                if (msg != null && msg == "StopPreview")
                {
                    StopPreview();
                }

            });

        }

        [Obsolete]
        public void OnClick(Android.Views.View v)
        {
            try
            {
                if (layoutFlash.Visibility == ViewStates.Visible)
                {
                    layoutFlash.Visibility = ViewStates.Gone;
                    layoutFlash.StartAnimation(AnimHideFlash);
                    btnSelectFlashMode.Alpha = 1f;

                    if (v.Id != Resource.Id.btnSelectedFlashMode)
                    {
                        SelectingFlashMode = false;
                    }
                }

                if (v.Id == Resource.Id.btnTakePicture)
                {
                    TakePicture();
                }
                else if (v.Id == Resource.Id.preview)
                {
                    if (dialogViewer == null || dialogViewer.IsEmpty)
                    {
                        return;
                    }

                    dialogViewer.Show();
                    //MessagingCenter.Send("StartPreview", "StartPreview");
                    StopPreview();
                }
                else if (v.Id == Resource.Id.btnSelectedFlashMode)
                {
                    if (!SelectingFlashMode)
                    {
                        layoutFlash.Visibility = ViewStates.Visible;
                        layoutFlash.StartAnimation(AnimShowFlash);
                        v.Alpha = 0.3f;
                        SelectingFlashMode = true;
                    }
                    else
                    {
                        SelectingFlashMode = false;
                    }
                }
                else if (v.Id == Resource.Id.flashModeAuto)
                {
                    btnSelectFlashMode.SetImageResource(Resource.Drawable.baseline_flash_auto_white_24);
                    CurrentFlashMode = FlashMode.Single;
                    if (useLegacyCamera)
                    {
                        var parameters = _camera.GetParameters();
                        parameters.FlashMode = Android.Hardware.Camera.Parameters.FlashModeAuto;
                        _camera.SetParameters(parameters);
                        _camera.StartPreview();
                        return;
                    }
                    PreviewBuilder.Set(CaptureRequest.FlashMode, (int)FlashMode.Single);
                   
                    ApplySettings();
                }
                else if (v.Id == Resource.Id.flashModeOn)
                {
                    btnSelectFlashMode.SetImageResource(Resource.Drawable.baseline_flash_on_white_24);
                    CurrentFlashMode = FlashMode.Torch;
                    if (useLegacyCamera)
                    {
                        var parameters = _camera.GetParameters();
                        parameters.FlashMode = Android.Hardware.Camera.Parameters.FlashModeOn;
                        _camera.SetParameters(parameters);
                        _camera.StartPreview();
                        return;
                    }

                    PreviewBuilder.Set(CaptureRequest.FlashMode, (int)FlashMode.Torch);
                   
                    ApplySettings();
                }
                else if (v.Id == Resource.Id.flashModeOff)
                {
                    btnSelectFlashMode.SetImageResource(Resource.Drawable.baseline_flash_off_white_24);
                    CurrentFlashMode = FlashMode.Off;
                    if (useLegacyCamera)
                    {
                        var parameters = _camera.GetParameters();
                        parameters.FlashMode = Android.Hardware.Camera.Parameters.FlashModeOff;
                        _camera.SetParameters(parameters);
                        _camera.StartPreview();
                        return;
                    }

                    PreviewBuilder.Set(CaptureRequest.FlashMode, (int)FlashMode.Off);
                   
                    ApplySettings();
                }
                else if (v.Id == Resource.Id.btnGuardar)
                {
                    int value = myparametro.GetParFotosEncuestasCount();

                    if (dialogViewer.IsEmpty)
                    {
                        ShowAlert("Aun no has tomado ninguna foto");
                        return;
                    }
                    else if (dialogViewer.ImageCount < value)
                    {
                        ShowAlert($"Son minimas {value} requeridas fotos");
                        return;
                    }

                    MessagingCenter.Send("SavePictures", "SavePictures");
                }
            } catch (Exception e)
            {
                Functions.DisplayAlert("Error", e.Message);
            }
        }

        private void ShowAlert(string msg, bool Finish = false, string title = "Aviso")
        {
            new AlertDialog.Builder(Context)
                .SetTitle("Aviso")
                .SetMessage(msg)
                .SetPositiveButton("Aceptar", (s, a) =>
                {
                    if (Finish)
                    {
                        MessagingCenter.Send("Finish", "Finish");
                    }

                })
                .SetCancelable(false)
                .Show();
        }

        private void SetPreviewCircular(Bitmap bit)
        {
            try
            {
                preview.SetImageBitmap(bit);

                int srcBitmapWidth = bit.Width;
                int srcBitmapHeight = bit.Height;

                int borderWidth = 12;
                int shadowWidth = 10;

                int dstBitmapWidth = Math.Min(srcBitmapWidth, srcBitmapHeight) + borderWidth * 2;

                //float radius = Math.min(srcBitmapWidth,srcBitmapHeight)/2;

                // Initializing a new bitmap to draw source bitmap, border and shadow
                Bitmap dstBitmap = Bitmap.CreateBitmap(dstBitmapWidth, dstBitmapWidth, Bitmap.Config.Argb8888);

                // Initialize a new canvas
                Canvas canvas = new Canvas(dstBitmap);

                canvas.DrawColor(Android.Graphics.Color.White);

                // Draw the source bitmap to destination bitmap by keeping border and shadow spaces
                canvas.DrawBitmap(bit, (dstBitmapWidth - srcBitmapWidth) / 2, (dstBitmapWidth - srcBitmapHeight) / 2, null);

                Paint paint = new Paint();

                paint.SetStyle(Paint.Style.Stroke);
                paint.StrokeWidth = borderWidth * 2;
                paint.Color = Android.Graphics.Color.White;

                // Draw the border in destination bitmap
                canvas.DrawCircle(canvas.Width / 2, canvas.Height / 2, canvas.Width / 2, paint);

                // Use Paint to draw shadow
                paint.Color = Android.Graphics.Color.LightGray;
                paint.StrokeWidth = shadowWidth;

                // Draw the shadow on circular bitmap
                canvas.DrawCircle(canvas.Width / 2, canvas.Height / 2, canvas.Width / 2, paint);

                RoundedBitmapDrawable roundedBitmapDrawable = RoundedBitmapDrawableFactory.Create(Resources, dstBitmap);

                roundedBitmapDrawable.Circular = true;

                roundedBitmapDrawable.SetAntiAlias(true);

                preview.SetImageDrawable(roundedBitmapDrawable);

            } catch (Exception e)
            {
                Console.Write(e.Message);
            }

        }

        private void TakePictureLegacy()
        {
            if (_camera == null)
            {
                return;
            }

            /* _camera.AutoFocus(new CameraAutoFocusCallback((success, camera) =>
             {
                 Task.Run(() => { _camera.TakePicture(null, null, new PictureCallback((data, cam) =>
                 {
                     try
                     {
                         SaveImage(data);
                     }catch(Exception e)
                     {
                         Console.Write(e.Message);
                     }
                 }));
                 });
             }));*/

            try
            {
                _camera.TakePicture(null, null, new PictureCallback((data, cam) =>
                {
                    try
                    {
                        SaveImage(data);
                    }
                    catch (Exception e)
                    {
                        Device.BeginInvokeOnMainThread(() => { Functions.DisplayAlert("Error capturando foto", e.Message); });
                    }

                    OpenCamera();
                }));
            }catch(Exception e)
            {
                Functions.DisplayAlert("Aviso", e.Message);
            }

        }

        private void TakePicture()
        {
            try
            {
                if (useLegacyCamera)
                {
                    TakePictureLegacy();
                    return;
                }

                if (CameraDevice == null)
                {
                    return;
                }

                lblCapturing.Visibility = ViewStates.Visible;

                CameraManager manager = (CameraManager)Context.GetSystemService(Context.CameraService);


                CameraCharacteristics characteristics = manager.GetCameraCharacteristics(CameraDevice.Id);

                if (characteristics == null)
                {
                    JpegSizes = (characteristics.Get(CameraCharacteristics.ScalerStreamConfigurationMap) as StreamConfigurationMap).GetOutputSizes((int)ImageFormatType.Jpeg);
                }

                int width = 640, height = 480;

                if (JpegSizes != null && JpegSizes.Length > 0)
                {
                    width = JpegSizes[0].Width;
                    height = JpegSizes[0].Height;
                }

                ImageReader reader = ImageReader.NewInstance(width, height, ImageFormatType.Jpeg, 1);

                List<Surface> outputSurface = new List<Surface>(2)
                {
                    reader.Surface,
                    new Surface(Surface.SurfaceTexture)
                };

                CaptureRequest.Builder captureBuilder = CameraDevice.CreateCaptureRequest(CameraTemplate.StillCapture);
                captureBuilder.AddTarget(reader.Surface);
                captureBuilder.Set(CaptureRequest.ControlMode, (int)ControlMode.Auto);

                int rotation = (int)MainActivity.Instance.WindowManager.DefaultDisplay.Rotation;

                captureBuilder.Set(CaptureRequest.JpegOrientation, ORIENTATIONS.Get(rotation));
                captureBuilder.Set(CaptureRequest.ControlAeMode, (int)ControlAEMode.On);

                captureBuilder.Set(CaptureRequest.ControlSceneMode, (int)ControlSceneMode.Night);
                captureBuilder.Set(CaptureRequest.ControlAwbMode, (int)ControlAwbMode.Auto);
                captureBuilder.Set(CaptureRequest.ControlAeLock, false);
                captureBuilder.Set(CaptureRequest.FlashMode, (int)CurrentFlashMode);
                captureBuilder.Set(CaptureRequest.ControlAfMode, (int)ControlAFMode.ContinuousPicture);
                captureBuilder.Set(CaptureRequest.ColorCorrectionGains, new RggbChannelVector(86, 86, 86, 86));

                zoom.SetZoom(captureBuilder, CurrentZoom);

                HandlerThread handlerThread = new HandlerThread("takePicture");
                handlerThread.Start();

                Handler handler = new Handler(handlerThread.Looper);

                reader.SetOnImageAvailableListener(this, handler);

                var captureCall = new CaptureCallback((s, r, d) => { StartCamera(); });

                CameraDevice.CreateCaptureSession(outputSurface, new SessionCallBack((s) =>
                {
                    try
                    {
                        s.Capture(captureBuilder.Build(), captureCall, handler);
                    }
                    catch (Exception e) { Console.Write(e.Message); }
                }), handler);

            }
            catch (Exception e)
            {
                ShowAlert(e.Message, false, "Error tomando foto");
            }
        }

        private bool useLegacyCamera;
        private Android.Hardware.Camera _camera;
        private void OpenCamera()
        {

            try
            {

                if (useLegacyCamera)
                {
                    _camera = Android.Hardware.Camera.Open();

                    if (_camera == null)
                        _camera = Android.Hardware.Camera.Open(0);

                    var previewSize = _camera.GetParameters().PreviewSize;
                    Surface.LayoutParameters =
                        new FrameLayout.LayoutParams(previewSize.Width, previewSize.Height, GravityFlags.Center);

                    var parameters = _camera.GetParameters();
                    switch (CurrentFlashMode)
                    {
                        case FlashMode.Off:
                            parameters.FlashMode = Android.Hardware.Camera.Parameters.FlashModeOff;
                            break;
                        case FlashMode.Single:
                            parameters.FlashMode = Android.Hardware.Camera.Parameters.FlashModeAuto;
                            break;
                        case FlashMode.Torch:
                            parameters.FlashMode = Android.Hardware.Camera.Parameters.FlashModeOn;
                            break;
                    }


                    try
                    {
                        if (parameters.FocusMode != Android.Hardware.Camera.Parameters.FocusModeContinuousPicture)
                        {
                            parameters.FocusMode = Android.Hardware.Camera.Parameters.FocusModeContinuousPicture;

                            if (parameters.MaxNumFocusAreas > 0)
                            {
                                parameters.FocusAreas = null;
                            }

                        }
                        _camera.SetParameters(parameters);
                        _camera.SetPreviewTexture(Surface.SurfaceTexture);
                        _camera.StartPreview();                 
                    }
                    catch (Java.IO.IOException ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                    Surface.Rotation = 90.0f;

                    return;
                }

                CameraManager manager = (CameraManager)Context.GetSystemService(Context.CameraService);

                string cameraId = manager.GetCameraIdList()[0];

                CameraCharacteristics characteristics = manager.GetCameraCharacteristics(cameraId);

                zoom = new CameraZoomHandler(characteristics);

                StreamConfigurationMap map = characteristics.Get(CameraCharacteristics.ScalerStreamConfigurationMap) as StreamConfigurationMap;
                Previewsize = map.GetOutputSizes(Java.Lang.Class.FromType(typeof(SurfaceTexture)))[0];
                if (Android.Support.V4.Content.ContextCompat.CheckSelfPermission(Context, Manifest.Permission.Camera) != Android.Content.PM.Permission.Granted)
                {
                    Functions.DisplayAlert("Aviso", "No se tiene permiso para usar la camara");
                    //return;
                }
                manager.OpenCamera(cameraId, StateCallBack, null);
                //Android.Hardware.Camera.Open(cameraId);
            }
            catch (Exception e)
            {
                Functions.DisplayAlert("Error abriendo camara", e.Message);
            }
        }

        private void StartCamera()
        {
            try
            {
                if (CameraDevice == null || !Surface.IsAvailable || Previewsize == null)
                {
                    return;
                }

                MainActivity.Instance.RunOnUiThread(() => { lblCapturing.Visibility = ViewStates.Gone; });

                try
                {
                    PreviewBuilder = CameraDevice.CreateCaptureRequest(CameraTemplate.StillCapture);
                }
                catch (Exception e)
                {
                    Console.Write(e.Message);
                }

                PreviewBuilder.Set(CaptureRequest.ControlMode, (int)ControlMode.Auto);
                PreviewBuilder.Set(CaptureRequest.ControlAeMode, (int)ControlAEMode.On);
                //PreviewBuilder.Set(CaptureRequest.ControlSceneMode, (int)ControlSceneMode.Night);
                PreviewBuilder.Set(CaptureRequest.ControlAwbMode, (int)ControlAwbMode.Auto);
                PreviewBuilder.Set(CaptureRequest.ControlAeLock, false);
                PreviewBuilder.Set(CaptureRequest.FlashMode, (int)CurrentFlashMode);
                PreviewBuilder.Set(CaptureRequest.ControlAeExposureCompensation, 6);
                PreviewBuilder.Set(CaptureRequest.ControlAfMode, (int)ControlAFMode.ContinuousPicture);
                PreviewBuilder.Set(CaptureRequest.ColorCorrectionGains, new RggbChannelVector(86, 86, 86, 86));

                SurfaceTexture texture = Surface.SurfaceTexture;

                if (texture == null || PreviewBuilder == null)
                {
                    return;
                }

                texture.SetDefaultBufferSize(Previewsize.Width, Previewsize.Height);
                Surface surface = new Surface(texture);

                PreviewBuilder.AddTarget(surface);

                try
                {
                    CameraDevice.CreateCaptureSession(new List<Surface>() { surface }, new SessionCallBack((s) => { PreviewSession = s; SetChangedPreview(); }), new Handler());
                }
                catch (Exception e)
                {
                    Console.Write(e.Message);
                }


            } catch (Exception e)
            {
                Functions.DisplayAlert("Error inicializando camara", e.Message);
            }

        }

        private void ApplySettings()
        {
            try
            {
                PreviewSession.SetRepeatingRequest(PreviewBuilder.Build(), null, null);
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
        }

        private void SetChangedPreview()
        {
            try
            {
                if (CameraDevice == null)
                {
                    return;
                }

                int rotation = (int)MainActivity.Instance.WindowManager.DefaultDisplay.Rotation;
                // PreviewBuilder.Set(CaptureRequest.JpegOrientation, ORIENTATIONS.Get(rotation));


                HandlerThread thread = new HandlerThread("changed Preview");
                thread.Start();
                Handler handler = new Handler(thread.Looper);

                PreviewSession.SetRepeatingRequest(PreviewBuilder.Build(), null, handler);
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

        }

        public void StopPreview()
        {
            try
            {
                if (CameraDevice != null)
                {
                    CameraDevice.Close();
                }

                if (useLegacyCamera && _camera != null)
                {
                    _camera.StopPreview();
                    _camera.Release();
                    _camera = null;
                }
            } catch (Exception e)
            {
                Console.Write(e.Message);
            }
        }

        private bool IsBusy;
        private void SetZoom(int value, bool In)
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;
            try
            {
                if (useLegacyCamera)
                {
                    if(_camera == null)
                    {
                        return;
                    }

                    var args = _camera.GetParameters();

                    if (!args.IsZoomSupported)
                    {
                        return;
                    }

                    var maxZoom = args.MaxZoom;

                    var currentZoom = args.Zoom;

                    if (In && currentZoom < maxZoom)
                    {
                        currentZoom++;
                    }
                    else if (currentZoom > 0)
                    {
                        currentZoom--;
                    }

                    args.Zoom = currentZoom;
                    CurrentZoom = currentZoom;
                    _camera.SetParameters(args);
                    _camera.SetPreviewTexture(Surface.SurfaceTexture);

                    _camera.StopPreview();

                    /*
                     var display = activity.WindowManager.DefaultDisplay;
        if (display.Rotation == SurfaceOrientation.Rotation0)
        {
            camera.SetDisplayOrientation(90);
        }

        if (display.Rotation == SurfaceOrientation.Rotation270)
        {
            camera.SetDisplayOrientation(180);
        }
                     */

                    _camera.StartPreview();


                }
                else
                {
                    if(zoom == null)
                    {
                        return;
                    }

                    var newZoom = zoom.SetZoom(PreviewBuilder, value);

                    if(newZoom != -1)
                    {
                        CurrentZoom = (int)newZoom;
                    }

                    ApplySettings();
                }
            }catch(Exception e)
            {
                Console.Write(e.Message);
            }

            IsBusy = false;
        }

        public void OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
        {
            OpenCamera();
        }

        public bool OnSurfaceTextureDestroyed(SurfaceTexture surface)
        {
            if (useLegacyCamera && _camera != null)
            {
                _camera.StopPreview();
                _camera.Release();
                _camera = null;
            }
            return true;
        }

        public void OnSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height)
        {
            if (useLegacyCamera)
            {
                OpenCamera();
            }
        }

        public void OnSurfaceTextureUpdated(SurfaceTexture surface)
        {
            
        }

        private void SaveImage(byte[] bytes)
        {
            Bitmap raw = BitmapFactory.DecodeByteArray(bytes, 0, bytes.Length, new BitmapFactory.Options() { InSampleSize = 3 });

            int width = raw.Width;
            int height = raw.Height;
            float scaleWidth = ((float)315) / width;
            float scaleHeight = ((float)300) / height;
            // CREATE A MATRIX FOR THE MANIPULATION
            Matrix matrix = new Matrix();
            // RESIZE THE BIT MAP
            matrix.PostScale(scaleWidth, scaleHeight);

            if (useLegacyCamera)
            {
                var rotMatrix = new Matrix();
                rotMatrix.PostRotate(90f);
                raw = Bitmap.CreateBitmap(raw, 0, 0, raw.Width, raw.Height, rotMatrix, true);

                width = raw.Width;
                height = raw.Height;

                bytes = BitmapToByte(raw);
            }

            // "RECREATE" THE NEW BITMAP
            raw = Bitmap.CreateBitmap(raw, 0, 0, width, height, matrix, false);

            ///raw = droidFunciones.ResizeBitmap(raw, 315, 300);

            myTranImg.SaveImagenInTemp(bytes, TableName, RepTablaKey);

            MainActivity.Instance.RunOnUiThread(() =>
            {
                try
                {
                    SetPreviewCircular(raw);
                    dialogViewer.AddPicture(raw);
                }
                catch (Exception e)
                {
                    ShowAlert(e.Message, false, "Error tomando foto");
                }
            });
        }
        public void OnImageAvailable(ImageReader reader)
        {

            Android.Media.Image image = null;

            try
            {
                image = reader.AcquireLatestImage();
                ByteBuffer buffer = image.GetPlanes()[0].Buffer;

                byte[] bytes = new byte[buffer.Capacity()];
                buffer.Get(bytes);

                SaveImage(bytes);
            }
            catch (Exception e)
            {
                ShowAlert(e.Message, false, "Error tomando foto");
            }
        }

        private byte[] BitmapToByte(Bitmap raw)
        {
            MemoryStream stream = new MemoryStream();
            raw.Compress(Bitmap.CompressFormat.Jpeg, 100, stream);
            var bytes = stream.ToArray();

            return bytes;
        }

        private bool manualFocus = false;

        public bool OnTouch(Android.Views.View v, MotionEvent e)
        {
            try
            {
                if (!(v is TextureView) || useLegacyCamera)
                {
                    return false;
                }

                MotionEventActions actionMasked = e.ActionMasked;

                if (actionMasked != MotionEventActions.Down)
                {
                    return false;
                }

                if (manualFocus)
                {
                    return true;
                }

                CameraManager manager = (CameraManager)Context.GetSystemService(Context.CameraService);
                var characteristics = manager.GetCameraCharacteristics(CameraDevice.Id);

                Rect sensorArraySize = (Rect)characteristics.Get(CameraCharacteristics.SensorInfoActiveArraySize);

                int y = (int)((e.XPrecision / (float)v.Width) * (float)sensorArraySize.Height());
                int x = (int)((e.YPrecision / (float)v.Height) * (float)sensorArraySize.Width());
                int halfTouchWidth = 150;
                int halfTouchHeight = 150;

                MeteringRectangle focusAreaTouch = new MeteringRectangle(Math.Max(x - halfTouchWidth, 0), Math.Max(y - halfTouchHeight, 0), halfTouchWidth * 2, halfTouchHeight * 2, MeteringRectangle.MeteringWeightMax - 1);

                PreviewSession.StopRepeating();

                CaptureCallback callback = new CaptureCallback((session, request, result) =>
                {
                    manualFocus = false;

                    if (request != null && request.Tag != null && request.Tag.ToString() == "FOCUS_TAG")
                    {
                        PreviewBuilder.Set(CaptureRequest.ControlAfTrigger, null);
                        PreviewSession.SetRepeatingRequest(PreviewBuilder.Build(), null, null);
                    }

                });

                PreviewBuilder.Set(CaptureRequest.ControlAfTrigger, (int)ControlAFTrigger.Cancel);
                PreviewBuilder.Set(CaptureRequest.ControlAfMode, (int)ControlAFMode.Off);

                HandlerThread thread = new HandlerThread("FOCUS_TAG");
                thread.Start();
                Handler handler = new Handler(thread.Looper);

                PreviewSession.Capture(PreviewBuilder.Build(), callback, handler);

                if ((int)characteristics.Get(CameraCharacteristics.ControlMaxRegionsAf) >= 1)
                {
                    PreviewBuilder.Set(CaptureRequest.ControlAfRegions, new MeteringRectangle[] { focusAreaTouch });
                }

                PreviewBuilder.Set(CaptureRequest.ControlMode, (int)ControlMode.Auto);
                PreviewBuilder.Set(CaptureRequest.ControlAfMode, (int)ControlAFMode.Auto);
                PreviewBuilder.Set(CaptureRequest.ControlAfTrigger, (int)ControlAFTrigger.Start);
                PreviewBuilder.SetTag("FOCUS_TAG");

                PreviewSession.Capture(PreviewBuilder.Build(), callback, handler);
                manualFocus = true;

            }catch(Exception ex)
            {
                Console.Write(ex.Message);
            }

            return true;
        }

        public void OnDismiss(IDialogInterface dialog)
        {
            OpenCamera();
        }

        private class SessionCallBack : CameraCaptureSession.StateCallback
        {
            private Action<CameraCaptureSession> OnSessionConfigured;

            public SessionCallBack(Action<CameraCaptureSession> OnSessionConfigured)
            {
                this.OnSessionConfigured = OnSessionConfigured;
            }
            public override void OnConfigured(CameraCaptureSession session)
            {
                OnSessionConfigured?.Invoke(session);
            }

            public override void OnConfigureFailed(CameraCaptureSession session)
            {
                Console.Write(session.ToString());
            }
        }

        private class CaptureCallback : CameraCaptureSession.CaptureCallback
        {
            private readonly Action<CameraCaptureSession, CaptureRequest, TotalCaptureResult> CaptureCompleted;

            public CaptureCallback(Action<CameraCaptureSession, CaptureRequest, TotalCaptureResult> CaptureCompleted)
            {
                this.CaptureCompleted = CaptureCompleted;
            }
            public override void OnCaptureCompleted(CameraCaptureSession session, CaptureRequest request, TotalCaptureResult result)
            {
                CaptureCompleted?.Invoke(session, request, result);
            }
        }

        private class CameraCallback : CameraDevice.StateCallback
        {
            private readonly Action<CameraDevice> OnCameraOpened;

            public CameraCallback(Action<CameraDevice> OnCameraOpened)
            {
                this.OnCameraOpened = OnCameraOpened;
            }

            public override void OnDisconnected(CameraDevice camera)
            {
                Console.Write(camera.ToString());
            }

            public override void OnError(CameraDevice camera, [GeneratedEnum] CameraError error)
            {
                Console.Write(error.ToString());
            }

            public override void OnOpened(CameraDevice camera)
            {
                OnCameraOpened?.Invoke(camera);
            }
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            base.OnLayout(changed, l, t, r, b);

            var msw = MeasureSpec.MakeMeasureSpec(r - l, MeasureSpecMode.Exactly);
            var msh = MeasureSpec.MakeMeasureSpec(b - t, MeasureSpecMode.Exactly);

            view.Measure(msw, msh);
            view.Layout(0, 0, r - l, b - t);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                MessagingCenter.Unsubscribe<string>(this, "StopPreview");
                StopPreview();
            }

            base.Dispose(disposing);
        }
    }
}