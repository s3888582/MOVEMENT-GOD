using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_DisplaySpeed : MonoBehaviour {

    void Update() {
        PlayerMovement player = new PlayerMovement();

    if (player.walkSpeed != null || player.runSpeed != null) {
        Debug.Log("Walk Speed : " + player.walkSpeed + "\nRun speed : " + player.runSpeed);
    } else {
        Debug.Log("PLAYER MOVEMENT ERROR");
        }
    }
}