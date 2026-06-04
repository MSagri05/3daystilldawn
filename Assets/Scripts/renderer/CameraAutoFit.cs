using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraAutoFit : MonoBehaviour
{
    public int gridWidth = 10;
    public int gridHeight = 10;
    public float cellSize = 1f;
    public float padding = 1f;

    void Start()
    {
        var cam = GetComponent<Camera>();
        cam.orthographic = true;

        float totalW = gridWidth * cellSize;
        float totalH = gridHeight * cellSize;

        // center camera on the grid
        float centerX = totalW / 2f;
        float centerY = totalH / 2f;
        transform.position = new Vector3(centerX, centerY, -10f);

        // fit orthographic size to show the full grid
        float orthoH = totalH / 2f + padding;
        float orthoW = (totalW / 2f + padding) / cam.aspect;
        cam.orthographicSize = Mathf.Max(orthoH, orthoW);
    }
}
