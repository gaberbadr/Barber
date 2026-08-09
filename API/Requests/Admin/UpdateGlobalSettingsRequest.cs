namespace Requests.Admin
{
    public class UpdateGlobalSettingsRequest
    {
        public int MaximumBookingAdvanceDays { get; set; }
        public int CancellationWindowHours { get; set; }
    }
}
