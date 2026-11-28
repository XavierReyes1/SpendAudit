import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { extraerMensajeErrorBackend } from '../../../core/api/extraerMensajeError'
import { Alert, Button, Input, Select } from '../../../core/ui'
import { crearUsuarioAsync } from '../api/usuariosApi'
import { crearUsuarioSchema } from '../schemas/usuarios.schema'
import type { CrearUsuarioFormValues } from '../schemas/usuarios.schema'
import type { Puesto, Usuario } from '../types/usuarios.types'

interface UsuarioFormProps {
  puestos: Puesto[]
  onCreado: (usuario: Usuario) => void
}

export function UsuarioForm({ puestos, onCreado }: UsuarioFormProps) {
  const [errorServidor, setErrorServidor] = useState<string | null>(null)

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isValid, isSubmitting },
  } = useForm<CrearUsuarioFormValues>({
    resolver: zodResolver(crearUsuarioSchema),
    mode: 'onChange',
    defaultValues: { nombreCompleto: '', email: '', password: '', rol: 'Empleado', puestoId: '' },
  })

  const enviar = async (datos: CrearUsuarioFormValues) => {
    try {
      setErrorServidor(null)
      const creado = await crearUsuarioAsync({
        ...datos,
        puestoId: datos.puestoId === '' ? null : datos.puestoId,
      })
      reset()
      onCreado(creado)
    } catch (error) {
      setErrorServidor(extraerMensajeErrorBackend(error)) // p. ej. 409: correo repetido
    }
  }

  const limpiar = { onChange: () => setErrorServidor(null) }

  return (
    <form
      onSubmit={handleSubmit(enviar)}
      className="h-fit space-y-4 rounded-xl border border-indigo-500/40 bg-slate-800/90 p-5 shadow-lg"
      noValidate
    >
      <h2 className="text-base font-bold text-white">✨ Nuevo usuario</h2>

      <Input
        etiqueta="Nombre completo"
        disabled={isSubmitting}
        error={errors.nombreCompleto?.message}
        {...register('nombreCompleto', limpiar)}
      />
      <Input
        etiqueta="Correo electrónico"
        type="email"
        autoComplete="off"
        disabled={isSubmitting}
        error={errors.email?.message}
        {...register('email', limpiar)}
      />
      <Input
        etiqueta="Contraseña inicial"
        type="password"
        autoComplete="new-password"
        ayuda="Mínimo 8 caracteres, con letra y número."
        disabled={isSubmitting}
        error={errors.password?.message}
        {...register('password', limpiar)}
      />
      <Select etiqueta="Rol" disabled={isSubmitting} error={errors.rol?.message} {...register('rol', limpiar)}>
        <option value="Empleado">Empleado: carga y liquida sus gastos</option>
        <option value="Auditor">Auditor: revisa los lotes de la empresa</option>
        <option value="Admin">Admin: administra todo</option>
      </Select>
      <Select
        etiqueta="Puesto (define su tope diario)"
        disabled={isSubmitting}
        error={errors.puestoId?.message}
        {...register('puestoId', limpiar)}
      >
        <option value="">Sin puesto</option>
        {puestos.map((p) => (
          <option key={p.id} value={p.id}>
            {p.nombre}
          </option>
        ))}
      </Select>

      {errorServidor && <Alert titulo="Respuesta del servidor:">{errorServidor}</Alert>}

      <Button type="submit" className="w-full" disabled={!isValid} cargando={isSubmitting}>
        Crear usuario
      </Button>
    </form>
  )
}
