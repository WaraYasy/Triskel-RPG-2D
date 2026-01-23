namespace Triskel.API
{
    /// <summary>
    /// Constantes de la API de Triskel.
    /// Usa estas constantes para evitar errores de escritura.
    /// </summary>
    public static class APIConstants
    {
        // ==========================================
        // NIVELES
        // ==========================================
        public static class Levels
        {
            public const string HUB_CENTRAL = "hub_central";
            public const string SENDA_EBANO = "senda_ebano";
            public const string FORTALEZA_GIGANTES = "fortaleza_gigantes";
            public const string AQUELARRE_SOMBRAS = "aquelarre_sombras";
            public const string CLARO_ALMAS = "claro_almas";

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
        public static class GameStatus
        {
            public const string IN_PROGRESS = "in_progress";
            public const string COMPLETED = "completed";
            public const string ABANDONED = "abandoned";
        }

        // ==========================================
        // DECISIONES MORALES
        // ==========================================
        public static class Choices
        {
            // Senda Ebano
            public const string SANAR = "sanar";     // Buena
            public const string FORZAR = "forzar";   // Mala

            // Fortaleza Gigantes
            public const string CONSTRUIR = "construir"; // Buena
            public const string DESTRUIR = "destruir";   // Mala

            // Aquelarre Sombras
            public const string REVELAR = "revelar"; // Buena
            public const string OCULTAR = "ocultar"; // Mala

            /// <summary>
            /// Indica si una decision es "buena" (moral positiva).
            /// </summary>
            public static bool IsGoodChoice(string choice)
            {
                return choice == SANAR || choice == CONSTRUIR || choice == REVELAR;
            }
        }

        // ==========================================
        // RELIQUIAS
        // ==========================================
        public static class Relics
        {
            public const string LIRIO = "lirio";   // Senda Ebano
            public const string HACHA = "hacha";   // Fortaleza Gigantes
            public const string MANTO = "manto";   // Aquelarre Sombras

            public static readonly string[] All = new[] { LIRIO, HACHA, MANTO };
        }

        // ==========================================
        // TIPOS DE EVENTO
        // ==========================================
        public static class EventTypes
        {
            public const string PLAYER_DEATH = "player_death";
            public const string LEVEL_START = "level_start";
            public const string LEVEL_END = "level_end";
            public const string NPC_INTERACTION = "npc_interaction";
            public const string ITEM_COLLECTED = "item_collected";
            public const string CHECKPOINT_REACHED = "checkpoint_reached";
            public const string BOSS_ENCOUNTER = "boss_encounter";
            public const string CUSTOM_EVENT = "custom_event";
        }

        // ==========================================
        // CAUSAS DE MUERTE
        // ==========================================
        public static class DeathCauses
        {
            public const string FALL = "fall";
            public const string ENEMY_ATTACK = "enemy_attack";
            public const string TRAP = "trap";
            public const string BOSS = "boss";
            public const string ENVIRONMENTAL = "environmental";
        }

        // ==========================================
        // FINALES DEL JUEGO
        // ==========================================
        public static class Endings
        {
            /// <summary>
            /// Calcula el numero de final basado en decisiones buenas.
            /// 3 buenas = Final 1 (mejor), 0 buenas = Final 4 (peor)
            /// </summary>
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
        public static class Platforms
        {
            public const string WINDOWS = "windows";
            public const string ANDROID = "android";

            /// <summary>
            /// Obtiene la plataforma actual automaticamente.
            /// </summary>
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
