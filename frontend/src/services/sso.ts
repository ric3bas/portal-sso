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
  CollectionQueryParams,
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
  PaginatedResult,
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

const DEFAULT_COLLECTION_QUERY = {
  Direcao: 'asc',
  Pagina: 1,
  TamanhoPagina: 20,
} as const

function buildQueryParams(params: object) {
  const entries = Object.entries(params).filter(([, value]) => value !== undefined && value !== null && value !== '')

  return entries.length > 0 ? Object.fromEntries(entries) : undefined
}

function buildCollectionQueryParams(params?: object) {
  return buildQueryParams({
    ...DEFAULT_COLLECTION_QUERY,
    ...params,
  })
}

function toPositiveInteger(value: unknown, fallback: number) {
  const parsedValue = Number(value)

  if (!Number.isFinite(parsedValue) || parsedValue <= 0) {
    return fallback
  }

  return Math.trunc(parsedValue)
}

function extractPaginationMeta(payload: unknown, fallback: { page: number; pageSize: number; totalRecords?: number }) {
  if (!payload || typeof payload !== 'object' || Array.isArray(payload)) {
    const totalRegistros = fallback.totalRecords ?? 0
    const totalPaginas = Math.max(1, Math.ceil(totalRegistros / fallback.pageSize))

    return {
      page: fallback.page,
      pageSize: fallback.pageSize,
      totalRecords: totalRegistros,
      totalPages: totalPaginas,
    }
  }

  const record = payload as Record<string, unknown>
  const registroPaginacao =
    (record.paginacao && typeof record.paginacao === 'object' ? record.paginacao as Record<string, unknown> : null) ??
    null

  const source = registroPaginacao ?? record
  const registrosFiltrados = toPositiveInteger(
    source.registrosFiltrados ?? source.totalFiltrado,
    fallback.totalRecords ?? 0,
  )
  const totalRegistros = toPositiveInteger(
    source.registrosTotal ??
      source.totalRegistros ??
      source.total ??
      source.totalItens ??
      source.quantidadeTotal ??
      source.registrosFiltrados ??
      fallback.totalRecords,
    registrosFiltrados,
  )
  const pagina = toPositiveInteger(
    source.paginaAtual ?? source.pagina ?? source.numeroPagina,
    fallback.page,
  )
  const tamanhoPagina = toPositiveInteger(
    source.tamanhoPagina ?? source.registrosPorPagina ?? source.itensPorPagina,
    fallback.pageSize,
  )
  const totalPaginas = toPositiveInteger(
    source.totalPaginas ?? source.paginasTotais,
    Math.max(1, Math.ceil(Math.max(registrosFiltrados, totalRegistros) / tamanhoPagina)),
  )

  return {
    page: pagina,
    pageSize: tamanhoPagina,
    totalRecords: registrosFiltrados,
    totalPages: totalPaginas,
  }
}

function extractPaginatedPayload<T>(payload: unknown, fallback: { page: number; pageSize: number }): PaginatedResult<T> {
  const items = extractArrayPayload<T>(payload)

  return {
    items,
    pagination: extractPaginationMeta(payload, {
      ...fallback,
      totalRecords: items.length,
    }),
  }
}

function extractArrayPayload<T>(payload: unknown): T[] {
  if (Array.isArray(payload)) {
    return payload as T[]
  }

  if (!payload || typeof payload !== 'object') {
    return []
  }

  const record = payload as Record<string, unknown>
  const candidates = [record.data, record.dados, record.items, record.itens, record.value, record.values, record.result, record.results, record.content]

  for (const candidate of candidates) {
    if (Array.isArray(candidate)) {
      return candidate as T[]
    }
  }

  return []
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
    const response = await http.get<EscopoResponse[]>('/api/v1/escopos', {
      params: buildCollectionQueryParams(),
    })
    return extractArrayPayload<EscopoResponse>(response.data)
  },
  async listPage(query?: CollectionQueryParams) {
    const page = query?.Pagina ?? DEFAULT_COLLECTION_QUERY.Pagina
    const pageSize = query?.TamanhoPagina ?? DEFAULT_COLLECTION_QUERY.TamanhoPagina
    const response = await http.get('/api/v1/escopos', {
      params: buildCollectionQueryParams(query),
    })
    return extractPaginatedPayload<EscopoResponse>(response.data, { page, pageSize })
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
    const response = await http.get<CategoriaResponse[]>('/api/v1/categorias', {
      params: buildCollectionQueryParams(),
    })
    return extractArrayPayload<CategoriaResponse>(response.data)
  },
  async listFilter(nome?: string) {
    const response = await http.get<CategoriaResponse[]>('/api/v1/categorias/filtro', {
      params: buildCollectionQueryParams({ Nome: nome }),
    })
    return extractArrayPayload<CategoriaResponse>(response.data)
  },
  async listPage(query?: CollectionQueryParams) {
    const page = query?.Pagina ?? DEFAULT_COLLECTION_QUERY.Pagina
    const pageSize = query?.TamanhoPagina ?? DEFAULT_COLLECTION_QUERY.TamanhoPagina
    const response = await http.get('/api/v1/categorias', {
      params: buildCollectionQueryParams(query),
    })
    return extractPaginatedPayload<CategoriaResponse>(response.data, { page, pageSize })
  },
  async listFilterPage(nome?: string, query?: CollectionQueryParams) {
    const page = query?.Pagina ?? DEFAULT_COLLECTION_QUERY.Pagina
    const pageSize = query?.TamanhoPagina ?? DEFAULT_COLLECTION_QUERY.TamanhoPagina
    const response = await http.get('/api/v1/categorias/filtro', {
      params: buildCollectionQueryParams({ Nome: nome, ...query }),
    })
    return extractPaginatedPayload<CategoriaResponse>(response.data, { page, pageSize })
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
      params: buildCollectionQueryParams({ Nome: nome }),
    })
    return extractArrayPayload<ParceiroResponse>(response.data)
  },
  async list() {
    const response = await http.get<ParceiroResponse[]>('/api/v1/parceiros', {
      params: buildCollectionQueryParams(),
    })
    return extractArrayPayload<ParceiroResponse>(response.data)
  },
  async listPage(query?: CollectionQueryParams) {
    const page = query?.Pagina ?? DEFAULT_COLLECTION_QUERY.Pagina
    const pageSize = query?.TamanhoPagina ?? DEFAULT_COLLECTION_QUERY.TamanhoPagina
    const response = await http.get('/api/v1/parceiros', {
      params: buildCollectionQueryParams(query),
    })
    return extractPaginatedPayload<ParceiroResponse>(response.data, { page, pageSize })
  },
  async listFilterPage(nome?: string, query?: CollectionQueryParams) {
    const page = query?.Pagina ?? DEFAULT_COLLECTION_QUERY.Pagina
    const pageSize = query?.TamanhoPagina ?? DEFAULT_COLLECTION_QUERY.TamanhoPagina
    const response = await http.get('/api/v1/parceiros/filtro', {
      params: buildCollectionQueryParams({ Nome: nome, ...query }),
    })
    return extractPaginatedPayload<ParceiroResponse>(response.data, { page, pageSize })
  },
  async getById(id: string) {
    const response = await http.get<ParceiroResponse>(`/api/v1/parceiros/${id}`)
    return response.data
  },
  async create(payload: ParceiroRequest) {
    const response = await http.post<string>('/api/v1/parceiros', payload)
    return response.data
  },
  async update(id: string, payload: AtualizarParceiroRequest) {
    const response = await http.patch<string>(`/api/v1/parceiros/${id}`, payload)
    return response.data
  },
}

export const clientesApi = {
  async list() {
    const response = await http.get<ClienteResponse[]>('/api/v1/clientes', {
      params: buildCollectionQueryParams(),
    })
    return extractArrayPayload<ClienteResponse>(response.data)
  },
  async listFilter(filters: ClientesFilterParams) {
    const response = await http.get<ClienteResponse[]>('/api/v1/clientes/filtro', {
      params: buildCollectionQueryParams(filters),
    })
    return extractArrayPayload<ClienteResponse>(response.data)
  },
  async listPage(query?: CollectionQueryParams) {
    const page = query?.Pagina ?? DEFAULT_COLLECTION_QUERY.Pagina
    const pageSize = query?.TamanhoPagina ?? DEFAULT_COLLECTION_QUERY.TamanhoPagina
    const response = await http.get('/api/v1/clientes', {
      params: buildCollectionQueryParams(query),
    })
    return extractPaginatedPayload<ClienteResponse>(response.data, { page, pageSize })
  },
  async listFilterPage(filters: ClientesFilterParams, query?: CollectionQueryParams) {
    const page = query?.Pagina ?? DEFAULT_COLLECTION_QUERY.Pagina
    const pageSize = query?.TamanhoPagina ?? DEFAULT_COLLECTION_QUERY.TamanhoPagina
    const response = await http.get('/api/v1/clientes/filtro', {
      params: buildCollectionQueryParams({ ...filters, ...query }),
    })
    return extractPaginatedPayload<ClienteResponse>(response.data, { page, pageSize })
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
    const response = await http.get<EquipamentoResponse[]>('/api/v1/equipamentos', {
      params: buildCollectionQueryParams(),
    })
    return extractArrayPayload<EquipamentoResponse>(response.data)
  },
  async listFilter(filters: EquipamentosFilterParams) {
    const response = await http.get<EquipamentoResponse[]>('/api/v1/equipamentos/filtro', {
      params: buildCollectionQueryParams(filters),
    })
    return extractArrayPayload<EquipamentoResponse>(response.data)
  },
  async listPage(query?: CollectionQueryParams) {
    const page = query?.Pagina ?? DEFAULT_COLLECTION_QUERY.Pagina
    const pageSize = query?.TamanhoPagina ?? DEFAULT_COLLECTION_QUERY.TamanhoPagina
    const response = await http.get('/api/v1/equipamentos', {
      params: buildCollectionQueryParams(query),
    })
    return extractPaginatedPayload<EquipamentoResponse>(response.data, { page, pageSize })
  },
  async listFilterPage(filters: EquipamentosFilterParams, query?: CollectionQueryParams) {
    const page = query?.Pagina ?? DEFAULT_COLLECTION_QUERY.Pagina
    const pageSize = query?.TamanhoPagina ?? DEFAULT_COLLECTION_QUERY.TamanhoPagina
    const response = await http.get('/api/v1/equipamentos/filtro', {
      params: buildCollectionQueryParams({ ...filters, ...query }),
    })
    return extractPaginatedPayload<EquipamentoResponse>(response.data, { page, pageSize })
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
    const response = await http.get<FinanceiroResponse[]>('/api/v1/financeiro', {
      params: buildCollectionQueryParams(),
    })
    return extractArrayPayload<FinanceiroResponse>(response.data)
  },
  async listByPeriod(filters: FinanceiroPeriodoParams) {
    const response = await http.get<FinanceiroResponse[]>('/api/v1/financeiro/periodo', {
      params: buildCollectionQueryParams({
        DataInicio: filters.dataInicio,
        DataFim: filters.dataFim,
      }),
    })
    return extractArrayPayload<FinanceiroResponse>(response.data)
  },
  async listPage(query?: CollectionQueryParams) {
    const page = query?.Pagina ?? DEFAULT_COLLECTION_QUERY.Pagina
    const pageSize = query?.TamanhoPagina ?? DEFAULT_COLLECTION_QUERY.TamanhoPagina
    const response = await http.get('/api/v1/financeiro', {
      params: buildCollectionQueryParams(query),
    })
    return extractPaginatedPayload<FinanceiroResponse>(response.data, { page, pageSize })
  },
  async listByPeriodPage(filters: FinanceiroPeriodoParams, query?: CollectionQueryParams) {
    const page = query?.Pagina ?? DEFAULT_COLLECTION_QUERY.Pagina
    const pageSize = query?.TamanhoPagina ?? DEFAULT_COLLECTION_QUERY.TamanhoPagina
    const response = await http.get('/api/v1/financeiro/periodo', {
      params: buildCollectionQueryParams({
        DataInicio: filters.dataInicio,
        DataFim: filters.dataFim,
        ...query,
      }),
    })
    return extractPaginatedPayload<FinanceiroResponse>(response.data, { page, pageSize })
  },
}

export const locacoesApi = {
  async list() {
    const response = await http.get<LocacaoResponse[]>('/api/v1/locacoes', {
      params: buildCollectionQueryParams(),
    })
    return extractArrayPayload<LocacaoResponse>(response.data)
  },
  async listFilter(filters: LocacoesFilterParams) {
    const response = await http.get<LocacaoResponse[]>('/api/v1/locacoes/filtro', {
      params: buildCollectionQueryParams(filters),
    })
    return extractArrayPayload<LocacaoResponse>(response.data)
  },
  async listLate() {
    const response = await http.get<LocacaoResponse[]>('/api/v1/locacoes/atrasadas', {
      params: buildCollectionQueryParams(),
    })
    return extractArrayPayload<LocacaoResponse>(response.data)
  },
  async listPage(query?: CollectionQueryParams) {
    const page = query?.Pagina ?? DEFAULT_COLLECTION_QUERY.Pagina
    const pageSize = query?.TamanhoPagina ?? DEFAULT_COLLECTION_QUERY.TamanhoPagina
    const response = await http.get('/api/v1/locacoes', {
      params: buildCollectionQueryParams(query),
    })
    return extractPaginatedPayload<LocacaoResponse>(response.data, { page, pageSize })
  },
  async listFilterPage(filters: LocacoesFilterParams, query?: CollectionQueryParams) {
    const page = query?.Pagina ?? DEFAULT_COLLECTION_QUERY.Pagina
    const pageSize = query?.TamanhoPagina ?? DEFAULT_COLLECTION_QUERY.TamanhoPagina
    const response = await http.get('/api/v1/locacoes/filtro', {
      params: buildCollectionQueryParams({ ...filters, ...query }),
    })
    return extractPaginatedPayload<LocacaoResponse>(response.data, { page, pageSize })
  },
  async listLatePage(query?: CollectionQueryParams) {
    const page = query?.Pagina ?? DEFAULT_COLLECTION_QUERY.Pagina
    const pageSize = query?.TamanhoPagina ?? DEFAULT_COLLECTION_QUERY.TamanhoPagina
    const response = await http.get('/api/v1/locacoes/atrasadas', {
      params: buildCollectionQueryParams(query),
    })
    return extractPaginatedPayload<LocacaoResponse>(response.data, { page, pageSize })
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
    const response = await http.get<PerfilComEscopoResponse[]>('/api/v1/perfis', {
      params: buildCollectionQueryParams(),
    })
    return extractArrayPayload<PerfilComEscopoResponse>(response.data)
  },
  async listPage(query?: CollectionQueryParams) {
    const page = query?.Pagina ?? DEFAULT_COLLECTION_QUERY.Pagina
    const pageSize = query?.TamanhoPagina ?? DEFAULT_COLLECTION_QUERY.TamanhoPagina
    const response = await http.get('/api/v1/perfis', {
      params: buildCollectionQueryParams(query),
    })
    return extractPaginatedPayload<PerfilComEscopoResponse>(response.data, { page, pageSize })
  },
  async listCombo() {
    const response = await http.get<PerfilResponse[]>('/api/v1/perfis/combo', {
      params: buildCollectionQueryParams(),
    })
    return extractArrayPayload<PerfilResponse>(response.data)
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
      params: buildCollectionQueryParams({ ParceiroId: parceiroId }),
    })
    return extractArrayPayload<UsuarioComPerfilResponse>(response.data)
  },
  async listPage(parceiroId?: string, query?: CollectionQueryParams) {
    const page = query?.Pagina ?? DEFAULT_COLLECTION_QUERY.Pagina
    const pageSize = query?.TamanhoPagina ?? DEFAULT_COLLECTION_QUERY.TamanhoPagina
    const response = await http.get('/api/v1/usuarios', {
      params: buildCollectionQueryParams({ ParceiroId: parceiroId, ...query }),
    })
    return extractPaginatedPayload<UsuarioComPerfilResponse>(response.data, { page, pageSize })
  },
  async create(payload: RegisterRequest) {
    await http.post('/api/v1/usuarios', payload)
  },
  async update(id: number, payload: AtualizarUsuarioRequest) {
    await http.patch(`/api/v1/usuarios/${id}`, payload)
  },
}