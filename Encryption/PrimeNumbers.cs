using System;
using System.Numerics;
using System.Security.Cryptography;

namespace Encryption
{
    public static class PrimeNumbers
    {
        private static readonly Random Random = new Random();
        
        public static long Generate()
        {
            const int left = 1000, right = 10000;
            long number;

            while (true)
            {
                number = Random.Next(left, right);
                if (IsPrime(number))
                    break;
            }

            return number;
        }

        public static bool IsPrime(long number)
        {
            const int rounds = 10;
            
            if (!MillerRabinTest(number, rounds)) return false;
            
            for (long i = 2; i <= Math.Sqrt(number); i++)
                if (number % i == 0)
                    return false;
                
            return true;
        }

        private static bool MillerRabinTest(BigInteger n, int k)
        {
            if (n == 2 || n == 3)
                return true;

            if (n < 2 || n % 2 == 0)
                return false;

            BigInteger t = n - 1;
            int s = 0;

            while (t % 2 == 0)
            {
                t /= 2;
                s += 1;
            }

            var rng = new RNGCryptoServiceProvider();
            for (int i = 0; i < k; i++)
            {
                byte[] randomArray = new byte[n.ToByteArray().LongLength];

                BigInteger a;

                do
                {
                    rng.GetBytes(randomArray);
                    a = new BigInteger(randomArray);
                } while (a < 2 || a >= n - 2);

                BigInteger x = BigInteger.ModPow(a, t, n);

                if (x == 1 || x == n - 1)
                    continue;

                for (int r = 1; r < s; r++)
                {
                    x = BigInteger.ModPow(x, 2, n);

                    if (x == 1)
                        return false;

                    if (x == n - 1)
                        break;
                }

                if (x != n - 1)
                    return false;
            }

            return true;
        }
    }
}