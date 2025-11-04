using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public static class StringSimilarity
{
    /// <summary>
    /// Determines whether two strings are similar based on the Levenshtein distance ratio or exact character differences for short strings.
    /// </summary>
    /// <param name="input">The input string to compare.</param>
    /// <param name="target">The target string to compare against.</param>
    /// <param name="threshold">The Levenshtein distance ratio threshold for similarity. Defaults to 0.3.</param>
    /// <returns>A boolean indicating whether the strings are considered similar.</returns>
    public static bool IsSimilar(string input, string target, float threshold = 0.3f)
    {
        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(target)) return false;

        int minLength = Mathf.Min(Normalize(input).Length, Normalize(target).Length);
        if (minLength <= 3) return GetLevenshteinDistance(input, target) <= 2;
        if (minLength <= 5) return GetLevenshteinDistance(input, target) <= 3;
        
        return GetLevenshteinDistanceRatio(input, target) <= threshold;
    }

    /// <summary>
    /// Calculates the Levenshtein distance ratio between two strings, which is a normalized value representing their similarity.
    /// </summary>
    /// <param name="firstString">The first string to compare.</param>
    /// <param name="secondString">The second string to compare.</param>
    /// <returns>A float representing the Levenshtein distance ratio. A value of 0 indicates identical strings, while higher values indicate greater dissimilarity.</returns>
    public static float GetLevenshteinDistanceRatio(string firstString, string secondString)
    {
        int levenshteinDistance = GetLevenshteinDistance(firstString, secondString);
        
        int denominator = Mathf.Max(Normalize(firstString).Length, Normalize(secondString).Length);

        if (denominator == 0) return 0f;
        
        return (float)levenshteinDistance / denominator;
    }

    /// <summary>
    /// Normalizes a string by removing spaces, punctuation, and parentheses, and converting it to lowercase.
    /// </summary>
    /// <param name="stringToNormalize">The string to normalize.</param>
    /// <returns>A normalized string with spaces, punctuation, and parentheses removed and converted to lowercase.</returns>
    public static string Normalize(string stringToNormalize)
    {
        if (stringToNormalize == null) return string.Empty;
        
        string noParentheses = Regex.Replace(stringToNormalize, @"\([^)]*\)", "");
        string normalizedString = Regex.Replace(noParentheses, "[ .!?,\"-]", "").ToLowerInvariant();
        return normalizedString;
    }

    /// <summary>
    /// Calculates the Levenshtein distance between two strings, which is the minimum number of
    /// single-character edits (insertions, deletions, or substitutions) required to transform one string into the other.
    /// </summary>
    /// <param name="firstString">The first string to compare.</param>
    /// <param name="secondString">The second string to compare.</param>
    /// <returns>The Levenshtein distance between the two normalized strings.</returns>
    public static int GetLevenshteinDistance(string firstString, string secondString)
    {
        string normalizedFirstString = Normalize(firstString);
        string normalizedSecondString = Normalize(secondString);
        
        int firstLength = normalizedFirstString.Length;
        int secondLength = normalizedSecondString.Length;
        
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
                int substitutionCost = (normalizedFirstString[i - 1] == normalizedSecondString[j - 1]) ? 0 : 1;

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
