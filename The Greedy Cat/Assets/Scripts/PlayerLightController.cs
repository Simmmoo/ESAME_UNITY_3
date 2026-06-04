using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;

public class PlayerLightController : MonoBehaviour
{
    // Non ha più bisogno di stare sul player
    // Va attaccato a un GameObject della UI persistente (es. il GameManager o il Canvas)

    private bool isLightOn = false;
    public bool IsLightOn => isLightOn;

    [Header("Riferimento UI")]
    [SerializeField] private GameObject toggleLightButton;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void Start()
    {
        GestisciMeccanicaEInterfaccia();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Resetta la luce ogni volta che si carica un nuovo livello
        isLightOn = false;
        GestisciMeccanicaEInterfaccia();
    }

    private void OnSceneUnloaded(Scene scene)
    {
        // Ricalcola UI anche quando una scena viene scaricata (fix problema LVL_01 dopo LVL_02)
        GestisciMeccanicaEInterfaccia();
    }

    private void GestisciMeccanicaEInterfaccia()
    {
        bool inLivelloConsentito = false;

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            string nomeScena = SceneManager.GetSceneAt(i).name;
            if (nomeScena == "LVL_02" || nomeScena == "LVL_03")
            {
                inLivelloConsentito = true;
                break;
            }
        }

        if (toggleLightButton != null)
            toggleLightButton.SetActive(inLivelloConsentito);

        // Aggiorna la luce sul player corrente (cercato dinamicamente)
        AggiornaLuceSuPlayer();
    }

    public void ToggleLight()
    {
        isLightOn = !isLightOn;
        AggiornaLuceSuPlayer();
        Debug.Log("Luce: " + (isLightOn ? "ACCESA" : "SPENTA"));
    }

    public void AggiornaLuceSuPlayer()
    {
        // Cerca sempre il player corrente in scena, funziona anche dopo il respawn
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return;

        Light2D luce = playerObj.GetComponentInChildren<Light2D>();
        if (luce != null)
            luce.enabled = isLightOn;
    }
}