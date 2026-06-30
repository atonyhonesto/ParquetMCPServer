using Parquet;
using Parquet.Data;
using Parquet.Schema;

public class ParquetService
{
    private readonly string _dataDirectory;

    public ParquetService(string dataDirectory)
    {
        _dataDirectory = dataDirectory;
    }

    public IEnumerable<string> GetAvailableFiles()
    {
        if (!Directory.Exists(_dataDirectory))
            return [];

        return Directory.GetFiles(_dataDirectory, "*.parquet", SearchOption.AllDirectories)
                        .Select(f => Path.GetRelativePath(_dataDirectory, f));
    }

    public async Task<List<object>> GetFileSchemaAsync(string relativeFilePath)
    {
        var fullPath = Path.Combine(_dataDirectory, relativeFilePath);

        using var stream = File.OpenRead(fullPath);
        using var reader = await ParquetReader.CreateAsync(stream);

        return reader.Schema.GetDataFields()
            .Select(f => (object)new { name = f.Name, type = f.ClrType.Name })
            .ToList();
    }

    public async Task<List<Dictionary<string, object?>>> QueryFileAsync(
        string relativeFilePath, string[]? columns = null, int limit = 100)
    {
        var fullPath = Path.Combine(_dataDirectory, relativeFilePath);

        using var stream = File.OpenRead(fullPath);
        using var reader = await ParquetReader.CreateAsync(stream);

        var allFields = reader.Schema.GetDataFields();
        var targetFields = columns == null
            ? allFields
            : allFields.Where(f => columns.Contains(f.Name, StringComparer.OrdinalIgnoreCase)).ToArray();

        var results = new List<Dictionary<string, object?>>();

        for (int i = 0; i < reader.RowGroupCount; i++)
        {
            using var rowGroupReader = reader.OpenRowGroupReader(i);

            // Read all target columns, disambiguating duplicate names with _2, _3, etc.
            var columnData = new List<(string Key, Array Data)>();
            var nameCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var field in targetFields)
            {
                var dataColumn = await rowGroupReader.ReadColumnAsync(field);
                nameCounts.TryGetValue(field.Name, out int count);
                string key = count == 0 ? field.Name : $"{field.Name}_{count + 1}";
                nameCounts[field.Name] = count + 1;
                columnData.Add((key, dataColumn.Data));
            }

            if (columnData.Count == 0) continue;

            // Use the longest column to drive row iteration (handles mix of game-level and player-level columns)
            int rowCount = columnData.Max(c => c.Data.Length);
            for (int row = 0; row < rowCount && results.Count < limit; row++)
            {
                var record = new Dictionary<string, object?>();
                foreach (var (colName, data) in columnData)
                    record[colName] = row < data.Length ? data.GetValue(row) : null;
                results.Add(record);
            }

            if (results.Count >= limit) break;
        }

        return results;
    }
}