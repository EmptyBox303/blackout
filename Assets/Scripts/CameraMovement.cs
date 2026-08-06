using System;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

public class CameraMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private const float FullZoomWidth = 40;
    private const float FullZoomHeight = 20.4f;
    private const float RegularZoomZ = -19.5f;
    
    
    private Player p;

    [Range(0, 1)] 
    public float cameraFollowRate;
    void Start()
    {
        p = Player.p;
    }

    // Update is called once per frame
    void Update()
    {
        Area a = Player.p.currentArea;

        float X = a.width;
        float Y = a.height;
        Vector3 preferredCoords = Player.p.transform.position;
        Vector3 areaCoords = a.transform.position;
        preferredCoords.x = Mathf.Clamp(preferredCoords.x, -X / 2 + FullZoomWidth / 2 + areaCoords.x,
            X / 2 - FullZoomWidth / 2 + areaCoords.x);
        preferredCoords.y = Mathf.Clamp(preferredCoords.y, -Y / 2 + FullZoomHeight / 2 + areaCoords.y,
            Y / 2 - FullZoomHeight / 2 + areaCoords.y);
        preferredCoords.z = RegularZoomZ;
        
        
        
        //
        if (cameraFollowRate == 1f)
        {
            transform.position = preferredCoords;
        }
        
        float coeff = cameraFollowRate/(cameraFollowRate-1);
        Vector3 diff = preferredCoords - transform.position;

        diff *= -Time.unscaledDeltaTime * coeff;
        transform.position += diff;
        
    }
}
