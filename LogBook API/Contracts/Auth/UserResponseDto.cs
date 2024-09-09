namespace LogBook_API.Contracts.Auth
{
    public class UserResponseDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string NormalizedUserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string NormalizedEmail { get; set; }
        public string PhoneCountryCode { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
    }
}
