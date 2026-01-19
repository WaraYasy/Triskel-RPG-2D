using System;
using System.Collections.Generic;

namespace Triskel.API.Models
{
    // ==========================================
    // MODELOS DE EVENTOS (Event)
    // ==========================================

    /// <summary>
    /// Request para crear un evento.
    /// POST /v1/events
    /// </summary>
    [Serializable]
    public class CreateEventRequest
    {
        public string game_id;
        public string player_id;
        public string event_type;
        public string level;
        public EventData data;
    }

    /// <summary>
    /// Datos adicionales del evento (flexible).
    /// Usa los campos que necesites segun el tipo de evento.
    /// </summary>
    [Serializable]
    public class EventData
    {
        // Para player_death
        public string cause;
        public float position_x;
        public float position_y;
        public string enemy_type;

        // Para checkpoint_reached
        public string checkpoint_id;

        // Para item_collected
        public string item_type;
        public string relic_name;

        // Para npc_interaction
        public string npc_id;
        public string action;

        // Para boss_encounter
        public string boss_name;

        // Para custom_event
        public string event_name;
        public int ending_number;
    }

    /// <summary>
    /// Response de un evento creado.
    /// </summary>
    [Serializable]
    public class GameEvent
    {
        public string event_id;
        public string game_id;
        public string player_id;
        public string timestamp;
        public string event_type;
        public string level;
        public EventData data;
    }

    /// <summary>
    /// Request para crear eventos en batch.
    /// POST /v1/events/batch
    /// </summary>
    [Serializable]
    public class BatchEventsRequest
    {
        public CreateEventRequest[] events;
    }

    /// <summary>
    /// Response de eventos creados en batch.
    /// </summary>
    [Serializable]
    public class BatchEventsResponse
    {
        public int created;
        public GameEvent[] events;
    }

    /// <summary>
    /// Wrapper para deserializar arrays de eventos.
    /// </summary>
    [Serializable]
    public class EventArrayWrapper
    {
        public GameEvent[] events;
    }
}
