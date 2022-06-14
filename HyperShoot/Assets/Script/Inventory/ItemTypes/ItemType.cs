using UnityEngine;

namespace HyperShoot.Inventory
{
	[CreateAssetMenu(fileName = "ItemType", menuName = "ScriptableObjects/Database/ItemType", order = 0)]
	[System.Serializable]
	public class ItemType : ScriptableObject
	{
		public string IndefiniteArticle = "a";
		public string DisplayName;
		public string Description;
		public Texture2D Icon;
		public ItemType()
		{
		}

		[SerializeField]
		public string DisplayNameFull
		{ get { return IndefiniteArticle + " " + DisplayName; } }
	}
}
