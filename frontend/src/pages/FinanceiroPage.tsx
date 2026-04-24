import { useEffect, useRef, useState } from 'react'
import { AdminPage, AdminPanel, FilterBar } from '../components/admin'
import { DataTable, type DataTableColumn } from '../components/DataTable'
import { Button } from '../components/ui'
import { getErrorMessage } from '../lib/errors'
import { financeiroApi } from '../services/sso'
import type { FinanceiroResponse, PaginatedResult } from '../types/api'

function formatCurrency(value: number) {
  return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value)
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

function toIsoDateTime(value: string) {
  return value ? new Date(value).toISOString() : undefined
}

export function FinanceiroPage() {
  const didLoadInitiallyRef = useRef(false)
  const [items, setItems] = useState<FinanceiroResponse[]>([])
  const [dataInicio, setDataInicio] = useState('')
  const [dataFim, setDataFim] = useState('')
  const [tableMessage, setTableMessage] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [pagination, setPagination] = useState<PaginatedResult<FinanceiroResponse>['pagination']>({ page: 1, pageSize: 20, totalRecords: 0, totalPages: 1 })

  async function loadData(filters?: { dataInicio?: string; dataFim?: string }, page = pagination.page, pageSize = pagination.pageSize) {
    setIsLoading(true)

    try {
      const response = filters?.dataInicio || filters?.dataFim
        ? await financeiroApi.listByPeriodPage(filters, { Pagina: page, TamanhoPagina: pageSize })
        : await financeiroApi.listPage({ Pagina: page, TamanhoPagina: pageSize })

      setItems(response.items)
      setPagination(response.pagination)
      setTableMessage(response.items.length === 0 ? 'Nenhum lançamento encontrado' : null)
    } catch (error) {
      setItems([])
      setPagination((current) => ({ ...current, totalRecords: 0, totalPages: 1 }))
      setTableMessage(getErrorMessage(error, 'Falha ao carregar lançamentos financeiros.'))
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

  async function handleSearch() {
    await loadData({
      dataInicio: toIsoDateTime(dataInicio),
      dataFim: toIsoDateTime(dataFim),
    }, 1, pagination.pageSize)
  }

  async function handleClear() {
    setDataInicio('')
    setDataFim('')
    await loadData(undefined, 1, pagination.pageSize)
  }

  const columns: DataTableColumn<FinanceiroResponse>[] = [
    {
      key: 'cliente',
      header: 'Cliente',
      cellClassName: 'font-medium text-[var(--text)] dark:text-slate-900',
      renderCell: (item) => item.clienteNome || '-',
    },
    {
      key: 'equipamento',
      header: 'Equipamento',
      renderCell: (item) => item.equipamentoNome || '-',
    },
    {
      key: 'retirada',
      header: 'Retirada',
      renderCell: (item) => formatDateTime(item.dataRetirada),
    },
    {
      key: 'devolucao',
      header: 'Devolução',
      renderCell: (item) => formatDateTime(item.dataDevolucao),
    },
    {
      key: 'dias',
      header: 'Dias',
      renderCell: (item) => item.diasLocados,
    },
    {
      key: 'diaria',
      header: 'Diária',
      renderCell: (item) => formatCurrency(item.valorDiaria),
    },
    {
      key: 'total',
      header: 'Total',
      renderCell: (item) => formatCurrency(item.valorTotal),
    },
    {
      key: 'lancamento',
      header: 'Lançamento',
      renderCell: (item) => formatDateTime(item.dataLancamento),
    },
  ]

  return (
    <AdminPage title="Financeiro">
      <AdminPanel>
        <FilterBar
          className="grid gap-4 md:grid-cols-[minmax(0,240px)_minmax(0,240px)_auto_auto] md:items-end"
          actions={(
            <>
              <Button onClick={() => void handleSearch()} type="button">Buscar</Button>
              <Button onClick={() => void handleClear()} type="button" variant="secondary">Limpar</Button>
            </>
          )}
        >
          <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
            <span>Data inicial</span>
            <input className="w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900 outline-none transition-colors focus:border-slate-900 focus:ring-2 focus:ring-slate-200 dark:border-slate-300 dark:bg-white dark:text-slate-900 dark:focus:border-slate-900 dark:focus:ring-slate-200" onChange={(event) => setDataInicio(event.target.value)} type="datetime-local" value={dataInicio} />
          </label>
          <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
            <span>Data final</span>
            <input className="w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900 outline-none transition-colors focus:border-slate-900 focus:ring-2 focus:ring-slate-200 dark:border-slate-300 dark:bg-white dark:text-slate-900 dark:focus:border-slate-900 dark:focus:ring-slate-200" onChange={(event) => setDataFim(event.target.value)} type="datetime-local" value={dataFim} />
          </label>
        </FilterBar>

        <DataTable
          columns={columns}
          emptyMessage={tableMessage ?? 'Nenhum lançamento encontrado'}
          getRowKey={(item) => item.id}
          items={items}
          loading={isLoading}
          loadingMessage="Carregando financeiro..."
          pagination={{
            ...pagination,
            onPageChange: (page) =>
              void loadData(
                dataInicio || dataFim
                  ? {
                      dataInicio: toIsoDateTime(dataInicio),
                      dataFim: toIsoDateTime(dataFim),
                    }
                  : undefined,
                page,
                pagination.pageSize,
              ),
            onPageSizeChange: (pageSize) =>
              void loadData(
                dataInicio || dataFim
                  ? {
                      dataInicio: toIsoDateTime(dataInicio),
                      dataFim: toIsoDateTime(dataFim),
                    }
                  : undefined,
                1,
                pageSize,
              ),
          }}
        />
      </AdminPanel>
    </AdminPage>
  )
}