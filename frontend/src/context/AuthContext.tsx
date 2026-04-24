import { createContext, useContext, useEffect, useState, type PropsWithChildren } from 'react'
import { Navigate, useLocation } from 'react-router-dom'
import { getAccessTokenFromResponse, getExpiresInMinutesFromResponse, getRefreshTokenFromResponse } from '../lib/auth'
import { clearSession, getSession, saveSession, subscribeSessionChange } from '../lib/storage'
import { getThemeColorsFromAccessToken, tryGetIsMasterFromAccessToken } from '../lib/jwt'
import { authApi } from '../services/sso'
import type { LoginRequest, LoginResponse, SessionData } from '../types/api'

interface AuthContextValue {
  session: SessionData | null
  isAuthenticated: boolean
  isMaster: boolean
  login: (payload: LoginRequest) => Promise<void>
  refreshSession: (refreshToken?: string) => Promise<void>
  clearAuth: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

function mapLoginResponse(response: LoginResponse, login?: string): SessionData {
  const accessToken = getAccessTokenFromResponse(response)
  const resolvedIsMaster = tryGetIsMasterFromAccessToken(accessToken)
  const themeColors = getThemeColorsFromAccessToken(accessToken)

  return {
    accessToken,
    refreshToken: getRefreshTokenFromResponse(response),
    expiresInMinutes: getExpiresInMinutesFromResponse(response),
    login,
    isMaster: resolvedIsMaster ?? false,
    corPrimaria: themeColors.corPrimaria,
    corSecundaria: themeColors.corSecundaria,
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
          current?.login === nextSession?.login &&
          current?.isMaster === nextSession?.isMaster &&
          current?.corPrimaria === nextSession?.corPrimaria &&
          current?.corSecundaria === nextSession?.corSecundaria
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

    if (!token) {
      throw new Error('Refresh token nao encontrado.')
    }

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
        isMaster: Boolean(session?.isMaster),
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

export function ProtectedRoute({ children, requireMaster = false }: PropsWithChildren<{ requireMaster?: boolean }>) {
  const { isAuthenticated, isMaster } = useAuth()
  const location = useLocation()

  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location }} />
  }

  if (requireMaster && !isMaster) {
    return <Navigate to="/app" replace />
  }

  return children
}