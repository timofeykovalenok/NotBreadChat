namespace Core.Extensions
{
    public static class ByteExtensions
    {
        public static byte[] ConcatBytes(this byte[] array1, byte[] array2)
        {
            byte[] resultArray = new byte[array1.Length + array2.Length];
            Buffer.BlockCopy(array1, 0, resultArray, 0, array1.Length);
            Buffer.BlockCopy(array2, 0, resultArray, array1.Length, array2.Length);

            return resultArray;
        }
    }
}
