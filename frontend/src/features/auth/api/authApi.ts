import { apiClient } from '../../../core/api/apiClient'
import type { LoginRequest, LoginResponse, RegistrarEmpresaRequest } from '../types/auth.types'

export const registrarEmpresaAsync = async (datos: RegistrarEmpresaRequest): Promise<LoginResponse> => {
  const respuesta = await apiClient.post<LoginResponse>('/auth/registro-empresa', datos)
  return respuesta.data
}

export const loginAsync = async (credenciales: LoginRequest): Promise<LoginResponse> => {
  const respuesta = await apiClient.post<LoginResponse>('/auth/login', credenciales)
  return respuesta.data
}