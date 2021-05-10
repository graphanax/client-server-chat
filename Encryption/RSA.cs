using System;
using System.Numerics;
using System.Text;
using System.Collections.Generic;

namespace Encryption
{
    public class Rsa
    {
        private long p, q, d;
        public long r { get; private set; }
        public long e { get; private set; }
        
        private bool _isReady;

        public Rsa()
        {
            Initialize();
        }

        private void Initialize()
        {
            p = PrimeNumbers.Generate();
            q = PrimeNumbers.Generate();

            r = p * q;

            var fi = (p - 1) * (q - 1);

            e = GetPublicPartKey(fi);
            d = GetPrivatePartKey(e, fi);

            _isReady = true;
        }

        public string[] Encrypt(string text, long publicE, long publicR)
        {
            if (!_isReady)
                throw new ArgumentException("Method Initialize() not called.");

            return Encode(text, publicE, publicR);
        }

        public string Decrypt(string[] data)
        {
            if (!_isReady)
                throw new ArgumentException("Method Initialize() not called.");

            return Decode(data, d, r);
        }

        private static string[] Encode(string text, long e, long r)
        {
            var data = new List<string>();

            foreach (var ch in text)
            {
                int index = ch;

                var num = FastExp(index, e, r);

                data.Add(num.ToString());
            }

            return data.ToArray();
        }

        private static string Decode(string[] data, long d, long r)
        {
            var strBuilder = new StringBuilder();

            foreach (var item in data)
            {
                var val = new BigInteger(Convert.ToInt64(item));
                var num = FastExp(val, d, r);

                strBuilder.Append((char) num);
            }

            return strBuilder.ToString();
        }

        private static long GetPublicPartKey(long fi)
        {
            long e = fi - 1;

            while (true)
            {
                if (PrimeNumbers.IsPrime(e) &&
                    e < fi &&
                    BigInteger.GreatestCommonDivisor(new BigInteger(e), new BigInteger(fi)) == BigInteger.One)
                    break;
                e--;
            }

            return e;
        }
        
        // Advanced Euclid algorithm
        private static long GetPrivatePartKey(long a, long n)
        {
            long d0 = n;
            long d1 = a;
            long y0 = 0;
            long y1 = 1;

            while (d1 > 1)
            {
                long q = d0 / d1;
                long d2 = d0 % d1;
                long y2 = y0 - q * y1;
                d0 = d1;
                d1 = d2;
                y0 = y1;
                y1 = y2;
            }

            return y1 < 0 ? y1 + n : y1;
        }

        private static BigInteger FastExp(BigInteger a, BigInteger z, BigInteger n)
        {
            BigInteger a1 = a, z1 = z, x = 1;
            while (z1 != 0)
            {
                while (z1 % 2 == 0)
                {
                    z1 = z1 / 2;
                    a1 = (a1 * a1) % n;
                }

                z1 = z1 - 1;
                x = (x * a1) % n;
            }

            return x;
        }
    }
}