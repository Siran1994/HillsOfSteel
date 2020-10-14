using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0618
namespace I2.Loc
{
	[AddComponentMenu("I2/Localization/Localize")]
	public class Localize : MonoBehaviour
	{
		public enum TermModification
		{
			DontModify,
			ToUpper,
			ToLower,
			ToUpperFirst,
			ToTitle
		}

		public delegate void DelegateSetFinalTerms(string Main, string Secondary, out string primaryTerm, out string secondaryTerm, bool RemoveNonASCII);

		public delegate void DelegateDoLocalize(string primaryTerm, string secondaryTerm);

		public string mTerm = string.Empty;

		public string mTermSecondary = string.Empty;

		[NonSerialized]
		public string FinalTerm;

		[NonSerialized]
		public string FinalSecondaryTerm;

		public TermModification PrimaryTermModifier;

		public TermModification SecondaryTermModifier;

		public string TermPrefix;

		public string TermSuffix;

		public bool LocalizeOnAwake = true;

		private string LastLocalizedLanguage;

		public UnityEngine.Object mTarget;

		public DelegateSetFinalTerms EventSetFinalTerms;

		public DelegateDoLocalize EventDoLocalize;

		public bool CanUseSecondaryTerm;

		public bool AllowMainTermToBeRTL;

		public bool AllowSecondTermToBeRTL;

		public bool IgnoreRTL;

		public int MaxCharactersInRTL;

		public bool IgnoreNumbersInRTL = true;

		public bool CorrectAlignmentForRTL = true;

		public UnityEngine.Object[] TranslatedObjects;

		public EventCallback LocalizeCallBack = new EventCallback();

		public static string MainTranslation;

		public static string SecondaryTranslation;

		public static string CallBackTerm;

		public static string CallBackSecondaryTerm;

		public static Localize CurrentLocalizeComponent;

		public bool AlwaysForceLocalize;

		public bool mGUI_ShowReferences;

		public bool mGUI_ShowTems = true;

		public bool mGUI_ShowCallback;

		private TextMeshPro mTarget_TMPLabel;

		private TextMeshProUGUI mTarget_TMPUGUILabel;

		private TextAlignmentOptions mAlignmentTMPro_RTL = TextAlignmentOptions.TopRight;

		private TextAlignmentOptions mAlignmentTMPro_LTR = TextAlignmentOptions.TopLeft;

		private bool mAlignmentTMPwasRTL;

		[NonSerialized]
		public string TMP_previewLanguage;

		private Text mTarget_uGUI_Text;

		private Image mTarget_uGUI_Image;

		private RawImage mTarget_uGUI_RawImage;

		private TextAnchor mAlignmentUGUI_RTL = TextAnchor.UpperRight;

		private TextAnchor mAlignmentUGUI_LTR;

		private bool mAlignmentUGUIwasRTL;

		private GUIText mTarget_GUIText;

		private TextMesh mTarget_TextMesh;

		private AudioSource mTarget_AudioSource;

		private GUITexture mTarget_GUITexture;

		private GameObject mTarget_Child;

		private SpriteRenderer mTarget_SpriteRenderer;

		private bool mInitializeAlignment = true;

		private TextAlignment mAlignmentStd_LTR;

		private TextAlignment mAlignmentStd_RTL = TextAlignment.Right;

		public string Term
		{
			get
			{
				return mTerm;
			}
			set
			{
				SetTerm(value);
			}
		}

		public string SecondaryTerm
		{
			get
			{
				return mTermSecondary;
			}
			set
			{
				SetTerm(null, value);
			}
		}

		public event Action EventFindTarget;

		private void Awake()
		{
			RegisterTargets();
			if (HasTargetCache())
			{
				this.EventFindTarget();
			}
			if (LocalizeOnAwake)
			{
				OnLocalize();
			}
		}

		private void RegisterTargets()
		{
			if (this.EventFindTarget == null)
			{
				RegisterEvents_NGUI();
				RegisterEvents_DFGUI();
				RegisterEvents_UGUI();
				RegisterEvents_2DToolKit();
				RegisterEvents_TextMeshPro();
				RegisterEvents_UnityStandard();
				RegisterEvents_SVG();
			}
		}

		private void OnEnable()
		{
			OnLocalize();
		}

		public void OnLocalize(bool Force = false)
		{
			if ((!Force && (!base.enabled || base.gameObject == null || !base.gameObject.activeInHierarchy)) || string.IsNullOrEmpty(LocalizationManager.CurrentLanguage) || (!AlwaysForceLocalize && !Force && !LocalizeCallBack.HasCallback() && LastLocalizedLanguage == LocalizationManager.CurrentLanguage))
			{
				return;
			}
			LastLocalizedLanguage = LocalizationManager.CurrentLanguage;
			if (!HasTargetCache())
			{
				FindTarget();
			}
			if (!HasTargetCache())
			{
				return;
			}
			if (string.IsNullOrEmpty(FinalTerm) || string.IsNullOrEmpty(FinalSecondaryTerm))
			{
				GetFinalTerms(out FinalTerm, out FinalSecondaryTerm);
			}
			bool flag = Application.isPlaying && LocalizeCallBack.HasCallback();
			if (!flag && string.IsNullOrEmpty(FinalTerm) && string.IsNullOrEmpty(FinalSecondaryTerm))
			{
				return;
			}
			CallBackTerm = FinalTerm;
			CallBackSecondaryTerm = FinalSecondaryTerm;
			MainTranslation = ((string.IsNullOrEmpty(FinalTerm) || FinalTerm == "-") ? null : LocalizationManager.GetTermTranslation(FinalTerm, FixForRTL: false));
			SecondaryTranslation = ((string.IsNullOrEmpty(FinalSecondaryTerm) || FinalSecondaryTerm == "-") ? null : LocalizationManager.GetTermTranslation(FinalSecondaryTerm, FixForRTL: false));
			if (!flag && string.IsNullOrEmpty(FinalTerm) && string.IsNullOrEmpty(SecondaryTranslation))
			{
				return;
			}
			CurrentLocalizeComponent = this;
			if (Application.isPlaying)
			{
				LocalizeCallBack.Execute(this);
				LocalizationManager.ApplyLocalizationParams(ref MainTranslation, base.gameObject);
			}
			bool flag2 = LocalizationManager.IsRight2Left && !IgnoreRTL;
			if (flag2)
			{
				if (AllowMainTermToBeRTL && !string.IsNullOrEmpty(MainTranslation))
				{
					MainTranslation = LocalizationManager.ApplyRTLfix(MainTranslation, MaxCharactersInRTL, IgnoreNumbersInRTL);
				}
				if (AllowSecondTermToBeRTL && !string.IsNullOrEmpty(SecondaryTranslation))
				{
					SecondaryTranslation = LocalizationManager.ApplyRTLfix(SecondaryTranslation);
				}
			}
			if (PrimaryTermModifier != 0)
			{
				MainTranslation = (MainTranslation ?? string.Empty);
			}
			switch (PrimaryTermModifier)
			{
			case TermModification.ToUpper:
				MainTranslation = MainTranslation.ToUpper();
				break;
			case TermModification.ToLower:
				MainTranslation = MainTranslation.ToLower();
				break;
			case TermModification.ToUpperFirst:
				MainTranslation = GoogleTranslation.UppercaseFirst(MainTranslation);
				break;
			case TermModification.ToTitle:
				MainTranslation = GoogleTranslation.TitleCase(MainTranslation);
				break;
			}
			if (SecondaryTermModifier != 0)
			{
				SecondaryTranslation = (SecondaryTranslation ?? string.Empty);
			}
			switch (SecondaryTermModifier)
			{
			case TermModification.ToUpper:
				SecondaryTranslation = SecondaryTranslation.ToUpper();
				break;
			case TermModification.ToLower:
				SecondaryTranslation = SecondaryTranslation.ToLower();
				break;
			case TermModification.ToUpperFirst:
				SecondaryTranslation = GoogleTranslation.UppercaseFirst(SecondaryTranslation);
				break;
			case TermModification.ToTitle:
				SecondaryTranslation = GoogleTranslation.TitleCase(SecondaryTranslation);
				break;
			}
			if (!string.IsNullOrEmpty(TermPrefix))
			{
				MainTranslation = (flag2 ? (MainTranslation + TermPrefix) : (TermPrefix + MainTranslation));
			}
			if (!string.IsNullOrEmpty(TermSuffix))
			{
				MainTranslation = (flag2 ? (TermSuffix + MainTranslation) : (MainTranslation + TermSuffix));
			}
			EventDoLocalize(MainTranslation, SecondaryTranslation);
			CurrentLocalizeComponent = null;
		}

		public bool FindTarget()
		{
			if (HasTargetCache())
			{
				return true;
			}
			if (this.EventFindTarget == null)
			{
				RegisterTargets();
			}
			this.EventFindTarget();
			return HasTargetCache();
		}

		public void FindAndCacheTarget<T>(ref T targetCache, DelegateSetFinalTerms setFinalTerms, DelegateDoLocalize doLocalize, bool UseSecondaryTerm, bool MainRTL, bool SecondRTL) where T : Component
		{
			if (mTarget != null)
			{
				targetCache = (mTarget as T);
			}
			else
			{
				mTarget = (targetCache = GetComponent<T>());
			}
			if ((UnityEngine.Object)targetCache != (UnityEngine.Object)null)
			{
				EventSetFinalTerms = setFinalTerms;
				EventDoLocalize = doLocalize;
				CanUseSecondaryTerm = UseSecondaryTerm;
				AllowMainTermToBeRTL = MainRTL;
				AllowSecondTermToBeRTL = SecondRTL;
			}
		}

		private void FindAndCacheTarget(ref GameObject targetCache, DelegateSetFinalTerms setFinalTerms, DelegateDoLocalize doLocalize, bool UseSecondaryTerm, bool MainRTL, bool SecondRTL)
		{
			if (mTarget != targetCache && (bool)targetCache)
			{
				UnityEngine.Object.Destroy(targetCache);
			}
			if (mTarget != null)
			{
				targetCache = (mTarget as GameObject);
			}
			else
			{
				Transform transform = base.transform;
				mTarget = (targetCache = ((transform.childCount < 1) ? null : transform.GetChild(0).gameObject));
			}
			if (targetCache != null)
			{
				EventSetFinalTerms = setFinalTerms;
				EventDoLocalize = doLocalize;
				CanUseSecondaryTerm = UseSecondaryTerm;
				AllowMainTermToBeRTL = MainRTL;
				AllowSecondTermToBeRTL = SecondRTL;
			}
		}

		private bool HasTargetCache()
		{
			return EventDoLocalize != null;
		}

		public void GetFinalTerms(out string primaryTerm, out string secondaryTerm)
		{
			if (EventSetFinalTerms == null || (!mTarget && !HasTargetCache()))
			{
				FindTarget();
			}
			primaryTerm = string.Empty;
			secondaryTerm = string.Empty;
			if (mTarget != null && (string.IsNullOrEmpty(mTerm) || string.IsNullOrEmpty(mTermSecondary)) && EventSetFinalTerms != null)
			{
				EventSetFinalTerms(mTerm, mTermSecondary, out primaryTerm, out secondaryTerm, RemoveNonASCII: true);
			}
			if (!string.IsNullOrEmpty(mTerm))
			{
				primaryTerm = mTerm;
			}
			if (!string.IsNullOrEmpty(mTermSecondary))
			{
				secondaryTerm = mTermSecondary;
			}
			if (primaryTerm != null)
			{
				primaryTerm = primaryTerm.Trim();
			}
			if (secondaryTerm != null)
			{
				secondaryTerm = secondaryTerm.Trim();
			}
		}

		public string GetMainTargetsText(bool RemoveNonASCII)
		{
			string primaryTerm = null;
			string secondaryTerm = null;
			if (EventSetFinalTerms != null)
			{
				EventSetFinalTerms(null, null, out primaryTerm, out secondaryTerm, RemoveNonASCII);
			}
			if (!string.IsNullOrEmpty(primaryTerm))
			{
				return primaryTerm;
			}
			return mTerm;
		}

		private void SetFinalTerms(string Main, string Secondary, out string primaryTerm, out string secondaryTerm, bool RemoveNonASCII)
		{
			primaryTerm = ((RemoveNonASCII && !string.IsNullOrEmpty(Main)) ? Regex.Replace(Main, "[^a-zA-Z0-9_ ]+", " ") : Main);
			secondaryTerm = Secondary;
		}

		public void SetTerm(string primary)
		{
			if (!string.IsNullOrEmpty(primary))
			{
				FinalTerm = (mTerm = primary);
			}
			OnLocalize(Force: true);
		}

		public void SetTerm(string primary, string secondary)
		{
			if (!string.IsNullOrEmpty(primary))
			{
				FinalTerm = (mTerm = primary);
			}
			FinalSecondaryTerm = (mTermSecondary = secondary);
			OnLocalize(Force: true);
		}

		private T GetSecondaryTranslatedObj<T>(ref string mainTranslation, ref string secondaryTranslation) where T : UnityEngine.Object
		{
			DeserializeTranslation(mainTranslation, out string value, out string secondary);
			T val = null;
			if (!string.IsNullOrEmpty(secondary))
			{
				val = GetObject<T>(secondary);
				if ((UnityEngine.Object)val != (UnityEngine.Object)null)
				{
					mainTranslation = value;
					secondaryTranslation = secondary;
				}
			}
			if ((UnityEngine.Object)val == (UnityEngine.Object)null)
			{
				val = GetObject<T>(secondaryTranslation);
			}
			return val;
		}

		private T GetObject<T>(string Translation) where T : UnityEngine.Object
		{
			if (string.IsNullOrEmpty(Translation))
			{
				return null;
			}
			T translatedObject = GetTranslatedObject<T>(Translation);
			if ((UnityEngine.Object)translatedObject == (UnityEngine.Object)null)
			{
				translatedObject = GetTranslatedObject<T>(Translation);
			}
			return translatedObject;
		}

		private T GetTranslatedObject<T>(string Translation) where T : UnityEngine.Object
		{
			return FindTranslatedObject<T>(Translation);
		}

		private void DeserializeTranslation(string translation, out string value, out string secondary)
		{
			if (!string.IsNullOrEmpty(translation) && translation.Length > 1 && translation[0] == '[')
			{
				int num = translation.IndexOf(']');
				if (num > 0)
				{
					secondary = translation.Substring(1, num - 1);
					value = translation.Substring(num + 1);
					return;
				}
			}
			value = translation;
			secondary = string.Empty;
		}

		public T FindTranslatedObject<T>(string value) where T : UnityEngine.Object
		{
			if (string.IsNullOrEmpty(value))
			{
				return null;
			}
			if (TranslatedObjects != null)
			{
				int i = 0;
				for (int num = TranslatedObjects.Length; i < num; i++)
				{
					if (TranslatedObjects[i] is T && value.EndsWith(TranslatedObjects[i].name, StringComparison.OrdinalIgnoreCase) && string.Compare(value, TranslatedObjects[i].name, StringComparison.OrdinalIgnoreCase) == 0)
					{
						return (T)TranslatedObjects[i];
					}
				}
			}
			T val = LocalizationManager.FindAsset(value) as T;
			if ((bool)(UnityEngine.Object)val)
			{
				return val;
			}
			return ResourceManager.pInstance.GetAsset<T>(value);
		}

		public bool HasTranslatedObject(UnityEngine.Object Obj)
		{
			if (Array.IndexOf(TranslatedObjects, Obj) >= 0)
			{
				return true;
			}
			return ResourceManager.pInstance.HasAsset(Obj);
		}

		public void AddTranslatedObject(UnityEngine.Object Obj)
		{
			Array.Resize(ref TranslatedObjects, TranslatedObjects.Length + 1);
			TranslatedObjects[TranslatedObjects.Length - 1] = Obj;
		}

		public void SetGlobalLanguage(string Language)
		{
			LocalizationManager.CurrentLanguage = Language;
		}

		public static void RegisterEvents_2DToolKit()
		{
		}

		public static void RegisterEvents_DFGUI()
		{
		}

		public static void RegisterEvents_NGUI()
		{
		}

		public static void RegisterEvents_SVG()
		{
		}

		public void RegisterEvents_TextMeshPro()
		{
			EventFindTarget += FindTarget_TMPLabel;
			EventFindTarget += FindTarget_TMPUGUILabel;
		}

		private void FindTarget_TMPLabel()
		{
			FindAndCacheTarget(ref mTarget_TMPLabel, this.SetFinalTerms_TMPLabel, DoLocalize_TMPLabel, UseSecondaryTerm: true, MainRTL: true, SecondRTL: false);
		}

		private void FindTarget_TMPUGUILabel()
		{
			FindAndCacheTarget(ref mTarget_TMPUGUILabel, this.SetFinalTerms_TMPUGUILabel, DoLocalize_TMPUGUILabel, UseSecondaryTerm: true, MainRTL: true, SecondRTL: false);
		}

		private void SetFinalTerms_TMPLabel(string Main, string Secondary, out string primaryTerm, out string secondaryTerm, bool RemoveNonASCII)
		{
			string secondary = (mTarget_TMPLabel.font != null) ? mTarget_TMPLabel.font.name : string.Empty;
			SetFinalTerms(mTarget_TMPLabel.text, secondary, out primaryTerm, out secondaryTerm, RemoveNonASCII);
		}

		private void SetFinalTerms_TMPUGUILabel(string Main, string Secondary, out string primaryTerm, out string secondaryTerm, bool RemoveNonASCII)
		{
			string secondary = (mTarget_TMPUGUILabel.font != null) ? mTarget_TMPUGUILabel.font.name : string.Empty;
			SetFinalTerms(mTarget_TMPUGUILabel.text, secondary, out primaryTerm, out secondaryTerm, RemoveNonASCII);
		}

		public void DoLocalize_TMPLabel(string mainTranslation, string secondaryTranslation)
		{
			bool isPlaying = Application.isPlaying;
			TMP_FontAsset secondaryTranslatedObj = GetSecondaryTranslatedObj<TMP_FontAsset>(ref mainTranslation, ref secondaryTranslation);
			if (secondaryTranslatedObj != null)
			{
				if (mTarget_TMPLabel.font != secondaryTranslatedObj)
				{
					mTarget_TMPLabel.font = secondaryTranslatedObj;
				}
			}
			else
			{
				Material secondaryTranslatedObj2 = GetSecondaryTranslatedObj<Material>(ref mainTranslation, ref secondaryTranslation);
				if (secondaryTranslatedObj2 != null && mTarget_TMPLabel.fontMaterial != secondaryTranslatedObj2)
				{
					if (!secondaryTranslatedObj2.name.StartsWith(mTarget_TMPLabel.font.name, StringComparison.Ordinal))
					{
						secondaryTranslatedObj = GetTMPFontFromMaterial(secondaryTranslation.EndsWith(secondaryTranslatedObj2.name, StringComparison.Ordinal) ? secondaryTranslation : secondaryTranslatedObj2.name);
						if (secondaryTranslatedObj != null)
						{
							mTarget_TMPLabel.font = secondaryTranslatedObj;
						}
					}
					mTarget_TMPLabel.fontSharedMaterial = secondaryTranslatedObj2;
				}
			}
			if (mInitializeAlignment)
			{
				mInitializeAlignment = false;
				mAlignmentTMPwasRTL = LocalizationManager.IsRight2Left;
				InitAlignment_TMPro(mAlignmentTMPwasRTL, mTarget_TMPLabel.alignment, out mAlignmentTMPro_LTR, out mAlignmentTMPro_RTL);
			}
			else
			{
				InitAlignment_TMPro(mAlignmentTMPwasRTL, mTarget_TMPLabel.alignment, out TextAlignmentOptions alignLTR, out TextAlignmentOptions alignRTL);
				if ((mAlignmentTMPwasRTL && mAlignmentTMPro_RTL != alignRTL) || (!mAlignmentTMPwasRTL && mAlignmentTMPro_LTR != alignLTR))
				{
					mAlignmentTMPro_LTR = alignLTR;
					mAlignmentTMPro_RTL = alignRTL;
				}
				mAlignmentTMPwasRTL = LocalizationManager.IsRight2Left;
			}
			if (mainTranslation == null || !(mTarget_TMPLabel.text != mainTranslation))
			{
				return;
			}
			if (CurrentLocalizeComponent.CorrectAlignmentForRTL)
			{
				mTarget_TMPLabel.alignment = (LocalizationManager.IsRight2Left ? mAlignmentTMPro_RTL : mAlignmentTMPro_LTR);
				mTarget_TMPLabel.isRightToLeftText = LocalizationManager.IsRight2Left;
				if (LocalizationManager.IsRight2Left)
				{
					mainTranslation = ReverseText(mainTranslation);
				}
			}
			mTarget_TMPLabel.text = mainTranslation;
		}

		private void InitAlignment_TMPro(bool isRTL, TextAlignmentOptions alignment, out TextAlignmentOptions alignLTR, out TextAlignmentOptions alignRTL)
		{
			alignLTR = (alignRTL = alignment);
			if (isRTL)
			{
				switch (alignment)
				{
				case TextAlignmentOptions.TopRight:
					alignLTR = TextAlignmentOptions.TopLeft;
					break;
				case TextAlignmentOptions.Right:
					alignLTR = TextAlignmentOptions.Left;
					break;
				case TextAlignmentOptions.BottomRight:
					alignLTR = TextAlignmentOptions.BottomLeft;
					break;
				case TextAlignmentOptions.BaselineRight:
					alignLTR = TextAlignmentOptions.BaselineLeft;
					break;
				case TextAlignmentOptions.MidlineRight:
					alignLTR = TextAlignmentOptions.MidlineLeft;
					break;
				case TextAlignmentOptions.CaplineRight:
					alignLTR = TextAlignmentOptions.CaplineLeft;
					break;
				case TextAlignmentOptions.TopLeft:
					alignLTR = TextAlignmentOptions.TopRight;
					break;
				case TextAlignmentOptions.Left:
					alignLTR = TextAlignmentOptions.Right;
					break;
				case TextAlignmentOptions.BottomLeft:
					alignLTR = TextAlignmentOptions.BottomRight;
					break;
				case TextAlignmentOptions.BaselineLeft:
					alignLTR = TextAlignmentOptions.BaselineRight;
					break;
				case TextAlignmentOptions.MidlineLeft:
					alignLTR = TextAlignmentOptions.MidlineRight;
					break;
				case TextAlignmentOptions.CaplineLeft:
					alignLTR = TextAlignmentOptions.CaplineRight;
					break;
				}
			}
			else
			{
				switch (alignment)
				{
				case TextAlignmentOptions.TopRight:
					alignRTL = TextAlignmentOptions.TopLeft;
					break;
				case TextAlignmentOptions.Right:
					alignRTL = TextAlignmentOptions.Left;
					break;
				case TextAlignmentOptions.BottomRight:
					alignRTL = TextAlignmentOptions.BottomLeft;
					break;
				case TextAlignmentOptions.BaselineRight:
					alignRTL = TextAlignmentOptions.BaselineLeft;
					break;
				case TextAlignmentOptions.MidlineRight:
					alignRTL = TextAlignmentOptions.MidlineLeft;
					break;
				case TextAlignmentOptions.CaplineRight:
					alignRTL = TextAlignmentOptions.CaplineLeft;
					break;
				case TextAlignmentOptions.TopLeft:
					alignRTL = TextAlignmentOptions.TopRight;
					break;
				case TextAlignmentOptions.Left:
					alignRTL = TextAlignmentOptions.Right;
					break;
				case TextAlignmentOptions.BottomLeft:
					alignRTL = TextAlignmentOptions.BottomRight;
					break;
				case TextAlignmentOptions.BaselineLeft:
					alignRTL = TextAlignmentOptions.BaselineRight;
					break;
				case TextAlignmentOptions.MidlineLeft:
					alignRTL = TextAlignmentOptions.MidlineRight;
					break;
				case TextAlignmentOptions.CaplineLeft:
					alignRTL = TextAlignmentOptions.CaplineRight;
					break;
				}
			}
		}

		public void DoLocalize_TMPUGUILabel(string mainTranslation, string secondaryTranslation)
		{
			TMP_FontAsset secondaryTranslatedObj = GetSecondaryTranslatedObj<TMP_FontAsset>(ref mainTranslation, ref secondaryTranslation);
			if (secondaryTranslatedObj != null)
			{
				if (mTarget_TMPUGUILabel.font != secondaryTranslatedObj)
				{
					mTarget_TMPUGUILabel.font = secondaryTranslatedObj;
				}
			}
			else
			{
				Material secondaryTranslatedObj2 = GetSecondaryTranslatedObj<Material>(ref mainTranslation, ref secondaryTranslation);
				if (secondaryTranslatedObj2 != null && mTarget_TMPUGUILabel.fontMaterial != secondaryTranslatedObj2)
				{
					if (!secondaryTranslatedObj2.name.StartsWith(mTarget_TMPUGUILabel.font.name, StringComparison.Ordinal))
					{
						secondaryTranslatedObj = GetTMPFontFromMaterial(secondaryTranslation.EndsWith(secondaryTranslatedObj2.name, StringComparison.Ordinal) ? secondaryTranslation : secondaryTranslatedObj2.name);
						if (secondaryTranslatedObj != null)
						{
							mTarget_TMPUGUILabel.font = secondaryTranslatedObj;
						}
					}
					mTarget_TMPUGUILabel.fontSharedMaterial = secondaryTranslatedObj2;
				}
			}
			if (mInitializeAlignment)
			{
				mInitializeAlignment = false;
				mAlignmentTMPwasRTL = LocalizationManager.IsRight2Left;
				InitAlignment_TMPro(mAlignmentTMPwasRTL, mTarget_TMPUGUILabel.alignment, out mAlignmentTMPro_LTR, out mAlignmentTMPro_RTL);
			}
			else
			{
				InitAlignment_TMPro(mAlignmentTMPwasRTL, mTarget_TMPUGUILabel.alignment, out TextAlignmentOptions alignLTR, out TextAlignmentOptions alignRTL);
				if ((mAlignmentTMPwasRTL && mAlignmentTMPro_RTL != alignRTL) || (!mAlignmentTMPwasRTL && mAlignmentTMPro_LTR != alignLTR))
				{
					mAlignmentTMPro_LTR = alignLTR;
					mAlignmentTMPro_RTL = alignRTL;
				}
				mAlignmentTMPwasRTL = LocalizationManager.IsRight2Left;
			}
			if (mainTranslation == null || !(mTarget_TMPUGUILabel.text != mainTranslation))
			{
				return;
			}
			if (CurrentLocalizeComponent.CorrectAlignmentForRTL)
			{
				mTarget_TMPUGUILabel.alignment = (LocalizationManager.IsRight2Left ? mAlignmentTMPro_RTL : mAlignmentTMPro_LTR);
				mTarget_TMPUGUILabel.isRightToLeftText = LocalizationManager.IsRight2Left;
				if (LocalizationManager.IsRight2Left)
				{
					mainTranslation = ReverseText(mainTranslation);
				}
			}
			mTarget_TMPUGUILabel.text = mainTranslation;
		}

		private string ReverseText(string source)
		{
			int length = source.Length;
			char[] array = new char[length];
			for (int i = 0; i < length; i++)
			{
				array[length - 1 - i] = source[i];
			}
			return new string(array);
		}

		private TMP_FontAsset GetTMPFontFromMaterial(string matName)
		{
			string text = " .\\/-[]()";
			int num = matName.Length - 1;
			while (num > 0)
			{
				while (num > 0 && text.IndexOf(matName[num]) >= 0)
				{
					num--;
				}
				if (num <= 0)
				{
					break;
				}
				string translation = matName.Substring(0, num + 1);
				TMP_FontAsset @object = GetObject<TMP_FontAsset>(translation);
				if (@object != null)
				{
					return @object;
				}
				while (num > 0 && text.IndexOf(matName[num]) < 0)
				{
					num--;
				}
			}
			return null;
		}

		public void RegisterEvents_UGUI()
		{
			EventFindTarget += FindTarget_uGUI_Text;
			EventFindTarget += FindTarget_uGUI_Image;
			EventFindTarget += FindTarget_uGUI_RawImage;
		}

		private void FindTarget_uGUI_Text()
		{
			FindAndCacheTarget(ref mTarget_uGUI_Text, this.SetFinalTerms_uGUI_Text, DoLocalize_uGUI_Text, UseSecondaryTerm: true, MainRTL: true, SecondRTL: false);
		}

		private void FindTarget_uGUI_Image()
		{
			FindAndCacheTarget(ref mTarget_uGUI_Image, this.SetFinalTerms_uGUI_Image, DoLocalize_uGUI_Image, UseSecondaryTerm: false, MainRTL: false, SecondRTL: false);
		}

		private void FindTarget_uGUI_RawImage()
		{
			FindAndCacheTarget(ref mTarget_uGUI_RawImage, this.SetFinalTerms_uGUI_RawImage, DoLocalize_uGUI_RawImage, UseSecondaryTerm: false, MainRTL: false, SecondRTL: false);
		}

		private void SetFinalTerms_uGUI_Text(string Main, string Secondary, out string primaryTerm, out string secondaryTerm, bool RemoveNonASCII)
		{
			string secondary = (mTarget_uGUI_Text.font != null) ? mTarget_uGUI_Text.font.name : string.Empty;
			SetFinalTerms(mTarget_uGUI_Text.text, secondary, out primaryTerm, out secondaryTerm, RemoveNonASCII);
		}

		public void SetFinalTerms_uGUI_Image(string Main, string Secondary, out string primaryTerm, out string secondaryTerm, bool RemoveNonASCII)
		{
			SetFinalTerms(mTarget_uGUI_Image.mainTexture ? mTarget_uGUI_Image.mainTexture.name : "", null, out primaryTerm, out secondaryTerm, RemoveNonASCII: false);
		}

		public void SetFinalTerms_uGUI_RawImage(string Main, string Secondary, out string primaryTerm, out string secondaryTerm, bool RemoveNonASCII)
		{
			SetFinalTerms(mTarget_uGUI_RawImage.texture ? mTarget_uGUI_RawImage.texture.name : "", null, out primaryTerm, out secondaryTerm, RemoveNonASCII: false);
		}

		public static T FindInParents<T>(Transform tr) where T : Component
		{
			if (!tr)
			{
				return null;
			}
			T component = tr.GetComponent<T>();
			while (!(UnityEngine.Object)component && (bool)tr)
			{
				component = tr.GetComponent<T>();
				tr = tr.parent;
			}
			return component;
		}

		public void DoLocalize_uGUI_Text(string mainTranslation, string secondaryTranslation)
		{
			Font secondaryTranslatedObj = GetSecondaryTranslatedObj<Font>(ref mainTranslation, ref secondaryTranslation);
			if (secondaryTranslatedObj != null && secondaryTranslatedObj != mTarget_uGUI_Text.font)
			{
				mTarget_uGUI_Text.font = secondaryTranslatedObj;
			}
			if (mInitializeAlignment)
			{
				mInitializeAlignment = false;
				mAlignmentUGUIwasRTL = LocalizationManager.IsRight2Left;
				InitAlignment_UGUI(mAlignmentUGUIwasRTL, mTarget_uGUI_Text.alignment, out mAlignmentUGUI_LTR, out mAlignmentUGUI_RTL);
			}
			else
			{
				InitAlignment_UGUI(mAlignmentUGUIwasRTL, mTarget_uGUI_Text.alignment, out TextAnchor alignLTR, out TextAnchor alignRTL);
				if ((mAlignmentUGUIwasRTL && mAlignmentUGUI_RTL != alignRTL) || (!mAlignmentUGUIwasRTL && mAlignmentUGUI_LTR != alignLTR))
				{
					mAlignmentUGUI_LTR = alignLTR;
					mAlignmentUGUI_RTL = alignRTL;
				}
				mAlignmentUGUIwasRTL = LocalizationManager.IsRight2Left;
			}
			if (mainTranslation != null && mTarget_uGUI_Text.text != mainTranslation)
			{
				if (CurrentLocalizeComponent.CorrectAlignmentForRTL)
				{
					mTarget_uGUI_Text.alignment = (LocalizationManager.IsRight2Left ? mAlignmentUGUI_RTL : mAlignmentUGUI_LTR);
				}
				mTarget_uGUI_Text.text = mainTranslation;
				mTarget_uGUI_Text.SetVerticesDirty();
			}
		}

		private void InitAlignment_UGUI(bool isRTL, TextAnchor alignment, out TextAnchor alignLTR, out TextAnchor alignRTL)
		{
			alignLTR = (alignRTL = alignment);
			if (isRTL)
			{
				switch (alignment)
				{
				case TextAnchor.UpperCenter:
				case TextAnchor.MiddleCenter:
				case TextAnchor.LowerCenter:
					break;
				case TextAnchor.UpperRight:
					alignLTR = TextAnchor.UpperLeft;
					break;
				case TextAnchor.MiddleRight:
					alignLTR = TextAnchor.MiddleLeft;
					break;
				case TextAnchor.LowerRight:
					alignLTR = TextAnchor.LowerLeft;
					break;
				case TextAnchor.UpperLeft:
					alignLTR = TextAnchor.UpperRight;
					break;
				case TextAnchor.MiddleLeft:
					alignLTR = TextAnchor.MiddleRight;
					break;
				case TextAnchor.LowerLeft:
					alignLTR = TextAnchor.LowerRight;
					break;
				}
			}
			else
			{
				switch (alignment)
				{
				case TextAnchor.UpperCenter:
				case TextAnchor.MiddleCenter:
				case TextAnchor.LowerCenter:
					break;
				case TextAnchor.UpperRight:
					alignRTL = TextAnchor.UpperLeft;
					break;
				case TextAnchor.MiddleRight:
					alignRTL = TextAnchor.MiddleLeft;
					break;
				case TextAnchor.LowerRight:
					alignRTL = TextAnchor.LowerLeft;
					break;
				case TextAnchor.UpperLeft:
					alignRTL = TextAnchor.UpperRight;
					break;
				case TextAnchor.MiddleLeft:
					alignRTL = TextAnchor.MiddleRight;
					break;
				case TextAnchor.LowerLeft:
					alignRTL = TextAnchor.LowerRight;
					break;
				}
			}
		}

		public void DoLocalize_uGUI_Image(string mainTranslation, string secondaryTranslation)
		{
			Sprite sprite = mTarget_uGUI_Image.sprite;
			if (sprite == null || sprite.name != mainTranslation)
			{
				mTarget_uGUI_Image.sprite = FindTranslatedObject<Sprite>(mainTranslation);
			}
		}

		public void DoLocalize_uGUI_RawImage(string mainTranslation, string secondaryTranslation)
		{
			Texture texture = mTarget_uGUI_RawImage.texture;
			if (texture == null || texture.name != mainTranslation)
			{
				mTarget_uGUI_RawImage.texture = FindTranslatedObject<Texture>(mainTranslation);
			}
		}

		public void RegisterEvents_UnityStandard()
		{
			EventFindTarget += FindTarget_GUIText;
			EventFindTarget += FindTarget_TextMesh;
			EventFindTarget += FindTarget_AudioSource;
			EventFindTarget += FindTarget_GUITexture;
			EventFindTarget += FindTarget_Child;
			EventFindTarget += FindTarget_SpriteRenderer;
		}

		private void FindTarget_GUIText()
		{
			FindAndCacheTarget(ref mTarget_GUIText, this.SetFinalTerms_GUIText, DoLocalize_GUIText, UseSecondaryTerm: true, MainRTL: true, SecondRTL: false);
		}

		private void FindTarget_TextMesh()
		{
			FindAndCacheTarget(ref mTarget_TextMesh, this.SetFinalTerms_TextMesh, DoLocalize_TextMesh, UseSecondaryTerm: true, MainRTL: true, SecondRTL: false);
		}

		private void FindTarget_AudioSource()
		{
			FindAndCacheTarget(ref mTarget_AudioSource, this.SetFinalTerms_AudioSource, DoLocalize_AudioSource, UseSecondaryTerm: false, MainRTL: false, SecondRTL: false);
		}

		private void FindTarget_GUITexture()
		{
			FindAndCacheTarget(ref mTarget_GUITexture, this.SetFinalTerms_GUITexture, DoLocalize_GUITexture, UseSecondaryTerm: false, MainRTL: false, SecondRTL: false);
		}

		private void FindTarget_Child()
		{
			FindAndCacheTarget(ref mTarget_Child, this.SetFinalTerms_Child, DoLocalize_Child, UseSecondaryTerm: false, MainRTL: false, SecondRTL: false);
		}

		private void FindTarget_SpriteRenderer()
		{
			FindAndCacheTarget(ref mTarget_SpriteRenderer, this.SetFinalTerms_SpriteRenderer, DoLocalize_SpriteRenderer, UseSecondaryTerm: false, MainRTL: false, SecondRTL: false);
		}

		public void SetFinalTerms_GUIText(string Main, string Secondary, out string PrimaryTerm, out string secondaryTranslation, bool RemoveNonASCII)
		{
			if (string.IsNullOrEmpty(Secondary) && mTarget_GUIText.font != null)
			{
				Secondary = mTarget_GUIText.font.name;
			}
			SetFinalTerms(mTarget_GUIText.text, Secondary, out PrimaryTerm, out secondaryTranslation, RemoveNonASCII);
		}

		public void SetFinalTerms_TextMesh(string Main, string Secondary, out string PrimaryTerm, out string secondaryTranslation, bool RemoveNonASCII)
		{
			string secondary = (mTarget_TextMesh.font != null) ? mTarget_TextMesh.font.name : string.Empty;
			SetFinalTerms(mTarget_TextMesh.text, secondary, out PrimaryTerm, out secondaryTranslation, RemoveNonASCII);
		}

		public void SetFinalTerms_GUITexture(string Main, string Secondary, out string PrimaryTerm, out string secondaryTranslation, bool RemoveNonASCII)
		{
			if (!mTarget_GUITexture || !mTarget_GUITexture.texture)
			{
				SetFinalTerms(string.Empty, string.Empty, out PrimaryTerm, out secondaryTranslation, RemoveNonASCII: false);
			}
			else
			{
				SetFinalTerms(mTarget_GUITexture.texture.name, string.Empty, out PrimaryTerm, out secondaryTranslation, RemoveNonASCII: false);
			}
		}

		public void SetFinalTerms_AudioSource(string Main, string Secondary, out string PrimaryTerm, out string secondaryTranslation, bool RemoveNonASCII)
		{
			if (!mTarget_AudioSource || !mTarget_AudioSource.clip)
			{
				SetFinalTerms(string.Empty, string.Empty, out PrimaryTerm, out secondaryTranslation, RemoveNonASCII: false);
			}
			else
			{
				SetFinalTerms(mTarget_AudioSource.clip.name, string.Empty, out PrimaryTerm, out secondaryTranslation, RemoveNonASCII: false);
			}
		}

		public void SetFinalTerms_Child(string Main, string Secondary, out string PrimaryTerm, out string secondaryTranslation, bool RemoveNonASCII)
		{
			SetFinalTerms(mTarget_Child.name, string.Empty, out PrimaryTerm, out secondaryTranslation, RemoveNonASCII: false);
		}

		public void SetFinalTerms_SpriteRenderer(string Main, string Secondary, out string PrimaryTerm, out string secondaryTranslation, bool RemoveNonASCII)
		{
			SetFinalTerms((mTarget_SpriteRenderer.sprite != null) ? mTarget_SpriteRenderer.sprite.name : string.Empty, string.Empty, out PrimaryTerm, out secondaryTranslation, RemoveNonASCII: false);
		}

		private void DoLocalize_GUIText(string mainTranslation, string secondaryTranslation)
		{
			Font secondaryTranslatedObj = GetSecondaryTranslatedObj<Font>(ref mainTranslation, ref secondaryTranslation);
			if (secondaryTranslatedObj != null && mTarget_GUIText.font != secondaryTranslatedObj)
			{
				mTarget_GUIText.font = secondaryTranslatedObj;
			}
			if (mInitializeAlignment)
			{
				mInitializeAlignment = false;
				mAlignmentStd_LTR = (mAlignmentStd_RTL = mTarget_GUIText.alignment);
				if (LocalizationManager.IsRight2Left && mAlignmentStd_RTL == TextAlignment.Right)
				{
					mAlignmentStd_LTR = TextAlignment.Left;
				}
				if (!LocalizationManager.IsRight2Left && mAlignmentStd_LTR == TextAlignment.Left)
				{
					mAlignmentStd_RTL = TextAlignment.Right;
				}
			}
			if (mainTranslation != null && mTarget_GUIText.text != mainTranslation)
			{
				if (CurrentLocalizeComponent.CorrectAlignmentForRTL && mTarget_GUIText.alignment != TextAlignment.Center)
				{
					mTarget_GUIText.alignment = (LocalizationManager.IsRight2Left ? mAlignmentStd_RTL : mAlignmentStd_LTR);
				}
				mTarget_GUIText.text = mainTranslation;
			}
		}

		private void DoLocalize_TextMesh(string mainTranslation, string secondaryTranslation)
		{
			Font secondaryTranslatedObj = GetSecondaryTranslatedObj<Font>(ref mainTranslation, ref secondaryTranslation);
			if (secondaryTranslatedObj != null && mTarget_TextMesh.font != secondaryTranslatedObj)
			{
				mTarget_TextMesh.font = secondaryTranslatedObj;
				GetComponent<Renderer>().sharedMaterial = secondaryTranslatedObj.material;
			}
			if (mInitializeAlignment)
			{
				mInitializeAlignment = false;
				mAlignmentStd_LTR = (mAlignmentStd_RTL = mTarget_TextMesh.alignment);
				if (LocalizationManager.IsRight2Left && mAlignmentStd_RTL == TextAlignment.Right)
				{
					mAlignmentStd_LTR = TextAlignment.Left;
				}
				if (!LocalizationManager.IsRight2Left && mAlignmentStd_LTR == TextAlignment.Left)
				{
					mAlignmentStd_RTL = TextAlignment.Right;
				}
			}
			if (mainTranslation != null && mTarget_TextMesh.text != mainTranslation)
			{
				if (CurrentLocalizeComponent.CorrectAlignmentForRTL && mTarget_TextMesh.alignment != TextAlignment.Center)
				{
					mTarget_TextMesh.alignment = (LocalizationManager.IsRight2Left ? mAlignmentStd_RTL : mAlignmentStd_LTR);
				}
				mTarget_TextMesh.text = mainTranslation;
			}
		}

		private void DoLocalize_AudioSource(string mainTranslation, string secondaryTranslation)
		{
			bool isPlaying = mTarget_AudioSource.isPlaying;
			AudioClip clip = mTarget_AudioSource.clip;
			AudioClip audioClip = FindTranslatedObject<AudioClip>(mainTranslation);
			if (clip != audioClip)
			{
				mTarget_AudioSource.clip = audioClip;
			}
			if (isPlaying && (bool)mTarget_AudioSource.clip)
			{
				mTarget_AudioSource.Play();
			}
		}

		private void DoLocalize_GUITexture(string mainTranslation, string secondaryTranslation)
		{
			Texture texture = mTarget_GUITexture.texture;
			if (texture != null && texture.name != mainTranslation)
			{
				mTarget_GUITexture.texture = FindTranslatedObject<Texture>(mainTranslation);
			}
		}

		private void DoLocalize_Child(string mainTranslation, string secondaryTranslation)
		{
			if (!mTarget_Child || !(mTarget_Child.name == mainTranslation))
			{
				GameObject gameObject = mTarget_Child;
				GameObject gameObject2 = FindTranslatedObject<GameObject>(mainTranslation);
				if ((bool)gameObject2)
				{
					mTarget_Child = UnityEngine.Object.Instantiate(gameObject2);
					Transform transform = mTarget_Child.transform;
					Transform transform2 = gameObject ? gameObject.transform : gameObject2.transform;
					transform.SetParent(base.transform);
					transform.localScale = transform2.localScale;
					transform.localRotation = transform2.localRotation;
					transform.localPosition = transform2.localPosition;
				}
				if ((bool)gameObject)
				{
					UnityEngine.Object.Destroy(gameObject);
				}
			}
		}

		private void DoLocalize_SpriteRenderer(string mainTranslation, string secondaryTranslation)
		{
			Sprite sprite = mTarget_SpriteRenderer.sprite;
			if (sprite == null || sprite.name != mainTranslation)
			{
				mTarget_SpriteRenderer.sprite = FindTranslatedObject<Sprite>(mainTranslation);
			}
		}
	}
}
