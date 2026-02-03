using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Yarn.Unity;
using Triskel.Core;

public class BossEndingDialogueManager : MonoBehaviour
{
    [Header("Referencias")]
    public BossManager bossManager;
    public DialogueRunner dialogueRunner;

    [Header("Nodos del Boss (Derrota)")]
    public string boss0_Malo = "Final_Boss_Defeated_0";
    public string boss1_Agridulce = "Final_Boss_Defeated_1";
    public string boss2_Bueno = "Final_Boss_Defeated_2";
    public string boss3_Perfecto = "Final_Boss_Defeated_3";

    [Header("Nodos de Historia (Final)")]
    public string story0_Usurpador = "Final_Ending_0_Usurpador";
    public string story1_Ermitano = "Final_Ending_1_Ermitano";
    public string story2_Cicatrizado = "Final_Ending_2_Cicatrizado";
    public string story3_Leyenda = "Final_Ending_3_Leyenda";

    [Header("UI Transition")]
    public GameObject blackScreenPanel;
    public Image finalArtDisplay;

    [Header("Sprites de Final")]
    public Sprite art0_Malo;
    public Sprite art1_Agridulce;
    public Sprite art2_Bueno;
    public Sprite art3_Perfecto;

    private int finalMoralState = 0;

    private void Awake()
    {
        // Auto-encontrar referencias si faltan (Fallback de seguridad)
        if (bossManager == null)
            bossManager = FindFirstObjectByType<BossManager>();
            
        if (dialogueRunner == null)
            dialogueRunner = FindFirstObjectByType<DialogueRunner>();

        if (blackScreenPanel == null)
            blackScreenPanel = GameObject.Find("BlackScreenPanel");
            
        if (finalArtDisplay == null)
        {
             GameObject imgObj = GameObject.Find("FinalArtDisplay");
             if (imgObj != null) finalArtDisplay = imgObj.GetComponent<Image>();
        }
    }

    private void OnEnable()
    {
        if (bossManager != null)
            bossManager.OnVictory.AddListener(OnBossDefeated);
    }

    private void OnBossDefeated()
    {
        if (GameManager.Instance == null || dialogueRunner == null) return;

        // Determinar el estado segun la moral para toda la secuencia
        int moral = GameManager.Instance.MoralScore;
        if (moral >= 3) finalMoralState = 3;
        else if (moral >= 1) finalMoralState = 2;
        else if (moral >= -1) finalMoralState = 1;
        else finalMoralState = 0;

        string bossNode = GetBossNodeByState(finalMoralState);
        Debug.Log($"[BossEndingDialogueManager] Boss derrotado. Moral: {moral}. Habla Boss: {bossNode}");

        dialogueRunner.onDialogueComplete.AddListener(OnBossDialogueComplete);
        dialogueRunner.StartDialogue(bossNode);
    }

    private void OnBossDialogueComplete()
    {
        dialogueRunner.onDialogueComplete.RemoveListener(OnBossDialogueComplete);
        
        // ASIGNAR ARTE Y ACTIVAR PANTALLA NEGRA
        if (finalArtDisplay != null)
        {
            finalArtDisplay.sprite = GetArtByState(finalMoralState);
        }

        if (blackScreenPanel != null)
        {
            blackScreenPanel.SetActive(true);
        }

        Invoke("StartStoryDialogue", 2.0f); // Pausa dramática en negro
    }

    private void StartStoryDialogue()
    {
        string storyNode = GetStoryNodeByState(finalMoralState);
        Debug.Log($"[BossEndingDialogueManager] Iniciando epílogo: {storyNode}");
        
        dialogueRunner.onDialogueComplete.AddListener(OnStoryComplete);
        dialogueRunner.StartDialogue(storyNode);
    }

    private void OnStoryComplete()
    {
        dialogueRunner.onDialogueComplete.RemoveListener(OnStoryComplete);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.FinalizarJuego();
        }
    }

    private string GetBossNodeByState(int state)
    {
        return state switch { 3 => boss3_Perfecto, 2 => boss2_Bueno, 1 => boss1_Agridulce, _ => boss0_Malo };
    }

    private string GetStoryNodeByState(int state)
    {
        return state switch { 3 => story3_Leyenda, 2 => story2_Cicatrizado, 1 => story1_Ermitano, _ => story0_Usurpador };
    }

    private Sprite GetArtByState(int state)
    {
        return state switch { 3 => art3_Perfecto, 2 => art2_Bueno, 1 => art1_Agridulce, _ => art0_Malo };
    }
}
