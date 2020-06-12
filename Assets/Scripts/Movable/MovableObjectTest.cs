
using Nullspace;
using UnityEngine;

public class MovableObjectTest : MonoBehaviour
{
    private void Start()
    {
        CheckRayIntersect();
        for (int i = 0; i < 1; ++i)
        {
            CheckBulletMove();
        }
    }

    void CheckRayIntersect()
    {
        Vector3 dir = new Vector3(1, 0, 1);
        dir.Normalize();
        // 点在内部
        // CheckRayIntersect(Vector3.zero, dir);
        // 点在外部
        CheckRayIntersect(Vector3.zero - 1000 * dir, dir);
    }

    void CheckRayIntersect(Vector3 pos, Vector3 dir)
    {
        Vector3 min = new Vector3(-667, 0, -375);
        Vector3 max = new Vector3(667, 0, 375);

        Vector3 min1 = new Vector3(667, 0, -375);
        Vector3 max1 = new Vector3(-667, 0, 375);

        Vector3 refPos = pos;
        Vector3 refDir = dir;
        Vector3 nextPos = pos;
        Debug.DrawLine(min, min1, Color.red, 1000000);
        Debug.DrawLine(min1, max, Color.red, 1000000);
        Debug.DrawLine(max, max1, Color.red, 1000000);
        Debug.DrawLine(max1, min, Color.red, 1000000);
    }


    void CheckBulletMove()
    {
        GameObject sphere = new GameObject("movable");
        sphere.transform.localScale = new Vector3(50, 50, 50);
        sphere.transform.position = new Vector3(100, 0, 100);
        MovableObject target = sphere.AddComponent<MovableObject>();
        target.transform.position = new Vector3(Random.Range(-1000, 1000), 0, Random.Range(-1000, 1000));
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Collider col = cube.GetComponent<Collider>();
        GameObject.Destroy(col);
        MovableObject movable = cube.AddComponent<MovableObject>();
        movable.transform.localScale = new Vector3(50, 1, 100);

        Vector3 dir = new Vector3(Random.Range(1, 10), 0, Random.Range(1, 10));
        dir.Normalize();
        movable.SetInfo(200, new Vector3(Random.Range(-1000, 1000), 0, Random.Range(-1000, 1000)), dir, -1, 400);
        movable.SetBoundInfo(-667, -375, 667, 375, true);
        // bullet.SetTarget(new Vector3(100, 0, 100));
        movable.SetTarget(target);
        // bullet.SetTarget();
        movable.BulletFly(true, false, false, true, true, false);
    }
}
