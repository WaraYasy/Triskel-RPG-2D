using UnityEngine;

namespace Triskel.Core
{
    /// <summary>
    /// Define un item coleccionable (reliquia) del juego.
    /// ScriptableObject = Asset reutilizable que se crea en el Project.
    ///
    /// USO: Click derecho en Project → Create → Triskel/Collectible Item
    /// </summary>
    [CreateAssetMenu(fileName = "NewCollectible", menuName = "Triskel/Collectible Item")]
    public class CollectibleItem : ScriptableObject
    {
        [Header("Identificación")]
        [Tooltip("ID único del item (debe coincidir con la API: 'lirio', 'hacha', 'manto')")]
        public string itemID = "lirio";

        [Tooltip("Nombre visible del item")]
        public string displayName = "Lirio";

        [Header("Visualización")]
        [Tooltip("Icono que aparece en el inventario (UI)")]
        public Sprite icon;

        [Tooltip("Color del slot: 'blue', 'green', 'red', 'yellow', 'grey'")]
        public string slotColor = "grey";

        [Header("Descripción")]
        [Tooltip("Descripción del item (para tooltips, diálogos, etc.)")]
        [TextArea(2, 4)]
        public string description = "Una reliquia misteriosa...";

        /// <summary>
        /// Validación automática al editar en el Inspector.
        /// Genera itemID basado en el nombre del asset si está vacío.
        /// </summary>
        private void OnValidate()
        {
            // Auto-generar itemID desde el nombre del archivo
            if (string.IsNullOrEmpty(itemID))
            {
                itemID = name.ToLower().Replace(" ", "_");
            }
        }
    }
}
