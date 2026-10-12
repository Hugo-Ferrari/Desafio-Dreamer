import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { API_URL } from './api.config';
import {
  CriarFuncionarioRequest,
  FechamentoPeriodoResponse,
  FuncionarioResponse,
  OpcoesCadastroFuncionarioResponse,
} from './models';


@Injectable({ providedIn: 'root' })
export class AdminApi {
  private readonly http = inject(HttpClient);

  funcionarios() {
    return this.http.get<FuncionarioResponse[]>(`${API_URL}/admin/funcionarios`);
  }

  opcoesCadastroFuncionario() {
    return this.http.get<OpcoesCadastroFuncionarioResponse>(`${API_URL}/admin/funcionarios/opcoes-cadastro`);
  }

  criarFuncionario(request: CriarFuncionarioRequest) {
    return this.http.post<FuncionarioResponse>(`${API_URL}/admin/funcionarios`, request);
  }

  
  fecharPeriodo(competencia: string, codigoPais: string) {
    const params = new HttpParams().set('codigoPais', codigoPais);
    return this.http.post<FechamentoPeriodoResponse>(`${API_URL}/admin/periodos/${competencia}/fechar`, null, { params });
  }

  
  relatorioCsv(competencia: string, codigoPais?: string) {
    let params = new HttpParams();
    if (codigoPais) params = params.set('codigoPais', codigoPais);
    return this.http.get(`${API_URL}/admin/relatorios/${competencia}.csv`, { params, responseType: 'blob' });
  }
}
