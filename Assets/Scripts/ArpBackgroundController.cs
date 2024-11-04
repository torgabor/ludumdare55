using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Star{
    public SpriteRenderer sprite;
    public Color color;
    public float timer;
    public MaterialPropertyBlock mpb;
}

public class ArpBackgroundController : AudioSyncer
{
    public Bounds bounds;

    [Range (1,100)]
    public int spawnCount;
    public SpriteRenderer starPrefab;

    public Gradient spawnColor;

    public float lifeTime;

    public AnimationCurve scaleCurve;
    public AnimationCurve colorCurve;

    public List<Star> stars = new List<Star>();

    public bool enableStars = true;

    

    // Start is called before the first frame update
    void Start()
    {
        stars.Clear();
    }

    [ContextMenu("Set Bounds To Screen")]
    void SetBoundsToScreen(){
        bounds = new Bounds(Camera.main.transform.position, new Vector3(Screen.width, Screen.height, 10));
    }

    public override void OnDoubleBeat(){
        if (!enableStars) { return; }
        int spawnAmount = (int)Random.Range(1, spawnCount);

        for (int i = 0; i < spawnAmount; i++)
        {
            Vector3 spawnPosition = new Vector3(Random.Range(bounds.min.x, bounds.max.x), Random.Range(bounds.min.y, bounds.max.y), 0) + transform.position;
            var star = Instantiate(starPrefab, spawnPosition, Quaternion.identity, transform);
            star.color = spawnColor.Evaluate(Random.Range(0f, 1f));
            stars.Add(new Star{sprite = star, timer = 0, color = star.color, mpb = new MaterialPropertyBlock()});
            int idx = stars.Count - 1;
            UpdateStar(idx);
        }

    }

    public void UpdateStar(int idx){
        var star = stars[idx];
        star.timer += Time.deltaTime;
        star.sprite.transform.localScale = Vector3.one * scaleCurve.Evaluate(star.timer / lifeTime);
        star.mpb.SetColor("_Color", star.color * colorCurve.Evaluate(star.timer / lifeTime));
        star.sprite.SetPropertyBlock(star.mpb);
        if (star.timer > lifeTime)
        {
            Destroy(star.sprite.gameObject);
            stars.RemoveAt(idx);
        }
    }

    public override void OnUpdate(){
        base.OnUpdate();
        for (int i = 0; i < stars.Count; i++)
        {
            UpdateStar(i);
        }
    }

    // Update is called once per frame


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(bounds.center + transform.position, bounds.size);
    }
}
