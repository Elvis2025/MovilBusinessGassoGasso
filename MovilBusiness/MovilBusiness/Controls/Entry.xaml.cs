using System;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Controls
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Entry : Xamarin.Forms.Entry
	{
        public static readonly BindableProperty CommandProperty =
            BindableProperty.Create("Command", typeof(ICommand), typeof(Entry), null);

        
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public Entry ()
		{
			InitializeComponent ();
		}

        private void SearchCommand(object sender, EventArgs args)
        {
            Command?.Execute(null);
        }

    }
}