using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject cubePrefab;
    public Text bufferText;
    public Text substringText;
    public Text scoreText;
    public Text progressText;
    public Text comboText;

    public bool debug = true;

    public float blockSpawnTime = 1;
    public float blockMergeTime = 2;
    public MergeAnimation mergeAnimation = MergeAnimation.Linear;
    
    // private const int BlocksPerRow = 10;
    private const float MinX = 10;
    private const float MaxX = 40;
    private const float MinZ = 10;
    // private const float MinY = 10;
    private const int RowSpacing = 10;

    private string _buffer = "";
    private string _currentSubstr = "";
    private int _score;
    private int _combo;
    private readonly HashSet<string> _wordsGotten = new HashSet<string>();

    private Transform _blocksParent;

    // TODO: upgrade in shop
    private int _minWordLength = 1;
    private int _maxWordLength = 20;
    private int _wordsNeeded = 5;
    private readonly WordManager _dict = new WordManager();

    private readonly LinkedList<BlockController> _blocks = new LinkedList<BlockController>();

    private enum Animation
    {
        Spawn, Merge
    }

    public enum MergeAnimation
    {
        Linear, Parabolic, Elliptical, Bezier
    }

    private readonly LinkedList<Animation> _animationQueue = new LinkedList<Animation>();
    private bool _currentlyAnimating;

    // TODO: settings: keyboard repeat delay and speed, space as termination
    // TODO: upgrade - shows you if word is real or not before entering
    // TODO: skips - can buy or earn

    private void Start()
    {
        GameObject blocksParentObject = new GameObject("Blocks Parent");
        _blocksParent = blocksParentObject.transform;
        _currentSubstr = _dict.NextSubstring();
        UpdateTexts();
        bufferText.text = "";
    }

    private void Update()
    {
        DoNextAnimation(false);
        
        bool changed = false;
        // get every letter with with GetKeyDown, to prevent repeats from keyboard
        for (KeyCode key = KeyCode.A; key <= KeyCode.Z; key++)
        {
            if (_buffer.Length < _maxWordLength && Input.GetKeyDown(key))
            {
                _buffer += key.ToString().ToLower();
                changed = true;
                
                // if (debug)
                //     Debug.Log("typed " + key);
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
            else if (_wordsGotten.Contains(_buffer))
            {
                if (debug)
                    Debug.Log(_buffer + " has already been used");
            }
            else
            {
                if (debug)
                    Debug.Log(_buffer + " is a real word, success");
                // successful hit
                // SpawnBlock();
                // StartCoroutine(SpawnBlockAnimated(2));
                EnqueueSpawnAnimation();
                _score++;
                _combo++;
                _wordsGotten.Add(_buffer);

                if (_wordsGotten.Count == _wordsNeeded)
                {
                    _wordsGotten.Clear();
                    _currentSubstr = _dict.NextSubstring();
                }
                
                UpdateTexts();
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

    private void DoNextAnimation(bool isContinued)
    {
        if (!isContinued && _currentlyAnimating) return;
        if (_animationQueue.Count == 0)
        {
            _combo = 0;
            UpdateTexts();
            _currentlyAnimating = false;
            return;
        }

        _currentlyAnimating = true;
        Animation next = _animationQueue.First.Value;
        _animationQueue.RemoveFirst();
        Debug.Log("popping, now: " + AnimationQueueString());

        switch (next)
        {
            case Animation.Spawn:
                StartCoroutine(SpawnBlockAnimated(blockSpawnTime));
                break;
            case Animation.Merge:
                StartCoroutine(AnimatedMerge(blockMergeTime));
                break;
            default:
                Debug.Log("unknown animation in queue, value " + next);
                break;
        }
    }

    private string AnimationQueueString()
    {
        if (_animationQueue.Count == 0) return "{}";
        
        string s = "{";
            
        for (LinkedListNode<Animation> n = _animationQueue.First; n != _animationQueue.Last; n = n?.Next)
        {
            s += n?.Value + ", ";
        }

        return s + _animationQueue.Last?.Value + "}";
    }

    private void EnqueueSpawnAnimation()
    {
        _animationQueue.AddLast(Animation.Spawn);
        Debug.Log("adding spawn, now: " + AnimationQueueString());
        // there will be as many merges for as many 1's that the binary
        // representation of the current score ends with
        int tempScore = _score;
        while ((tempScore & 1) == 1)
        {
            _animationQueue.AddLast(Animation.Merge);
            Debug.Log("adding merge, now: " + AnimationQueueString());
            tempScore >>= 1;
        }

        // if (!_currentlyAnimating)
        // {
        //     DoNextAnimation();
        // }
    }

    private void UpdateTexts()
    {
        substringText.text = "Substring: " + _currentSubstr;
        scoreText.text = "Score: " + _score;
        progressText.text = "Words: " + _wordsGotten.Count + " / " + _wordsNeeded;
        comboText.text = _combo + "x combo";
    }
    
    // private void SpawnBlock()
    // {
    //     // TODO: merging
    //     // TODO: crit chance in shop
    //     
    //     GameObject newChild = Instantiate(cubePrefab, _blocksParent);
    //     BlockController block = newChild.GetComponent<BlockController>();
    //     _blocks.AddLast(block);
    //     _score++;
    //
    //     LinkedListNode<BlockController> oldLast = _blocks.Last.Previous;
    //     float newX, newZ;
    //
    //     if (oldLast == null)
    //     {
    //         newX = MinX;
    //         newZ = MinZ;
    //         if (debug)
    //             Debug.Log("placing first block at (" + newX + ", " + newZ + ")");
    //     }
    //     else
    //     {
    //         Vector3 pos = oldLast.Value.transform.position;
    //         newX = pos.x + RowSpacing;
    //         newZ = pos.z;
    //
    //         if (newX > MaxX)
    //         {
    //             newX = MinX;
    //             newZ += RowSpacing;
    //         }
    //         
    //         if (debug)
    //             Debug.Log("placing another block at (" + newX + ", " + newZ + ")");
    //     }
    //
    //     block.MoveTo(newX, newZ);
    //     
    //     while (Merge()) { }
    // }
    //
    // // returns true if merge happened
    // private bool Merge()
    // { 
    //     BlockController last = _blocks.Last?.Value;
    //     BlockController secondLast = _blocks.Last?.Previous?.Value;
    //
    //     if (last == null || secondLast == null ||
    //         Math.Abs(last.GetHeight() - secondLast.GetHeight()) >= float.Epsilon)
    //     {
    //         if (debug)
    //             Debug.Log("stopping merge");
    //         return false;
    //     }
    //     
    //     _blocks.RemoveLast();
    //     Destroy(last.gameObject);
    //     secondLast.DoubleHeight();
    //
    //     if (debug)
    //         Debug.Log("merged to new height " + secondLast.transform.localScale.y);
    //     
    //     return true;
    // }

    private IEnumerator SpawnBlockAnimated(float animationTime)
    {
        GameObject newChild = Instantiate(cubePrefab, _blocksParent);
        BlockController block = newChild.GetComponent<BlockController>();
        _blocks.AddLast(block);
        // _score++;

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
        
        Transform t = block.transform;
        float maxHeight = t.localScale.y;

        float timer = 0;
        while (timer < animationTime)
        {
            float p = timer / animationTime;
            float p2 = p * (2 - p);
            Vector3 s = t.localScale;
            Vector3 pos = t.position;
            s = new Vector3(s.x, p2 * maxHeight, s.z);
            pos = new Vector3(pos.x, p2 * maxHeight / 2, pos.z);
            t.localScale = s;
            t.position = pos;
            timer += Time.deltaTime;
            yield return null;
        }

        Vector3 sf = t.localScale;
        sf = new Vector3(sf.x, maxHeight, sf.z);
        t.localScale = sf;

        DoNextAnimation(true);
        // StartCoroutine(AnimatedMerge(4));
    }

    private IEnumerator AnimatedMerge(float animationTime)
    {
        BlockController last = _blocks.Last?.Value;
        BlockController secondLast = _blocks.Last?.Previous?.Value;

        if (last == null || secondLast == null ||
            Math.Abs(last.GetHeight() - secondLast.GetHeight()) >= float.Epsilon)
        {
            if (debug)
                Debug.Log("stopping merge");
            yield break;
        }

        Vector3 originalPos = last.transform.position;
        Transform slt = secondLast.transform;
        Vector3 endPos = slt.position + slt.localScale.y * Vector3.up;
        Vector3 midPos = 0.5f * (originalPos + endPos);
        midPos.y *= 5;
        float yTop = midPos.y;

        int partitions;
        float timer, timeStep;
        float deltaX1, deltaZ1, deltaX2, deltaZ2, maxD1, maxD2;
        float theta1, theta2, sinTheta1, sinTheta2, cosTheta1, cosTheta2;
        
        switch (mergeAnimation)
        {
            case MergeAnimation.Linear:
                partitions = 50;
                timeStep = (animationTime / 2) / partitions;
        
                timer = 0;
                while (timer < animationTime / 2)
                {
                    float p = timer / (animationTime / 2);
                    last.transform.position = Vector3.Lerp(originalPos, midPos, p);
                    timer += timeStep;
                    yield return new WaitForSeconds(timeStep);
                }

                timer = 0;
                while (timer < animationTime / 2)
                {
                    float p = timer / (animationTime / 2);
                    last.transform.position = Vector3.Lerp(midPos, endPos, p);
                    timer += timeStep;
                    yield return new WaitForSeconds(timeStep);
                }

                break;
            
            case MergeAnimation.Elliptical:
                partitions = 50;
                timeStep = (animationTime / 2) / partitions;

                deltaX1 = midPos.x - originalPos.x;
                deltaZ1 = midPos.z - originalPos.z;
                maxD1 = Mathf.Sqrt(deltaX1 * deltaX1 + deltaZ1 * deltaZ1);
                theta1 = Mathf.Atan2(deltaZ1, deltaX1);
                cosTheta1 = Mathf.Cos(theta1);
                sinTheta1 = Mathf.Sin(theta1);

                deltaX2 = endPos.x - midPos.x;
                deltaZ2 = endPos.z - midPos.z;
                maxD2 = Mathf.Sqrt(deltaX2 * deltaX2 + deltaZ2 * deltaZ2);
                theta2 = Mathf.Atan2(deltaZ2, deltaX2);
                cosTheta2 = Mathf.Cos(theta2);
                sinTheta2 = Mathf.Sin(theta2);

                timer = 0;
                while (timer < animationTime / 2)
                {
                    float p = Mathf.PI / 2 * timer / (animationTime / 2);
                    // x = (src.x - mid.x) cos t + mid.x
                    float d = -maxD1 * Mathf.Cos(p) + maxD1;
                    last.transform.position = new Vector3(
                        originalPos.x + d * cosTheta1,
                        (midPos.y - originalPos.y) * Mathf.Sin(p) + originalPos.y,
                        originalPos.z + d * sinTheta1
                    );
                    timer += timeStep;
                    yield return new WaitForSeconds(timeStep);
                }

                last.transform.position = midPos;
                
                timer = 0;
                while (timer < animationTime / 2)
                {
                    float p = Mathf.PI / 2 * timer / (animationTime / 2);
                    float d = maxD2 * Mathf.Sin(p);
                    last.transform.position = new Vector3(
                        midPos.x + d * cosTheta2,
                        (midPos.y - endPos.y) * Mathf.Cos(p) + endPos.y,
                        midPos.z + d * sinTheta2
                    );
                    timer += timeStep;
                    yield return new WaitForSeconds(timeStep);
                }

                last.transform.position = endPos;

                break;
            
            case MergeAnimation.Parabolic:
                partitions = 100;
                timeStep = animationTime / partitions;

                float x1 = originalPos.x;
                float y1 = originalPos.y;
                float z1 = originalPos.z;
                float x2 = endPos.x;
                float y2 = endPos.y;
                float z2 = endPos.z;
                
                deltaX1 = x2 - x1;
                deltaZ1 = z2 - z1;
                maxD1 = Mathf.Sqrt(deltaX1 * deltaX1 + deltaZ1 * deltaZ1);
                theta1 = Mathf.Atan2(deltaZ1, deltaX1);
                cosTheta1 = Mathf.Cos(theta1);
                sinTheta1 = Mathf.Sin(theta1);

                float m = (y2 - y1) / (x2 - x1);
                float a = (y1 + y2 - 2 * yTop - 2 * Mathf.Sqrt((y1 - yTop) * (y2 - yTop))) / (maxD1 * maxD1);
                float b = m - (x1 + x2) * a;
                float c = y1 - m * x1 + a * x1 * x2;

                float minD = (-b + Mathf.Sqrt(b * b - 4 * a * (c - y1))) / (2 * a);
                float maxD = (-b - Mathf.Sqrt(b * b - 4 * a * (c - y2))) / (2 * a);
                
                timer = 0;
                while (timer < animationTime)
                {
                    float p = timer / animationTime;
                    // x = (src.x - mid.x) cos t + mid.x
                    float d = p * maxD1;
                    float trueD = (maxD - minD) * p + minD;
                    last.transform.position = new Vector3(
                        originalPos.x + d * cosTheta1,
                        a * trueD * trueD + b * trueD + c,
                        originalPos.z + d * sinTheta1
                    );
                    timer += timeStep;
                    yield return new WaitForSeconds(timeStep);
                }

                last.transform.position = endPos;

                break;
            
            case MergeAnimation.Bezier:
            default:
                partitions = 100;
                timeStep = animationTime / partitions;

                float u = endPos.y - originalPos.y;
                
                Vector3 p0 = originalPos;
                Vector3 p1 = originalPos + 4 * u * Vector3.up;
                Vector3 p2 = endPos + 2 * u * Vector3.up;
                Vector3 p3 = endPos;
                
                timer = 0;
                while (timer < animationTime)
                {
                    // smoothing beginning and end of animation
                    float p = timer / animationTime;
                    float preT = Mathf.Sin(Mathf.PI / 2 * p);
                    float t = preT * preT;
                    float t2 = t * t;
                    float t3 = t2 * t;
                    float tInv = 1 - t;
                    float tInv2 = tInv * tInv;
                    float tInv3 = tInv2 * tInv;
                    // formula from here:
                    // https://www.gamedeveloper.com/business/how-to-work-with-bezier-curve-in-games-with-unity
                    Vector3 newP = (tInv3 * p0) + (3 * tInv2 * t * p1) + (3 * tInv * t2 * p2) + (t3 * p3);
                    last.transform.position = newP;
                    timer += timeStep;
                    yield return new WaitForSeconds(timeStep);
                }

                break;
        }

        _blocks.RemoveLast();
        Destroy(last.gameObject);
        secondLast.DoubleHeight();

        if (debug)
            Debug.Log("merged to new height " + secondLast.transform.localScale.y);
        
        DoNextAnimation(true);
        // StartCoroutine(AnimatedMerge(animationTime));
    }
}
