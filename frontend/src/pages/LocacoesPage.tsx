import { Check, Pencil, X } from 'lucide-react'
import { useEffect, useRef, useState, type FormEvent } from 'react'
import { Button, Feedback, Modal, PageIntro, Panel, TextareaField } from '../components/ui'
import { getErrorMessage } from '../lib/errors'
import { clientesApi, equipamentosApi, locacoesApi } from '../services/sso'
import type { ClienteResponse, EquipamentoResponse, LocacaoResponse, StatusLocacao } from '../types/api'

interface LocacaoFormState {
  clienteId: string
  equipamentoId: string
  dataRetirada: string
  previsaoDevolucao: string
  valorDiaria: string
  observacao: string
}

const initialFormState: LocacaoFormState = {
  clienteId: '',
  equipamentoId: '',
  dataRetirada: '',
  previsaoDevolucao: '',
  valorDiaria: '0',
  observacao: '',
}

function fieldClass(error: string) {
  return [
    'w-full rounded-md border bg-white px-3 py-2 text-sm text-slate-900 outline-none transition-colors placeholder:text-slate-400',
    error
      ? 'border-red-500 focus:border-red-600 focus:ring-2 focus:ring-red-100 dark:border-red-400 dark:focus:border-red-300 dark:focus:ring-red-950'
      : 'border-slate-300 focus:border-slate-900 focus:ring-2 focus:ring-slate-200 dark:border-slate-300 dark:focus:border-slate-900 dark:focus:ring-slate-200',
  ].join(' ')
}

function formatDateTime(value?: string | null) {
  if (!value) {
    return '-'
  }

  return new Intl.DateTimeFormat('pt-BR', {
    dateStyle: 'short',
    timeStyle: 'short',
  }).format(new Date(value))
}

function formatCurrency(value?: number | null) {
  return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value ?? 0)
}

function toIsoDateTime(value: string) {
  return value ? new Date(value).toISOString() : ''
}

function toDateTimeLocalValue(value?: string | null) {
  if (!value) {
    return ''
  }

  const date = new Date(value)
  const offset = date.getTimezoneOffset()
  const adjusted = new Date(date.getTime() - offset * 60_000)

  return adjusted.toISOString().slice(0, 16)
}

const statusOptions: Array<{ value: string; label: string }> = [
  { value: '', label: 'Todos' },
  { value: '1', label: 'Status 1' },
  { value: '2', label: 'Status 2' },
  { value: '3', label: 'Status 3' },
  { value: '4', label: 'Status 4' },
]

export function LocacoesPage() {
  const didLoadInitiallyRef = useRef(false)
  const [items, setItems] = useState<LocacaoResponse[]>([])
  const [clientes, setClientes] = useState<ClienteResponse[]>([])
  const [equipamentos, setEquipamentos] = useState<EquipamentoResponse[]>([])
  const [filterClienteId, setFilterClienteId] = useState('')
  const [filterEquipamentoId, setFilterEquipamentoId] = useState('')
  const [filterStatus, setFilterStatus] = useState('')
  const [filterInicio, setFilterInicio] = useState('')
  const [filterFim, setFilterFim] = useState('')
  const [feedback, setFeedback] = useState<{ tone: 'success' | 'danger'; message: string } | null>(null)
  const [tableMessage, setTableMessage] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [isSaving, setIsSaving] = useState(false)
  const [isReturning, setIsReturning] = useState(false)
  const [isModalOpen, setIsModalOpen] = useState(false)
  const [isReturnModalOpen, setIsReturnModalOpen] = useState(false)
  const [editingItem, setEditingItem] = useState<LocacaoResponse | null>(null)
  const [returningItem, setReturningItem] = useState<LocacaoResponse | null>(null)
  const [returnDate, setReturnDate] = useState(toDateTimeLocalValue(new Date().toISOString()))
  const [returnObservation, setReturnObservation] = useState('')
  const [form, setForm] = useState<LocacaoFormState>(initialFormState)
  const [clienteError, setClienteError] = useState('')
  const [equipamentoError, setEquipamentoError] = useState('')

  async function loadReferenceData() {
    const [loadedClientes, loadedEquipamentos] = await Promise.all([clientesApi.list(), equipamentosApi.list()])
    setClientes(loadedClientes)
    setEquipamentos(loadedEquipamentos)
  }

  async function loadData(filters?: {
    clienteId?: string
    equipamentoId?: string
    status?: string
    dataRetiradaInicio?: string
    dataRetiradaFim?: string
    onlyLate?: boolean
  }) {
    setIsLoading(true)

    try {
      const response = filters?.onlyLate
        ? await locacoesApi.listLate()
        : filters?.clienteId || filters?.equipamentoId || filters?.status || filters?.dataRetiradaInicio || filters?.dataRetiradaFim
          ? await locacoesApi.listFilter({
              ClienteId: filters.clienteId,
              EquipamentoId: filters.equipamentoId,
              Status: filters.status ? Number.parseInt(filters.status, 10) as StatusLocacao : undefined,
              DataRetiradaInicio: filters.dataRetiradaInicio,
              DataRetiradaFim: filters.dataRetiradaFim,
            })
          : await locacoesApi.list()

      setItems(response)
      setTableMessage(response.length === 0 ? 'Nenhuma locação encontrada' : null)
    } catch (error) {
      setItems([])
      setTableMessage(getErrorMessage(error, 'Falha ao carregar locações.'))
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
        setFeedback({ tone: 'danger', message: getErrorMessage(error, 'Falha ao carregar dados iniciais de locações.') })
        setIsLoading(false)
      }
    }

    void bootstrap()
  }, [])

  function openCreateModal() {
    setEditingItem(null)
    setForm(initialFormState)
    setClienteError('')
    setEquipamentoError('')
    setIsModalOpen(true)
  }

  async function openEditModal(item: LocacaoResponse) {
    setIsLoading(true)

    try {
      const fullItem = await locacoesApi.getById(item.id)
      setEditingItem(fullItem)
      setForm({
        clienteId: fullItem.clienteId,
        equipamentoId: fullItem.equipamentoId,
        dataRetirada: toDateTimeLocalValue(fullItem.dataRetirada),
        previsaoDevolucao: toDateTimeLocalValue(fullItem.previsaoDevolucao),
        valorDiaria: String(fullItem.valorDiaria),
        observacao: fullItem.observacao ?? '',
      })
      setClienteError('')
      setEquipamentoError('')
      setIsModalOpen(true)
    } catch (error) {
      setFeedback({ tone: 'danger', message: getErrorMessage(error, 'Falha ao carregar a locação selecionada.') })
    } finally {
      setIsLoading(false)
    }
  }

  async function handleSearch() {
    await loadData({
      clienteId: filterClienteId || undefined,
      equipamentoId: filterEquipamentoId || undefined,
      status: filterStatus || undefined,
      dataRetiradaInicio: filterInicio ? toIsoDateTime(filterInicio) : undefined,
      dataRetiradaFim: filterFim ? toIsoDateTime(filterFim) : undefined,
    })
  }

  async function handleClearFilters() {
    setFilterClienteId('')
    setFilterEquipamentoId('')
    setFilterStatus('')
    setFilterInicio('')
    setFilterFim('')
    await loadData()
  }

  async function handleShowLate() {
    await loadData({ onlyLate: true })
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setFeedback(null)

    if (!form.clienteId) {
      setClienteError('Selecione um cliente.')
      return
    }

    if (!form.equipamentoId) {
      setEquipamentoError('Selecione um equipamento.')
      return
    }

    setIsSaving(true)

    try {
      const payload = {
        clienteId: form.clienteId,
        equipamentoId: form.equipamentoId,
        dataRetirada: toIsoDateTime(form.dataRetirada),
        previsaoDevolucao: toIsoDateTime(form.previsaoDevolucao),
        valorDiaria: Number.parseFloat(form.valorDiaria) || 0,
        observacao: form.observacao.trim(),
      }

      if (editingItem) {
        await locacoesApi.update(editingItem.id, payload)
        setFeedback({ tone: 'success', message: 'Locação atualizada com sucesso.' })
      } else {
        await locacoesApi.create(payload)
        setFeedback({ tone: 'success', message: 'Locação criada com sucesso.' })
      }

      setIsModalOpen(false)
      await handleSearch()
    } catch (error) {
      setFeedback({ tone: 'danger', message: getErrorMessage(error, 'Falha ao salvar locação.') })
    } finally {
      setIsSaving(false)
    }
  }

  function openReturnModal(item: LocacaoResponse) {
    setReturningItem(item)
    setReturnDate(toDateTimeLocalValue(new Date().toISOString()))
    setReturnObservation('')
    setIsReturnModalOpen(true)
  }

  async function handleReturnRental(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    if (!returningItem) {
      return
    }

    setIsReturning(true)
    setFeedback(null)

    try {
      await locacoesApi.returnRental(returningItem.id, {
        id: returningItem.id,
        dataDevolucao: toIsoDateTime(returnDate),
        observacao: returnObservation.trim() || undefined,
      })
      setIsReturnModalOpen(false)
      setReturningItem(null)
      setFeedback({ tone: 'success', message: 'Locação devolvida com sucesso.' })
      await handleSearch()
    } catch (error) {
      setFeedback({ tone: 'danger', message: getErrorMessage(error, 'Falha ao devolver locação.') })
    } finally {
      setIsReturning(false)
    }
  }

  async function handleCancelRental(item: LocacaoResponse) {
    setFeedback(null)

    try {
      await locacoesApi.cancel(item.id)
      setFeedback({ tone: 'success', message: 'Locação cancelada com sucesso.' })
      await handleSearch()
    } catch (error) {
      setFeedback({ tone: 'danger', message: getErrorMessage(error, 'Falha ao cancelar locação.') })
    }
  }

  return (
    <div className="space-y-6">
      <PageIntro action={<Button onClick={openCreateModal}>Novo</Button>} eyebrow="" title="Locações" description="" />

      {feedback ? <Feedback tone={feedback.tone}>{feedback.message}</Feedback> : null}

      <Panel className="p-5 md:p-6">
        <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-6 xl:items-end">
          <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
            <span>Cliente</span>
            <select className={fieldClass('')} onChange={(event) => setFilterClienteId(event.target.value)} value={filterClienteId}>
              <option value="">Todos</option>
              {clientes.map((cliente) => (
                <option key={cliente.id} value={cliente.id}>{cliente.nome}</option>
              ))}
            </select>
          </label>
          <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
            <span>Equipamento</span>
            <select className={fieldClass('')} onChange={(event) => setFilterEquipamentoId(event.target.value)} value={filterEquipamentoId}>
              <option value="">Todos</option>
              {equipamentos.map((equipamento) => (
                <option key={equipamento.id} value={equipamento.id}>{equipamento.nome}</option>
              ))}
            </select>
          </label>
          <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
            <span>Status</span>
            <select className={fieldClass('')} onChange={(event) => setFilterStatus(event.target.value)} value={filterStatus}>
              {statusOptions.map((option) => (
                <option key={option.value || 'all'} value={option.value}>{option.label}</option>
              ))}
            </select>
          </label>
          <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
            <span>Retirada inicial</span>
            <input className={fieldClass('')} onChange={(event) => setFilterInicio(event.target.value)} type="datetime-local" value={filterInicio} />
          </label>
          <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
            <span>Retirada final</span>
            <input className={fieldClass('')} onChange={(event) => setFilterFim(event.target.value)} type="datetime-local" value={filterFim} />
          </label>
          <div className="flex flex-wrap gap-2 xl:justify-end">
            <Button onClick={() => void handleSearch()} type="button">Buscar</Button>
            <Button onClick={() => void handleClearFilters()} type="button" variant="secondary">Limpar</Button>
            <Button onClick={() => void handleShowLate()} type="button" variant="secondary">Atrasadas</Button>
          </div>
        </div>

        {isLoading ? (
          <div className="mt-6 rounded-md border border-[var(--line)] bg-[var(--surface-strong)] px-5 py-8 text-sm text-[var(--text-soft)]">Carregando locações...</div>
        ) : (
          <div className="mt-6 overflow-hidden rounded-md border border-[var(--line)] dark:border-slate-300">
            <div className="overflow-x-auto">
              <table className="min-w-full border-collapse text-left text-sm">
                <thead className="bg-[var(--surface-strong)] text-[var(--text-soft)] dark:bg-white dark:text-slate-700">
                  <tr>
                    <th className="px-4 py-3 font-semibold">Cliente</th>
                    <th className="px-4 py-3 font-semibold">Equipamento</th>
                    <th className="px-4 py-3 font-semibold">Status</th>
                    <th className="px-4 py-3 font-semibold">Retirada</th>
                    <th className="px-4 py-3 font-semibold">Previsão</th>
                    <th className="px-4 py-3 font-semibold">Diária</th>
                    <th className="px-4 py-3 font-semibold">Total</th>
                    <th className="px-4 py-3 font-semibold text-right">Ações</th>
                  </tr>
                </thead>
                <tbody>
                  {items.length > 0 ? (
                    items.map((item) => (
                      <tr key={item.id} className="border-t border-[var(--line)] bg-white/70 dark:border-slate-300 dark:bg-white">
                        <td className="px-4 py-3 font-medium text-[var(--text)] dark:text-slate-900">{item.clienteNome || '-'}</td>
                        <td className="px-4 py-3 text-[var(--text-soft)] dark:text-slate-600">{item.equipamentoNome || '-'}</td>
                        <td className="px-4 py-3 text-[var(--text-soft)] dark:text-slate-600">{item.statusDescricao || `Status ${item.status}`}</td>
                        <td className="px-4 py-3 text-[var(--text-soft)] dark:text-slate-600">{formatDateTime(item.dataRetirada)}</td>
                        <td className="px-4 py-3 text-[var(--text-soft)] dark:text-slate-600">{formatDateTime(item.previsaoDevolucao)}</td>
                        <td className="px-4 py-3 text-[var(--text-soft)] dark:text-slate-600">{formatCurrency(item.valorDiaria)}</td>
                        <td className="px-4 py-3 text-[var(--text-soft)] dark:text-slate-600">{formatCurrency(item.valorTotal)}</td>
                        <td className="px-4 py-3 text-right">
                          <div className="flex justify-end gap-2">
                            <Button aria-label="Editar locação" className="h-11 w-11 px-0" onClick={() => void openEditModal(item)} title="Editar" type="button" variant="secondary">
                              <Pencil className="h-5 w-5" />
                            </Button>
                            <Button aria-label="Devolver locação" className="h-11 w-11 px-0" onClick={() => openReturnModal(item)} title="Devolver" type="button" variant="secondary">
                              <Check className="h-5 w-5" />
                            </Button>
                            <Button aria-label="Cancelar locação" className="h-11 w-11 px-0" onClick={() => void handleCancelRental(item)} title="Cancelar" type="button" variant="danger">
                              <X className="h-5 w-5" />
                            </Button>
                          </div>
                        </td>
                      </tr>
                    ))
                  ) : (
                    <tr className="border-t border-[var(--line)] bg-white/70 dark:border-slate-300 dark:bg-white">
                      <td className="px-4 py-6 text-sm text-[var(--text-soft)] dark:text-slate-600" colSpan={8}>
                        {tableMessage ?? 'Nenhuma locação encontrada'}
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          </div>
        )}
      </Panel>

      <Modal onClose={() => setIsModalOpen(false)} open={isModalOpen} title={editingItem ? 'Editar locação' : 'Nova locação'}>
        <form className="space-y-5" noValidate onSubmit={handleSubmit}>
          <div className="grid gap-4 md:grid-cols-2">
            <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
              <span>Cliente</span>
              <select className={fieldClass(clienteError)} onChange={(event) => setForm((current) => ({ ...current, clienteId: event.target.value }))} onFocus={() => setClienteError('')} value={form.clienteId}>
                <option value="">Selecione</option>
                {clientes.map((cliente) => (
                  <option key={cliente.id} value={cliente.id}>{cliente.nome}</option>
                ))}
              </select>
              {clienteError ? <span className="text-xs text-red-600 dark:text-red-400">{clienteError}</span> : null}
            </label>

            <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
              <span>Equipamento</span>
              <select className={fieldClass(equipamentoError)} onChange={(event) => setForm((current) => ({ ...current, equipamentoId: event.target.value }))} onFocus={() => setEquipamentoError('')} value={form.equipamentoId}>
                <option value="">Selecione</option>
                {equipamentos.map((equipamento) => (
                  <option key={equipamento.id} value={equipamento.id}>{equipamento.nome}</option>
                ))}
              </select>
              {equipamentoError ? <span className="text-xs text-red-600 dark:text-red-400">{equipamentoError}</span> : null}
            </label>

            <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
              <span>Data de retirada</span>
              <input className={fieldClass('')} onChange={(event) => setForm((current) => ({ ...current, dataRetirada: event.target.value }))} type="datetime-local" value={form.dataRetirada} />
            </label>

            <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
              <span>Previsão de devolução</span>
              <input className={fieldClass('')} onChange={(event) => setForm((current) => ({ ...current, previsaoDevolucao: event.target.value }))} type="datetime-local" value={form.previsaoDevolucao} />
            </label>

            <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200 md:col-span-2">
              <span>Valor da diária</span>
              <input className={fieldClass('')} min="0" onChange={(event) => setForm((current) => ({ ...current, valorDiaria: event.target.value }))} step="0.01" type="number" value={form.valorDiaria} />
            </label>

            <div className="md:col-span-2">
              <TextareaField label="Observação" onChange={(event) => setForm((current) => ({ ...current, observacao: event.target.value }))} placeholder="Opcional" value={form.observacao} />
            </div>
          </div>

          <div className="flex justify-end gap-3 border-t border-slate-200 pt-4 dark:border-slate-800">
            <Button onClick={() => setIsModalOpen(false)} type="button" variant="ghost">Cancelar</Button>
            <Button disabled={isSaving} type="submit">{isSaving ? 'Salvando...' : 'Salvar'}</Button>
          </div>
        </form>
      </Modal>

      <Modal onClose={() => setIsReturnModalOpen(false)} open={isReturnModalOpen} title="Devolver locação">
        <form className="space-y-5" noValidate onSubmit={handleReturnRental}>
          <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
            <span>Data da devolução</span>
            <input className={fieldClass('')} onChange={(event) => setReturnDate(event.target.value)} type="datetime-local" value={returnDate} />
          </label>

          <TextareaField label="Observação" onChange={(event) => setReturnObservation(event.target.value)} placeholder="Opcional" value={returnObservation} />

          <div className="flex justify-end gap-3 border-t border-slate-200 pt-4 dark:border-slate-800">
            <Button onClick={() => setIsReturnModalOpen(false)} type="button" variant="ghost">Cancelar</Button>
            <Button disabled={isReturning} type="submit">{isReturning ? 'Salvando...' : 'Confirmar devolução'}</Button>
          </div>
        </form>
      </Modal>
    </div>
  )
}