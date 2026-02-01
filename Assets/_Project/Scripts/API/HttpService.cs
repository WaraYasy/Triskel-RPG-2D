// =======================================================================================
// Triskel RPG 2D - HTTP Service
// =======================================================================================
// Autor: Mandrágora - Wara Pacheco
// Descripción: Servicio HTTP centralizado para realizar peticiones a la API REST.
//              Maneja autenticación mediante headers personalizados (X-Player-ID,
//              X-Player-Token), serialización/deserialización JSON y manejo de errores
//              de forma unificada.
// =======================================================================================

using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Triskel.UI;

namespace Triskel.API
{
    /// <summary>
    /// Servicio HTTP base para realizar peticiones a la API REST de Triskel.
    /// </summary>
    /// <remarks>
    /// Este servicio encapsula toda la lógica de comunicación HTTP usando UnityWebRequest.
    /// Maneja automáticamente:
    /// - Headers de autenticación (X-Player-ID, X-Player-Token)
    /// - Serialización/deserialización JSON con JsonUtility
    /// - Manejo de errores HTTP con callbacks
    /// - Logging de peticiones y respuestas
    ///
    /// Es utilizado internamente por TriskelAPIClient.
    /// </remarks>
    public class HttpService
    {
        private readonly string baseURL;
        private readonly MonoBehaviour coroutineRunner;

        // Credenciales del jugador
        /// <summary>
        /// ID del jugador para incluir en headers de autenticación.
        /// </summary>
        public string PlayerID { get; set; }
        /// <summary>
        /// Token del jugador para incluir en headers de autenticación.
        /// </summary>
        public string PlayerToken { get; set; }

        // Eventos para notificaciones globales
        /// <summary>
        /// Evento que se dispara cuando hay un error de conexión (no se puede alcanzar el servidor).
        /// </summary>
        /// <remarks>
        /// Este evento es escuchado por GameplayUIManager para mostrar la alerta de error de conexión.
        /// </remarks>
        public event Action OnConnectionError;

        /// <summary>
        /// Constructor del servicio HTTP.
        /// </summary>
        /// <param name="baseURL">URL base de la API (ej: "http://localhost:8000").</param>
        /// <param name="runner">MonoBehaviour para ejecutar corrutinas.</param>
        public HttpService(string baseURL, MonoBehaviour runner)
        {
            this.baseURL = baseURL.TrimEnd('/');
            this.coroutineRunner = runner;
        }

        #region Public Methods

        /// <summary>
        /// Realiza una petición GET a la API.
        /// </summary>
        /// <typeparam name="T">Tipo de la respuesta esperada.</typeparam>
        /// <param name="endpoint">Endpoint relativo (ej: "/v1/players/me").</param>
        /// <param name="onSuccess">Callback ejecutado con la respuesta deserializada.</param>
        /// <param name="onError">Callback ejecutado si ocurre un error.</param>
        public void Get<T>(string endpoint, Action<T> onSuccess, Action<string> onError = null)
        {
            coroutineRunner.StartCoroutine(GetCoroutine(endpoint, onSuccess, onError));
        }

        /// <summary>
        /// Realiza una petición POST con body JSON a la API.
        /// </summary>
        /// <typeparam name="TRequest">Tipo del objeto a enviar (será serializado a JSON).</typeparam>
        /// <typeparam name="TResponse">Tipo de la respuesta esperada.</typeparam>
        /// <param name="endpoint">Endpoint relativo (ej: "/v1/games").</param>
        /// <param name="body">Objeto a serializar y enviar como body.</param>
        /// <param name="onSuccess">Callback ejecutado con la respuesta deserializada.</param>
        /// <param name="onError">Callback ejecutado si ocurre un error.</param>
        public void Post<TRequest, TResponse>(string endpoint, TRequest body, Action<TResponse> onSuccess, Action<string> onError = null)
        {
            string json = JsonUtility.ToJson(body);
            coroutineRunner.StartCoroutine(PostCoroutine(endpoint, json, onSuccess, onError));
        }

        /// <summary>
        /// Realiza una peticion POST sin body.
        /// </summary>
        public void Post<T>(string endpoint, Action<T> onSuccess, Action<string> onError = null)
        {
            coroutineRunner.StartCoroutine(PostCoroutine<T>(endpoint, null, onSuccess, onError));
        }

        /// <summary>
        /// Realiza una petición PATCH con body JSON a la API.
        /// </summary>
        /// <typeparam name="TRequest">Tipo del objeto a enviar (será serializado a JSON).</typeparam>
        /// <typeparam name="TResponse">Tipo de la respuesta esperada.</typeparam>
        /// <param name="endpoint">Endpoint relativo (ej: "/v1/games/{game_id}").</param>
        /// <param name="body">Objeto a serializar y enviar como body.</param>
        /// <param name="onSuccess">Callback ejecutado con la respuesta deserializada.</param>
        /// <param name="onError">Callback ejecutado si ocurre un error.</param>
        public void Patch<TRequest, TResponse>(string endpoint, TRequest body, Action<TResponse> onSuccess, Action<string> onError = null)
        {
            string json = JsonUtility.ToJson(body);
            coroutineRunner.StartCoroutine(PatchCoroutine(endpoint, json, onSuccess, onError));
        }

        /// <summary>
        /// Realiza una petición PATCH con JSON raw (string).
        /// </summary>
        /// <typeparam name="TResponse">Tipo de la respuesta esperada.</typeparam>
        /// <param name="endpoint">Endpoint relativo.</param>
        /// <param name="jsonBody">JSON como string (ya serializado).</param>
        /// <param name="onSuccess">Callback ejecutado con la respuesta deserializada.</param>
        /// <param name="onError">Callback ejecutado si ocurre un error.</param>
        public void PatchRaw<TResponse>(string endpoint, string jsonBody, Action<TResponse> onSuccess, Action<string> onError = null)
        {
            coroutineRunner.StartCoroutine(PatchCoroutine(endpoint, jsonBody, onSuccess, onError));
        }

        /// <summary>
        /// Realiza una petición DELETE a la API.
        /// </summary>
        /// <param name="endpoint">Endpoint relativo (ej: "/v1/games/{game_id}").</param>
        /// <param name="onSuccess">Callback ejecutado si la operación es exitosa.</param>
        /// <param name="onError">Callback ejecutado si ocurre un error.</param>
        public void Delete(string endpoint, Action onSuccess, Action<string> onError = null)
        {
            coroutineRunner.StartCoroutine(DeleteCoroutine(endpoint, onSuccess, onError));
        }

        #endregion

        #region Coroutines

        /// <summary>
        /// Crea una petición HTTP con body JSON (para POST/PATCH).
        /// </summary>
        /// <param name="url">URL completa de la petición.</param>
        /// <param name="method">Método HTTP (POST, PATCH).</param>
        /// <param name="jsonBody">Body JSON como string.</param>
        /// <returns>UnityWebRequest configurado.</returns>
        private UnityWebRequest CreateRequestWithBody(string url, string method, string jsonBody)
        {
            UnityWebRequest request = new UnityWebRequest(url, method);

            if (!string.IsNullOrEmpty(jsonBody))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            }

            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            AddAuthHeaders(request);

            return request;
        }

        private IEnumerator GetCoroutine<T>(string endpoint, Action<T> onSuccess, Action<string> onError)
        {
            string url = $"{baseURL}{endpoint}";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                AddAuthHeaders(request);
                LogRequest("GET", url, null);

                yield return request.SendWebRequest();

                HandleResponse(request, onSuccess, onError);
            }
        }

        private IEnumerator PostCoroutine<T>(string endpoint, string jsonBody, Action<T> onSuccess, Action<string> onError)
        {
            string url = $"{baseURL}{endpoint}";

            using (UnityWebRequest request = CreateRequestWithBody(url, "POST", jsonBody))
            {
                LogRequest("POST", url, jsonBody);
                yield return request.SendWebRequest();
                HandleResponse(request, onSuccess, onError);
            }
        }

        private IEnumerator PatchCoroutine<T>(string endpoint, string jsonBody, Action<T> onSuccess, Action<string> onError)
        {
            string url = $"{baseURL}{endpoint}";

            using (UnityWebRequest request = CreateRequestWithBody(url, "PATCH", jsonBody))
            {
                LogRequest("PATCH", url, jsonBody);
                yield return request.SendWebRequest();
                HandleResponse(request, onSuccess, onError);
            }
        }

        private IEnumerator DeleteCoroutine(string endpoint, Action onSuccess, Action<string> onError)
        {
            string url = $"{baseURL}{endpoint}";

            using (UnityWebRequest request = UnityWebRequest.Delete(url))
            {
                request.downloadHandler = new DownloadHandlerBuffer();
                AddAuthHeaders(request);
                LogRequest("DELETE", url, null);

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"[HTTP] DELETE exitoso: {url}");
                    onSuccess?.Invoke();
                }
                else
                {
                    HandleError(request, onError);
                }
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Añade headers de autenticación a la petición HTTP.
        /// </summary>
        /// <param name="request">Petición UnityWebRequest a modificar.</param>
        /// <remarks>
        /// Añade X-Player-ID y X-Player-Token si están configurados.
        /// </remarks>
        private void AddAuthHeaders(UnityWebRequest request)
        {
            if (!string.IsNullOrEmpty(PlayerID))
                request.SetRequestHeader("X-Player-ID", PlayerID);

            if (!string.IsNullOrEmpty(PlayerToken))
                request.SetRequestHeader("X-Player-Token", PlayerToken);
        }

        /// <summary>
        /// Maneja la respuesta de una petición HTTP, deserializando el JSON y ejecutando callbacks.
        /// </summary>
        /// <typeparam name="T">Tipo de la respuesta esperada.</typeparam>
        /// <param name="request">Petición completada.</param>
        /// <param name="onSuccess">Callback de éxito.</param>
        /// <param name="onError">Callback de error.</param>
        private void HandleResponse<T>(UnityWebRequest request, Action<T> onSuccess, Action<string> onError)
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                Debug.Log($"[HTTP] Response: {json}");

                try
                {
                    T result = JsonUtility.FromJson<T>(json);
                    onSuccess?.Invoke(result);
                }
                catch (Exception e)
                {
                    string error = $"Error deserializando respuesta: {e.Message}";
                    Debug.LogError($"[HTTP] {error}");
                    onError?.Invoke(error);
                }
            }
            else
            {
                HandleError(request, onError);
            }
        }

        /// <summary>
        /// Maneja errores de peticiones HTTP, logueando detalles y ejecutando callbacks.
        /// </summary>
        /// <param name="request">Petición con error.</param>
        /// <param name="onError">Callback de error.</param>
        private void HandleError(UnityWebRequest request, Action<string> onError)
        {
            string error = request.error;
            long statusCode = request.responseCode;
            string responseBody = "";

            try
            {
                responseBody = request.downloadHandler?.text ?? "";
            }
            catch (Exception ex)
            {
                Debug.LogError($"[HTTP] Error leyendo response body: {ex.Message}");
            }

            Debug.LogError($"[HTTP] Error {statusCode}: {error}");
            if (!string.IsNullOrEmpty(responseBody))
                Debug.LogError($"[HTTP] Response: {responseBody}");

            // Detectar errores de conexión
            bool isConnectionError = request.result == UnityWebRequest.Result.ConnectionError ||
                                     request.result == UnityWebRequest.Result.ProtocolError && statusCode == 0 ||
                                     request.error.Contains("Cannot connect") ||
                                     request.error.Contains("Cannot resolve") ||
                                     request.error.Contains("Unable to connect") ||
                                     request.error.Contains("No connection") ||
                                     request.error.Contains("Timeout") ||
                                     request.error.Contains("timeout") ||
                                     request.error.Contains("destination host") ||
                                     statusCode == 0; // statusCode 0 generalmente indica fallo de conexión

            if (isConnectionError)
            {
                Debug.LogWarning("[HTTP] Error de conexión detectado");

                // Disparar evento para que la capa de UI lo maneje
                OnConnectionError?.Invoke();
            }

            // Ejecutar callback de error si existe
            onError?.Invoke(error);
        }

        /// <summary>
        /// Loguea información de una petición HTTP para debugging.
        /// </summary>
        /// <param name="method">Método HTTP (GET, POST, etc.).</param>
        /// <param name="url">URL completa.</param>
        /// <param name="body">Body JSON (opcional).</param>
        private void LogRequest(string method, string url, string body)
        {
            Debug.Log($"[HTTP] {method} {url}");
            if (!string.IsNullOrEmpty(body))
                Debug.Log($"[HTTP] Body: {body}");
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Verifica si hay credenciales configuradas.
        /// </summary>
        public bool HasCredentials()
        {
            return !string.IsNullOrEmpty(PlayerID) && !string.IsNullOrEmpty(PlayerToken);
        }

        /// <summary>
        /// Limpia las credenciales (logout).
        /// </summary>
        public void ClearCredentials()
        {
            PlayerID = "";
            PlayerToken = "";
        }

        #endregion
    }
}
