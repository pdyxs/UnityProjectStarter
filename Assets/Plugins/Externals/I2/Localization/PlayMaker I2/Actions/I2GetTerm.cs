using HutongGames.PlayMaker;

namespace I2.Loc
{
	[ActionCategory("I2 Localization")]
	[Tooltip("Get the localization Terms")]
	public class I2GetTerm : FsmStateAction
	{

		[RequiredField]
		[CheckForComponent(typeof(Localize))]
		[Tooltip("Game Object with Localize Component.")]
		public FsmOwnerDefault gameObject;

		[Tooltip("The Primary Term (e.g. text, sprite name, etc)")]
		[UIHint(UIHint.Variable)]
        public FsmString PrimaryTerm;

		[Tooltip("The secondary term (e.g. font)")]
		[UIHint(UIHint.Variable)]
        public FsmString SecondaryTerm;

		Localize localize;

		public override void Reset()
		{
			gameObject = null;
			PrimaryTerm = null;
			SecondaryTerm = null;
		}

		public override void OnEnter()
		{
			
			var go = Fsm.GetOwnerDefaultTarget (gameObject);

			if (go != null) {
				localize = go.GetComponent<Localize> ();
			}
				
			if (localize != null)
			{
				if (!PrimaryTerm.IsNone)
					PrimaryTerm.Value = localize.Term;

				if (!SecondaryTerm.IsNone)
					SecondaryTerm.Value = localize.SecondaryTerm;
			}

			Finish();
		}
	}
}