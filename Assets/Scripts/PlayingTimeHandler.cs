using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayingTimeHandler : MonoBehaviour
{
    [SerializeField] string StartDateSaveName;
    [SerializeField] string PlayingTimeSaveName;

    private int playedTime;
    public int PlayingTime { get { return playedTime + (int)Time.time; } }

    private string startDate;
    public string StartDate { get { return startDate; } }
    // Start is called before the first frame update
    void Awake()
    {
        if (!PlayerPrefs.HasKey(StartDateSaveName))
        {
            PlayerPrefs.SetString(StartDateSaveName, DateTime.UtcNow.ToString("dd-MM-yyyy HH:mm:ss.fff"));
        }
        startDate = PlayerPrefs.GetString(StartDateSaveName);

        playedTime = PlayerPrefs.GetInt(PlayingTimeSaveName, 0);
    }

    public int SavePlayingTime()
    {
        PlayerPrefs.SetInt(PlayingTimeSaveName, PlayingTime);
        return PlayingTime;
    }
}
