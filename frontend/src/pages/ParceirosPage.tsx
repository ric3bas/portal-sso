import { Pencil } from 'lucide-react'
import { useEffect, useState, type FormEvent } from 'react'
import { Badge, Button, Feedback, Modal, PageIntro, Panel, CheckboxField, TextareaField } from '../components/ui'
import { getErrorMessage } from '../lib/errors'
import { parceirosApi } from '../services/sso'
import type { ParceiroResponse } from '../types/api'

export function ParceirosPage() {
  const [items, setItems] = useState<ParceiroResponse[]>([])
  const [filterNome, setFilterNome] = useState('')
  const [feedback, setFeedback] = useState<{ tone: 'success' | 'danger'; message: string } | null>(null)
  const [tableMessage, setTableMessage] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [isSaving, setIsSaving] = useState(false)
  const [isModalOpen, setIsModalOpen] = useState(false)
  const [editingItem, setEditingItem] = useState<ParceiroResponse | null>(null)
  const [form, setForm] = useState({ nome: '', descricao: '', ativo: true })
  const [nomeError, setNomeError] = useState('')

  async function loadData(nome?: string) {
    setIsLoading(true)

    try {
      const response = await parceirosApi.list(nome)
      setItems(response)
      setTableMessage(response.length === 0 ? 'Nenhum parceiro encontrado' : null)
    } catch (error) {
      setItems([])
      setTableMessage(getErrorMessage(error, 'Falha ao carregar parceiros.'))
    } finally {
      setIsLoading(false)
    }
  }

  useEffect(() => {
    void loadData()
  }, [])

  function openCreateModal() {
    setEditingItem(null)
    setForm({ nome: '', descricao: '', ativo: true })
    setNomeError('')
    setIsModalOpen(true)
  }

  async function openEditModal(item: ParceiroResponse) {
    setIsLoading(true)

    try {
      const fullItem = await parceirosApi.getById(item.id)
      setEditingItem(fullItem)
      setForm({
        nome: fullItem.nome ?? '',
        descricao: fullItem.descricao ?? '',
        ativo: fullItem.ativo,
      })
      setNomeError('')
      setIsModalOpen(true)
    } catch (error) {
      setFeedback({ tone: 'danger', message: getErrorMessage(error, 'Falha ao carregar parceiro selecionado.') })
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
        await parceirosApi.update(editingItem.id, { ...form, nome: form.nome.trim() })
        setFeedback({ tone: 'success', message: 'Parceiro atualizado com sucesso.' })
      } else {
        await parceirosApi.create({ nome: form.nome.trim(), descricao: form.descricao })
        setFeedback({ tone: 'success', message: 'Parceiro criado com sucesso.' })
      }

      setIsModalOpen(false)
      await loadData(filterNome)
    } catch (error) {
      setFeedback({ tone: 'danger', message: getErrorMessage(error, 'Falha ao salvar parceiro.') })
    } finally {
      setIsSaving(false)
    }
  }

  return (
    <div className="space-y-6">
      <PageIntro
        action={<Button onClick={openCreateModal}>Novo</Button>}
        eyebrow=""
        title="Parceiros"
        description=""
      />

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
          <div className="mt-6 rounded-md border border-[var(--line)] bg-[var(--surface-strong)] px-5 py-8 text-sm text-[var(--text-soft)]">Carregando parceiros...</div>
        ) : (
          <div className="mt-6 overflow-hidden rounded-md border border-[var(--line)] dark:border-slate-300">
            <div className="overflow-x-auto">
              <table className="min-w-full border-collapse text-left text-sm">
                <thead className="bg-[var(--surface-strong)] text-[var(--text-soft)] dark:bg-white dark:text-slate-700">
                  <tr>
                    <th className="px-4 py-3 font-semibold">Nome</th>
                    <th className="px-4 py-3 font-semibold">Descricao</th>
                    <th className="px-4 py-3 font-semibold">Status</th>
                    <th className="px-4 py-3 font-semibold text-right">Ações</th>
                  </tr>
                </thead>
                <tbody>
                  {items.length > 0 ? (
                    items.map((item) => (
                      <tr key={item.id} className="border-t border-[var(--line)] bg-white/70 align-top dark:border-slate-300 dark:bg-white">
                        <td className="px-4 py-3 font-medium text-[var(--text)] dark:text-slate-900">{item.nome}</td>
                        <td className="px-4 py-3 text-[var(--text-soft)] dark:text-slate-600">{item.descricao || '-'}</td>
                        <td className="px-4 py-3">
                          <Badge tone={item.ativo ? 'success' : 'danger'}>{item.ativo ? 'Ativo' : 'Inativo'}</Badge>
                        </td>
                        <td className="px-4 py-3 text-right">
                          <Button
                            aria-label="Editar parceiro"
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
                      <td className="px-4 py-6 text-sm text-[var(--text-soft)] dark:text-slate-600" colSpan={5}>
                        {tableMessage ?? 'Nenhum parceiro encontrado'}
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
        title={editingItem ? 'Editar parceiro' : 'Novo parceiro'}
      >
        <form className="space-y-5" noValidate onSubmit={handleSubmit}>
          <div className="grid gap-4 md:grid-cols-2">
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
                onChange={(event) => setForm((current) => ({ ...current, nome: event.target.value }))}
                onFocus={() => setNomeError('')}
                placeholder="Informe o nome"
                value={form.nome}
              />
              {nomeError ? <span className="text-xs text-red-600 dark:text-red-400">{nomeError}</span> : null}
            </label>
            <TextareaField
              label="Descricao"
              onChange={(event) => setForm((current) => ({ ...current, descricao: event.target.value }))}
              placeholder="Opcional"
              value={form.descricao}
            />
          </div>

          {editingItem ? (
            <CheckboxField
              checked={form.ativo}
              label="Parceiro ativo"
              onChange={(event) => setForm((current) => ({ ...current, ativo: event.target.checked }))}
            />
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