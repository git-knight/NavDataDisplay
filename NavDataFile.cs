﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;

namespace NavDataDisplay
{
    public class NavDataFile
    {
        static Random rng = new Random();

        public bool Selected { get; set; } = true;
        public bool Flipped { get; set; }
        public string Path { get; set; }
        public string FileName { get; set; }
        public Dictionary<DateTime, List<NavDataEntry>> Data => Flipped ? DataFlipped : DataFwd;
        public Dictionary<DateTime, List<NavDataEntry>> DataFwd { get; } = new Dictionary<DateTime, List<NavDataEntry>>();
        public Dictionary<DateTime, List<NavDataEntry>> DataFlipped { get; } = new Dictionary<DateTime, List<NavDataEntry>>();
        public Dictionary<DateTime, DateRange> TimeRanges { get; set; } = new Dictionary<DateTime, DateRange>();
        public DateRange DataRange { get; set; } = new DateRange();
        //public Dictionary<DateTime, int> DataPerDayStartIdx { get; set; } = new Dictionary<DateTime, int>();
        public List<(double m, double l, double h)> CurrentDataSet { get; set; } = new List<(double, double, double)>();
        //public 

        public int MaxDistance => (int)Math.Ceiling(Data.Select(x => x.Value.Select(v => v.Distance).Aggregate(Math.Max)).Aggregate(Math.Max));

        public Brush FColor { get; set; } = new Brush[] {
        /*
        Brushes.Green,
        Brushes.DarkRed,
        Brushes.DarkMagenta,
        Brushes.DarkOliveGreen,
        Brushes.DarkOrchid,
        Brushes.DarkSalmon,
        Brushes.DodgerBlue,
        Brushes.ForestGreen,
        Brushes.Fuchsia,
        new SolidColorBrush(Color.FromRgb(155,100,50)),
        Brushes.Indigo,
        Brushes.Ivory
        */
          
            new SolidColorBrush(Color.FromRgb (81, 45, 168)),
            new SolidColorBrush(Color.FromRgb (48, 63, 159)),
            new SolidColorBrush(Color.FromRgb (0, 121, 107)),
            new SolidColorBrush(Color.FromRgb (56, 142, 60)),
            new SolidColorBrush(Color.FromRgb (245, 124, 0)),
            new SolidColorBrush(Color.FromRgb (230, 74, 25)),
            new SolidColorBrush(Color.FromRgb (93, 64, 55)),
            new SolidColorBrush(Color.FromRgb (69, 90, 100))
        }.OrderBy(x => Guid.NewGuid()).First();


        public NavDataFile(string path)
        {
            Path = path;
            FileName = new string(System.IO.Path.GetFileNameWithoutExtension(path));

            NavDataEntry entryPrev = null;
            foreach (var line in File.ReadAllLines(path))
            {
                var entry = new NavDataEntry(line);
                if (Data.TryGetValue(entry.Time.Date, out var entriesPerDay))
                {
                    if (entriesPerDay.Any())
                        entry.CalculateDistance(entriesPerDay.Last());
                    entriesPerDay.Add(entry);
                }
                else Data.Add(entry.Time.Date, new List<NavDataEntry>(1000) { entry });
                
                DataRange.Update(entry.Time);
                //if (!DataPerDayStartIdx.ContainsKey(entry.Time.Date))
                //    DataPerDayStartIdx.Add(entry.Time.Date, Data.Count - 1);
            }

            foreach(var day in Data)
            {
                var rEntries = new List<NavDataEntry>();
                var entries = day.Value;
                DateRange timeRange = new DateRange();
                foreach (var e in entries)
                    timeRange.Update(e.Time);
                TimeRanges.Add(day.Key, timeRange);


                for (int i = entries.Count - 1; i >= 0; i--)
                {
                    var timeDist = timeRange.Max - entries[i].Time;
                    rEntries.Add(new NavDataEntry(entries[i], timeRange.Min + timeDist));
                }

                DataFlipped.Add(day.Key, rEntries);
            }
        }

        public bool IncludesDay(DateTime date) =>
            Data.ContainsKey(date.Date);

        public void CreateDataSet(DateTime date, TimeSpan step, int graphType)
        {
            if (!Data.TryGetValue(date.Date, out var values)) return;

            //values.

            var lastValue = (m: 0.0, l: 0.0, h: 0.0);

            var valsInPoint = 0;
            var currValue = (m: 0.0, l: double.MaxValue, h: double.MinValue);
            var currSpan = date;
            foreach(var entry in values)
            {
                while (currSpan + step < entry.Time)
                {
                    if (valsInPoint == 0)
                        currValue = lastValue;

                    CurrentDataSet.Add((currValue.m / valsInPoint, currValue.l, currValue.h));
                    lastValue = currValue;
                    currSpan += step;
                    valsInPoint = 0;
                }

                var val = entry.GetValueByGraphNumber(graphType);
                currValue = (currValue.m + val, Math.Min(currValue.l, val), Math.Max(currValue.h, val));
                valsInPoint++;
            }
        }


        
    }
}
