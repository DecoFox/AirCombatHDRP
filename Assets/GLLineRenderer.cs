using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GLLineRenderer : MonoBehaviour
{
    public Material LineMaterial;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnPostRender()
    {
        RenderLines();
    }


    public void RenderLines()
    {

        GL.PushMatrix();
        GL.MultMatrix(transform.localToWorldMatrix);

        GL.Begin(GL.LINES);

        LineMaterial.SetPass(0);
        GL.Color(Color.red);
        GL.Vertex3(0, 0, 1);
        GL.Color(Color.blue);
        GL.Vertex3(1, 1, 1);
        GL.End();
        GL.PopMatrix();
    }
}
