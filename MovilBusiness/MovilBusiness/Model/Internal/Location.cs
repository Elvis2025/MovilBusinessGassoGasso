using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model.Internal
{
    public class Location
    {
        public Location(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
        public double Latitude { get; }
        public double Longitude { get; }

        public string Label { get; set; }
        public string Position { get; set; }
    }
}
