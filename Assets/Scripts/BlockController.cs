using System.Collections.Generic;
using UnityEngine;

public class BlockController : MonoBehaviour
{
    private int _value;
    private Renderer _renderer;
    private static readonly List<Material> Materials = new List<Material>();
    private const float Increment = 0.05f;

    private void Start()
    {
        _renderer = GetComponent<Renderer>();

        if (Materials.Count == 0)
        {
            _renderer.material.name = "BlockMat0";
            Materials.Add(_renderer.material);
        }
        else
            UpdateColor();
    }

    public void DoubleHeight()
    {
        Transform t = transform; // for copy efficiency, according to Rider
        
        Vector3 s = t.localScale;
        s = new Vector3(s.x, 2 * s.y, s.z);
        Vector3 p = t.position;
        p = new Vector3(p.x, s.y / 2, p.z);

        t.localScale = s;
        t.position = p;
        _value++;

        UpdateColor();
    }

    public void MoveTo(float x, float z)
    {
        Transform t = transform; // for copy efficiency, according to Rider
        Vector3 p = t.position;
        t.position = new Vector3(x, p.y, z);
    }

    public float GetHeight()
    {
        return transform.localScale.y;
    }

    private void UpdateColor()
    {
        if (_value < Materials.Count)
        {
            _renderer.material = Materials[_value];
            // Debug.Log("using already created material");
        }
        else
        {
            // Debug.Log("creating new material");
            Material oldMat = _renderer.material;
            Color.RGBToHSV(oldMat.color, out float h, out _, out _);
            
            Material newMat = new Material(oldMat)
            {
                name = "BlockMat" + _value,
                color = Color.HSVToRGB((h + Increment) % 1f, 1f, 1f)
            };

            _renderer.material = newMat;
            Materials.Add(newMat);
        }
    }
}
