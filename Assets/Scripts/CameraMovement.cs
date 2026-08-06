using System;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

public class CameraMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private float _fullZoomWidth = 40;
    private float _fullZoomHeight = 20.4f;
    private float _regularZoomZ = -19.5f;

    private Camera c;
    private Player p;

    [Range(0, 1)] 
    public float cameraFollowRate;
    void Start()
    {
        p = Player.p;
        c = Camera.main;
        Vector3 proj = c.ViewportToWorldPoint(new Vector3(1, 1, -transform.position.z));
        _fullZoomWidth = 2*proj.x;
        _fullZoomHeight = 2*proj.y;
    }

    // Update is called once per frame
    void Update()
    {
        Area a = Player.p.currentArea;

        float X = a.width;
        float Y = a.height;
        Vector3 preferredCoords = Player.p.transform.position;
        Vector3 areaCoords = a.transform.position;
        preferredCoords.x = Mathf.Clamp(preferredCoords.x, -X / 2 + _fullZoomWidth / 2 + areaCoords.x,
            X / 2 - _fullZoomWidth / 2 + areaCoords.x);
        preferredCoords.y = Mathf.Clamp(preferredCoords.y, -Y / 2 + _fullZoomHeight / 2 + areaCoords.y,
            Y / 2 - _fullZoomHeight / 2 + areaCoords.y);
        preferredCoords.z = _regularZoomZ;
        
        
        
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
