using DPhysx;
using UnityEngine;

public class PhysicTester : SceneManager {
    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            Debug.Log("Space");
            DPhysxBox box = new DPhysxBox(FixedPoint2.FromVector2(transform.position), 1, 1);
            box.velocity += new FixedPoint2(1, 0.5f);
            GlobalManager.Instance.dPhysxManager.AddShape(box);
        }
    }
}
