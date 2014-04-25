using System;
using Gtk;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;


namespace OpenGLTkVerletDemo
{
	class MainClass
	{
		[STAThread]
		public static void Main (string[] args)
		{
			//Application.Init ();

			//MainWindow win = new MainWindow ();
			using (var game = new GLWindow()) {
				//60 fps, updates every 0.5 s
				game.Run (30.0,60.0);
			
			}

			//win.Show ();

		//	Application.Run ();
		}


	}
}
