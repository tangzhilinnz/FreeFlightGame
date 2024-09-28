using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseEffectFlash : MonoBehaviour
{
    public float gapTime; // time interval of flashing
    private float temp = 0.0f;
    bool IsDisplay = true;
    // make the MouseEffect flash by controling the switch of MeshRenderer component
    private MeshRenderer render;

    // Start is called before the first frame update
    void Start()
    {
        render = gameObject.GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        Effect();
        // destroy the gameObject when the Ethan is stepping into the mouseEffect
        if (Vector3.Distance(this.transform.position,
            GameObject.Find("EthanCtr").transform.position) <= 0.5f)
        {
            Destroy(this.gameObject);
        }
    }
    public void Effect()
    {
        temp += Time.deltaTime;
        if (temp >=  gapTime)
        {
            if(IsDisplay)
            {
                render.enabled = false;
                IsDisplay = false;
                temp = 0;
            }
            else
            {
                render.enabled = true;
                IsDisplay = true;
                temp = 0;
            }
        }
    }
}
