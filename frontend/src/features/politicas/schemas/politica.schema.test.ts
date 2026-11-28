import { describe, expect, it } from 'vitest'
import { parsearTerminos, politicaSchema } from './politica.schema'

const valida = {
  diasVigenciaMaxima: '15',
  topes: [{ puestoId: 'p1', topeDiarioHNL: '1000' }],
  terminosTexto: 'alcohol\ncerveza',
}
const mensajes = (datos: object) => {
  const r = politicaSchema.safeParse({ ...valida, ...datos })
  return r.success ? [] : r.error.issues.map((i) => i.message)
}

describe('politicaSchema (espejo de UpdatePoliticaRequestValidator)', () => {
  it('acepta una política válida y una vacía', () => {
    expect(mensajes({})).toEqual([])
    expect(mensajes({ topes: [], terminosTexto: '' })).toEqual([])
  })

  it.each(['0', '366', '-1', 'abc', '', '1.5'])('rechaza la vigencia %s', (dias) => {
    expect(mensajes({ diasVigenciaMaxima: dias }).length).toBeGreaterThan(0)
  })

  it.each(['1', '365'])('acepta la vigencia en el límite (%s)', (dias) => {
    expect(mensajes({ diasVigenciaMaxima: dias })).toEqual([])
  })

  it('el tope debe ser mayor que cero y con máximo 2 decimales', () => {
    expect(mensajes({ topes: [{ puestoId: 'p1', topeDiarioHNL: '0' }] })).toContain(
      'El tope diario debe ser mayor que cero.',
    )
    expect(mensajes({ topes: [{ puestoId: 'p1', topeDiarioHNL: '10.123' }] })).toContain(
      'Usa un número válido con máximo 2 decimales.',
    )
  })

  it('exige elegir el puesto', () => {
    expect(mensajes({ topes: [{ puestoId: '', topeDiarioHNL: '100' }] })).toContain('Selecciona un puesto.')
  })

  it('no admite dos topes para el mismo puesto', () => {
    expect(
      mensajes({
        topes: [
          { puestoId: 'p1', topeDiarioHNL: '100' },
          { puestoId: 'p1', topeDiarioHNL: '200' },
        ],
      }),
    ).toContain('Hay más de un tope para el mismo puesto.')
  })

  it('limita los términos a 60 caracteres y a 200 en total', () => {
    expect(mensajes({ terminosTexto: 'x'.repeat(61) })).toContain('Un término no puede exceder 60 caracteres.')
    const demasiados = Array.from({ length: 201 }, (_, i) => `t${i}`).join('\n')
    expect(mensajes({ terminosTexto: demasiados })).toContain('Máximo 200 términos restringidos.')
  })
})

describe('parsearTerminos', () => {
  it('separa por líneas o comas y descarta vacíos', () => {
    expect(parsearTerminos(' alcohol \n\ncerveza, licor ,')).toEqual(['alcohol', 'cerveza', 'licor'])
  })
})
