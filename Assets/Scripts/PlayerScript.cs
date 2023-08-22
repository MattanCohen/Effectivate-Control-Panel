using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

[RequireComponent (typeof(AudioSource))] 
public class PlayerScript : MonoBehaviour {
    [SerializeField] int health = 2;

    [SerializeField] GameObject [] hearts;
    
    [SerializeField] GameObject scoreBar;
    
    AudioSource audioSource;
    bool dead = false;

    TMP_Text scoreCounter;
    ParticleSystem scoreParticles; 



    private void Start() {
        audioSource = GetComponent<AudioSource>();
        
        scoreCounter = scoreBar.GetComponentInChildren<TMP_Text>();
        scoreCounter.text = "0";
        scoreParticles = scoreBar.GetComponentInChildren<ParticleSystem>();
    }

    public void TakeHit(){
        if (health < 1){
            Die();
            return;
        }
            health --;
            // hide hearth #health in row 
            hearts[health].transform.GetChild(0).gameObject.SetActive(false);
            hearts[health].GetComponent<ParticleSystem>().Play();
            audioSource.Play();
    }

    void Die(){
        // TODO
    }

    public void AddScore(){
        scoreParticles.Play();
        scoreParticles.GetComponent<AudioSource>().Stop();
        scoreParticles.GetComponent<AudioSource>().Play();
        scoreCounter.text = (int.Parse(scoreCounter.text) + 1).ToString();
    }

}

