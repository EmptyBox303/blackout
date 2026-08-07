using UnityEngine;

public class TextureAspect : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public RectTransform rt;
    public RenderTexture rtx;
    void Start()
    {
        rt.sizeDelta = new Vector2(Screen.width, Screen.height);
        rtx.width = Mathf.RoundToInt(Screen.width * 180f / Screen.height);
        rtx.height = 180;
        //try to always scale the height of the renderTexture to 180 pixels
        //
    }

    // Update is called once per frame
    void Update()
    {
    }
}
