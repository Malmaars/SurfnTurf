using System;
using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class Challenge : MonoBehaviour
{
    public string challengeName;
    public String[] challengeObjectives;
    public bool forceRestrictions = false;
    [SerializeField] private Restriction restrictions;
    [SerializeField] private float maxTime;
    private float currentTime;
    private Restriction savedRestrictions;
    [SerializeField] private Collider challengeBounds;
    [SerializeField] private Collider endZone;
    [SerializeField] private Transform startPoint;
    [SerializeField] private GameObject startButton;
    [SerializeField] private GameObject[] stars;
    GameObject player;
    MovementController MC;
    WaterMovementController WMC;
    bool challengeInProgress = false;
    public UnityEvent OnStart;
    public UnityEvent OnStop;
    IEnumerator Start()
    {
        player = FindFirstObjectByType<MovementController>().gameObject;
        MC = player.GetComponent<MovementController>();
        WMC = player.GetComponent<WaterMovementController>();

        yield return new WaitUntil(() => CloudSaveSystem.Instance != null && CloudSaveSystem.Instance.IsInitialized);
        if (CloudSaveSystem.Instance.data.challengeDatas.Find(item => item.key == challengeName) != null) { }
        else
        {
            ChallengeData localChallengeData = new ChallengeData();
            localChallengeData.key = challengeName;
            CloudSaveSystem.Instance.data.challengeDatas.Add(localChallengeData);
        }
        UpdateStars();
    }

    [Button("Start Challenge", EButtonEnableMode.Playmode)]
    public void StartChallenge()
    {
        foreach (Toggle toggle in ChallengeUI.instance.objectivesChecks)
        {
            toggle.isOn = true;
        }
        ChallengeUI.instance.objectivesChecks[0].isOn = false;
        player.GetComponent<Rigidbody>().MovePosition(startPoint.position);
        challengeInProgress = true;
        currentTime = 0;
        if (forceRestrictions)
        {
            savedRestrictions = new Restriction(MC, WMC);
            restrictions.ApplyRestriction(null, MC, WMC);
        }
        ChallengeUI.instance.OpenChallengeUI(challengeName, challengeObjectives);
        startButton.SetActive(false);
        OnStart.Invoke();
        InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Challenge.Reset, ResetChallenge);
        Debug.Log($"▶️Starting challenge: {challengeName}");
    }
    public void ResetChallenge(InputAction.CallbackContext context)
    {
        foreach (Toggle toggle in ChallengeUI.instance.objectivesChecks)
        {
            toggle.isOn = true;
        }
        ChallengeUI.instance.objectivesChecks[0].isOn = false;
        player.GetComponent<Rigidbody>().MovePosition(startPoint.position);
        currentTime = 0;
        OnStart.Invoke();
        Debug.Log($"🔁Reset challenge: {challengeName}");
    }
    public void ResetTheChallenge()
    {
        ResetChallenge(new InputAction.CallbackContext());
    }

    private void ChallengeUpdate()
    {
        if (restrictions.isPerformingRestriction(MC, WMC))
        {
            Debug.Log("⭕NOT ALOUD");
            ChallengeUI.instance.objectivesChecks[1].isOn = false;
        }
        currentTime += Time.deltaTime;
        ChallengeUI.instance.Timer.text = Mathf.Floor(currentTime).ToString();

        if (currentTime > maxTime)
        {
            ChallengeUI.instance.objectivesChecks[2].isOn = false;
        }

        if (!IsPlayerInsideTrigger())
        {
            LeftChallenge();
        }

        if (IsPlayerInsideAtEnd())
        {
            CompleteChallenge();
        }

    }
    private void Update()
    {
        if (challengeInProgress)
        {
            ChallengeUpdate();
        }
    }
    public void LeftChallenge()
    {
        InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Challenge.Reset, ResetChallenge);
        challengeInProgress = false;
        ChallengeUI.instance.CloseChallengeUI();
        if (forceRestrictions)
        {
            savedRestrictions.ApplyRestriction(null, MC, WMC);
        }
        startButton.SetActive(true);
        OnStop.Invoke();
        Debug.Log($"💀Challenge Left: {challengeName}");
    }
    public void CompleteChallenge()
    {
        InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Challenge.Reset, ResetChallenge);
        challengeInProgress = false;
        ChallengeUI.instance.objectivesChecks[0].isOn = true;
        ChallengeUI.instance.StartCoroutine(ChallengeUI.instance.EndingSequance(this));
        if (forceRestrictions)
        {
            savedRestrictions.ApplyRestriction(null, MC, WMC);
        }
        startButton.SetActive(true);
        OnStop.Invoke();
        Debug.Log($"🏁Challenge completed: {challengeName}");
    }

    bool IsPlayerInsideTrigger()
    {
        if (challengeBounds == null) return false;

        // Get all colliders inside the trigger bounds
        Collider[] hits = Physics.OverlapBox(challengeBounds.bounds.center, challengeBounds.bounds.extents);

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
                return true;
        }

        return false;
    }
    bool IsPlayerInsideAtEnd()
    {
        if (challengeBounds == null) return false;

        // Get all colliders inside the trigger bounds
        Collider[] hits = Physics.OverlapSphere(endZone.bounds.center, endZone.bounds.extents.magnitude);

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
                return true;
        }

        return false;
    }
    public void UpdateStars()
    {
        stars[0].SetActive(CloudSaveSystem.Instance.data.challengeDatas.Find(item => item.key == challengeName).hasReachedEnd);
        stars[1].SetActive(CloudSaveSystem.Instance.data.challengeDatas.Find(item => item.key == challengeName).hasNotUsedRestrictions);
        stars[2].SetActive(CloudSaveSystem.Instance.data.challengeDatas.Find(item => item.key == challengeName).hasReachedendWithinTime);
    }

}

[System.Serializable]
public class Restriction
{
    public bool jump = true;
    public bool doubleJump = true;
    public bool swipe = true;
    public bool dash = true;
    public bool surf = true;
    public bool ledgeGrab = true;
    public bool wallJump = true;
    public bool airDash = true;
    public bool dive = true;
    public bool leap = true;
    public bool twirlJump = true;
    public bool spinDash = true;
    public bool tricks = true;
    public bool parry = true;
    public bool railGrind = true;

    public Restriction(MovementController MC, WaterMovementController WMC)
    {
        GetRestriction(this, MC, WMC);
    }
    public void ApplyRestriction(Restriction RS, MovementController MC, WaterMovementController WMC)
    {
        if (RS == null) RS = this;
        if (MC == null || WMC == null) return;
        MC.jc.active = RS.jump;
        MC.av.sdj.active = RS.doubleJump;
        MC.lgv.active = RS.ledgeGrab;
        MC.wjv.active = RS.wallJump;
        MC.dv.active = RS.dash;
        MC.av.ad.active = RS.airDash;
        MC.av.div.active = RS.dive;
        MC.av.lv.active = RS.leap;
        MC.swv.active = RS.swipe;
        MC.av.tj.active = RS.twirlJump;
        MC.av.spd.active = RS.spinDash;
        MC.av.btv.active = RS.tricks;
        WMC.wtv.active = RS.tricks;
        MC.av.sp.active = RS.parry;
        MC.av.gv.active = RS.railGrind;
        MC.suv.active = RS.surf;
        WMC.suv.active = RS.surf;
    }
    public void GetRestriction(Restriction RS, MovementController MC, WaterMovementController WMC)
    {
        if (RS == null) RS = this;

        RS.jump = MC.jc.active;
        RS.doubleJump = MC.av.sdj.active;
        RS.ledgeGrab = MC.lgv.active;
        RS.wallJump = MC.wjv.active;
        RS.dash = MC.dv.active;
        RS.airDash = MC.av.ad.active;
        RS.dive = MC.av.div.active;
        RS.leap = MC.av.lv.active;
        RS.swipe = MC.swv.active;
        RS.twirlJump = MC.av.tj.active;
        RS.spinDash = MC.av.spd.active;
        RS.tricks = MC.av.btv.active;
        RS.tricks = WMC.wtv.active;
        RS.parry = MC.av.sp.active;
        RS.railGrind = MC.av.gv.active;
        RS.surf = MC.suv.active;
        RS.surf = WMC.suv.active;
    }

    public bool isPerformingRestriction(MovementController MC, WaterMovementController WMC)
    {
        bool hasPerformed = false;
        if (jump == false && MC.jc.jumping) hasPerformed = true;
        if (doubleJump == false && MC.av.sdj.jumped) hasPerformed = true;
        if (ledgeGrab == false && MC.lgv.ledgeGrabbing) hasPerformed = true;
        if (wallJump == false && MC.wjv.wallJumped) hasPerformed = true;
        if (dash == false && MC.dv.dashing) hasPerformed = true;
        if (airDash == false && MC.av.ad.airDashing) hasPerformed = true;
        if (dive == false && MC.av.div.diving) hasPerformed = true;
        if (leap == false && MC.av.lv.leaping) hasPerformed = true;
        if (swipe == false && MC.swv.swiping) hasPerformed = true;
        if (twirlJump == false && MC.av.tj.twirlJumping) hasPerformed = true;
        if (spinDash == false && MC.av.spd.spinDashing) hasPerformed = true;
        if (tricks == false && (MC.av.btv.shoveItAnimation || MC.av.btv.kickFlipAnimation || WMC.wtv.kickFlipAnimation || WMC.wtv.shoveItAnimation)) hasPerformed = true;
        if (parry == false && MC.av.sp.parryAnimation) hasPerformed = true;
        if (railGrind == false && MC.av.gv.grinding) hasPerformed = true;
        if (surf == false && (MC.suv.surfing || WMC.suv.surfing)) hasPerformed = true;


        return hasPerformed;
    }
}

[Serializable]
public class ChallengeData
{
    public string key;
    public bool hasReachedEnd = false;
    public bool hasNotUsedRestrictions = false;
    public bool hasReachedendWithinTime = false;

}
