using UnityEngine;
using Render;

using SK.Libretro.VideoDrivers;
using System;
using System.Collections;
using GL = SK.Libretro.VideoDrivers.GL;
using Random = UnityEngine.Random;


public class LibretroWrapper : MonoBehaviour
{
    public Renderer _rendererComponent;
    public Material _originalMaterial;

    public int width = 800;
    public int height = 600;
    
    private VideoProcessor _videoProcessor;
    // Start is called before the first frame update
    void Start()
    {
        _videoProcessor = new VideoProcessor();
        
        BindTextureToMaterial();
        InitOpenGL();
        DrawOpenGL(2);
    }

    // connect texture to material of GameObject this script is attached to
    private void BindTextureToMaterial()
    {
        _originalMaterial = _rendererComponent.material;
        var _newMaterial = new Material(_rendererComponent.material);
        _rendererComponent.material = _newMaterial;
        _rendererComponent.material.mainTextureScale = new Vector2(1f, -1f);
        _rendererComponent.material.color = Color.black;
        _rendererComponent.material.EnableKeyword("_EMISSION");
        _rendererComponent.material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        _rendererComponent.material.SetColor("_EmissionColor", Color.white);
    }

    void InitOpenGL()
    {
        if (!OpenGLDriver.Init(width,height))
        {
            Console.WriteLine("GLFW init error");
            return;
        }
        Console.WriteLine("GLFW init OK");
    }

    
    void DrawOpenGL(int choice)
    {
        Debug.Log($"DrawOpenGL({choice})");
        /* Loop until the user closes the window */
        {
            
            switch (choice)
            {
                case 1:
                    GL.glClear(GL.GL_COLOR_BUFFER_BIT | GL.GL_DEPTH_BUFFER_BIT);
                    GL.glBegin(GL.GL_TRIANGLES);
//                    GL.glColor3f(r,g,b);
                    float rnd = 0.5f;
                    GL.glVertex2f(-rnd, -rnd);
                    GL.glVertex2f(0f, rnd);
                    GL.glVertex2f(rnd, -rnd);
                    GL.glEnd();
                    break;
                case 2:
                    GL.glClear(GL.GL_COLOR_BUFFER_BIT | GL.GL_DEPTH_BUFFER_BIT);
                    //GL.glfwGetFramebufferSize(_windowHandle, &width, &height);
                    float ratio = width / (float) height;
                    // glViewport(0, 0, width, height);
                    GL.glMatrixMode(GL.GL_PROJECTION);
                    GL.glLoadIdentity();
                    GL.glOrtho(-ratio, ratio, -1f, 1f, 1f, -1f);
                    GL.glMatrixMode(GL.GL_MODELVIEW);
                    GL.glLoadIdentity();
                    GL.glRotatef((float) GL.glGetTime() * 50f, 0f, 0f, 1f);
                    GL.glBegin(GL.GL_TRIANGLES);
                    GL.glColor3f(1f, 0f, 0f);
                    GL.glVertex3f(-0.6f, -0.4f, 0f);
                    GL.glColor3f(0f, 1f, 0f);
                    GL.glVertex3f(0.6f, -0.4f, 0f);
                    GL.glColor3f(0f, 0f, 1f);
                    GL.glVertex3f(0f, 0.6f, 0f);
                    GL.glEnd();
                    break;

            }

            //GL.glFlush();

        }
    }
    
    


    private void OnDestroy()
    {
        OpenGLDriver.DeInit();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.D))
        {
            float r = Random.Range(0f, 1f);
            float g = Random.Range(0f, 1f);
            float b = Random.Range(0f, 1f);

            OpenGLDriver.SwapBuffers(()=>DrawOpenGL(1));
        }

        if (Input.GetKey(KeyCode.E))
        {
            OpenGLDriver.SwapBuffers(()=>DrawOpenGL(2));
        }
        uint currentFrameBuffer = GL.GetCurrentFrameBuffer();
        var texture = randomTexture2D(10, 10);
        _rendererComponent.material.SetTexture("_EmissionMap", texture);
    }
    
    

    public Texture2D randomTexture2D(int width, int height)
    {
        Texture2D t2 = new Texture2D(width, height);
 
        for (int x = 0; x < t2.width; x++)
        {
            for (int y = 0; y < t2.height; y++)
            {
                float r = Random.Range(0f, 1f);
                float g = Random.Range(0f, 1f);
                float b = Random.Range(0f, 1f);
                t2.SetPixel(x, y, new Color(r, g, b, 1f) );
            }
        }
        t2.Apply();
        return t2;
    }
}
