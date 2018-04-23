using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.IO;
using Newtonsoft.Json;

public class GameServerManager : MonoBehaviour {

    public GameObject NoticePanel;
    public GameObject LoginPanel;
    public GameObject ScorePanel;

    [SerializeField] Text userText;
    [SerializeField] Text nameText;
    [SerializeField] Text username;
    [SerializeField] InputField passText;
    [SerializeField] GameObject createPanel;
    [SerializeField] GameObject playerScorePrefab;

    [SerializeField] internal string newUser;
    [SerializeField] internal string newPassword;
    [SerializeField] internal string newName;
    [SerializeField] internal string newRequest;

    [SerializeField] Player[] players;
    [SerializeField] Player[] sortedPlayers;

    [SerializeField] string URL;

    // Use this for initialization
    void Start()
    {
    }

    public void Login()
    {
        newRequest = URL + "login?username=" + userText.text + "&password=" + passText.text;
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(newRequest);
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        Stream stream = response.GetResponseStream();
        string responseBody = new StreamReader(stream).ReadToEnd();
        if (responseBody == "0")
        {
            newRequest = URL + "onlineauthen?username=" + userText.text;
            HttpWebRequest loginRequest = (HttpWebRequest)WebRequest.Create(newRequest);
            HttpWebResponse loginResponse = (HttpWebResponse)loginRequest.GetResponse();
            Stream loginStream = loginResponse.GetResponseStream();
            string loginResponseBody = new StreamReader(loginStream).ReadToEnd();
            username.text = loginResponseBody;
            createPanel.SetActive(false);
            LoginPanel.SetActive(false);
            InitRanking();
        }
        else
        {
            NoticePanel.transform.GetChild(0).GetComponent<Text>().text = "INVALID USER OR PASSWORD.";
            NoticePanel.SetActive(true);
        }
    }

    public void CreateStepOne()
    {
        if (!string.IsNullOrEmpty(userText.text))
        {
            newRequest = URL + "authen/id?username=" + userText.text;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(newRequest);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();
            string responseBody = new StreamReader(stream).ReadToEnd();
            if (responseBody == "0")
            {
                if (!string.IsNullOrEmpty(passText.text))
                {
                    newUser = userText.text;
                    newPassword = passText.text;
                    createPanel.SetActive(true);
                }
                else
                {
                    NoticePanel.transform.GetChild(0).GetComponent<Text>().text = "TO PROCEED ENTER YOUR PASSWORD.";
                    NoticePanel.SetActive(true);
                }
            }
            else if (responseBody == "1" && string.IsNullOrEmpty(passText.text))
            {
                NoticePanel.transform.GetChild(0).GetComponent<Text>().text = "TO PROCEED ENTER YOUR PASSWORD.";
                NoticePanel.SetActive(true);
            }
            else
            {
                NoticePanel.transform.GetChild(0).GetComponent<Text>().text = "YOUR ACCOUNT ID IS DUPLICATED.";
                NoticePanel.SetActive(true);
            }
        }
        else
        {
            NoticePanel.transform.GetChild(0).GetComponent<Text>().text = "TO PROCEED ENTER YOUR USERNAME.";
            NoticePanel.SetActive(true);
        }
    }

    void CreateAcc() {
        newName = nameText.text;
        int r = Random.Range(0, 999999999);
        newRequest = URL + "user/add/user?" + "username=" + newUser + "&password=" + newPassword + "&name=" + newName + "&score=" + r;
        HttpWebRequest completeRequest = (HttpWebRequest)WebRequest.Create(newRequest);
        HttpWebResponse completetResponse = (HttpWebResponse)completeRequest.GetResponse();
        Stream completeStream = completetResponse.GetResponseStream();
        string completeResponseBody = new StreamReader(completeStream).ReadToEnd();
        print(completeResponseBody);
        if (completeResponseBody == "0")
        {
            // login
            createPanel.SetActive(false);
            Login();
        }
    }

    public void SetName() {
        if (!string.IsNullOrEmpty(nameText.text))
        {
            newRequest = URL + "authen/name?name=" + nameText.text;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(newRequest);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();
            string responseBody = new StreamReader(stream).ReadToEnd();
            if (responseBody == "0")
            {
                CreateAcc();
            }
            else
            {
                NoticePanel.transform.GetChild(0).GetComponent<Text>().text = "YOUR NAME IS DUPLICATED.";
                NoticePanel.SetActive(true);
            }
        }
        else
        {
            NoticePanel.transform.GetChild(0).GetComponent<Text>().text = "TO PROCEED ENTER YOUR NAME.";
            NoticePanel.SetActive(true);
        }
    }

    public void CancelSetName()
    {
        if (createPanel.activeInHierarchy)
        {
            createPanel.SetActive(false);
        }
    }

    public void CancelNotice()
    {
        if (NoticePanel.activeInHierarchy)
        {
            NoticePanel.SetActive(false);
        }
    }

    public void InitRanking() {
        newRequest = URL + "topscore";
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(newRequest);
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        Stream stream = response.GetResponseStream();
        string responseBody = new StreamReader(stream).ReadToEnd();
        players = JsonConvert.DeserializeObject<Player[]>(responseBody);

        SortingPlayer();
        for (int x = 0; x < ScorePanel.transform.GetChild(0).GetChild(0).GetChild(0).childCount; x++)
        {
            Destroy(ScorePanel.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(x).gameObject);
        }
        for (int j = 0; j < sortedPlayers.Length; j++)
        {
            GameObject playerScore = Instantiate(playerScorePrefab, Vector3.zero, Quaternion.identity, ScorePanel.transform.GetChild(0).GetChild(0).GetChild(0).gameObject.transform);
            playerScore.transform.GetChild(0).GetComponent<Text>().text = sortedPlayers[j].name;
            playerScore.transform.GetChild(1).GetComponent<Text>().text = (sortedPlayers[j].score).ToString();
        }
        ScorePanel.SetActive(true);
    }

    public void LogoutButton()
    {
        if (ScorePanel.activeInHierarchy)
        {
            ScorePanel.SetActive(false);
            userText.text = "";
            passText.text = "";
            LoginPanel.SetActive(true);
        }
    }

    Player[] SortingPlayer()
    {
        sortedPlayers = players;
        int max;
        Player temp;
        for (int i = 0; i < sortedPlayers.Length; i++)
        {
            max = i;
            for (int j = i + 1; j < sortedPlayers.Length; j++)
            {
                if (sortedPlayers[j].score > sortedPlayers[max].score)
                {
                    max = j;
                }
            }
            if (max != i)
            {
                temp = sortedPlayers[i];
                sortedPlayers[i] = sortedPlayers[max];
                sortedPlayers[max] = temp;
            }
        }
        return sortedPlayers;
    }
}
