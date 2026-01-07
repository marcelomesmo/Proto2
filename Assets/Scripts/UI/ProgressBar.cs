using UnityEngine;

public class ProgressBar : MonoBehaviour
{
    /*
     *  DEPRECATED
     */

    // TODO: Simple shitty progress bar. Deprecate this later.
    
    // Still used in reload bar and building bar, should replace those with new ones.
    
    [SerializeField] private Transform fillTransform;
    [SerializeField] private SpriteRenderer fillRenderer;
    //[SerializeField] private float maxWidth = 1f;

    private Vector3 _originalScale;

    private void Awake()
    {
        // Store the full (100%) size
        _originalScale = fillTransform.localScale;

        // Calculate width in world units (optional)
        //maxWidth = fillRenderer.bounds.size.x;
    }

    public void SetProgress(float value)
    {
        float clamped = Mathf.Clamp01(value);
        fillTransform.localScale = new Vector3(_originalScale.x * clamped, _originalScale.y, _originalScale.z);
        //fillRenderer.color = Color.Lerp(Color.red, Color.green, value);
    }
}