using UnityEngine;
using TensorFlowLite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TensorFlowLite
{
  public class PoseClassifier {
    private static readonly String TAG = "PoseClassifier";
    private static readonly int MAX_DISTANCE_TOP_K = 30;
    private static readonly int MEAN_DISTANCE_TOP_K = 10;
    // Note Z has a lower weight as it is generally less accurate than X & Y.
    private static readonly Vector3 AXES_WEIGHTS = new Vector3(1, 1, 0.2f);

    private readonly List<PoseSample> poseSamples;
    private readonly int maxDistanceTopK;
    private readonly int meanDistanceTopK;
    private readonly Vector3 axesWeights;

    public PoseClassifier(List<PoseSample> poseSamples) {
      this.poseSamples = poseSamples;
      this.maxDistanceTopK = MAX_DISTANCE_TOP_K;
      this.meanDistanceTopK = MEAN_DISTANCE_TOP_K; 
      this.axesWeights=AXES_WEIGHTS;
    }

    public PoseClassifier(List<PoseSample> poseSamples, int maxDistanceTopK,
        int meanDistanceTopK, Vector3 axesWeights) {
      this.poseSamples = poseSamples;
      this.maxDistanceTopK = maxDistanceTopK;
      this.meanDistanceTopK = meanDistanceTopK;
      this.axesWeights = axesWeights;
    }

    private static List<Vector3> extractPoseLandmarks(PoseLandmarkDetect.Result pose) {
      List<Vector3> landmarks = new List<Vector3>();
      foreach (Vector3 poseLandmark in pose.joints) {
        landmarks.Add(poseLandmark);
      }
      return landmarks;
    }

    /**
    * Returns the max range of confidence values.
    *
    * <p><Since we calculate confidence by counting {@link PoseSample}s that survived
    * outlier-filtering by maxDistanceTopK and meanDistanceTopK, this range is the minimum of two.
    */
    public int confidenceRange() {
      return Math.Min(maxDistanceTopK, meanDistanceTopK);
    }

    public ClassificationResult classify(PoseLandmarkDetect.Result pose) {
     
      return classify(extractPoseLandmarks(pose));
      

    }

    public ClassificationResult classify(List<Vector3> landmarks) {
      ClassificationResult result = new ClassificationResult();
      // Return early if no landmarks detected.
      
      if (landmarks.Count==0) {
        return result;
      }

      // We do flipping on X-axis so we are horizontal (mirror) invariant.
      List<Vector3> flippedLandmarks = new List<Vector3>(landmarks);
      Utils.multiplyAll(flippedLandmarks, new Vector3(-1, 1, 1));

      List<Vector3> embedding = PoseEmbedding.getPoseEmbedding(landmarks);
      List<Vector3> flippedEmbedding = PoseEmbedding.getPoseEmbedding(flippedLandmarks);


      // Classification is done in two stages:
      //  * First we pick top-K samples by MAX distance. It allows to remove samples that are almost
      //    the same as given pose, but maybe has few joints bent in the other direction.
      //  * Then we pick top-K samples by MEAN distance. After outliers are removed, we pick samples
      //    that are closest by average.

      // Keeps max distance on top so we can pop it when top_k size is reached.
      List<Tuple<PoseSample, float>> maxDistances = new List<Tuple<PoseSample, float>>(maxDistanceTopK);
      // Retrieve top K poseSamples by least distance to remove outliers.
      foreach (PoseSample poseSample in poseSamples) {
        List<Vector3> sampleEmbedding = poseSample.getEmbedding();

        float originalMax = 0;
        float flippedMax = 0;
        for (int i = 0; i < embedding.Count; i++) {
          originalMax =
              Math.Max(
                  originalMax,
                  Utils.maxAbs(Utils.multiply(Utils.subtract(embedding[i], sampleEmbedding[i]), axesWeights)));
          flippedMax =
              Math.Max(
                  flippedMax,
                  Utils.maxAbs(
                      Utils.multiply(
                        Utils.subtract(flippedEmbedding[i], sampleEmbedding[i]), axesWeights)));
        }
        // Set the max distance as min of original and flipped max distance.
        maxDistances.Add(new Tuple<PoseSample, float>(poseSample, Math.Min(originalMax, flippedMax)));

        
        // We only want to retain top n so pop the highest distance.
        if (maxDistances.Count > maxDistanceTopK) {
          
          maxDistances = maxDistances.OrderBy(i => i.Item2).ToList();
          maxDistances.RemoveAt(0);
        }
      }

      // Keeps higher mean distances on top so we can pop it when top_k size is reached.
      List<Tuple<PoseSample, float>> meanDistances = new List<Tuple<PoseSample, float>>(); //
      // Retrive top K poseSamples by least mean distance to remove outliers.
      foreach (Tuple<PoseSample, float> sampleDistances in maxDistances) {
        PoseSample poseSample = sampleDistances.Item1;
        List<Vector3> sampleEmbedding = poseSample.getEmbedding();

        float originalSum = 0;
        float flippedSum = 0;
        for (int i = 0; i < embedding.Count; i++) {
          originalSum += Utils.sumAbs(Utils.multiply(
              Utils.subtract(embedding[i], sampleEmbedding[i]), axesWeights));
          flippedSum += Utils.sumAbs(
              Utils.multiply(Utils.subtract(flippedEmbedding[i], sampleEmbedding[i]), axesWeights));
        }
        // Set the mean distance as min of original and flipped mean distances.
        float meanDistance = Math.Min(originalSum, flippedSum) / (embedding.Count * 2);
        meanDistances.Add(new Tuple<PoseSample, float>(poseSample, meanDistance));
        // We only want to retain top k so pop the highest mean distance.
        if (meanDistances.Count > meanDistanceTopK) {
          meanDistances = meanDistances.OrderBy(i => i.Item2).ToList();
          meanDistances.RemoveAt(0);
        }
      }

      foreach (Tuple<PoseSample, float> sampleDistances in meanDistances) {
        String className = sampleDistances.Item1.getClassName();
        result.incrementClassConfidence(className);
      }

      return result;
    }
  }
}