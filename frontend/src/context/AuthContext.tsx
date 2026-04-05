import { createContext, useContext, useEffect, useState, type PropsWithChildren } from 'react'
import { Navigate, useLocation } from 'react-router-dom'
import { clearSession, getSession, saveSession, subscribeSessionChange } from '../lib/storage'
import { authApi } from '../services/sso'
import type { LoginRequest, LoginResponse, SessionData } from '../types/api'

interface AuthContextValue {
  session: SessionData | null
  isAuthenticated: boolean
  login: (payload: LoginRequest) => Promise<void>
  refreshSession: (refreshToken?: string) => Promise<void>
  clearAuth: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

function mapLoginResponse(response: LoginResponse, login?: string): SessionData {
  return {
    accessToken: response.access_token ?? '',
    refreshToken: response.refresh_token ?? '',
    expiresInMinutes: response.expire_in_minutes,
    login,
  }
}

export function AuthProvider({ children }: PropsWithChildren) {
  const [session, setSession] = useState<SessionData | null>(() => getSession())

  useEffect(() => {
    if (session) {
      saveSession(session)
      return
    }

    clearSession()
  }, [session])

  useEffect(() => {
    return subscribeSessionChange((nextSession) => {
      setSession((current) => {
        if (
          current?.accessToken === nextSession?.accessToken &&
          current?.refreshToken === nextSession?.refreshToken &&
          current?.expiresInMinutes === nextSession?.expiresInMinutes &&
          current?.login === nextSession?.login
        ) {
          return current
        }

        return nextSession
      })
    })
  }, [])

  async function login(payload: LoginRequest) {
    const response = await authApi.login(payload)
    setSession(mapLoginResponse(response, payload.login))
  }

  async function refreshSession(refreshToken?: string) {
    const token = refreshToken ?? session?.refreshToken ?? ''

    const response = await authApi.refreshToken({ refreshToken: token })

    setSession((current) => ({
      ...mapLoginResponse(response, current?.login),
      login: current?.login,
    }))
  }

  function clearAuth() {
    setSession(null)
  }

  return (
    <AuthContext.Provider
      value={{
        session,
        isAuthenticated: Boolean(session?.accessToken),
        login,
        refreshSession,
        clearAuth,
      }}
    >
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const context = useContext(AuthContext)

  if (!context) {
    throw new Error('useAuth must be used within AuthProvider')
  }

  return context
}

export function ProtectedRoute({ children }: PropsWithChildren) {
  const { isAuthenticated } = useAuth()
  const location = useLocation()

  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location }} />
  }

  return children
}