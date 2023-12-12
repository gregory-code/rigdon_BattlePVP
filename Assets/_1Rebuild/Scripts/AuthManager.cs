using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class AuthManager : MonoBehaviour
{
    notifScript NotificationScript;
    loadingScript loading;

    [Header("Firebase Data")]
    public FirebaseScript fireBaseScript;

    [SerializeField] TMP_InputField profileUsernameField;

    //PlayerPref
    private string savedEmail;
    private string savedPassword;
    private int bSaveAutomatically;

    //Background switching
    [SerializeField] private GameObject background;

    //Firebase
    [SerializeField] private DependencyStatus _dependentStatus;
    [SerializeField] private FirebaseAuth auth;
    [SerializeField] private FirebaseUser user;


    //Login
    [SerializeField] TMP_InputField usernameField;
    [SerializeField] TMP_InputField emailField;
    [SerializeField] TMP_InputField passwordField;

    //Checkmark
    [SerializeField] private Image checkmarkImage;


    private void Awake()
    {
        NotificationScript = GameObject.FindGameObjectWithTag("Canvas").GetComponent<notifScript>();

        loading = GameObject.FindObjectOfType<loadingScript>();
        if(loading != null)
            loading.show();

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            _dependentStatus = task.Result;
            if(_dependentStatus == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                fireBaseScript.InitializeDatabase();
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
        getPlayerPrefs();
        StartCoroutine(signInDelay());
    }

    private void getPlayerPrefs()
    {
        savedEmail = PlayerPrefs.GetString("email");
        savedPassword = PlayerPrefs.GetString("password");
        bSaveAutomatically = PlayerPrefs.GetInt("save");

        checkmarkImage.color = (bSaveAutomatically == 1) ? new Color(255, 255, 255, 255) : new Color(0, 0, 0, 0);
    }

    private IEnumerator signInDelay()
    {
        yield return new WaitForSeconds(1);
 
        if (bSaveAutomatically == 1 && savedEmail != "" && savedPassword != "")
        {
            StartCoroutine(Login(savedEmail, savedPassword));
        }
        else
        {
            loading.hide();
        }
    }

    public void loginAutoToggle()
    {
        bSaveAutomatically = (bSaveAutomatically == 0) ? 1 : 0;
        checkmarkImage.color = (bSaveAutomatically == 1) ? new Color(255, 255, 255, 255) : new Color(0, 0, 0, 0);
        PlayerPrefs.SetInt("save", bSaveAutomatically);
    }

    public void LoginButton()
    {
        StartCoroutine(Login(emailField.text, passwordField.text));
    }

    private IEnumerator Login(string email, string password)
    {
        var loginTask = auth.SignInWithEmailAndPasswordAsync(email, password);

        loading.show();

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

            loading.hide();
        }
        else
        {
            user = loginTask.Result.User;
            if(user != null) fireBaseScript.GetUser(user);

            NotificationScript.createNotif($"User {user.DisplayName} Signed in", Color.green);

            PlayerPrefs.SetString("email", email);
            PlayerPrefs.SetString("password", password);

            fireBaseScript.LoadCloudData();
        }
    }

    public void RegisterButton()
    {
        StartCoroutine(Register(emailField.text, passwordField.text, usernameField.text));
    }

    private IEnumerator Register(string email, string password, string username)
    {
        if(username == "")
        {
            NotificationScript.createNotif($"Missing Username", Color.red);
        }
        else
        {
            var registerTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);

            loading.show();

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
                loading.hide();
            }
            else
            {
                user = registerTask.Result.User;
                if(user != null)
                {
                    fireBaseScript.GetUser(user);

                    UserProfile profile = new UserProfile();
                    profile.DisplayName = username;

                    var profileTask = user.UpdateUserProfileAsync(profile);
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
                        profileUsernameField.text = username;
                        StartCoroutine(fireBaseScript.UpdateUsernameAuth(username));
                        StartCoroutine(fireBaseScript.UpdateObject("username", username));
                        //GameObject.FindGameObjectWithTag("Online").GetComponent<onlineScript>().setNickName(username);
                        //GameObject.FindGameObjectWithTag("Canvas").transform.Find("menu").GetComponent<menuScript>().UsernameField.text = username;
                        StartCoroutine(Login(email, password));
                    }
                }
            }
        }
    }

    public void SignOut()
    {
        auth.SignOut();
        usernameField.text = "";
        emailField.text = "";
        passwordField.text = "";

        bSaveAutomatically = 0;
        PlayerPrefs.SetInt("save", bSaveAutomatically);

        SceneManager.LoadScene(0);
    }
}
