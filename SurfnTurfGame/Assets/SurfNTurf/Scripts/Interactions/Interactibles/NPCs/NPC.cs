using System;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Cinemachine;
using Google.Apis.Util;
using NaughtyAttributes;

public enum ConvoType
{
    normal,
    completedQuest,
    failedQuest,
    DontWantDish
}

public class NPC : Interactible, IDishGetter
{
    [SerializeField]
    Canvas inputPromptCanvas;
    RectTransform textBubble;
    [SerializeField]
	CinemachineCamera npcCamera;

    public bool TextBubbleLooksAtCamera;

    public TalkingUI talkingUi;
    
    public Conversation convo;

	public Conversation[] convos;

    int currentConvoIndex;
    ConvoType currentConvoType;

    int convoIndex;
    bool talking;
    bool servingManagerOpened;



    public PlateHolder currentDish { get; private set; }

	public override void Initialize()
	{
		base.Initialize();
        talkingUi.Initialize();
	}
	public override bool InteractWith()
	{
        string[] currentConvo = convos[currentConvoIndex].sentences;

        switch (currentConvoType)
        {
            case ConvoType.normal:
                currentConvo = convos[currentConvoIndex].sentences;
                break;
			case ConvoType.completedQuest:
				currentConvo = convos[currentConvoIndex].completedQuestSentences;
				break;
			case ConvoType.failedQuest:
				currentConvo = convos[currentConvoIndex].failedQuestSentences;
				break;
			case ConvoType.DontWantDish:
				currentConvo = convos[currentConvoIndex].IDontWantADishSentences;
				break;
		}
        if (convoIndex == 0)
        {
            if (!talking)
            {
                SpawnTextBubble();
				switch (currentConvoType)
				{
					case ConvoType.normal:
                        BlackBoard.cameraController.SwitchToCamera(npcCamera, 0.5f);
						break;
					case ConvoType.completedQuest:
                        if (convos[currentConvoIndex].QuestCompletedCamera != null)
                            BlackBoard.cameraController.SwitchToCamera(convos[currentConvoIndex].QuestCompletedCamera, 0.5f);
						break;
					case ConvoType.failedQuest:
						if (convos[currentConvoIndex].QuestFailedCamera != null)
							BlackBoard.cameraController.SwitchToCamera(convos[currentConvoIndex].QuestFailedCamera, 0.5f);
						break;
					case ConvoType.DontWantDish:
						if (convos[currentConvoIndex].IDontWantADishCamera != null)
							BlackBoard.cameraController.SwitchToCamera(convos[currentConvoIndex].IDontWantADishCamera, 0.5f);
						break;
				}
            }
			talkingUi.SetTitle(convos[currentConvoIndex].myName);

        }
        if (convoIndex < currentConvo.Length)
        {
            talkingUi.SetText(currentConvo[convoIndex]);
            convoIndex++;

        }
        else
        {
            //if there's a quest included, give it to the player
            if (convos[currentConvoIndex].hasQuest && convos[currentConvoIndex].quest.objectives.Length > 0 && !servingManagerOpened)
            {
                if (!BlackBoard.myquests.ContainsKey(convos[currentConvoIndex].quest.questName))
                    BlackBoard.myquests.Add(convos[currentConvoIndex].quest.questName, convos[currentConvoIndex].quest);

                //open the serving manager?
                if (convos[currentConvoIndex].menuOpenCamera != null)
                    BlackBoard.cameraController.SwitchToCamera(convos[currentConvoIndex].menuOpenCamera, 0.5f);
				ServingManager.instance.OpenServingMenu(this);
                //should call on GiveDish(); in this script

                servingManagerOpened = true;
				return true;
			}
			else
            {
                //go to the next convo type
                if (currentConvoType == ConvoType.completedQuest || convos[currentConvoIndex].automaticallyGoesToNextConvo && currentConvoType != ConvoType.DontWantDish)
                    currentConvoIndex++;

                ServingManager.instance.CloseServingMenu();
                DespawnTextBubble();
				return false;
			}
		}
        return true;
	}

	private void Update()
	{
		if(talking && TextBubbleLooksAtCamera)
            talkingUi.transform.forward = new Vector3((talkingUi.transform.position - Camera.main.transform.position).x, 0, (talkingUi.transform.position - Camera.main.transform.position).z).normalized;
	}

    public override bool Exit()
    {
        DespawnTextBubble();
        return false;
    }
    void SpawnTextBubble()
    {
        talking = true;
		talkingUi.SpawnTextBubble(TextBubbleType.sweet);
    }

    void DespawnTextBubble()
    {
        talking = false;
        convoIndex = 0;
        servingManagerOpened = false;
		talkingUi.DespawnTextBubble();
	}


	public override void Highlight()
    {
        base.Highlight();
        //show a little button above the character that indicates interaction
        if(!inputPromptCanvas.gameObject.activeSelf)
            inputPromptCanvas.gameObject.SetActive(true);

        inputPromptCanvas.transform.forward = (inputPromptCanvas.transform.position - Camera.main.transform.position).normalized;
    }

    public override void RemoveHighlight()
    {
        base.RemoveHighlight();
        inputPromptCanvas.gameObject.SetActive(false);
    }

	//IDishGetter Values

	public bool GiveDish(PlateHolder _dish)
    {
        if (convos[currentConvoIndex].quest.objectives.Length == 0)
        {
            //say something about it not wanting a dish
            currentConvoType = ConvoType.DontWantDish;
			convoIndex = 0;
			return false;
		}

		currentDish = _dish;
        //check what the current convo is and if the dish aligns with the quest
        convos[currentConvoIndex].quest.CheckQuest(this);
        currentDish = null;

        bool completedQuest = convos[currentConvoIndex].quest.CheckIfFinished();

        if (!completedQuest)
        {
			//say something about it not being the correct dish
			currentConvoType = ConvoType.failedQuest;
            convoIndex = 0;
		}
		else
        {
			//say something about it being the correct dish
			currentConvoType = ConvoType.completedQuest;
			convoIndex = 0;
		}
		return completedQuest;
	}
}

[Serializable]
public class Conversation
{
    public string myName;
    public string[] sentences;
    public bool automaticallyGoesToNextConvo;

    public bool hasQuest;

	public Quest quest;

	public CinemachineCamera menuOpenCamera;

	public CinemachineCamera QuestCompletedCamera;

	public string[] completedQuestSentences;
    public CinemachineCamera QuestFailedCamera;
    public string[] failedQuestSentences;
    public CinemachineCamera IDontWantADishCamera;
    public string[] IDontWantADishSentences;

}
