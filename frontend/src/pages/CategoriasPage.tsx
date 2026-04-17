import { Pencil, Power } from 'lucide-react'
import { useEffect, useRef, useState, type FormEvent } from 'react'
import { Badge, Button, Feedback, Modal, PageIntro, Panel } from '../components/ui'
import { getErrorMessage } from '../lib/errors'
import { categoriasApi } from '../services/sso'
import type { CategoriaResponse } from '../types/api'

interface CategoriaFormState {
  nome: string
  ativo: boolean
}

const initialFormState: CategoriaFormState = {
  nome: '',
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

export function CategoriasPage() {
  const didLoadInitiallyRef = useRef(false)
  const [items, setItems] = useState<CategoriaResponse[]>([])
  const [filterNome, setFilterNome] = useState('')
  const [feedback, setFeedback] = useState<{ tone: 'success' | 'danger'; message: string } | null>(null)
  const [tableMessage, setTableMessage] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [isSaving, setIsSaving] = useState(false)
  const [isModalOpen, setIsModalOpen] = useState(false)
  const [editingItem, setEditingItem] = useState<CategoriaResponse | null>(null)
  const [form, setForm] = useState<CategoriaFormState>(initialFormState)
  const [nomeError, setNomeError] = useState('')

  async function loadData(nome?: string) {
    setIsLoading(true)

    try {
      const response = nome?.trim() ? await categoriasApi.listFilter(nome.trim()) : await categoriasApi.list()
      setItems(response)
      setTableMessage(response.length === 0 ? 'Nenhuma categoria encontrada' : null)
    } catch (error) {
      setItems([])
      setTableMessage(getErrorMessage(error, 'Falha ao carregar categorias.'))
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
    setNomeError('')
    setIsModalOpen(true)
  }

  async function openEditModal(item: CategoriaResponse) {
    setIsLoading(true)

    try {
      const fullItem = await categoriasApi.getById(item.id)
      setEditingItem(fullItem)
      setForm({
        nome: fullItem.nome ?? '',
        ativo: fullItem.ativo,
      })
      setNomeError('')
      setIsModalOpen(true)
    } catch (error) {
      setFeedback({ tone: 'danger', message: getErrorMessage(error, 'Falha ao carregar a categoria selecionada.') })
    } finally {
      setIsLoading(false)
    }
  }

  async function handleFilterKeyUp(value: string) {
    const normalizedValue = value.trim()

    if (normalizedValue.length >= 3) {
      await loadData(normalizedValue)
      return
    }

    if (normalizedValue.length === 0) {
      await loadData()
    }
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setFeedback(null)

    if (!form.nome.trim()) {
      setNomeError('O campo nome é obrigatório.')
      return
    }

    setIsSaving(true)

    try {
      if (editingItem) {
        await categoriasApi.update(editingItem.id, {
          nome: form.nome.trim(),
          ativo: form.ativo,
        })
        setFeedback({ tone: 'success', message: 'Categoria atualizada com sucesso.' })
      } else {
        await categoriasApi.create({ nome: form.nome.trim() })
        setFeedback({ tone: 'success', message: 'Categoria criada com sucesso.' })
      }

      setIsModalOpen(false)
      await loadData(filterNome)
    } catch (error) {
      setFeedback({ tone: 'danger', message: getErrorMessage(error, 'Falha ao salvar categoria.') })
    } finally {
      setIsSaving(false)
    }
  }

  async function handleInactivate(item: CategoriaResponse) {
    setFeedback(null)

    try {
      await categoriasApi.inactivate(item.id)
      setItems((current) => current.map((entry) => (entry.id === item.id ? { ...entry, ativo: false } : entry)))
      setFeedback({ tone: 'success', message: 'Categoria inativada com sucesso.' })
    } catch (error) {
      setFeedback({ tone: 'danger', message: getErrorMessage(error, 'Falha ao inativar categoria.') })
    }
  }

  return (
    <div className="space-y-6">
      <PageIntro action={<Button onClick={openCreateModal}>Novo</Button>} eyebrow="" title="Categorias" description="" />

      {feedback ? <Feedback tone={feedback.tone}>{feedback.message}</Feedback> : null}

      <Panel className="p-5 md:p-6">
        <div className="max-w-md">
          <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
            <span>Filtrar por nome</span>
            <input
              className="w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900 outline-none transition-colors placeholder:text-slate-400 focus:border-slate-900 focus:ring-2 focus:ring-slate-200 dark:border-slate-300 dark:bg-white dark:text-slate-900 dark:placeholder:text-slate-500 dark:focus:border-slate-900 dark:focus:ring-slate-200"
              onChange={(event) => setFilterNome(event.target.value)}
              onKeyUp={(event) => void handleFilterKeyUp(event.currentTarget.value)}
              placeholder="Digite para pesquisar"
              value={filterNome}
            />
          </label>
        </div>

        {isLoading ? (
          <div className="mt-6 rounded-md border border-[var(--line)] bg-[var(--surface-strong)] px-5 py-8 text-sm text-[var(--text-soft)]">Carregando categorias...</div>
        ) : (
          <div className="mt-6 overflow-hidden rounded-md border border-[var(--line)] dark:border-slate-300">
            <div className="overflow-x-auto">
              <table className="min-w-full border-collapse text-left text-sm">
                <thead className="bg-[var(--surface-strong)] text-[var(--text-soft)] dark:bg-white dark:text-slate-700">
                  <tr>
                    <th className="px-4 py-3 font-semibold">Nome</th>
                    <th className="px-4 py-3 font-semibold">Status</th>
                    <th className="px-4 py-3 font-semibold text-right">Ações</th>
                  </tr>
                </thead>
                <tbody>
                  {items.length > 0 ? (
                    items.map((item) => (
                      <tr key={item.id} className="border-t border-[var(--line)] bg-white/70 dark:border-slate-300 dark:bg-white">
                        <td className="px-4 py-3 font-medium text-[var(--text)] dark:text-slate-900">{item.nome}</td>
                        <td className="px-4 py-3">
                          <Badge tone={item.ativo ? 'success' : 'danger'}>{item.ativo ? 'Ativa' : 'Inativa'}</Badge>
                        </td>
                        <td className="px-4 py-3 text-right">
                          <div className="flex justify-end gap-2">
                            <Button aria-label="Editar categoria" className="h-11 w-11 px-0" onClick={() => void openEditModal(item)} title="Editar" type="button" variant="secondary">
                              <Pencil className="h-5 w-5" />
                            </Button>
                            {item.ativo ? (
                              <Button aria-label="Inativar categoria" className="h-11 w-11 px-0" onClick={() => void handleInactivate(item)} title="Inativar" type="button" variant="danger">
                                <Power className="h-5 w-5" />
                              </Button>
                            ) : null}
                          </div>
                        </td>
                      </tr>
                    ))
                  ) : (
                    <tr className="border-t border-[var(--line)] bg-white/70 dark:border-slate-300 dark:bg-white">
                      <td className="px-4 py-6 text-sm text-[var(--text-soft)] dark:text-slate-600" colSpan={3}>
                        {tableMessage ?? 'Nenhuma categoria encontrada'}
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          </div>
        )}
      </Panel>

      <Modal onClose={() => setIsModalOpen(false)} open={isModalOpen} title={editingItem ? 'Editar categoria' : 'Nova categoria'}>
        <form className="space-y-5" noValidate onSubmit={handleSubmit}>
          <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
            <span>
              Nome <span className="text-red-600 dark:text-red-400">*</span>
            </span>
            <input
              className={fieldClass(nomeError)}
              onChange={(event) => setForm((current) => ({ ...current, nome: event.target.value }))}
              onFocus={() => setNomeError('')}
              placeholder="Informe o nome"
              value={form.nome}
            />
            {nomeError ? <span className="text-xs text-red-600 dark:text-red-400">{nomeError}</span> : null}
          </label>

          {editingItem ? (
            <label className="flex items-center gap-3 rounded-md border border-slate-200 bg-slate-50 px-4 py-3 dark:border-slate-800 dark:bg-slate-900">
              <input checked={form.ativo} className="h-4 w-4 accent-slate-900 dark:accent-slate-100" onChange={(event) => setForm((current) => ({ ...current, ativo: event.target.checked }))} type="checkbox" />
              <span className="text-sm font-semibold text-slate-900 dark:text-slate-100">Categoria ativa</span>
            </label>
          ) : null}

          <div className="flex justify-end gap-3 border-t border-slate-200 pt-4 dark:border-slate-800">
            <Button onClick={() => setIsModalOpen(false)} type="button" variant="ghost">
              Cancelar
            </Button>
            <Button disabled={isSaving} type="submit">
              {isSaving ? 'Salvando...' : 'Salvar'}
            </Button>
          </div>
        </form>
      </Modal>
    </div>
  )
}