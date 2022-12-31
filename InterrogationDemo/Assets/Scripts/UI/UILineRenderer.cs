using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILineRenderer : Graphic
{
    public List<Vector2> points = new List<Vector2>();
    public float thickness;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (points.Count < 2) return;

        float angle = 0;

        for(int i = 0; i < points.Count; i++)
        {
            Vector2 point = points[i];

            if(i < points.Count - 1) 
            {
                angle = GetAngle(points[i], points[i + 1]) + 45f;
            }

            DrawVertices(point, vh, angle);
        }

        for(int i = 0; i < points.Count - 1; i++)
        {
            int index = i * 2;
            vh.AddTriangle(index + 0, index + 1, index + 3);
            vh.AddTriangle(index + 3, index + 2, index + 0);
        }
    }

    private void DrawVertices(Vector2 point, VertexHelper vh, float angle)
    {
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;

        vertex.position = Quaternion.Euler(0, 0, angle) * new Vector3(-thickness / 2, 0);
        vertex.position += new Vector3(point.x, point.y);
        vh.AddVert(vertex);

        vertex.position = Quaternion.Euler(0, 0, angle) * new Vector3(thickness / 2, 0);
        vertex.position += new Vector3(point.x, point.y);
        vh.AddVert(vertex);
    }

    private float GetAngle(Vector2 start, Vector2 end)
    {
        return (float)(Mathf.Atan2(end.y - start.y, end.x - start.x) * (180 / Mathf.PI)); 
    }
}
