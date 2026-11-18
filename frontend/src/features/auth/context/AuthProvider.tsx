import { useCallback, useEffect, useMemo, useState } from 'react'
import type { ReactNode } from 'react'
import type { Rol } from '../../../core/auth/roles'
import {
  EVENTO_SESION_EXPIRADA,
  guardarSesion,
  leerSesion,
  limpiarSesion,
} from '../../../core/auth/sesionStorage'
import type { Sesion } from '../../../core/auth/sesionStorage'
import type { LoginResponse } from '../types/auth.types'
import { AuthContext } from './AuthContext'
import type { AuthContextValue } from './AuthContext'

// setTimeout no admite retardos mayores a 2^31-1 ms
const MAX_TIMEOUT = 2_147_483_647

export function AuthProvider({ children }: { children: ReactNode }) {
  const [sesion, setSesion] = useState<Sesion | null>(() => leerSesion())

  // El interceptor de Axios avisa cuando el backend rechaza el token (401)
  useEffect(() => {
    const alExpirar = () => setSesion(null)
    window.addEventListener(EVENTO_SESION_EXPIRADA, alExpirar)
    return () => window.removeEventListener(EVENTO_SESION_EXPIRADA, alExpirar)
  }, [])

  // Cierre automático cuando vence el JWT
  useEffect(() => {
    if (!sesion) return
    const restante = new Date(sesion.expiraEn).getTime() - Date.now()
    const temporizador = setTimeout(
      () => {
        limpiarSesion()
        setSesion(null)
      },
      Math.max(0, Math.min(restante, MAX_TIMEOUT)),
    )
    return () => clearTimeout(temporizador)
  }, [sesion])

  const iniciarSesion = useCallback((respuesta: LoginResponse) => {
    const nueva: Sesion = {
      token: respuesta.token,
      expiraEn: respuesta.expiraEn,
      usuario: respuesta.usuario,
    }
    guardarSesion(nueva)
    setSesion(nueva)
  }, [])

  const cerrarSesion = useCallback(() => {
    limpiarSesion()
    setSesion(null)
  }, [])

  const valor = useMemo<AuthContextValue>(
    () => ({
      usuario: sesion?.usuario ?? null,
      estaAutenticado: sesion !== null,
      iniciarSesion,
      cerrarSesion,
      tieneRol: (...roles: Rol[]) => (sesion ? roles.includes(sesion.usuario.rol) : false),
    }),
    [sesion, iniciarSesion, cerrarSesion],
  )

  return <AuthContext.Provider value={valor}>{children}</AuthContext.Provider>
}