import { createContext, useContext, useEffect, useState, type PropsWithChildren } from 'react'
import { getSession, subscribeSessionChange } from '../lib/storage'
import type { SessionData } from '../types/api'

type ThemeMode = 'light' | 'dark'

interface ThemeContextValue {
  theme: ThemeMode
  toggleTheme: () => void
  setTheme: (theme: ThemeMode) => void
}

const THEME_STORAGE_KEY = 'portal-sso-theme'

const DEFAULT_BRAND_COLORS = {
  light: {
    corPrimaria: '#2563EB',
    corSecundaria: '#1D4ED8',
  },
  dark: {
    corPrimaria: '#60A5FA',
    corSecundaria: '#93C5FD',
  },
} as const

const ThemeContext = createContext<ThemeContextValue | null>(null)

function normalizeHexColor(value: string | undefined, fallback: string) {
  if (!value) {
    return fallback
  }

  const normalizedValue = value.trim().toUpperCase()

  return /^#[0-9A-F]{6}$/.test(normalizedValue) ? normalizedValue : fallback
}

function hexToRgb(value: string) {
  const normalizedValue = value.replace('#', '')

  return {
    r: Number.parseInt(normalizedValue.slice(0, 2), 16),
    g: Number.parseInt(normalizedValue.slice(2, 4), 16),
    b: Number.parseInt(normalizedValue.slice(4, 6), 16),
  }
}

function mixHexColors(baseColor: string, blendColor: string, ratio: number) {
  const base = hexToRgb(baseColor)
  const blend = hexToRgb(blendColor)
  const clampRatio = Math.min(1, Math.max(0, ratio))
  const mixChannel = (baseChannel: number, blendChannel: number) => Math.round(baseChannel * (1 - clampRatio) + blendChannel * clampRatio)

  const red = mixChannel(base.r, blend.r)
  const green = mixChannel(base.g, blend.g)
  const blue = mixChannel(base.b, blend.b)

  return `#${[red, green, blue].map((channel) => channel.toString(16).padStart(2, '0')).join('').toUpperCase()}`
}

function toRgbCssValue(value: string) {
  const { r, g, b } = hexToRgb(value)
  return `${r} ${g} ${b}`
}

function getContrastColor(value: string) {
  const { r, g, b } = hexToRgb(value)
  const luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255

  return luminance > 0.62 ? '#0F172A' : '#FFFFFF'
}

function applyBrandColors(theme: ThemeMode, session: SessionData | null) {
  const root = document.documentElement
  const defaults = DEFAULT_BRAND_COLORS[theme]
  const corPrimaria = normalizeHexColor(session?.corPrimaria, defaults.corPrimaria)
  const corSecundaria = normalizeHexColor(session?.corSecundaria, defaults.corSecundaria)
  const neutralBase = theme === 'dark' ? '#0F172A' : '#FFFFFF'
  const borderBase = theme === 'dark' ? '#1E293B' : '#DBEAFE'
  const softColor = mixHexColors(corPrimaria, neutralBase, theme === 'dark' ? 0.78 : 0.88)
  const surfaceColor = mixHexColors(corSecundaria, borderBase, theme === 'dark' ? 0.72 : 0.82)
  const pageBackgroundColor = mixHexColors(corSecundaria, neutralBase, theme === 'dark' ? 0.84 : 0.9)
  const headerBackgroundColor = mixHexColors(corSecundaria, neutralBase, theme === 'dark' ? 0.7 : 0.8)
  const tableFooterBackgroundColor = mixHexColors(corSecundaria, neutralBase, theme === 'dark' ? 0.76 : 0.86)

  root.style.setProperty('--brand', corPrimaria)
  root.style.setProperty('--brand-dark', corSecundaria)
  root.style.setProperty('--brand-contrast', getContrastColor(corPrimaria))
  root.style.setProperty('--brand-rgb', toRgbCssValue(corPrimaria))
  root.style.setProperty('--brand-secondary-rgb', toRgbCssValue(corSecundaria))
  root.style.setProperty('--brand-soft', softColor)
  root.style.setProperty('--brand-surface', surfaceColor)
  root.style.setProperty('--page-bg', pageBackgroundColor)
  root.style.setProperty('--header-bg', headerBackgroundColor)
  root.style.setProperty('--table-footer-bg', tableFooterBackgroundColor)
}

function getInitialTheme(): ThemeMode {
  if (typeof window === 'undefined') {
    return 'light'
  }

  const storedTheme = window.localStorage.getItem(THEME_STORAGE_KEY)

  if (storedTheme === 'light' || storedTheme === 'dark') {
    return storedTheme
  }

  return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light'
}

export function ThemeProvider({ children }: PropsWithChildren) {
  const [theme, setTheme] = useState<ThemeMode>(() => getInitialTheme())
  const [session, setSession] = useState<SessionData | null>(() => getSession())

  useEffect(() => {
    return subscribeSessionChange((nextSession) => {
      setSession(nextSession)
    })
  }, [])

  useEffect(() => {
    const root = document.documentElement

    root.classList.toggle('dark', theme === 'dark')
    root.style.colorScheme = theme
    window.localStorage.setItem(THEME_STORAGE_KEY, theme)
    applyBrandColors(theme, session)
  }, [session, theme])

  function toggleTheme() {
    setTheme((current) => (current === 'dark' ? 'light' : 'dark'))
  }

  return <ThemeContext.Provider value={{ theme, toggleTheme, setTheme }}>{children}</ThemeContext.Provider>
}

export function useTheme() {
  const context = useContext(ThemeContext)

  if (!context) {
    throw new Error('useTheme must be used within ThemeProvider')
  }

  return context
}