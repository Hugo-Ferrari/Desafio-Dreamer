import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { API_URL } from './api.config';
import {
  CriarSolicitacaoAjusteRequest,
  SolicitacaoAjusteResponse,
  StatusSolicitacaoAjuste,
} from './models';

@Injectable({ providedIn: 'root' })
export class AjustesApi {
  private readonly http = inject(HttpClient);

  criar(solicitacao: CriarSolicitacaoAjusteRequest) {
    return this.http.post<SolicitacaoAjusteResponse>(`${API_URL}/ajustes`, solicitacao);
  }

  
  listar(status?: StatusSolicitacaoAjuste) {
    let params = new HttpParams();
    if (status) params = params.set('status', status);
    return this.http.get<SolicitacaoAjusteResponse[]>(`${API_URL}/ajustes`, { params });
  }


  aprovar(id: string, comentario?: string) {
    return this.http.post<SolicitacaoAjusteResponse>(`${API_URL}/ajustes/${id}/aprovar`, { comentario: comentario ?? null });
  }

  rejeitar(id: string, comentario?: string) {
    return this.http.post<SolicitacaoAjusteResponse>(`${API_URL}/ajustes/${id}/rejeitar`, { comentario: comentario ?? null });
  }
}
