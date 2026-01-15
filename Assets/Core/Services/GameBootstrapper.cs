using System.Collections;
using Core.Services.Manager;
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

            // Let all Awake() calls settle
            yield return null;
            
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
    }
}
