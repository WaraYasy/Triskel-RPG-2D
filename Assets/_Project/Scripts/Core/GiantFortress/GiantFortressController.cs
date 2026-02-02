using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using Triskel.API;

namespace Triskel.GiantFortress
{
    /// <summary>
    /// GiantFortressController - Controlador principal del puzzle del Gigante.
    /// Gestiona el estado global, la moral y el desbloqueo del camino.
    /// </summary>
    public class GiantFortressController : MonoBehaviour
    {
        [Header("Configuracion")]
        [SerializeField] private int treesToClear = 2;
        
        [Header("El Gigante")]
        [SerializeField] private GameObject giantObstacle;
        [SerializeField] private Animator giantAnimator;
        [SerializeField] private string sadTrigger = "Sad";
        [SerializeField] private string happyTrigger = "Happy";
        
        [Header("Sistema de Moral")]
        [SerializeField] private int moralRewardPerHeal = 1;
        [SerializeField] private int moralPenaltyPerChop = -1;
        
        [Header("Camino Bloqueado")]
        [SerializeField] private GameObject blockedPath;
        [SerializeField] private Transform pathUnlockPosition;
        
        [Header("Eventos")]
        public UnityEvent OnPathCleared;
        public UnityEvent OnGoodEnding;
        public UnityEvent OnBadEnding;
        public UnityEvent OnMixedEnding;
        
        private int treesHealed = 0;
        private int treesChopped = 0;
        private bool isPuzzleComplete = false;
        
        /// <summary>
        /// Llamar desde el UnityEvent OnHealed del CursedTree
        /// </summary>
        public void OnTreeHealed()
        {
            treesHealed++;
            Debug.Log($"Arbol sanado ({treesHealed} total). Gigante aliviado.");
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ModifyMoralWithChoice(moralRewardPerHeal, APIConstants.Choices.CONSTRUIR);
            }
            
            if (giantAnimator != null && !string.IsNullOrEmpty(happyTrigger))
            {
                giantAnimator.SetTrigger(happyTrigger);
            }
            
            CheckPuzzleCompletion();
        }
        
        /// <summary>
        /// Llamar desde el UnityEvent OnChopped del CursedTree
        /// </summary>
        public void OnTreeChopped()
        {
            treesChopped++;
            Debug.Log($"Arbol talado ({treesChopped} total). Gigante entristecido.");
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ModifyMoralWithChoice(moralPenaltyPerChop, APIConstants.Choices.DESTRUIR);
            }
            
            if (giantAnimator != null && !string.IsNullOrEmpty(sadTrigger))
            {
                giantAnimator.SetTrigger(sadTrigger);
            }
            
            CheckPuzzleCompletion();
        }
        
        private void CheckPuzzleCompletion()
        {
            int totalResolved = treesHealed + treesChopped;
            
            if (totalResolved >= treesToClear && !isPuzzleComplete)
            {
                isPuzzleComplete = true;
                CompletePuzzle();
            }
        }
        
        private void CompletePuzzle()
        {
            Debug.Log("Puzzle de la Fortaleza completado!");
            
            if (blockedPath != null)
            {
                blockedPath.SetActive(false);
            }
            
            if (giantObstacle != null && pathUnlockPosition != null)
            {
                StartCoroutine(MoveGiantAside());
            }
            
            OnPathCleared?.Invoke();
            
            if (treesChopped == 0)
            {
                Debug.Log("FINAL BUENO: Todos los arboles sanados");
                OnGoodEnding?.Invoke();
            }
            else if (treesHealed == 0)
            {
                Debug.Log("FINAL MALO: Todos los arboles talados");
                OnBadEnding?.Invoke();
            }
            else
            {
                Debug.Log("FINAL MIXTO: Algunos arboles sanados, otros talados");
                OnMixedEnding?.Invoke();
            }
        }
        
        private System.Collections.IEnumerator MoveGiantAside()
        {
            if (giantObstacle == null || pathUnlockPosition == null) yield break;
            
            Vector3 startPos = giantObstacle.transform.position;
            Vector3 endPos = pathUnlockPosition.position;
            float duration = 2f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = t * t * (3f - 2f * t);
                
                giantObstacle.transform.position = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }
            
            giantObstacle.transform.position = endPos;
            Debug.Log("El Gigante se ha movido.");
        }
        
        [ContextMenu("Debug: Estado del Puzzle")]
        public void DebugPuzzleState()
        {
            Debug.Log("===== ESTADO FORTALEZA =====");
            Debug.Log($"Necesarios para pasar: {treesToClear}");
            Debug.Log($"Sanados: {treesHealed}");
            Debug.Log($"Talados: {treesChopped}");
            Debug.Log($"Puzzle completo: {isPuzzleComplete}");
            Debug.Log("============================");
        }
        
        public bool IsPuzzleComplete() => isPuzzleComplete;
        public int GetTreesHealed() => treesHealed;
        public int GetTreesChopped() => treesChopped;
    }
}
