import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Link, useNavigate } from 'react-router-dom'
import { extraerMensajeErrorBackend } from '../../../core/api/extraerMensajeError'
import { Alert, Button, Input } from '../../../core/ui'
import { registrarEmpresaAsync } from '../api/authApi'
import { useAuth } from '../hooks/useAuth'
import { registroEmpresaSchema } from '../schemas/auth.schema'
import type { RegistroEmpresaFormValues } from '../schemas/auth.schema'
import { AuthShell } from './AuthShell'

export function RegistroEmpresaView() {
  const { iniciarSesion } = useAuth()
  const navigate = useNavigate()
  const [errorServidor, setErrorServidor] = useState<string | null>(null)

  const {
    register,
    handleSubmit,
    formState: { errors, isValid, isSubmitting },
  } = useForm<RegistroEmpresaFormValues>({
    resolver: zodResolver(registroEmpresaSchema),
    mode: 'onChange',
    defaultValues: {
      nombreRazonSocial: '',
      rtn: '',
      adminNombre: '',
      adminEmail: '',
      adminPassword: '',
      confirmarPassword: '',
    },
  })

  const enviar = async (valores: RegistroEmpresaFormValues) => {
    try {
      setErrorServidor(null)
      // confirmarPassword es solo del cliente: no viaja al backend
      const { confirmarPassword: _omitido, ...dto } = valores
      void _omitido
      const respuesta = await registrarEmpresaAsync(dto)
      iniciarSesion(respuesta) // el backend devuelve el JWT: el admin queda con sesión
      navigate('/', { replace: true })
    } catch (error) {
      // 409: RTN o correo repetido · 400: reglas de FluentValidation
      setErrorServidor(extraerMensajeErrorBackend(error))
    }
  }

  const limpiarError = { onChange: () => setErrorServidor(null) }

  return (
    <AuthShell
      titulo="Registra tu empresa"
      subtitulo="Crea el espacio aislado de tu empresa y tu cuenta de administrador."
      pie={
        <>
          ¿Ya tienes cuenta?{' '}
          <Link to="/login" className="font-medium text-indigo-400 hover:text-indigo-300">
            Inicia sesión
          </Link>
        </>
      }
    >
      <form onSubmit={handleSubmit(enviar)} className="space-y-4" noValidate>
        <Input
          etiqueta="Nombre o razón social de la empresa"
          autoFocus
          disabled={isSubmitting}
          error={errors.nombreRazonSocial?.message}
          {...register('nombreRazonSocial', limpiarError)}
        />
        <Input
          etiqueta="RTN de la empresa"
          inputMode="numeric"
          maxLength={14}
          placeholder="14 dígitos, sin guiones"
          disabled={isSubmitting}
          error={errors.rtn?.message}
          {...register('rtn', limpiarError)}
        />

        <div className="border-t border-slate-700 pt-4">
          <p className="mb-3 text-xs font-semibold uppercase tracking-wide text-slate-400">
            Cuenta del administrador
          </p>
          <div className="space-y-4">
            <Input
              etiqueta="Nombre completo"
              autoComplete="name"
              disabled={isSubmitting}
              error={errors.adminNombre?.message}
              {...register('adminNombre', limpiarError)}
            />
            <Input
              etiqueta="Correo electrónico"
              type="email"
              autoComplete="email"
              disabled={isSubmitting}
              error={errors.adminEmail?.message}
              {...register('adminEmail', limpiarError)}
            />
            <Input
              etiqueta="Contraseña"
              type="password"
              autoComplete="new-password"
              ayuda="Mínimo 8 caracteres, con al menos una letra y un número."
              disabled={isSubmitting}
              error={errors.adminPassword?.message}
              {...register('adminPassword', limpiarError)}
            />
            <Input
              etiqueta="Confirmar contraseña"
              type="password"
              autoComplete="new-password"
              disabled={isSubmitting}
              error={errors.confirmarPassword?.message}
              {...register('confirmarPassword', limpiarError)}
            />
          </div>
        </div>

        {errorServidor && <Alert titulo="Respuesta del servidor:">{errorServidor}</Alert>}

        <Button type="submit" className="w-full" disabled={!isValid} cargando={isSubmitting}>
          {isSubmitting ? 'Creando empresa…' : 'Crear empresa'}
        </Button>
      </form>
    </AuthShell>
  )
}