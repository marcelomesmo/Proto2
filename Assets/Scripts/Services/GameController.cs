using Services.Manager;
using UnityEngine;

namespace Services
{
    public class GameController : MonoBehaviour
    {
        // TODO: TBD
        
        public void OnGameEnded()
        {
            ServiceLocator.Get<EnemyPoolManager>().ReleaseAll();
            ServiceLocator.Get<ProjectilePoolManager>().ReleaseAll();
            ServiceLocator.Get<VFXPoolManager>().ReleaseAll();
            ServiceLocator.Get<AudioManager>().ReleaseAll();
        }
    }
}
