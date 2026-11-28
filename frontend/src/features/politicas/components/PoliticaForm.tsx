import { useState } from 'react'
import { useFieldArray, useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { extraerMensajeErrorBackend } from '../../../core/api/extraerMensajeError'
import { Alert, Button, Input, Select, Textarea } from '../../../core/ui'
import type { Puesto } from '../../usuarios/types/usuarios.types'
import { actualizarPoliticaAsync } from '../api/politicasApi'
import { parsearTerminos, politicaSchema } from '../schemas/politica.schema'
import type { PoliticaFormValues } from '../schemas/politica.schema'
import type { Politica } from '../types/politicas.types'

interface PoliticaFormProps {
  politica: Politica
  puestos: Puesto[]
  soloLectura: boolean
}

const aValoresFormulario = (p: Politica): PoliticaFormValues => ({
  diasVigenciaMaxima: String(p.diasVigenciaMaxima),
  topes: p.topes.map((t) => ({ puestoId: t.puestoId, topeDiarioHNL: String(t.topeDiarioHNL) })),
  terminosTexto: p.terminosRestringidos.join('\n'),
})

export function PoliticaForm({ politica, puestos, soloLectura }: PoliticaFormProps) {
  const [errorServidor, setErrorServidor] = useState<string | null>(null)
  const [guardado, setGuardado] = useState<boolean>(false)
  // Última versión confirmada por el servidor: a ella vuelve «Descartar cambios»
  const [base, setBase] = useState<Politica>(politica)

  const {
    register,
    control,
    handleSubmit,
    reset,
    formState: { errors, isValid, isDirty, isSubmitting },
  } = useForm<PoliticaFormValues>({
    resolver: zodResolver(politicaSchema),
    mode: 'onChange',
    defaultValues: aValoresFormulario(politica),
  })

  const { fields, append, remove } = useFieldArray({ control, name: 'topes' })

  const marcarEdicion = () => {
    setErrorServidor(null)
    setGuardado(false)
  }

  const enviar = async (valores: PoliticaFormValues) => {
    try {
      marcarEdicion()
      const actualizada = await actualizarPoliticaAsync({
        diasVigenciaMaxima: Number(valores.diasVigenciaMaxima),
        topes: valores.topes.map((t) => ({ puestoId: t.puestoId, topeDiarioHNL: Number(t.topeDiarioHNL) })),
        terminosRestringidos: parsearTerminos(valores.terminosTexto),
      })
      setBase(actualizada)
      reset(aValoresFormulario(actualizada)) // refleja lo que el servidor normalizó (minúsculas, sin duplicados)
      setGuardado(true)
    } catch (error) {
      setErrorServidor(extraerMensajeErrorBackend(error))
    }
  }

  const errorTopes = errors.topes?.root?.message ?? errors.topes?.message
  const puestosSinTope = puestos.filter((p) => !fields.some((f) => f.puestoId === p.id))

  return (
    <form onSubmit={handleSubmit(enviar)} className="space-y-8" noValidate>
      <fieldset disabled={soloLectura || isSubmitting} className="space-y-8">
        {/* Vigencia */}
        <section className="rounded-xl border border-slate-700 bg-slate-800/60 p-5">
          <h2 className="text-base font-bold text-white">📅 Vigencia máxima</h2>
          <p className="mb-4 mt-1 text-sm text-slate-400">
            Una factura con más días de antigüedad que este límite se marca en <strong className="text-amber-400">Amarillo</strong>.
          </p>
          <div className="max-w-xs">
            <Input
              etiqueta="Días"
              inputMode="numeric"
              error={errors.diasVigenciaMaxima?.message}
              {...register('diasVigenciaMaxima', { onChange: marcarEdicion })}
            />
          </div>
        </section>

        {/* Topes por puesto */}
        <section className="rounded-xl border border-slate-700 bg-slate-800/60 p-5">
          <h2 className="text-base font-bold text-white">💰 Tope diario por puesto</h2>
          <p className="mb-4 mt-1 text-sm text-slate-400">
            Si lo gastado en un día supera el tope del puesto del empleado, la factura se marca en{' '}
            <strong className="text-amber-400">Amarillo</strong>. Un puesto sin tope no tiene límite.
          </p>

          {puestos.length === 0 && (
            <Alert tipo="advertencia">
              Primero crea puestos en «Equipo y puestos» para poder asignarles un tope.
            </Alert>
          )}

          <div className="space-y-3">
            {fields.map((campo, i) => (
              <div key={campo.id} className="grid items-start gap-3 sm:grid-cols-[1fr_12rem_auto]">
                <Select
                  etiqueta="Puesto"
                  error={errors.topes?.[i]?.puestoId?.message}
                  {...register(`topes.${i}.puestoId`, { onChange: marcarEdicion })}
                >
                  <option value="">Selecciona un puesto…</option>
                  {puestos.map((p) => (
                    <option key={p.id} value={p.id}>
                      {p.nombre}
                    </option>
                  ))}
                </Select>
                <Input
                  etiqueta="Tope diario (L)"
                  inputMode="decimal"
                  error={errors.topes?.[i]?.topeDiarioHNL?.message}
                  {...register(`topes.${i}.topeDiarioHNL`, { onChange: marcarEdicion })}
                />
                <div className="sm:pt-6">
                  <Button
                    variante="fantasma"
                    aria-label="Quitar tope"
                    onClick={() => {
                      remove(i)
                      marcarEdicion()
                    }}
                  >
                    Quitar
                  </Button>
                </div>
              </div>
            ))}
          </div>

          {errorTopes && (
            <p role="alert" className="mt-3 text-xs font-medium text-amber-400">
              ⚡ {errorTopes}
            </p>
          )}

          <div className="mt-4">
            <Button
              variante="secundario"
              disabled={puestosSinTope.length === 0}
              onClick={() => {
                append({ puestoId: '', topeDiarioHNL: '' })
                marcarEdicion()
              }}
            >
              + Agregar tope
            </Button>
          </div>
        </section>

        {/* Términos restringidos */}
        <section className="rounded-xl border border-slate-700 bg-slate-800/60 p-5">
          <h2 className="text-base font-bold text-white">🚫 Términos restringidos</h2>
          <p className="mb-4 mt-1 text-sm text-slate-400">
            Una factura que contenga alguno de estos términos se marca en <strong className="text-red-400">Rojo</strong>.
            Uno por línea. Coincide con el inicio de la palabra, sin importar mayúsculas ni tildes
            («cerveza» detecta «Cervezas»).
          </p>
          <Textarea
            etiqueta="Términos"
            rows={6}
            placeholder={'alcohol\ncerveza\nlicor'}
            error={errors.terminosTexto?.message}
            {...register('terminosTexto', { onChange: marcarEdicion })}
          />
        </section>
      </fieldset>

      {errorServidor && <Alert titulo="Respuesta del servidor:">{errorServidor}</Alert>}
      {guardado && <Alert tipo="exito">Política actualizada. Se aplicará a las próximas liquidaciones.</Alert>}

      {!soloLectura && (
        <div className="flex gap-3">
          <Button type="submit" disabled={!isValid || !isDirty} cargando={isSubmitting}>
            {isSubmitting ? 'Guardando…' : '💾 Guardar política'}
          </Button>
          <Button
            variante="fantasma"
            disabled={!isDirty || isSubmitting}
            onClick={() => {
              reset(aValoresFormulario(base))
              marcarEdicion()
            }}
          >
            Descartar cambios
          </Button>
        </div>
      )}
    </form>
  )
}
