import { ProductFamily } from './product-family.model';

export interface ProductType {
  code: string;
  name: string;
  variant: string;
  description: string;
  family: ProductFamily;
  displayOrder: number;
}
