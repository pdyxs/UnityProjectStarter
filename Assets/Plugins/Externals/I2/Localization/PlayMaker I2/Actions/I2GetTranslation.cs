using HutongGames.PlayMaker;

namespace I2.Loc
{
	[ActionCategory("I2 Localization")]
	[Tooltip("Set the localization CurrentLanguage ")]
	public class I2GetTranslation : FsmStateAction
	{

		public LanguageSource mSource;

		public I2LocPlayMaker_SelectionMode SelectionMode = I2LocPlayMaker_SelectionMode.Selection;

		[Tooltip("The Term to Translate")]
		public FsmString term;

        [Tooltip("The resulting translation")]
        public FsmString translation;


		public override void Reset()
		{
			term = null;
			translation = null;
		}

		public override void OnEnter()
		{
		    var termTranslation = mSource == null ? LocalizationManager.GetTranslation(term.Value) : mSource.GetTranslation(term.Value);
		    if (!translation.IsNone)
		        translation.Value = termTranslation;

			Finish();
		}
	}
}