using UnityEngine;

//Solely exists for the Windows Build, to enable the Player/User to quit the App by pressing Escape
//Isn't in the Scene for the WebGl Build, since that doesn't work there anyway.
public class EscapeChecker : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
    }
}
