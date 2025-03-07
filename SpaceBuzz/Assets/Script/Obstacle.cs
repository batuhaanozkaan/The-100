using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MusicFilesNM;
using TMPro;

public class Obstacle : MonoBehaviour
{
    private GameObject sound;
    private MusicFiles deathSound; // �l�m ses efekti
    [SerializeField] private int deathSoundIndex;

    [SerializeField] GameObject UI;

    [SerializeField] private CharacterController _controller;

    [SerializeField] private LevelDistance _distance; // Ekosistem i�in eklendi
    [SerializeField] private Animator _anim; // Ekosistem i�in eklendi
    [SerializeField] private GameObject _UIDistance; // Ekosistem i�in eklendi

    [SerializeField] private GameObject _UIScore;
    [SerializeField] private GameObject _UIOxygen;
    [SerializeField] private GameObject _UILeftButton;
    [SerializeField] private GameObject _UIRightButton;
    [SerializeField] private GameObject _UIPauseButton;
    [SerializeField] private GameObject _UIHighscore;
    [SerializeField] private GameObject _UIres;

    [SerializeField] private SkinnedMeshRenderer _skinned;

    [SerializeField] private TextMeshProUGUI highScoreText;

    [SerializeField] private CapsuleCollider capCol;
    private bool freezeDistance=false;
    private int freezedHighScore;
    private bool callMeOnce=true;
    private bool callMeOnce2 = true;

    public PlayfabManager _playfabmanager;
    public AdsManager ads;

    public AudioSource _audio;

    

    

    private void Awake()
    {
        sound = GameObject.Find("AudioManager");
        deathSound = sound.GetComponent(typeof(MusicFiles)) as MusicFiles;

        //RESET HIGH SCORE
        //PlayerPrefs.DeleteKey("HighScore");
        
    }
    private void LateUpdate()
    {
        OxygenZero();
        
        if (Input.GetKeyDown(KeyCode.H))
        {
            Oxygen.oxygenCylinder = 1;
        }

        
        if (callMeOnce)
        {
            if(Oxygen.oxygenCylinder <= 0)
            {
                freezeDistance = true;
                if (freezeDistance)
                {
                    freezedHighScore = _distance.Idistance;
                    freezeDistance = false;
                    
                }
                callMeOnce = false;
            }
        }

        if (callMeOnce2)
        {
            highScoreText.text = PlayerPrefs.GetInt("HighScore").ToString();
            callMeOnce2 = false;
        }
        
        Invoke("calculateHighScore", 2f);
    }

    public void Resurrection()
    {
        UI.SetActive(false);
        //Time.timeScale = 1f;
        Oxygen.oxygenCylinder = 500f;
        StartCoroutine(skinmeshTrue());
        //_skinned.enabled = true;

        // Ekosistemi devam ettirmek i�in kullan�lanlar
        _controller.enabled = true;
        _distance.enabled = true;
        _anim.SetBool("run", true);
        _anim.SetBool("flip", false);
        _UIDistance.SetActive(true);
        _UIScore.SetActive(true);
        _UIOxygen.SetActive(true);
        _UILeftButton.SetActive(true);
        _UIRightButton.SetActive(true);
        _UIPauseButton.SetActive(true);
        _UIHighscore.SetActive(true);

        _audio.UnPause();

        callMeOnce = true;
        isDied = true;
        _UIres.SetActive(false);

        _distance.Idistance = freezedHighScore;

        StartCoroutine(WaitRes());

    }

    // ADS
    void OnRewardedAdSuccess()
    {
        Resurrection();
    }

    public void ResurrectionAdButton()
    {
        ads.PlayRewardedAd(OnRewardedAdSuccess);
        
    }

    public void OxygenZero()
    {
        if (Oxygen.oxygenCylinder <= 0)
        {
            UI.SetActive(true);
            Time.timeScale = 1f;

            

            // Ekosistemi devam ettirmek i�in kullan�lanlar
            _controller.enabled = false;
            _distance.enabled = false;
            _anim.SetBool("run", false);
            _anim.SetBool("flip", true);
            _UIDistance.SetActive(false);
            _UIScore.SetActive(false);
            _UIOxygen.SetActive(false);
            _UILeftButton.SetActive(false);
            _UIRightButton.SetActive(false);
            _UIPauseButton.SetActive(false);
            _UIHighscore.SetActive(false);

            MobileControl.leftMobileInput = false;
            MobileControl.rightMobileInput = false;
            StartCoroutine(skinmeshFalse());
            _audio.Pause();

            if (PlayerPrefs.GetInt("HighScore") <= freezedHighScore || PlayerPrefs.GetInt("HighScore") == 0)
            {
                
                PlayerPrefs.SetInt("HighScore", freezedHighScore);
                _playfabmanager.SendLeaderboard(PlayerPrefs.GetInt("HighScore"));
            }

            
        }
    }
    
    //Died Efekti
    private GameObject diedClonePickupEffect;
    public GameObject diedPickupEffect;
    private bool isDied = true;

    public void Pickup() // Pacticle Efect
    {
        diedClonePickupEffect = Instantiate(diedPickupEffect, transform.position, transform.rotation);
        Invoke("deleteParticle", 0.80f); // Clone Particle Destroy
    }

    void deleteParticle() // Clone Particle Destroy
    {
        Destroy(diedClonePickupEffect);
    }
    
    IEnumerator skinmeshFalse()
    {
        if (isDied)
        {
            isDied = false;
            yield return new WaitForSecondsRealtime(1f);
            Pickup();
            _skinned.enabled = false;
            AudioSource.PlayClipAtPoint(deathSound.audioClipList[deathSoundIndex], gameObject.transform.position);
        }
    }
    IEnumerator skinmeshTrue()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        _skinned.enabled = true;
    }

    
    IEnumerator WaitRes()
    {
        capCol.enabled = false;
        _skinned.enabled = false;
        float tempSpeed = _controller.moveSpeed;
        _controller.moveSpeed = 0;
        yield return new WaitForSecondsRealtime(0.25f);
        _skinned.enabled = true;
        yield return new WaitForSecondsRealtime(0.25f);
        _skinned.enabled = false;
        yield return new WaitForSecondsRealtime(0.25f);
        _skinned.enabled = true;
        yield return new WaitForSecondsRealtime(0.25f);
        _skinned.enabled = false;
        yield return new WaitForSecondsRealtime(0.25f);
        _controller.moveSpeed = tempSpeed;
        _skinned.enabled = true;
        capCol.enabled = true;
    }

    public void calculateHighScore()
    {
        if (_distance.Idistance <= PlayerPrefs.GetInt("HighScore") && Oxygen.oxygenCylinder != 0)
        {
            int calculatedHighScore = PlayerPrefs.GetInt("HighScore") - _distance.Idistance;
            highScoreText.text = calculatedHighScore.ToString();
        }
        else if (PlayerPrefs.GetInt("HighScore") < _distance.Idistance && Oxygen.oxygenCylinder != 0)
        {
            highScoreText.text = _distance.Idistance.ToString();
        }
        else if (Oxygen.oxygenCylinder <= 0)
        {
            highScoreText.text = PlayerPrefs.GetInt("HighScore").ToString();
        }
    }
}
