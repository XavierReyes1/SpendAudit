import { z } from 'zod'

// Espejo de UsuariosValidators.cs (FluentValidation)

export const crearPuestoSchema = z.object({
  nombre: z
    .string()
    .trim()
    .min(1, 'El nombre del puesto es obligatorio.')
    .max(100, 'El nombre no puede exceder 100 caracteres.'),
})

export type CrearPuestoFormValues = z.infer<typeof crearPuestoSchema>

export const crearUsuarioSchema = z.object({
  nombreCompleto: z
    .string()
    .trim()
    .min(1, 'El nombre es obligatorio.')
    .max(150, 'El nombre no puede exceder 150 caracteres.'),
  email: z
    .string()
    .trim()
    .min(1, 'El correo es obligatorio.')
    .max(200, 'El correo no puede exceder 200 caracteres.')
    .pipe(z.email('El correo no tiene un formato válido.')),
  password: z
    .string()
    .min(1, 'La contraseña es obligatoria.')
    .min(8, 'La contraseña debe tener al menos 8 caracteres.')
    .regex(/[A-Za-z]/, 'La contraseña debe incluir al menos una letra.')
    .regex(/[0-9]/, 'La contraseña debe incluir al menos un número.')
    .max(72, 'La contraseña no puede exceder 72 caracteres.'),
  rol: z.enum(['Admin', 'Empleado', 'Auditor'], { message: 'El rol debe ser Admin, Empleado o Auditor.' }),
  puestoId: z.string(), // '' = sin puesto
})

export type CrearUsuarioFormValues = z.infer<typeof crearUsuarioSchema>
