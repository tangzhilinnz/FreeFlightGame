using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageHP : MonoBehaviour
{
    public float Power = 10; // emission velocity
    public float Gravity = -9.8f; // gravitational acceleration

    private Vector3 MoveSpeed; // initial velocity vector
    private Vector3 graAcceleration = Vector3.zero; // gravitational acceleration vector
    private float dTime; // passed time

    private bool isMoving = false;

    // used for managing fade out effect
    public float fadeSpeed = 0.1f;
    public float StartFadingTime = 1f;
    private MeshRenderer rend;

    public void Awake()
    {
        Invoke("StartFading", StartFadingTime);
    }

    // Start is called before the first frame update
    void Start()
    {
        // make the 3D Text show in front of other objects
        transform.SetAsLastSibling();
        rend = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!isMoving) return;
        // calculate gravity velocity: vG = gt ;
        graAcceleration.y = Gravity * (dTime += Time.fixedDeltaTime);
        // simulate the parabolic trajectory
        transform.position += (MoveSpeed + graAcceleration) * Time.fixedDeltaTime;

        if (transform.position.y <= 0)
            Destroy(this.gameObject);
    }

    internal void SetMove(Vector3 danmageDirection)
    {
        MoveSpeed = danmageDirection * Power;
        isMoving = true;
    }

    private IEnumerator FadeOut()
    {
        Color color = rend.material.color;
        float alpha = 1.0f;

        while (color.a > 0.0f)
        {
            alpha -= fadeSpeed * Time.deltaTime;
            color.a = alpha;
            rend.material.color = color;
            yield return null;
        }
        color.a = 0.0f;
    }

    private void StartFading()
    {
        StartCoroutine("FadeOut");
    }

}