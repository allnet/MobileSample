using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;


public class JsonTestParser : MonoBehaviour
{
    //Serializes and deserializes JSON

    public enum CharacterType
    {

        oge = 10,
        human = 20,
        elf = 40

    }

    public class PlayerInfo
    {
        public string name { get; set; }
        public int age { get; set; }

        public CharacterType characterType { get; set; }
    }

    // Use this for initialization
    void Start()
    {


        Test();
    }


    void Test()
    {
        var testPlayer = new PlayerInfo() { name = "Darryl", age = 25, characterType = CharacterType.elf };
        var jsonString = JsonConvert.SerializeObject(testPlayer, Formatting.None);  // convert to json
        Debug.Log(jsonString);

        testPlayer = JsonConvert.DeserializeObject<PlayerInfo>(jsonString);     // convert into object  
        testPlayer.name = "Darryl-changed";
  
        var changedJson = JsonConvert.SerializeObject(testPlayer, Formatting.None);
        JsonConvert.PopulateObject(changedJson, testPlayer);

        Debug.Log("Original  = " + jsonString);
        Debug.Log("Changed = " + changedJson + testPlayer.ToString());
    }

}
