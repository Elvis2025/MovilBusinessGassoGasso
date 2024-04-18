using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace MovilBusiness.Controls.Behavior
{
    public class NumericValidation : Behavior<Xamarin.Forms.Entry>
    {
        private static bool AllowPointFive = false;

        public NumericValidation(bool allowPointFive = false)
        {
            AllowPointFive = allowPointFive;
        }

        public NumericValidation()
        {
            AllowPointFive = false;
        }

        protected override void OnAttachedTo(Xamarin.Forms.Entry entry)
        {
            entry.TextChanged += OnEntryTextChanged;
            base.OnAttachedTo(entry);           
        }

        protected override void OnDetachingFrom(Xamarin.Forms.Entry entry)
        {
            entry.TextChanged -= OnEntryTextChanged;
            base.OnDetachingFrom(entry);            
        }

        private static void OnEntryTextChanged(object sender, TextChangedEventArgs args)
        {
            if (!string.IsNullOrWhiteSpace(args.NewTextValue) && !((Xamarin.Forms.Entry)sender).Text.All(char.IsDigit))
            {
                /* bool isValid;//args.NewTextValue.ToCharArray().All(x => char.IsDigit(x)); //Make sure all characters are numbers

                 if (AllowPointFive)
                 {
                     if (args.NewTextValue.Contains("."))
                     {
                         if(args.NewTextValue.EndsWith(".5") || args.NewTextValue.EndsWith("."))
                         {
                             isValid = true;
                         }
                         else
                         {
                             isValid = false;
                         }
                     }else
                     {
                         isValid = double.TryParse(args.NewTextValue.Last().ToString(), out double value2);
                     }
                 }
                 else
                 {
                     isValid = int.TryParse(args.NewTextValue.Last().ToString(), out int value);
                 }

                 ((Xamarin.Forms.Entry)sender).Text = isValid ? args.NewTextValue : args.NewTextValue.Remove(args.NewTextValue.Length - 1);*/

                //bool isValid = args.NewTextValue.ToCharArray().All(x => char.IsDigit(x)); //Make sure all characters are numbers

                bool isValid = args.NewTextValue.ToCharArray().All(char.IsDigit);

                ((Xamarin.Forms.Entry)sender).Text = isValid ? args.NewTextValue : (args.OldTextValue != null? args.OldTextValue : "");
                
            }
        }
    }
}
