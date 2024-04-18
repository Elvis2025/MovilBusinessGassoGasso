using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model.Internal
{
    public class MenuItem
    {
        public int Id { get; set; }
        public string Icon { get; set; }
        public string Title { get; set; }
        public bool SeparatorVisible { get; set; }
        public string TextColor { get { return SeparatorVisible ? "#757575" : "#212121"; } } 
        public bool IconVisible { get { return !SeparatorVisible; } }

        public string Badge { get; set; }
        public bool ShowBadge { get => !string.IsNullOrWhiteSpace(Badge) && Badge.Trim().Length > 0; }

        public MenuItem Copy()
        {
            return (MenuItem)MemberwiseClone();
        }
    }
}
