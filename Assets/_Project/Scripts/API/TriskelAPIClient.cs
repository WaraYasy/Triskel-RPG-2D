using System;
using UnityEngine;
using Triskel.API.Models;

namespace Triskel.API
{
    /// <summary>
    /// Cliente principal para comunicarse con la API de Triskel.
    ///
    /// COMO USAR:
    /// 1. Accede via TriskelAPIClient.Instance
    /// 2. Configura la URL base en el Inspector
    /// 3. Usa RegisterPlayer() para crear/cargar jugador
    /// 4. Llama a los metodos que necesites
    ///
    /// EJEMPLO:
    /// TriskelAPIClient.Instance.CreateGame(game => {
    ///     Debug.Log($"Partida creada: {game.game_id}");
    /// });
    /// </summary>
    public class TriskelAPIClient : MonoBehaviour
    {
        // ==========================================
        // SINGLETON
        // ==========================================
        public static TriskelAPIClient Instance { get; private set; }

        // ==========================================
        // CONFIGURACION
        // ==========================================
        [Header("Configuracion de la API")]
        [SerializeField] private string baseURL = "http://localhost:8000";

        [Header("Debug")]

        // ==========================================
        // ESTADO
        // ==========================================
        private HttpService http;
        private string currentGameID;
        private string currentSessionID;

        // Propiedades publicas de solo lectura
        public string PlayerID => http?.PlayerID ?? "";
        public string PlayerToken => http?.PlayerToken ?? "";
        public string CurrentGameID => currentGameID;
        public string CurrentSessionID => currentSessionID;
        public bool IsLoggedIn => http?.HasCredentials() ?? false;
        public bool HasActiveSession => !string.IsNullOrEmpty(currentSessionID);

        // Eventos
        public event Action<string> OnError;
        public event Action OnLoggedIn;
        public event Action OnLoggedOut;

        // ==========================================
        // UNITY LIFECYCLE
        // ==========================================
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Inicializar HTTP service
            http = new HttpService(baseURL, this);
            http.OnRequestError += error => OnError?.Invoke(error);

            // Cargar credenciales guardadas
            LoadCredentials();
        }

        // ==========================================
        // PLAYERS - Registro y Autenticacion
        // ==========================================

        /// <summary>
        /// Registra un nuevo jugador con username y password.
        /// Las credenciales se guardan automaticamente.
        /// </summary>
        public void RegisterPlayer(string username, string password, string email = null,
            Action<CreatePlayerResponse> onSuccess = null, Action<string> onError = null)
        {
            var request = new CreatePlayerRequest
            {
                username = username,
                password = password,
                email = email
            };

            http.Post<CreatePlayerRequest, CreatePlayerResponse>("/v1/players", request,
                response =>
                {
                    // Guardar credenciales
                    http.PlayerID = response.player_id;
                    http.PlayerToken = response.player_token;
                    SaveCredentials();

                    Debug.Log($"[TriskelAPI] Jugador registrado: {response.username}");
                    OnLoggedIn?.Invoke();
                    onSuccess?.Invoke(response);
                },
                onError);
        }

        /// <summary>
        /// Inicia sesion con username y password.
        /// Las credenciales se guardan automaticamente.
        /// Si hay una partida activa, se carga automaticamente.
        /// </summary>
        public void Login(string username, string password,
            Action<LoginResponse> onSuccess = null, Action<string> onError = null)
        {
            var request = new LoginRequest
            {
                username = username,
                password = password
            };

            http.Post<LoginRequest, LoginResponse>("/v1/players/login", request,
                response =>
                {
                    // Guardar credenciales
                    http.PlayerID = response.player_id;
                    http.PlayerToken = response.player_token;
                    SaveCredentials();

                    // Si hay partida activa, cargarla
                    if (!string.IsNullOrEmpty(response.active_game_id))
                    {
                        currentGameID = response.active_game_id;
                        SaveCurrentGameID();
                        Debug.Log($"[TriskelAPI] Partida activa encontrada: {response.active_game_id}");
                    }

                    Debug.Log($"[TriskelAPI] Login exitoso: {response.username}");
                    OnLoggedIn?.Invoke();
                    onSuccess?.Invoke(response);
                },
                onError);
        }

        /// <summary>
        /// Verifica si las credenciales guardadas son validas.
        /// Usa esto al iniciar el juego para comprobar la sesion.
        /// </summary>
        public void VerifySession(Action<PlayerProfile> onSuccess = null, Action<string> onError = null)
        {
            if (!IsLoggedIn)
            {
                onError?.Invoke("No hay credenciales guardadas");
                return;
            }

            http.Get<PlayerProfile>("/v1/players/me", onSuccess,
                error =>
                {
                    // Si falla, limpiar credenciales invalidas
                    ClearCredentials();
                    onError?.Invoke(error);
                });
        }

        /// <summary>
        /// Obtiene el perfil completo del jugador actual.
        /// </summary>
        public void GetMyProfile(Action<PlayerProfile> onSuccess = null, Action<string> onError = null)
        {
            http.Get<PlayerProfile>("/v1/players/me", onSuccess, onError);
        }

        /// <summary>
        /// Actualiza datos del jugador.
        /// </summary>
        public void UpdatePlayer(UpdatePlayerRequest data,
            Action<PlayerProfile> onSuccess = null, Action<string> onError = null)
        {
            http.Patch<UpdatePlayerRequest, PlayerProfile>($"/v1/players/{PlayerID}", data, onSuccess, onError);
        }

        /// <summary>
        /// Cierra sesion (borra credenciales locales).
        /// Si hay una sesion de juego activa, la termina primero.
        /// </summary>
        public void Logout()
        {
            // Terminar sesion de juego si existe
            if (HasActiveSession)
            {
                EndSession();
            }

            ClearCredentials();
            currentGameID = null;
            currentSessionID = null;
            OnLoggedOut?.Invoke();
            Debug.Log("[TriskelAPI] Sesion cerrada");
        }

        // ==========================================
        // GAMES - Partidas
        // ==========================================

        /// <summary>
        /// Crea una nueva partida.
        /// El game_id se guarda automaticamente como partida actual.
        /// </summary>
        public void CreateGame(Action<GameData> onSuccess = null, Action<string> onError = null)
        {
            var request = new CreateGameRequest { player_id = PlayerID };

            http.Post<CreateGameRequest, GameData>("/v1/games", request,
                game =>
                {
                    currentGameID = game.game_id;
                    SaveCurrentGameID();
                    Debug.Log($"[TriskelAPI] Partida creada: {game.game_id}");
                    onSuccess?.Invoke(game);
                },
                onError);
        }

        /// <summary>
        /// Obtiene los datos de la partida actual.
        /// </summary>
        public void GetCurrentGame(Action<GameData> onSuccess = null, Action<string> onError = null)
        {
            if (string.IsNullOrEmpty(currentGameID))
            {
                onError?.Invoke("No hay partida activa");
                return;
            }
            GetGame(currentGameID, onSuccess, onError);
        }

        /// <summary>
        /// Obtiene los datos de una partida especifica.
        /// </summary>
        public void GetGame(string gameId, Action<GameData> onSuccess = null, Action<string> onError = null)
        {
            http.Get<GameData>($"/v1/games/{gameId}", onSuccess, onError);
        }

        /// <summary>
        /// Obtiene todas las partidas del jugador actual.
        /// </summary>
        public void GetMyGames(Action<GameData[]> onSuccess = null, Action<string> onError = null)
        {
            http.Get<GameArrayWrapper>($"/v1/games/player/{PlayerID}",
                wrapper => onSuccess?.Invoke(wrapper.games),
                onError);
        }

        /// <summary>
        /// Inicia un nivel en la partida actual.
        /// </summary>
        public void StartLevel(string level, Action<GameData> onSuccess = null, Action<string> onError = null)
        {
            if (string.IsNullOrEmpty(currentGameID))
            {
                onError?.Invoke("No hay partida activa");
                return;
            }

            var request = new StartLevelRequest { level = level };
            http.Post<StartLevelRequest, GameData>($"/v1/games/{currentGameID}/level/start", request, onSuccess, onError);
        }

        /// <summary>
        /// Completa un nivel en la partida actual.
        /// </summary>
        public void CompleteLevel(string level, int timeSeconds, int deaths,
            string choice = null, string relic = null,
            Action<GameData> onSuccess = null, Action<string> onError = null)
        {
            if (string.IsNullOrEmpty(currentGameID))
            {
                onError?.Invoke("No hay partida activa");
                return;
            }

            var request = new CompleteLevelRequest
            {
                level = level,
                time_seconds = timeSeconds,
                deaths = deaths,
                choice = choice,
                relic = relic
            };

            http.Post<CompleteLevelRequest, GameData>($"/v1/games/{currentGameID}/level/complete", request,
                game =>
                {
                    Debug.Log($"[TriskelAPI] Nivel completado: {level}");
                    onSuccess?.Invoke(game);
                },
                onError);
        }

        /// <summary>
        /// Actualiza datos de la partida actual.
        /// </summary>
        public void UpdateCurrentGame(UpdateGameRequest data,
            Action<GameData> onSuccess = null, Action<string> onError = null)
        {
            if (string.IsNullOrEmpty(currentGameID))
            {
                onError?.Invoke("No hay partida activa");
                return;
            }

            http.Patch<UpdateGameRequest, GameData>($"/v1/games/{currentGameID}", data, onSuccess, onError);
        }

        /// <summary>
        /// Marca la partida actual como completada.
        /// Usa esto al derrotar al jefe final.
        /// </summary>
        public void CompleteGame(bool bossDefeated = true,
            Action<GameData> onSuccess = null, Action<string> onError = null)
        {
            var data = new UpdateGameRequest
            {
                status = APIConstants.GameStatus.COMPLETED,
                boss_defeated = bossDefeated,
                completion_percentage = 100f
            };

            UpdateCurrentGame(data,
                game =>
                {
                    Debug.Log("[TriskelAPI] Partida completada!");
                    onSuccess?.Invoke(game);
                },
                onError);
        }

        /// <summary>
        /// Abandona la partida actual.
        /// </summary>
        public void AbandonGame(Action<GameData> onSuccess = null, Action<string> onError = null)
        {
            var data = new UpdateGameRequest
            {
                status = APIConstants.GameStatus.ABANDONED
            };

            UpdateCurrentGame(data, onSuccess, onError);
        }

        /// <summary>
        /// Carga una partida existente como la partida actual.
        /// </summary>
        public void LoadGame(string gameId)
        {
            currentGameID = gameId;
            SaveCurrentGameID();
            Debug.Log($"[TriskelAPI] Partida cargada: {gameId}");
        }

        // ==========================================
        // EVENTS - Eventos de Gameplay
        // ==========================================

        /// <summary>
        /// Registra un evento generico.
        /// </summary>
        public void SendEvent(string eventType, string level, EventData data = null,
            Action<GameEvent> onSuccess = null, Action<string> onError = null)
        {
            if (string.IsNullOrEmpty(currentGameID))
            {
                Debug.LogWarning("[TriskelAPI] No hay partida activa, evento no enviado");
                return;
            }

            var request = new CreateEventRequest
            {
                game_id = currentGameID,
                player_id = PlayerID,
                event_type = eventType,
                level = level,
                data = data
            };

            http.Post<CreateEventRequest, GameEvent>("/v1/events", request, onSuccess, onError);
        }

        /// <summary>
        /// Registra la muerte del jugador.
        /// </summary>
        public void SendDeathEvent(string level, string cause, Vector2 position, string enemyType = null)
        {
            var data = new EventData
            {
                cause = cause,
                position_x = position.x,
                position_y = position.y,
                enemy_type = enemyType
            };

            SendEvent(APIConstants.EventTypes.PLAYER_DEATH, level, data);
        }

        /// <summary>
        /// Registra que el jugador alcanzo un checkpoint.
        /// </summary>
        public void SendCheckpointEvent(string level, string checkpointId)
        {
            var data = new EventData { checkpoint_id = checkpointId };
            SendEvent(APIConstants.EventTypes.CHECKPOINT_REACHED, level, data);
        }

        /// <summary>
        /// Registra que el jugador recogio un item.
        /// </summary>
        public void SendItemCollectedEvent(string level, string itemType, string relicName = null)
        {
            var data = new EventData
            {
                item_type = itemType,
                relic_name = relicName
            };
            SendEvent(APIConstants.EventTypes.ITEM_COLLECTED, level, data);
        }

        /// <summary>
        /// Registra interaccion con NPC.
        /// </summary>
        public void SendNPCInteractionEvent(string level, string npcId, string action)
        {
            var data = new EventData
            {
                npc_id = npcId,
                action = action
            };
            SendEvent(APIConstants.EventTypes.NPC_INTERACTION, level, data);
        }

        /// <summary>
        /// Registra encuentro con jefe.
        /// </summary>
        public void SendBossEncounterEvent(string level, string bossName)
        {
            var data = new EventData { boss_name = bossName };
            SendEvent(APIConstants.EventTypes.BOSS_ENCOUNTER, level, data);
        }

        /// <summary>
        /// Registra el final del juego.
        /// </summary>
        public void SendGameEndingEvent(int endingNumber)
        {
            var data = new EventData
            {
                event_name = "game_ending",
                ending_number = endingNumber
            };
            SendEvent(APIConstants.EventTypes.CUSTOM_EVENT, APIConstants.Levels.CLARO_ALMAS, data);
        }

        // ==========================================
        // SESSIONS - Sesiones de Juego
        // ==========================================

        /// <summary>
        /// Inicia una nueva sesion de juego.
        /// Llamar al abrir el juego o iniciar una partida.
        /// </summary>
        public void StartSession(Action<SessionData> onSuccess = null, Action<string> onError = null)
        {
            if (string.IsNullOrEmpty(currentGameID))
            {
                onError?.Invoke("No hay partida activa para iniciar sesion");
                return;
            }

            var request = new CreateSessionRequest
            {
                game_id = currentGameID,
                platform = APIConstants.Platforms.GetCurrentPlatform()
            };

            http.Post<CreateSessionRequest, SessionData>("/v1/sessions", request,
                session =>
                {
                    currentSessionID = session.session_id;
                    SaveCurrentSessionID();
                    Debug.Log($"[TriskelAPI] Sesion iniciada: {session.session_id}");
                    onSuccess?.Invoke(session);
                },
                onError);
        }

        /// <summary>
        /// Termina la sesion de juego actual.
        /// Llamar al cerrar el juego o salir de una partida.
        /// </summary>
        public void EndSession(Action<SessionData> onSuccess = null, Action<string> onError = null)
        {
            if (string.IsNullOrEmpty(currentSessionID))
            {
                Debug.LogWarning("[TriskelAPI] No hay sesion activa para terminar");
                onError?.Invoke("No hay sesion activa");
                return;
            }

            http.Patch<object, SessionData>($"/v1/sessions/{currentSessionID}/end", new { },
                session =>
                {
                    Debug.Log($"[TriskelAPI] Sesion terminada: {session.session_id} (duracion: {session.duration_seconds}s)");
                    currentSessionID = null;
                    ClearCurrentSessionID();
                    onSuccess?.Invoke(session);
                },
                onError);
        }

        /// <summary>
        /// Obtiene todas las sesiones del jugador actual.
        /// </summary>
        public void GetMySessions(int limit = 100, Action<SessionData[]> onSuccess = null, Action<string> onError = null)
        {
            http.Get<SessionArrayWrapper>($"/v1/sessions/player/{PlayerID}?limit={limit}",
                wrapper => onSuccess?.Invoke(wrapper.sessions),
                onError);
        }

        /// <summary>
        /// Obtiene todas las sesiones de una partida especifica.
        /// </summary>
        public void GetGameSessions(string gameId, int limit = 100, Action<SessionData[]> onSuccess = null, Action<string> onError = null)
        {
            http.Get<SessionArrayWrapper>($"/v1/sessions/game/{gameId}?limit={limit}",
                wrapper => onSuccess?.Invoke(wrapper.sessions),
                onError);
        }

        // ==========================================
        // PERSISTENCIA LOCAL
        // ==========================================

        private void SaveCredentials()
        {
            PlayerPrefs.SetString("triskel_player_id", http.PlayerID);
            PlayerPrefs.SetString("triskel_player_token", http.PlayerToken);
            PlayerPrefs.Save();
        }

        private void LoadCredentials()
        {
            http.PlayerID = PlayerPrefs.GetString("triskel_player_id", "");
            http.PlayerToken = PlayerPrefs.GetString("triskel_player_token", "");
            currentGameID = PlayerPrefs.GetString("triskel_current_game", "");
            currentSessionID = PlayerPrefs.GetString("triskel_current_session", "");

            if (IsLoggedIn)
                Debug.Log($"[TriskelAPI] Credenciales cargadas: {PlayerID}");
        }

        private void ClearCredentials()
        {
            http.ClearCredentials();
            PlayerPrefs.DeleteKey("triskel_player_id");
            PlayerPrefs.DeleteKey("triskel_player_token");
            PlayerPrefs.DeleteKey("triskel_current_game");
            PlayerPrefs.DeleteKey("triskel_current_session");
            PlayerPrefs.Save();
        }

        private void SaveCurrentGameID()
        {
            PlayerPrefs.SetString("triskel_current_game", currentGameID ?? "");
            PlayerPrefs.Save();
        }

        private void SaveCurrentSessionID()
        {
            PlayerPrefs.SetString("triskel_current_session", currentSessionID ?? "");
            PlayerPrefs.Save();
        }

        private void ClearCurrentSessionID()
        {
            currentSessionID = null;
            PlayerPrefs.DeleteKey("triskel_current_session");
            PlayerPrefs.Save();
        }

        // ==========================================
        // DEBUG
        // ==========================================

        /// <summary>
        /// Obtiene partidas en progreso del jugador.
        /// Util para mostrar menu "Continuar partida".
        /// </summary>
        public void GetActiveGames(Action<GameData[]> onSuccess, Action<string> onError = null)
        {
            GetMyGames(
                games =>
                {
                    var activeGames = System.Array.FindAll(games,
                        g => g.status == APIConstants.GameStatus.IN_PROGRESS);
                    onSuccess?.Invoke(activeGames);
                },
                onError);
        }

        [ContextMenu("Debug: Test Connection")]
        public void DebugTestConnection()
        {
            Debug.Log($"[TriskelAPI] URL: {baseURL}");
            Debug.Log($"[TriskelAPI] PlayerID: {PlayerID}");
            Debug.Log($"[TriskelAPI] IsLoggedIn: {IsLoggedIn}");
            Debug.Log($"[TriskelAPI] CurrentGame: {currentGameID}");
            Debug.Log($"[TriskelAPI] CurrentSession: {currentSessionID}");
            Debug.Log($"[TriskelAPI] HasActiveSession: {HasActiveSession}");

            if (IsLoggedIn)
            {
                VerifySession(
                    profile => Debug.Log($"[TriskelAPI] Sesion valida: {profile.username}"),
                    error => Debug.LogError($"[TriskelAPI] Sesion invalida: {error}")
                );
            }
        }

        [ContextMenu("Debug: Clear All Data")]
        public void DebugClearAllData()
        {
            ClearCredentials();
            currentGameID = null;
            Debug.Log("[TriskelAPI] Todos los datos borrados");
        }
    }
}
