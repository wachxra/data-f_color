using UnityEngine;

[CreateAssetMenu(fileName = "BoxMergeRule", menuName = "Game/Box Merge Rule")]
public class BoxMergeRule : ScriptableObject
{
    [System.Serializable]
    public class MergeEntry
    {
        public ColorType colorA;
        public ColorType colorB;
        public GameObject resultPrefab;
        public bool explodeIfMatched = false;
    }

    public MergeEntry[] mergeRules;
    public GameObject explosionPrefab;

    public GameObject GetResult(ColorType a, ColorType b, out bool explode, out GameObject usedExplosionPrefab)
    {
        explode = false;
        usedExplosionPrefab = null;

        foreach (var rule in mergeRules)
        {
            bool match = (rule.colorA == a && rule.colorB == b) ||
                         (rule.colorA == b && rule.colorB == a);

            if (match)
            {
                explode = rule.explodeIfMatched;

                if (explode)
                    usedExplosionPrefab = explosionPrefab;

                return rule.resultPrefab;
            }
        }

        return null;
    }
}