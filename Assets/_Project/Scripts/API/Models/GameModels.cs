// =======================================================================================
// Triskel RPG 2D - Game Models
// =======================================================================================
// Autor: Mandrágora - Wara Pacheco
// Descripción: Modelos de datos (DTOs) para operaciones relacionadas con partidas:
//              creación, actualización, completar niveles, métricas y decisiones.
//              Estos modelos se serializan/deserializan con JsonUtility para
//              comunicación con la API REST.
// =======================================================================================

using System;

namespace Triskel.API.Models
{
    // ==========================================
    // MODELOS DE PARTIDA (Game)
    // ==========================================

    /// <summary>
    /// Request para crear una nueva partida.
    /// POST /v1/games
    /// </summary>
    [Serializable]
    public class CreateGameRequest
    {
        public string player_id;
    }

    /// <summary>
    /// Datos completos de una partida.
    /// </summary>
    [Serializable]
    public class GameData
    {
        public string game_id;
        public string player_id;
        public string started_at;
        public string ended_at;           // null si activa
        public string status;             // "in_progress", "completed", "abandoned"
        public float completion_percentage;
        public int total_time_seconds;
        public string[] levels_completed;
        public string current_level;
        public GameChoices choices;
        public string[] relics;
        public bool boss_defeated;
        public string[] npcs_helped;
        public GameMetrics metrics;
    }

    /// <summary>
    /// Decisiones morales tomadas en cada nivel.
    /// </summary>
    [Serializable]
    public class GameChoices
    {
        public string senda_ebano;        // "sanar" | "forzar" | null
        public string fortaleza_gigantes; // "construir" | "destruir" | null
        public string aquelarre_sombras;  // "revelar" | "ocultar" | null
    }

    /// <summary>
    /// Metricas de gameplay por partida.
    /// </summary>
    [Serializable]
    public class GameMetrics
    {
        public int total_deaths;
        // Nota: time_per_level y deaths_per_level son objetos dinamicos
        // Unity JsonUtility no los soporta bien, se manejan como strings JSON raw si es necesario
    }

    /// <summary>
    /// Request para iniciar un nivel.
    /// POST /v1/games/{game_id}/level/start
    /// </summary>
    [Serializable]
    public class StartLevelRequest
    {
        public string level;
    }

    /// <summary>
    /// Request para completar un nivel.
    /// POST /v1/games/{game_id}/level/complete
    /// </summary>
    /// <remarks>
    /// NUEVO: time_seconds es opcional. Si se omite, la API lo calcula automáticamente
    /// usando el timestamp de /level/start y el timestamp actual.
    /// </remarks>
    [Serializable]
    public class CompleteLevelRequest
    {
        public string level;
        public int? time_seconds; // OPCIONAL: la API lo calcula automáticamente si es null
        public int deaths;
        public string choice; // Opcional: decision moral
        public string relic;  // Opcional: reliquia obtenida
    }

    /// <summary>
    /// Request para actualizar una partida.
    /// PATCH /v1/games/{game_id}
    /// </summary>
    [Serializable]
    public class UpdateGameRequest
    {
        public string status;              // "in_progress", "completed", "abandoned"
        public string ended_at;
        public float completion_percentage;
        public int total_time_seconds;
        public string current_level;
        public bool boss_defeated;
        public string[] relics;
    }

    /// <summary>
    /// Wrapper para deserializar arrays de partidas.
    /// Unity JsonUtility no soporta arrays directos.
    /// </summary>
    [Serializable]
    public class GameArrayWrapper
    {
        public GameData[] games;
    }
}
