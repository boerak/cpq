using Cpq.Api.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace Cpq.Api.Data.Seeding;

public static class MotorSeeder
{
    public static async Task SeedAsync(DbContext context)
    {
        var dbSet = context.Set<Motor>();

        if (await dbSet.AnyAsync())
            return;

        var now = DateTimeOffset.UtcNow;

        var motors = new[]
        {
            new Motor
            {
                Id = new Guid("40000000-0000-0000-0000-000000000001"),
                Code = "SOMFY-IO-15",
                Brand = "Somfy",
                Model = "Ilmo 2 IO 15/17",
                TorqueNm = 15m,
                SpeedRpm = 17m,
                MaxWeightKg = 36m,
                MaxSurfaceM2 = 4.5m,
                ControlTypes = new List<string> { "io_homecontrol" },
                TubeDiameterMm = 60,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Motor
            {
                Id = new Guid("40000000-0000-0000-0000-000000000002"),
                Code = "SOMFY-IO-25",
                Brand = "Somfy",
                Model = "Oximo IO 25/17",
                TorqueNm = 25m,
                SpeedRpm = 17m,
                MaxWeightKg = 56m,
                MaxSurfaceM2 = 7.5m,
                ControlTypes = new List<string> { "io_homecontrol" },
                TubeDiameterMm = 60,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Motor
            {
                Id = new Guid("40000000-0000-0000-0000-000000000003"),
                Code = "SOMFY-IO-40",
                Brand = "Somfy",
                Model = "Oximo IO 40/17",
                TorqueNm = 40m,
                SpeedRpm = 17m,
                MaxWeightKg = 80m,
                MaxSurfaceM2 = 12m,
                ControlTypes = new List<string> { "io_homecontrol" },
                TubeDiameterMm = 60,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Motor
            {
                Id = new Guid("40000000-0000-0000-0000-000000000004"),
                Code = "SOMFY-RTS-20",
                Brand = "Somfy",
                Model = "Ilmo 2 RTS 20/17",
                TorqueNm = 20m,
                SpeedRpm = 17m,
                MaxWeightKg = 45m,
                MaxSurfaceM2 = 5.5m,
                ControlTypes = new List<string> { "rts", "rts_timer" },
                TubeDiameterMm = 60,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Motor
            {
                Id = new Guid("40000000-0000-0000-0000-000000000005"),
                Code = "BECKER-R12",
                Brand = "Becker",
                Model = "R12-17C",
                TorqueNm = 12m,
                SpeedRpm = 17m,
                MaxWeightKg = 28m,
                MaxSurfaceM2 = 3.5m,
                ControlTypes = new List<string> { "wired_switch", "wired_timer" },
                TubeDiameterMm = 60,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },

            // ── Simu motors (for screens) ────────────────────────────────
            new Motor
            {
                Id = new Guid("40000000-0000-0000-0000-000000000006"),
                Code = "SIMU-T5-10",
                Brand = "Simu",
                Model = "T5 Hz 10/17",
                TorqueNm = 10m,
                SpeedRpm = 17m,
                MaxWeightKg = 24m,
                MaxSurfaceM2 = 3.0m,
                ControlTypes = new List<string> { "wired_switch", "wired_timer" },
                TubeDiameterMm = 60,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Motor
            {
                Id = new Guid("40000000-0000-0000-0000-000000000007"),
                Code = "SIMU-T5-20",
                Brand = "Simu",
                Model = "T5 Hz 20/17",
                TorqueNm = 20m,
                SpeedRpm = 17m,
                MaxWeightKg = 45m,
                MaxSurfaceM2 = 6.0m,
                ControlTypes = new List<string> { "wired_switch", "wired_timer" },
                TubeDiameterMm = 60,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Motor
            {
                Id = new Guid("40000000-0000-0000-0000-000000000008"),
                Code = "SIMU-T6-25",
                Brand = "Simu",
                Model = "T6 Hz 25/17",
                TorqueNm = 25m,
                SpeedRpm = 17m,
                MaxWeightKg = 56m,
                MaxSurfaceM2 = 7.5m,
                ControlTypes = new List<string> { "wired_switch", "wired_timer" },
                TubeDiameterMm = 60,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            }
        };

        await dbSet.AddRangeAsync(motors);
        await context.SaveChangesAsync();
    }
}
