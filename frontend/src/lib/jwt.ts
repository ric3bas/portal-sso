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

function normalizeHexColorClaim(value: unknown) {
  if (typeof value !== 'string') {
    return undefined
  }

  const normalizedValue = value.trim().toUpperCase()

  if (/^#[0-9A-F]{6}$/.test(normalizedValue)) {
    return normalizedValue
  }

  if (/^[0-9A-F]{6}$/.test(normalizedValue)) {
    return `#${normalizedValue}`
  }

  return undefined
}

function decodeAccessTokenPayload(accessToken?: string | null) {
  if (!accessToken) {
    return undefined
  }

  const [, payload] = accessToken.split('.')

  if (!payload) {
    return undefined
  }

  try {
    const decodedPayload = decodeBase64Url(payload)
    return JSON.parse(decodedPayload) as Record<string, unknown>
  } catch {
    return undefined
  }
}

function getClaimValue(payload: Record<string, unknown>, claimName: string) {
  const directKeys = [claimName, claimName[0].toUpperCase() + claimName.slice(1), claimName.toLowerCase(), claimName.toUpperCase()]

  for (const key of directKeys) {
    if (key in payload) {
      return payload[key]
    }
  }

  const normalizedClaimName = claimName.trim().toLowerCase()
  const matchingEntry = Object.entries(payload).find(([key]) => {
    const normalizedKey = key.trim().toLowerCase()
    return normalizedKey.endsWith(`/${normalizedClaimName}`) || normalizedKey.endsWith(`:${normalizedClaimName}`) || normalizedKey === normalizedClaimName
  })

  return matchingEntry?.[1]
}

function getMasterClaimValue(payload: Record<string, unknown>) {
  return getClaimValue(payload, 'isMaster')
}

export function tryGetIsMasterFromAccessToken(accessToken?: string | null) {
  const parsedPayload = decodeAccessTokenPayload(accessToken)

  if (!parsedPayload) {
    return undefined
  }

  const claimValue = getMasterClaimValue(parsedPayload)

  if (claimValue === undefined) {
    return undefined
  }

  return parseBooleanClaim(claimValue)
}

export function getIsMasterFromAccessToken(accessToken?: string | null) {
  return tryGetIsMasterFromAccessToken(accessToken) ?? false
}

export function getThemeColorsFromAccessToken(accessToken?: string | null) {
  const parsedPayload = decodeAccessTokenPayload(accessToken)

  if (!parsedPayload) {
    return {
      corPrimaria: undefined,
      corSecundaria: undefined,
    }
  }

  return {
    corPrimaria: normalizeHexColorClaim(getClaimValue(parsedPayload, 'corPrimaria')),
    corSecundaria: normalizeHexColorClaim(getClaimValue(parsedPayload, 'corSecundaria')),
  }
}