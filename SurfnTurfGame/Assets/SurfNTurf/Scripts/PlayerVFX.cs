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
    public VisualEffect swipe;
    public VisualEffect doubleJump;
    public VisualEffect parrySpark;
    public VisualEffect spinner;
    public VisualEffect GodRays;
    public GameObject player;
    private Rigidbody rb;
    private bool submerged = false;
    [SerializeField] private float waterLevel = 0.5f;
    private bool isPlaying;
    [SerializeField] private float threshold;
    private WaterMovementController waterMovementController;
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
                if (waterTrail.GetInt("TrailIndex") == 6)
                {
                    waterTrail.SetInt("TrailIndex", 0);
                }
                else
                {
                    waterTrail.SetInt("TrailIndex", waterTrail.GetInt("TrailIndex") + 1);
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
        waterMovementController = BlackBoard.playerBody.GetComponent<WaterMovementController>();
        rb = player.GetComponent<Rigidbody>();
    }

    public void VFXSpawn(VisualEffect visualEffect)
    {
        visualEffect.SetVector3("PlayerPosition", new Vector3(player.transform.position.x, waterLevel, player.transform.position.z));
        visualEffect.SendEvent("OnPlay");
    }

    private void Update()
    {
        if (waterMovementController.isActiveAndEnabled)
        {
            if (OnWater != true)
                OnWater = true;
        }
        else
        {
            if (OnWater == true)
                OnWater = false;
        }
        Splash();
        TrailSpawn();
        SetSunDirection();
    }
    private void SetSunDirection()
    {
        Vector3 cameraForward = Camera.main.transform.forward; // world-space forward direction

        // Sun direction — must be normalized
        Vector3 sunDirection = ShaderManager.instance.SunDirection.normalized; // already normalized as per your note

        GodRays.SetVector3("CameraForward", cameraForward);
        GodRays.SetVector3("SunDirection", sunDirection);
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
        if (OnWater)
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
                StartCoroutine(ShaderManager.instance.PlayRippleIdle()); ;
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
