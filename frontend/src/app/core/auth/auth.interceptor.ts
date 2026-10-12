import { HttpErrorResponse, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { API_URL } from '../api/api.config';
import { AuthService } from './auth.service';


export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  const caminhosPublicosDeAutenticacao = ['/login', '/renovar', '/sair'];
  const ehChamadaPublicaDeAutenticacao = caminhosPublicosDeAutenticacao.some((caminho) =>
    req.url.endsWith(`/autenticacao${caminho}`),
  );
  if (!req.url.startsWith(API_URL) || ehChamadaPublicaDeAutenticacao) {
    return next(req);
  }

  const comToken = (requisicao: HttpRequest<unknown>, token: string | null) =>
    token ? requisicao.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : requisicao;

  return next(comToken(req, auth.tokenAcesso())).pipe(
    catchError((erro: unknown) => {
      const naoAutorizado = erro instanceof HttpErrorResponse && erro.status === 401;
      if (!naoAutorizado || !auth.temRenovacao()) {
        return throwError(() => erro);
      }

      return auth.renovar().pipe(
        switchMap((novoToken) => next(comToken(req, novoToken))),
        catchError((erroRenovacao: unknown) => {
          auth.limparSessao();
          router.navigate(['/login']);
          return throwError(() => erroRenovacao);
        }),
      );
    }),
  );
};
