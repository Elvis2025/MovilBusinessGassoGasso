using MovilBusiness.Controls.Behavior;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class InputItemView : StackLayout
    {
        public InputItemView()
        {
            InitializeComponent();

            edittext.Behaviors.Add(new NumericValidation(true));
        }

        public void SetTitle(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                lblDescription.Text = "";
                return;
            }
            lblDescription.Text = value.ToUpper();
        }

        public Xamarin.Forms.Entry GetEdit()
        {
            return edittext;
        }

        public Xamarin.Forms.Label GetLabel()
        {
            return lblDescription;
        }

        public void SetReturnType(ReturnType type)
        {
            edittext.ReturnType = type;
        }
    }
}