/* Ideas
 * create tips array and use get it by typing "help"
 * give a random tip to user randomly
 */
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using Unity.VisualScripting;
using Color = UnityEngine.Color;
using System.Threading.Tasks;
using System.Net.Http;

public class WordJumbleConsoleGame : MonoBehaviour, ICLIGame
{
    [Header("Preferences")]
    [Tooltip("Unity sends \"feedback\" every \"SendingDataDensity\" turn")]
    [SerializeField] int SendingDataDensity;

    [SerializeField] Color ScoreHighlightColor;

    [SerializeField] Color PointHighlightColor;

    [SerializeField] Color LosePointHighlightColor;

    [SerializeField] Color ScrumbledWordHighlightColor;

    [SerializeField] int HypenAmount;

    [Header("Instances")]
    [SerializeField] ConsoleManager Console;

    [SerializeField] PlayingTimeHandler playingTimeHandler;

    [SerializeField] FeedBackHandler feedBackHandler;


    WordsClass words;
    List<string> Words { get { return words.Words; }
    }
    int IndexOfWord;

    string ScrumbledWord;

    int PlayedTurn;

    int BestScore;
    int Score = 0;
    [SerializeField] int ScoreMultiplier = 100;
    int currentQuestionScore;
    int CurrentQuestionScore
    {
        get { return currentQuestionScore; }
        set
        {
            currentQuestionScore = value <= 0 ? 0 : value;
        }
    }
    int currentQuestionLoseScore;
    int CurrentQuestionLoseScore
    {
        get { return currentQuestionLoseScore; }
        set
        {
            currentQuestionLoseScore = value <= 0 ? 0 : value;
        }
    }

    Dictionary<string, WordsClass> languages = new Dictionary<string, WordsClass>()
    {
        {WordsClass.Turkish.Name, WordsClass.Turkish},
        {WordsClass.German.Name, WordsClass.German},
        {WordsClass.English.Name, WordsClass.English},
    };

    Action ICLIGame.OnStart { get => OnStart; }

    private Dictionary<string, Action> InputFunctions;
    Dictionary<string, Action> ICLIGame.InputFunctions { get { return InputFunctions; } set { InputFunctions = value; } }

    Action<string> OnCommandNotFoundDelegate;
    Action<string> ICLIGame.OnCommandNotFound { get { return OnCommandNotFoundDelegate; }  }

    public Action OnHelpCommand { get { return Help; } }

    Action<object> Print, DPrint;

    Dictionary<string, Action> DefaultInputFunctions = new Dictionary<string, Action>();

    string[] Tips = new string[] 
    {
        "Don't forget that if you struggle so much, type \"give a tip\"",
        "Don't forget that you can find all game commands by typing \"help\""
    };

    string RandomTip { get { return Tips[Random.Range(0, Tips.Length)]; } }

    static class PlayerPrefsNames
    {
        public const string Score = "Score";
        public const string BestScore = "Best Score";
        public const string Language = "Language";
        public const string PlayedTurn = "Played Turn";
    }

    void DefaultInputFunctionsInitializer()
    {
        Dictionary<string, Action> _DefaultInputFunctions = new Dictionary<string, Action>()
        {
            {"help", OnHelpCommand},

            {"new game", NewGame},
            {"give a tip", GiveATip},

            {"switch to tr", SwitchToTurkish},
            {"switch to en", SwitchToEnglish},
            {"switch to de", SwitchToGerman},

            {"save",Save},
            {"delete save", DeleteSave },

            {"feedback", SendFeedbackFromUser },

            {"debug mode on",DebugModeOn},
            {"debug mode off",DebugModeOff},
        };
        this.DefaultInputFunctions = _DefaultInputFunctions;

    }

    private void OnAwake()
    {
        Print = Console.Print;
        DPrint = Console.DPrint;
        DefaultInputFunctionsInitializer();
        InputFunctions = DefaultInputFunctions;

        OnCommandNotFoundDelegate = OnCommandNotFound;

        BestScore = PlayerPrefs.GetInt(PlayerPrefsNames.BestScore, 0);

        PlayedTurn = PlayerPrefs.GetInt(PlayerPrefsNames.PlayedTurn, 0);
    }

    private void OnStart()
    {
        OnAwake();
        Score = PlayerPrefs.GetInt(PlayerPrefsNames.Score, 0);
        words = languages[PlayerPrefs.GetString(PlayerPrefsNames.Language, WordsClass.English.Name)];

        Print("Unscramble the letters to make a word.");
        Print("If you struggle so much, type \"give a tip\"");
        Print("Type \"help\" to see all available commands");
        //Print("You can change language by typing \"switch to \"[language] ");
        //Print("Eg: \"switch to en\" command switches to english words. Available language codes: tr = Turkish, de = German, en = English");
        newTurn();
    }
    void newTurn()
    {
        PlayedTurn++;
        PlayerPrefs.SetInt(PlayerPrefsNames.PlayedTurn, PlayedTurn);

        Print("Your score is " + CombineHighLight(ScoreHighlightColor, Score.ToString()));

        IndexOfWord = Random.Range(0, Words.Count);
        ScrumbledWord = ScrumbleWord(Words[IndexOfWord]);

        Print("Here is a new jumbled word:");
        Print(CombineHighLight(ScrumbledWordHighlightColor, ScrumbledWord));
        Print("");

        CalculatePoints();

        if (Random.Range(0, 2) == 1)
            Print(RandomTip);

            Print("Enter your guess:");

        //send statistics periodically
        if(PlayedTurn%SendingDataDensity == 0)
            SendFeedBack();
    }

    void CalculatePoints()
    {
        CurrentQuestionScore = CalculateWinPoint(Words[IndexOfWord], ScrumbledWord, ScoreMultiplier);
        CurrentQuestionLoseScore = CalculateLosePoint(Words[IndexOfWord].Length, currentQuestionScore, ScoreMultiplier);

        Print("You will earn " + CombineHighLight(PointHighlightColor, "" + CurrentQuestionScore)
            + " point if you guess the word!");
        Print("You will lose " + CombineHighLight(LosePointHighlightColor, "" + currentQuestionLoseScore)
            + " point if you can't guess the word!");
    }

    Task<bool> SendFeedBack(string feedback = null, string email = null)
    {
        string Id = playingTimeHandler.StartDate;
        string consoleLog = Console.GetConsoleLog();
        int playingTimeInSeconds = playingTimeHandler.SavePlayingTime();
        int playingTimeInMinutes = playingTimeInSeconds / 60;
        string language = words.Name;
        string systemLanguage = Application.systemLanguage.ToString();
        var result = feedBackHandler.SendFeedback(Id, feedback, consoleLog,
            "" + playingTimeInSeconds , "" + playingTimeInMinutes, "" + BestScore, language, "" + PlayedTurn 
            ,systemLanguage, email);
        return result;
    }

    #region Commands for input

    string feedback;
    void SendFeedbackFromUser()
    {
        Print("Did you like my game? Is there any bug? Please write your thoughts:");

        // make it empty so user don't enter a command and writes feedback.
        InputFunctions = new Dictionary<string, Action>();

        // Apply your code. Then run the game again
        OnCommandNotFoundDelegate = (_feedback) =>
        {
            this.feedback = _feedback;
            Print("If you enter your email I can send you a feedback when I solve the bug or contact for the bug. Also you can get informations about my games.");
            Print("Please enter your email");
            Print("Enter \"no\" if you don't want to enter your email");
            OnCommandNotFoundDelegate = async (_email) =>
            {
                string email = "";
                if (_email.Contains("@"))
                {
                    email = _email;
                }
                var result = await SendFeedBack(feedback, email);
                if (result)
                {
                    Print("Your feedback has sent succesfully");
                    Print("Thank you for your feedback :)");
                }
                else
                {
                    Print("Your feedback could not be sent");
                    Print("Maybe your internet connection is not good or there is another connection problem");
                    Print("Please try again later");
                }

                //Return to default settings
                InputFunctions = DefaultInputFunctions;
                OnCommandNotFoundDelegate = OnCommandNotFound;
                newTurn();
            };
         };
    }

    void Help()
    {
        Print("All available commands are below:");
        foreach (var item in DefaultInputFunctions.Keys)
        {
            Print(item);
        }
    }

    void Save()
    {
        PlayerPrefs.SetInt(PlayerPrefsNames.Score, Score);
        PlayerPrefs.SetString(PlayerPrefsNames.Language, words.Name);
        CheckBestScore();
        Print("Game saved...");
    }
    void CheckBestScore()
    {
        if (Score > BestScore)
        {
            BestScore = Score;
            PlayerPrefs.SetInt(PlayerPrefsNames.BestScore, BestScore);
            /*if(Score % (ScoreMultiplier *10) == 0)
                Print("Congratulations this is your new best score!");*/
        }
    }

    void NewGame()
    {
        Console.ClearConsole();
        Print("new game");
        Print("New game starting...");
        PlayerPrefs.DeleteKey(PlayerPrefsNames.Score);
        OnStart();
    }

    private void GiveATip()
    {
        string temp = ScrumbledWord;
        ScrumbledWord = UnscrumbleLetter(Words[IndexOfWord], ScrumbledWord);
        if(temp == ScrumbledWord)
        {
            Print("This word can't be more unscrumbled");
            Print("");
            Print(CombineHighLight(ScrumbledWordHighlightColor, ScrumbledWord));
            CalculatePoints();
            return;
        }
        Print("Here is a bit Unscrumbled version:");
        Print(CombineHighLight(ScrumbledWordHighlightColor, ScrumbledWord));
        Print("");
        CalculatePoints();
    }

    void DebugModeOn()
    {
        Console.DebugMode = true;
    }
    void DebugModeOff()
    {
        Console.DebugMode = false;
    }

    void SwitchLanguage(WordsClass wordsClass)
    {
        this.words = wordsClass;
        PlayerPrefs.SetString(PlayerPrefsNames.Language, words.Name);
        Print("Word language switched to " + wordsClass.Name);
        Print("There are " + wordsClass.Words.Count + " " + wordsClass.Name + " words in data");
        newTurn();
    }
    void SwitchToTurkish()
    {
        SwitchLanguage(WordsClass.Turkish);
    }
    void SwitchToEnglish()
    {
        SwitchLanguage(WordsClass.English);
    }
    void SwitchToGerman()
    {
        SwitchLanguage(WordsClass.German);
    }

    void DeleteSave()
    {
        PlayerPrefs.DeleteKey(PlayerPrefsNames.Score);
        PlayerPrefs.DeleteKey(PlayerPrefsNames.BestScore);
        PlayerPrefs.DeleteKey(PlayerPrefsNames.Language);

        //back to default:
        Score = 0;
        BestScore = 0;
        words = WordsClass.English;

        Print("Player save deleted");
        Print("Best score is " + BestScore);
        hypen();
        Print("Type \"new game\" for new game");
    }

    void OnCommandNotFound(string Input)
    {
        if (Input.Contains(' '))
        {
            Print("Do not enter space");
        }
        else if (Input == Words[IndexOfWord])
        {
            hypen();
            Print("You win!");
            Score += CurrentQuestionScore;
            CheckBestScore();
            Print("Best score is " + BestScore);
            newTurn();
        }
        else
        {
            hypen();
            Print("The right answer was " + Words[IndexOfWord]);
            Print("But your answer is " + Input);

            Score -= CurrentQuestionLoseScore;
            Print("You lost " + CombineHighLight(LosePointHighlightColor, 
                ""+CurrentQuestionLoseScore) + " points");

            if (CheckLosing(Score, Input))
                return;
            Print("Sorry, try again.");
            newTurn();
        }
    }
    #endregion

    bool CheckLosing(int score, string input)
    {
        if(score <= 0)
        {
            Console.ClearConsole();
            Print(input);
            hypen();
            Print("The right answer was " + Words[IndexOfWord]);
            Print("But your answer is " + input);

            Score -= CurrentQuestionLoseScore;
            Print("You lost " + CombineHighLight(LosePointHighlightColor,
                "" + CurrentQuestionLoseScore) + " points");
            Print("Your score is " + score);
            Print("You lost");
            Print("Best score " + BestScore);

            Print("new game");
            Print("New game starting...");
            PlayerPrefs.DeleteKey(PlayerPrefsNames.Score);
            OnStart();
            return true;
        }
        return false;
    }

    string ScrumbleWord(string word)
    {
        char[] CharWord = word.ToCharArray();
        for (int i = 0; i < word.Length; i++)
        {
            int randomIndex2 = Random.Range(0, CharWord.Length - 0);
            char temp = CharWord[randomIndex2];
            CharWord[randomIndex2] = CharWord[i];
            CharWord[i] = temp;
        }
        return new string(CharWord);
    }

    string UnscrumbleLetter(string word, string scrumbledWord, int howMuchLetterShouldBeUnscrumbled = 1)
    {
            Dictionary<char, int> wordDictionary = new Dictionary<char, int>();
            for (int i = 0; i < word.Length; i++)
            {
                wordDictionary.TryAdd(word[i], i);
            }

            for (int i = 0; i < howMuchLetterShouldBeUnscrumbled; i++)
            {
                while (true)
                {
                    int RandIndex = Random.Range(0, wordDictionary.Count);
                    char key = wordDictionary.Keys.ToArray()[RandIndex];
                    int index = wordDictionary[key];
                    if (word[index] == scrumbledWord[index])
                    {
                        wordDictionary.Remove(key);
                        if (wordDictionary.Count == 0)
                        {
                            return scrumbledWord;
                        }
                        continue;
                    }

                    char temp = scrumbledWord[index];
                    scrumbledWord = scrumbledWord.Remove(index, 1);
                    scrumbledWord = scrumbledWord.Insert(index, "$");

                    scrumbledWord = ReplaceString(scrumbledWord, scrumbledWord.IndexOf(word[index]), temp);

                    scrumbledWord = scrumbledWord.Remove(index, 1);
                    
                    scrumbledWord = scrumbledWord.Insert(index, word[index].ToString());

                    wordDictionary.Remove(key);

                    if (wordDictionary.Count == 0)
                        return scrumbledWord;

                    break;
                }
            }
            return scrumbledWord;
    }

    string ReplaceString(string str, int index, char NewValue)
    {
        char[] temp = str.ToCharArray();
        temp[index] = NewValue;

        return new string(temp);
    }

    int CalculateWinPoint(string word, string scrumbledWord, int multiplier = 100)
    {
        int point = 0;
        for (int i = 0; i < word.Length; i++)
        {
            if (word[i] != scrumbledWord[i])
                point += multiplier;
        }
        return point;
    }

    int CalculateLosePoint(string word, string scrumbledWord, int multiplier = 100)
    {
        int point = 0;
        for (int i = 0; i < word.Length; i++)
        {
            if (word[i] == scrumbledWord[i])
                point += multiplier;
        }
        return point;
    }
    int CalculateLosePoint(int wordLength, int CurrentPoint, int multiplier = 100)
    {

        return wordLength * multiplier - CurrentPoint;
    }

    string CombineHighLight(Color color, string text)
    {
        string hex = color.ToHexString();
        hex = "#" + hex.Substring(0, hex.Length - 2);
        return "<" + hex + ">" + text + "</color >";
    }

    /// <summary>
    /// returns hypens
    /// </summary>
    /// <returns>"-------------"</returns>
    string hypen(int hypenAmount = 0)
    {
        string Hypens = "";
        if (hypenAmount <= 0) hypenAmount = HypenAmount;
        for (int i = 0; i < hypenAmount; i++)
        {
            Hypens += '-';
        }
        Print(Hypens);
        return Hypens;
    }

    [EButton("PlayerPrefs.DeleteAll()")]
    void DeleteSaveInEditor()
    {
        PlayerPrefs.DeleteAll();
        print("Saves deleted");
    }
}
