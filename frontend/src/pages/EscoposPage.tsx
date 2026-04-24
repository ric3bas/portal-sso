import { Pencil } from 'lucide-react'
import { useDeferredValue, useEffect, useState, type FormEvent } from 'react'
import { AdminPage, AdminPanel } from '../components/admin'
import { DataTable, type DataTableColumn } from '../components/DataTable'
import { Button, Modal, TextField } from '../components/ui'
import { getErrorMessage } from '../lib/errors'
import { escoposApi } from '../services/sso'
import type { EscopoResponse, PaginatedResult } from '../types/api'

let initialEscoposLoadPromise: Promise<PaginatedResult<EscopoResponse>> | null = null

function loadInitialEscoposPage() {
  if (!initialEscoposLoadPromise) {
    initialEscoposLoadPromise = escoposApi.listPage({ Pagina: 1, TamanhoPagina: 20 }).finally(() => {
      initialEscoposLoadPromise = null
    })
  }

  return initialEscoposLoadPromise
}

export function EscoposPage() {
  const [items, setItems] = useState<EscopoResponse[]>([])
  const [filter, setFilter] = useState('')
  const [feedback, setFeedback] = useState<{ tone: 'success' | 'danger'; message: string } | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [isSaving, setIsSaving] = useState(false)
  const [tableMessage, setTableMessage] = useState<string | null>(null)
  const [pagination, setPagination] = useState<PaginatedResult<EscopoResponse>['pagination']>({ page: 1, pageSize: 20, totalRecords: 0, totalPages: 1 })
  const [isModalOpen, setIsModalOpen] = useState(false)
  const [editingItem, setEditingItem] = useState<EscopoResponse | null>(null)
  const [nome, setNome] = useState('')
  const [nomeError, setNomeError] = useState('')
  const deferredFilter = useDeferredValue(filter)

  const filteredItems = items.filter((item) =>
    item.nome?.toLocaleLowerCase().includes(deferredFilter.toLocaleLowerCase()),
  )

  async function loadData(page = pagination.page, pageSize = pagination.pageSize) {
    setIsLoading(true)

    try {
      const response = page === 1 && pageSize === 20
        ? await loadInitialEscoposPage()
        : await escoposApi.listPage({ Pagina: page, TamanhoPagina: pageSize })
      setItems(response.items)
      setPagination(response.pagination)
      setTableMessage(response.items.length === 0 ? 'Nenhum escopo encontrado' : null)
    } catch (error) {
      setItems([])
      setPagination((current) => ({ ...current, totalRecords: 0, totalPages: 1 }))
      setTableMessage(getErrorMessage(error, 'Falha ao carregar escopos.'))
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
        await loadData(pagination.page, pagination.pageSize)
    } catch (error) {
      setFeedback({ tone: 'danger', message: getErrorMessage(error, 'Falha ao salvar escopo.') })
    } finally {
      setIsSaving(false)
    }
  }

  return (
    <AdminPage action={<Button onClick={openCreateModal}>Novo</Button>} feedback={feedback} title="Escopos">
      <AdminPanel>
        <div className="grid gap-4 md:grid-cols-[minmax(0,320px)_1fr] md:items-end">
          <TextField label="Filtrar por nome" onChange={(event) => setFilter(event.target.value)} placeholder="Digite para pesquisar" value={filter} />
        </div>
        <DataTable
          columns={[
            {
              key: 'nome',
              header: 'Nome',
              cellClassName: 'font-medium text-[var(--text)] dark:text-slate-900',
              renderCell: (item) => item.nome,
            },
            {
              key: 'acoes',
              header: 'Ações',
              headerClassName: 'text-right',
              cellClassName: 'text-right',
              renderCell: (item) => (
                <Button
                  aria-label="Editar escopo"
                  className="h-8 w-8 !p-0"
                  onClick={() => void openEditModal(item)}
                  title="Editar"
                  type="button"
                  variant="secondary"
                >
                  <Pencil className="h-[18px] w-[18px]" />
                </Button>
              ),
            },
          ] satisfies DataTableColumn<EscopoResponse>[]}
          emptyMessage={tableMessage ?? 'Nenhum escopo encontrado'}
          getRowKey={(item) => item.id}
          items={filteredItems}
          loading={isLoading}
          loadingMessage="Carregando escopos..."
          pagination={{
            ...pagination,
            totalRecords: filter.trim() ? filteredItems.length : pagination.totalRecords,
            totalPages: filter.trim() ? 1 : pagination.totalPages,
            onPageChange: (page) => void loadData(page, pagination.pageSize),
            onPageSizeChange: (pageSize) => void loadData(1, pageSize),
          }}
        />
      </AdminPanel>

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
    </AdminPage>
  )
}