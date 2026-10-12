import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AjustesApi } from '../../core/api/ajustes.api';
import { MarcacoesApi } from '../../core/api/marcacoes.api';
import { MarcacaoResponse, SolicitacaoAjusteResponse, TipoMarcacao, TipoSolicitacaoAjuste } from '../../core/api/models';
import { fusoDoNavegador, horaDe, mensagemDeErro } from '../../core/api/utils';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-ajustes',
  imports: [FormsModule],
  template: `
    <section class="page-shell">
      <header class="page-header">
        <div>
          <p class="page-kicker">Solicitações</p>
          <h2>Ajustes de jornada</h2>
          <p>Peça a correção de uma marcação e acompanhe a decisão do seu gestor.</p>
        </div>
      </header>

      <form class="panel" (ngSubmit)="enviar()">
        <h3>Nova solicitação</h3>
        <p class="panel-hint">Descreva o que aconteceu para que a solicitação possa ser avaliada.</p>
        <div class="field-grid">
          <label>
            Tipo
            <select [(ngModel)]="tipo" name="tipo">
              <option value="Adicionar">Adicionar marcação</option>
              <option value="Alterar">Alterar marcação</option>
              <option value="Anular">Anular marcação</option>
            </select>
          </label>

          @if (tipo !== 'Adicionar') {
            <label>
              Marcação (últimos 30 dias)
              <select [(ngModel)]="alvoId" name="alvo" required>
                <option value="">Selecione…</option>
                @for (m of marcacoes(); track m.id) {
                  <option [value]="m.id">{{ m.dataJornadaLocal }} {{ hora(m.ocorridoEmLocal) }} {{ m.tipo }}</option>
                }
              </select>
            </label>
          }

          @if (tipo === 'Adicionar') {
            <label>
              Tipo da marcação
              <select [(ngModel)]="tipoProposto" name="tipoProposto">
                <option value="Entrada">Entrada</option><option value="InicioIntervalo">Início de intervalo</option>
                <option value="FimIntervalo">Fim de intervalo</option><option value="Saida">Saída</option>
              </select>
            </label>
          }

          @if (tipo !== 'Anular') {
            <label>
              Data e hora corretas
              <input type="datetime-local" [(ngModel)]="quando" name="quando" required />
            </label>
          }

          <label class="field-wide">
            Motivo
            <textarea [(ngModel)]="motivo" name="motivo" required rows="3"></textarea>
          </label>
        </div>

        @if (erro()) { <p class="erro" role="alert">{{ erro() }}</p> }
        <div class="action-row action-row-spaced">
          <button type="submit" [disabled]="enviando()">{{ enviando() ? 'Enviando…' : 'Enviar solicitação' }}</button>
        </div>
      </form>

      <section class="panel">
        <h3>Solicitações</h3>
        <div class="table-panel">
          <table>
            <caption class="sr-only">Solicitações de ajuste de jornada</caption>
            <thead><tr><th>Criada em</th><th>Funcionário</th><th>Tipo</th><th>Motivo</th><th>Status</th><th>Comentário</th><th></th></tr></thead>
            <tbody>
              @for (s of solicitacoes(); track s.id) {
                <tr>
                  <td>{{ s.criadaEmUtc.substring(0, 10) }}</td><td>{{ s.nomeFuncionario }}</td><td>{{ s.tipoSolicitacao }}</td>
                  <td>{{ s.motivo }}</td>                  <td><span class="chip" [class.tom-ok]="s.status === 'Aprovada'" [class.tom-alerta]="s.status === 'Pendente'" [class.tom-erro]="s.status === 'Rejeitada'">{{ s.status }}</span></td><td>{{ s.comentarioAvaliacao }}</td>
                  <td>
                    @if (auth.ehGestorOuRh() && s.status === 'Pendente') {
                      <div style="display:grid; gap:8px; min-width: 220px;">
                        <input placeholder="Comentário" [(ngModel)]="comentarios[s.id]" [name]="'c' + s.id" />
                        <div class="action-row">
                          <button type="button" class="pequeno" (click)="avaliar(s, true)">Aprovar</button>
                          <button type="button" class="pequeno secundario" (click)="avaliar(s, false)">Rejeitar</button>
                        </div>
                      </div>
                    }
                  </td>
                </tr>
              } @empty { <tr><td colspan="7" class="empty-state">Nenhuma solicitação.</td></tr> }
            </tbody>
          </table>
        </div>
      </section>
    </section>
  `,
})
export class AjustesPage implements OnInit {
  private readonly api = inject(AjustesApi);
  private readonly marcacoesApi = inject(MarcacoesApi);
  readonly auth = inject(AuthService);
  readonly hora = horaDe;
  tipo: TipoSolicitacaoAjuste = 'Adicionar';
  tipoProposto: TipoMarcacao = 'Entrada';
  alvoId = '';
  quando = '';
  motivo = '';
  comentarios: Record<string, string> = {};
  readonly marcacoes = signal<MarcacaoResponse[]>([]);
  readonly solicitacoes = signal<SolicitacaoAjusteResponse[]>([]);
  readonly enviando = signal(false);
  readonly erro = signal('');

  ngOnInit() {
    this.marcacoesApi.listar().subscribe((l) => this.marcacoes.set(l.filter((m) => !m.anulada)));
    this.recarregar();
  }

  recarregar() {
    this.api.listar().subscribe({ next: (l) => this.solicitacoes.set(l), error: (e) => this.erro.set(mensagemDeErro(e)) });
  }

  enviar() {
    this.erro.set('');
    this.enviando.set(true);
    const instante = this.quando ? new Date(this.quando).toISOString() : null;
    this.api
      .criar({
        tipoSolicitacao: this.tipo,
        motivo: this.motivo,
        marcacaoAlvoId: this.tipo === 'Adicionar' ? null : this.alvoId || null,
        tipoMarcacaoProposto: this.tipo === 'Adicionar' ? this.tipoProposto : null,
        ocorridoEmProposto: this.tipo === 'Anular' ? null : instante,
        fusoHorarioProposto: this.tipo === 'Anular' ? null : fusoDoNavegador(),
      })
      .subscribe({
        next: () => { this.enviando.set(false); this.motivo = ''; this.quando = ''; this.recarregar(); },
        error: (e) => { this.enviando.set(false); this.erro.set(mensagemDeErro(e)); },
      });
  }

  avaliar(s: SolicitacaoAjusteResponse, aprovar: boolean) {
    this.erro.set('');
    const comentario = this.comentarios[s.id];
    const req = aprovar ? this.api.aprovar(s.id, comentario) : this.api.rejeitar(s.id, comentario);
    req.subscribe({ next: () => this.recarregar(), error: (e) => this.erro.set(mensagemDeErro(e)) });
  }
}
