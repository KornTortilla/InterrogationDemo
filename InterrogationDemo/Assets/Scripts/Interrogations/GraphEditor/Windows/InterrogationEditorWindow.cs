using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Interrogation.Windows
{
    using Utilities;

    public class InterrogationEditorWindow : EditorWindow
    {
        private InterrogationGraphView graphView;

        private readonly string defaultFileName = "InterrogationFile";
        private static TextField fileNameTextField;
        private Button saveButton;

        //Allows the method to show on the unity toolbar
        [MenuItem("Window/Interrogation System/Dialogue Graph")]
        public static void Open()
        {
            //Instantiates Editor Window
            GetWindow<InterrogationEditorWindow>("Dialogue Graph");
        }

        private void CreateGUI()
        {
            AddGraphView();
            
            AddToolBar();

            AddStyles();
        }

        private void AddGraphView()
        {
            //Creates the graph view, stretches it to the size of the editor window, and adds to the root
            graphView = new InterrogationGraphView(this);

            graphView.StretchToParentSize();

            rootVisualElement.Add(graphView);
        }

        private void AddToolBar()
        {
            //Creates a toolbar at the top for various functions
            Toolbar toolbar = new Toolbar();

            fileNameTextField = InterrogationElementUtility.CreateTextField(defaultFileName, "File Name:", callback =>
            {
                //Actively changes value of field without spaces and special characters
                fileNameTextField.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();
            });

            //Creates buttons and ties press to functions
            saveButton = InterrogationElementUtility.CreateButton("Save", () => Save());
            Button loadButton = InterrogationElementUtility.CreateButton("Load", () => Load());
            Button clearButton = InterrogationElementUtility.CreateButton("Clear", () => Clear());
            Button miniMapButton = InterrogationElementUtility.CreateButton("Mini Map", () => graphView.ToggleMiniMap());

            toolbar.Add(fileNameTextField);
            toolbar.Add(saveButton);
            toolbar.Add(loadButton);
            toolbar.Add(clearButton);
            toolbar.Add(miniMapButton);

            rootVisualElement.Add(toolbar);
        }

        private void Save()
        {
            //If non-empty filen name given, will save
            if (string.IsNullOrEmpty(fileNameTextField.value))
            {
                EditorUtility.DisplayDialog(
                    "Invalid file name.",
                    "Please make sure the file name you've got is valid.",
                    "Okay"
                );

                return;
            }

            InterrogationIOUtility.Intialize(graphView, fileNameTextField.value);
            InterrogationIOUtility.Save();
        }

        private void Load()
        {
            //If file selected, clears current graph and loads new one
            string filePath = EditorUtility.OpenFilePanel("Interrogation Graphs", "Assets/Scripts/Interrogations/GraphEditor/SavedGraphs", "asset");

            if(string.IsNullOrEmpty(filePath))
            {
                return;
            }

            Clear();

            InterrogationIOUtility.Intialize(graphView, Path.GetFileNameWithoutExtension(filePath));
            InterrogationIOUtility.Load();
        }

        private void Clear()
        {
            graphView.ClearGraph();
        }

        public static void UpdateFileName(string fileName)
        {
            fileNameTextField.value = fileName;
        }

        public void EnableSaveButton()
        {
            saveButton.SetEnabled(true);
        }

        public void DisableSaveButton()
        {
            saveButton.SetEnabled(false);
        }

        private void AddStyles()
        {
            //Loads variables to be used by styles
            rootVisualElement.AddStyleSheet("InterrogationSystem/Variables.uss");
        }
    }
}

