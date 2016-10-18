using System;

namespace Octopus.Time
{
    public interface IClock
    {
        DateTimeOffset GetUtcTime();
        DateTimeOffset GetLocalTime();
    }
}