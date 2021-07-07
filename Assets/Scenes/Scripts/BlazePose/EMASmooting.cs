using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class EMASmoothing
{
    private static int DEFAULT_WINDOW_SIZE = 10;
    private static float DEFAULT_ALPHA = 0.2f;
    private int windowSize;
    private float alpha;
    // This is a window of {@link ClassificationResult}s as outputted by the {@link PoseClassifier}.
    // We run smoothing over this window of size {@link windowSize}.
    //private  Deque<ClassificationResult> window;
    private LinkedList<ClassificationResult> window=new LinkedList<ClassificationResult>();
    public EMASmoothing()
    {
        this.windowSize = DEFAULT_WINDOW_SIZE;
        this.alpha = DEFAULT_ALPHA;
    }

    public EMASmoothing(int windowSize, float alpha)
    {
        this.windowSize = windowSize;
        this.alpha = alpha;
        
        // this.window = new LinkedBlockingDeque<ClassificationResult>(windowSize);    //Couldn't find LinkedBlockingDeque in c#

    }

    public ClassificationResult getSmoothedResult(ClassificationResult classificationResult)
    {
        // If we are at window size, remove the last (oldest) result.
            if (window.Count == windowSize)
        {
            window.RemoveLast();
        }
        // Insert at the beginning of the window.
        window.AddFirst(classificationResult);


        HashSet<String> allClasses = new HashSet<String>();
        foreach (ClassificationResult result in window)
        {
            allClasses.UnionWith(result.getAllClasses());

        }

        ClassificationResult smoothedResult = new ClassificationResult();

        foreach (String className in allClasses)
        {
            float factor = 1;
            float topSum = 0;
            float bottomSum = 0;
            foreach (ClassificationResult result in window)
            {
                float value = result.getClassConfidence(className);

                topSum += factor * value;
                bottomSum += factor;

                factor = (float)(factor * (1.0 - alpha));
            }
            smoothedResult.putClassConfidence(className, topSum / bottomSum);
        }

        return smoothedResult;
    }
}