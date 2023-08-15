using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace Interrogation.Elements
{
    using Windows;
    using Utilities;

    public class SuspectGroup : Group
    {
        string suspectName;

        public SuspectGroup(InterrogationGraphView interrogationGraphView)
        {
            TextField suspectNameField = InterrogationElementUtility.CreateTextField(suspectName, "Partner Name:", callback =>
            {
                suspectName = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();
            });

            this.Add(suspectNameField);
        }
    }
}
