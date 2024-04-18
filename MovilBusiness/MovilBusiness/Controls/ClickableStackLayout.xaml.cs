using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Controls
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ClickableStackLayout : StackLayout
	{
        public static readonly BindableProperty CommandProperty =
           BindableProperty.Create("Command", typeof(ICommand), typeof(ClickableStackLayout), null);

        public object CommandParameter { get; set; }

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public ClickableStackLayout ()
		{
			InitializeComponent ();
		}

        private void OnClick(object sender, EventArgs args)
        {
            Command?.Execute(CommandParameter);
        }

    }
}