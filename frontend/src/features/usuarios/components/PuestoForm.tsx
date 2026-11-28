import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { extraerMensajeErrorBackend } from '../../../core/api/extraerMensajeError'
import { Alert, Button, Input } from '../../../core/ui'
import { crearPuestoAsync } from '../api/usuariosApi'
import { crearPuestoSchema } from '../schemas/usuarios.schema'
import type { CrearPuestoFormValues } from '../schemas/usuarios.schema'
import type { Puesto } from '../types/usuarios.types'

export function PuestoForm({ onCreado }: { onCreado: (puesto: Puesto) => void }) {
  const [errorServidor, setErrorServidor] = useState<string | null>(null)

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isValid, isSubmitting },
  } = useForm<CrearPuestoFormValues>({
    resolver: zodResolver(crearPuestoSchema),
    mode: 'onChange',
    defaultValues: { nombre: '' },
  })

  const enviar = async (datos: CrearPuestoFormValues) => {
    try {
      setErrorServidor(null)
      const creado = await crearPuestoAsync(datos)
      reset()
      onCreado(creado)
    } catch (error) {
      setErrorServidor(extraerMensajeErrorBackend(error)) // p. ej. 409: nombre repetido
    }
  }

  return (
    <form
      onSubmit={handleSubmit(enviar)}
      className="h-fit space-y-4 rounded-xl border border-indigo-500/40 bg-slate-800/90 p-5 shadow-lg"
      noValidate
    >
      <h2 className="text-base font-bold text-white">✨ Nuevo puesto</h2>
      <Input
        etiqueta="Nombre del puesto"
        placeholder="Ej. Vendedor"
        disabled={isSubmitting}
        error={errors.nombre?.message}
        {...register('nombre', { onChange: () => setErrorServidor(null) })}
      />
      {errorServidor && <Alert titulo="Respuesta del servidor:">{errorServidor}</Alert>}
      <Button type="submit" className="w-full" disabled={!isValid} cargando={isSubmitting}>
        Crear puesto
      </Button>
    </form>
  )
}
