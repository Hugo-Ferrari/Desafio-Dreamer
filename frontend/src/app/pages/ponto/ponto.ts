import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { EuApi } from '../../core/api/eu.api';
import { MarcacoesApi } from '../../core/api/marcacoes.api';
import { MarcacaoResponse, ModalidadeTrabalho, StatusJornadaResponse, TipoMarcacao } from '../../core/api/models';
import { dataIso, formatarMinutos, horaDe, mensagemDeErro } from '../../core/api/utils';

const ROTULOS: Record<TipoMarcacao, string> = {
  Entrada: 'Registrar entrada',
  InicioIntervalo: 'Iniciar intervalo',
  FimIntervalo: 'Voltar do intervalo',
  Saida: 'Registrar saída',
};
const ESTADOS = { ForaDeJornada: 'Fora de jornada', Trabalhando: 'Trabalhando', EmIntervalo: 'Em intervalo' };

@Component({
  selector: 'app-ponto',
  imports: [FormsModule],
  template: `
    <section class="page-shell">
      <header class="page-header">
        <div>
          <p class="page-kicker">Painel</p>
          <h2>Registro de ponto</h2>
          <p>Acompanhe sua jornada e registre a próxima etapa do dia.</p>
        </div>
      </header>

      @if (status(); as s) {
        <div class="info-banner" role="status">
          <strong>{{ estados[s.estado] }}</strong>
          <p>Trabalhado na jornada: {{ minutos(s.minutosTrabalhadosNaJornada) }}</p>
        </div>

        <section class="panel">
          <h3>Registrar uma marcação</h3>
          <p class="panel-hint">As opções disponíveis acompanham a sequência da sua jornada.</p>
          <div class="field-grid">
            <label>
              Modalidade
              <select [(ngModel)]="modalidade" name="modalidade">
                <option value="Escritorio">Escritório</option>
                <option value="Remoto">Remoto</option>
                <option value="Viagem">Viagem</option>
              </select>
            </label>
          </div>

          <div class="action-row action-row-spaced">
            @for (tipo of s.proximasMarcacoesPermitidas; track tipo) {
              <button type="button" [disabled]="enviando()" (click)="registrar(tipo)">{{ rotulos[tipo] }}</button>
            }
          </div>
        </section>
      }

      @if (erro()) { <p class="erro" role="alert">{{ erro() }}</p> }
      @if (sucesso()) { <p class="ok" role="status">{{ sucesso() }}</p> }

      <section class="panel">
        <h3>Marcações de hoje</h3>
        <div class="table-panel">
          <table>
            <caption class="sr-only">Marcações registradas hoje</caption>
            <thead><tr><th>Hora</th><th>Tipo</th><th>Modalidade</th><th>Fuso</th></tr></thead>
            <tbody>
              @for (m of hoje(); track m.id) {
                <tr [class.anulada]="m.anulada">
                  <td>{{ hora(m.ocorridoEmLocal) }}</td><td>{{ m.tipo }}{{ m.anulada ? ' (anulada)' : '' }}</td>
                  <td>{{ m.modalidadeTrabalho }}</td><td>{{ m.fusoHorarioId }}</td>
                </tr>
              } @empty { <tr><td colspan="4" class="empty-state">Nenhuma marcação hoje.</td></tr> }
            </tbody>
          </table>
        </div>
      </section>
    </section>
  `,
})
export class PontoPage implements OnInit {
  private readonly eu = inject(EuApi);
  private readonly marcacoes = inject(MarcacoesApi);
  readonly rotulos = ROTULOS;
  readonly estados = ESTADOS;
  readonly minutos = formatarMinutos;
  readonly hora = horaDe;
  modalidade: ModalidadeTrabalho = 'Escritorio';
  readonly status = signal<StatusJornadaResponse | null>(null);
  readonly hoje = signal<MarcacaoResponse[]>([]);
  readonly enviando = signal(false);
  readonly erro = signal('');
  readonly sucesso = signal('');

  ngOnInit() { this.carregar(); }

  carregar() {
    this.eu.status().subscribe({ next: (s) => this.status.set(s), error: (e) => this.erro.set(mensagemDeErro(e)) });
    const h = dataIso(new Date());
    this.marcacoes.listar(h, h).subscribe({ next: (l) => this.hoje.set(l), error: (e) => this.erro.set(mensagemDeErro(e)) });
  }

  registrar(tipo: TipoMarcacao) {
    this.enviando.set(true);
    this.erro.set('');
    this.sucesso.set('');
    this.marcacoes.registrar(tipo, this.modalidade).subscribe({
      next: () => {
        this.sucesso.set('Marcação registrada.');
        this.enviando.set(false);
        this.carregar();
      },
      error: (e) => {
        this.erro.set(mensagemDeErro(e));
        this.enviando.set(false);
        this.carregar();
      },
    });
  }
}
