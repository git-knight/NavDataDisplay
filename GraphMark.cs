using System.Linq;

namespace NavDataDisplay
{
    internal class GraphMark
    {
        public double Lat { get; set; }
        public double Lon { get; set; }
        public double Atm { get; set; }

        public GraphMark() { }
        public GraphMark(NavDataEntry entry)
        {
            Lat = entry.Lat;
            Lon = entry.Lon;
            Atm = entry.Atm;
        }

        public GraphMark(NavDataEntry[] locs, double atm)
        {
            Lat = locs.Sum(x => x.Lat) / locs.Length;
            Lon = locs.Sum(x => x.Lon) / locs.Length;
            Atm = atm;
        }
    }
}