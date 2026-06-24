using Core.Enum;
using UnityEngine;

namespace Core.Level
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "Levels/Level Config")]
    public class LevelConfig : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique ID used by save system — never change after shipping")]
        [SerializeField] private string levelId;    // e.g. "level_01" — stable forever
        [Tooltip("Unity scene name — must match Build Settings")]  
        [SerializeField] private string sceneName;  // e.g. "Level_01" — can change
        [Tooltip("Displayed in UI case necessary")]  
        [SerializeField] private string displayName;
        
        [Header("Transition")]
        [SerializeField] private TransitionStyle exitTransitionStyle = TransitionStyle.None;
        [SerializeField] private float transitionDuration = 0.4f;
        
        [Header("Presentation")]
        [SerializeField] private Sprite previewBackground;
        
        public string LevelId          => levelId;
        public string SceneName          => sceneName;
        public string DisplayName   => displayName;
        public TransitionStyle ExitTransitionStyle => exitTransitionStyle;
        public float TransitionDuration           => transitionDuration;
        public Sprite PreviewBackground => previewBackground;
    }
}
