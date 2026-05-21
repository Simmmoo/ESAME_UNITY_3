using UnityEngine;

public class PersistentObject : MonoBehaviour
{
    private static PersistentObject instance;

    private void Awake()
    {
        // Questo controllo evita che, se torni al Menu Principale e poi rigiochi, 
        // si creino dei gatti o dei Canvas duplicati nella scena.
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
