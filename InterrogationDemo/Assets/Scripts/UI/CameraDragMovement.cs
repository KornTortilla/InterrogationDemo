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
        if (Input.GetMouseButtonDown(0))
        {
            touchStart = flowCamera.ScreenToWorldPoint(Input.mousePosition);
        }
        if (Input.GetMouseButton(0))
        {
            flowCamera.transform.position += touchStart - flowCamera.ScreenToWorldPoint(Input.mousePosition);
        }

        float scrollData = Input.GetAxis("Mouse ScrollWheel");
        targetZoom -= scrollData * zoomFactor;
        flowCamera.orthographicSize = Mathf.Lerp(flowCamera.orthographicSize, targetZoom, Time.deltaTime * zoomLerpSpeed);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            flowCamera.transform.position = new Vector2(0, 0);
            flowCamera.orthographicSize = defaultZoom;
            targetZoom = defaultZoom;
        }
    }
}
