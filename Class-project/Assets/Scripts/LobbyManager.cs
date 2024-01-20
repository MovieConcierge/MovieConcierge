using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using TMPro;


public class LobbyManager : MonoBehaviourPunCallbacks
{
    #region variables
    public TMP_InputField playerNameInput;
    public TMP_InputField createdLobbyNameInput;
    public TMP_InputField joinedLobbyNameInput;
    public GameObject playerNameCanvas;
    public GameObject lobbyCanvas;
    public GameObject roomCanvas;
    public GameObject RoomName;
    public TextMeshProUGUI connectButtonText;
    public Button connectButton;
    public Button createRoomButton;
    public Button joinRoomButton;
    public Button startButton;
    public Button leaveRoomButton;
    public GameObject lobbyPlayerList;

    #endregion

    private void Start()
    {
        PhotonNetwork.AddCallbackTarget(this);
        PhotonNetwork.AutomaticallySyncScene = true; //for starting the game

        lobbyCanvas.SetActive(false);
        roomCanvas.SetActive(false);
        leaveRoomButton.onClick.AddListener(OnClickLeaveRoomButton);
    }

    #region Photon connection and room management
    public void OnClickConnect()
    {
        if (PhotonNetwork.IsConnected)
        {
            //Player is trying to connect without having disconnected before
            PhotonNetwork.Disconnect();
        }
        

        if (playerNameInput.text.Length >= 1)
        {
            PhotonNetwork.NickName = playerNameInput.text;
            connectButtonText.text = "Connecting...";

            // Start the connection attempt
            PhotonNetwork.ConnectToRegion("jp");
            connectButton.interactable = false;
            
        }
    }

    public override void OnConnectedToMaster()
    {
        createRoomButton.interactable = true;
        joinRoomButton.interactable = true;
        connectButton.interactable = true;
        playerNameCanvas.gameObject.SetActive(false);
        lobbyCanvas.gameObject.SetActive(true);

        PhotonNetwork.JoinLobby();
    }

    public void OnClickCreateRoomButton()
    {
        createRoomButton.interactable = false;
        joinRoomButton.interactable = false;
        if (string.IsNullOrEmpty(createdLobbyNameInput.text))
        {
            Debug.LogError("Lobby name cannot be empty. Please enter a valid lobby name.");

            createRoomButton.interactable = true;
            joinRoomButton.interactable = true;
            return;
        }

        // Create room with custom properties
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 10,
            CustomRoomProperties = new ExitGames.Client.Photon.Hashtable { { "Winners", "" } },
            CustomRoomPropertiesForLobby = new string[] { "Winners" }
        };

        PhotonNetwork.CreateRoom(createdLobbyNameInput.text, roomOptions);
        RoomName.GetComponent<TextMeshProUGUI>().text = "Room: " + createdLobbyNameInput.text;
    }

    public void OnClickJoinRoomButton()
    {
        createRoomButton.interactable = false;
        joinRoomButton.interactable = false;
        if (string.IsNullOrEmpty(joinedLobbyNameInput.text))
        {
            Debug.LogError("Lobby name cannot be empty. Please enter a valid lobby name.");

            createRoomButton.interactable = true;
            joinRoomButton.interactable = true;
            return;
        }
        RoomName.GetComponent<TextMeshProUGUI>().text = "Room: " + joinedLobbyNameInput.text;
        PhotonNetwork.JoinRoom(joinedLobbyNameInput.text);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Room creation failed: {message}");
        // Handle the failure (e.g., show an error message to the user)
        createRoomButton.interactable = true;
        joinRoomButton.interactable = true;
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Failed to join room: " + message);
        // Provide feedback to the user or take appropriate action, such as prompting to create the room
        createRoomButton.interactable = true;
        joinRoomButton.interactable = true;
    }

    public void OnClickStartButton()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(DelayedLoadScene());
        }
    }

    private IEnumerator DelayedLoadScene()
    {
        
        yield return new WaitForSeconds(0.5f); // Adjust the delay duration as needed

        if (PhotonNetwork.CurrentRoom.PlayerCount > 1)
        {
            PhotonNetwork.LoadLevel("Solo");
        }
        else
        {
            Debug.Log("Cannot start the game. There should be at least 2 players in the room.");
        }
    }


    public void OnClickLeaveRoomButton()
    {
        leaveRoomButton.interactable = false;
        if (PhotonNetwork.InRoom)
        {
            if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount > 1) MigrateMaster();
            else
            {
                PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.LocalPlayer);
            }
        }
        PhotonNetwork.LeaveRoom();
    }

    public void MigrateMaster()
    {
        var dict = PhotonNetwork.CurrentRoom.Players;

        // Find the next player to set as master client
        foreach (var player in dict.Values)
        {
            if (!player.IsLocal)
            {
                if (PhotonNetwork.SetMasterClient(player))
                {
                    break;
                }
            }
        }
    }


    public override void OnLeftRoom()
    {
        leaveRoomButton.interactable = true;
        roomCanvas.gameObject.SetActive(false);
        lobbyCanvas.gameObject.SetActive(true);

        if (!PhotonNetwork.InRoom && PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer)
        {
            PhotonNetwork.JoinLobby();
        }
    }

    public void OnClickReturnToMenuButton()
    {
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene("Title");
    }

    public override void OnJoinedRoom()
    {
        lobbyCanvas.SetActive(false);
        roomCanvas.SetActive(true);
        
        UpdatePlayerList();
    }

    public void UpdatePlayerList()
    {
        string playerListText = "Players in the Lobby:\n";
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            playerListText += player.NickName + "\n";
        }
        lobbyPlayerList.GetComponent<TextMeshProUGUI>().text = playerListText;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        startButton.gameObject.SetActive(PhotonNetwork.CurrentRoom.PlayerCount > 1 && PhotonNetwork.IsMasterClient);
        UpdatePlayerList();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        startButton.gameObject.SetActive(PhotonNetwork.CurrentRoom.PlayerCount > 1 && PhotonNetwork.IsMasterClient);
        UpdatePlayerList();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        startButton.gameObject.SetActive(PhotonNetwork.CurrentRoom.PlayerCount > 1 && PhotonNetwork.IsMasterClient);
    }

    #endregion
    


}
