using System;
using System.Collections.Generic;

[Serializable]
public class GameHistoryModel
{
    /// <summary>
    /// Unity always generate with the same name order, so we can just store the names to reload game.
    /// </summary>
    public List<string> Names;
}
