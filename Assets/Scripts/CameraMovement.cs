using System;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

public class CameraMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private const float FullZoomWidth = 40;
    private const float FullZoomHeight = 22.5f;
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
        _regularZoomZ *= (proj.x / FullZoomWidth + proj.y / FullZoomHeight);
        print(_regularZoomZ);
    }

    // Update is called once per frame
    void Update()
    {
        Area a = Player.p.currentArea;

        float X = a.width;
        float Y = a.height;
        Vector3 preferredCoords = Player.p.transform.position;
        Vector3 areaCoords = a.transform.position + a.cameraFocal;
        float xLow = -X / 2 + FullZoomWidth / 2 + areaCoords.x;
        float xHigh = X / 2 - FullZoomWidth / 2 + areaCoords.x;
        if (xLow > xHigh)
        {
            float xMid = (xHigh + xLow) / 2;
            xLow = xMid;
            xHigh = xMid;
        }
        
        float yLow = -Y / 2 + FullZoomHeight / 2 + areaCoords.y;
        float yHigh = Y / 2 - FullZoomHeight / 2 + areaCoords.y;

        if (yLow > yHigh)
        {
            float yMid = (yHigh + yLow) / 2;
            yLow = yMid;
            yHigh = yMid;
        }
        preferredCoords.x = Mathf.Clamp(preferredCoords.x, xLow,xHigh);
        preferredCoords.y = Mathf.Clamp(preferredCoords.y, yLow,yHigh);
        //print(-Y / 2 + _fullZoomHeight / 2 + areaCoords.y);
        //print(Y / 2 - _fullZoomHeight / 2 + areaCoords.y);
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
