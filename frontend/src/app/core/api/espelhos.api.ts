import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { API_URL } from './api.config';
import { EspelhoMensalResponse } from './models';

@Injectable({ providedIn: 'root' })
export class EspelhosApi {
  private readonly http = inject(HttpClient);

 
  obter(competencia: string) {
    return this.http.get<EspelhoMensalResponse>(`${API_URL}/espelhos/${competencia}`);
  }
}
