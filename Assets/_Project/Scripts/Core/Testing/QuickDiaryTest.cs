using UnityEngine;
using Triskel.Core;

namespace Triskel.Testing
{
    /// <summary>
    /// Script de prueba para el sistema de diario.
    /// TECLAS: F1-F6 = Desbloquear entradas, F12 = Limpiar, J = Abrir diario
    /// </summary>
    public class QuickDiaryTest : MonoBehaviour
    {
        [Header("Testing Control")]
        [Tooltip("Desactiva este script si no quieres testing (evita conflictos con Input System)")]
        [SerializeField] private bool enableTesting = false; // ← FALSE por defecto
        
        private void Start()
        {
            if (!enableTesting)
            {
                Debug.Log("[QuickDiaryTest] Testing desactivado. Activa 'Enable Testing' en Inspector si lo necesitas.");
                return;
            }
            
            Debug.Log("=== DIARIO TEST ===");
            Debug.Log("F1: Nivel 0 Bueno | F2: Nivel 0 Malo");
            Debug.Log("F3: Nivel 1 Bueno | F4: Nivel 1 Malo");
            Debug.Log("F5: Nivel 2 Bueno | F6: Nivel 2 Malo");
            Debug.Log("F12: Limpiar | J: Ver diario");
        }

        private void Update()
        {
            if (!enableTesting) return; // ← Salir si testing desactivado
            if (DiaryManager.Instance == null) return;

            if (Input.GetKeyDown(KeyCode.F1))
                DiaryManager.Instance.UnlockEntry(0, "intro");

            if (Input.GetKeyDown(KeyCode.F2))
                DiaryManager.Instance.UnlockEntry(1, "malo");

            if (Input.GetKeyDown(KeyCode.F3))
                DiaryManager.Instance.UnlockEntry(1, "bueno");

            if (Input.GetKeyDown(KeyCode.F4))
                DiaryManager.Instance.UnlockEntry(2, "malo");

            if (Input.GetKeyDown(KeyCode.F5))
                DiaryManager.Instance.UnlockEntry(2, "bueno");

            if (Input.GetKeyDown(KeyCode.F6))
                DiaryManager.Instance.UnlockEntry(3, "malo");
            
            if (Input.GetKeyDown(KeyCode.F7))
                DiaryManager.Instance.UnlockEntry(3, "bueno");
            
            if (Input.GetKeyDown(KeyCode.F8))
                DiaryManager.Instance.UnlockEntry(4, "1");
            
            if (Input.GetKeyDown(KeyCode.F9))
                DiaryManager.Instance.UnlockEntry(4, "2");
            
            if (Input.GetKeyDown(KeyCode.F10))
                DiaryManager.Instance.UnlockEntry(4, "3");
            
            if (Input.GetKeyDown(KeyCode.F11))
                DiaryManager.Instance.UnlockEntry(4, "4");

            if (Input.GetKeyDown(KeyCode.F12))
                DiaryManager.Instance.ClearAll();
        }
    }
}
