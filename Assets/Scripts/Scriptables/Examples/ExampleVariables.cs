using ScriptableArchitecture.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExampleVariables : MonoBehaviour
{
    [SerializeField] private IntReference IntReference;
    [SerializeField] private FloatReference FloatReference;
    //[SerializeField] private BoolReference _bool2;
    //[SerializeField private LongReference _lonh1;
    //private Image _image;
    //
    //void Start()
    //{
    //    _image = GetComponent<Image>();
    //}
    //
    //// Update is called once per frame
    //void Update()
    //{
    //    if (_bool1.Value)
    //        _image.color = Color.white;
    //    else
    //        _image.color = Color.red;
    //}
}
