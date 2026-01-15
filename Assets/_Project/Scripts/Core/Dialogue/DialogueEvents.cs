using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class DialogueEvents
{
    public event Action<string> onEnterDialogue;

    public void EnterDialogue(string knotname)
    {
        if (onEnterDialogue != null)
        {
            onEnterDialogue(knotname);
        }
    }
}
