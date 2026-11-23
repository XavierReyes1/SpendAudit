/// <reference types="vite/client" />

interface ImportMetaEnv {
  /** Base de la API, incluyendo el prefijo de versión. Ej: http://localhost:5080/api/v1 */
  readonly VITE_API_BASE_URL: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
