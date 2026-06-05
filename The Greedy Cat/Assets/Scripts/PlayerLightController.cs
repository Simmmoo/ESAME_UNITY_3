using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

public class PlayerLightController : MonoBehaviour
{
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
        isLightOn = false;
        GestisciMeccanicaEInterfaccia();
    }

    private void OnSceneUnloaded(Scene scene)
    {
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

        AggiornaLuceSuPlayer();
    }

    public void ToggleLight()
    {
        isLightOn = !isLightOn;
        AggiornaLuceSuPlayer();
        Debug.Log("Luce: " + (isLightOn ? "ACCESA" : "SPENTA"));
    }

    public void ResetLuce()
    {
        isLightOn = false;
        AggiornaLuceSuPlayer();
        GestisciMeccanicaEInterfaccia();
    }

    public void AggiornaLuceSuPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return;

        Light2D luce = playerObj.GetComponentInChildren<Light2D>();
        if (luce != null)
            luce.enabled = isLightOn;
    }
}