import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AdminApi } from '../../core/api/admin.api';
import {
  FuncionarioResponse,
  OpcoesCadastroFuncionarioResponse,
  PerfilCadastroFuncionario,
} from '../../core/api/models';
import { baixarArquivo, competenciaDe, mensagemDeErro } from '../../core/api/utils';

@Component({
  selector: 'app-admin',
  imports: [FormsModule],
  template: `
    <section class="page-shell">
      <header class="page-header">
        <div>
          <p class="page-kicker">RH</p>
          <h2>Administração</h2>
          <p>Consulte funcionários, feche períodos e gere relatórios mensais. O cadastro de novos acessos é opcional.</p>
        </div>
      </header>

      @if (erro()) { <p class="erro" role="alert">{{ erro() }}</p> }
      @if (ok()) { <p class="ok" role="status">{{ ok() }}</p> }

      <section class="panel">
        <div class="action-row action-row-spaced">
          <div>
            <h3>Cadastro de funcionário</h3>
            <p class="panel-hint">Use esta opção somente quando precisar adicionar um novo acesso ao sistema.</p>
          </div>
          <button type="button" class="secundario" (click)="alternarCadastro()" [attr.aria-expanded]="cadastroAberto()">
            {{ cadastroAberto() ? 'Cancelar cadastro' : 'Cadastrar funcionário' }}
          </button>
        </div>
        @if (cadastroAberto()) {
          <p class="panel-hint">O acesso é criado como colaborador ou gestor. A senha inicial deve ser entregue por um canal seguro.</p>
          <form ngNativeValidate (ngSubmit)="criarFuncionario()">
            @if (carregandoOpcoes()) {
              <p class="panel-hint" role="status">Carregando países, jornadas e gestores…</p>
            } @else if (opcoesCadastro(); as opcoes) {
              @if (opcoes.paises.length && opcoes.jornadasContratuais.length) {
                <div class="field-grid">
                  <label>
                    Nome completo
                    <input [(ngModel)]="nomeNovo" name="nomeNovo" required maxlength="150" autocomplete="name" />
                  </label>
                  <label>
                    E-mail de acesso
                    <input [(ngModel)]="emailNovo" name="emailNovo" type="email" required maxlength="254" autocomplete="email" />
                  </label>
                  <label>
                    Perfil
                    <select [(ngModel)]="perfilNovo" name="perfilNovo" required>
                      <option value="Colaborador">Colaborador</option>
                      <option value="Gestor">Gestor</option>
                    </select>
                  </label>
                  <label>
                    País do contrato
                    <select
                      [(ngModel)]="codigoPaisNovo"
                      (ngModelChange)="atualizarFuso($event)"
                      name="codigoPaisNovo"
                      required
                    >
                      <option [ngValue]="''" disabled>Selecione um país</option>
                      @for (pais of opcoes.paises; track pais.codigo) {
                        <option [ngValue]="pais.codigo">{{ pais.nome }} ({{ pais.codigo }})</option>
                      }
                    </select>
                  </label>
                  <label>
                    Fuso horário padrão (IANA)
                    <input [(ngModel)]="fusoNovo" name="fusoNovo" required maxlength="100" placeholder="America/Sao_Paulo" />
                  </label>
                  <label>
                    Jornada contratual
                    <select [(ngModel)]="jornadaNovo" name="jornadaNovo" required>
                      <option [ngValue]="''" disabled>Selecione uma jornada</option>
                      @for (jornada of opcoes.jornadasContratuais; track jornada.id) {
                        <option [ngValue]="jornada.id">
                          {{ jornada.nome }} — {{ jornada.minutosDiarios }} min/dia, {{ jornada.minutosSemanais }} min/semana
                        </option>
                      }
                    </select>
                  </label>
                  <label>
                    Gestor responsável (opcional)
                    <select [(ngModel)]="gestorNovo" name="gestorNovo">
                      <option [ngValue]="''">Sem gestor associado</option>
                      @for (gestor of opcoes.gestores; track gestor.id) {
                        <option [ngValue]="gestor.id">{{ gestor.nome }} ({{ gestor.perfil }})</option>
                      }
                    </select>
                  </label>
                  <label>
                    Senha inicial (mínimo de 8 caracteres)
                    <input
                      [(ngModel)]="senhaNova"
                      name="senhaNova"
                      type="password"
                      required
                      minlength="8"
                      maxlength="128"
                      autocomplete="new-password"
                    />
                  </label>
                </div>
                <div class="action-row action-row-spaced">
                  <button type="submit" [disabled]="salvando() || !cadastroDisponivel()">
                    {{ salvando() ? 'Cadastrando…' : 'Salvar funcionário' }}
                  </button>
                </div>
              } @else {
                @if (!opcoes.paises.length) {
                  <p class="panel-hint">Nenhum país cadastrado está disponível para seleção.</p>
                }
                @if (!opcoes.jornadasContratuais.length) {
                  <p class="panel-hint">Nenhuma jornada contratual cadastrada está disponível para seleção.</p>
                }
                <button type="button" class="secundario" (click)="carregarOpcoesCadastro()">
                  Tentar carregar opções novamente
                </button>
              }
            } @else {
              <button type="button" class="secundario" (click)="carregarOpcoesCadastro()">
                Tentar carregar opções novamente
              </button>
            }
          </form>
        }
      </section>

      <section class="panel">
        <h3>Fechamento e relatório</h3>
        <p class="panel-hint">Escolha o mês e o país antes de executar uma ação.</p>
        <div class="field-grid">
          <label>
            Mês
            <input type="month" [(ngModel)]="competencia" name="competencia" />
          </label>
          <label>
            País (código)
            <input [(ngModel)]="pais" name="pais" maxlength="2" placeholder="BR" />
          </label>
        </div>
        <div class="action-row action-row-spaced">
          <button type="button" class="perigo" (click)="fechar()">Fechar período</button>
          <button type="button" class="secundario" (click)="baixar()">Baixar relatório CSV</button>
        </div>
      </section>

      <section class="panel">
        <h3>Funcionários</h3>
        <div class="table-panel">
          <table>
            <caption class="sr-only">Funcionários cadastrados</caption>
            <thead><tr><th>Nome</th><th>E-mail</th><th>Perfil</th><th>País</th><th>Jornada</th><th>Gestor</th><th>Ativo</th></tr></thead>
            <tbody>
              @for (f of funcionarios(); track f.id) {
                <tr>
                  <td>{{ f.nome }}</td><td>{{ f.email }}</td><td>{{ f.perfil }}</td><td>{{ f.codigoPais }}</td>
                  <td>{{ f.jornadaContratual.nome }}</td><td>{{ f.nomeGestor }}</td><td>{{ f.ativo ? 'Sim' : 'Não' }}</td>
                </tr>
              } @empty {
                <tr><td colspan="7" class="empty-state">Nenhum funcionário encontrado.</td></tr>
              }
            </tbody>
          </table>
        </div>
      </section>
    </section>
  `,
})
export class AdminPage implements OnInit {
  private readonly api = inject(AdminApi);
  competencia = competenciaDe();
  pais = 'BR';
  readonly cadastroAberto = signal(false);
  nomeNovo = '';
  emailNovo = '';
  senhaNova = '';
  perfilNovo: PerfilCadastroFuncionario = 'Colaborador';
  codigoPaisNovo = '';
  fusoNovo = '';
  jornadaNovo = '';
  gestorNovo = '';
  readonly funcionarios = signal<FuncionarioResponse[]>([]);
  readonly opcoesCadastro = signal<OpcoesCadastroFuncionarioResponse | null>(null);
  readonly erro = signal('');
  readonly ok = signal('');
  readonly salvando = signal(false);
  readonly carregandoOpcoes = signal(false);
  readonly cadastroDisponivel = computed(() => {
    const opcoes = this.opcoesCadastro();
    return !!opcoes && opcoes.paises.length > 0 && opcoes.jornadasContratuais.length > 0;
  });

  ngOnInit() {
    this.api.funcionarios().subscribe({ next: (l) => this.funcionarios.set(l), error: (e) => this.erro.set(mensagemDeErro(e)) });
  }

  alternarCadastro() {
    const abrir = !this.cadastroAberto();
    this.cadastroAberto.set(abrir);
    if (abrir && !this.opcoesCadastro() && !this.carregandoOpcoes()) {
      this.carregarOpcoesCadastro();
    }
  }

  carregarOpcoesCadastro() {
    this.erro.set('');
    this.carregandoOpcoes.set(true);
    this.api.opcoesCadastroFuncionario().subscribe({
      next: (opcoes) => {
        this.opcoesCadastro.set(opcoes);
        this.carregandoOpcoes.set(false);
        this.erro.set('');
        const pais = opcoes.paises.find((item) => item.codigo === 'BR') ?? opcoes.paises[0];
        this.codigoPaisNovo = pais?.codigo ?? '';
        this.fusoNovo = pais?.fusoHorarioPadrao ?? '';
        this.jornadaNovo = opcoes.jornadasContratuais[0]?.id ?? '';
      },
      error: (e) => {
        this.carregandoOpcoes.set(false);
        this.erro.set(mensagemDeErro(e));
      },
    });
  }

  atualizarFuso(codigoPais: string) {
    const pais = this.opcoesCadastro()?.paises.find((item) => item.codigo === codigoPais);
    if (pais) this.fusoNovo = pais.fusoHorarioPadrao;
  }

  criarFuncionario() {
    if (this.salvando() || !this.cadastroDisponivel()) return;

    this.erro.set('');
    this.ok.set('');
    this.salvando.set(true);
    this.api.criarFuncionario({
      nome: this.nomeNovo.trim(),
      email: this.emailNovo.trim(),
      senhaInicial: this.senhaNova,
      perfil: this.perfilNovo,
      codigoPais: this.codigoPaisNovo.trim().toUpperCase(),
      fusoHorarioPadrao: this.fusoNovo.trim(),
      jornadaContratualId: this.jornadaNovo,
      gestorId: this.gestorNovo || null,
    }).subscribe({
      next: (funcionario) => {
        this.funcionarios.update((lista) =>
          [...lista, funcionario].sort((a, b) => a.nome.localeCompare(b.nome)),
        );
        this.nomeNovo = '';
        this.emailNovo = '';
        this.senhaNova = '';
        this.perfilNovo = 'Colaborador';
        this.gestorNovo = '';
        this.salvando.set(false);
        this.erro.set('');
        this.ok.set('Funcionário cadastrado. Entregue a senha inicial por um canal seguro.');
      },
      error: (e) => {
        this.salvando.set(false);
        this.erro.set(mensagemDeErro(e));
      },
    });
  }

  fechar() {
    this.erro.set(''); this.ok.set('');
    if (!confirm(`Fechar ${this.competencia} para ${this.pais}? Depois disso não entram mais marcações nem ajustes.`)) return;
    this.api.fecharPeriodo(this.competencia, this.pais.toUpperCase()).subscribe({
      next: () => this.ok.set('Período fechado.'),
      error: (e) => this.erro.set(mensagemDeErro(e)),
    });
  }

  baixar() {
    this.erro.set(''); this.ok.set('');
    this.api.relatorioCsv(this.competencia, this.pais ? this.pais.toUpperCase() : undefined).subscribe({
      next: (blob) => baixarArquivo(blob, `relatorio-${this.competencia}.csv`),
      error: (e) => this.erro.set('Não foi possível gerar o relatório.' + (e?.status ? ` (erro ${e.status})` : '')),
    });
  }
}
