import { useId } from 'react'
import type { ComponentProps, ReactNode } from 'react'

interface CampoBase {
  etiqueta: string
  error?: string
  ayuda?: ReactNode
}

const claseControl = (error?: string) =>
  `w-full bg-slate-950 border rounded-lg px-4 py-2.5 text-sm text-white placeholder-slate-500 focus:outline-none transition-colors disabled:opacity-60 ${
    error ? 'border-amber-500 focus:border-amber-400' : 'border-slate-700 focus:border-indigo-500'
  }`

function Envoltorio({
  id,
  etiqueta,
  error,
  ayuda,
  children,
}: CampoBase & { id: string; children: ReactNode }) {
  return (
    <div>
      <label htmlFor={id} className="mb-1.5 block text-xs font-medium text-slate-300">
        {etiqueta}
      </label>
      {children}
      {ayuda && !error && <p className="mt-1.5 text-xs text-slate-500">{ayuda}</p>}
      {error && (
        <p role="alert" className="mt-1.5 flex items-center gap-1.5 text-xs font-medium text-amber-400">
          <span>⚡</span> {error}
        </p>
      )}
    </div>
  )
}

export function Input({
  etiqueta,
  error,
  ayuda,
  id,
  ...props
}: CampoBase & ComponentProps<'input'>) {
  const generado = useId()
  const inputId = id ?? generado
  return (
    <Envoltorio id={inputId} etiqueta={etiqueta} error={error} ayuda={ayuda}>
      <input id={inputId} aria-invalid={Boolean(error)} className={claseControl(error)} {...props} />
    </Envoltorio>
  )
}

export function Select({
  etiqueta,
  error,
  ayuda,
  id,
  children,
  ...props
}: CampoBase & ComponentProps<'select'>) {
  const generado = useId()
  const selectId = id ?? generado
  return (
    <Envoltorio id={selectId} etiqueta={etiqueta} error={error} ayuda={ayuda}>
      <select id={selectId} aria-invalid={Boolean(error)} className={claseControl(error)} {...props}>
        {children}
      </select>
    </Envoltorio>
  )
}

export function Textarea({
  etiqueta,
  error,
  ayuda,
  id,
  ...props
}: CampoBase & ComponentProps<'textarea'>) {
  const generado = useId()
  const areaId = id ?? generado
  return (
    <Envoltorio id={areaId} etiqueta={etiqueta} error={error} ayuda={ayuda}>
      <textarea id={areaId} aria-invalid={Boolean(error)} className={claseControl(error)} {...props} />
    </Envoltorio>
  )
}
