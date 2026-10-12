import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { mensagemDeErro } from '../../core/api/utils';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-alterar-senha-inicial',
  imports: [FormsModule],
  template: `
    <main class="login">
      <form (ngSubmit)="alterarSenha()">
        <div class="login-brand">
          <div class="login-brand-mark">PJ</div>
          <div>
            <p class="page-kicker">Primeiro acesso</p>
            <h1>Crie sua senha pessoal</h1>
          </div>
        </div>
        @if (!alterada()) {
          <p class="login-intro">Por segurança, troque a senha inicial antes de usar a plataforma. A nova senha deve ter pelo menos 12 caracteres.</p>
          <label>
            Nova senha
            <input
              type="password"
              name="novaSenha"
              [(ngModel)]="novaSenha"
              required
              minlength="12"
              maxlength="128"
              autocomplete="new-password"
            />
          </label>
          <label>
            Confirmar nova senha
            <input
              type="password"
              name="confirmacao"
              [(ngModel)]="confirmacao"
              required
              minlength="12"
              maxlength="128"
              autocomplete="new-password"
            />
          </label>
          @if (erro()) { <p class="erro" role="alert">{{ erro() }}</p> }
          <button type="submit" [disabled]="salvando()">
            {{ salvando() ? 'Salvando…' : 'Salvar nova senha' }}
          </button>
          <button type="button" class="secundario" (click)="sair()">Sair</button>
        } @else {
          <p class="ok" role="status">Senha alterada. Entre novamente com sua nova senha.</p>
          <button type="button" (click)="irParaLogin()">Ir para o login</button>
        }
      </form>
    </main>
  `,
})
export class AlterarSenhaInicialPage {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  novaSenha = '';
  confirmacao = '';
  readonly erro = signal('');
  readonly salvando = signal(false);
  readonly alterada = signal(false);

  alterarSenha() {
    this.erro.set('');
    if (this.novaSenha !== this.confirmacao) {
      this.erro.set('As senhas informadas não são iguais.');
      return;
    }

    this.salvando.set(true);
    this.auth.alterarSenhaInicial(this.novaSenha).subscribe({
      next: () => {
        this.salvando.set(false);
        this.alterada.set(true);
      },
      error: (erro: unknown) => {
        this.salvando.set(false);
        this.erro.set(mensagemDeErro(erro));
      },
    });
  }

  sair() {
    this.auth.sair().subscribe();
  }

  irParaLogin() {
    this.router.navigate(['/login']);
  }
}
