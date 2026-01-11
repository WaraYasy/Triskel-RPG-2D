using UnityEngine;

namespace Triskel.Core
{
    /// <summary>
    /// Define una entrada del diario narrativo.
    /// ScriptableObject = Asset reutilizable que se crea en el Project.
    ///
    /// USO: Click derecho en Project → Create → Triskel/Diary Entry
    /// </summary>
    [CreateAssetMenu(fileName = "NewDiaryEntry", menuName = "Triskel/Diary Entry")]
    public class DiaryEntryData : ScriptableObject
    {
        [Header("Identificación")]
        [Tooltip("ID único de la entrada (ej: 'level1_saved_child')")]
        public string entryID = "";

        [Header("Contexto del Nivel")]
        [Tooltip("Índice del nivel que desbloquea esta entrada (0 = Nivel 1)")]
        public int levelIndex = 0;

        [Tooltip("ID de la condición que activa esta entrada (ej: 'saved_child', 'ignored_child')")]
        public string conditionID = "";

        [Header("Contenido")]
        [Tooltip("Título de la entrada que aparece en la lista")]
        public string title = "Nueva Entrada";

        [Tooltip("Texto completo de la entrada (soporta múltiples líneas)")]
        [TextArea(5, 20)]
        public string text = "";

        /// <summary>
        /// Validación automática al editar en el Inspector.
        /// Genera entryID basado en el nombre del asset si está vacío.
        /// </summary>
        private void OnValidate()
        {
            // Auto-generar entryID desde el nombre del archivo
            if (string.IsNullOrEmpty(entryID))
            {
                entryID = name.ToLower().Replace(" ", "_");
            }
        }
    }
}
