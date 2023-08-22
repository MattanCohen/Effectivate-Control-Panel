using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;

public class RollDial : Dial
{
    
    [SerializeField] GameObject background;
    [SerializeField] Slider reserveSlider;
    [SerializeField] AudioClip successClip;    
    [SerializeField] Transform clouds;
    [SerializeField] Transform arrow;
    [SerializeField] float movementSpeed = 25;
    float startPos = 0;
    float _movementThreshhold = 180;
    float _orangeThreshhold = 60;
    float cloudsMaxXShift = 90; 
    bool movementDirection = true;
    bool screenFlashing =  false;
    bool reserving =  false;

    // Start is called before the first frame update
    void Start()
    {
        // fix movement interval to accommodate with fixed update speed (60 FPS);
        movementSpeed /= 100f;

        // adjustments for good practice - move arrow to 90 degrees 
        Vector3 temp = arrow.localEulerAngles;
        temp.z = 90;
        arrow.localEulerAngles = temp;

        // save arrow start pos
        startPos = arrow.localEulerAngles.z;

        // general reset for the dial
        ResetDial();

        audioSource.volume = 0;

    }

    void ResetDial(){
        // gradualy reset the arrow
        StartCoroutine(ResetArrow());

        // set arrow to not malufunction
        screenFlashing = false;
        
        // hide flashing red background
        background.SetActive(false);

        // set reserving scale to 0
        reserveSlider.value = 0;

        // hide reserve scale
        reserveSlider.gameObject.SetActive(false);


    }

    IEnumerator ResetArrow(){
        float delta = movementSpeed * 5;
        Vector3 temp;
        while (Math.Abs(arrow.localEulerAngles.z - startPos) > delta){
            
            // make the arrow move towards startPos
            movementDirection = arrow.localEulerAngles.z > startPos;
            
            // move arrow by delta
            Move(delta);
            yield return new WaitForSeconds(0.05f);
        }

        // get arrow back in place
        temp = arrow.localEulerAngles;
        temp.z = startPos;
        arrow.localEulerAngles = temp;
    }

    void FixedUpdate(){
        
        if (Input.GetKeyDown("s"))
            ShiftUnshift();
        if (shift)
        {
            // move arrow 
            CheckToMoveArrow();

            // check if arrow is beyond orange threshhold
            CheckMalufunction();
        }
    }

    void CheckToMoveArrow(){

        bool shouldMove = arrow.localEulerAngles.z <= _movementThreshhold; 

        if (shouldMove)
        {
            Move(movementSpeed);
        }
        else
        {
            player.TakeHit();
            ChangeDirection();

            // nudge arrow to the max threshhold
            Vector3 temp = arrow.localEulerAngles;
            temp.z = movementDirection ?  _movementThreshhold : 0;
            arrow.localEulerAngles = temp;
        }

    }

    void Move(float speed){

        float angle = arrow.localEulerAngles.z;
        Vector3 cloudsTemp = clouds.localEulerAngles;

        if (movementDirection) // rotate right
        {
            angle -= speed;

            // move clouds to match with arrow
            cloudsTemp.z += (cloudsMaxXShift / _movementThreshhold) * (speed);
        }
        else // rotate left
        {
            angle += speed;

            // move clouds to match with arrow
            cloudsTemp.z -= (cloudsMaxXShift / _movementThreshhold) * (speed);
        }
        clouds.localEulerAngles = cloudsTemp;

        Vector3 temp = arrow.localEulerAngles;
        temp.z = angle;
        arrow.localEulerAngles = temp;
    }

    public void ChangeDirection(){
        movementDirection = !movementDirection;
    }

    void CheckMalufunction(){
        float angle = arrow.localEulerAngles.z;
        bool passedOrange = (angle < _orangeThreshhold || angle > _orangeThreshhold * 2);

       
        if (!screenFlashing)
        {
            screenFlashing = true;
            StartCoroutine(FlashBackground());
        }
        if (!passedOrange && !reserving)
        {
            reserving = true;
            StartCoroutine(ReserveAndReset());
        }
        if (passedOrange && reserving)
            reserving = false;

    }

    IEnumerator FlashBackground(){
        audioSource.Stop();
        audioSource.volume = 1;
        audioSource.Play();
        while (shift){
            background.SetActive(!background.activeSelf);
            yield return new WaitForSeconds(0.65f);
        }
        background.SetActive(false);
    }

    IEnumerator ReserveAndReset(){
        while (reserving){
            // make sure scale is shown
            reserveSlider.gameObject.SetActive(true);

            yield return new WaitForSeconds(0.1f);
            
            // fill another precent
            reserveSlider.value += _reserveSpeed;

            if (reserveSlider.value >= reserveSlider.maxValue){
                break;
            }
        }

        if (!reserving){
            //reserve failed:
            
            // reset to 0 reserve 
            reserveSlider.value = reserveSlider.minValue;
            
            // hide reserve slider
            reserveSlider.gameObject.SetActive(false);

        }

        else{
            // reserve successful:
            // reset the dial
            ResetDial();
            // stop shifting
            shift = false;
            reserving = false;
            audioSource.volume = 0;

            GameObject.FindObjectOfType<PlayerScript>().AddScore();
            // GameObject reserveSliderParent = reserveSlider.transform.parent.gameObject;
            // reserveSliderParent.GetComponent<ParticleSystem>().Play();
            // reserveSliderParent.GetComponent<AudioSource>().PlayOneShot(successClip);
        }

    }

}