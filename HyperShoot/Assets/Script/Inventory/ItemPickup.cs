using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace HyperShoot.Inventory
{
	[RequireComponent(typeof(SphereCollider))]
	[System.Serializable]
	public class ItemPickup : MonoBehaviour
	{
		public int ID;
		public int Amount;

		protected Type m_ItemType = null;
		protected Type ItemType
		{
			get
			{
#if UNITY_EDITOR
				if (m_Item.Type == null)
				{
					Debug.LogWarning(string.Format(MissingItemTypeError, this), gameObject);
					return null;
				}
				return m_Item.Type.GetType();
#else
			if (m_ItemType == null)
				m_ItemType = m_Item.Type.GetType();
			return m_ItemType;
#endif
			}
		}


		protected ItemType m_ItemTypeObject = null;
		public ItemType ItemTypeObject
		{
			get
			{
#if UNITY_EDITOR
				if (m_Item.Type == null)
				{
					Debug.LogWarning(string.Format(MissingItemTypeError, this), gameObject);
					return null;
				}
				return m_Item.Type;
#else
			if (m_ItemTypeObject == null)
				m_ItemTypeObject = m_Item.Type;
			return m_ItemTypeObject;
#endif
			}
		}

		protected AudioSource m_Audio = null;
		protected AudioSource Audio
		{
			get
			{
				if (m_Audio == null)
				{
					m_Audio = GetComponent<AudioSource>();
					if (m_Audio == null)
						m_Audio = gameObject.AddComponent<AudioSource>();
				}
				return m_Audio;
			}
		}

		protected Collider m_Collider = null;
		protected Collider Collider
		{
			get
			{
				if (m_Collider == null)
					m_Collider = GetComponent<Collider>();
				return m_Collider;
			}
		}

		//protected fp_Respawner m_Respawner = null;
		//protected fp_Respawner Respawner
		//{
		//	get
		//	{
		//		if (m_Respawner == null)
		//			m_Respawner = GetComponent<fp_Respawner>();
		//		return m_Respawner;
		//	}
		//}

		protected Collider[] m_Colliders = null;
		protected Collider[] Colliders
		{
			get
			{
				if (m_Colliders == null)
					m_Colliders = GetComponents<Collider>();
				return m_Colliders;
			}
		}

		protected Renderer m_Renderer = null;
		protected Renderer Renderer
		{
			get
			{
				if (m_Renderer == null)
					m_Renderer = GetComponent<Renderer>();
				return m_Renderer;
			}
		}

		protected Rigidbody m_Rigidbody = null;
		protected Rigidbody Rigidbody
		{
			get
			{
				if ((this != null) && !m_RigidbodyWasCached && (m_Rigidbody == null))
				{
					m_Rigidbody = GetComponent<Rigidbody>();
					if (m_Rigidbody != null)
						m_HaveRigidbody = true;
					m_RigidbodyWasCached = true;
				}
				return m_Rigidbody;
			}
		}
		bool m_HaveRigidbody = false;
		bool m_RigidbodyWasCached = false;


		//////////////// 'Item' section ////////////////
		[System.Serializable]
		public class ItemSection
		{

			public ItemType Type = null;
			public bool GiveOnContact = true;
		}
		[SerializeField]
		protected ItemSection m_Item;

		public bool GiveOnContact
		{
			get { return m_Item.GiveOnContact; }
			set { m_Item.GiveOnContact = value; }
		}

		[System.Serializable]
		public class RecipientTagsSection
		{
			public List<string> Tags = new List<string>();
		}
		[SerializeField]
		protected RecipientTagsSection m_Recipient;

		//////////////// 'Sounds' section ////////////////
		[System.Serializable]
		public class SoundSection
		{
			public AudioClip PickupSound = null;        // player triggers the pickup
			public bool PickupSoundSlomo = true;
			public AudioClip PickupFailSound = null;    // player failed to pick up the item (i.e. ammo full)
			public bool FailSoundSlomo = true;
		}
		[SerializeField]
		protected SoundSection m_Sound;


		//////////////// 'Messages' section ////////////////
		[System.Serializable]
		public class MessageSection
		{
			public string SuccessSingle = "Picked up {2}.";
			public string SuccessMultiple = "Picked up {4} {1}s.";
			public string FailSingle = "Can't pick up {2} right now.";
			public string FailMultiple = "Can't pick up {4} {1}s right now.";
		}
		[SerializeField]
		protected MessageSection m_Messages;

		protected bool m_Depleted = false;
		protected int m_PickedUpAmount;

		protected string MissingItemTypeError = "Warning: {0} has no ItemType object!";

		protected bool m_AlreadyFailed = false;

		static Dictionary<Collider, Inventory> m_ColliderInventories = new Dictionary<Collider, Inventory>();

		protected const float COLLIDER_DISABLE_DELAY = 0.5f;

		protected virtual void Awake()
		{
			if (ItemType == typeof(UnitType))
				Amount = Mathf.Max(1, Amount);

			// set the main collider of this gameobject to be a trigger
			Collider.isTrigger = true;

			if ((m_Sound.PickupSound != null) || (m_Sound.PickupFailSound != null))
			{
				Audio.clip = m_Sound.PickupSound;
				Audio.playOnAwake = false;
			}
		}

		bool m_WasSleepingLastFrame = false;

		protected virtual void Update()
		{
			TryRemoveOnDeplete();

			TryDisableColliderOnSleep();
		}

		protected virtual void TryRemoveOnDeplete()
		{
			if (!m_Depleted)
				return;

			if (Audio.isPlaying)
				return;
			//if (Respawner != null)
			//	SendMessage("Die", SendMessageOptions.DontRequireReceiver);
			//else
			//	fp_Utility.Destroy(gameObject);
		}

		protected virtual void TryDisableColliderOnSleep()
		{
			if (!m_HaveRigidbody)
				return;

			if (m_Depleted)
				return;

			if (Rigidbody.isKinematic)
				return;

			if (!Rigidbody.IsSleeping())
				return;

			if (m_WasSleepingLastFrame)
				return;

			// allow some time for the pickup to touch down or it may pause floating
			fp_Timer.In(COLLIDER_DISABLE_DELAY, () =>
			{

				if (Rigidbody != null)
					Rigidbody.isKinematic = true;

				for (int c = 0; c < Colliders.Length; c++)
				{
					if (Colliders[c] == null)
						continue;
					if (Colliders[c].isTrigger)
						continue;
					Colliders[c].enabled = false;
				}

			});

			m_WasSleepingLastFrame = Rigidbody.IsSleeping();
		}

		protected virtual void OnEnable()
		{
			if (Rigidbody != null)
			{
				Rigidbody.isKinematic = false;
				foreach (Collider c in Colliders)
				{
					if (c.isTrigger)
						continue;
					c.enabled = true;
				}
			}

			Renderer.enabled = true;
			m_Depleted = false;
			m_AlreadyFailed = false;
		}

		protected virtual void OnTriggerEnter(Collider col)
		{
			if (!m_Item.GiveOnContact)
				return;

			if (ItemType == null)
				return;

			//if (!fp_Gameplay.IsMaster)
			//	return;

			if (!Collider.enabled)
				return;

			TryGiveTo(col);
		}

		public void TryGiveTo(Collider col)
		{
			// only do something if the trigger is still active
			if (m_Depleted)
				return;

			Inventory inventory;
			if (!m_ColliderInventories.TryGetValue(col, out inventory))
			{
				inventory = fp_TargetEventReturn<Inventory>.SendUpwards(col, "GetInventory");
				m_ColliderInventories.Add(col, inventory);
			}

			if (inventory == null)
				return;

			// see if the colliding object was a valid recipient
			if ((m_Recipient.Tags.Count > 0) && !m_Recipient.Tags.Contains(col.gameObject.tag))
				return;

			bool result = false;

			int prevAmount = fp_TargetEventReturn<ItemType, int>.SendUpwards(col, "GetItemCount", m_Item.Type);


			if (ItemType == typeof(ItemType))
				result = fp_TargetEventReturn<ItemType, int, bool>.SendUpwards(col, "TryGiveItem", m_Item.Type, ID);
			else if (ItemType == typeof(UnitBankType))
				result = fp_TargetEventReturn<UnitBankType, int, int, bool>.SendUpwards(col, "TryGiveUnitBank", (m_Item.Type as UnitBankType), Amount, ID);
			else if (ItemType == typeof(UnitType))
				result = fp_TargetEventReturn<UnitType, int, bool>.SendUpwards(col, "TryGiveUnits", (m_Item.Type as UnitType), Amount);
			else if (ItemType.BaseType == typeof(ItemType))
				result = fp_TargetEventReturn<ItemType, int, bool>.SendUpwards(col, "TryGiveItem", m_Item.Type, ID);
			else if (ItemType.BaseType == typeof(UnitBankType))
				result = fp_TargetEventReturn<UnitBankType, int, int, bool>.SendUpwards(col, "TryGiveUnitBank", (m_Item.Type as UnitBankType), Amount, ID);
			else if (ItemType.BaseType == typeof(UnitType))
				result = fp_TargetEventReturn<UnitType, int, bool>.SendUpwards(col, "TryGiveUnits", (m_Item.Type as UnitType), Amount);

			if (result == true)
			{
				m_PickedUpAmount = (fp_TargetEventReturn<ItemType, int>.SendUpwards(col, "GetItemCount", m_Item.Type) - prevAmount); // calculate resulting amount given
				OnSuccess(col.transform);
			}
			else
			{
				OnFail(col.transform);
			}
		}

		protected virtual void OnTriggerExit()
		{
			m_AlreadyFailed = false;
		}

		protected virtual void OnSuccess(Transform recipient)
		{
			m_Depleted = true;

			if ((m_Sound.PickupSound != null)
				&& fp_Utility.IsActive(gameObject)
				&& Audio.enabled)
			{
				Audio.pitch = (m_Sound.PickupSoundSlomo ? Time.timeScale : 1.0f);
				Audio.Play();
			}

			Renderer.enabled = false;

			string msg = "";

			if ((m_PickedUpAmount < 2) || (ItemType == typeof(UnitBankType)) || (ItemType.BaseType == typeof(UnitBankType)))
				msg = string.Format(m_Messages.SuccessSingle, m_Item.Type.IndefiniteArticle, m_Item.Type.DisplayName, m_Item.Type.DisplayNameFull, m_Item.Type.Description, m_PickedUpAmount.ToString());
			else
				msg = string.Format(m_Messages.SuccessMultiple, m_Item.Type.IndefiniteArticle, m_Item.Type.DisplayName, m_Item.Type.DisplayNameFull, m_Item.Type.Description, m_PickedUpAmount.ToString());

			PlayScreen.Instance.PickUpItem(msg);
			//fp_LocalPlayer.EventHandler.HUDText.Send(msg);

			//if (fp_Gameplay.IsMultiplayer && fp_Gameplay.IsMaster)
			//	fp_GlobalEvent<fp_ItemPickup, Transform>.Send("TransmitPickup", this, recipient);   // will only execute on the master in multiplayer
		}

		protected virtual void Die()
		{
			fp_Utility.Activate(gameObject, false);
		}
		protected virtual void OnFail(Transform recipient)
		{
			//fp_FPPlayerEventHandler localPlayer = recipient.transform.root.GetComponentInChildren<fp_FPPlayerEventHandler>();
			//if (localPlayer != null)
			//	if (localPlayer.Dead.Active)
			//		return;

			//if (!m_AlreadyFailed
			//	&& (m_Sound.PickupFailSound != null)
			//	&& (!fp_Gameplay.IsMultiplayer || (fp_Gameplay.IsMultiplayer && (recipient.GetComponent<fp_FPPlayerEventHandler>() != null)))
			//	)
			//{
			//	Audio.pitch = m_Sound.FailSoundSlomo ? Time.timeScale : 1.0f;
			//	Audio.PlayOneShot(m_Sound.PickupFailSound);
			//}
			//m_AlreadyFailed = true;

			//string msg = "";

			//if ((m_PickedUpAmount < 2) || (ItemType == typeof(fp_UnitBankType)) || (ItemType.BaseType == typeof(fp_UnitBankType)))
			//	msg = string.Format(m_Messages.FailSingle, m_Item.Type.IndefiniteArticle, m_Item.Type.DisplayName, m_Item.Type.DisplayNameFull, m_Item.Type.Description, Amount.ToString());
			//else
			//	msg = string.Format(m_Messages.FailMultiple, m_Item.Type.IndefiniteArticle, m_Item.Type.DisplayName, m_Item.Type.DisplayNameFull, m_Item.Type.Description, Amount.ToString());

			//fp_GlobalEvent<Transform, string>.Send("HUDText", recipient, msg);

		}


	}
}

