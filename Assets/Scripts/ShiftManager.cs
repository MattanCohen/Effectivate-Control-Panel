using System.Collections;
using UnityEngine;
using System.Linq;

public class ShiftManager : MonoBehaviour {

    // all dials
    public Dial [] dials;

    // minimum time between each shift
    public float minTimeBetweenShifts;

    // time between each shift
    public float timeBetweenShifts;

    // time multiplier to scale down between each shift
    public float timeMultiplier;

    // shifts needed to decrease the time by multiplier (timeBetweenShifts = timeBetweenShifts * timeMultiplier)
    public int [] shiftsToMultiplyTime;
    int timeIndex = 0;



    // maximum number of concurrent shifts
    public int concurrentShiftsLimit = 1;

    // times to increase maximum number of concurrent shifts
    public int [] shiftsToIncreaseConcurrent;
    int concurrentIndex = 0;

    // should Shift practice round
    public bool shouldPractice = true;


    int shifts;

    public bool debug = true;
    

    void Start(){
        if (shouldPractice)
            StartCoroutine(PracticeShifting());
        

    }

    void FixedUpdate(){
        if (!shouldPractice){
            Debug.Log("started spawning");
            shouldPractice = true;
            StartCoroutine(ContantShift());
        }
            
    }

    // Shifts all dials in order
    IEnumerator PracticeShifting(){
        
        
        // wait if paused
        yield return new WaitForSeconds(0.1f);
        // wait for real seconds
        yield return new WaitForSecondsRealtime(1.2f);
        


        // shift each dial in order
        foreach (Dial dial in dials)
        {
            dial.shift = true;
            
            PrintDebug(string.Format("PRACTICE MODE - Shifted {0}.", dial.gameObject.name));

            // while the dial is shifted, wait
            while (dial.shift){
                yield return new WaitForSeconds(0.1f);
            }
        
            PrintDebug(string.Format("PRACTICE MODE - {0} is reserved.", dial.gameObject.name));
            
            // wait if paused
            yield return new WaitForSeconds(0.1f);
            // wait for real seconds
            yield return new WaitForSecondsRealtime(timeBetweenShifts);
            


        }

        shouldPractice = false;

    }

    IEnumerator ContantShift(){

        PrintDebug(string.Format("CONSTANT SHIFT - started constant shifting."));
        
        // shift another dial
        while (true)
        {
            // check if there are possible extra shifts. if not possible, wait until possible
            bool maxShiftsPossible = true;
            int concurrentCounter = -1;
            do{
                // count how much dials are shifted
                concurrentCounter = dials.Where(dial => dial.shift).Count();
                
                // check if too many are shifted at the same time 
                maxShiftsPossible = concurrentCounter >= concurrentShiftsLimit;

                PrintDebug(string.Format("CONSTANT SHIFT - {0} dials shifted out of {1} possible. {2}.", 
                            concurrentCounter, concurrentShiftsLimit, maxShiftsPossible ? "cant shift more dials" : "shifting another dial"));

                // wait if paused
                yield return new WaitForSeconds(0.1f);
                // wait for real seconds
                yield return new WaitForSecondsRealtime(0.5f);
            } while (maxShiftsPossible);

            // wait if paused
            yield return new WaitForSeconds(0.1f);
            // wait for real seconds
            yield return new WaitForSecondsRealtime(timeBetweenShifts);

            // extra shifts are possible:
            // pick a random unshifted dial
            Dial dial = dials[Random.Range(0, dials.Length)];
            while(dial.shift){
                dial = dials[Random.Range(0, dials.Length)];
            }

            // shift the dial
            dial.shift = true;
        
            // count the shift
            shifts++;

            PrintDebug(string.Format("CONSTANT SHIFT - shifted {0}. overall shifted {1} times.", dial.gameObject.name, shifts));

            // wait if paused
            yield return new WaitForSeconds(0.1f);
            // wait for real seconds
            yield return new WaitForSecondsRealtime(timeBetweenShifts);
            
        
            // shouldMulTime = if the number of shifts in shiftsToMultiplyTime[timeIndex] == shifts
            //                 and time isnt min time
            bool shouldMulTime = timeIndex < shiftsToMultiplyTime.Length &&     
                                shiftsToMultiplyTime[timeIndex] == shifts &&
                                timeBetweenShifts > minTimeBetweenShifts; 

            PrintDebug(string.Format("CONSTANT SHIFT - {0}.", 
                                        (shouldMulTime 
                                            ? string.Format("multiplying time between shifts from {0} to {1}.", timeBetweenShifts, (timeBetweenShifts * timeMultiplier))
                                            : timeIndex < shiftsToMultiplyTime.Length 
                                                ? string.Format("shouldnt multiply time until shift #{0}. current time: {1}.", shiftsToMultiplyTime[timeIndex], timeBetweenShifts)
                                                : string.Format("shouldnt multiply time. last multiplication was at shift #{0}. current time: {1}.", shiftsToMultiplyTime[timeIndex - 1], timeBetweenShifts)
                                        )));

            if (shouldMulTime)
            {
                // multiply timeBetweenShifts by shouldMulTime
                timeBetweenShifts *= timeMultiplier;
                // to afford a grain of error
                if (timeBetweenShifts < minTimeBetweenShifts){
                    timeBetweenShifts *= 0.9f;

                }   

                // append time index
                timeIndex++;
            }


 
            // shouldIncCon = if the number of shifts in shiftsToIncreaseConcurrent[concurrentIndex] == shifts
            //                 and concurrent counter isnt at its limit, and the next limit isnt bigger than the possible dials
            bool shouldIncCon = concurrentIndex < shiftsToIncreaseConcurrent.Length &&  
                                shiftsToIncreaseConcurrent[concurrentIndex] == shifts &&
                                concurrentCounter < concurrentShiftsLimit &&
                                concurrentShiftsLimit + 1 <= dials.Length; 
            
            PrintDebug(string.Format("CONSTANT SHIFT - {0}.", 
                                    (shouldIncCon 
                                        ? string.Format("increasing concurrentLimit from {0} to {1}.", concurrentShiftsLimit, (concurrentShiftsLimit + 1))
                                        : concurrentIndex < shiftsToIncreaseConcurrent.Length 
                                            ? string.Format("shouldnt increase concurrent limit until shift #{0}. current limit: {1}.", shiftsToIncreaseConcurrent[concurrentIndex], concurrentShiftsLimit)
                                            : string.Format("shouldnt increase concurrent limit. last increase was at shift #{0}. current limit: {1}.", shiftsToIncreaseConcurrent[concurrentIndex - 1], concurrentShiftsLimit)
                                    )));

            if (shouldIncCon)
            {
                // increase concurrentShiftsLimit by 1
                concurrentShiftsLimit ++;

                //append concurrent index
                concurrentIndex++;
            }

 
        }

    }


    void PrintDebug(string str){ if (debug) Debug.Log(str);}

}