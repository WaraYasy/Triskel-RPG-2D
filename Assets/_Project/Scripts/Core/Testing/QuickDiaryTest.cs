using UnityEngine;
using Triskel.Core;

namespace Triskel.Testing
{
    /// <summary>
    /// Script de prueba para el sistema de diario.
    /// TECLAS: 1 y 2 = Nivel 0, 3 y 4 = Nivel 1, 5 y 6 = Nivel 2, 0 = Limpiar, J = Abrir diario
    /// </summary>
    public class QuickDiaryTest : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log("=== DIARIO TEST ===");
            Debug.Log("1: Nivel 0 Bueno | 2: Nivel 0 Malo");
            Debug.Log("3: Nivel 1 Bueno | 4: Nivel 1 Malo");
            Debug.Log("5: Nivel 2 Bueno | 6: Nivel 2 Malo");
            Debug.Log("0: Limpiar | J: Ver diario");
        }

        private void Update()
        {
            if (DiaryManager.Instance == null) return;

            if (Input.GetKeyDown(KeyCode.Alpha1))
                DiaryManager.Instance.UnlockEntry(0, "bueno");

            if (Input.GetKeyDown(KeyCode.Alpha2))
                DiaryManager.Instance.UnlockEntry(0, "malo");

            if (Input.GetKeyDown(KeyCode.Alpha3))
                DiaryManager.Instance.UnlockEntry(1, "bueno");

            if (Input.GetKeyDown(KeyCode.Alpha4))
                DiaryManager.Instance.UnlockEntry(1, "malo");

            if (Input.GetKeyDown(KeyCode.Alpha5))
                DiaryManager.Instance.UnlockEntry(2, "bueno");

            if (Input.GetKeyDown(KeyCode.Alpha6))
                DiaryManager.Instance.UnlockEntry(2, "malo");

            if (Input.GetKeyDown(KeyCode.Alpha0))
                DiaryManager.Instance.ClearAll();
        }
    }
}
