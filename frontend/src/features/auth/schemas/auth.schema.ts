import { z } from 'zod'

// Espejo de AuthValidators.cs (FluentValidation) del backend.

const email = z
  .string()
  .trim()
  .min(1, 'El correo es obligatorio.')
  .max(200, 'El correo no puede exceder 200 caracteres.')
  .pipe(z.email('El correo no tiene un formato válido.'))

export const loginSchema = z.object({
  email,
  password: z.string().min(1, 'La contraseña es obligatoria.'),
})

export type LoginFormValues = z.infer<typeof loginSchema>

export const registroEmpresaSchema = z
  .object({
    nombreRazonSocial: z
      .string()
      .trim()
      .min(1, 'El nombre de la empresa es obligatorio.')
      .max(200, 'El nombre no puede exceder 200 caracteres.'),
    rtn: z
      .string()
      .trim()
      .min(1, 'El RTN es obligatorio.')
      .regex(/^\d{14}$/, 'El RTN debe tener 14 dígitos, sin guiones.'),
    adminNombre: z
      .string()
      .trim()
      .min(1, 'El nombre del administrador es obligatorio.')
      .max(150, 'El nombre no puede exceder 150 caracteres.'),
    adminEmail: email,
    adminPassword: z
      .string()
      .min(1, 'La contraseña es obligatoria.')
      .min(8, 'La contraseña debe tener al menos 8 caracteres.')
      .regex(/[A-Za-z]/, 'La contraseña debe incluir al menos una letra.')
      .regex(/[0-9]/, 'La contraseña debe incluir al menos un número.')
      .max(72, 'La contraseña no puede exceder 72 caracteres.'),
    // Solo existe en el cliente: el backend no la recibe.
    confirmarPassword: z.string().min(1, 'Confirma tu contraseña.'),
  })
  .refine((v) => v.adminPassword === v.confirmarPassword, {
    path: ['confirmarPassword'],
    message: 'Las contraseñas no coinciden.',
  })

export type RegistroEmpresaFormValues = z.infer<typeof registroEmpresaSchema>