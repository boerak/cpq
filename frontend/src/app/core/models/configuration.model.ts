import { ProductType } from './product-type.model';
import { ValidationResult } from './validation-result.model';
import { AvailableOption } from './available-options.model';

export interface Configuration {
  id: string;
  productType: ProductType;
  status: 'draft' | 'valid' | 'finalized' | 'invalid';
  config: Record<string, unknown>;
  validation?: ValidationResult;
  availableOptions?: Record<string, AvailableOption[]>;
  resetFields?: string[];
  isComplete: boolean;
  canFinalize: boolean;
  version: number;
  createdAt: string;
  updatedAt: string;
}
