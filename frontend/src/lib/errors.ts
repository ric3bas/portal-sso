import axios from 'axios'
import type { CustomProblemDetails } from '../types/api'

export function getErrorMessage(error: unknown, fallback = 'Nao foi possivel concluir a operacao.') {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data as CustomProblemDetails | string | undefined

    if (typeof data === 'string' && data.trim()) {
      return data
    }

    if (data && typeof data === 'object' && Array.isArray(data.errors) && data.errors.length > 0) {
      return data.errors.join(' | ')
    }

    if (error.message) {
      return error.message
    }
  }

  if (error instanceof Error && error.message) {
    return error.message
  }

  return fallback
}