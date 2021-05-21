using System.Collections.Generic;
using System;

public class ClassificationResult
{
    private Dictionary<string, float> classConfidences;

    public ClassificationResult() {
    classConfidences = new Dictionary<string, float>();
  }

  public HashSet<String> getAllClasses() {
    return new HashSet<String>(classConfidences.Keys);
  }

  public float getClassConfidence(String className) {
    return classConfidences.ContainsKey(className) ? classConfidences[className] : 0;
  }

  public String getMaxConfidenceClass() {

    string maxKey = "";
    float max = 0;

    foreach (KeyValuePair<string, float> de in classConfidences)
    {
        maxKey = de.Value > max ? de.Key : maxKey;
        max = de.Value > max ? de.Value : max;
    }
    
    return maxKey;
  }

  public void incrementClassConfidence(String className) {
    classConfidences[className] =  classConfidences.ContainsKey(className) ? classConfidences[className] + 1 : 1;
  }


  public void putClassConfidence(String className, float confidence) {
    classConfidences[className] = confidence;
  }
}