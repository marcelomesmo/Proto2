using Game.Entity.Player;
using Game.Entity.Player.Progression;
using TMPro;
using UnityEngine;

namespace Game.UI.Roster
{
    public class InitialCharacterTooltip : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private TextMeshProUGUI description;

        public void Show(CharacterEvolutionData def)
        {
            gameObject.SetActive(true);

            title.text = def.stages[0].displayName;
            description.text = def.stages[0].displayDescription;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
