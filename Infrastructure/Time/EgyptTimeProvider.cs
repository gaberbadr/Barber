using System;

namespace Infrastructure.Time
{
    public class EgyptTimeProvider : TimeProvider
    {
        private static readonly TimeZoneInfo CairoTimeZone = TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows() ? "Egypt Standard Time" : "Africa/Cairo");

        public override TimeZoneInfo LocalTimeZone => CairoTimeZone;
    }
}
