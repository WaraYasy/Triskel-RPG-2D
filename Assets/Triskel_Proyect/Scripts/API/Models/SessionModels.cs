// =======================================================================================
// Triskel RPG 2D - Session Models
// =======================================================================================
// Autor: Mandrágora - Wara Pacheco
// Descripción: Modelos de datos (DTOs) para operaciones relacionadas con sesiones de
//              juego: iniciar sesión, terminar sesión y tracking de tiempo. Las sesiones
//              representan períodos continuos de tiempo jugando una partida.
// =======================================================================================

using System;

namespace Triskel.API.Models
{
    // ==========================================
    // MODELOS DE SESION (Session)
    // ==========================================

    /// <summary>
    /// Request para iniciar una sesion de juego.
    /// POST /v1/sessions
    /// </summary>
    [Serializable]
    public class CreateSessionRequest
    {
        public string game_id;
        public string platform; // "windows" o "android"
    }

    /// <summary>
    /// Datos de una sesion de juego.
    /// Representa un periodo continuo de tiempo jugando.
    /// </summary>
    [Serializable]
    public class SessionData
    {
        public string session_id;
        public string player_id;
        public string game_id;
        public string started_at;
        public string ended_at;        // null si la sesion esta activa
        public int duration_seconds;
        public string platform;        // "windows" o "android"
        public bool is_active;
    }

    /// <summary>
    /// Wrapper para deserializar arrays de sesiones.
    /// Unity JsonUtility no soporta arrays directos.
    /// </summary>
    [Serializable]
    public class SessionArrayWrapper
    {
        public SessionData[] sessions;
    }
}
