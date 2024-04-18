using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Hardware.Camera2;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Math;
using Android.Views;
using Android.Widget;
using Java.Lang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MovilBusiness.Droid.utils
{
    public class CameraZoomHandler
    {
        private static readonly float DEFAULT_ZOOM_FACTOR = 1.0f;
        private readonly Rect mCropRegion = new Rect();

        public readonly float maxZoom;

        private readonly Rect mSensorSize;

        public readonly bool hasSupport;

        public CameraZoomHandler(CameraCharacteristics characteristics)
        {
            this.mSensorSize = (Android.Graphics.Rect)characteristics.Get(CameraCharacteristics.SensorInfoActiveArraySize);//characteristics.Get(CameraCharacteristics.Key.SensorInfoActiveArraySize);

            if (this.mSensorSize == null)
            {
                this.maxZoom = DEFAULT_ZOOM_FACTOR;
                this.hasSupport = false;
                return;
            }

            float value = (float)characteristics.Get(CameraCharacteristics.ScalerAvailableMaxDigitalZoom);

            this.maxZoom = ((value == null) || (value < DEFAULT_ZOOM_FACTOR))
                    ? DEFAULT_ZOOM_FACTOR
                    : value;

            this.hasSupport = (Float.Compare(this.maxZoom, DEFAULT_ZOOM_FACTOR) > 0);
        }

        public float SetZoom(CaptureRequest.Builder builder, float zoom)
        {
            if (this.hasSupport == false)
            {
                return -1;
            }

            float newZoom = MathUtils.Clamp(zoom, DEFAULT_ZOOM_FACTOR, this.maxZoom);

            int centerX = this.mSensorSize.Width() / 2;
            int centerY = this.mSensorSize.Height() / 2;
            int deltaX = (int)((0.5f * this.mSensorSize.Width()) / newZoom);
            int deltaY = (int)((0.5f * this.mSensorSize.Height()) / newZoom);

            this.mCropRegion.Set(centerX - deltaX,
                    centerY - deltaY,
                    centerX + deltaX,
                    centerY + deltaY);

            builder.Set(CaptureRequest.ScalerCropRegion, this.mCropRegion);

            return newZoom;
        }
    }
}