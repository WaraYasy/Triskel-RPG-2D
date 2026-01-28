 using UnityEngine;
 using UnityEngine.UIElements;
 using UnityEngine.SceneManagement;
 using UnityEngine.InputSystem;
 using System.Collections;
 using System.Collections.Generic;
 using System.Linq;

 namespace _Project.Scripts.Core.Transition
  {
      public class TransitionManager : MonoBehaviour
      {
          [Header("Referencias")]
          [SerializeField] private UIDocument uiDocument;

          [Header("Configuración")]
          [SerializeField] private string jsonFileName = "TransitionTexts";
          [SerializeField] private float tiempoFadeIn = 1f;
          [SerializeField] private float tiempoTextoPorDefecto = 4f;
          [SerializeField] private float tiempoFadeOut = 1f;

          // Datos estáticos (pasados desde la escena anterior)
          public static string TransitionID { get; set; }
          public static string SiguienteEscena { get; set; }

          // Diccionario de transiciones cargadas desde JSON
          private Dictionary<string, TransitionEntry> transitions = new Dictionary<string, TransitionEntry>();

          // Elementos UI
          private VisualElement root;
          private Label transitionTitle;
          private Label transitionText;

          private void Awake()
          {
              CargarTransicionesDesdeJSON();
          }

          private void Start()
          {
              // Obtener elementos del UXML
              root = uiDocument.rootVisualElement.Q<VisualElement>("root");
              transitionTitle = root.Q<Label>("transition-title");
              transitionText = root.Q<Label>("transition-text");

              // Verificar si tenemos datos
              if (string.IsNullOrEmpty(TransitionID))
              {
                  Debug.LogWarning("[Transition] No hay ID de transición. Cargando escena directamente.");
                  CargarSiguienteEscena();
                  return;
              }

              // Obtener el texto desde el JSON
              TransitionEntry entry = ObtenerTransicion(TransitionID);
              if (entry == null)
              {
                  Debug.LogWarning($"[Transition] No se encontró transición '{TransitionID}'");
                  CargarSiguienteEscena();
                  return;
              }

              // Iniciar secuencia
              if (transitionTitle != null)
              {
                  transitionTitle.text = string.IsNullOrEmpty(entry.title) ? "" : entry.title;
              }

              transitionText.text = entry.text;
              float tiempoTexto = entry.displayTime > 0 ? entry.displayTime
  : tiempoTextoPorDefecto;
              StartCoroutine(SecuenciaTransicion(tiempoTexto));
          }

          /// <summary>
          /// Carga las transiciones desde el archivo JSON.
          /// </summary>
          private void CargarTransicionesDesdeJSON()
          {
              TextAsset json =
  Resources.Load<TextAsset>($"TransitionData/{jsonFileName}");
              if (json == null)
              {
                  Debug.LogError($"[Transition] No se encontró {jsonFileName}.json en Resources/TransitionData/");
                  return;
              }

              TransitionData data =
  JsonUtility.FromJson<TransitionData>(json.text);
              foreach (var entry in data.transitions)
              {
                  transitions[entry.id] = entry;
              }

              Debug.Log($"[Transition] {transitions.Count} transiciones cargadas desde JSON");
          }

          /// <summary>
          /// Obtiene una transición por ID.
          /// </summary>
          private TransitionEntry ObtenerTransicion(string id)
          {
              return transitions.ContainsKey(id) ? transitions[id] : null;
          }

          /// <summary>
          /// Secuencia de fade in → mostrar texto → fade out → cargar escena.
          /// </summary>
          private IEnumerator SecuenciaTransicion(float tiempoTexto)
          {
              // 1. Fade In
              root.style.opacity = 0f;
              float tiempo = 0f;

              while (tiempo < tiempoFadeIn)
              {
                  tiempo += Time.deltaTime;
                  root.style.opacity = Mathf.Lerp(0f, 1f, tiempo /
  tiempoFadeIn);
                  yield return null;
              }
              root.style.opacity = 1f;

              // 2. Mostrar texto
              yield return new WaitForSeconds(tiempoTexto);

              // 3. Fade Out
              tiempo = 0f;
              while (tiempo < tiempoFadeOut)
              {
                  tiempo += Time.deltaTime;
                  root.style.opacity = Mathf.Lerp(1f, 0f, tiempo /
  tiempoFadeOut);
                  yield return null;
              }

              // 4. Cargar siguiente escena
              CargarSiguienteEscena();
          }

          private void CargarSiguienteEscena()
          {
              if (string.IsNullOrEmpty(SiguienteEscena))
              {
                  Debug.LogError("[Transition] No hay escena siguiente especificada.");
                  return;
              }

              Debug.Log($"[Transition] Cargando escena: {SiguienteEscena}");
              SceneManager.LoadScene(SiguienteEscena);
          }

          private void Update()
          {
              // Saltar con SPACE (útil para testing)
              if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
              {
                  StopAllCoroutines();
                  CargarSiguienteEscena();
              }
          }
      }
  }