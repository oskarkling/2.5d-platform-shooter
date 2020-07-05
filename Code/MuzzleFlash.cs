using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuzzleFlash : MonoBehaviour
{
    public GameObject flashHolder;
    public Sprite[] flashSpritesArr;
    public SpriteRenderer[] spriteRenderersArr;

    public float flashTime;

    void Start()
    {
        Deactivate();
    }
    public void Activate()
    {
        flashHolder.SetActive(true);

        int flashSpriteIndex = Random.Range(0, flashSpritesArr.Length);
        for(int i = 0; i < spriteRenderersArr.Length; i++)
        {
            spriteRenderersArr[i].sprite = flashSpritesArr[flashSpriteIndex];
        }

        Invoke("Deactivate", flashTime);
    }

    void Deactivate()
    {
        flashHolder.SetActive(false);
    }
}
