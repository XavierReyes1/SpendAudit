import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { extraerMensajeErrorBackend } from '../../../core/api/extraerMensajeError'
import { Alert, Button, Input } from '../../../core/ui'
import { loginAsync } from '../api/authApi'
import { useAuth } from '../hooks/useAuth'
import { loginSchema } from '../schemas/auth.schema'
import type { LoginFormValues } from '../schemas/auth.schema'
import { AuthShell } from './AuthShell'

export function LoginView() {
  const { iniciarSesion } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const destino = (location.state as { desde?: string } | null)?.desde ?? '/'

  const [errorServidor, setErrorServidor] = useState<string | null>(null)

  const {
    register,
    handleSubmit,
    formState: { errors, isValid, isSubmitting },
  } = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
    mode: 'onChange',
    defaultValues: { email: '', password: '' },
  })

  const enviar = async (datos: LoginFormValues) => {
    try {
      setErrorServidor(null)
      const respuesta = await loginAsync(datos)
      iniciarSesion(respuesta)
      navigate(destino, { replace: true })
    } catch (error) {
      setErrorServidor(extraerMensajeErrorBackend(error))
    }
  }

  return (
    <AuthShell
      titulo="Iniciar sesión"
      subtitulo="Accede al espacio de tu empresa."
      pie={
        <>
          ¿Tu empresa aún no usa SpendAudit?{' '}
          <Link to="/registro" className="font-medium text-indigo-400 hover:text-indigo-300">
            Registra tu empresa
          </Link>
        </>
      }
    >
      <form onSubmit={handleSubmit(enviar)} className="space-y-4" noValidate>
        <Input
          etiqueta="Correo electrónico"
          type="email"
          autoComplete="email"
          autoFocus
          disabled={isSubmitting}
          error={errors.email?.message}
          {...register('email', { onChange: () => setErrorServidor(null) })}
        />
        <Input
          etiqueta="Contraseña"
          type="password"
          autoComplete="current-password"
          disabled={isSubmitting}
          error={errors.password?.message}
          {...register('password', { onChange: () => setErrorServidor(null) })}
        />

        {errorServidor && <Alert titulo="Respuesta del servidor:">{errorServidor}</Alert>}

        <Button type="submit" className="w-full" disabled={!isValid} cargando={isSubmitting}>
          {isSubmitting ? 'Ingresando…' : 'Ingresar'}
        </Button>
      </form>
    </AuthShell>
  )
}