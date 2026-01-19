# Cómo Cerrar el Ink Player Window

Si ves errores de "Missing function binding" en la consola, es porque la ventana **Ink Player** está abierta.

## Pasos para cerrar:

1. En Unity, busca una pestaña llamada **"Ink Player"** (normalmente está junto a las pestañas Console, Project, etc.)
2. Haz clic derecho en la pestaña
3. Selecciona **"Close Tab"**

O simplemente:
- Busca la ventana que dice "Ink Player" en la parte superior
- Haz clic en la X para cerrarla

## ¿Por qué ocurre este error?

El **Ink Player** es una herramienta de testing del editor que ejecuta archivos `.ink` **sin Unity en ejecución**. Por lo tanto, no tiene acceso a las funciones de C# que has creado en `InkExternalFunctions.cs`.

## Para probar tus diálogos correctamente:

**NO uses el Ink Player** - En su lugar:

1. Ejecuta el juego en Unity (botón Play ▶)
2. Llama al diálogo desde código:
   ```csharp
   GameManager.Instance.dialogueEvents.EnterDialogue("npc");
   ```
3. Las funciones externas funcionarán correctamente

Las funciones placeholder en el archivo `.ink` permiten que compile sin errores, pero las funciones reales solo funcionan cuando ejecutas el juego.
