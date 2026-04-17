import { http } from '../lib/api'
import type {
  AtualizarCategoriaRequest,
  AtualizarClienteRequest,
  AtualizarEquipamentoRequest,
  AtualizarLocacaoRequest,
  AtualizarUsuarioRequest,
  AtualizarEscopoRequest,
  AtualizarParceiroRequest,
  CategoriaRequest,
  CategoriaResponse,
  ClienteRequest,
  ClienteResponse,
  ClientesFilterParams,
  DevolverLocacaoRequest,
  EscopoRequest,
  EscopoResponse,
  EquipamentoRequest,
  EquipamentoResponse,
  EquipamentosFilterParams,
  FinanceiroPeriodoParams,
  FinanceiroResponse,
  LoginRequest,
  LoginResponse,
  LocacaoRequest,
  LocacaoResponse,
  LocacoesFilterParams,
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

function buildQueryParams(params: object) {
  const entries = Object.entries(params).filter(([, value]) => value !== undefined && value !== null && value !== '')

  return entries.length > 0 ? Object.fromEntries(entries) : undefined
}

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

export const categoriasApi = {
  async list() {
    const response = await http.get<CategoriaResponse[]>('/api/v1/categorias')
    return response.data
  },
  async listFilter(nome?: string) {
    const response = await http.get<CategoriaResponse[]>('/api/v1/categorias/filtro', {
      params: buildQueryParams({ nome }),
    })
    return response.data
  },
  async getById(id: string) {
    const response = await http.get<CategoriaResponse>('/api/v1/categorias/id', {
      params: { id },
    })
    return response.data
  },
  async create(payload: CategoriaRequest) {
    const response = await http.post<string>('/api/v1/categorias', payload)
    return response.data
  },
  async update(id: string, payload: AtualizarCategoriaRequest) {
    const response = await http.patch<string>('/api/v1/categorias/id', payload, {
      params: { id },
    })
    return response.data
  },
  async inactivate(id: string) {
    const response = await http.patch<string>(`/api/v1/categorias/${id}/inativar`)
    return response.data
  },
}

export const parceirosApi = {
  async listFilter(nome?: string) {
    const response = await http.get<ParceiroResponse[]>('/api/v1/parceiros/filtro', {
      params: nome ? { nome } : undefined,
    })
    return response.data
  },
  async list() {
    const response = await http.get<ParceiroResponse[]>('/api/v1/parceiros')
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

export const clientesApi = {
  async list() {
    const response = await http.get<ClienteResponse[]>('/api/v1/clientes')
    return response.data
  },
  async listFilter(filters: ClientesFilterParams) {
    const response = await http.get<ClienteResponse[]>('/api/v1/clientes/filtro', {
      params: buildQueryParams(filters),
    })
    return response.data
  },
  async getById(id: string) {
    const response = await http.get<ClienteResponse>('/api/v1/clientes/id', {
      params: { id },
    })
    return response.data
  },
  async create(payload: ClienteRequest) {
    const response = await http.post<string>('/api/v1/clientes', payload)
    return response.data
  },
  async update(id: string, payload: AtualizarClienteRequest) {
    const response = await http.patch<string>('/api/v1/clientes/id', payload, {
      params: { id },
    })
    return response.data
  },
  async block(id: string) {
    const response = await http.patch<string>(`/api/v1/clientes/${id}/bloquear`)
    return response.data
  },
  async unblock(id: string) {
    const response = await http.patch<string>(`/api/v1/clientes/${id}/desbloquear`)
    return response.data
  },
  async inactivate(id: string) {
    const response = await http.patch<string>(`/api/v1/clientes/${id}/inativar`)
    return response.data
  },
}

export const equipamentosApi = {
  async list() {
    const response = await http.get<EquipamentoResponse[]>('/api/v1/equipamentos')
    return response.data
  },
  async listFilter(filters: EquipamentosFilterParams) {
    const response = await http.get<EquipamentoResponse[]>('/api/v1/equipamentos/filtro', {
      params: buildQueryParams(filters),
    })
    return response.data
  },
  async getById(id: string) {
    const response = await http.get<EquipamentoResponse>('/api/v1/equipamentos/id', {
      params: { id },
    })
    return response.data
  },
  async create(payload: EquipamentoRequest) {
    const response = await http.post<string>('/api/v1/equipamentos', payload)
    return response.data
  },
  async update(id: string, payload: AtualizarEquipamentoRequest) {
    const response = await http.patch<string>('/api/v1/equipamentos/id', payload, {
      params: { id },
    })
    return response.data
  },
  async inactivate(id: string) {
    const response = await http.patch<string>(`/api/v1/equipamentos/${id}/inativar`)
    return response.data
  },
}

export const financeiroApi = {
  async list() {
    const response = await http.get<FinanceiroResponse[]>('/api/v1/financeiro')
    return response.data
  },
  async listByPeriod(filters: FinanceiroPeriodoParams) {
    const response = await http.get<FinanceiroResponse[]>('/api/v1/financeiro/periodo', {
      params: buildQueryParams(filters),
    })
    return response.data
  },
}

export const locacoesApi = {
  async list() {
    const response = await http.get<LocacaoResponse[]>('/api/v1/locacoes')
    return response.data
  },
  async listFilter(filters: LocacoesFilterParams) {
    const response = await http.get<LocacaoResponse[]>('/api/v1/locacoes/filtro', {
      params: buildQueryParams(filters),
    })
    return response.data
  },
  async listLate() {
    const response = await http.get<LocacaoResponse[]>('/api/v1/locacoes/atrasadas')
    return response.data
  },
  async getById(id: string) {
    const response = await http.get<LocacaoResponse>('/api/v1/locacoes/id', {
      params: { id },
    })
    return response.data
  },
  async create(payload: LocacaoRequest) {
    const response = await http.post<string>('/api/v1/locacoes', payload)
    return response.data
  },
  async update(id: string, payload: AtualizarLocacaoRequest) {
    const response = await http.patch<string>('/api/v1/locacoes/id', payload, {
      params: { id },
    })
    return response.data
  },
  async returnRental(id: string, payload: DevolverLocacaoRequest) {
    const response = await http.patch<string>(`/api/v1/locacoes/${id}/devolver`, payload)
    return response.data
  },
  async cancel(id: string) {
    const response = await http.patch<string>(`/api/v1/locacoes/${id}/cancelar`)
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
    await http.patch(`/api/v1/usuarios/${id}`, payload)
  },
}