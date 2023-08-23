using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Linq;
using System;
using UnityEngine.SceneManagement;


public class AuthHandle : MonoBehaviour
{
    public TMP_InputField UsernameInputField;
    public TMP_InputField PasswordInputField;
    public TMP_InputField ScoreInputField;
    public string URL;
    public List<TextMeshProUGUI> puntajes_TXT;
    public TextMeshProUGUI puntjaeUsario;

    private string token;
    private string username;


    private void Start()
    {
        token = PlayerPrefs.GetString("token");

        if (string.IsNullOrEmpty(token))
        {
            Debug.Log("No hay token almacenado.");
        }
        else
        {
            username = PlayerPrefs.GetString("username");
            Scene scene = SceneManager.GetActiveScene();

            if (scene.buildIndex == 2) 
            {
                StartCoroutine(GetAll());
                StartCoroutine(GetPerfil(username, false));
                return;
            }
            StartCoroutine(GetPerfil(username,true));
        }
    }

    public void Puntaje()
    {
        PuntajeEnviar data = new PuntajeEnviar();
        data.data = new DataUser();
        data.username = username;
        data.data.score = int.Parse(ScoreInputField.text);
        string json = JsonUtility.ToJson(data);
        StartCoroutine(PatchScore(json));

    }
    public void Register()
    {
        Data data = new Data();
        data.username = UsernameInputField.text;
        data.password = PasswordInputField.text;
        string json = JsonUtility.ToJson(data);
        StartCoroutine(SendRegister(json));
    }

    public void Login()
    {
        Data data = new Data();
        data.username = UsernameInputField.text;
        data.password = PasswordInputField.text;
        string json = JsonUtility.ToJson(data);
        StartCoroutine(SendLogin(json));
        
    }

    public void LoginScene() 
    {
        PlayerPrefs.SetString("token", String.Empty);
        PlayerPrefs.SetString("username", String.Empty);
        SceneManager.LoadScene(1);
    }

    public void RegisterScene() 
    {
        SceneManager.LoadScene(0);
    }

    IEnumerator SendRegister(string json)
    {
        using (UnityWebRequest request = UnityWebRequest.Put(URL + "usuarios", json))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            request.method = "POST";
            yield return request.SendWebRequest();

            if (request.isNetworkError)
            {
                Debug.Log("Error de red: " + request.error);
            }
            else if (request.responseCode == 200)
            {
                Data data = JsonUtility.FromJson<Data>(request.downloadHandler.text);
                Debug.Log("Se registró el usuario con id " + data.usuario._id);
                SceneManager.LoadScene(1);
            }
            else
            {
                Debug.Log("Error en la solicitud: " + request.responseCode);
            }
        }
    }
    IEnumerator SendLogin(string json)
    {
        UnityWebRequest request = UnityWebRequest.Put(URL + "auth/login", json);
        request.SetRequestHeader("Content-Type", "application/json");
        request.method = "POST";
        yield return request.SendWebRequest();

        if (request.isNetworkError)
        {
            Debug.Log("Error");
        }
        else
        {
            Debug.Log(request.downloadHandler.text);
            if (request.responseCode == 200)
            {
                Data data = JsonUtility.FromJson<Data>(request.downloadHandler.text);
                Debug.Log("Inicio de sesión del usuario  " + data.usuario.username);

                // Actualizar valores en PlayerPrefs
                PlayerPrefs.SetString("token", data.token);
                PlayerPrefs.SetString("username", data.usuario.username);

                // Actualizar variables de la clase
                token = data.token;
                username = data.usuario.username;

                Debug.Log(data.token);
                SceneManager.LoadScene(2);
                request.downloadHandler.Dispose();
            }
            else
            {
                Debug.Log(request.error);
            }
        }
    }




    IEnumerator GetPerfil(string username, bool B)
    {
        UnityWebRequest request = UnityWebRequest.Get(URL + "usuarios/" + username);
        request.SetRequestHeader("x-token", token);
        yield return request.SendWebRequest();



        if (request.isNetworkError)
        {
            Debug.Log("Error");
        }
        else
        {
            //Debug.Log(request.downloadHandler.text);
            Data data = JsonUtility.FromJson<Data>(request.downloadHandler.text);
            Debug.Log("El Usuario " + data.usuario.username + " está activo");
            Debug.Log(data.usuario.data.score);
            if (B) 
            {
                SceneManager.LoadScene(2);
            }
            else 
            {
                changeTXT(data.usuario.username, data.usuario.data.score);
            }

        }
    }



    IEnumerator GetAll()
    {
        UnityWebRequest request = UnityWebRequest.Get(URL + "usuarios");
        request.SetRequestHeader("x-token", token);
        yield return request.SendWebRequest();

        if (request.isNetworkError)
        {
            Debug.Log("Error");
        }
        else
        {
            Data data = JsonUtility.FromJson<Data>(request.downloadHandler.text);

            var sortedUsuarios = data.usuarios.OrderByDescending(u => u.data.score).ToArray();

            for (int i = 0; i< puntajes_TXT.Count; i++) 
            {
                puntajes_TXT[i].text = sortedUsuarios[i].username + " Puntaje: " + sortedUsuarios[i].data.score;
            }
        }
    }


    void changeTXT(string name, int score) 
    {
        puntjaeUsario.text = name + " Puntaje: " + score;
    }

    IEnumerator PatchScore(string json)
    {
        UnityWebRequest request = UnityWebRequest.Put(URL + "usuarios", json);
        request.SetRequestHeader("Content-Type", "application/json");
        request.method = "PATCH";
        request.SetRequestHeader("x-token", token);
        yield return request.SendWebRequest();



        if (request.isNetworkError)
        {
            Debug.Log("Error");
        }
        else
        {
            //Debug.Log(request.downloadHandler.text);
            Data data = JsonUtility.FromJson<Data>(request.downloadHandler.text);
            Debug.Log(data.usuario.username + ": " + data.usuario.data.score);


            

            StartCoroutine(GetAll());

        }
    }
}



[System.Serializable]
public class Data
{
    public string username;
    public string password;
    public UserData usuario;
    public string token;
    public UserData[] usuarios;
}



public class PuntajeEnviar
{
    public string username;
    public DataUser data;
}





[System.Serializable]
public class UserData
{
    public string _id;
    public string username;
    public bool estado;
    public DataUser data;
}



[System.Serializable]
public class DataUser
{
    public int score;
}