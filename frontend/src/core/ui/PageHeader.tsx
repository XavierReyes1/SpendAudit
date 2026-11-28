import type { ReactNode } from 'react'

export function PageHeader({
  titulo,
  descripcion,
  acciones,
}: {
  titulo: string
  descripcion?: ReactNode
  acciones?: ReactNode
}) {
  return (
    <div className="mb-6 flex flex-col gap-4 border-b border-slate-800 pb-6 sm:flex-row sm:items-center sm:justify-between">
      <div>
        <h1 className="text-2xl font-bold text-white md:text-3xl">{titulo}</h1>
        {descripcion && <p className="mt-1 text-sm text-slate-400">{descripcion}</p>}
      </div>
      {acciones && <div className="flex flex-wrap items-center gap-2 self-start sm:self-auto">{acciones}</div>}
    </div>
  )
}
