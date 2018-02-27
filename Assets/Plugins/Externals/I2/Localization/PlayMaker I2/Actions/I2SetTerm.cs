using HutongGames.PlayMaker;

namespace I2.Loc
{
	public enum I2LocPlayMaker_SelectionMode
	{
		FsmVariable,
		Selection
	}

	[ActionCategory("I2 Localization")]
    [HutongGames.PlayMaker.TooltipAttribute("Set the localization Terms")]
	public class I2SetTerm : FsmStateAction
	{		
		public I2LocPlayMaker_SelectionMode SelectionMode = I2LocPlayMaker_SelectionMode.Selection;

		[RequiredField]
		[CheckForComponent(typeof(Localize))]
		[Tooltip("Game Object with Localize Component.")]
		public FsmOwnerDefault GameObject;

        [Tooltip("The Primary Term (e.g. text, sprite name, etc)")]
		public FsmString PrimaryTerm;

		[Tooltip("The secondary term (e.g. font)")]
		public FsmString SecondaryTerm;

		[ActionSection("Result")]
		[UIHint(UIHint.Variable)]
		public FsmBool success;

		[Tooltip("Event sent if the current language was set")]
		public FsmEvent successEvent;

		[Tooltip("Event sent if there was a problem, likely because the current localizaion doesn't have this language")]
		public FsmEvent failureEvent;

		Localize mLocalize;
		public override void Reset()
		{
			GameObject = null;
			PrimaryTerm = null;
			SecondaryTerm = null;
			success = null;
			successEvent = null;
			failureEvent = null;
		}

		public override void OnEnter()
		{
			var ok = false;

			var go = Fsm.GetOwnerDefaultTarget (GameObject);

			if (go != null) {
				mLocalize = go.GetComponent<Localize> ();
			}
			
			if (mLocalize!=null)
            {
                if (string.IsNullOrEmpty(SecondaryTerm.Value))
					mLocalize.SetTerm(PrimaryTerm.Value);
                else
					mLocalize.SetTerm(PrimaryTerm.Value, SecondaryTerm.Value);

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