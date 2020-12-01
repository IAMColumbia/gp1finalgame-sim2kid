using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcManager : MonoBehaviour
{
    [SerializeField] private int predictionOffset = 4;
    [SerializeField] private int numberOfMarkers = 40;
    [SerializeField] private Sprite markerElement;
    [SerializeField] private PlayerController player;
    private GameObject[] markerPool;
    private float[] rollingFrameRate;
    private int onFrame;
    // Start is called before the first frame update
    void Start()
    {
        markerPool = new GameObject[numberOfMarkers];
        for (int i = 0; i < markerPool.Length; i++) 
        {
            markerPool[i] = new GameObject();
            markerPool[i].name = "Marker #" + i;
            markerPool[i].transform.parent = this.transform; // Hides markers under the ArcManager
            SpriteRenderer sr = markerPool[i].AddComponent<SpriteRenderer>();
            sr.sprite = markerElement;
            markerPool[i].SetActive(false);
        }

        rollingFrameRate = new float[50];
        resetAverageFrameRate();

        if (player == null) 
        {
            try
            {
                player = GameObject.FindObjectOfType<PlayerController>();
            }
            catch (System.NullReferenceException e) 
            {
                Debug.LogError("You need to have a PlayerController in the Scene to use the ArcManager!\nException: " + e);
                throw e;
            }
        }
    }

    void recordFrameRate(float frameRate) 
    {
        rollingFrameRate[onFrame] = frameRate;
        onFrame++;
        if (onFrame >= rollingFrameRate.Length) 
        {
            onFrame = 0;
        }
    }

    void resetAverageFrameRate() 
    {
        onFrame = 0;
        for (int i = 0; i < rollingFrameRate.Length; i++)
        {
            rollingFrameRate[i] = 0;
        }
    }

    float averageFrameRate() 
    {
        float added = 0;
        for (int i = 0; i < rollingFrameRate.Length; i++)
        {
            added += rollingFrameRate[i];
        }
        return added / rollingFrameRate.Length;
    }

    void OnDisable()
    {
        for (int i = 0; i < markerPool.Length; i++)
        {
            markerPool[i].SetActive(false);
        }
        resetAverageFrameRate();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        recordFrameRate(Time.fixedDeltaTime);
        Vector2 nextPos = (Vector2)player.transform.position;
        Vector2 velocity = player.JumpStrength;
        for (int i = 0; i < markerPool.Length * predictionOffset; i++)
        {
            velocity += Physics2D.gravity * averageFrameRate();
            nextPos += velocity * averageFrameRate();
            if (i % predictionOffset == 0) 
            {
                int y = (int)(i / predictionOffset);
                markerPool[y].SetActive(true);
                markerPool[y].transform.position = nextPos;
            }
        }
    }
}
