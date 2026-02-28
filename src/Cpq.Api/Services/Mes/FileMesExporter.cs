using System.Text.Json;
using Cpq.Api.Data;
using Cpq.Api.Exceptions;
using Cpq.Api.Models.Domain;
using Cpq.Api.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace Cpq.Api.Services.Mes;

/// <summary>
/// PoC file-based MES exporter. Writes MES payload to a local file directory.
/// In production, this would be replaced with an actual MES integration.
/// </summary>
public class FileMesExporter : IMesExporter
{
    private readonly CpqDbContext _db;
    private readonly ILogger<FileMesExporter> _logger;
    private readonly string _exportDirectory;

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public FileMesExporter(CpqDbContext db, ILogger<FileMesExporter> logger, IConfiguration configuration)
    {
        _db = db;
        _logger = logger;
        _exportDirectory = configuration["Mes:ExportDirectory"] ?? Path.Combine(Path.GetTempPath(), "cpq_mes_exports");
        Directory.CreateDirectory(_exportDirectory);
    }

    public async Task<MesExportResponse> ExportAsync(Guid configurationId, CancellationToken ct = default)
    {
        var configuration = await _db.Configurations
            .Include(c => c.ProductType)
                .ThenInclude(pt => pt.Family)
            .Include(c => c.BomLines)
            .FirstOrDefaultAsync(c => c.Id == configurationId, ct)
            ?? throw new EntityNotFoundException(nameof(Configuration), configurationId);

        if (configuration.Status != "finalized")
        {
            throw new InvalidOperationException(
                $"Configuration {configurationId} must be finalized before MES export (current status: {configuration.Status}).");
        }

        // Build the MES payload
        var payload = new
        {
            exportId = Guid.NewGuid(),
            configurationId = configuration.Id,
            reference = configuration.Reference,
            productType = configuration.ProductType.Code,
            productFamily = configuration.ProductType.Family.Code,
            exportedAt = DateTimeOffset.UtcNow,
            configData = configuration.ConfigData,
            bomLines = configuration.BomLines.OrderBy(l => l.SortOrder).Select(l => new
            {
                sku = l.PartSku,
                name = l.PartName,
                category = l.Category,
                quantity = l.Quantity,
                unit = l.Unit,
                cutLengthMm = l.CutLengthMm,
                notes = l.Notes
            })
        };

        var payloadJson = JsonSerializer.Serialize(payload, JsonOptions);
        var payloadDoc = JsonDocument.Parse(payloadJson);

        var mesExport = new MesExport
        {
            ConfigurationId = configurationId,
            Payload = payloadDoc,
            Status = "pending",
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _db.MesExports.AddAsync(mesExport, ct);
        await _db.SaveChangesAsync(ct);

        // Write to file (PoC)
        try
        {
            var fileName = $"mes_export_{mesExport.Id:N}_{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.json";
            var filePath = Path.Combine(_exportDirectory, fileName);
            await File.WriteAllTextAsync(filePath, payloadJson, ct);

            mesExport.Status = "sent";
            mesExport.SentAt = DateTimeOffset.UtcNow;

            _logger.LogInformation("MES export {ExportId} written to {FilePath}", mesExport.Id, filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write MES export file for configuration {ConfigId}", configurationId);
            mesExport.Status = "failed";
            mesExport.ErrorMessage = ex.Message;
        }

        await _db.SaveChangesAsync(ct);

        return MapToResponse(mesExport);
    }

    public async Task<MesExportResponse> GetExportAsync(Guid exportId, CancellationToken ct = default)
    {
        var export = await _db.MesExports
            .FirstOrDefaultAsync(e => e.Id == exportId, ct)
            ?? throw new EntityNotFoundException(nameof(MesExport), exportId);

        return MapToResponse(export);
    }

    private static MesExportResponse MapToResponse(MesExport export) => new()
    {
        Id = export.Id,
        ConfigurationId = export.ConfigurationId,
        Status = export.Status,
        SentAt = export.SentAt,
        ErrorMessage = export.ErrorMessage,
        CreatedAt = export.CreatedAt
    };
}
