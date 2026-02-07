using CsvHelper;
using System.Globalization;
using TaskApp.Domain.Sync;

namespace TaskApp.Infrastructures.FileIO
{
    internal class CsvService : ICsvService
    {
        public Task<IEnumerable<T>> ReadAsync<T>(string inboxDir)
        {
            if (!Directory.Exists(inboxDir))
            {
               throw new DirectoryNotFoundException($"The directory '{inboxDir}' does not exist.");
            }

            using var stream = new FileStream(inboxDir, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var records = csv.GetRecords<T>();

            return Task.FromResult(records);
        }

        public async Task WriteAsync<T>(string filePath, IAsyncEnumerable<T> records, CancellationToken ct = default)
        {
            using (var writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {

                csv.WriteHeader<T>();
                await csv.NextRecordAsync();

                await foreach (var record in records.WithCancellation(ct))
                {
                    csv.WriteRecord(record);
                    await csv.NextRecordAsync();
                }

                await writer.FlushAsync();
            }
        }
    }
}
