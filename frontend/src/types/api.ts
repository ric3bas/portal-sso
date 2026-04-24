export interface CustomProblemDetails {
  errors?: string[] | null
  traceId?: string | null
}

export interface LoginRequest {
  login: string
  senha: string
}

export interface LoginResponse {
  access_token?: string | null
  accessToken?: string | null
  refresh_token?: string | null
  refreshToken?: string | null
  expire_in_minutes?: string | null
  expireInMinutes?: string | null
  token?: string | null
}

export interface RefreshTokenRequest {
  refreshToken: string
}

export interface LogoutRequest {
  refreshToken: string
}

export interface RecuperarSenhaRequest {
  login: string
}

export interface TrocarSenhaRequest {
  token: string
  novaSenha: string
  confirmarSenha: string
}

export interface TrocarSenhaResponse {
  mensagem?: string | null
}

export interface EscopoRequest {
  nome: string
}

export interface AtualizarEscopoRequest {
  nome: string
}

export interface EscopoResponse {
  id: number
  nome?: string | null
}

export interface CategoriaRequest {
  nome: string
}

export interface AtualizarCategoriaRequest {
  nome: string
  ativo: boolean
}

export interface CategoriaResponse {
  id: string
  nome?: string | null
  ativo: boolean
}

export interface ParceiroRequest {
  nome: string
  descricao: string
  corPrimaria: string
  corSecundaria: string
}

export interface AtualizarParceiroRequest {
  nome: string
  descricao: string
  corPrimaria: string
  corSecundaria: string
  ativo: boolean
}

export interface ParceiroResponse {
  id: string
  nome?: string | null
  descricao?: string | null
  corPrimaria?: string | null
  corSecundaria?: string | null
  ativo: boolean
}

export interface TelefoneRequest {
  id?: string
  ddd: string
  numero: string
}

export interface TelefoneResponse {
  id: string
  ddd?: string | null
  numero?: string | null
}

export interface EnderecoRequest {
  id?: string
  logradouro: string
  cidade: string
  estado: string
  numero: string
  complemento?: string
}

export interface EnderecoResponse {
  id: string
  logradouro?: string | null
  cidade?: string | null
  estado?: string | null
  numero?: string | null
  complemento?: string | null
}

export interface ClienteRequest {
  nome: string
  cpf: string
  email: string
  observacao: string
  telefones?: TelefoneRequest[]
  enderecos?: EnderecoRequest[]
}

export interface AtualizarClienteRequest {
  id?: string
  nome: string
  cpf: string
  email: string
  observacao: string
  bloqueado: boolean
  ativo: boolean
  telefones?: TelefoneRequest[]
  enderecos?: EnderecoRequest[]
}

export interface ClienteResponse {
  id: string
  nome?: string | null
  cpf?: string | null
  email?: string | null
  observacao?: string | null
  bloqueado: boolean
  ativo: boolean
  telefones?: TelefoneResponse[] | null
  enderecos?: EnderecoResponse[] | null
}

export interface EquipamentoRequest {
  nome: string
  categoriaId: string
  quantidadeEstoque: number
  precoDiaria: number
  marca: string
  modelo: string
  numeroSerie: string
  anoFabricacao: number
  descricao: string
  observacaoInternas: string
}

export interface AtualizarEquipamentoRequest {
  id?: string
  nome: string
  categoriaId: string
  quantidadeEstoque: number
  precoDiaria: number
  marca: string
  modelo: string
  numeroSerie: string
  anoFabricacao: number
  descricao: string
  observacaoInternas: string
  ativo: boolean
}

export interface EquipamentoResponse {
  id: string
  nome?: string | null
  categoriaId: string
  categoriaNome?: string | null
  quantidadeEstoque: number
  precoDiaria: number
  marca?: string | null
  modelo?: string | null
  numeroSerie?: string | null
  anoFabricacao: number
  descricao?: string | null
  observacaoInternas?: string | null
  ativo: boolean
}

export interface FinanceiroResponse {
  id: string
  locacaoId: string
  clienteId: string
  clienteNome?: string | null
  equipamentoId: string
  equipamentoNome?: string | null
  dataRetirada: string
  dataDevolucao: string
  diasLocados: number
  valorDiaria: number
  valorTotal: number
  dataLancamento: string
}

export type StatusLocacao = 1 | 2 | 3 | 4

export interface LocacaoRequest {
  clienteId: string
  equipamentoId: string
  dataRetirada: string
  previsaoDevolucao: string
  valorDiaria: number
  observacao: string
}

export interface AtualizarLocacaoRequest {
  id?: string
  clienteId: string
  equipamentoId: string
  dataRetirada: string
  previsaoDevolucao: string
  valorDiaria: number
  observacao: string
}

export interface DevolverLocacaoRequest {
  id?: string
  dataDevolucao: string
  observacao?: string
}

export interface LocacaoResponse {
  id: string
  clienteId: string
  clienteNome?: string | null
  equipamentoId: string
  equipamentoNome?: string | null
  status: StatusLocacao
  statusDescricao?: string | null
  dataRetirada: string
  previsaoDevolucao: string
  dataDevolucaoReal?: string | null
  valorDiaria: number
  valorTotal?: number | null
  diasLocados?: number | null
  observacao?: string | null
}

export interface PerfilRequest {
  nome: string
}

export interface PerfilEscopoItemResponse {
  id: number
  nome?: string | null
}

export interface PerfilComEscopoResponse {
  id: number
  nome?: string | null
  escopos?: PerfilEscopoItemResponse[] | null
}

export interface PerfilResponse {
  id: number
  nome?: string | null
}

export interface VincularEscopoRequest {
  escopoIds: number[]
}

export interface RegisterRequest {
  nome: string
  email: string
  login: string
  senha: string
  perfilId?: number
  perfil?: number
  parceiroId?: string
  ativo?: boolean
  bloqueado?: boolean
}

export interface AtualizarUsuarioRequest {
  Nome: string
  Login: string
  Email: string
  Ativo: boolean
  Bloqueado: boolean
}

export interface UsuarioComPerfilResponse {
  id: number
  nome?: string | null
  login?: string | null
  email?: string | null
  parceiroId?: string | null
  parceiro?: string | null
  perfilId?: number | null
  perfil?: number | string | null
  ativo?: boolean
  bloqueado?: boolean
}

export interface ClientesFilterParams {
  Nome?: string
  Cpf?: string
}

export interface EquipamentosFilterParams {
  Nome?: string
  Marca?: string
  Modelo?: string
  CategoriaId?: string
  Ativo?: boolean
}

export interface FinanceiroPeriodoParams {
  dataInicio?: string
  dataFim?: string
}

export interface CollectionQueryParams {
  Direcao?: 'asc' | 'desc'
  Pagina?: number
  TamanhoPagina?: number
}

export interface PaginatedResult<T> {
  items: T[]
  pagination: {
    page: number
    pageSize: number
    totalRecords: number
    totalPages: number
  }
}

export interface LocacoesFilterParams {
  ClienteId?: string
  EquipamentoId?: string
  Status?: StatusLocacao
  DataRetiradaInicio?: string
  DataRetiradaFim?: string
}

export interface SessionData {
  accessToken: string
  refreshToken?: string
  expiresInMinutes?: string | null
  login?: string
  isMaster: boolean
  corPrimaria?: string
  corSecundaria?: string
}

export interface SelectOption {
  label: string
  value: string
}