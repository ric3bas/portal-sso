import { Pencil, Power } from 'lucide-react'
import { useEffect, useRef, useState, type FormEvent } from 'react'
import { AdminPage, AdminPanel, FilterBar, FormModalFooter, RowActions } from '../components/admin'
import { DataTable, type DataTableColumn } from '../components/DataTable'
import { Badge, Button, Modal } from '../components/ui'
import { getErrorMessage } from '../lib/errors'
import { categoriasApi } from '../services/sso'
import type { CategoriaResponse, PaginatedResult } from '../types/api'

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
  const [pagination, setPagination] = useState<PaginatedResult<CategoriaResponse>['pagination']>({ page: 1, pageSize: 20, totalRecords: 0, totalPages: 1 })
  const [isLoading, setIsLoading] = useState(true)
  const [isSaving, setIsSaving] = useState(false)
  const [isModalOpen, setIsModalOpen] = useState(false)
  const [editingItem, setEditingItem] = useState<CategoriaResponse | null>(null)
  const [form, setForm] = useState<CategoriaFormState>(initialFormState)
  const [nomeError, setNomeError] = useState('')

  async function loadData(nome?: string, page = pagination.page, pageSize = pagination.pageSize) {
    setIsLoading(true)

    try {
      const response = nome?.trim()
        ? await categoriasApi.listFilterPage(nome.trim(), { Pagina: page, TamanhoPagina: pageSize })
        : await categoriasApi.listPage({ Pagina: page, TamanhoPagina: pageSize })
      setItems(response.items)
      setPagination(response.pagination)
      setTableMessage(response.items.length === 0 ? 'Nenhuma categoria encontrada' : null)
    } catch (error) {
      setItems([])
      setPagination((current) => ({ ...current, totalRecords: 0, totalPages: 1 }))
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
      await loadData(filterNome, pagination.page, pagination.pageSize)
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

  const columns: DataTableColumn<CategoriaResponse>[] = [
    {
      key: 'nome',
      header: 'Nome',
      cellClassName: 'font-medium text-[var(--text)] dark:text-slate-900',
      renderCell: (item) => item.nome,
    },
    {
      key: 'status',
      header: 'Status',
      renderCell: (item) => <Badge tone={item.ativo ? 'success' : 'danger'}>{item.ativo ? 'Ativa' : 'Inativa'}</Badge>,
    },
    {
      key: 'acoes',
      header: 'Ações',
      headerClassName: 'text-right',
      cellClassName: 'text-right',
      renderCell: (item) => (
        <RowActions>
          <Button aria-label="Editar categoria" className="h-8 w-8 !p-0" onClick={() => void openEditModal(item)} title="Editar" type="button" variant="secondary">
            <Pencil className="h-[18px] w-[18px]" />
          </Button>
          {item.ativo ? (
            <Button aria-label="Inativar categoria" className="h-8 w-8 !p-0" onClick={() => void handleInactivate(item)} title="Inativar" type="button" variant="danger">
              <Power className="h-[18px] w-[18px]" />
            </Button>
          ) : null}
        </RowActions>
      ),
    },
  ]

  return (
    <AdminPage action={<Button onClick={openCreateModal}>Novo</Button>} feedback={feedback} title="Categorias">
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
          columns={columns}
          emptyMessage={tableMessage ?? 'Nenhuma categoria encontrada'}
          getRowKey={(item) => item.id}
          items={items}
          loading={isLoading}
          loadingMessage="Carregando categorias..."
          pagination={{
            ...pagination,
            onPageChange: (page) => void loadData(filterNome || undefined, page, pagination.pageSize),
            onPageSizeChange: (pageSize) => void loadData(filterNome || undefined, 1, pageSize),
          }}
        />
      </AdminPanel>

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

          <FormModalFooter isSaving={isSaving} onCancel={() => setIsModalOpen(false)} />
        </form>
      </Modal>
    </AdminPage>
  )
}