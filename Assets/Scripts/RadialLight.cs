using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RadialLight : Switch
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Light2D _l;
    private float _angle;
    private float _radius;
    private float _threshold;
    private float _strictThreshold;
    public LayerMask layerMask;
    void Start()
    {
        _l = GetComponent<Light2D>();
        _radius = _l.pointLightOuterRadius;
        _angle = _l.pointLightInnerAngle/2;
        _threshold = Mathf.Cos(_angle * Mathf.Deg2Rad);
        _strictThreshold = Mathf.Cos(_angle+20f * Mathf.Deg2Rad);
    }

    // Update is called once per frame
    protected override void Update()
    {
        if (active)
        {
            if (Player.p.IsDashing() || Player.p.IsDying()) return;
            
            //the ray cast will want to "try" different coordinates
            //Player size is always 1.75 by 2
            Vector3 diff = Player.p.transform.position - transform.position;

            if (diff.magnitude > _radius+2.5f || 
                Vector3.Dot(diff.normalized, transform.up) <= _strictThreshold) 
                return;

            bool detected = false;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    Vector3 samplePoint = diff + new Vector3(i * Player.Dimensions.x/2, j * Player.Dimensions.y/2, 0);
                    float dot = Vector3.Dot(samplePoint.normalized, transform.up);
                    //val 
                    if (dot <= _threshold || samplePoint.magnitude > _radius)
                    {
                        _l.color = Color.white;
                        continue;
                    }
                    
                    detected = true;
                    _l.color = Color.red;
                    break;
                }

                if (detected) break;
            }

            if (detected)
            {
                Player.p.Death(true);
            }
            
            
            
            
            //check if player is within angle and within radius
            //within angle: 
            
        }
    }
    
}
