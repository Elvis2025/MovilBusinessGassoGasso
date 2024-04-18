using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Graphics;
using Java.Lang;
using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.Droid.utils;
using Xamarin.Forms;

[assembly: Dependency(typeof(PlatformImageConverterImpl))]
namespace MovilBusiness.Droid.utils
{
    public class PlatformImageConverterImpl : IPlatformImageConverter
    {
        public object Create(byte[] image, int width, int height)
        {
            Bitmap bitmap = BitmapFactory.DecodeByteArray(image, 0, image.Length);

            Bitmap final = Bitmap.CreateScaledBitmap(bitmap, width, height, false);

            return final;
        }

        private string[] binaryArray = { "0000", "0001", "0010", "0011",
            "0100", "0101", "0110", "0111", "1000", "1001", "1010", "1011",
            "1100", "1101", "1110", "1111" };
        private string hexStr = "0123456789ABCDEF";

        private void decodeBitmap(Bitmap bmp)
        {
            int bmpWidth = bmp.Width;
            int bmpHeight = bmp.Height;

            List<string> list = new List<string>(); //binaryString list
            StringBuffer sb;


            int bitLen = bmpWidth / 8;
            int zeroCount = bmpWidth % 8;

            Java.Lang.String zeroStr = new Java.Lang.String("");
            if (zeroCount > 0)
            {
                bitLen = bmpWidth / 8 + 1;
                for (int i = 0; i < (8 - zeroCount); i++)
                {
                    zeroStr = new Java.Lang.String(zeroStr.ToString() + "0");
                }
            }

            for (int i = 0; i < bmpHeight; i++)
            {
                sb = new StringBuffer();
                for (int j = 0; j < bmpWidth; j++)
                {
                    int color = bmp.GetPixel(j, i);

                    int r = (color >> 16) & 0xff;
                    int g = (color >> 8) & 0xff;
                    int b = color & 0xff;

                    // if color close to white，bit='0', else bit='1'
                    if (r > 160 && g > 160 && b > 160)
                        sb.Append("0");
                    else
                        sb.Append("1");
                }
                if (zeroCount > 0)
                {
                    sb.Append(zeroStr);
                }
                list.Add(sb.ToString());
            }

            var bmpHexList = binaryListToHexStringList(list);

            Java.Lang.String commandHexString = new Java.Lang.String("1D763000");

            Java.Lang.String widthHexString = new Java.Lang.String(Integer
                    .ToHexString(bmpWidth % 8 == 0 ? bmpWidth / 8
                            : (bmpWidth / 8 + 1)));

            if (widthHexString.Length() > 2)
            {
                //Log.e("decodeBitmap error", " width is too large");
                return;
            }
            else if (widthHexString.Length() == 1)
            {
                widthHexString = new Java.Lang.String("0" + widthHexString.ToString());
            }
            widthHexString = new Java.Lang.String(widthHexString.ToString() + "00");

            Java.Lang.String heightHexString = new Java.Lang.String(Integer.ToHexString(bmpHeight));
            if (heightHexString.Length() > 2)
            {
                //Log.e("decodeBitmap error", " height is too large");
                return;
            }
            else if (heightHexString.Length() == 1)
            {
                heightHexString = new Java.Lang.String("0" + heightHexString.ToString());
            }
            heightHexString = new Java.Lang.String(heightHexString.ToString() + "00");

            List<string> commandList = new List<string>();
            commandList.Add((commandHexString.ToString() + widthHexString.ToString() + heightHexString.ToString()));
            commandList.AddRange(bmpHexList);
            Arguments.Imagenes = hexList2Byte(commandList);
        }

        private List<string> binaryListToHexStringList(List<string> list)
        {
            List<string> hexList = new List<string>();
            foreach(var binaryStr in list)
            {
                StringBuffer sb = new StringBuffer();
                for (int i = 0; i < binaryStr.Length; i += 8)
                {
                    string str = binaryStr.Substring(i, 8);
                    
                    Java.Lang.String hexString = myBinaryStrToHexString(str);
                    sb.Append(hexString);
                }
                hexList.Add(sb.ToString());
            }
            return hexList;

        }

        private byte[] hexList2Byte(List<string> list)
        {
            List<byte[]> commandList = new List<byte[]>();

            foreach(var hexStr in list)
            {
                commandList.Add(hexStringToBytes(hexStr));
            }
            byte[] bytes = sysCopy(commandList);
            return bytes;
        }

        private byte[] hexStringToBytes(string hexString)
        {
            if (hexString == null || hexString.Equals(""))
            {
                return null;
            }
            hexString = hexString.ToUpper();
            int length = hexString.Length / 2;
            char[] hexChars = hexString.ToCharArray();
            byte[] d = new byte[length];
            for (int i = 0; i < length; i++)
            {
                int pos = i * 2;
                d[i] = (byte)(charToByte(hexChars[pos]) << 4 | charToByte(hexChars[pos + 1]));
            }
            return d;
        }

        private byte charToByte(char c)
        {
            return (byte)new Java.Lang.String("0123456789ABCDEF").IndexOf(c);
        }

        private byte[] sysCopy(List<byte[]> srcArrays)
        {
            int len = 0;
            foreach(byte[] srcArray in srcArrays)
            {
                len += srcArray.Length;
            }
            byte[] destArray = new byte[len];
            int destLen = 0;
            foreach(byte[] srcArray in srcArrays)
            {
                Array.Copy(srcArray, 0, destArray, destLen, srcArray.Length);
                destLen += srcArray.Length;
            }
            return destArray;
        }

        public Java.Lang.String myBinaryStrToHexString(string binaryStr)
        {
            string hex = "";
            string f4 = binaryStr.Substring(0, 4);
            string b4 = binaryStr.Substring(4, 4);

            for (int i = 0; i < binaryArray.Length; i++)
            {
                if (f4.Equals(binaryArray[i]))
                    hex += hexStr.Substring(i, 1);
            }
            for (int i = 0; i < binaryArray.Length; i++)
            {
                if (b4.Equals(binaryArray[i]))
                    hex += hexStr.Substring(i, 1);
            }

            return new Java.Lang.String(hex);
        }

        public void DecodeForEscPos(byte[] image, int width, int height)
        {
            var final = CreateESCPOS(image, width, height) as Bitmap;
            decodeBitmap(final);
        }

        public object CreateESCPOS(byte[] image, int width, int height)
        {
            Bitmap bitmap = BitmapFactory.DecodeByteArray(image, 0, image.Length);

            Bitmap.Config conf = Bitmap.Config.Argb8888;
            Bitmap final = Bitmap.CreateBitmap(width, height, conf);
            Canvas canvas = new Canvas(final);
            canvas.DrawColor(Android.Graphics.Color.White);
            canvas.DrawBitmap(bitmap, null, new RectF(170, 0, width, height), null);

            return final;
        }
    }
}