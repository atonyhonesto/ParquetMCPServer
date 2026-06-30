using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

[McpServerToolType]
public static class DataTools
{
    [McpServerTool, Description("Returns the status of the MCP server and confirms it is running.")]
    public static string GetServerStatus()
    {
        return $"ParquetMcpServer is online. UTC: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";
    }

    [McpServerTool, Description(
        "Lists all available Parquet files in the data directory. " +
        "Call this first to discover what datasets are available before querying.")]
    public static string ListDatasets(ParquetService parquetService)
    {
        var files = parquetService.GetAvailableFiles().ToList();
        return files.Count == 0
            ? "No Parquet files found in the configured data directory."
            : JsonSerializer.Serialize(new { file_count = files.Count, files });
    }

    [McpServerTool, Description(
        "Returns the column names and data types for a specific Parquet file. " +
        "Use the filename exactly as returned by ListDatasets.")]
    public static async Task<string> GetFileSchema(
        ParquetService parquetService,
        [Description("Relative file path as returned by ListDatasets, e.g. 'race_results\\2024.parquet'")]
        string fileName)
    {
        var schema = await parquetService.GetFileSchemaAsync(fileName);
        return JsonSerializer.Serialize(new { file = fileName, columns = schema });
    }

    [McpServerTool, Description(
        "Queries rows from a Parquet file. Optionally filter to specific columns and limit row count. " +
        "Always call GetFileSchema first to know available columns.")]
    public static async Task<string> QueryData(
        ParquetService parquetService,
        [Description("Relative file path as returned by ListDatasets")]
        string fileName,
        [Description("Optional comma-separated list of column names to include. Leave empty for all columns.")]
        string? columns,
        [Description("Maximum number of rows to return. Default 100, max 1000.")]
        int limit = 100)
    {
        limit = Math.Clamp(limit, 1, 1000);
        var colArray = string.IsNullOrWhiteSpace(columns)
            ? null
            : columns.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var rows = await parquetService.QueryFileAsync(fileName, colArray, limit);
        return JsonSerializer.Serialize(new
        {
            file = fileName,
            row_count = rows.Count,
            rows
        });
    }
}