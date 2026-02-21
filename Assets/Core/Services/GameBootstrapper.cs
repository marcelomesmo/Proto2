using System.Collections;
using Core.Services.Manager;
using Core.Services.Save;
using Core.Upgrades;
using Core.Upgrades.Database;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.Services
{
    public class GameBootstrapper : MonoBehaviour
    {
        [Header("Service Prefabs")]
        [SerializeField] private GameController gameControllerPrefab;
        [SerializeField] private AudioManager audioManagerPrefab;
        [SerializeField] private VFXPoolManager vfxPoolPrefab;
        [SerializeField] private ProjectilePoolManager projectilePoolPrefab;
        [SerializeField] private EntityPoolManager entityPoolPrefab;
        [SerializeField] private MonoBehaviour saveManagerPrefab;
        
        [Header("Databases")]
        [Tooltip("Optional Upgrade Database. If missing, no upgrades will be available.")]
        [SerializeField] private UpgradeDatabase upgradeDatabase;
        
        [Header("Game Input Actions")]
        [SerializeField] private InputActionAsset inputActions;
        
        private static bool _bootstrapped;

        private void Awake()
        {
            if (_bootstrapped)
            {
                Destroy(gameObject);
                return;
            }

            _bootstrapped = true;
            DontDestroyOnLoad(gameObject);

            ServiceLocator.Initialize();
            SceneLoader.Initialize(this);
            InputMapController.Initialize(inputActions);

            StartCoroutine(BootstrapRoutine());
        }

        private IEnumerator BootstrapRoutine()
        {
            //
            //  Register
            //
            
            // Boilerplate
            RegisterService(gameControllerPrefab);
            RegisterService(audioManagerPrefab);
            RegisterService(vfxPoolPrefab);
            RegisterService(projectilePoolPrefab);
            RegisterService(entityPoolPrefab);

            RegisterSaveManager();
            
            //
            // Register (catalogs/databases)
            //
            // Upgrade Databases (always register interface, use Null object when missing)
            RegisterUpgradeDatabase();

            // Let all Awake() calls settle
            yield return null;
            
            ServiceLocator.Get<GameController>().Initialize();
            ServiceLocator.Get<ISaveManager>().Initialize();    // loud crash if missing
            /*var save = ServiceLocator.Get<ISaveManager>();
            if (save == null)
                Debug.LogError("[Bootstrap] No SaveManager registered.");
            else
                save.Initialize();*/
            
            // Transition to menu (async-ready)
            SceneLoader.LoadMenu();
        }

        private void RegisterService<T>(T prefab) where T : MonoBehaviour
        {
            if (!prefab)
            {
                Debug.LogError($"[Bootstrap] Missing prefab for {typeof(T)}");
                return;
            }

            T instance = Instantiate(prefab);
            DontDestroyOnLoad(instance.gameObject);
            ServiceLocator.Register(instance);
        }
        
        private void RegisterSaveManager()
        {
            if (!saveManagerPrefab)
                return;

            var instance = Instantiate(saveManagerPrefab);
            if (instance is not ISaveManager saveManager)
            {
                Debug.LogError(
                    "[Bootstrap] SaveManager must implement ISaveManager");

                Destroy(instance.gameObject);
                return;
            }
            
            DontDestroyOnLoad(instance.gameObject);
            ServiceLocator.Register(saveManager);
        }
        
        private void RegisterUpgradeDatabase()
        {
            if (!upgradeDatabase)
            {
                Debug.LogWarning("[Bootstrap] Missing UpgradeDatabase reference on GameBootstrapper.");
                
                // Always register an IUpgradeDatabase so callers can safely use Get<IUpgradeDatabase>().
                ServiceLocator.Register<IUpgradeDatabase>(new NullUpgradeDatabase());
                return;
            }

            // Ensure its index is ready even if OnEnable order is odd.
            upgradeDatabase.BuildIndex();

            // Register as a service (no singleton/static required).
            ServiceLocator.Register(upgradeDatabase);
        }
    }
}
