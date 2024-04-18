using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MovilBusiness.Views.Components.Popup
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PopupDesignRow : PopupPage, INotifyPropertyChanged
    {
        public new event PropertyChangedEventHandler PropertyChanged;
        public string rowscurrent { get; set; }
        public string RowsCurrent { get => rowscurrent; set { rowscurrent = value; RaiseOnPropertyChanged(); } }

        public int NumOfRow = 0;

        public Action<int> OnOptionItemSelected { get; set; }

        public PopupDesignRow(string rowscurrent)
        {
            RowsCurrent = rowscurrent;

            InitializeComponent();

            pickerrows.SelectedItem = rowscurrent;
        }
        public void RaiseOnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void BorderlessPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            switch (pickerrows.SelectedItem.ToString())
            {
                case "Diseño No.1":
                    NumOfRow = 1;
                    break;
                case "Diseño No.2":
                    NumOfRow = 2;
                    break;
                case "Diseño No.3":
                    NumOfRow = 3;
                    break;
                case "Diseño No.5":
                    NumOfRow = 5;
                    break;
                case "Diseño No.6":
                    NumOfRow = 6;
                    break;
                case "Diseño No.7":
                    NumOfRow = 7;
                    break;
                case "Diseño No.9":
                    NumOfRow = 9;
                    break;
                case "Diseño No.10":
                    NumOfRow = 10;
                    break;
                case "Diseño No.11":
                    NumOfRow = 11;
                    break;
                case "Diseño No.12":
                    NumOfRow = 12;
                    break;
                case "Diseño No.13":
                    NumOfRow = 13;
                    break;
                case "Diseño No.19":
                    NumOfRow = 19;
                    break;
                case "Diseño No.20":
                    NumOfRow = 20;
                    break;
                case "Diseño No.26":
                    NumOfRow = 26;
                    break;
                case "Diseño No.28":
                    NumOfRow = 28;
                    break;
                case "Diseño No.29":
                    NumOfRow = 29;
                    break;
                case "Diseño Grid":
                    NumOfRow = 15;
                    break;
                default:
                    NumOfRow = -1;
                    break;
            }

            if(OnOptionItemSelected == null)
            {
                return;
            }
            PopupNavigation.Instance.RemovePageAsync(this);
            OnOptionItemSelected.Invoke(NumOfRow);            

        }
    }
}