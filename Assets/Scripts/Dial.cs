using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent (typeof(AudioSource))] 
public class Dial : MonoBehaviour
{

    public bool shift = false;

    public PlayerScript player;
    public AudioSource audioSource;
    public float _reserveSpeed = 15e-3f;

    
    public bool ShiftUnshift(){
        if (shift)
            return false;
        
        shift = !shift;
        return true;
    }
}
