import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ProductFamily } from '../models/product-family.model';
import { ProductType } from '../models/product-type.model';
import { ParametersResponse } from '../models/product-parameters.model';

@Injectable({
  providedIn: 'root'
})
export class ProductService extends ApiService {

  getFamilies(): Observable<ProductFamily[]> {
    return this.http.get<ProductFamily[]>(`${this.baseUrl}/families`);
  }

  getProductsByFamily(familyCode: string): Observable<ProductType[]> {
    return this.http.get<ProductType[]>(`${this.baseUrl}/families/${familyCode}/products`);
  }

  getParameters(productCode: string): Observable<ParametersResponse> {
    return this.http.get<ParametersResponse>(`${this.baseUrl}/products/${productCode}/parameters`);
  }
}
