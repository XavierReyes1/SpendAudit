import type { ReactNode } from 'react'

/** Tarjeta centrada para las pantallas públicas (login y registro). */
export function AuthShell({
  titulo,
  subtitulo,
  children,
  pie,
}: {
  titulo: string
  subtitulo: string
  children: ReactNode
  pie: ReactNode
}) {
  return (
    <div className="flex min-h-screen items-center justify-center bg-slate-900 p-4 text-slate-100">
      <div className="w-full max-w-md">
        <div className="mb-6 flex items-center justify-center gap-3">
          <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-indigo-600 text-lg font-bold text-white shadow">
            SA
          </div>
          <span className="text-2xl font-bold tracking-tight text-white">SpendAudit</span>
        </div>

        <div className="rounded-xl border border-slate-700 bg-slate-800/90 p-6 shadow-lg">
          <h1 className="text-xl font-bold text-white">{titulo}</h1>
          <p className="mb-5 mt-1 text-sm text-slate-400">{subtitulo}</p>
          {children}
        </div>

        <p className="mt-5 text-center text-sm text-slate-400">{pie}</p>
      </div>
    </div>
  )
}