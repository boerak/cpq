export interface Option {
  code: string;
  displayName: string;
  available: boolean;
}

export interface Parameter {
  code: string;
  name: string;
  dataType: 'integer' | 'select' | 'multi_select' | 'boolean' | 'text';
  unit?: string;
  isRequired: boolean;
  dependsOn: string[];
  metadata: Record<string, unknown>;
  options: Option[];
}

export interface Step {
  stepNumber: number;
  stepName: string;
  parameters: Parameter[];
}

export interface ParametersResponse {
  steps: Step[];
}
