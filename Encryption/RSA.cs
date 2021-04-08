using System;
using System.Numerics;
using System.Text;
using System.Collections.Generic;

namespace Encryption
{
    public class Rsa
    {
        private long p, q, r, e, d;
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
            d = GetPrivatePartKey(fi, e);

            _isReady = true;
        }

        public string[] Encrypt(string text, long publicE, long privateD)
        {
            if (!_isReady)
                throw new ArgumentException("Method Initialize() not called.");

            return Encode(text, publicE, privateD);
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

        private static long GetPrivatePartKey(long fi, long e)
        {
            long d = e + 1;

            while (true)
            {
                if ((d * e) % fi == 1)
                    break;
                d++;
            }

            return d;
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