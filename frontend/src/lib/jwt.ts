function normalizeBase64Url(value: string) {
  const normalized = value.replace(/-/g, '+').replace(/_/g, '/')
  const padding = normalized.length % 4

  if (padding === 0) {
    return normalized
  }

  return normalized.padEnd(normalized.length + (4 - padding), '=')
}

function decodeBase64Url(value: string) {
  const decoded = window.atob(normalizeBase64Url(value))
  const bytes = Uint8Array.from(decoded, (character) => character.charCodeAt(0))

  return new TextDecoder().decode(bytes)
}

function parseBooleanClaim(value: unknown) {
  if (typeof value === 'boolean') {
    return value
  }

  if (typeof value === 'number') {
    return value === 1
  }

  if (typeof value === 'string') {
    const normalized = value.trim().toLowerCase()
    return normalized === 'true' || normalized === '1'
  }

  return false
}

export function getIsMasterFromAccessToken(accessToken?: string | null) {
  if (!accessToken) {
    return false
  }

  const [, payload] = accessToken.split('.')

  if (!payload) {
    return false
  }

  try {
    const decodedPayload = decodeBase64Url(payload)
    const parsedPayload = JSON.parse(decodedPayload) as { isMaster?: unknown }

    return parseBooleanClaim(parsedPayload.isMaster)
  } catch {
    return false
  }
}