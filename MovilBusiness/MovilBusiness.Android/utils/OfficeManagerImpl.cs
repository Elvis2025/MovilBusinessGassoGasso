using System;
using Android.Content;
using Java.IO;
using MovilBusiness.Abstraction;
using MovilBusiness.Droid.utils;
using Xamarin.Forms;

[assembly: Dependency(typeof(OfficeManagerImpl))]
namespace MovilBusiness.Droid.utils
{
    public class OfficeManagerImpl : IOfficeManager
    {
        public void OpenPowerPoint(string fileName)
        {
            try
            {
                Intent intent = new Intent();
                intent.SetAction(Intent.ActionView);

                File directory = new File(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/external_sd/documents/");

                if (!fileName.ToLower().Contains(".pptx"))
                {
                    fileName += ".pptx";
                }

                File file = new File(directory, fileName);
                
                intent.SetData(Android.Net.Uri.FromFile(file));

                if (file.Exists())
                {
                    try
                    {
                        MainActivity.Instance.StartActivity(intent);
                    }
                    catch (Exception e)
                    {
                        System.Console.Write(e.Message);
                    }
                }
            }catch(Exception e)
            {
                System.Console.Write(e.Message);
            }
        }
    }
}