using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Triskel.API
{
    /// <summary>
    /// Cliente HTTP para comunicarse con la API de Triskel (FastAPI).
    ///
    /// ENDPOINTS RELACIONADOS CON INVENTARIO:
    /// - PATCH /v1/games/{game_id} → Actualizar reliquias de la partida
    /// - GET /v1/games/{game_id}   → Obtener datos de la partida
    ///
    /// USO:
    /// 1. Configurar baseURL y credenciales en el Inspector
    /// 2. Llamar métodos con await o coroutines
    /// </summary>
    public class TriskelAPIClient : MonoBehaviour
    {
        // Singleton
        public static TriskelAPIClient Instance { get; private set; }

        [Header("Configuración de la API")]
        [Tooltip("URL base de la API (ejemplo: https://tu-api.com)")]
        [SerializeField] private string baseURL = "https://localhost:8000";

        [Header("Autenticación")]
        [Tooltip("Player ID (se obtiene al crear jugador)")]
        public string playerID = "";

        [Tooltip("Player Token (se obtiene al crear jugador)")]
        public string playerToken = "";

        [Header("Partida Actual")]
        [Tooltip("Game ID de la partida en curso")]
        public string currentGameID = "";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        #region Game Endpoints

        /// <summary>
        /// Actualiza las reliquias de la partida actual en la API.
        /// Endpoint: PATCH /v1/games/{game_id}
        /// </summary>
        /// <param name="relics">Array de IDs de reliquias: ["lirio", "hacha", "manto"]</param>
        /// <param name="onSuccess">Callback cuando la operación es exitosa</param>
        /// <param name="onError">Callback cuando hay error</param>
        public void UpdateGameRelics(string[] relics, Action onSuccess = null, Action<string> onError = null)
        {
            if (string.IsNullOrEmpty(currentGameID))
            {
                Debug.LogError("[TriskelAPI] No hay gameID configurado. No se puede actualizar reliquias.");
                onError?.Invoke("No game ID");
                return;
            }

            StartCoroutine(UpdateGameRelicsCoroutine(currentGameID, relics, onSuccess, onError));
        }

        private IEnumerator UpdateGameRelicsCoroutine(string gameID, string[] relics, Action onSuccess, Action<string> onError)
        {
            string url = $"{baseURL}/v1/games/{gameID}";

            // Crear JSON body
            string jsonBody = CreateRelicsJSON(relics);

            // Crear request
            using (UnityWebRequest request = new UnityWebRequest(url, "PATCH"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();

                // Headers
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("X-Player-ID", playerID);
                request.SetRequestHeader("X-Player-Token", playerToken);

                Debug.Log($"[TriskelAPI] PATCH {url}");
                Debug.Log($"[TriskelAPI] Body: {jsonBody}");

                // Enviar request
                yield return request.SendWebRequest();

                // Manejar respuesta
                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"[TriskelAPI] Reliquias actualizadas correctamente: {string.Join(", ", relics)}");
                    onSuccess?.Invoke();
                }
                else
                {
                    Debug.LogError($"[TriskelAPI] Error al actualizar reliquias: {request.error}");
                    Debug.LogError($"[TriskelAPI] Response: {request.downloadHandler.text}");
                    onError?.Invoke(request.error);
                }
            }
        }

        /// <summary>
        /// Obtiene los datos de una partida.
        /// Endpoint: GET /v1/games/{game_id}
        /// </summary>
        public void GetGame(string gameID, Action<GameData> onSuccess = null, Action<string> onError = null)
        {
            StartCoroutine(GetGameCoroutine(gameID, onSuccess, onError));
        }

        private IEnumerator GetGameCoroutine(string gameID, Action<GameData> onSuccess, Action<string> onError)
        {
            string url = $"{baseURL}/v1/games/{gameID}";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                // Headers de autenticación
                request.SetRequestHeader("X-Player-ID", playerID);
                request.SetRequestHeader("X-Player-Token", playerToken);

                Debug.Log($"[TriskelAPI] GET {url}");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string json = request.downloadHandler.text;
                    Debug.Log($"[TriskelAPI] Game obtenido: {json}");

                    // Parsear JSON a GameData
                    GameData gameData = JsonUtility.FromJson<GameData>(json);
                    onSuccess?.Invoke(gameData);
                }
                else
                {
                    Debug.LogError($"[TriskelAPI] Error al obtener game: {request.error}");
                    onError?.Invoke(request.error);
                }
            }
        }

        /// <summary>
        /// Obtiene todas las partidas de un jugador.
        /// Endpoint: GET /v1/games/player/{player_id}
        /// </summary>
        public void GetPlayerGames(Action<GameData[]> onSuccess = null, Action<string> onError = null)
        {
            StartCoroutine(GetPlayerGamesCoroutine(playerID, onSuccess, onError));
        }

        private IEnumerator GetPlayerGamesCoroutine(string playerID, Action<GameData[]> onSuccess, Action<string> onError)
        {
            string url = $"{baseURL}/v1/games/player/{playerID}";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("X-Player-ID", this.playerID);
                request.SetRequestHeader("X-Player-Token", playerToken);

                Debug.Log($"[TriskelAPI] GET {url}");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string json = request.downloadHandler.text;
                    Debug.Log($"[TriskelAPI] Games obtenidos: {json}");

                    // Parsear JSON array
                    GameData[] games = ParseGameArray(json);
                    onSuccess?.Invoke(games);
                }
                else
                {
                    Debug.LogError($"[TriskelAPI] Error al obtener games del jugador: {request.error}");
                    onError?.Invoke(request.error);
                }
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Crea el JSON para actualizar reliquias.
        /// Formato: { "relics": ["lirio", "hacha", "manto"] }
        /// </summary>
        private string CreateRelicsJSON(string[] relics)
        {
            // Construir manualmente el JSON
            string relicsArray = "[\"" + string.Join("\", \"", relics) + "\"]";
            return $"{{ \"relics\": {relicsArray} }}";
        }

        /// <summary>
        /// Parsea un array JSON de games.
        /// Unity JsonUtility no soporta arrays directamente, así que usamos un wrapper.
        /// </summary>
        private GameData[] ParseGameArray(string json)
        {
            // Wrapper para deserializar arrays
            string wrappedJson = $"{{ \"games\": {json} }}";
            GameArrayWrapper wrapper = JsonUtility.FromJson<GameArrayWrapper>(wrappedJson);
            return wrapper.games;
        }

        #endregion

        #region Data Classes

        /// <summary>
        /// Modelo de datos de una partida (Game).
        /// Debe coincidir con la estructura de tu API.
        /// </summary>
        [Serializable]
        public class GameData
        {
            public string game_id;
            public string player_id;
            public string status; // "in_progress", "completed", "abandoned"
            public string[] relics; // ["lirio", "hacha", "manto"]
            public string[] levels_completed;
            public bool boss_defeated;
            // Puedes añadir más campos según necesites
        }

        [Serializable]
        private class GameArrayWrapper
        {
            public GameData[] games;
        }

        #endregion

        #region Debug Methods

        /// <summary>
        /// Test rápido de conexión a la API (Context Menu en Inspector).
        /// </summary>
        [ContextMenu("Debug: Test API Connection")]
        public void TestAPIConnection()
        {
            Debug.Log($"[TriskelAPI] Probando conexión a: {baseURL}");
            Debug.Log($"[TriskelAPI] Player ID: {playerID}");
            Debug.Log($"[TriskelAPI] Game ID: {currentGameID}");

            if (string.IsNullOrEmpty(playerID) || string.IsNullOrEmpty(playerToken))
            {
                Debug.LogWarning("[TriskelAPI] No hay credenciales configuradas.");
                return;
            }

            // Probar obtener partidas del jugador
            GetPlayerGames(
                onSuccess: (games) => Debug.Log($"[TriskelAPI] ✓ Conexión exitosa. Games encontrados: {games.Length}"),
                onError: (error) => Debug.LogError($"[TriskelAPI] ✗ Error de conexión: {error}")
            );
        }

        #endregion
    }
}
