import { useState } from 'react'
import { Alert, Badge, Cargando, EstadoVacio, PageHeader } from '../../../core/ui'
import { useCargaDatos } from '../../../core/hooks/useCargaDatos'
import { obtenerPuestosAsync, obtenerUsuariosAsync } from '../api/usuariosApi'
import type { Puesto, Usuario } from '../types/usuarios.types'
import { PuestoForm } from './PuestoForm'
import { UsuarioForm } from './UsuarioForm'

const colorRol: Record<string, string> = {
  Admin: 'border-indigo-500/30 bg-indigo-500/10 text-indigo-300',
  Empleado: 'border-emerald-500/30 bg-emerald-500/10 text-emerald-300',
  Auditor: 'border-amber-500/30 bg-amber-500/10 text-amber-300',
}

type Pestana = 'usuarios' | 'puestos'

export function AdministracionView() {
  const [pestana, setPestana] = useState<Pestana>('usuarios')
  const puestos = useCargaDatos<Puesto[]>(obtenerPuestosAsync, [])
  const usuarios = useCargaDatos<Usuario[]>(obtenerUsuariosAsync, [])

  const clasePestana = (activa: boolean) =>
    `cursor-pointer rounded-lg px-4 py-2 text-sm font-medium transition-colors ${
      activa ? 'bg-indigo-600 text-white' : 'bg-slate-800 text-slate-300 hover:bg-slate-700'
    }`

  return (
    <div>
      <PageHeader
        titulo="Equipo y puestos"
        descripcion="Da de alta a los colaboradores de tu empresa y define los puestos a los que se aplican los topes de gasto."
      />

      <div className="mb-6 flex gap-2" role="tablist">
        <button
          type="button"
          role="tab"
          aria-selected={pestana === 'usuarios'}
          className={clasePestana(pestana === 'usuarios')}
          onClick={() => setPestana('usuarios')}
        >
          👥 Usuarios ({usuarios.datos.length})
        </button>
        <button
          type="button"
          role="tab"
          aria-selected={pestana === 'puestos'}
          className={clasePestana(pestana === 'puestos')}
          onClick={() => setPestana('puestos')}
        >
          🏷️ Puestos ({puestos.datos.length})
        </button>
      </div>

      {pestana === 'puestos' && (
        <div className="grid gap-6 lg:grid-cols-[1fr_20rem]">
          <section>
            {puestos.cargando && <Cargando mensaje="Consultando puestos…" />}
            {puestos.error && <Alert>{puestos.error}</Alert>}
            {!puestos.cargando && !puestos.error && puestos.datos.length === 0 && (
              <EstadoVacio icono="🏷️" titulo="Aún no hay puestos">
                Crea el primero (por ejemplo, «Vendedor» o «Gerente») para poder asignarle un tope diario.
              </EstadoVacio>
            )}
            {puestos.datos.length > 0 && (
              <ul className="divide-y divide-slate-800 overflow-hidden rounded-xl border border-slate-700 bg-slate-800/60">
                {puestos.datos.map((p) => (
                  <li key={p.id} className="px-4 py-3 text-sm font-medium text-white">
                    {p.nombre}
                  </li>
                ))}
              </ul>
            )}
          </section>
          <PuestoForm onCreado={(nuevo) => puestos.setDatos((actuales) => [...actuales, nuevo])} />
        </div>
      )}

      {pestana === 'usuarios' && (
        <div className="grid gap-6 lg:grid-cols-[1fr_22rem]">
          <section>
            {usuarios.cargando && <Cargando mensaje="Consultando usuarios…" />}
            {usuarios.error && <Alert>{usuarios.error}</Alert>}
            {usuarios.datos.length > 0 && (
              <div className="overflow-x-auto rounded-xl border border-slate-700 bg-slate-800/60">
                <table className="w-full min-w-[32rem] text-left text-sm">
                  <thead className="border-b border-slate-700 text-xs uppercase tracking-wide text-slate-400">
                    <tr>
                      <th className="px-4 py-3">Nombre</th>
                      <th className="px-4 py-3">Correo</th>
                      <th className="px-4 py-3">Rol</th>
                      <th className="px-4 py-3">Puesto</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-800">
                    {usuarios.datos.map((u) => (
                      <tr key={u.id}>
                        <td className="px-4 py-3 font-medium text-white">{u.nombreCompleto}</td>
                        <td className="px-4 py-3 text-slate-300">{u.email}</td>
                        <td className="px-4 py-3">
                          <Badge className={colorRol[u.rol] ?? ''}>{u.rol}</Badge>
                        </td>
                        <td className="px-4 py-3 text-slate-400">{u.puesto ?? '—'}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </section>
          <UsuarioForm
            puestos={puestos.datos}
            onCreado={(nuevo) => usuarios.setDatos((actuales) => [...actuales, nuevo])}
          />
        </div>
      )}
    </div>
  )
}
