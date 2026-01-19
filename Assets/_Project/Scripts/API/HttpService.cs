using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Triskel.API
{
    /// <summary>
    /// Servicio HTTP base para realizar peticiones a la API.
    /// Maneja autenticacion, headers y errores de forma centralizada.
    /// </summary>
    public class HttpService
    {
        private readonly string baseURL;
        private readonly MonoBehaviour coroutineRunner;

        // Credenciales del jugador
        public string PlayerID { get; set; }
        public string PlayerToken { get; set; }

        // Eventos para notificaciones globales
        public event Action<string> OnRequestError;
        public event Action<int, string> OnHttpError; // statusCode, message

        public HttpService(string baseURL, MonoBehaviour runner)
        {
            this.baseURL = baseURL.TrimEnd('/');
            this.coroutineRunner = runner;
        }

        #region Public Methods

        /// <summary>
        /// Realiza una peticion GET.
        /// </summary>
        public void Get<T>(string endpoint, Action<T> onSuccess, Action<string> onError = null)
        {
            coroutineRunner.StartCoroutine(GetCoroutine(endpoint, onSuccess, onError));
        }

        /// <summary>
        /// Realiza una peticion POST con body JSON.
        /// </summary>
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
        /// Realiza una peticion PATCH con body JSON.
        /// </summary>
        public void Patch<TRequest, TResponse>(string endpoint, TRequest body, Action<TResponse> onSuccess, Action<string> onError = null)
        {
            string json = JsonUtility.ToJson(body);
            coroutineRunner.StartCoroutine(PatchCoroutine(endpoint, json, onSuccess, onError));
        }

        /// <summary>
        /// Realiza una peticion DELETE.
        /// </summary>
        public void Delete(string endpoint, Action onSuccess, Action<string> onError = null)
        {
            coroutineRunner.StartCoroutine(DeleteCoroutine(endpoint, onSuccess, onError));
        }

        #endregion

        #region Coroutines

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

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                if (!string.IsNullOrEmpty(jsonBody))
                {
                    byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                }
                request.downloadHandler = new DownloadHandlerBuffer();

                request.SetRequestHeader("Content-Type", "application/json");
                AddAuthHeaders(request);
                LogRequest("POST", url, jsonBody);

                yield return request.SendWebRequest();

                HandleResponse(request, onSuccess, onError);
            }
        }

        private IEnumerator PatchCoroutine<T>(string endpoint, string jsonBody, Action<T> onSuccess, Action<string> onError)
        {
            string url = $"{baseURL}{endpoint}";

            using (UnityWebRequest request = new UnityWebRequest(url, "PATCH"))
            {
                if (!string.IsNullOrEmpty(jsonBody))
                {
                    byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                }
                request.downloadHandler = new DownloadHandlerBuffer();

                request.SetRequestHeader("Content-Type", "application/json");
                AddAuthHeaders(request);
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

        private void AddAuthHeaders(UnityWebRequest request)
        {
            if (!string.IsNullOrEmpty(PlayerID))
                request.SetRequestHeader("X-Player-ID", PlayerID);

            if (!string.IsNullOrEmpty(PlayerToken))
                request.SetRequestHeader("X-Player-Token", PlayerToken);
        }

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
                    OnRequestError?.Invoke(error);
                }
            }
            else
            {
                HandleError(request, onError);
            }
        }

        private void HandleError(UnityWebRequest request, Action<string> onError)
        {
            string error = request.error;
            long statusCode = request.responseCode;
            string responseBody = request.downloadHandler?.text ?? "";

            Debug.LogError($"[HTTP] Error {statusCode}: {error}");
            if (!string.IsNullOrEmpty(responseBody))
                Debug.LogError($"[HTTP] Response: {responseBody}");

            onError?.Invoke(error);
            OnRequestError?.Invoke(error);
            OnHttpError?.Invoke((int)statusCode, responseBody);
        }

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
