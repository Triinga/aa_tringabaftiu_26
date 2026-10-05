static (int min, int max) FindMinMaxIterative(int[] arr)
{
    int min = arr[0];
    int max = arr[0];

    for (int i = 1; i < arr.Length; i++)
    {
        if (arr[i] < min)
            min = arr[i];

        if (arr[i] > max)
            max = arr[i];
    }

    return (min, max);
}

static (int min, int max) FindMinMaxRecursive(int[] arr, int index)
{
    if (index == arr.Length - 1)
        return (arr[index], arr[index]);

    var result = FindMinMaxRecursive(arr, index + 1);

    int min = Math.Min(arr[index], result.min);
    int max = Math.Max(arr[index], result.max);

    return (min, max);
}

int[] numbers = { 7, 2, 15, 4, 11, 9 };


var resultIterative = FindMinMaxIterative(numbers);
var resultRecursive = FindMinMaxRecursive(numbers, 0);

Console.WriteLine($"Iterative min: {resultIterative.min}, max: {resultIterative.max}");
Console.WriteLine($"Recursive min: {resultRecursive.min}, max: {resultRecursive.max}");