using UnityEngine;

public class BattleSound : MonoBehaviour
{
    public AudioClip playerHurt;
    public AudioClip playerDie;
    public AudioClip reload;
    public AudioClip pistolShoot;
    public AudioClip enemyAppear;
    public AudioClip enemyAttack;

    AudioSource battleSound;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        battleSound = GetComponent<AudioSource>();
    }

    public void PlayerHurtSound()
    {
        battleSound.PlayOneShot(playerHurt);
    }

    public void PlayerDieSound()
    {
        battleSound.PlayOneShot(playerDie);
    }

    public void ReloadSound()
    {
        battleSound.PlayOneShot(reload);
    }

    public void PistolShootSound()
    {
        battleSound.PlayOneShot(pistolShoot);
    }

    public void EnemyAppearSound()
    {
        battleSound.PlayOneShot(enemyAppear);
    }

    public void EnemyAttackSound()
    {
        battleSound.PlayOneShot(enemyAttack);
    }
}
