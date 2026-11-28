// Espejo de Application/Features/Politicas/DTOs

export interface TopePuesto {
  puestoId: string
  puesto: string | null
  topeDiarioHNL: number
}

export interface Politica {
  diasVigenciaMaxima: number
  topes: TopePuesto[]
  terminosRestringidos: string[]
}

export interface ActualizarPoliticaRequest {
  diasVigenciaMaxima: number
  topes: { puestoId: string; topeDiarioHNL: number }[]
  terminosRestringidos: string[]
}
