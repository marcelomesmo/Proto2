using System.Collections;
using Core.Services.Manager;
using Core.Services.Save;
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
    }
}
