export interface ValidationError {
  parameter: string;
  rule: string;
  message: string;
}

export interface ValidationWarning {
  parameter: string;
  rule: string;
  message: string;
}

export interface ValidationResult {
  valid: boolean;
  errors: ValidationError[];
  warnings: ValidationWarning[];
}
