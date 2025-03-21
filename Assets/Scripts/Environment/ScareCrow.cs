using UnityEngine;

public class Scarecrow : MonoBehaviour
{
    [SerializeField] private ParticleSystem destructionParticles;
    [SerializeField] private float destroyDelay = 1f;

    private SpriteRenderer sprite;
    private Collider2D col;
    private bool isDestroyed;

    private void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    public void PlayDestructionEffect()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        sprite.enabled = false;
        col.enabled = false;

        if (destructionParticles != null)
        {
            ParticleSystem instance = Instantiate(destructionParticles, transform.position, Quaternion.identity);
            instance.Play();
            Destroy(instance.gameObject, instance.main.duration);
        }

        Destroy(gameObject, destroyDelay);
    }
}