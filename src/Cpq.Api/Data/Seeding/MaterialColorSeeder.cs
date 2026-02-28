using Cpq.Api.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace Cpq.Api.Data.Seeding;

public static class MaterialColorSeeder
{
    public static async Task SeedAsync(DbContext context)
    {
        var dbSet = context.Set<MaterialColor>();

        if (await dbSet.AnyAsync())
            return;

        // ALU: all 20 colors
        var aluColors = new[]
        {
            "RAL9010", "RAL9001", "RAL9016", "RAL7016", "RAL7035",
            "RAL7021", "RAL8014", "RAL8003", "RAL8017", "RAL6005",
            "RAL5010", "RAL3000", "RAL1015", "RAL6009", "DB703",
            "RAL1013", "RAL7022", "RAL7039", "RAL8019", "RAL9005"
        };

        // PVC: 7 colors
        var pvcColors = new[]
        {
            "RAL9010", "RAL9001", "RAL9016", "RAL7016", "RAL7035",
            "RAL8014", "RAL1015"
        };

        // ALU_INSULATED: 8 colors
        var aluInsulatedColors = new[]
        {
            "RAL9010", "RAL9016", "RAL7016", "RAL7035",
            "RAL7021", "DB703", "RAL8017", "RAL9005"
        };

        var materialColors = new List<MaterialColor>();

        foreach (var colorCode in aluColors)
        {
            materialColors.Add(new MaterialColor
            {
                MaterialCode = "ALU",
                ColorCode = colorCode,
                IsActive = true
            });
        }

        foreach (var colorCode in pvcColors)
        {
            materialColors.Add(new MaterialColor
            {
                MaterialCode = "PVC",
                ColorCode = colorCode,
                IsActive = true
            });
        }

        foreach (var colorCode in aluInsulatedColors)
        {
            materialColors.Add(new MaterialColor
            {
                MaterialCode = "ALU_INSULATED",
                ColorCode = colorCode,
                IsActive = true
            });
        }

        await dbSet.AddRangeAsync(materialColors);
        await context.SaveChangesAsync();
    }
}
