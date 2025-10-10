using System.Linq;

namespace SimpleSilksongLocalizer;

public static class ArrayUtils
{
    public static void Add<T>(ref T[] arr, params T[] newElem)
    {
        var newArr = new T[arr.Length + newElem.Length];
        for (var i = 0; i < arr.Length; ++i)
        {
            newArr[i] = arr[i];
        }

        for (var i = arr.Length; i < newArr.Length; ++i)
        {
            newArr[i] = newElem[i - arr.Length];
        }
        arr = newArr;
    }

    public static void Shrink<T>(ref T[] arr, int newLength)
    {
        var newArr = new T[arr.Length];

        for (var i = 0; i < newLength; ++i)
        {
            newArr[i] = arr[i];
        }

        arr = newArr;
    }
}