using UnityEngine;
using TensorFlowLite;
using System.Collections.Generic;
/**
 * Generates embedding for given list of Pose landmarks.
 */
public class PoseEmbedding {
  // Multiplier to apply to the torso to get minimal body size. Picked this by experimentation.
  private static  float TORSO_MULTIPLIER = 2.5f;

  public static List<Vector3> getPoseEmbedding(List<Vector3> landmarks) {
    List<Vector3> normalizedLandmarks = normalize(landmarks);
    return getEmbedding(normalizedLandmarks);
  }

  private static List<Vector3> normalize(List<Vector3> landmarks) {
    List<Vector3> normalizedLandmarks = new List<Vector3>(landmarks);
    // Normalize translation.
    Vector3 center = Utils.average(
        landmarks[PoseLandmarkDetect.LEFT_HIP], landmarks[PoseLandmarkDetect.RIGHT_HIP]);
    Utils.subtractAll(center, normalizedLandmarks);

    // Normalize scale.
    Utils.multiplyAll(normalizedLandmarks, 1 / getPoseSize(normalizedLandmarks));
    // Multiplication by 100 is not required, but makes it easier to debug.
    Utils.multiplyAll(normalizedLandmarks, 100);
    return normalizedLandmarks;
  }

  // Translation normalization should've been done prior to calling this method.
  private static float getPoseSize(List<Vector3> landmarks) {
    // Note: This approach uses only 2D landmarks to compute pose size as using Z wasn't helpful
    // in our experimentation but you're welcome to tweak.
    Vector3 hipsCenter = Utils.average(
        landmarks[PoseLandmarkDetect.LEFT_HIP], landmarks[PoseLandmarkDetect.RIGHT_HIP]);

    Vector3 shouldersCenter = Utils.average(
        landmarks[PoseLandmarkDetect.LEFT_SHOULDER],
        landmarks[PoseLandmarkDetect.RIGHT_SHOULDER]);

    float torsoSize = Utils.l2Norm2D(Utils.subtract(hipsCenter, shouldersCenter));

    float maxDistance = torsoSize * TORSO_MULTIPLIER;
    // torsoSize * TORSO_MULTIPLIER is the floor we want based on experimentation but actual size
    // can be bigger for a given pose depending on extension of limbs etc so we calculate that.
    foreach (Vector3 landmark in landmarks) {
      float distance = Utils.l2Norm2D(Utils.subtract(hipsCenter, landmark));
      if (distance > maxDistance) {
        maxDistance = distance;
      }
    }
    return maxDistance;
  }

  private static List<Vector3> getEmbedding(List<Vector3> lm) {
    List<Vector3> embedding = new List<Vector3>();

    // We use several pairwise 3D distances to form pose embedding. These were selected
    // based on experimentation for best results with our default pose classes as captued in the
    // pose samples csv. Feel free to play with this and add or remove for your use-cases.

    // We group our distances by number of joints between the pairs.
    // One joint.
    embedding.Add(Utils.subtract(
        Utils.average(lm[PoseLandmarkDetect.LEFT_HIP], lm[PoseLandmarkDetect.RIGHT_HIP]),
        Utils.average(lm[PoseLandmarkDetect.LEFT_SHOULDER], lm[PoseLandmarkDetect.RIGHT_SHOULDER])
    ));

    embedding.Add(Utils.subtract(
        lm[PoseLandmarkDetect.LEFT_SHOULDER], lm[PoseLandmarkDetect.LEFT_ELBOW]));
    embedding.Add(Utils.subtract(
        lm[PoseLandmarkDetect.RIGHT_SHOULDER], lm[PoseLandmarkDetect.RIGHT_ELBOW]));

    embedding.Add(Utils.subtract(lm[PoseLandmarkDetect.LEFT_ELBOW], lm[PoseLandmarkDetect.LEFT_WRIST]));
    embedding.Add(Utils.subtract(lm[PoseLandmarkDetect.RIGHT_ELBOW], lm[PoseLandmarkDetect.RIGHT_WRIST]));

    embedding.Add(Utils.subtract(lm[PoseLandmarkDetect.LEFT_HIP], lm[PoseLandmarkDetect.LEFT_KNEE]));
    embedding.Add(Utils.subtract(lm[PoseLandmarkDetect.RIGHT_HIP], lm[PoseLandmarkDetect.RIGHT_KNEE]));

    embedding.Add(Utils.subtract(lm[PoseLandmarkDetect.LEFT_KNEE], lm[PoseLandmarkDetect.LEFT_ANKLE]));
    embedding.Add(Utils.subtract(lm[PoseLandmarkDetect.RIGHT_KNEE], lm[PoseLandmarkDetect.RIGHT_ANKLE]));

    // Two joints.
    embedding.Add(Utils.subtract(
        lm[PoseLandmarkDetect.LEFT_SHOULDER], lm[PoseLandmarkDetect.LEFT_WRIST]));
    embedding.Add(Utils.subtract(
        lm[PoseLandmarkDetect.RIGHT_SHOULDER], lm[PoseLandmarkDetect.RIGHT_WRIST]));

    embedding.Add(Utils.subtract(lm[PoseLandmarkDetect.LEFT_HIP], lm[PoseLandmarkDetect.LEFT_ANKLE]));
    embedding.Add(Utils.subtract(lm[PoseLandmarkDetect.RIGHT_HIP], lm[PoseLandmarkDetect.RIGHT_ANKLE]));

    // Four joints.
    embedding.Add(Utils.subtract(lm[PoseLandmarkDetect.LEFT_HIP], lm[PoseLandmarkDetect.LEFT_WRIST]));
    embedding.Add(Utils.subtract(lm[PoseLandmarkDetect.RIGHT_HIP], lm[PoseLandmarkDetect.RIGHT_WRIST]));

    // Five joints.
    embedding.Add(Utils.subtract(
        lm[PoseLandmarkDetect.LEFT_SHOULDER], lm[PoseLandmarkDetect.LEFT_ANKLE]));
    embedding.Add(Utils.subtract(
        lm[PoseLandmarkDetect.RIGHT_SHOULDER], lm[PoseLandmarkDetect.RIGHT_ANKLE]));

    embedding.Add(Utils.subtract(lm[PoseLandmarkDetect.LEFT_HIP], lm[PoseLandmarkDetect.LEFT_WRIST]));
    embedding.Add(Utils.subtract(lm[PoseLandmarkDetect.RIGHT_HIP], lm[PoseLandmarkDetect.RIGHT_WRIST]));

    // Cross body.
    embedding.Add(Utils.subtract(lm[PoseLandmarkDetect.LEFT_ELBOW], lm[PoseLandmarkDetect.RIGHT_ELBOW]));
    embedding.Add(Utils.subtract(lm[PoseLandmarkDetect.LEFT_KNEE], lm[PoseLandmarkDetect.RIGHT_KNEE]));

    embedding.Add(Utils.subtract(lm[PoseLandmarkDetect.LEFT_WRIST], lm[PoseLandmarkDetect.RIGHT_WRIST]));
    embedding.Add(Utils.subtract(lm[PoseLandmarkDetect.LEFT_ANKLE], lm[PoseLandmarkDetect.RIGHT_ANKLE]));

    return embedding;
  }

  private PoseEmbedding() {}
}
