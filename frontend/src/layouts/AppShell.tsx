import {
  BadgeCheck,
  Building2,
  ChevronDown,
  KeyRound,
  LayoutDashboard,
  LogOut,
  Menu,
  Moon,
  ShieldCheck,
  Sun,
  UserCircle2,
  Users2,
  X,
} from 'lucide-react'
import { useEffect, useRef, useState } from 'react'
import { NavLink, Outlet, useLocation, useNavigate } from 'react-router-dom'
import { Button, Feedback } from '../components/ui'
import { useAuth } from '../context/AuthContext'
import { useTheme } from '../context/ThemeContext'
import { getErrorMessage } from '../lib/errors'
import { authApi } from '../services/sso'

const navigationItems = [
  { to: '/app', label: 'Visao geral', icon: LayoutDashboard, end: true },
  { to: '/app/parceiros', label: 'Parceiros', icon: Building2, requiresMaster: true },
  { to: '/app/escopos', label: 'Escopos', icon: ShieldCheck, requiresMaster: true },
  { to: '/app/perfis', label: 'Perfis', icon: BadgeCheck, requiresMaster: true },
  { to: '/app/usuarios', label: 'Usuarios', icon: Users2 },
]

function Navigation({ onNavigate }: { onNavigate?: () => void }) {
  const { isMaster } = useAuth()

  return (
    <nav className="space-y-1">
      {navigationItems
        .filter((item) => !item.requiresMaster || isMaster)
        .map((item) => {
        const Icon = item.icon

        return (
          <NavLink
            key={item.to}
            className={({ isActive }) =>
              [
                'flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors',
                isActive
                  ? 'bg-slate-900 text-white dark:bg-slate-100 dark:text-slate-900'
                  : 'text-slate-600 hover:bg-slate-100 hover:text-slate-900 dark:text-slate-300 dark:hover:bg-slate-800 dark:hover:text-slate-100',
              ].join(' ')
            }
            end={item.end}
            onClick={onNavigate}
            to={item.to}
          >
            <Icon className="h-4 w-4" />
            {item.label}
          </NavLink>
        )
        })}
    </nav>
  )
}

export function AppShell() {
  const { session, clearAuth } = useAuth()
  const { theme, toggleTheme } = useTheme()
  const navigate = useNavigate()
  const location = useLocation()
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false)
  const [isLoggingOut, setIsLoggingOut] = useState(false)
  const [isSendingPasswordReset, setIsSendingPasswordReset] = useState(false)
  const [isUserMenuOpen, setIsUserMenuOpen] = useState(false)
  const userMenuRef = useRef<HTMLDivElement>(null)
  const [feedback, setFeedback] = useState<{ tone: 'success' | 'danger'; message: string } | null>(null)

  useEffect(() => {
    setIsMobileMenuOpen(false)
    setIsUserMenuOpen(false)
  }, [location.pathname])

  useEffect(() => {
    function handlePointerDown(event: MouseEvent) {
      if (!userMenuRef.current) {
        return
      }

      if (event.target instanceof Node && !userMenuRef.current.contains(event.target)) {
        setIsUserMenuOpen(false)
      }
    }

    document.addEventListener('mousedown', handlePointerDown)

    return () => {
      document.removeEventListener('mousedown', handlePointerDown)
    }
  }, [])

  async function handleLogout() {
    setIsLoggingOut(true)

    try {
      if (session?.refreshToken) {
        await authApi.logout({ refreshToken: session.refreshToken })
      }
    } catch (error) {
      window.alert(getErrorMessage(error, 'Falha ao revogar refresh token.'))
    } finally {
      clearAuth()
      setIsLoggingOut(false)
      navigate('/login', { replace: true })
    }
  }

  async function handleRequestPasswordChange() {
    if (!session?.login) {
      setFeedback({ tone: 'danger', message: 'Nao foi possivel identificar o usuario autenticado para iniciar a troca de senha.' })
      setIsUserMenuOpen(false)
      return
    }

    setIsSendingPasswordReset(true)

    try {
      const token = await authApi.forgotPasswordLogged({ login: session.login })

      if (!token) {
        throw new Error('Token de troca de senha nao retornado pelo backend.')
      }

      navigate(`/auth/trocar-senha?token=${encodeURIComponent(token)}`)
    } catch (error) {
      setFeedback({ tone: 'danger', message: getErrorMessage(error, 'Falha ao iniciar troca de senha.') })
    } finally {
      setIsSendingPasswordReset(false)
      setIsUserMenuOpen(false)
    }
  }

  return (
    <div className="flex h-screen flex-col overflow-hidden bg-slate-50 text-slate-900 dark:bg-slate-950 dark:text-slate-100">
      {feedback ? <Feedback tone={feedback.tone}>{feedback.message}</Feedback> : null}

      <header className="shrink-0 border-b border-slate-200 bg-white dark:border-slate-800 dark:bg-slate-900">
        <div className="mx-auto flex max-w-7xl items-center justify-between gap-4 px-4 py-4 sm:px-6 lg:px-8">
          <div className="flex items-center gap-3">
            <Button className="lg:hidden" onClick={() => setIsMobileMenuOpen(true)} type="button" variant="secondary">
              <Menu className="h-4 w-4" />
            </Button>

            <div className="flex h-12 min-w-40 items-center justify-center rounded-md border border-dashed border-slate-300 bg-slate-50 px-4 text-sm font-medium text-slate-500 dark:border-slate-700 dark:bg-slate-800 dark:text-slate-300">
              Espaco da logo
            </div>
          </div>

          <div className="flex items-center gap-3">
            <Button onClick={toggleTheme} type="button" variant="secondary">
              {theme === 'dark' ? <Sun className="mr-2 h-4 w-4" /> : <Moon className="mr-2 h-4 w-4" />}
            </Button>

            <div className="relative" ref={userMenuRef}>
              <button
                className="flex items-center gap-3 rounded-md border border-slate-300 bg-white px-3 py-2 text-left text-sm text-slate-700 transition-colors hover:bg-slate-50 dark:border-slate-700 dark:bg-slate-900 dark:text-slate-200 dark:hover:bg-slate-800"
                onClick={() => setIsUserMenuOpen((current) => !current)}
                type="button"
              >
                <UserCircle2 className="h-5 w-5" />
                <span className="hidden sm:block">
                  <span className="block font-medium">{session?.login ?? 'Usuario autenticado'}</span>
                </span>
                <ChevronDown className="h-4 w-4" />
              </button>

              {isUserMenuOpen ? (
                <div className="absolute right-0 z-30 mt-2 w-56 rounded-md border border-slate-200 bg-white py-2 shadow-lg dark:border-slate-800 dark:bg-slate-900">
                  <button
                    className="flex w-full items-center gap-3 px-4 py-2 text-sm text-slate-700 hover:bg-slate-50 dark:text-slate-200 dark:hover:bg-slate-800"
                    disabled={isSendingPasswordReset}
                    onClick={() => void handleRequestPasswordChange()}
                    type="button"
                  >
                    <KeyRound className="h-4 w-4" />
                    {isSendingPasswordReset ? 'Enviando link...' : 'Trocar senha'}
                  </button>
                  <button
                    className="flex w-full items-center gap-3 px-4 py-2 text-sm text-red-600 hover:bg-red-50 dark:text-red-300 dark:hover:bg-red-950"
                    disabled={isLoggingOut}
                    onClick={() => void handleLogout()}
                    type="button"
                  >
                    <LogOut className="h-4 w-4" />
                    {isLoggingOut ? 'Saindo...' : 'Sair'}
                  </button>
                </div>
              ) : null}
            </div>
          </div>
        </div>
      </header>

      <div className="mx-auto grid h-full min-h-0 w-full max-w-7xl flex-1 grid-cols-1 gap-6 overflow-hidden px-4 py-6 sm:px-6 lg:grid-cols-[240px_minmax(0,1fr)] lg:px-8">
        <aside className="hidden min-h-0 overflow-y-auto rounded-lg border border-slate-200 bg-white p-4 shadow-sm dark:border-slate-800 dark:bg-slate-900 lg:block">
          <div className="border-b border-slate-200 pb-4 dark:border-slate-800">
            <p className="text-sm font-semibold text-slate-900 dark:text-slate-100">Portal Administrativo</p>
          </div>

          <div className="mt-4">
            <Navigation />
          </div>
        </aside>

        <main className="min-h-0 min-w-0 overflow-y-auto rounded-lg border border-slate-200 bg-white p-4 shadow-sm dark:border-slate-800 dark:bg-slate-900 sm:p-6">
          <Outlet />
        </main>
      </div>

      {isMobileMenuOpen ? (
        <div className="fixed inset-0 z-40 bg-slate-950/50 lg:hidden">
          <div className="h-full w-[88%] max-w-xs border-r border-slate-200 bg-white p-6 dark:border-slate-800 dark:bg-slate-900">
            <div className="flex items-center justify-between border-b border-slate-200 pb-4 dark:border-slate-800">
              <div>
                <p className="text-base font-semibold text-slate-900 dark:text-slate-100">Portal SSO</p>
                <p className="mt-1 text-sm text-slate-500 dark:text-slate-400">Navegacao</p>
              </div>
              <Button onClick={() => setIsMobileMenuOpen(false)} type="button" variant="ghost">
                <X className="h-4 w-4" />
              </Button>
            </div>

            <div className="mt-4">
              <Navigation onNavigate={() => setIsMobileMenuOpen(false)} />
            </div>
          </div>
        </div>
      ) : null}
    </div>
  )
}