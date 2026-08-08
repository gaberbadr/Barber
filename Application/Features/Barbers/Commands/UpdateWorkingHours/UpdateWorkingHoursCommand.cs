using MediatR;
using ErrorOr;
using Application.Features.Barbers.DTOs;

namespace Application.Features.Barbers.Commands.UpdateWorkingHours
{
    public class UpdateWorkingHoursCommand : IRequest<ErrorOr<BarberDTO>>
    {
        public string BarberId { get; set; } = string.Empty;
        public List<WorkingHourInput> WorkingHours { get; set; } = new();
    }

    public class WorkingHourInput
    {
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly OpeningTime { get; set; }
        public TimeOnly ClosingTime { get; set; }
        public bool IsClosed { get; set; }
    }
}