using Core.Services.Manager;
using UnityEngine;

namespace Core.Services
{
    public class GameController : MonoBehaviour
    {
        // TODO: TBD
        
        public void OnGameEnded()
        {
            ServiceLocator.Get<EntityPoolManager>().ReleaseAll();
            ServiceLocator.Get<ProjectilePoolManager>().ReleaseAll();
            ServiceLocator.Get<VFXPoolManager>().ReleaseAll();
            ServiceLocator.Get<AudioManager>().ReleaseAll();
        }
    }
}
