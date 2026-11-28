import axios from 'axios'
import { EVENTO_SESION_EXPIRADA, limpiarSesion, obtenerToken } from '../auth/sesionStorage'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL

if (!API_BASE_URL) {
  console.error('🚨 ERROR CRÍTICO: No se detectó VITE_API_BASE_URL (revisa frontend/.env)')
}

// Sin Content-Type fijo: Axios usa JSON para objetos y deja que el navegador arme el boundary en FormData.
export const apiClient = axios.create({
  baseURL: API_BASE_URL,
})

// Interceptor de salida: adjunta el JWT (Bearer) en cada petición.
apiClient.interceptors.request.use((config) => {
  const token = obtenerToken()
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// Interceptor de entrada: si el backend rechaza el token (401) con una sesión activa,
// se cierra la sesión local y se avisa a la app para que redirija al login.
// Un 401 del propio login (credenciales inválidas) no tiene token previo y no dispara esto.
apiClient.interceptors.response.use(
  (respuesta) => respuesta,
  (error: unknown) => {
    if (axios.isAxiosError(error) && error.response?.status === 401 && obtenerToken()) {
      limpiarSesion()
      window.dispatchEvent(new Event(EVENTO_SESION_EXPIRADA))
    }
    return Promise.reject(error)
  },
)
