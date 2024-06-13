using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavDataDisplay
{
    public class NavDataEntry
    {
        public DateTime Time { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        public double Atm { get; set; }
        public double Speed { get; set; }
        public double Angle { get; set; }
        public double Distance { get; set; }

        public NavDataEntry() { }
        public NavDataEntry(string dataEntry)
        {
            if (dataEntry[dataEntry.IndexOf("AZ") - 1] != ';')
                dataEntry = dataEntry.Insert(dataEntry.IndexOf("AZ"), ";");

            var parts = dataEntry.Split(';');

            var time = long.Parse(dataEntry.Substring(5, 13));
            Time = new DateTime(621355968000000000 + time * 10 * 1000, DateTimeKind.Utc);
            Lat = float.Parse(parts[1].Substring(2), CultureInfo.InvariantCulture.NumberFormat);
            Lon = float.Parse(parts[2].Substring(2), CultureInfo.InvariantCulture.NumberFormat);
            Atm = float.Parse(parts[3].Substring(2), CultureInfo.InvariantCulture.NumberFormat);
            Speed = float.Parse(parts[4].Substring(2), CultureInfo.InvariantCulture.NumberFormat);
            Angle = float.Parse(parts[5].Substring(4), CultureInfo.InvariantCulture.NumberFormat) + 180;
        }

        public double GetValueByGraphNumber(int num) => num switch
        {
            0 => this.Atm,
            1 => Angle,
            _ => Atm
        };

        public override string ToString()
        {
            return Atm.ToString("#.#####") + " " + Time.ToString("G");
        }

        public string ToJavascript() => $"[{Lat}, {Lon}]";

        const double radiusEarth = 6372795;
        internal void CalculateDistance(NavDataEntry prev)
        {
            var lat1 = prev.Lat * Math.PI / 180;
            var lat2 = Lat * Math.PI / 180;

            var long1 = prev.Lon * Math.PI / 180;
            var long2 = Lon * Math.PI / 180;

            var cl1 = Math.Cos(lat1);
            var cl2 = Math.Cos(lat2);
            var sl1 = Math.Sin(lat1);
            var sl2 = Math.Sin(lat2);
            var delta = long2 - long1;
            var cDelta = Math.Cos(delta);
            var sDelta = Math.Sin(delta);

            var y = Math.Sqrt(Math.Pow(cl2 * sDelta, 2) + Math.Pow(cl1 * sl2 - sl1 * cl2 * cDelta, 2));
            var x = sl1 * sl2 + cl1 * cl2 * cDelta;
            var ad = Math.Atan2(y, x);
            var dist = (ad * radiusEarth);

            Distance = prev.Distance + dist;
        }
    }
}
