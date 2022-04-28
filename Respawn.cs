using UnityEngine;
using System.IO;
using System.Reflection;

public class Respawn : FortressCraftMod
{
    private bool playerMoved;
    private float saveTimer;
    private static readonly string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

    // Mod registration.
    public override ModRegistrationData Register()
    {
        ModRegistrationData modRegistrationData = new ModRegistrationData();
        return modRegistrationData;
    }

    // Called once per frame by unity engine.
    public void Update()
    {
        if (GameState.PlayerSpawned)
        { 
            if (playerMoved == false)
            {
                MovePlayer();
            }
            else
            {
                saveTimer += 1 * Time.deltaTime;
                if (saveTimer >= 10)
                {
                    SaveLocation();
                    saveTimer = 0;
                }
            }
        }
    }

    // Moves the player to their saved location.
    private void MovePlayer()
    {
        string respawnFilePath;

        if (NetworkManager.instance.mClientThread != null)
        {
            System.Net.IPAddress serverIP = NetworkManager.instance.mClientThread.serverIP;
            ulong userID = NetworkManager.instance.mClientThread.mPlayer.mUserID;
            respawnFilePath = Path.Combine(assemblyFolder, userID + ":" + serverIP + ".txt");
        }
        else
        {
            ulong userID = WorldScript.instance.localPlayerInstance.mPlayer.mUserID;
            string worldName = WorldScript.instance.mWorldData.mName;
            respawnFilePath = Path.Combine(assemblyFolder, userID + ":" + worldName + ".txt");
        }

        if (File.Exists(respawnFilePath))
        {
            string fileContents = File.ReadAllText(respawnFilePath);
            string[] coords = fileContents.Split(',');
            float x = float.Parse(coords[0]);
            float y = float.Parse(coords[1]) + 5;
            float z = float.Parse(coords[2]);
            WorldScript.Teleport(x + " " + y + " " + z);
        }

        playerMoved = true;
    }

    // Saves the player's location.
    private void SaveLocation()
    {
        string worldID;
        Player player;

        if (NetworkManager.instance.mClientThread != null)
        {
            worldID = NetworkManager.instance.mClientThread.serverIP.ToString();
            player = NetworkManager.instance.mClientThread.mPlayer;
        }
        else
        {
            worldID = WorldScript.instance.mWorldData.mName;
            player = WorldScript.instance.localPlayerInstance.mPlayer;
        }

        if (player != null)
        {
            ulong userID = player.mUserID;
            float x = player.mnWorldX - 4611686017890516992L;
            float y = player.mnWorldY - 4611686017890516992L;
            float z = player.mnWorldZ - 4611686017890516992L;
            string respawnFilePath = Path.Combine(assemblyFolder, userID + ":" + worldID + ".txt");
            File.WriteAllText(respawnFilePath, x + "," + y + "," + z);
        }
    }
}
