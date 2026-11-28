import { z } from 'zod'

// Espejo de PoliticasValidators.cs (FluentValidation).
// Los campos numéricos viajan como texto en el formulario y se convierten al enviar.

const MAX_TOPE = 10_000_000
const MAX_TERMINOS = 200
const MAX_LARGO_TERMINO = 60

export const topeSchema = z.object({
  puestoId: z.string().min(1, 'Selecciona un puesto.'),
  topeDiarioHNL: z
    .string()
    .trim()
    .min(1, 'El tope es obligatorio.')
    .regex(/^\d+(\.\d{1,2})?$/, 'Usa un número válido con máximo 2 decimales.')
    .refine((v) => Number(v) > 0, 'El tope diario debe ser mayor que cero.')
    .refine((v) => Number(v) <= MAX_TOPE, 'El tope diario es demasiado alto.'),
})

/** "cerveza\nlicor" → ['cerveza', 'licor'] (sin vacíos). */
export const parsearTerminos = (texto: string): string[] =>
  texto
    .split(/[\n,]/)
    .map((t) => t.trim())
    .filter((t) => t.length > 0)

export const politicaSchema = z.object({
  diasVigenciaMaxima: z
    .string()
    .trim()
    .min(1, 'La vigencia es obligatoria.')
    .regex(/^\d+$/, 'La vigencia debe ser un número entero de días.')
    .refine((v) => Number(v) >= 1 && Number(v) <= 365, 'La vigencia debe estar entre 1 y 365 días.'),
  topes: z
    .array(topeSchema)
    .refine((t) => new Set(t.map((x) => x.puestoId)).size === t.length, 'Hay más de un tope para el mismo puesto.'),
  terminosTexto: z
    .string()
    .refine((t) => parsearTerminos(t).length <= MAX_TERMINOS, `Máximo ${MAX_TERMINOS} términos restringidos.`)
    .refine(
      (t) => parsearTerminos(t).every((x) => x.length <= MAX_LARGO_TERMINO),
      `Un término no puede exceder ${MAX_LARGO_TERMINO} caracteres.`,
    ),
})

export type PoliticaFormValues = z.infer<typeof politicaSchema>
