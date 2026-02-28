export interface BomLine {
  partSku: string;
  partName: string;
  category: string;
  quantity: number;
  unit: string;
  cutLengthMm?: number;
  sortOrder: number;
  notes?: string;
}

export interface BomResponse {
  lines: BomLine[];
  totalWeight: number;
  generatedAt: string;
}
