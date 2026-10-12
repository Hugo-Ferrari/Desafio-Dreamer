import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { EspelhosApi } from '../../core/api/espelhos.api';
import { EspelhoMensalResponse } from '../../core/api/models';
import { competenciaDe, formatarMinutos, horaDe, mensagemDeErro } from '../../core/api/utils';

@Component({
  selector: 'app-espelho',
  imports: [FormsModule],
  template: `
    <section class="page-shell">
      <header class="page-header">
        <div>
          <p class="page-kicker">Relatórios</p>
          <h2>Espelho de ponto</h2>
          <p>Confira as marcações, os totais e o saldo do mês selecionado.</p>
        </div>
      </header>

      <section class="panel">
        <div class="field-grid">
          <label>
            Mês
            <input type="month" [(ngModel)]="competencia" name="competencia" (change)="carregar()" />
          </label>
        </div>
      </section>

      @if (erro()) { <p class="erro" role="alert">{{ erro() }}</p> }
      @if (espelho(); as e) {
        <section class="panel">
          <div class="info-banner">
            <strong>{{ e.nomeFuncionario }}</strong>
            <p>{{ e.codigoPais }} · {{ e.periodoFechado ? 'Período fechado' : 'Período aberto' }}</p>
          </div>

          <ul class="status-pills">
            <li>Esperado: {{ min(e.totais.minutosEsperados) }}</li>
            <li>Trabalhado: {{ min(e.totais.minutosTrabalhados) }}</li>
            <li>Saldo: {{ min(e.totais.saldoMinutos) }}</li>
            <li>Dias incompletos: {{ e.totais.diasIncompletos }}</li>
            <li>Faltas: {{ e.totais.faltas }}</li>
          </ul>
        </section>

        <section class="panel">
          <div class="table-panel">
            <table>
              <caption class="sr-only">Detalhamento diário do espelho de ponto</caption>
              <thead><tr><th>Data</th><th>Situação</th><th>Marcações</th><th>Trabalhado</th><th>Saldo</th><th>Obs.</th></tr></thead>
              <tbody>
                @for (d of e.dias; track d.data) {
                  <tr>
                    <td>{{ d.data }}</td><td>{{ d.situacao }}</td>
                    <td>@for (m of d.marcacoes; track m.id) { @if (!m.anulada) { <span class="chip">{{ hora(m.ocorridoEmLocal) }} {{ m.tipo }}</span> } }</td>
                    <td>{{ min(d.minutosTrabalhados) }}</td><td>{{ min(d.saldoMinutos) }}</td>
                    <td>{{ d.observacao }}{{ d.intervaloAbaixoDoMinimo ? ' Intervalo abaixo do mínimo.' : '' }}</td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        </section>
      }
    </section>
  `,
})
export class EspelhoPage implements OnInit {
  private readonly api = inject(EspelhosApi);
  readonly min = formatarMinutos;
  readonly hora = horaDe;
  competencia = competenciaDe();
  readonly espelho = signal<EspelhoMensalResponse | null>(null);
  readonly erro = signal('');

  ngOnInit() { this.carregar(); }

  carregar() {
    if (!this.competencia) return;
    this.erro.set('');
    this.api.obter(this.competencia).subscribe({
      next: (e) => this.espelho.set(e),
      error: (e) => { this.espelho.set(null); this.erro.set(mensagemDeErro(e)); },
    });
  }
}
