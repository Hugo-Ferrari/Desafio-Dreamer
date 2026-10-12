import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { API_URL } from './api.config';
import { MarcacaoResponse, ModalidadeTrabalho, TipoMarcacao } from './models';
import { fusoDoNavegador } from './utils';

@Injectable({ providedIn: 'root' })
export class MarcacoesApi {
  private readonly http = inject(HttpClient);

  
  registrar(tipo: TipoMarcacao, modalidade: ModalidadeTrabalho, chaveIdempotencia?: string) {
    return this.http.post<MarcacaoResponse>(`${API_URL}/marcacoes`, {
      tipo,
      modalidadeTrabalho: modalidade,
      origemRegistro: 'Web',
      fusoHorarioId: fusoDoNavegador(),
      chaveIdempotencia: chaveIdempotencia ?? crypto.randomUUID(),
    });
  }


  listar(de?: string, ate?: string) {
    let params = new HttpParams();
    if (de) params = params.set('de', de);
    if (ate) params = params.set('ate', ate);
    return this.http.get<MarcacaoResponse[]>(`${API_URL}/marcacoes`, { params });
  }
}
