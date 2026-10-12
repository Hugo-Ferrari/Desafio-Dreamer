import { Routes } from '@angular/router';
import { authGuard, perfilGuard, senhaInicialGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  { path: 'login', loadComponent: () => import('./pages/login/login').then((m) => m.LoginPage) },
  {
    path: 'alterar-senha-inicial',
    canActivate: [senhaInicialGuard],
    loadComponent: () => import('./pages/alterar-senha-inicial/alterar-senha-inicial').then((m) => m.AlterarSenhaInicialPage),
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./pages/shell/shell').then((m) => m.ShellPage),
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'ponto' },
      { path: 'ponto', loadComponent: () => import('./pages/ponto/ponto').then((m) => m.PontoPage) },
      { path: 'espelho', loadComponent: () => import('./pages/espelho/espelho').then((m) => m.EspelhoPage) },
      { path: 'ajustes', loadComponent: () => import('./pages/ajustes/ajustes').then((m) => m.AjustesPage) },
      {
        path: 'equipe',
        canActivate: [perfilGuard('Gestor', 'RH')],
        loadComponent: () => import('./pages/equipe/equipe').then((m) => m.EquipePage),
      },
      {
        path: 'admin',
        canActivate: [perfilGuard('RH')],
        loadComponent: () => import('./pages/admin/admin').then((m) => m.AdminPage),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
