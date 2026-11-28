const formatoMoneda = new Intl.NumberFormat('es-HN', {
  style: 'currency',
  currency: 'HNL',
  currencyDisplay: 'narrowSymbol',
})

export const formatearMonto = (valor: number | null | undefined): string =>
  valor === null || valor === undefined ? '—' : formatoMoneda.format(valor)

/** Fecha sin hora ("2026-11-05") → "05/11/2026". Se parte el texto para evitar el desfase de zona horaria. */
export const formatearFecha = (iso: string | null | undefined): string => {
  if (!iso) return '—'
  const [anio, mes, dia] = iso.slice(0, 10).split('-')
  return anio && mes && dia ? `${dia}/${mes}/${anio}` : iso
}

export const formatearFechaHora = (iso: string | null | undefined): string => {
  if (!iso) return '—'
  const fecha = new Date(iso)
  if (Number.isNaN(fecha.getTime())) return iso
  return fecha.toLocaleString('es-HN', {
    timeZone: 'America/Tegucigalpa',
    dateStyle: 'short',
    timeStyle: 'short',
  })
}
export const hoyHonduras = (): string =>
  new Date(Date.now() - 6 * 60 * 60 * 1000).toISOString().slice(0, 10)
