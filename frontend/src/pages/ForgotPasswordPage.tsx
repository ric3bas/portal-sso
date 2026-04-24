import { useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { AuthCard, AuthHeader, AuthLayout } from '../components/auth'
import { Button, Feedback } from '../components/ui'
import { getErrorMessage } from '../lib/errors'
import { authApi } from '../services/sso'

function fieldClass(error: string) {
  return [
    'w-full rounded-md border bg-white px-3 py-2 text-sm text-slate-900 outline-none transition-colors placeholder:text-slate-400 dark:bg-white dark:text-slate-900 dark:placeholder:text-slate-500',
    error
      ? 'border-red-500 focus:border-red-600 focus:ring-2 focus:ring-red-100 dark:border-red-400 dark:focus:border-red-300 dark:focus:ring-red-950'
      : 'border-slate-300 focus:border-slate-900 focus:ring-2 focus:ring-slate-200 dark:border-slate-300 dark:focus:border-slate-900 dark:focus:ring-slate-200',
  ].join(' ')
}

export function ForgotPasswordPage() {
  const [login, setLogin] = useState('')
  const [loginError, setLoginError] = useState('')
  const [message, setMessage] = useState('')
  const [errorMessage, setErrorMessage] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setErrorMessage('')
    setMessage('')

    if (!login.trim()) {
      setLoginError('O campo login ou e-mail e obrigatorio.')
      return
    }

    setIsSubmitting(true)

    try {
      const response = await authApi.forgotPassword({ login })
      setMessage(response)
    } catch (error) {
      setErrorMessage(getErrorMessage(error, 'Falha ao solicitar recuperação de senha.'))
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <AuthLayout>
      <AuthCard className="max-w-xl">
        <form className="space-y-5" onSubmit={handleSubmit}>
          <AuthHeader title="Esqueci minha senha" />

          <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
            <span>
              Login ou e-mail <span className="text-red-600 dark:text-red-400">*</span>
            </span>
            <input
              className={fieldClass(loginError)}
              onChange={(event) => setLogin(event.target.value)}
              onFocus={() => setLoginError('')}
              placeholder="usuario@empresa.com"
              value={login}
            />
            {loginError ? <span className="text-xs text-red-600 dark:text-red-400">{loginError}</span> : null}
          </label>

          {message ? <Feedback tone="success">{message}</Feedback> : null}
          {errorMessage ? <Feedback tone="danger">{errorMessage}</Feedback> : null}

          <div className="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
            <Button disabled={isSubmitting} type="submit">
              {isSubmitting ? 'Enviando...' : 'Solicitar recuperação de senha'}
            </Button>
            <Link className="text-sm font-semibold text-[var(--brand)]" to="/login">
              Voltar ao login
            </Link>
          </div>
        </form>
      </AuthCard>
    </AuthLayout>
  )
}