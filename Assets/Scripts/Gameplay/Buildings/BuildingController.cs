using Enemy.Spawner;
using EventChannels;
using Gameplay.Loot;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BuildingController : MonoBehaviour, IInteractable
{
    private static readonly int IsExtracting = Animator.StringToHash("IsExtracting");

    [Header("UI Elements")]
    [Tooltip("Button to display on top of the building")]
    [SerializeField] private GameObject overlayButton;
    [Tooltip("Building progress bar to display on top of the building")]
    [SerializeField] private GameObject buildingFillBar;
    [Tooltip("VFX to display while building")]
    [SerializeField] private GameObject buildingVFXPrefab;
 
    //public AudioClip interactAudio;
    //public AudioClip buildingAudio;
    //public AudioClip extractingAudio;
    // public AudioSource audioSource;

    internal Animator Animator;

    private BuildingState _buildingState;
    private PlayerInteractState _playerInteractState;

    [Header("Spawn Settings")]
    [SerializeField] private EnemySpawner spawner;
    
    [Header("Building Settings")]
    public float timeToBuild = 3f;
    private float _buildingTimer = 0f;

    public float timeToExtract = 20f;
    private float _extractingTimer = 0f;

    [Header("Collectable Spawn")]
    public BaseCollectable resourceToSpawn;     // TODO: Replace this with loot table.
    public int numberOfResourcesToSpawn = 10;
    private int _spawnedResources = 0;
    private float _spawnCooldown = 0.25f;  // Cooldown between each resource spawn
    private float _spawnTimer = 0f;
    //public float initialDelayToSpawn = 0.15f;
    
    //public float minDistance = 1f;
    //public float maxDistance = 5f;
    //public float minHeight = 1.5f;
    //public float maxHeight = 3.5f;
    //public float minDuration = 0.6f;
    //public float maxDuration = 1.2f;
    
    [Header("Broadcast")]
    [SerializeField] private FeedbackEventChannelSO cameraShakeEvent;

    [Header("Visual Elements")] 
    public Light2D light2DIdleLeft; 
    public Light2D light2DIdleRight;

    void Awake()
    {
        Animator = GetComponentInChildren<Animator>();
        //audioSource = GetComponent<AudioSource>();

        _buildingState = BuildingState.Idle;
        _playerInteractState = PlayerInteractState.None;

        //_spawnCooldown = (timeToExtract - initialDelayToSpawn) / numberOfResourcesToSpawn;
    }

    // Update is called once per frame
    void Update()
    {
        if(_buildingState == BuildingState.Building)
        {
            if (_playerInteractState == PlayerInteractState.Interacting
                && _buildingTimer < timeToBuild)
                _buildingTimer += Time.deltaTime;
            else if (_buildingTimer > 0)
                _buildingTimer -= Time.deltaTime;

            if (_playerInteractState != PlayerInteractState.Interacting
                && _buildingTimer <= 0 && buildingFillBar.activeSelf)
            {
                _buildingTimer = 0f;
                buildingFillBar.SetActive(false);
                _buildingState = BuildingState.Idle;
            }

            buildingFillBar.GetComponent<ProgressBar>().SetProgress(_buildingTimer/timeToBuild);

            if (_buildingTimer >= timeToBuild)
            {
                StartExtracting();
            }
        }

        if (_buildingState == BuildingState.Extracting)
        {
            if (_extractingTimer < timeToExtract)
                _extractingTimer += Time.deltaTime;
            else if (_extractingTimer >= timeToExtract)
                FinishExtracting();
            
            // update enemy spawn for node
            if(!spawner.IsSpawning)
                spawner.StartSpawning();
        }

        if (_buildingState == BuildingState.Finished)
        {
            // update resource drop for node
            if (_spawnedResources < numberOfResourcesToSpawn)
            {
                _spawnTimer += Time.deltaTime;
                // spawn resources?
                if (_spawnTimer >= _spawnCooldown)
                {
                    _spawnTimer = 0f;
                    _spawnedResources++;
                    SpawnCollectable();
                }
            }
        }
    }

    private void SpawnCollectable()
    {
        //Debug.Log("Spawning resource");
        
        //float distance = Random.Range(minDistance, maxDistance);
        //float direction = Random.value < 0.5f ? -1f : 1f;
        //float height = Random.Range(minHeight, maxHeight);
        //float duration = Random.Range(minDuration, maxDuration);

        Vector2 spawnPoint = transform.position;
        //Vector2 end = start + new Vector2(distance * direction, 0f);
        float spacing = 0.3f;
        Vector2 offset = Vector2.right * (spacing * _spawnedResources);

        BaseCollectable loot = Instantiate(resourceToSpawn, spawnPoint + offset, Quaternion.identity);
        //spawned.Launch(start, end, height, duration);
        loot.Bouncer.Launch(new Vector2(Random.value < 0.5f ? -1 : 1, 0));
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 'other' refers to the collider that entered this trigger.
        //Debug.Log("Object entered trigger: " + other.gameObject.name);

        // You can check for specific objects using tags or components
        if (other.CompareTag("Player"))
        {
            //Debug.Log("Player entered the trigger!");
            // Perform actions specific to the player entering
            if(_buildingState == BuildingState.Idle)
                overlayButton.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // 'other' refers to the collider that entered this trigger.
        //Debug.Log("Object left trigger: " + other.gameObject.name);

        // You can check for specific objects using tags or components
        if (other.CompareTag("Player"))
        {
            //Debug.Log("Player left the trigger!");
            // Perform actions specific to the player entering
            _playerInteractState = PlayerInteractState.None;
            overlayButton.SetActive(false);
        }
    }

    public void Interact()
    {
        if (_buildingState == BuildingState.Extracting || _buildingState == BuildingState.Finished) 
            return;
        
        //Debug.Log("Start Interacting!");

        overlayButton.SetActive(false);
        buildingFillBar.SetActive(true);
        if(buildingVFXPrefab) buildingVFXPrefab.SetActive(true);
        _playerInteractState = PlayerInteractState.Interacting;
        _buildingState = BuildingState.Building;
        Animator.SetBool("IsBuilding", true);
        //Animator.speed = 1;
        light2DIdleLeft.intensity = 0f;
        light2DIdleRight.intensity = 0f;
    }
    
    public void StopInteract()
    {
        if (_buildingState == BuildingState.Extracting || _buildingState == BuildingState.Finished) 
            return;
        
        //Debug.Log("Stopped Interacting!");

        _playerInteractState = PlayerInteractState.None;
        //Animator.speed = 0;
        if(buildingVFXPrefab) buildingVFXPrefab.SetActive(false);
    }

    public void StartExtracting()
    {
        _buildingState = BuildingState.Extracting;
        buildingFillBar.SetActive(false);
        if(buildingVFXPrefab) buildingVFXPrefab.SetActive(false);
        _playerInteractState = PlayerInteractState.None;
        Animator.SetBool(IsExtracting, true);
        Animator.speed = 1;
        
        cameraShakeEvent.OnEventRaised(new FeedbackRequest
        {
            Scope = FeedbackScope.Global,
            Type = FeedbackType.ExtractionStart,
            Intensity = 0.2f
        });

        //Debug.Log("Building Started Extracting");
    }

    public void FinishExtracting()
    {
        _buildingState = BuildingState.Finished;
        Animator.SetBool(IsExtracting, false);
        //Debug.Log("Building Finished Extracting");
    }

    private enum BuildingState
    {
        Idle,
        Building,
        Extracting,
        Finished
    }

    private enum PlayerInteractState
    {
        None,
        Interacting
    }
    
#if UNITY_EDITOR
    private BoxCollider2D boxCollider;
    void OnDrawGizmos()
    {
        if (boxCollider == null)
            boxCollider = GetComponent<BoxCollider2D>();

        // Set the gizmo color
        Gizmos.color = new Color(0f, 1f, 0f, 0.1f); // Green with 10% transparency

        // Apply the object's transform to the gizmo drawing space
        // This ensures the gizmo rotates and scales with the object
        Gizmos.matrix = transform.localToWorldMatrix;

        // Draw a solid cube using the collider's size and center
        // Note: Collider2D.size and center are in local space
        Gizmos.DrawCube(boxCollider.offset, boxCollider.size);
    }
#endif
}
