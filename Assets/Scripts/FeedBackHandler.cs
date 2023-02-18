using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

public class FeedBackHandler : MonoBehaviour
{
    [SerializeField] string FormId;

    [Header("Entry Ids")]
    [SerializeField] string IdEId;
    [SerializeField] string FeedbackEId;
    [SerializeField] string EMailEId;
    [SerializeField] string ConsoleLogEId;
    [SerializeField] string PlayingTimeInSecondsEId;
    [SerializeField] string PlayingTimeInMinutesEId;
    [SerializeField] string BestScoreEId;
    [SerializeField] string LanguageEId;
    [SerializeField] string SystemLanguageEId;
    [SerializeField] string PlayedTurnEId;


    string FormUrl {
        get { return "https://docs.google.com/forms/d/e/" + FormId + "/formResponse"; }
    }

    public async Task<bool> SendFeedback(string Id, string Feedback, string ConsoleLog,
        string PlayingTimeInSeconds, string PlayingTimeInMinutes,
        string BestScore, string Language, string PlayedTurn, string SystemLanguage,
        string EMail = null)
    {

        var submissionService = new GoogleFormsSubmissionService(FormUrl);

        //just for shorting the codes
        string e = "entry.";
        var fields = new Dictionary<string, string>
        {
            { e + IdEId, Id},
            { e + FeedbackEId, Feedback},
            { e + ConsoleLogEId, ConsoleLog},
            { e + PlayingTimeInSecondsEId, PlayingTimeInSeconds},
            { e + PlayingTimeInMinutesEId, PlayingTimeInMinutes},
            { e + BestScoreEId, BestScore},
            { e + LanguageEId, Language},
            { e + SystemLanguageEId, SystemLanguage},
            { e + PlayedTurnEId, PlayedTurn},
        };

        if (!string.IsNullOrEmpty(EMail))
        {
            fields.Add("entry." + EMailEId, EMail);
        }
        submissionService.SetFieldValues(fields);
        var result = await submissionService.SubmitAsync();

        return result;
    }

}
