import type { ReactNode } from 'react'

type TipoAlerta = 'error' | 'advertencia' | 'exito' | 'info'

const estilosAlerta: Record<TipoAlerta, { caja: string; icono: string }> = {
  error: { caja: 'bg-red-950/60 border-red-500/40 text-red-300', icono: '🛡️' },
  advertencia: { caja: 'bg-amber-950/50 border-amber-500/40 text-amber-300', icono: '⚠️' },
  exito: { caja: 'bg-emerald-950/50 border-emerald-500/40 text-emerald-300', icono: '✅' },
  info: { caja: 'bg-sky-950/50 border-sky-500/40 text-sky-300', icono: 'ℹ️' },
}

export function Alert({
  tipo = 'error',
  titulo,
  children,
}: {
  tipo?: TipoAlerta
  titulo?: string
  children: ReactNode
}) {
  const { caja, icono } = estilosAlerta[tipo]
  return (
    <div role={tipo === 'error' ? 'alert' : 'status'} className={`flex items-start gap-2.5 rounded-lg border p-3 text-xs ${caja}`}>
      <span className="text-base leading-none">{icono}</span>
      <div>
        {titulo && <strong className="mr-1">{titulo}</strong>}
        {children}
      </div>
    </div>
  )
}

export function Cargando({ mensaje = 'Cargando…' }: { mensaje?: string }) {
  return (
    <div className="rounded-xl border border-slate-800 bg-slate-800/50 p-12 text-center">
      <p className="animate-pulse text-lg font-semibold text-indigo-400">⏳ {mensaje}</p>
    </div>
  )
}

export function EstadoVacio({
  icono = '📭',
  titulo,
  children,
}: {
  icono?: string
  titulo: string
  children?: ReactNode
}) {
  return (
    <div className="rounded-xl border border-dashed border-slate-700 bg-slate-800/30 p-12 text-center">
      <div className="mb-2 text-4xl">{icono}</div>
      <p className="font-semibold text-white">{titulo}</p>
      {children && <div className="mt-1 text-sm text-slate-400">{children}</div>}
    </div>
  )
}

export function Badge({ children, className = '' }: { children: ReactNode; className?: string }) {
  return (
    <span className={`inline-flex items-center rounded-md border px-2.5 py-1 text-xs font-semibold ${className}`}>
      {children}
    </span>
  )
}
