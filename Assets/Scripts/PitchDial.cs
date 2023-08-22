using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PitchDial : Dial
{
    
    [SerializeField] GameObject background;
    [SerializeField] Slider reserveSlider;
    [SerializeField] AudioClip successClip;    
    [SerializeField] Transform clouds;
    [SerializeField] Transform arrow;
    [SerializeField] float movementSpeed = 80;
    float startPos = 0;
    float _movementThreshhold = 333;
    float _orangeThreshhold = 110;
    float cloudsMaxXShift = 90; 
    bool movementDirection = true;
    bool screenFlashing =  false;
    bool reserving =  false;

    // Start is called before the first frame update
    void Start()
    {
        // fix movement interval to accommodate with fixed update speed (60 FPS);
        movementSpeed /= 100f;

        // adjustments for good practice - move arrow back to 0 
        Vector3 temp = arrow.localPosition;
        temp.y = 0;
        arrow.localPosition = temp;

        // save arrow start pos
        startPos = arrow.localPosition.y;

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
        while (Math.Abs(arrow.localPosition.y - startPos) > delta){
            
            // make the arrow move towards startPos
            movementDirection = arrow.localPosition.y < startPos;
            
            // move arrow by delta
            Move(delta);
            yield return new WaitForSeconds(0.05f);
        }

        // get arrow back in place
        temp = arrow.localPosition;
        temp.y = startPos;
        arrow.localPosition = temp;
    }

    void FixedUpdate(){
        // LATER TODO : GET RID
        if (Input.GetKeyDown("a"))
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

        bool shouldMove = Math.Abs(arrow.localPosition.y) <= _movementThreshhold; 

        if (shouldMove)
        {
            Move(movementSpeed);
        }
        else
        {
            player.TakeHit();

            ChangeDirection();

            // nudge arrow to the nearest max threshhold
            Vector3 temp = arrow.localPosition;
            temp.y = Math.Sign(temp.y) * _movementThreshhold;
            arrow.localPosition = temp;
        }

    }

    void Move(float speed){

        float pos = arrow.localPosition.y;
        Vector3 cloudsTemp = clouds.localPosition;

        if (movementDirection) // move up
        {
            pos += speed;

            // move clouds to match with arrow
            cloudsTemp.y -= (cloudsMaxXShift / _movementThreshhold) * (speed);
        }
        else // rotate left
        {
            pos -= speed;

            // move clouds to match with arrow
            cloudsTemp.y += (cloudsMaxXShift / _movementThreshhold) * (speed);
        }
        clouds.localPosition = cloudsTemp;

        Vector3 temp = arrow.localPosition;
        temp.y = pos;
        arrow.localPosition = temp;
    }

    public void ChangeDirection(){
        movementDirection = !movementDirection;
    }

    void CheckMalufunction(){
        float angle = arrow.localPosition.y;
        bool passedOrange = Math.Abs(arrow.localPosition.y) > _orangeThreshhold;

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