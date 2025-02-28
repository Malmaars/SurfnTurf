using UnityEngine;

public class PickupCollectable : MonoBehaviour
{
    [Header("Visual Settings")]
    public float bobbingSpeed;
    public float bobbingAmplitude;
    public float rotationSpeed;
    public float randomnesRange;
    public float centerHeight;

    [Header("Collection Settings")]
    public float attractionSpeed;
    [Range(1f, 2f)]
    public float speedModifier;
    public float minimalScale;
    private float originalScale;
    public float outerDistance;
    public float innerDistance;
    public int collectionScore;
    private float currentSpeed = 0;

    private Transform visual;
    private Transform player;
    private bool isCollecting = false;
    

    private void Start()
    {
        //Setup collider
        SphereCollider collider = gameObject.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = outerDistance;
        collider.center = new Vector3(0, centerHeight, 0);

        visual = transform.GetChild(0);
        randomnesRange = Random.Range(0, randomnesRange);
        originalScale = visual.localScale.x;
        currentSpeed = attractionSpeed;
    }

    public void Update()
    {
        if (isCollecting) Collect();
        else UpdateVisual();
    }

    public void UpdateVisual()
    {
        //bobbing calculations for the visual
        float bobbingHeight = centerHeight + Mathf.Sin(Time.time * (bobbingSpeed + randomnesRange)) * bobbingAmplitude;
        visual.localPosition = new Vector3(visual.localPosition.x, bobbingHeight, visual.localPosition.z);

        //adding rotation to the visual
        visual.Rotate(Vector3.up * (rotationSpeed + randomnesRange) * Time.deltaTime);
    }

    public void Collect()
    {
        //Move the visual towards the player when collecting
        visual.position = Vector3.MoveTowards(visual.position, player.position, currentSpeed * Time.deltaTime);

        //Distance, scale and rotation calculations for animating while collecting
        float distance = Vector3.Distance(visual.position, player.position);
        float scaleAmount = Remap(distance, outerDistance, innerDistance, originalScale, minimalScale);
        if (distance > outerDistance) scaleAmount = originalScale;
        visual.localScale = Vector3.one * scaleAmount;
        visual.Rotate(Vector3.up * (rotationSpeed + randomnesRange) * (currentSpeed * 10f) * Time.deltaTime);

        //Destroy collectable when the distance is small enough to collect
        if(distance < innerDistance)
        {
            if (player.parent.GetComponent<CollectionManager>() != null)
                player.parent.GetComponent<CollectionManager>().UpdateScore(collectionScore);
            else
                Debug.LogError("CollectionManager not found on player parent object");
            Destroy(gameObject);
        }

        //if not collected yet, increase speed
        currentSpeed *= speedModifier;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player") && !isCollecting)
        {
            player = other.transform;
            isCollecting = true;
        }
    }

    float Remap(float s, float a1, float a2, float b1, float b2)
    {
        return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
    }
}
