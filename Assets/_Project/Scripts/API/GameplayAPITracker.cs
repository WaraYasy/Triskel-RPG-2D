// =======================================================================================
// Triskel RPG 2D - Gameplay API Tracker
// =======================================================================================
// Autor: Mandrágora - Wara Pacheco
// Descripción: Componente MonoBehaviour (NO Singleton) que trackea métricas de gameplay
//              y las sincroniza con la API REST. Gestiona inicio de nivel, completar
//              nivel, muerte del jugador, recogida de reliquias y auto-save cada 30s.
//              Diseñado para ser gestionado por GameManager.
// =======================================================================================

using System.Collections;
using UnityEngine;
using Triskel.Core;
using Triskel.API.Models;

namespace Triskel.API
{
    /// <summary>
    /// Tracker de métricas de gameplay integrado con la API REST de Triskel.
    /// </summary>
    /// <remarks>
    /// Este componente NO es Singleton. GameManager lo crea e inicializa en Start().
    ///
    /// RESPONSABILIDADES:
    /// - Trackear tiempo desde inicio de nivel
    /// - Contar muertes por nivel y totales
    /// - Registrar decisiones morales
    /// - Detectar recogida de reliquias (vía evento OnItemAdded)
    /// - Auto-guardar progreso cada 30 segundos
    /// - Enviar eventos a la API en momentos clave
    ///
    /// FLUJO TÍPICO:
    /// 1. GameManager.Start() → Initialize(apiClient, inventoryData)
    /// 2. LevelExit → OnLevelStart(apiLevel) → POST /level/start
    /// 3. Durante gameplay → AutoSave cada 30s → PATCH /games/{id}
    /// 4. PlayerHealth.Die() → OnPlayerDeath() → POST /events (player_death)
    /// 5. CollectibleObject → OnItemAdded() → POST /events (item_collected)
    /// 6. LevelExit → OnLevelComplete() → POST /level/complete
    /// </remarks>
    public class GameplayAPITracker : MonoBehaviour
    {
        // ==========================================
        // DEPENDENCIAS
        // ==========================================
        private TriskelAPIClient apiClient;
        private InventoryData inventoryData;

        // ==========================================
        // ESTADO DEL NIVEL ACTUAL
        // ==========================================
        private string currentLevel;
        private float levelStartTime;
        private int levelDeaths;
        private string moralChoice;
        private string relicObtained;

        // ==========================================
        // ESTADO GLOBAL
        // ==========================================
        private int totalDeaths;

        // ==========================================
        // AUTO-SAVE
        // ==========================================
        private Coroutine autoSaveCoroutine;
        private const float AUTO_SAVE_INTERVAL = 30f; // 30 segundos

        // ==========================================
        // INICIALIZACION
        // ==========================================

        /// <summary>
        /// Inicializa el tracker con las dependencias necesarias.
        /// </summary>
        /// <param name="client">Cliente API para comunicación REST.</param>
        /// <param name="inventory">Inventario del jugador para suscribirse a eventos de ítems.</param>
        /// <remarks>
        /// IMPORTANTE: Llamar desde GameManager.Start() DESPUÉS de que todos los Singletons
        /// estén creados. No usar Awake() para evitar race conditions.
        /// </remarks>
        public void Initialize(TriskelAPIClient client, InventoryData inventory)
        {
            apiClient = client;
            inventoryData = inventory;

            // Validar dependencias
            if (apiClient == null)
            {
                Debug.LogError("[APITracker] TriskelAPIClient es null. No se puede inicializar.");
                return;
            }

            if (inventoryData == null)
            {
                Debug.LogWarning("[APITracker] InventoryData es null. No se detectarán reliquias.");
            }

            // Suscribirse a eventos
            SubscribeToEvents();

            // Iniciar auto-save si hay partida activa
            if (!string.IsNullOrEmpty(apiClient.CurrentGameID))
            {
                StartAutoSave();
                Debug.Log($"[APITracker] Inicializado con partida: {apiClient.CurrentGameID}");
            }
            else
            {
                Debug.LogWarning("[APITracker] No hay partida activa. El tracker estará inactivo.");
            }
        }

        // ==========================================
        // EVENTOS DEL SISTEMA
        // ==========================================

        private void SubscribeToEvents()
        {
            if (inventoryData != null)
            {
                inventoryData.OnItemAdded += OnItemAdded;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (inventoryData != null)
            {
                inventoryData.OnItemAdded -= OnItemAdded;
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
            StopAutoSave();
        }

        // ==========================================
        // MÉTODOS PÚBLICOS - TRACKING DE NIVEL
        // ==========================================

        /// <summary>
        /// Inicia el tracking de un nuevo nivel.
        /// </summary>
        /// <param name="levelName">Constante API del nivel (usar APIConstants.Levels).</param>
        /// <remarks>
        /// Llamar desde LevelExit.HandleHubToLevel() al entrar a un nivel.
        /// Resetea métricas del nivel (tiempo, muertes) y envía evento a la API.
        /// </remarks>
        public void OnLevelStart(string levelName)
        {
            if (apiClient == null || string.IsNullOrEmpty(apiClient.CurrentGameID))
            {
                Debug.LogWarning("[APITracker] No hay partida activa. No se puede iniciar nivel.");
                return;
            }

            currentLevel = levelName;
            levelStartTime = Time.time;
            levelDeaths = 0;
            moralChoice = null;
            relicObtained = null;

            Debug.Log($"[APITracker] Nivel iniciado: {levelName}");

            // Enviar evento StartLevel a la API
            apiClient.StartLevel(levelName,
                game => Debug.Log($"[TriskelAPI] StartLevel enviado: {levelName}"),
                error => Debug.LogError($"[TriskelAPI] Error al iniciar nivel: {error}")
            );
        }

        /// <summary>
        /// Completa el nivel actual y envía los datos a la API.
        /// </summary>
        /// <remarks>
        /// Llamar desde LevelExit.HandleLevelComplete() al completar un nivel.
        /// Envía tiempo, muertes, decisión moral y reliquia obtenida.
        /// Resetea métricas del nivel tras enviar.
        /// </remarks>
        public void OnLevelComplete()
        {
            if (apiClient == null || string.IsNullOrEmpty(apiClient.CurrentGameID))
            {
                Debug.LogWarning("[APITracker] No hay partida activa. No se puede completar nivel.");
                return;
            }

            if (string.IsNullOrEmpty(currentLevel))
            {
                Debug.LogWarning("[APITracker] No hay nivel activo para completar.");
                return;
            }

            int timeSeconds = GetLevelTimeSeconds();

            // Decisión por defecto según el nivel (decisión buena si no se registró ninguna)
            string defaultChoice = GetDefaultChoiceForLevel(currentLevel);
            string finalChoice = string.IsNullOrEmpty(moralChoice) ? defaultChoice : moralChoice;

            // Reliquia: si no hay, enviar null (la API lo acepta como opcional)
            string finalRelic = string.IsNullOrEmpty(relicObtained) ? null : relicObtained;

            Debug.Log($"[APITracker] Nivel completado: {currentLevel} (Tiempo: {timeSeconds}s, Muertes: {levelDeaths}, Choice: {finalChoice}, Relic: {finalRelic ?? "NONE"})");

            // IMPORTANTE: Guardar reliquias del inventario ANTES de completar el nivel
            // Esto asegura que todas las reliquias se sincronicen incluso si el nivel se completa en <30s
            if (inventoryData != null)
            {
                var updateData = new UpdateGameRequest
                {
                    current_level = currentLevel,
                    relics = inventoryData.GetItemIDs()
                };

                apiClient.UpdateCurrentGame(updateData,
                    game => Debug.Log("[APITracker] Reliquias guardadas antes de completar nivel"),
                    error => Debug.LogWarning($"[APITracker] Error guardando reliquias: {error}")
                );
            }

            // Enviar a la API
            apiClient.CompleteLevel(
                level: currentLevel,
                timeSeconds: timeSeconds,
                deaths: levelDeaths,
                choice: finalChoice,
                relic: finalRelic,
                game =>
                {
                    Debug.Log($"[TriskelAPI] Nivel completado: {currentLevel}");
                    ResetLevelTracking();
                },
                error => Debug.LogError($"[TriskelAPI] Error al completar nivel: {error}")
            );
        }

        /// <summary>
        /// Registra la muerte del jugador.
        /// </summary>
        /// <param name="position">Posición donde murió el jugador.</param>
        /// <param name="cause">Causa de muerte (usar APIConstants.DeathCauses).</param>
        /// <param name="enemyType">Tipo de enemigo que causó la muerte (opcional).</param>
        /// <remarks>
        /// Llamar desde PlayerHealth.Die() al morir.
        /// Incrementa contadores de muerte (nivel y total) y envía evento a la API.
        /// </remarks>
        public void OnPlayerDeath(Vector2 position, string cause, string enemyType = null)
        {
            if (apiClient == null || string.IsNullOrEmpty(apiClient.CurrentGameID))
            {
                Debug.LogWarning("[APITracker] No hay partida activa. No se puede registrar muerte.");
                return;
            }

            if (string.IsNullOrEmpty(currentLevel))
            {
                Debug.LogWarning("[APITracker] No hay nivel activo para registrar muerte.");
                return;
            }

            levelDeaths++;
            totalDeaths++;

            Debug.Log($"[APITracker] Muerte registrada en {currentLevel} (Muerte #{levelDeaths} del nivel, Total: #{totalDeaths})");

            // Enviar evento de muerte a la API
            apiClient.SendDeathEvent(currentLevel, cause, position, enemyType);
        }

        /// <summary>
        /// Registra una decisión moral tomada por el jugador.
        /// </summary>
        /// <param name="choice">Decisión moral (usar APIConstants.Choices).</param>
        /// <remarks>
        /// Llamar desde scripts de diálogo (Yarn Spinner) o GameManager al tomar decisiones.
        /// Se enviará junto con el evento CompleteLevel.
        /// </remarks>
        public void RegistrarDecisionMoral(string choice)
        {
            moralChoice = choice;
            Debug.Log($"[APITracker] Decisión moral registrada: {choice}");
        }

        /// <summary>
        /// Resetea las métricas del nivel actual (tiempo, muertes, decisión, reliquia).
        /// </summary>
        /// <remarks>
        /// Llamar desde GameManager.OnJugadorMuerto() al morir o desde PauseController.OnRestartClicked()
        /// al reiniciar el nivel manualmente.
        /// NO resetea totalDeaths (se mantiene acumulado).
        /// </remarks>
        public void ResetLevelTracking()
        {
            levelStartTime = Time.time; // Reiniciar tiempo
            levelDeaths = 0; // Reiniciar muertes del nivel
            moralChoice = null;
            // relicObtained NO se resetea (la reliquia ya está en inventario)

            Debug.Log($"[APITracker] Tracking de nivel reseteado: {currentLevel}");
        }

        // ==========================================
        // EVENTOS DE INVENTARIO
        // ==========================================

        private void OnItemAdded(CollectibleItem item)
        {
            if (apiClient == null || string.IsNullOrEmpty(apiClient.CurrentGameID))
                return;

            if (string.IsNullOrEmpty(currentLevel))
                return;

            // Detectar si es una reliquia
            string relicID = item.itemID.ToLower();
            if (relicID == APIConstants.Relics.LIRIO ||
                relicID == APIConstants.Relics.HACHA ||
                relicID == APIConstants.Relics.MANTO)
            {
                relicObtained = relicID;
                Debug.Log($"[APITracker] Reliquia recogida: {relicID}");

                // Enviar evento a la API
                apiClient.SendItemCollectedEvent(currentLevel, "relic", relicID);
            }
        }

        // ==========================================
        // AUTO-SAVE
        // ==========================================

        private void StartAutoSave()
        {
            if (autoSaveCoroutine != null)
                StopCoroutine(autoSaveCoroutine);

            autoSaveCoroutine = StartCoroutine(AutoSaveCoroutine());
            Debug.Log($"[APITracker] Auto-save activado (intervalo: {AUTO_SAVE_INTERVAL}s)");
        }

        private void StopAutoSave()
        {
            if (autoSaveCoroutine != null)
            {
                StopCoroutine(autoSaveCoroutine);
                autoSaveCoroutine = null;
            }
        }

        private IEnumerator AutoSaveCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(AUTO_SAVE_INTERVAL);

                // Solo guardar si hay partida activa
                if (apiClient != null && !string.IsNullOrEmpty(apiClient.CurrentGameID))
                {
                    SaveProgress();
                }
            }
        }

        private void SaveProgress()
        {
            if (inventoryData == null)
                return;

            // No guardar si no hay nivel activo (estamos en hub o entre niveles)
            if (string.IsNullOrEmpty(currentLevel))
            {
                Debug.Log("[APITracker] Auto-save omitido (no hay nivel activo)");
                return;
            }

            var updateData = new UpdateGameRequest
            {
                current_level = currentLevel,
                relics = inventoryData.GetItemIDs()
                // Nota: total_deaths se sincroniza automáticamente al completar nivel
            };

            apiClient.UpdateCurrentGame(updateData,
                game => Debug.Log("[APITracker] Auto-save ejecutado"),
                error => Debug.LogWarning($"[APITracker] Error en auto-save: {error}")
            );
        }

        // ==========================================
        // UTILIDADES
        // ==========================================

        private int GetLevelTimeSeconds()
        {
            return Mathf.RoundToInt(Time.time - levelStartTime);
        }

        /// <summary>
        /// Obtiene la decisión buena por defecto para un nivel.
        /// </summary>
        /// <param name="level">Nivel actual (usar constantes de APIConstants.Levels).</param>
        /// <returns>Decisión buena por defecto, o "sanar" si no se reconoce el nivel.</returns>
        private string GetDefaultChoiceForLevel(string level)
        {
            switch (level)
            {
                case APIConstants.Levels.SENDA_EBANO:
                    return APIConstants.Choices.SANAR; // "sanar"
                case APIConstants.Levels.FORTALEZA_GIGANTES:
                    return APIConstants.Choices.CONSTRUIR; // "construir"
                case APIConstants.Levels.AQUELARRE_SOMBRAS:
                    return APIConstants.Choices.REVELAR; // "revelar"
                default:
                    return APIConstants.Choices.SANAR; // Fallback (nivel 4 o desconocido)
            }
        }
    }
}
