﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace encryption.model.AsymmetricEncryption
{
    public enum KeyAmount { b32 = 32, b64 = 64, b128 = 128, b256 = 256, b512 = 512, b1024 = 1024, b2048 = 2048 };

    public static class AsymmetricEncryption
    {
        private static Random rnd = new Random();

        //генерация ключей для алгоритма RSA
        public static Tuple<Tuple<BigInteger, BigInteger>, Tuple<BigInteger, BigInteger>> GetRSAKeys(KeyAmount b = KeyAmount.b1024)
        {
            BigInteger p = GetSimpleNumber(b);
            BigInteger q = GetSimpleNumber(b);
            BigInteger n = BigInteger.Multiply(p, q);
            BigInteger phin = BigInteger.Multiply(BigInteger.Subtract(p, BigInteger.One), BigInteger.Subtract(q, BigInteger.One));
            BigInteger d;
            BigInteger e;
            do
            {
                e = BigInteger.Add(rnd.NextBigInteger(BigInteger.Subtract(phin, BigInteger.One)), BigInteger.One);
                var nod = EuclidEx(phin, e);
                if (nod.Item3.IsOne)
                {
                    if (nod.Item2 < 0) d = BigInteger.Add(nod.Item2, phin);
                    else d = nod.Item2;
                    break;
                }
            } while (true);

            if (e == d) return GetRSAKeys(b);
            return new Tuple<Tuple<BigInteger, BigInteger>, Tuple<BigInteger, BigInteger>>(
                new Tuple<BigInteger, BigInteger>(e, n),
                new Tuple<BigInteger, BigInteger>(d, n));
        }

        //получить простое число, размерностью b бит
        private static BigInteger GetSimpleNumber(KeyAmount b = KeyAmount.b1024)
        {
            BigInteger result;
            while (true)
            {
                result = GetBigNumber(b);
                if (checkSimplicity(result, b)) break;
            }
            return result;
        }

        //алгоритм Евклида
        private static Tuple<BigInteger, BigInteger, BigInteger> EuclidEx(BigInteger a, BigInteger b)
        {
            BigInteger d0 = a;
            BigInteger d1 = b;
            BigInteger x0 = 1;
            BigInteger x1 = 0;
            BigInteger y0 = 0;
            BigInteger y1 = 1;
            while (d1 > 1)
            {
                BigInteger q = BigInteger.Divide(d0, d1);
                BigInteger d2;
                BigInteger.DivRem(d0, d1, out d2);
                BigInteger x2 = BigInteger.Subtract(x0, BigInteger.Multiply(q, x1));
                BigInteger y2 = BigInteger.Subtract(y0, BigInteger.Multiply(q, y1));
                d0 = d1; d1 = d2;
                x0 = x1; x1 = x2;
                y0 = y1; y1 = y2;
            }
            return new Tuple<BigInteger, BigInteger, BigInteger>(x1, y1, d1);
        }

        //генерация большого числа, размеров определенного количества бит
        private static BigInteger GetBigNumber(KeyAmount keyAmount)
        {
            int nBits = (int)keyAmount;
            byte[] bytes = new byte[nBits / 8];
            rnd.NextBytes(bytes);
            return BigInteger.Abs(new BigInteger(bytes));
        }

        //проверка простоты методом Миллера — Рабина
        private static bool checkSimplicity(BigInteger n, KeyAmount keyAmount)
        {
            int k = (int)keyAmount;         //размерность ключа
            //исключаем числа делимые на простые числа от 2 до 256 либо к
            int[] simpleNumberForCheck = getSimplicityNumbers(k <= 256 ? 256 : k);
            for (int i = 0; i < simpleNumberForCheck.Length; i++)
            {
                BigInteger remainder;
                BigInteger.DivRem(n, new BigInteger(simpleNumberForCheck[i]), out remainder);
                if (remainder.IsZero || n.CompareTo(new BigInteger(simpleNumberForCheck[i])) == 0) return false;
            }

            Random rnd = new Random();
            int s = 0;
            BigInteger nmm = BigInteger.Subtract(n, BigInteger.One);
            BigInteger t = nmm;
            //вычисляем коэффициенты t и s
            do
            {
                t = BigInteger.Divide(t, new BigInteger(2));
                s++;
                BigInteger remainder;
                BigInteger.DivRem(t, new BigInteger(2), out remainder);
                if (remainder != 0) break;
            } while (true);

            //проверяем условия простоты
            int kk = 30720 / k;
            for (int i = 0; i < kk; i++)
            {
                BigInteger a;
                for (;;)
                {
                    a = rnd.NextBigInteger(n);
                    if (nmm.CompareTo(a) > 0 && a.CompareTo(new BigInteger(2)) >= 0) break;
                }
                //проверяем сравнимость по модулю
                BigInteger x = BigInteger.ModPow(a, t, n);
                if (x.IsOne || x.CompareTo(nmm) == 0) continue;
                for (int j = 1; j < s; j++)
                {
                    x = BigInteger.ModPow(x, new BigInteger(2), n);
                    if (x.IsOne) return false;                      //составное
                    if (x.CompareTo(nmm) == 0) goto ff;             //перейти на следующую проверку
                }
                return false;                                       //составное
            ff:;
            }
            return true;
        }

        //Решето Эратосфена
        private static int[] getSimplicityNumbers(int n)
        {
            //массив чисел от 0 до n включительно
            int[] numbers = new int[n + 1];
            for (int i = 0; i < numbers.Length; i++)
                numbers[i] = i;
            numbers[0] = numbers[1] = -1;

            long p = 2;     //первоначальное число
            do
            {
                long k = p;
                //вычеркиваем неподходящие числа
                for (long j = p * p; j <= n; j += p * (p == 2 ? 1 : 2))
                    numbers[j] = -1;
                //выбираем следующее опорное число
                for (long j = p + 1; j <= n; j++)
                    if (numbers[j] != -1)
                    {
                        p = j;
                        break;
                    }
                if (k == p) break;
            } while (true);
            //формируем массив простых чисел
            int k1 = 0;
            for (int i = 0; i < numbers.Length; i++)
                if (numbers[i] != -1) k1++;

            int[] result = new int[k1];

            k1 = 0;
            for (int i = 0; i < numbers.Length; i++)
                if (numbers[i] != -1) result[k1++] = numbers[i];

            return result;
        }
    }

    //расширения стандартных классов
    public static class Extensions
    {
        //генерация случайного числа типа BigInteger
        //число выбирается из диапазона [0; bigN)
        public static BigInteger NextBigInteger(this Random rnd, BigInteger bigN)
        {
            for (;;)
            {
                //генерируем новое число размерностью bigN
                byte[] bytes = bigN.ToByteArray();
                rnd.NextBytes(bytes);
                int bitsToRemove = rnd.Next(bytes.Length * 8);      //количество обнуляемых бит
                int kk = bytes.Length - 1;                          //индекс обрабатываемого байта
                for (int i = 0; i < bitsToRemove; i += 8)
                {
                    //обнуляем целый байт
                    if (i + 8 <= bitsToRemove) bytes[kk--] = 0;
                    //если битов меньше 8 то обнуляем только часть байта
                    if (i < bitsToRemove && i + 8 > bitsToRemove)
                    {
                        bytes[kk] >>= bitsToRemove - i + 1;
                        bytes[kk] <<= bitsToRemove - i + 1;
                    }
                }
                //проверяем подходит ли число
                BigInteger result = BigInteger.Abs(new BigInteger(bytes));
                if (bigN.CompareTo(result) > 0) return result;
            }
        }
    }
}
