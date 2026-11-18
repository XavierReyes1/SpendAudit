// @vitest-environment jsdom
import { cleanup, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { AxiosError } from 'axios'
import type { AxiosResponse } from 'axios'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { guardarSesion, leerSesion, limpiarSesion } from '../core/auth/sesionStorage'
import type { Sesion } from '../core/auth/sesionStorage'
import { AuthProvider } from '../features/auth/context/AuthProvider'
import { LoginView } from '../features/auth/components/LoginView'
import { loginAsync } from '../features/auth/api/authApi'
import { RedireccionInicial, RutaPorRol, RutaProtegida, RutaPublica } from '../router/guards'

vi.mock('../features/auth/api/authApi', () => ({
  loginAsync: vi.fn(),
  registrarEmpresaAsync: vi.fn(),
}))

const sesion = (rol: 'Admin' | 'Empleado' | 'Auditor'): Sesion => ({
  token: 'jwt-falso',
  expiraEn: new Date(Date.now() + 3_600_000).toISOString(),
  usuario: {
    id: 'u1',
    nombreCompleto: 'Carlos Vendedor',
    email: 'carlos@ferreteria.hn',
    rol,
    empresaId: 'e1',
    puesto: 'Vendedor',
  },
})

/** Mismo árbol de rutas que AppRouter, pero con MemoryRouter y vistas de relleno. */
function Arbol({ ruta }: { ruta: string }) {
  return (
    <AuthProvider>
      <MemoryRouter initialEntries={[ruta]}>
        <Routes>
          <Route element={<RutaPublica />}>
            <Route path="/login" element={<LoginView />} />
          </Route>
          <Route element={<RutaProtegida />}>
            <Route index element={<RedireccionInicial />} />
            <Route element={<RutaPorRol roles={['Empleado', 'Admin']} />}>
              <Route path="/album" element={<p>VISTA ALBUM</p>} />
            </Route>
            <Route path="/lotes" element={<p>VISTA LOTES</p>} />
            <Route element={<RutaPorRol roles={['Admin']} />}>
              <Route path="/administracion" element={<p>VISTA ADMIN</p>} />
            </Route>
          </Route>
        </Routes>
      </MemoryRouter>
    </AuthProvider>
  )
}

beforeEach(() => limpiarSesion())
afterEach(() => {
  cleanup()
  vi.clearAllMocks()
})

describe('guardias de ruta', () => {
  it('sin JWT, una ruta protegida envía al login', () => {
    render(<Arbol ruta="/album" />)
    expect(screen.getByRole('heading', { name: 'Iniciar sesión' })).toBeInTheDocument()
    expect(screen.queryByText('VISTA ALBUM')).not.toBeInTheDocument()
  })

  it('con sesión, el Empleado aterriza en su álbum', () => {
    guardarSesion(sesion('Empleado'))
    render(<Arbol ruta="/" />)
    expect(screen.getByText('VISTA ALBUM')).toBeInTheDocument()
  })

  it('con sesión, el Auditor aterriza en los lotes', () => {
    guardarSesion(sesion('Auditor'))
    render(<Arbol ruta="/" />)
    expect(screen.getByText('VISTA LOTES')).toBeInTheDocument()
  })

  it('un Empleado no entra a la administración', () => {
    guardarSesion(sesion('Empleado'))
    render(<Arbol ruta="/administracion" />)
    expect(screen.getByText(/No tienes permisos/)).toBeInTheDocument()
    expect(screen.queryByText('VISTA ADMIN')).not.toBeInTheDocument()
  })

  it('el Admin sí entra a la administración', () => {
    guardarSesion(sesion('Admin'))
    render(<Arbol ruta="/administracion" />)
    expect(screen.getByText('VISTA ADMIN')).toBeInTheDocument()
  })

  it('con sesión activa, el login redirige adentro', () => {
    guardarSesion(sesion('Empleado'))
    render(<Arbol ruta="/login" />)
    expect(screen.getByText('VISTA ALBUM')).toBeInTheDocument()
  })

  it('una sesión con el JWT vencido se descarta', () => {
    guardarSesion({ ...sesion('Admin'), expiraEn: new Date(Date.now() - 1000).toISOString() })
    expect(leerSesion()).toBeNull()
    render(<Arbol ruta="/administracion" />)
    expect(screen.getByRole('heading', { name: 'Iniciar sesión' })).toBeInTheDocument()
  })
})

describe('LoginView', () => {
  it('deshabilita el botón hasta que Zod valida el formulario (mode: onChange)', async () => {
    const usuario = userEvent.setup()
    render(<Arbol ruta="/login" />)

    const boton = screen.getByRole('button', { name: 'Ingresar' })
    expect(boton).toBeDisabled()

    await usuario.type(screen.getByLabelText('Correo electrónico'), 'no-es-correo')
    await usuario.type(screen.getByLabelText('Contraseña'), 'abc')
    expect(await screen.findByText('El correo no tiene un formato válido.')).toBeInTheDocument()
    expect(boton).toBeDisabled()

    await usuario.clear(screen.getByLabelText('Correo electrónico'))
    await usuario.type(screen.getByLabelText('Correo electrónico'), 'carlos@ferreteria.hn')
    await waitFor(() => expect(boton).toBeEnabled())
  })

  it('inicia sesión: guarda el JWT y entra a la aplicación', async () => {
    const usuario = userEvent.setup()
    const s = sesion('Empleado')
    vi.mocked(loginAsync).mockResolvedValue({ token: s.token, expiraEn: s.expiraEn, usuario: s.usuario })

    render(<Arbol ruta="/login" />)
    await usuario.type(screen.getByLabelText('Correo electrónico'), 'carlos@ferreteria.hn')
    await usuario.type(screen.getByLabelText('Contraseña'), 'Clave1234')
    await usuario.click(await screen.findByRole('button', { name: 'Ingresar' }))

    expect(await screen.findByText('VISTA ALBUM')).toBeInTheDocument()
    expect(loginAsync).toHaveBeenCalledWith({ email: 'carlos@ferreteria.hn', password: 'Clave1234' })
    expect(leerSesion()?.token).toBe('jwt-falso')
  })

  it('muestra el error 401 del backend y no guarda sesión', async () => {
    const usuario = userEvent.setup()
    vi.mocked(loginAsync).mockRejectedValue(
      new AxiosError('x', 'ERR_BAD_REQUEST', undefined, undefined, {
        status: 401,
        statusText: '',
        headers: {},
        config: {} as AxiosResponse['config'],
        data: { mensaje: 'Credenciales inválidas.' },
      }),
    )

    render(<Arbol ruta="/login" />)
    await usuario.type(screen.getByLabelText('Correo electrónico'), 'carlos@ferreteria.hn')
    await usuario.type(screen.getByLabelText('Contraseña'), 'incorrecta')
    await usuario.click(await screen.findByRole('button', { name: 'Ingresar' }))

    expect(await screen.findByText(/\[HTTP 401\] Credenciales inválidas\./)).toBeInTheDocument()
    expect(leerSesion()).toBeNull()
  })
})