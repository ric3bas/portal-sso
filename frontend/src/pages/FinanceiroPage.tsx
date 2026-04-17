import { useEffect, useRef, useState } from 'react'
import { Button, PageIntro, Panel } from '../components/ui'
import { getErrorMessage } from '../lib/errors'
import { financeiroApi } from '../services/sso'
import type { FinanceiroResponse } from '../types/api'

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

  async function loadData(filters?: { dataInicio?: string; dataFim?: string }) {
    setIsLoading(true)

    try {
      const response = filters?.dataInicio || filters?.dataFim
        ? await financeiroApi.listByPeriod(filters)
        : await financeiroApi.list()

      setItems(response)
      setTableMessage(response.length === 0 ? 'Nenhum lançamento encontrado' : null)
    } catch (error) {
      setItems([])
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
    })
  }

  async function handleClear() {
    setDataInicio('')
    setDataFim('')
    await loadData()
  }

  return (
    <div className="space-y-6">
      <PageIntro eyebrow="" title="Financeiro" description="" />

      <Panel className="p-5 md:p-6">
        <div className="grid gap-4 md:grid-cols-[minmax(0,240px)_minmax(0,240px)_auto_auto] md:items-end">
          <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
            <span>Data inicial</span>
            <input className="w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900 outline-none transition-colors focus:border-slate-900 focus:ring-2 focus:ring-slate-200 dark:border-slate-300 dark:bg-white dark:text-slate-900 dark:focus:border-slate-900 dark:focus:ring-slate-200" onChange={(event) => setDataInicio(event.target.value)} type="datetime-local" value={dataInicio} />
          </label>
          <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
            <span>Data final</span>
            <input className="w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900 outline-none transition-colors focus:border-slate-900 focus:ring-2 focus:ring-slate-200 dark:border-slate-300 dark:bg-white dark:text-slate-900 dark:focus:border-slate-900 dark:focus:ring-slate-200" onChange={(event) => setDataFim(event.target.value)} type="datetime-local" value={dataFim} />
          </label>
          <Button onClick={() => void handleSearch()} type="button">Buscar</Button>
          <Button onClick={() => void handleClear()} type="button" variant="secondary">Limpar</Button>
        </div>

        {isLoading ? (
          <div className="mt-6 rounded-md border border-[var(--line)] bg-[var(--surface-strong)] px-5 py-8 text-sm text-[var(--text-soft)]">Carregando financeiro...</div>
        ) : (
          <div className="mt-6 overflow-hidden rounded-md border border-[var(--line)] dark:border-slate-300">
            <div className="overflow-x-auto">
              <table className="min-w-full border-collapse text-left text-sm">
                <thead className="bg-[var(--surface-strong)] text-[var(--text-soft)] dark:bg-white dark:text-slate-700">
                  <tr>
                    <th className="px-4 py-3 font-semibold">Cliente</th>
                    <th className="px-4 py-3 font-semibold">Equipamento</th>
                    <th className="px-4 py-3 font-semibold">Retirada</th>
                    <th className="px-4 py-3 font-semibold">Devolução</th>
                    <th className="px-4 py-3 font-semibold">Dias</th>
                    <th className="px-4 py-3 font-semibold">Diária</th>
                    <th className="px-4 py-3 font-semibold">Total</th>
                    <th className="px-4 py-3 font-semibold">Lançamento</th>
                  </tr>
                </thead>
                <tbody>
                  {items.length > 0 ? (
                    items.map((item) => (
                      <tr key={item.id} className="border-t border-[var(--line)] bg-white/70 dark:border-slate-300 dark:bg-white">
                        <td className="px-4 py-3 font-medium text-[var(--text)] dark:text-slate-900">{item.clienteNome || '-'}</td>
                        <td className="px-4 py-3 text-[var(--text-soft)] dark:text-slate-600">{item.equipamentoNome || '-'}</td>
                        <td className="px-4 py-3 text-[var(--text-soft)] dark:text-slate-600">{formatDateTime(item.dataRetirada)}</td>
                        <td className="px-4 py-3 text-[var(--text-soft)] dark:text-slate-600">{formatDateTime(item.dataDevolucao)}</td>
                        <td className="px-4 py-3 text-[var(--text-soft)] dark:text-slate-600">{item.diasLocados}</td>
                        <td className="px-4 py-3 text-[var(--text-soft)] dark:text-slate-600">{formatCurrency(item.valorDiaria)}</td>
                        <td className="px-4 py-3 text-[var(--text-soft)] dark:text-slate-600">{formatCurrency(item.valorTotal)}</td>
                        <td className="px-4 py-3 text-[var(--text-soft)] dark:text-slate-600">{formatDateTime(item.dataLancamento)}</td>
                      </tr>
                    ))
                  ) : (
                    <tr className="border-t border-[var(--line)] bg-white/70 dark:border-slate-300 dark:bg-white">
                      <td className="px-4 py-6 text-sm text-[var(--text-soft)] dark:text-slate-600" colSpan={8}>
                        {tableMessage ?? 'Nenhum lançamento encontrado'}
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          </div>
        )}
      </Panel>
    </div>
  )
}