using UnityEngine;
using Yarn.Unity;

namespace Triskel.Dialogue
{
    /// <summary>
    /// Configura variables globales para los diálogos de Yarn.
    /// </summary>
    public class DialogueVariables : MonoBehaviour
    {
        [SerializeField] private DialogueRunner dialogueRunner;

        /// <summary>
        /// Inicializa las variables. Llamado por GameManager.
        /// </summary>
        public void Initialize()
        {
            if (dialogueRunner == null)
            {
                dialogueRunner = FindFirstObjectByType<DialogueRunner>();
            }

            if (dialogueRunner == null)
            {
                Debug.LogError("[DialogueVariables] DialogueRunner no encontrado.");
                return;
            }

            SetPlatformVariables();
            Debug.Log("[DialogueVariables] Variables inicializadas.");
        }

        private void SetPlatformVariables()
        {
            bool isMobile = Application.isMobilePlatform;
            dialogueRunner.VariableStorage.SetValue("$isMobile", isMobile);
        }

        /// <summary>
        /// Permite añadir variables desde otros scripts.
        /// </summary>
        public void SetVariable(string name, bool value)
        {
            dialogueRunner.VariableStorage.SetValue(name, value);
        }

        public void SetVariable(string name, string value)
        {
            dialogueRunner.VariableStorage.SetValue(name, value);
        }

        public void SetVariable(string name, float value)
        {
            dialogueRunner.VariableStorage.SetValue(name, value);
        }
    }
}
