using System;
using System.Collections.Generic;

public interface ICLIGame
{
    /// <summary>
    /// On game started
    /// </summary>
    Action OnStart { get; }

    /// <summary>
    /// Command list and its functions
    /// </summary>
    Dictionary<string, Action> InputFunctions { get; protected set; }

    /// <summary>
    /// Called when command not found in the dictionary
    /// </summary>
    Action<string> OnCommandNotFound  { get;}

    /// <summary>
    /// When given input is "help"
    /// </summary>
    Action OnHelpCommand { get; }
}
