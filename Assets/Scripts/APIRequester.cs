using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;


namespace EVgo
{


    public class APIRequester : MonoBehaviour
    {
        //public delegate void OnThrustChanged(float amount);
        public static event UnityAction<string> OnLocationDataLoad = delegate { };

        private const string baseUrl = "https://api-demo.evconnect.com/rest/v5/";
        private const string accept = "application/json";
        private const string appToken = "eyJhbGciOiJIUzUxMiJ9.eyJzdWIiOiJkMTZlODlmMy1mNGU5LTQ2NTMtYmMzNy1jYzI1NjY2NjUzMGIiLCJ1c2VyUm9sZSI6IkFETUlOIn0.OKsshezl_EkmsWrsdni5sK0GCKyuP2vCKlQWK5aBnZ9AKATHXo7NVJLLjSjEP1fV4L4W9-pnUrpJwGaZCLTknw";

        public string section = "locations/";
        public Text responseText;
        public InputField responseOutput;
        public string specifics = "mapping?latitude=33.923&longitude=-118.388&distance=10&metric=MILES";


        public void Request()
        {
            WWWForm form = new WWWForm();
            Dictionary<string, string> headers = form.headers;
            headers["accept"] = accept;
            headers["EVC-API-TOKEN"] = appToken;

            WWW request = new WWW(baseUrl + section + specifics, null, headers);
            StartCoroutine(OnResponse(request));
        }

        private IEnumerator OnResponse(WWW wwwResponse)
        {
            yield return wwwResponse;

            responseText.text = wwwResponse.text;
            responseOutput.text = wwwResponse.text;

            if (OnLocationDataLoad != null) OnLocationDataLoad(wwwResponse.text);
            // OnResponseChanged();  
        }


    }
}
