using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AVFoundation;
using CoreGraphics;
using Foundation;
using MovilBusiness.DataAccess;
using MovilBusiness.iOS.renders;
using MovilBusiness.Views;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CameraPage), typeof(CameraPreviewRenderer))]
namespace MovilBusiness.iOS.renders
{
    public class CameraPreviewRenderer : PageRenderer
    {
        private DS_TransaccionesImagenes myTranImg;

        AVCaptureSession captureSession;
        AVCaptureDeviceInput captureDeviceInput;
        AVCaptureStillImageOutput stillImageOutput;
        UIView liveCameraStream;
        UIButton takePhotoButton, previewButton;
        UIButton toggleCameraButton;
        UIButton toggleFlashButton;
        List<ImageSource> imagesPreview;
        UICollectionView prue;

        private string RepTablaKey;
        public string TableName;

        public CameraPreviewRenderer()
        {
            myTranImg = new DS_TransaccionesImagenes();

            CameraPage.PreviewRenderer = new List<ImageSource>();

            CameraPage.PreviewRendererToAdd = new List<byte[]>();

            imagesPreview = new List<ImageSource>();

            MessagingCenter.Subscribe<string>(this, "Start", (msg) =>
            {
                if (msg == "Start")
                {

                    captureSession.StartRunning();

                    if (CameraPage.PreviewRenderer.Count <= 0)
                    previewButton.Hidden = true;                    
                }
            });

        }

        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null || Element == null)
            {
                return;
            }

            RepTablaKey = ((CameraPage)e.NewElement).repTablaKey;
            TableName = ((CameraPage)e.NewElement).tableName;

            try
            {
                SetupUserInterface();
                SetupEventHandlers();
                AuthorizeCameraUse();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"\t\t\tERROR: {ex.Message}");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (captureDeviceInput != null && captureSession != null)
            {
                captureSession.RemoveInput(captureDeviceInput);
            }

            if (captureDeviceInput != null)
            {
                captureDeviceInput.Dispose();
                captureDeviceInput = null;
            }

            if (captureSession != null)
            {
                captureSession.StopRunning();
                captureSession.Dispose();
                captureSession = null;
            }

            if (stillImageOutput != null)
            {
                stillImageOutput.Dispose();
                stillImageOutput = null;
            }

            base.Dispose(disposing);
        }

        void SetupUserInterface()
        {
            var centerButtonX = View.Bounds.GetMidX() - 35f;
            var topLeftX = View.Bounds.X + 25;
            var topRightX = View.Bounds.Right - 65;
            var bottomButtonY = View.Bounds.Bottom - 150;
            var topButtonY = View.Bounds.Top + 15;
            var buttonWidth = 70;
            var buttonHeight = 70;

            liveCameraStream = new UIView()
            {
                Frame = View.Frame
            };          

            toggleCameraButton = new UIButton()
            {
                Frame = new CGRect(topRightX, topButtonY + 5, 35, 26)
            };
            toggleCameraButton.SetBackgroundImage(UIImage.FromBundle("baseline_switch_camera_white_24"), UIControlState.Normal);

            toggleFlashButton = new UIButton()
            {
                Frame = new CGRect(topLeftX, topButtonY, 37, 37)
            };

            toggleFlashButton.SetBackgroundImage(UIImage.FromBundle("baseline_flash_off_white_24"), UIControlState.Normal);

            var topBar = new UIView(new CGRect(0, 0, View.Frame.Width, 65))
            {
                BackgroundColor = new UIColor(0,0,0,0),
             
            };
            topBar.Add(toggleFlashButton);
            topBar.Add(toggleCameraButton);

            var bottomBar = new UIView(new CGRect(0, View.Frame.Height - 165, View.Frame.Width, 165))
            {
                BackgroundColor = new UIColor(0, 0, 0, 0),
            };

            takePhotoButton = new UIButton()
            {
                Frame = new CGRect(centerButtonX, (bottomBar.Frame.Height / 2) - 100, buttonWidth, buttonHeight)
            };

            takePhotoButton.SetBackgroundImage(UIImage.FromBundle("baseline_adjust_white_48"), UIControlState.Normal);

            previewButton = new UIButton()
            {
                Frame = new CGRect(6.5f, (bottomBar.Frame.Height / 2) - 100, buttonWidth, buttonHeight),
            };

            previewButton.Layer.MasksToBounds = true;
            //previewButton.Layer.BorderColor = UIColor.White.CGColor;
            previewButton.Layer.CornerRadius = previewButton.Bounds.Size.Width / 2.0f;
            previewButton.ClipsToBounds = true;
            previewButton.Hidden = true;

            //var imageView = new UIImageView(UIImage.FromBundle("placeholder.png"));

            //var viewcell = new UICollectionViewCell
            //    (new CGRect(0, 0, UIScreen.MainScreen.Bounds.Size.Width, 300)).Add(topBar);

            //collectionView.Source = new UICollectionViewCell().AddSubview(imageView);
            SetupLiveCameraStream();

            View.Add(liveCameraStream);
            bottomBar.Add(takePhotoButton);
            bottomBar.Add(previewButton);
            View.Add(topBar);
            View.Add(bottomBar);
        }

        void SetupEventHandlers()
        {
            takePhotoButton.TouchUpInside += async (object sender, EventArgs e) => {
                await CapturePhoto();
            };

            previewButton.TouchUpInside += (object sender, EventArgs e) => {

                captureSession.StopRunning();

                MessagingCenter.Send("StartPreview", "StartPreview");
            };

            toggleCameraButton.TouchUpInside += (object sender, EventArgs e) => {
                ToggleFrontBackCamera();
            };

            toggleFlashButton.TouchUpInside += (object sender, EventArgs e) => {
                ToggleFlash();
            };
        }

        async Task CapturePhoto()
        {
            var videoConnection = stillImageOutput.ConnectionFromMediaType(AVMediaType.Video);
            var sampleBuffer = await stillImageOutput.CaptureStillImageTaskAsync(videoConnection);
            var jpegImage = AVCaptureStillImageOutput.JpegStillToNSData(sampleBuffer);

            var photo = new UIImage(jpegImage);

            var data = photo.AsJPEG().ToArray();
            previewButton.Hidden = false;
            previewButton.SetBackgroundImage(photo, UIControlState.Normal);

            CameraPage.PreviewRendererToAdd.Add(data);

            CameraPage.PreviewRenderer.Add(ImageSource.FromStream(() => new MemoryStream(data)));

        }

        void ToggleFrontBackCamera()
        {
            var devicePosition = captureDeviceInput.Device.Position;
            if (devicePosition == AVCaptureDevicePosition.Front)
            {
                devicePosition = AVCaptureDevicePosition.Back;
            }
            else
            {
                devicePosition = AVCaptureDevicePosition.Front;
            }

            var device = GetCameraForOrientation(devicePosition);
            ConfigureCameraForDevice(device);

            captureSession.BeginConfiguration();
            captureSession.RemoveInput(captureDeviceInput);
            captureDeviceInput = AVCaptureDeviceInput.FromDevice(device);
            captureSession.AddInput(captureDeviceInput);
            captureSession.CommitConfiguration();
        }

        void ToggleFlash()
        {
            var device = captureDeviceInput.Device;

            var error = new NSError();
            if (device.HasFlash)
            {
                if (device.FlashMode == AVCaptureFlashMode.On)
                {
                    device.LockForConfiguration(out error);
                    device.FlashMode = AVCaptureFlashMode.Off;
                    device.UnlockForConfiguration();
                    toggleFlashButton.SetBackgroundImage(UIImage.FromBundle("baseline_flash_off_white_24"), UIControlState.Normal);
                }
                else
                {
                    device.LockForConfiguration(out error);
                    device.FlashMode = AVCaptureFlashMode.On;
                    device.UnlockForConfiguration();
                    toggleFlashButton.SetBackgroundImage(UIImage.FromBundle("baseline_flash_on_white_24"), UIControlState.Normal);
                }
            }
        }

        AVCaptureDevice GetCameraForOrientation(AVCaptureDevicePosition orientation)
        {
            var devices = AVCaptureDevice.DevicesWithMediaType(AVMediaType.Video);

            foreach (var device in devices)
            {
                if (device.Position == orientation)
                {
                    return device;
                }
            }
            return null;
        }

        void SetupLiveCameraStream()
        {
             captureSession = new AVCaptureSession();
             var viewLayer = new AVCaptureVideoPreviewLayer(captureSession)
             {
                 Frame = View.Bounds,
                 VideoGravity = AVLayerVideoGravity.ResizeAspectFill
             };

             var videoDevices = AVCaptureDevice.DevicesWithMediaType(AVMediaType.Video);
             var cameraPosition = AVCaptureDevicePosition.Back;
             var device = videoDevices.FirstOrDefault(d => d.Position == cameraPosition);

             if (device == null)
             {
                 return;
             }

             stillImageOutput = new AVCaptureStillImageOutput();

             captureDeviceInput = new AVCaptureDeviceInput(device, out NSError error);
             captureSession.AddInput(captureDeviceInput);
             captureSession.AddOutput(stillImageOutput);
             captureSession.CommitConfiguration();
             View.Layer.AddSublayer(viewLayer);
             captureSession.StartRunning();
        }

        void ConfigureCameraForDevice(AVCaptureDevice device)
        {
            var error = new NSError();
            if (device.IsFocusModeSupported(AVCaptureFocusMode.ContinuousAutoFocus))
            {
                device.LockForConfiguration(out error);
                device.FocusMode = AVCaptureFocusMode.ContinuousAutoFocus;
                device.UnlockForConfiguration();
            }
            else if (device.IsExposureModeSupported(AVCaptureExposureMode.ContinuousAutoExposure))
            {
                device.LockForConfiguration(out error);
                device.ExposureMode = AVCaptureExposureMode.ContinuousAutoExposure;
                device.UnlockForConfiguration();
            }
            else if (device.IsWhiteBalanceModeSupported(AVCaptureWhiteBalanceMode.ContinuousAutoWhiteBalance))
            {
                device.LockForConfiguration(out error);
                device.WhiteBalanceMode = AVCaptureWhiteBalanceMode.ContinuousAutoWhiteBalance;
                device.UnlockForConfiguration();
            }
        }

        async void AuthorizeCameraUse()
        {
            var authorizationStatus = AVCaptureDevice.GetAuthorizationStatus(AVMediaType.Video);
            if (authorizationStatus != AVAuthorizationStatus.Authorized)
            {
                await AVCaptureDevice.RequestAccessForMediaTypeAsync(AVMediaType.Video);
            }
        }

        private UIColor GetColorFromHexString(string hexValue)
        {
            hexValue = hexValue.Substring(1, 6); // string will be passed in with a leading #
            var r = Convert.ToByte(hexValue.Substring(0, 2), 16);
            var g = Convert.ToByte(hexValue.Substring(2, 2), 16);
            var b = Convert.ToByte(hexValue.Substring(4, 2), 16);
            return UIColor.FromRGB(r, g, b);
        }

        
    }

}