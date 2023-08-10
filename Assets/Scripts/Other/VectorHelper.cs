using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class VectorHelper : MonoBehaviour
{
    static public Vector2 AngleVector(Vector2 vector, float angle)
    {
        return Quaternion.AngleAxis(angle, Vector3.forward) * vector;
    }

    static public List<Vector2> CreateVectorSpread(Vector2 direction, float spreadAngle, int vectorAmount)
    {
        List<Vector2> output = new();

        if (vectorAmount % 2 != 1)
            vectorAmount--;

        if (vectorAmount <= 1)
            output.Add(direction);

        else
        {
            output.Add(direction);
            output.Add(AngleVector(direction, spreadAngle));
            output.Add(AngleVector(direction, -spreadAngle));
        }

        if (vectorAmount > 3)
        {
            int remainingVectorAmount = (vectorAmount - 3);

            for (int x = -(remainingVectorAmount / 2); x <= remainingVectorAmount / 2; x++)
            {
                if (x == 0)
                    continue;

                float angle = spreadAngle / (remainingVectorAmount / 2 + 1) * x;
                output.Add(AngleVector(direction, angle));
            }
        }


        return output;
    }
}
public class GameObjectHelper : MonoBehaviour
{
    static public bool AnyObjectsWithinDistance(GameObject start, float distance, string objectTag)
    {
        return GameObject.FindGameObjectsWithTag(objectTag).Any(o => Vector2.Distance(o.transform.position, start.transform.position) <= distance);
    }

    static public bool IsObjectWithinDistance(GameObject start, GameObject target,float distance)
    {
        return Vector2.Distance(start.transform.position, target.transform.position) <= distance;
    }

    static public List<GameObject> GetGameObjectsInRange(GameObject start, string objectTag, float maxDistance = float.PositiveInfinity)
    {
        List<GameObject> gameObjects = GameObject.FindGameObjectsWithTag(objectTag).ToList();
        return gameObjects.Where(o => Vector2.Distance(o.transform.position, start.transform.position) <= maxDistance).ToList();
    }
}
