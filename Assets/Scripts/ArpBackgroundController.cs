using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[System.Serializable]
public class Star{
    public SpriteRenderer sprite;
    public Color color;
    public float timer;
    public MaterialPropertyBlock mpb;    

    public int layer;
}

[System.Serializable]
public class StarLayer{
    public float scale = 1f;
    public float alpha = 1f;

    public float zOffset = 0f;
}

public class ArpBackgroundController : MonoBehaviour
{
    public Bounds bounds;

    [Range (1,100)]
    public int spawnCount;

    public int maxStars;
    public float spawnCooloff;
    public float lastSpawnTime;
    public SpriteRenderer starPrefab;

    public Gradient spawnColor;

    public float lifeTime;

    public AnimationCurve scaleCurve;
    public AnimationCurve colorCurve;



    public List<Star> stars = new List<Star>();

    public bool enableStars = true;

     public AudioSyncSettings[] m_settings;

    private float m_previousAudioValue;
    private float m_audioValue;


    public AudioPlayerSync player;

    
    public Vector3 beatWeights;
    private Vector3 beatTime;

    public StarLayer[] layers;

    public Vector3 displacement;


    // Start is called before the first frame update
    void Start()
    {
        stars.Clear();
        lastSpawnTime = Time.time;
    }

    [ContextMenu("Set Bounds To Screen")]
    void SetBoundsToScreen(){
        bounds = new Bounds(Camera.main.transform.position, new Vector3(Screen.width, Screen.height, 10));
    }

    public  void SpawnStars(){
        if (!enableStars) { return; }
        if(spawnCooloff > 0 && Time.time - lastSpawnTime < spawnCooloff){
            return;
        }
        lastSpawnTime = Time.time;
        int spawnAmount = Mathf.Min(maxStars - stars.Count, ( int)Random.Range(1, spawnCount));


        for (int i = 0; i < spawnAmount; i++)
        {
            Vector3 spawnPosition = new Vector3(Random.Range(bounds.min.x, bounds.max.x), Random.Range(bounds.min.y, bounds.max.y), 0) + transform.position;
            var star = Instantiate(starPrefab, spawnPosition, Quaternion.identity, transform);
            var starColor = spawnColor.Evaluate(Random.Range(0f, 1f));            
            star.color = starColor;
            
            var layer =Random.Range(0, layers.Length);
            star.transform.localPosition += new Vector3(0, 0, layers[layer].zOffset);
            stars.Add(new Star{sprite = star, timer = 0, color = starColor, mpb = new MaterialPropertyBlock(), layer = layer});
            int idx = stars.Count - 1;
            UpdateStar(idx);
        }

    }

    public void UpdateStar(int idx){
        var star = stars[idx];
        var layerInfo = layers[star.layer];
        star.timer += Time.deltaTime;
        star.sprite.transform.localScale = Vector3.one * scaleCurve.Evaluate(star.timer / lifeTime) * layerInfo.scale * displacement[star.layer];
        var starColor = star.color * colorCurve.Evaluate(star.timer / lifeTime) * layerInfo.alpha;
        star.mpb.SetColor("_Color", starColor);
        star.sprite.SetPropertyBlock(star.mpb);
        
        if (star.timer > lifeTime)
        {
            Destroy(star.sprite.gameObject);
            stars.RemoveAt(idx);
        }
    }

    public void  Update(){

        SpawnStars();
        //// update audio value
        m_previousAudioValue = m_audioValue;
        var currentTime = Time.time;
        // get the data
        m_audioValue = player.GetSpectrumData();
        
        for (var i = 0; i < m_settings.Length; i++)
        {
            var setting = m_settings[i];
            var nextValidActivationTime = setting.timeStep + beatTime[i] + setting.totalTime;
            // if audio value went below the bias during this frame
            if (m_previousAudioValue > setting.bias &&
                m_audioValue <= setting.bias)
            {
                // if minimum beat interval is reached
                
                if (currentTime > nextValidActivationTime)
                    beatTime[i] = Time.time;
            }

            // if audio value went above the bias during this frame
            if (m_previousAudioValue <= setting.bias &&
                m_audioValue > setting.bias)
            {
                // if minimum beat interval is reached
                if (currentTime > nextValidActivationTime)
                    beatTime[i] = Time.time;
            }
        }

        
        
        displacement = Vector3.zero;
        for (var i = 0; i < m_settings.Length; i++)
        {
            var setting = m_settings[i];
            var timeElapsed = currentTime - beatTime[i];
            if (timeElapsed > setting.totalTime)
            {
                displacement[i] = 0;
                continue;
            }
            else
            {
                float t = timeElapsed / setting.totalTime;
                var val = setting.curve.Evaluate(t);
                displacement[i] = val * beatWeights[i];
            }
        }
        

             for (int i = 0; i < stars.Count; i++)
        {
            UpdateStar(i);
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(bounds.center + transform.position, bounds.size);
    }
}
