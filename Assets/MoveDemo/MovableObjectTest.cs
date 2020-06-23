
using Nullspace;
using UnityEngine;

public class MovableObjectTest : MonoBehaviour
{
    private MovableObject Movable;
    private Camera Camera;
    private void Start()
    {
        Camera = Camera.main;
        for (int i = 0; i < 1; ++i)
        {
            CheckBulletMove();
        }
    }

    private void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 touchPos = Input.mousePosition;
            ChangeTarget(touchPos);
        }
        else if (Input.GetMouseButton(0))
        {
            Vector3 touchPos = Input.mousePosition;
            ChangeTarget(touchPos);
        }
    }

    private void ChangeTarget(Vector3 touchPos)
    {

        touchPos.z = Camera.farClipPlane;
        Vector3 pos = Camera.ScreenToWorldPoint(touchPos);
        Movable.SetTarget(pos);
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
        Movable = cube.AddComponent<MovableObject>();
        Movable.transform.localScale = new Vector3(50, 1, 100);

        Vector3 dir = new Vector3(Random.Range(1, 10), 0, Random.Range(1, 10));
        dir.Normalize();
        Movable.SetInfo(200, new Vector3(Random.Range(-1000, 1000), 0, Random.Range(-1000, 1000)), dir, 400);
        Movable.SetBoundInfo(1, 1, Screen.width - 1, Screen.height - 1, false);
        // Movable.SetTarget(new Vector3(100, 0, 100));
        Movable.SetTarget(target);
        // Movable.SetTarget();
        Movable.Move(true, false, false, true, true, false);
    }
}
