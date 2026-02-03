using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.CharacterHUD
{
    public sealed class AttackHUDItemView : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private Image cooldownMask;

        public void SetIcon(Sprite sprite)
        {
            icon.sprite = sprite;
        }

        public void SetCooldown(float normalized, bool ready)
        {
            cooldownMask.fillAmount = 1f - normalized;
            icon.color = ready ? Color.white : Color.gray;
        }
    }
}
