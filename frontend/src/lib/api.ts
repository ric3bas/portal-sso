import axios, { type InternalAxiosRequestConfig } from 'axios'
import type { LoginResponse, SessionData } from '../types/api'
import { clearSession, getSession, saveSession } from './storage'

const baseURL = import.meta.env.VITE_API_URL ?? 'https://localhost:44349'

interface RetryableRequestConfig extends InternalAxiosRequestConfig {
  _retry?: boolean
}

function mapRefreshResponse(response: LoginResponse, currentSession: SessionData): SessionData {
  return {
    accessToken: response.access_token ?? '',
    refreshToken: response.refresh_token ?? currentSession.refreshToken,
    expiresInMinutes: response.expire_in_minutes ?? currentSession.expiresInMinutes ?? null,
    login: currentSession.login,
  }
}

export const http = axios.create({
  baseURL,
  headers: {
    'Content-Type': 'application/json',
  },
})

const refreshHttp = axios.create({
  baseURL,
  headers: {
    'Content-Type': 'application/json',
  },
})

let refreshRequest: Promise<SessionData> | null = null

async function refreshAccessToken() {
  const currentSession = getSession()

  if (!currentSession?.refreshToken) {
    clearSession()
    throw new Error('Refresh token nao encontrado.')
  }

  if (!refreshRequest) {
    refreshRequest = refreshHttp
      .post<LoginResponse>('/api/v1/auth/refresh-token', { refreshToken: currentSession.refreshToken })
      .then((response) => {
        const nextSession = mapRefreshResponse(response.data, currentSession)

        if (!nextSession.accessToken) {
          throw new Error('Falha ao renovar access token.')
        }

        saveSession(nextSession)
        return nextSession
      })
      .catch((error) => {
        clearSession()
        throw error
      })
      .finally(() => {
        refreshRequest = null
      })
  }

  return refreshRequest
}

http.interceptors.request.use((config) => {
  const session = getSession()

  if (session?.accessToken) {
    config.headers.Authorization = `Bearer ${session.accessToken}`
  }

  return config
})

http.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config as RetryableRequestConfig | undefined
    const status = error.response?.status
    const requestUrl = originalRequest?.url ?? ''

    if (!originalRequest || status !== 401) {
      return Promise.reject(error)
    }

    if (
      originalRequest._retry ||
      requestUrl.includes('/api/v1/auth/login') ||
      requestUrl.includes('/api/v1/auth/refresh-token')
    ) {
      if (requestUrl.includes('/api/v1/auth/refresh-token')) {
        clearSession()
      }

      return Promise.reject(error)
    }

    try {
      originalRequest._retry = true

      const session = await refreshAccessToken()
      originalRequest.headers.Authorization = `Bearer ${session.accessToken}`

      return http(originalRequest)
    } catch (refreshError) {
      return Promise.reject(refreshError)
    }
  },
)