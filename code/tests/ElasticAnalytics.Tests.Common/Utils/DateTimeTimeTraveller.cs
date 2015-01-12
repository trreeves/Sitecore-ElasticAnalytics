using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElasticAnalytics.Utils.DateTime;

namespace ElasticAnalytics.Tests.Common.Utils
{
    public class DateTimeTimeTraveller : IDateTimeController
    {
        public TimeSpan TimeOffset { get; private set; }

        public DateTimeTimeTraveller()
        {
            this.TimeOffset = TimeSpan.FromSeconds(0);
        }

        public DateTime UtcNow
        {
            get
            {
                return DateTime.UtcNow + TimeOffset;
            }
        }

        public DateTimeTimeTraveller Travel(TimeSpan duration)
        {
            this.TimeOffset += duration;
            return this;
        }

        public DateTimeTimeTraveller Travel(int hours, int minutes = 0, int seconds = 0)
        {
            this.Travel(new TimeSpan(hours, minutes, seconds));
            return this;
        }

        public DateTimeTimeTraveller ReturnToPresent()
        {
            this.TimeOffset = TimeSpan.FromSeconds(0);
            return this;
        }

        public TimeTravelContext NewJourney(int initialHoursTravel = 0)
        {
            return new TimeTravelContext(this, initialHoursTravel);
        }
    }

    public class TimeTravelContext : IDisposable
    {
        private readonly DateTimeTimeTraveller timeTraveller;

        private readonly TimeSpan originalTimeDeviation;

        public TimeTravelContext(DateTimeTimeTraveller timeTraveller, int initialHoursTravel = 0)
        {
            this.timeTraveller = timeTraveller;
            this.originalTimeDeviation = timeTraveller.TimeOffset;

            if (initialHoursTravel != 0)
            {
                this.timeTraveller.Travel(initialHoursTravel);
            }
        }

        public void Dispose()
        {
            this.timeTraveller
                .ReturnToPresent()
                .Travel(this.originalTimeDeviation);
        }
    }
}
