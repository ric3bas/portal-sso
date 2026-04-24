import { ShieldCheck } from 'lucide-react'
import { useState, type FormEvent } from 'react'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { getErrorMessage } from '../lib/errors'
import { Button, Feedback, Panel } from '../components/ui'

interface LoginErrors {
  login: string
  senha: string
}

const initialErrors: LoginErrors = {
  login: '',
  senha: '',
}

function fieldClass(error: string) {
  return [
    'w-full rounded-md border bg-[var(--surface)] px-3 py-2 text-sm text-[var(--text)] outline-none transition-colors placeholder:text-slate-400',
    error
      ? 'border-red-500 focus:border-red-600 focus:ring-2 focus:ring-red-100 dark:border-red-400 dark:focus:border-red-300 dark:focus:ring-red-950'
      : 'border-[var(--line)]',
  ].join(' ')
}

export function LoginPage() {
  const navigate = useNavigate()
  const location = useLocation()
  const { login } = useAuth()
  const [form, setForm] = useState({ login: '', senha: '' })
  const [formErrors, setFormErrors] = useState<LoginErrors>(initialErrors)
  const [errorMessage, setErrorMessage] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setErrorMessage('')

    const nextErrors: LoginErrors = {
      login: form.login.trim() ? '' : 'O campo login ou e-mail é obrigatório.',
      senha: form.senha.trim() ? '' : 'O campo senha é obrigatório.',
    }

    setFormErrors(nextErrors)

    if (nextErrors.login || nextErrors.senha) {
      return
    }

    setIsSubmitting(true)

    try {
      await login(form)
      const destination = location.state?.from?.pathname ?? '/app'
      navigate(destination, { replace: true })
    } catch (error) {
      setErrorMessage(getErrorMessage(error, 'Nao foi possivel autenticar o usuario.'))
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div className="min-h-screen bg-[linear-gradient(160deg,rgba(255,255,255,0.7),rgba(255,255,255,0))] px-4 py-6 md:px-8 md:py-10">
      <div className="mx-auto grid min-h-[calc(100vh-3rem)] max-w-7xl gap-6 lg:grid-cols-[1.1fr_0.9fr]">
        <Panel className="relative overflow-hidden p-8 md:p-12">
          <div className="absolute right-0 top-0 h-60 w-60 rounded-full bg-[rgba(200,91,47,0.12)] blur-3xl" />
          <div className="absolute bottom-0 left-0 h-52 w-52 rounded-full bg-[rgba(45,106,79,0.12)] blur-3xl" />
          <div className="relative z-10 flex h-full flex-col justify-between gap-10">
            <div className="space-y-5">
              <div className="inline-flex rounded-full border border-[var(--line)] bg-[rgba(255,255,255,0.7)] px-4 py-2 text-sm font-semibold text-[var(--brand)]">
                <ShieldCheck className="mr-2 h-4 w-4" />
                Portal
              </div>
              <div className="space-y-4">
                <h1 className="max-w-xl text-4xl font-semibold tracking-[-0.05em] text-[var(--text)] md:text-6xl">
                  Bem vindo
                </h1>
                <p className="max-w-2xl text-base text-[var(--text-soft)] md:text-lg">
                  O painel foi estruturado a partir do swagger para oferecer login, recuperacao de senha, CRUD responsivo e operAções administrativas por modulo.
                </p>
              </div>
            </div>

            <div className="grid gap-4 md:grid-cols-3">
              <div className="rounded-[28px] border border-[var(--line)] bg-[rgba(255,255,255,0.66)] p-5">
                <p className="font-mono-ui text-xs uppercase tracking-[0.3em] text-[var(--brand)]">Auth</p>
                <p className="mt-3 text-sm text-[var(--text-soft)]">Login, refresh token, logout e recuperacao de senha no mesmo fluxo.</p>
              </div>
              <div className="rounded-[28px] border border-[var(--line)] bg-[rgba(255,255,255,0.66)] p-5">
                <p className="font-mono-ui text-xs uppercase tracking-[0.3em] text-[var(--brand)]">CRUD</p>
                <p className="mt-3 text-sm text-[var(--text-soft)]">Modelos de tela com filtros, modais e tratamento de checkbox para campos booleanos.</p>
              </div>
              <div className="rounded-[28px] border border-[var(--line)] bg-[rgba(255,255,255,0.66)] p-5">
                <p className="font-mono-ui text-xs uppercase tracking-[0.3em] text-[var(--brand)]">Responsivo</p>
                <p className="mt-3 text-sm text-[var(--text-soft)]">Menu lateral no desktop e navegação móvel adaptada para operação rápida.</p>
              </div>
            </div>
          </div>
        </Panel>

        <Panel className="flex items-center p-6 md:p-10">
          <form className="mx-auto w-full max-w-md space-y-5" onSubmit={handleSubmit}>
            <div>
              <p className="font-mono-ui text-xs uppercase tracking-[0.3em] text-[var(--brand)]">Entrar</p>
              <h2 className="mt-3 text-3xl font-semibold tracking-[-0.05em] text-[var(--text)]">Acesse o painel</h2>
              <p className="mt-2 text-sm text-[var(--text-soft)]">Informe login ou e-mail e a senha configurada na API.</p>
            </div>

            <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
              <span>
                Login ou e-mail <span className="text-red-600 dark:text-red-400">*</span>
              </span>
              <input
                autoComplete="username"
                className={fieldClass(formErrors.login)}
                onChange={(event) => setForm((current) => ({ ...current, login: event.target.value }))}
                onFocus={() => setFormErrors((current) => ({ ...current, login: '' }))}
                placeholder="usuario@empresa.com"
                value={form.login}
              />
              {formErrors.login ? <span className="text-xs text-red-600 dark:text-red-400">{formErrors.login}</span> : null}
            </label>

            <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
              <span>
                Senha <span className="text-red-600 dark:text-red-400">*</span>
              </span>
              <input
                autoComplete="current-password"
                className={fieldClass(formErrors.senha)}
                onChange={(event) => setForm((current) => ({ ...current, senha: event.target.value }))}
                onFocus={() => setFormErrors((current) => ({ ...current, senha: '' }))}
                placeholder="Digite sua senha"
                type="password"
                value={form.senha}
              />
              {formErrors.senha ? <span className="text-xs text-red-600 dark:text-red-400">{formErrors.senha}</span> : null}
            </label>

            {errorMessage ? <Feedback tone="danger">{errorMessage}</Feedback> : null}

            <Button className="w-full" disabled={isSubmitting} type="submit">
              {isSubmitting ? 'Autenticando...' : 'Entrar'}
            </Button>

            <div className="flex flex-col gap-3 border-t border-[var(--line)] pt-5 text-sm text-[var(--text-soft)] md:flex-row md:items-center md:justify-between">
              <Link className="font-semibold text-[var(--brand)]" to="/auth/esqueceu-senha">
                Esqueci minha senha
              </Link>
            </div>
          </form>
        </Panel>
      </div>
    </div>
  )
}