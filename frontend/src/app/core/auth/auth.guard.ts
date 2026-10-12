import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { PerfilFuncionario } from '../api/models';
import { AuthService } from './auth.service';


export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (!auth.logado()) return router.createUrlTree(['/login']);
  return auth.precisaAlterarSenha() ? router.createUrlTree(['/alterar-senha-inicial']) : true;
};

export const senhaInicialGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (!auth.logado()) return router.createUrlTree(['/login']);
  return auth.precisaAlterarSenha() ? true : router.createUrlTree(['/']);
};


export const perfilGuard =
  (...perfisPermitidos: PerfilFuncionario[]): CanActivateFn =>
  () => {
    const auth = inject(AuthService);
    const router = inject(Router);
    const perfil = auth.perfil();
    return perfil && perfisPermitidos.includes(perfil) ? true : router.createUrlTree(['/']);
  };
