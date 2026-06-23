using Core.Enum;
using UnityEngine;

namespace Core.Level
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "Levels/Level Config")]
    public class LevelConfig : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string levelId;

        [Header("Transition")]
        [SerializeField] private TransitionStyle exitTransitionStyle = TransitionStyle.None;
        [SerializeField] private float transitionDuration = 0.4f;

        public string LevelId          => levelId;
        public TransitionStyle ExitTransitionStyle => exitTransitionStyle;
        public float TransitionDuration           => transitionDuration;
    }
}
