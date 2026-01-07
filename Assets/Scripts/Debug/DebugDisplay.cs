using System.Text;
using Enemy;
using Entity;
using Services.Manager;
using UnityEngine;
using TMPro; // Required for TextMeshPro
using UnityEngine.InputSystem;

public class DebugDisplay : MonoBehaviour
{
    private InputAction m_ToggleDebugAction;

    [Header("General")]
    public GameObject debugCanvas;
    [Tooltip("Start with Debug visible?")]
    [SerializeField] private bool showDebug = false;
    
    [Header("Player")]
    public TMP_Text debugTextElement_Player; // Drag your UI Text (TMP) object here in the Inspector
    public PlayerController Player;
    
    [Header("Pools")]
    public TMP_Text debugTextElement_Pools;

    public void Awake()
    {
        _ToggleDebugCanvas(showDebug);
        m_ToggleDebugAction = InputSystem.actions.FindAction("Player/Debug");
    }

    public void ToggleDebugCanvas(bool show)
    {
        if (showDebug != show)
        {
            _ToggleDebugCanvas(show);
        }
    }
    void _ToggleDebugCanvas(bool show)
    {
        debugCanvas.SetActive(show);
        showDebug = show;
    }

    void Update()
    {
        if(m_ToggleDebugAction.WasPressedThisFrame())
            ToggleDebugCanvas(show: !showDebug);

        if(!showDebug)
            return;

        DrawPlayerDebug();

        DrawPoolDebug();
    }

    private void DrawPlayerDebug()
    {
        var sb = new StringBuilder();

        // Example: Display the current frame rate
        sb.Append("FPS: ").Append((1.0f / Time.deltaTime).ToString("F2"));

        if(Player)
        {
            /*
            sb.Append("\n<color=\"blue\">Vel X:</color> ").Append(Player.velocity.x.ToString("F2")).Append(" <color=\"blue\">Y:</color> ").Append(Player.velocity.y.ToString("F2"));
            sb.Append("\n<color=\"blue\">Overlapping:</color> ");
            if(Player.GetCurrentInteractable() != null)
                sb.Append((Player.GetCurrentInteractable() as MonoBehaviour)?.gameObject.name);
            else
                sb.Append("none");
            sb.Append("\n<color=\"blue\">Action State:</color> ").Append(Player.actionState);
            sb.Append("\n<color=\"blue\">Aim State:</color> ").Append(Player.aimState);
        
            sb.Append("\n<color=\"blue\">Primary Weapon:</color> ").Append(Player.primaryWeapon.weaponData.weaponName);
            sb.Append(" <color=\"blue\">Projectile:</color> ").Append(Player.primaryWeapon.weaponData.projectilePrefab.gameObject.name);
            sb.Append("\n<color=\"blue\">FireRate:</color> ").Append(Player.primaryWeapon.weaponData.fireRate);
            sb.Append(" <color=\"blue\">Range:</color> ").Append(Player.primaryWeapon.weaponData.range);
            sb.Append(" <color=\"blue\">Reload:</color> ").Append(Player.primaryWeapon.weaponData.reloadTime);

            sb.Append("\n<color=\"blue\">Secondary Weapon:</color> ").Append(Player.secondaryWeapon.weaponData.weaponName);
            sb.Append(" <color=\"blue\">Projectile:</color> ").Append(Player.secondaryWeapon.weaponData.projectilePrefab.gameObject.name);
            sb.Append("\n<color=\"blue\">FireRate:</color> ").Append(Player.secondaryWeapon.weaponData.fireRate);
            sb.Append(" <color=\"blue\">Range:</color> ").Append(Player.secondaryWeapon.weaponData.range);
            sb.Append(" <color=\"blue\">Reload:</color> ").Append(Player.secondaryWeapon.weaponData.reloadTime);
            */
        }
        debugTextElement_Player.text = sb.ToString();
    }

    private void DrawPoolDebug()
    {
        var sb = new StringBuilder();

        sb.Append("\n<color=\"blue\">Enemy Pool </color> ")
            .Append(EnemyPoolManager.Instance.GetPoolActive());

        sb.Append("\n<color=\"blue\">Projectile Pool </color> ")
            .Append(ProjectilePoolManager.Instance.GetPoolActive());

        sb.Append("\n<color=\"blue\">VFX Pool </color> ")
            .Append(VFXPoolManager.Instance.GetPoolActive());
        
        debugTextElement_Pools.text = sb.ToString();
    }
}