using System;

namespace Octopus.Time
{
    public class FixedClock : IClock
    {
        private DateTimeOffset now;

        public FixedClock(DateTimeOffset now)
        {
            this.now = now;
        }

        public DateTimeOffset GetUtcTime()
        {
            return Clone().now.ToUniversalTime();
        }

        public DateTimeOffset GetLocalTime()
        {
            return Clone().now.ToLocalTime();
        }

        public void Set(DateTimeOffset value)
        {
            now = value;
        }

        public void WindForward(TimeSpan time)
        {
            now = now.Add(time);
        }

        private FixedClock Clone()
        {
            return (FixedClock)MemberwiseClone();
        }
    }
}