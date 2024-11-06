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

using System;
using System.Security.Cryptography;
using UnityEngine;

namespace Google.Play.Integrity.Samples.IntegrityTestApp
{
    /// <summary>
    /// Provides helper functions for generating nonces and decoding tokens.
    /// </summary>
    internal class FakeIntegrityVerifierServer
    {
        /// <summary>
        /// Generates the nonce as a random byte array of length numBytes, returns the Base64 representation of this
        /// byte array.
        /// </summary>
        public static string GenerateNonce(int numBytes)
        {
            var byteArray = new byte[numBytes];
            var rngProvider = new RNGCryptoServiceProvider();
            rngProvider.GetBytes(byteArray);
            var nonce = Convert.ToBase64String(byteArray);
            // Convert to URL-safe encoding
            nonce = nonce.Replace("+", "-").Replace("/", "_");
            return nonce;
        }

        /// <summary>
        /// Returns whether the provided token is a valid Base64 encoded JWE token (five Base64 encoded chunks,
        /// separated by period characters).
        /// </summary>
        public static bool IsValidToken(string base64Token)
        {
            var base64Segments = base64Token.Split('.');
            if (base64Segments.Length != 5)
            {
                Debug.LogErrorFormat("Expected token to have 5 segments, actual segment count is: {0}",
                    base64Segments.Length);
                return false;
            }

            for (var i = 0; i < base64Segments.Length; i++)
            {
                var segment = base64Segments[i];

                if (string.IsNullOrEmpty(segment))
                {
                    Debug.LogErrorFormat("Base64 string at index {0} is empty.", i);
                    return false;
                }

                // Convert from URL-safe encoding
                segment = segment.Replace("-", "+").Replace("_", "/");
                // Append paddings
                switch (segment.Length % 4)
                {
                    case 2:
                        segment += "==";
                        break;
                    case 3:
                        segment += "=";
                        break;
                }

                try
                {
                    Convert.FromBase64String(segment);
                }
                catch (FormatException e)
                {
                    Debug.LogErrorFormat("Unable to decode Base64 string at index {0}: {1}", i, e);
                    Debug.LogErrorFormat("Undecodable string = {0}", segment);
                    return false;
                }
            }

            return true;
        }
    }
}