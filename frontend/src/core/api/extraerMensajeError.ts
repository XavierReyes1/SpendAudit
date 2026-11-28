import axios from 'axios'

/** Contrato de error de FluentValidation (ValidationFailure de .NET) que devuelve el middleware global. */
export interface FluentValidationError {
  propertyName: string
  errorMessage: string
  attemptedValue?: unknown
  errorCode?: string
}

/**
 * Extractor universal de errores HTTP (Clase 16). Entiende lo que puede devolver el backend:
 *  - 400 → arreglo [{ propertyName, errorMessage }] (FluentValidation)
 *  - 401/403/404/409/500 → { mensaje }
 *  - ProblemDetails (errores de binding) y strings planos
 *  - Sin respuesta → servidor apagado o bloqueo CORS
 */
export const extraerMensajeErrorBackend = (error: unknown): string => {
  if (axios.isAxiosError(error)) {
    // A. Sin respuesta: servidor apagado, sin red o bloqueo CORS
    if (!error.response) {
      return 'Sin respuesta del servidor (API apagada, sin conexión o bloqueo de seguridad CORS).'
    }

    const status = error.response.status
    const data: unknown = error.response.data

    // B. Arreglo de FluentValidation
    if (Array.isArray(data)) {
      const mensajes = (data as FluentValidationError[])
        .map((item) => item?.errorMessage)
        .filter((msg): msg is string => typeof msg === 'string' && msg.trim().length > 0)

      if (mensajes.length > 0) {
        return `[HTTP ${status}] ${mensajes.join(' | ')}`
      }
    }

    // C. String directo
    if (typeof data === 'string' && data.trim().length > 0) {
      return `[HTTP ${status}] ${data}`
    }

    // D. Objeto JSON: { mensaje } de nuestro middleware, ProblemDetails u otros
    if (data && typeof data === 'object') {
      const obj = data as Record<string, unknown>

      if ('errors' in obj && obj.errors && typeof obj.errors === 'object') {
        const lista = Object.values(obj.errors as Record<string, string[]>).flat()
        if (lista.length > 0) {
          return `[HTTP ${status}] ${lista.join(' | ')}`
        }
      }

      const explicito = obj.mensaje || obj.error || obj.message || obj.detail || obj.title
      if (typeof explicito === 'string' && explicito.trim().length > 0) {
        return `[HTTP ${status}] ${explicito}`
      }
    }

    return `[HTTP ${status}] El servidor rechazó la operación.`
  }

  return error instanceof Error ? error.message : 'Error inesperado en la aplicación.'
}
