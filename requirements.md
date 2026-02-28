# CPQ PoC — Requirements & Implementation Guide

## 1. Project Overview

### 1.1 Goal

Replace the legacy Navision-based CPQ system ("Econ") with a modern, maintainable product configurator for solar protection / external shutter products. The system must support hundreds of product variants with complex interdependent rules, managed by technical staff without developer involvement.

### 1.2 Core Outcomes

| Stakeholder | Outcome |
|---|---|
| **End user (customer / sales)** | Configure a valid product by making guided choices. Cannot submit an invalid configuration. Real-time feedback on every choice. |
| **Internal technical staff** | Define and maintain product rules using a visual editor (decision tables, expressions, JS functions). Add new product variants by composing existing rule building blocks. No C# knowledge required. |
| **Production / MES** | Receive a Bill of Materials (BOM) generated from the validated configuration, ready for the MES system. |

### 1.3 Key Principles

- Rules are **data, not code** — stored as JSON (JDM format), editable without redeployment.
- The C# application **never contains product logic** — all validation, option filtering, calculations, and BOM rules live in GoRules.
- Product specifications (dimensions, materials, SKUs) live in a **database** and are **injected into the rule context** at runtime — rules are generic, data makes them product-specific.
- Rules are **layered and composable** — shared building blocks are reused across product families via GoRules Decision nodes. Adding a new product variant should require minimal new rules.
- The architecture must scale to **hundreds of product types** without linear growth in rule files.

### 1.4 Scale Requirements

| Metric | Target |
|---|---|
| Product families | 10-20 (e.g., roller shutters, screens, awnings, venetian blinds) |
| Product variants per family | 10-50 (e.g., standard, insulated, security, mini, maxi) |
| Total product configurations | 200-500+ |
| Configuration parameters per product | 10-30 |
| Rules per product family | 50-200 |
| Concurrent users | 20-50 sales staff |
| Configuration response time | < 500ms per interaction |

---

## 2. Architecture

### 2.1 High-Level Components

```
┌──────────────────────────────────────────────────────────────┐
│                       DOCKER COMPOSE                          │
│                                                               │
│  ┌───────────────────┐         ┌───────────────────────────┐ │
│  │ GoRules Editor     │         │ GoRules Agent              │ │
│  │ gorules/editor     │         │ gorules/agent               │ │
│  │ Port: 3000         │         │ Port: 8080                 │ │
│  │                    │         │                            │ │
│  │ Used by: internal  │         │ REST API:                  │ │
│  │ technical staff    │         │ POST /decisions/{n}/       │ │
│  │                    │         │      evaluate              │ │
│  └────────┬───────────┘         └────────┬───────────────────┘ │
│           │ writes .json                 │ reads .json         │
│           ▼                              ▼                     │
│      ┌──────────────────────────────────────────┐             │
│      │ /rules (shared Docker volume)              │             │
│      │                                            │             │
│      │  shared/                                   │             │
│      │    motor-torque-calculation.json            │             │
│      │    weight-calculation.json                  │             │
│      │    color-availability.json                  │             │
│      │    dimension-validation.json                │             │
│      │  families/                                  │             │
│      │    roller-shutter/                           │             │
│      │      validate.json   (calls shared/*)       │             │
│      │      options.json                           │             │
│      │      bom.json                               │             │
│      │    screen/                                   │             │
│      │      validate.json                          │             │
│      │      options.json                           │             │
│      │      bom.json                               │             │
│      └──────────────────────────────────────────┘             │
│                                                               │
│  ┌──────────────────────────────────────────────────────┐     │
│  │ ASP.NET Core Web API  (Port: 5000)                    │     │
│  │                                                       │     │
│  │  Orchestration Layer:                                 │     │
│  │    1. Receives config request                         │     │
│  │    2. Loads product spec from DB                      │     │
│  │    3. Merges spec + user selections into context      │     │
│  │    4. Calls GoRules Agent with enriched context       │     │
│  │    5. Returns validation + available options + BOM    │     │
│  └──────────────────────────────────────────────────────┘     │
│                                                               │
│  ┌──────────────┐    ┌────────────────────────────────┐       │
│  │ PostgreSQL   │    │ Frontend (Angular 18)          │       │
│  │ Port: 5432   │    │ Port: 4200                     │       │
│  └──────────────┘    └────────────────────────────────┘       │
└──────────────────────────────────────────────────────────────┘
```

### 2.2 Technology Stack

| Component | Technology | Justification |
|---|---|---|
| Backend API | ASP.NET Core 8 Web API | Dev team preference, LTS |
| Database | PostgreSQL | Product catalog, specs, configurations, orders |
| ORM | Entity Framework Core | Standard .NET ORM |
| Rules Engine | GoRules Agent (Docker) | REST-based, language-agnostic |
| Rules Editor | GoRules Editor (Docker) | Free standalone editor, no license needed |
| Rules Format | JDM (JSON Decision Model) | Portable, versionable, Git-friendly |
| Frontend | Angular 18 | Dev team choice, TypeScript-native, reactive forms |
| Containerization | Docker Compose | Local dev + deployment |

---

## 3. Layered Rule Architecture

This is the core design decision that enables scaling to hundreds of products. Rules are organized in three layers, with higher layers calling lower layers via GoRules **Decision nodes**.

### 3.1 The Three Layers

```
Layer 3: PRODUCT ENTRY POINT (per product family)
         ┌─────────────────────────────────────────┐
         │  families/roller-shutter/validate.json    │
         │                                           │
         │  Input ──► Switch (by variant) ──┬──► Standard path
         │                                  ├──► Insulated path
         │                                  └──► Security path
         │           Each path calls shared rules    │
         │           with product-specific params     │
         └─────────────────────────────────────────┘
                           │ calls via Decision nodes
                           ▼
Layer 2: SHARED RULE BUILDING BLOCKS (reusable across all products)
         ┌─────────────────────────────────────────┐
         │  shared/dimension-validation.json         │
         │  shared/motor-torque-calculation.json      │
         │  shared/weight-calculation.json            │
         │  shared/color-availability.json            │
         │  shared/guide-rail-selection.json           │
         │  shared/box-size-selection.json             │
         │  shared/slat-calculation.json               │
         │  shared/hardware-selection.json              │
         └─────────────────────────────────────────┘
                           │ reads from
                           ▼
Layer 1: PRODUCT SPECIFICATIONS (database, injected by C# at runtime)
         ┌─────────────────────────────────────────┐
         │  Product spec data (from PostgreSQL):     │
         │  - min/max dimensions per material        │
         │  - available materials, profiles, colors   │
         │  - motor specs (torque, compatibility)     │
         │  - weight tables, pricing coefficients     │
         │  - SKU mappings                            │
         └─────────────────────────────────────────┘
```

### 3.2 Why This Matters

Without layering, you'd need:
- 3 JDM files × 200 products = **600 rule files**, each with duplicated logic

With layering:
- ~15 shared building blocks + ~20 family entry points = **~35 rule files total**
- Product-specific behavior comes from **data in the database**, not from separate rule files
- Adding a new product variant = add a row to `product_specs`, possibly add a Switch branch

### 3.3 GoRules Node Types Used

| Node Type | Purpose in CPQ | Example |
|---|---|---|
| **Decision Table** | Lookup rules, constraint tables, option filtering | Material ↔ max dimension table |
| **Expression Node** | Field calculations and transformations | `rollDiameter = numSlats * slatThickness * 2 / PI` |
| **Function Node** (JS) | Complex multi-step algorithms, BOM assembly logic | Weight calculation with material density lookup, iterative box sizing |
| **Switch Node** | Branching by product variant or configuration path | If variant == "insulated" → insulated rules path |
| **Decision Node** | Calls a sub-decision (another JDM file) — the key to composition | `validate.json` calls `shared/motor-torque-calculation.json` |
| **Input/Output** | Entry and exit of every graph | Context in → result out |

### 3.4 Decision Node Pattern (Sub-Graph Calls)

The Decision node is GoRules' composition mechanism. When a graph contains a Decision node pointing to `shared/weight-calculation.json`, the Agent's loader resolves that file from the filesystem and executes it inline, passing the current context forward.

```
families/roller-shutter/validate.json:

  Input ──► Expression: "Enrich context" ──► Decision: shared/dimension-validation.json
                                            ──► Decision: shared/weight-calculation.json
                                            ──► Decision: shared/motor-torque-calculation.json
                                            ──► Function: "Merge validation results"
                                            ──► Output
```

The shared graphs are **generic** — they don't know about roller shutters specifically. They operate on abstract fields like `spec.maxWidth`, `spec.slatWeightPerM2`, etc. The C# orchestration layer injects the correct product spec data so the same shared graph works for any product.

---

## 4. Data-Driven Product Specifications

### 4.1 Core Insight

Instead of encoding "PVC max width is 3000mm" as a hardcoded row in a decision table, we store that constraint as **data** in the database and inject it into the rule context at runtime. The rule becomes generic: "if `width > spec.maxWidth` then error".

This means:
- Adding a new material = add rows to `product_specs`, no rule changes
- Changing a dimension limit = update a database value, no rule changes
- Adding a new product variant = add a product spec record + possibly a Switch branch

### 4.2 Database Schema

```sql
-- ═══════════════════════════════════════════════════════════
-- PRODUCT HIERARCHY
-- ═══════════════════════════════════════════════════════════

-- Top-level product families (roller_shutter, screen, awning, etc.)
CREATE TABLE product_families (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    code VARCHAR(50) UNIQUE NOT NULL,               -- "roller_shutter"
    name VARCHAR(200) NOT NULL,                      -- "Rolluik"
    description TEXT,
    rule_prefix VARCHAR(100) NOT NULL,               -- "families/roller-shutter"
    -- ^ maps to rule folder: families/roller-shutter/validate.json etc.
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMPTZ DEFAULT now(),
    updated_at TIMESTAMPTZ DEFAULT now()
);

-- Specific product types within a family
-- e.g., "roller_shutter_standard", "roller_shutter_insulated", "roller_shutter_security"
CREATE TABLE product_types (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    family_id UUID NOT NULL REFERENCES product_families(id),
    code VARCHAR(100) UNIQUE NOT NULL,               -- "roller_shutter_insulated"
    name VARCHAR(200) NOT NULL,                      -- "Rolluik geïsoleerd"
    variant VARCHAR(50) NOT NULL,                    -- "insulated" (used in Switch nodes)
    description TEXT,
    display_order INT DEFAULT 0,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMPTZ DEFAULT now(),
    updated_at TIMESTAMPTZ DEFAULT now()
);

-- ═══════════════════════════════════════════════════════════
-- PRODUCT SPECIFICATIONS (the data that drives generic rules)
-- ═══════════════════════════════════════════════════════════

-- Configuration parameters defined per product type
-- These define WHAT can be configured (the wizard steps)
CREATE TABLE product_parameters (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    product_type_id UUID NOT NULL REFERENCES product_types(id),
    code VARCHAR(100) NOT NULL,                      -- "width", "material", "motor_brand"
    name VARCHAR(200) NOT NULL,                      -- "Breedte", "Materiaal"
    data_type VARCHAR(50) NOT NULL,                  -- "integer", "decimal", "select", "multi_select", "boolean"
    unit VARCHAR(20),                                -- "mm", "kg", null
    step_number INT NOT NULL,                        -- wizard step this belongs to
    step_name VARCHAR(200),                          -- "Afmetingen", "Materiaal & Profiel"
    display_order INT DEFAULT 0,
    is_required BOOLEAN DEFAULT true,
    default_value VARCHAR(200),                      -- default selection
    depends_on VARCHAR(100)[],                       -- parameters that must be set first
    metadata JSONB,                                  -- UI hints: min, max, placeholder, help text
    UNIQUE(product_type_id, code)
);

-- Available options per parameter (for select/multi_select types)
CREATE TABLE product_options (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    product_type_id UUID NOT NULL REFERENCES product_types(id),
    parameter_code VARCHAR(100) NOT NULL,             -- "material", "color", "motor_brand"
    code VARCHAR(100) NOT NULL,                       -- "ALU", "RAL7016", "somfy"
    display_name VARCHAR(200) NOT NULL,               -- "Aluminium", "Antraciet", "Somfy"
    display_order INT DEFAULT 0,
    is_active BOOLEAN DEFAULT true,
    metadata JSONB,                                   -- color hex, image URL, description
    UNIQUE(product_type_id, parameter_code, code)
);

-- Product specifications — the constraints and properties
-- This is the KEY TABLE that drives the generic rules
CREATE TABLE product_specs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    product_type_id UUID NOT NULL REFERENCES product_types(id),
    spec_group VARCHAR(100) NOT NULL,                 -- "dimensions", "materials", "motors", "profiles"
    spec_key VARCHAR(200) NOT NULL,                   -- "ALU.max_width", "PVC.min_height"
    spec_value JSONB NOT NULL,                        -- 4500, true, {"torque": 15, "brand": "somfy"}
    description TEXT,                                 -- human-readable explanation
    UNIQUE(product_type_id, spec_group, spec_key)
);

-- ═══════════════════════════════════════════════════════════
-- MATERIAL & PART SPECIFICATIONS
-- ═══════════════════════════════════════════════════════════

-- Material properties (shared across products)
CREATE TABLE materials (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    code VARCHAR(50) UNIQUE NOT NULL,                 -- "ALU", "PVC", "ALU_INSULATED"
    name VARCHAR(200) NOT NULL,                       -- "Aluminium"
    density_kg_per_m3 DECIMAL(10,2),
    is_active BOOLEAN DEFAULT true,
    properties JSONB                                  -- thermal conductivity, UV resistance, etc.
);

-- Slat/profile specifications
CREATE TABLE profiles (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    code VARCHAR(50) UNIQUE NOT NULL,                 -- "ALU-39", "PVC-37", "ALU-INS-55"
    name VARCHAR(200) NOT NULL,                       -- "Aluminium lat 39mm"
    material_code VARCHAR(50) NOT NULL REFERENCES materials(code),
    height_mm DECIMAL(10,2) NOT NULL,                 -- 39
    thickness_mm DECIMAL(10,2) NOT NULL,              -- 8.5
    weight_per_meter_kg DECIMAL(10,4) NOT NULL,       -- 1.85
    max_width_mm INT NOT NULL,                        -- 4500
    min_width_mm INT DEFAULT 400,
    properties JSONB                                  -- insulation value, wind class, etc.
);

-- Motor specifications
CREATE TABLE motors (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    code VARCHAR(50) UNIQUE NOT NULL,                 -- "SOMFY-IO-15"
    brand VARCHAR(100) NOT NULL,                      -- "Somfy"
    model VARCHAR(200) NOT NULL,                      -- "Ilmo 2 IO 15/17"
    torque_nm DECIMAL(10,2) NOT NULL,                 -- 15
    speed_rpm DECIMAL(10,2),
    max_weight_kg DECIMAL(10,2),
    max_surface_m2 DECIMAL(10,2),
    control_types VARCHAR(50)[] NOT NULL,             -- {"switch", "remote", "smart_home"}
    tube_diameter_mm INT,                             -- 60
    properties JSONB                                  -- power consumption, IP rating, etc.
);

-- Box/housing specifications
CREATE TABLE boxes (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    code VARCHAR(50) UNIQUE NOT NULL,                 -- "BOX-ALU-165"
    name VARCHAR(200) NOT NULL,
    type VARCHAR(50) NOT NULL,                        -- "surface_mount", "built_in", "concealed"
    inner_diameter_mm INT NOT NULL,                   -- 145
    outer_height_mm INT NOT NULL,                     -- 165
    compatible_materials VARCHAR(50)[] NOT NULL,       -- {"ALU", "ALU_INSULATED"}
    max_width_mm INT,
    properties JSONB
);

-- SKU / part catalog for BOM generation
CREATE TABLE parts (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    sku VARCHAR(100) UNIQUE NOT NULL,                 -- "SLAT-ALU-39-RAL7016"
    name VARCHAR(300) NOT NULL,                       -- "Aluminium lat 39mm RAL7016"
    category VARCHAR(100) NOT NULL,                   -- "slat", "guide", "motor", "box", "hardware", "accessory"
    unit VARCHAR(20) NOT NULL,                        -- "pcs", "mm", "m", "set"
    is_cuttable BOOLEAN DEFAULT false,
    weight_kg DECIMAL(10,4),
    cost_price DECIMAL(10,2),
    supplier_code VARCHAR(100),
    properties JSONB,
    is_active BOOLEAN DEFAULT true
);

-- SKU resolution rules: maps configuration choices to actual SKUs
-- This avoids encoding SKU logic in GoRules
CREATE TABLE sku_mappings (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    product_family_code VARCHAR(50) NOT NULL,
    category VARCHAR(100) NOT NULL,                   -- "slat", "guide", "box", "motor"
    match_criteria JSONB NOT NULL,                    -- {"material": "ALU", "profile": "39", "color": "RAL7016"}
    sku VARCHAR(100) NOT NULL REFERENCES parts(sku),
    priority INT DEFAULT 0,                           -- higher = more specific match wins
    is_active BOOLEAN DEFAULT true
);

-- ═══════════════════════════════════════════════════════════
-- CONFIGURATIONS & ORDERS
-- ═══════════════════════════════════════════════════════════

CREATE TABLE configurations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    product_type_id UUID NOT NULL REFERENCES product_types(id),
    reference VARCHAR(100),                           -- customer order reference
    status VARCHAR(50) DEFAULT 'draft',               -- draft, validated, ordered, sent_to_mes
    config_data JSONB NOT NULL,                       -- user selections
    validation_result JSONB,                          -- last validation from rules engine
    bom_data JSONB,                                   -- last generated BOM
    created_at TIMESTAMPTZ DEFAULT now(),
    updated_at TIMESTAMPTZ DEFAULT now(),
    created_by VARCHAR(200)
);

CREATE TABLE bom_lines (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    configuration_id UUID NOT NULL REFERENCES configurations(id),
    part_sku VARCHAR(100) NOT NULL,
    part_name VARCHAR(300),
    category VARCHAR(100),
    quantity DECIMAL(10,2) NOT NULL,
    unit VARCHAR(20) NOT NULL,
    cut_length_mm INT,
    sort_order INT DEFAULT 0,
    notes TEXT
);

CREATE TABLE mes_exports (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    configuration_id UUID NOT NULL REFERENCES configurations(id),
    payload JSONB NOT NULL,
    status VARCHAR(50) DEFAULT 'pending',             -- pending, sent, acknowledged, failed
    sent_at TIMESTAMPTZ,
    response JSONB,
    error_message TEXT
);
```

### 4.3 How Product Specs Drive Generic Rules

Example: the shared `dimension-validation.json` rule graph receives this context (assembled by C#):

```json
{
  "userSelections": {
    "width": 3200,
    "height": 1800,
    "material": "PVC"
  },
  "spec": {
    "dimensions": {
      "PVC": { "minWidth": 400, "maxWidth": 3000, "minHeight": 400, "maxHeight": 3000 },
      "ALU": { "minWidth": 400, "maxWidth": 4500, "minHeight": 400, "maxHeight": 4000 },
      "ALU_INSULATED": { "minWidth": 400, "maxWidth": 4000, "minHeight": 400, "maxHeight": 3500 }
    }
  }
}
```

The decision table in `dimension-validation.json` doesn't hardcode materials or limits — it uses expression-based lookups against the `spec` object. The same rule works for any product because the spec data changes, not the rule.

---

## 5. Complex Decision Logic Patterns

### 5.1 Pattern: Multi-Step Calculation Chain

Used for: motor sizing, weight calculation, box selection.

These calculations have intermediate results that feed subsequent steps. GoRules handles this by chaining nodes left-to-right in the graph.

```
Graph: shared/motor-sizing.json

Input ──► Expression: "Calculate weight"
          │  slatWeight = userSelections.width * userSelections.height
          │              * spec.profiles[userSelections.profile].weightPerM2 / 1000000
          │  guideWeight = userSelections.height * spec.guides[userSelections.guideType].weightPerMeter * 2 / 1000
          │  totalWeight = slatWeight + guideWeight
          │
          ├──► Expression: "Calculate roll geometry"
          │    │  numSlats = ceil(userSelections.height / spec.profiles[userSelections.profile].pitch)
          │    │  rollDiameter = numSlats * spec.profiles[userSelections.profile].thickness * 2 / 3.14159
          │    │  requiredTorque = totalWeight * rollDiameter / 2000 * 1.3  // 30% safety margin
          │    │
          │    ├──► Decision Table: "Filter compatible motors"
          │    │    │  torque_nm >= requiredTorque AND
          │    │    │  max_surface_m2 >= area AND
          │    │    │  brand == userSelections.motorBrand (if set)
          │    │    │  → returns: compatibleMotors[], recommendedMotor, warnings[]
          │    │    │
          │    │    └──► Output
```

### 5.2 Pattern: Cascading Dependencies with Switch

Used for: product variants that share 80% of rules but differ in specific areas.

```
Graph: families/roller-shutter/validate.json

Input ──► Decision: shared/dimension-validation.json
      ──► Decision: shared/weight-calculation.json
      ──►
          Switch (on: userSelections.variant)
          ├── "standard"  ──► Decision Table: standard-specific-rules
          │                   (basic material/motor constraints)
          │
          ├── "insulated" ──► Decision Table: insulated-specific-rules
          │                   (only ALU_INSULATED, extra U-value check)
          │                ──► Expression: thermal-calculations
          │
          ├── "security"  ──► Decision Table: security-specific-rules
          │                   (min thickness, anti-lift check, certified lock)
          │                ──► Decision: shared/security-certification.json
          │
          └── (default)   ──► Expression: { error: "Unknown variant" }
      ──►
          Function (JS): "Merge all validation results"
          │  // Collects errors/warnings from all paths
          │  // Deduplicates, sorts by severity
          │  // Returns unified validation response
      ──► Output
```

### 5.3 Pattern: Conditional Sub-Configurations

Used for: when selecting a motor opens up a whole sub-configuration (brand → model → control type → accessories).

```
Graph: shared/drive-configuration.json

Input ──►
    Switch (on: userSelections.driveType)
    │
    ├── "manual_strap"  ──► Decision Table: strap-options
    │                       (strap width by shutter weight, side selection)
    │
    ├── "manual_crank"  ──► Decision Table: crank-options
    │                       (crank type, gear ratio by weight)
    │
    ├── "motor"         ──► Decision: shared/motor-sizing.json
    │                   ──► Decision Table: motor-control-options
    │                       (control types by motor brand)
    │                   ──► Decision Table: motor-accessories
    │                       (receiver, remote, smart home bridge)
    │
    └── (default)       ──► Expression: { error: "Invalid drive type" }
    │
    ──► Output
```

### 5.4 Pattern: BOM Assembly with Function Nodes

BOM generation is the most complex logic — it requires iterative calculations, SKU resolution, and conditional part inclusion. This is where **Function nodes** (JavaScript) shine because the logic is too algorithmic for pure decision tables.

```
Graph: families/roller-shutter/bom.json

Input ──► Decision: shared/weight-calculation.json        (get calculated values)
      ──► Decision: shared/motor-sizing.json               (get motor specs)
      ──►
          Function (JS): "Generate slat BOM lines"
          │  // Calculate number of slats, cut lengths
          │  // Account for end clearances per guide type
          │  // Handle color-matched vs. standard slats
      ──►
          Function (JS): "Generate box BOM lines"
          │  // Select box size from roll diameter
          │  // Include endcaps, axle, bearings
          │  // Handle built-in vs surface-mount differences
      ──►
          Function (JS): "Generate guide rail BOM lines"
          │  // Cut lengths, brackets per height
          │  // Wind-resistant variants need extra hardware
      ──►
          Function (JS): "Generate motor & drive BOM lines"
          │  // Motor SKU, adapter, control unit
          │  // Cable length based on installation height
      ──►
          Function (JS): "Generate hardware & fastener BOM"
          │  // Mount type determines bracket type + count
          │  // Screws, plugs, sealant by wall material
      ──►
          Function (JS): "Assemble final BOM"
          │  // Merge all BOM sections
          │  // Resolve SKUs from sku_mappings (passed in spec)
          │  // Calculate total weight, pack size
          │  // Add production notes
      ──► Output
```

### 5.5 Pattern: Cross-Field Constraint Validation

Some rules involve relationships between multiple fields that aren't simple lookups. These use Function nodes with JavaScript.

```javascript
// Function node: complex cross-field validation
export const handler = async (input) => {
  const { userSelections: s, spec } = input;
  const errors = [];
  const warnings = [];

  // Area-based constraints
  const area = (s.width * s.height) / 1000000; // m²
  const maxArea = spec.materials[s.material]?.maxArea;
  if (maxArea && area > maxArea) {
    errors.push({
      parameter: "width,height",
      rule: "max_area",
      message: `Oppervlakte ${area.toFixed(2)}m² overschrijdt max ${maxArea}m² voor ${s.material}`
    });
  }

  // Wind resistance class check (depends on width, height, guide type, material)
  if (s.guideType === "wind_resistant") {
    const windClass = calculateWindClass(s.width, s.height, s.material, spec);
    if (windClass < spec.minimumWindClass) {
      errors.push({
        parameter: "guideType",
        rule: "wind_class_insufficient",
        message: `Windklasse ${windClass} is onvoldoende (min: ${spec.minimumWindClass})`
      });
    }
  }

  // Weight-based motor override
  const weight = input.$nodes.WeightCalculation.totalWeight;
  if (s.driveType === "manual_strap" && weight > 30) {
    warnings.push({
      parameter: "driveType",
      rule: "heavy_for_manual",
      message: `Gewicht ${weight.toFixed(1)}kg is zwaar voor handbediening. Overweeg motor.`
    });
  }

  // Insulated variant requires insulated material
  if (s.variant === "insulated" && s.material !== "ALU_INSULATED") {
    errors.push({
      parameter: "material",
      rule: "insulated_requires_insulated_material",
      message: "Geïsoleerde variant vereist geïsoleerd aluminium profiel"
    });
  }

  return { errors, warnings, valid: errors.length === 0 };
};
```

### 5.6 Pattern: Dynamic Option Filtering

The option filtering graph returns which options are still valid given current selections. It must handle partial configurations (not all fields filled yet).

```javascript
// Function node in families/roller-shutter/options.json
export const handler = async (input) => {
  const { userSelections: s, spec, allOptions } = input;
  const result = {};

  // Materials: filter by dimension constraints
  if (s.width || s.height) {
    result.material = allOptions.material.map(opt => ({
      code: opt.code,
      available: (!s.width || s.width <= spec.dimensions[opt.code]?.maxWidth)
              && (!s.height || s.height <= spec.dimensions[opt.code]?.maxHeight),
      reason: s.width > spec.dimensions[opt.code]?.maxWidth
              ? `Max breedte ${spec.dimensions[opt.code].maxWidth}mm` : null
    }));
  }

  // Motors: filter by calculated torque requirement
  if (s.driveType === "motor") {
    const required = input.$nodes?.MotorSizing?.requiredTorque || 0;
    result.motorModel = allOptions.motorModel.map(opt => ({
      code: opt.code,
      available: spec.motors[opt.code]?.torque >= required,
      reason: spec.motors[opt.code]?.torque < required
              ? `Koppel ${spec.motors[opt.code].torque}Nm < vereist ${required.toFixed(1)}Nm` : null
    }));
  }

  // Box types: filter by material compatibility
  if (s.material) {
    result.boxType = allOptions.boxType.map(opt => ({
      code: opt.code,
      available: spec.boxes[opt.code]?.compatibleMaterials?.includes(s.material),
      reason: !spec.boxes[opt.code]?.compatibleMaterials?.includes(s.material)
              ? `Niet beschikbaar voor ${s.material}` : null
    }));
  }

  return { availableOptions: result };
};
```

---

## 6. C# Orchestration Layer

### 6.1 The Enrichment Pattern

The C# backend does NOT contain product logic. Its job is to **assemble the full context** by combining user selections with product spec data from the database, then call the GoRules Agent.

```csharp
public class ConfigurationService : IConfigurationService
{
    private readonly IRulesEngineClient _rules;
    private readonly IProductSpecRepository _specs;
    private readonly IProductOptionRepository _options;

    public async Task<ConfigurationResponse> UpdateConfiguration(
        Guid configId, UpdateConfigurationRequest request, CancellationToken ct)
    {
        // 1. Load current configuration
        var config = await _configs.GetAsync(configId, ct);
        var productType = await _productTypes.GetWithFamilyAsync(config.ProductTypeId, ct);

        // 2. Merge new selections into existing
        config.MergeSelections(request.Selections);

        // 3. Load product specifications from DB
        var spec = await _specs.GetSpecContextAsync(config.ProductTypeId, ct);

        // 4. Load all available options for this product type
        var allOptions = await _options.GetAllOptionsAsync(config.ProductTypeId, ct);

        // 5. Build the enriched context for the rules engine
        var context = new
        {
            userSelections = config.ConfigData,
            variant = productType.Variant,
            productFamily = productType.Family.Code,
            spec = spec,            // all constraints, dimensions, motor specs etc.
            allOptions = allOptions  // all possible option values
        };

        // 6. Determine which rule set to call based on product family
        var rulePrefix = productType.Family.RulePrefix;
        // e.g., "families/roller-shutter"

        // 7. Call validation rules
        var validationResult = await _rules.EvaluateAsync<ValidationResult>(
            $"{rulePrefix}/validate", context, ct);

        // 8. Call option filtering rules
        var optionsResult = await _rules.EvaluateAsync<AvailableOptionsResponse>(
            $"{rulePrefix}/options", context, ct);

        // 9. Save and return
        config.ValidationResult = validationResult;
        await _configs.SaveAsync(config, ct);

        return new ConfigurationResponse
        {
            Id = config.Id,
            Config = config.ConfigData,
            Validation = validationResult,
            AvailableOptions = optionsResult,
            IsComplete = IsConfigurationComplete(config, productType),
            CanFinalize = validationResult.Valid && IsConfigurationComplete(config, productType)
        };
    }

    public async Task<BomResponse> GenerateBom(Guid configId, CancellationToken ct)
    {
        var config = await _configs.GetAsync(configId, ct);
        var productType = await _productTypes.GetWithFamilyAsync(config.ProductTypeId, ct);

        // Full spec context including SKU mappings
        var spec = await _specs.GetFullSpecContextAsync(config.ProductTypeId, ct);
        var skuMappings = await _skuMappings.GetForFamilyAsync(productType.Family.Code, ct);

        var context = new
        {
            userSelections = config.ConfigData,
            variant = productType.Variant,
            spec = spec,
            skuMappings = skuMappings
        };

        var rulePrefix = productType.Family.RulePrefix;
        var bomResult = await _rules.EvaluateAsync<BomResult>(
            $"{rulePrefix}/bom", context, ct);

        // Post-process: resolve SKUs, enrich with part details from DB
        var enrichedBom = await _bomService.EnrichBomWithPartDetails(bomResult, ct);

        config.BomData = enrichedBom;
        await _configs.SaveAsync(config, ct);

        return enrichedBom;
    }
}
```

### 6.2 RulesEngineClient

```csharp
public interface IRulesEngineClient
{
    Task<T> EvaluateAsync<T>(string decisionPath, object context,
                              CancellationToken ct = default);
    Task<JsonDocument> EvaluateRawAsync(string decisionPath, object context,
                                         CancellationToken ct = default);
}

public class RulesEngineClient : IRulesEngineClient
{
    private readonly HttpClient _http;
    private readonly ILogger<RulesEngineClient> _logger;

    public async Task<T> EvaluateAsync<T>(string decisionPath, object context,
                                           CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var payload = new { context };
            var response = await _http.PostAsJsonAsync(
                $"/decisions/{decisionPath}/evaluate", payload, ct);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<GoRulesResponse<T>>(ct);
            _logger.LogInformation(
                "Rule {Decision} evaluated in {Elapsed}ms",
                decisionPath, stopwatch.ElapsedMilliseconds);

            return result!.Result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Rules engine call failed for {Decision}", decisionPath);
            throw new RulesEngineException($"Failed to evaluate {decisionPath}", ex);
        }
    }
}

// Registration in Program.cs:
builder.Services.AddHttpClient<IRulesEngineClient, RulesEngineClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["RulesEngine:BaseUrl"]!);
    client.Timeout = TimeSpan.FromSeconds(10);
})
.AddPolicyHandler(GetRetryPolicy())     // Polly retry for transient failures
.AddPolicyHandler(GetCircuitBreaker()); // Circuit breaker for Agent downtime
```

### 6.3 Spec Context Builder

Transforms normalized database rows into the nested JSON structure that rules expect.

```csharp
public class ProductSpecRepository : IProductSpecRepository
{
    public async Task<object> GetSpecContextAsync(Guid productTypeId, CancellationToken ct)
    {
        var specs = await _db.ProductSpecs
            .Where(s => s.ProductTypeId == productTypeId)
            .ToListAsync(ct);

        var profiles = await _db.Profiles.Where(p => p.IsActive).ToListAsync(ct);
        var motors = await _db.Motors.Where(m => m.IsActive).ToListAsync(ct);
        var boxes = await _db.Boxes.Where(b => b.IsActive).ToListAsync(ct);

        return new
        {
            dimensions = specs
                .Where(s => s.SpecGroup == "dimensions")
                .ToDictionary(s => s.SpecKey, s => s.SpecValue),

            profiles = profiles.ToDictionary(p => p.Code, p => new
            {
                heightMm = p.HeightMm,
                thicknessMm = p.ThicknessMm,
                weightPerMeter = p.WeightPerMeterKg,
                maxWidth = p.MaxWidthMm,
                minWidth = p.MinWidthMm,
                pitch = p.HeightMm + 0.5m
            }),

            motors = motors.ToDictionary(m => m.Code, m => new
            {
                torque = m.TorqueNm,
                maxWeight = m.MaxWeightKg,
                maxSurface = m.MaxSurfaceM2,
                controlTypes = m.ControlTypes,
                brand = m.Brand
            }),

            boxes = boxes.ToDictionary(b => b.Code, b => new
            {
                type = b.Type,
                innerDiameter = b.InnerDiameterMm,
                outerHeight = b.OuterHeightMm,
                compatibleMaterials = b.CompatibleMaterials,
                maxWidth = b.MaxWidthMm
            })
        };
    }
}
```

---

## 7. Rule File Organization

### 7.1 Directory Structure

```
/rules/
├── shared/                                    # Layer 2: reusable building blocks
│   ├── dimension-validation.json              # Generic dimension constraint checker
│   ├── weight-calculation.json                # Weight from dimensions + profile + material
│   ├── motor-torque-calculation.json           # Required torque from weight + geometry
│   ├── motor-sizing.json                       # Orchestrates weight → torque → motor filter
│   ├── color-availability.json                 # Color filtering by material + supplier
│   ├── guide-rail-selection.json               # Guide type constraints + sizing
│   ├── box-size-selection.json                 # Roll diameter → box selection
│   ├── drive-configuration.json                # Manual/motor sub-configuration
│   ├── security-certification.json             # Security-rated product validation
│   ├── wind-class-calculation.json             # Wind resistance class from dimensions
│   ├── hardware-selection.json                 # Brackets, fasteners by mount type
│   └── accessory-validation.json               # Accessory prerequisites + conflicts
│
├── families/                                   # Layer 3: product family entry points
│   ├── roller-shutter/
│   │   ├── validate.json                       # Calls shared/* + Switch by variant
│   │   ├── options.json                        # Available options given current selections
│   │   └── bom.json                            # BOM generation
│   ├── screen/
│   │   ├── validate.json
│   │   ├── options.json
│   │   └── bom.json
│   ├── venetian-blind/
│   │   ├── validate.json
│   │   ├── options.json
│   │   └── bom.json
│   ├── awning/
│   │   ├── validate.json
│   │   ├── options.json
│   │   └── bom.json
│   └── zip-screen/
│       ├── validate.json
│       ├── options.json
│       └── bom.json
│
└── README.md                                   # Rule authoring guide for technical staff
```

### 7.2 Adding a New Product Family

1. Create a new folder under `families/` (e.g., `families/pergola/`)
2. Create `validate.json`, `options.json`, `bom.json` — compose from `shared/*` building blocks
3. Add product family record to `product_families` table with `rule_prefix = "families/pergola"`
4. Add product types, parameters, options, and specs to database
5. **No C# code changes needed**

### 7.3 Adding a New Product Variant Within a Family

1. Add product type record (e.g., `roller_shutter_mini`) with `variant = "mini"`
2. Add product specs for the new variant (dimensions, constraints)
3. In the family's `validate.json`, add a new branch in the Switch node for "mini"
4. **No C# code changes needed**

### 7.4 Adding a New Constraint to an Existing Product

1. Update `product_specs` rows in the database
2. If it's a new type of constraint (not just changing a value), add a node in the relevant rule graph
3. Test via GoRules Editor simulator
4. **No C# code changes needed**

---

## 8. API Endpoints

### 8.1 Product Catalog

```
GET    /api/families                                  — list product families
GET    /api/families/{code}/products                  — list product types in family
GET    /api/products/{code}                           — get product type with parameters + options
GET    /api/products/{code}/parameters                — get configuration parameters (wizard definition)
```

### 8.2 Configuration (Wizard)

```
POST   /api/configurations
       Body: { "productTypeCode": "roller_shutter_standard" }
       Returns: { id, productType, config: {}, availableOptions, steps[] }

PATCH  /api/configurations/{id}
       Body: { "selections": { "width": 2500, "material": "ALU" } }
       Returns: {
         id, config,
         validation: { valid, errors[], warnings[] },
         availableOptions: { material: [...], motor: [...] },
         isComplete, canFinalize
       }

GET    /api/configurations/{id}
GET    /api/configurations/{id}/validate
POST   /api/configurations/{id}/finalize
```

### 8.3 BOM

```
POST   /api/configurations/{id}/bom
GET    /api/configurations/{id}/bom
```

### 8.4 MES Export

```
POST   /api/configurations/{id}/export
GET    /api/exports/{exportId}
```

---

## 9. Dynamic Wizard Definition

### 9.1 Data-Driven Steps

Wizard steps are defined in `product_parameters`, not in code. Each product type defines its own steps and parameter ordering. The frontend reads this and renders accordingly.

```json
{
  "steps": [
    {
      "stepNumber": 1,
      "name": "Afmetingen",
      "parameters": [
        { "code": "width", "name": "Breedte", "type": "integer", "unit": "mm",
          "required": true, "metadata": { "min": 400, "max": 6000, "step": 10 } },
        { "code": "height", "name": "Hoogte", "type": "integer", "unit": "mm",
          "required": true, "metadata": { "min": 400, "max": 4000, "step": 10 } }
      ]
    },
    {
      "stepNumber": 2,
      "name": "Materiaal & Profiel",
      "parameters": [
        { "code": "material", "name": "Materiaal", "type": "select",
          "required": true, "dependsOn": ["width", "height"] },
        { "code": "profile", "name": "Latprofiel", "type": "select",
          "required": true, "dependsOn": ["material"] }
      ]
    },
    {
      "stepNumber": 3,
      "name": "Kleur & Kast",
      "parameters": [
        { "code": "color", "name": "Kleur", "type": "select", "dependsOn": ["material"] },
        { "code": "boxType", "name": "Kasttype", "type": "select", "dependsOn": ["material"] }
      ]
    },
    {
      "stepNumber": 4,
      "name": "Aandrijving",
      "parameters": [
        { "code": "driveType", "name": "Bediening", "type": "select" },
        { "code": "motorBrand", "name": "Motormerk", "type": "select",
          "dependsOn": ["driveType"],
          "metadata": { "visibleWhen": "driveType == 'motor'" } },
        { "code": "motorModel", "name": "Motortype", "type": "select",
          "dependsOn": ["motorBrand", "width", "height", "material"],
          "metadata": { "visibleWhen": "driveType == 'motor'" } },
        { "code": "controlType", "name": "Besturing", "type": "select",
          "dependsOn": ["motorModel"],
          "metadata": { "visibleWhen": "driveType == 'motor'" } }
      ]
    },
    {
      "stepNumber": 5,
      "name": "Geleiders & Toebehoren",
      "parameters": [
        { "code": "guideType", "name": "Geleidertype", "type": "select" },
        { "code": "mountType", "name": "Montage", "type": "select" },
        { "code": "installSide", "name": "Bedieningszijde", "type": "select" },
        { "code": "accessories", "name": "Toebehoren", "type": "multi_select" }
      ]
    }
  ]
}
```

### 9.2 Real-Time Feedback Loop

On every PATCH (user changes a value), the backend:
1. Merges new selections into configuration state
2. Loads product specs from DB, builds enriched context
3. Calls `{family}/validate` via GoRules Agent → errors + warnings
4. Calls `{family}/options` via GoRules Agent → available options
5. Returns both to frontend in single response

The frontend uses this to:
- Show validation errors inline next to relevant fields
- Disable/gray out unavailable options with reason tooltip
- Show warnings as non-blocking info banners
- Conditionally show/hide parameters based on `visibleWhen`
- Enable "Finalize" button only when `canFinalize = true`

---

## 10. Angular Frontend

### 10.1 Technology Choices

| Concern | Choice |
|---|---|
| Framework | Angular 18 (standalone components, signals) |
| State | Signal-based store per feature (no NgRx for PoC) |
| HTTP | Built-in HttpClient with interceptors |
| Forms | Reactive forms for the wizard |
| Styling | Angular Material or PrimeNG (TBD) + SCSS |
| Routing | Angular Router with lazy-loaded feature modules |

### 10.2 Key Components

**ConfiguratorComponent** — The main wizard container. Receives the step definition from the API and renders `WizardStepComponent` instances dynamically.

**WizardStepComponent** — Renders a single wizard step. Iterates over the step's parameters and renders the appropriate field component based on `data_type`.

**ParameterFieldComponent** — Factory component that delegates to:
- `DimensionFieldComponent` — numeric input with unit label, min/max from metadata
- `SelectFieldComponent` — dropdown with options, disabled items show reason tooltip
- `MultiSelectFieldComponent` — checkbox group with disabled items
- `BooleanFieldComponent` — toggle

**ValidationPanelComponent** — Displays errors (blocking) and warnings (informational) returned from the PATCH response. Errors link to the relevant field.

### 10.3 Reactive Configuration Flow

```typescript
// configurator.store.ts — signal-based state management
@Injectable()
export class ConfiguratorStore {
  // State
  readonly configId = signal<string | null>(null);
  readonly selections = signal<Record<string, any>>({});
  readonly validation = signal<ValidationResult | null>(null);
  readonly availableOptions = signal<AvailableOptions | null>(null);
  readonly steps = signal<WizardStep[]>([]);
  readonly currentStep = signal<number>(1);
  readonly isLoading = signal(false);

  // Computed
  readonly currentStepDef = computed(() =>
    this.steps().find(s => s.stepNumber === this.currentStep()));
  readonly canFinalize = computed(() =>
    this.validation()?.valid === true && this.isComplete());
  readonly errors = computed(() =>
    this.validation()?.errors ?? []);
  readonly warnings = computed(() =>
    this.validation()?.warnings ?? []);

  constructor(private configService: ConfigurationService) {}

  // On every field change: PATCH → update state
  async updateField(code: string, value: any): Promise<void> {
    this.isLoading.set(true);
    try {
      const response = await firstValueFrom(
        this.configService.updateConfiguration(this.configId()!, { [code]: value })
      );
      this.selections.set(response.config);
      this.validation.set(response.validation);
      this.availableOptions.set(response.availableOptions);
    } finally {
      this.isLoading.set(false);
    }
  }
}
```

### 10.4 Select Field with Availability

```typescript
// select-field.component.ts
@Component({
  selector: 'cpq-select-field',
  template: `
    <label>{{ parameter().name }}</label>
    <select [formControl]="control" (change)="onChange()">
      <option value="">-- Kies --</option>
      @for (opt of options(); track opt.code) {
        <option
          [value]="opt.code"
          [disabled]="!opt.available"
          [title]="opt.reason ?? ''">
          {{ opt.displayName }}
          @if (!opt.available) { ({{ opt.reason }}) }
        </option>
      }
    </select>
  `
})
export class SelectFieldComponent {
  parameter = input.required<ProductParameter>();
  options = input.required<OptionWithAvailability[]>();
  control = new FormControl('');
  onChange = output<string>();
}
```

---

## 11. Docker Compose Setup

```yaml
version: "3.8"

services:
  gorules-editor:
    image: gorules/editor:latest
    ports:
      - "3000:3000"
    volumes:
      - rules-data:/data

  gorules-agent:
    image: gorules/agent:latest
    ports:
      - "8080:8080"
    volumes:
      - rules-data:/data
    environment:
      - PROVIDER__TYPE=Filesystem
      - PROVIDER__ROOT_DIR=/data

  cpq-api:
    build:
      context: ./src
      dockerfile: Dockerfile
    ports:
      - "5000:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__Default=Host=postgres;Database=cpq;Username=cpq;Password=cpq_secret
      - RulesEngine__BaseUrl=http://gorules-agent:8080
    depends_on:
      - postgres
      - gorules-agent

  cpq-frontend:
    build:
      context: ./frontend
      dockerfile: Dockerfile
    ports:
      - "4200:80"
    depends_on:
      - cpq-api
    environment:
      - API_URL=http://cpq-api:8080

  postgres:
    image: postgres:16
    ports:
      - "5432:5432"
    environment:
      - POSTGRES_DB=cpq
      - POSTGRES_USER=cpq
      - POSTGRES_PASSWORD=cpq_secret
    volumes:
      - pg-data:/var/lib/postgresql/data

volumes:
  rules-data:
  pg-data:
```

---

## 12. C# Project Structure

```
src/
├── Cpq.Api/
│   ├── Program.cs
│   ├── appsettings.json
│   ├── Dockerfile
│   ├── Controllers/
│   │   ├── ProductFamilyController.cs
│   │   ├── ProductController.cs
│   │   ├── ConfigurationController.cs
│   │   ├── BomController.cs
│   │   └── MesExportController.cs
│   ├── Services/
│   │   ├── Rules/
│   │   │   ├── IRulesEngineClient.cs
│   │   │   ├── RulesEngineClient.cs
│   │   │   └── RulesEngineException.cs
│   │   ├── Configuration/
│   │   │   ├── IConfigurationService.cs
│   │   │   └── ConfigurationService.cs
│   │   ├── Specs/
│   │   │   ├── IProductSpecRepository.cs
│   │   │   ├── ProductSpecRepository.cs
│   │   │   └── SpecContextBuilder.cs
│   │   ├── Bom/
│   │   │   ├── IBomService.cs
│   │   │   ├── BomService.cs
│   │   │   └── SkuResolver.cs
│   │   └── Mes/
│   │       ├── IMesExportService.cs
│   │       └── MesExportService.cs
│   ├── Models/
│   │   ├── Domain/
│   │   │   ├── ProductFamily.cs
│   │   │   ├── ProductType.cs
│   │   │   ├── ProductParameter.cs
│   │   │   ├── ProductOption.cs
│   │   │   ├── ProductSpec.cs
│   │   │   ├── Material.cs
│   │   │   ├── Profile.cs
│   │   │   ├── Motor.cs
│   │   │   ├── Box.cs
│   │   │   ├── Part.cs
│   │   │   ├── SkuMapping.cs
│   │   │   ├── Configuration.cs
│   │   │   ├── BomLine.cs
│   │   │   └── MesExport.cs
│   │   ├── Requests/
│   │   │   ├── CreateConfigurationRequest.cs
│   │   │   └── UpdateConfigurationRequest.cs
│   │   └── Responses/
│   │       ├── ConfigurationResponse.cs
│   │       ├── ValidationResult.cs
│   │       ├── AvailableOptionsResponse.cs
│   │       ├── BomResponse.cs
│   │       ├── ProductParametersResponse.cs
│   │       └── MesExportResponse.cs
│   ├── Data/
│   │   ├── CpqDbContext.cs
│   │   ├── Migrations/
│   │   └── Seeding/
│   │       ├── ProductFamilySeeder.cs
│   │       ├── RollerShutterSeeder.cs
│   │       └── PartsCatalogSeeder.cs
│   └── Configuration/
│       ├── RulesEngineOptions.cs
│       └── PollyPolicies.cs
├── Cpq.Api.Tests/
│   ├── Services/
│   │   ├── ConfigurationServiceTests.cs
│   │   ├── SpecContextBuilderTests.cs
│   │   └── BomServiceTests.cs
│   ├── Integration/
│   │   ├── RulesEngineIntegrationTests.cs
│   │   └── ConfigurationFlowTests.cs
│   └── TestData/
│       └── SampleConfigurations.cs
└── Cpq.sln

frontend/                                    # Angular 18 application
├── src/
│   ├── app/
│   │   ├── app.component.ts
│   │   ├── app.routes.ts
│   │   ├── core/
│   │   │   ├── services/
│   │   │   │   ├── product.service.ts          ← product catalog API
│   │   │   │   ├── configuration.service.ts    ← configuration CRUD + PATCH
│   │   │   │   ├── bom.service.ts              ← BOM generation
│   │   │   │   └── mes-export.service.ts       ← MES export
│   │   │   ├── models/
│   │   │   │   ├── product-family.model.ts
│   │   │   │   ├── product-type.model.ts
│   │   │   │   ├── configuration.model.ts
│   │   │   │   ├── validation-result.model.ts
│   │   │   │   ├── available-options.model.ts
│   │   │   │   └── bom.model.ts
│   │   │   └── interceptors/
│   │   │       └── error.interceptor.ts
│   │   ├── features/
│   │   │   ├── product-selector/
│   │   │   │   ├── product-selector.component.ts
│   │   │   │   └── product-card.component.ts
│   │   │   ├── configurator/
│   │   │   │   ├── configurator.component.ts     ← main wizard container
│   │   │   │   ├── wizard-step.component.ts      ← dynamic step renderer
│   │   │   │   ├── parameter-field.component.ts  ← renders by data_type
│   │   │   │   ├── select-field.component.ts     ← with disabled + reason
│   │   │   │   ├── dimension-field.component.ts  ← numeric input with unit
│   │   │   │   ├── validation-panel.component.ts ← errors + warnings
│   │   │   │   └── configurator.store.ts         ← signal-based state
│   │   │   ├── bom-viewer/
│   │   │   │   ├── bom-viewer.component.ts
│   │   │   │   └── bom-line.component.ts
│   │   │   └── export/
│   │   │       └── mes-export.component.ts
│   │   └── shared/
│   │       ├── components/
│   │       │   ├── loading-spinner.component.ts
│   │       │   └── status-badge.component.ts
│   │       └── pipes/
│   │           └── unit-format.pipe.ts
│   ├── environments/
│   │   ├── environment.ts
│   │   └── environment.prod.ts
│   └── styles.scss
├── Dockerfile
├── angular.json
├── package.json
└── tsconfig.json
```

---

## 13. MES Export Payload

```json
{
  "orderReference": "WO-2026-001234",
  "productFamily": "roller_shutter",
  "productType": "roller_shutter_standard",
  "configurationId": "uuid",
  "specifications": {
    "width": 2500,
    "height": 1800,
    "material": "ALU",
    "profile": "ALU-39",
    "color": "RAL7016",
    "boxType": "surface_mount",
    "driveType": "motor",
    "motorModel": "SOMFY-IO-15",
    "guideType": "wind_resistant",
    "mountType": "wall",
    "installSide": "left"
  },
  "calculatedValues": {
    "totalWeightKg": 28.5,
    "rollDiameterMm": 145,
    "requiredTorqueNm": 12.3,
    "numberOfSlats": 46,
    "windClass": 4
  },
  "bom": [
    {
      "lineNumber": 1,
      "sku": "SLAT-ALU-39-RAL7016",
      "name": "Aluminium lat 39mm antraciet",
      "category": "slat",
      "quantity": 46,
      "unit": "pcs",
      "cutLengthMm": 2470,
      "workstation": "cutting"
    }
  ],
  "productionNotes": [
    "Windvaste geleiders — verstevigde beugels gebruiken",
    "Motorzijde: LINKS",
    "Kleur: RAL7016 — enkelzijdig gelakt"
  ],
  "timestamps": {
    "configuredAt": "2026-02-28T14:30:00Z",
    "exportedAt": "2026-02-28T14:35:00Z",
    "requestedDelivery": "2026-03-15T00:00:00Z"
  }
}
```

---

## 14. PoC Milestones

### Phase 1: Infrastructure (Day 1-2)

- [ ] Docker Compose with PostgreSQL, GoRules Editor, GoRules Agent
- [ ] ASP.NET Core project scaffolded with EF Core
- [ ] Full database schema with migrations
- [ ] Seed data: roller_shutter family with 2 variants (standard + insulated)
- [ ] Seed reference data: 3 materials, 4 profiles, 5 motors, 4 box types, 20 colors
- [ ] Seed parts catalog with SKU mappings
- [ ] `RulesEngineClient` with Polly resilience policies

### Phase 2: Shared Rule Building Blocks (Day 3-4)

- [ ] `shared/dimension-validation.json` — generic dimension checker using spec data
- [ ] `shared/weight-calculation.json` — weight from profile + dimensions
- [ ] `shared/motor-torque-calculation.json` — torque requirement calculation
- [ ] `shared/motor-sizing.json` — orchestrates weight → torque → motor filter
- [ ] `shared/box-size-selection.json` — roll diameter → box selection
- [ ] `shared/drive-configuration.json` — manual/motor sub-config with Switch
- [ ] Test each shared graph individually via Agent REST API

### Phase 3: Product Family Rules (Day 5-6)

- [ ] `families/roller-shutter/validate.json` — composes shared blocks + Switch by variant
- [ ] `families/roller-shutter/options.json` — dynamic option filtering
- [ ] `families/roller-shutter/bom.json` — BOM generation with Function nodes
- [ ] Test complete flow: partial config → validate → filter → complete → BOM

### Phase 4: API & Orchestration (Day 7-8)

- [ ] `ProductSpecRepository` + `SpecContextBuilder`
- [ ] `ConfigurationService` with enrichment pattern
- [ ] All controllers + endpoints
- [ ] Integration tests: C# → Agent → rule evaluation → response
- [ ] Verify adding insulated variant requires no code changes

### Phase 5: Angular Frontend (Day 9-10)

- [ ] Angular 18 project scaffolded with routing + HTTP client
- [ ] Product selector page (family → type picker)
- [ ] ConfiguratorStore (signal-based state management)
- [ ] Dynamic wizard step renderer from API parameter definition
- [ ] Parameter field components (dimension, select, multi-select)
- [ ] Real-time validation panel (errors + warnings)
- [ ] Option filtering (disabled options with reason)
- [ ] BOM viewer component
- [ ] MES export trigger

### Phase 6: Scale Validation (Day 11-12)

- [ ] Add second product family (e.g., `screen`) using existing shared blocks
- [ ] Verify shared rules work for both families without changes
- [ ] Performance test: 50 concurrent sessions
- [ ] Document process for technical staff to add new products
- [ ] Rule authoring guide (`/rules/README.md`)

---

## 15. Rule Authoring Guide (for Technical Staff)

### 15.1 GoRules Expression Language

```
-- Comparison (used in decision table cells)
= "ALU"                              // equals
!= "PVC"                             // not equals
> 3000                                // greater than
>= 600, <= 6000                       // range

-- Arithmetic (used in expression nodes)
width * height / 1000000             // area in m²
ceil(height / pitch)                  // number of slats

-- Logical
width > 3000 and material == "PVC"   // AND
motor == null or driveType != "motor" // OR with null

-- Object access
spec.dimensions.ALU.maxWidth          // nested object
spec.dimensions[material].maxWidth    // dynamic key lookup

-- Array
contains(controlTypes, "smart_home")
len(accessories)

-- Reference operator (in table cells)
$                                     // current cell value
```

### 15.2 How to Add a New Material

1. Add record to `materials` table
2. Add records to `profiles` table
3. Add `product_specs` rows for dimension constraints
4. Add `product_options` rows so it appears in UI
5. Add `sku_mappings` for BOM resolution
6. **No rule file changes needed**

### 15.3 How to Add a New Product Variant

1. Add `product_types` record with `variant` code
2. Add `product_parameters` for the variant
3. Add `product_specs` with variant-specific constraints
4. In family's `validate.json`, add Switch branch for new variant
5. Test via GoRules Editor simulator

### 15.4 How to Add a New Product Family

1. Create folder: `families/{new-family}/`
2. Create `validate.json`, `options.json`, `bom.json` — compose from `shared/*`
3. Add DB records: family, types, parameters, options, specs, parts, SKU mappings
4. **No C# code changes needed**

---

## 16. Performance & Resilience

### 16.1 Caching

| Data | Cache | TTL |
|---|---|---|
| Product specs | IMemoryCache | 5 min |
| Product options | IMemoryCache | 5 min |
| Agent health | Circuit breaker | Auto |

Rule evaluation is NOT cached — each call has unique context. Agent keeps compiled rules in memory.

### 16.2 Resilience (Polly)

- Retry: 3 attempts with exponential backoff
- Circuit breaker: open after 5 failures in 30s, stay open 30s
- Timeout: 10s per call

### 16.3 Agent Scaling

- Single Agent handles ~10,000 eval/sec for decision tables
- Function nodes (JS): ~1,000-5,000 eval/sec
- Scale: run multiple Agent replicas behind load balancer (stateless)

---

## 17. Open Questions

1. **MES system API format** — REST, SOAP, file drop, message queue?
2. **Existing rule data** — Can we export current Econ/Navision rules as spreadsheets?
3. **Parts catalog** — Is there an existing SKU/parts database to import?
4. **Authentication** — Entra ID from start or post-PoC?
5. **Product count** — Exact number of families and variants?
6. **GoRules licensing** — Free Editor for PoC. BRMS for production (Azure AD SSO, audit logging)?
7. **Rule versioning** — Git for rule files, or BRMS versioning?
8. **Multi-language** — Dutch only, or also French/English?
9. **Pricing** — Part of CPQ, or separate?

---

## 18. Out of Scope (PoC)

- Multi-tenant / multi-user permissions
- Pricing calculation
- Full order management workflow
- Customer-facing portal
- PDF generation (quotes, order confirmations)
- Legacy data migration from Navision/Econ
- Multi-language support
- Offline/mobile configuration
