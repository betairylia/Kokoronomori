using UnityEngine;
using System.Collections;

public class cameraController : MonoBehaviour
{
    public float radius;
    public float tiltMax = 70f, tiltMin = -10f, rad = 0f, tilt = -10f, localRad = 0f, localTilt = 0f;
    public float speed = 2f, radSpeed = 2f, radMax = 10f, radMin = 1f;

    Touch oldTouch1, oldTouch2;
    public float normalizeSpeed = 2f;

    public GameObject UIPanel;
    public fractalRenderer fRenderer;

    public bool freeCam = false, flag = false;

    // Use this for initialization
    void Start ()
    {
        transform.position = new Vector3(0, 8, 0) + Quaternion.Euler(0, rad, tilt) * (radius * Vector3.right);
        transform.LookAt(new Vector3(0, 8, 0));
        UIPanel.SetActive(false);
    }
	
	// Update is called once per frame
	void Update ()
    {
	    if(Input.GetKey(KeyCode.A))
        {
            rad += 100 * speed * Time.deltaTime;
        }

        if(Input.GetKey(KeyCode.D))
        {
            rad -= 100 * speed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.W))
        {
            tilt += 100 * speed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.S))
        {
            tilt -= 100 * speed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.Q))
        {
            radius += 100 * speed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.E))
        {
            radius -= 100 * speed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.Plus) || Input.GetKey(KeyCode.Equals))
        {
            fRenderer.changeGrowthRate(4 * Time.deltaTime);
            //fRenderer.startGrowRate;
        }

        if (Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.Underscore))
        {
            fRenderer.changeGrowthRate((-4) * Time.deltaTime);
        }

        if(Input.GetKeyDown(KeyCode.R))
        {
            fRenderer.changeRandomSeed(Random.Range(-10000, 10000));
        }

        if (Input.touchCount > 0)
        {
            if(Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                Vector2 dPos = touch.deltaPosition;

                if(!freeCam)
                {
                    tilt -= dPos.y * speed;
                    rad += dPos.x * speed;

                    if (tilt > tiltMax) { tilt = tiltMax; }
                    if (tilt < tiltMin) { tilt = tiltMin; }
                }
                else
                {
                    transform.Rotate((-dPos.y) * speed, dPos.x * speed, 0);
                }
            }

            if(Input.touchCount == 2)
            {
                Touch newTouch1 = Input.GetTouch(0);
                Touch newTouch2 = Input.GetTouch(1);

                if(newTouch2.phase == TouchPhase.Began)
                {
                    oldTouch1 = newTouch1;
                    oldTouch2 = newTouch2;
                }
                else
                {
                    float oldDis = Vector2.Distance(oldTouch1.position, oldTouch2.position);
                    float newDis = Vector2.Distance(newTouch1.position, newTouch2.position);
                    float offset = oldDis - newDis;

                    oldTouch1 = newTouch1;
                    oldTouch2 = newTouch2;

                    radius += offset * radSpeed;
                    if (radius > radMax) { radius = radMax; }
                    if (radius < radMin) { radius = radMin; }
                }
            }

            if (Input.touchCount == 3)
            {
                if(Input.GetTouch(2).phase == TouchPhase.Began)
                {
                    freeCam = !freeCam;
                }
            }

            if (Input.touchCount == 4)
            {
                if (Input.GetTouch(3).phase == TouchPhase.Began)
                {
                    UIPanel.SetActive(!UIPanel.activeSelf);
                }
            }
        }

        if (radius > radMax) { radius = radMax; }
        if (radius < radMin) { radius = radMin; }
        if (tilt > tiltMax) { tilt = tiltMax; }
        if (tilt < tiltMin) { tilt = tiltMin; }

        //transform.position = Quaternion.AngleAxis(tilt, Quaternion.AngleAxis(rad, Vector3.up) * Vector3.forward) *
        //    Quaternion.AngleAxis(rad, Vector3.up) * (radius * Vector3.right);
        transform.position = new Vector3(0, 8, 0) + Quaternion.Euler(0, rad, tilt) * (radius * Vector3.right);
        if (!freeCam)
        {
            transform.LookAt(new Vector3(0, 8, 0));
        }
    }
}
