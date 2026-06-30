using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

var dataDirectory = Environment.GetEnvironmentVariable("PARQUET_DATA_DIR")
    ?? @"D:\Temp\_CSharp Project MCP Parquet"; 

var builder = Host.CreateApplicationBuilder(args);

// Stdio transport — logs cannot go to console, redirect to a file
builder.Logging.ClearProviders();
builder.Logging.AddProvider(new FileLoggerProvider("mcp-server.log"));

builder.Services.AddSingleton(new ParquetService(dataDirectory));

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();