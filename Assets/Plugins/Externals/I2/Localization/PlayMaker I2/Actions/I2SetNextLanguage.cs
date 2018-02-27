using HutongGames.PlayMaker;

namespace I2.Loc
{

	[ActionCategory("I2 Localization")]
	[Tooltip("Set localization CurrentLanguage with the next available language")]
	public class I2SetNextLanguage : FsmStateAction
	{

		public enum I2LocPlayMaker_SelectionMode
		{
			FsmVariable,
			Selection
		}

		[ActionSection("Result")]
		[UIHint(UIHint.Variable)]
		public FsmBool success;

		[Tooltip("Event sent if the current language was set")]
		public FsmEvent successEvent;

		[Tooltip("Event sent if there was a problem, likely because there is only 1 language")]
		public FsmEvent failureEvent;

	

		public override void Reset()
		{
			success = null;
			successEvent = null;
			failureEvent = null;
		}

		public override void OnEnter()
		{
			var ok = false;
			var languages = LocalizationManager.GetAllLanguages();
			var idx = languages.IndexOf(LocalizationManager.CurrentLanguage);

			if (idx>=0 && languages.Count>1)
			{
				// select next language
				LocalizationManager.CurrentLanguage = languages[(idx + 1) % languages.Count];
				ok = true;
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