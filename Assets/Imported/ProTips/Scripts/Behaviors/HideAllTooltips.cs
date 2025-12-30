using UnityEngine;
using UnityEngine.UI;

namespace ModelShark
{
    /// <summary>Purpose of this script is to hide all open tooltips with the push of a button. Put this script on a Button UI object.</summary>
    [RequireComponent(typeof(UnityEngine.UI.Button))]
    public class HideAllTooltips : MonoBehaviour
    {
        private void Start()
        {
            // Get the button on this object.
            UnityEngine.UI.Button button = gameObject.GetComponent<UnityEngine.UI.Button>();

            // Wireup the button's OnClick event.
            button.onClick.AddListener(()=>TooltipManager.Instance.HideAll());
        }
    }
}