namespace LogBook_API.Persistence.RequestParameters
{
    public class DateFilterParameter
    {
        public DateOnly StartDate { get; set; } = DateOnly.MinValue;
        public DateOnly EndDate { get; set; } = DateOnly.MaxValue;
    }
}
