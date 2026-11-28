import { AxiosError } from 'axios'
import type { AxiosResponse } from 'axios'
import { describe, expect, it } from 'vitest'
import { extraerMensajeErrorBackend } from './extraerMensajeError'

const errorHttp = (status: number, data: unknown): AxiosError =>
  new AxiosError('fallo', 'ERR_BAD_REQUEST', undefined, undefined, {
    status,
    statusText: '',
    headers: {},
    config: {} as AxiosResponse['config'],
    data,
  })

describe('extraerMensajeErrorBackend', () => {
  it('une los errores de FluentValidation (400)', () => {
    const msg = extraerMensajeErrorBackend(
      errorHttp(400, [
        { propertyName: 'RTN', errorMessage: 'El RTN debe tener 14 dígitos, sin guiones.' },
        { propertyName: 'AdminEmail', errorMessage: 'El correo no tiene un formato válido.' },
      ]),
    )
    expect(msg).toBe(
      '[HTTP 400] El RTN debe tener 14 dígitos, sin guiones. | El correo no tiene un formato válido.',
    )
  })

  it.each([
    [401, 'Credenciales inválidas.'],
    [403, 'No tienes permisos para realizar esta acción.'],
    [404, 'El comprobante no existe.'],
    [409, 'Ya existe una empresa registrada con ese RTN.'],
  ])('lee { mensaje } del middleware global (%i)', (status, mensaje) => {
    expect(extraerMensajeErrorBackend(errorHttp(status, { mensaje }))).toBe(`[HTTP ${status}] ${mensaje}`)
  })

  it('lee los errores de binding en formato ProblemDetails', () => {
    const msg = extraerMensajeErrorBackend(errorHttp(400, { errors: { fechaEmision: ['Fecha inválida'] } }))
    expect(msg).toBe('[HTTP 400] Fecha inválida')
  })

  it('acepta un string plano', () => {
    expect(extraerMensajeErrorBackend(errorHttp(500, 'Boom'))).toBe('[HTTP 500] Boom')
  })

  it('avisa cuando no hay respuesta (API apagada o CORS)', () => {
    const sinRespuesta = new AxiosError('Network Error', 'ERR_NETWORK')
    expect(extraerMensajeErrorBackend(sinRespuesta)).toContain('Sin respuesta del servidor')
  })

  it('tiene un mensaje de último recurso para cuerpos desconocidos', () => {
    expect(extraerMensajeErrorBackend(errorHttp(502, {}))).toBe('[HTTP 502] El servidor rechazó la operación.')
  })

  it('maneja errores que no son de Axios', () => {
    expect(extraerMensajeErrorBackend(new Error('algo local'))).toBe('algo local')
    expect(extraerMensajeErrorBackend('texto raro')).toBe('Error inesperado en la aplicación.')
  })
})
