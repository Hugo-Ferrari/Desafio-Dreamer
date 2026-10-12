import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, catchError, finalize, map, of, shareReplay, tap, throwError } from 'rxjs';
import { API_URL } from '../api/api.config';
import { LoginRequest, PerfilFuncionario, TokensResponse } from '../api/models';

export interface UsuarioLogado {
  id: string;
  nome: string;
  email: string;
  perfil: PerfilFuncionario;
}

const CHAVE_STORAGE = 'ponto.tokens';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly tokens = signal<TokensResponse | null>(this.lerDoStorage());
  private renovacaoEmAndamento$: Observable<string> | null = null;

  readonly tokenAcesso = computed(() => this.tokens()?.tokenAcesso ?? null);
  readonly logado = computed(() => this.tokens() !== null);
  readonly precisaAlterarSenha = computed(() => {
    const token = this.tokenAcesso();
    const claims = token ? lerClaims(token) : null;
    return claims?.['troca_senha_inicial'] === true;
  });

  readonly usuario = computed<UsuarioLogado | null>(() => {
    const token = this.tokenAcesso();
    const claims = token ? lerClaims(token) : null;
    if (!claims) {
      return null;
    }
    return {
      id: String(claims['sub']),
      nome: String(claims['name']),
      email: String(claims['email']),
      perfil: claims['role'] as PerfilFuncionario,
    };
  });

  readonly perfil = computed(() => this.usuario()?.perfil ?? null);
  readonly ehGestorOuRh = computed(() => this.perfil() === 'Gestor' || this.perfil() === 'RH');
  readonly ehRh = computed(() => this.perfil() === 'RH');

  entrar(credenciais: LoginRequest): Observable<UsuarioLogado | null> {
    return this.http.post<TokensResponse>(`${API_URL}/autenticacao/login`, credenciais).pipe(
      tap((resposta) => this.guardar(resposta)),
      map(() => this.usuario()),
    );
  }

  alterarSenhaInicial(novaSenha: string): Observable<void> {
    return this.http
      .post<void>(`${API_URL}/autenticacao/alterar-senha-inicial`, { novaSenha })
      .pipe(tap(() => this.limparSessao()));
  }

 
  renovar(): Observable<string> {
    if (this.renovacaoEmAndamento$) {
      return this.renovacaoEmAndamento$;
    }

    const tokenRenovacao = this.tokens()?.tokenRenovacao;
    if (!tokenRenovacao) {
      return throwError(() => new Error('Sessão sem token de renovação.'));
    }

    this.renovacaoEmAndamento$ = this.http
      .post<TokensResponse>(`${API_URL}/autenticacao/renovar`, { tokenRenovacao })
      .pipe(
        tap((resposta) => this.guardar(resposta)),
        map((resposta) => resposta.tokenAcesso),
        finalize(() => (this.renovacaoEmAndamento$ = null)),
        shareReplay({ bufferSize: 1, refCount: false }),
      );

    return this.renovacaoEmAndamento$;
  }

  temRenovacao(): boolean {
    return !!this.tokens()?.tokenRenovacao;
  }

  
  sair(): Observable<void> {
    const tokenRenovacao = this.tokens()?.tokenRenovacao;
    this.limparSessao();
    this.router.navigate(['/login']);

    if (!tokenRenovacao) {
      return of(undefined);
    }
    return this.http.post<void>(`${API_URL}/autenticacao/sair`, { tokenRenovacao }).pipe(
      catchError(() => of(undefined)),
    );
  }

  limparSessao(): void {
    this.tokens.set(null);
    localStorage.removeItem(CHAVE_STORAGE);
  }

  private guardar(resposta: TokensResponse): void {
    this.tokens.set(resposta);
    localStorage.setItem(CHAVE_STORAGE, JSON.stringify(resposta));
  }

  private lerDoStorage(): TokensResponse | null {
    try {
      const texto = localStorage.getItem(CHAVE_STORAGE);
      return texto ? (JSON.parse(texto) as TokensResponse) : null;
    } catch {
      return null;
    }
  }
}

function lerClaims(token: string): Record<string, unknown> | null {
  try {
    const base64 = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
    const bytes = Uint8Array.from(atob(base64), (c) => c.charCodeAt(0));
    return JSON.parse(new TextDecoder().decode(bytes)) as Record<string, unknown>;
  } catch {
    return null;
  }
}
