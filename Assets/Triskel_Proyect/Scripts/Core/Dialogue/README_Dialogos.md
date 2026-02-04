# Sistema de Diálogos

## Por Posición (DialogueZone)

Usa `DialogueZone` para activar diálogos cuando el player entra en un área.

### Requisitos:
- El jugador debe tener el tag **Player** (exactamente así, con mayúscula)
- Debe existir un **DialogueRunner** en la escena

### Setup:
1. **Hierarchy** → Click derecho → **Create Empty**
2. Renombrar el objeto (ej: `Zona_Intro`)
3. **Add Component** → **Box Collider 2D** → Marcar **Is Trigger**
4. Ajustar el tamaño del collider en **Size** para cubrir el área deseada
5. **Add Component** → **DialogueZone**
6. Configurar en el Inspector:
   - `Nodo Dialogo`: nombre del nodo en el archivo .yarn (ej: `Nivel1_Intro`)
   - `Dialogue Runner`: arrastrar el DialogueRunner de la escena
   - `Solo Una Vez`: activar si no quieres que se repita

### Notas:
- Cada zona es independiente (puedes crear múltiples zonas)
- Si `Solo Una Vez` está activado, el diálogo solo se muestra la primera vez
- Si ya hay un diálogo en curso, no se interrumpirá
- El campo `Ya Usado` en el Inspector muestra si la zona ya fue activada

### Depuración:
Si tienes problemas, descomenta los `Debug.Log` en `DialogueZone.cs` para ver logs en la Console.

---

## Por Evento (desde código)

Llama al diálogo desde cualquier script cuando ocurra algo.

### Ejemplo:
```csharp
using Yarn.Unity;

public class MiScript : MonoBehaviour
{
    public DialogueRunner dialogueRunner;

    // Llamar cuando quieras iniciar el diálogo
    public void IniciarDialogo()
    {
        dialogueRunner.StartDialogue("Nivel1_Lirio");
    }
}
```

### Casos de uso:
- Al recoger un item → `OnCollect()` llama `StartDialogue("Item_Recogido")`
- Al derrotar enemigo → `OnEnemyDeath()` llama `StartDialogue("Victoria")`
- Al pulsar tecla → `if (Input.GetKeyDown(KeyCode.E))` llama `StartDialogue("NPC_Habla")`

---

## Nodos disponibles (Level1.yarn)

| Nodo | Uso |
|------|-----|
| `Nivel1_Intro` | Entrada al nivel |
| `Nivel1_Lirio` | Encontrar el Lirio |
| `Nivel1_Fantasmas` | Ver los Olvidados |
| `Nivel1_Decision` | Elección del jugador |
| `Ebano_Frases_01-05` | Frases ambientales |
