namespace OAI_PMH.Models.SGI.Project
{
    /// <summary>
    /// Proyecto presupuesto totales
    /// </summary>
    public class ProyectoPresupuestoTotales : SGI_Base
    {
        /// <summary>
        /// Importe total (suma del importe de todas las anualidades) presupuestado a la Universidad para el desarrollo del proyecto,
        /// asociado a conceptos de gasto que no representan costes indirectos.
        /// </summary>
        public float importeTotalPresupuestoUniversidadSinCosteIndirecto { get; set; }
        /// <summary>
        /// Importe total (suma del importe de todas las anualidades) presupuestado al resto de socios de la Universidad para el desarrollo del proyecto.
        /// </summary>
        public float importeTotalPresupuestoSocios { get; set; }
        /// <summary>
        /// Importe total (suma del importe de todas las anualidades) concedido a la Universidad para el desarrollo del proyecto, 
        /// asociado a conceptos de gasto que no representan costes indirectos.
        /// </summary>
        public float importeTotalConcedidoUniversidadSinCosteIndirecto { get; set; }
        /// <summary>
        /// Importe total (suma del importe de todas las anualidades) concedido al resto de socios de la Universidad para el desarrollo del proyecto.
        /// </summary>
        public float importeTotalConcedidoSocios { get; set; }
        /// <summary>
        /// Importe total (suma del importe de todas las anualidades) presupuestado.
        /// </summary>
        public float importeTotalPresupuesto { get; set; }
        /// <summary>
        /// Importe total (suma del importe de todas las anualidades) concedido.
        /// </summary>
        public float importeTotalConcedido { get; set; }
        /// <summary>
        /// Importe total (suma del importe de todas las anualidades) presupuestado a la Universidad para el desarrollo del proyecto,
        /// asociado a conceptos de gasto que representan costes indirectos.
        /// </summary>
        public float importeTotalPresupuestoUniversidadCostesIndirectos { get; set; }
        /// <summary>
        /// Importe total (suma del importe de todas las anualidades) concedido a la Universidad para el desarrollo del proyecto,
        /// asociado a conceptos de gasto que representan costes indirectos.
        /// </summary>
        public float importeTotalConcedidoUniversidadCostesIndirectos { get; set; }
    }
}
