using MovilBusiness.Views.Components.TemplateSelector;
namespace MovilBusiness.Model.Internal
{
    public class ReportesNameQuantity : RowLinker
    {
        public string Name { get; set; }
        public string Quantity { get; set; } = "";
        public string Amount { get; set; }
        public string DiscountAmount { get; set; }
        public string QuantityFormated { get => !string.IsNullOrWhiteSpace(Quantity) && IsDecimalQuantity && double.TryParse(Quantity, out double cantidad) ? cantidad.ToString("N2") : Quantity; }
        public string AmountFormated { get => !string.IsNullOrWhiteSpace(Amount) && IsMoneyAmount && double.TryParse(Amount, out double monto) ? monto.ToString("C2") : Amount; }

        public bool IsMoneyAmount { get; set; }
        public bool IsDecimalQuantity { get; set; }

        /*public bool Bold { get; set; }

        public FontAttributes FontAttribute { get => Bold ? FontAttributes.Bold : FontAttributes.None; }*/
    }
}
