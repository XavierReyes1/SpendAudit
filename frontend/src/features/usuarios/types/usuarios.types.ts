import type { Rol } from '../../../core/auth/roles'

// Espejo de Application/Features/Usuarios/DTOs

export interface Puesto {
  id: string
  nombre: string
}

export interface Usuario {
  id: string
  nombreCompleto: string
  email: string
  rol: Rol
  empresaId: string
  puesto: string | null
}

export interface CrearPuestoRequest {
  nombre: string
}

export interface CrearUsuarioRequest {
  nombreCompleto: string
  email: string
  password: string
  rol: Rol
  puestoId: string | null
}
