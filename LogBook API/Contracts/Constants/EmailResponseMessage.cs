namespace LogBook_API.Contracts.Constants
{
    public class EmailResponseMessage
    {
        public static string GetEmailSuccessMessage(string emailAddress) => $"Email sent successfully to {emailAddress}";   
    }
}
