using UnityEngine;
using System.Collections;

public class MultiShadowRenderer : MonoBehaviour
{
    public Camera redFirstCamera, greenSecondCamera, blueThirdCamera, alphaForthCamera, blackClearerCamera;
    public RenderTexture RT;
    public Material shadeMat;
    int[] remains = new int[4] { 1, 2, 4, 8 };

	// Use this for initialization
	void Start ()
    {
        blackClearerCamera.enabled = false;
        blackClearerCamera.Render();

        redFirstCamera.enabled = false;
        greenSecondCamera.enabled = false;
        //blueThirdCamera.enabled = false;
        //alphaForthCamera.enabled = false;

        //根据近剪裁平面分辨摄像机和要绘制的通道
        redFirstCamera.nearClipPlane = 0.5f;
        greenSecondCamera.nearClipPlane = 0.6f;
        //blueThirdCamera.nearClipPlane = 0.7f;
        //alphaForthCamera.nearClipPlane = 0.8f;

        //redFirstCamera.Render();
    }
	
	// Update is called once per frame
	void Update ()
    {
        int i;
        /*for(i=3;i>=0 ;i--)
        {
            remains[i]--;
            if(remains[i] == 0)
            {
                switch(i)
                {
                    case 0:
                        //redFirstCamera.Render();
                        break;
                    case 1:
                        blackClearerCamera.Render();
                        greenSecondCamera.Render();
                        break;
                    case 2:
                        //blueThirdCamera.Render();
                        break;
                    case 3:
                        //alphaForthCamera.Render();
                        //blackClearerCamera.Render();
                        break;
                }
                remains[i] = (int)Mathf.Pow(2f, i);
            }
        }*/
        redFirstCamera.Render();
        shadeMat.SetTexture("shadowTex", RT);
        greenSecondCamera.Render();
    }
}
