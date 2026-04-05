import { Pencil } from 'lucide-react'
import { useDeferredValue, useEffect, useState, type FormEvent } from 'react'
import { Button, Feedback, Modal, PageIntro, Panel, TextField } from '../components/ui'
import { getErrorMessage } from '../lib/errors'
import { escoposApi } from '../services/sso'
import type { EscopoResponse } from '../types/api'

export function EscoposPage() {
  const [items, setItems] = useState<EscopoResponse[]>([])
  const [filter, setFilter] = useState('')
  const [feedback, setFeedback] = useState<{ tone: 'success' | 'danger'; message: string } | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [isSaving, setIsSaving] = useState(false)
  const [isModalOpen, setIsModalOpen] = useState(false)
  const [editingItem, setEditingItem] = useState<EscopoResponse | null>(null)
  const [nome, setNome] = useState('')
  const [nomeError, setNomeError] = useState('')
  const deferredFilter = useDeferredValue(filter)

  const filteredItems = items.filter((item) =>
    item.nome?.toLocaleLowerCase().includes(deferredFilter.toLocaleLowerCase()),
  )

  async function loadData() {
    setIsLoading(true)

    try {
      const response = await escoposApi.list()
      setItems(response)
    } catch (error) {
      setFeedback({ tone: 'danger', message: getErrorMessage(error, 'Falha ao carregar escopos.') })
    } finally {
      setIsLoading(false)
    }
  }

  useEffect(() => {
    void loadData()
  }, [])

  function openCreateModal() {
    setEditingItem(null)
    setNome('')
    setNomeError('')
    setIsModalOpen(true)
  }

  async function openEditModal(item: EscopoResponse) {
    setIsLoading(true)

    try {
      const fullItem = await escoposApi.getById(item.id)
      setEditingItem(fullItem)
      setNome(fullItem.nome ?? '')
      setNomeError('')
      setIsModalOpen(true)
    } catch (error) {
      setFeedback({ tone: 'danger', message: getErrorMessage(error, 'Falha ao carregar o escopo selecionado.') })
    } finally {
      setIsLoading(false)
    }
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setFeedback(null)

    if (!nome.trim()) {
      setNomeError('O campo nome é obrigatório.')
      return
    }

    setIsSaving(true)

    try {
      if (editingItem) {
        await escoposApi.update(editingItem.id, { nome: nome.trim() })
        setFeedback({ tone: 'success', message: 'Escopo atualizado com sucesso.' })
      } else {
        await escoposApi.create({ nome: nome.trim() })
        setFeedback({ tone: 'success', message: 'Escopo criado com sucesso.' })
      }

      setIsModalOpen(false)
      await loadData()
    } catch (error) {
      setFeedback({ tone: 'danger', message: getErrorMessage(error, 'Falha ao salvar escopo.') })
    } finally {
      setIsSaving(false)
    }
  }

  return (
    <div className="space-y-6">
      <PageIntro
        action={<Button onClick={openCreateModal}>Novo</Button>}
        eyebrow=""
        title="Escopos"
        description=""
      />

      {feedback ? <Feedback tone={feedback.tone}>{feedback.message}</Feedback> : null}

      <Panel className="p-5 md:p-6">
        <div className="grid gap-4 md:grid-cols-[minmax(0,320px)_1fr] md:items-end">
          <TextField label="Filtrar por nome" onChange={(event) => setFilter(event.target.value)} placeholder="Digite para pesquisar" value={filter} />
        </div>
        {isLoading ? (
          <div className="mt-6 rounded-md border border-[var(--line)] bg-[var(--surface-strong)] px-5 py-8 text-sm text-[var(--text-soft)]">Carregando escopos...</div>
        ) : (
          <div className="mt-6 overflow-hidden rounded-md border border-[var(--line)] dark:border-slate-300">
            <div className="overflow-x-auto">
              <table className="min-w-full border-collapse text-left text-sm">
                <thead className="bg-[var(--surface-strong)] text-[var(--text-soft)] dark:bg-white dark:text-slate-700">
                  <tr>
                    <th className="px-4 py-3 font-semibold">Id</th>
                    <th className="px-4 py-3 font-semibold">Nome</th>
                    <th className="px-4 py-3 font-semibold text-right">Ações</th>
                  </tr>
                </thead>
                <tbody>
                  {filteredItems.length > 0 ? (
                    filteredItems.map((item) => (
                      <tr key={item.id} className="border-t border-[var(--line)] bg-white/70 dark:border-slate-300 dark:bg-white">
                        <td className="px-4 py-3 font-mono-ui text-xs text-[var(--text-soft)] dark:text-slate-500">#{item.id}</td>
                        <td className="px-4 py-3 font-medium text-[var(--text)] dark:text-slate-900">{item.nome}</td>
                        <td className="px-4 py-3 text-right">
                          <Button
                            aria-label="Editar escopo"
                            className="h-11 w-11 px-0"
                            onClick={() => void openEditModal(item)}
                            title="Editar"
                            type="button"
                            variant="secondary"
                          >
                            <Pencil className="h-5 w-5" />
                          </Button>
                        </td>
                      </tr>
                    ))
                  ) : (
                    <tr className="border-t border-[var(--line)] bg-white/70 dark:border-slate-300 dark:bg-white">
                      <td className="px-4 py-6 text-sm text-[var(--text-soft)] dark:text-slate-600" colSpan={3}>
                        Nenhum escopo encontrado
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          </div>
        )}
      </Panel>

      <Modal
        onClose={() => setIsModalOpen(false)}
        open={isModalOpen}
        title={editingItem ? 'Editar escopo' : 'Novo escopo'}
      >
        <form className="space-y-5" noValidate onSubmit={handleSubmit}>
          <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
            <span>
              Nome <span className="text-red-600 dark:text-red-400">*</span>
            </span>
            <input
              className={[
                'w-full rounded-md border bg-white px-3 py-2 text-sm text-slate-900 outline-none transition-colors placeholder:text-slate-400 dark:bg-white dark:text-slate-900 dark:placeholder:text-slate-500',
                nomeError
                  ? 'border-red-500 focus:border-red-600 focus:ring-2 focus:ring-red-100 dark:border-red-400 dark:focus:border-red-300 dark:focus:ring-red-950'
                  : 'border-slate-300 focus:border-slate-900 focus:ring-2 focus:ring-slate-200 dark:border-slate-700 dark:focus:border-slate-100 dark:focus:ring-slate-800',
              ].join(' ')}
              onChange={(event) => setNome(event.target.value)}
              onFocus={() => setNomeError('')}
              placeholder="Informe o nome"
              value={nome}
            />
            {nomeError ? <span className="text-xs text-red-600 dark:text-red-400">{nomeError}</span> : null}
          </label>
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