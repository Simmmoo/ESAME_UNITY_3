using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI; // Usato per il prototipo visivo

public class FireworksManager : MonoBehaviour
{
    public enum FireworkState { Attesa, Lancio, Esplosione, Dissolvenza }

    [Header("Timers (in secondi)")]
    public float tempoTraFuochi = 10f;  // Ogni quanto parte un fuoco
    public float durataLancio = 3f;      // Tempo per correre ai ripari
    public float durataEsplosione = 1.5f;// Finestra di pericolo mortale
    public float durataDissolvenza = 2f; // Tempo in cui l'esplosione sparisce

    [Header("Prototipo Visivo (UI Temporanea)")]
    public TextMeshProUGUI debugText; // Trascina qui un testo UI per vedere lo stato attuale

    [Header("Stato Attuale")]
    public FireworkState statoAttuale = FireworkState.Attesa;

    private bool playerIsSafe = false;
    private PlayerController playerScript;

    void Start()
    {
        playerScript = FindFirstObjectByType<PlayerController>();
        StartCoroutine(FireworksLoop());
    }

    IEnumerator FireworksLoop()
    {
        while (true)
        {
            // 1. FASE DI ATTESA
            statoAttuale = FireworkState.Attesa;
            UpdateDebugUI("Situazione Calma");
            yield return new WaitForSeconds(tempoTraFuochi);

            // 2. FASE DI LANCIO (IL FUOCO SALE)
            statoAttuale = FireworkState.Lancio;
            UpdateDebugUI("FUOCO LANCIATO");
            // [QUI IN FUTURO AVVII L'ANIMAZIONE DEL FUOCO CHE SALE]
            yield return new WaitForSeconds(durataLancio);

            // 3. FASE DI ESPLOSIONE (PERICOLO MORTALE)
            statoAttuale = FireworkState.Esplosione;
            UpdateDebugUI("BOOM!");
            // [QUI IN FUTURO AVVII L'ANIMAZIONE DELL'ESPLOSIONE]

            // Controlla istantaneamente se il player è al sicuro
            if (!playerIsSafe)
            {
                KillPlayer();
            }
            yield return new WaitForSeconds(durataEsplosione);

            // 4. FASE DI DISSOLVENZA
            statoAttuale = FireworkState.Dissolvenza;
            UpdateDebugUI("Il fumo si dissolve");
            yield return new WaitForSeconds(durataDissolvenza);
        }
    }

    // Funzioni per la Cuccia per cambiare lo stato del Player
    public void SetPlayerSafe(bool safe)
    {
        playerIsSafe = safe;

        // Se il giocatore esce dalla cuccia PROPRIO durante l'esplosione, muore all'istante
        if (!playerIsSafe && statoAttuale == FireworkState.Esplosione)
        {
            KillPlayer();
        }
    }

    private void KillPlayer()
    {
        if (playerScript != null)
        {
            Debug.Log("Il gatto è stato spaventato a morte dal rumore!");
            playerScript.Die();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.deathCount += 1;
                GameManager.Instance.RespawnPlayer();
            }
        }
    }

    private void UpdateDebugUI(string messaggio)
    {
        if (debugText != null) debugText.text = messaggio;
        Debug.Log("[Fireworks] " + messaggio);
    }
}