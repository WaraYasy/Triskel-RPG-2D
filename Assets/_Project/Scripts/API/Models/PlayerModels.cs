using System;

namespace Triskel.API.Models
{
    // ==========================================
    // MODELOS DE JUGADOR (Player)
    // ==========================================

    /// <summary>
    /// Request para crear un nuevo jugador.
    /// POST /v1/players
    /// </summary>
    [Serializable]
    public class CreatePlayerRequest
    {
        public string username;
        public string email; // Opcional
    }

    /// <summary>
    /// Response al crear un jugador.
    /// Contiene el token que DEBE guardarse localmente.
    /// </summary>
    [Serializable]
    public class CreatePlayerResponse
    {
        public string player_id;
        public string username;
        public string player_token; // IMPORTANTE: Guardar este token
    }

    /// <summary>
    /// Perfil completo del jugador.
    /// GET /v1/players/me
    /// </summary>
    [Serializable]
    public class PlayerProfile
    {
        public string player_id;
        public string username;
        public string email;
        public string created_at;
        public string last_login;
        public int total_playtime_seconds;
        public int games_played;
        public int games_completed;
        public PlayerStats stats;
    }

    /// <summary>
    /// Estadisticas agregadas del jugador.
    /// </summary>
    [Serializable]
    public class PlayerStats
    {
        public int total_good_choices;
        public int total_bad_choices;
        public int total_deaths;
        public string favorite_relic;      // Puede ser null
        public int best_speedrun_seconds;  // Puede ser 0 si no hay
        public float moral_alignment;      // -1.0 a 1.0
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
