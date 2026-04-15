using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundController : MonoBehaviour
{
    private float startPosX, startPosY;
    private float lengthX;
    public GameObject cam;
    public float parallaxEffectX;
    public float parallaxEffectY;
    public float yOffset = -2f; // Offset per regolare la posizione Y

    void Start()
    {
        startPosX = transform.position.x;
        startPosY = transform.position.y;
        lengthX = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void FixedUpdate()
    {
        float distanceX = cam.transform.position.x * parallaxEffectX;
        float distanceY = (cam.transform.position.y - yOffset) * parallaxEffectY; // Applica l'offset

        float movementX = cam.transform.position.x * (1 - parallaxEffectX);

        // Imposta la posizione con offset in Y
        transform.position = new Vector3(startPosX + distanceX, startPosY + distanceY, transform.position.z);

        // Effetto looping solo sull'asse X
        if (movementX > startPosX + lengthX)
        {
            startPosX += lengthX;
        }
        else if (movementX < startPosX - lengthX)
        {
            startPosX -= lengthX;
        }
    }
}
