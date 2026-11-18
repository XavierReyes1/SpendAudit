import { describe, expect, it } from 'vitest'
import { loginSchema, registroEmpresaSchema } from './auth.schema'

const registroValido = {
  nombreRazonSocial: 'Ferretería El Buen Precio',
  rtn: '08011999123456',
  adminNombre: 'Ana Gerente',
  adminEmail: 'ana@ferreteria.hn',
  adminPassword: 'Clave1234',
  confirmarPassword: 'Clave1234',
}

const mensajes = (r: ReturnType<typeof registroEmpresaSchema.safeParse>) =>
  r.success ? [] : r.error.issues.map((i) => i.message)

describe('registroEmpresaSchema (espejo de RegistrarEmpresaRequestValidator)', () => {
  it('acepta datos válidos', () => {
    expect(registroEmpresaSchema.safeParse(registroValido).success).toBe(true)
  })

  it.each([
    ['0801-1999-123456', 'con guiones'],
    ['1234', 'muy corto'],
    ['0801199912345A', 'con letras'],
    ['', 'vacío'],
  ])('rechaza el RTN %s (%s)', (rtn) => {
    expect(registroEmpresaSchema.safeParse({ ...registroValido, rtn }).success).toBe(false)
  })

  it('exige contraseña de 8+ caracteres con letra y número', () => {
    const probar = (p: string) =>
      mensajes(registroEmpresaSchema.safeParse({ ...registroValido, adminPassword: p, confirmarPassword: p }))

    expect(probar('corta1')).toContain('La contraseña debe tener al menos 8 caracteres.')
    expect(probar('soloLetrasAqui')).toContain('La contraseña debe incluir al menos un número.')
    expect(probar('12345678')).toContain('La contraseña debe incluir al menos una letra.')
    expect(probar('a1'.repeat(40))).toContain('La contraseña no puede exceder 72 caracteres.')
  })

  it('rechaza un correo inválido', () => {
    expect(mensajes(registroEmpresaSchema.safeParse({ ...registroValido, adminEmail: 'no-es-correo' }))).toContain(
      'El correo no tiene un formato válido.',
    )
  })

  it('exige que las contraseñas coincidan (solo existe en el cliente)', () => {
    expect(
      mensajes(registroEmpresaSchema.safeParse({ ...registroValido, confirmarPassword: 'Otra12345' })),
    ).toContain('Las contraseñas no coinciden.')
  })
})

describe('loginSchema', () => {
  it('acepta credenciales con formato correcto', () => {
    expect(loginSchema.safeParse({ email: 'ana@ferreteria.hn', password: 'x' }).success).toBe(true)
  })

  it('rechaza campos vacíos', () => {
    expect(loginSchema.safeParse({ email: '', password: '' }).success).toBe(false)
  })
})