import type { Rol } from './roles'

/** Usuario autenticado tal como lo devuelve POST /auth/login (UsuarioDto del backend). */
export interface SesionUsuario {
  id: string
  nombreCompleto: string
  email: string
  rol: Rol
  empresaId: string
  puesto: string | null
}

export interface Sesion {
  token: string
  expiraEn: string // ISO 8601 (UTC)
  usuario: SesionUsuario
}

const CLAVE = 'spendaudit.sesion'

/** Evento que el interceptor de Axios emite cuando el backend responde 401 con una sesión activa. */
export const EVENTO_SESION_EXPIRADA = 'spendaudit:sesion-expirada'

export function sesionVigente(sesion: Sesion): boolean {
  const expira = new Date(sesion.expiraEn).getTime()
  return Number.isFinite(expira) && expira > Date.now()
}

export function leerSesion(): Sesion | null {
  try {
    const crudo = localStorage.getItem(CLAVE)
    if (!crudo) return null

    const sesion = JSON.parse(crudo) as Sesion
    if (!sesion?.token || !sesion?.usuario?.empresaId || !sesionVigente(sesion)) {
      limpiarSesion()
      return null
    }
    return sesion
  } catch {
    limpiarSesion()
    return null
  }
}

export function guardarSesion(sesion: Sesion): void {
  localStorage.setItem(CLAVE, JSON.stringify(sesion))
}

export function limpiarSesion(): void {
  localStorage.removeItem(CLAVE)
}

export function obtenerToken(): string | null {
  return leerSesion()?.token ?? null
}
