import type {
  ButtonHTMLAttributes,
  InputHTMLAttributes,
  PropsWithChildren,
  ReactNode,
  SelectHTMLAttributes,
  TextareaHTMLAttributes,
} from 'react'
import { useEffect, useMemo, useRef, useState } from 'react'
import { X } from 'lucide-react'
import type { SelectOption } from '../types/api'

let nextToastId = 1
let activeToastIds: number[] = []
const toastListeners = new Set<(ids: number[]) => void>()

function notifyToastListeners() {
  const snapshot = [...activeToastIds]

  toastListeners.forEach((listener) => {
    listener(snapshot)
  })
}

function subscribeToastStack(listener: (ids: number[]) => void) {
  toastListeners.add(listener)
  listener([...activeToastIds])

  return () => {
    toastListeners.delete(listener)
  }
}

function addToast(id: number) {
  if (activeToastIds.includes(id)) {
    return
  }

  activeToastIds = [...activeToastIds, id]
  notifyToastListeners()
}

function removeToast(id: number) {
  if (!activeToastIds.includes(id)) {
    return
  }

  activeToastIds = activeToastIds.filter((toastId) => toastId !== id)
  notifyToastListeners()
}

function cn(...values: Array<string | false | null | undefined>) {
  return values.filter(Boolean).join(' ')
}

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'secondary' | 'ghost' | 'danger'
}

export function Button({ className, variant = 'primary', ...props }: ButtonProps) {
  const styles = {
    primary: 'border border-[var(--brand)] bg-[var(--brand)] text-[var(--brand-contrast)] hover:border-[var(--brand-dark)] hover:bg-[var(--brand-dark)]',
    secondary: 'border border-[var(--line)] bg-[var(--surface)] text-[var(--text)] hover:bg-[var(--brand-soft)]',
    ghost: 'border border-[var(--line)] bg-[var(--surface)] text-[var(--text-soft)] hover:bg-[var(--brand-surface)] hover:text-[var(--text)]',
    danger: 'border border-red-600 bg-red-600 text-white hover:bg-red-500 dark:border-red-500 dark:bg-red-500 dark:hover:bg-red-400',
  }

  return (
    <button
      className={cn(
        'inline-flex cursor-pointer items-center justify-center rounded-md px-3 py-1.5 text-sm font-medium transition-colors disabled:cursor-not-allowed disabled:opacity-60',
        styles[variant],
        className,
      )}
      {...props}
    />
  )
}

export function Panel({ className, children }: PropsWithChildren<{ className?: string }>) {
  return <section className={cn('rounded-lg border border-slate-200 bg-white shadow-sm dark:border-slate-800 dark:bg-slate-950', className)}>{children}</section>
}

interface FieldProps {
  label: string
  hint?: string
}

interface ColorFieldProps extends FieldProps {
  value: string
  onChange: (value: string) => void
}

export function TextField({ label, hint, className, ...props }: FieldProps & InputHTMLAttributes<HTMLInputElement>) {
  return (
    <label className="flex flex-col gap-1.5 text-sm font-medium text-slate-700 dark:text-slate-200">
      <span>{label}</span>
      <input
        className={cn(
          'w-full rounded-md border border-[var(--line)] bg-[var(--surface)] px-2.5 py-1.5 text-sm text-[var(--text)] outline-none transition-colors placeholder:text-slate-400',
          className,
        )}
        {...props}
      />
      {hint ? <span className="text-xs text-slate-500 dark:text-slate-400">{hint}</span> : null}
    </label>
  )
}

export function TextareaField({ label, hint, className, ...props }: FieldProps & TextareaHTMLAttributes<HTMLTextAreaElement>) {
  return (
    <label className="flex flex-col gap-1.5 text-sm font-medium text-slate-700 dark:text-slate-200">
      <span>{label}</span>
      <textarea
        className={cn(
          'min-h-24 w-full rounded-md border border-[var(--line)] bg-[var(--surface)] px-2.5 py-1.5 text-sm text-[var(--text)] outline-none transition-colors placeholder:text-slate-400',
          className,
        )}
        {...props}
      />
      {hint ? <span className="text-xs text-slate-500 dark:text-slate-400">{hint}</span> : null}
    </label>
  )
}

export function SelectField({
  label,
  options,
  className,
  ...props
}: FieldProps & SelectHTMLAttributes<HTMLSelectElement> & { options: SelectOption[] }) {
  return (
    <label className="flex flex-col gap-1.5 text-sm font-medium text-slate-700 dark:text-slate-200">
      <span>{label}</span>
      <select
        className={cn(
          'w-full rounded-md border border-[var(--line)] bg-[var(--surface)] px-2.5 py-1.5 text-sm text-[var(--text)] outline-none transition-colors',
          className,
        )}
        {...props}
      >
        {options.map((option) => (
          <option key={option.value} value={option.value}>
            {option.label}
          </option>
        ))}
      </select>
    </label>
  )
}

export function ColorField({ label, hint, value, onChange }: ColorFieldProps) {
  return (
    <label className="flex flex-col gap-1.5 text-sm font-medium text-slate-700 dark:text-slate-200">
      <span>{label}</span>
      <div className="flex items-center gap-2 rounded-md border border-[var(--line)] bg-[var(--surface)] px-2.5 py-1.5">
        <input
          className="h-9 w-10 cursor-pointer appearance-none overflow-hidden rounded border-0 bg-transparent p-0 [&::-moz-color-swatch]:border-0 [&::-moz-color-swatch]:rounded-md [&::-webkit-color-swatch-wrapper]:p-0 [&::-webkit-color-swatch]:border-0 [&::-webkit-color-swatch]:rounded-md"
          onChange={(event) => onChange(event.target.value.toUpperCase())}
          type="color"
          value={value}
        />
        <input
          className="w-full rounded-md border border-[var(--line)] bg-[var(--surface)] px-2.5 py-1.5 text-sm text-[var(--text)] outline-none transition-colors placeholder:text-slate-400"
          maxLength={7}
          onChange={(event) => onChange(event.target.value.toUpperCase())}
          placeholder="#000000"
          value={value}
        />
      </div>
      {hint ? <span className="text-xs text-slate-500 dark:text-slate-400">{hint}</span> : null}
    </label>
  )
}

export function CheckboxField({
  label,
  hint,
  className,
  ...props
}: FieldProps & InputHTMLAttributes<HTMLInputElement>) {
  return (
    <label className={cn('flex items-start gap-2.5 rounded-md border border-[var(--line)] bg-[var(--brand-soft)] px-3 py-2.5', className)}>
      <input className="mt-1 h-4 w-4 accent-[var(--brand)]" type="checkbox" {...props} />
      <span className="flex flex-col">
        <span className="text-sm font-semibold text-[var(--text)]">{label}</span>
        {hint ? <span className="text-xs text-[var(--text-soft)]">{hint}</span> : null}
      </span>
    </label>
  )
}

export function Feedback({
  tone = 'neutral',
  children,
}: PropsWithChildren<{ tone?: 'neutral' | 'success' | 'danger' | 'warning' }>) {
  const [isRendered, setIsRendered] = useState(true)
  const [isActive, setIsActive] = useState(false)
  const [stackIndex, setStackIndex] = useState(0)
  const toastIdRef = useRef(nextToastId++)
  const contentKey = useMemo(() => {
    if (typeof children === 'string' || typeof children === 'number') {
      return `${tone}:${String(children)}`
    }

    return tone
  }, [children, tone])

  useEffect(() => {
    setIsRendered(true)

    if (tone === 'neutral') {
      return
    }

    const enterFrame = window.requestAnimationFrame(() => {
      setIsActive(true)
    })

    const timeoutId = window.setTimeout(() => {
      setIsActive(false)
    }, 5000)

    const removeTimeoutId = window.setTimeout(() => {
      setIsRendered(false)
    }, 5180)

    return () => {
      window.cancelAnimationFrame(enterFrame)
      window.clearTimeout(timeoutId)
      window.clearTimeout(removeTimeoutId)
    }
  }, [contentKey, tone])

  useEffect(() => {
    if (tone !== 'neutral' && !isRendered && isActive) {
      setIsActive(false)
    }
  }, [isActive, isRendered, tone])

  function dismissToast() {
    setIsActive(false)

    window.setTimeout(() => {
      setIsRendered(false)
    }, 180)
  }

  useEffect(() => {
    if (tone === 'neutral' || !isRendered) {
      return
    }

    const unsubscribe = subscribeToastStack((ids) => {
      const nextIndex = ids.indexOf(toastIdRef.current)
      setStackIndex(nextIndex === -1 ? 0 : nextIndex)
    })

    addToast(toastIdRef.current)

    return () => {
      unsubscribe()
      removeToast(toastIdRef.current)
    }
  }, [contentKey, isRendered, tone])

  const styles = {
    neutral: 'border-slate-200 bg-slate-50 text-slate-700 dark:border-slate-800 dark:bg-slate-900 dark:text-slate-200',
    success: 'border-green-200 bg-green-50 text-green-700 dark:border-green-900 dark:bg-green-950 dark:text-green-300',
    danger: 'border-red-200 bg-red-50 text-red-700 dark:border-red-900 dark:bg-red-950 dark:text-red-300',
    warning: 'border-amber-200 bg-amber-50 text-amber-700 dark:border-amber-900 dark:bg-amber-950 dark:text-amber-300',
  }

  if (!isRendered) {
    return null
  }

  if (tone === 'neutral') {
    return <div className={cn('rounded-md border px-4 py-3 text-sm', styles[tone])}>{children}</div>
  }

  return (
    <div
      className={cn(
        'fixed right-4 z-[70] w-[min(360px,calc(100vw-2rem))] rounded-md border px-4 py-3 text-sm shadow-lg transition-all duration-200 sm:right-6',
        isActive ? 'translate-y-0 opacity-100' : '-translate-y-2 opacity-0',
        styles[tone],
      )}
      style={{ top: `${16 + stackIndex * 88}px` }}
    >
      <div className="flex items-start gap-3">
        <div className="min-w-0 flex-1">{children}</div>
        <button
          aria-label="Fechar mensagem"
          className="mt-0.5 inline-flex h-6 w-6 items-center justify-center rounded text-current/70 transition-colors hover:bg-black/5 hover:text-current dark:hover:bg-white/10"
          onClick={dismissToast}
          type="button"
        >
          <X className="h-4 w-4" />
        </button>
      </div>
    </div>
  )
}

export function Badge({ children, tone = 'default' }: PropsWithChildren<{ tone?: 'default' | 'success' | 'danger' }>) {
  const styles = {
    default: 'bg-slate-100 text-slate-600 dark:bg-slate-800 dark:text-slate-300',
    success: 'bg-green-100 text-green-700 dark:bg-green-950 dark:text-green-300',
    danger: 'bg-red-100 text-red-700 dark:bg-red-950 dark:text-red-300',
  }

  return <span className={cn('inline-flex rounded-full px-2 py-0.5 text-[11px] font-medium leading-4', styles[tone])}>{children}</span>
}

export function PageIntro({
  eyebrow,
  title,
  description,
  action,
}: {
  eyebrow: string
  title: string
  description: string
  action?: ReactNode
}) {
  return (
    <div className="flex flex-col gap-4 md:flex-row md:items-end md:justify-between">
      <div className="space-y-2">
        <p className="font-mono-ui text-xs uppercase tracking-[0.2em] text-slate-500 dark:text-slate-400">{eyebrow}</p>
        <div className="space-y-1">
          <h1 className="text-3xl font-semibold text-slate-900 md:text-4xl dark:text-slate-100">{title}</h1>
          <p className="max-w-3xl text-sm text-slate-600 md:text-base dark:text-slate-300">{description}</p>
        </div>
      </div>
      {action}
    </div>
  )
}

export function EmptyState({ title, description }: { title: string; description: string }) {
  return (
    <div className="rounded-lg border border-dashed border-slate-300 bg-slate-50 px-6 py-10 text-center dark:border-slate-700 dark:bg-slate-900">
      <p className="text-lg font-semibold text-slate-900 dark:text-slate-100">{title}</p>
      <p className="mt-2 text-sm text-slate-500 dark:text-slate-400">{description}</p>
    </div>
  )
}

export function Modal({
  open,
  title,
  description,
  children,
  onClose,
}: PropsWithChildren<{
  open: boolean
  title: string
  description?: string
  onClose: () => void
}>) {
  if (!open) {
    return null
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-950/50 p-4 backdrop-blur-sm">
      <div className="flex max-h-[calc(100vh-2rem)] w-full max-w-2xl flex-col overflow-hidden rounded-lg border border-slate-200 bg-white shadow-xl dark:border-slate-800 dark:bg-slate-950">
        <div className="flex items-start justify-between gap-6 border-b border-slate-200 px-6 py-5 dark:border-slate-800 md:px-8">
          <div className="min-w-0">
            <h2 className="text-2xl font-semibold text-slate-900 dark:text-slate-100">{title}</h2>
            {description ? <p className="mt-2 text-sm text-slate-500 dark:text-slate-400">{description}</p> : null}
          </div>
          <Button aria-label="Fechar modal" className="h-11 w-11 px-0" variant="ghost" onClick={onClose} type="button">
            <X className="h-5 w-5" />
          </Button>
        </div>
        <div className="overflow-y-auto px-6 py-6 md:px-8 md:py-8">
          {children}
        </div>
      </div>
    </div>
  )
}