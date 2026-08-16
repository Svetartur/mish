using System;

namespace ASP_P42.Services.Time
{
    public class TimeService : ITimeService
    {
        public String GetTimestamp()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }
    }
}
