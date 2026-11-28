import { useCallback, useEffect, useState } from 'react'
import { extraerMensajeErrorBackend } from '../api/extraerMensajeError'

/**
 * Carga datos asíncronos con estados de carga/error y recarga manual.
 * `cargar` debe ser una referencia estable (una función de módulo o un useCallback).
 */
export function useCargaDatos<T>(cargar: () => Promise<T>, valorInicial: T) {
  const [datos, setDatos] = useState<T>(valorInicial)
  const [cargando, setCargando] = useState<boolean>(true)
  const [error, setError] = useState<string | null>(null)
  const [version, setVersion] = useState<number>(0)

  useEffect(() => {
    let cancelado = false
    setCargando(true)
    setError(null)

    cargar()
      .then((resultado) => {
        if (!cancelado) setDatos(resultado)
      })
      .catch((e: unknown) => {
        if (!cancelado) setError(extraerMensajeErrorBackend(e))
      })
      .finally(() => {
        if (!cancelado) setCargando(false)
      })

    // Evita actualizar el estado de un componente desmontado (o de una petición ya obsoleta)
    return () => {
      cancelado = true
    }
  }, [cargar, version])

  const recargar = useCallback(() => setVersion((v) => v + 1), [])

  return { datos, setDatos, cargando, error, recargar }
}
