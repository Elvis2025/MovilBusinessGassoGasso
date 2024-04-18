using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace MovilBusiness.Controls
{
    public class SegmentedControl : Frame, IViewContainer<SegmentedControlOption>
    {
        //public new IEnumerable<SegmentedControlOption> Children { get; set; }
        public event EventHandler<int> OnSegmentSelected;

        public new IList<SegmentedControlOption> Children { get; set; }
        public int SelectedSegment { get; set; } = -1;
        public Color SelectedColor { get; set; } = Color.FromHex("#1976D2");

        public SegmentedControl() { 
        
            Children = new List<SegmentedControlOption>();
            HasShadow = false;
            Padding = new Thickness(1.5);
            BackgroundColor = Color.White;
            IsClippedToBounds = true;
            HeightRequest = 32;
            Content = new StackLayout()
            {
                Orientation = StackOrientation.Horizontal,
                Spacing = 0,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand   
            };           
        }

        protected override void OnBindingContextChanged()
        {           
            if (Children == null || Children.ToList().Count == 0)
            {
                return;
            }

            var list = Children.ToList();

            for (int i = 0; i < list.Count; i++)
            {
                var child = new Label() { Text = list[i].Text, HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand, VerticalTextAlignment = TextAlignment.Center, HorizontalTextAlignment = TextAlignment.Center };
                child.BindingContext = i.ToString();
                child.GestureRecognizers.Add(new TapGestureRecognizer() { Command = new Command(() => {
                    if (IsEnabled)
                    {
                        OnSegmentedSelected(child.BindingContext.ToString());
                    }
                }) });

                (Content as StackLayout).Children.Add(child);
            }

            OnSegmentedSelected("0");

            base.OnBindingContextChanged();
        }

        public void Select(int pos)
        {
            OnSegmentedSelected(pos.ToString());
        }

        private void OnSegmentedSelected(string raw)
        {
            try
            {
                var pos = Convert.ToInt32(raw);

                SelectedSegment = pos;

                //var color = Color.FromHex("#1976D2");

                foreach (var label in (Content as StackLayout).Children)
                {
                    label.BackgroundColor = SelectedColor;
                    (label as Label).TextColor = BackgroundColor;
                }

            ((Content as StackLayout).Children[pos] as Label).BackgroundColor = BackgroundColor;
                ((Content as StackLayout).Children[pos] as Label).TextColor = SelectedColor;

                OnSegmentSelected?.Invoke(this, pos);

                InvalidateLayout();
            }catch(Exception e)
            {
                Console.Write(e.Message);
            }
        }
        
        public void IsVisibleAbonosLabel(bool isvisible)
        {

            bool isValid = (Content as StackLayout).Children.Count() > 0;

            if(isValid)
            {
                ((Content as StackLayout).Children[1] as Label).IsVisible = isvisible;
                //InvalidateLayout();
            }
        }

    }
}
