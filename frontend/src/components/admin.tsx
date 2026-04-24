import type { PropsWithChildren, ReactNode } from 'react'
import { Button, Feedback, PageIntro, Panel } from './ui'

function cn(...values: Array<string | false | null | undefined>) {
  return values.filter(Boolean).join(' ')
}

export function AdminPage({
  title,
  description = '',
  eyebrow = '',
  action,
  feedback,
  children,
}: PropsWithChildren<{
  title: string
  description?: string
  eyebrow?: string
  action?: ReactNode
  feedback?: { tone: 'success' | 'danger' | 'warning' | 'neutral'; message: string } | null
}>) {
  return (
    <div className="flex min-h-0 flex-1 flex-col gap-4">
      <PageIntro action={action} description={description} eyebrow={eyebrow} title={title} />
      {feedback ? <Feedback tone={feedback.tone}>{feedback.message}</Feedback> : null}
      {children}
    </div>
  )
}

export function FilterBar({ className, actions, children }: PropsWithChildren<{ className?: string; actions?: ReactNode }>) {
  return (
    <div className={cn('flex flex-col gap-3 md:flex-row md:items-end md:justify-between', className)}>
      <div className="min-w-0 flex-1">{children}</div>
      {actions ? <div className="flex shrink-0 gap-2">{actions}</div> : null}
    </div>
  )
}

export function RowActions({ children }: PropsWithChildren) {
  return <div className="flex justify-end gap-1">{children}</div>
}

export function FormModalFooter({
  onCancel,
  isSaving,
  saveLabel = 'Salvar',
  savingLabel = 'Salvando...',
}: {
  onCancel: () => void
  isSaving?: boolean
  saveLabel?: string
  savingLabel?: string
}) {
  return (
    <div className="flex justify-end gap-2.5 border-t border-slate-200 pt-3 dark:border-slate-800">
      <Button onClick={onCancel} type="button" variant="ghost">
        Cancelar
      </Button>
      <Button disabled={isSaving} type="submit">
        {isSaving ? savingLabel : saveLabel}
      </Button>
    </div>
  )
}

export function AdminPanel({ className, children }: PropsWithChildren<{ className?: string }>) {
  return <Panel className={cn('flex min-h-0 flex-1 flex-col p-4 md:p-5', className)}>{children}</Panel>
}