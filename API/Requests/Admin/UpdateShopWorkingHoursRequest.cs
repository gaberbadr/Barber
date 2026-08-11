using System;
using System.Collections.Generic;

namespace Requests.Admin
{
    public class UpdateShopWorkingHoursRequest
    {
        public List<ShopWorkingHourItemRequest> WorkingHours { get; set; } = new();
    }

    public class ShopWorkingHourItemRequest
    {
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly OpeningTime { get; set; }
        public TimeOnly ClosingTime { get; set; }
        public bool IsClosed { get; set; }
    }
}
