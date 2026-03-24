using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem; // Add the new Input System namespace
using UnityEngine.SceneManagement;

public class TestUiAni : MonoBehaviour
{

    [SerializeField] private ScoreCounter score;
    [SerializeField] private SlidingPanel panel;
    [SerializeField] private PopUpDialog dialog;
    [SerializeField] private CardFlip card;

    [Header("FlyToUI Effect Test")]
    [SerializeField] private FlyToUIEffect flyEffect;
    [SerializeField] private GameObject flyPrefab;
    [SerializeField] private RectTransform flySpawnPoint;
    [SerializeField] private RectTransform flyTargetUI;
    [SerializeField] private Transform flyParent;


    [SerializeField] private ToastNotification toastNotification;
    [SerializeField] private ScreenTransition screenTransition;
    [SerializeField] private RewardCelebration celebrate;

    void Start()
    {
       
    }

    void Update()
    {
        // Make sure a keyboard is currently connected
        if (Keyboard.current == null) return;

        //when press 1 , score will increase by 10
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            score.AddScore(15);
        }
        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            score.AddScore(-20);
        }

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            panel.Show();
        }
        if (Keyboard.current.digit4Key.wasPressedThisFrame)
        {
            panel.Hide();
        }

        if (Keyboard.current.digit5Key.wasPressedThisFrame)
        {
            dialog.Show();
        }
        if (Keyboard.current.digit6Key.wasPressedThisFrame)
        {
            dialog.Hide();
        }

        if (Keyboard.current.digit7Key.wasPressedThisFrame)
        {
            card.Flip();
        }
        if (Keyboard.current.digit8Key.wasPressedThisFrame)
        {
            card.DramaticReveal();
        }

        // Test FlyToUIEffect with Digit 9
        if (Keyboard.current.digit9Key.wasPressedThisFrame)
        {
            if (flyEffect != null)
            {
                // Spawn 10 items. Optionally, hook up onEachArrival to increase the score by 1!
                flyEffect.SpawnFlyEffect(flyPrefab, flySpawnPoint, flyTargetUI, 10, flyParent,
                    onEachArrival: () => {

                        Debug.Log("Item arrived! Score increased by 1.");
                    }, onAllComplete: () => { score.AddScore(10); Debug.Log(" All items have arrived at the target!"); });
            }
        }

        // Test ToastNotification with Digit 0
        if (Keyboard.current.digit0Key.wasPressedThisFrame)
        {
            if (toastNotification != null)
            {
                toastNotification.ShowToast("This is a toast notification!");
            }
        }

        //if press Q , test ScreenTransition
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            screenTransition.FadeTransition(
                onMidpoint: () => {
                    SceneManager.LoadScene("Scene02");
                },
                onComplete: () => {
                    Debug.Log("Transition complete, new scene loaded.");
                });
        }

        // if press W , test RewardCelebration
        if (Keyboard.current.wKey.wasPressedThisFrame)
        {
            celebrate.PlayCelebration();
        }




    }


}
