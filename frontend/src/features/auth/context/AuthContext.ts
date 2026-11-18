import { createContext } from 'react'
import type { Rol } from '../../../core/auth/roles'
import type { SesionUsuario } from '../../../core/auth/sesionStorage'
import type { LoginResponse } from '../types/auth.types'

export interface AuthContextValue {
  usuario: SesionUsuario | null
  estaAutenticado: boolean
  iniciarSesion: (respuesta: LoginResponse) => void
  cerrarSesion: () => void
  tieneRol: (...roles: Rol[]) => boolean
}

export const AuthContext = createContext<AuthContextValue | null>(null)