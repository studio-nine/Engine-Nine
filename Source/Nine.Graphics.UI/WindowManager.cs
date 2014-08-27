namespace Nine.Graphics.UI
{
	using System.Linq;
	using System.Collections.Generic;
	using Microsoft.Xna.Framework;
	using System.Collections;
	using System.Diagnostics;

	/// <summary>
	/// Handles multiple windows and input.
	/// </summary>
	[System.Windows.Markup.ContentProperty("Windows")]
	public class WindowManager : Nine.Object, IContainer
	{
		public bool InputEnabled
		{
			get { return input != null && input.Enabled; }
			set
			{
				if (value) EnsureInput();
				if (input != null) input.Enabled = value;
			}
		}

		public bool TrackEvents { get; set; }

		public IList<BaseWindow> Windows
		{
			get { return windows; }
		}
		private NotificationCollection<BaseWindow> windows;

		IList IContainer.Children { get { return windows; } }
		
		private Input input;

		public WindowManager()
		{
			this.windows = new NotificationCollection<BaseWindow>();
			this.TrackEvents = true;

			EnsureInput();
		}

		// TODO: Window Input

		#region Mouse

		void MouseMove(object sender, MouseEventArgs e)
		{
			// TODO: Improve this
			UIElement element = FindElement(e.X, e.Y);
			if (element != null)
			{
				if (TrackEvents) Debug.WriteLine(string.Format("Event: {0}, Element: {1}", "MouseMove", element));
				element.InvokeMouseMove(this, e);
			}
		}
		
		void MouseUp(object sender, MouseEventArgs e)
		{
			UIElement element = FindElement(e.X, e.Y);
			if (element != null)
			{
				if (TrackEvents) Debug.WriteLine(string.Format("Event: {0}, Element: {1}", "MouseUp", element));
				element.InvokeOnMouseUp(this, e);
			}
		}

		void MouseDown(object sender, MouseEventArgs e)
		{
			BaseWindow window;
			int index = FindWindow(e.X, e.Y, out window);

			if (window == null)
				return;

			// Bring window to front
			if (e.Button == MouseButtons.Right && index != 0)
			{
				Windows.RemoveAt(index);
				Windows.Insert(0, window);
			}

			UIElement element = FindElement(window, e.X, e.Y);
			if (element != null)
			{
				if (TrackEvents) Debug.WriteLine(string.Format("Event: {0}, Element: {1}", "MouseDown", element));
				element.InvokeMouseDown(this, e);
			}
		}

		void MouseWheel(object sender, MouseEventArgs e)
		{

		}

		#endregion

		#region Keyboard

		void KeyDown(object sender, KeyboardEventArgs e)
		{

		}

		void KeyUp(object sender, KeyboardEventArgs e)
		{

		}

		#endregion

		#region GamePad

		void ButtonUp(object sender, GamePadEventArgs e)
		{

		}

		void ButtonDown(object sender, GamePadEventArgs e)
		{

		}

		#endregion

		#region Touch

		void GestureSampled(object sender, GestureEventArgs e)
		{

		}

		#endregion

		#region Private Methods

		private void EnsureInput()
		{
			if (input == null)
			{
				input = new Input();
				input.MouseMove += MouseMove;
				input.MouseUp += MouseUp;
				input.MouseDown += MouseDown;
				input.MouseWheel += MouseWheel;
				input.KeyDown += KeyDown;
				input.KeyUp += KeyUp;
				input.ButtonUp += ButtonUp;
				input.ButtonDown += ButtonDown;
				input.GestureSampled += GestureSampled;
			}
		}

		private int FindWindow(int x, int y, out BaseWindow result)
		{
			for (int i = 0; i < windows.Count; i++)
			{
				BaseWindow window = windows[i];
				if (window.Viewport.Contains(x, y) == ContainmentType.Contains)
				{
					result = window;
					return i;
				}
			}
			result = null;
			return -1;
		}

		private UIElement FindElement(int x, int y)
		{
			BaseWindow window;
			FindWindow(x, y, out window);

			if (window == null)
				return null;

			return FindElement(window, x, y);
		}

		private UIElement FindElement(BaseWindow window, int x, int y)
		{
			if (window == null)
				return null;

			Vector2 hit = new Vector2(x, y);
			UIElement element = null;

			var container = window as IContainer;
			if (container != null)
			{
				foreach (var child in container.Children)
				{
					var uiElement = child as UIElement;
					if (uiElement != null)
					{
						element = HitTest(uiElement, hit);
						break;
					}
				}
			}

			return element;
		}

		private static UIElement HitTest(UIElement element, Vector2 hit)
		{
			if (element.HitTest(hit))
			{
				var container = element as IContainer;
				if (container != null)
				{
					foreach (var child in container.Children)
					{
						var uiElement = child as UIElement;
						if (uiElement != null && uiElement.HitTest(hit))
						{
							return HitTest(uiElement, hit);
						}
					}
				}
				return element;
			}
			return null;
		}

		#endregion 
	}
}
