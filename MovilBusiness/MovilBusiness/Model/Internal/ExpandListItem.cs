using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model.Internal
{
    public class ExpandListItem<T>
    {
        public string Icon { get => IsExpanded ? "ic_remove_circle_outline_black_24dp" : "ic_add_circle_outline_black_24dp.png"; }
        public bool IsExpanded { get; set; }
        public string Title { get; set; }
        public string Descripcion { get; set; }
        public bool IsChild { get; set; }
        public double IconOpacity { get => IsChild ? 0 : 0.6; }

        public int TitId { get; set; } = -1;
        public int Count { get; set; } //para saber la cantidad de transacciones que hay del actual tipo

        public string CountLabel { get => Count > 999 ? "999+" : Count.ToString(); }

        public T Data { get; set; }

        public List<ExpandListItem<T>> Childs { get; set; } = null;

        public string RowBg { get => IsChild ? "#ECEFF1" : "White"; }
        public string CountBg { get => IsChild ? "Green" : "#0D47A1"; }

        public ExpandListItem<T> Copy()
        {
            return (ExpandListItem<T>)MemberwiseClone();
        }
    }
}
