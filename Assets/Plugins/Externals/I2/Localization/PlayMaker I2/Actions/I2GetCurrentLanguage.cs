using HutongGames.PlayMaker;

namespace I2.Loc
{
    [ActionCategory("I2 Localization")]
    [Tooltip("Get the localization current Language")]
    public class I2GetCurrentLanguage : FsmStateAction
    {
        [Tooltip("The LocalizationManager.CurrentLanguage")]
		[UIHint(UIHint.Variable)]
        public FsmString CurrentLanguage;

        [Tooltip("The LocalizationManager.CurrentLanguageCode")]
		[UIHint(UIHint.Variable)]
        public FsmString CurrentLanguageCode;

        public override void Reset()
        {
            CurrentLanguage = null;
			CurrentLanguageCode = null;
        }

        public override void OnEnter()
        {
            if (!CurrentLanguage.IsNone)
                CurrentLanguage.Value = LocalizationManager.CurrentLanguage;

            if (!CurrentLanguageCode.IsNone)
                CurrentLanguageCode.Value = LocalizationManager.CurrentLanguageCode;

            Finish();
        }
    }
}