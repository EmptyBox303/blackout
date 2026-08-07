using System;
using System.Collections.Generic;
using UnityEngine;

public class Area : MonoBehaviour
{
    [SerializeField]
    public List<PairedLogic> exits;

    [Header("Boundaries")] 
    public float width;
    public float height;

    public Vector3 cameraFocal;

    [Header("Respawn Coords")] 
    public GameObject respawnMarker;
    
    [Serializable]
    public class PairedLogic
    {
        public Switch s;
        public AreaTransition at;
    }
    void Start()
    {
        foreach (var p in exits)
        {
            if (!p.at.gameObject.activeSelf)
            {
                p.at.gameObject.SetActive(!p.s || p.s.IsActive());
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var p in exits)
        {
            if (!p.at.gameObject.activeSelf)
            {
                p.at.gameObject.SetActive(!p.s || p.s.IsActive());
            }
        }
    }

    public Vector3 RespawnPos()
    {
        return respawnMarker.transform.position;
    }
    
    /*
    An area has an Switch object named exitSwitch; if its exit is not blocked by any doorway, this will be null;
    Each area will use its exitSwitch to determine when to spawn in the EnterArea trigger toward next area; If the default is null, then the EnterArea trigger will spawn in from the start.

        An area has a CameraMovement object, which defines how the camera should move while the player is within said area
        An area will hold child objects in its hierarchy that includes the terrain, obstacles, light switches, puzzle mechanics, kill zones, but not necessarily reference them within its code.
        */

}

