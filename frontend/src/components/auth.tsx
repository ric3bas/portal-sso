import { Eye, EyeOff } from 'lucide-react'
import { useState, type InputHTMLAttributes, type PropsWithChildren, type ReactNode } from 'react'
import { Panel } from './ui'

function cn(...values: Array<string | false | null | undefined>) {
  return values.filter(Boolean).join(' ')
}

export function AuthLayout({ children, className }: PropsWithChildren<{ className?: string }>) {
  return <div className={cn('flex min-h-screen items-center justify-center px-4 py-8', className)}>{children}</div>
}

export function AuthCard({ children, className }: PropsWithChildren<{ className?: string }>) {
  return <Panel className={cn('w-full p-6 md:p-8', className)}>{children}</Panel>
}

export function AuthHeader({ title, description }: { title: string; description?: ReactNode }) {
  return (
    <div>
      <h1 className="mt-3 text-3xl font-semibold tracking-[-0.05em] text-[var(--text)]">{title}</h1>
      {description ? <div className="mt-2 text-sm text-[var(--text-soft)]">{description}</div> : null}
    </div>
  )
}

export function PasswordField({
  label,
  error,
  className,
  required,
  ...props
}: InputHTMLAttributes<HTMLInputElement> & {
  label: string
  error?: string
}) {
  const [isVisible, setIsVisible] = useState(false)

  return (
    <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
      <span>
        {label}
        {required ? <span className="text-red-600 dark:text-red-400"> *</span> : null}
      </span>
      <div className="relative">
        <input
          className={cn(
            'w-full rounded-md border bg-white px-3 py-2 pr-11 text-sm text-slate-900 outline-none transition-colors placeholder:text-slate-400 dark:bg-white dark:text-slate-900 dark:placeholder:text-slate-500',
            error
              ? 'border-red-500 focus:border-red-600 focus:ring-2 focus:ring-red-100 dark:border-red-400 dark:focus:border-red-300 dark:focus:ring-red-950'
              : 'border-slate-300 focus:border-slate-900 focus:ring-2 focus:ring-slate-200 dark:border-slate-300 dark:focus:border-slate-900 dark:focus:ring-slate-200',
            className,
          )}
          {...props}
          type={isVisible ? 'text' : 'password'}
        />
        <button
          aria-label={isVisible ? 'Ocultar senha' : 'Mostrar senha'}
          className="absolute inset-y-0 right-0 flex w-11 items-center justify-center text-slate-500 transition-colors hover:text-slate-700 dark:text-slate-400 dark:hover:text-slate-200"
          onClick={() => setIsVisible((current) => !current)}
          type="button"
        >
          {isVisible ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
        </button>
      </div>
      {error ? <span className="text-xs text-red-600 dark:text-red-400">{error}</span> : null}
    </label>
  )
}