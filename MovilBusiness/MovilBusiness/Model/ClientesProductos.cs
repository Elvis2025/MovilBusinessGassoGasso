using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace MovilBusiness.Model
{
    public class ClientesProductos : INotifyPropertyChanged
    {
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public string RepCodigo { get; set; }
        public int CliID { get; set; }
        public int ProId { get; set; }
        public string CliFechaActualizacion { get; set; }
        public string ProDescripcion { get; set; }
        public string CliFechasYCantidades { get; set; }

        public DateTime fecha1 { get; set; }
        public DateTime fecha2 { get; set; }
        public DateTime fecha3 { get; set; }

        public int catidad1 { get; set; }
        public int catidad2 { get; set; }
        public int catidad3 { get; set; }
    }
}
