import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { BomResponse } from '../models/bom.model';

@Injectable({
  providedIn: 'root'
})
export class BomService extends ApiService {

  generateBom(configurationId: string): Observable<BomResponse> {
    return this.http.post<BomResponse>(`${this.baseUrl}/configurations/${configurationId}/bom`, {});
  }

  getBom(configurationId: string): Observable<BomResponse> {
    return this.http.get<BomResponse>(`${this.baseUrl}/configurations/${configurationId}/bom`);
  }
}
