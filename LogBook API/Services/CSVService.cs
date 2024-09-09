using CsvHelper;
using LogBook_API.Services.Abstractions;
using System.Globalization;
using System.Text;

namespace LogBook_API.Services
{
    public class CSVService : ICSVService
    {
        public byte[] WriteCSV<T>(List<T> records)
        {
            using var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream, Encoding.UTF8);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            // write records to the csv
            csv.WriteRecords(records);  
            writer.Flush();
            return memoryStream.ToArray();
                
        }
    }
}
