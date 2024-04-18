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
    public class PictureCallback : Java.Lang.Object, Camera.IPictureCallback
    {
       // public JniManagedPeerStates JniManagedPeerState => throw new NotImplementedException();

        private Action<byte[],Camera> onPictureTaken;
        public PictureCallback(Action<byte[],Camera> onPictureTaken)
        {
            this.onPictureTaken = onPictureTaken;

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

        public void OnPictureTaken(byte[] data, Camera camera)
        {
            onPictureTaken?.Invoke(data, camera);
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
    }
}