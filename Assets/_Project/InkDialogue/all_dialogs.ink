// ===================================================================
// SISTEMA DE DIÁLOGO DE TRISKEL CON INK
// ===================================================================
// Este archivo contiene ejemplos de diálogos usando Ink.
// Demuestra: variables, opciones, funciones externas, y flujo.
// ===================================================================

// FUNCIONES EXTERNAS
// Estas funciones tienen implementaciones placeholder en Ink (para testing)
// Unity las sobrescribirá con las funciones reales usando BindExternalFunction
=== function ModifyMoral(delta) ===
// Esta función será sobrescrita por Unity
~ return

=== function UnlockDiaryEntry(entryId) ===
// Esta función será sobrescrita por Unity
~ return

=== function AddItem(itemName) ===
// Esta función será sobrescrita por Unity
~ return

=== function Log(message) ===
// Esta función será sobrescrita por Unity
~ return

// VARIABLES GLOBALES
// Estas variables se pueden leer/modificar desde Unity
VAR has_talked_to_npc = false
VAR player_moral = 0
VAR quest_started = false

// ===================================================================
// KNOT: npc
// Diálogo básico con un NPC
// Para iniciar: GameManager.Instance.dialogueEvents.EnterDialogue("npc")
// ===================================================================
=== npc ===
{not has_talked_to_npc:
    ¡Hola, viajero! Es la primera vez que te veo por aquí.
    ~ has_talked_to_npc = true
- else:
    Ah, has vuelto. ¿En qué puedo ayudarte?
}

+ [Cuéntame sobre este lugar]
    -> about_place
+ [¿Tienes alguna misión para mí?]
    -> quest_offer
+ [Adiós]
    -> farewell

// ===================================================================
// KNOT: about_place
// Información sobre el lugar
// ===================================================================
=== about_place ===
Este bosque ha sido mi hogar durante años. Es un lugar mágico, pero también peligroso.

Hay criaturas que acechan en las sombras...

+ [¿Qué tipo de criaturas?]
    Lobos oscuros, principalmente. Se vuelven más agresivos por la noche.
    -> npc
+ [Interesante...]
    -> npc

// ===================================================================
// KNOT: quest_offer
// Ofrece una misión al jugador con ramificación moral
// ===================================================================
=== quest_offer ===
{quest_started:
    Ya te di una misión. ¿La has completado?
    -> npc
}

~ quest_started = true

Necesito que encuentres tres hierbas medicinales raras en el bosque.

Son difíciles de encontrar, pero te pagaré bien.

* [Acepto la misión]
    ~ Log("Misión aceptada!")
    Excelente. Ve al bosque profundo y busca las hierbas brillantes.
    ~ player_moral = player_moral + 1
    ~ ModifyMoral(1)
    -> npc
* [No me interesa]
    ~ player_moral = player_moral - 1
    ~ ModifyMoral(-1)
    Vaya... esperaba que me ayudaras.
    -> npc

// ===================================================================
// KNOT: merchant
// Ejemplo de un mercader con opciones de compra
// ===================================================================
=== merchant ===
# speaker:Merchant
# emotion:happy

¡Bienvenido a mi tienda, viajero!

Tengo los mejores artículos de toda la región.

+ [¿Qué vendes?]
    -> merchant_items
+ [Quiero comprar algo]
    -> merchant_buy
+ [Adiós]
    ¡Vuelve pronto!
    -> END

=== merchant_items ===
Vendo pociones, armas, y mapas antiguos.

Todo de la mejor calidad, ¡te lo aseguro!

-> merchant

=== merchant_buy ===
¿Qué te interesa?

+ [Poción de vida (10 monedas)]
    ~ AddItem("health_potion")
    ¡Excelente elección! Aquí tienes tu poción.
    -> merchant
+ [Mapa del bosque (25 monedas)]
    ~ AddItem("forest_map")
    ~ UnlockDiaryEntry("map_entry")
    Este mapa te será muy útil. ¡He añadido notas a tu diario!
    -> merchant
+ [Nada por ahora]
    No hay problema. Avísame si cambias de opinión.
    -> merchant

// ===================================================================
// KNOT: farewell
// Despedida genérica
// ===================================================================
=== farewell ===
¡Que tengas un buen viaje!
-> END

// ===================================================================
// KNOT: test_dialogue
// Diálogo de prueba con todas las características
// ===================================================================
=== test_dialogue ===
# speaker:TestNPC
# emotion:neutral

Este es un diálogo de prueba para demostrar todas las características.

Variables actuales:
- has_talked_to_npc: {has_talked_to_npc}
- player_moral: {player_moral}
- quest_started: {quest_started}

+ [Modificar moral (+1)]
    ~ ModifyMoral(1)
    ¡Moral aumentada!
    -> test_dialogue
+ [Añadir item de prueba]
    ~ AddItem("test_item")
    Item añadido al inventario.
    -> test_dialogue
+ [Desbloquear entrada del diario]
    ~ UnlockDiaryEntry("test_entry")
    Entrada del diario desbloqueada.
    -> test_dialogue
+ [Salir]
    -> END