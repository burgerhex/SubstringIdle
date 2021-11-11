using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject cubePrefab;
    public Text bufferText;
    public Text substringText;

    public bool debug = true;
    
    // private const int BlocksPerRow = 10;
    private const float MinX = 10;
    private const float MaxX = 100;
    private const float MinZ = 10;
    // private const float MinY = 10;
    private const int RowSpacing = 5;

    private string _buffer = "";
    private string _currentSubstr = "";

    private Transform _blocksParent;

    // TODO: upgrade in shop
    private int _minWordLength = 1;
    private int _maxWordLength = 20;
    private readonly WordManager _dict = new WordManager();

    private readonly LinkedList<BlockController> _blocks = new LinkedList<BlockController>();

    // TODO: settings: keyboard repeat delay and speed, space as termination
    // TODO: upgrade - shows you if word is real or not before entering

    private void Start()
    {
        GameObject blocksParentObject = new GameObject("Blocks Parent");
        _blocksParent = blocksParentObject.transform;
        _currentSubstr = _dict.NextSubstring();
        substringText.text = "Substring: " + _currentSubstr;
    }

    private void Update()
    {
        bool changed = false;
        // get every letter with with GetKeyDown, to prevent repeats from keyboard
        for (KeyCode key = KeyCode.A; key <= KeyCode.Z; key++)
        {
            if (_buffer.Length < _maxWordLength && Input.GetKeyDown(key))
            {
                _buffer += key.ToString().ToLower();
                changed = true;
                
                if (debug)
                    Debug.Log("typed " + key);
            }
        }


        // get enter with GetKeyDown to prevent repeats
        // TODO: add space as an option
        if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)) && 
            _buffer.Length >= _minWordLength)
        {
            if (!_buffer.Contains(_currentSubstr))
            {
                if (debug)
                    Debug.Log(_buffer + " doesn't contain substring " + _currentSubstr);
            }
            else if (!_dict.WordExists(_buffer))
            {
                if (debug)
                    Debug.Log(_buffer + " is not a real word");
            }
            else
            {
                SpawnBlock();
                _currentSubstr = _dict.NextSubstring();
                substringText.text = "Substring: " + _currentSubstr;
            }

            changed = true;
            _buffer = "";
        }
        
        // get backspace with regular GetKey, but TODO: add a timer to implement repeats
        if (Input.GetKeyDown(KeyCode.Backspace) && _buffer.Length > 0)
        {
            _buffer = _buffer.Remove(_buffer.Length - 1);
            changed = true;
        }

        if (changed)
        {
            string newText = _buffer.Replace(_currentSubstr, 
                "<color=green>" + _currentSubstr + "</color>");
            bufferText.text = newText;
        }
    }

    private void SpawnBlock()
    {
        // TODO: merging
        // TODO: crit chance in shop
        
        GameObject newChild = Instantiate(cubePrefab, _blocksParent);
        BlockController block = newChild.GetComponent<BlockController>();
        _blocks.AddLast(block);

        LinkedListNode<BlockController> oldLast = _blocks.Last.Previous;
        float newX, newZ;

        if (oldLast == null)
        {
            newX = MinX;
            newZ = MinZ;
            if (debug)
                Debug.Log("placing first block at (" + newX + ", " + newZ + ")");
        }
        else
        {
            Vector3 pos = oldLast.Value.transform.position;
            newX = pos.x + RowSpacing;
            newZ = pos.z;

            if (newX > MaxX)
            {
                newX = MinX;
                newZ += RowSpacing;
            }
            
            if (debug)
                Debug.Log("placing another block at (" + newX + ", " + newZ + ")");
        }

        block.MoveTo(newX, newZ);
        
        while (Merge()) { }
    }

    // returns true if merge happened
    private bool Merge()
    { 
        BlockController last = _blocks.Last?.Value;
        BlockController secondLast = _blocks.Last?.Previous?.Value;

        if (last == null || secondLast == null ||
            Math.Abs(last.GetHeight() - secondLast.GetHeight()) >= float.Epsilon)
        {
            if (debug)
                Debug.Log("stopping merge");
            return false;
        }
        
        _blocks.RemoveLast();
        Destroy(last.gameObject);
        secondLast.DoubleHeight();

        if (debug)
            Debug.Log("merged to new height " + secondLast.transform.localScale.y);
        
        return true;
    }
}
