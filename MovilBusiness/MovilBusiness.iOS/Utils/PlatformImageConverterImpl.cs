using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using MovilBusiness.Abstraction;
using MovilBusiness.iOS.Utils;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(PlatformImageConverterImpl))]
namespace MovilBusiness.iOS.Utils
{
    public class PlatformImageConverterImpl : IPlatformImageConverter
    {
        public object Create(byte[] data, int width, int height)
        {
            var image = new UIImage(NSData.FromArray(data));

            CGSize scaleSize = new CGSize(width, height);
		    UIGraphics.BeginImageContextWithOptions(scaleSize, false, 0);
		    image.Draw(new CGRect(0,0, scaleSize.Width, scaleSize.Height));
		    UIImage resizedImage = UIGraphics.GetImageFromCurrentImageContext();
		    UIGraphics.EndImageContext();

		    return resizedImage.CGImage;
        }

        public object CreateESCPOS(byte[] image, int width, int height)
        {
            return null;
        }

        public  Task DecodeForEscPos(byte[] image, int width, int height)
        {
            return null;
        }

        void IPlatformImageConverter.DecodeForEscPos(byte[] image, int width, int height)
        {
            throw new NotImplementedException();
        }
    }
}