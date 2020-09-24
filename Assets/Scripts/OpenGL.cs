using System;
using System.Runtime.InteropServices;
using UnityEngine;
using static SK.Libretro.VideoDrivers.GLFW;
using static SK.Libretro.VideoDrivers.GL;

using GLbitfield = System.UInt32;
using GLenum     = System.UInt32;
using GLint      = System.Int32;
using GLsizei    = System.Int32;
using GLuint     = System.UInt32;
using GLfloat    = System.Single;

namespace SK.Libretro.VideoDrivers
{
    internal static class OpenGLDriver
    {
        // GLFW
        private static IntPtr _windowHandle;

        // GL
        private static readonly GLuint _frameBuffer;
        private static readonly GLuint _texture;
        private static readonly GLuint _renderBuffer;

        public static bool IsWindowOpened => glfwWindowShouldClose(_windowHandle) == GLFW_TRUE;
        
        private static int _width;
        private static int _height;
        public unsafe static bool Init(int width, int height)
        {
            if (!OpenWindow(width, height))
            {
                return false;
            }

            glfwMakeContextCurrent(_windowHandle);

            InitGLFunctions();

            fixed (GLuint* p = &_frameBuffer)
            {
                glGenFramebuffers(1, p);
            }
            glBindFramebuffer(GL_DRAW_FRAMEBUFFER, _frameBuffer);

            fixed (GLuint* p = &_texture)
            {
                glGenTextures(1, p);
            }
            glBindTexture(GL_TEXTURE_2D, _texture);
            glTexStorage2D(GL_TEXTURE_2D, 1, GL_RGBA8, width, height);
            glFramebufferTexture2D(GL_DRAW_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, _texture, 0);
            glBindTexture(GL_TEXTURE_2D, 0);

            fixed (GLuint* p = &_renderBuffer)
            {
                glGenRenderbuffers(1, p);
            }
            glBindRenderbuffer(GL_RENDERBUFFER, _renderBuffer);
            glRenderbufferStorage(GL_RENDERBUFFER, GL_DEPTH_COMPONENT16, width, height);
            glFramebufferRenderbuffer(GL_DRAW_FRAMEBUFFER, GL_DEPTH_ATTACHMENT, GL_RENDERBUFFER, _renderBuffer);
            glBindRenderbuffer(GL_RENDERBUFFER, 0);

            bool result = glCheckFramebufferStatus(GL_DRAW_FRAMEBUFFER) == GL_FRAMEBUFFER_COMPLETE;
            if (!result)
            {
                Console.WriteLine("Framebuffer creation failed");
            }
            Console.WriteLine("Framebuffer creation OK");

            glBindFramebuffer(GL_DRAW_FRAMEBUFFER, 0);

            return result;
        }

        public unsafe static void DeInit()
        {
            try
            {
                fixed (GLuint* p = &_frameBuffer)
                {
                    glDeleteFramebuffers(1, p);
                }

                fixed (GLuint* p = &_renderBuffer)
                {
                    glDeleteRenderbuffers(1, p);
                }

                fixed (GLuint* p = &_texture)
                {
                    glDeleteTextures(1, p);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                glfwDestroyWindow(_windowHandle);
                glfwTerminate();
            }
        }

        
        public static void SwapBuffers(Action callback)
        {
            if (glfwWindowShouldClose(_windowHandle) == GLFW_FALSE)
            {
                Debug.Log("SwapBuffers-OK");
                callback();
                glfwSwapBuffers(_windowHandle);
                glfwPollEvents();
            }
            else
            {
                Debug.Log("SwapBuffers-NOK");
            }
        }

        private static bool OpenWindow(int width, int height)
        {
            if (glfwInit() == GLFW_FALSE)
            {
                return false;
            }

            glfwWindowHint(GLFW_DOUBLEBUFFER, GLFW_TRUE);
            glfwWindowHint(GLFW_CLIENT_API, GLFW_OPENGL_API);
            glfwWindowHint(GLFW_CONTEXT_CREATION_API, GLFW_NATIVE_CONTEXT_API);
            // glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 4);
            // glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 3);
            //glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);
            //glfwWindowHint(GLFW_OPENGL_FORWARD_COMPAT, GLFW_FALSE);
            _width = width;
            _height = height;
            _windowHandle = glfwCreateWindow(width, height, "LibretroUnityFE: GLWindow", IntPtr.Zero, IntPtr.Zero);
            if (_windowHandle == IntPtr.Zero)
            {
                glfwTerminate();
                return false;
            }

            return true;
        }

        private static void InitGLFunctions()
        {
            glGetIntegerv             = Marshal.GetDelegateForFunctionPointer<PFNglGetIntegerv>(glfwGetProcAddress(nameof(glGetIntegerv)));
            glClear                   = Marshal.GetDelegateForFunctionPointer<PFNglClear>(glfwGetProcAddress(nameof(glClear)));
            glGenFramebuffers         = Marshal.GetDelegateForFunctionPointer<PFNglGenFramebuffers>(glfwGetProcAddress(nameof(glGenFramebuffers)));
            glDeleteFramebuffers      = Marshal.GetDelegateForFunctionPointer<PFNglDeleteFramebuffers>(glfwGetProcAddress(nameof(glDeleteFramebuffers)));
            glBindFramebuffer         = Marshal.GetDelegateForFunctionPointer<PFNglBindFramebuffer>(glfwGetProcAddress(nameof(glBindFramebuffer)));
            glFramebufferTexture2D    = Marshal.GetDelegateForFunctionPointer<PFNglFramebufferTexture2D>(glfwGetProcAddress(nameof(glFramebufferTexture2D)));
            glFramebufferRenderbuffer = Marshal.GetDelegateForFunctionPointer<PFNglFramebufferRenderbuffer>(glfwGetProcAddress(nameof(glFramebufferRenderbuffer)));
            glCheckFramebufferStatus  = Marshal.GetDelegateForFunctionPointer<PFNglCheckFramebufferStatus>(glfwGetProcAddress(nameof(glCheckFramebufferStatus)));
            glGenTextures             = Marshal.GetDelegateForFunctionPointer<PFNglGenTextures>(glfwGetProcAddress(nameof(glGenTextures)));
            glDeleteTextures          = Marshal.GetDelegateForFunctionPointer<PFNglDeleteTextures>(glfwGetProcAddress(nameof(glDeleteTextures)));
            glBindTexture             = Marshal.GetDelegateForFunctionPointer<PFNglBindTexture>(glfwGetProcAddress(nameof(glBindTexture)));
            glTexStorage2D            = Marshal.GetDelegateForFunctionPointer<PFNglTexStorage2D>(glfwGetProcAddress(nameof(glTexStorage2D)));
            glGenRenderbuffers        = Marshal.GetDelegateForFunctionPointer<PFNglGenRenderbuffers>(glfwGetProcAddress(nameof(glGenRenderbuffers)));
            glDeleteRenderbuffers     = Marshal.GetDelegateForFunctionPointer<PFNglDeleteRenderbuffers>(glfwGetProcAddress(nameof(glDeleteRenderbuffers)));
            glBindRenderbuffer        = Marshal.GetDelegateForFunctionPointer<PFNglBindRenderbuffer>(glfwGetProcAddress(nameof(glBindRenderbuffer)));
            glRenderbufferStorage     = Marshal.GetDelegateForFunctionPointer<PFNglRenderbufferStorage>(glfwGetProcAddress(nameof(glRenderbufferStorage)));
            
            glVertex2f = Marshal.GetDelegateForFunctionPointer<glVertex2f_f>(glfwGetProcAddress("glVertex2f"));
            glBegin = Marshal.GetDelegateForFunctionPointer<glBegin_f>(glfwGetProcAddress("glBegin"));
            glEnd = Marshal.GetDelegateForFunctionPointer<glEnd_f>(glfwGetProcAddress("glEnd"));
            glColor3f = Marshal.GetDelegateForFunctionPointer<glColor3f_f>(glfwGetProcAddress("glColor3f"));
            glRotatef = Marshal.GetDelegateForFunctionPointer<glRotatef_f>(glfwGetProcAddress("glRotatef"));
            
            glfwGetFramebufferSize = Marshal.GetDelegateForFunctionPointer<glfwGetFramebufferSize_f>(glfwGetProcAddress("glfwGetFramebufferSize"));
            glMatrixMode = Marshal.GetDelegateForFunctionPointer<glMatrixMode_f>(glfwGetProcAddress("glMatrixMode"));
            // glViewport = Marshal.GetDelegateForFunctionPointer<glViewport_f>(glfwGetProcAddress("glViewport"));
            glLoadIdentity = Marshal.GetDelegateForFunctionPointer<glLoadIdentity_f>(glfwGetProcAddress("glLoadIdentity"));
            glError = Marshal.GetDelegateForFunctionPointer<glError_f>(glfwGetProcAddress("glError"));
            glGetTime = Marshal.GetDelegateForFunctionPointer<glGetTime_f>(glfwGetProcAddress("glfwGetTime"));
            
            glOrtho = Marshal.GetDelegateForFunctionPointer<glOrtho_f>(glfwGetProcAddress("glOrtho"));
            glVertex3f = Marshal.GetDelegateForFunctionPointer<glVertex3f_f>(glfwGetProcAddress("glVertex3f"));
            glFlush = Marshal.GetDelegateForFunctionPointer<glFlush_f>(glfwGetProcAddress("glFlush"));
            
               
            
        }
    }

    internal static class GLFW
    {
        public const int GLFW_DOUBLEBUFFER          = 0x00021010;
        public const int GLFW_CLIENT_API            = 0x00022001;
        public const int GLFW_CONTEXT_CREATION_API  = 0x0002200B;
        public const int GLFW_OPENGL_PROFILE        = 0x00022008;
        public const int GLFW_CONTEXT_VERSION_MAJOR = 0x00022002;
        public const int GLFW_CONTEXT_VERSION_MINOR = 0x00022003;
        public const int GLFW_OPENGL_FORWARD_COMPAT = 0x00022006;

        public const int GLFW_TRUE                  = 1;
        public const int GLFW_FALSE                 = 0;
        public const int GLFW_OPENGL_API            = 0x00030001;
        public const int GLFW_NATIVE_CONTEXT_API    = 0x00036001;
        public const int GLFW_OPENGL_CORE_PROFILE   = 0x00032001;

        

#pragma warning disable IDE1006 // Naming Styles
        public const string GLFW_LIB = "libglfw";
        [DllImport(GLFW_LIB, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr glfwGetProcAddress([In, MarshalAs(UnmanagedType.LPStr)] string procname);

        [DllImport(GLFW_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int glfwInit();

        [DllImport(GLFW_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern void glfwTerminate();

        [DllImport(GLFW_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern void glfwWindowHint(int hint, int value);

        [DllImport(GLFW_LIB, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr glfwCreateWindow(int width, int height, [In, MarshalAs(UnmanagedType.LPStr)] string title, IntPtr monitor, IntPtr share);

        [DllImport(GLFW_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern void glfwDestroyWindow(IntPtr window);

        [DllImport(GLFW_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern void glfwMakeContextCurrent(IntPtr window);

        [DllImport(GLFW_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern int glfwWindowShouldClose(IntPtr window);

        [DllImport(GLFW_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern void glfwPollEvents();

        [DllImport(GLFW_LIB, CallingConvention = CallingConvention.Cdecl)]
        public static extern void glfwSwapBuffers(IntPtr window);
/*
        [DllImport(GLFW_LIB)] static extern bool glfwVertex2f(float x, float y);
        [DllImport(GLFW_LIB)] static extern bool glfwClear(int flags);
        [DllImport(GLFW_LIB)] static extern bool glfwBegin(int flags);
        [DllImport(GLFW_LIB)] static extern bool glfwEnd();
        [DllImport(GLFW_LIB)] public static extern double glfwGetTime();
*/



#pragma warning restore IDE1006 // Naming Styles
    }

    internal static class GL
    {
        public const GLint  GL_TRUE                     = 1;
        public const GLint  GL_FALSE                    = 0;
        public const GLuint GL_FRAMEBUFFER              = 0x8D40;
        public const GLuint GL_TEXTURE_2D               = 0x0DE1;
        public const GLuint GL_RGBA8                    = 0x8058;
        public const GLuint GL_COLOR_ATTACHMENT0        = 0x8CE0;
        public const GLuint GL_RENDERBUFFER             = 0x8D41;
        public const GLuint GL_DEPTH_COMPONENT16        = 0x81A5;
        public const GLuint GL_DEPTH_ATTACHMENT         = 0x8D00;
        public const GLuint GL_FRAMEBUFFER_COMPLETE     = 0x8CD5;
        public const GLuint GL_COLOR_BUFFER_BIT         = 0x00004000;
        public const GLuint GL_DEPTH_BUFFER_BIT         = 0x00000100;
        public const GLuint GL_DRAW_FRAMEBUFFER         = 0x8CA9;



        public const uint GL_TRIANGLES                          = 0x0004;
        public const uint GL_MODELVIEW  = 0x1700;
        public const uint GL_PROJECTION = 0x1701;


        public unsafe static GLuint GetCurrentFrameBuffer()
        {
            GLint framebuffer;
            glGetIntegerv(GL_DRAW_FRAMEBUFFER, &framebuffer);
            return (GLuint)framebuffer;
        }

#pragma warning disable IDE1006 // Naming Styles
        public static PFNglGetIntegerv glGetIntegerv;
        public static PFNglClear glClear;
        public static PFNglGenFramebuffers glGenFramebuffers;
        public static PFNglDeleteFramebuffers glDeleteFramebuffers;
        public static PFNglBindFramebuffer glBindFramebuffer;
        public static PFNglFramebufferTexture2D glFramebufferTexture2D;
        public static PFNglFramebufferRenderbuffer glFramebufferRenderbuffer;
        public static PFNglCheckFramebufferStatus glCheckFramebufferStatus;
        public static PFNglGenTextures glGenTextures;
        public static PFNglDeleteTextures glDeleteTextures;
        public static PFNglBindTexture glBindTexture;
        public static PFNglTexStorage2D glTexStorage2D;
        public static PFNglGenRenderbuffers glGenRenderbuffers;
        public static PFNglDeleteRenderbuffers glDeleteRenderbuffers;
        public static PFNglBindRenderbuffer glBindRenderbuffer;
        public static PFNglRenderbufferStorage glRenderbufferStorage;
        
		public static glVertex2f_f glVertex2f;
		public static glBegin_f glBegin;
		public static glEnd_f glEnd;     
        public static glColor3f_f glColor3f;
        public static glRotatef_f glRotatef;

        public static glOrtho_f glOrtho;
        public static glVertex3f_f glVertex3f;

        public static glfwGetFramebufferSize_f glfwGetFramebufferSize;
        public static glMatrixMode_f glMatrixMode;
        // static glViewport_f glViewport;
        public static glLoadIdentity_f glLoadIdentity;
        public static glError_f glError;

        public static glGetTime_f glGetTime;

        public static glFlush_f glFlush;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public unsafe delegate void PFNglGetIntegerv(GLenum pname, GLint* data);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void PFNglClear(GLbitfield flags);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public unsafe delegate void PFNglGenFramebuffers(GLsizei n, GLuint* framebuffers);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public unsafe delegate void PFNglDeleteFramebuffers(GLsizei n, GLuint* framebuffers);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void PFNglBindFramebuffer(GLenum target, GLuint framebuffer);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void PFNglFramebufferTexture2D(GLenum target, GLenum attachment, GLenum textarget, GLuint texture, GLint level);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void PFNglFramebufferRenderbuffer(GLenum target, GLenum attachment, GLenum renderbuffertarget, GLuint renderbuffer);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate GLuint PFNglCheckFramebufferStatus(GLenum target);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public unsafe delegate void PFNglGenTextures(GLsizei n, GLuint* textures);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public unsafe delegate void PFNglDeleteTextures(GLsizei n, GLuint* textures);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void PFNglBindTexture(GLenum target, GLuint texture);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void PFNglTexStorage2D(GLenum target, GLsizei levels, GLenum internalformat, GLsizei width, GLsizei height);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public unsafe delegate void PFNglGenRenderbuffers(GLsizei n, GLuint* renderbuffers);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public unsafe delegate void PFNglDeleteRenderbuffers(GLsizei n, GLuint* renderbuffers);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void PFNglBindRenderbuffer(GLenum target, GLuint renderbuffer);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void PFNglRenderbufferStorage(GLenum target, GLenum internalformat, GLsizei width, GLsizei height);

        public delegate uint glVertex2f_f(float x,float y);
        public delegate uint glBegin_f(uint flags);
        public delegate uint glEnd_f();
        public delegate void glColor3f_f (GLfloat red,GLfloat green,GLfloat blue);
        public delegate void glRotatef_f (float angle, float x, float y, float z);
        
        public delegate void glOrtho_f(float left, float right, float bottom, float top, float zNear, float zFar);
        public delegate void glVertex3f_f(GLfloat x, GLfloat y, GLfloat z);

        public unsafe delegate void glfwGetFramebufferSize_f (IntPtr window, int* width, int* height);
        public delegate void glMatrixMode_f(GLenum glenum);
        public delegate void glViewport_f( GLint x,GLint y,GLsizei width, GLsizei height );
        public delegate void glLoadIdentity_f();

        public delegate GLenum glError_f();

        public delegate double glGetTime_f();
        public delegate void glFlush_f();


#pragma warning restore IDE1006 // Naming Styles
    }
}
