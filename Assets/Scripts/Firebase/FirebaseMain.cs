using System;
using System.Collections.Generic;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Firestore;
using Meta.XR.MRUtilityKit.SceneDecorator;
using TMPro;

public class FirebaseMain : MonoBehaviour
{
    public TMP_InputField emailField;
    public TMP_InputField pwField;
    public GameObject emailCheckBtn;
    public GameObject signUpBtn;

    //private readonly string appID = "854253330667-u7tjog2gp15n2e2h1kc17uohtgv9bpvn.apps.googleusercontent.com";

    private FirebaseApp app;
    public FirebaseAuth auth;


    int invokeNum = 0; //It use count 3minute. 

    /// <summary>
    /// They're allow SignUp or Change PW, Delete account.
    /// </summary>
    private bool checkedEmail = false;
    private bool checkedPassword = false;
    private void Awake()
    {
        DontDestroy();
        InitKeyword();
        InitFirebase();
    }

    #region Init
    /// <summary>
    /// Init this script's property
    /// </summary>
    private void InitKeyword()
    {
        invokeNum = 0;
        checkedEmail = false;
        checkedPassword = false;
    }

    /// <summary>
    /// Check Firebase Dependecies and init
    /// </summary>
    private void InitFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                //Success
                app = Firebase.FirebaseApp.DefaultInstance;
                auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
                Debug.Log("Firebase Dependency check success");
            }
            else
            {
                //Failed
                UnityEngine.Debug.Log(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                //firebase sdk를 사용할 수 없는 상태
                //이후 처리를 코드에 추가해야 한다
            }
        });
    }

    public void InitFindObject()
    {
        emailField = GameObject.Find("EmailField").GetComponent<TMP_InputField>();
        pwField = GameObject.Find("PWField").GetComponent<TMP_InputField>();
        emailCheckBtn = GameObject.Find("EmailCheckBtn");
        signUpBtn = GameObject.Find("SignUpBtn");
    }
    #endregion

    #region LogIn

    /// <summary>
    /// LogIn with your Email to Auth. this fun connected btn.
    /// </summary>
    public void LogIn()
    {
        if (emailField.text != ""
            && pwField.text != ""
            && pwField.text != "@JOH123") //'@JOH123' is using FireStore Fake SignUp's pw
        {
            Debug.Log("Ready to SignIn");
            SignInEmail(emailField.text, pwField.text);
        }
        else if (pwField.text == "@JOH123")
        {
            Debug.Log("You can't select your PW by \"JOH\"");
        }
    }
    /// <summary>
    /// LogIn with your Email to Auth. This fun called other fun.
    /// </summary>
    /// <param name="eTxt"> the string that your login Email
    /// </param>
    /// <param name="pwTxt"> the string that your login PW
    /// </param>
    public void SignEmail(string eTxt, string pwTxt)
    {
        if (eTxt != ""
            && pwTxt != ""
            && pwTxt != "@JOH123")
        {
            SignInEmail(eTxt, pwTxt);
        }
        else
        {
            Debug.Log("text can't be Blank or @JOH123");
        }
    }

    /// <summary>
    /// LogIn to auth. and go to get UID.
    /// </summary>
    /// <param name="eTxt"> string that your login Email
    /// </param>
    /// <param name="pwTxt"> string that your login PW
    /// </param>
    private void SignInEmail(string eTxt, string pwTxt)
    {
        auth.SignInWithEmailAndPasswordAsync(eTxt, pwTxt).
            ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.Log("SingIn Falut");
                }
                else if (task.IsCompleted)
                {
                    Debug.Log("Success to Login");
                    //Get UID to Auth
                    GetUid();
                }
                else
                {
                    Debug.Log("Canceld to Login");
                }
            });
    }

    /// <summary>
    /// Get UID to Auth. this fun connect Auth to FireStore
    /// </summary>
    private void GetUid()
    {
        string authUID = auth.CurrentUser.UserId;
        string email = auth.CurrentUser.Email;
        Debug.Log(authUID);
        LoginFirebase(authUID, email);
    }

    /// <summary>
    /// connect to your FireStore's Doccument by UID
    /// </summary>
    /// <param name="uid"> this is Doccument's name. It's unique for Each account
    /// </param>
    /// <param name="email"> this is Field of Doccument. it can use Email auth.
    /// </param>
    private void LoginFirebase(string uid, string email)
    {
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        DocumentReference docRef = db.Collection("users").Document(uid);
        //docRef is your Doccument.

        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result.Exists) //SignIn to FireStore
            {
                Debug.Log("Login database success");
                //after goto main Scene
            }
            else //IF new Account. You SignUp to FireStore
            {
                Dictionary<string, object> user = new Dictionary<string, object>
                {
                    {"uid", uid},
                    {"email", email},
                    {"nickName", default},
                    {"win", 0},
                    {"lose", 0},
                    {"char", "GoblinMale"}
                };
                docRef.SetAsync(user).ContinueWithOnMainThread(task =>
                {
                    Debug.Log("Make user data to database");
                    //after goto main Scene
                });
            }
        });
    }
    #endregion

    #region SignUp
    /// <summary>
    /// SignUp with your Email to Auth
    /// before you try to SignUp, you need to checked your Email.
    /// </summary>
    public void SignUpUser()
    {
        if (emailField.text != ""
            && pwField.text != ""
            && pwField.text != "@JOH123"  //@JOH123 is Default PW, so you can't use it
            && checkedEmail) //before you try to SignUp, you need to checked your Email.
        {
            Debug.Log("Ready to SignUp");
            SignUpEmail();
            checkedEmail = false;
        }
        else if (pwField.text == "@JOH123")
        {
            Debug.Log("You can't select your PW by \"JOH\"");
        }
    }

    /// <summary>
    /// Create new Account to Auth.
    /// after create, you will be login by auto.
    /// </summary>
    private void SignUpEmail()
    {
        auth.CreateUserWithEmailAndPasswordAsync(emailField.text, pwField.text).
            ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    SignInEmail(emailField.text, pwField.text);
                    //do LogIn with your account that you made
                }
                else
                {
                    Debug.Log("SignUp Faulted");
                }

            });
    }
    #endregion

    #region SignUpFake
    /// <summary>
    /// Make fake account by your email.
    /// It will be use your email auth.
    /// </summary>
    public void SignFakeE()
    {
        if (emailField.text != ""
            && pwField.text != "@JOH123"
            && !checkedEmail)
        {
            Debug.Log("Ready to SignUp");
            SignUpEmailFake();
        }
        else if (pwField.text == "@JOH123")
        {
            Debug.Log("You can't select your PW by \"@JOH123\"");
        }
    }

    /// <summary>
    /// Create fake account and LogIn that account
    /// </summary>
    private void SignUpEmailFake()
    {
        string fakePw = "@JOH123"; //It's fake account's defalut Password. Nobody can't use this PW to other way.
        auth.CreateUserWithEmailAndPasswordAsync(emailField.text, fakePw).
            ContinueWithOnMainThread(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    //LogIn
                    auth.SignInWithEmailAndPasswordAsync(emailField.text, fakePw).
                        ContinueWithOnMainThread(task =>
                        {
                            Debug.Log(auth.CurrentUser);
                            if (task.IsCompletedSuccessfully)
                            {
                                SendEmail();
                            }
                            else
                            {
                                Debug.Log("긴급! Fire Auth에서 아이디 삭제 필요");
                            }
                        });
                }
                else
                {
                    Debug.Log("SignUp Faulted");
                }

            });
    }

    /// <summary>
    /// Send auth mail to your Email. If you Click the Url in this mail, your account is already SignUp. 
    /// </summary>
    private void SendEmail()
    {
        Firebase.Auth.FirebaseUser user = auth.CurrentUser;
        if (user != null)
        {
            if (user.Email != null)
            {
                user.SendEmailVerificationAsync();
                //send the mail to your Email.
                InvokeRepeating("InvokeCheckEmail", 3, 1);
                //Check your email authed by owner. It will be Checked while 3 Minutes;
            }
        }
        else
        {
            Debug.Log("sendE fail");
            user.DeleteAsync();
            //Delete fake ID. If this ID is left Auth, you never made account by same Email.
        }
    }

    /// <summary>
    /// Check your Email is authed by owner.It will be Checked while 3 Minutes
    /// </summary>
    private void InvokeCheckEmail()
    {
        //LogIn fake ID again. If we check CurrentUser's State, we need to init account every time;
        auth.SignInWithEmailAndPasswordAsync(emailField.text, "@JOH123").
            ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    if (auth.CurrentUser.IsEmailVerified) // Success.
                    {
                        auth.CurrentUser.DeleteAsync();
                        checkedEmail = true;  // It means "you are already to SignUp.".
                        Debug.Log("checking success");
                        CancelInvoke(); //Cancel this Invoke.
                        emailCheckBtn.GetComponent<Image>().color = Color.gray;
                        signUpBtn.GetComponent<Image>().color = Color.white;
                    }
                    else
                    {
                        invokeNum++;
                        Debug.Log(invokeNum);
                        if (invokeNum > 177)
                        {
                            CancelInvoke();
                            auth.CurrentUser.DeleteAsync();
                        }
                    }
                }
            });
    }
    #endregion

    #region UpdateData
    /// <summary>
    /// You can change your nickname by this fun.
    /// </summary>
    /// <param name="nick"> Your next nickname.
    /// </param>
    public void UpdateNick(string nick)
    {
        UpdateNickName(nick);
    }

    /// <summary>
    /// Connect to DB and update new data.
    /// </summary>
    /// <param name="nick"></param>
    private void UpdateNickName(string nick)
    {
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        DocumentReference docRef = db.Collection("users").Document(auth.CurrentUser.UserId);
        Dictionary<string, object> updateDic = new Dictionary<string, object>
        {
            {"nick", nick }
        };
        //upadte new data to user's doc.
        docRef.UpdateAsync(updateDic).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log("Update Complete");
            }
            else
            {
                Debug.Log("Plese Update One More");
            }
        });
    }

    /// <summary>
    /// Update user game date to FireStore
    /// </summary>
    /// <param name="nick"> user's Nickname.
    /// </param>
    /// <param name="win"> user's Win Count.
    /// </param>
    /// <param name="lose"> user's Lose Count.
    /// </param>
    /// <param name="charac"> user's Last play character.
    /// </param>
    public void UpdateUserData(int win, int lose, string charac)
    {
        UpdateFirebase(win, lose, charac);
    }

    /// <summary>
    /// Update 4 param to Currentuser's DB
    /// </summary>
    /// <param name="win"></param>
    /// <param name="lose"></param>
    /// <param name="charac"></param>
    private void UpdateFirebase(int win, int lose, string charac)
    {
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        DocumentReference docRef = db.Collection("users").Document(auth.CurrentUser.UserId);
        Dictionary<string, object> updateDic = new Dictionary<string, object>
        {
            {"win", win},
            {"lose", lose},
            {"char", charac}
        };
        //upadte new data to user's doc.
        docRef.UpdateAsync(updateDic).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log("Update Complete");
            }
            else
            {
                Debug.Log("Plese Update One More");
            }
        });
    }
    #endregion

    #region CheckPassword

    /// <summary>
    /// after changePW or Delete account, you must be check PW first.
    /// </summary>
    /// <param name="originPW"> original password of your account.
    /// </param>
    public void CheckPW(string originPW)
    {
        CheckingPW(originPW);
    }

    /// <summary>
    ///  Check your origin PW. If you write collect PW, you can Change new PW.
    /// </summary>
    private void CheckingPW(string originPW)
    {
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        DocumentReference docRef = db.Collection("users").Document(auth.CurrentUser.UserId);

        string myEmail = "";
        //pull your Email to DB.
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            DocumentSnapshot snap = task.Result;
            if ((snap.Exists))
            {
                Dictionary<string, object> snapDic = snap.ToDictionary();
                myEmail = snapDic["email"].ToString();
                //LogIn! If you can success login, your PW is write PW.
                auth.SignInWithEmailAndPasswordAsync(myEmail, originPW).ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompletedSuccessfully)
                    {
                        checkedPassword = true;
                        Debug.Log("success to check PW.");
                    }
                    else
                    {
                        Debug.Log("wrong PW. Please check your email or pw.");
                    }
                });
            }
            else
            {
                Debug.Log("search db data error");
            }
        });
    }
    #endregion

    #region ChangePW

    /// <summary>
    /// Change your acccount's PW. but CheckPW() is move first.
    /// </summary>
    /// <param name="newPW"> PW that your next PW.
    /// </param>
    public void PWChange(string newPW)
    {
        if (checkedPassword)
        {
            ChangePW(newPW);
            checkedPassword = false;
        }
        else
        {
            Debug.Log("Check your PW first");
        }
    }

    /// <summary>
    /// Changing your PW.
    /// </summary>
    /// <param name="newPW"> PW that is your next PW.
    /// </param>
    private void ChangePW(string newPW)
    {
        if (auth.CurrentUser != null)
        {
            auth.CurrentUser.UpdatePasswordAsync(newPW).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("Success to change PW");
                }
                else
                {
                    Debug.Log("Can't change your PW");
                }
            });
        }
        else
        {
            Debug.Log("Please Complete your Login");
        }
    }
    #endregion

    #region DeleteAccount

    /// <summary>
    /// This fun make your account gone.
    /// </summary>
    public void DeleteID()
    {
        if (checkedPassword)
        {
            DeleteAccount();
            checkedPassword = false;
        }
        else
        {
            Debug.Log("Check your PW first");
        }
    }

    /// <summary>
    /// Checked the LogIn.
    /// Kill your data by DB.
    /// Kill your account in Auth.
    /// </summary>
    private void DeleteAccount()
    {
        if (auth.CurrentUser != null)
        {
            FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
            DocumentReference docRef = db.Collection("users").Document(auth.CurrentUser.UserId);

            docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    docRef.DeleteAsync();
                }
            }).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    auth.CurrentUser.DeleteAsync().ContinueWithOnMainThread(task =>
                    {
                        if (task.IsCompletedSuccessfully)
                        {
                            Debug.Log("Success to Delete Account");
                            //Goto Login Scene
                        }
                        else
                        {
                            Debug.Log("Your data is lost but Account still alive");
                        }
                    });
                }
            });
        }
        else
        {
            Debug.Log("Please Complete your Login");
        }
    }
    #endregion

    #region LogOut
    /// <summary>
    /// Log Out Fun.
    /// </summary>
    public void LogOut()
    {
        LogOutGotoLoginScene();
    }

    /// <summary>
    /// Log Out and go LogIn Scene.
    /// </summary>
    private void LogOutGotoLoginScene()
    {
        auth.SignOut();
        //Goto Login Scene
    }
    #endregion

    private void DontDestroy()
    {
        if (GameObject.Find(gameObject.name))
        {
            Destroy(gameObject);
        }
        else 
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}