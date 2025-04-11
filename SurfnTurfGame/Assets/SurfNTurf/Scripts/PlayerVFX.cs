using UnityEngine;
using UnityEngine.VFX;

public class PlayerVFX : MonoBehaviour
{
    public VisualEffect pickUp;
    public VisualEffect pickUpCoin;
    public VisualEffect onJump;
    public VisualEffect waterTrail;
    public VisualEffect waterSplash;
    public VisualEffect runningDust;
    public VisualEffect twirl;
    public VisualEffect parrySpark;
    public VisualEffect spinner;
    public GameObject player;
    private Rigidbody rb;
    private bool submerged = false;
    [SerializeField] private float waterLevel = 0.5f;
    private bool isPlaying;
    [SerializeField] private float threshold;
    private bool onWater;
    public bool OnWater
    {
        get
        {
            return onWater;
        }
        set
        {
            onWater = value;
            if (onWater)
            {
                if(waterTrail.GetInt("TrailIndex") == 6)
                {
                    waterTrail.SetInt("TrailIndex", 0);
                }
                else
                {
                    waterTrail.SetInt("TrailIndex",waterTrail.GetInt("TrailIndex") + 1);
                }
            }
        }
    }
    //singelton instance
    public static PlayerVFX instance { get; private set; }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        rb = player.GetComponent<Rigidbody>();
    }

    public void VFXSpawn(VisualEffect visualEffect)
    {
        visualEffect.SetVector3("PlayerPosition", new Vector3(player.transform.position.x, waterLevel, player.transform.position.z));
        visualEffect.SendEvent("OnPlay");
    }

    private void Update()
    {
        Splash();
        TrailSpawn();
    }

    private void Splash()
    {
        if (player.transform.position.y < waterLevel)
        {
            if (!submerged)
            {
                VFXSpawn(waterSplash);
                StartCoroutine(ShaderManager.instance.PlayRipple());
                submerged = true;
            }
        }
        else
        {
            if (submerged)
            {
                submerged = false;
            }
        }

    }

    private void TrailSpawn()
    {
        if (onWater)
        {

            if (rb.linearVelocity.magnitude > threshold)
            {
                if (!isPlaying)
                {
                    isPlaying = true;
                    waterTrail.Play();
                }
            }
            else
            {
                StartCoroutine(ShaderManager.instance.PlayRippleIdle());
                waterTrail.Stop();
                isPlaying = false;
            }
            waterTrail.SetFloat("Velocity", rb.linearVelocity.magnitude);
        }
        else
        {
            if (isPlaying)
            {
                waterTrail.Stop();
                isPlaying = false;
            }
        }

    }
}
