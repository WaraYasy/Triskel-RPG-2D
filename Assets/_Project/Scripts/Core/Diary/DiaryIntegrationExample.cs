using UnityEngine;

namespace Triskel.Core
{
    /// <summary>
    /// EJEMPLO de integración del sistema de diario.
    ///
    /// USO SIMPLE:
    /// Cuando el jugador complete un nivel con una decisión:
    /// DiaryManager.Instance.UnlockEntry(nivel, decision);
    /// </summary>
    public class DiaryIntegrationExample : MonoBehaviour
    {
        [SerializeField] private int currentLevel = 0;

        /// <summary>
        /// Ejemplo: Cuando el jugador toma una decisión buena.
        /// </summary>
        public void OnGoodDecision()
        {
            Debug.Log($"[Ejemplo] Decisión buena en nivel {currentLevel}");
            DiaryManager.Instance.UnlockEntry(currentLevel, "bueno");
            AdvanceLevel();
        }

        /// <summary>
        /// Ejemplo: Cuando el jugador toma una decisión mala.
        /// </summary>
        public void OnBadDecision()
        {
            Debug.Log($"[Ejemplo] Decisión mala en nivel {currentLevel}");
            DiaryManager.Instance.UnlockEntry(currentLevel, "malo");
            AdvanceLevel();
        }

        /// <summary>
        /// Ejemplo: Decisión neutral o alternativa.
        /// </summary>
        public void OnNeutralDecision()
        {
            Debug.Log($"[Ejemplo] Decisión neutral en nivel {currentLevel}");
            DiaryManager.Instance.UnlockEntry(currentLevel, "neutral");
            AdvanceLevel();
        }

        private void AdvanceLevel()
        {
            currentLevel++;
            Debug.Log($"[Ejemplo] Avanzando a nivel {currentLevel}");
        }

        #region Context Menu Debug

        [ContextMenu("Simular: Decisión Buena")]
        private void TestGoodDecision()
        {
            OnGoodDecision();
        }

        [ContextMenu("Simular: Decisión Mala")]
        private void TestBadDecision()
        {
            OnBadDecision();
        }

        [ContextMenu("Resetear Nivel")]
        private void ResetLevel()
        {
            currentLevel = 0;
            Debug.Log("[Ejemplo] Nivel reseteado");
        }

        #endregion
    }
}
