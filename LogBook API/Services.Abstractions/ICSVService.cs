namespace LogBook_API.Services.Abstractions
{
    public interface ICSVService
    {
        byte[] WriteCSV<T>(List<T> records);
    }
}
