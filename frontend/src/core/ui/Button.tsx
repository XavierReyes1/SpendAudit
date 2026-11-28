import type { ComponentProps } from 'react'

type Variante = 'primario' | 'secundario' | 'peligro' | 'fantasma'

interface ButtonProps extends ComponentProps<'button'> {
  variante?: Variante
  cargando?: boolean
}

const estilos: Record<Variante, string> = {
  primario:
    'bg-indigo-600 hover:bg-indigo-500 text-white disabled:bg-slate-800 disabled:text-slate-500 disabled:border disabled:border-slate-700',
  secundario: 'bg-slate-700 hover:bg-slate-600 text-slate-100 disabled:opacity-50',
  peligro: 'bg-red-600 hover:bg-red-500 text-white disabled:opacity-50',
  fantasma: 'bg-transparent hover:bg-slate-800 text-slate-300 border border-slate-700 disabled:opacity-50',
}

export function Button({
  variante = 'primario',
  cargando = false,
  disabled,
  children,
  className = '',
  type = 'button',
  ...props
}: ButtonProps) {
  return (
    <button
      type={type}
      disabled={disabled || cargando}
      className={`inline-flex items-center justify-center gap-2 rounded-lg px-4 py-2.5 text-sm font-medium transition-colors cursor-pointer disabled:cursor-not-allowed ${estilos[variante]} ${className}`}
      {...props}
    >
      {cargando && (
        <span className="h-3.5 w-3.5 animate-spin rounded-full border-2 border-current border-t-transparent" />
      )}
      {children}
    </button>
  )
}
