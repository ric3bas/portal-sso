import { Lock, LockOpen, Pencil, Plus, Power, Trash2 } from 'lucide-react'
import { useEffect, useRef, useState, type FormEvent } from 'react'
import { AdminPage, AdminPanel } from '../components/admin'
import { DataTable } from '../components/DataTable'
import { Badge, Button, Modal, TextareaField } from '../components/ui'
import { getErrorMessage } from '../lib/errors'
import { clientesApi } from '../services/sso'
import type { ClienteResponse, PaginatedResult } from '../types/api'

interface ClienteTelefoneFormState {
  id?: string
  ddd: string
  numero: string
}

interface ClienteEnderecoFormState {
  id?: string
  logradouro: string
  cidade: string
  estado: string
  numero: string
  complemento: string
}

interface ClienteFormState {
  nome: string
  cpf: string
  email: string
  observacao: string
  telefones: ClienteTelefoneFormState[]
  enderecos: ClienteEnderecoFormState[]
  ativo: boolean
  bloqueado: boolean
}

interface ClienteTelefoneFormErrors {
  ddd: string
  numero: string
}

interface ClienteEnderecoFormErrors {
  logradouro: string
  cidade: string
  estado: string
  numero: string
}

const initialFormState: ClienteFormState = {
  nome: '',
  cpf: '',
  email: '',
  observacao: '',
  telefones: [{ ddd: '', numero: '' }],
  enderecos: [{ logradouro: '', cidade: '', estado: '', numero: '', complemento: '' }],
  ativo: true,
  bloqueado: false,
}

interface ClienteFormErrors {
  nome: string
  telefones: ClienteTelefoneFormErrors[]
  enderecos: ClienteEnderecoFormErrors[]
}

const initialFormErrors: ClienteFormErrors = {
  nome: '',
  telefones: [{ ddd: '', numero: '' }],
  enderecos: [{ logradouro: '', cidade: '', estado: '', numero: '' }],
}

function createEmptyTelefone(): ClienteTelefoneFormState {
  return { ddd: '', numero: '' }
}

function createEmptyEndereco(): ClienteEnderecoFormState {
  return { logradouro: '', cidade: '', estado: '', numero: '', complemento: '' }
}

function createTelefoneError(): ClienteTelefoneFormErrors {
  return { ddd: '', numero: '' }
}

function createEnderecoError(): ClienteEnderecoFormErrors {
  return { logradouro: '', cidade: '', estado: '', numero: '' }
}

function onlyDigits(value: string) {
  return value.replace(/\D/g, '')
}

function formatCpf(value: string) {
  const digits = onlyDigits(value).slice(0, 11)

  if (digits.length <= 3) {
    return digits
  }

  if (digits.length <= 6) {
    return `${digits.slice(0, 3)}.${digits.slice(3)}`
  }

  if (digits.length <= 9) {
    return `${digits.slice(0, 3)}.${digits.slice(3, 6)}.${digits.slice(6)}`
  }

  return `${digits.slice(0, 3)}.${digits.slice(3, 6)}.${digits.slice(6, 9)}-${digits.slice(9)}`
}

function formatDdd(value: string) {
  return onlyDigits(value).slice(0, 2)
}

function formatPhoneNumber(value: string) {
  const digits = onlyDigits(value).slice(0, 9)

  if (digits.length <= 4) {
    return digits
  }

  if (digits.length <= 8) {
    return `${digits.slice(0, 4)}-${digits.slice(4)}`
  }

  return `${digits.slice(0, 5)}-${digits.slice(5)}`
}

function fieldClass(error: string) {
  return [
    'w-full rounded-md border bg-white px-3 py-2 text-sm text-slate-900 outline-none transition-colors placeholder:text-slate-400',
    error
      ? 'border-red-500 focus:border-red-600 focus:ring-2 focus:ring-red-100 dark:border-red-400 dark:focus:border-red-300 dark:focus:ring-red-950'
      : 'border-slate-300 focus:border-slate-900 focus:ring-2 focus:ring-slate-200 dark:border-slate-300 dark:focus:border-slate-900 dark:focus:ring-slate-200',
  ].join(' ')
}

export function ClientesPage() {
  const didLoadInitiallyRef = useRef(false)
  const [items, setItems] = useState<ClienteResponse[]>([])
  const [filterNome, setFilterNome] = useState('')
  const [filterCpf, setFilterCpf] = useState('')
  const [feedback, setFeedback] = useState<{ tone: 'success' | 'danger'; message: string } | null>(null)
  const [tableMessage, setTableMessage] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [pagination, setPagination] = useState<PaginatedResult<ClienteResponse>['pagination']>({ page: 1, pageSize: 20, totalRecords: 0, totalPages: 1 })
  const [isSaving, setIsSaving] = useState(false)
  const [isModalOpen, setIsModalOpen] = useState(false)
  const [editingItem, setEditingItem] = useState<ClienteResponse | null>(null)
  const [form, setForm] = useState<ClienteFormState>(initialFormState)
  const [formErrors, setFormErrors] = useState<ClienteFormErrors>(initialFormErrors)

  async function loadData(options?: { nome?: string; cpf?: string }, page = pagination.page, pageSize = pagination.pageSize) {
    setIsLoading(true)

    try {
      const response = options?.nome || options?.cpf
        ? await clientesApi.listFilterPage({ Nome: options.nome, Cpf: options.cpf }, { Pagina: page, TamanhoPagina: pageSize })
        : await clientesApi.listPage({ Pagina: page, TamanhoPagina: pageSize })

      setItems(response.items)
      setPagination(response.pagination)
      setTableMessage(response.items.length === 0 ? 'Nenhum cliente encontrado' : null)
    } catch (error) {
      setItems([])
      setPagination((current) => ({ ...current, totalRecords: 0, totalPages: 1 }))
      setTableMessage(getErrorMessage(error, 'Falha ao carregar clientes.'))
    } finally {
      setIsLoading(false)
    }
  }

  useEffect(() => {
    if (didLoadInitiallyRef.current) {
      return
    }

    didLoadInitiallyRef.current = true
    void loadData()
  }, [])

  function openCreateModal() {
    setEditingItem(null)
    setForm(initialFormState)
    setFormErrors(initialFormErrors)
    setIsModalOpen(true)
  }

  async function openEditModal(item: ClienteResponse) {
    setIsLoading(true)

    try {
      const fullItem = await clientesApi.getById(item.id)
      setEditingItem(fullItem)
      setForm({
        nome: fullItem.nome ?? '',
        cpf: formatCpf(fullItem.cpf ?? ''),
        email: fullItem.email ?? '',
        observacao: fullItem.observacao ?? '',
        telefones:
          fullItem.telefones?.length
            ? fullItem.telefones.map((telefone) => ({
                id: telefone.id,
                ddd: formatDdd(telefone.ddd ?? ''),
                numero: formatPhoneNumber(telefone.numero ?? ''),
              }))
            : [createEmptyTelefone()],
        enderecos:
          fullItem.enderecos?.length
            ? fullItem.enderecos.map((endereco) => ({
                id: endereco.id,
                logradouro: endereco.logradouro ?? '',
                cidade: endereco.cidade ?? '',
                estado: endereco.estado ?? '',
                numero: endereco.numero ?? '',
                complemento: endereco.complemento ?? '',
              }))
            : [createEmptyEndereco()],
        ativo: fullItem.ativo,
        bloqueado: fullItem.bloqueado,
      })
      setFormErrors({
        nome: '',
        telefones: (fullItem.telefones?.length ? fullItem.telefones : [null]).map(() => createTelefoneError()),
        enderecos: (fullItem.enderecos?.length ? fullItem.enderecos : [null]).map(() => createEnderecoError()),
      })
      setIsModalOpen(true)
    } catch (error) {
      setFeedback({ tone: 'danger', message: getErrorMessage(error, 'Falha ao carregar o cliente selecionado.') })
    } finally {
      setIsLoading(false)
    }
  }

  function clearFieldError(field: keyof ClienteFormErrors) {
    setFormErrors((current) => ({ ...current, [field]: current[field] instanceof Array ? current[field] : '' }))
  }

  function clearTelefoneFieldError(index: number, field: keyof ClienteTelefoneFormErrors) {
    setFormErrors((current) => ({
      ...current,
      telefones: current.telefones.map((telefone, telefoneIndex) =>
        telefoneIndex === index ? { ...telefone, [field]: '' } : telefone,
      ),
    }))
  }

  function clearEnderecoFieldError(index: number, field: keyof ClienteEnderecoFormErrors) {
    setFormErrors((current) => ({
      ...current,
      enderecos: current.enderecos.map((endereco, enderecoIndex) =>
        enderecoIndex === index ? { ...endereco, [field]: '' } : endereco,
      ),
    }))
  }

  function addTelefone() {
    setForm((current) => ({ ...current, telefones: [...current.telefones, createEmptyTelefone()] }))
    setFormErrors((current) => ({ ...current, telefones: [...current.telefones, createTelefoneError()] }))
  }

  function removeTelefone(index: number) {
    setForm((current) => ({
      ...current,
      telefones: current.telefones.length > 1 ? current.telefones.filter((_, itemIndex) => itemIndex !== index) : current.telefones,
    }))
    setFormErrors((current) => ({
      ...current,
      telefones: current.telefones.length > 1 ? current.telefones.filter((_, itemIndex) => itemIndex !== index) : current.telefones,
    }))
  }

  function addEndereco() {
    setForm((current) => ({ ...current, enderecos: [...current.enderecos, createEmptyEndereco()] }))
    setFormErrors((current) => ({ ...current, enderecos: [...current.enderecos, createEnderecoError()] }))
  }

  function removeEndereco(index: number) {
    setForm((current) => ({
      ...current,
      enderecos: current.enderecos.length > 1 ? current.enderecos.filter((_, itemIndex) => itemIndex !== index) : current.enderecos,
    }))
    setFormErrors((current) => ({
      ...current,
      enderecos: current.enderecos.length > 1 ? current.enderecos.filter((_, itemIndex) => itemIndex !== index) : current.enderecos,
    }))
  }

  function validateForm() {
    const nextErrors: ClienteFormErrors = {
      nome: '',
      telefones: form.telefones.map(() => createTelefoneError()),
      enderecos: form.enderecos.map(() => createEnderecoError()),
    }

    if (!form.nome.trim()) {
      nextErrors.nome = 'O campo nome é obrigatório.'
    }

    form.telefones.forEach((telefone, index) => {
      if (!telefone.ddd.trim()) {
        nextErrors.telefones[index].ddd = 'O DDD é obrigatório.'
      }

      if (!telefone.numero.trim()) {
        nextErrors.telefones[index].numero = 'O número do telefone é obrigatório.'
      }
    })

    form.enderecos.forEach((endereco, index) => {
      if (!endereco.logradouro.trim()) {
        nextErrors.enderecos[index].logradouro = 'O logradouro é obrigatório.'
      }

      if (!endereco.cidade.trim()) {
        nextErrors.enderecos[index].cidade = 'A cidade é obrigatória.'
      }

      if (!endereco.estado.trim()) {
        nextErrors.enderecos[index].estado = 'O estado é obrigatório.'
      }

      if (!endereco.numero.trim()) {
        nextErrors.enderecos[index].numero = 'O número do endereço é obrigatório.'
      }
    })

    setFormErrors(nextErrors)

    return Boolean(nextErrors.nome)
      || nextErrors.telefones.some((telefone) => Boolean(telefone.ddd) || Boolean(telefone.numero))
      || nextErrors.enderecos.some((endereco) => Boolean(endereco.logradouro) || Boolean(endereco.cidade) || Boolean(endereco.estado) || Boolean(endereco.numero))
  }

  async function handleSearch() {
    await loadData({ nome: filterNome.trim() || undefined, cpf: filterCpf.trim() || undefined }, 1, pagination.pageSize)
  }

  async function handleClearFilters() {
    setFilterNome('')
    setFilterCpf('')
    await loadData(undefined, 1, pagination.pageSize)
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setFeedback(null)

    if (validateForm()) {
      return
    }

    setIsSaving(true)

    try {
      const payload = {
        nome: form.nome.trim(),
        cpf: onlyDigits(form.cpf),
        email: form.email.trim(),
        observacao: form.observacao.trim(),
        telefones: form.telefones.map((telefone) => ({
          id: telefone.id,
          ddd: onlyDigits(telefone.ddd),
          numero: onlyDigits(telefone.numero),
        })),
        enderecos: form.enderecos.map((endereco) => ({
          id: endereco.id,
          logradouro: endereco.logradouro.trim(),
          cidade: endereco.cidade.trim(),
          estado: endereco.estado.trim(),
          numero: endereco.numero.trim(),
          complemento: endereco.complemento.trim() || undefined,
        })),
      }

      if (editingItem) {
        await clientesApi.update(editingItem.id, {
          ...payload,
          ativo: form.ativo,
          bloqueado: form.bloqueado,
        })
        setFeedback({ tone: 'success', message: 'Cliente atualizado com sucesso.' })
      } else {
        await clientesApi.create(payload)
        setFeedback({ tone: 'success', message: 'Cliente criado com sucesso.' })
      }

      setIsModalOpen(false)
      await handleSearch()
    } catch (error) {
      setFeedback({ tone: 'danger', message: getErrorMessage(error, 'Falha ao salvar cliente.') })
    } finally {
      setIsSaving(false)
    }
  }

  async function handleToggleBlocked(item: ClienteResponse) {
    setFeedback(null)

    try {
      if (item.bloqueado) {
        await clientesApi.unblock(item.id)
      } else {
        await clientesApi.block(item.id)
      }

      setItems((current) => current.map((entry) => (entry.id === item.id ? { ...entry, bloqueado: !item.bloqueado } : entry)))
      setFeedback({ tone: 'success', message: item.bloqueado ? 'Cliente desbloqueado com sucesso.' : 'Cliente bloqueado com sucesso.' })
    } catch (error) {
      setFeedback({ tone: 'danger', message: getErrorMessage(error, 'Falha ao atualizar bloqueio do cliente.') })
    }
  }

  async function handleInactivate(item: ClienteResponse) {
    setFeedback(null)

    try {
      await clientesApi.inactivate(item.id)
      setItems((current) => current.map((entry) => (entry.id === item.id ? { ...entry, ativo: false } : entry)))
      setFeedback({ tone: 'success', message: 'Cliente inativado com sucesso.' })
    } catch (error) {
      setFeedback({ tone: 'danger', message: getErrorMessage(error, 'Falha ao inativar cliente.') })
    }
  }

  return (
    <AdminPage action={<Button onClick={openCreateModal}>Novo</Button>} feedback={feedback} title="Clientes">
      <AdminPanel>
        <div className="grid gap-4 md:grid-cols-[minmax(0,1fr)_minmax(0,220px)_auto_auto] md:items-end">
          <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
            <span>Filtrar por nome</span>
            <input className={fieldClass('')} onChange={(event) => setFilterNome(event.target.value)} placeholder="Digite o nome" value={filterNome} />
          </label>
          <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
            <span>CPF</span>
            <input className={fieldClass('')} onChange={(event) => setFilterCpf(event.target.value)} placeholder="Digite o CPF" value={filterCpf} />
          </label>
          <Button onClick={() => void handleSearch()} type="button">Buscar</Button>
          <Button onClick={() => void handleClearFilters()} type="button" variant="secondary">Limpar</Button>
        </div>

        <DataTable
          colSpan={6}
          emptyMessage={tableMessage ?? 'Nenhum cliente encontrado'}
          getRowKey={(item) => item.id}
          headers={(
            <tr>
              <th className="px-2.5 py-2 font-semibold">Nome</th>
              <th className="px-2.5 py-2 font-semibold">CPF</th>
              <th className="px-2.5 py-2 font-semibold">E-mail</th>
              <th className="px-2.5 py-2 font-semibold">Ativo</th>
              <th className="px-2.5 py-2 font-semibold">Bloqueado</th>
              <th className="px-2.5 py-2 font-semibold text-right">Ações</th>
            </tr>
          )}
          items={items}
          loading={isLoading}
          loadingMessage="Carregando clientes..."
          pagination={{
            ...pagination,
            onPageChange: (page) => void loadData({ nome: filterNome.trim() || undefined, cpf: filterCpf.trim() || undefined }, page, pagination.pageSize),
            onPageSizeChange: (pageSize) => void loadData({ nome: filterNome.trim() || undefined, cpf: filterCpf.trim() || undefined }, 1, pageSize),
          }}
          renderRow={(item) => (
            <tr key={item.id} className="border-t border-[var(--line)] bg-white/70 dark:border-slate-300 dark:bg-white">
              <td className="px-2.5 py-1.5 font-medium text-[var(--text)] dark:text-slate-900">{item.nome}</td>
              <td className="px-2.5 py-1.5 text-[var(--text-soft)] dark:text-slate-600">{item.cpf || '-'}</td>
              <td className="px-2.5 py-1.5 text-[var(--text-soft)] dark:text-slate-600">{item.email || '-'}</td>
              <td className="px-2.5 py-1.5"><Badge tone={item.ativo ? 'success' : 'danger'}>{item.ativo ? 'Sim' : 'Não'}</Badge></td>
              <td className="px-2.5 py-1.5"><Badge tone={item.bloqueado ? 'danger' : 'success'}>{item.bloqueado ? 'Sim' : 'Não'}</Badge></td>
              <td className="px-2.5 py-1.5 text-right">
                <div className="flex justify-end gap-2">
                  <Button aria-label="Editar cliente" className="h-8 w-8 !p-0" onClick={() => void openEditModal(item)} title="Editar" type="button" variant="secondary">
                    <Pencil className="h-[18px] w-[18px]" />
                  </Button>
                  <Button aria-label={item.bloqueado ? 'Desbloquear cliente' : 'Bloquear cliente'} className="h-8 w-8 !p-0" onClick={() => void handleToggleBlocked(item)} title={item.bloqueado ? 'Desbloquear' : 'Bloquear'} type="button" variant="secondary">
                    {item.bloqueado ? <LockOpen className="h-[18px] w-[18px]" /> : <Lock className="h-[18px] w-[18px]" />}
                  </Button>
                  {item.ativo ? (
                    <Button aria-label="Inativar cliente" className="h-8 w-8 !p-0" onClick={() => void handleInactivate(item)} title="Inativar" type="button" variant="danger">
                      <Power className="h-[18px] w-[18px]" />
                    </Button>
                  ) : null}
                </div>
              </td>
            </tr>
          )}
        />
      </AdminPanel>

      <Modal onClose={() => setIsModalOpen(false)} open={isModalOpen} title={editingItem ? 'Editar cliente' : 'Novo cliente'}>
        <form className="space-y-5" noValidate onSubmit={handleSubmit}>
          <div className="grid gap-4 md:grid-cols-2">
            <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
              <span>
                Nome <span className="text-red-600 dark:text-red-400">*</span>
              </span>
              <input className={fieldClass(formErrors.nome)} onChange={(event) => setForm((current) => ({ ...current, nome: event.target.value }))} onFocus={() => clearFieldError('nome')} placeholder="Informe o nome" value={form.nome} />
              {formErrors.nome ? <span className="text-xs text-red-600 dark:text-red-400">{formErrors.nome}</span> : null}
            </label>

            <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
              <span>CPF</span>
              <input className={fieldClass('')} onChange={(event) => setForm((current) => ({ ...current, cpf: formatCpf(event.target.value) }))} placeholder="Informe o CPF" value={form.cpf} />
            </label>

            <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200 md:col-span-2">
              <span>E-mail</span>
              <input className={fieldClass('')} onChange={(event) => setForm((current) => ({ ...current, email: event.target.value }))} placeholder="Informe o e-mail" type="email" value={form.email} />
            </label>

            <div className="md:col-span-2">
              <TextareaField label="Observação" onChange={(event) => setForm((current) => ({ ...current, observacao: event.target.value }))} placeholder="Opcional" value={form.observacao} />
            </div>

            <div className="md:col-span-2 mt-2 border-t border-slate-200 pt-4 dark:border-slate-800">
              <div className="flex items-center justify-between gap-3">
                <p className="text-sm font-semibold text-slate-900 dark:text-slate-100">Telefones</p>
                <Button onClick={addTelefone} type="button" variant="secondary">
                  <Plus className="mr-2 h-4 w-4" />
                  Adicionar telefone
                </Button>
              </div>
            </div>

            {form.telefones.map((telefone, index) => (
              <div className="md:col-span-2 rounded-md border border-slate-200 p-4 dark:border-slate-800" key={telefone.id ?? `telefone-${index}`}>
                <div className="mb-4 flex items-center justify-between gap-3">
                  <p className="text-sm font-semibold text-slate-900 dark:text-slate-100">Telefone {index + 1}</p>
                  {form.telefones.length > 1 ? (
                    <Button aria-label="Remover telefone" className="h-10 w-10 px-0" onClick={() => removeTelefone(index)} title="Remover telefone" type="button" variant="danger">
                      <Trash2 className="h-4 w-4" />
                    </Button>
                  ) : null}
                </div>

                <div className="grid gap-4 md:grid-cols-2">
                  <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
                    <span>
                      DDD <span className="text-red-600 dark:text-red-400">*</span>
                    </span>
                    <input
                      className={fieldClass(formErrors.telefones[index]?.ddd ?? '')}
                      onChange={(event) =>
                        setForm((current) => ({
                          ...current,
                          telefones: current.telefones.map((item, itemIndex) =>
                            itemIndex === index ? { ...item, ddd: formatDdd(event.target.value) } : item,
                          ),
                        }))
                      }
                      onFocus={() => clearTelefoneFieldError(index, 'ddd')}
                      placeholder="Ex.: 11"
                      value={telefone.ddd}
                    />
                    {formErrors.telefones[index]?.ddd ? <span className="text-xs text-red-600 dark:text-red-400">{formErrors.telefones[index].ddd}</span> : null}
                  </label>

                  <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
                    <span>
                      Número <span className="text-red-600 dark:text-red-400">*</span>
                    </span>
                    <input
                      className={fieldClass(formErrors.telefones[index]?.numero ?? '')}
                      onChange={(event) =>
                        setForm((current) => ({
                          ...current,
                          telefones: current.telefones.map((item, itemIndex) =>
                            itemIndex === index ? { ...item, numero: formatPhoneNumber(event.target.value) } : item,
                          ),
                        }))
                      }
                      onFocus={() => clearTelefoneFieldError(index, 'numero')}
                      placeholder="Informe o número"
                      value={telefone.numero}
                    />
                    {formErrors.telefones[index]?.numero ? <span className="text-xs text-red-600 dark:text-red-400">{formErrors.telefones[index].numero}</span> : null}
                  </label>
                </div>
              </div>
            ))}

            <div className="md:col-span-2 mt-2 border-t border-slate-200 pt-4 dark:border-slate-800">
              <div className="flex items-center justify-between gap-3">
                <p className="text-sm font-semibold text-slate-900 dark:text-slate-100">Endereços</p>
                <Button onClick={addEndereco} type="button" variant="secondary">
                  <Plus className="mr-2 h-4 w-4" />
                  Adicionar endereço
                </Button>
              </div>
            </div>

            {form.enderecos.map((endereco, index) => (
              <div className="md:col-span-2 rounded-md border border-slate-200 p-4 dark:border-slate-800" key={endereco.id ?? `endereco-${index}`}>
                <div className="mb-4 flex items-center justify-between gap-3">
                  <p className="text-sm font-semibold text-slate-900 dark:text-slate-100">Endereço {index + 1}</p>
                  {form.enderecos.length > 1 ? (
                    <Button aria-label="Remover endereço" className="h-10 w-10 px-0" onClick={() => removeEndereco(index)} title="Remover endereço" type="button" variant="danger">
                      <Trash2 className="h-4 w-4" />
                    </Button>
                  ) : null}
                </div>

                <div className="grid gap-4 md:grid-cols-2">
                  <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200 md:col-span-2">
                    <span>
                      Logradouro <span className="text-red-600 dark:text-red-400">*</span>
                    </span>
                    <input
                      className={fieldClass(formErrors.enderecos[index]?.logradouro ?? '')}
                      onChange={(event) =>
                        setForm((current) => ({
                          ...current,
                          enderecos: current.enderecos.map((item, itemIndex) =>
                            itemIndex === index ? { ...item, logradouro: event.target.value } : item,
                          ),
                        }))
                      }
                      onFocus={() => clearEnderecoFieldError(index, 'logradouro')}
                      placeholder="Informe o logradouro"
                      value={endereco.logradouro}
                    />
                    {formErrors.enderecos[index]?.logradouro ? <span className="text-xs text-red-600 dark:text-red-400">{formErrors.enderecos[index].logradouro}</span> : null}
                  </label>

                  <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
                    <span>
                      Cidade <span className="text-red-600 dark:text-red-400">*</span>
                    </span>
                    <input
                      className={fieldClass(formErrors.enderecos[index]?.cidade ?? '')}
                      onChange={(event) =>
                        setForm((current) => ({
                          ...current,
                          enderecos: current.enderecos.map((item, itemIndex) =>
                            itemIndex === index ? { ...item, cidade: event.target.value } : item,
                          ),
                        }))
                      }
                      onFocus={() => clearEnderecoFieldError(index, 'cidade')}
                      placeholder="Informe a cidade"
                      value={endereco.cidade}
                    />
                    {formErrors.enderecos[index]?.cidade ? <span className="text-xs text-red-600 dark:text-red-400">{formErrors.enderecos[index].cidade}</span> : null}
                  </label>

                  <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
                    <span>
                      Estado <span className="text-red-600 dark:text-red-400">*</span>
                    </span>
                    <input
                      className={fieldClass(formErrors.enderecos[index]?.estado ?? '')}
                      maxLength={2}
                      onChange={(event) =>
                        setForm((current) => ({
                          ...current,
                          enderecos: current.enderecos.map((item, itemIndex) =>
                            itemIndex === index ? { ...item, estado: event.target.value.toUpperCase().slice(0, 2) } : item,
                          ),
                        }))
                      }
                      onFocus={() => clearEnderecoFieldError(index, 'estado')}
                      placeholder="UF"
                      value={endereco.estado}
                    />
                    {formErrors.enderecos[index]?.estado ? <span className="text-xs text-red-600 dark:text-red-400">{formErrors.enderecos[index].estado}</span> : null}
                  </label>

                  <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
                    <span>
                      Número <span className="text-red-600 dark:text-red-400">*</span>
                    </span>
                    <input
                      className={fieldClass(formErrors.enderecos[index]?.numero ?? '')}
                      onChange={(event) =>
                        setForm((current) => ({
                          ...current,
                          enderecos: current.enderecos.map((item, itemIndex) =>
                            itemIndex === index ? { ...item, numero: event.target.value } : item,
                          ),
                        }))
                      }
                      onFocus={() => clearEnderecoFieldError(index, 'numero')}
                      placeholder="Número"
                      value={endereco.numero}
                    />
                    {formErrors.enderecos[index]?.numero ? <span className="text-xs text-red-600 dark:text-red-400">{formErrors.enderecos[index].numero}</span> : null}
                  </label>

                  <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
                    <span>Complemento</span>
                    <input
                      className={fieldClass('')}
                      onChange={(event) =>
                        setForm((current) => ({
                          ...current,
                          enderecos: current.enderecos.map((item, itemIndex) =>
                            itemIndex === index ? { ...item, complemento: event.target.value } : item,
                          ),
                        }))
                      }
                      placeholder="Opcional"
                      value={endereco.complemento}
                    />
                  </label>
                </div>
              </div>
            ))}
          </div>

          {editingItem ? (
            <div className="grid gap-4 md:grid-cols-2">
              <label className="flex items-center gap-3 rounded-md border border-slate-200 bg-slate-50 px-4 py-3 dark:border-slate-800 dark:bg-slate-900">
                <input checked={form.ativo} className="h-4 w-4 accent-slate-900 dark:accent-slate-100" onChange={(event) => setForm((current) => ({ ...current, ativo: event.target.checked }))} type="checkbox" />
                <span className="text-sm font-semibold text-slate-900 dark:text-slate-100">Cliente ativo</span>
              </label>
              <label className="flex items-center gap-3 rounded-md border border-slate-200 bg-slate-50 px-4 py-3 dark:border-slate-800 dark:bg-slate-900">
                <input checked={form.bloqueado} className="h-4 w-4 accent-slate-900 dark:accent-slate-100" onChange={(event) => setForm((current) => ({ ...current, bloqueado: event.target.checked }))} type="checkbox" />
                <span className="text-sm font-semibold text-slate-900 dark:text-slate-100">Cliente bloqueado</span>
              </label>
            </div>
          ) : null}

          <div className="flex justify-end gap-3 border-t border-slate-200 pt-4 dark:border-slate-800">
            <Button onClick={() => setIsModalOpen(false)} type="button" variant="ghost">Cancelar</Button>
            <Button disabled={isSaving} type="submit">{isSaving ? 'Salvando...' : 'Salvar'}</Button>
          </div>
        </form>
      </Modal>
    </AdminPage>
  )
}