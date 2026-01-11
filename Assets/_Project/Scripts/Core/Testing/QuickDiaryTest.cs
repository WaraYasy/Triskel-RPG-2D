using UnityEngine;
using Triskel.Core;

namespace Triskel.Testing
{
    /// <summary>
    /// Script de prueba rápida para el sistema de diario.
    ///
    /// INSTRUCCIONES:
    /// 1. Añade este script a cualquier GameObject en la escena
    /// 2. Entra en Play Mode
    /// 3. Usa las teclas numéricas para probar:
    ///    - 1: Desbloquear entrada de nivel 0 con condición "saved_child"
    ///    - 2: Desbloquear entrada de nivel 0 con condición "ignored_child"
    ///    - 3: Desbloquear entrada de nivel 1 con condición "defeated_boss"
    ///    - 4: Desbloquear entrada de nivel 1 con condición "fled_boss"
    ///    - J: Abrir/Cerrar diario (configurable en DiaryUI)
    ///    - 0: Limpiar todas las entradas
    ///
    /// CONSOLA:
    /// - Verás mensajes indicando qué se desbloqueó
    /// - Útil para debugging
    /// </summary>
    public class QuickDiaryTest : MonoBehaviour
    {
        [Header("Configuración de Prueba")]
        [Tooltip("Mostrar mensajes de ayuda al iniciar")]
        [SerializeField] private bool showHelpOnStart = true;

        private void Start()
        {
            if (showHelpOnStart)
            {
                Debug.Log("=== QUICK DIARY TEST - CONTROLES ===");
                Debug.Log("1: Desbloquear 'saved_child' (Nivel 0)");
                Debug.Log("2: Desbloquear 'ignored_child' (Nivel 0)");
                Debug.Log("3: Desbloquear 'defeated_boss' (Nivel 1)");
                Debug.Log("4: Desbloquear 'fled_boss' (Nivel 1)");
                Debug.Log("J: Abrir/Cerrar diario");
                Debug.Log("0: Limpiar todas las entradas");
                Debug.Log("=====================================");
            }
        }

        private void Update()
        {
            // Verificar que DiaryManager existe
            if (DiaryManager.Instance == null)
            {
                return;
            }

            // Tecla 1: Nivel 0 - Salvar niño
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                UnlockEntry(0, "saved_child", "Nivel 0: Salvaste al niño");
            }

            // Tecla 2: Nivel 0 - Ignorar niño
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                UnlockEntry(0, "ignored_child", "Nivel 0: Ignoraste al niño");
            }

            // Tecla 3: Nivel 1 - Derrotar boss
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                UnlockEntry(1, "defeated_boss", "Nivel 1: Derrotaste al boss");
            }

            // Tecla 4: Nivel 1 - Huir del boss
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                UnlockEntry(1, "fled_boss", "Nivel 1: Huiste del boss");
            }

            // Tecla 0: Limpiar todo
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                ClearAllEntries();
            }

            // Tecla H: Mostrar ayuda
            if (Input.GetKeyDown(KeyCode.H))
            {
                ShowHelp();
            }
        }

        /// <summary>
        /// Desbloquea una entrada del diario.
        /// </summary>
        private void UnlockEntry(int levelIndex, string conditionID, string description)
        {
            Debug.Log($"[QuickTest] 🔓 {description}");

            DiaryManager.Instance.OnLevelCompleted(levelIndex, new string[] { conditionID });

            Debug.Log($"[QuickTest] ✅ Entrada desbloqueada. Presiona J para ver el diario.");
        }

        /// <summary>
        /// Limpia todas las entradas del diario.
        /// </summary>
        private void ClearAllEntries()
        {
            Debug.Log("[QuickTest] 🗑️ Limpiando todas las entradas del diario...");

            DiaryManager.Instance.ClearAllEntries();

            Debug.Log("[QuickTest] ✅ Diario limpiado. Todas las entradas eliminadas.");
        }

        /// <summary>
        /// Muestra la ayuda de controles.
        /// </summary>
        private void ShowHelp()
        {
            Debug.Log("=== QUICK DIARY TEST - CONTROLES ===");
            Debug.Log("1: Desbloquear 'saved_child' (Nivel 0)");
            Debug.Log("2: Desbloquear 'ignored_child' (Nivel 0)");
            Debug.Log("3: Desbloquear 'defeated_boss' (Nivel 1)");
            Debug.Log("4: Desbloquear 'fled_boss' (Nivel 1)");
            Debug.Log("J: Abrir/Cerrar diario");
            Debug.Log("0: Limpiar todas las entradas");
            Debug.Log("H: Mostrar esta ayuda");
            Debug.Log("=====================================");
        }

        #region Context Menus (para usar fuera de Play Mode)

        [ContextMenu("Desbloquear: Saved Child")]
        private void TestSavedChild()
        {
            UnlockEntry(0, "saved_child", "Test: Saved Child");
        }

        [ContextMenu("Desbloquear: Ignored Child")]
        private void TestIgnoredChild()
        {
            UnlockEntry(0, "ignored_child", "Test: Ignored Child");
        }

        [ContextMenu("Desbloquear: Defeated Boss")]
        private void TestDefeatedBoss()
        {
            UnlockEntry(1, "defeated_boss", "Test: Defeated Boss");
        }

        [ContextMenu("Desbloquear: Fled Boss")]
        private void TestFledBoss()
        {
            UnlockEntry(1, "fled_boss", "Test: Fled Boss");
        }

        [ContextMenu("Limpiar Todo")]
        private void TestClearAll()
        {
            ClearAllEntries();
        }

        [ContextMenu("Mostrar Entradas Desbloqueadas")]
        private void TestShowUnlocked()
        {
            if (DiaryManager.Instance != null)
            {
                DiaryManager.Instance.DebugShowUnlockedEntries();
            }
        }

        #endregion
    }
}
