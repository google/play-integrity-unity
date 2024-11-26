// Copyright 2024 Google LLC
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

using System;
using Google.Play.Common;
using Google.Play.Core.Internal;
using UnityEngine;

namespace Google.Play.Integrity.Internal
{
    internal class PlayCoreIntegrityTokenResponse : IDisposable
    {
        private readonly AndroidJavaObject _javaIntegrityTokenResponse;

        internal PlayCoreIntegrityTokenResponse(AndroidJavaObject javaIntegrityTokenResponse)
        {
            _javaIntegrityTokenResponse = javaIntegrityTokenResponse;
        }

        internal PlayServicesTask<int> ShowDialog(int typeCode)
        {
            if (_javaIntegrityTokenResponse == null)
            {
                throw new NullReferenceException("ShowDialog called with a null IntegrityTokenResponse.");
            }

            AndroidJavaObject javaTask;
            using (var activity = UnityPlayerHelper.GetCurrentActivity())
            {
                javaTask =
                    _javaIntegrityTokenResponse.Call<AndroidJavaObject>("showDialog", activity, typeCode);
            }
            return new PlayServicesTask<int>(javaTask);
        }

        public void Dispose()
        {
            _javaIntegrityTokenResponse.Dispose();
        }
    }
}