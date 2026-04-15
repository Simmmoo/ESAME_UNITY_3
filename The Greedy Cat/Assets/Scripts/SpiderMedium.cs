using UnityEngine;

public class SpiderMedium : SpiderManager
{
    void Start()
    {
        startPos = transform.position;
        speed = 3f;
        damage = 2;
    }
}

