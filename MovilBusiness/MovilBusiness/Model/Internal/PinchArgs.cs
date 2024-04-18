using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.Model.Internal
{
    public class PinchArgs
    {
        public double startScale { get; set; } = 1;
        public double currentScale { get; set; } = 1;
        public double xOffset { get; set; } = 0;
        public double yOffset { get; set; } = 0;


        public double MIN_SCALE = 1;
        public double MAX_SCALE = 2.5;
        public double OVERSHOOT = 0.15;
        public double StartX, StartY;
        public double StartScale;
    }
}
