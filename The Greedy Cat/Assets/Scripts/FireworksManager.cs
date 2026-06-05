using System.Collections;
using TMPro;
using UnityEngine;

public class FireworksManager : MonoBehaviour
{
    public enum FireworkState { Attesa, Lancio, Esplosione, Dissolvenza }

    [Header("Timers (in secondi)")]
    public float tempoTraFuochi = 10f;
    public float durataLancio = 3f;
    public float durataEsplosione = 1.5f;
    public float durataDissolvenza = 2f;

    [Header("Prototipo Visivo (UI Temporanea)")]
    public TextMeshProUGUI debugText;

    [Header("Stato Attuale")]
    public FireworkState statoAttuale = FireworkState.Attesa;

    private bool playerIsSafe = false;
    private PlayerController playerScript;

    void Start()
    {
        playerScript = FindFirstObjectByType<PlayerController>();
        StartCoroutine(FireworksLoop());
    }

    public void AggiornaRiferimentoPlayer(PlayerController nuovoPlayer)
    {
        playerScript = nuovoPlayer;
    }

    IEnumerator FireworksLoop()
    {
        while (true)
        {
            // 1. FASE DI ATTESA
            statoAttuale = FireworkState.Attesa;
            UpdateDebugUI("Situazione Calma");
            yield return new WaitForSeconds(tempoTraFuochi);

            // 2. FASE DI LANCIO
            statoAttuale = FireworkState.Lancio;
            UpdateDebugUI("FUOCO LANCIATO");
            yield return new WaitForSeconds(durataLancio);

            // 3. FASE DI ESPLOSIONE
            statoAttuale = FireworkState.Esplosione;
            UpdateDebugUI("BOOM!");

            if (!playerIsSafe)
                KillPlayer();

            yield return new WaitForSeconds(durataEsplosione);

            // 4. FASE DI DISSOLVENZA
            statoAttuale = FireworkState.Dissolvenza;
            UpdateDebugUI("Il fumo si dissolve");
            yield return new WaitForSeconds(durataDissolvenza);
        }
    }

    public void SetPlayerSafe(bool safe)
    {
        playerIsSafe = safe;

        if (!playerIsSafe && statoAttuale == FireworkState.Esplosione)
            KillPlayer();
    }

    private void KillPlayer()
    {
        if (playerScript != null)
        {
            Debug.Log("Il gatto è stato spaventato a morte dal rumore!");

            Vector3 deathPosition = playerScript.transform.position;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.deathCount += 1;
                GameManager.Instance.RespawnPlayer(deathPosition);
            }

            playerScript.Die();
            playerScript = null; // Evita chiamate doppie
        }
    }

    private void UpdateDebugUI(string messaggio)
    {
        if (debugText != null) debugText.text = messaggio;
        Debug.Log("[Fireworks] " + messaggio);
    }
}