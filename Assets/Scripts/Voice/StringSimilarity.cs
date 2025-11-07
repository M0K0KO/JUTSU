using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public static class StringSimilarity
{
    private static readonly Regex AsteriskPattern = new Regex(@"\*[^*]*\*", RegexOptions.Compiled);
    private static readonly Regex ParenthesesPattern = new Regex(@"\([^)]*\)", RegexOptions.Compiled);
    private static readonly Regex PunctuationPattern = new Regex(@"[- .!?,""]", RegexOptions.Compiled);
    
    public static bool IsSimilar(string input, string target, float jaroWinklerThreshold = 0.7f, 
        float levenshteinThreshold = 0.7f)
    {
        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(target))
        {
            Debug.LogWarning($"String is null or empty! input: {input}, target: {target}");
            return false;
        }
        
        string normalizedInput = Normalize(input);
        string normalizedTarget = Normalize(target);

        if (string.IsNullOrEmpty(normalizedInput) || string.IsNullOrEmpty(normalizedTarget))
        {
            Debug.LogWarning("Normalized string is null or empty! " +
                             $"normalized input: {normalizedInput}, normalized target: {normalizedTarget}");
        }

        if (IsTargetStringSingleWord(target))
        {
            // Use JW algorithm
            float jaroWinklerSimilarity = GetJaroWinklerSimilarity(normalizedInput, normalizedTarget);
            return jaroWinklerSimilarity >= jaroWinklerThreshold;
        }

        // Use Levenshtein Distance Similarity
        float levenshteinDistanceSimilarity = GetLevenshteinDistanceSimilarity(normalizedInput, normalizedTarget);
        return levenshteinDistanceSimilarity >= levenshteinThreshold;
    }

    public static bool IsTargetStringSingleWord(string targetString)
    {
        string trimmedTarget = targetString.Trim();

        foreach (char character in trimmedTarget)
        {
            if (char.IsWhiteSpace(character))
            {
                return false;
            }
        }
        
        return true;
    }

    public static float GetJaroSimilarity(string firstString, string secondString)
    {
        if (firstString == secondString) return 1f;
        
        int firstLength = firstString.Length;
        int secondLength = secondString.Length;

        int matchDistance = Mathf.Max(0, Mathf.Max(firstLength, secondLength) / 2 - 1);

        var firstMatch = new bool[firstLength];
        var secondMatch = new bool[secondLength];

        int matches = 0;

        for (int i = 0; i < firstLength; i++)
        {
            int start = Mathf.Max(0, i - matchDistance);
            int end = Mathf.Min(secondLength, i + matchDistance + 1);

            for (int j = start; j < end; j++)
            {
                if (secondMatch[j]) continue;
                if (firstString[i] != secondString[j]) continue;
                firstMatch[i] = true;
                secondMatch[j] = true;
                matches++;
                break;
            }
        }

        if (matches == 0) return 0f;

        int k = 0;
        int transpositions = 0;

        for (int i = 0; i < firstLength; i++)
        {
            if (!firstMatch[i]) continue;
            while (!secondMatch[k]) k++;
            if (firstString[i] != secondString[k]) transpositions++;
            k++;
        }

        float m = matches;

        return (m / firstLength + m / secondLength + (m - transpositions / 2f) / m) / 3f;
    }

    public static float GetJaroWinklerSimilarity(string firstString, string secondString)
    {
        if (firstString == secondString) return 1f;
        if (firstString.Length == 0 || secondString.Length == 0) return 0f;
        
        float j = GetJaroSimilarity(firstString, secondString);
        if (j == 0f) return 0f;
        
        int maxPrefix = Mathf.Min(4, Mathf.Min(firstString.Length, secondString.Length));
        int prefix = 0;
        while (prefix < maxPrefix && firstString[prefix] == secondString[prefix]) prefix++;
        
        if (j < 0.7f) return j;
        
        const float p = 0.1f;
        
        return j + prefix * p * (1f - j);
    }

    
    public static float GetLevenshteinDistanceSimilarity(string firstString, string secondString)
    {

        if (firstString == secondString) return 1f;
        
        int levenshteinDistance = GetLevenshteinDistance(firstString, secondString);
        int denominator = Mathf.Max(firstString.Length, secondString.Length);

        if (denominator == 0)
        {
            Debug.LogError("Max length of input strings is 0!");
            return 0f;
        }
        
        float levenshteinDistanceSimilarity = Mathf.Clamp01(1 - (float)levenshteinDistance / denominator);
        
        return levenshteinDistanceSimilarity;
    }
    
    public static string Normalize(string stringToNormalize)
    {
        if (string.IsNullOrEmpty(stringToNormalize)) return string.Empty;
        
        string asteriskBlockRemoved = AsteriskPattern.Replace(stringToNormalize, "");
        string parenthesesBlockRemoved = ParenthesesPattern.Replace(asteriskBlockRemoved, "");
        string punctuationRemoved = PunctuationPattern.Replace(parenthesesBlockRemoved, "");
        string lowercaseConverted = punctuationRemoved.ToLowerInvariant();
        return lowercaseConverted;
    }
    
    public static int GetLevenshteinDistance(string firstString, string secondString)
    {
        
        int firstLength = firstString.Length;
        int secondLength = secondString.Length;
        
        if (firstLength == 0) return secondLength;
        if (secondLength == 0) return firstLength;

        int[][] matrix = new int[firstLength + 1][];
        for (int index = 0; index < firstLength + 1; index++)
        {
            matrix[index] = new int[secondLength + 1];
        }

        for (int i = 0; i <= firstLength; i++)
        {
            matrix[i][0] = i;
        }

        for (int j = 0; j <= secondLength; j++)
        {
            matrix[0][j] = j;
        }

        for (int j = 1; j <= secondLength; j++)
        {
            for (int i = 1; i <= firstLength; i++)
            {
                int substitutionCost = (firstString[i - 1] == secondString[j - 1]) ? 0 : 1;

                int deletion = matrix[i - 1][j] + 1;
                int insertion = matrix[i][j - 1] + 1;
                int substitution = matrix[i - 1][j - 1] + substitutionCost;
                
                int[] nums = { deletion, insertion, substitution };
                 
                matrix[i][j] = Mathf.Min(nums);
            }
        }
        
        return matrix[firstLength][secondLength];
    }
    
    
}
