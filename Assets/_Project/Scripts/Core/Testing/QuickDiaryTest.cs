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
        private void Start()
        {
            Debug.Log("=== DIARIO TEST ===");
            Debug.Log("F1: Nivel 0 Bueno | F2: Nivel 0 Malo");
            Debug.Log("F3: Nivel 1 Bueno | F4: Nivel 1 Malo");
            Debug.Log("F5: Nivel 2 Bueno | F6: Nivel 2 Malo");
            Debug.Log("F12: Limpiar | J: Ver diario");
        }

        private void Update()
        {
            if (DiaryManager.Instance == null) return;

            if (Input.GetKeyDown(KeyCode.F1))
                DiaryManager.Instance.UnlockEntry(0, "bueno");

            if (Input.GetKeyDown(KeyCode.F2))
                DiaryManager.Instance.UnlockEntry(0, "malo");

            if (Input.GetKeyDown(KeyCode.F3))
                DiaryManager.Instance.UnlockEntry(1, "bueno");

            if (Input.GetKeyDown(KeyCode.F4))
                DiaryManager.Instance.UnlockEntry(1, "malo");

            if (Input.GetKeyDown(KeyCode.F5))
                DiaryManager.Instance.UnlockEntry(2, "bueno");

            if (Input.GetKeyDown(KeyCode.F6))
                DiaryManager.Instance.UnlockEntry(2, "malo");

            if (Input.GetKeyDown(KeyCode.F12))
                DiaryManager.Instance.ClearAll();
        }
    }
}
