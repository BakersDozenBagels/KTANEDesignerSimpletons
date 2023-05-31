using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class DissipatingCubeScript : DesignerSimpletonDesign
{
    public Material[] cubeMats;
    public MeshRenderer[] cubes;
    public GameObject cubeParent;
    public KMSelectable cubeSelectable;
    private bool pressed;
    private readonly List<Quaternion> points = new List<Quaternion>();
    private readonly List<float> weights = new List<float>();

    private void Start()
    {
        for (int i = 0; i < cubes.Length; i++)
        {
            int rand = Rnd.Range(0, cubeMats.Length);
            cubes[i].material = cubeMats[rand];
        }
    }

    private IEnumerator RemoveCubes()
    {
        var cubeShuffle = Enumerable.Range(0, cubes.Length).ToArray().Shuffle();
        for (int i = 0; i < cubeShuffle.Length; i++)
        {
            StartCoroutine(ShrinkCube(cubes[cubeShuffle[i]].gameObject));
            yield return null;
        }
    }

    private IEnumerator ShrinkCube(GameObject gameObject)
    {
        for(int i = 10; i >= 0; i--)
        {
            float s = i / 1000f;
            gameObject.transform.localScale = new Vector3(s, s, s);
            yield return null;
        }
    }

    private void FixedUpdate()
    {
        Quaternion calc = cubeParent.transform.localRotation;
        List<int> toRemove = new List<int>();
        for (int i = 0; i < points.Count; i++)
        {
            calc = Quaternion.Lerp(calc, points[i], weights[i]);
            weights[i] += (1) * Time.fixedDeltaTime;
            if (weights[i] > 1f)
                toRemove.Add(i);
        }
        foreach (int i in toRemove.OrderByDescending(c => c))
        {
            weights.RemoveAt(i);
            points.RemoveAt(i);
        }

        cubeParent.transform.localRotation = calc;

        if (weights.Count < 2)
        {
            points.Add(Rnd.rotation);
            weights.Add(0f);
        }
    }

    public override void Hook(DesignerSimpletonScript module, DesignerSimpletonData data)
    {
        cubeSelectable.OnInteract += () =>
        {
            if(!pressed)
                StartCoroutine(RemoveCubes());
            pressed = true;
            return false;
        };
    }
}