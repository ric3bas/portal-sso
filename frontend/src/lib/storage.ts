import type { SessionData } from '../types/api'
import { getIsMasterFromAccessToken } from './jwt'

const SESSION_KEY = 'portal-sso.session'
const SESSION_CHANGED_EVENT = 'portal-sso.session-changed'

function notifySessionChanged(session: SessionData | null) {
  window.dispatchEvent(new CustomEvent<SessionData | null>(SESSION_CHANGED_EVENT, { detail: session }))
}

export function getSession(): SessionData | null {
  const rawValue = window.localStorage.getItem(SESSION_KEY)

  if (!rawValue) {
    return null
  }

  try {
    const parsedSession = JSON.parse(rawValue) as Partial<SessionData>

    if (!parsedSession.accessToken || !parsedSession.refreshToken) {
      window.localStorage.removeItem(SESSION_KEY)
      return null
    }

    return {
      accessToken: parsedSession.accessToken,
      refreshToken: parsedSession.refreshToken,
      expiresInMinutes: parsedSession.expiresInMinutes ?? null,
      login: parsedSession.login,
      isMaster:
        typeof parsedSession.isMaster === 'boolean'
          ? parsedSession.isMaster
          : getIsMasterFromAccessToken(parsedSession.accessToken),
    }
  } catch {
    window.localStorage.removeItem(SESSION_KEY)
    return null
  }
}

export function saveSession(session: SessionData) {
  window.localStorage.setItem(SESSION_KEY, JSON.stringify(session))
  notifySessionChanged(session)
}

export function clearSession() {
  window.localStorage.removeItem(SESSION_KEY)
  notifySessionChanged(null)
}

export function subscribeSessionChange(listener: (session: SessionData | null) => void) {
  const handler = (event: Event) => {
    listener((event as CustomEvent<SessionData | null>).detail ?? null)
  }

  window.addEventListener(SESSION_CHANGED_EVENT, handler)

  return () => {
    window.removeEventListener(SESSION_CHANGED_EVENT, handler)
  }
}