using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameObject dialogueCameraObject;
    [SerializeField] private GameObject flowChartCameraObject;
    [SerializeField] private Canvas flowChartCanvas;

    public void AlternateCameras()
    {
        dialogueCameraObject.SetActive(!dialogueCameraObject.activeSelf);
        flowChartCameraObject.SetActive(!dialogueCameraObject.activeSelf);
        flowChartCanvas.GetComponent<GraphicRaycaster>().enabled = !flowChartCanvas.GetComponent<GraphicRaycaster>().enabled;
    }
}
