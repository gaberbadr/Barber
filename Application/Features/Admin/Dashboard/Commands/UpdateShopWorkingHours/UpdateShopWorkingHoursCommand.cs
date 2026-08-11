using ErrorOr;
using MediatR;
using Application.Features.Admin.Dashboard.DTOs;
using System;
using System.Collections.Generic;

namespace Application.Features.Admin.Dashboard.Commands.UpdateShopWorkingHours
{
    public class UpdateShopWorkingHoursCommand : IRequest<ErrorOr<List<ShopWorkingHourDTO>>>
    {
        public List<ShopWorkingHourInput> WorkingHours { get; set; } = new();
    }

    public class ShopWorkingHourInput
    {
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly OpeningTime { get; set; }
        public TimeOnly ClosingTime { get; set; }
        public bool IsClosed { get; set; }
    }
}
