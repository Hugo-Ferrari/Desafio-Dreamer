

export type DataIso = string; 
export type InstanteIso = string; 

export type PerfilFuncionario = 'Colaborador' | 'Gestor' | 'RH';
export type TipoMarcacao = 'Entrada' | 'InicioIntervalo' | 'FimIntervalo' | 'Saida';
export type ModalidadeTrabalho = 'Escritorio' | 'Remoto' | 'Viagem';
export type OrigemRegistro = 'Web' | 'Celular' | 'Offline' | 'Ajuste';
export type EstadoJornada = 'ForaDeJornada' | 'Trabalhando' | 'EmIntervalo';
export type SituacaoDia =
  | 'Regular'
  | 'Incompleto'
  | 'Falta'
  | 'Feriado'
  | 'Ausencia'
  | 'FimDeSemana'
  | 'Pendente';
export type TipoSolicitacaoAjuste = 'Adicionar' | 'Alterar' | 'Anular';
export type StatusSolicitacaoAjuste = 'Pendente' | 'Aprovada' | 'Rejeitada';
export type StatusFechamento = 'Aberto' | 'Fechado';


export interface LoginRequest {
  email: string;
  senha: string;
}

export interface TokensResponse {
  tokenAcesso: string;
  expiraEmUtc: InstanteIso;
  tokenRenovacao: string;
}


export interface JornadaContratualResponse {
  id: string;
  nome: string;
  minutosDiarios: number;
  minutosSemanais: number;
  minutosIntervaloMinimo: number;
}

export interface FuncionarioResponse {
  id: string;
  nome: string;
  email: string;
  perfil: PerfilFuncionario;
  codigoPais: string;
  fusoHorarioPadrao: string;
  jornadaContratual: JornadaContratualResponse;
  gestorId: string | null;
  nomeGestor: string | null;
  ativo: boolean;
}

export type PerfilCadastroFuncionario = 'Colaborador' | 'Gestor';

export interface CriarFuncionarioRequest {
  nome: string;
  email: string;
  senhaInicial: string;
  perfil: PerfilCadastroFuncionario;
  codigoPais: string;
  jornadaContratualId: string;
  gestorId?: string | null;
  fusoHorarioPadrao?: string | null;
}

export interface PaisOpcaoResponse {
  codigo: string;
  nome: string;
  fusoHorarioPadrao: string;
}

export interface GestorOpcaoResponse {
  id: string;
  nome: string;
  perfil: PerfilFuncionario;
}

export interface OpcoesCadastroFuncionarioResponse {
  paises: PaisOpcaoResponse[];
  jornadasContratuais: JornadaContratualResponse[];
  gestores: GestorOpcaoResponse[];
}

export interface RegistrarMarcacaoRequest {
  tipo: TipoMarcacao;
  modalidadeTrabalho: ModalidadeTrabalho;
  origemRegistro: OrigemRegistro;
  fusoHorarioId?: string | null;

  ocorridoEm?: InstanteIso | null;
  chaveIdempotencia?: string | null;
}

export interface MarcacaoResponse {
  id: string;
  tipo: TipoMarcacao;
  ocorridoEmUtc: InstanteIso;
  ocorridoEmLocal: InstanteIso;
  registradoEmUtc: InstanteIso;
  fusoHorarioId: string;
  dataJornadaLocal: DataIso;
  modalidadeTrabalho: ModalidadeTrabalho;
  origemRegistro: OrigemRegistro;
  anulada: boolean;
  substituiMarcacaoId: string | null;
}

export interface StatusJornadaResponse {
  estado: EstadoJornada;
  dataJornadaLocal: DataIso | null;
  minutosTrabalhadosNaJornada: number;
  ultimaMarcacao: MarcacaoResponse | null;
  proximasMarcacoesPermitidas: TipoMarcacao[];
}


export interface DiaEspelhoResponse {
  data: DataIso;
  situacao: SituacaoDia;
  observacao: string | null;
  minutosEsperados: number;
  minutosTrabalhados: number;
  minutosIntervalo: number;
  saldoMinutos: number;
  intervaloAbaixoDoMinimo: boolean;
  marcacoes: MarcacaoResponse[];
}

export interface TotaisEspelhoResponse {
  minutosEsperados: number;
  minutosTrabalhados: number;
  saldoMinutos: number;
  diasIncompletos: number;
  faltas: number;
}

export interface EspelhoMensalResponse {
  funcionarioId: string;
  nomeFuncionario: string;
  codigoPais: string;
  competencia: string; 
  periodoFechado: boolean;
  totais: TotaisEspelhoResponse;
  dias: DiaEspelhoResponse[];
}


export interface CriarSolicitacaoAjusteRequest {
  tipoSolicitacao: TipoSolicitacaoAjuste;
  motivo: string;
  
  marcacaoAlvoId?: string | null;
  
  tipoMarcacaoProposto?: TipoMarcacao | null;
  
  ocorridoEmProposto?: InstanteIso | null;
  fusoHorarioProposto?: string | null;
}

export interface AvaliarSolicitacaoAjusteRequest {
  comentario?: string | null;
}

export interface SolicitacaoAjusteResponse {
  id: string;
  funcionarioId: string;
  nomeFuncionario: string;
  tipoSolicitacao: TipoSolicitacaoAjuste;
  marcacaoAlvoId: string | null;
  tipoMarcacaoProposto: TipoMarcacao | null;
  ocorridoEmUtcProposto: InstanteIso | null;
  fusoHorarioProposto: string | null;
  motivo: string;
  status: StatusSolicitacaoAjuste;
  avaliadorId: string | null;
  avaliadaEmUtc: InstanteIso | null;
  comentarioAvaliacao: string | null;
  criadaEmUtc: InstanteIso;
}


export interface MembroEquipeStatusResponse {
  funcionarioId: string;
  nome: string;
  codigoPais: string;
  estado: EstadoJornada;
  ultimaMarcacao: MarcacaoResponse | null;
}

export interface FechamentoPeriodoResponse {
  id: string;
  codigoPais: string;
  competencia: string;
  status: StatusFechamento;
  fechadoPorId: string | null;
  fechadoEmUtc: InstanteIso | null;
}


export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;

  errors?: Record<string, string[]>;
}
