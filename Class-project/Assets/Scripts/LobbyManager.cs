using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using TMPro;


public class LobbyManager : MonoBehaviourPunCallbacks
{
    public TMP_InputField playerNameInput;
    public TMP_InputField createdLobbyNameInput;
    public TMP_InputField joinedLobbyNameInput;
    public GameObject playerNameCanvas;
    public GameObject lobbyCanvas;
    public GameObject roomCanvas;
    public TextMeshProUGUI connectButtonText;
    public Button startButton;
    public Button leaveRoomButton;
    public GameObject lobbyPlayerList;
    private List<int> winners = new List<int>();


    private void Start()
    {
        PhotonNetwork.AddCallbackTarget(this);

        PhotonNetwork.AutomaticallySyncScene = true;
        lobbyCanvas.SetActive(false);
        roomCanvas.SetActive(false);
        leaveRoomButton.onClick.AddListener(OnClickLeaveRoomButton);
    }

    public void OnClickConnect()
    {
        if (playerNameInput.text.Length >= 1)
        {
            PhotonNetwork.NickName = playerNameInput.text;
            connectButtonText.text = "Connecting...";
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        playerNameCanvas.gameObject.SetActive(false);
        lobbyCanvas.gameObject.SetActive(true);

        PhotonNetwork.JoinLobby();
        PhotonNetwork.NetworkingClient.EventReceived += OnPhotonEvent;
    }

    public void OnClickCreateRoomButton()
    {
        PhotonNetwork.CreateRoom(createdLobbyNameInput.text, new RoomOptions { MaxPlayers = 10 });
    }

    public void OnClickJoinRoomButton()
    {
        PhotonNetwork.JoinRoom(joinedLobbyNameInput.text);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Room creation failed: {message}");
        // Handle the failure (e.g., show an error message to the user)
    }


    public void OnClickStartButton()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("Solo");
        }
    }

    public void OnClickLeaveRoomButton()
    {
        if (PhotonNetwork.InRoom)
        {
            if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount > 1) MigrateMaster();
            else
            {
                PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.LocalPlayer);
                PhotonNetwork.LeaveRoom();
            }
        }
    }

    private void MigrateMaster()
    {
        var dict = PhotonNetwork.CurrentRoom.Players;
        if (PhotonNetwork.SetMasterClient(dict[dict.Count - 1]))
            PhotonNetwork.LeaveRoom();
    }


    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Group");
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
        startButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        UpdatePlayerList();
    }


    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerList();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerList();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        startButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
    }

    private void UpdatePlayerList()
    {
        Debug.Log("hi4");
        string playerListText = "Players in the Lobby:\n";
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            playerListText += player.NickName + "\n";
        }
        lobbyPlayerList.GetComponent<TextMeshProUGUI>().text = playerListText;
    }

    private void OnPhotonEvent(ExitGames.Client.Photon.EventData photonEvent)
    {
        if (photonEvent.Code == 159)
        {
            int winnerId = (int)photonEvent.CustomData;

            if (!winners.Contains(winnerId))
            {
                winners.Add(winnerId);

                // Update the custom property on the Photon room
                UpdateWinnersCustomProperty();
            }
        }
    }

    private void UpdateWinnersCustomProperty()
    {
        // Convert the winners list to a format that can be easily sent through Photon (e.g., a string)
        string winnersString = string.Join(",", winners.Select(w => w.ToString()).ToArray());

        // Set the custom property on the Photon room
        PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "Winners", winnersString } });
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        // Check if the "Winners" custom property has changed
        if (propertiesThatChanged.ContainsKey("Winners"))
        {
            // Extract the updated winners string and update the local list
            string winnersString = (string)propertiesThatChanged["Winners"];
            winners = winnersString.Split(',').Select(int.Parse).ToList();
        }
    }

}
