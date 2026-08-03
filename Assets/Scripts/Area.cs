using System.Collections.Generic;
using UnityEngine;

public class Area : MonoBehaviour
{
    [SerializeField]
    public Dictionary<Switch, AreaTransition> exits;
    
    void Start()
    {
        foreach (var (s, at) in exits)
        {
            if (!at.gameObject.activeSelf)
            {
                at.gameObject.SetActive(!s || s.IsActive());
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var (s, at) in exits)
        {
            if (!at.gameObject.activeSelf)
            {
                at.gameObject.SetActive(!s || s.IsActive());
            }
        }
    }
    
    /*
    An area has an Switch object named exitSwitch; if its exit is not blocked by any doorway, this will be null;
    Each area will use its exitSwitch to determine when to spawn in the EnterArea trigger toward next area; If the default is null, then the EnterArea trigger will spawn in from the start.

        An area has a CameraMovement object, which defines how the camera should move while the player is within said area
        An area will hold child objects in its hierarchy that includes the terrain, obstacles, light switches, puzzle mechanics, kill zones, but not necessarily reference them within its code.
        */

}

