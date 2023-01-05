using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameObject dialogueCameraObject;
    [SerializeField] private GameObject flowChartCameraObject;
    [SerializeField] private Canvas dialogueCanvas;
    [SerializeField] private Canvas flowChartCanvas;

    public void AlternateCameras()
    {
        //Switches view between dialogue and flowchart
        dialogueCameraObject.SetActive(!dialogueCameraObject.activeSelf);
        flowChartCameraObject.SetActive(!dialogueCameraObject.activeSelf);
        //Turns off respective graphic raycaster so can't mess with objects in the other canvas
        dialogueCanvas.GetComponent<GraphicRaycaster>().enabled = !dialogueCanvas.GetComponent<GraphicRaycaster>().enabled;
        flowChartCanvas.GetComponent<GraphicRaycaster>().enabled = !flowChartCanvas.GetComponent<GraphicRaycaster>().enabled;
    }
}
