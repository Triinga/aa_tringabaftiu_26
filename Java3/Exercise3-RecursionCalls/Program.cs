// Predictions made before running the counter:
//
// Function             Returns                  Calls
// m1, length 8         15                       15
// m2(20)               difficult to predict     difficult to predict
// m3(20)               524,288                  1,048,575
//
// m2 is difficult to predict by inspection because m2 and mm call each other,
// with recursive calls nested inside the arguments of other calls.
//
// Results checked with the counter:
// Function             Returns                  Calls
// m1, length 8         15                       15
// m2(20)               13                       1,627 combined m2/mm calls
// m3(20)               524,288                  1,048,575
//
// For m1, the 8 base cases each return 1, and the 7 split calls each add 1,
// so the result is 8 + 7 = 15. There are 8 + 7 = 15 calls in total.
// For m3, T(n) = 2T(n - 1) + 1, so T(20) = 2^20 - 1 calls.

int m1Calls = 0;
int m2Calls = 0;
int m3Calls = 0;
int[] values = { 1, 2, 3, 4, 5, 6, 7, 8 };

m1Calls = 0;
int m1Result = M1(values);
Console.WriteLine($"m1(length 8): returns {m1Result}, calls {m1Calls}");

m2Calls = 0;
int m2Result = M2(20);
Console.WriteLine($"m2(20): returns {m2Result}, calls to m2/mm combined {m2Calls}");

m3Calls = 0;
int m3Result = M3(20);
Console.WriteLine($"m3(20): returns {m3Result}, calls {m3Calls}");

int M1(int[] values)
{
    m1Calls++;
    if (values.Length <= 1)
        return values.Length;

    int mid = values.Length >> 1;
    return M1(values[..mid]) + M1(values[mid..]) + 1;
}

int M2(int n)
{
    m2Calls++;
    if (n == 0)
        return 1;

    return n - Mm(M2(n - 1));
}

int Mm(int n)
{
    m2Calls++;
    if (n == 0)
        return 0;

    return n - M2(Mm(n - 1));
}

int M3(int n)
{
    m3Calls++;
    if (n <= 1)
        return 1;

    return M3(n - 1) + M3(n - 1);
}
