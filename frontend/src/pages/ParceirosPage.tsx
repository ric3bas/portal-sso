import { Pencil } from 'lucide-react'
import { useEffect, useRef, useState, type FormEvent } from 'react'
import { AdminPage, AdminPanel, FilterBar, FormModalFooter, RowActions } from '../components/admin'
import { DataTable } from '../components/DataTable'
import { Badge, Button, ColorField, Modal, CheckboxField, TextareaField } from '../components/ui'
import { getErrorMessage } from '../lib/errors'
import { parceirosApi } from '../services/sso'
import type { PaginatedResult, ParceiroResponse } from '../types/api'

const DEFAULT_PRIMARY_COLOR = '#0F172A'
const DEFAULT_SECONDARY_COLOR = '#CBD5E1'

function normalizeHexColor(value?: string | null, fallback = DEFAULT_PRIMARY_COLOR) {
  if (!value) {
    return fallback
  }

  const normalizedValue = value.trim().toUpperCase()

  return /^#[0-9A-F]{6}$/.test(normalizedValue) ? normalizedValue : fallback
}

function createInitialFormState() {
  return {
    nome: '',
    descricao: '',
    corPrimaria: DEFAULT_PRIMARY_COLOR,
    corSecundaria: DEFAULT_SECONDARY_COLOR,
    ativo: true,
  }
}

export function ParceirosPage() {
  const didLoadInitiallyRef = useRef(false)
  const [items, setItems] = useState<ParceiroResponse[]>([])
  const [filterNome, setFilterNome] = useState('')
  const [feedback, setFeedback] = useState<{ tone: 'success' | 'danger'; message: string } | null>(null)
  const [tableMessage, setTableMessage] = useState<string | null>(null)
  const [pagination, setPagination] = useState<PaginatedResult<ParceiroResponse>['pagination']>({ page: 1, pageSize: 20, totalRecords: 0, totalPages: 1 })
  const [isLoading, setIsLoading] = useState(true)
  const [isSaving, setIsSaving] = useState(false)
  const [isModalOpen, setIsModalOpen] = useState(false)
  const [editingItem, setEditingItem] = useState<ParceiroResponse | null>(null)
  const [form, setForm] = useState(createInitialFormState)
  const [nomeError, setNomeError] = useState('')

  async function loadData(nome?: string, page = pagination.page, pageSize = pagination.pageSize) {
    setIsLoading(true)

    try {
      const response = nome?.trim()
        ? await parceirosApi.listFilterPage(nome, { Pagina: page, TamanhoPagina: pageSize })
        : await parceirosApi.listPage({ Pagina: page, TamanhoPagina: pageSize })
      setItems(response.items)
      setPagination(response.pagination)
      setTableMessage(response.items.length === 0 ? 'Nenhum parceiro encontrado' : null)
    } catch (error) {
      setItems([])
      setPagination((current) => ({ ...current, totalRecords: 0, totalPages: 1 }))
      setTableMessage(getErrorMessage(error, 'Falha ao carregar parceiros.'))
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
    setForm(createInitialFormState())
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
        corPrimaria: normalizeHexColor(fullItem.corPrimaria, DEFAULT_PRIMARY_COLOR),
        corSecundaria: normalizeHexColor(fullItem.corSecundaria, DEFAULT_SECONDARY_COLOR),
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
      await loadData(normalizedValue, 1, pagination.pageSize)
      return
    }

    if (normalizedValue.length === 0) {
      await loadData(undefined, 1, pagination.pageSize)
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
        await parceirosApi.create({
          nome: form.nome.trim(),
          descricao: form.descricao,
          corPrimaria: form.corPrimaria,
          corSecundaria: form.corSecundaria,
        })
        setFeedback({ tone: 'success', message: 'Parceiro criado com sucesso.' })
      }

      setIsModalOpen(false)
      await loadData(filterNome, pagination.page, pagination.pageSize)
    } catch (error) {
      setFeedback({ tone: 'danger', message: getErrorMessage(error, 'Falha ao salvar parceiro.') })
    } finally {
      setIsSaving(false)
    }
  }

  return (
    <AdminPage action={<Button onClick={openCreateModal}>Novo</Button>} feedback={feedback} title="Parceiros">
      <AdminPanel>
        <FilterBar className="md:block">
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
        </FilterBar>

        <DataTable
          colSpan={4}
          emptyMessage={tableMessage ?? 'Nenhum parceiro encontrado'}
          getRowKey={(item) => item.id}
          headers={(
            <tr>
              <th className="px-2.5 py-2 font-semibold">Nome</th>
              <th className="px-2.5 py-2 font-semibold">Descricao</th>
              <th className="px-2.5 py-2 font-semibold">Status</th>
              <th className="px-2.5 py-2 font-semibold text-right">Ações</th>
            </tr>
          )}
          items={items}
          loading={isLoading}
          loadingMessage="Carregando parceiros..."
          pagination={{
            ...pagination,
            onPageChange: (page) => void loadData(filterNome || undefined, page, pagination.pageSize),
            onPageSizeChange: (pageSize) => void loadData(filterNome || undefined, 1, pageSize),
          }}
          renderRow={(item) => (
            <tr key={item.id} className="border-t border-[var(--line)] bg-white/70 align-top dark:border-slate-300 dark:bg-white">
              <td className="px-2.5 py-1.5 font-medium text-[var(--text)] dark:text-slate-900">{item.nome}</td>
              <td className="px-2.5 py-1.5 text-[var(--text-soft)] dark:text-slate-600">{item.descricao || '-'}</td>
              <td className="px-2.5 py-1.5">
                <Badge tone={item.ativo ? 'success' : 'danger'}>{item.ativo ? 'Ativo' : 'Inativo'}</Badge>
              </td>
              <td className="px-2.5 py-1.5 text-right">
                <RowActions>
                  <Button
                    aria-label="Editar parceiro"
                    className="h-8 w-8 !p-0"
                    onClick={() => void openEditModal(item)}
                    title="Editar"
                    type="button"
                    variant="secondary"
                  >
                    <Pencil className="h-[18px] w-[18px]" />
                  </Button>
                </RowActions>
              </td>
            </tr>
          )}
        />
      </AdminPanel>

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

          <div className="grid gap-4 md:grid-cols-2">
            <ColorField
              hint="Cor principal da identidade visual do parceiro"
              label="Cor primária"
              onChange={(value) => setForm((current) => ({ ...current, corPrimaria: normalizeHexColor(value, current.corPrimaria) }))}
              value={form.corPrimaria}
            />
            <ColorField
              hint="Cor de apoio usada em botões, detalhes e contraste"
              label="Cor secundária"
              onChange={(value) => setForm((current) => ({ ...current, corSecundaria: normalizeHexColor(value, current.corSecundaria) }))}
              value={form.corSecundaria}
            />
          </div>

          {editingItem ? (
            <CheckboxField
              checked={form.ativo}
              label="Parceiro ativo"
              onChange={(event) => setForm((current) => ({ ...current, ativo: event.target.checked }))}
            />
          ) : null}

          <FormModalFooter isSaving={isSaving} onCancel={() => setIsModalOpen(false)} />
        </form>
      </Modal>
    </AdminPage>
  )
}