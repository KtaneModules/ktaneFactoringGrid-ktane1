using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;

public class factoringGridScript : MonoBehaviour
{

    public new KMAudio audio;
    public KMBombInfo bomb;
    public KMBombModule module;

    public KMSelectable[] hSelects;
    public KMSelectable[] vSelects;
    public KMSelectable[] otherButtons;

    public GameObject[] hPaths;
    public GameObject[] vPaths;
    public GameObject[] gridText;

    private int[] generatedSequence = new int[36];
    private int[] chosenPath = new int[36] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
    private int[] altPath = new int[36];
    private bool[] correctHP = new bool[30];
    private bool[] correctVP = new bool[30];
    private List<int> otherPaths = new List<int>();
    private List<int> validHP = new List<int>();
    private List<int> validVP = new List<int>();

    int[] primes = new int[] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59 };

    private static int moduleIdCounter = 1;
    private int moduleId;
    private bool moduleSolved;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        for (int i = 0; i < hSelects.Length; i++)
        {
            int j = i;
            hSelects[j].OnInteract += () => { vertToggle(j); return false; };
        }
        for (int i = 0; i < vSelects.Length; i++)
        {
            int j = i;
            vSelects[j].OnInteract += () => { horizToggle(j); return false; };
        }
        for (int i = 0; i < otherButtons.Length; i++)
        {
            int j = i;
            otherButtons[j].OnInteract += () => { buttonHandler(j); return false; };
        }
    }


    void Start()
    {
        generatePath();
        generateNumbers();
        for (int i = 0; i < gridText.Length; i++)
        {
            gridText[i].GetComponent<TextMesh>().text = generatedSequence[i].ToString();
        }
        var sb = new StringBuilder();
        for (int i = 0; i < gridText.Length; i++)//Logging
        {
            sb.Append(gridText[i].GetComponent<TextMesh>().text + ", ");
        }
        sb.Remove(sb.Length - 2, 2);
        Debug.LogFormat("[Factoring Grid #{0}] The number grid generated, in reading order, is {1}", moduleId, sb.ToString());
        sb = new StringBuilder();
        for (int i = 0; i < generatedSequence.Length; i++)
        {
            sb.Append((generatedSequence[chosenPath[i]]) + ", ");
        }
        sb.Remove(sb.Length - 2, 2);
        Debug.LogFormat("[Factoring Grid #{0}] The solution path generated is as follows: {1}", moduleId, sb.ToString());
    }

    void generatePath()
    {
        string order = "UDLR";
        string[][] orderMap = new string[6][] { new string[6], new string[6], new string[6], new string[6], new string[6], new string[6] };//Row first, then column
        int[][] gridIndex = new int[6][]
        {
            new int[6] {0, 1, 2, 3, 4, 5,},
            new int[6] {6, 7, 8, 9, 10, 11},
            new int[6] {12, 13, 14, 15, 16, 17},
            new int[6] {18, 19, 20, 21, 22, 23},
            new int[6] {24, 25, 26, 27, 28, 29,},
            new int[6] {30, 31, 32, 33, 34, 35,}
        };
        int[] usedDirections = new int[36];

        for (int j = 0; j < orderMap.Length; j++)
        {
            for (int i = 0; i < orderMap[j].Length; i++)
            {
                var temp = order.ToCharArray().Shuffle();
                orderMap[j][i] = new string(temp);
            }
        }

        chosenPath[0] = UnityEngine.Random.Range(0, 36);
        //Debug.Log(chosenPath[0]);
        int r = chosenPath[0] / 6;
        int c = chosenPath[0] % 6;
        int n = 0;
        bool repeat = false;
        while (n < 35)
        {
            //Debug.Log(orderMap[r][c] + " and " + usedDirections[n]);
            switch (orderMap[r][c][usedDirections[n]])
            {
                case 'U':
                    r--;
                    if (r < 0)
                    {
                        r++;
                        repeat = true;
                    }
                    else if (chosenPath.Contains(gridIndex[r][c]))
                    {
                        r++;
                        repeat = true;
                    }
                    else
                    {
                        repeat = false;
                    }
                    break;
                case 'D':
                    r++;
                    if (r > 5)
                    {
                        r--;
                        repeat = true;
                    }
                    else if (chosenPath.Contains(gridIndex[r][c]))
                    {
                        r--;
                        repeat = true;
                    }
                    else
                    {
                        repeat = false;
                    }
                    break;
                case 'L':
                    c--;
                    if (c < 0)
                    {
                        c++;
                        repeat = true;
                    }
                    else if (chosenPath.Contains(gridIndex[r][c]))
                    {
                        c++;
                        repeat = true;
                    }
                    else
                    {
                        repeat = false;
                    }
                    break;
                case 'R':
                    c++;
                    if (c > 5)
                    {
                        c--;
                        repeat = true;
                    }
                    else if (chosenPath.Contains(gridIndex[r][c]))
                    {
                        c--;
                        repeat = true;
                    }
                    else
                    {
                        repeat = false;
                    }
                    break;
            }
            if (repeat)
            {
                usedDirections[n]++;
            }
            if (usedDirections[n] > 3)
            {
                while (usedDirections[n] > 3)
                {
                    chosenPath[n] = -1;
                    usedDirections[n] = 0;
                    n--;
                    usedDirections[n]++;
                }
                r = chosenPath[n] / 6;
                c = chosenPath[n] % 6;
            }
            //Debug.Log("back to " + n);
            if (!repeat)
            {
                n++;
                chosenPath[n] = gridIndex[r][c];
                //Debug.Log(n + " path is " + chosenPath[n]);
            }
        }

        for (int i = 0; i < chosenPath.Length - 1; i++)
        {
            switch (chosenPath[i + 1] - chosenPath[i])
            {
                case 1://Right
                    correctHP[(chosenPath[i] / 6 * 5) + chosenPath[i] % 6] = true;
                    validHP.Add((chosenPath[i] / 6 * 5) + chosenPath[i] % 6);
                    break;
                case -1://Left
                    correctHP[(chosenPath[i] / 6 * 5) + chosenPath[i] % 6 - 1] = true;
                    validHP.Add((chosenPath[i] / 6 * 5) + chosenPath[i] % 6 - 1);
                    break;
                case -6://Up
                    correctVP[chosenPath[i] - 6] = true;
                    validVP.Add(chosenPath[i] - 6);
                    break;
                case 6://Down
                    correctVP[chosenPath[i]] = true;
                    validVP.Add(chosenPath[i]);
                    break;
            }
        }
        var possibleEdges = Enumerable.Range(0, 60).ToList();//0-29 for horizontal paths, 30-59 for vertical paths
        for (int i = 0; i < correctHP.Length; i++)
        {
            if (correctHP[i]) { possibleEdges.Remove(i); }
        }
        for (int i = 0; i < correctVP.Length; i++)
        {
            if (correctVP[i]) { possibleEdges.Remove(i + 30); }
        }
        possibleEdges.Shuffle();
        for (int i = 0; i < possibleEdges.Count; i++)
        {
            if (possibleEdges[i] > 30) { validVP.Add(possibleEdges[i] - 30); /*correctVP[possibleEdges[i] - 30] = true;*/ }
            else { validHP.Add(possibleEdges[i]); /*correctHP[possibleEdges[i]] = true;*/ }
            otherPaths.Add(possibleEdges[i]);
            int test = pathChecker();
            if (test != 2)//One detected from the start to end, another detected from end to start
            {
                if (possibleEdges[i] > 30) { validVP.Remove(possibleEdges[i] - 30); /*correctVP[possibleEdges[i] - 30] = false;*/ }
                else { validHP.Remove(possibleEdges[i]); /*correctHP[possibleEdges[i]] = false;*/ }
                otherPaths.Remove(possibleEdges[i]);
            }
        }
    }

    int pathChecker()
    {
        var paths = new List<List<int>>();
        for (int i = 0; i < 36; i++)
        {
            var path = new List<int>();
            var counter = new int[36] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
            int n = 0;
            path.Add(i);
            counter[n]++;
            //Debug.Log("starting with " + i);
            while (n >= 0)
            {
                switch (counter[n])
                {
                    case 0:
                        counter[n]++;
                        if (path[n] - 6 >= 0 && !path.Contains(path[n] - 6) && validVP.Contains(path[n] - 6))//Up
                        {
                            path.Add(path[n] - 6);
                            n++;
                            //Debug.Log("moving up to " + path[n]);
                            counter[n]++;
                        }
                        break;
                    case 1:
                        counter[n]++;
                        if (path[n] + 6 < 36 && !path.Contains(path[n] + 6) && validVP.Contains(path[n]))//Down
                        {
                            path.Add(path[n] + 6);
                            n++;
                            //Debug.Log("moving down to " + path[n]);
                            counter[n]++;
                        }
                        break;
                    case 2:
                        counter[n]++;
                        if ((path[n] - 1 + 6) % 6 != 5 && !path.Contains(path[n] - 1) && validHP.Contains((path[n] / 6 * 5) + path[n] % 6 - 1))//Left
                        {
                            path.Add(path[n] - 1);
                            n++;
                            //Debug.Log("moving left to " + path[n]);
                            counter[n]++;
                        }
                        break;
                    case 3:
                        counter[n]++;
                        if ((path[n] + 1) % 6 != 0 && !path.Contains(path[n] + 1) && validHP.Contains((path[n] / 6 * 5) + path[n] % 6))//Right
                        {
                            path.Add(path[n] + 1);
                            n++;
                            //Debug.Log("moving right to " + path[n]);
                            counter[n]++;
                        }
                        break;
                    default:
                        while (counter[n] > 3)
                        {
                            // Debug.Log("no way to go, backtracking... ");
                            path.RemoveAt(path.Count() - 1);
                            counter[n] = -1;
                            n--;
                            if (n < 0)
                            {
                                //Debug.Log("welp, ended");
                                break;
                            }
                            counter[n]++;
                            //Debug.Log("...to path #" + n + " which is " + path[n]);
                        }
                        break;
                }
                if (path.Count() > 35)
                {
                    //Debug.Log("found a path!");
                    paths.Add(path);
                    path.RemoveAt(path.Count() - 1);
                    counter[n] = -1;
                    n--;
                    counter[n]++;
                }
            }

        }
        return paths.Count();
    }

    void generateNumbers()
    {
        var vFactors = new int[30] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
        var hFactors = new int[30] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
        int prev = -1;
        int current;
        bool a = false;

        for (int i = 0; i < chosenPath.Length - 1; i++)//Prime factors on generated solution paths
        {
            a = false;
            switch (chosenPath[i + 1] - chosenPath[i])
            {
                case 1://Right
                    current = (chosenPath[i] / 6 * 5) + chosenPath[i] % 6;
                    if (prev < 0)
                    {
                        hFactors[current] = primes[UnityEngine.Random.Range(0, primes.Length)];
                    }
                    else
                    {
                        do
                        {
                            a = false;
                            hFactors[current] = primes[UnityEngine.Random.Range(0, primes.Length)];
                            if (current % 5 > 0) { a = hFactors[current - 1] == hFactors[current]; }
                            if (current % 5 < 4) { a = hFactors[current + 1] == hFactors[current]; }
                        } while (hFactors[current] * prev > 180 || a);
                    }
                    prev = hFactors[current];
                    break;
                case -1://Left
                    current = (chosenPath[i] / 6 * 5) + chosenPath[i] % 6 - 1;
                    if (prev < 0)
                    {
                        hFactors[current] = primes[UnityEngine.Random.Range(0, primes.Length)];
                    }
                    else
                    {
                        do
                        {
                            a = false;
                            hFactors[current] = primes[UnityEngine.Random.Range(0, primes.Length)];
                            if (current % 5 > 0) { a = hFactors[current - 1] == hFactors[current]; }
                            if (current % 5 < 4) { a = hFactors[current + 1] == hFactors[current]; }
                        } while (hFactors[current] * prev > 180 || a);
                    }
                    prev = hFactors[current];
                    break;
                case -6://Up
                    current = chosenPath[i] - 6;
                    if (prev < 0)
                    {
                        vFactors[current] = primes[UnityEngine.Random.Range(0, primes.Length)];
                    }
                    else
                    {
                        do
                        {
                            a = false;
                            vFactors[current] = primes[UnityEngine.Random.Range(0, primes.Length)];
                            if (current > 5) { a = vFactors[current - 6] == vFactors[current]; }
                            if (current < 24) { a = vFactors[current + 6] == vFactors[current]; }
                        } while (vFactors[current] * prev > 180 || a);
                    }
                    prev = vFactors[current];
                    break;
                case 6://Down
                    current = chosenPath[i];
                    if (prev < 0)
                    {
                        vFactors[current] = primes[UnityEngine.Random.Range(0, primes.Length)];
                    }
                    else
                    {
                        do
                        {
                            a = false;
                            vFactors[current] = primes[UnityEngine.Random.Range(0, primes.Length)];
                            if (current > 5) { a = vFactors[current - 6] == vFactors[current]; }
                            if (current < 24) { a = vFactors[current + 6] == vFactors[current]; }
                        } while (vFactors[current] * prev > 180 || a);
                    }
                    prev = vFactors[current];
                    break;
            }
        }
        for (int i = 0; i < generatedSequence.Length; i++)
        {
            var primeList = new List<int>();
            if (i - 6 >= 0 && validVP.Contains(i - 6)) { primeList.Add(vFactors[i - 6]); }//Up
            if (i + 6 < 36 && validVP.Contains(i)) { primeList.Add(vFactors[i]); }//Down
            if ((i - 1 + 6) % 6 != 5 && validHP.Contains((i / 6 * 5) + i % 6 - 1)) { primeList.Add(hFactors[(i / 6 * 5) + i % 6 - 1]); }//Left
            if ((i + 1) % 6 != 0 && validHP.Contains((i / 6 * 5) + i % 6)) { primeList.Add(hFactors[(i / 6 * 5) + i % 6]); }//Right
            generatedSequence[i] = primeList.Aggregate(1, (acc, val) => acc * val);
        }

        var b = new int[] { 1 };
        var offsets = b.Concat(primes).ToArray();
        for (int i = 0; i < otherPaths.Count(); i++)//Prime factors for decoy paths
        {//0-29 for horizontal paths, 30-59 for vertical paths
            if (i < 30)
            {
                do
                {
                    hFactors[i] = offsets[UnityEngine.Random.Range(0, offsets.Length)];
                }
                while (generatedSequence[i / 5 * 6 + i % 5] * hFactors[i] > 200 || generatedSequence[i / 5 * 6 + i % 5 + 1] * hFactors[i] > 200);
                generatedSequence[i / 5 * 6 + i % 5] *= hFactors[i];//Left number
                generatedSequence[i / 5 * 6 + i % 5 + 1] *= hFactors[i];//Right number
            }
            else
            {
                do
                {
                    vFactors[i - 30] = offsets[UnityEngine.Random.Range(0, offsets.Length)];
                }
                while (generatedSequence[i - 30] * vFactors[i] > 200 || generatedSequence[i - 30 + 6] * vFactors[i] > 200);
                generatedSequence[i - 30] *= vFactors[i];//Top number
                generatedSequence[i - 30 + 6] *= vFactors[i];//Bottom number
            }
        }

        for (int i = 0; i < generatedSequence.Length; i++)//Prime offsets for each cell for extra randomisation
        {
            do
            {
                a = false;
                current = offsets[UnityEngine.Random.Range(0, offsets.Length)];
                if (current != 1)
                {
                    if (i > 5) { if (!validVP.Contains(i - 6)) { a = !ExMath.IsCoprime(current, generatedSequence[i - 6]); } }//Up
                    if (i < 30) { if (!validVP.Contains(i)) { a = !ExMath.IsCoprime(current, generatedSequence[i]); } }//Down
                    if (i % 6 > 0) { if (!validHP.Contains((i / 6 * 5) + i % 6 - 1)) { a = !ExMath.IsCoprime(current, generatedSequence[i]); } }//Left
                    if (i % 6 < 5) { if (!validHP.Contains((i / 6 * 5) + i % 6)) { a = !ExMath.IsCoprime(current, generatedSequence[i]); } }//Right
                }
            }
            while (generatedSequence[i] * current > 200 || a);
            generatedSequence[i] *= current;
        }
    }

    void buttonHandler(int k)
    {
        if (moduleSolved) { return; }
        audio.PlaySoundAtTransform("button", transform);
        if (k == 0)//Reset
        {
            foreach (GameObject a in hPaths)
            {
                a.GetComponent<MeshRenderer>().enabled = false;
            }
            foreach (GameObject a in vPaths)
            {
                a.GetComponent<MeshRenderer>().enabled = false;
            }
            Debug.LogFormat("<Factoring Grid #{0}> Reset button pressed.", moduleId);
        }
        else if (k == 1)//Submit
        {
            Debug.LogFormat("<Factoring Grid #{0}> Submit button pressed.", moduleId);
            bool isWrong = false;
            for (int i = 0; i < hPaths.Length; i++)
            {
                if (hPaths[i].GetComponent<MeshRenderer>().enabled != correctHP[i])
                    isWrong = true;
            }
            for (int i = 0; i < vPaths.Length; i++)
            {
                if (vPaths[i].GetComponent<MeshRenderer>().enabled != correctVP[i])
                    isWrong = true;
            }
            if (isWrong)
            {
                var sb = new StringBuilder();
                string[] connectedLines = new string[36];
                for (int i = 0; i < 36; i++)
                {
                    if (i / 6 * 5 + i % 6 < 30 && i % 6 != 5)//Right relation
                        if (hPaths[i / 6 * 5 + i % 6].GetComponent<MeshRenderer>().enabled)
                        {
                            if (ExMath.IsCoprime(Int32.Parse(gridText[i].GetComponent<TextMesh>().text), Int32.Parse(gridText[i + 1].GetComponent<TextMesh>().text)))
                            {
                                Debug.LogFormat("[Factoring Grid #{0}] Invalid number connection found between {1} and {2}, strike.", moduleId, gridText[i].GetComponent<TextMesh>().text, gridText[i + 1].GetComponent<TextMesh>().text);
                                module.HandleStrike();
                                audio.PlaySoundAtTransform("strike", transform);
                                return;
                            }
                            sb.Append("R");
                        }
                    if (i / 6 * 5 + i % 6 - 1 >= 0 && i % 6 != 0)//Left relation
                        if (hPaths[i / 6 * 5 + i % 6 - 1].GetComponent<MeshRenderer>().enabled)
                        {
                            if (ExMath.IsCoprime(Int32.Parse(gridText[i].GetComponent<TextMesh>().text), Int32.Parse(gridText[i - 1].GetComponent<TextMesh>().text)))
                            {
                                Debug.LogFormat("[Factoring Grid #{0}] Invalid number connection found between {1} and {2}, strike.", moduleId, gridText[i].GetComponent<TextMesh>().text, gridText[i - 1].GetComponent<TextMesh>().text);
                                module.HandleStrike();
                                audio.PlaySoundAtTransform("strike", transform);
                                return;
                            }
                            sb.Append("L");
                        }
                    if (i - 6 >= 0)//Up relation
                        if (vPaths[i - 6].GetComponent<MeshRenderer>().enabled)
                        {
                            if (ExMath.IsCoprime(Int32.Parse(gridText[i].GetComponent<TextMesh>().text), Int32.Parse(gridText[i - 6].GetComponent<TextMesh>().text)))
                            {
                                Debug.LogFormat("[Factoring Grid #{0}] Invalid number connection found between {1} and {2}, strike.", moduleId, gridText[i].GetComponent<TextMesh>().text, gridText[i - 6].GetComponent<TextMesh>().text);
                                module.HandleStrike();
                                audio.PlaySoundAtTransform("strike", transform);
                                return;
                            }
                            sb.Append("U");
                        }
                    if (i < 30)//Down relation
                        if (vPaths[i].GetComponent<MeshRenderer>().enabled)
                        {
                            if (ExMath.IsCoprime(Int32.Parse(gridText[i].GetComponent<TextMesh>().text), Int32.Parse(gridText[i + 6].GetComponent<TextMesh>().text)))
                            {
                                Debug.LogFormat("[Factoring Grid #{0}] Invalid number connection found between {1} and {2}, strike.", moduleId, gridText[i].GetComponent<TextMesh>().text, gridText[i + 6].GetComponent<TextMesh>().text);
                                module.HandleStrike();
                                audio.PlaySoundAtTransform("strike", transform);
                                return;
                            }
                            sb.Append("D");
                        }
                    connectedLines[i] = sb.ToString();
                    sb.Remove(0, sb.Length);
                    if (connectedLines[i].Length != 1 && connectedLines[i].Length != 2)
                    {
                        Debug.LogFormat("[Factoring Grid #{0}] Invalid path formed, strike.", moduleId);
                        module.HandleStrike();
                        audio.PlaySoundAtTransform("strike", transform);
                        return;
                    }
                }
                if (connectedLines.Count(n => n.Length == 1) != 2)
                {
                    Debug.LogFormat("[Factoring Grid #{0}] Invalid path formed, strike.", moduleId);
                    module.HandleStrike();
                    audio.PlaySoundAtTransform("strike", transform);
                    return;
                }
                int a = 0;
                for (int i = 0; i < connectedLines.Length; i++)
                {
                    if (connectedLines[i].Length == 1)
                    {
                        a = i;
                        i = connectedLines.Length;
                    }
                }
                string number = "";
                int counter = 1;
                altPath[0] = a;
                sb.Append(gridText[a].GetComponent<TextMesh>().text + ", ");
                while (connectedLines[a].Length == 1)
                {
                    switch (connectedLines[a])
                    {
                        case "L":
                            a = a - 1;
                            connectedLines[a] = connectedLines[a].Replace("R", "");
                            break;
                        case "R":
                            a = a + 1;
                            connectedLines[a] = connectedLines[a].Replace("L", "");
                            break;
                        case "U":
                            a = a - 6;
                            connectedLines[a] = connectedLines[a].Replace("D", "");
                            break;
                        case "D":
                            a = a + 6;
                            connectedLines[a] = connectedLines[a].Replace("U", "");
                            break;
                    }
                    number = gridText[a].GetComponent<TextMesh>().text;
                    altPath[counter] = a;
                    counter++;
                    sb.Append(number + ", ");
                }
                if (counter != 36)
                {
                    Debug.LogFormat("[Factoring Grid #{0}] Invalid path formed, strike.", moduleId);
                    audio.PlaySoundAtTransform("strike", transform);
                    module.HandleStrike();
                }
                else
                {
                    Debug.LogFormat("[Factoring Grid #{0}] Submitted alternative solution, module solved.", moduleId);
                    sb.Remove(sb.Length - 2, 2);
                    Debug.LogFormat("[Factoring Grid #{0}] The solution path in question: {1}", moduleId, sb.ToString());
                    module.HandlePass();
                    audio.PlaySoundAtTransform("solve", transform);
                    moduleSolved = true;
                    StartCoroutine(solveAnim(altPath));
                    for (int i = 0; i < hPaths.Length; i++)
                    {
                        hPaths[i].GetComponent<MeshRenderer>().material.color = Color.green;
                    }
                    for (int i = 0; i < vPaths.Length; i++)
                    {
                        vPaths[i].GetComponent<MeshRenderer>().material.color = Color.green;
                    }
                }
            }
            else
            {
                Debug.LogFormat("[Factoring Grid #{0}] Submitted intended solution, module solved.", moduleId);
                module.HandlePass();
                audio.PlaySoundAtTransform("solve", transform);
                moduleSolved = true;
                StartCoroutine(solveAnim(chosenPath));
                for (int i = 0; i < hPaths.Length; i++)
                {
                    hPaths[i].GetComponent<MeshRenderer>().material.color = Color.green;
                }
                for (int i = 0; i < vPaths.Length; i++)
                {
                    vPaths[i].GetComponent<MeshRenderer>().material.color = Color.green;
                }
            }
        }
    }

    void horizToggle(int k)
    {
        if (moduleSolved) { return; }
        audio.PlaySoundAtTransform("select", transform);
        hPaths[k].GetComponent<MeshRenderer>().enabled = !hPaths[k].GetComponent<MeshRenderer>().enabled;
    }

    void vertToggle(int k)
    {
        if (moduleSolved) { return; }
        audio.PlaySoundAtTransform("select", transform);
        vPaths[k].GetComponent<MeshRenderer>().enabled = !vPaths[k].GetComponent<MeshRenderer>().enabled;
    }

    IEnumerator solveAnim(int[] path)
    {
        foreach (int i in path)
        {
            gridText[i].GetComponent<TextMesh>().color = Color.green;
            yield return new WaitForSeconds(0.08f);
        }
        foreach (int i in path)
        {
            gridText[i].GetComponent<TextMesh>().color = Color.black;
            yield return new WaitForSeconds(0.08f);
        }
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"<!{0} a1 a2;a1 b1> to toggle the edges between adjacent cells, <!{0} reset> to reset the grid, <!{0} submit> to submit";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant().Trim();
        if (Regex.IsMatch(command, @"^\s*reset\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            otherButtons[0].OnInteract();
            yield break;
        }
        if (Regex.IsMatch(command, @"^\s*submit\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            otherButtons[1].OnInteract();
            yield break;
        }
        command = command.Replace(" ", String.Empty);
        string[] parameters = command.Split(',', ';');
        yield return null;
        for (int i = 0; i < parameters.Length; i++)
        {
            if (!Regex.IsMatch(parameters[i], @"^\s*([a-f][1-6]){2}\s*$", RegexOptions.IgnoreCase))
            {
                yield return "sendtochaterror The specified cell pair '" + parameters[i] + "' is invalid!";
                yield break;
            }
            char[] parametersInCharArray = parameters[i].ToCharArray();
            int charDifference = Math.Abs(parametersInCharArray[0] - parametersInCharArray[2]);
            int numberDifference = Math.Abs(parametersInCharArray[1] - parametersInCharArray[3]);
            if (!((charDifference == 1 && numberDifference == 0) || (charDifference == 0 && numberDifference == 1)))
            {
                yield return "sendtochaterror The specified cell pair '" + parameters[i] + "' is not adjacent!";
                yield break;
            }
        }
        for (int i = 0; i < parameters.Length; i++)
        {
            char[] parametersInCharArray = parameters[i].ToCharArray();
            int charDifference = parametersInCharArray[0] - parametersInCharArray[2];
            int numberDifference = parametersInCharArray[1] - parametersInCharArray[3];
            if (Math.Abs(charDifference) == 1 && Math.Abs(numberDifference) == 0)
            {
                int buttonToPress = (parametersInCharArray[1] - '1') * 5 + ((charDifference > 0 ? parametersInCharArray[2] : parametersInCharArray[0]) - 'a');
                hSelects[buttonToPress].OnInteract();
            }
            else if (Math.Abs(charDifference) == 0 && Math.Abs(numberDifference) == 1)
            {
                int buttonToPress = (parametersInCharArray[0] - 'a') * 5 + ((numberDifference > 0 ? parametersInCharArray[3] : parametersInCharArray[1]) - '1');
                vSelects[buttonToPress].OnInteract();
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        while (!moduleSolved)
        {
            for (int i = 0; i < correctHP.Length; i++)
            {
                if (hPaths[i].GetComponent<MeshRenderer>().enabled != correctHP[i])
                {
                    vSelects[i].OnInteract();
                    yield return null;
                }
                yield return null;
            }
            for (int i = 0; i < correctVP.Length; i++)
            {
                if (vPaths[i].GetComponent<MeshRenderer>().enabled != correctVP[i])
                {
                    hSelects[i].OnInteract();
                    yield return null;
                }
                yield return null;
            }
            otherButtons[1].OnInteract();
        }

    }

}
