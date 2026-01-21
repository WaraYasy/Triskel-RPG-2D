using UnityEngine;
using UnityEngine.InputSystem;
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
            if (Keyboard.current == null) return;

            if (Keyboard.current.f1Key.wasPressedThisFrame)
                DiaryManager.Instance.UnlockEntry(0, "intro");

            if (Keyboard.current.f2Key.wasPressedThisFrame)
                DiaryManager.Instance.UnlockEntry(1, "malo");

            if (Keyboard.current.f3Key.wasPressedThisFrame)
                DiaryManager.Instance.UnlockEntry(1, "bueno");

            if (Keyboard.current.f4Key.wasPressedThisFrame)
                DiaryManager.Instance.UnlockEntry(2, "malo");

            if (Keyboard.current.f5Key.wasPressedThisFrame)
                DiaryManager.Instance.UnlockEntry(2, "bueno");

            if (Keyboard.current.f6Key.wasPressedThisFrame)
                DiaryManager.Instance.UnlockEntry(3, "malo");

            if (Keyboard.current.f7Key.wasPressedThisFrame)
                DiaryManager.Instance.UnlockEntry(3, "bueno");

            if (Keyboard.current.f8Key.wasPressedThisFrame)
                DiaryManager.Instance.UnlockEntry(4, "1");

            if (Keyboard.current.f9Key.wasPressedThisFrame)
                DiaryManager.Instance.UnlockEntry(4, "2");

            if (Keyboard.current.f10Key.wasPressedThisFrame)
                DiaryManager.Instance.UnlockEntry(4, "3");

            if (Keyboard.current.f11Key.wasPressedThisFrame)
                DiaryManager.Instance.UnlockEntry(4, "4");

            if (Keyboard.current.f12Key.wasPressedThisFrame)
                DiaryManager.Instance.ClearAll();
        }
    }
}
