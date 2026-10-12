import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { mensagemDeErro } from '../../core/api/utils';

@Component({
  selector: 'app-login',
  imports: [FormsModule],
  template: `
    <main class="login">
      <form (ngSubmit)="entrar()">
        <div class="login-brand">
          <div class="login-brand-mark">PJ</div>
          <div>
            <p class="page-kicker">Acesso</p>
            <h1>Plataforma de Jornada</h1>
          </div>
        </div>
        <p class="login-intro">Entre para registrar sua jornada e consultar suas informações de trabalho.</p>

        <label>
          E-mail
          <input id="email" type="email" name="email" [(ngModel)]="email" required autocomplete="username" />
        </label>
        <label>
          Senha
          <input id="senha" type="password" name="senha" [(ngModel)]="senha" required autocomplete="current-password" />
        </label>
        @if (erro()) { <p class="erro" role="alert">{{ erro() }}</p> }
        <button type="submit" [disabled]="carregando()">{{ carregando() ? 'Entrando…' : 'Entrar' }}</button>
      </form>
    </main>
  `,
})
export class LoginPage {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  email = '';
  senha = '';
  readonly carregando = signal(false);
  readonly erro = signal('');

  constructor() {
    if (this.auth.logado()) {
      this.router.navigate([this.auth.precisaAlterarSenha() ? '/alterar-senha-inicial' : '/']);
    }
  }

  entrar() {
    this.carregando.set(true);
    this.erro.set('');
    this.auth.entrar({ email: this.email, senha: this.senha }).subscribe({
      next: () => this.router.navigate([this.auth.precisaAlterarSenha() ? '/alterar-senha-inicial' : '/']),
      error: (e) => {
        this.erro.set(mensagemDeErro(e));
        this.carregando.set(false);
      },
    });
  }
}
