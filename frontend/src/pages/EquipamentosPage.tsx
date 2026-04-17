import { Pencil, Power } from 'lucide-react'
import { useEffect, useRef, useState, type FormEvent } from 'react'
import { Badge, Button, Feedback, Modal, PageIntro, Panel, TextareaField } from '../components/ui'
import { getErrorMessage } from '../lib/errors'
import { categoriasApi, equipamentosApi } from '../services/sso'
import type { CategoriaResponse, EquipamentoResponse } from '../types/api'

interface EquipamentoFormState {
  nome: string
  categoriaId: string
  quantidadeEstoque: string
  precoDiaria: string
  marca: string
  modelo: string
  numeroSerie: string
  anoFabricacao: string
  descricao: string
  observacaoInternas: string
  ativo: boolean
}

const initialFormState: EquipamentoFormState = {
  nome: '',
  categoriaId: '',
  quantidadeEstoque: '0',
  precoDiaria: '0',
  marca: '',
  modelo: '',
  numeroSerie: '',
  anoFabricacao: '',
  descricao: '',
  observacaoInternas: '',
  ativo: true,
}

function fieldClass(error: string) {
  return [
    'w-full rounded-md border bg-white px-3 py-2 text-sm text-slate-900 outline-none transition-colors placeholder:text-slate-400',
    error
      ? 'border-red-500 focus:border-red-600 focus:ring-2 focus:ring-red-100 dark:border-red-400 dark:focus:border-red-300 dark:focus:ring-red-950'
      : 'border-slate-300 focus:border-slate-900 focus:ring-2 focus:ring-slate-200 dark:border-slate-300 dark:focus:border-slate-900 dark:focus:ring-slate-200',
  ].join(' ')
}

function formatCurrency(value: number) {
  return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value)
}

export function EquipamentosPage() {
  const didLoadInitiallyRef = useRef(false)
  const [items, setItems] = useState<EquipamentoResponse[]>([])
  const [categorias, setCategorias] = useState<CategoriaResponse[]>([])
  const [filterNome, setFilterNome] = useState('')
  const [filterMarca, setFilterMarca] = useState('')
  const [filterModelo, setFilterModelo] = useState('')
  const [filterCategoriaId, setFilterCategoriaId] = useState('')
  const [filterAtivo, setFilterAtivo] = useState('')
  const [feedback, setFeedback] = useState<{ tone: 'success' | 'danger'; message: string } | null>(null)
  const [tableMessage, setTableMessage] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [isSaving, setIsSaving] = useState(false)
  const [isModalOpen, setIsModalOpen] = useState(false)
  const [editingItem, setEditingItem] = useState<EquipamentoResponse | null>(null)
  const [form, setForm] = useState<EquipamentoFormState>(initialFormState)
  const [nomeError, setNomeError] = useState('')
  const [categoriaError, setCategoriaError] = useState('')

  async function loadReferenceData() {
    const response = await categoriasApi.list()
    setCategorias(response)
  }

  async function loadData(options?: { nome?: string; marca?: string; modelo?: string; categoriaId?: string; ativo?: string }) {
    setIsLoading(true)

    try {
      const response = options?.nome || options?.marca || options?.modelo || options?.categoriaId || options?.ativo
        ? await equipamentosApi.listFilter({
            Nome: options.nome,
            Marca: options.marca,
            Modelo: options.modelo,
            CategoriaId: options.categoriaId,
            Ativo: options.ativo === '' ? undefined : options.ativo === 'true',
          })
        : await equipamentosApi.list()

      setItems(response)
      setTableMessage(response.length === 0 ? 'Nenhum equipamento encontrado' : null)
    } catch (error) {
      setItems([])
      setTableMessage(getErrorMessage(error, 'Falha ao carregar equipamentos.'))
    } finally {
      setIsLoading(false)
    }
  }

  useEffect(() => {
    if (didLoadInitiallyRef.current) {
      return
    }

    didLoadInitiallyRef.current = true

    async function bootstrap() {
      setIsLoading(true)

      try {
        await Promise.all([loadReferenceData(), loadData()])
      } catch (error) {
        setFeedback({ tone: 'danger', message: getErrorMessage(error, 'Falha ao carregar dados iniciais de equipamentos.') })
        setIsLoading(false)
      }
    }

    void bootstrap()
  }, [])

  function openCreateModal() {
    setEditingItem(null)
    setForm(initialFormState)
    setNomeError('')
    setCategoriaError('')
    setIsModalOpen(true)
  }

  async function openEditModal(item: EquipamentoResponse) {
    setIsLoading(true)

    try {
      const fullItem = await equipamentosApi.getById(item.id)
      setEditingItem(fullItem)
      setForm({
        nome: fullItem.nome ?? '',
        categoriaId: fullItem.categoriaId,
        quantidadeEstoque: String(fullItem.quantidadeEstoque),
        precoDiaria: String(fullItem.precoDiaria),
        marca: fullItem.marca ?? '',
        modelo: fullItem.modelo ?? '',
        numeroSerie: fullItem.numeroSerie ?? '',
        anoFabricacao: fullItem.anoFabricacao ? String(fullItem.anoFabricacao) : '',
        descricao: fullItem.descricao ?? '',
        observacaoInternas: fullItem.observacaoInternas ?? '',
        ativo: fullItem.ativo,
      })
      setNomeError('')
      setCategoriaError('')
      setIsModalOpen(true)
    } catch (error) {
      setFeedback({ tone: 'danger', message: getErrorMessage(error, 'Falha ao carregar o equipamento selecionado.') })
    } finally {
      setIsLoading(false)
    }
  }

  async function handleSearch() {
    await loadData({
      nome: filterNome.trim() || undefined,
      marca: filterMarca.trim() || undefined,
      modelo: filterModelo.trim() || undefined,
      categoriaId: filterCategoriaId || undefined,
      ativo: filterAtivo,
    })
  }

  async function handleClearFilters() {
    setFilterNome('')
    setFilterMarca('')
    setFilterModelo('')
    setFilterCategoriaId('')
    setFilterAtivo('')
    await loadData()
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setFeedback(null)

    if (!form.nome.trim()) {
      setNomeError('O campo nome é obrigatório.')
      return
    }

    if (!form.categoriaId) {
      setCategoriaError('O campo categoria é obrigatório.')
      return
    }

    setIsSaving(true)

    try {
      const payload = {
        nome: form.nome.trim(),
        categoriaId: form.categoriaId,
        quantidadeEstoque: Number.parseInt(form.quantidadeEstoque, 10) || 0,
        precoDiaria: Number.parseFloat(form.precoDiaria) || 0,
        marca: form.marca.trim(),
        modelo: form.modelo.trim(),
        numeroSerie: form.numeroSerie.trim(),
        anoFabricacao: Number.parseInt(form.anoFabricacao, 10) || 0,
        descricao: form.descricao.trim(),
        observacaoInternas: form.observacaoInternas.trim(),
      }

      if (editingItem) {
        await equipamentosApi.update(editingItem.id, {
          ...payload,
          ativo: form.ativo,
        })
        setFeedback({ tone: 'success', message: 'Equipamento atualizado com sucesso.' })
      } else {
        await equipamentosApi.create(payload)
        setFeedback({ tone: 'success', message: 'Equipamento criado com sucesso.' })
      }

      setIsModalOpen(false)
      await handleSearch()
    } catch (error) {
      setFeedback({ tone: 'danger', message: getErrorMessage(error, 'Falha ao salvar equipamento.') })
    } finally {
      setIsSaving(false)
    }
  }

  async function handleInactivate(item: EquipamentoResponse) {
    setFeedback(null)

    try {
      await equipamentosApi.inactivate(item.id)
      setItems((current) => current.map((entry) => (entry.id === item.id ? { ...entry, ativo: false } : entry)))
      setFeedback({ tone: 'success', message: 'Equipamento inativado com sucesso.' })
    } catch (error) {
      setFeedback({ tone: 'danger', message: getErrorMessage(error, 'Falha ao inativar equipamento.') })
    }
  }

  return (
    <div className="space-y-6">
      <PageIntro action={<Button onClick={openCreateModal}>Novo</Button>} eyebrow="" title="Equipamentos" description="" />

      {feedback ? <Feedback tone={feedback.tone}>{feedback.message}</Feedback> : null}

      <Panel className="p-5 md:p-6">
        <div className="grid gap-4 md:grid-cols-3 xl:grid-cols-6 xl:items-end">
          <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
            <span>Nome</span>
            <input className={fieldClass('')} onChange={(event) => setFilterNome(event.target.value)} placeholder="Nome" value={filterNome} />
          </label>
          <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
            <span>Marca</span>
            <input className={fieldClass('')} onChange={(event) => setFilterMarca(event.target.value)} placeholder="Marca" value={filterMarca} />
          </label>
          <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
            <span>Modelo</span>
            <input className={fieldClass('')} onChange={(event) => setFilterModelo(event.target.value)} placeholder="Modelo" value={filterModelo} />
          </label>
          <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
            <span>Categoria</span>
            <select className={fieldClass('')} onChange={(event) => setFilterCategoriaId(event.target.value)} value={filterCategoriaId}>
              <option value="">Todas</option>
              {categorias.map((categoria) => (
                <option key={categoria.id} value={categoria.id}>{categoria.nome}</option>
              ))}
            </select>
          </label>
          <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
            <span>Status</span>
            <select className={fieldClass('')} onChange={(event) => setFilterAtivo(event.target.value)} value={filterAtivo}>
              <option value="">Todos</option>
              <option value="true">Ativos</option>
              <option value="false">Inativos</option>
            </select>
          </label>
          <div className="flex gap-2 xl:justify-end">
            <Button onClick={() => void handleSearch()} type="button">Buscar</Button>
            <Button onClick={() => void handleClearFilters()} type="button" variant="secondary">Limpar</Button>
          </div>
        </div>

        {isLoading ? (
          <div className="mt-6 rounded-md border border-[var(--line)] bg-[var(--surface-strong)] px-5 py-8 text-sm text-[var(--text-soft)]">Carregando equipamentos...</div>
        ) : (
          <div className="mt-6 overflow-hidden rounded-md border border-[var(--line)] dark:border-slate-300">
            <div className="overflow-x-auto">
              <table className="min-w-full border-collapse text-left text-sm">
                <thead className="bg-[var(--surface-strong)] text-[var(--text-soft)] dark:bg-white dark:text-slate-700">
                  <tr>
                    <th className="px-4 py-3 font-semibold">Nome</th>
                    <th className="px-4 py-3 font-semibold">Categoria</th>
                    <th className="px-4 py-3 font-semibold">Estoque</th>
                    <th className="px-4 py-3 font-semibold">Diária</th>
                    <th className="px-4 py-3 font-semibold">Marca</th>
                    <th className="px-4 py-3 font-semibold">Status</th>
                    <th className="px-4 py-3 font-semibold text-right">Ações</th>
                  </tr>
                </thead>
                <tbody>
                  {items.length > 0 ? (
                    items.map((item) => (
                      <tr key={item.id} className="border-t border-[var(--line)] bg-white/70 dark:border-slate-300 dark:bg-white">
                        <td className="px-4 py-3 font-medium text-[var(--text)] dark:text-slate-900">{item.nome}</td>
                        <td className="px-4 py-3 text-[var(--text-soft)] dark:text-slate-600">{item.categoriaNome || '-'}</td>
                        <td className="px-4 py-3 text-[var(--text-soft)] dark:text-slate-600">{item.quantidadeEstoque}</td>
                        <td className="px-4 py-3 text-[var(--text-soft)] dark:text-slate-600">{formatCurrency(item.precoDiaria)}</td>
                        <td className="px-4 py-3 text-[var(--text-soft)] dark:text-slate-600">{item.marca || '-'}</td>
                        <td className="px-4 py-3"><Badge tone={item.ativo ? 'success' : 'danger'}>{item.ativo ? 'Ativo' : 'Inativo'}</Badge></td>
                        <td className="px-4 py-3 text-right">
                          <div className="flex justify-end gap-2">
                            <Button aria-label="Editar equipamento" className="h-11 w-11 px-0" onClick={() => void openEditModal(item)} title="Editar" type="button" variant="secondary">
                              <Pencil className="h-5 w-5" />
                            </Button>
                            {item.ativo ? (
                              <Button aria-label="Inativar equipamento" className="h-11 w-11 px-0" onClick={() => void handleInactivate(item)} title="Inativar" type="button" variant="danger">
                                <Power className="h-5 w-5" />
                              </Button>
                            ) : null}
                          </div>
                        </td>
                      </tr>
                    ))
                  ) : (
                    <tr className="border-t border-[var(--line)] bg-white/70 dark:border-slate-300 dark:bg-white">
                      <td className="px-4 py-6 text-sm text-[var(--text-soft)] dark:text-slate-600" colSpan={7}>
                        {tableMessage ?? 'Nenhum equipamento encontrado'}
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          </div>
        )}
      </Panel>

      <Modal onClose={() => setIsModalOpen(false)} open={isModalOpen} title={editingItem ? 'Editar equipamento' : 'Novo equipamento'}>
        <form className="space-y-5" noValidate onSubmit={handleSubmit}>
          <div className="grid gap-4 md:grid-cols-2">
            <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
              <span>
                Nome <span className="text-red-600 dark:text-red-400">*</span>
              </span>
              <input className={fieldClass(nomeError)} onChange={(event) => setForm((current) => ({ ...current, nome: event.target.value }))} onFocus={() => setNomeError('')} placeholder="Informe o nome" value={form.nome} />
              {nomeError ? <span className="text-xs text-red-600 dark:text-red-400">{nomeError}</span> : null}
            </label>

            <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
              <span>
                Categoria <span className="text-red-600 dark:text-red-400">*</span>
              </span>
              <select className={fieldClass(categoriaError)} onChange={(event) => setForm((current) => ({ ...current, categoriaId: event.target.value }))} onFocus={() => setCategoriaError('')} value={form.categoriaId}>
                <option value="">Selecione</option>
                {categorias.map((categoria) => (
                  <option key={categoria.id} value={categoria.id}>{categoria.nome}</option>
                ))}
              </select>
              {categoriaError ? <span className="text-xs text-red-600 dark:text-red-400">{categoriaError}</span> : null}
            </label>

            <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
              <span>Quantidade em estoque</span>
              <input className={fieldClass('')} min="0" onChange={(event) => setForm((current) => ({ ...current, quantidadeEstoque: event.target.value }))} type="number" value={form.quantidadeEstoque} />
            </label>

            <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
              <span>Preço da diária</span>
              <input className={fieldClass('')} min="0" onChange={(event) => setForm((current) => ({ ...current, precoDiaria: event.target.value }))} step="0.01" type="number" value={form.precoDiaria} />
            </label>

            <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
              <span>Marca</span>
              <input className={fieldClass('')} onChange={(event) => setForm((current) => ({ ...current, marca: event.target.value }))} placeholder="Informe a marca" value={form.marca} />
            </label>

            <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
              <span>Modelo</span>
              <input className={fieldClass('')} onChange={(event) => setForm((current) => ({ ...current, modelo: event.target.value }))} placeholder="Informe o modelo" value={form.modelo} />
            </label>

            <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
              <span>Número de série</span>
              <input className={fieldClass('')} onChange={(event) => setForm((current) => ({ ...current, numeroSerie: event.target.value }))} placeholder="Informe o número de série" value={form.numeroSerie} />
            </label>

            <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
              <span>Ano de fabricação</span>
              <input className={fieldClass('')} min="0" onChange={(event) => setForm((current) => ({ ...current, anoFabricacao: event.target.value }))} type="number" value={form.anoFabricacao} />
            </label>

            <div className="md:col-span-2">
              <TextareaField label="Descrição" onChange={(event) => setForm((current) => ({ ...current, descricao: event.target.value }))} placeholder="Opcional" value={form.descricao} />
            </div>

            <div className="md:col-span-2">
              <TextareaField label="Observações internas" onChange={(event) => setForm((current) => ({ ...current, observacaoInternas: event.target.value }))} placeholder="Opcional" value={form.observacaoInternas} />
            </div>
          </div>

          {editingItem ? (
            <label className="flex items-center gap-3 rounded-md border border-slate-200 bg-slate-50 px-4 py-3 dark:border-slate-800 dark:bg-slate-900">
              <input checked={form.ativo} className="h-4 w-4 accent-slate-900 dark:accent-slate-100" onChange={(event) => setForm((current) => ({ ...current, ativo: event.target.checked }))} type="checkbox" />
              <span className="text-sm font-semibold text-slate-900 dark:text-slate-100">Equipamento ativo</span>
            </label>
          ) : null}

          <div className="flex justify-end gap-3 border-t border-slate-200 pt-4 dark:border-slate-800">
            <Button onClick={() => setIsModalOpen(false)} type="button" variant="ghost">Cancelar</Button>
            <Button disabled={isSaving} type="submit">{isSaving ? 'Salvando...' : 'Salvar'}</Button>
          </div>
        </form>
      </Modal>
    </div>
  )
}