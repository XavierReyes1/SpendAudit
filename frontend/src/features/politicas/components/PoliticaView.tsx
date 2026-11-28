import { useCallback } from 'react'
import { Alert, Cargando, PageHeader } from '../../../core/ui'
import { useCargaDatos } from '../../../core/hooks/useCargaDatos'
import { useAuth } from '../../auth/hooks/useAuth'
import { obtenerPuestosAsync } from '../../usuarios/api/usuariosApi'
import { obtenerPoliticaAsync } from '../api/politicasApi'
import { PoliticaForm } from './PoliticaForm'

export function PoliticaView() {
  const { tieneRol } = useAuth()
  const soloLectura = !tieneRol('Admin')

  // Política y puestos se consultan en paralelo
  const cargarTodo = useCallback(
    async () => {
      const [politica, puestos] = await Promise.all([obtenerPoliticaAsync(), obtenerPuestosAsync()])
      return { politica, puestos }
    },
    [],
  )
  const { datos, cargando, error } = useCargaDatos(cargarTodo, null)

  return (
    <div>
      <PageHeader
        titulo="Política de gastos"
        descripcion="Reglas que el sistema aplica al auditar cada comprobante de tu empresa."
      />

      {cargando && <Cargando mensaje="Consultando la política activa…" />}
      {error && <Alert>{error}</Alert>}

      {datos && (
        <>
          {soloLectura && (
            <div className="mb-5">
              <Alert tipo="info">Eres Auditor: puedes consultar la política, pero solo el Administrador la modifica.</Alert>
            </div>
          )}
          <PoliticaForm politica={datos.politica} puestos={datos.puestos} soloLectura={soloLectura} />
        </>
      )}
    </div>
  )
}
