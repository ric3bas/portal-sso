import type { LoginResponse } from '../types/api'

export function getAccessTokenFromResponse(response: LoginResponse) {
  return response.access_token ?? response.accessToken ?? response.token ?? ''
}

export function getRefreshTokenFromResponse(response: LoginResponse) {
  return response.refresh_token ?? response.refreshToken ?? undefined
}

export function getExpiresInMinutesFromResponse(response: LoginResponse) {
  return response.expire_in_minutes ?? response.expireInMinutes ?? null
}