using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Hardware;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Interop;

namespace MovilBusiness.Droid.utils
{
    [Obsolete]
    public class CameraAutoFocusCallback : Java.Lang.Object, Android.Hardware.Camera.IAutoFocusCallback
    {
        private Action<bool, Camera> onAutoFocus;

        public CameraAutoFocusCallback(Action<bool, Camera> onAutoFocus)
        {
            this.onAutoFocus = onAutoFocus;
        }

        public new void Dispose()
        {
           
        }

        public void Disposed()
        {
           
        }

        public void DisposeUnlessReferenced()
        {
            
        }

        public void Finalized()
        {
            
        }

        public void OnAutoFocus(bool success, Camera camera)
        {
            onAutoFocus?.Invoke(success, camera);
        }

        public void SetJniIdentityHashCode(int value)
        {
            
        }

        public void SetJniManagedPeerState(JniManagedPeerStates value)
        {
           
        }

        public void SetPeerReference(JniObjectReference reference)
        {
           
        }

        public new void UnregisterFromRuntime()
        {
           
        }
    }
}