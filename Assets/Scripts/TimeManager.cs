using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    [SerializeField] GameObject informationLayout;
    [SerializeField] Dial [] dials;
    List <Dial> activatedDials;

    private void Start() {
        Pause();
        informationLayout.SetActive(true);

        activatedDials = new List<Dial>();
    }

    public void Pause(){
        Time.timeScale = 0;
        foreach (Dial dial in dials)
        {
            if (dial.shift) {
                activatedDials.Add(dial);
                dial.GetComponent<AudioSource>().volume = 0;
            }
        }
        
    }

    public void Resume(){
        Time.timeScale = 1;
        foreach (Dial dial in activatedDials.ToArray())
        {
            dial.GetComponent<AudioSource>().volume = 1;
            activatedDials.Remove(dial);
        }
    }


}
