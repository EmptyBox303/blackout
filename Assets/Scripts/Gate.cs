using System.Collections;
using UnityEngine;

public class Gate : Switch
{
    public GameObject upperGate;
    public GameObject lowerGate;

    public float openDuration;

    private Coroutine _openActive = null;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    protected override void Update()
    {
        //gated condition: once gate lifts, cannot be unlifted
        if (active)
        {
            if (_openActive == null)
            {
                _openActive = StartCoroutine(Open());
            }
            return;
        }

        if (targets.Count == 0)
        {
            return;
        }
        base.Update();
    }

    IEnumerator Open()
    {

        float upperL = upperGate.transform.localScale.y;
        float lowerL = lowerGate.transform.localScale.y;
        
        for (float t = 0; t < openDuration; t += Time.deltaTime)
        {
            float r = t/openDuration;

            upperGate.transform.localScale -= new Vector3(0, upperL * Time.deltaTime / openDuration, 0);
            upperGate.transform.localPosition += new Vector3(0, upperL * Time.deltaTime / (openDuration * 2), 0);
            lowerGate.transform.localScale -= new Vector3(0, lowerL * Time.deltaTime / openDuration, 0);
            lowerGate.transform.localPosition -= new Vector3(0, lowerL * Time.deltaTime / (openDuration * 2), 0);
            
            yield return null;
        }
        upperGate.transform.localScale = Vector3.zero;
        lowerGate.transform.localScale = Vector3.zero;
        gameObject.SetActive(false);
    }
}
