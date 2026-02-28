import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Configuration } from '../models/configuration.model';

export interface CreateConfigurationRequest {
  productTypeCode: string;
}

export interface UpdateConfigurationRequest {
  selections: Record<string, unknown>;
  expectedVersion: number;
}

@Injectable({
  providedIn: 'root'
})
export class ConfigurationService extends ApiService {

  createConfiguration(request: CreateConfigurationRequest): Observable<Configuration> {
    return this.http.post<Configuration>(`${this.baseUrl}/configurations`, request);
  }

  getConfiguration(id: string): Observable<Configuration> {
    return this.http.get<Configuration>(`${this.baseUrl}/configurations/${id}`);
  }

  updateConfiguration(id: string, request: UpdateConfigurationRequest): Observable<Configuration> {
    return this.http.patch<Configuration>(`${this.baseUrl}/configurations/${id}`, request);
  }

  finalizeConfiguration(id: string): Observable<Configuration> {
    return this.http.post<Configuration>(`${this.baseUrl}/configurations/${id}/finalize`, {});
  }

  deleteConfiguration(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/configurations/${id}`);
  }

  cloneConfiguration(id: string): Observable<Configuration> {
    return this.http.post<Configuration>(`${this.baseUrl}/configurations/${id}/clone`, {});
  }

  listConfigurations(): Observable<Configuration[]> {
    return this.http.get<Configuration[]>(`${this.baseUrl}/configurations`);
  }
}
