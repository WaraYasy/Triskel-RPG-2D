// =======================================================================================
// Triskel RPG 2D - Level Mapper
// =======================================================================================
// Autor: Mandrágora - Wara Pacheco
// Descripción: Utilidad estática para convertir nombres de escenas Unity a constantes
//              de la API REST. Mapea escenas como "Cuadrante1" → "senda_ebano" y valida
//              si una escena es un nivel jugable.
// =======================================================================================

using UnityEngine.SceneManagement;

namespace Triskel.API
{
    /// <summary>
    /// Utilidad estática para mapear nombres de escenas Unity a constantes API.
    /// </summary>
    /// <remarks>
    /// Centraliza la lógica de conversión de escenas a niveles API para evitar
    /// duplicación de código y facilitar mantenimiento.
    ///
    /// COMO USAR:
    /// <code>
    /// string apiLevel = LevelMapper.SceneToAPILevel("Cuadrante1");
    /// // apiLevel = "senda_ebano"
    ///
    /// bool isLevel = LevelMapper.IsPlayableLevel("DentroDelHub1");
    /// // isLevel = false
    /// </code>
    /// </remarks>
    public static class LevelMapper
    {
        /// <summary>
        /// Convierte un nombre de escena Unity a su constante API correspondiente.
        /// </summary>
        /// <param name="sceneName">Nombre de la escena (ej: "Cuadrante1", "Hub").</param>
        /// <returns>Constante API del nivel (ej: "senda_ebano", "hub_central"), o cadena vacía si no es válido.</returns>
        /// <remarks>
        /// Mapeos soportados:
        /// - Cuadrante1 → senda_ebano
        /// - Cuadrante2 → fortaleza_gigantes
        /// - Cuadrante3 → aquelarre_sombras
        /// - Cuadrante4 → claro_almas
        /// - DentroDelHub1/Hub → hub_central
        /// </remarks>
        public static string SceneToAPILevel(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
                return "";

            // Normalizar nombre (quitar espacios, convertir a minúsculas)
            sceneName = sceneName.Trim().ToLower();

            // Mapeo de escenas a niveles API
            switch (sceneName)
            {
                // Niveles jugables
                case "cuadrante1":
                    return APIConstants.Levels.SENDA_EBANO;
                case "cuadrante2":
                    return APIConstants.Levels.FORTALEZA_GIGANTES;
                case "cuadrante3":
                    return APIConstants.Levels.AQUELARRE_SOMBRAS;
                case "cuadrante4":
                    return APIConstants.Levels.CLARO_ALMAS;

                // Hub (no jugable)
                case "dentrodelhub1":
                case "hub":
                    return APIConstants.Levels.HUB_CENTRAL;

                // Escena no reconocida
                default:
                    return "";
            }
        }

        /// <summary>
        /// Convierte un índice de nivel (1-4) a su constante API correspondiente.
        /// </summary>
        /// <param name="levelIndex">Índice del nivel (1-4).</param>
        /// <returns>Constante API del nivel (ej: "senda_ebano"), o cadena vacía si es inválido.</returns>
        /// <remarks>
        /// Mapeos:
        /// - 1 → senda_ebano
        /// - 2 → fortaleza_gigantes
        /// - 3 → aquelarre_sombras
        /// - 4 → claro_almas
        /// </remarks>
        public static string LevelIndexToAPILevel(int levelIndex)
        {
            switch (levelIndex)
            {
                case 1:
                    return APIConstants.Levels.SENDA_EBANO;
                case 2:
                    return APIConstants.Levels.FORTALEZA_GIGANTES;
                case 3:
                    return APIConstants.Levels.AQUELARRE_SOMBRAS;
                case 4:
                    return APIConstants.Levels.CLARO_ALMAS;
                default:
                    return "";
            }
        }

        /// <summary>
        /// Valida si una escena es un nivel jugable (no hub ni transiciones).
        /// </summary>
        /// <param name="sceneName">Nombre de la escena a validar.</param>
        /// <returns>True si es un nivel jugable (Cuadrante1-4), False si es hub o escena especial.</returns>
        /// <remarks>
        /// Niveles jugables: Cuadrante1, Cuadrante2, Cuadrante3, Cuadrante4.
        /// NO jugables: Hub, DentroDelHub1, LevelTransition, etc.
        /// </remarks>
        public static bool IsPlayableLevel(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
                return false;

            sceneName = sceneName.Trim().ToLower();

            // Solo los Cuadrantes son niveles jugables
            return sceneName.StartsWith("cuadrante");
        }

        /// <summary>
        /// Obtiene el nivel API de la escena activa actual.
        /// </summary>
        /// <returns>Constante API del nivel actual, o cadena vacía si no es nivel válido.</returns>
        /// <remarks>
        /// Método de conveniencia para obtener el nivel actual sin pasar parámetros.
        /// Internamente usa SceneManager.GetActiveScene().name.
        /// </remarks>
        public static string GetCurrentAPILevel()
        {
            string currentScene = SceneManager.GetActiveScene().name;
            return SceneToAPILevel(currentScene);
        }
    }
}
