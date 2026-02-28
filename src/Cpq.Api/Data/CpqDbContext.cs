using Cpq.Api.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace Cpq.Api.Data;

public class CpqDbContext : DbContext
{
    public CpqDbContext(DbContextOptions<CpqDbContext> options) : base(options) { }

    public DbSet<ProductFamily> ProductFamilies => Set<ProductFamily>();
    public DbSet<ProductType> ProductTypes => Set<ProductType>();
    public DbSet<ProductParameter> ProductParameters => Set<ProductParameter>();
    public DbSet<ProductOption> ProductOptions => Set<ProductOption>();
    public DbSet<ProductSpec> ProductSpecs => Set<ProductSpec>();
    public DbSet<Material> Materials => Set<Material>();
    public DbSet<Color> Colors => Set<Color>();
    public DbSet<MaterialColor> MaterialColors => Set<MaterialColor>();
    public DbSet<Models.Domain.Profile> Profiles => Set<Models.Domain.Profile>();
    public DbSet<Motor> Motors => Set<Motor>();
    public DbSet<GuideRail> GuideRails => Set<GuideRail>();
    public DbSet<Box> Boxes => Set<Box>();
    public DbSet<Accessory> Accessories => Set<Accessory>();
    public DbSet<Part> Parts => Set<Part>();
    public DbSet<SkuMapping> SkuMappings => Set<SkuMapping>();
    public DbSet<Models.Domain.Configuration> Configurations => Set<Models.Domain.Configuration>();
    public DbSet<ConfigurationHistory> ConfigurationHistories => Set<ConfigurationHistory>();
    public DbSet<BomLine> BomLines => Set<BomLine>();
    public DbSet<MesExport> MesExports => Set<MesExport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ProductFamily
        modelBuilder.Entity<ProductFamily>(e =>
        {
            e.ToTable("product_families");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
            e.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            e.Property(x => x.Description).HasColumnName("description");
            e.Property(x => x.RulePrefix).HasColumnName("rule_prefix").HasMaxLength(100).IsRequired();
            e.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
            e.HasIndex(x => x.Code).IsUnique().HasDatabaseName("ix_product_families_code");
        });

        // ProductType
        modelBuilder.Entity<ProductType>(e =>
        {
            e.ToTable("product_types");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.FamilyId).HasColumnName("family_id").IsRequired();
            e.Property(x => x.Code).HasColumnName("code").HasMaxLength(100).IsRequired();
            e.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            e.Property(x => x.Variant).HasColumnName("variant").HasMaxLength(50).IsRequired();
            e.Property(x => x.Description).HasColumnName("description");
            e.Property(x => x.DisplayOrder).HasColumnName("display_order").HasDefaultValue(0);
            e.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
            e.HasIndex(x => x.Code).IsUnique().HasDatabaseName("ix_product_types_code");
            e.HasIndex(x => x.FamilyId).HasDatabaseName("ix_product_types_family_id");
            e.HasOne(x => x.Family)
                .WithMany(x => x.ProductTypes)
                .HasForeignKey(x => x.FamilyId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_product_types_family");
        });

        // ProductParameter
        modelBuilder.Entity<ProductParameter>(e =>
        {
            e.ToTable("product_parameters");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.ProductTypeId).HasColumnName("product_type_id").IsRequired();
            e.Property(x => x.Code).HasColumnName("code").HasMaxLength(100).IsRequired();
            e.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            e.Property(x => x.DataType).HasColumnName("data_type").HasMaxLength(50).IsRequired();
            e.Property(x => x.Unit).HasColumnName("unit").HasMaxLength(20);
            e.Property(x => x.StepNumber).HasColumnName("step_number");
            e.Property(x => x.StepName).HasColumnName("step_name").HasMaxLength(200);
            e.Property(x => x.DisplayOrder).HasColumnName("display_order").HasDefaultValue(0);
            e.Property(x => x.IsRequired).HasColumnName("is_required").HasDefaultValue(true);
            e.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            e.Property(x => x.DefaultValue).HasColumnName("default_value").HasMaxLength(200);
            e.Property(x => x.DependsOn).HasColumnName("depends_on").HasColumnType("text[]");
            e.Property(x => x.Metadata).HasColumnName("metadata").HasColumnType("jsonb");
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
            e.HasIndex(x => new { x.ProductTypeId, x.Code }).IsUnique().HasDatabaseName("ix_product_parameters_type_code");
            e.HasOne(x => x.ProductType)
                .WithMany(x => x.Parameters)
                .HasForeignKey(x => x.ProductTypeId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_product_parameters_type");
        });

        // ProductOption
        modelBuilder.Entity<ProductOption>(e =>
        {
            e.ToTable("product_options");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.ProductTypeId).HasColumnName("product_type_id").IsRequired();
            e.Property(x => x.ParameterCode).HasColumnName("parameter_code").HasMaxLength(100).IsRequired();
            e.Property(x => x.Code).HasColumnName("code").HasMaxLength(100).IsRequired();
            e.Property(x => x.DisplayName).HasColumnName("display_name").HasMaxLength(200).IsRequired();
            e.Property(x => x.DisplayOrder).HasColumnName("display_order").HasDefaultValue(0);
            e.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            e.Property(x => x.Metadata).HasColumnName("metadata").HasColumnType("jsonb");
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
            e.HasIndex(x => new { x.ProductTypeId, x.ParameterCode, x.Code }).IsUnique().HasDatabaseName("ix_product_options_type_param_code");
            e.HasOne(x => x.ProductType)
                .WithMany(x => x.Options)
                .HasForeignKey(x => x.ProductTypeId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_product_options_type");
        });

        // ProductSpec
        modelBuilder.Entity<ProductSpec>(e =>
        {
            e.ToTable("product_specs");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.ProductTypeId).HasColumnName("product_type_id").IsRequired();
            e.Property(x => x.SpecGroup).HasColumnName("spec_group").HasMaxLength(100).IsRequired();
            e.Property(x => x.SpecKey).HasColumnName("spec_key").HasMaxLength(200).IsRequired();
            e.Property(x => x.SpecValue).HasColumnName("spec_value").HasColumnType("jsonb").IsRequired();
            e.Property(x => x.Description).HasColumnName("description");
            e.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
            e.HasIndex(x => new { x.ProductTypeId, x.SpecGroup, x.SpecKey }).IsUnique().HasDatabaseName("ix_product_specs_type_group_key");
            e.HasOne(x => x.ProductType)
                .WithMany(x => x.Specs)
                .HasForeignKey(x => x.ProductTypeId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_product_specs_type");
        });

        // Material
        modelBuilder.Entity<Material>(e =>
        {
            e.ToTable("materials");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
            e.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            e.Property(x => x.DensityKgPerM3).HasColumnName("density_kg_per_m3").HasColumnType("decimal(10,2)");
            e.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            e.Property(x => x.Properties).HasColumnName("properties").HasColumnType("jsonb");
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
            e.HasIndex(x => x.Code).IsUnique().HasDatabaseName("ix_materials_code");
        });

        // Color
        modelBuilder.Entity<Color>(e =>
        {
            e.ToTable("colors");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
            e.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            e.Property(x => x.ColorSystem).HasColumnName("color_system").HasMaxLength(50).IsRequired();
            e.Property(x => x.HexValue).HasColumnName("hex_value").HasMaxLength(7);
            e.Property(x => x.IsStandard).HasColumnName("is_standard").HasDefaultValue(true);
            e.Property(x => x.Properties).HasColumnName("properties").HasColumnType("jsonb");
            e.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
            e.HasIndex(x => x.Code).IsUnique().HasDatabaseName("ix_colors_code");
        });

        // MaterialColor (join table)
        modelBuilder.Entity<MaterialColor>(e =>
        {
            e.ToTable("material_colors");
            e.HasKey(x => new { x.MaterialCode, x.ColorCode });
            e.Property(x => x.MaterialCode).HasColumnName("material_code").HasMaxLength(50).IsRequired();
            e.Property(x => x.ColorCode).HasColumnName("color_code").HasMaxLength(50).IsRequired();
            e.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            e.HasOne(x => x.Material)
                .WithMany(x => x.MaterialColors)
                .HasForeignKey(x => x.MaterialCode)
                .HasPrincipalKey(x => x.Code)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_material_colors_material");
            e.HasOne(x => x.Color)
                .WithMany(x => x.MaterialColors)
                .HasForeignKey(x => x.ColorCode)
                .HasPrincipalKey(x => x.Code)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_material_colors_color");
        });

        // Profile
        modelBuilder.Entity<Models.Domain.Profile>(e =>
        {
            e.ToTable("profiles", t => t.HasCheckConstraint("chk_profiles_width", "min_width_mm <= max_width_mm"));
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
            e.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            e.Property(x => x.MaterialCode).HasColumnName("material_code").HasMaxLength(50).IsRequired();
            e.Property(x => x.HeightMm).HasColumnName("height_mm").HasColumnType("decimal(10,2)");
            e.Property(x => x.ThicknessMm).HasColumnName("thickness_mm").HasColumnType("decimal(10,2)");
            e.Property(x => x.WeightPerMeterKg).HasColumnName("weight_per_meter_kg").HasColumnType("decimal(10,4)");
            e.Property(x => x.MaxWidthMm).HasColumnName("max_width_mm");
            e.Property(x => x.MinWidthMm).HasColumnName("min_width_mm").HasDefaultValue(400);
            e.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            e.Property(x => x.Properties).HasColumnName("properties").HasColumnType("jsonb");
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
            e.HasIndex(x => x.Code).IsUnique().HasDatabaseName("ix_profiles_code");
            e.HasOne(x => x.Material)
                .WithMany()
                .HasForeignKey(x => x.MaterialCode)
                .HasPrincipalKey(x => x.Code)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_profiles_material");
        });

        // Motor
        modelBuilder.Entity<Motor>(e =>
        {
            e.ToTable("motors");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
            e.Property(x => x.Brand).HasColumnName("brand").HasMaxLength(100).IsRequired();
            e.Property(x => x.Model).HasColumnName("model").HasMaxLength(200).IsRequired();
            e.Property(x => x.TorqueNm).HasColumnName("torque_nm").HasColumnType("decimal(10,2)");
            e.Property(x => x.SpeedRpm).HasColumnName("speed_rpm").HasColumnType("decimal(10,2)");
            e.Property(x => x.MaxWeightKg).HasColumnName("max_weight_kg").HasColumnType("decimal(10,2)");
            e.Property(x => x.MaxSurfaceM2).HasColumnName("max_surface_m2").HasColumnType("decimal(10,2)");
            e.Property(x => x.ControlTypes).HasColumnName("control_types").HasColumnType("text[]");
            e.Property(x => x.TubeDiameterMm).HasColumnName("tube_diameter_mm");
            e.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            e.Property(x => x.Properties).HasColumnName("properties").HasColumnType("jsonb");
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
            e.HasIndex(x => x.Code).IsUnique().HasDatabaseName("ix_motors_code");
        });

        // GuideRail
        modelBuilder.Entity<GuideRail>(e =>
        {
            e.ToTable("guide_rails");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
            e.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            e.Property(x => x.Type).HasColumnName("type").HasMaxLength(50).IsRequired();
            e.Property(x => x.MaterialCode).HasColumnName("material_code").HasMaxLength(50).IsRequired();
            e.Property(x => x.WidthMm).HasColumnName("width_mm").HasColumnType("decimal(10,2)");
            e.Property(x => x.DepthMm).HasColumnName("depth_mm").HasColumnType("decimal(10,2)");
            e.Property(x => x.MaxHeightMm).HasColumnName("max_height_mm");
            e.Property(x => x.WeightPerMeterKg).HasColumnName("weight_per_meter_kg").HasColumnType("decimal(10,4)");
            e.Property(x => x.BracketSpacingMm).HasColumnName("bracket_spacing_mm").HasDefaultValue(600);
            e.Property(x => x.CompatibleProfiles).HasColumnName("compatible_profiles").HasColumnType("text[]");
            e.Property(x => x.WindClass).HasColumnName("wind_class");
            e.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            e.Property(x => x.Properties).HasColumnName("properties").HasColumnType("jsonb");
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
            e.HasIndex(x => x.Code).IsUnique().HasDatabaseName("ix_guide_rails_code");
            e.HasOne(x => x.Material)
                .WithMany()
                .HasForeignKey(x => x.MaterialCode)
                .HasPrincipalKey(x => x.Code)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_guide_rails_material");
        });

        // Box
        modelBuilder.Entity<Box>(e =>
        {
            e.ToTable("boxes");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
            e.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            e.Property(x => x.Type).HasColumnName("type").HasMaxLength(50).IsRequired();
            e.Property(x => x.InnerDiameterMm).HasColumnName("inner_diameter_mm");
            e.Property(x => x.OuterHeightMm).HasColumnName("outer_height_mm");
            e.Property(x => x.CompatibleMaterials).HasColumnName("compatible_materials").HasColumnType("text[]");
            e.Property(x => x.MaxWidthMm).HasColumnName("max_width_mm");
            e.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            e.Property(x => x.Properties).HasColumnName("properties").HasColumnType("jsonb");
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
            e.HasIndex(x => x.Code).IsUnique().HasDatabaseName("ix_boxes_code");
        });

        // Accessory
        modelBuilder.Entity<Accessory>(e =>
        {
            e.ToTable("accessories");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
            e.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            e.Property(x => x.Category).HasColumnName("category").HasMaxLength(100).IsRequired();
            e.Property(x => x.RequiresMotor).HasColumnName("requires_motor").HasDefaultValue(false);
            e.Property(x => x.CompatibleFamilies).HasColumnName("compatible_families").HasColumnType("text[]");
            e.Property(x => x.IncompatibleWith).HasColumnName("incompatible_with").HasColumnType("text[]");
            e.Property(x => x.PrerequisiteAccessories).HasColumnName("prerequisite_accessories").HasColumnType("text[]");
            e.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            e.Property(x => x.Properties).HasColumnName("properties").HasColumnType("jsonb");
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
            e.HasIndex(x => x.Code).IsUnique().HasDatabaseName("ix_accessories_code");
        });

        // Part
        modelBuilder.Entity<Part>(e =>
        {
            e.ToTable("parts");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Sku).HasColumnName("sku").HasMaxLength(100).IsRequired();
            e.Property(x => x.Name).HasColumnName("name").HasMaxLength(300).IsRequired();
            e.Property(x => x.Category).HasColumnName("category").HasMaxLength(100).IsRequired();
            e.Property(x => x.Unit).HasColumnName("unit").HasMaxLength(20).IsRequired();
            e.Property(x => x.IsCuttable).HasColumnName("is_cuttable").HasDefaultValue(false);
            e.Property(x => x.WeightKg).HasColumnName("weight_kg").HasColumnType("decimal(10,4)");
            e.Property(x => x.CostPrice).HasColumnName("cost_price").HasColumnType("decimal(12,4)");
            e.Property(x => x.SupplierCode).HasColumnName("supplier_code").HasMaxLength(100);
            e.Property(x => x.Properties).HasColumnName("properties").HasColumnType("jsonb");
            e.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
            e.HasIndex(x => x.Sku).IsUnique().HasDatabaseName("ix_parts_sku");
            e.HasIndex(x => x.Category).HasDatabaseName("ix_parts_category");
        });

        // SkuMapping
        modelBuilder.Entity<SkuMapping>(e =>
        {
            e.ToTable("sku_mappings");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.ProductFamilyCode).HasColumnName("product_family_code").HasMaxLength(50).IsRequired();
            e.Property(x => x.Category).HasColumnName("category").HasMaxLength(100).IsRequired();
            e.Property(x => x.MatchCriteria).HasColumnName("match_criteria").HasColumnType("jsonb").IsRequired();
            e.Property(x => x.Sku).HasColumnName("sku").HasMaxLength(100).IsRequired();
            e.Property(x => x.Priority).HasColumnName("priority").HasDefaultValue(0);
            e.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
            e.HasIndex(x => new { x.ProductFamilyCode, x.Category, x.Priority }).HasDatabaseName("ix_sku_mappings_family_category_priority");
            e.HasOne(x => x.ProductFamily)
                .WithMany()
                .HasForeignKey(x => x.ProductFamilyCode)
                .HasPrincipalKey(x => x.Code)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_sku_mappings_family");
            e.HasOne(x => x.Part)
                .WithMany()
                .HasForeignKey(x => x.Sku)
                .HasPrincipalKey(x => x.Sku)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_sku_mappings_part");
        });

        // Configuration
        modelBuilder.Entity<Models.Domain.Configuration>(e =>
        {
            e.ToTable("configurations");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.ProductTypeId).HasColumnName("product_type_id").IsRequired();
            e.Property(x => x.Reference).HasColumnName("reference").HasMaxLength(255);
            e.Property(x => x.Status).HasColumnName("status").HasMaxLength(50).HasDefaultValue("draft").IsRequired();
            e.Property(x => x.ConfigData).HasColumnName("config_data").HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");
            e.Property(x => x.ValidationResult).HasColumnName("validation_result").HasColumnType("jsonb");
            e.Property(x => x.BomData).HasColumnName("bom_data").HasColumnType("jsonb");
            e.Property(x => x.Version).HasColumnName("version").HasDefaultValue(1).IsConcurrencyToken();
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
            e.Property(x => x.CreatedBy).HasColumnName("created_by").HasMaxLength(200);
            e.HasIndex(x => x.ProductTypeId).HasDatabaseName("ix_configurations_product_type_id");
            e.HasIndex(x => x.Status).HasDatabaseName("ix_configurations_status");
            e.HasIndex(x => x.CreatedAt).HasDatabaseName("ix_configurations_created_at");
            e.HasOne(x => x.ProductType)
                .WithMany()
                .HasForeignKey(x => x.ProductTypeId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_configurations_product_type");
        });

        // ConfigurationHistory
        modelBuilder.Entity<ConfigurationHistory>(e =>
        {
            e.ToTable("configuration_history");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.ConfigurationId).HasColumnName("configuration_id").IsRequired();
            e.Property(x => x.Action).HasColumnName("action").HasMaxLength(50).IsRequired();
            e.Property(x => x.SelectionsSnapshot).HasColumnName("selections_snapshot").HasColumnType("jsonb");
            e.Property(x => x.ValidationSnapshot).HasColumnName("validation_snapshot").HasColumnType("jsonb");
            e.Property(x => x.ChangedFields).HasColumnName("changed_fields").HasColumnType("text[]");
            e.Property(x => x.PerformedBy).HasColumnName("performed_by").HasMaxLength(200);
            e.Property(x => x.PerformedAt).HasColumnName("performed_at").HasDefaultValueSql("now()");
            e.HasIndex(x => x.ConfigurationId).HasDatabaseName("ix_configuration_history_configuration_id");
            e.HasOne(x => x.Configuration)
                .WithMany(x => x.History)
                .HasForeignKey(x => x.ConfigurationId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_configuration_history_configuration");
        });

        // BomLine
        modelBuilder.Entity<BomLine>(e =>
        {
            e.ToTable("bom_lines");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.ConfigurationId).HasColumnName("configuration_id").IsRequired();
            e.Property(x => x.PartSku).HasColumnName("part_sku").HasMaxLength(100).IsRequired();
            e.Property(x => x.PartName).HasColumnName("part_name").HasMaxLength(300);
            e.Property(x => x.Category).HasColumnName("category").HasMaxLength(100);
            e.Property(x => x.Quantity).HasColumnName("quantity").HasColumnType("decimal(10,2)");
            e.Property(x => x.Unit).HasColumnName("unit").HasMaxLength(20).IsRequired();
            e.Property(x => x.CutLengthMm).HasColumnName("cut_length_mm");
            e.Property(x => x.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
            e.Property(x => x.Notes).HasColumnName("notes");
            e.HasIndex(x => x.ConfigurationId).HasDatabaseName("ix_bom_lines_configuration_id");
            e.HasOne(x => x.Configuration)
                .WithMany(x => x.BomLines)
                .HasForeignKey(x => x.ConfigurationId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_bom_lines_configuration");
            e.HasOne(x => x.Part)
                .WithMany()
                .HasForeignKey(x => x.PartSku)
                .HasPrincipalKey(x => x.Sku)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_bom_lines_part");
        });

        // MesExport
        modelBuilder.Entity<MesExport>(e =>
        {
            e.ToTable("mes_exports");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.ConfigurationId).HasColumnName("configuration_id").IsRequired();
            e.Property(x => x.Payload).HasColumnName("payload").HasColumnType("jsonb").IsRequired();
            e.Property(x => x.Status).HasColumnName("status").HasMaxLength(50).HasDefaultValue("pending").IsRequired();
            e.Property(x => x.SentAt).HasColumnName("sent_at");
            e.Property(x => x.Response).HasColumnName("response").HasColumnType("jsonb");
            e.Property(x => x.ErrorMessage).HasColumnName("error_message");
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            e.HasIndex(x => x.ConfigurationId).HasDatabaseName("ix_mes_exports_configuration_id");
            e.HasIndex(x => x.Status).HasDatabaseName("ix_mes_exports_status");
            e.HasOne(x => x.Configuration)
                .WithMany(x => x.MesExports)
                .HasForeignKey(x => x.ConfigurationId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_mes_exports_configuration");
        });
    }
}
