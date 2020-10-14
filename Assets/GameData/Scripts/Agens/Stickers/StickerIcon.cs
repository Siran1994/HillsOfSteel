using System;
using UnityEngine;

namespace Agens.Stickers
{
	public class StickerIcon
	{
		public enum Idiom
		{
			Iphone,
			Ipad,
			Universal,
			IosMarketing
		}

		public enum Scale
		{
			Original = 1,
			Double,
			Triple
		}

		public Vector2 size;

		public Idiom idiom;

		public string filename;

		public Scale scale;

		public string platform;

		public StickerIcon(Texture2D texture, int width, int height, Idiom idiom, Scale scale = Scale.Double, string platform = null)
		{
			size = new Vector2(width, height);
			filename = texture.name + ".png";
			this.idiom = idiom;
			this.scale = scale;
			this.platform = platform;
		}

		public string GetIdiom()
		{
			Idiom idiom = this.idiom;
			if (idiom == Idiom.IosMarketing)
			{
				return "ios-marketing";
			}
			return Enum.GetName(typeof(Idiom), this.idiom).ToLower();
		}

		public string GetScale()
		{
			return (int)scale + "x";
		}
	}
}
