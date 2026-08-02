using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Switch : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] protected bool active;
    [SerializeField] protected int activationState;
    [SerializeField] protected SwitchType switchType;
    [SerializeField] protected SpriteRenderer sr;
    
    [SerializeField] protected List<Switch> targets;
    [SerializeField] protected int constraint;
    public float readDelay;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        switch (switchType)
        {
            case SwitchType.none: return;
            case SwitchType.inherit:
                SetActive(targets[0].IsActive());
                SetActivation(targets[0].ActivationNumber());
                break;
            case SwitchType.invert:
                SetActive(!targets[0].IsActive());
              
                SetActivation(targets[0].ActivationNumber());
                break;
            case SwitchType.union:
                var unionState = false;
                foreach(Switch s in targets)
                {
                    if (s.IsActive())
                    {
                        unionState = true;
                        break;
                    }
                }
                SetActive(unionState);
                SetActivation(0);

                break;
            case SwitchType.intersect:
                var intersectState = true;
                foreach (Switch s in targets)
                {
                    if (!s.IsActive())
                    {
                        intersectState = false;
                        break;
                    }
                }
                SetActive(intersectState);
                SetActivation(0);
                break;
            case SwitchType.count:
                int count = 0;
                foreach (Switch s in targets)
                {
                    if (s.IsActive()) count++;
                }
                activationState = count;
                break;
            case SwitchType.total:
                int total = 0;
                foreach (Switch s in targets)
                {
                    total += s.ActivationNumber();
                }
                activationState = total;
                break;
            case SwitchType.subtract:
                activationState = targets[0].ActivationNumber() - targets[1].ActivationNumber();
                break;
            
            case SwitchType.max:
                activationState = targets[0].ActivationNumber();
                active = (activationState <= constraint);
                break;
            
            case SwitchType.min:
                activationState = targets[0].ActivationNumber();
                active = (activationState >= constraint);
                break;
            
            case SwitchType.equals:
                activationState = targets[0].ActivationNumber();
                active = (activationState == constraint);
                break;
                
        }

    }

    void SetActive(bool b)
    {
        if (b != active)
        {
            StartCoroutine(SetActiveWithDelay(b));
        }
    }

    IEnumerator SetActiveWithDelay(bool b)
    {
        yield return new WaitForSeconds(readDelay);
        active = b;
    }

    void SetActivation(int n)
    {
        if (n != activationState)
        {
            StartCoroutine(SetActivationWithDelay(n));
        }
    }

    IEnumerator SetActivationWithDelay(int n)
    {
        yield return new WaitForSeconds(readDelay);
        activationState = n;
    }
    public bool IsActive()
    {
        return active;
    }
    
    public int ActivationNumber()
    {
        return activationState;
    }

    public enum SwitchType
    {
        none, // inputswitch state
        
        inherit, //binary; inherit activity from target switch
        invert, //binary; invert activity from target switch
        union, //binary; OR of all boolean activity from a list of target switches
        intersect, //binary; AND of all boolean from a list of targets
        count, //number; count the number of actives(bool) from targets; no boolean activation behavior
        total, //number; total of the numbers from targets; no boolean activation behavior
        subtract, //number; difference between n of t1 and n of t2; no boolean activation behavior
        max, //binary; inherit numerical activation state from target; only become active if n <= some limit
        min, //binary; vice versa, but with n >=
        equals, //binary; n equals
    }
}
