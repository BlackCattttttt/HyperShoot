using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class fp_State
{
	public fp_StateManager StateManager = null;
	public string TypeName = null;
	public string Name = null;
	public fp_ComponentPreset Preset = null;                // (runtime only)
	public TextAsset TextAsset = null;   
	public List<int> StatesToBlock;                         // a list of states that this state will block when enabled

	// NOTE: made non-serialized to prevent serialization depth error on Unity 4.5
	[System.NonSerialized]
	protected bool m_Enabled = false;
	[System.NonSerialized]
	protected List<fp_State> m_CurrentlyBlockedBy = null;   // (runtime) a list of states that is currently blocking this state


	/// <summary>
	/// represents a snapshot of all or some of a component's properties.
	/// controlled by the state manager, and may be enabled, disabled or
	/// blocked
	/// </summary>
	public fp_State(string typeName, string name = "Untitled", string path = null, TextAsset asset = null)
	{

		TypeName = typeName;
		Name = name;
		TextAsset = asset;

	}


	/// <summary>
	/// enables or disables this state and imposes or relaxes
	/// its blocking list, respectively
	/// </summary>
	public bool Enabled
	{
		get { return m_Enabled; }
		set
		{

			m_Enabled = value;

#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			if (StateManager == null)
				return;

			if (m_Enabled)
				StateManager.ImposeBlockingList(this);
			else
				StateManager.RelaxBlockingList(this);
		}
	}


	/// <summary>
	/// whether this state is currently blocked
	/// </summary>
	public bool Blocked
	{
		get
		{
			return CurrentlyBlockedBy.Count > 0;
		}
	}


	/// <summary>
	/// how many states are currently blocking this state
	/// </summary>
	public int BlockCount
	{
		get
		{
			return CurrentlyBlockedBy.Count;
		}
	}


	/// <summary>
	/// the list of states that are currently blocking this one
	/// </summary>
	protected List<fp_State> CurrentlyBlockedBy
	{
		get
		{
			if (m_CurrentlyBlockedBy == null)
				m_CurrentlyBlockedBy = new List<fp_State>();
			return m_CurrentlyBlockedBy;
		}
	}


	/// <summary>
	/// adds a state to the list of states that blocks this one
	/// </summary>
	public void AddBlocker(fp_State blocker)
	{

		if (!CurrentlyBlockedBy.Contains(blocker))
			CurrentlyBlockedBy.Add(blocker);

	}


	/// <summary>
	/// removes a state from the list of states that blocks this one
	/// </summary>
	public void RemoveBlocker(fp_State blocker)
	{

		if (CurrentlyBlockedBy.Contains(blocker))
			CurrentlyBlockedBy.Remove(blocker);

	}
}
