using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shell : MonoBehaviour
{
    public Rigidbody rb;
    public float forceMinimum;
    public float forceMaximum;

    float lifetime = 2;
    float fadetime = 1f;

    void Start()
    {
        float force = Random.Range(forceMinimum, forceMaximum);
        rb.AddForce(transform.right * force);
        rb.AddTorque(Random.insideUnitSphere * force);

        StartCoroutine(Fade());
    }

    IEnumerator Fade()
    {
        yield return new WaitForSeconds(lifetime);
        
        float percent = 0;
        float fadeSpeed = 1 / fadetime;
        Material shellMat = GetComponent<Renderer>().material;
        Color initialColor = shellMat.color;

        while(percent < 1)
        {
            percent += Time.deltaTime * fadeSpeed;
            shellMat.color = Color.Lerp(initialColor, Color.clear, percent);
            yield return null; //väntar en frame för varje steg i den här loopen
        }

        Destroy(gameObject);
    }
}
