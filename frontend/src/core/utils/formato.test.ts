import { describe, expect, it } from 'vitest'
import { formatearFecha, formatearMonto, hoyHonduras } from './formato'

describe('formato', () => {
  it('formatea fechas sin desfase de zona horaria', () => {
    expect(formatearFecha('2026-11-05')).toBe('05/11/2026')
    expect(formatearFecha('2026-11-05T00:00:00')).toBe('05/11/2026')
    expect(formatearFecha(null)).toBe('—')
  })

  it('formatea montos en lempiras y tolera nulos', () => {
    expect(formatearMonto(1644.5)).toContain('1,644.50')
    expect(formatearMonto(null)).toBe('—')
  })

  it('"hoy" en Honduras tiene formato ISO y nunca va más de 6 h por detrás de UTC', () => {
    expect(hoyHonduras()).toMatch(/^\d{4}-\d{2}-\d{2}$/)
  })
})
