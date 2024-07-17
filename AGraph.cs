using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavDataDisplay
{
    internal class AGraph
    {
        public List<GraphMark> Marks { get; } = new List<GraphMark>();

        public AGraph() { }

        void AddMark(NavDataEntry entry)
        {
            Marks.Add(new GraphMark(entry));
        }

        void AddMark(GraphMark entry)
        {
            Marks.Add(entry);
        }

        public void Feed(IEnumerable<NavDataEntry> data)
        {
            bool? isAsc = null;

            NavDataEntry? entryLast = null;
            foreach(var entry in data)
            {
                if (entryLast == null)
                {
                    AddMark(entry);
                    entryLast = entry;
                    continue;
                }

                var isAscCurr = entry.Atm > entryLast.Atm;
                isAsc ??= isAscCurr;
                if (isAsc != isAscCurr)
                {
                    AddMark(entryLast);
                    isAsc = isAscCurr;
                }

                entryLast = entry;
            }
        }

        public void Feed(NavDataFile dataFile)
        {
            foreach (var dataDayId in dataFile.Data)
            {
                var data = dataDayId.Value;
                var lastPeak = data[0].Atm;
                var isAsc = data[1].Atm > lastPeak;
                //AddMark(data[0]);

                for (var i = 1; i < data.Count; i++)
                {
                    var isAscCurr = data[i].Atm > data[i - 1].Atm;
                    if (isAsc != isAscCurr)
                    {
                        AddMark(data[i - 1]);
                        isAsc = isAscCurr;
                    }
                }
            }
        }

        public void Feed(List<GraphMark> marks)
        {
            var lastPeak = marks[0].Atm;
            var isAsc = marks[1].Atm > lastPeak;
            //AddMark(marks[0]);

            for (var i = 1; i < marks.Count; i++)
            {
                var isAscCurr = marks[i].Atm > marks[i - 1].Atm;
                if (isAsc != isAscCurr)
                {
                    AddMark(marks[i - 1]);
                    isAsc = isAscCurr;
                }
            }
        }

        const double trhdDef = 0.01;

        public void ApplyThreshold(double threshold = trhdDef)
        {
            var i = 0;
            while (i < Marks.Count - 2) 
            {
                if((Marks[i].Atm < Marks[i + 1].Atm) == (Marks[i+1].Atm < Marks[i + 2].Atm))
                {
                    Marks.RemoveAt(i + 1);
                    i = 0;
                }
                else if (Math.Abs(Marks[i].Atm - Marks[i + 1].Atm) <= threshold)
                {
                    Marks[i].Atm += Marks[i + 1].Atm - Marks[i + 2].Atm;
                    Marks.RemoveRange(i + 1, 2);
                    i = 0;
                }
                else i++;
            }
        }

        bool CheckPos(int i, double[] values, double threshold)
        {
            for (var j = 0; j < values.Length; j++)
                if (Math.Abs(Marks[i + j].Atm - values[j]) > threshold)
                    return false;
            return true;
        }



        public int? FindSeq(double[] values, double threshold = trhdDef)
        {
            for (int i = 0; i < Marks.Count - values.Length; i++)
                if (CheckPos(i, values, threshold))
                    return i; 
            
            return null;
        }
    }
}
