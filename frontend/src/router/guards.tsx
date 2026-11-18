import { Navigate, Outlet, useLocation } from 'react-router-dom'
import type { Rol } from '../core/auth/roles'
import { useAuth } from '../features/auth/hooks/useAuth'

/** Bloquea a quien no tenga JWT: lo envía al login y recuerda a dónde quería ir. */
export function RutaProtegida() {
  const { estaAutenticado } = useAuth()
  const location = useLocation()

  if (!estaAutenticado) {
    return <Navigate to="/login" replace state={{ desde: location.pathname }} />
  }
  return <Outlet />
}

/** Las pantallas de login y registro no tienen sentido con sesión activa. */
export function RutaPublica() {
  const { estaAutenticado } = useAuth()
  return estaAutenticado ? <Navigate to="/" replace /> : <Outlet />
}

/** Restringe una sección a ciertos roles. El backend vuelve a validar (403): esto es solo UX. */
export function RutaPorRol({ roles }: { roles: Rol[] }) {
  const { tieneRol } = useAuth()

  if (!tieneRol(...roles)) {
    return (
      <div className="rounded-xl border border-amber-500/30 bg-amber-950/30 p-10 text-center">
        <div className="mb-2 text-4xl">🔒</div>
        <p className="text-lg font-bold text-white">No tienes permisos para ver esta sección</p>
        <p className="mt-1 text-sm text-slate-400">Pide acceso al administrador de tu empresa.</p>
      </div>
    )
  }
  return <Outlet />
}

/** Cada rol aterriza en su pantalla principal. */
export function RedireccionInicial() {
  const { tieneRol } = useAuth()
  return <Navigate to={tieneRol('Empleado', 'Admin') ? '/album' : '/lotes'} replace />
}