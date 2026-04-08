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
  refresh_token?: string | null
  expire_in_minutes?: string | null
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

export interface ParceiroRequest {
  nome: string
  descricao: string
}

export interface AtualizarParceiroRequest {
  nome: string
  descricao: string
  ativo: boolean
}

export interface ParceiroResponse {
  id: string
  nome?: string | null
  descricao?: string | null
  ativo: boolean
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

export interface SessionData {
  accessToken: string
  refreshToken: string
  expiresInMinutes?: string | null
  login?: string
  isMaster: boolean
}

export interface SelectOption {
  label: string
  value: string
}