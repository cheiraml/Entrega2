using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class Httptest : MonoBehaviour
{
    public string BaseDatos;
    public string RickYMorty;
    public List<RawImage> Images;
    public TextMeshProUGUI textoDelJugador;
    public List<TextMeshProUGUI> textoDeCartas;
    private int UserID = 1;

    void Update() 
    {
        if (Input.GetKeyDown(KeyCode.A)) 
        {   if (UserID == 3)
            {
                UserID = 0;
            }
            UserID += 1;
            
        }

        if (Input.GetKeyDown(KeyCode.B))
        {   
            if (UserID == 1)
            {
                UserID = 4;
            }
            UserID -= 1;
            
        }
    }

    public void yoquese()
    {
        StartCoroutine(GetUsers());
    }

    IEnumerator GetUsers()
    {
        UnityWebRequest request = UnityWebRequest.Get(BaseDatos + UserID);
        yield return request.SendWebRequest();
            
        if (request.isNetworkError)
        {
            Debug.Log("Error");
        }
        else
        {
            Debug.Log(request.downloadHandler.text);
            User MyUser = JsonUtility.FromJson<User>(request.downloadHandler.text);
            textoDelJugador.text = MyUser.username;



            for(int i = 0; i< MyUser.deck.Count; i++) 
            {
                StartCoroutine(GetCharacter(i, MyUser.deck[i]));
            }

        }
    }



    IEnumerator GetCharacter(int i, int characterID)
    {
        UnityWebRequest request = UnityWebRequest.Get(RickYMorty + "/" + characterID);
        yield return request.SendWebRequest();

        if (request.isNetworkError)
        {
            Debug.Log("Error");
        }
        else
        {
            //Debug.Log(request.downloadHandler.text);
            Character character = JsonUtility.FromJson<Character>(request.downloadHandler.text);
            textoDeCartas[i].text = character.name;
            StartCoroutine(GetImage(character.image, i));
        }
    }


    IEnumerator GetImage(string url, int i)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.isNetworkError)
        {
            Debug.Log("Error");
        }
        else if (!request.isHttpError)
        {
            Images[i].texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
        }
    }
}


public class User
{
    public int id;
    public string username;
    public bool state;
    public List<int> deck;
}


public class Character
{
    public string name;
    public string image;
}