using UnityEngine;
using TimerLibrary; 

public class TimerTest : MonoBehaviour
{
    private UnitySafeTimer _timer; 

    private void Awake()
    {
        _timer = new UnitySafeTimer();
        _timer.Start();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            Debug.Log($"[Timer] Seconds so far: {_timer.ElapsedSeconds:F3}");

        if (Input.GetKeyDown(KeyCode.R))
            Debug.Log($"[Timer] Consumed: {_timer.ConsumeSeconds():F3} sec; restarted.");
    }
}