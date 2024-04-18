
using MovilBusiness.DataAccess;
using MovilBusiness.model;
using MovilBusiness.model.Internal;
using MovilBusiness.Resx;
using MovilBusiness.views;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MovilBusiness.viewmodel
{
    public class ConsultaTransaccionesViewModel : BaseViewModel
    {
        private DS_Estados myEst;

        private ObservableCollection<ExpandListItem<Estados>> transaccionesfecha;
        public ObservableCollection<ExpandListItem<Estados>> TransaccionesFecha { get => transaccionesfecha; set { transaccionesfecha = value; RaiseOnPropertyChanged(); } }

        private ObservableCollection<ExpandListItem<Estados>> transaccionesestatus;
        public ObservableCollection<ExpandListItem<Estados>> TransaccionesEstatus { get => transaccionesestatus; set { transaccionesestatus = value; RaiseOnPropertyChanged(); } }

        private int currentCliId = -1;
        public ConsultaTransaccionesViewModel(Page page, int currentCliId = -1) : base(page)
        {
            this.currentCliId = currentCliId;
            myEst = new DS_Estados();
        }

        public async void LoadList()
        {
            IsBusy = true;

            try
            {
                await Task.Run(() =>
                {
                    TransaccionesEstatus = new ObservableCollection<ExpandListItem<Estados>>(myEst.GetAllTransaccionesEstados(currentCliId));
                    TransaccionesFecha = new ObservableCollection<ExpandListItem<Estados>>(myEst.GetAllTransaccionesFecha(currentCliId));
                });
            }catch(Exception e)
            {
                await DisplayAlert(AppResource.ErrorLoadingTransactions, e.Message);
            }
            
            IsBusy = false;
        }

        public void ItemTapped(ExpandListItem<Estados> Item, bool forFecha = false)
        {
            if(Item == null)
            {
                return;
            }

            if (Item.IsExpanded)
            {
                if (!Item.IsChild && Item.Childs != null && Item.Childs.Count > 0)
                {
                    Item.IsExpanded = false;

                    if (forFecha)
                    {
                        TransaccionesFecha[TransaccionesFecha.IndexOf(Item)] = Item.Copy();
                    }
                    else
                    {
                        TransaccionesEstatus[TransaccionesEstatus.IndexOf(Item)] = Item.Copy();
                    }


                    if (forFecha)
                    {
                        foreach (ExpandListItem<Estados> item in Item.Childs)
                        {
                            TransaccionesFecha.Remove(item);
                        }
                    }
                    else
                    {
                        foreach (ExpandListItem<Estados> item in Item.Childs)
                        {
                            TransaccionesEstatus.Remove(item);
                        }
                    }
                    
                }
            }
            else
            {
                if (Item.IsChild)
                {
                    PushAsync(new ConsultaTransaccionesDetallePage(Item, forFecha, currentCliId));

                }else if(!Item.IsChild && Item.Childs != null && Item.Childs.Count > 0)
                {
                    Item.IsExpanded = true;
                    var index = forFecha ? TransaccionesFecha.IndexOf(Item) : TransaccionesEstatus.IndexOf(Item);

                    if (forFecha)
                    {
                        TransaccionesFecha[index] = Item.Copy();

                        foreach (ExpandListItem<Estados> item in Item.Childs)
                        {
                            item.TitId = Item.TitId;
                            TransaccionesFecha.Insert(index + 1, item);
                        }
                    }
                    else
                    {
                        TransaccionesEstatus[index] = Item.Copy();

                        foreach (ExpandListItem<Estados> item in Item.Childs)
                        {
                            item.TitId = Item.TitId;
                            TransaccionesEstatus.Insert(index + 1, item);
                        }
                    }     
                }
            }
        }
    }
}
