using System;

namespace Octopus.Time
{
    public class SystemClock : IClock
    {
        public DateTimeOffset GetUtcTime() => DateTimeOffset.UtcNow;

        public DateTimeOffset GetLocalTime() => DateTimeOffset.Now;
    }
}
