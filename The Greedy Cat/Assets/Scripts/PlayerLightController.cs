using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal; // Obbligatorio per gestire Light2D

public class PlayerLightController : MonoBehaviour
{
    private Light2D playerLight;
    private bool isLightOn = false;
    public bool IsLightOn => isLightOn;

    [Header("Riferimento UI")]
    [SerializeField] private GameObject toggleLightButton; // Il pulsante della luce da nascondere/mostrare

    private void Awake()
    {
        // Trova automaticamente la luce 2D posizionata sotto il gatto
        playerLight = GetComponentInChildren<Light2D>();
    }

    private void OnEnable()
    {
        // Ascolta quando viene caricata una nuova scena (essenziale per il caricamento Additivo)
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GestisciMeccanicaEInterfaccia();
    }

    private void Start()
    {
        GestisciMeccanicaEInterfaccia();
    }

    private void GestisciMeccanicaEInterfaccia()
    {
        bool inLivelloConsentito = false;

        // Controlla se tra le scene attualmente caricate c'è il LVL_02 o il LVL_03
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            string nomeScena = SceneManager.GetSceneAt(i).name;
            if (nomeScena == "LVL_02" || nomeScena == "LVL_03")
            {
                inLivelloConsentito = true;
                break;
            }
        }

        // Mostra il pulsante UI solo se siamo nel livello 2 o 3, altrimenti lo nasconde
        if (toggleLightButton != null)
        {
            toggleLightButton.SetActive(inLivelloConsentito);
        }

        // Se siamo nel livello 1 (o menu), spegne forzatamente la luce sul gatto
        if (playerLight != null)
        {
            if (!inLivelloConsentito)
            {
                isLightOn = false;
                playerLight.enabled = false;
            }
            else
            {
                playerLight.enabled = isLightOn;
            }
        }
    }

    // Funzione da collegare al click del pulsante UI
    public void ToggleLight()
    {
        if (playerLight != null)
        {
            isLightOn = !isLightOn; // Inverte lo stato (se è accesa spegne, se è spenta accende)
            playerLight.enabled = isLightOn;
            Debug.Log("Luce del player: " + (isLightOn ? "ACCESA" : "SPENTA"));
        }
    }
}