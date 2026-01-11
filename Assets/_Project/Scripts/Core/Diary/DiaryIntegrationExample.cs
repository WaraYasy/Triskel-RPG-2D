using UnityEngine;
using System.Collections.Generic;

namespace Triskel.Core
{
    /// <summary>
    /// EJEMPLO de integración del sistema de diario con un controlador de nivel.
    ///
    /// Este script es solo una REFERENCIA. Adapta el código a tu controlador de nivel existente.
    ///
    /// USO:
    /// 1. Copia los métodos relevantes a tu LevelController/GameManager.
    /// 2. Llama a NotifyDiaryOnLevelComplete() cuando el nivel termine.
    /// 3. Asegúrate de rastrear las decisiones del jugador durante el nivel.
    /// </summary>
    public class DiaryIntegrationExample : MonoBehaviour
    {
        [Header("Nivel Actual")]
        [SerializeField] private int currentLevelIndex = 0;

        // Variables para rastrear decisiones del jugador
        private List<string> levelConditions = new List<string>();

        private void Start()
        {
            // Inicializar condiciones del nivel
            levelConditions.Clear();
        }

        #region Ejemplo: Registro de Decisiones

        /// <summary>
        /// Ejemplo: El jugador salvó a un NPC.
        /// Registra la condición para el diario.
        /// </summary>
        public void OnPlayerSavedNPC()
        {
            Debug.Log("[DiaryExample] Jugador salvó NPC");
            levelConditions.Add("saved_npc");
        }

        /// <summary>
        /// Ejemplo: El jugador ignoró a un NPC.
        /// Registra la condición para el diario.
        /// </summary>
        public void OnPlayerIgnoredNPC()
        {
            Debug.Log("[DiaryExample] Jugador ignoró NPC");
            levelConditions.Add("ignored_npc");
        }

        /// <summary>
        /// Ejemplo: El jugador encontró un secreto.
        /// Registra la condición para el diario.
        /// </summary>
        public void OnPlayerFoundSecret()
        {
            Debug.Log("[DiaryExample] Jugador encontró secreto");
            levelConditions.Add("found_secret");
        }

        /// <summary>
        /// Ejemplo: El jugador derrotó al boss.
        /// Registra la condición para el diario.
        /// </summary>
        public void OnPlayerDefeatedBoss()
        {
            Debug.Log("[DiaryExample] Jugador derrotó boss");
            levelConditions.Add("defeated_boss");
        }

        /// <summary>
        /// Ejemplo: El jugador huyó del combate.
        /// Registra la condición para el diario.
        /// </summary>
        public void OnPlayerFledCombat()
        {
            Debug.Log("[DiaryExample] Jugador huyó del combate");
            levelConditions.Add("fled_combat");
        }

        #endregion

        #region Ejemplo: Notificación al Diario

        /// <summary>
        /// Llama a este método cuando el nivel se complete.
        /// Notifica al DiaryManager con todas las condiciones cumplidas.
        /// </summary>
        public void OnLevelCompleted()
        {
            Debug.Log($"[DiaryExample] Nivel {currentLevelIndex} completado");

            // Notificar al diario
            NotifyDiaryOnLevelComplete();

            // Avanzar al siguiente nivel
            currentLevelIndex++;
            levelConditions.Clear();
        }

        /// <summary>
        /// Notifica al DiaryManager que el nivel se completó con las condiciones actuales.
        /// </summary>
        private void NotifyDiaryOnLevelComplete()
        {
            if (DiaryManager.Instance == null)
            {
                Debug.LogError("[DiaryExample] DiaryManager no está disponible.");
                return;
            }

            // Verificar que hay condiciones
            if (levelConditions.Count == 0)
            {
                Debug.LogWarning($"[DiaryExample] Nivel {currentLevelIndex} completado sin condiciones registradas. Usando condición por defecto.");
                levelConditions.Add("level_completed");
            }

            // Notificar al diario
            DiaryManager.Instance.OnLevelCompleted(currentLevelIndex, levelConditions.ToArray());

            Debug.Log($"[DiaryExample] Notificado al diario: Nivel {currentLevelIndex}, Condiciones: {string.Join(", ", levelConditions)}");
        }

        #endregion

        #region Ejemplo: Casos de Uso Completos

        /// <summary>
        /// EJEMPLO COMPLETO: Nivel con decisión binaria (salvar o ignorar NPC).
        /// </summary>
        public void ExampleScenario_SaveOrIgnore()
        {
            Debug.Log("=== EJEMPLO: Decisión Salvar/Ignorar NPC ===");

            // Simular decisión del jugador
            bool playerDecidedToSave = Random.value > 0.5f;

            if (playerDecidedToSave)
            {
                OnPlayerSavedNPC();
            }
            else
            {
                OnPlayerIgnoredNPC();
            }

            // Al completar el nivel
            OnLevelCompleted();
        }

        /// <summary>
        /// EJEMPLO COMPLETO: Nivel con múltiples decisiones posibles.
        /// </summary>
        public void ExampleScenario_MultiplePaths()
        {
            Debug.Log("=== EJEMPLO: Nivel con Múltiples Decisiones ===");

            // El jugador puede tomar varias decisiones en el mismo nivel
            OnPlayerFoundSecret();
            OnPlayerSavedNPC();
            OnPlayerDefeatedBoss();

            // Al completar el nivel, el diario elegirá la primera coincidencia
            OnLevelCompleted();
        }

        /// <summary>
        /// EJEMPLO COMPLETO: Nivel sin decisiones específicas (solo completado).
        /// </summary>
        public void ExampleScenario_SimpleCompletion()
        {
            Debug.Log("=== EJEMPLO: Nivel Simple (Solo Completado) ===");

            // No se registran condiciones específicas, solo se completa
            // La condición por defecto será "level_completed"

            OnLevelCompleted();
        }

        #endregion

        #region Debug Methods

        [ContextMenu("Debug: Simular Nivel - Salvar NPC")]
        public void DebugSimulateSaveNPC()
        {
            OnPlayerSavedNPC();
            OnLevelCompleted();
        }

        [ContextMenu("Debug: Simular Nivel - Ignorar NPC")]
        public void DebugSimulateIgnoreNPC()
        {
            OnPlayerIgnoredNPC();
            OnLevelCompleted();
        }

        [ContextMenu("Debug: Simular Nivel - Múltiples Decisiones")]
        public void DebugSimulateMultipleDecisions()
        {
            OnPlayerFoundSecret();
            OnPlayerSavedNPC();
            OnPlayerDefeatedBoss();
            OnLevelCompleted();
        }

        [ContextMenu("Debug: Simular Nivel - Solo Completado")]
        public void DebugSimulateSimpleCompletion()
        {
            OnLevelCompleted();
        }

        [ContextMenu("Debug: Resetear Nivel Actual")]
        public void DebugResetLevel()
        {
            currentLevelIndex = 0;
            levelConditions.Clear();
            Debug.Log("[DiaryExample] Nivel reseteado a 0");
        }

        #endregion
    }
}
