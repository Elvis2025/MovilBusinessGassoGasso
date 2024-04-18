using MovilBusiness.viewmodel;
using Xamarin.Forms;

namespace MovilBusiness.Controls
{
	public partial class BorderlessPicker : Picker
    {
        public bool ManualDefault { get; set; } = false;

		public BorderlessPicker ()
		{
			//InitializeComponent ();

            //  SelectedIndex = 0;
		}

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            /*if(!(BindingContext is PedidosViewModel))
            {
                SelectedIndex = 0;
            }*/
            if (!ManualDefault)
            {
                if(SelectedIndex == -1)
                    SelectedIndex = 0;
            }
        }
    }
}