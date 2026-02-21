using Core.Upgrades;
using TMPro;
using UnityEngine;

namespace Game.UI.Upgrades
{
    public sealed class UpgradeTooltip : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private TextMeshProUGUI description;
        
        // todo: pass the button position as well if we want to move the tooltip.
        public void Show(UpgradeDefinition def)
        {
            if (def == null)
                return;

            // todo: move to the button as well in case we want it displayed next to it instead of a fixed position.
            
            gameObject.SetActive(true);

            title.text = def.displayName;

            description.text = def.description;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
