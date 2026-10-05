int[] sortedNumbers = { 2, 4, 7, 9, 11, 15, 18, 21, 26, 30 };
int target = 18;

int iterativeIndex = LinearSearch(sortedNumbers, target);
int recursiveIndex = BinarySearch(sortedNumbers, target, 0, sortedNumbers.Length - 1);

Console.WriteLine($"Iterative linear search: index {iterativeIndex}");
Console.WriteLine($"Recursive binary search: index {recursiveIndex}");

// Checks elements one by one: O(n) time and O(1) extra space.
static int LinearSearch(int[] numbers, int target)
{
    for (int i = 0; i < numbers.Length; i++)
    {
        if (numbers[i] == target)
            return i;
    }

    return -1;
}

// Searches only the half that can contain the target: O(log n) time and
// O(log n) call-stack space. The input array must be sorted.
static int BinarySearch(int[] numbers, int target, int left, int right)
{
    if (left > right)
        return -1;

    int mid = left + (right - left) / 2;

    if (numbers[mid] == target)
        return mid;

    if (target < numbers[mid])
        return BinarySearch(numbers, target, left, mid - 1);

    return BinarySearch(numbers, target, mid + 1, right);
}
