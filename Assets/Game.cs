using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Game : MonoBehaviour
{
    public PlayerInputManager pim;
    public Camera mainCamera;

    //a list containing all the current players
    public List<Player> players;

    public Color A_COLOR;
    public Color B_COLOR;


    // Start is called before the first frame update
    void Start()
    {
        pim = GetComponent<PlayerInputManager>();

        if (mainCamera == null)
            mainCamera = Camera.main;


    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PlayerJoined(Player p)
    {
        /*
         * the centralized script is in the best position to track players and 
         * game events, the player script is best used for movements and behaviors that
         * are specific of each player
         */


        //if the input manager is set to split screen kill the main camera
        if (pim.splitScreen)
        {
            //disable the main camera, it will be replaced by the player camera
            mainCamera.enabled = false;
        }
        else if (p.playerInput != null)
            if (p.playerInput.camera != null)
            {

                //if it's not split screen kill the player camera just to be sure
                p.playerInput.camera.enabled = false;
            }


        //add to the array
        players.Add(p);

        //give it a unique name
        p.gameObject.name = "Player" + (players.Count).ToString();


        //assign a team
        if (players.Count <= 2)
        {
            p.ChangeColor(A_COLOR);
        }
        else
        {
            p.ChangeColor(B_COLOR);
        }

        print("Player " + p.gameObject.name + " joined the game");

        //note: I'm not managing the controller disconnection so things can get messy if
        //players unplug and replug during the game

    }



}
