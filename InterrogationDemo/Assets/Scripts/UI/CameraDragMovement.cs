using UnityEngine;

public class CameraDragMovement : MonoBehaviour
{
    private Camera flowCamera;

    private Vector3 touchStart;

    private float defaultZoom;
    private float targetZoom;
    [SerializeField] private float zoomFactor;
    [SerializeField] private float zoomLerpSpeed;

    void Start()
    {
        flowCamera = GetComponent<Camera>();
        defaultZoom = flowCamera.orthographicSize;
        targetZoom = flowCamera.orthographicSize;
    }

    void Update()
    {
        //On first frame of mouse click, get starting position
        if (Input.GetMouseButtonDown(0))
        {
            touchStart = flowCamera.ScreenToWorldPoint(Input.mousePosition);
        }
        //On all frames that the mouse is down, add to the position based on the difference between the start and new mouse position
        if (Input.GetMouseButton(0))
        {
            flowCamera.transform.position += touchStart - flowCamera.ScreenToWorldPoint(Input.mousePosition);
        }

        //Gets scrollData
        float scrollData = Input.GetAxis("Mouse ScrollWheel");
        //Adds to the target zoom based on data
        targetZoom -= scrollData * zoomFactor;
        //Inches towards target zoom
        flowCamera.orthographicSize = Mathf.Lerp(flowCamera.orthographicSize, targetZoom, Time.deltaTime * zoomLerpSpeed);

        //Resets values of camera if spacebar is pressed
        if (Input.GetKeyDown(KeyCode.Space))
        {
            flowCamera.transform.position = new Vector2(0, 0);
            flowCamera.orthographicSize = defaultZoom;
            targetZoom = defaultZoom;
        }
    }
}
