// <copyright file="SigninSampleScript.cs" company="Google Inc.">
// Copyright (C) 2017 Google Inc. All Rights Reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations

namespace SignInSample
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Google;
    using UnityEngine;
    using UnityEngine.UI;

    public class SigninSampleScript : MonoBehaviour
    {
        public Text statusText;

        [SerializeField]private string webClientId;

        private GoogleSignInConfiguration configuration;

        // Defer the configuration creation until Awake so the web Client ID
        // Can be set via the property inspector in the Editor.
        void Awake()
        {
            configuration = new GoogleSignInConfiguration
            {
                WebClientId = webClientId,
                UseGameSignIn = false,
                RequestIdToken = true,
                RequestAuthCode = true,
            };
        }

        public void OnSignIn()
        {
            GoogleSignIn.Configuration = configuration;
            AddStatusText("Calling SignIn");

            GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnAuthenticationFinished);
        }

        public void OnSignOut()
        {
            AddStatusText("Calling SignOut");
            GoogleSignIn.DefaultInstance.SignOut();
        }

        public void OnDisconnect()
        {
            AddStatusText("Calling Disconnect");
            GoogleSignIn.DefaultInstance.Disconnect();
        }

        internal void OnAuthenticationFinished(Task<GoogleSignInUser> task)
        {
            if (task.IsFaulted)
            {
                using var enumerator = task.Exception!.InnerExceptions.GetEnumerator();
                if (enumerator.MoveNext())
                {
                    var error = (GoogleSignIn.SignInException)enumerator.Current;
                    AddStatusText($"Got Error: {error!.Status}\n{error.Message}\n{error.StackTrace}\n{error.InnerException}");
                }
                else
                {
                    AddStatusText("Got Unexpected Exception?!?" + task.Exception);
                }
            }
            else if (task.IsCanceled)
            {
                AddStatusText("Canceled");
            }
            else
            {
                var sb = new System.Text.StringBuilder("Welcome: ");

                var displayName = string.IsNullOrEmpty(task.Result.DisplayName) ? "DisplayNameIsNull" : task.Result.DisplayName;
                var email = string.IsNullOrEmpty(task.Result.Email) ? "EmailIsNull" : task.Result.Email;
                var userId = string.IsNullOrEmpty(task.Result.UserId) ? "UserIdIsNull" : task.Result.UserId;
                var idToken = string.IsNullOrEmpty(task.Result.IdToken) ? "IdTokenIsNull" : task.Result.IdToken;
                var authCode = string.IsNullOrEmpty(task.Result.AuthCode) ? "AuthCodeIsNull" : task.Result.AuthCode;
                
                sb.AppendLine($"{displayName} ({email})\n");
                sb.AppendLine($"UserId: {userId}\n");
                sb.AppendLine($"IdToken: {idToken}\n");
                sb.AppendLine($"AuthCode: {authCode}\n");

                AddStatusText(sb.ToString());
            }
        }

        public void OnSignInSilently()
        {
            GoogleSignIn.Configuration = configuration;
            GoogleSignIn.Configuration.UseGameSignIn = false;
            GoogleSignIn.Configuration.RequestIdToken = true;
            AddStatusText("Calling SignIn Silently");

            GoogleSignIn.DefaultInstance.SignInSilently()
                .ContinueWith(OnAuthenticationFinished);
        }


        public void OnGamesSignIn()
        {
            GoogleSignIn.Configuration = configuration;
            GoogleSignIn.Configuration.UseGameSignIn = true;
            GoogleSignIn.Configuration.RequestIdToken = false;

            AddStatusText("Calling Games SignIn");

            GoogleSignIn.DefaultInstance.SignIn().ContinueWith(
                OnAuthenticationFinished);
        }

        private List<string> messages = new List<string>();

        void AddStatusText(string text)
        {
            Debug.Log(text);
            if (messages.Count == 5)
            {
                messages.RemoveAt(0);
            }

            messages.Add(text);
            string txt = "";
            foreach (string s in messages)
            {
                txt += "\n" + s;
            }

            statusText.text = txt;
        }
    }
}