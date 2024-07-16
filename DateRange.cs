using System;

namespace NavDataDisplay
{
    public class DateRange
    {
        public DateRange()
        {
            Min = DateTime.MaxValue;
            Max = DateTime.MinValue;
        }

        public DateRange(DateTime min, DateTime max)
        {
            Min = min;
            Max = max;
        }

        public DateTime Min { get; set; } = DateTime.MaxValue;
        public DateTime Max { get; set; } = DateTime.MinValue;

        public bool Initialized => Min != DateTime.MaxValue;
        public TimeSpan Length => Max - Min;
        public DateTime Center => Min + Length / 2;


        public void Update(DateTime dtNew)
        {
            if (Min.Ticks == 0)
            {
                Min = dtNew;
                Max = dtNew;
                return;
            }
            Min = Min.Ticks > dtNew.Ticks ? dtNew : Min;
            Max = Max.Ticks < dtNew.Ticks ? dtNew : Max;
        }

        public DateRange Combine(DateRange other)
            => new DateRange(Min < other.Min ? Min : other.Min, Max > other.Max ? Max : other.Max);

        public bool Includes(DateTime dt) =>
            Min < dt && dt < Max;
            //Min.Date.Ticks <= dt.Ticks && (Max + TimeSpan.FromDays(1)).Date.Ticks > dt.Ticks;

        public bool Includes(DateTime start, TimeSpan len) =>
            start < Max && Max < start + len + Length;
        //Min.Date.Ticks <= dt.Ticks && (Max + TimeSpan.FromDays(1)).Date.Ticks > dt.Ticks;

        public override string ToString()
            => $"{Min.ToString("T")} - {Max.ToString("T")}";
    }
}
