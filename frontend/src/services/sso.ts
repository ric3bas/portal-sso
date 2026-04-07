import { http } from '../lib/api'
import type {
  AtualizarUsuarioRequest,
  AtualizarEscopoRequest,
  AtualizarParceiroRequest,
  EscopoRequest,
  EscopoResponse,
  LoginRequest,
  LoginResponse,
  LogoutRequest,
  ParceiroRequest,
  ParceiroResponse,
  PerfilComEscopoResponse,
  PerfilResponse,
  PerfilRequest,
  RecuperarSenhaRequest,
  RefreshTokenRequest,
  RegisterRequest,
  TrocarSenhaRequest,
  TrocarSenhaResponse,
  UsuarioComPerfilResponse,
  VincularEscopoRequest,
} from '../types/api'

export const authApi = {
  async login(payload: LoginRequest) {
    const response = await http.post<LoginResponse>('/api/v1/auth/login', payload)
    return response.data
  },
  async refreshToken(payload: RefreshTokenRequest) {
    const response = await http.post<LoginResponse>('/api/v1/auth/refresh-token', payload)
    return response.data
  },
  async logout(payload: LogoutRequest) {
    await http.post('/api/v1/auth/logout', payload)
  },
  async forgotPassword(payload: RecuperarSenhaRequest) {
    const response = await http.post<string>('/api/v1/auth/esqueceu-senha', payload)
    return response.data
  },
  async forgotPasswordLogged(payload: RecuperarSenhaRequest) {
    const response = await http.post<string>('/api/v1/auth/esqueceu-senha-logado', payload)
    return response.data
  },
  async validateResetToken(token: string) {
    const response = await http.get<string>('/api/v1/auth/validar-token', {
      params: { token },
    })
    return response.data
  },
  async resetPassword(payload: TrocarSenhaRequest) {
    const response = await http.post<TrocarSenhaResponse>('/api/v1/auth/trocar-senha', payload)
    return response.data
  },
}

export const escoposApi = {
  async list() {
    const response = await http.get<EscopoResponse[]>('/api/v1/escopos')
    return response.data
  },
  async getById(id: number) {
    const response = await http.get<EscopoResponse>(`/api/v1/escopos/${id}`)
    return response.data
  },
  async create(payload: EscopoRequest) {
    const response = await http.post<string>('/api/v1/escopos', payload)
    return response.data
  },
  async update(id: number, payload: AtualizarEscopoRequest) {
    const response = await http.put<string>(`/api/v1/escopos/${id}`, payload)
    return response.data
  },
}

export const parceirosApi = {
  async list(nome?: string) {
    const response = await http.get<ParceiroResponse[]>('/api/v1/parceiros', {
      params: nome ? { nome } : undefined,
    })
    return response.data
  },
  async getById(id: string) {
    const response = await http.get<ParceiroResponse>('/api/v1/parceiros/id', {
      params: { id },
    })
    return response.data
  },
  async create(payload: ParceiroRequest) {
    const response = await http.post<string>('/api/v1/parceiros', payload)
    return response.data
  },
  async update(id: string, payload: AtualizarParceiroRequest) {
    const response = await http.patch<string>('/api/v1/parceiros/id', payload, {
      params: { id },
    })
    return response.data
  },
}

export const perfisApi = {
  async list() {
    const response = await http.get<PerfilComEscopoResponse[]>('/api/v1/perfis')
    return response.data
  },
  async listCombo() {
    const response = await http.get<PerfilResponse[]>('/api/v1/perfis/combo')
    return response.data
  },
  async getById(id: number) {
    const response = await http.get<PerfilComEscopoResponse>(`/api/v1/perfis/${id}`)
    return response.data
  },
  async create(payload: PerfilRequest) {
    const response = await http.post<number>('/api/v1/perfis', payload)
    return response.data
  },
  async updateName(id: number, payload: string) {
    await http.put(`/api/v1/perfis/${id}`, payload)
  },
  async clone(id: number) {
    const response = await http.post<number>(`/api/v1/perfis/${id}/clonar`)
    return response.data
  },
  async delete(id: number) {
    await http.delete(`/api/v1/perfis/${id}`)
  },
  async vincularEscopos(id: number, payload: VincularEscopoRequest) {
    await http.post(`/api/v1/perfis/vincular/${id}/escopos`, payload)
  },
}

export const UsuariosApi = {
  async list(parceiroId?: string) {
    const response = await http.get<UsuarioComPerfilResponse[]>('/api/v1/usuarios', {
      params: parceiroId ? { parceiroId } : undefined,
    })
    return response.data
  },
  async create(payload: RegisterRequest) {
    await http.post('/api/v1/usuarios', payload)
  },
  async update(id: number, payload: AtualizarUsuarioRequest) {
    await http.put(`/api/v1/usuarios/${id}`, payload)
  },
}