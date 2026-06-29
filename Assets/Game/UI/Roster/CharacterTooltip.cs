using Game.Entity.Player;
using Game.Entity.Player.Progression;
using TMPro;
using UnityEngine;

namespace Game.UI.Roster
{
    public class CharacterTooltip : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private TextMeshProUGUI stats;
        [SerializeField] private TextMeshProUGUI attacks;

        public void Show(CharacterDefinition def, CharacterEvolutionData evolutionData)
        {
            gameObject.SetActive(true);

            title.text = evolutionData.stages[0].displayName;
            stats.text = BuildStats(def);
            attacks.text = BuildAttacks(def, evolutionData);

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

        private string BuildAttacks(CharacterDefinition def, CharacterEvolutionData evolutionData)
        {
            if (def.initialAttackLoadout == null)
                return "Attacks not found";

            var text = "";
            
            if (!evolutionData)
            {
                // Fallback: Build from base attacks
                foreach (var atk in def.initialAttackLoadout.Attacks)
                    text += $"- {atk.displayName}\n{atk.description}\n";

                return text;
            }

            var attackList = 
                evolutionData
                    .stages[evolutionData.MaxStage]
                    .attackLoadoutOverride
                    .Attacks;
            
            // Build from last stage attacks
            foreach (var atk in attackList)
                text += $"- {atk.displayName}\n{atk.description}\n";

            return text;
        }
    }
}
