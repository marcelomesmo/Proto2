using Game.Entity.Player;
using TMPro;
using UnityEngine;

namespace Game.UI.Roster
{
    public class CharacterTooltip : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private TextMeshProUGUI stats;
        [SerializeField] private TextMeshProUGUI attacks;

        public void Show(CharacterDefinition def)
        {
            gameObject.SetActive(true);

            title.text = def.name;
            stats.text = BuildStats(def);
            attacks.text = BuildAttacks(def);

            // in the future we can pass an anchor if we want to change the ui position
            //transform.position = anchor.position;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private string BuildStats(CharacterDefinition def)
        {
            var s = def.baseStats;
            return ""; //$"HP: {s.maxHealth}\n"; //+
            //$"Attack: {s.attackPower}\n" +
            //$"Defense: {s.defense}";
        }

        private string BuildAttacks(CharacterDefinition def)
        {
            if (def.initialAttackLoadout == null)
                return "No attacks";

            var text = "";
            foreach (var atk in def.initialAttackLoadout.Attacks)
                text += $"- {atk.displayName}\n{atk.description}\n";

            return text;
        }
    }
}
