using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;

public class Destructible : MonoBehaviour
{
    [SerializeField] VisualEffectAsset vfxAsset;
    [SerializeField] float lifetime = 2f;
    [SerializeField] float size = 1f;
    [SerializeField] bool canRegrow = false;
    [ShowIf("canRegrow")]
    [SerializeField] float regrowTime = 2f;
    [ShowIf("canRegrow")]
    [SerializeField] float growDelay = 2f;
    [ReadOnly] public List<VisualEffect> vfxObjects = new List<VisualEffect>();
    string texturePropertyName = "_BaseMap";
    MeshRenderer meshRenderer;
    Vector3 originalSize;

    // start but giving it a vfx component
    void Start()
    {
        //get all mesh renderes in the children of the game object
        meshRenderer = GetComponent<MeshRenderer>();
        originalSize = transform.localScale;
        foreach (Material material in meshRenderer.materials)
        {
            GameObject vfxObject = new GameObject(gameObject.name + material.name + "Explosion VFX");
            if (GameObject.Find("Explosion VFX Pool") == null)
            {
                GameObject pool = new GameObject("Explosion VFX Pool");
                vfxObject.transform.SetParent(pool.transform);
            }
            {
                GameObject pool = GameObject.Find("Explosion VFX Pool");
                vfxObject.transform.SetParent(pool.transform);
            }

            vfxObject.transform.position = transform.position;
            vfxObject.transform.rotation = transform.rotation;

            VisualEffect vfx = vfxObject.AddComponent<VisualEffect>(); ;
            vfx.visualEffectAsset = vfxAsset;
            vfx.initialEventName = "";
            vfxObjects.Add(vfx);
            vfxObject.SetActive(false);
        }


    }
    //Destroy on trigger enter of the player
    private void OnTriggerEnter(Collider other)
    {
        /*if (other.CompareTag("Player"))
        {
            Destruct();
        }*/
    }

    public void Destruct(Transform destructionCause)
    {
		for (int i = 0; i < vfxObjects.Count; i++)
		{
			vfxObjects[i].gameObject.SetActive(true);
			vfxObjects[i].SetTexture(texturePropertyName, meshRenderer.materials[i].GetTexture(texturePropertyName));
			vfxObjects[i].SetVector4("_BaseColor", meshRenderer.materials[i].GetColor("_BaseColor"));
			vfxObjects[i].SetFloat("_Size", size);
			vfxObjects[i].SetFloat("_LifeTime", lifetime);
			vfxObjects[i].SetVector3("_PlayerPosition", destructionCause.position);
			vfxObjects[i].SendEvent("OnPlay");
			StartCoroutine(Disable(vfxObjects[i].gameObject, lifetime));
		}
		Death(gameObject);
	}

    //Ongizmos to show the size of the explosion
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, size);
    }

    private IEnumerator Disable(GameObject localGameObject, float seconds = 0f)
    {
        yield return new WaitForSeconds(seconds);
        localGameObject.SetActive(false);
    }
    private void Death(GameObject localGameObject)
    {
        if (canRegrow)
        {
            localGameObject.transform.localScale = Vector3.zero;
            localGameObject.GetComponent<Collider>().enabled = false;
            StartCoroutine(Regrow(localGameObject));
        }
        else
        {
            Destroy(localGameObject);
        }
    }

    private IEnumerator Regrow(GameObject localGameObject)
    {
        //lerp from size 0 to 1 and then turn the trigger back on
        yield return new WaitForSeconds(growDelay);
        float elapsedTime = 0f;
        while (elapsedTime < regrowTime)
        {
            elapsedTime += Time.deltaTime;
            localGameObject.transform.localScale = Vector3.Lerp(Vector3.zero, originalSize, elapsedTime / regrowTime);
            yield return null;
        }
        localGameObject.GetComponent<Collider>().enabled = true;
    }
}
