using HutongGames.PlayMaker;

namespace I2.Loc
{

	[ActionCategory("I2 Localization")]
	[HutongGames.PlayMaker.TooltipAttribute("Set the localization CurrentLanguage ")]
	public class I2SetCurrentLanguage : FsmStateAction
	{
		public I2LocPlayMaker_SelectionMode SelectionMode = I2LocPlayMaker_SelectionMode.Selection;

		[Tooltip("The language to set as current")]
		public FsmString language;

		[ActionSection("Result")]
		[UIHint(UIHint.Variable)]
		public FsmBool success;

		[Tooltip("Event sent if the current language was set")]
		public FsmEvent successEvent;

		[Tooltip("Event sent if there was a problem, likely because the current localizaion doesn't have this language")]
		public FsmEvent failureEvent;

		LanguageSource mSource;

		public override void Reset()
		{
			language = null;
			success = null;
			successEvent = null;
			failureEvent = null;
		}

		public override void OnEnter()
		{
			var ok = false;
			if ( LocalizationManager.HasLanguage(language.Value))
			{
				ok = true;
				LocalizationManager.CurrentLanguage = language.Value;
			}

			if (!success.IsNone) 
			{
				success.Value = ok;
			}

			Fsm.Event (ok ? successEvent : failureEvent);

			Finish();
		}
	}
}