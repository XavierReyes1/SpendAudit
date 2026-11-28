import { apiClient } from '../../../core/api/apiClient'
import type { CrearPuestoRequest, CrearUsuarioRequest, Puesto, Usuario } from '../types/usuarios.types'

export const obtenerPuestosAsync = async (): Promise<Puesto[]> => {
  const respuesta = await apiClient.get<Puesto[]>('/puestos')
  return respuesta.data
}

export const crearPuestoAsync = async (dto: CrearPuestoRequest): Promise<Puesto> => {
  const respuesta = await apiClient.post<Puesto>('/puestos', dto)
  return respuesta.data
}

export const obtenerUsuariosAsync = async (): Promise<Usuario[]> => {
  const respuesta = await apiClient.get<Usuario[]>('/usuarios')
  return respuesta.data
}

export const crearUsuarioAsync = async (dto: CrearUsuarioRequest): Promise<Usuario> => {
  const respuesta = await apiClient.post<Usuario>('/usuarios', dto)
  return respuesta.data
}
