using UnityEngine;

public class TutorialRestrictionZone : MonoBehaviour
{
    public Restriction restriction;
    private MovementController playerMovement;
    private WaterMovementController playerWaterMovement;
    public Color gizmoColor = Color.red;
    public bool showTutorial = true;
    public bool dontShowTutorial = false;
    public bool enableCompass = false;
    public TutorialUIPart tutorialPart;
    private void Start()
    {
        playerMovement = BlackBoard.playerBody.GetComponent<MovementController>();
        playerWaterMovement = BlackBoard.playerBody.GetComponent<WaterMovementController>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            restriction.ApplyRestriction(restriction, playerMovement, playerWaterMovement);
            // Show the tutorial text
            if (!dontShowTutorial)
            {
                UIManager.instance.tutorialPart = tutorialPart;
                UIManager.instance.ShowTutorial(showTutorial);
            }

            if (enableCompass)
                BlackBoard.compass.gameObject.SetActive(true);
            else
                BlackBoard.compass.gameObject.SetActive(false);
        }

    }

    private void OnDrawGizmos()
    {
        //draw the trigger box as a transparent box
        Gizmos.color = gizmoColor;
        Gizmos.DrawCube(GetComponent<BoxCollider>().bounds.center, GetComponent<BoxCollider>().bounds.size);
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

    private bool jumpPrevious = true;
    private bool doubleJumpPrevious = true;
    private bool swipePrevious = true;
    private bool dashPrevious = true;
    private bool surfPrevious = true;
    private bool ledgeGrabPrevious = true;
    private bool wallJumpPrevious = true;
    private bool airDashPrevious = true;
    private bool divePrevious = true;
    private bool leapPrevious = true;
    private bool twirlJumpPrevious = true;
    private bool spinDashPrevious = true;
    private bool tricksPrevious = true;
    private bool parryPrevious = true;
    private bool railGrindPrevious = true;

    public Restriction(MovementController MC, WaterMovementController WMC)
    {
        GetRestriction(this, MC, WMC);
    }
    public void ApplyRestriction(Restriction RS, MovementController MC, WaterMovementController WMC)
    {
        if (RS == null) RS = this;
        if (MC == null || WMC == null) return;
        GetRestriction(RS, MC, WMC);
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

    public void UndoRestriction(Restriction RS, MovementController MC, WaterMovementController WMC)
    {
        if (RS == null) RS = this;
        if (MC == null || WMC == null) return;
        MC.jc.active = RS.jumpPrevious;
        MC.av.sdj.active = RS.doubleJumpPrevious;
        MC.lgv.active = RS.ledgeGrabPrevious;
        MC.wjv.active = RS.wallJumpPrevious;
        MC.dv.active = RS.dashPrevious;
        MC.av.ad.active = RS.airDashPrevious;
        MC.av.div.active = RS.divePrevious;
        MC.av.lv.active = RS.leapPrevious;
        MC.swv.active = RS.swipePrevious;
        MC.av.tj.active = RS.twirlJumpPrevious;
        MC.av.spd.active = RS.spinDashPrevious;
        MC.av.btv.active = RS.tricksPrevious;
        WMC.wtv.active = RS.tricksPrevious;
        MC.av.sp.active = RS.parryPrevious;
        MC.av.gv.active = RS.railGrindPrevious;
        MC.suv.active = RS.surfPrevious;
        WMC.suv.active = RS.surfPrevious;
    }

    public void GetRestriction(Restriction RS, MovementController MC, WaterMovementController WMC)
    {
        if (RS == null) RS = this;

        RS.jumpPrevious = MC.jc.active;
        RS.doubleJumpPrevious = MC.av.sdj.active;
        RS.ledgeGrabPrevious = MC.lgv.active;
        RS.wallJumpPrevious = MC.wjv.active;
        RS.dashPrevious = MC.dv.active;
        RS.airDashPrevious = MC.av.ad.active;
        RS.divePrevious = MC.av.div.active;
        RS.leapPrevious = MC.av.lv.active;
        RS.swipePrevious = MC.swv.active;
        RS.twirlJumpPrevious = MC.av.tj.active;
        RS.spinDashPrevious = MC.av.spd.active;
        RS.tricksPrevious = MC.av.btv.active;
        RS.tricksPrevious = WMC.wtv.active;
        RS.parryPrevious = MC.av.sp.active;
        RS.railGrindPrevious = MC.av.gv.active;
        RS.surfPrevious = MC.suv.active;
        RS.surfPrevious = WMC.suv.active;
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
