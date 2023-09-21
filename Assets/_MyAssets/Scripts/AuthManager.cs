using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using TMPro;
using System;
using UnityEngine.UI;

public class AuthManager : MonoBehaviour
{
    notifScript NotificationScript;

    //PlayerPref
    private string _savedEmail;
    private string _savedPassword;
    private int _bSaveAutomatically;

    //Background switching
    [SerializeField] private GameObject background;

    //Firebase
    [SerializeField] private DependencyStatus _dependentStatus;
    [SerializeField] private FirebaseAuth _auth;
    [SerializeField] private FirebaseUser _user;


    //Login
    private TMP_InputField _usernameField;
    private TMP_InputField _emailField;
    private TMP_InputField _passwordField;

    //Checkmark
    [SerializeField] private Image _checkmark;


    private void Awake()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            _dependentStatus = task.Result;
            if(_dependentStatus == DependencyStatus.Available)
            {
                _auth = FirebaseAuth.DefaultInstance;
                Debug.Log("setting up Auth");
            }
            else
            {
                Debug.LogError("Could not resolve dependencies: " + _dependentStatus);
            }    
        });
    }

    private void Start()
    {
        getDependencies();
        getPlayerPrefs();
        if (_bSaveAutomatically == 1 && _savedEmail != "" && _savedPassword != "") StartCoroutine(signInAuto());
    }

    private void getDependencies()
    {
        //Login
        _usernameField = background.transform.Find("usernameField").GetComponent<TMP_InputField>();
        _emailField = background.transform.Find("emailField").GetComponent<TMP_InputField>();
        _passwordField = background.transform.Find("passwordField").GetComponent<TMP_InputField>();

        NotificationScript = GameObject.FindGameObjectWithTag("Canvas").GetComponent<notifScript>();
    }

    private void getPlayerPrefs()
    {
        _savedEmail = PlayerPrefs.GetString("email");
        _savedPassword = PlayerPrefs.GetString("password");
        _bSaveAutomatically = PlayerPrefs.GetInt("save");

        _checkmark.color = (_bSaveAutomatically == 1) ? new Color(255, 255, 255, 255) : new Color(0, 0, 0, 0);
    }

    private IEnumerator signInAuto()
    {
        yield return new WaitForSeconds(1);
        StartCoroutine(Login(_savedEmail, _savedPassword));
    }

    public void loginAutoToggle()
    {
        _bSaveAutomatically = (_bSaveAutomatically == 0) ? 1 : 0;
        _checkmark.color = (_bSaveAutomatically == 1) ? new Color(255, 255, 255, 255) : new Color(0, 0, 0, 0);
        PlayerPrefs.SetInt("save", _bSaveAutomatically);
    }

    public void LoginButton()
    {
        StartCoroutine(Login(_emailField.text, _passwordField.text));
    }

    private IEnumerator Login(string email, string password)
    {
        var loginTask = _auth.SignInWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(predicate: () => loginTask.IsCompleted);

        if (loginTask.Exception != null) // error message
        {
            Debug.Log(message: $"Failed task: {loginTask.Exception}");
            FirebaseException firebaseException = loginTask.Exception.GetBaseException() as FirebaseException;
            AuthError error = (AuthError)firebaseException.ErrorCode;

            string message = "Failed";
            switch(error)
            {
                case AuthError.MissingEmail: message = "No Email"; break;
                case AuthError.MissingPassword: message = "No Password"; break;
                case AuthError.WrongPassword: message = "Wrong Password"; break;
                case AuthError.InvalidEmail: message = "Wrong Email"; break;
                case AuthError.UserNotFound: message = "User does not exist"; break;
                case AuthError.NetworkRequestFailed: message = "There's no internet"; break;
            }
            NotificationScript.createNotif($"Failed to login: {message}", Color.red);
        }
        else
        {
            _user = loginTask.Result.User;
            NotificationScript.createNotif($"User {_user.DisplayName} Signed in", Color.green);

            PlayerPrefs.SetString("email", email);
            PlayerPrefs.SetString("password", password);

            //Load scene here when the login
        }


    }

    public void RegisterButton()
    {
        StartCoroutine(Register(_emailField.text, _passwordField.text, _usernameField.text));
    }

    private IEnumerator Register(string email, string password, string username)
    {
        if(username == "")
        {
            NotificationScript.createNotif($"Missing Username", Color.red);
        }
        else
        {
            var registerTask = _auth.CreateUserWithEmailAndPasswordAsync(email, password);
            yield return new WaitUntil(predicate: () => registerTask.IsCompleted);

            if(registerTask.Exception != null)
            {
                Debug.Log(message: $"Failed: {registerTask.Exception}");
                FirebaseException firebaseException = registerTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)firebaseException.ErrorCode;

                string message = "Failed";
                switch(errorCode)
                {
                    case AuthError.MissingEmail: message = "Missing Email"; break;
                    case AuthError.MissingPassword: message = "Missing Password"; break;
                    case AuthError.WeakPassword: message = "Weak Password"; break;
                    case AuthError.EmailAlreadyInUse: message = "Email Already in use"; break;
                    case AuthError.NetworkRequestFailed: message = "There's no internet"; break;
                }
                NotificationScript.createNotif($"Failed register: {message}", Color.red);
            }
            else
            {
                _user = registerTask.Result.User;
                if(_user != null)
                {
                    UserProfile profile = new UserProfile();
                    profile.DisplayName = username;

                    var profileTask = _user.UpdateUserProfileAsync(profile);
                    yield return new WaitUntil(predicate: () => profileTask.IsCompleted);

                    if(profileTask.Exception != null)
                    {
                        Debug.Log(message: $"Failed: {profileTask.Exception}");
                        FirebaseException firebaseException = profileTask.Exception.GetBaseException() as FirebaseException;
                        AuthError error = (AuthError)firebaseException.ErrorCode;
                        NotificationScript.createNotif($"Username failed", Color.red);

                    }
                    else
                    {
                        NotificationScript.createNotif($"You Registered!", Color.green);
                        //Login using the data
                        //username set, return to login screen...
                    }
                }
            }
        }
    }
}
