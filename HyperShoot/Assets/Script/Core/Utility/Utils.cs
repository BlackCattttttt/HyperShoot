using System.Collections;
using System.Text;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Newtonsoft.Json;
using UnityEngine.UI;
using TMPro;
using Object = System.Object;
using Random = System.Random;

public static class Utils
{
    public static float GetRandomPercent()
    {
        return UnityEngine.Random.Range(0, 100f);
    }

    public static Vector2 Vector3to2(Vector3 vector3)
    {
        return new Vector2(vector3.x, vector3.y);
    }

    public static Vector3 Round(this Vector3 vector, int decimalPlaces = 2)
    {
        float multiplier = 1;
        for (int i = 0; i < decimalPlaces; i++)
        {
            multiplier *= 10f;
        }

        return new Vector3(
            Mathf.Round(vector.x * multiplier) / multiplier,
            Mathf.Round(vector.y * multiplier) / multiplier,
            Mathf.Round(vector.z * multiplier) / multiplier);
    }

    public static void FlipVector3(this List<Vector3> lsV3, int flipX = 1, int flipY = 1)
    {
        Vector3 temp = Vector3.zero;
        for (int i = 0; i < lsV3.Count; i++)
        {
            temp = lsV3[i];
            temp.x *= flipX;
            temp.y *= flipY;
            lsV3[i] = temp;
        }
    }

    public static void FlipVector3(this Vector3[] lsV3, int flipX = 1, int flipY = 1)
    {
        Vector3 temp = Vector3.zero;
        for (int i = 0; i < lsV3.Length; i++)
        {
            temp = lsV3[i];
            temp.x *= flipX;
            temp.y *= flipY;
            lsV3[i] = temp;
        }
    }

    public static Vector2 Round(this Vector2 vector, int decimalPlaces = 2)
    {
        float multiplier = 1;
        for (int i = 0; i < decimalPlaces; i++)
        {
            multiplier *= 10f;
        }

        return new Vector2(
            Mathf.Round(vector.x * multiplier) / multiplier,
            Mathf.Round(vector.y * multiplier) / multiplier);
    }

    public static string StringReplaceAt(string value, int index, char newchar)
    {
        if (value.Length <= index)
            return value;
        StringBuilder sb = new StringBuilder(value);
        sb[index] = newchar;
        return sb.ToString();
    }

    public static IEnumerator WaitFor(Func<bool> Action, float delay)
    {
        float time = 0;
        while (time <= delay)
        {
            if (Action())
                time += Time.deltaTime;
            else
                time = 0;
            yield return 0;
        }

        yield break;
    }

    public static Vector3 ScaleVector(Vector3 original, float x, float y, float z)
    {
        return new Vector3(original.x * x, original.y * y, original.z * z);
    }

    public static bool CheckArrayHasString(string[] c, string b)
    {
        bool d = false;
        foreach (string child in c)
        {
            if (child != b)
                continue;
            d = true;
        }

        return d;
    }

    public static T GetRandom<T>(this ICollection<T> collection)
    {
        if (collection == null)
            return default(T);
        int t = UnityEngine.Random.Range(0, collection.Count);
        foreach (T element in collection)
        {
            if (t == 0)
                return element;
            t--;
        }

        return default(T);
    }

    public static T GetLast<T>(this T[] collection)
    {
        if (collection == null)
            return default(T);
        return collection[collection.Length - 1];
    }

    public static T GetLast<T>(this List<T> collection)
    {
        if (collection == null)
            return default(T);
        return collection[collection.Count - 1];
    }

    public static bool CheckHaveItemNull<T>(this List<T> collection)
    {
        foreach (T element in collection)
        {
            if (element == null)
                return true;
        }

        return false;
    }

    public static bool CheckHaveItemNull<T>(this T[] collection)
    {
        foreach (T element in collection)
        {
            if (element == null)
                return true;
        }

        return false;
    }

    public static bool CheckHaveItemNull<T, Z>(this Dictionary<T, Z> collection)
    {
        foreach (var element in collection)
        {
            if (element.Value == null)
                return true;
        }

        return false;
    }

    public static bool CheckIsNullOrEmpty<T>(this T[] collection)
    {
        if (collection == null || collection.Length == 0)
            return true;
        else
            return false;
    }

    public static bool CheckIsNullOrEmpty<T>(this List<T> collection)
    {
        if (collection == null || collection.Count == 0)
            return true;
        else
            return false;
    }

    public static bool CheckIsNullOrEmpty<T, Z>(this Dictionary<T, Z> collection)
    {
        if (collection == null || collection.Count == 0)
            return true;
        else
            return false;
    }


    public static T GetRandom<T>(this List<T> collection)
    {
        if (collection == null)
            return default(T);
        int t = UnityEngine.Random.Range(0, collection.Count);
        return collection[t];
    }

    public static void ChangeText(Text text, int start, int end, float duration)
    {
        DOTween.defaultTimeScaleIndependent = true;
        DOTween.To(x => text.text = ((int) x).ToString(), start, end, duration);
    }

    public static void ChangeText(this Text text, long start, long end, float duration, string format, float delay = 0)
    {
        DOTween.defaultTimeScaleIndependent = true;
        DOTween.To(x => text.text = string.Format(format, (int) x), start, end, duration).SetDelay(delay);
    }

    public static void ChangeText(this TMP_Text text, int start, int end, float duration, float delay = 0)
    {
        DOTween.defaultTimeScaleIndependent = true;
        DOTween.To(x => text.text = ((int) x).ToString(), start, end, duration).SetDelay(delay);
    }

    // public static void ChangeText(TMP_Text text, float start, float end, float duration, string format)
    // {
    //     DOTween.defaultTimeScaleIndependent = true;
    //     DOTween.To(x => text.text = string.Format(format, x.ToString(GameString.FLOAT1)), start, end, duration);
    // }

    public static void ChangeText(TMP_Text text, long start, long end, float duration, string format, float delay = 0)
    {
        DOTween.defaultTimeScaleIndependent = true;
        DOTween.To(x => text.text = string.Format(format, (int) x), start, end, duration).SetDelay(delay);
    }

    public static void ChangeImageFill(Image image, float start, float end, float duration, float delay = 0)
    {
        DOTween.To(x => image.fillAmount = x, start, end, duration).SetDelay(delay);
    }

    public static void ChangeImageWidth(RectTransform image, float end, float duration)
    {
        image.DOSizeDelta(new Vector2(end, image.sizeDelta.y), duration);
    }

    public static bool IsCollider(Vector2 posObject1, BoxCollider2D boxObject1, Vector2 posObject2,
        BoxCollider2D boxObject2)
    {
        float l1X, l1Y, r1X, r1Y;
        float l2X, l2Y, r2X, r2Y;

        l1X = posObject1.x - boxObject1.bounds.size.x / 2;
        l1Y = posObject1.y + boxObject1.bounds.size.y / 2;
        r1X = posObject1.x + boxObject1.bounds.size.x / 2;
        r1Y = posObject1.y - boxObject1.bounds.size.y / 2;


        l2X = posObject2.x - boxObject2.bounds.size.x / 2;
        l2Y = posObject2.y + boxObject2.bounds.size.y / 2;
        r2X = posObject2.x + boxObject2.bounds.size.x / 2;
        r2Y = posObject2.y - boxObject2.bounds.size.y / 2;

        if (l1X > r2X || l2X > r1X)
        {
            return false;
        }

        // If one rectangle is above other  
        if (l1Y < r2Y || l2Y < r1Y)
        {
            return false;
        }

        return true;
    }

    public static string ToCountFormat(int value, int digits)
    {
        string s = value.ToString();
        int originLength = s.Length;
        if (originLength >= digits)
            return s;

        for (int i = 0; i < digits - originLength; i++)
            s = "0" + s;

        return s;
    }

    public static string ToFormatString(this long s)
    {
        return String.Format("{0:n0}", s);
    }

    public static string ToFormatString(this int s)
    {
        return String.Format("{0:n0}", s);
    }

    public static string ToFormatString(this double s)
    {
        return String.Format("{0:n0}", s);
    }


    public static string ToQuantityString(this long s)
    {
        return String.Format("x{0:n0}", s);
    }

    public static string ToQuantityString(this int s)
    {
        return String.Format("x{0:n0}", s);
    }

    public static string ToQuantityString(this double s)
    {
        return String.Format("x{0:n0}", s);
    }

    public static List<T> Clone<T>(this List<T> collection)
    {
        var newList = new List<T>();
        foreach (T element in collection)
        {
            newList.Add(element);
        }

        return newList;
    }

    public static int RaiseFlag(int giatri, int flag_idx)
    {
        return (giatri | (1 << flag_idx));
    }

    public static bool CheckFlag(int giatri, int flag_idx) //gia ti la database,flag_idx laf client truyen len
    {
        return ((giatri & (1 << flag_idx)) != 0);
    }

    public static int DownFlag(int giatri, int flag_idx)
    {
        return (giatri & (~(1 << flag_idx)));
    }

    public static long RaiseFlagLong(long giatri, int flag_idx)
    {
        long flag_val = (long) (1) << flag_idx;
        return (giatri | flag_val);
    }

    public static long DownFlagLong(long giatri, int flag_idx)
    {
        long flag_val = (long) (1) << flag_idx;

        return (giatri & (~(flag_val)));
    }

    public static bool CheckFlagLong(long giatri, int flag_idx) //gia ti la database,flag_idx laf client truyen len
    {
        long flag_val = (long) (1) << (int) (flag_idx);
        long and_val = giatri & flag_val;
        return (and_val != 0);
    }

    public static T PickRandom<T>(this IEnumerable<T> source)
    {
        return source.PickRandom(1).SingleOrDefault();
    }

    public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count)
    {
        return source.Shuffle().Take(count);
    }

    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
    {
        return source.OrderBy(x => Guid.NewGuid());
    }

    public static IList<T> Shuffle<T>(this IList<T> list, Random random)
    {
        if (random == null)
            random = new Random(System.DateTime.Now.Millisecond);
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }

        return list;
    }

    public static IList<T> PickRandom<T>(this IList<T> source, int count, Random random = null)
    {
        return source.Shuffle(random).Take(count).ToList();
    }

    public static T PickRandom<T>(this IList<T> source, Random random)
    {
        return source.PickRandom(1, random).SingleOrDefault();
    }

    public static int GetMaxElementCount<T>(this IEnumerable<T> source,
        int _numberElementDesireToGet)
    {
        if (source.Count() < _numberElementDesireToGet)
            return source.Count();

        return _numberElementDesireToGet;
    }

    public static bool Compare<T>(this List<T> list1, List<T> list2)
    {
        if (list1.Count != list2.Count)
            return false;

        for (int i = 0; i < list1.Count; i++)
        {
            if (!list1[i].Equals(list2[i]))
                return false;
        }

        var firstNotSecond = list1.Except(list2).ToList();
        var secondNotFirst = list2.Except(list1).ToList();
        return !firstNotSecond.Any() && !secondNotFirst.Any();
    }

    public static bool Compare<T>(this T[] list1, T[] list2)
    {
        if (list1.Length != list2.Length)
            return false;

        for (int i = 0; i < list1.Length; i++)
        {
            if (!list1[i].Equals(list2[i]))
                return false;
        }

        var firstNotSecond = list1.Except(list2).ToList();
        var secondNotFirst = list2.Except(list1).ToList();
        return !firstNotSecond.Any() && !secondNotFirst.Any();
    }

    public static string GetRomanNumeralsForShipTier(int value)
    {
        var tierStr = string.Empty;

        switch (value)
        {
            case 0:
                tierStr = "I";
                break;

            case 1:
                tierStr = "II";
                break;

            case 2:
                tierStr = "III";
                break;

            case 3:
                tierStr = "IV";
                break;
        }

        return tierStr;
    }
}

[Serializable]
public class Int2
{
    public int x;
    public int y;

    public Int2(int _x, int _y)
    {
        x = _x;
        y = _y;
    }

    public Int2()
    {
        x = 0;
        y = 0;
    }
}