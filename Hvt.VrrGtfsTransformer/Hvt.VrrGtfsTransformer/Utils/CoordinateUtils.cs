using System;
using System.Collections.Generic;
using System.Linq;

namespace Hvt.VrrGtfsTransformer.Utils
{
    public static class CoordinateUtils
    {
        public static (double lat, double lon) GetCentralGeoCoordinate(IList<(double lat, double lon)> geoCoordinates)
        {
            if (geoCoordinates.Count == 1)
            {
                return geoCoordinates.Single();
            }

            double x = 0;
            double y = 0;
            double z = 0;

            foreach (var geoCoordinate in geoCoordinates)
            {
                var latitude = geoCoordinate.lat * Math.PI / 180;
                var longitude = geoCoordinate.lon * Math.PI / 180;

                x += Math.Cos(latitude) * Math.Cos(longitude);
                y += Math.Cos(latitude) * Math.Sin(longitude);
                z += Math.Sin(latitude);
            }

            var total = geoCoordinates.Count;

            x /= total;
            y /= total;
            z /= total;

            var centralLongitude = Math.Atan2(y, x);
            var centralSquareRoot = Math.Sqrt((x * x) + (y * y));
            var centralLatitude = Math.Atan2(z, centralSquareRoot);

            return (centralLatitude * 180 / Math.PI, centralLongitude * 180 / Math.PI);
        }
    }
}
