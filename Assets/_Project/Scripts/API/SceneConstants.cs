// =======================================================================================
// Triskel RPG 2D - Scene Constants
// =======================================================================================
// Autor: Mandrágora - Wara Pacheco
// Descripción: Constantes de nombres de escenas de Unity y mapeo con niveles de la API.
//              Centraliza la conversión entre identificadores de API y nombres de escenas.
// =======================================================================================

namespace Triskel.API
{
    /// <summary>
    /// Constantes de nombres de escenas de Unity.
    /// </summary>
    /// <remarks>
    /// Esta clase proporciona:
    /// - Nombres constantes de todas las escenas del juego
    /// - Método de conversión de niveles API a nombres de escena Unity
    ///
    /// USO:
    /// - string sceneName = SceneConstants.HUB;
    /// - string sceneName = SceneConstants.GetSceneForAPILevel(APIConstants.Levels.SENDA_EBANO);
    /// </remarks>
    public static class SceneConstants
    {
        // Nombres de escenas de Unity
        public const string HUB = "DentroDelHub1";
        public const string LEVEL_1 = "Cuadrante1";
        public const string LEVEL_2 = "Cuadrante2";
        public const string LEVEL_3 = "Cuadrante3";
        public const string LEVEL_4 = "Cuadrante4";
        public const string MAIN_MENU = "Home";

        /// <summary>
        /// Convierte un nivel de la API a su nombre de escena correspondiente en Unity.
        /// </summary>
        /// <param name="apiLevel">Nivel desde la API (usar APIConstants.Levels).</param>
        /// <returns>Nombre de la escena Unity correspondiente, o HUB si no se reconoce el nivel.</returns>
        /// <remarks>
        /// Mapeo:
        /// - hub_central → DentroDelHub1
        /// - senda_ebano → Cuadrante1
        /// - fortaleza_gigantes → Cuadrante2
        /// - aquelarre_sombras → Cuadrante3
        /// - claro_almas → Cuadrante4
        /// </remarks>
        public static string GetSceneForAPILevel(string apiLevel)
        {
            switch (apiLevel)
            {
                case APIConstants.Levels.HUB_CENTRAL:
                    return HUB;
                case APIConstants.Levels.SENDA_EBANO:
                    return LEVEL_1;
                case APIConstants.Levels.FORTALEZA_GIGANTES:
                    return LEVEL_2;
                case APIConstants.Levels.AQUELARRE_SOMBRAS:
                    return LEVEL_3;
                case APIConstants.Levels.CLARO_ALMAS:
                    return LEVEL_4;
                default:
                    return HUB;
            }
        }
    }
}
