import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <div class="shell-app">
      <a class="skip-link" href="#conteudo">Pular para o conteúdo</a>
      <header class="topo">
        <div class="brand" aria-label="Plataforma de Jornada">
          <div class="brand-mark">PJ</div>
          <div class="brand-text">
            <span class="eyebrow">Dashboard</span>
            <strong>Plataforma de Jornada</strong>
          </div>
        </div>

        <nav class="menu" aria-label="Menu principal">
          <a routerLink="/ponto" routerLinkActive="ativo" ariaCurrentWhenActive="page">Ponto</a>
          <a routerLink="/espelho" routerLinkActive="ativo" ariaCurrentWhenActive="page">Espelho</a>
          <a routerLink="/ajustes" routerLinkActive="ativo" ariaCurrentWhenActive="page">Ajustes</a>
          @if (auth.ehGestorOuRh()) { <a routerLink="/equipe" routerLinkActive="ativo" ariaCurrentWhenActive="page">Equipe</a> }
          @if (auth.ehRh()) { <a routerLink="/admin" routerLinkActive="ativo" ariaCurrentWhenActive="page">Administração</a> }
        </nav>

        <div class="usuario" aria-label="Perfil do usuário">
          <div class="usuario-info">
            <span class="usuario-label">Usuário</span>
            <strong>{{ auth.usuario()?.nome ?? 'Usuário' }}</strong>
            <small>{{ auth.perfil() }}</small>
          </div>
          <button type="button" (click)="auth.sair().subscribe()">Sair</button>
        </div>
      </header>

      <main id="conteudo" class="conteudo" tabindex="-1">
        <div class="conteudo-inner">
          <router-outlet />
        </div>
      </main>
    </div>
  `,
  styles: `
    :host {
      display: block;
      min-height: 100vh;
      background: #f4f6fb;
      color: #0f172a;
      font-family: 'Segoe UI', sans-serif;
    }

    .shell-app {
      min-height: 100vh;
      display: grid;
      grid-template-rows: auto 1fr;
      gap: 28px;
      width: min(100%, 1440px);
      margin: 0 auto;
      padding: 20px clamp(16px, 3vw, 40px) 48px;
      box-sizing: border-box;
    }

    .topo {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 22px;
      padding: 14px 18px;
      border: 1px solid #e0e5ef;
      border-radius: 18px;
      background: #fff;
      color: #14213d;
      box-shadow: 0 8px 24px rgba(20, 33, 61, 0.06);
      position: sticky;
      top: 12px;
      z-index: 5;
    }

    .brand {
      display: flex;
      align-items: center;
      gap: 12px;
      min-width: 0;
    }

    .brand-mark {
      width: 40px;
      height: 40px;
      border-radius: 12px;
      background: linear-gradient(135deg, #315de8, #193da9);
      display: grid;
      place-items: center;
      font-size: 0.82rem;
      font-weight: 800;
      letter-spacing: 0.08em;
      color: white;
      box-shadow: 0 5px 12px rgba(47, 91, 234, 0.2);
    }

    .brand-text {
      display: flex;
      flex-direction: column;
      line-height: 1.2;
      min-width: 0;
    }

    .brand-text strong {
      font-size: 0.98rem;
      white-space: nowrap;
    }

    .eyebrow {
      color: #596781;
      font-size: 0.65rem;
      text-transform: uppercase;
      letter-spacing: 0.1em;
    }

    .menu {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 4px;
      flex-wrap: wrap;
      padding: 4px;
      background: #f4f6fa;
      border: 1px solid #edf0f5;
      border-radius: 12px;
      flex: 1;
    }

    .menu a {
      color: #46536e;
      text-decoration: none;
      padding: 9px 13px;
      border-radius: 9px;
      font-size: 0.9rem;
      font-weight: 600;
      transition: background 0.15s ease, color 0.15s ease, box-shadow 0.15s ease;
    }

    .menu a:hover {
      background: #e9edf6;
      color: #14213d;
    }

    .menu a.ativo {
      background: #fff;
      color: #193da9;
      box-shadow: 0 1px 4px rgba(20, 33, 61, 0.12);
    }

    .usuario {
      display: flex;
      align-items: center;
      gap: 12px;
      padding-left: 16px;
      border-left: 1px solid #e5e9f1;
    }

    .usuario-info {
      display: flex;
      flex-direction: column;
      align-items: flex-start;
      line-height: 1.2;
    }

    .usuario-label {
      color: #596781;
      font-size: 0.62rem;
      text-transform: uppercase;
      letter-spacing: 0.08em;
    }

    .usuario-info strong {
      font-size: 0.86rem;
      max-width: 180px;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    .usuario-info small {
      color: #596781;
      font-weight: 600;
    }

    .usuario button {
      min-height: 40px;
      border: 1px solid #d9dfeb;
      background: #fff;
      color: #34415c;
      padding: 8px 12px;
      border-radius: 9px;
      cursor: pointer;
      font-weight: 600;
      transition: background 0.15s ease, border-color 0.15s ease;
    }

    .usuario button:hover {
      background: #f4f6fa;
      border-color: #b9c1d3;
    }

    .conteudo {
      min-width: 0;
    }

    .conteudo-inner {
      min-width: 0;
    }

    @media (max-width: 1040px) {
      .topo {
        display: grid;
        grid-template-columns: 1fr auto;
        gap: 12px 16px;
      }

      .menu {
        grid-column: 1 / -1;
        grid-row: 2;
        justify-content: flex-start;
      }

      .usuario {
        justify-self: end;
      }
    }

    @media (max-width: 560px) {
      .shell-app {
        gap: 18px;
        padding: 12px 12px 32px;
      }

      .topo {
        top: 8px;
        padding: 12px;
        border-radius: 14px;
      }

      .brand {
        gap: 9px;
      }

      .brand-text strong {
        white-space: normal;
        font-size: 0.88rem;
      }

      .menu {
        flex-wrap: nowrap;
        justify-content: flex-start;
        overflow-x: auto;
        overscroll-behavior-x: contain;
        scrollbar-width: thin;
      }

      .menu a {
        flex: 0 0 auto;
        white-space: nowrap;
      }

      .usuario {
        gap: 8px;
        padding-left: 10px;
      }

      .usuario-label,
      .usuario-info small {
        display: none;
      }

      .usuario-info strong {
        max-width: 98px;
        font-size: 0.8rem;
      }

      .usuario button {
        min-height: 38px;
        padding: 7px 9px;
      }
    }
  `,
})
export class ShellPage {
  readonly auth = inject(AuthService);
}
