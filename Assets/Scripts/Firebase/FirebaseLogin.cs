using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Google;
using UnityEngine.UI;
using Firebase.Firestore;
using UnityEngine.Tilemaps;

public class FirebaseLogin : MonoBehaviour
{
    public InputField emailField;
    public InputField pwField;
    public InputField newPWField;

    private readonly string appID = "854253330667-6qtqvqnu1he8n12ujsh867dun9b24tn0.apps.googleusercontent.com";

    private FirebaseApp app;
    private FirebaseAuth auth;

    void Awake()
    {
        InitFirebase();
    }

    #region Sign
    public void SignEmail()
    {
        SignInEmail();
    }

    private void SignInEmail()
    {
        auth.SignInWithEmailAndPasswordAsync(emailField.text, pwField.text).
            ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.Log("SingIn Falut");
                }
                else if (task.IsCompleted)
                {
                    Debug.Log("Success to Login");
                    GetUid();
                }
                else
                {
                    Debug.Log("Canceld to Login");
                }
            });
    }

    private void SignUpEmail()
    {
        auth.CreateUserWithEmailAndPasswordAsync(emailField.text, pwField.text).
            ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    SignInEmail();
                }
                else
                {
                    Debug.Log("SignUp Faulted");
                }

            });
    }


    private void SignUpEmailFake()
    {
        string fakePw = "JOH";
        auth.CreateUserWithEmailAndPasswordAsync(emailField.text, fakePw).
            ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {

                    auth.SignInWithEmailAndPasswordAsync(emailField.text, fakePw).
                        ContinueWithOnMainThread(task =>
                        {
                            if (task.IsCompleted)
                            {
                                SendEmail();
                            }
                            else
                            {
                                //유저 지우기
                                //auth.CurrentUser.DeleteAsync();
                            }
                        });
                }
                else
                {
                    Debug.Log("SignUp Faulted");
                }

            });
    }

    private void SendEmail()
    {
        Firebase.Auth.FirebaseUser user = auth.CurrentUser;
        if (user != null)
        {
            if (user.Email != null)
            {
                user.SendEmailVerificationAsync();
            }
        }
        else
        {
            Debug.Log("sendE fail");
        }
    }

    public void CheckEmail()
    {
        CheckEmailAuth();
    }

    private void CheckEmailAuth()
    {
        auth.SignOut();
    }

    private void GetUid()
    {
        string authUID = auth.CurrentUser.UserId;
        string email = auth.CurrentUser.Email;
        Debug.Log(authUID);
        LoginFirebase(authUID, email);
    }

    private void LoginFirebase(string uid, string email)
    {
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        DocumentReference docRef = db.Collection("users").Document(uid);
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result.Exists) //SignIn to FireStore
            {
                Debug.Log("Login database success");
                //goto main Scene
            }
            else //SignUp to FireStore
            {
                Dictionary<string, object> user = new Dictionary<string, object>
                {
                    {"uid", uid},
                    {"email", email},
                    {"nickName", default},
                    {"win", 0}
                };
                docRef.SetAsync(user).ContinueWithOnMainThread(task =>
                {
                    Debug.Log("Make user data to database");
                    //goto main Scene
                });
            }
        });
    }
    #endregion

    #region UpdateData
    public void UpdateUserData(string nick, int win)
    {
        UpdateFirebase(nick, win);
    }

    private void UpdateFirebase(string nick, int win)
    {
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        DocumentReference docRef = db.Collection("users").Document(auth.CurrentUser.UserId);
        Dictionary<string, object> updateDic = new Dictionary<string, object>
        {
            {"nickName", nick},
            {"win", win}
        };
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

    #region ChangePW
    public void PWChange(string newPW)
    {
        ChangePW(newPW);
    }
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
    public void DeleteID()
    {
        DeleteAccount();
    }
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
    public void LogOut()
    {
        LogOutGotoLoginScene();
    }

    private void LogOutGotoLoginScene()
    {
        auth.SignOut();
        //Goto Login Scene
    }
    #endregion

    void InitFirebase() //Firebase 의존성 확인 및 초기화
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                //성공처리
                app = Firebase.FirebaseApp.DefaultInstance;
                auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
                Debug.Log("Firebase Dependency check success");
            }
            else
            {
                UnityEngine.Debug.Log(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                //실패 처리
                //firebase sdk를 사용할 수 없는 상태
                //이후 처리를 코드에 추가해야 한다
            }
        });
    }

    public void SignUpUser()
    {
        if (emailField.text != ""
            && pwField.text != ""
            && pwField.text != "JOH") //JOH is using FireStore Fake SignUp's pw
        {
            Debug.Log("Ready to SignUp");
            SignUpEmail();
        }
        else if (pwField.text == "JOH")
        {
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.Return))
            {
                Debug.Log("You can't select your PW by \"JOH\"");
            }
        }
    }
    public void LogIn()
    {
        if (emailField.text != ""
            && pwField.text != ""
            && pwField.text != "JOH") //JOH is using FireStore Fake SignUp's pw
        {
            Debug.Log("Ready to SignIn");
            SignEmail();
        }
        else if (pwField.text == "JOH")
        {
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.Return))
            {
                Debug.Log("You can't select your PW by \"JOH\"");
            }
        }
    }
    public void Delete()
    {
        Debug.Log("Go to delete");
        DeleteID();
    }

    public void ChangeP()
    {
        Debug.Log("Go to change");
        ChangePW(pwField.text);
    }
}