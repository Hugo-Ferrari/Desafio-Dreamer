import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { API_URL } from './api.config';
import { EspelhoMensalResponse, MembroEquipeStatusResponse } from './models';


@Injectable({ providedIn: 'root' })
export class EquipeApi {
  private readonly http = inject(HttpClient);

  status() {
    return this.http.get<MembroEquipeStatusResponse[]>(`${API_URL}/equipe/status`);
  }

  espelho(funcionarioId: string, competencia: string) {
    return this.http.get<EspelhoMensalResponse>(`${API_URL}/equipe/${funcionarioId}/espelhos/${competencia}`);
  }
}
