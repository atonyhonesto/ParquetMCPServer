# ParquetMCPServer

A [Model Context Protocol (MCP)](https://modelcontextprotocol.io) server written in C# that exposes Parquet files as queryable datasets to any MCP-compatible AI client (Claude Desktop, VS Code, etc.).

## What It Does

Point it at a directory of `.parquet` files and it gives your AI assistant four tools to explore and query those datasets without any manual data wrangling.

## Tools

| Tool | Description |
|------|-------------|
| `GetServerStatus` | Confirms the server is running and returns the current UTC time |
| `ListDatasets` | Lists all `.parquet` files found in the configured data directory |
| `GetFileSchema` | Returns column names and data types for a specific file |
| `QueryData` | Queries rows from a file with optional column filtering and row limit (max 1000) |

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Getting Started

### 1. Clone and build

```bash
git clone https://github.com/atonyhonesto/ParquetMCPServer.git
cd ParquetMCPServer
dotnet build
```

### 2. Set the data directory

By default the server looks for `.parquet` files in `D:\Temp\_CSharp Project MCP Parquet`. Override this with an environment variable:

```bash
# Windows
set PARQUET_DATA_DIR=C:\path\to\your\parquet\files

# macOS / Linux
export PARQUET_DATA_DIR=/path/to/your/parquet/files
```

### 3. Register with Claude Desktop

Add the following to your `claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "parquet": {
      "command": "dotnet",
      "args": ["run", "--project", "C:\\path\\to\\ParquetMCPServer"],
      "env": {
        "PARQUET_DATA_DIR": "C:\\path\\to\\your\\parquet\\files"
      }
    }
  }
}
```

Or point to the compiled binary:

```json
{
  "mcpServers": {
    "parquet": {
      "command": "C:\\path\\to\\ParquetMCPServer\\bin\\Release\\net10.0\\ParquetMCPServer.exe",
      "env": {
        "PARQUET_DATA_DIR": "C:\\path\\to\\your\\parquet\\files"
      }
    }
  }
}
```

## Logging

Since the server uses stdio transport (required by MCP), all logs are written to `mcp-server.log` in the project directory instead of the console.

## Dependencies

| Package | Version |
|---------|---------|
| `Microsoft.Extensions.Hosting` | 10.0.9 |
| `ModelContextProtocol` | 1.4.0 |
| `Parquet.Net` | 5.6.0 |

## Project Structure

```
ParquetMCPServer/
├── Program.cs              # Host setup, DI, MCP server registration
├── FileLogger.cs           # File-based logger (stdio-safe)
├── Services/
│   └── ParquetService.cs   # Parquet file reading and querying logic
└── Tools/
    └── DataTools.cs        # MCP tool definitions exposed to AI clients
```

## License

MIT
