/** Roles del backend (Claim "role" del JWT). */
export type Rol = 'Admin' | 'Empleado' | 'Auditor'

export const ROLES = {
  Admin: 'Admin',
  Empleado: 'Empleado',
  Auditor: 'Auditor',
} as const satisfies Record<Rol, Rol>
