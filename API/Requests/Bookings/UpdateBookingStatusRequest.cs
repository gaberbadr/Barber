using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Requests.Bookings
{
    public class UpdateBookingStatusRequest
    {
        [Required]
        public BookingStatus Status { get; set; }
    }
}
