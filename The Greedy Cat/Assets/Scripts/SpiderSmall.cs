using UnityEngine;

public class SpiderSmall : SpiderManager
{
    void Start()
    {
        startPos = transform.position;
        speed = 2f;
        damage = 1;
    }
}

