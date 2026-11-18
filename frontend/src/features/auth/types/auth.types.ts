import type { SesionUsuario } from '../../../core/auth/sesionStorage'

// Espejo de los DTOs de Application/Features/Auth/DTOs (backend .NET)

export interface RegistrarEmpresaRequest {
  nombreRazonSocial: string
  rtn: string
  adminNombre: string
  adminEmail: string
  adminPassword: string
}

export interface LoginRequest {
  email: string
  password: string
}

export interface LoginResponse {
  token: string
  expiraEn: string
  usuario: SesionUsuario
}