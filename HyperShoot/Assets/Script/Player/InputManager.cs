using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : Singleton<InputManager>
{
    [System.Serializable]
    public class InputAxis
    {
        public KeyCode Positive;
        public KeyCode Negative;
    }
    // primary buttons
    public Dictionary<string, KeyCode> Buttons = new Dictionary<string, KeyCode>();
    public List<string> ButtonKeys = new List<string>();
    public List<KeyCode> ButtonValues = new List<KeyCode>();

	// axis
	public Dictionary<string, InputAxis> Axis = new Dictionary<string, InputAxis>();
	public List<string> AxisKeys = new List<string>();
	public List<InputAxis> AxisValues = new List<InputAxis>();

	// Unity Input Axis
	public List<string> UnityAxis = new List<string>();

    protected override void Awake()
    {
        base.Awake();
		SetupDefaults();
    }
    [Button]
	public virtual void SetupDefaults(string type = "")
	{

		if (type == "" || type == "Buttons")
		{
			if (ButtonKeys.Count == 0)
			{
				AddButton("Attack", KeyCode.Mouse0);
				AddButton("SetNextWeapon", KeyCode.E);
				AddButton("SetPrevWeapon", KeyCode.Q);
				AddButton("ClearWeapon", KeyCode.Backspace);
				AddButton("Zoom", KeyCode.Mouse1);
				AddButton("Reload", KeyCode.R);
				AddButton("Jump", KeyCode.Space);
				AddButton("Crouch", KeyCode.C);
				AddButton("Run", KeyCode.LeftShift);
				AddButton("Interact", KeyCode.F);
				AddButton("Accept1", KeyCode.Return);
				AddButton("Accept2", KeyCode.KeypadEnter);
				AddButton("Pause", KeyCode.P);
				AddButton("Menu", KeyCode.Escape);
				AddButton("Toggle3rdPerson", KeyCode.V);
				AddButton("ScoreBoard", KeyCode.Tab);
				AddButton("SetWeapon1", KeyCode.Alpha1);
				AddButton("SetWeapon2", KeyCode.Alpha2);
				AddButton("SetWeapon3", KeyCode.Alpha3);
				AddButton("SetWeapon4", KeyCode.Alpha4);
				AddButton("SetWeapon5", KeyCode.Alpha5);
				AddButton("SetWeapon6", KeyCode.Alpha6);
				AddButton("SetWeapon7", KeyCode.Alpha7);
				AddButton("SetWeapon8", KeyCode.Alpha8);
				AddButton("SetWeapon9", KeyCode.Alpha9);
				AddButton("SetWeapon10", KeyCode.Alpha0);
				AddButton("Teleport", KeyCode.None);

			}
		}

		if (type == "" || type == "Axis")
		{
			if (AxisKeys.Count == 0)
			{
				AddAxis("Vertical", KeyCode.W, KeyCode.S);
				AddAxis("Horizontal", KeyCode.D, KeyCode.A);
			}
		}

		if (type == "" || type == "UnityAxis")
		{
			if (UnityAxis.Count == 0)
			{
				AddUnityAxis("Mouse X");
				AddUnityAxis("Mouse Y");
				AddUnityAxis("Horizontal");
				AddUnityAxis("Vertical");
				AddUnityAxis("LeftTrigger");
				AddUnityAxis("RightTrigger");
			}
		}

		UpdateDictionaries();

	}
	bool HaveBinding(string button)
	{

		if (Buttons.ContainsKey(button))
			return true;

		Debug.LogError("Error (" + this + ") \"" + button + "\" is not declared in the UFPS Input Manager. You must add it from the 'UFPS -> Input Manager' editor menu for this button to work.");

		return false;

	}


	/// <summary>
	/// Adds a button with a specified keycode
	/// </summary>
	public virtual void AddButton(string n, KeyCode k = KeyCode.None)
	{

		if (ButtonKeys.Contains(n))
			ButtonValues[ButtonKeys.IndexOf(n)] = k;
		else
		{
			ButtonKeys.Add(n);
			ButtonValues.Add(k);
		}

	}


	/// <summary>
	/// Adds an axis with a positive and negative key
	/// </summary>
	public virtual void AddAxis(string n, KeyCode pk = KeyCode.None, KeyCode nk = KeyCode.None)
	{

		if (AxisKeys.Contains(n))
			AxisValues[AxisKeys.IndexOf(n)] = new InputAxis { Positive = pk, Negative = nk };
		else
		{
			AxisKeys.Add(n);
			AxisValues.Add(new InputAxis { Positive = pk, Negative = nk });
		}

	}


	/// <summary>
	/// Adds a unity axis.
	/// </summary>
	public virtual void AddUnityAxis(string n)
	{

		if (UnityAxis.Contains(n))
			UnityAxis[UnityAxis.IndexOf(n)] = n;
		else
		{
			UnityAxis.Add(n);
		}

	}


	/// <summary>
	/// Updates the input dictionaries
	/// </summary>
	public virtual void UpdateDictionaries()
	{
		Buttons.Clear();
		for (int i = 0; i < ButtonKeys.Count; i++)
		{
			if (!Buttons.ContainsKey(ButtonKeys[i]))
				Buttons.Add(ButtonKeys[i], ButtonValues[i]);
		}

		Axis.Clear();
		for (int i = 0; i < AxisKeys.Count; i++)
		{
			Axis.Add(AxisKeys[i], new InputAxis { Positive = AxisValues[i].Positive, Negative = AxisValues[i].Negative });
		}
	}
	public static bool GetButtonAny(string button)
	{

		return Instance.DoGetButtonAny(button);

	}


	/// <summary>
	/// handles keyboard, mouse and joystick input for any button state
	/// </summary>
	public virtual bool DoGetButtonAny(string button)
	{

		if (!HaveBinding(button))   // post an error if binding is not found
			return false;

		if (Input.GetKey(Buttons[button]) || Input.GetKeyDown(Buttons[button]) || Input.GetKeyUp(Buttons[button]))
			return true;    // button held, pressed or released as primary binding

		return false;   // button has not been touched in any way


	}


	/// <summary>
	/// handles keyboard, mouse and joystick input while a button is held
	/// </summary>
	public static bool GetButton(string button)
	{

		return Instance.DoGetButton(button);

	}


	/// <summary>
	/// handles keyboard, mouse and joystick input while a button is held
	/// </summary>
	public virtual bool DoGetButton(string button)
	{

		if (!HaveBinding(button))   // post an error if binding is not found
			return false;

		if (Input.GetKey(Buttons[button]))
			return true;    // button held down as primary binding

		return false;   // button not held down

	}


	/// <summary>
	/// handles keyboard, mouse and joystick input for a button down event
	/// </summary>
	public static bool GetButtonDown(string button)
	{

		return Instance.DoGetButtonDown(button);

	}


	/// <summary>
	/// handles keyboard, mouse and joystick input for a button down event
	/// </summary>
	public virtual bool DoGetButtonDown(string button)
	{

		if (!HaveBinding(button))   // post an error if binding is not found
			return false;

		if (Input.GetKeyDown(Buttons[button]))
			return true;    // button pressed as primary binding

		return false;   // button not pressed

	}


	/// <summary>
	/// handles keyboard, mouse and joystick input when a button is released
	/// </summary>
	public static bool GetButtonUp(string button)
	{

		return Instance.DoGetButtonUp(button);

	}


	/// <summary>
	/// handles keyboard, mouse and joystick input when a button is released
	/// </summary>
	public virtual bool DoGetButtonUp(string button)
	{

		if (!HaveBinding(button))   // post an error if binding is not found
			return false;

		if (Input.GetKeyUp(Buttons[button]))
			return true;    // button released as primary binding

		return false;   // button not released

	}


	/// <summary>
	/// handles keyboard, mouse and joystick input for axes
	/// </summary>
	public static float GetAxisRaw(string axis)
	{

		return Instance.DoGetAxisRaw(axis);

	}


	/// <summary>
	/// handles keyboard, mouse and joystick input for axes
	/// </summary>
	public virtual float DoGetAxisRaw(string axis)
	{

		if (Axis.ContainsKey(axis))
		{
			float val = 0;
			if (Input.GetKey(Axis[axis].Positive))
				val = 1;
			if (Input.GetKey(Axis[axis].Negative))
				val = -1;
			return val;
		}
		else if (UnityAxis.Contains(axis))
		{
			return Input.GetAxisRaw(axis);
		}
		else
		{
			Debug.LogError("Error (" + this + ") \"" + axis + "\" is not declared in the UFPS Input Manager. You must add it from the 'UFPS -> Input Manager' editor menu for this axis to work.");
			return 0;
		}

	}
}
