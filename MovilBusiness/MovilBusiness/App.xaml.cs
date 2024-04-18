using DLToolkit.Forms.Controls;
using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.Resx;
using MovilBusiness.views;
using System;
using System.Globalization;
using Xamarin.Forms;/*
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;*/

namespace MovilBusiness
{
	public partial class App : Application
	{
		public App (IPlatform platform)
		{
            Arguments.PlatformService = platform;

            AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;

            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CreateSpecificCulture("en-US");

            Device.SetFlags(new string[] { "RadioButton_Experimental" });

            InitializeComponent();

            SQLitePCL.Batteries_V2.Init();

            FlowListView.Init();
        }

        private void HandleUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = args.ExceptionObject as Exception;

            // log won't be available, because dalvik is destroying the process
            //Log.Debug (logTag, "MyHandler caught : " + e.Message);
            // instead, your err handling code shoudl be run:
            Console.WriteLine("========= MyHandler caught : " + e.Message);
        }

        protected override void OnStart ()
		{
           /*AppCenter.Start("android=46611ca0-a554-46bf-ba3c-a8a25c4259a9;" +
                        "ios={75971011-fa3a-440b-b65f-142fb959fb4c}",
                  typeof(Analytics), typeof(Crashes));*/

            CheckLogin();
        }

        private void CheckLogin()
        {
            /*var info = new CultureInfo("es-ES");

            AppResources.Culture = info;*/

            MainPage = new NavigationPage(new Views.LoginPage())
            {
                BarBackgroundColor = Color.FromHex("#1976D2"),
                BarTextColor = Color.White
            };
        }

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
	}
}
