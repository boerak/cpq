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
│  │ gorules/editor     │         │ gorules/jdm-editor         │ │
│  │ 127.0.0.1:3000     │         │ Port: 8080                 │ │
│  │ (localhost only)   │         │                            │ │
│  │ Used by: internal  │         │ REST API:                  │ │
│  │ technical staff    │         │ POST /api/projects/        │ │
│  │                    │         │   {project}/evaluate/{key} │ │
│  └────────┬───────────┘         └────────┬───────────────────┘ │
│           │ writes .json                 │ reads .json         │
│           ▼                              ▼                     │
│      ┌──────────────────────────────────────────┐             │
│      │ ./rules (bind mount, Git-versioned)        │             │
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
│  │    2. Validates input against product parameter schema │     │
│  │    3. Loads product spec from DB                      │     │
│  │    4. Merges spec + user selections into context      │     │
│  │    5. Calls GoRules Agent with enriched context       │     │
│  │    6. Returns validation + available options + BOM    │     │
│  │                                                       │     │
│  │  Health: GET /health, GET /health/ready               │     │
│  └──────────────────────────────────────────────────────┘     │
│                                                               │
│  ┌──────────────┐    ┌────────────────────────────────┐       │
│  │ PostgreSQL   │    │ Frontend (Angular 18 + nginx)  │       │
│  │ Port: 5432   │    │ Port: 4200                     │       │
│  │              │    │                                │       │
│  │              │    │ nginx reverse proxy:            │       │
│  │              │    │   /api/* → cpq-api:8080        │       │
│  │              │    │   /*    → index.html (SPA)     │       │
│  └──────────────┘    └────────────────────────────────┘       │
└──────────────────────────────────────────────────────────────┘
```

**Networking note:** The Angular app runs in the user's browser, not inside the Docker network. The browser cannot resolve Docker service names like `cpq-api`. Therefore the frontend container runs nginx as a reverse proxy: browser requests to `/api/*` are proxied to `http://cpq-api:8080` internally. This eliminates CORS issues and keeps all traffic same-origin.

### 2.2 Technology Stack

| Component | Technology | Justification |
|---|---|---|
| Backend API | ASP.NET Core 8 Web API | Dev team preference, LTS |
| Database | PostgreSQL 16 | Product catalog, specs, configurations, orders |
| ORM | Entity Framework Core 8 | Standard .NET ORM, PostgreSQL arrays via Npgsql |
| Rules Engine | GoRules Agent (Docker) | REST-based, language-agnostic |
| Rules Editor | GoRules Editor (Docker) | Free standalone editor for PoC |
| Rules Format | JDM (JSON Decision Model) | Portable, versionable, Git-friendly |
| Frontend | Angular 18 | Dev team choice, TypeScript-native, signals |
| Frontend Server | nginx | Reverse proxy + SPA routing |
| Containerization | Docker Compose | Local dev + PoC deployment |
| Resilience | Polly v8 | Retry, circuit breaker, timeout policies |
| Logging | Serilog | Structured JSON logging with correlation IDs |

### 2.3 Day 0 Validation Tasks

Before writing application code, validate critical assumptions about GoRules:

| # | Validation | Why | How |
|---|---|---|---|
| V1 | Agent API endpoint format | The actual path may be `/api/projects/{project}/evaluate/{key}`, not `/decisions/{path}/evaluate`. Filesystem-mode project slug is undocumented. | Deploy Agent with filesystem provider, `curl` the evaluate endpoint with test payloads |
| V2 | Editor file persistence | The standalone Editor may not write `.json` directly to a filesystem volume. It may only support browser download/upload. | Deploy Editor with shared volume, create a graph, verify `.json` appears on the volume |
| V3 | ZEN dynamic key lookup | The entire data-driven spec pattern depends on `spec.dimensions[material].maxWidth` working in Expression nodes. If bracket-notation variable access doesn't work, all expressions must become Function nodes. | Create a test graph with an Expression node using dynamic key access, evaluate via Agent |
| V4 | Function node timeout | GoRules may enforce a 50ms hard timeout on Function nodes. BOM generation chains 6 functions. | Create a Function node with realistic BOM logic, measure execution time |
| V5 | Sub-decision resolution | Decision nodes must resolve `shared/weight-calculation.json` from the filesystem. Key format (with/without `.json`, path separator) is undocumented. | Create a parent graph that calls a sub-graph via Decision node, evaluate via Agent |

**If V1 fails:** Update `RulesEngineClient` endpoint path accordingly.
**If V2 fails:** Use BRMS instead of standalone Editor, or use Editor for authoring + manual file export.
**If V3 fails:** Move all dynamic-key logic to Function nodes (JavaScript). Expression nodes limited to static field access.
**If V4 fails:** Consolidate multiple Function nodes into fewer, leaner functions. Move simple logic to Expression nodes.
**If V5 fails:** Flatten all rules into single graphs (loses composability) or use the `zen` library inside Function nodes for dynamic sub-decision calls.

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
| **Switch Node** | Branching by product variant or configuration path | Each branch uses a ZEN expression condition, e.g. `userSelections.variant == "insulated"` |
| **Decision Node** | Calls a sub-decision (another JDM file) — the key to composition | `validate.json` calls `shared/motor-torque-calculation.json` |
| **Input/Output** | Entry and exit of every graph (Request/Response nodes in JDM) | Context in → result out |

**Switch node clarification:** GoRules Switch nodes use arbitrary ZEN expression conditions on each branch (like if/else chains), NOT field-value matching (like a traditional switch/case). Each branch defines a full expression. Use "First Hit" hit policy to take the first matching branch, or "Collect" to merge multiple branches.

**Decision Table hit policies:** Tables support "First Hit" (return first matching row) and "Collect" (return all matching rows merged). Choose appropriate policy per use case — validation tables typically use Collect (accumulate all errors), option filtering uses First Hit.

**Function node timeout:** GoRules enforces a timeout on Function nodes (historically 50ms, may be higher in newer versions). Keep each Function node lean. Use the built-in `dayjs`, `big.js`, and `zod` libraries. The `zen` library enables calling sub-decisions from within JS: `await zen.evaluate(decisionId, input)`.

**Data flow:** Nodes use pass-through mode by default — they carry forward all incoming data plus their own outputs. When multiple nodes connect to a single node, outputs are merged (later connections overwrite same-named fields). Use `outputPath` to namespace each node's output and avoid conflicts.

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

**Key risk:** The exact key format for Decision node references (e.g., `shared/weight-calculation` vs `shared/weight-calculation.json`) must be validated during Day 0 (V5).

---

## 4. Data-Driven Product Specifications

### 4.1 Core Insight

Instead of encoding "PVC max width is 3000mm" as a hardcoded row in a decision table, we store that constraint as **data** in the database and inject it into the rule context at runtime. The rule becomes generic: "if `width > spec.maxWidth` then error".

This means:
- Adding a new material = add rows to `product_specs`, no rule changes
- Changing a dimension limit = update a database value, no rule changes
- Adding a new product variant = add a product spec record + possibly a Switch branch

**Key risk:** This pattern requires dynamic key lookup in ZEN expressions (e.g., `spec.dimensions[material].maxWidth`). If Day 0 validation (V3) reveals this syntax is unsupported, all dynamic lookups must move to Function nodes, or the spec context must be flattened (e.g., `spec.dimensions_PVC_maxWidth`).

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

CREATE INDEX ix_product_types_family ON product_types(family_id) WHERE is_active = true;

-- ═══════════════════════════════════════════════════════════
-- PRODUCT SPECIFICATIONS (the data that drives generic rules)
-- ═══════════════════════════════════════════════════════════

-- Configuration parameters defined per product type
-- These define WHAT can be configured (the wizard steps)
CREATE TABLE product_parameters (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    product_type_id UUID NOT NULL REFERENCES product_types(id) ON DELETE CASCADE,
    code VARCHAR(100) NOT NULL,                      -- "width", "material", "motor_brand"
    name VARCHAR(200) NOT NULL,                      -- "Breedte", "Materiaal"
    data_type VARCHAR(50) NOT NULL,                  -- "integer", "decimal", "select", "multi_select", "boolean"
    unit VARCHAR(20),                                -- "mm", "kg", null
    step_number INT NOT NULL,                        -- wizard step this belongs to
    step_name VARCHAR(200),                          -- "Afmetingen", "Materiaal & Profiel"
    display_order INT DEFAULT 0,
    is_required BOOLEAN DEFAULT true,
    is_active BOOLEAN DEFAULT true,
    default_value VARCHAR(200),                      -- default selection
    depends_on VARCHAR(100)[],                       -- parameters that must be set first
    metadata JSONB,                                  -- UI hints: min, max, placeholder, help text, visibleWhen
    created_at TIMESTAMPTZ DEFAULT now(),
    updated_at TIMESTAMPTZ DEFAULT now(),
    UNIQUE(product_type_id, code),
    CONSTRAINT chk_product_parameters_data_type
        CHECK (data_type IN ('integer', 'decimal', 'select', 'multi_select', 'boolean', 'text')),
    CONSTRAINT chk_product_parameters_step_positive CHECK (step_number > 0)
);

CREATE INDEX ix_product_parameters_type_step
    ON product_parameters(product_type_id, step_number, display_order);

-- Available options per parameter (for select/multi_select types)
CREATE TABLE product_options (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    product_type_id UUID NOT NULL REFERENCES product_types(id) ON DELETE CASCADE,
    parameter_code VARCHAR(100) NOT NULL,             -- "material", "color", "motor_brand"
    code VARCHAR(100) NOT NULL,                       -- "ALU", "RAL7016", "somfy"
    display_name VARCHAR(200) NOT NULL,               -- "Aluminium", "Antraciet", "Somfy"
    display_order INT DEFAULT 0,
    is_active BOOLEAN DEFAULT true,
    metadata JSONB,                                   -- color hex, image URL, description
    created_at TIMESTAMPTZ DEFAULT now(),
    updated_at TIMESTAMPTZ DEFAULT now(),
    UNIQUE(product_type_id, parameter_code, code),
    -- Ensure parameter_code references an existing parameter for this product type
    FOREIGN KEY (product_type_id, parameter_code)
        REFERENCES product_parameters(product_type_id, code) ON DELETE CASCADE
);

CREATE INDEX ix_product_options_type_param
    ON product_options(product_type_id, parameter_code) WHERE is_active = true;

-- Product specifications — the constraints and properties
-- This is the KEY TABLE that drives the generic rules (EAV pattern)
-- The EAV pattern is intentional: different product families have completely
-- different spec shapes. Validation of spec values happens in the C# admin layer.
CREATE TABLE product_specs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    product_type_id UUID NOT NULL REFERENCES product_types(id) ON DELETE CASCADE,
    spec_group VARCHAR(100) NOT NULL,                 -- "dimensions", "materials", "motors", "profiles"
    spec_key VARCHAR(200) NOT NULL,                   -- "ALU.max_width", "PVC.min_height"
    spec_value JSONB NOT NULL,                        -- 4500, true, {"torque": 15, "brand": "somfy"}
    description TEXT,                                 -- human-readable explanation
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMPTZ DEFAULT now(),
    updated_at TIMESTAMPTZ DEFAULT now(),
    UNIQUE(product_type_id, spec_group, spec_key)
);

CREATE INDEX ix_product_specs_type_group ON product_specs(product_type_id, spec_group);

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
    properties JSONB,                                 -- thermal conductivity, UV resistance, etc.
    created_at TIMESTAMPTZ DEFAULT now(),
    updated_at TIMESTAMPTZ DEFAULT now()
);

-- Color definitions (shared across products)
CREATE TABLE colors (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    code VARCHAR(50) UNIQUE NOT NULL,                 -- "RAL7016", "RAL9010", "DB703"
    name VARCHAR(200) NOT NULL,                       -- "Antraciet", "Zuiver wit"
    color_system VARCHAR(50) NOT NULL,                -- 'RAL', 'DB', 'NCS', 'custom'
    hex_value VARCHAR(7),                             -- "#383E42"
    is_standard BOOLEAN DEFAULT true,                 -- vs. custom/surcharge colors
    properties JSONB,                                 -- gloss level, texture, sides
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMPTZ DEFAULT now(),
    updated_at TIMESTAMPTZ DEFAULT now()
);

-- Which colors are available for which materials
CREATE TABLE material_colors (
    material_code VARCHAR(50) NOT NULL REFERENCES materials(code),
    color_code VARCHAR(50) NOT NULL REFERENCES colors(code),
    is_active BOOLEAN DEFAULT true,
    PRIMARY KEY (material_code, color_code)
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
    is_active BOOLEAN DEFAULT true,
    properties JSONB,                                 -- insulation value, wind class, etc.
    created_at TIMESTAMPTZ DEFAULT now(),
    updated_at TIMESTAMPTZ DEFAULT now(),
    CONSTRAINT chk_profiles_height_positive CHECK (height_mm > 0),
    CONSTRAINT chk_profiles_thickness_positive CHECK (thickness_mm > 0),
    CONSTRAINT chk_profiles_weight_positive CHECK (weight_per_meter_kg > 0),
    CONSTRAINT chk_profiles_max_width_positive CHECK (max_width_mm > 0),
    CONSTRAINT chk_profiles_min_lte_max CHECK (min_width_mm <= max_width_mm)
);

CREATE INDEX ix_profiles_material ON profiles(material_code);

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
    is_active BOOLEAN DEFAULT true,
    properties JSONB,                                 -- power consumption, IP rating, etc.
    created_at TIMESTAMPTZ DEFAULT now(),
    updated_at TIMESTAMPTZ DEFAULT now(),
    CONSTRAINT chk_motors_torque_positive CHECK (torque_nm > 0)
);

-- Guide rail specifications
CREATE TABLE guide_rails (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    code VARCHAR(50) UNIQUE NOT NULL,                 -- "ALU-STANDARD-20", "ALU-WIND-25"
    name VARCHAR(200) NOT NULL,
    type VARCHAR(50) NOT NULL,                        -- 'standard', 'wind_resistant', 'zip'
    material_code VARCHAR(50) NOT NULL REFERENCES materials(code),
    width_mm DECIMAL(10,2) NOT NULL,
    depth_mm DECIMAL(10,2) NOT NULL,
    max_height_mm INT NOT NULL,
    weight_per_meter_kg DECIMAL(10,4) NOT NULL,
    bracket_spacing_mm INT NOT NULL DEFAULT 600,      -- how often brackets are needed
    compatible_profiles VARCHAR(50)[],                -- which slat profiles fit
    wind_class INT,                                   -- for wind-resistant types
    is_active BOOLEAN DEFAULT true,
    properties JSONB,
    created_at TIMESTAMPTZ DEFAULT now(),
    updated_at TIMESTAMPTZ DEFAULT now()
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
    is_active BOOLEAN DEFAULT true,
    properties JSONB,
    created_at TIMESTAMPTZ DEFAULT now(),
    updated_at TIMESTAMPTZ DEFAULT now(),
    CONSTRAINT chk_boxes_inner_positive CHECK (inner_diameter_mm > 0),
    CONSTRAINT chk_boxes_outer_positive CHECK (outer_height_mm > 0)
);

-- Accessory definitions
CREATE TABLE accessories (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    code VARCHAR(50) UNIQUE NOT NULL,                 -- "INSECT_SCREEN", "WIND_SENSOR", "TIMER"
    name VARCHAR(200) NOT NULL,
    category VARCHAR(100) NOT NULL,                   -- 'insect_screen', 'sensor', 'remote', 'cosmetic'
    requires_motor BOOLEAN DEFAULT false,             -- only available with motorized drive
    compatible_families VARCHAR(50)[],                -- which product families support this
    incompatible_with VARCHAR(50)[],                  -- mutually exclusive accessory codes
    prerequisite_accessories VARCHAR(50)[],           -- must also select these
    is_active BOOLEAN DEFAULT true,
    properties JSONB,
    created_at TIMESTAMPTZ DEFAULT now(),
    updated_at TIMESTAMPTZ DEFAULT now()
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
    cost_price DECIMAL(12,4),
    supplier_code VARCHAR(100),
    properties JSONB,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMPTZ DEFAULT now(),
    updated_at TIMESTAMPTZ DEFAULT now(),
    CONSTRAINT chk_parts_weight_non_negative CHECK (weight_kg IS NULL OR weight_kg >= 0),
    CONSTRAINT chk_parts_cost_non_negative CHECK (cost_price IS NULL OR cost_price >= 0)
);

-- SKU resolution rules: maps configuration choices to actual SKUs
-- This avoids encoding SKU logic in GoRules
CREATE TABLE sku_mappings (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    product_family_code VARCHAR(50) NOT NULL REFERENCES product_families(code),
    category VARCHAR(100) NOT NULL,                   -- "slat", "guide", "box", "motor"
    match_criteria JSONB NOT NULL,                    -- {"material": "ALU", "profile": "39", "color": "RAL7016"}
    sku VARCHAR(100) NOT NULL REFERENCES parts(sku),
    priority INT DEFAULT 0,                           -- higher = more specific match wins
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMPTZ DEFAULT now(),
    updated_at TIMESTAMPTZ DEFAULT now(),
    CONSTRAINT chk_sku_mappings_priority_non_negative CHECK (priority >= 0)
);

CREATE INDEX ix_sku_mappings_family_category
    ON sku_mappings(product_family_code, category) WHERE is_active = true;
CREATE INDEX ix_sku_mappings_criteria ON sku_mappings USING GIN (match_criteria);

-- ═══════════════════════════════════════════════════════════
-- CONFIGURATIONS & ORDERS
-- ═══════════════════════════════════════════════════════════

CREATE TABLE configurations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    product_type_id UUID NOT NULL REFERENCES product_types(id),
    reference VARCHAR(255),                           -- customer order reference
    status VARCHAR(50) DEFAULT 'draft',
    config_data JSONB NOT NULL DEFAULT '{}',          -- user selections
    validation_result JSONB,                          -- last validation from rules engine
    bom_data JSONB,                                   -- last generated BOM
    version INT NOT NULL DEFAULT 1,                   -- optimistic concurrency token
    created_at TIMESTAMPTZ DEFAULT now(),
    updated_at TIMESTAMPTZ DEFAULT now(),
    created_by VARCHAR(200),
    CONSTRAINT chk_configurations_status
        CHECK (status IN ('draft', 'validated', 'ordered', 'sent_to_mes', 'cancelled'))
);

CREATE INDEX ix_configurations_status ON configurations(status);
CREATE INDEX ix_configurations_product_type ON configurations(product_type_id);
CREATE INDEX ix_configurations_created_by ON configurations(created_by)
    WHERE created_by IS NOT NULL;

-- Configuration change history for audit trail
CREATE TABLE configuration_history (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    configuration_id UUID NOT NULL REFERENCES configurations(id) ON DELETE CASCADE,
    action VARCHAR(50) NOT NULL,                      -- 'created', 'selection_updated', 'validated',
                                                      -- 'finalized', 'bom_generated', 'exported'
    selections_snapshot JSONB,                        -- config_data at this point
    validation_snapshot JSONB,                        -- validation_result at this point
    changed_fields VARCHAR(100)[],                    -- which fields changed in this action
    performed_by VARCHAR(200),
    performed_at TIMESTAMPTZ DEFAULT now()
);

CREATE INDEX ix_config_history_config
    ON configuration_history(configuration_id, performed_at);

CREATE TABLE bom_lines (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    configuration_id UUID NOT NULL REFERENCES configurations(id) ON DELETE CASCADE,
    part_sku VARCHAR(100) NOT NULL REFERENCES parts(sku),
    part_name VARCHAR(300),
    category VARCHAR(100),
    quantity DECIMAL(10,2) NOT NULL,
    unit VARCHAR(20) NOT NULL,
    cut_length_mm INT,
    sort_order INT DEFAULT 0,
    notes TEXT,
    CONSTRAINT chk_bom_lines_quantity_positive CHECK (quantity > 0),
    CONSTRAINT chk_bom_lines_cut_length_positive CHECK (cut_length_mm IS NULL OR cut_length_mm > 0)
);

CREATE INDEX ix_bom_lines_configuration ON bom_lines(configuration_id);

CREATE TABLE mes_exports (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    configuration_id UUID NOT NULL REFERENCES configurations(id),
    -- ON DELETE RESTRICT: don't delete configs that have been exported to MES
    payload JSONB NOT NULL,
    status VARCHAR(50) DEFAULT 'pending',
    sent_at TIMESTAMPTZ,
    response JSONB,
    error_message TEXT,
    created_at TIMESTAMPTZ DEFAULT now(),
    CONSTRAINT chk_mes_exports_status
        CHECK (status IN ('pending', 'sent', 'acknowledged', 'failed'))
);

CREATE INDEX ix_mes_exports_configuration ON mes_exports(configuration_id);
CREATE INDEX ix_mes_exports_status ON mes_exports(status);

-- ═══════════════════════════════════════════════════════════
-- AUTO-UPDATE TRIGGER FOR updated_at
-- ═══════════════════════════════════════════════════════════

CREATE OR REPLACE FUNCTION update_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = now();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Apply to all tables with updated_at:
CREATE TRIGGER trg_product_families_updated BEFORE UPDATE ON product_families FOR EACH ROW EXECUTE FUNCTION update_updated_at();
CREATE TRIGGER trg_product_types_updated BEFORE UPDATE ON product_types FOR EACH ROW EXECUTE FUNCTION update_updated_at();
CREATE TRIGGER trg_product_parameters_updated BEFORE UPDATE ON product_parameters FOR EACH ROW EXECUTE FUNCTION update_updated_at();
CREATE TRIGGER trg_product_options_updated BEFORE UPDATE ON product_options FOR EACH ROW EXECUTE FUNCTION update_updated_at();
CREATE TRIGGER trg_product_specs_updated BEFORE UPDATE ON product_specs FOR EACH ROW EXECUTE FUNCTION update_updated_at();
CREATE TRIGGER trg_materials_updated BEFORE UPDATE ON materials FOR EACH ROW EXECUTE FUNCTION update_updated_at();
CREATE TRIGGER trg_colors_updated BEFORE UPDATE ON colors FOR EACH ROW EXECUTE FUNCTION update_updated_at();
CREATE TRIGGER trg_profiles_updated BEFORE UPDATE ON profiles FOR EACH ROW EXECUTE FUNCTION update_updated_at();
CREATE TRIGGER trg_motors_updated BEFORE UPDATE ON motors FOR EACH ROW EXECUTE FUNCTION update_updated_at();
CREATE TRIGGER trg_guide_rails_updated BEFORE UPDATE ON guide_rails FOR EACH ROW EXECUTE FUNCTION update_updated_at();
CREATE TRIGGER trg_boxes_updated BEFORE UPDATE ON boxes FOR EACH ROW EXECUTE FUNCTION update_updated_at();
CREATE TRIGGER trg_accessories_updated BEFORE UPDATE ON accessories FOR EACH ROW EXECUTE FUNCTION update_updated_at();
CREATE TRIGGER trg_parts_updated BEFORE UPDATE ON parts FOR EACH ROW EXECUTE FUNCTION update_updated_at();
CREATE TRIGGER trg_sku_mappings_updated BEFORE UPDATE ON sku_mappings FOR EACH ROW EXECUTE FUNCTION update_updated_at();
CREATE TRIGGER trg_configurations_updated BEFORE UPDATE ON configurations FOR EACH ROW EXECUTE FUNCTION update_updated_at();
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
          Switch (conditions per branch):
          ├── userSelections.variant == "standard"
          │   ──► Decision Table: standard-specific-rules
          │       (basic material/motor constraints)
          │
          ├── userSelections.variant == "insulated"
          │   ──► Decision Table: insulated-specific-rules
          │       (only ALU_INSULATED, extra U-value check)
          │   ──► Expression: thermal-calculations
          │
          ├── userSelections.variant == "security"
          │   ──► Decision Table: security-specific-rules
          │       (min thickness, anti-lift check, certified lock)
          │   ──► Decision: shared/security-certification.json
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
    Switch (conditions per branch):
    │
    ├── userSelections.driveType == "manual_strap"
    │   ──► Decision Table: strap-options
    │       (strap width by shutter weight, side selection)
    │
    ├── userSelections.driveType == "manual_crank"
    │   ──► Decision Table: crank-options
    │       (crank type, gear ratio by weight)
    │
    ├── userSelections.driveType == "motor"
    │   ──► Decision: shared/motor-sizing.json
    │   ──► Decision Table: motor-control-options
    │       (control types by motor brand)
    │   ──► Decision Table: motor-accessories
    │       (receiver, remote, smart home bridge)
    │
    └── (default) ──► Expression: { error: "Invalid drive type" }
    │
    ──► Output
```

### 5.4 Pattern: BOM Assembly with Function Nodes

BOM generation is the most complex logic — it requires iterative calculations, SKU resolution, and conditional part inclusion. This is where **Function nodes** (JavaScript) shine because the logic is too algorithmic for pure decision tables.

**Timeout risk:** Each Function node has a timeout limit. Keep each function focused and lean. If a function approaches the timeout, split it or move simple calculations to Expression nodes.

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

The option filtering graph returns which options are still valid given current selections. It must handle partial configurations (not all fields filled yet). The response includes a `resetFields` array to signal which dependent fields the frontend should clear.

```javascript
// Function node in families/roller-shutter/options.json
export const handler = async (input) => {
  const { userSelections: s, spec, allOptions } = input;
  const result = {};
  const resetFields = [];

  // Materials: filter by dimension constraints
  if (s.width || s.height) {
    result.material = allOptions.material.map(opt => ({
      code: opt.code,
      available: (!s.width || s.width <= spec.dimensions[opt.code]?.maxWidth)
              && (!s.height || s.height <= spec.dimensions[opt.code]?.maxHeight),
      reason: s.width > spec.dimensions[opt.code]?.maxWidth
              ? `Max breedte ${spec.dimensions[opt.code].maxWidth}mm` : null
    }));

    // If current material selection is now unavailable, signal reset
    if (s.material && result.material.find(m => m.code === s.material && !m.available)) {
      resetFields.push('material', 'profile', 'color', 'boxType');
    }
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

  return { availableOptions: result, resetFields };
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
    private readonly IConfigurationRepository _configs;
    private readonly IProductTypeRepository _productTypes;
    private readonly ISkuMappingRepository _skuMappings;
    private readonly IBomService _bomService;
    private readonly ILogger<ConfigurationService> _logger;

    public async Task<ConfigurationResponse> UpdateConfiguration(
        Guid configId, UpdateConfigurationRequest request, CancellationToken ct)
    {
        // 1. Load current configuration with concurrency check
        var config = await _configs.GetAsync(configId, ct)
            ?? throw new EntityNotFoundException($"Configuration {configId} not found");

        if (config.Version != request.ExpectedVersion)
            throw new ConcurrencyConflictException(
                $"Configuration was modified. Expected version {request.ExpectedVersion}, "
                + $"actual {config.Version}. Reload and retry.");

        var productType = await _productTypes.GetWithFamilyAsync(config.ProductTypeId, ct);

        // 2. Merge new selections into existing (null values clear the field)
        config.MergeSelections(request.Selections);

        // 3. Load product specifications from DB (cached, parallelized)
        var spec = await _specs.GetSpecContextAsync(config.ProductTypeId, ct);

        // 4. Load all available options for this product type (cached)
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

        // 7. Call validation + option filtering in parallel
        var validateTask = _rules.EvaluateAsync<ValidationResult>(
            $"{rulePrefix}/validate", context, ct);
        var optionsTask = _rules.EvaluateAsync<AvailableOptionsResult>(
            $"{rulePrefix}/options", context, ct);

        await Task.WhenAll(validateTask, optionsTask);

        var validationResult = validateTask.Result;
        var optionsResult = optionsTask.Result;

        // 8. Apply server-side field resets from options engine
        if (optionsResult.ResetFields?.Count > 0)
        {
            foreach (var field in optionsResult.ResetFields)
                config.ClearSelection(field);
        }

        // 9. Save with version increment and return
        config.ValidationResult = validationResult;
        config.Version++;
        await _configs.SaveAsync(config, ct);

        // 10. Record history
        await _configs.RecordHistoryAsync(config, "selection_updated",
            request.Selections.Keys.ToArray(), ct);

        return new ConfigurationResponse
        {
            Id = config.Id,
            Config = config.ConfigData,
            Validation = validationResult,
            AvailableOptions = optionsResult.AvailableOptions,
            ResetFields = optionsResult.ResetFields,
            IsComplete = IsConfigurationComplete(config, productType),
            CanFinalize = validationResult.Valid && IsConfigurationComplete(config, productType),
            Version = config.Version
        };
    }

    public async Task<BomResponse> GenerateBom(Guid configId, CancellationToken ct)
    {
        var config = await _configs.GetAsync(configId, ct)
            ?? throw new EntityNotFoundException($"Configuration {configId} not found");
        var productType = await _productTypes.GetWithFamilyAsync(config.ProductTypeId, ct);

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

**MergeSelections semantics:**

```csharp
// In Configuration entity:
public void MergeSelections(Dictionary<string, JsonElement> incoming)
{
    foreach (var (key, value) in incoming)
    {
        if (value.ValueKind == JsonValueKind.Null)
            ConfigData.Remove(key);  // explicit null = remove/reset field
        else
            ConfigData[key] = value; // upsert
    }
}

public void ClearSelection(string key)
{
    ConfigData.Remove(key);
}
```

### 6.2 RulesEngineClient

**Important:** The actual Agent API endpoint must be validated during Day 0 (V1). The path shown below (`/api/projects/{project}/evaluate/{key}`) is based on the latest documentation but the `{project}` value in filesystem mode needs empirical testing.

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
    private readonly RulesEngineOptions _options;

    public RulesEngineClient(HttpClient http, ILogger<RulesEngineClient> logger,
                              IOptions<RulesEngineOptions> options)
    {
        _http = http;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<T> EvaluateAsync<T>(string decisionPath, object context,
                                           CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            // NOTE: Actual endpoint path TBD after Day 0 validation (V1).
            // Filesystem mode may use a different project slug or path format.
            var url = $"/api/projects/{_options.ProjectSlug}/evaluate/{decisionPath}";
            var payload = new { context };
            var response = await _http.PostAsJsonAsync(url, payload, ct);

            response.EnsureSuccessStatusCode();

            // GoRules Agent returns the graph output directly (no envelope wrapper).
            // If it wraps in { result, trace, details }, adjust deserialization here.
            var result = await response.Content.ReadFromJsonAsync<T>(ct)
                ?? throw new RulesEngineException($"Empty response from {decisionPath}");

            _logger.LogInformation(
                "Rule {Decision} evaluated in {Elapsed}ms",
                decisionPath, stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Rules engine HTTP error for {Decision}", decisionPath);
            throw new RulesEngineException($"Failed to evaluate {decisionPath}", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Rules engine deserialization error for {Decision}", decisionPath);
            throw new RulesEngineException($"Invalid response from {decisionPath}", ex);
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            _logger.LogError(ex, "Rules engine timeout for {Decision}", decisionPath);
            throw new RulesEngineException($"Timeout evaluating {decisionPath}", ex);
        }
    }
}

// Options:
public class RulesEngineOptions
{
    public const string SectionName = "RulesEngine";
    public string BaseUrl { get; set; } = "http://localhost:8080";
    public string ProjectSlug { get; set; } = "default"; // TBD after V1 validation
    public int TimeoutSeconds { get; set; } = 10;
    public int RetryCount { get; set; } = 3;
    public int CircuitBreakerThreshold { get; set; } = 5;
    public int CircuitBreakerDurationSeconds { get; set; } = 30;
}

// Polly policies:
public static class PollyPolicies
{
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int retryCount)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()  // 5xx + 408 only — do NOT retry 4xx
            .OrResult(r => r.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                retryCount,
                attempt => TimeSpan.FromMilliseconds(200 * Math.Pow(2, attempt)));
    }

    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreaker(
        int threshold, int durationSeconds)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(threshold, TimeSpan.FromSeconds(durationSeconds));
    }
}

// Registration in Program.cs (order matters: circuit breaker outer, retry inner):
builder.Services.Configure<RulesEngineOptions>(
    builder.Configuration.GetSection(RulesEngineOptions.SectionName));

builder.Services.AddHttpClient<IRulesEngineClient, RulesEngineClient>(client =>
{
    var opts = builder.Configuration.GetSection(RulesEngineOptions.SectionName)
        .Get<RulesEngineOptions>()!;
    client.BaseAddress = new Uri(opts.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(opts.TimeoutSeconds);
})
.AddPolicyHandler((sp, _) =>
{
    var opts = sp.GetRequiredService<IOptions<RulesEngineOptions>>().Value;
    return PollyPolicies.GetCircuitBreaker(opts.CircuitBreakerThreshold,
        opts.CircuitBreakerDurationSeconds);
})
.AddPolicyHandler((sp, _) =>
{
    var opts = sp.GetRequiredService<IOptions<RulesEngineOptions>>().Value;
    return PollyPolicies.GetRetryPolicy(opts.RetryCount);
});
```

### 6.3 Spec Context Builder

Transforms normalized database rows into the nested JSON structure that rules expect. Queries are parallelized and reference data is cached.

```csharp
public class ProductSpecRepository : IProductSpecRepository
{
    private readonly CpqDbContext _db;
    private readonly IMemoryCache _cache;

    public async Task<object> GetSpecContextAsync(Guid productTypeId, CancellationToken ct)
    {
        // Product-specific specs (not cached — vary per product type)
        var specsTask = _db.ProductSpecs
            .Where(s => s.ProductTypeId == productTypeId && s.IsActive)
            .ToListAsync(ct);

        // Global reference data (cached 5 min — same for all product types)
        var profilesTask = GetCachedAsync("profiles:active",
            () => _db.Profiles.Where(p => p.IsActive).ToListAsync(ct));
        var motorsTask = GetCachedAsync("motors:active",
            () => _db.Motors.Where(m => m.IsActive).ToListAsync(ct));
        var boxesTask = GetCachedAsync("boxes:active",
            () => _db.Boxes.Where(b => b.IsActive).ToListAsync(ct));
        var guideRailsTask = GetCachedAsync("guide_rails:active",
            () => _db.GuideRails.Where(g => g.IsActive).ToListAsync(ct));

        await Task.WhenAll(specsTask, profilesTask, motorsTask, boxesTask, guideRailsTask);

        return new
        {
            dimensions = specsTask.Result
                .Where(s => s.SpecGroup == "dimensions")
                .ToDictionary(s => s.SpecKey, s => s.SpecValue),

            profiles = profilesTask.Result.ToDictionary(p => p.Code, p => new
            {
                heightMm = p.HeightMm,
                thicknessMm = p.ThicknessMm,
                weightPerMeter = p.WeightPerMeterKg,
                maxWidth = p.MaxWidthMm,
                minWidth = p.MinWidthMm,
                pitch = p.HeightMm + 0.5m
            }),

            motors = motorsTask.Result.ToDictionary(m => m.Code, m => new
            {
                torque = m.TorqueNm,
                maxWeight = m.MaxWeightKg,
                maxSurface = m.MaxSurfaceM2,
                controlTypes = m.ControlTypes,
                brand = m.Brand
            }),

            boxes = boxesTask.Result.ToDictionary(b => b.Code, b => new
            {
                type = b.Type,
                innerDiameter = b.InnerDiameterMm,
                outerHeight = b.OuterHeightMm,
                compatibleMaterials = b.CompatibleMaterials,
                maxWidth = b.MaxWidthMm
            }),

            guideRails = guideRailsTask.Result.ToDictionary(g => g.Code, g => new
            {
                type = g.Type,
                maxHeight = g.MaxHeightMm,
                weightPerMeter = g.WeightPerMeterKg,
                bracketSpacing = g.BracketSpacingMm,
                windClass = g.WindClass
            })
        };
    }

    private async Task<T> GetCachedAsync<T>(string key, Func<Task<T>> factory)
    {
        return await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return await factory();
        }) ?? await factory();
    }
}
```

### 6.4 Exception Handling Middleware

```csharp
// In Program.cs middleware pipeline:
app.UseExceptionHandler(appBuilder =>
{
    appBuilder.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        var problem = exception switch
        {
            EntityNotFoundException ex => new ProblemDetails
                { Status = 404, Title = "Not found", Detail = ex.Message },
            ConcurrencyConflictException ex => new ProblemDetails
                { Status = 409, Title = "Conflict", Detail = ex.Message },
            RulesEngineException ex => new ProblemDetails
                { Status = 502, Title = "Rules engine unavailable",
                  Detail = "Configuration engine is temporarily unavailable. Your selections have been saved." },
            _ => new ProblemDetails
                { Status = 500, Title = "Internal error" }
        };

        context.Response.StatusCode = problem.Status ?? 500;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problem);
    });
});
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
├── .git/                                       # Git repo for rule versioning
└── README.md                                   # Rule authoring guide for technical staff
```

**Rule file versioning:** The `/rules/` directory is a Git repository. Every rule change is committed with a message describing the change. The Git commit SHA is logged with each configuration evaluation for traceability. In production, consider GoRules BRMS for built-in versioning and audit logging.

### 7.2 Adding a New Product Family

1. Create a new folder under `families/` (e.g., `families/pergola/`)
2. Create `validate.json`, `options.json`, `bom.json` — compose from `shared/*` building blocks
3. Add product family record to `product_families` table with `rule_prefix = "families/pergola"`
4. Add product types, parameters, options, and specs to database
5. **No C# code changes needed**

### 7.3 Adding a New Product Variant Within a Family

1. Add product type record (e.g., `roller_shutter_mini`) with `variant = "mini"`
2. Add product specs for the new variant (dimensions, constraints)
3. In the family's `validate.json`, add a new branch in the Switch node for "mini" (condition: `userSelections.variant == "mini"`)
4. **No C# code changes needed**

### 7.4 Adding a New Constraint to an Existing Product

1. Update `product_specs` rows in the database
2. If it's a new type of constraint (not just changing a value), add a node in the relevant rule graph
3. Test via GoRules Editor simulator
4. **No C# code changes needed**

---

## 8. API Endpoints

All endpoints use the `/api/v1/` prefix for versioning.

### 8.1 Product Catalog

```
GET    /api/v1/families                                — list product families
GET    /api/v1/families/{code}/products                — list product types in family
GET    /api/v1/products/{code}                         — get product type with parameters + options
GET    /api/v1/products/{code}/parameters              — get configuration parameters (wizard definition)
```

### 8.2 Configuration (Wizard)

```
POST   /api/v1/configurations
       Body: { "productTypeCode": "roller_shutter_standard" }
       Returns: { id, productType, config: {}, availableOptions, steps[], version }

GET    /api/v1/configurations                          — list configurations (filterable)
       Query: ?status=draft&createdBy=me&page=1&pageSize=20

GET    /api/v1/configurations/{id}

PATCH  /api/v1/configurations/{id}
       Body: { "selections": { "width": 2500, "material": "ALU" }, "expectedVersion": 1 }
       Notes:
         - "expectedVersion" is required for optimistic concurrency.
         - Setting a field to null clears it: { "material": null }
         - Multi-select sends full array: { "accessories": ["insect_screen", "wind_lock"] }
         - Returns 409 Conflict if version mismatch.
       Returns: {
         id, config,
         validation: { valid, errors[], warnings[] },
         availableOptions: { material: [...], motor: [...] },
         resetFields: ["profile", "color"],      // fields cleared by server due to dependency changes
         isComplete, canFinalize, version
       }

GET    /api/v1/configurations/{id}/validate
POST   /api/v1/configurations/{id}/finalize
DELETE /api/v1/configurations/{id}                     — abandon draft configuration
POST   /api/v1/configurations/{id}/clone               — duplicate a configuration
```

### 8.3 BOM

```
POST   /api/v1/configurations/{id}/bom
GET    /api/v1/configurations/{id}/bom
```

### 8.4 MES Export

The MES export is implemented as a pluggable adapter (`IMesExporter`) since the target system's API format is TBD (Open Question #1). For the PoC, a `FileMesExporter` writes JSON to a directory.

```
POST   /api/v1/configurations/{id}/export
GET    /api/v1/exports/{exportId}
```

### 8.5 Health Checks

```
GET    /health                                         — liveness (process is running)
GET    /health/ready                                   — readiness (PostgreSQL + GoRules Agent reachable)
```

### 8.6 Input Validation

All `selections` in PATCH requests are validated server-side against the product type's `product_parameters`:
- Only known parameter codes are accepted; unknown keys are rejected
- Data types are enforced (`integer`, `decimal`, `select` values from known options)
- Min/max from parameter metadata are checked
- String lengths are limited
- Total payload size is capped at 64KB

This prevents malformed or malicious input from reaching the rules engine.

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
1. Validates input against product parameter schema
2. Merges new selections into configuration state (null = clear field)
3. Loads product specs from DB, builds enriched context
4. Calls `{family}/validate` and `{family}/options` via GoRules Agent **in parallel**
5. Applies server-side field resets (from `resetFields` in options response)
6. Returns validation + available options + reset list in single response

The frontend uses this to:
- Show validation errors inline next to relevant fields
- Disable/gray out unavailable options with reason tooltip
- Show warnings as non-blocking info banners
- Conditionally show/hide parameters based on `visibleWhen` (evaluated safely, see 10.5)
- Clear fields listed in `resetFields` and show a toast notification
- Enable "Finalize" button only when `canFinalize = true`

### 9.3 Debounce and Request Management

- **Numeric inputs** (width, height): debounced 400ms — typing "2500" sends one PATCH, not four
- **Select/toggle inputs**: immediate — single-action, no debounce needed
- **Request cancellation**: `switchMap` pattern — new PATCH cancels any in-flight previous PATCH
- **Sequence tracking**: each PATCH carries an incrementing sequence number; responses with a lower sequence than the latest sent are discarded (prevents stale response overwrites)

---

## 10. Angular Frontend

### 10.1 Technology Choices

| Concern | Choice |
|---|---|
| Framework | Angular 18 (standalone components, signals) |
| State | Signal-based store per feature (no NgRx for PoC) |
| HTTP | Built-in HttpClient with interceptors |
| Forms | Signal-based bindings (single source of truth: ConfiguratorStore) |
| Styling | Angular Material + SCSS (good a11y defaults) |
| Routing | Angular Router with lazy-loaded feature modules |
| Expression parsing | `jsep` (safe AST-based parser for `visibleWhen`) |

### 10.2 Key Components

**ConfiguratorComponent** — The main wizard container. Receives the step definition from the API and renders `WizardStepComponent` instances dynamically. Injects `ConfiguratorStore`. Route: `/configure/:configId`.

**WizardStepComponent** — Renders a single wizard step. Iterates over the step's parameters and renders the appropriate field component based on `data_type`. Computes per-step status (`pristine | incomplete | valid | error`) from the validation response.

**ParameterFieldComponent** — Factory component that delegates to the correct sub-component based on `data_type`. Injects `ConfiguratorStore` directly and calls `store.updateField(code, value)` on changes. Handles debounce for numeric inputs.

Sub-components (stateless, communicate via `input()`/`output()`):
- `DimensionFieldComponent` — numeric input with unit label, min/max from metadata
- `SelectFieldComponent` — dropdown with options, disabled items show reason tooltip
- `MultiSelectFieldComponent` — checkbox group with disabled items
- `BooleanFieldComponent` — toggle

**ValidationPanelComponent** — Displays errors (blocking) and warnings (informational) returned from the PATCH response. Uses `aria-live="polite"` for screen reader announcements. Errors link to the relevant field.

**ConfigurationListComponent** — Lists user's configurations with status filtering (draft, validated, ordered). Route: `/configurations`.

### 10.3 Reactive Configuration Flow

The `ConfiguratorStore.selections` signal is the **single source of truth** for field values. Sub-components read from it and write to it. There are no standalone `FormControl` instances that could drift from the store.

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
  readonly connectionError = signal<string | null>(null);
  readonly version = signal<number>(1);

  // Request sequencing: discard stale responses
  private requestSeq = 0;

  // Computed
  readonly currentStepDef = computed(() =>
    this.steps().find(s => s.stepNumber === this.currentStep()));
  readonly canFinalize = computed(() =>
    this.validation()?.valid === true && this.isComplete());
  readonly errors = computed(() =>
    this.validation()?.errors ?? []);
  readonly warnings = computed(() =>
    this.validation()?.warnings ?? []);
  readonly stepStatuses = computed(() =>
    this.steps().map(step => ({
      stepNumber: step.stepNumber,
      status: this.computeStepStatus(step)
    })));

  constructor(private configService: ConfigurationService) {}

  // On every field change: PATCH → update state
  // Numeric inputs are debounced 400ms at the component level before calling this.
  // Select inputs call this immediately.
  async updateField(code: string, value: any): Promise<void> {
    const seq = ++this.requestSeq;
    this.isLoading.set(true);
    this.connectionError.set(null);

    try {
      const response = await firstValueFrom(
        this.configService.updateConfiguration(
          this.configId()!,
          { [code]: value },
          this.version()
        )
      );

      // Discard stale responses
      if (seq < this.requestSeq) return;

      // Server response is always the source of truth
      this.selections.set(response.config);
      this.validation.set(response.validation);
      this.availableOptions.set(response.availableOptions);
      this.version.set(response.version);

      // Show toast if server reset dependent fields
      if (response.resetFields?.length) {
        this.showResetNotification(response.resetFields);
      }
    } catch (error: any) {
      if (seq < this.requestSeq) return;

      if (error.status === 409) {
        // Concurrency conflict — reload configuration
        await this.reloadConfiguration();
      } else if (error.status === 0) {
        this.connectionError.set('Connection lost. Your selections are saved.');
      }
    } finally {
      if (seq === this.requestSeq) {
        this.isLoading.set(false);
      }
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
    <mat-form-field>
      <mat-label>{{ parameter().name }}</mat-label>
      <mat-select
        [value]="currentValue()"
        (selectionChange)="onSelect($event.value)">
        <mat-option value="">-- Kies --</mat-option>
        @for (opt of options(); track opt.code) {
          <mat-option
            [value]="opt.code"
            [disabled]="!opt.available">
            {{ opt.displayName }}
            @if (!opt.available) { — {{ opt.reason }} }
          </mat-option>
        }
      </mat-select>
    </mat-form-field>
  `
})
export class SelectFieldComponent {
  parameter = input.required<ProductParameter>();
  options = input.required<OptionWithAvailability[]>();
  currentValue = input.required<string>();
  valueChange = output<string>();

  onSelect(value: string) {
    this.valueChange.emit(value);
  }
}
```

### 10.5 Safe `visibleWhen` Expression Evaluation

The `visibleWhen` expressions from `product_parameters.metadata` are evaluated using a safe AST-based parser (`jsep`), **never `eval()` or `new Function()`**. Only a whitelist of operations is allowed.

```typescript
// visible-when.evaluator.ts
import jsep from 'jsep';

export function evaluateVisibleWhen(
  expression: string,
  selections: Record<string, any>
): boolean {
  try {
    const ast = jsep(expression);
    return evaluateNode(ast, selections);
  } catch {
    return true; // show field if expression is invalid
  }
}

function evaluateNode(node: jsep.Expression, ctx: Record<string, any>): any {
  switch (node.type) {
    case 'BinaryExpression':
      const left = evaluateNode(node.left, ctx);
      const right = evaluateNode(node.right, ctx);
      switch (node.operator) {
        case '==': return left === right;
        case '!=': return left !== right;
        case '&&': return left && right;
        case '||': return left || right;
        default: return true;
      }
    case 'Identifier':
      return ctx[node.name];
    case 'Literal':
      return node.value;
    default:
      return true;
  }
}
```

### 10.6 Step Navigation

- **Free backward navigation**: users can click any previous step freely
- **Gated forward navigation**: all required fields in the current step must be set and error-free before advancing
- **Step status badges**: each step shows a status indicator (`pristine` / `incomplete` / `valid` / `error`) derived from the validation response
- **Dependency change notification**: when a parent field changes and downstream fields are reset (via `resetFields`), affected steps show a warning badge and a toast: "Profiel was gereset omdat u materiaal hebt gewijzigd"

### 10.7 Frontend Build & Deployment

The Angular app is built as static files and served by nginx inside the Docker container. nginx handles both SPA routing and API reverse proxying.

```nginx
# frontend/nginx.conf
server {
    listen 80;
    root /usr/share/nginx/html;
    index index.html;

    # Reverse proxy API calls to the backend (Docker-internal)
    location /api/ {
        proxy_pass http://cpq-api:8080/api/;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_read_timeout 30s;
    }

    # SPA fallback: serve index.html for all non-file routes
    location / {
        try_files $uri $uri/ /index.html;
    }
}
```

```dockerfile
# frontend/Dockerfile
FROM node:20-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npx ng build --configuration=production

FROM nginx:alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/cpq-frontend/browser /usr/share/nginx/html
EXPOSE 80
```

The Angular app uses relative URLs (`/api/v1/configurations/...`). No `API_URL` environment variable is needed — nginx proxies all `/api/*` requests to the backend internally.

### 10.8 Routing

```typescript
// app.routes.ts
export const routes: Routes = [
  { path: '', redirectTo: 'products', pathMatch: 'full' },
  { path: 'products', component: ProductSelectorComponent },
  { path: 'configure/:configId', component: ConfiguratorComponent,
    canDeactivate: [unsavedChangesGuard] },
  { path: 'configurations', component: ConfigurationListComponent },
  { path: 'bom/:configId', component: BomViewerComponent },
];
```

The `unsavedChangesGuard` warns before navigating away from an unfinalized configuration.

### 10.9 Error Handling

The `error.interceptor.ts` handles:
- **Network errors** (`status === 0`): sets `connectionError` signal, shows banner with retry
- **409 Conflict**: reloads configuration from server, shows diff notification
- **502/503** (rules engine down): shows "Configuration engine temporarily unavailable"
- **400** (validation): shows inline field errors
- **Timeout**: `HttpClient` timeout set to 15s; shows retry option

---

## 11. Docker Compose Setup

```yaml
services:
  gorules-editor:
    image: gorules/jdm-editor:latest
    ports:
      - "127.0.0.1:3000:3000"       # localhost only — not exposed to network
    volumes:
      - ./rules:/data
    # WARNING: The standalone editor's file persistence to the volume must be
    # validated during Day 0 (V2). If it does not write files directly, consider
    # using the BRMS or a manual export workflow.

  gorules-agent:
    image: gorules/jdm-editor:latest
    ports:
      - "8080:8080"
    volumes:
      - ./rules:/data
    environment:
      - PROVIDER__TYPE=Filesystem
      - PROVIDER__ROOT_DIR=/data
    healthcheck:
      test: ["CMD", "wget", "--spider", "-q", "http://localhost:8080/api/health"]
      interval: 10s
      timeout: 5s
      retries: 3

  cpq-api:
    build:
      context: ./src
      dockerfile: Dockerfile
    ports:
      - "5000:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__Default=Host=postgres;Database=cpq;Username=cpq;Password=${POSTGRES_PASSWORD}
      - RulesEngine__BaseUrl=http://gorules-agent:8080
      - RulesEngine__ProjectSlug=${GORULES_PROJECT_SLUG:-default}
    depends_on:
      postgres:
        condition: service_healthy
      gorules-agent:
        condition: service_healthy
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health/live"]
      interval: 10s
      timeout: 5s
      retries: 3

  cpq-frontend:
    build:
      context: ./frontend
      dockerfile: Dockerfile
    ports:
      - "4200:80"
    depends_on:
      cpq-api:
        condition: service_healthy
    # No API_URL needed — nginx reverse proxy handles routing internally

  postgres:
    image: postgres:16
    ports:
      - "5432:5432"
    environment:
      - POSTGRES_DB=cpq
      - POSTGRES_USER=cpq
      - POSTGRES_PASSWORD=${POSTGRES_PASSWORD}
    volumes:
      - pg-data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U cpq -d cpq"]
      interval: 5s
      timeout: 3s
      retries: 5

volumes:
  pg-data:
```

**Secrets:** Credentials are stored in a `.env` file (excluded from Git via `.gitignore`). A `.env.example` template is committed:

```bash
# .env.example — copy to .env and fill in values
POSTGRES_PASSWORD=cpq_secret
GORULES_PROJECT_SLUG=default
```

**Rules storage:** Rules use a bind mount (`./rules:/data`) instead of a named volume. This allows Git versioning of rule files and survives `docker volume prune`. The `./rules/` directory is a Git repository.

---

## 12. C# Project Structure

```
src/
├── Cpq.Api/
│   ├── Program.cs
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── Dockerfile
│   ├── Controllers/
│   │   ├── ProductFamilyController.cs
│   │   ├── ProductController.cs
│   │   ├── ConfigurationController.cs
│   │   ├── BomController.cs
│   │   └── MesExportController.cs
│   ├── Middleware/
│   │   ├── CorrelationIdMiddleware.cs           ← X-Correlation-Id propagation
│   │   └── RequestLoggingMiddleware.cs          ← structured request/response logging
│   ├── Services/
│   │   ├── Rules/
│   │   │   ├── IRulesEngineClient.cs
│   │   │   ├── RulesEngineClient.cs
│   │   │   └── RulesEngineException.cs
│   │   ├── Configuration/
│   │   │   ├── IConfigurationService.cs
│   │   │   ├── IConfigurationRepository.cs
│   │   │   └── ConfigurationService.cs
│   │   ├── Specs/
│   │   │   ├── IProductSpecRepository.cs
│   │   │   ├── IProductTypeRepository.cs
│   │   │   ├── IProductOptionRepository.cs
│   │   │   ├── ProductSpecRepository.cs
│   │   │   └── SpecContextBuilder.cs
│   │   ├── Bom/
│   │   │   ├── IBomService.cs
│   │   │   ├── BomService.cs
│   │   │   └── SkuResolver.cs
│   │   ├── Mes/
│   │   │   ├── IMesExporter.cs                  ← pluggable adapter interface
│   │   │   ├── FileMesExporter.cs               ← PoC: writes JSON to directory
│   │   │   └── MesExportService.cs
│   │   └── Validation/
│   │       ├── SelectionValidator.cs             ← validates PATCH input against parameters
│   │       └── UpdateConfigurationRequestValidator.cs
│   ├── Models/
│   │   ├── Domain/
│   │   │   ├── ProductFamily.cs
│   │   │   ├── ProductType.cs
│   │   │   ├── ProductParameter.cs
│   │   │   ├── ProductOption.cs
│   │   │   ├── ProductSpec.cs
│   │   │   ├── Material.cs
│   │   │   ├── Color.cs
│   │   │   ├── Profile.cs
│   │   │   ├── Motor.cs
│   │   │   ├── GuideRail.cs
│   │   │   ├── Box.cs
│   │   │   ├── Accessory.cs
│   │   │   ├── Part.cs
│   │   │   ├── SkuMapping.cs
│   │   │   ├── Configuration.cs
│   │   │   ├── ConfigurationHistory.cs
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
│   ├── Configuration/
│   │   ├── RulesEngineOptions.cs
│   │   ├── CachingOptions.cs
│   │   └── PollyPolicies.cs
│   └── Exceptions/
│       ├── EntityNotFoundException.cs
│       └── ConcurrencyConflictException.cs
├── Cpq.Api.Tests/
│   ├── Services/
│   │   ├── ConfigurationServiceTests.cs          ← mocks IRulesEngineClient
│   │   ├── SpecContextBuilderTests.cs
│   │   ├── SelectionValidatorTests.cs
│   │   └── BomServiceTests.cs
│   ├── Integration/
│   │   ├── RulesEngineIntegrationTests.cs        ← requires Agent running (Docker)
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
│   │   │   │   ├── product.service.ts
│   │   │   │   ├── configuration.service.ts
│   │   │   │   ├── bom.service.ts
│   │   │   │   └── mes-export.service.ts
│   │   │   ├── models/
│   │   │   │   ├── product-family.model.ts
│   │   │   │   ├── product-type.model.ts
│   │   │   │   ├── configuration.model.ts
│   │   │   │   ├── validation-result.model.ts
│   │   │   │   ├── available-options.model.ts
│   │   │   │   └── bom.model.ts
│   │   │   ├── interceptors/
│   │   │   │   └── error.interceptor.ts
│   │   │   ├── guards/
│   │   │   │   └── unsaved-changes.guard.ts
│   │   │   └── utils/
│   │   │       └── visible-when.evaluator.ts    ← safe expression parser (jsep)
│   │   ├── features/
│   │   │   ├── product-selector/
│   │   │   │   ├── product-selector.component.ts
│   │   │   │   └── product-card.component.ts
│   │   │   ├── configurator/
│   │   │   │   ├── configurator.component.ts
│   │   │   │   ├── wizard-step.component.ts
│   │   │   │   ├── parameter-field.component.ts
│   │   │   │   ├── select-field.component.ts
│   │   │   │   ├── dimension-field.component.ts
│   │   │   │   ├── multi-select-field.component.ts
│   │   │   │   ├── validation-panel.component.ts
│   │   │   │   └── configurator.store.ts
│   │   │   ├── configuration-list/
│   │   │   │   └── configuration-list.component.ts
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
├── nginx.conf
├── Dockerfile
├── angular.json
├── package.json
└── tsconfig.json
```

---

## 13. MES Export Payload

**Note:** This payload format is a placeholder. The actual format depends on the MES system's API (Open Question #1). The MES export is implemented as a pluggable adapter (`IMesExporter`) so the transport and format can be changed without modifying the rest of the system.

```json
{
  "orderReference": "WO-2026-001234",
  "productFamily": "roller_shutter",
  "productType": "roller_shutter_standard",
  "configurationId": "uuid",
  "ruleVersion": "abc123",
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

### Phase 0: Day 0 Validation (Day 1)

- [ ] Deploy GoRules Agent + Editor via Docker Compose
- [ ] **V1:** Test actual Agent API endpoint format in filesystem mode (`curl`)
- [ ] **V2:** Test Editor file persistence to shared bind mount
- [ ] **V3:** Test ZEN expression dynamic key lookup (`spec.dimensions[material].maxWidth`)
- [ ] **V4:** Test Function node execution time with realistic BOM logic
- [ ] **V5:** Test Decision node sub-graph resolution via filesystem path
- [ ] Document findings and update `RulesEngineClient` endpoint path accordingly
- [ ] If V2 fails: decide on BRMS vs manual export workflow
- [ ] If V3 fails: redesign Expression nodes as Function nodes or flatten spec context

### Phase 1: Infrastructure (Day 2-3)

- [ ] Docker Compose with PostgreSQL, GoRules Editor, GoRules Agent, nginx reverse proxy
- [ ] `.env` file for secrets, `.env.example` committed, `.gitignore` excludes `.env`
- [ ] ASP.NET Core project scaffolded with EF Core, Serilog, health checks
- [ ] Full database schema with migrations (including all indexes, FKs, constraints, triggers)
- [ ] Seed data: roller_shutter family with 2 variants (standard + insulated)
- [ ] Seed reference data: 3 materials, 4 profiles, 5 motors, 4 box types, 20 colors, 3 guide rails
- [ ] Seed parts catalog with SKU mappings
- [ ] `RulesEngineClient` with Polly policies (endpoint path from V1 findings)
- [ ] Correlation ID middleware, exception handling middleware
- [ ] Input validation for PATCH selections (against `product_parameters`)
- [ ] Initialize Git repo in `./rules/` directory

### Phase 2: Shared Rule Building Blocks (Day 4-5)

- [ ] `shared/dimension-validation.json` — generic dimension checker using spec data
- [ ] `shared/weight-calculation.json` — weight from profile + dimensions
- [ ] `shared/motor-torque-calculation.json` — torque requirement calculation
- [ ] `shared/motor-sizing.json` — orchestrates weight → torque → motor filter
- [ ] `shared/box-size-selection.json` — roll diameter → box selection
- [ ] `shared/drive-configuration.json` — manual/motor sub-config with Switch
- [ ] Test each shared graph individually via Agent REST API
- [ ] Verify Function node execution within timeout limits

### Phase 3: Product Family Rules (Day 6-7)

- [ ] `families/roller-shutter/validate.json` — composes shared blocks + Switch by variant
- [ ] `families/roller-shutter/options.json` — dynamic option filtering + `resetFields`
- [ ] `families/roller-shutter/bom.json` — BOM generation with Function nodes
- [ ] Test complete flow: partial config → validate → filter → complete → BOM

### Phase 4: API & Orchestration (Day 8-9)

- [ ] `ProductSpecRepository` + `SpecContextBuilder` (parallelized, cached)
- [ ] `ConfigurationService` with enrichment pattern + optimistic concurrency
- [ ] `BomService` with `EnrichBomWithPartDetails` + SKU resolution
- [ ] `SelectionValidator` — validates PATCH input against product parameters
- [ ] All controllers + endpoints (with `/api/v1/` prefix)
- [ ] `FileMesExporter` — writes MES payload to directory (adapter pattern)
- [ ] Integration tests: C# → Agent → rule evaluation → response
- [ ] Verify adding insulated variant requires no code changes

### Phase 5: Angular Frontend (Day 10-11)

- [ ] Angular 18 project scaffolded with routing + HTTP client + Angular Material
- [ ] nginx.conf for reverse proxy + SPA routing
- [ ] Product selector page (family → type picker)
- [ ] Configuration list page with status filtering
- [ ] ConfiguratorStore (signal-based state, request sequencing, concurrency)
- [ ] Dynamic wizard step renderer from API parameter definition
- [ ] Parameter field components (dimension with debounce, select, multi-select)
- [ ] `visibleWhen` evaluator using `jsep` (safe, no `eval`)
- [ ] Step navigation with status badges and forward gating
- [ ] Real-time validation panel (errors + warnings, `aria-live`)
- [ ] Option filtering (disabled options with reason)
- [ ] Field reset notification (toast when `resetFields` received)
- [ ] Unsaved changes route guard
- [ ] Error interceptor (network errors, 409, 502/503)
- [ ] BOM viewer component
- [ ] MES export trigger
- [ ] Unit tests for ConfiguratorStore + visibleWhen evaluator

### Phase 6: Scale Validation (Day 12)

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
spec.dimensions[material].maxWidth    // dynamic key lookup (verify V3!)

-- Array
contains(controlTypes, "smart_home")
len(accessories)

-- Reference operator (in table cells)
$                                     // current cell value

-- Null coalescing and ternary
value ?? defaultValue                 // null coalescing
condition ? "yes" : "no"             // ternary

-- Within-node self-reference (in Expression nodes)
$.subtotal * taxRate                  // reference earlier row in same node
```

### 15.2 Switch Node Conditions

Switch nodes use **ZEN expression conditions**, not simple value matching. Each branch has a full expression:

```
Branch 1: userSelections.variant == "standard"
Branch 2: userSelections.variant == "insulated"
Branch 3: userSelections.variant == "security"
Default:  (catches everything else)
```

### 15.3 How to Add a New Material

1. Add record to `materials` table
2. Add records to `profiles` table
3. Add `product_specs` rows for dimension constraints
4. Add `product_options` rows so it appears in UI
5. Add `colors` and `material_colors` rows for available colors
6. Add `sku_mappings` for BOM resolution
7. **No rule file changes needed**

### 15.4 How to Add a New Product Variant

1. Add `product_types` record with `variant` code
2. Add `product_parameters` for the variant
3. Add `product_specs` with variant-specific constraints
4. In family's `validate.json`, add Switch branch with condition `userSelections.variant == "new_variant"`
5. Test via GoRules Editor simulator

### 15.5 How to Add a New Product Family

1. Create folder: `families/{new-family}/`
2. Create `validate.json`, `options.json`, `bom.json` — compose from `shared/*`
3. Add DB records: family, types, parameters, options, specs, parts, SKU mappings
4. Commit rule files to Git in the `./rules/` repository
5. **No C# code changes needed**

---

## 16. Performance, Resilience & Observability

### 16.1 Caching

| Data | Cache | TTL | Invalidation |
|---|---|---|---|
| Product specs (per product type) | Not cached | N/A | Always fresh |
| Profiles, motors, boxes, guide rails (global) | IMemoryCache | 5 min | TTL expiry; manual via admin endpoint for immediate invalidation |
| Product options | IMemoryCache | 5 min | TTL expiry |
| Agent health | Circuit breaker | Auto | Polly manages |

Rule evaluation is NOT cached — each call has unique context. Agent keeps compiled rules in memory.

**Multi-instance note:** `IMemoryCache` is per-process. If multiple API replicas are deployed, caches are inconsistent for up to 5 minutes. For production with multiple replicas, consider Redis as a distributed cache.

### 16.2 Resilience (Polly)

```
Policy stack (outer to inner):
  Circuit Breaker → Retry → HTTP call

- Retry: 3 attempts with exponential backoff (200ms, 400ms, 800ms)
  - Only retries 5xx + 408 + 429 (NOT 4xx validation errors)
- Circuit breaker: open after 5 failures in 30s, stay open 30s
- Timeout: 10s per individual call (HttpClient.Timeout)
```

When the circuit breaker opens, the API returns:
```json
{
  "status": 502,
  "title": "Rules engine unavailable",
  "detail": "Configuration engine is temporarily unavailable. Your selections have been saved."
}
```

### 16.3 Agent Scaling

- Single Agent handles ~1,000-10,000 eval/sec depending on graph complexity
- Function nodes are slower due to JS execution (may have 50ms timeout — validate in V4)
- Scale: run multiple Agent replicas behind load balancer (stateless, no shared state)

### 16.4 Logging & Observability

- **Structured logging** via Serilog with JSON output for log aggregation
- **Correlation IDs**: `X-Correlation-Id` header propagated from frontend through API to Agent calls
- **Request logging**: method, path, status code, elapsed time, correlation ID on every request
- **Rules engine logging**: decision path, elapsed time, correlation ID on every evaluation
- **Health checks**: `/health` (liveness), `/health/ready` (PostgreSQL + Agent connectivity)
- **Metrics** (post-PoC): Prometheus counters for `cpq_rule_evaluations_total`, `cpq_rule_evaluation_duration_seconds`, `cpq_configurations_created_total`

### 16.5 Rate Limiting

ASP.NET Core rate limiting on the PATCH endpoint to prevent runaway clients:

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("configuration-patch", opt =>
    {
        opt.PermitLimit = 10;        // max 10 PATCHes per window
        opt.Window = TimeSpan.FromSeconds(1);
    });
});
```

Combined with frontend debouncing (400ms on numeric inputs), this ensures the Agent is not overwhelmed.

---

## 17. Open Questions

1. **MES system API format** — REST, SOAP, file drop, message queue? (Determines `IMesExporter` implementation)
2. **Existing rule data** — Can we export current Econ/Navision rules as spreadsheets?
3. **Parts catalog** — Is there an existing SKU/parts database to import?
4. **Authentication** — Entra ID from start or post-PoC? (At minimum, basic API key for PoC to populate `created_by`)
5. **Product count** — Exact number of families and variants?
6. **GoRules licensing** — Free Editor for PoC. BRMS for production (Azure AD SSO, audit logging)?
7. **Rule versioning** — Git for PoC (bind mount). BRMS versioning for production?
8. **Multi-language** — Dutch only, or also French/English? (Affects DB schema: `name` vs `{ nl: "...", en: "..." }`)
9. **Pricing** — Part of CPQ, or separate?
10. **GoRules Editor persistence** — Does the standalone Editor write directly to filesystem? (Day 0 V2)
11. **GoRules Agent project slug** — What value to use in filesystem mode? (Day 0 V1)

---

## 18. Out of Scope (PoC)

- Multi-tenant / multi-user permissions (basic API key auth only)
- Pricing calculation
- Full order management workflow
- Customer-facing portal
- PDF generation (quotes, order confirmations)
- Legacy data migration from Navision/Econ
- Multi-language support
- Offline/mobile configuration
- Kubernetes / production deployment manifests
- Distributed caching (Redis)
- Full CI/CD pipeline (integration tests require Docker Compose)

---

## 19. Architecture Decision Records

### ADR-1: Reverse Proxy for Frontend Networking

**Decision:** The Angular frontend container runs nginx as a reverse proxy. All API calls use relative URLs (`/api/v1/...`). nginx proxies `/api/*` to `http://cpq-api:8080` internally.

**Context:** The browser cannot resolve Docker-internal hostnames. Setting `API_URL=http://cpq-api:8080` as an environment variable does not work because (a) Angular bakes environment variables at build time, and (b) the browser cannot resolve `cpq-api`.

**Consequences:** No CORS configuration needed. Same-origin for all traffic. SPA routing handled by `try_files`. Single point of entry for the frontend.

### ADR-2: Bind Mount for Rule Files

**Decision:** Rule files are stored in `./rules/` (bind mount) instead of a Docker named volume. The directory is a Git repository.

**Context:** Docker named volumes are opaque and can be lost via `docker volume prune`. Rule files contain critical business logic. Git versioning provides audit trail and rollback.

**Consequences:** Rule changes are tracked in Git. Developers can review rule changes in PRs. `docker volume prune` does not destroy rules.

### ADR-3: Optimistic Concurrency on Configurations

**Decision:** Configurations have a `version` column. Every PATCH request includes `expectedVersion`. Version mismatch returns 409 Conflict.

**Context:** The frontend fires a PATCH on every field change. With 100-500ms round trips, concurrent PATCHes from rapid user interaction or multiple tabs can cause lost updates (read-modify-write race).

**Consequences:** Frontend must track and send version. 409 responses trigger a reload-and-retry flow. No silent data loss.

### ADR-4: Server-Authoritative Field Resets

**Decision:** When a parent field changes (e.g., `material`), the rules engine's options graph returns a `resetFields` array. The C# service clears those fields server-side. The PATCH response reflects the cleared fields. The frontend updates its state from the response and shows a notification.

**Context:** Encoding dependency knowledge in the frontend violates the "no product logic in C#/frontend" principle. The rules engine already knows the dependency graph.

**Consequences:** Single source of truth for field dependencies. No product logic in frontend or C#. Users see clear feedback about why fields were reset.

### ADR-5: Safe Expression Evaluation for `visibleWhen`

**Decision:** `visibleWhen` expressions are evaluated using `jsep` (JavaScript Expression Parser) with a restricted evaluator. `eval()` and `new Function()` are never used.

**Context:** `visibleWhen` strings come from the database (set by technical staff). Using `eval()` would create an XSS vector if a malicious or compromised value is stored.

**Consequences:** Only `==`, `!=`, `&&`, `||`, identifiers, and literals are supported. Complex visibility logic must be handled differently (e.g., multiple simple expressions). No XSS risk.
