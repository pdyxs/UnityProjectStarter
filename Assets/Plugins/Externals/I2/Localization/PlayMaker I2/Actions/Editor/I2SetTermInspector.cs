using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using HutongGames.PlayMakerEditor;
using UnityEditor;
using UnityEngine;

namespace I2.Loc
{

    [CustomActionEditor(typeof(I2SetTerm))]
    public class I2SetTermInspector : CustomActionEditor
    {
        I2SetTerm mTarget;

		Localize mLocalize;

        public override bool OnGUI()
        {
            var changed = false;
            mTarget = (I2SetTerm)target;


			EditField ("GameObject");

            EditField("SelectionMode");

            if (mTarget.SelectionMode == I2LocPlayMaker_SelectionMode.FsmVariable)
            {
				
                EditField("PrimaryTerm");
                EditField("SecondaryTerm");
            }
            else
            {
                changed = EditTerms();
            }

            EditField("success");
            EditField("successEvent");
            EditField("failureEvent");


            return GUI.changed || changed;
        }


        bool EditTerms()
        {
            var changed = false;

			if (mTarget.PrimaryTerm == null) {
				return false;
			}

            // make sure our FsmString is not pointing to a variable.
            mTarget.PrimaryTerm.UseVariable = false;
            mTarget.SecondaryTerm.UseVariable = false;

			var go = mTarget.Fsm.GetOwnerDefaultTarget (mTarget.GameObject);

			if (go != null) {
				mLocalize = go.GetComponent<Localize> ();
			}

            var terms = (mLocalize!=null && mLocalize.Source!=null) ? mLocalize.Source.GetTermsList() : LocalizationManager.GetTermsList();
            terms.Sort(System.StringComparer.OrdinalIgnoreCase);
            terms.Add("");
            terms.Add("<inferred from text>");
            terms.Add("<none>");
            var aTerms = terms.ToArray();

            changed |= DoTermPopup("Primary Term", mTarget.PrimaryTerm, aTerms);
            changed |= DoTermPopup("Secondary Term", mTarget.SecondaryTerm, aTerms);

            return GUI.changed || changed;

        }

        bool DoTermPopup(string label, FsmString sTerm, string[] aTerms )
        {
            var index = (sTerm.Value == "-" || sTerm.Value == "" ? aTerms.Length - 1: 
                        (sTerm.Value == " " ? aTerms.Length - 2 : 
                        System.Array.IndexOf( aTerms, sTerm.Value)));

            var newIndex = EditorGUILayout.Popup(label, index, aTerms);

            if (index == newIndex) return false;

            sTerm.Value = (newIndex < 0 || newIndex == aTerms.Length - 1) ? string.Empty : aTerms[newIndex];
            if (newIndex == aTerms.Length - 1)
                sTerm.Value = "-";
            else if (newIndex < 0 || newIndex == aTerms.Length - 2)
                sTerm.Value = string.Empty;
            else
                sTerm.Value = aTerms[newIndex];

            return true;
        }
    }
}