// =======================================================================================
// Triskel RPG 2D - Player Models
// =======================================================================================
// Autor: Mandrágora - Wara Pacheco
// Descripción: Modelos de datos (DTOs) para operaciones relacionadas con jugadores:
//              registro, login, perfil, estadísticas. Estos modelos se serializan/
//              deserializan con JsonUtility para comunicación con la API REST.
// =======================================================================================

using System;

namespace Triskel.API.Models
{
    // ==========================================
    // MODELOS DE JUGADOR (Player)
    // ==========================================

    /// <summary>
    /// Request para crear un nuevo jugador.
    /// </summary>
    /// <remarks>
    /// Endpoint: POST /v1/players
    /// </remarks>
    [Serializable]
    public class CreatePlayerRequest
    {
        /// <summary>Nombre de usuario (3-20 caracteres).</summary>
        public string username;
        /// <summary>Contraseña (6-100 caracteres, requerido).</summary>
        public string password;
        /// <summary>Email del jugador (opcional).</summary>
        public string email;
    }

    /// <summary>
    /// Response al crear un jugador exitosamente.
    /// </summary>
    /// <remarks>
    /// IMPORTANTE: El player_token debe guardarse localmente (PlayerPrefs) para
    /// autenticación en futuras peticiones.
    /// </remarks>
    [Serializable]
    public class CreatePlayerResponse
    {
        /// <summary>ID único del jugador creado.</summary>
        public string player_id;
        /// <summary>Nombre de usuario.</summary>
        public string username;
        /// <summary>Token de autenticación (IMPORTANTE: guardar localmente).</summary>
        public string player_token;
    }

    /// <summary>
    /// Request para login.
    /// POST /v1/players/login
    /// </summary>
    [Serializable]
    public class LoginRequest
    {
        public string username;
        public string password;
    }

    /// <summary>
    /// Response de login exitoso.
    /// </summary>
    /// <remarks>
    /// Si el jugador tiene una partida activa (status = "in_progress"), el campo
    /// active_game_id contendrá el ID de esa partida para poder continuarla.
    /// </remarks>
    [Serializable]
    public class LoginResponse
    {
        /// <summary>ID único del jugador.</summary>
        public string player_id;
        /// <summary>Nombre de usuario.</summary>
        public string username;
        /// <summary>Token de autenticación (IMPORTANTE: guardar localmente).</summary>
        public string player_token;
        /// <summary>ID de la partida activa (null si no hay partida en progreso).</summary>
        public string active_game_id;
    }

    /// <summary>
    /// Perfil completo del jugador con estadísticas agregadas.
    /// </summary>
    /// <remarks>
    /// Endpoint: GET /v1/players/me
    /// </remarks>
    [Serializable]
    public class PlayerProfile
    {
        /// <summary>ID único del jugador.</summary>
        public string player_id;
        /// <summary>Nombre de usuario.</summary>
        public string username;
        /// <summary>Email del jugador.</summary>
        public string email;
        /// <summary>Fecha de creación de la cuenta (formato ISO 8601).</summary>
        public string created_at;
        /// <summary>Fecha del último login (formato ISO 8601).</summary>
        public string last_login;
        /// <summary>Tiempo total jugado en todas las partidas (en segundos).</summary>
        public int total_playtime_seconds;
        /// <summary>Número total de partidas jugadas.</summary>
        public int games_played;
        /// <summary>Número total de partidas completadas.</summary>
        public int games_completed;
        /// <summary>Estadísticas agregadas del jugador.</summary>
        public PlayerStats stats;
    }

    /// <summary>
    /// Estadísticas agregadas de todas las partidas de un jugador.
    /// </summary>
    [Serializable]
    public class PlayerStats
    {
        /// <summary>Total de decisiones buenas (morales positivas) tomadas.</summary>
        public int total_good_choices;
        /// <summary>Total de decisiones malas (morales negativas) tomadas.</summary>
        public int total_bad_choices;
        /// <summary>Total de muertes en todas las partidas.</summary>
        public int total_deaths;
        /// <summary>Reliquia favorita (más usada), puede ser null.</summary>
        public string favorite_relic;
        /// <summary>Mejor tiempo (speedrun) en segundos, 0 si no hay registro.</summary>
        public int best_speedrun_seconds;
        /// <summary>Alineación moral global (-1.0 = malo, 0.0 = neutral, 1.0 = bueno).</summary>
        public float moral_alignment;
    }

    /// <summary>
    /// Request para actualizar datos del jugador.
    /// PATCH /v1/players/{player_id}
    /// </summary>
    [Serializable]
    public class UpdatePlayerRequest
    {
        public string username;
        public string email;
        public int total_playtime_seconds;
        public int games_played;
        public int games_completed;
    }
}
