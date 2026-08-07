using UnityEngine;

public class InputSwitch : Switch
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        switchType = SwitchType.none;
    }

    public virtual void Interact()
    {
        
    }

    // Update is called once per frame
    protected override void Update()
    {
        
    }
    
    public void Activate()
    {
        active = true;
        activationState = 1;
    }

    public void Deactivate()
    {
        active = false;
        activationState = 0;
        
        
    }

    public void Toggle()
    {
        active = !active;
        activationState = (active) ? 1 : 0;
    }

    public void Increment()
    {
        
    }

    public void Decrement()
    {
        
    }

    public void ResetN()
    {
        
    }

    public void SetN(int n)
    {
        
    }
}
