using UnityEditor;
using UnityEngine.UIElements;

namespace Interrogation.Utilities
{
    public static class InterrogationStyleUtility
    {
        public static VisualElement AddStyleSheet(this VisualElement element, params string[] styleSheetNames)
        {
            foreach(string styleSheetName in styleSheetNames)
            {
                StyleSheet style = (StyleSheet) EditorGUIUtility.Load(styleSheetName);

                element.styleSheets.Add(style);
            }

            return element;
        }

        public static VisualElement AddStyleClasses(this VisualElement element, params string[] classNames)
        {
            foreach (string className in classNames)
            {
                element.AddToClassList(className);
            }

            return element;
        }
    }
}

