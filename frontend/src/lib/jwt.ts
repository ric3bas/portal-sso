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

function getMasterClaimValue(payload: Record<string, unknown>) {
  const directKeys = ['isMaster', 'IsMaster', 'ismaster']

  for (const key of directKeys) {
    if (key in payload) {
      return payload[key]
    }
  }

  const matchingEntry = Object.entries(payload).find(([key]) => {
    const normalizedKey = key.trim().toLowerCase()
    return normalizedKey.endsWith('/ismaster') || normalizedKey.endsWith(':ismaster') || normalizedKey === 'ismaster'
  })

  return matchingEntry?.[1]
}

export function tryGetIsMasterFromAccessToken(accessToken?: string | null) {
  if (!accessToken) {
    return undefined
  }

  const [, payload] = accessToken.split('.')

  if (!payload) {
    return undefined
  }

  try {
    const decodedPayload = decodeBase64Url(payload)
    const parsedPayload = JSON.parse(decodedPayload) as Record<string, unknown>
    const claimValue = getMasterClaimValue(parsedPayload)

    if (claimValue === undefined) {
      return undefined
    }

    return parseBooleanClaim(claimValue)
  } catch {
    return undefined
  }
}

export function getIsMasterFromAccessToken(accessToken?: string | null) {
  return tryGetIsMasterFromAccessToken(accessToken) ?? false
}