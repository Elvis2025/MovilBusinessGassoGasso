using System;
using Android.Support.Design.Widget;
using Android.Views;

namespace MovilBusiness.Droid.utils
{
    public class BottomSheetCallBack : BottomSheetBehavior.BottomSheetCallback
    {
        private Action DismissAction;

        public BottomSheetCallBack(Action Dismiss)
        {
            DismissAction = Dismiss;
        }
        public override void OnSlide(View bottomSheet, float slideOffset) { }

        public override void OnStateChanged(View bottomSheet, int newState)
        {
            switch (newState)
            {
                case BottomSheetBehavior.StateHidden:
                    DismissAction?.Invoke();
                    //if you want the modal to be dismissed when user drags the bottomsheet down
                    break;
                case BottomSheetBehavior.StateExpanded:
                    break;
                case BottomSheetBehavior.StateCollapsed:
                    break;
                case BottomSheetBehavior.StateDragging:
                    break;
                case BottomSheetBehavior.StateSettling:
                    break;
            }
        }
    }
}