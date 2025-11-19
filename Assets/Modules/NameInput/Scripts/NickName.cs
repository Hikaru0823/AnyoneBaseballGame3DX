using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NickName : MonoBehaviour
{
    private static string value;
    private string[] init_Names = new string[] {"Tiger", "Lion", "Elephant", "Giraffe", "Panda", "Kangaroo", "Koala", "Dolphin", "Whale", "Shark", "Eagle", "Penguin", "Polar Bear", "Grizzly Bear", "Wolf", "Fox", "Rabbit", "Deer", "Horse", "Cow", "Pig", "Sheep", "Goat", "Chicken", "Dog", "Cat", "Cheetah", "Leopard", "Gorilla", "Chimpanzee"};

    // Start is called before the first frame update
    void Start()
    {
        value = ES3.Load<string>("UserName", defaultValue:"");
        string init_name = init_Names[Random.Range(0, init_Names.Length)];
        if(value == "" || value == null)   
            value = init_name;
        ES3.Save<string>("UserName", value);
        FindFirstObjectByType<NameInput>().Init(value);
    }

    public void Set_Value(string name)
    {
        value = name;
    }

    //    public static string Get_Value()
    //    {
    //        string[] init_names = new string[] { "Lion", "Zebra", "Horse", "Tiger" };
    //        string init_name = init_names[Random.Range(0, init_names.Length)];
    //        return (value is string _value) ? _value : SaveGame.Load<string>(Key, init_name);
    //    }
}
