using UnityEngine;
using UnityEngine.UI;

public class LevelSelectMenu : MonoBehaviour
{
    [Header("Bottoni livelli")]
    public Button buttonLvl1;
    public Button buttonLvl2;
    public Button buttonLvl3;

    private void OnEnable()
    {
        // Viene ricalcolato ogni volta che il menu si apre
        AggiornaBottoni();
    }

    private void AggiornaBottoni()
    {
        int highestUnlocked = PlayerPrefs.GetInt("HighestLevelUnlocked", 1);

        // LVL_01 sempre accessibile
        buttonLvl1.interactable = true;

        // LVL_02 e LVL_03 dipendono dalla progressione
        buttonLvl2.interactable = highestUnlocked >= 2;
        buttonLvl3.interactable = highestUnlocked >= 3;
    }
}