using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace I2.Loc
{
	public enum eTermType 
	{ 
		Text, Font, Texture, AudioClip, GameObject, Sprite, Material, Child,
		#if NGUI
			UIAtlas, UIFont,
		#endif
		#if TK2D
			TK2dFont, TK2dCollection,
		#endif
		#if TextMeshPro
			TextMeshPFont,
		#endif
		#if SVG
			SVGAsset,
		#endif
		Object 
	}

	public enum TranslationFlag : byte
	{
		AutoTranslated_Normal = 1,
		AutoTranslated_Touch = 2,
		AutoTranslated_All = 255
	}

	public enum eTransTag_Input { Any, PC, Touch, VR, XBox, PS4, Controller  };


    [Serializable]
	public class TermData
	{
		public string 			Term 			= string.Empty;
		public eTermType		TermType 		= eTermType.Text;
		public string 			Description	    = string.Empty;
		public string[]			Languages		= new string[0];
		public string[]			Languages_Touch = new string[0];
		public byte[]			Flags 			= new byte[0];	// flags for each translation

		public string GetTranslation ( int idx, eTransTag_Input input=eTransTag_Input.Any )
		{
            string text;
			if (IsTouchType())
			{
				text = !string.IsNullOrEmpty(Languages_Touch[idx]) ? Languages_Touch[idx] : Languages[idx];
			}
			else
			{
                text = !string.IsNullOrEmpty(Languages[idx]) ? Languages[idx] : Languages_Touch[idx];
			}
            if (text != null)
            {
                text = text.Replace("[i2nt]", "").Replace("[/i2nt]", "");
            }
            return text;
		}

        public void SetTranslation( int idx, string translation )
        {
            if (IsTouchType())
            {
                Languages_Touch[idx] = translation;
            }
            else
            {
                Languages[idx] = translation;
            }
        }

        public bool IsAutoTranslated( int idx, bool IsTouch )
		{
			if (IsTouch)
				return (Flags[idx] & (byte)TranslationFlag.AutoTranslated_Touch) > 0;
			else
				return (Flags[idx] & (byte)TranslationFlag.AutoTranslated_Normal) > 0;
		}

		public bool HasTouchTranslations ()
		{
			for (int i=0, imax=Languages_Touch.Length; i<imax; ++i)
				if (!string.IsNullOrEmpty(Languages_Touch[i]) && !string.IsNullOrEmpty(Languages[i]) &&
					Languages_Touch[i]!=Languages[i])
					return true;
			return false;
		}

		public void Validate ()
		{
			int nLanguages = Mathf.Max(Languages.Length, 
							 Mathf.Max(Languages_Touch.Length, Flags.Length));

			if (Languages.Length != nLanguages) 		Array.Resize(ref Languages, nLanguages);
			if (Languages_Touch.Length != nLanguages) 	Array.Resize(ref Languages_Touch, nLanguages);
			if (Flags.Length!=nLanguages) 				Array.Resize(ref Flags, nLanguages);

            for (int i = 0; i < nLanguages; ++i)
            {
                if (string.IsNullOrEmpty(Languages[i]) && !string.IsNullOrEmpty(Languages_Touch[i]))
                {
                    Languages[i] = Languages_Touch[i];
                    Languages_Touch[i] = null;
                }
            }
        }

		public static bool IsTouchType()
		{
			#if UNITY_ANDROID || UNITY_IOS || UNITY_WP8
				return true;
			#else
				return false;
			#endif
		}

		public bool IsTerm( string name, bool allowCategoryMistmatch)
		{
			if (!allowCategoryMistmatch)
				return name == Term;

			return name == LanguageSource.GetKeyFromFullTerm (Term);
		}
	};

	public enum eLanguageDataFlags
	{
		DISABLED = 1,
		KEEP_LOADED = 2,
		NOT_LOADED = 4
	}
	[Serializable]
	public class LanguageData
	{
		public string Name;
		public string Code;
		public byte Flags;      // eLanguageDataFlags

		[NonSerialized]
		public bool Compressed = false;  // This will be used in the next version for only loading used Languages

		public bool IsEnabled () { return (Flags & (int)eLanguageDataFlags.DISABLED) == 0; }

        public void SetEnabled( bool bEnabled )
        {
            if (bEnabled) Flags = (byte)(Flags & (~(int)eLanguageDataFlags.DISABLED));
                     else Flags = (byte)(Flags | (int)eLanguageDataFlags.DISABLED);
        }

        public bool IsLoaded () { return (Flags & (int)eLanguageDataFlags.NOT_LOADED) == 0; }
		public bool CanBeUnloaded () { return (Flags & (int)eLanguageDataFlags.KEEP_LOADED) == 0; }

		public void SetLoaded ( bool loaded ) 
		{
			if (loaded) Flags = (byte)(Flags & (~(int)eLanguageDataFlags.NOT_LOADED));
	  			   else Flags = (byte)(Flags | (int)eLanguageDataFlags.NOT_LOADED);
		}
        public void SetCanBeUnLoaded(bool allowUnloading)
        {
            if (allowUnloading) Flags = (byte)(Flags & (~(int)eLanguageDataFlags.KEEP_LOADED));
                           else Flags = (byte)(Flags | (int)eLanguageDataFlags.KEEP_LOADED);
        }
    }

    public class TermsPopup : PropertyAttribute
    {
        public TermsPopup(string filter = "")
        {
            this.Filter = filter;
        }

        public string Filter { get; private set; }
    }
}