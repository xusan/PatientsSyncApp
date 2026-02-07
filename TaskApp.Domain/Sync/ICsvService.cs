namespace TaskApp.Domain.Sync
{
    public interface ICsvService
    {
        public Task<IEnumerable<T>> ReadAsync<T>(string filePath);
        Task WriteAsync<T>(string filePath, IAsyncEnumerable<T> records, CancellationToken ct = default);
    }
}
