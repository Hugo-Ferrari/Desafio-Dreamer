import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { API_URL } from './api.config';
import { FuncionarioResponse, StatusJornadaResponse } from './models';

@Injectable({ providedIn: 'root' })
export class EuApi {
  private readonly http = inject(HttpClient);

  perfil() {
    return this.http.get<FuncionarioResponse>(`${API_URL}/eu`);
  }


  status() {
    return this.http.get<StatusJornadaResponse>(`${API_URL}/eu/status`);
  }
}
