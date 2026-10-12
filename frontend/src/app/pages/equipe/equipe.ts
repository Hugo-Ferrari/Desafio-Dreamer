import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { EquipeApi } from '../../core/api/equipe.api';
import { EspelhoMensalResponse, MembroEquipeStatusResponse } from '../../core/api/models';
import { competenciaDe, formatarMinutos, horaDe, mensagemDeErro } from '../../core/api/utils';

@Component({
  selector: 'app-equipe',
  imports: [FormsModule],
  template: `
    <section class="page-shell">
      <header class="page-header">
        <div>
          <p class="page-kicker">Gestão</p>
          <h2>Equipe</h2>
          <p>Acompanhe a situação da equipe e consulte o espelho mensal de cada pessoa.</p>
        </div>
      </header>

      @if (erro()) { <p class="erro" role="alert">{{ erro() }}</p> }

      <section class="panel">
        <div class="table-panel">
          <table>
            <caption class="sr-only">Situação atual dos membros da equipe</caption>
            <thead><tr><th>Nome</th><th>País</th><th>Situação</th><th>Última marcação</th><th></th></tr></thead>
            <tbody>
              @for (m of membros(); track m.funcionarioId) {
                <tr>
                  <td>{{ m.nome }}</td><td>{{ m.codigoPais }}</td><td><span class="chip">{{ m.estado }}</span></td>
                  <td>{{ m.ultimaMarcacao ? m.ultimaMarcacao.tipo + ' às ' + hora(m.ultimaMarcacao.ocorridoEmLocal) : '—' }}</td>
                  <td><button type="button" class="pequeno" (click)="abrirEspelho(m)">Ver espelho</button></td>
                </tr>
              } @empty {
                <tr><td colspan="5" class="empty-state">Nenhum membro da equipe para exibir.</td></tr>
              }
            </tbody>
          </table>
        </div>
      </section>

      @if (selecionado(); as s) {
        <section class="panel">
          <h3>Espelho de {{ s.nome }}</h3>
          <div class="field-grid field-grid-spaced">
            <label>
              Mês
              <input type="month" [(ngModel)]="competencia" name="competencia" (change)="abrirEspelho(s)" />
            </label>
          </div>
          @if (espelho(); as e) {
            <div class="info-banner summary-banner">
              <strong>Resumo</strong>
              <p>Esperado {{ min(e.totais.minutosEsperados) }} · Trabalhado {{ min(e.totais.minutosTrabalhados) }} · Saldo {{ min(e.totais.saldoMinutos) }} · Faltas {{ e.totais.faltas }}</p>
            </div>
            <div class="table-panel">
              <table>
                <caption class="sr-only">Resumo diário do espelho de {{ s.nome }}</caption>
                <thead><tr><th>Data</th><th>Situação</th><th>Trabalhado</th><th>Saldo</th></tr></thead>
                <tbody>
                  @for (d of e.dias; track d.data) {
                    <tr><td>{{ d.data }}</td><td>{{ d.situacao }}</td><td>{{ min(d.minutosTrabalhados) }}</td><td>{{ min(d.saldoMinutos) }}</td></tr>
                  }
                </tbody>
              </table>
            </div>
          }
        </section>
      }
    </section>
  `,
})
export class EquipePage implements OnInit {
  private readonly api = inject(EquipeApi);
  readonly min = formatarMinutos;
  readonly hora = horaDe;
  competencia = competenciaDe();
  readonly membros = signal<MembroEquipeStatusResponse[]>([]);
  readonly selecionado = signal<MembroEquipeStatusResponse | null>(null);
  readonly espelho = signal<EspelhoMensalResponse | null>(null);
  readonly erro = signal('');

  ngOnInit() {
    this.api.status().subscribe({ next: (l) => this.membros.set(l), error: (e) => this.erro.set(mensagemDeErro(e)) });
  }

  abrirEspelho(m: MembroEquipeStatusResponse) {
    this.selecionado.set(m);
    this.erro.set('');
    this.api.espelho(m.funcionarioId, this.competencia).subscribe({
      next: (e) => this.espelho.set(e),
      error: (e) => { this.espelho.set(null); this.erro.set(mensagemDeErro(e)); },
    });
  }
}
