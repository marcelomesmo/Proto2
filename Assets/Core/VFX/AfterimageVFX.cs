using UnityEngine;

namespace Core.VFX
{
    // Spawn Sprites in the world with a fade-out effect.
    public class AfterimageVFX  : PooledTimedVFX
    {
        [Tooltip("This should be an empty SpriteRenderer in the VFX itself.")]
        [SerializeField] private SpriteRenderer renderer;
        [Tooltip("Tint the original Sprite.")]
        [SerializeField] private Color tint = Color.white;

        public void Initialize(Sprite sprite, Vector3 pos, bool flipX, bool flipY, 
            SpriteRenderer sourceRenderer)
        {
            transform.position = pos;
        
            renderer.sprite = sprite;
            renderer.flipX = flipX;
            renderer.flipY = flipY;
        
            renderer.sortingLayerID = sourceRenderer.sortingLayerID;
            renderer.sortingOrder = sourceRenderer.sortingOrder - 1;
        
            // Set full tint initially (alpha handled in Update)
            renderer.color = new Color(tint.r, tint.g, tint.b, tint.a);
        }

        protected override void OnUpdate()
        {
            float alpha = Mathf.Clamp01(Timer / lifetime);
            
            renderer.color = new Color(
                tint.r,
                tint.g,
                tint.b,
                tint.a * alpha
            );
        }
    }
}