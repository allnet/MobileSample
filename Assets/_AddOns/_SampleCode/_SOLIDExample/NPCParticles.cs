using UnityEngine;

public class NPCParticles : MonoBehaviour
{
    [SerializeField] private ParticleSystem deathParticlePrefab;

    private void Start()
    {
        GetComponent<IHealth>().OnDied += HandleNPCDied;
    }

    private void HandleNPCDied()
    {
        var deathparticle = Instantiate(deathParticlePrefab, transform.position, transform.rotation);
        Destroy(deathparticle, 4f);
    }
}