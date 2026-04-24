import type { Key, ReactNode } from 'react'

function cn(...values: Array<string | false | null | undefined>) {
  return values.filter(Boolean).join(' ')
}

export interface DataTableColumn<T> {
  key: string
  header: ReactNode
  renderCell: (item: T) => ReactNode
  headerClassName?: string
  cellClassName?: string
}

export interface DataTablePagination {
  page: number
  pageSize: number
  totalRecords: number
  totalPages: number
  pageSizeOptions?: number[]
  onPageChange: (page: number) => void
  onPageSizeChange: (pageSize: number) => void
}

export function DataTable<T>({
  items,
  loading,
  loadingMessage,
  emptyMessage,
  colSpan,
  columns,
  headers,
  getRowKey,
  renderRow,
  rowClassName,
  pagination,
}: {
  items: T[]
  loading: boolean
  loadingMessage: string
  emptyMessage: string
  colSpan?: number
  columns?: DataTableColumn<T>[]
  headers?: ReactNode
  getRowKey: (item: T) => Key
  renderRow?: (item: T) => ReactNode
  rowClassName?: string
  pagination?: DataTablePagination
}) {
  const resolvedColSpan = colSpan ?? columns?.length ?? 1
  const pageSizeOptions = pagination?.pageSizeOptions ?? [20, 50, 100]
  const totalPages = pagination ? Math.max(1, pagination.totalPages) : 1
  const canGoToPreviousPage = Boolean(pagination && pagination.page > 1)
  const canGoToNextPage = Boolean(pagination && pagination.page < totalPages)
  const headerContent = columns ? (
    <tr>
      {columns.map((column) => (
        <th key={column.key} className={cn('px-2.5 py-2 font-semibold', column.headerClassName)}>
          {column.header}
        </th>
      ))}
    </tr>
  ) : headers

  if (loading) {
    return (
      <div className="mt-4 rounded-md border border-[var(--line)] bg-[var(--surface-strong)] px-4 py-6 text-sm text-[var(--text-soft)]">
        {loadingMessage}
      </div>
    )
  }

  return (
    <div className="mt-4 flex min-h-0 flex-1 flex-col overflow-hidden rounded-md border border-[var(--line)] dark:border-slate-300">
      <div className="shrink-0 overflow-x-auto border-b border-[var(--line)] bg-[var(--surface-strong)] dark:border-slate-300 dark:bg-white">
        <table className="min-w-full table-fixed border-collapse text-left text-sm">
          <thead className="text-[var(--text-soft)] dark:text-slate-700">
            {headerContent}
          </thead>
        </table>
      </div>

      <div className="min-h-0 flex-1 overflow-x-auto overflow-y-auto">
        <table className="min-w-full table-fixed border-collapse text-left text-sm">
          <tbody>
            {items.length > 0 ? (
              items.map((item) => {
                if (columns) {
                  return (
                    <tr key={getRowKey(item)} className={cn('border-t border-[var(--line)] bg-white/70 dark:border-slate-300 dark:bg-white', rowClassName)}>
                      {columns.map((column) => (
                        <td key={column.key} className={cn('px-2.5 py-1.5 text-[var(--text-soft)] dark:text-slate-600', column.cellClassName)}>
                          {column.renderCell(item)}
                        </td>
                      ))}
                    </tr>
                  )
                }

                return renderRowWithKey(getRowKey(item), renderRow?.(item))
              })
            ) : (
              <tr className="border-t border-[var(--line)] bg-white/70 dark:border-slate-300 dark:bg-white">
                <td className="px-2.5 py-4 text-sm text-[var(--text-soft)] dark:text-slate-600" colSpan={resolvedColSpan}>
                  {emptyMessage}
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      {pagination ? (
        <div className="shrink-0 flex flex-col gap-2 border-t border-[var(--line)] bg-[var(--table-footer-bg)] px-3 py-3 text-sm text-[var(--text-soft)] dark:border-slate-300 dark:text-slate-700 md:flex-row md:items-center md:justify-between">
          <div className="flex flex-col gap-1 md:flex-row md:items-center md:gap-3">
            <span>Total de registros: <strong className="text-[var(--text)] dark:text-slate-900">{pagination.totalRecords}</strong></span>
            <span>Página <strong className="text-[var(--text)] dark:text-slate-900">{pagination.page}</strong> de <strong className="text-[var(--text)] dark:text-slate-900">{totalPages}</strong></span>
          </div>

          <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between md:justify-end">
            <label className="flex items-center gap-2">
              <span>Registros por página</span>
              <select
                className="rounded-md border border-slate-300 bg-white px-2.5 py-1.5 text-sm text-slate-900 outline-none transition-colors focus:border-slate-900 focus:ring-2 focus:ring-slate-200 dark:border-slate-300 dark:bg-white dark:text-slate-900 dark:focus:border-slate-900 dark:focus:ring-slate-200"
                onChange={(event) => pagination.onPageSizeChange(Number.parseInt(event.target.value, 10))}
                value={pagination.pageSize}
              >
                {pageSizeOptions.map((option) => (
                  <option key={option} value={option}>{option}</option>
                ))}
              </select>
            </label>

            <div className="flex items-center gap-2">
              <button
                className="rounded-md border border-slate-300 bg-white px-2.5 py-1.5 text-sm font-medium text-slate-700 transition-colors disabled:cursor-not-allowed disabled:opacity-60 dark:border-slate-700 dark:bg-slate-900 dark:text-slate-200"
                disabled={!canGoToPreviousPage}
                onClick={() => pagination.onPageChange(pagination.page - 1)}
                type="button"
              >
                Anterior
              </button>
              <button
                className="rounded-md border border-slate-300 bg-white px-2.5 py-1.5 text-sm font-medium text-slate-700 transition-colors disabled:cursor-not-allowed disabled:opacity-60 dark:border-slate-700 dark:bg-slate-900 dark:text-slate-200"
                disabled={!canGoToNextPage}
                onClick={() => pagination.onPageChange(pagination.page + 1)}
                type="button"
              >
                Próxima
              </button>
            </div>
          </div>
        </div>
      ) : null}
    </div>
  )
}

function renderRowWithKey(key: Key, row: ReactNode | undefined) {
  if (!row) {
    return null
  }

  if (row && typeof row === 'object' && 'type' in row && 'props' in row) {
    return row
  }

  return <tr key={key}>{row}</tr>
}