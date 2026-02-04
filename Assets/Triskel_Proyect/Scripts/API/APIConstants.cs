// =======================================================================================
// Triskel RPG 2D - API Constants
// =======================================================================================
// Autor: Mandrágora - Wara Pacheco
// Descripción: Constantes centralizadas para la API REST de Triskel. Incluye nombres de
//              niveles, estados de partida, decisiones morales, reliquias, tipos de
//              eventos, causas de muerte y cálculo de finales. Usar estas constantes
//              previene errores de escritura y facilita el mantenimiento.
// =======================================================================================

namespace Triskel.API
{
    /// <summary>
    /// Constantes centralizadas de la API de Triskel.
    /// </summary>
    /// <remarks>
    /// Usa estas constantes en lugar de strings literales para evitar errores de escritura
    /// y facilitar refactorizaciones futuras.
    /// </remarks>
    public static class APIConstants
    {
        // ==========================================
        // NIVELES
        // ==========================================
        /// <summary>
        /// Constantes de nombres de niveles del juego.
        /// </summary>
        public static class Levels
        {
            /// <summary>Hub central (área inicial).</summary>
            public const string HUB_CENTRAL = "hub_central";
            /// <summary>Nivel 1: Senda del Ébano.</summary>
            public const string SENDA_EBANO = "senda_ebano";
            /// <summary>Nivel 2: Fortaleza de los Gigantes.</summary>
            public const string FORTALEZA_GIGANTES = "fortaleza_gigantes";
            /// <summary>Nivel 3: Aquelarre de las Sombras.</summary>
            public const string AQUELARRE_SOMBRAS = "aquelarre_sombras";
            /// <summary>Nivel 4/Final: Claro de las Almas.</summary>
            public const string CLARO_ALMAS = "claro_almas";

            /// <summary>
            /// Array con todos los niveles del juego en orden.
            /// </summary>
            public static readonly string[] All = new[]
            {
                HUB_CENTRAL,
                SENDA_EBANO,
                FORTALEZA_GIGANTES,
                AQUELARRE_SOMBRAS,
                CLARO_ALMAS
            };
        }

        // ==========================================
        // ESTADOS DE PARTIDA
        // ==========================================
        /// <summary>
        /// Constantes de estados de una partida.
        /// </summary>
        public static class GameStatus
        {
            /// <summary>Partida en progreso (activa).</summary>
            public const string IN_PROGRESS = "in_progress";
            /// <summary>Partida completada (jefe final derrotado).</summary>
            public const string COMPLETED = "completed";
            /// <summary>Partida abandonada (jugador dejó de jugar).</summary>
            public const string ABANDONED = "abandoned";
        }

        // ==========================================
        // DECISIONES MORALES
        // ==========================================
        /// <summary>
        /// Constantes de decisiones morales por nivel.
        /// </summary>
        public static class Choices
        {
            // Senda Ebano
            /// <summary>Senda del Ébano: Decisión buena (sanar al espíritu).</summary>
            public const string SANAR = "sanar";
            /// <summary>Senda del Ébano: Decisión mala (forzar al espíritu).</summary>
            public const string FORZAR = "forzar";

            // Fortaleza Gigantes
            /// <summary>Fortaleza de los Gigantes: Decisión buena (construir el puente).</summary>
            public const string CONSTRUIR = "construir";
            /// <summary>Fortaleza de los Gigantes: Decisión mala (destruir el puente).</summary>
            public const string DESTRUIR = "destruir";

            // Aquelarre Sombras
            /// <summary>Aquelarre de las Sombras: Decisión buena (revelar la verdad).</summary>
            public const string REVELAR = "revelar";
            /// <summary>Aquelarre de las Sombras: Decisión mala (ocultar la verdad).</summary>
            public const string OCULTAR = "ocultar";

            /// <summary>
            /// Indica si una decisión es "buena" (moral positiva).
            /// </summary>
            /// <param name="choice">Decisión a evaluar.</param>
            /// <returns>True si la decisión es buena (sanar, construir, revelar).</returns>
            /// <remarks>
            /// TODO: Integrar con sistema de decisiones morales basado en acciones de gameplay.
            /// Actualmente hardcodeado. En el futuro, considerar:
            /// - Llamar desde scripts de objetos interactuables cuando el jugador tome una acción
            /// - Hacer configurable (JSON/ScriptableObject) en lugar de hardcoded
            /// - Registrar automáticamente en GameManager.RegistrarDecisionMoral()
            /// </remarks>
            public static bool IsGoodChoice(string choice)
            {
                return choice == SANAR || choice == CONSTRUIR || choice == REVELAR;
            }
        }

        // ==========================================
        // RELIQUIAS
        // ==========================================
        /// <summary>
        /// Constantes de reliquias mágicas obtenibles en el juego.
        /// </summary>
        public static class Relics
        {
            /// <summary>Lirio de Luz (obtenido en Senda del Ébano).</summary>
            public const string LIRIO = "lirio";
            /// <summary>Hacha Ancestral (obtenida en Fortaleza de los Gigantes).</summary>
            public const string HACHA = "hacha";
            /// <summary>Manto de Sombras (obtenido en Aquelarre de las Sombras).</summary>
            public const string MANTO = "manto";

            /// <summary>
            /// Array con todas las reliquias del juego.
            /// </summary>
            public static readonly string[] All = new[] { LIRIO, HACHA, MANTO };
        }

        // ==========================================
        // TIPOS DE EVENTO
        // ==========================================
        /// <summary>
        /// Constantes de tipos de eventos de gameplay.
        /// </summary>
        public static class EventTypes
        {
            /// <summary>Muerte del jugador.</summary>
            public const string PLAYER_DEATH = "player_death";
            /// <summary>Inicio de un nivel.</summary>
            public const string LEVEL_START = "level_start";
            /// <summary>Fin de un nivel.</summary>
            public const string LEVEL_END = "level_end";
            /// <summary>Interacción con NPC.</summary>
            public const string NPC_INTERACTION = "npc_interaction";
            /// <summary>Recogida de ítem o reliquia.</summary>
            public const string ITEM_COLLECTED = "item_collected";
            /// <summary>Llegada a checkpoint.</summary>
            public const string CHECKPOINT_REACHED = "checkpoint_reached";
            /// <summary>Encuentro con jefe.</summary>
            public const string BOSS_ENCOUNTER = "boss_encounter";
            /// <summary>Evento personalizado/custom.</summary>
            public const string CUSTOM_EVENT = "custom_event";
        }

        // ==========================================
        // CAUSAS DE MUERTE
        // ==========================================
        /// <summary>
        /// Constantes de causas de muerte del jugador.
        /// </summary>
        public static class DeathCauses
        {
            /// <summary>Muerte por caída.</summary>
            public const string FALL = "fall";
            /// <summary>Muerte por ataque de enemigo.</summary>
            public const string ENEMY_ATTACK = "enemy_attack";
            /// <summary>Muerte por trampa.</summary>
            public const string TRAP = "trap";
            /// <summary>Muerte por jefe.</summary>
            public const string BOSS = "boss";
            /// <summary>Muerte por peligro ambiental.</summary>
            public const string ENVIRONMENTAL = "environmental";
        }

        // ==========================================
        // FINALES DEL JUEGO
        // ==========================================
        /// <summary>
        /// Constantes y utilidades para calcular el final del juego.
        /// </summary>
        public static class Endings
        {
            /// <summary>
            /// Calcula el número de final basado en las decisiones buenas tomadas.
            /// </summary>
            /// <param name="goodChoicesCount">Cantidad de decisiones buenas (0-3).</param>
            /// <returns>Número de final (1-4), donde 1 es el mejor y 4 el peor.</returns>
            /// <remarks>
            /// - 3 decisiones buenas = Final 1 (mejor)
            /// - 2 decisiones buenas = Final 2
            /// - 1 decisión buena = Final 3
            /// - 0 decisiones buenas = Final 4 (peor)
            /// </remarks>
            public static int CalculateEnding(int goodChoicesCount)
            {
                return goodChoicesCount switch
                {
                    3 => 1,
                    2 => 2,
                    1 => 3,
                    _ => 4
                };
            }
        }

        // ==========================================
        // PLATAFORMAS
        // ==========================================
        /// <summary>
        /// Constantes y utilidades de plataformas soportadas.
        /// </summary>
        public static class Platforms
        {
            /// <summary>Plataforma Windows/PC.</summary>
            public const string WINDOWS = "windows";
            /// <summary>Plataforma Android/Móvil.</summary>
            public const string ANDROID = "android";

            /// <summary>
            /// Obtiene la plataforma actual automáticamente según la compilación.
            /// </summary>
            /// <returns>"android" si se compila para Android, "windows" en caso contrario.</returns>
            public static string GetCurrentPlatform()
            {
#if UNITY_ANDROID
                return ANDROID;
#else
                return WINDOWS;
#endif
            }
        }
    }
}
