namespace Triskel.GiantFortress
{
    /// <summary>
    /// Interfaz para objetos que pueden ser golpeados por el Hacha Sagrada.
    /// Permite extensibilidad para futuros cuadrantes y objetos destructibles.
    /// </summary>
    public interface IAxeDestructible
    {
        void OnAxeHit();
    }
}
