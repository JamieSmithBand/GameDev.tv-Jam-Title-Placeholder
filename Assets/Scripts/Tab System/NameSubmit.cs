﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Responsible for submitting the player name.
/// </summary>

public class NameSubmit : MonoBehaviour
{
    public TMP_InputField playerName;

    public PlayerStats player;

    public State state;

    StoryManager storyManager;

    private void Start()
    {
        storyManager = StoryManager.instance;
    }

    public void SubmitName()
    {
        if (playerName.text != "")
        {
            player.characterName = playerName.text;

            storyManager.AdvanceStory(state);

            this.gameObject.SetActive(false);
        }
    }
}
