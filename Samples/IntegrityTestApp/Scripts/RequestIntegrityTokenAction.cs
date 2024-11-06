// Copyright 2021 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections;
using Google.Play.Common;
using UnityEngine;
using UnityEngine.UI;

namespace Google.Play.Integrity.Samples.IntegrityTestApp
{
    /// <summary>
    /// Provides controls and status displays for requesting integrity tokens via Integrity API.
    /// </summary>
    public class RequestIntegrityTokenAction : MonoBehaviour
    {
        public Text integrityApiStatusText;
        public Button requestIntegrityTokenButton;
        public Button requestStandardIntegrityTokenButton;

        private IntegrityManager _integrityManager;
        private StandardIntegrityManager _standardIntegrityManager;

        void Start()
        {
            requestIntegrityTokenButton.onClick.AddListener(ButtonRequestIntegrityToken);
            requestStandardIntegrityTokenButton.onClick.AddListener(ButtonRequestStandardIntegrityToken);
            _integrityManager = new IntegrityManager();
            _standardIntegrityManager = new StandardIntegrityManager();
            Debug.Log("Integrity test app started");
        }

        private void Update()
        {
            // Provides an interface to test via key press.
            if (Input.GetKeyDown(KeyCode.A))
            {
                StartCoroutine(RequestIntegrityTokenCo());
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                StartCoroutine(RequestStandardIntegrityTokenCo());
            }
        }

        private void AppendStatusLog(string statusLog)
        {
            Debug.Log(statusLog);
            integrityApiStatusText.text += statusLog + "\n";
        }

        private void ButtonRequestIntegrityToken()
        {
            AppendStatusLog("Start RequestIntegrityToken flow");
            StartCoroutine(RequestIntegrityTokenCo());
        }

        private void ButtonRequestStandardIntegrityToken()
        {
            AppendStatusLog("Start RequestStandardIntegrityToken flow");
            StartCoroutine(RequestStandardIntegrityTokenCo());
        }

        IEnumerator RequestIntegrityTokenCo()
        {
            var nonce = FakeIntegrityVerifierServer.GenerateNonce(42);
            AppendStatusLog("Nonce = " + nonce);
            var tokenRequest = new IntegrityTokenRequest(nonce);
            var requestIntegrityTokenOperation =
                _integrityManager.RequestIntegrityToken(tokenRequest);

            yield return requestIntegrityTokenOperation;

            if (requestIntegrityTokenOperation.Error != IntegrityErrorCode.NoError)
            {
                AppendStatusLog("IntegrityAsyncOperation failed with error: " + requestIntegrityTokenOperation.Error);
                yield break;
            }

            var tokenResponse = requestIntegrityTokenOperation.GetResult();
            var jwtTokenBase64 = tokenResponse.Token;

            if (jwtTokenBase64 == null)
            {
                AppendStatusLog("IntegrityAsyncOperation succeeded, but token is null.");
                yield break;
            }

            AppendStatusLog("IntegrityAsyncOperation succeeded with token: " + jwtTokenBase64);

            var tokenIsValid = FakeIntegrityVerifierServer.IsValidToken(jwtTokenBase64);
            AppendStatusLog(tokenIsValid ? "Token is a valid JWE." : "Token is invalid.");
        }

        IEnumerator RequestStandardIntegrityTokenCo()
        {
            long cloudProjectNumber = 0; // Add cloud project number here.
            AppendStatusLog("CloudProjectNumber = " + cloudProjectNumber);
            var prepareIntegrityTokenRequest = new PrepareIntegrityTokenRequest(cloudProjectNumber);
            var prepareIntegrityTokenOperation =
                _standardIntegrityManager.PrepareIntegrityToken(prepareIntegrityTokenRequest);

            yield return prepareIntegrityTokenOperation;

            if (prepareIntegrityTokenOperation.Error != StandardIntegrityErrorCode.NoError)
            {
                AppendStatusLog("PrepareIntegrityTokenAsyncOperation failed with error: " +
                                prepareIntegrityTokenOperation.Error);
                yield break;
            }

            var tokenProvider = prepareIntegrityTokenOperation.GetResult();
            var standardIntegrityTokenRequest = new StandardIntegrityTokenRequest();
            var standardIntegrityTokenOperation = tokenProvider.Request(standardIntegrityTokenRequest);

            yield return standardIntegrityTokenOperation;

            if (standardIntegrityTokenOperation.Error != StandardIntegrityErrorCode.NoError)
            {
                AppendStatusLog("StandardIntegrityTokenAsyncOperation failed with error: " +
                                standardIntegrityTokenOperation.Error);
                yield break;
            }

            var tokenResponse = standardIntegrityTokenOperation.GetResult();
            var jwtTokenBase64 = tokenResponse.Token;

            if (jwtTokenBase64 == null)
            {
                AppendStatusLog("StandardIntegrityAsyncOperation succeeded, but token is null.");
                yield break;
            }

            AppendStatusLog("StandardIntegrityAsyncOperation succeeded with token: " + jwtTokenBase64);
        }
    }
}