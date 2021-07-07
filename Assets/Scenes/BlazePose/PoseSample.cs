
using System;
using System.Collections.Generic;
using UnityEngine;
/**
* Reads Pose samples from a csv file.
*/
public class PoseSample {
  private static readonly string TAG = "PoseSample";
  private static readonly int NUM_LANDMARKS = 33;
  private static readonly int NUM_DIMS = 3;

  private readonly string name;
  private readonly string className;
  private readonly List<Vector3> embedding;

  public PoseSample(string name, string className, List<Vector3> landmarks) {
    this.name = name;
    this.className = className;
    this.embedding = PoseEmbedding.getPoseEmbedding(landmarks);
  }

  public string getName() {
    return name;
  }

  public string getClassName() {
    return className;
  }

  public List<Vector3> getEmbedding() {
    return embedding;
  }

  public static PoseSample getPoseSample(string csvLine, string separator) {
    List<string> tokens = new List<string>(csvLine.Split(separator[0]));
    // Format is expected to be Name,Class,X1,Y1,Z1,X2,Y2,Z2...
    // + 2 is for Name & Class.
    if (tokens.Count != (NUM_LANDMARKS * NUM_DIMS) + 2) {
      Debug.Log(TAG + "Invalid number of tokens for PoseSample");
      return null;
    }
    string name = tokens[0];
    string className = tokens[1];
    List<Vector3> landmarks = new List<Vector3>();
    // Read from the third token, first 2 tokens are name and class.
    for (int i = 2; i < tokens.Count; i += NUM_DIMS) {
      try {
        landmarks.Add(
            new Vector3(
                float.Parse(tokens[i]),
                float.Parse(tokens[i + 1]),
                //float.Parse(tokens[i + 2])
                0));
      } catch (Exception) {
        Debug.Log(TAG + "Invalid value " + tokens[i] + " for landmark position.");
        return null;
      }
    }
    
    return new PoseSample(name, className, landmarks);
  }
}