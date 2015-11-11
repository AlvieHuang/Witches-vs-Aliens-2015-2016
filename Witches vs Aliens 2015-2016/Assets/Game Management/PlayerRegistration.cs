﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

[RequireComponent(typeof(SetupData))]
public class PlayerRegistration : MonoBehaviour {

#if UNITY_EDITOR
    [SerializeField]
    protected string mainGameSceneName;
#else
    const string mainGameSceneName = Tags.Scenes.root;
#endif
    [SerializeField]
    protected GameObject playerRegistrationPrefab;

    [SerializeField]
    protected GameObject playerRegistrationUIPrefab;

    [SerializeField]
    protected PlayerRegisters[] possiblePlayers;

    SetupData data;
    Transform UIParent;
    CharacterSelector[] playerSelections;
    RegisteredPlayerUIView[] playerUI;
    bool[] playersReady;
    bool[] previousAxisInputNonzero; //create edge-trigger instead of constant flipping

    Coroutine startCountdown;
	void Awake ()
    {
        data = GetComponent<SetupData>();

        playerSelections = new CharacterSelector[possiblePlayers.Length];
        playerUI = new RegisteredPlayerUIView[possiblePlayers.Length];

        playersReady = new bool[possiblePlayers.Length];
        previousAxisInputNonzero = new bool[possiblePlayers.Length];
        for (int i = 0; i < possiblePlayers.Length; i++)
        {
            playersReady[i] = false;
            previousAxisInputNonzero[i] = false;
        }
	}

    void Start()
    {
        UIParent = GameObject.FindGameObjectWithTag(Tags.canvas).transform.Find("RegisteredPlayers");
    }

    void Update()
    {
        for (int i = 0; i < possiblePlayers.Length; i++)
        {
            if (playerSelections[i] == null)
            {
                if (possiblePlayers[i].bindings.inputMode == InputConfiguration.PlayerInputType.MOUSE && Input.GetMouseButtonDown(0)
                    || possiblePlayers[i].bindings.inputMode == InputConfiguration.PlayerInputType.JOYSTICK && Input.GetAxis(possiblePlayers[i].bindings.AcceptAxis) != 0) //register
                {
                    GameObject spawnedPlayerRegistationPuck = (GameObject)Instantiate(playerRegistrationPrefab, Vector2.zero, Quaternion.identity); //the positions are temporary
                    switch (possiblePlayers[i].bindings.inputMode)
                    {
                        case InputConfiguration.PlayerInputType.MOUSE:
                            spawnedPlayerRegistationPuck.AddComponent<MousePlayerInput>().bindings = possiblePlayers[i].bindings;
                            break;
                        case InputConfiguration.PlayerInputType.JOYSTICK:
                            spawnedPlayerRegistationPuck.AddComponent<JoystickPlayerInput>().bindings = possiblePlayers[i].bindings;
                            break;
                    }

                    //spawn them
                    playerSelections[i] = spawnedPlayerRegistationPuck.AddComponent<CharacterSelector>();
                    playerUI[i] = SimplePool.Spawn(playerRegistrationUIPrefab).GetComponent<RegisteredPlayerUIView>();
                    playerUI[i].transform.SetParent(UIParent, Vector3.one, false);
                    InputToAction action = spawnedPlayerRegistationPuck.GetComponent<InputToAction>();
                    action.rotationEnabled = false;
                    action.movementEnabled = true;
                    playerUI[i].inputMode = possiblePlayers[i].bindings.inputMode;
                    playerUI[i].ready = false;
                    spawnedPlayerRegistationPuck.GetComponentInChildren<Image>().color = possiblePlayers[i].color;
                    playerUI[i].playerColor = possiblePlayers[i].color;
                    spawnedPlayerRegistationPuck.GetComponentInChildren<Text>().text = possiblePlayers[i].abbreviation;
                    playerUI[i].playerName = possiblePlayers[i].name;
                }
            }
            else
            {
                //check if ready
                if (playerSelections[i].selectedCharacter != null)
                {
                    if (possiblePlayers[i].bindings.inputMode == InputConfiguration.PlayerInputType.MOUSE && Input.GetMouseButtonDown(0))
                    {
                            //toggle ready
                        playerUI[i].ready = playersReady[i] = !playersReady[i];

                    }
                    else if (possiblePlayers[i].bindings.inputMode == InputConfiguration.PlayerInputType.JOYSTICK)
                    {
                        if (Input.GetAxis(possiblePlayers[i].bindings.AcceptAxis) != 0)
                        {
                            if (!previousAxisInputNonzero[i])
                            {
                                playerUI[i].ready = playersReady[i] = !playersReady[i]; //toggle
                            }
                            previousAxisInputNonzero[i] = true;
                        }
                        else
                        {
                            previousAxisInputNonzero[i] = false;
                        }
                    }
                }

                if (possiblePlayers[i].bindings.inputMode == InputConfiguration.PlayerInputType.MOUSE && Input.GetMouseButtonDown(1)
                    || possiblePlayers[i].bindings.inputMode == InputConfiguration.PlayerInputType.JOYSTICK && Input.GetAxis(possiblePlayers[i].bindings.genericAbilityAxis) != 0) //deregister
                {
                    Destroy(playerSelections[i].gameObject);
                    playerSelections[i] = null;
                    playerUI[i].Despawn();
                    playerUI[i] = null;
                    playersReady[i] = false;
                }

                
            }
        }
        bool allReady = true;
        bool oneReady = false;
        for (int i = 0; i < playersReady.Length; i++)
        {
            if (playerSelections[i] != null)
            {
                oneReady = true;
                if (!playersReady[i])
                    allReady = false;
            }
        }
        if (startCountdown == null)
        {
            if (oneReady && allReady)
            {
                startCountdown = Callback.FireAndForget(startGame, 5, this);
                Debug.Log("Counting Down");
            }
        }
        else
        {
            if (!oneReady || !allReady)
            {
                StopCoroutine(startCountdown);
                startCountdown = null;
            }
        }
    }

    void startGame()
    {
        int count = 0;
        for(int i = 0; i < playerSelections.Length; i++)
        {
            if (playerSelections[i] != null && playerSelections[i].selectedCharacter != null)
                count++;
        }
        //if count == 0 do something to reset
        data.playerComponentPrefabs = new PlayerComponents[count];

        count = 0;
        for (int i = 0; i < playerSelections.Length; i++)
        {
            if (playerSelections[i] != null && playerSelections[i].selectedCharacter != null)
                data.playerComponentPrefabs[count++] = new PlayerComponents(playerSelections[i].selectedCharacter, possiblePlayers[i].bindings);
        }
        Application.LoadLevel(mainGameSceneName);
        Destroy(this);
    }
}

[System.Serializable]
public class PlayerRegisters
{
    public string name;
    public string abbreviation;
    public Color color;
    public InputConfiguration bindings;
    public PlayerRegisters() { }
}