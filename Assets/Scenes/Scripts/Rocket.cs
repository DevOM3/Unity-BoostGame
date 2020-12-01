using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.SceneManagement;

public class Rocket : MonoBehaviour
{
    Rigidbody rigidBody;
    AudioSource audioSource;
    [SerializeField]float rcsThrust = 100f; // serialized filed is like export of GoDot to get the variable in inspector
    [SerializeField] float mainThrust = 100f;

    [SerializeField] AudioClip audioClip;
    [SerializeField] AudioClip success;
    [SerializeField] AudioClip death;

    [SerializeField] ParticleSystem mainEngineParticle;
    [SerializeField] ParticleSystem successParticle;
    [SerializeField] ParticleSystem deathParticle;

    bool collisionEnable = true;

    enum State { Alive, Dying, Transcending };
    State state = State.Alive;


    // Start is called before the first frame update
    void Start()
    {
        // getting the rigid body components
        rigidBody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (state == State.Alive)
        {
            ProcessInput();
            Rotate();
            if (Debug.isDebugBuild) { ProcessDebug(); } // check is this is debug build and we can use debug keys
        }
    }

    void ProcessDebug()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadNextScene();
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            collisionEnable = !collisionEnable;    
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!collisionEnable) { return;  }
        if (state != State.Alive)
        {
            return;
        }
        switch (collision.gameObject.tag)
        {
            case "Friendly":

                break;
            case "Finish":
                state = State.Transcending;
                audioSource.PlayOneShot(success); // playing the sound after the level is crossed
                successParticle.Play();
                Invoke("LoadNextScene", 1f); // it invokes a function given as parameter after given seconds
                break;
            default:
                state = State.Dying;
                audioSource.Stop();
                audioSource.PlayOneShot(death); // playing the sound after the level is failed
                deathParticle.Play();
                Invoke("LoadPreviousLevel", 2f);
                break;
        }
    }

    void LoadNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;
        if (nextSceneIndex == SceneManager.sceneCountInBuildSettings) { nextSceneIndex = 0; }
        SceneManager.LoadScene(nextSceneIndex);
    }

    void LoadPreviousLevel()
    {
        SceneManager.LoadScene(1);
    }

    void ProcessInput()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            rigidBody.AddRelativeForce(Vector3.up * mainThrust * Time.deltaTime);
            if (!audioSource.isPlaying)
            {
                audioSource.PlayOneShot(audioClip);
                mainEngineParticle.Play();
            }
        }
        else
        {
            mainEngineParticle.Stop();
            audioSource.Stop();
        }
    }

    void Rotate()
    {
        rigidBody.angularVelocity = Vector3.zero; // remove rotation due to physics engine

        float rotationThisFrame = rcsThrust * Time.deltaTime;
        if (Input.GetKey(KeyCode.A))
        {
            // Vector3.forward = Z-Axis 
            transform.Rotate(Vector3.forward * rotationThisFrame);
        }

        else if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(-Vector3.forward * rotationThisFrame);
        }
    }
}
