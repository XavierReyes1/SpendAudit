import { apiClient } from '../../../core/api/apiClient'
import type { ActualizarPoliticaRequest, Politica } from '../types/politicas.types'

export const obtenerPoliticaAsync = async (): Promise<Politica> => {
  const respuesta = await apiClient.get<Politica>('/politicas')
  return respuesta.data
}

export const actualizarPoliticaAsync = async (dto: ActualizarPoliticaRequest): Promise<Politica> => {
  const respuesta = await apiClient.put<Politica>('/politicas', dto)
  return respuesta.data
}
