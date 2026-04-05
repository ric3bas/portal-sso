import { Eye, EyeOff } from 'lucide-react'
import { useEffect, useRef, useState, type FormEvent } from 'react'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { Button, Feedback, Panel } from '../components/ui'
import { useAuth } from '../context/AuthContext'
import { getErrorMessage } from '../lib/errors'
import { authApi } from '../services/sso'

interface ResetPasswordErrors {
  novaSenha: string
  confirmarSenha: string
}

const initialErrors: ResetPasswordErrors = {
  novaSenha: '',
  confirmarSenha: '',
}

function validatePassword(password: string) {
  if (!password.trim()) {
    return 'O campo senha é obrigatório.'
  }

  if (password.length < 6) {
    return 'Campo Senha deve conter no minimo 6 caracteres'
  }

  if (password.length > 100) {
    return 'Campo Senha deve conter no maximo 100 caracteres'
  }

  if (!/[A-Z]/.test(password)) {
    return 'Campo Senha deve conter ao menos uma letra maiuscula'
  }

  if (!/[0-9]/.test(password)) {
    return 'Campo Senha deve conter ao menos um numero'
  }

  return ''
}

function validateForm(novaSenha: string, confirmarSenha: string): ResetPasswordErrors {
  const errors: ResetPasswordErrors = {
    novaSenha: validatePassword(novaSenha),
    confirmarSenha: '',
  }

  if (!confirmarSenha.trim()) {
    errors.confirmarSenha = 'O campo confirmar senha é obrigatório.'
  } else if (novaSenha !== confirmarSenha) {
    errors.confirmarSenha = 'As senhas devem ser iguais.'
  }

  return errors
}

function hasErrors(errors: ResetPasswordErrors) {
  return Boolean(errors.novaSenha || errors.confirmarSenha)
}

function fieldClass(error: string) {
  return [
    error
      ? 'border-red-500 focus:border-red-600 focus:ring-red-100 dark:border-red-400 dark:focus:border-red-300 dark:focus:ring-red-950'
      : '',
  ].join(' ')
}

export function ResetPasswordPage() {
  const navigate = useNavigate()
  const { session, clearAuth } = useAuth()
  const [searchParams] = useSearchParams()
  const token = searchParams.get('token') ?? ''
  const redirectTimeoutRef = useRef<number | null>(null)
  const [form, setForm] = useState({
    token,
    novaSenha: '',
    confirmarSenha: '',
  })
  const [responseMessage, setResponseMessage] = useState('')
  const [errorMessage, setErrorMessage] = useState('')
  const [isTokenValid, setIsTokenValid] = useState(false)
  const [validationMessage, setValidationMessage] = useState('')
  const [isValidating, setIsValidating] = useState(true)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [formErrors, setFormErrors] = useState<ResetPasswordErrors>(initialErrors)
  const [showNewPassword, setShowNewPassword] = useState(false)
  const [showConfirmPassword, setShowConfirmPassword] = useState(false)

  useEffect(() => {
    return () => {
      if (redirectTimeoutRef.current) {
        window.clearTimeout(redirectTimeoutRef.current)
      }
    }
  }, [])

  useEffect(() => {
    setForm((current) => ({ ...current, token }))
    setResponseMessage('')
    setValidationMessage('')
    setFormErrors(initialErrors)

    if (!token) {
      setIsTokenValid(false)
      setErrorMessage('Link de recuperacao invalido ou incompleto.')
      setIsValidating(false)
      return
    }

    let isMounted = true

    async function validateToken() {
      setErrorMessage('')
      setIsValidating(true)

      try {
        const response = await authApi.validateResetToken(token)

        if (!isMounted) {
          return
        }

        setIsTokenValid(true)
        setValidationMessage(response)
      } catch (error) {
        if (!isMounted) {
          return
        }

        setIsTokenValid(false)
        setErrorMessage(getErrorMessage(error, 'Link de recuperacao invalido ou expirado.'))
      } finally {
        if (isMounted) {
          setIsValidating(false)
        }
      }
    }

    void validateToken()

    return () => {
      isMounted = false
    }
  }, [token])

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setErrorMessage('')
    setResponseMessage('')

    if (!form.token) {
      setErrorMessage('Link de recuperacao invalido ou incompleto.')
      return
    }

    const validationErrors = validateForm(form.novaSenha, form.confirmarSenha)
    setFormErrors(validationErrors)

    if (hasErrors(validationErrors)) {
      return
    }

    setIsSubmitting(true)

    try {
      const response = await authApi.resetPassword(form)

      if (session?.refreshToken) {
        try {
          await authApi.logout({ refreshToken: session.refreshToken })
        } catch {
          // A senha ja foi alterada; ainda assim a sessao local deve ser encerrada.
        }

        clearAuth()
      }

      setResponseMessage(response.mensagem ?? 'Senha alterada com sucesso.')
      redirectTimeoutRef.current = window.setTimeout(() => {
        navigate('/login', { replace: true })
      }, 1500)
    } catch (error) {
      setErrorMessage(getErrorMessage(error, 'Falha ao trocar senha.'))
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div className="flex min-h-screen items-center justify-center px-4 py-8">
      <Panel className="w-full max-w-2xl p-6 md:p-8">
        <form className="space-y-5" noValidate onSubmit={handleSubmit}>
          <div>
            <h1 className="mt-3 text-3xl font-semibold tracking-[-0.05em] text-[var(--text)]">Definir nova senha</h1>
          </div>

          {isValidating ? <Feedback tone="neutral">Validando link de recuperacao...</Feedback> : null}
          {validationMessage ? <Feedback tone="success">{validationMessage}</Feedback> : null}
          {responseMessage ? <Feedback tone="success">{responseMessage}</Feedback> : null}
          {errorMessage ? <Feedback tone="danger">{errorMessage}</Feedback> : null}

          {isTokenValid ? (
            <div className="grid gap-4 md:grid-cols-2">
              <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
                <span>
                  Nova senha <span className="text-red-600 dark:text-red-400">*</span>
                </span>
                <div className="relative">
                  <input
                    autoComplete="new-password"
                    className={[
                      'w-full rounded-md border bg-white px-3 py-2 pr-11 text-sm text-slate-900 outline-none transition-colors placeholder:text-slate-400',
                      fieldClass(formErrors.novaSenha) || 'border-slate-300 focus:border-slate-900 focus:ring-2 focus:ring-slate-200 dark:border-slate-300 dark:focus:border-slate-900 dark:focus:ring-slate-200',
                    ].join(' ')}
                    onChange={(event) => {
                      setForm((current) => ({ ...current, novaSenha: event.target.value }))
                      setFormErrors((current) => ({ ...current, novaSenha: '' }))
                    }}
                    placeholder="Digite a nova senha"
                    required
                    type={showNewPassword ? 'text' : 'password'}
                    value={form.novaSenha}
                  />
                  <button
                    aria-label={showNewPassword ? 'Ocultar senha' : 'Mostrar senha'}
                    className="absolute inset-y-0 right-0 flex w-11 items-center justify-center text-slate-500 transition-colors hover:text-slate-700 dark:text-slate-400 dark:hover:text-slate-200"
                    onClick={() => setShowNewPassword((current) => !current)}
                    type="button"
                  >
                    {showNewPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                  </button>
                </div>
                {formErrors.novaSenha ? <span className="text-xs text-red-600 dark:text-red-400">{formErrors.novaSenha}</span> : null}
              </label>

              <label className="flex flex-col gap-2 text-sm font-medium text-slate-700 dark:text-slate-200">
                <span>
                  Confirmar senha <span className="text-red-600 dark:text-red-400">*</span>
                </span>
                <div className="relative">
                  <input
                    autoComplete="new-password"
                    className={[
                      'w-full rounded-md border bg-white px-3 py-2 pr-11 text-sm text-slate-900 outline-none transition-colors placeholder:text-slate-400',
                      fieldClass(formErrors.confirmarSenha) || 'border-slate-300 focus:border-slate-900 focus:ring-2 focus:ring-slate-200 dark:border-slate-300 dark:focus:border-slate-900 dark:focus:ring-slate-200',
                    ].join(' ')}
                    onChange={(event) => {
                      setForm((current) => ({ ...current, confirmarSenha: event.target.value }))
                      setFormErrors((current) => ({ ...current, confirmarSenha: '' }))
                    }}
                    placeholder="Repita a nova senha"
                    required
                    type={showConfirmPassword ? 'text' : 'password'}
                    value={form.confirmarSenha}
                  />
                  <button
                    aria-label={showConfirmPassword ? 'Ocultar senha' : 'Mostrar senha'}
                    className="absolute inset-y-0 right-0 flex w-11 items-center justify-center text-slate-500 transition-colors hover:text-slate-700 dark:text-slate-400 dark:hover:text-slate-200"
                    onClick={() => setShowConfirmPassword((current) => !current)}
                    type="button"
                  >
                    {showConfirmPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                  </button>
                </div>
                {formErrors.confirmarSenha ? <span className="text-xs text-red-600 dark:text-red-400">{formErrors.confirmarSenha}</span> : null}
              </label>

              <div className="text-xs text-slate-500 dark:text-slate-400 md:col-span-2">
                Use ao menos uma letra maiuscula, um numero, minimo de 6 e maximo de 100 caracteres.
              </div>
            </div>
          ) : null}

          <div className="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
            {isTokenValid ? (
              <Button disabled={isSubmitting || isValidating} type="submit">
                {isSubmitting ? 'Enviando...' : 'Trocar senha'}
              </Button>
            ) : (
              <span className="text-sm text-[var(--text-soft)]">Solicite um novo link caso este token nao seja valido.</span>
            )}
            <Link className="text-sm font-semibold text-[var(--brand)]" to="/login">
              Voltar ao login
            </Link>
          </div>
        </form>
      </Panel>
    </div>
  )
}